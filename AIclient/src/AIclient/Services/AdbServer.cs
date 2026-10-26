using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using AIclient.Models;

namespace AIclient.Services;

// Coordinates versioned SDKs with the server-status command; leaves legacy custom binaries alone.
public sealed class AdbServer
{
    public static AdbServer Shared { get; } = new();
    private readonly Func<string, string[], Task<ProcessResult>> execute;
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly ConcurrentDictionary<string, Lazy<Task>> initialized = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DateTimeOffset> lastRecovery = new(StringComparer.OrdinalIgnoreCase);
    private Task<bool>? recovery;

    public AdbServer(Func<string, string[], Task<ProcessResult>>? execute = null) =>
        this.execute = execute ?? ((path, args) => ProcessRunner.RunAsync(path, args, 8, CancellationToken.None));

    public static bool IsManaged(string path)
    {
        var properties = Path.Combine(Path.GetDirectoryName(path) ?? "", "source.properties");
        if (!Path.GetFileName(path).Equals("adb.exe", StringComparison.OrdinalIgnoreCase) || !File.Exists(properties)
            || !string.Equals(Path.GetFileName(Path.GetDirectoryName(path)), "platform-tools", StringComparison.OrdinalIgnoreCase)) return false;
        var version = Regex.Match(File.ReadAllText(properties), @"(?m)^Pkg\.Revision=(\d+)");
        return version.Success && int.TryParse(version.Groups[1].Value, out var major) && major >= 37;
    }

    public async Task EnsureAsync(string path, CancellationToken token)
    {
        if (!IsManaged(path)) return;
        var pendingRecovery = recovery;
        if (pendingRecovery is { IsCompleted: false }) await pendingRecovery.WaitAsync(token);
        var pending = initialized.GetOrAdd(path, p => new Lazy<Task>(() => InitializeAsync(p)));
        try { await pending.Value.WaitAsync(token); }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch { initialized.TryRemove(new KeyValuePair<string, Lazy<Task>>(path, pending)); throw; }
    }

    private async Task InitializeAsync(string path)
    {
        await gate.WaitAsync();
        try
        {
            Check(await execute(path, ["start-server"]));
            var status = await execute(path, ["server-status"]);
            // Old and new ADB both speak protocol 41, so start-server alone keeps the old daemon.
            if (!IsExpectedServer(status, path))
            {
                if (status.ExitCode != 0 && !(status.Error + status.Output).Contains("unknown host service", StringComparison.OrdinalIgnoreCase))
                    Check(status);
                await RestartAsync(path);
                var current = await execute(path, ["server-status"]);
                Check(current);
                if (!IsExpectedServer(current, path)) throw new IOException("ADB 后台路径与当前配置不一致，未使用旧后台继续扫描。");
            }
        }
        finally { gate.Release(); }
    }

    public static bool IsExpectedServer(ProcessResult status, string path)
    {
        if (status.ExitCode != 0) return false;
        var match = Regex.Match(status.Output, """(?m)^executable_absolute_path:\s*("(?:\\.|[^"\\])*")\s*$""");
        if (!match.Success) return false;
        var actual = Json.Read<string>(match.Groups[1].Value);
        return Path.GetFullPath(actual).Equals(Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> RecoverEmptyUsbAsync(string path, CancellationToken token)
    {
        if (!IsManaged(path)) return Task.FromResult(false);
        // Let the stop/start transaction finish even when a newer UI refresh cancels its waiter.
        if (recovery is not { IsCompleted: false }) recovery = RecoverCoreAsync(path);
        return recovery.WaitAsync(token);
    }

    private async Task<bool> RecoverCoreAsync(string path)
    {
        await gate.WaitAsync();
        try
        {
            if (lastRecovery.TryGetValue(path, out var previous) && DateTimeOffset.UtcNow - previous < TimeSpan.FromSeconds(30)) return false;
            var current = await execute(path, ["devices", "-l"]); Check(current);
            // Recheck immediately before restarting; never disturb another listed or unauthorised device.
            if (HasDevices(current.Output)) return false;
            lastRecovery[path] = DateTimeOffset.UtcNow;
            await RestartAsync(path);
            var status = await execute(path, ["server-status"]); Check(status);
            if (!IsExpectedServer(status, path)) throw new IOException("ADB 恢复后后台路径不一致。");
            return true;
        }
        finally { gate.Release(); }
    }

    private async Task RestartAsync(string path)
    {
        Check(await execute(path, ["kill-server"]));
        // A cancelled scan must not leave the server stopped.
        Check(await execute(path, ["start-server"]));
    }

    public static bool HasDevices(string output) => output.Split('\n').Any(line =>
        line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries) is var p && p.Length >= 2
        && p[1] is "device" or "offline" or "unauthorized" or "recovery" or "sideload" or "bootloader" or "connecting" or "authorizing");

    private static void Check(ProcessResult result)
    {
        if (result.ExitCode != 0) throw new IOException("ADB 后台操作失败：" + result.Error + result.Output);
    }
}

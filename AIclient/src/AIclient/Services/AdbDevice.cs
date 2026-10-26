using System.Globalization;
using System.Windows.Media.Imaging;
using AIclient.Models;

namespace AIclient.Services;

public sealed class AdbDevice(AppSettings settings)
{
    // Prefer the private SDK, including migration from the repository's legacy default.
    public static string ResolveExecutable(string configured, string? searchRoot = null)
    {
        var bare = configured.Equals("adb.exe", StringComparison.OrdinalIgnoreCase);
        for (var dir = new DirectoryInfo(searchRoot ?? AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var modern = Path.Combine(dir.FullName, "tools", "platform-tools", "adb.exe");
            var project = dir.Parent?.FullName ?? dir.FullName;
            var legacy = new[] { Path.Combine(project, "adb.exe"), Path.Combine(project, "Resource", "ADB", "adb.exe") };
            if (File.Exists(modern) && (bare || legacy.Any(p => Path.GetFullPath(configured).Equals(p, StringComparison.OrdinalIgnoreCase)))) return modern;
            if (bare)
                foreach (var candidate in new[] { Path.Combine(dir.FullName, "adb.exe"), Path.Combine(dir.FullName, "Resource", "ADB", "adb.exe") })
                    if (File.Exists(candidate)) return candidate;
        }
        return configured;
    }
    public string Serial(string role)
    {
        var slot = AdbSlots.Normalize(role);
        return settings.BoundAdbDevices().FirstOrDefault(d => d.Slot == slot)?.Serial
            ?? throw new InvalidOperationException($"未绑定 ADB 设备编号：{role}");
    }
    public async Task<string> ScanAsync(CancellationToken token)
    {
        var executable = ResolveExecutable(settings.AdbPath);
        await AdbServer.Shared.EnsureAsync(executable, token);
        var r = await ProcessRunner.RunAsync(executable, ["devices", "-l"], 3, token);
        Check(r); return r.Output;
    }
    public async Task CheckAsync(string role, CancellationToken token)
    {
        var r = await CommandAsync(role, ["get-state"], token);
        if (r.Trim() != "device") throw new InvalidOperationException($"ADB 设备 {role} 不在线：{r}");
    }
    public async Task<string> CommandAsync(string role, IEnumerable<string> args, CancellationToken token)
    {
        var executable = ResolveExecutable(settings.AdbPath);
        await AdbServer.Shared.EnsureAsync(executable, token);
        var r = await ProcessRunner.RunAsync(executable, new[] { "-s", Serial(role) }.Concat(args), 25, token);
        Check(r); return r.Output;
    }
    public async Task<(int width, int height)> CaptureAsync(string role, string destination, CancellationToken token)
    {
        var serial = Serial(role);
        var remote = "/data/local/tmp/aiclient_" + Guid.NewGuid().ToString("N") + ".png";
        try
        {
            await CommandAsync(role, ["shell", "screencap", "-p", remote], token);
            await CommandAsync(role, ["pull", remote, destination], token);
            using var stream = File.OpenRead(destination);
            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            var image = decoder.Frames[0];
            if (image.PixelWidth < 10 || image.PixelHeight < 10) throw new InvalidDataException($"设备 {serial} 截图尺寸异常。");
            return (image.PixelWidth, image.PixelHeight);
        }
        finally
        {
            try { await CommandAsync(role, ["shell", "rm", "-f", remote], CancellationToken.None); } catch { }
        }
    }
    public static string Number(int n) => n.ToString(CultureInfo.InvariantCulture);
    private static void Check(ProcessResult result)
    {
        if (result.ExitCode != 0) throw new InvalidOperationException("ADB 操作失败：" + result.Error + " " + result.Output);
    }
}

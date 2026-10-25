using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace AIclient.Services;

public sealed class LegacyBusClient : IAsyncDisposable
{
    private readonly Process process;
    private readonly Task reader, errors;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> pending = new();
    private readonly SemaphoreSlim commands = new(1);
    private readonly Action<string> frame;
    public static string HostPath => Path.Combine(AppContext.BaseDirectory, "device-host", "AIclient.DeviceHost.exe");
    public LegacyBusClient(string directory, Action<string> frame)
    {
        this.frame = frame;
        if (!File.Exists(HostPath)) throw new FileNotFoundException("缺少旧总线兼容进程，请完整构建 AIclient 解决方案。", HostPath);
        Directory.CreateDirectory(directory);
        process = new Process { StartInfo = new(HostPath) { WorkingDirectory = directory, UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, StandardInputEncoding = new UTF8Encoding(false), StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 } };
        process.Start();
        reader = ReadLoop();
        errors = Task.Run(async () => { using var writer = new StreamWriter(Path.Combine(directory, "driver-stderr.txt")); while (await process.StandardError.ReadLineAsync() is { } line) { await writer.WriteLineAsync(line); await writer.FlushAsync(); } });
    }
    private async Task ReadLoop()
    {
        Exception? failure = null;
        try
        {
            while (await process.StandardOutput.ReadLineAsync() is { } line)
            {
                using var doc = JsonDocument.Parse(line); var value = doc.RootElement;
                if (value.GetProperty("kind").GetString() == "frame") frame(line);
                else if (pending.TryRemove(value.GetProperty("id").GetString()!, out var response)) response.TrySetResult(value.Clone());
            }
        }
        catch (Exception ex) { failure = ex; }
        finally { foreach (var item in pending.Values) item.TrySetException(failure ?? new IOException("旧总线兼容进程已退出。")); }
    }
    public void CheckHealthy() { if (process.HasExited || reader.IsCompleted) throw new IOException("旧总线兼容进程已退出或接收已中断，请检查驱动日志。"); }
    public async Task<JsonElement> CallAsync(string operation, object? arguments, CancellationToken token, int timeoutSeconds = 20)
    {
        await commands.WaitAsync(token);
        var id = Guid.NewGuid().ToString("N");
        var response = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            if (process.HasExited) throw new IOException("旧总线兼容进程已退出。");
            pending[id] = response;
            var fields = arguments == null ? new Dictionary<string, JsonElement>() : Models.Json.Read<Dictionary<string, JsonElement>>(Models.Json.Serialize(arguments));
            fields["operation"] = JsonSerializer.SerializeToElement(operation); fields["requestId"] = JsonSerializer.SerializeToElement(id);
            await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(fields)); await process.StandardInput.FlushAsync();
            var result = await response.Task.WaitAsync(TimeSpan.FromSeconds(timeoutSeconds), token);
            if (!result.GetProperty("success").GetBoolean()) throw new IOException(result.GetProperty("error").GetString());
            return result.GetProperty("result").Clone();
        }
        catch { if (!process.HasExited) process.Kill(true); throw; }
        finally { pending.TryRemove(id, out _); commands.Release(); }
    }
    public async ValueTask DisposeAsync()
    {
        if (!process.HasExited)
        {
            try { await CallAsync("close", null, CancellationToken.None, 3); } catch { }
            if (!process.HasExited) { process.StandardInput.Close(); try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(3)); } catch { if (!process.HasExited) process.Kill(true); } }
        }
        try { await Task.WhenAll(reader, errors); }
        finally { process.Dispose(); commands.Dispose(); }
    }
}

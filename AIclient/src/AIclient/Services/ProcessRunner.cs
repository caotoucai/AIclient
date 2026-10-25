using System.Diagnostics;
using System.Text;
using AIclient.Models;

namespace AIclient.Services;

public static class ProcessRunner
{
    public static async Task<ProcessResult> RunAsync(string executable, IEnumerable<string> args, int timeoutSeconds, CancellationToken cancellationToken, string? input = null, string? cwd = null, string? stdoutPath = null, string? stderrPath = null)
    {
        var info = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true, RedirectStandardInput = input != null, StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 };
        if (cwd != null) info.WorkingDirectory = cwd;
        foreach (var arg in args) info.ArgumentList.Add(arg);
        using var process = new Process { StartInfo = info };
        if (!process.Start()) throw new InvalidOperationException($"无法启动：{executable}");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        using var registration = linked.Token.Register(() => { try { if (!process.HasExited) process.Kill(true); } catch (InvalidOperationException) { } catch (System.ComponentModel.Win32Exception) { } });
        try
        {
            if (input != null) { await process.StandardInput.WriteAsync(input.AsMemory(), linked.Token); process.StandardInput.Close(); }
            await process.WaitForExitAsync(linked.Token);
        }
        catch (Exception ex) when (linked.IsCancellationRequested)
        {
            if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException("进程已取消。", ex, cancellationToken);
            throw new TimeoutException($"进程超过 {timeoutSeconds} 秒未完成：{Path.GetFileName(executable)}", ex);
        }
        finally
        {
            if (!process.HasExited) { try { process.Kill(true); } catch (InvalidOperationException) { } }
            await process.WaitForExitAsync(CancellationToken.None);
            // 超时、取消和异常时也保存已产生的输出，避免丢失登录、额度或网络诊断。
            var finalOutput = await output; var finalError = await error;
            if (stdoutPath != null) await File.WriteAllTextAsync(stdoutPath, finalOutput, Encoding.UTF8);
            if (stderrPath != null) await File.WriteAllTextAsync(stderrPath, finalError, Encoding.UTF8);
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new ProcessResult(process.ExitCode, await output, await error);
    }
}

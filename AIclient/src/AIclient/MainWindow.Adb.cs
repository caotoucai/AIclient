using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AIclient.Models;
using AIclient.Services;

namespace AIclient;

public partial class MainWindow
{
    private readonly DispatcherTimer adbDiscoveryTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly AdbUsbDiscovery adbUsbDiscovery = new();
    private CancellationTokenSource? adbScanCancellation;
    private Task? adbRefreshTask;
    private int adbScanVersion;
    private string? adbSnapshot;
    private bool adbRefreshIsManual, adbDiscoveryClosed, adbDiscoverySuspended;
    private DateTimeOffset adbRetryAfter;
    private Func<string, CancellationToken, Task<string>> scanAdb = (path, token) =>
        new AdbDevice(new AppSettings { AdbPath = path }).ScanAsync(token);

    private bool CanDiscoverAdb => !adbDiscoveryClosed && !adbDiscoverySuspended && !busy && IsLoaded && IsVisible
        && WindowState != WindowState.Minimized && Tabs.SelectedItem == DevicesTab;

    private void InitializeAdbDiscovery()
    {
        scanAdb = adbUsbDiscovery.ScanAsync;
        adbUsbDiscovery.Status += (path, status) =>
        {
            if (!CanDiscoverAdb || path != AdbPathBox.Text.Trim()) return;
            SettingsMessage.Text = status;
            TraceAdbDiscovery(path, false, 0, "usb-recovery", [], status);
        };
        adbDiscoveryTimer.Tick += (_, _) => { if (CanDiscoverAdb) _ = RefreshAdbAsync(); };
        Loaded += (_, _) => { adbDiscoveryTimer.Start(); UpdateAdbDiscovery(); };
        IsVisibleChanged += (_, _) => UpdateAdbDiscovery();
        StateChanged += (_, _) => UpdateAdbDiscovery();
        Tabs.SelectionChanged += (_, e) => { if (e.Source == Tabs) UpdateAdbDiscovery(); };
        AdbPathBox.TextChanged += (_, _) =>
        {
            CancelAdbDiscovery(); adbSnapshot = null; adbRetryAfter = default;
            AdbScanList.ItemsSource = Array.Empty<string>();
            UpdateAdbDiscovery();
        };
        Closed += (_, _) => { adbDiscoveryClosed = true; adbDiscoveryTimer.Stop(); CancelAdbDiscovery(); };
    }

    private void CancelAdbDiscovery()
    {
        adbScanVersion++;
        adbScanCancellation?.Cancel();
    }

    private void UpdateAdbDiscovery()
    {
        if (CanDiscoverAdb) _ = RefreshAdbAsync();
        else CancelAdbDiscovery();
    }

    // Manual refresh supersedes a pending automatic scan; repeated manual clicks share the fresh scan.
    private Task RefreshAdbAsync(bool manual = false)
    {
        if (!CanDiscoverAdb) return Task.CompletedTask;
        if (adbRefreshTask is { IsCompleted: false })
        {
            if (!manual || adbRefreshIsManual) return adbRefreshTask;
            CancelAdbDiscovery();
        }
        if (!manual && DateTimeOffset.UtcNow < adbRetryAfter) return Task.CompletedTask;
        if (manual) SettingsMessage.Text = "正在刷新 ADB 设备…";
        var cancellation = new CancellationTokenSource();
        adbScanCancellation = cancellation;
        adbRefreshIsManual = manual;
        adbRefreshTask = ScanAdbAsync(AdbPathBox.Text.Trim(), adbScanVersion, cancellation, manual);
        return adbRefreshTask;
    }

    private async Task ScanAdbAsync(string path, int version, CancellationTokenSource cancellation, bool manual)
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation.Token);
            deadline.CancelAfter(TimeSpan.FromSeconds(3));
            string result;
            try { result = await scanAdb(path, deadline.Token).WaitAsync(deadline.Token); }
            catch (OperationCanceledException) when (!cancellation.IsCancellationRequested && deadline.IsCancellationRequested)
            { throw new TimeoutException("ADB 设备枚举 3 秒内未响应，已中止等待并将重新检测。"); }
            if (cancellation.IsCancellationRequested || version != adbScanVersion || !CanDiscoverAdb || path != AdbPathBox.Text.Trim()) return;
            var rows = result.Split('\n').Select(s => s.Trim()).Select(s => (Line: s, Parts: s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)))
                .Where(r => r.Parts.Length >= 2 && r.Parts[1] is "device" or "offline" or "unauthorized").OrderBy(r => r.Parts[0]).ToArray();
            var snapshot = string.Join("\n", rows.Select(r => r.Line));
            var changed = snapshot != adbSnapshot;
            if (changed)
            {
                var selectedSerial = (AdbScanList.SelectedItem as string)?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                var devices = rows.Where(r => r.Parts[1] == "device").Select(r => r.Line).ToArray();
                AdbScanList.ItemsSource = devices;
                AdbScanList.SelectedItem = devices.FirstOrDefault(d => d.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)[0] == selectedSerial) ?? devices.FirstOrDefault();
                adbSnapshot = snapshot;
            }
            if (changed || manual)
            {
                var online = rows.Count(r => r.Parts[1] == "device");
                var unauthorized = rows.Count(r => r.Parts[1] == "unauthorized");
                var offline = rows.Count(r => r.Parts[1] == "offline");
                SettingsMessage.Text = online > 0 ? $"发现 {online} 台在线设备，请选择后绑定。" : "未发现已授权的在线设备。";
                if (unauthorized > 0) SettingsMessage.Text += $" {unauthorized} 台设备待授权，请在设备上允许 USB 调试。";
                if (offline > 0) SettingsMessage.Text += $" {offline} 台设备离线，请检查连接。";
            }
            if (changed || manual || clock.ElapsedMilliseconds >= 1000)
                TraceAdbDiscovery(path, manual, clock.ElapsedMilliseconds, "completed", rows.Select(r => r.Line).ToArray());
            adbRetryAfter = default;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (version != adbScanVersion || !CanDiscoverAdb || path != AdbPathBox.Text.Trim()) return;
            AdbScanList.ItemsSource = Array.Empty<string>(); adbSnapshot = null;
            SettingsMessage.Text = "ADB 扫描失败：" + ex.Message;
            TraceAdbDiscovery(path, manual, clock.ElapsedMilliseconds, "failed", [], ex.Message);
            // A transient reconnect timeout must not add another five seconds of UI backoff.
            adbRetryAfter = DateTimeOffset.UtcNow.AddSeconds(ex is TimeoutException ? 1 : 5);
        }
        finally
        {
            if (ReferenceEquals(adbScanCancellation, cancellation)) adbScanCancellation = null;
            cancellation.Dispose();
        }
    }

    private void TraceAdbDiscovery(string path, bool manual, long elapsedMs, string result, string[] devices, string? error = null)
    {
        try
        {
            var folder = Path.Combine(storage.Root, "device-discovery"); Directory.CreateDirectory(folder);
            var line = System.Text.Json.JsonSerializer.Serialize(new { timestamp = DateTimeOffset.Now, adbPath = AdbDevice.ResolveExecutable(path), manual, elapsedMs, result, devices, error });
            File.AppendAllText(Path.Combine(folder, DateTime.Now.ToString("yyyyMMdd") + ".jsonl"), line + Environment.NewLine);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { Log("ADB 发现日志未保存：" + ex.Message); }
    }

    private async void ScanClick(object sender, RoutedEventArgs e) => await RefreshAdbAsync(true);
}

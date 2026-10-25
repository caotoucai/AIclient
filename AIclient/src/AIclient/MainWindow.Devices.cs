using System.Windows;
using Microsoft.Win32;
using AIclient.Models;
using AIclient.Services;

namespace AIclient;

public partial class MainWindow
{
    private async void DeviceRequested(string operation)
    {
        if (busy) return;
        try
        {
            if (operation == "scan-serial")
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");
                var values = key == null ? [] : key.GetValueNames().Select(n => key.GetValue(n)?.ToString()).OfType<string>().Distinct().OrderBy(v => v).ToArray();
                DeviceConfig.ShowSerial(values);
                DeviceConfig.Message(values.Length == 0 ? "未发现本机 COM 口，可手动输入。" : $"发现 {values.Length} 个串口，请选择并绑定。"); return;
            }
            if (operation == "scan-visa")
            {
                SetBusy(true);
                try { var resources = await Task.Run(() => { using var visa = new VisaInstrument(); return visa.Scan(); }); DeviceConfig.ShowVisa(resources); DeviceConfig.Message($"扫描到 {resources.Length} 个 VISA 资源。"); }
                finally { SetBusy(false); if (closeAfterStop) Close(); }
                return;
            }
            if (operation == "scan-bus")
            {
                SetBusy(true); runCancellation = new();
                try
                {
                    var scanDir = Path.Combine(storage.Root, "device-scans", DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N")[..6]);
                    await using var host = new LegacyBusClient(scanDir, _ => { });
                    var result = await host.CallAsync("scan", null, runCancellation.Token, 30);
                    var values = result.EnumerateArray().Select(v => v.GetString()!).ToArray(); DeviceConfig.ShowBus(values); DeviceConfig.Message($"旧库扫描到 {values.Length} 个通道；未发现设备时请检查厂商驱动。日志：{scanDir}");
                }
                finally { SetBusy(false); runCancellation.Dispose(); runCancellation = null; if (closeAfterStop) Close(); }
                return;
            }
            var config = ReadSettings();
            if (operation == "save") { settings = config; storage.SaveSettings(settings); FillSettings(); DeviceConfig.Message("设备配置已保存，与高级 JSON 同步。"); return; }
            Decision? action = operation switch
            {
                "preflight" => null,
                "power-on" => new() { Tool = "power.output", Arguments = "{\"on\":true}" },
                "power-off" => new() { Tool = "power.output", Arguments = "{\"on\":false}" },
                "power-set" => new() { Tool = "power.voltage", Arguments = Json.Serialize(new { volts = DeviceConfig.ManualVoltage }) },
                "power-voltage" => new() { Tool = "power.measure", Arguments = "{\"quantity\":\"voltage\"}" },
                "power-current" => new() { Tool = "power.measure", Arguments = "{\"quantity\":\"current\"}" },
                "send-alias" => DeviceConfig.SelectedCommand is { } command ? new() { Tool = command.Transport == "bus" ? "bus.send" : "serial.send", Arguments = Json.Serialize(new { alias = command.Alias }) } : throw new InvalidDataException("请先选择一条已配置命令。"),
                _ => throw new InvalidDataException("未知设备操作。")
            };
            config.DemoMode = false; config.AutoLearnSkills = false;
            SetBusy(true); runCancellation = new();
            DeviceConfig.Message("正在执行真实设备调用，结果和原始数据将保存在运行记录中…");
            try
            {
                var devices = new RunDevices(config, storage, Log, ShowScreenshot);
                runner = new TestRunner(storage, new DiagnosticBridge(action), devices, Log, UpdateRun);
                current = await runner.RunAsync("手动设备调用：" + operation, config, runCancellation.Token);
                ReportButton.IsEnabled = File.Exists(Path.Combine(current.Directory, "report.html")); FolderButton.IsEnabled = true;
                RefreshHistory(); DeviceConfig.Message(current.Outcome == Outcome.Inconclusive ? "设备调用完成，功能结论需结合证据判断。报告：" + current.Directory : current.Message);
            }
            finally { runner = null; runCancellation.Dispose(); runCancellation = null; SetBusy(false); if (closeAfterStop) Close(); }
        }
        catch (Exception ex) { DeviceConfig.Message(ex.Message); Log("设备操作：" + ex.Message); }
    }
    private sealed class DiagnosticBridge(Decision? action) : IAiBridge
    {
        private bool executed;
        public Task<TestPlan> PlanAsync(RunRecord run, CancellationToken token) => Task.FromResult(new TestPlan { Title = "手动设备调用", Scope = "连接、采集及所选设备方法；功能是否符合要求需另行验证", Steps = [new() { Id = "S01", Title = action?.Tool ?? "连接与采集自检", Expected = "记录真实设备调用与采集结果" }] });
        public Task<Decision> DecideAsync(RunRecord run, StepRecord step, IReadOnlyList<Evidence> evidence, CancellationToken token)
        {
            if (action != null && !executed) { executed = true; action.Kind = "action"; action.Observation = "用户在设备页手动调用配置方法。"; action.EvidenceIds = evidence.Select(e => e.Id).ToList(); return Task.FromResult(action); }
            return Task.FromResult(new Decision { Kind = "inconclusive", Observation = "设备调用与采集结束。方法返回和原始证据已记录，此自检不自动判定被测功能通过。", EvidenceIds = evidence.Select(e => e.Id).ToList() });
        }
    }
}

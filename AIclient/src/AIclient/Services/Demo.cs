using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AIclient.Models;

namespace AIclient.Services;

public sealed class DemoBridge : IAiBridge
{
    private readonly HashSet<string> acted = [];
    public async Task<TestPlan> PlanAsync(RunRecord run, CancellationToken token)
    {
        await Task.Delay(500, token);
        return new TestPlan
        {
            Title = "蓝牙基础联调 · 演示", Scope = "演示双设备观察、操作与证据报告流程",
            Uncovered = ["全部画面与设备数据均为演示模拟，未调用真实 Codex 或真实设备。", "没有验证真实蓝牙、通话和媒体声音。"],
            Steps = [new() { Id = "S01", Title = "进入双方蓝牙设置", Expected = "ADB1 与 ADB2 均显示蓝牙设置页面（模拟），不限定设备类型。" }, new() { Id = "S02", Title = "验证双方配对与连接", Expected = "双方显示目标设备已连接（模拟）。" }, new() { Id = "S03", Title = "验证断开后重连", Expected = "演示状态经过断开后恢复连接（模拟）。" }]
        };
    }
    public async Task<Decision> DecideAsync(RunRecord run, StepRecord step, IReadOnlyList<Evidence> observations, CancellationToken token)
    {
        await Task.Delay(500, token);
        if (acted.Add(step.Id)) return new Decision { Kind = "action", Observation = "演示观察：已获得双方当前画面，准备执行本步骤的模拟操作。", Tool = step.Id == "S01" ? "adb.key" : "adb.tap", Role = "adb1", Arguments = step.Id == "S01" ? "{\"key\":\"BLUETOOTH_SETTINGS\"}" : "{\"x\":760,\"y\":238}", EvidenceIds = observations.Select(e => e.Id).ToList() };
        return new Decision { Kind = "pass", Observation = $"演示判定：{step.Expected} 此结论仅用于验证软件流程，不是实际功能结果。", EvidenceIds = observations.Select(e => e.Id).ToList() };
    }
}

public sealed class DemoDevices(Storage storage, Action<Evidence, string> screenshot) : IRunDevices
{
    private int phase;
    public Task StartAsync(RunRecord run, CancellationToken token) { token.ThrowIfCancellationRequested(); return Task.CompletedTask; }
    public async Task<IReadOnlyList<Evidence>> ObserveAsync(RunRecord run, StepRecord step, CancellationToken token)
    {
        var list = new List<Evidence>();
        foreach (var role in new[] { "adb1", "adb2" })
        {
            token.ThrowIfCancellationRequested();
            var path = Path.Combine(run.Directory, "screenshots", $"demo_{run.Evidence.Count:D5}_{role}.png");
            await DrawAsync(path, role, phase);
            var e = storage.AddEvidence(run, path, "screenshot", step.Id, role, "DEMO-" + role, "模拟界面 · 非真实设备", role == "adb1" ? 1000 : 420, role == "adb1" ? 600 : 760);
            list.Add(e); screenshot(e, path);
        }
        var logPath = Path.Combine(run.Directory, "logs", $"demo_{run.Evidence.Count:D5}.json");
        Storage.WriteJson(logPath, new { simulated = true, at = DateTimeOffset.UtcNow, serial = "[DEMO] Bluetooth state=" + phase, can = "[DEMO] 示例数据，不代表真实报文" });
        list.Add(storage.AddEvidence(run, logPath, "monitor", step.Id, summary: "[DEMO] 模拟串口 / 总线观察，阶段=" + phase));
        return list;
    }
    public async Task<string> ExecuteAsync(RunRecord run, StepRecord step, Decision decision, CancellationToken token)
    {
        await Task.Delay(400, token); phase++; return "模拟动作完成，未发送任何真实设备命令。";
    }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static Task DrawAsync(string path, string role, int phase)
    {
        if (Application.Current != null) return Application.Current.Dispatcher.InvokeAsync(() => Draw(path, role, phase)).Task;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() => { try { Draw(path, role, phase); completion.SetResult(); } catch (Exception ex) { completion.SetException(ex); } });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); return completion.Task;
    }
    private static void Draw(string path, string role, int phase)
    {
        var w = role == "adb1" ? 1000 : 420; var h = role == "adb1" ? 600 : 760;
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            static SolidColorBrush B(string hex) => new((Color)ColorConverter.ConvertFromString(hex));
            void Text(string value, double size, double x, double y, string color = "#EAF2FF") => dc.DrawText(new FormattedText(value, CultureInfo.GetCultureInfo("zh-CN"), FlowDirection.LeftToRight, new Typeface("Microsoft YaHei UI"), size, B(color), 1), new Point(x, y));
            dc.DrawRectangle(B("#101A2A"), null, new Rect(0, 0, w, h));
            dc.DrawRectangle(B("#1D3048"), null, new Rect(0, 0, w, 74));
            Text(AdbSlots.Label(role) + " · Android", 24, 28, 22);
            Text("演示模拟", 15, w - 112, 29, "#F2C46D");
            Text("‹   蓝牙设置", 30, 28, 110);
            dc.DrawRoundedRectangle(B("#203248"), null, new Rect(24, 180, w - 48, 90), 14, 14);
            Text("蓝牙", 22, 44, 203); Text("已开启", 16, w - 106, 211, "#67E0C5");
            Text("我的设备", 17, 30, 302, "#94A8C5");
            dc.DrawRoundedRectangle(B("#203248"), null, new Rect(24, 342, w - 48, 110), 14, 14);
            Text(role == "adb1" ? "ADB2 Device" : "ADB1 Device", 24, 44, 362);
            Text(phase >= 2 ? "已连接 · 蓝牙音频" : "可配对设备", 16, 44, 404, phase >= 2 ? "#67E0C5" : "#94A8C5");
            Text("此图为 AIclient 生成的演示画面", 16, 28, h - 88, "#E2BD78");
            Text("阶段 " + phase + " · 无真实设备交互", 14, 28, h - 54, "#94A8C5");
        }
        var bitmap = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32); bitmap.Render(visual);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var file = File.Create(path); encoder.Save(file);
    }
}

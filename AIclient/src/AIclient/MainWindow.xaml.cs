using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AIclient.Models;
using AIclient.Services;

namespace AIclient;

public partial class MainWindow : Window
{
    private readonly Storage storage;
    private readonly TestSkills skills;
    private AppSettings settings;
    private CancellationTokenSource? runCancellation;
    private TestRunner? runner;
    private RunRecord? current;
    private bool closeAfterStop;
    private bool busy;
    public MainWindow(string dataRoot)
    {
        InitializeComponent();
        storage = new Storage(dataRoot);
        skills = new TestSkills(dataRoot);
        try { settings = storage.LoadSettings(); settings.Validate(); } catch (Exception ex) { settings = new(); Log("配置读取失败，已载入默认值（原配置未覆盖）：" + ex.Message); }
        var bundledAdb = Path.GetFullPath(Path.Combine(dataRoot, "..", "..", "Resource", "ADB", "adb.exe"));
        if (settings.AdbPath == "adb.exe" && File.Exists(bundledAdb)) settings.AdbPath = bundledAdb;
        FillSettings();
        DeviceConfig.Requested += DeviceRequested;
        RefreshSkills();
        storage.RecoverInterrupted(); RefreshHistory();
        Log("工作台就绪。演示模式可直接运行；真实模式请先绑定设备并检查 Codex 登录。");
        FooterText.Text = "运行数据：" + dataRoot;
    }
    private void Dispatch(Action action) { if (Dispatcher.CheckAccess()) action(); else Dispatcher.BeginInvoke(action); }
    private void Log(string message) => Dispatch(() =>
    {
        if (LogBox.Text.Length > 80000) LogBox.Text = LogBox.Text[^40000..];
        LogBox.AppendText($"{DateTime.Now:HH:mm:ss}  {message}\n"); LogBox.ScrollToEnd();
    });
    private void FillSettings()
    {
        CodexPathBox.Text = settings.CodexPath; PowerShellBox.Text = settings.PowerShellPath; AdbPathBox.Text = settings.AdbPath;
        Adb1Box.Text = settings.Adb1Serial; Adb2Box.Text = settings.Adb2Serial; ModelBox.Text = settings.Model; ContextBox.Text = settings.ProjectContext;
        DemoToggle.IsChecked = settings.DemoMode; AdvancedBox.Text = Json.Serialize(settings);
        DeviceConfig.Load(settings); AutoLearnToggle.IsChecked = settings.AutoLearnSkills;
        BindingSummary.Text = $"ADB1：{(settings.Adb1Serial.Length == 0 ? "未绑定" : settings.Adb1Serial)}    ADB2：{(settings.Adb2Serial.Length == 0 ? "未绑定" : settings.Adb2Serial)}";
    }
    private AppSettings ReadSettings()
    {
        var copy = Json.Read<AppSettings>(Json.Serialize(settings));
        copy.CodexPath = CodexPathBox.Text.Trim(); copy.PowerShellPath = PowerShellBox.Text.Trim(); copy.AdbPath = AdbPathBox.Text.Trim();
        copy.Adb1Serial = Adb1Box.Text.Trim(); copy.Adb2Serial = Adb2Box.Text.Trim(); copy.Model = ModelBox.Text.Trim(); copy.ProjectContext = ContextBox.Text;
        DeviceConfig.ReadInto(copy);
        copy.DemoMode = DemoToggle.IsChecked == true; copy.AutoLearnSkills = AutoLearnToggle.IsChecked == true; copy.Validate(); return copy;
    }
    private void SetBusy(bool value)
    {
        busy = value; StartButton.IsEnabled = !value; DemoToggle.IsEnabled = !value; RequestBox.IsReadOnly = value; SettingsPanel.IsEnabled = !value;
        PauseButton.IsEnabled = value; StopButton.IsEnabled = value; PauseButton.Content = "暂停";
        StatusBadge.Text = value ? "●  任务执行中" : current?.Finished != null ? "●  " + Report.Label(current.Outcome) + (current.Demo ? " · 演示" : " · 真实设备") : "●  本地工作台就绪";
    }
    private async void StartClick(object sender, RoutedEventArgs e) => await RunTaskAsync();
    public async Task<RunRecord?> RunTaskAsync()
    {
        if (busy) return null;
        try
        {
            if (string.IsNullOrWhiteSpace(RequestBox.Text)) throw new InvalidDataException("请输入测试需求。");
            settings = ReadSettings(); storage.SaveSettings(settings); AdvancedBox.Text = Json.Serialize(settings);
            runCancellation = new CancellationTokenSource();
            SetBusy(true); LogBox.Clear(); StepsList.ItemsSource = null; current = null; ReportButton.IsEnabled = false; FolderButton.IsEnabled = false;
            Adb1Image.Source = null; Adb2Image.Source = null;
            Adb1Caption.Text = "ADB1  /  等待本次任务截图"; Adb2Caption.Text = "ADB2  /  等待本次任务截图";
            IAiBridge ai = settings.DemoMode ? new DemoBridge() : new CodexBridge(settings, Log, skills);
            IRunDevices devices = settings.DemoMode ? new DemoDevices(storage, ShowScreenshot) : new RunDevices(settings, storage, Log, ShowScreenshot);
            runner = new TestRunner(storage, ai, devices, Log, UpdateRun, skills);
            current = await runner.RunAsync(RequestBox.Text.Trim(), settings, runCancellation.Token);
            ReportButton.IsEnabled = File.Exists(Path.Combine(current.Directory, "report.html")); FolderButton.IsEnabled = true;
            RefreshHistory(); RefreshLessons(); return current;
        }
        catch (Exception ex) { Log("任务无法继续：" + ex.Message); ObservationText.Text = ex.Message; return current; }
        finally { SetBusy(false); runner = null; runCancellation?.Dispose(); runCancellation = null; if (closeAfterStop) Close(); }
    }
    private void UpdateRun(RunRecord run) => Dispatch(() =>
    {
        current = run;
        StepsList.ItemsSource = run.Steps.Select(s => new { s.Id, s.Title, Detail = string.IsNullOrWhiteSpace(s.Actual) ? s.Expected : s.Actual, Status = Report.Label(s.Outcome), Color = s.Outcome switch { Outcome.Passed => "#6DDFC4", Outcome.Failed or Outcome.Error => "#FF9B9D", Outcome.Running => "#80C6FF", _ => "#A1B1C8" } }).ToList();
        StepCount.Text = $"{run.Steps.Count(s => s.Outcome == Outcome.Passed)} / {run.Steps.Count}";
        ScopeText.Text = (run.Demo ? "[演示模拟] " : "") + (run.Plan?.Scope ?? "正在检查设备并生成计划…");
        EvidenceCount.Text = run.Evidence.Count + " 个证据";
        var active = run.Steps.LastOrDefault(s => s.Outcome is not (Outcome.Pending or Outcome.Skipped));
        ObservationText.Text = active?.Actual.Length > 0 ? active.Actual : run.Message;
        if (run.Finished != null) StatusBadge.Text = "●  " + Report.Label(run.Outcome) + (run.Demo ? " · 演示" : " · 真实设备");
    });
    private void ShowScreenshot(Evidence evidence, string path) => Dispatch(() =>
    {
        var bitmap = new BitmapImage(); bitmap.BeginInit(); bitmap.CacheOption = BitmapCacheOption.OnLoad; bitmap.UriSource = new Uri(path); bitmap.EndInit(); bitmap.Freeze();
        var caption = $"{AdbSlots.Label(evidence.Role)}  /  {evidence.Device}  ·  {evidence.Width} × {evidence.Height}  ·  {evidence.CapturedAt.ToLocalTime():HH:mm:ss}";
        if (evidence.Role == "adb1") { Adb1Image.Source = bitmap; Adb1Caption.Text = caption; } else { Adb2Image.Source = bitmap; Adb2Caption.Text = caption; }
    });
    private void PauseClick(object sender, RoutedEventArgs e)
    {
        runner?.TogglePause(); PauseButton.Content = runner?.Paused == true ? "继续" : "暂停";
        Log(runner?.Paused == true ? "已请求暂停：当前原子操作完成后不再发新动作，设备采集继续。" : "继续执行。");
    }
    private void StopClick(object sender, RoutedEventArgs e) { runCancellation?.Cancel(); StopButton.IsEnabled = false; Log("已请求停止，正在收尾并保留证据…"); }
    private void SaveSettingsClick(object sender, RoutedEventArgs e)
    {
        try { settings = ReadSettings(); storage.SaveSettings(settings); FillSettings(); SettingsMessage.Text = "已保存到本地"; } catch (Exception ex) { SettingsMessage.Text = ex.Message; }
    }
    private void ApplyJsonClick(object sender, RoutedEventArgs e)
    {
        try { var parsed = Json.Read<AppSettings>(AdvancedBox.Text); parsed.Validate(); settings = parsed; storage.SaveSettings(settings); FillSettings(); SettingsMessage.Text = "JSON 设置已应用"; } catch (Exception ex) { SettingsMessage.Text = "JSON 无效：" + ex.Message; }
    }
    private void ExampleClick(object sender, RoutedEventArgs e)
    {
        var example = ReadSettings();
        example.SerialPorts = [new() { Name = "serial1", Port = "COM3", Enabled = false }, new() { Name = "power", Port = "COM4", BaudRate = 9600, Enabled = false }];
        example.Commands = [new() { Alias = "query_status", Description = "请替换成实际设备协议后再启用通道", Channel = "serial1", Payload = "STATUS?" }, new() { Alias = "example_can", Description = "仅为格式示例，须按真实项目修改", Transport = "bus", Payload = "t1232AABB" }];
        example.Bus.Serial.Port = "COM5";
        AdvancedBox.Text = Json.Serialize(example); SettingsMessage.Text = "示例已载入编辑器，通道保持关闭；修改后应用。";
    }
    private async void ScanClick(object sender, RoutedEventArgs e)
    {
        try { var result = await new AdbDevice(ReadSettings()).ScanAsync(CancellationToken.None); AdbScanList.ItemsSource = result.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0 && !s.StartsWith("List of devices")).ToList(); SettingsMessage.Text = "扫描完成；选择在线设备并绑定到 ADB1 或 ADB2，设备类型不限。"; }
        catch (Exception ex) { SettingsMessage.Text = "ADB 扫描失败：" + ex.Message; }
    }
    private void BindAdb1Click(object sender, RoutedEventArgs e) => Bind(true);
    private void BindAdb2Click(object sender, RoutedEventArgs e) => Bind(false);
    private void Bind(bool firstSlot)
    {
        if (AdbScanList.SelectedItem is not string line) { SettingsMessage.Text = "请先扫描并选择设备。"; return; }
        var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || parts[1] != "device") { SettingsMessage.Text = "只能绑定已授权、在线的 ADB 设备。"; return; }
        if (firstSlot) Adb1Box.Text = parts[0]; else Adb2Box.Text = parts[0]; SettingsMessage.Text = "已填写绑定，请保存基础设置。";
    }
    private async void CheckCodexClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var config = ReadSettings();
            // 固定脚本通过位置参数调用，不将输入拼接进 PowerShell 源码。
            var script = Path.Combine(storage.Root, "check-codex.ps1");
            File.WriteAllText(script, "param([string]$CodexPath)\n$ErrorActionPreference='Stop'\n& $CodexPath --version\n& $CodexPath login status\nexit $LASTEXITCODE\n");
            var r = await ProcessRunner.RunAsync(config.PowerShellPath, ["-NoProfile", "-NonInteractive", "-File", script, "-CodexPath", config.CodexPath], 20, CancellationToken.None);
            SettingsMessage.Text = (r.ExitCode == 0 ? "Codex 检查完成：" : "Codex 检查未通过：") + r.Output.Trim() + " " + r.Error.Trim();
        }
        catch (Exception ex) { SettingsMessage.Text = ex.Message; }
    }
    private void ReportClick(object sender, RoutedEventArgs e) { if (current != null) OpenPath(Path.Combine(current.Directory, "report.html")); }
    private void FolderClick(object sender, RoutedEventArgs e) { if (current != null) OpenPath(current.Directory); }
    private void OpenPath(string path) { try { if (!File.Exists(path) && !Directory.Exists(path)) throw new FileNotFoundException("文件尚未生成。", path); Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); } catch (Exception ex) { Log(ex.Message); } }
    private sealed record HistoryItem(string Title, string Detail, string Status, RunRecord Run);
    private void RefreshHistory() => HistoryList.ItemsSource = storage.History().Take(100).Select(r => new HistoryItem(r.Plan?.Title ?? "未生成计划的任务", $"{r.Started.ToLocalTime():yyyy-MM-dd HH:mm:ss}  ·  {r.Id}  ·  {r.Request}", Report.Label(r.Outcome) + (r.Demo ? " / 演示" : " / 真实"), r)).ToList();
    private void RefreshHistoryClick(object sender, RoutedEventArgs e) => RefreshHistory();
    private void OpenHistoryReportClick(object sender, RoutedEventArgs e) { if (HistoryList.SelectedItem is HistoryItem h) OpenPath(Path.Combine(h.Run.Directory, "report.html")); }
    private void OpenHistoryFolderClick(object sender, RoutedEventArgs e) { if (HistoryList.SelectedItem is HistoryItem h) OpenPath(h.Run.Directory); }
    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!busy) return; e.Cancel = true; closeAfterStop = true; runCancellation?.Cancel(); Log("等待任务收尾后退出…");
    }
    public async void StartSmoke()
    {
        await Task.Delay(500); DemoToggle.IsChecked = true;
        var result = await RunTaskAsync();
        await Task.Delay(300);
        var path = Path.Combine(storage.Root, "ui-smoke.png");
        SaveWindowScreenshot(path);
        Tabs.SelectedIndex = 1;
        await Task.Delay(250);
        var settingsPath = Path.Combine(storage.Root, "settings-smoke.png");
        SaveWindowScreenshot(settingsPath);
        var sample = Json.Read<AppSettings>(Json.Serialize(settings));
        sample.SerialPorts = [new() { Name = "serial1", Port = "COM3" }, new() { Name = "power", Port = "COM4", BaudRate = 9600 }];
        sample.Commands = [new() { Alias = "query_status", Channel = "serial1", Payload = "STATUS?", Description = "界面检查示例，不连接设备" }];
        sample.Bus.Adapter = "Legacy"; sample.Bus.Protocol = "CAN FD"; sample.Bus.DeviceName = "UI-only fixture";
        sample.Power.Resource = "USB::ui-fixture::INSTR"; sample.Power.MeasurementResource = "USB::meter-fixture::INSTR";
        DeviceConfig.Load(sample);
        var roundtrip = new AppSettings(); DeviceConfig.ReadInto(roundtrip);
        var deviceRoundtrip = roundtrip.SerialPorts.Count == 2 && roundtrip.Bus.Protocol == "CAN FD" && roundtrip.Power.MeasurementResource == sample.Power.MeasurementResource && roundtrip.Commands.Single().Alias == "query_status";
        var deviceBindingChecks = DeviceConfig.CheckBindingWorkflow(sample);
        for (var section = 0; section < 4; section++)
        {
            DeviceConfig.ShowSection(section); await Task.Delay(150);
            SaveWindowScreenshot(Path.Combine(storage.Root, $"device-section-{section}.png"));
        }
        var originalWidth = Width; var originalHeight = Height;
        Width = MinWidth; Height = MinHeight;
        for (var section = 0; section < 3; section++)
        {
            DeviceConfig.ShowSection(section); await Task.Delay(150);
            SaveWindowScreenshot(Path.Combine(storage.Root, $"device-narrow-{section}.png"));
        }
        Width = originalWidth; Height = originalHeight;
        Tabs.SelectedIndex = 2;
        await Task.Delay(250);
        var skillsPath = Path.Combine(storage.Root, "skills-smoke.png"); SaveWindowScreenshot(skillsPath);
        Storage.WriteJson(Path.Combine(storage.Root, "ui-smoke.json"), new { success = result?.Outcome == Outcome.Passed && deviceRoundtrip, deviceRoundtrip, deviceBindingChecks, screenshot = path, settingsScreenshot = settingsPath, skillsScreenshot = skillsPath, report = result == null ? "" : Path.Combine(result.Directory, "report.html") });
        Close();
    }
    private void SaveWindowScreenshot(string path)
    {
        var image = new RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Pbgra32); image.Render(this);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(image)); using var file = File.Create(path); encoder.Save(file);
    }
}

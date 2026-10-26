using System.Windows;
using System.Windows.Controls;
using AIclient.Models;
using AIclient.Services;

namespace AIclient;

public partial class MainWindow
{
    private async Task<string[]> CheckAdbDiscoveryAsync()
    {
        var passed = new List<string>();
        void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidDataException("ADB 自动发现自检失败：" + name);
            passed.Add(name);
        }
        var originalScanner = scanAdb; var originalPath = AdbPathBox.Text;
        var originalAdb1 = Adb1Box.Text; var originalAdb2 = Adb2Box.Text;
        var originalSettings = Json.Serialize(settings);
        adbDiscoveryTimer.Stop(); CancelAdbDiscovery();
        if (adbRefreshTask != null) await adbRefreshTask;
        try
        {
            var output = "List of devices attached\n";
            var calls = 0;
            scanAdb = (_, _) => { calls++; return Task.FromResult(output); };
            adbDiscoverySuspended = false; adbSnapshot = null; adbRetryAfter = default;
            await RefreshAdbAsync();
            Check(AdbScanList.Items.Count == 0 && !BindAdb1Button.IsEnabled, "空列表不允许绑定");
            adbDiscoveryTimer.Start();
            var clock = System.Diagnostics.Stopwatch.StartNew();
            output = "List of devices attached\nfixture-b device model:Phone\n";
            while (AdbScanList.Items.Count == 0 && clock.ElapsedMilliseconds < 2500) await Task.Delay(25);
            Check(AdbScanList.Items.Count == 1 && clock.ElapsedMilliseconds < 2500 && calls >= 2, "模拟接入由一秒轮询自动发现，无需手动刷新");
            adbDiscoveryTimer.Stop();
            Adb1Box.Text = "fixture-b";
            UnbindAdbClick(new Button { Tag = "adb1" }, new());
            Check(Adb1Box.Text.Length == 0 && AdbScanList.Items.Count == 1, "解绑ADB1不清除实际可用设备或停止发现");
            output = "List of devices attached\n"; await RefreshAdbAsync();
            output = "List of devices attached\nfixture-b device model:Phone transport_id:2\n";
            await RefreshAdbAsync(true);
            Check(Adb1Box.Text.Length == 0 && AdbScanList.Items.Count == 1 && ((string)AdbScanList.SelectedItem).Contains("transport_id:2"),
                "解绑后模拟拔出重插再手动刷新可发现新transport且不自动绑定");
            Adb1Box.Text = originalAdb1;
            var items = AdbScanList.ItemsSource;
            SettingsMessage.Text = "保留待保存提示";
            await RefreshAdbAsync();
            Check(ReferenceEquals(items, AdbScanList.ItemsSource) && SettingsMessage.Text == "保留待保存提示", "设备未变时不重建列表或覆盖操作提示");
            output += "fixture-a device model:NewPhone\n";
            await RefreshAdbAsync();
            Check(AdbScanList.Items.Count == 2 && ((string)AdbScanList.SelectedItem).StartsWith("fixture-b "), "新设备出现后保留用户已选设备");
            output = "List of devices attached\nfixture-b unauthorized\nfixture-a offline\n";
            await RefreshAdbAsync();
            Check(AdbScanList.Items.Count == 0 && !BindAdb1Button.IsEnabled && SettingsMessage.Text.Contains("待授权") && SettingsMessage.Text.Contains("离线")
                && Adb1Box.Text == originalAdb1 && Adb2Box.Text == originalAdb2, "授权和离线提示及时更新且保留已有绑定");

            var pending = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            calls = 0;
            scanAdb = (_, _) => { calls++; return pending.Task; };
            var first = RefreshAdbAsync(true); var second = RefreshAdbAsync(true);
            Check(ReferenceEquals(first, second) && calls == 1, "连续刷新合并为同一请求，不堆积扫描");
            Tabs.SelectedItem = BenchTab;
            SettingsMessage.Text = "已离开设备页";
            pending.SetResult("stale device model:Old\n"); await first;
            Check(AdbScanList.Items.Count == 0 && SettingsMessage.Text == "已离开设备页", "离开页面后迟到扫描不能回写");

            pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
            string? scannedPath = null;
            scanAdb = (path, _) =>
            {
                if (path == "different-adb.exe") { scannedPath = path; return Task.FromResult("new-path device model:Phone\n"); }
                return pending.Task;
            };
            Tabs.SelectedItem = DevicesTab; // Starts the pending scan immediately.
            var oldPathTask = adbRefreshTask!;
            AdbPathBox.Text = "different-adb.exe";
            pending.SetResult("stale device model:OldPath\n"); await oldPathTask;
            Check(!AdbScanList.Items.Cast<string>().Any(s => s.StartsWith("stale ")), "更换ADB路径后拒绝旧路径迟到结果");
            await RefreshAdbAsync();
            Check(scannedPath == "different-adb.exe" && ((string)AdbScanList.Items[0]).StartsWith("new-path "), "新路径立即用于后续发现");

            pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
            CancellationToken cancelledAuto = default;
            calls = 0;
            scanAdb = (_, token) =>
            {
                if (++calls == 1) { cancelledAuto = token; return pending.Task; }
                return Task.FromResult("reconnected device model:Phone\n");
            };
            var stuckAuto = RefreshAdbAsync();
            clock.Restart(); await RefreshAdbAsync(true);
            Check(calls == 2 && cancelledAuto.IsCancellationRequested && clock.ElapsedMilliseconds < 1500
                && ((string)AdbScanList.Items[0]).StartsWith("reconnected "), "手动刷新抢占卡住的自动扫描，不等待旧请求");
            pending.SetResult("old-transport device model:Stale\n"); await stuckAuto;
            Check(((string)AdbScanList.Items[0]).StartsWith("reconnected "), "被抢占的旧连接结果不能覆盖重连设备");

            pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
            CancellationToken timedOutScan = default;
            scanAdb = (_, token) => { timedOutScan = token; return pending.Task; };
            clock.Restart(); await RefreshAdbAsync(true);
            Check(timedOutScan.IsCancellationRequested && clock.ElapsedMilliseconds < 6000
                && SettingsMessage.Text.Contains("3 秒") && AdbScanList.Items.Count == 0,
                "忽略取消的扫描也受三秒等待上限约束，清除不可用选项");
            scanAdb = (_, _) => Task.FromResult("after-timeout device model:Phone\n");
            clock.Restart(); await RefreshAdbAsync(true);
            Check(clock.ElapsedMilliseconds < 1500 && ((string)AdbScanList.Items[0]).StartsWith("after-timeout "),
                "超时后手动刷新立即重试，不叠加自动退避等待");
            pending.SetResult("timeout-stale device model:Stale\n");
            SetBusy(true); scannedPath = null; await RefreshAdbAsync(true);
            Check(scannedPath == null && Json.Serialize(settings) == originalSettings, "运行中暂停发现且扫描不写入用户配置");
        }
        finally
        {
            adbDiscoverySuspended = true; CancelAdbDiscovery(); adbDiscoveryTimer.Stop();
            scanAdb = originalScanner; AdbPathBox.Text = originalPath;
            SetBusy(false); adbSnapshot = null; adbRetryAfter = default;
            AdbScanList.ItemsSource = Array.Empty<string>(); SettingsMessage.Text = "";
        }
        return passed.ToArray();
    }

    // Only used by --smoke with a dedicated data root, without Codex or hardware.
    private string[] CheckSettingsWorkflow()
    {
        var passed = new List<string>();
        void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidDataException("设置界面自检失败：" + name);
            passed.Add(name);
        }
        static bool Within(DependencyObject child, DependencyObject parent)
        {
            for (DependencyObject? node = child; node != null; node = LogicalTreeHelper.GetParent(node))
                if (node == parent) return true;
            return false;
        }
        var original = Json.Serialize(settings);
        try
        {
            Check(Within(CodexPathBox, AiTab) && Within(SkillManualBox, AiTab) && !Within(AdbPathBox, AiTab)
                && Within(AdbPathBox, DevicesTab) && Within(ContextBox, BenchTab), "Codex 与 Skills 集中在 AI 页，ADB 与台架背景分别独立");
            Check(!AdbScanList.IsEditable && Adb1Box.IsReadOnly && Adb2Box.IsReadOnly, "ADB 只能通过发现列表选择绑定");
            AdbScanList.ItemsSource = new[] { "fixture-1 device model:test", "fixture-2 device model:test", "fixture-offline offline" };
            Adb1Box.Clear(); Adb2Box.Clear();
            AdbScanList.SelectedIndex = 0; Bind(true); Bind(false);
            Check(Adb1Box.Text == "fixture-1" && Adb2Box.Text.Length == 0, "ADB 重复设备绑定被拒绝");
            AdbScanList.SelectedIndex = 1; Bind(false);
            Check(Adb2Box.Text == "fixture-2", "ADB 两通道分别绑定选中设备");
            AdbScanList.SelectedIndex = 2; Bind(true);
            Check(Adb1Box.Text == "fixture-1", "ADB 离线设备不能绑定");
            UnbindAdbClick(new Button { Tag = "adb2" }, new());
            Check(Adb2Box.Text.Length == 0 && Adb1Box.Text == "fixture-1", "ADB 解绑只影响目标通道");
            AdbScanList.ItemsSource = Array.Empty<string>();
            Check(!BindAdb1Button.IsEnabled && !BindAdb2Button.IsEnabled, "无扫描选项时禁用 ADB 绑定");
            FillSettings();
            var previousContext = settings.ProjectContext; var previousAdb = settings.Adb1Serial;
            ModelBox.Text = "ui-smoke-model"; ContextBox.Text = "待保存的台架背景"; Adb1Box.Text = "draft-adb1";
            SaveScopedSettings("ai");
            var saved = storage.LoadSettings();
            Check(saved.Model == "ui-smoke-model" && saved.ProjectContext == previousContext && saved.Adb1Serial == previousAdb
                && ContextBox.Text == "待保存的台架背景" && Adb1Box.Text == "draft-adb1", "保存 AI 设置保留其他页面待保存内容且不提前写盘");
            SaveScopedSettings("bench");
            saved = storage.LoadSettings();
            Check(saved.ProjectContext == ContextBox.Text && saved.Model == "ui-smoke-model" && saved.Adb1Serial == previousAdb, "台架背景独立保存");
            MaxAiCallsBox.Text = "invalid";
            SaveScopedSettings("devices");
            saved = storage.LoadSettings();
            Check(saved.Adb1Serial == "draft-adb1" && saved.MaxAiCalls == settings.MaxAiCalls, "设备保存不受 AI 页未保存的非法输入影响");
            var diskBeforeInvalid = Json.Serialize(saved);
            var rejected = false;
            try { SaveScopedSettings("ai"); } catch (InvalidDataException) { rejected = true; }
            Check(rejected && Json.Serialize(storage.LoadSettings()) == diskBeforeInvalid, "AI 非法预算拒绝保存且不改写配置");
            FillSettings();
            SetBusy(true);
            Check(!AiConfigPanel.IsEnabled && !SettingsPanel.IsEnabled && !BenchPanel.IsEnabled && !AdvancedPanel.IsEnabled
                && SkillManualBox.IsEnabled, "运行中锁定配置并允许更新测试知识");
            SetBusy(false);
            Check(AiConfigPanel.IsEnabled && SettingsPanel.IsEnabled && BenchPanel.IsEnabled && AdvancedPanel.IsEnabled, "结束后恢复各页编辑");
        }
        finally
        {
            SetBusy(false); settings = Json.Read<AppSettings>(original); storage.SaveSettings(settings); FillSettings();
            SettingsMessage.Text = ""; AiSettingsMessage.Text = ""; BenchMessage.Text = "";
        }
        return passed.ToArray();
    }
}

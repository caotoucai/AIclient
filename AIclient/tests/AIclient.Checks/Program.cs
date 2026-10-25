using System.IO;
using System.Security.Cryptography;
using AIclient.Models;
using AIclient.Services;

// 当前检查程序可作为受控 ADB 替身被再次启动，用于验证真实设备层的 serial 路由与 PNG 拉取。
if (args.Length > 0 && args[0] is "devices" or "-s")
{
    var fixtureRoot = Environment.GetEnvironmentVariable("AICLIENT_CHECK_ADB_ROOT") ?? throw new InvalidOperationException("Missing ADB check fixture root.");
    File.AppendAllText(Path.Combine(fixtureRoot, "commands.jsonl"), System.Text.Json.JsonSerializer.Serialize(args) + "\n");
    if (args[0] == "devices") { Console.WriteLine("List of devices attached\nphone-a\tdevice model:Android_Phone\nphone-b\tdevice model:Android_Phone"); return; }
    if (args.Length < 3 || args[1] is not ("phone-a" or "phone-b")) throw new InvalidOperationException("Unexpected fixture serial.");
    if (args[2] == "get-state") Console.WriteLine("device");
    else if (args[2] == "pull") File.Copy(Path.Combine(fixtureRoot, args[1] + ".png"), args[4], true);
    else if (args[2] == "shell" && args.Length > 3 && args[3] == "getprop") Console.WriteLine("fixture/android_phone/" + args[1]);
    else if (args[2] != "shell") throw new InvalidOperationException("Unexpected fixture command.");
    return;
}

var root = Path.GetFullPath(args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "checks"));
Directory.CreateDirectory(root);
var checks = new List<string>();
void Assert(bool value, string name) { if (!value) throw new Exception("FAILED: " + name); checks.Add(name); Console.WriteLine("PASS " + name); }
void Rejected(Action action, string name) { try { action(); } catch (InvalidDataException) { Assert(true, name); return; } throw new Exception("FAILED, not rejected: " + name); }

var little = new DbcSignal(0x123, false, "rpm", 0, 16, true, false, 0.5, -5, "rpm");
Assert(Math.Abs(little.Decode([0x10, 0x00]) - 3) < 0.0001, "DBC little endian with scaling");
var motorola = new DbcSignal(0x123, false, "big", 7, 16, false, false, 1, 0, "");
Assert(motorola.Decode([0x12, 0x34]) == 0x1234, "DBC Motorola bit order");
var signed = new DbcSignal(0x123, false, "signed", 0, 12, true, true, 1, 0, "");
Assert(signed.Decode([0xFF, 0x0F]) == -1, "DBC signed extension");
var f = Slcan.Parse("t1232AABB");
Assert(f != null && f.Id == 0x123 && f.Data.SequenceEqual(new byte[] { 0xAA, 0xBB }) && Slcan.Format(f) == "t1232AABB", "SLCAN roundtrip");
Assert(Slcan.Parse("tFFF8") == null && Slcan.Parse("t1239AABB") == null && Slcan.Parse("t1232ZZBB") == null, "SLCAN malformed frames rejected");
var dbcPath = Path.Combine(root, "basic.dbc");
File.WriteAllText(dbcPath, "BO_ 291 Sample: 8 NODE\n SG_ Speed : 0|16@1+ (0.1,0) [0|250] \"km/h\" NODE\n SG_ Mux m1 : 16|8@1+ (1,0) [0|255] \"\" NODE\n");
var db = DbcDatabase.Load(dbcPath);
Assert(db.Signals.Count == 1 && db.Warnings.Count == 1 && db.Signals[0].Name == "Speed", "DBC parser excludes unsupported multiplexed signals");
File.AppendAllText(dbcPath, "NS_ :\n SIG_VALTYPE_\n SG_MUL_VAL_\n");
Assert(DbcDatabase.Load(dbcPath).Signals.Count == 1, "DBC namespace declarations do not imply unsupported signal data");

Rejected(() => TestRunner.ValidateDecision(new() { Kind = "pass", Observation = "looks good" }, []), "Ungrounded pass rejected");
Rejected(() => TestRunner.ValidateDecision(new() { Kind = "pass", Observation = "looks good", EvidenceIds = ["old"] }, [new() { Id = "current" }]), "Stale evidence rejected");
Rejected(() => new AppSettings { Adb1Serial = "same", Adb2Serial = "same" }.Validate(), "Duplicate ADB binding rejected");

var legacy = Json.Read<AppSettings>("{\"headunitSerial\":\"old-a\",\"phoneSerial\":\"old-b\"}");
Assert(legacy.Adb1Serial == "old-a" && legacy.Adb2Serial == "old-b", "Legacy type-based settings migrate to ADB1 and ADB2");
var canonical = Json.Serialize(legacy);
Assert(canonical.Contains("\"adb1Serial\"") && canonical.Contains("\"adb2Serial\"") && !canonical.Contains("headunitSerial") && !canonical.Contains("phoneSerial"), "Saved settings contain only neutral ADB binding names");
var precedence = Json.Read<AppSettings>("{\"adb1Serial\":\"\",\"headunitSerial\":\"old-a\",\"phoneSerial\":\"old-b\",\"adb2Serial\":\"new-b\"}");
Assert(precedence.Adb1Serial == "" && precedence.Adb2Serial == "new-b", "Explicit new bindings override legacy fields independent of JSON order");
var oldRun = Json.Read<RunRecord>("{\"settingsSnapshot\":{\"headunitSerial\":\"old-a\",\"phoneSerial\":\"old-b\"}}");
Assert(oldRun.SettingsSnapshot.Adb1Serial == "old-a" && AdbSlots.Label("headunit") == "ADB1" && AdbSlots.Label("phone") == "ADB2", "Historical snapshots and evidence labels remain readable");
var twoPhones = new AppSettings { Adb1Serial = "phone-a", Adb2Serial = "phone-b", ProjectContext = "ADB1 和 ADB2 都是 Android 手机，进行双手机联调。", DemoMode = false };
twoPhones.Validate();
Assert(new AdbDevice(twoPhones).Serial("ADB1") == "phone-a" && new AdbDevice(twoPhones).Serial("adb2") == "phone-b", "Two phones route by neutral slot regardless of device type");
Rejected(() => TestRunner.ValidateDecision(new() { Kind = "action", Observation = "old selector", Tool = "adb.tap", Role = "headunit" }, []), "New ADB actions cannot route using legacy device-type selectors");
var neutralCapabilities = Json.Read<System.Text.Json.JsonElement>(Json.Serialize(new CodexBridge(twoPhones, _ => { }).Capabilities()));
Assert(neutralCapabilities.GetProperty("adbRoles").EnumerateArray().Select(x => x.GetString()).SequenceEqual(new[] { "adb1", "adb2" }) && neutralCapabilities.GetProperty("deviceUsage").GetString()!.Contains("两台均为手机"), "AI capabilities identify neutral slots and explicitly allow two phones");

var store = new Storage(Path.Combine(root, "data"));
var settings = new AppSettings { DemoMode = true };
var demo = await new TestRunner(store, new DemoBridge(), new DemoDevices(store, (_, _) => { }), _ => { }, _ => { }).RunAsync("<script>alert('x')</script> & 蓝牙", settings, CancellationToken.None);
Assert(demo.Outcome == Outcome.Passed && demo.Steps.Count == 3 && demo.Actions.Count == 3, "Complete demo execution pipeline");
var report = File.ReadAllText(Path.Combine(demo.Directory, "report.html"));
Assert(report.Contains("&lt;script&gt;") && !report.Contains("<script>alert") && report.Contains("演示模拟数据"), "HTML report escapes inputs and labels demo");
Assert(demo.Evidence.All(e => File.Exists(Path.Combine(demo.Directory, e.Path)) && Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(demo.Directory, e.Path)))) == e.Sha256), "Every evidence file exists and SHA256 matches");
Assert(demo.Actions.All(a => a.Success && a.Finished != null) && demo.Evidence.Count(e => e.Kind == "screenshot") == 12, "Before/after screenshots and actual actions retained");

var fixtureDir = Path.Combine(root, "two-phones-adb"); Directory.CreateDirectory(fixtureDir);
File.WriteAllText(Path.Combine(fixtureDir, "commands.jsonl"), "");
var portrait = demo.Evidence.First(e => e.Role == "adb2" && e.Kind == "screenshot");
foreach (var serial in new[] { "phone-a", "phone-b" }) File.Copy(Path.Combine(demo.Directory, portrait.Path), Path.Combine(fixtureDir, serial + ".png"), true);
twoPhones.AdbPath = Environment.ProcessPath ?? throw new InvalidOperationException("Cannot locate check executable.");
var oldFixtureRoot = Environment.GetEnvironmentVariable("AICLIENT_CHECK_ADB_ROOT");
RunRecord phoneRun;
try
{
    Environment.SetEnvironmentVariable("AICLIENT_CHECK_ADB_ROOT", fixtureDir);
    phoneRun = await new TestRunner(store, new TwoPhonesBridge(), new RunDevices(twoPhones, store, _ => { }, (_, _) => { }), _ => { }, _ => { }).RunAsync("自动检查：ADB1 和 ADB2 均为模拟手机，分别截图和点击，不涉及真实蓝牙验收。", twoPhones, CancellationToken.None);
}
finally { Environment.SetEnvironmentVariable("AICLIENT_CHECK_ADB_ROOT", oldFixtureRoot); }
Assert(phoneRun.Outcome == Outcome.Passed && phoneRun.Actions.Select(a => a.Role).SequenceEqual(new[] { "adb1", "adb2" }), "Full device-layer two-phone run executes one action on each neutral slot");
var commands = File.ReadAllLines(Path.Combine(fixtureDir, "commands.jsonl")).Select(Json.Read<string[]>).ToList();
Assert(new[] { "phone-a", "phone-b" }.All(serial => commands.Count(c => c.Length > 4 && c[1] == serial && c[2] == "shell" && c[3] == "input" && c[4] == "tap") == 1), "ADB1 and ADB2 taps use their distinct bound serials");
Assert(phoneRun.Evidence.Where(e => e.Kind == "screenshot").All(e => e.Width == 420 && e.Height == 760 && e.Device == (e.Role == "adb1" ? "phone-a" : "phone-b")), "Both portrait-phone captures preserve correct slot, identity and resolution");
var phoneReport = File.ReadAllText(Path.Combine(phoneRun.Directory, "report.html"));
Assert(phoneReport.Contains("ADB1：phone-a") && phoneReport.Contains("ADB2：phone-b") && !phoneReport.Contains("车机："), "Report labels both devices neutrally without forcing a headunit");

var fakeCodex = Path.Combine(root, "fake codex with spaces.ps1");
File.WriteAllText(fakeCodex, """
    $ErrorActionPreference = 'Stop'
    $promptText = $input | Out-String
    $outIndex = [Array]::IndexOf($args, '--output-last-message')
    $schemaIndex = [Array]::IndexOf($args, '--output-schema')
    $schema = Get-Content -LiteralPath $args[$schemaIndex + 1] -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($schema.properties.title) {
      $result = @{title='Fake CLI';scope='Bridge contract';uncovered=@('fake, not real Codex');steps=@(@{id='S01';title='Observe';expected='Current evidence is provided'})}
    } else {
      $ids = [regex]::Matches($promptText, '"id":\s*"(E[0-9]+)"')
      if ($ids.Count -eq 0) { throw 'Evidence not present in prompt' }
      $result = @{kind='pass';observation='Fake decision cites current evidence';tool='';role='';arguments='{}';evidenceIds=@($ids[$ids.Count - 1].Groups[1].Value)}
      $result.lessons = @(@{condition='Fixture-only special case';guidance='Observed current fixture monitor; recheck next time';evidenceIds=@($ids[$ids.Count - 1].Groups[1].Value)})
    }
    [IO.File]::WriteAllText($args[$outIndex + 1], ($result | ConvertTo-Json -Depth 10), (New-Object Text.UTF8Encoding($false)))
    Write-Output '{"type":"fake.event"}'
    exit 0
    """);
var bridgeSettings = new AppSettings { CodexPath = fakeCodex, DemoMode = false };
var testSkills = new TestSkills(store.Root);
testSkills.SaveManual("fixture-specific manual knowledge: pair only with configured target");
var bridge = new CodexBridge(bridgeSettings, _ => { }, testSkills);
var sentinel = Path.Combine(root, "should-not-exist.txt");
var injection = "测试 $([IO.File]::WriteAllText('" + sentinel + "','executed'))";
var bridgeRun = await new TestRunner(store, bridge, new StubDevices(store), _ => { }, _ => { }, testSkills).RunAsync(injection, bridgeSettings, CancellationToken.None);
Assert(bridgeRun.Outcome == Outcome.Passed && !File.Exists(sentinel), "PowerShell Bridge preserves prompt data without execution");
Assert(Directory.EnumerateFiles(Path.Combine(bridgeRun.Directory, "ai"), "response.json", SearchOption.AllDirectories).Count() == 2, "Bridge plan and decision final JSON persisted");
var skillFiles = Directory.EnumerateFiles(Path.Combine(bridgeRun.Directory, "ai"), "skills.json", SearchOption.AllDirectories).ToArray();
Assert(skillFiles.Length == 2 && skillFiles.All(p => File.ReadAllText(p).Contains("fixture-specific manual knowledge")), "Plan and decision archive the actual manual skill snapshot");
Assert(testSkills.ReadLessons().Count == 1 && testSkills.ReadLessons()[0].RunId == bridgeRun.Id && testSkills.ReadLessons()[0].EvidenceIds.Count == 1, "Validated decision automatically persists a lesson with source evidence");
var currentEvidence = bridgeRun.Evidence.Where(e => e.Kind == "monitor").ToArray();
var lessonDecision = new Decision { Kind = "pass", Observation = "fixture", EvidenceIds = currentEvidence.Select(e => e.Id).ToList(), Lessons = [new() { Condition = "Fixture-only special case", Guidance = "Observed current fixture monitor; recheck next time", EvidenceIds = currentEvidence.Select(e => e.Id).ToList() }] };
Assert(testSkills.Learn(bridgeRun, bridgeRun.Steps[0], lessonDecision, currentEvidence) == 0, "Duplicate learned guidance is not appended twice");
lessonDecision.Lessons[0].EvidenceIds = ["old-or-invented"];
Assert(testSkills.Learn(bridgeRun, bridgeRun.Steps[0], lessonDecision, currentEvidence) == 0, "Learning rejects stale or invented evidence");
lessonDecision.Lessons[0].EvidenceIds = currentEvidence.Select(e => e.Id).ToList();
Assert(testSkills.Learn(demo, demo.Steps[0], lessonDecision, currentEvidence) == 0, "Demo results never enter the real test skill library");
await bridge.DecideAsync(bridgeRun, bridgeRun.Steps[0], currentEvidence, CancellationToken.None);
Assert(Directory.EnumerateFiles(Path.Combine(bridgeRun.Directory, "ai"), "skills.json", SearchOption.AllDirectories).Any(p => File.ReadAllText(p).Contains("Fixture-only special case")), "The next analysis receives the newly learned lesson");
var otherBench = Json.Read<AppSettings>(Json.Serialize(bridgeSettings)); otherBench.ProjectContext = "different bench";
Assert(!Json.Serialize(testSkills.Snapshot(otherBench)).Contains("Fixture-only special case"), "Learned lessons are isolated from other bench configurations");
var learnedItem = testSkills.ReadLessons()[0]; testSkills.UpdateLesson(learnedItem.Id, "edited condition", "edited guidance");
Assert(testSkills.ReadLessons()[0].Guidance == "edited guidance" && testSkills.ReadLessons()[0].RunId == bridgeRun.Id, "Editing a lesson preserves its source provenance");
testSkills.UpdateLesson(learnedItem.Id, "", "", true);
Assert(testSkills.ReadLessons().Count == 0 && new TestSkills(store.Root).ReadManual().Contains("fixture-specific"), "Lesson deletion persists while manual supplements survive reload");
Rejected(() => new AppSettings { Bus = new() { Enabled = true, Protocol = "CAN FD", Adapter = "SLCAN" } }.Validate(), "SLCAN cannot silently accept CAN FD configuration");
Rejected(() => new AppSettings { SerialPorts = [new() { Name = "a", Enabled = true }, new() { Name = "b", Enabled = true }] }.Validate(), "Duplicate physical COM bindings are rejected before connection");
Rejected(() => new AppSettings { Power = new() { Enabled = true, Transport = "visa" } }.Validate(), "VISA power requires an actual resource binding");
var modernSettings = new AppSettings { Bus = new() { Enabled = true, Adapter = "Legacy", Protocol = "LIN", DeviceName = "fixture-channel", LinMaster = true, DbcPath = "fixture.ldf" }, Power = new() { Enabled = true, Transport = "visa", Resource = "USB::fixture::INSTR", MeasurementResource = "USB::meter::INSTR" } };
modernSettings.Validate(); var reloaded = Json.Read<AppSettings>(Json.Serialize(modernSettings));
Assert(reloaded.Bus.LinMaster && reloaded.Bus.Protocol == "LIN" && reloaded.Power.MeasurementResource == "USB::meter::INSTR", "Legacy bus and separate VISA measurement bindings survive configuration reload");
await using (var host = new LegacyBusClient(Path.Combine(root, "host-contract"), _ => { }))
{
    Assert((await host.CallAsync("ping", null, CancellationToken.None)).GetString() == "AIclient.DeviceHost/net48/x86", "The actual isolated net48 x86 host responds over the JSON protocol");
    var rejected = false; try { await host.CallAsync("send", new { id = 1, data = "00" }, CancellationToken.None); } catch (IOException) { rejected = true; }
    Assert(rejected, "The compatibility host rejects sending before a device is connected");
}

var late = new LateBridge();
var stub = new StubDevices(store);
using var stop = new CancellationTokenSource();
var pending = new TestRunner(store, late, stub, _ => { }, _ => { }).RunAsync("cancel", settings, stop.Token);
await late.Entered.Task;
stop.Cancel(); late.Release.SetResult();
var cancelled = await pending;
Assert(cancelled.Outcome == Outcome.Cancelled && stub.Executed == 0 && stub.Disposed && File.Exists(Path.Combine(cancelled.Directory, "report.html")), "Late AI response cannot trigger action after cancellation; report retained");

var pauseBridge = new PauseBridge(); var pauseDevices = new StubDevices(store);
var pauseRunner = new TestRunner(store, pauseBridge, pauseDevices, _ => { }, _ => { });
var pauseRunTask = pauseRunner.RunAsync("pause", settings, CancellationToken.None);
await pauseBridge.Entered.Task;
pauseRunner.TogglePause(); pauseRunner.TogglePause(); pauseBridge.Release.SetResult();
var pauseRun = await pauseRunTask;
Assert(pauseRun.Outcome == Outcome.Passed && pauseDevices.Executed == 0 && pauseBridge.Calls == 2, "Resume reobserves and discards pre-pause action suggestion");

var failed = await new TestRunner(store, new UngroundedBridge(), new StubDevices(store), _ => { }, _ => { }).RunAsync("ungrounded", settings, CancellationToken.None);
Assert(failed.Outcome == Outcome.Error && failed.Steps[0].Outcome == Outcome.Error, "Invalid AI verdict becomes execution error, never pass");
var interrupted = store.CreateRun("interrupted", settings); store.RecoverInterrupted();
Assert(Json.Read<RunRecord>(File.ReadAllText(Path.Combine(interrupted.Directory, "run.json"))).Outcome == Outcome.Error && File.Exists(Path.Combine(interrupted.Directory, "report.html")), "Interrupted run recovered as error with report");

var timeoutOutput = Path.Combine(root, "timeout-stdout.txt"); var didTimeout = false;
try { await ProcessRunner.RunAsync("powershell.exe", ["-NoProfile", "-NonInteractive", "-Command", "[Console]::WriteLine('before-wait'); Start-Sleep -Seconds 30"], 2, CancellationToken.None, stdoutPath: timeoutOutput); } catch (TimeoutException) { didTimeout = true; }
Assert(didTimeout && File.ReadAllText(timeoutOutput).Contains("before-wait"), "Timed-out child process terminated and partial output preserved");

Storage.WriteJson(Path.Combine(root, "checks.json"), new { success = true, checks = checks.Count, names = checks, demoReport = Path.Combine(demo.Directory, "report.html"), bridgeReport = Path.Combine(bridgeRun.Directory, "report.html") });
Console.WriteLine($"ALL {checks.Count} CHECKS PASSED. Artifacts: {root}");

sealed class StubDevices(Storage storage) : IRunDevices
{
    public int Executed;
    public bool Disposed;
    public Task StartAsync(RunRecord run, CancellationToken token) => Task.CompletedTask;
    public Task<IReadOnlyList<Evidence>> ObserveAsync(RunRecord run, StepRecord step, CancellationToken token)
    {
        var path = Path.Combine(run.Directory, "logs", Guid.NewGuid().ToString("N") + ".txt"); File.WriteAllText(path, "test observation");
        return Task.FromResult<IReadOnlyList<Evidence>>([storage.AddEvidence(run, path, "monitor", step.Id, summary: "test only")]);
    }
    public Task<string> ExecuteAsync(RunRecord run, StepRecord step, Decision decision, CancellationToken token) { Executed++; return Task.FromResult("executed"); }
    public ValueTask DisposeAsync() { Disposed = true; return ValueTask.CompletedTask; }
}
class LateBridge : IAiBridge
{
    public TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Task<TestPlan> PlanAsync(RunRecord run, CancellationToken token) => Task.FromResult(new TestPlan { Title = "test", Scope = "test", Steps = [new() { Id = "S01", Title = "test", Expected = "test" }] });
    public virtual async Task<Decision> DecideAsync(RunRecord run, StepRecord step, IReadOnlyList<Evidence> observations, CancellationToken token)
    {
        Entered.SetResult(); await Release.Task;
        return new Decision { Kind = "action", Observation = "late", Tool = "wait", Arguments = "{\"durationMs\":100}" };
    }
}
sealed class UngroundedBridge : LateBridge
{
    public override Task<Decision> DecideAsync(RunRecord run, StepRecord step, IReadOnlyList<Evidence> observations, CancellationToken token) => Task.FromResult(new Decision { Kind = "pass", Observation = "no evidence" });
}
sealed class PauseBridge : LateBridge
{
    public int Calls;
    public override async Task<Decision> DecideAsync(RunRecord run, StepRecord step, IReadOnlyList<Evidence> observations, CancellationToken token)
    {
        if (++Calls == 1) return await base.DecideAsync(run, step, observations, token);
        return new Decision { Kind = "pass", Observation = "fresh observation", EvidenceIds = observations.Select(e => e.Id).ToList() };
    }
}
sealed class TwoPhonesBridge : IAiBridge
{
    private int calls;
    public Task<TestPlan> PlanAsync(RunRecord run, CancellationToken token) => Task.FromResult(new TestPlan { Title = "双手机路由检查", Scope = "受控 ADB 替身，不是实机蓝牙测试", Steps = [new() { Id = "S01", Title = "分别操作 ADB1 和 ADB2", Expected = "两台模拟手机分别接收操作并生成新截图" }] });
    public Task<Decision> DecideAsync(RunRecord run, StepRecord step, IReadOnlyList<Evidence> observations, CancellationToken token)
    {
        var call = ++calls;
        return Task.FromResult(call <= 2
            ? new Decision { Kind = "action", Observation = "模拟手机画面已取得，按中性编号操作。", Tool = "adb.tap", Role = call == 1 ? "adb1" : "ADB2", Arguments = "{\"x\":100,\"y\":200}", EvidenceIds = observations.Select(e => e.Id).ToList() }
            : new Decision { Kind = "pass", Observation = "双手机软件路由检查完成，仅为受控设备替身。", EvidenceIds = observations.Select(e => e.Id).ToList() });
    }
}

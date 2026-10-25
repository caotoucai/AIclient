using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIclient.Models;

public static class Json
{
    public static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);
    public static T Read<T>(string text) => JsonSerializer.Deserialize<T>(text, Options) ?? throw new InvalidDataException("JSON 内容为空。");
}

public sealed class AppSettings
{
    public string PowerShellPath { get; set; } = "powershell.exe";
    public string CodexPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm", "codex.ps1");
    public string Model { get; set; } = "";
    public string AdbPath { get; set; } = "adb.exe";
    private string adb1Serial = "";
    private string adb2Serial = "";
    private bool adb1Specified;
    private bool adb2Specified;
    public string Adb1Serial { get => adb1Serial; set { adb1Serial = value ?? ""; adb1Specified = true; } }
    public string Adb2Serial { get => adb2Serial; set { adb2Serial = value ?? ""; adb2Specified = true; } }

    // 旧设置和历史记录可继续读取；保存时只输出中性的 adb1Serial / adb2Serial。
    [JsonPropertyName("headunitSerial"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LegacyHeadunitSerial { get => null; set { if (!adb1Specified) adb1Serial = value ?? ""; } }
    [JsonPropertyName("phoneSerial"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LegacyPhoneSerial { get => null; set { if (!adb2Specified) adb2Serial = value ?? ""; } }
    public string ProjectContext { get; set; } = "请仅测试实际可观察的功能。蓝牙名称、串口关键字和总线信号以台架配置为准。未配置音频采集时，不能证明实际媒体声音或通话音频正常。";
    public int AiTimeoutSeconds { get; set; } = 180;
    public int TaskTimeoutMinutes { get; set; } = 30;
    public int MaxActionsPerStep { get; set; } = 16;
    public int MaxAiCalls { get; set; } = 80;
    public bool DemoMode { get; set; } = true;
    public bool AutoLearnSkills { get; set; } = true;
    public List<SerialConfig> SerialPorts { get; set; } = [];
    public BusConfig Bus { get; set; } = new();
    public PowerConfig Power { get; set; } = new();
    public List<DeviceCommand> Commands { get; set; } = [];

    public void Validate()
    {
        if (AiTimeoutSeconds is < 10 or > 1800 || TaskTimeoutMinutes is < 1 or > 240 || MaxActionsPerStep is < 1 or > 100 || MaxAiCalls is < 1 or > 1000)
            throw new InvalidDataException("调用超时应为 10–1800 秒，任务时长 1–240 分钟，动作数 1–100，AI 调用数 1–1000。");
        if (!string.IsNullOrEmpty(Adb1Serial) && Adb1Serial == Adb2Serial) throw new InvalidDataException("ADB1 和 ADB2 不能绑定同一 serial；编号不限定设备类型。");
        if (SerialPorts.Select(p => p.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != SerialPorts.Count) throw new InvalidDataException("串口逻辑名称不能重复。");
        foreach (var port in SerialPorts) port.Validate();
        if (Commands.Select(c => c.Alias).Distinct(StringComparer.OrdinalIgnoreCase).Count() != Commands.Count || Commands.Any(c => string.IsNullOrWhiteSpace(c.Alias))) throw new InvalidDataException("命令别名必须非空且唯一。");
        if (Power.MinVoltage < 0 || Power.MaxVoltage < Power.MinVoltage || Power.MaxVoltage > 1000) throw new InvalidDataException("电源电压范围无效。");
        if (!double.IsFinite(Power.MinVoltage) || !double.IsFinite(Power.MaxVoltage)) throw new InvalidDataException("电源电压必须为有限值。");
        if (Power.Transport is not ("serial" or "visa")) throw new InvalidDataException("电源传输需为 serial 或 visa。");
        if (Power.Enabled && Power.Transport == "serial" && !SerialPorts.Any(p => p.Enabled && p.Name == Power.Channel)) throw new InvalidDataException("请启用电源对应的串口通道。");
        if (Power.Enabled && Power.Transport == "visa" && string.IsNullOrWhiteSpace(Power.Resource)) throw new InvalidDataException("请填写电源 VISA 资源。");
        if (Bus.Adapter is not ("SLCAN" or "Legacy")) throw new InvalidDataException("总线适配器需为 SLCAN 或 Legacy。");
        if (Bus.Protocol is not ("CAN" or "CAN FD" or "LIN" or "FlexRay")) throw new InvalidDataException("未知总线协议。");
        if (Bus.Enabled && Bus.Adapter == "SLCAN" && Bus.Protocol != "CAN") throw new InvalidDataException("SLCAN 只支持 classic CAN；其他协议请选择 Legacy 兼容驱动。");
        if (Bus.Enabled && Bus.Adapter == "SLCAN") { Bus.Serial.Validate(); _ = Services.Slcan.SpeedCommand(Bus.BitRate); }
        if (Bus.Enabled && Bus.Adapter == "Legacy" && string.IsNullOrWhiteSpace(Bus.DeviceName)) throw new InvalidDataException("请选择旧总线库扫描出的设备完整名称。");
        if (Bus.Enabled && Bus.Adapter == "Legacy" && Bus.Protocol == "FlexRay" && (string.IsNullOrWhiteSpace(Bus.DbcPath) || string.IsNullOrWhiteSpace(Bus.ErayFrame) || string.IsNullOrWhiteSpace(Bus.ColdFrame))) throw new InvalidDataException("FlexRay 需要 FIBEX 路径及本项目 E-Ray / Cold 启动帧名。");
        if (Power.TimeoutMs is < 100 or > 30000) throw new InvalidDataException("VISA 超时需为 100–30000ms。");
        var enabledPorts = SerialPorts.Where(p => p.Enabled).Select(p => p.Port).Concat(Bus.Enabled && Bus.Adapter == "SLCAN" ? [Bus.Serial.Port] : []);
        if (enabledPorts.Distinct(StringComparer.OrdinalIgnoreCase).Count() != enabledPorts.Count()) throw new InvalidDataException("同一个物理 COM 口不能用于多个通道。");
    }

    public IEnumerable<AdbBinding> BoundAdbDevices()
    {
        if (!string.IsNullOrWhiteSpace(Adb1Serial)) yield return new(AdbSlots.First, Adb1Serial);
        if (!string.IsNullOrWhiteSpace(Adb2Serial)) yield return new(AdbSlots.Second, Adb2Serial);
    }
}

public sealed record AdbBinding(string Slot, string Serial);

public static class AdbSlots
{
    public const string First = "adb1";
    public const string Second = "adb2";
    public const string UsageGuidance = "ADB1、ADB2 是中性设备编号，不限定设备类型。一般可将 ADB1 理解为车机、ADB2 理解为手机，但这只是弱默认约定；用户需求、项目背景和本轮实际画面优先。两台均为手机、两台均为车机或其他 Android 组合均有效，不得仅因编号或缺少车机标签拒绝可执行的双设备测试。计划与动作始终使用 adb1、adb2 指定设备；真正依赖特定硬件且缺失的能力单列为未覆盖，不能虚构通过。";

    public static string Normalize(string slot) => slot?.ToLowerInvariant() switch
    {
        First => First,
        Second => Second,
        _ => throw new InvalidDataException("ADB 工具的 role 只能使用 adb1 或 adb2，不使用车机/手机类型作为路由。")
    };

    // 旧 evidence 的 role 保持原样，仅在查看历史时转换显示名称。
    public static string Label(string slot) => slot?.ToLowerInvariant() switch
    {
        First or "headunit" => "ADB1",
        Second or "phone" => "ADB2",
        _ => slot ?? ""
    };
}

public sealed class SerialConfig
{
    public string Name { get; set; } = "serial1";
    public string Port { get; set; } = "COM3";
    public int BaudRate { get; set; } = 115200;
    public int DataBits { get; set; } = 8;
    public string Parity { get; set; } = "None";
    public int StopBits { get; set; } = 1;
    public string Encoding { get; set; } = "utf-8";
    public string NewLine { get; set; } = "\r\n";
    public bool Enabled { get; set; }
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) || !System.Text.RegularExpressions.Regex.IsMatch(Port, "^COM[1-9][0-9]*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase) || BaudRate < 300 || DataBits is < 5 or > 8 || StopBits is < 1 or > 2 || !new[] { "None", "Odd", "Even" }.Contains(Parity)) throw new InvalidDataException($"串口 {Name} 的参数无效。");
        _ = System.Text.Encoding.GetEncoding(Encoding);
    }
}

public sealed class BusConfig
{
    public string NodeName { get; set; } = "bench";
    public CanTiming Timing { get; set; } = new();
    public string Adapter { get; set; } = "SLCAN";
    public string Protocol { get; set; } = "CAN";
    public string DeviceName { get; set; } = "";
    public int DataRate { get; set; } = 2000000;
    public bool LinMaster { get; set; }
    public string LinBaud { get; set; } = "Baud_19200";
    public string LinVersion { get; set; } = "LIN_2_1";
    public string ErayFrame { get; set; } = "";
    public string ColdFrame { get; set; } = "";
    public bool Enabled { get; set; }
    public SerialConfig Serial { get; set; } = new() { Name = "can", BaudRate = 115200 };
    public int BitRate { get; set; } = 500000;
    public string DbcPath { get; set; } = "";
}
public sealed class CanTiming
{
    public bool Enabled { get; set; }
    public int ClockMHz { get; set; } = 80;
    public byte ArbitrationBrp { get; set; } = 1;
    public byte ArbitrationTseg1 { get; set; } = 63;
    public byte ArbitrationTseg2 { get; set; } = 16;
    public byte ArbitrationSjw { get; set; } = 2;
    public byte DataBrp { get; set; } = 1;
    public byte DataTseg1 { get; set; } = 14;
    public byte DataTseg2 { get; set; } = 5;
    public byte DataSjw { get; set; } = 2;
}
public sealed class PowerConfig
{
    public string Transport { get; set; } = "serial";
    public string Resource { get; set; } = "";
    public string MeasurementResource { get; set; } = "";
    public string VoltageQuery { get; set; } = "MEAS:VOLT?";
    public string CurrentQuery { get; set; } = "MEAS:CURR?";
    public int TimeoutMs { get; set; } = 5000;
    public bool Enabled { get; set; }
    public string Channel { get; set; } = "power";
    public string OnCommand { get; set; } = "OUTP ON";
    public string OffCommand { get; set; } = "OUTP OFF";
    public string VoltageCommand { get; set; } = "VOLT {value}";
    public double MinVoltage { get; set; } = 0;
    public double MaxVoltage { get; set; } = 16;
}
public sealed class DeviceCommand
{
    public string Alias { get; set; } = "";
    public string Description { get; set; } = "";
    public string Transport { get; set; } = "serial";
    public string Channel { get; set; } = "serial1";
    public string Payload { get; set; } = "";
    public bool Hex { get; set; }
    public uint FrameId { get; set; }
    public string FrameName { get; set; } = "";
}

public sealed class TestPlan
{
    public string Title { get; set; } = "";
    public string Scope { get; set; } = "";
    public List<string> Uncovered { get; set; } = [];
    public List<PlanStep> Steps { get; set; } = [];
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title) || Steps.Count is < 1 or > 30) throw new InvalidDataException("AI 计划须包含标题和 1–30 个步骤。");
        if (Steps.Any(s => string.IsNullOrWhiteSpace(s.Id) || string.IsNullOrWhiteSpace(s.Title) || string.IsNullOrWhiteSpace(s.Expected)) || Steps.Select(s => s.Id).Distinct().Count() != Steps.Count) throw new InvalidDataException("AI 计划步骤 ID 必须唯一，且有标题和预期。");
    }
}
public sealed class PlanStep
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Expected { get; set; } = "";
}
public enum Outcome { Pending, Running, Passed, Failed, Inconclusive, Error, Skipped, Cancelled }
public sealed class RunRecord
{
    public string Id { get; set; } = "";
    public string Request { get; set; } = "";
    public bool Demo { get; set; }
    public DateTimeOffset Started { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Finished { get; set; }
    public Outcome Outcome { get; set; }
    public string Message { get; set; } = "";
    public TestPlan? Plan { get; set; }
    public List<StepRecord> Steps { get; set; } = [];
    public List<Evidence> Evidence { get; set; } = [];
    public List<ActionRecord> Actions { get; set; } = [];
    public AppSettings SettingsSnapshot { get; set; } = new();
    [JsonIgnore] public string Directory { get; set; } = "";
}
public sealed class StepRecord
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Expected { get; set; } = "";
    public Outcome Outcome { get; set; }
    public string Actual { get; set; } = "";
    public DateTimeOffset? Started { get; set; }
    public DateTimeOffset? Finished { get; set; }
    public List<string> EvidenceIds { get; set; } = [];
}
public sealed class Evidence
{
    public string Id { get; set; } = "";
    public string StepId { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Role { get; set; } = "";
    public string Device { get; set; } = "";
    public string Path { get; set; } = "";
    public string Sha256 { get; set; } = "";
    public DateTimeOffset CapturedAt { get; set; } = DateTimeOffset.UtcNow;
    public int Width { get; set; }
    public int Height { get; set; }
    public string Summary { get; set; } = "";
}
public sealed class ActionRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string StepId { get; set; } = "";
    public DateTimeOffset Started { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Finished { get; set; }
    public string Tool { get; set; } = "";
    public string Role { get; set; } = "";
    public string Arguments { get; set; } = "";
    public string Observation { get; set; } = "";
    public string Result { get; set; } = "";
    public bool Success { get; set; }
}
public sealed class Decision
{
    public string Kind { get; set; } = "";
    public string Observation { get; set; } = "";
    public string Tool { get; set; } = "";
    public string Role { get; set; } = "";
    public string Arguments { get; set; } = "{}";
    public List<string> EvidenceIds { get; set; } = [];
    public List<LessonSuggestion> Lessons { get; set; } = [];
}
public class LessonSuggestion
{
    public string Condition { get; set; } = "";
    public string Guidance { get; set; } = "";
    public List<string> EvidenceIds { get; set; } = [];
}
public sealed class TestLesson : LessonSuggestion
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Scope { get; set; } = "";
    public string RunId { get; set; } = "";
    public string StepId { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EditedAt { get; set; }
    [JsonIgnore] public string Display => $"{CreatedAt.ToLocalTime():MM-dd HH:mm} · {Condition}";
}
public sealed record ProcessResult(int ExitCode, string Output, string Error);

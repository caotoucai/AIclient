using System.Net;
using System.Security.Cryptography;
using System.Text;
using AIclient.Models;

namespace AIclient.Services;

public sealed class Storage
{
    public string Root { get; }
    public Storage(string root) { Root = Path.GetFullPath(root); Directory.CreateDirectory(Root); Directory.CreateDirectory(Path.Combine(Root, "runs")); }
    public AppSettings LoadSettings() => File.Exists(Path.Combine(Root, "settings.json")) ? Json.Read<AppSettings>(File.ReadAllText(Path.Combine(Root, "settings.json"))) : new();
    public void SaveSettings(AppSettings settings) { settings.Validate(); WriteJson(Path.Combine(Root, "settings.json"), settings); }
    public static void WriteJson<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + ".tmp";
        File.WriteAllText(temp, Json.Serialize(value), new UTF8Encoding(false));
        File.Move(temp, path, true);
    }
    public RunRecord CreateRun(string request, AppSettings settings)
    {
        var id = DateTimeOffset.Now.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N")[..6];
        var run = new RunRecord { Id = id, Request = request, Demo = settings.DemoMode, SettingsSnapshot = Json.Read<AppSettings>(Json.Serialize(settings)), Directory = Path.Combine(Root, "runs", id), Outcome = Outcome.Running };
        Directory.CreateDirectory(run.Directory);
        Directory.CreateDirectory(Path.Combine(run.Directory, "screenshots"));
        Directory.CreateDirectory(Path.Combine(run.Directory, "logs"));
        Save(run);
        return run;
    }
    public void Save(RunRecord run) { WriteJson(Path.Combine(run.Directory, "run.json"), run); WriteJson(Path.Combine(run.Directory, "evidence.json"), run.Evidence); }
    public static void Event(RunRecord run, object entry) => File.AppendAllText(Path.Combine(run.Directory, "events.jsonl"), System.Text.Json.JsonSerializer.Serialize(new { at = DateTimeOffset.UtcNow, entry }, new System.Text.Json.JsonSerializerOptions(Json.Options) { WriteIndented = false }) + "\n", Encoding.UTF8);
    public Evidence AddEvidence(RunRecord run, string path, string kind, string step, string role = "", string device = "", string summary = "", int width = 0, int height = 0)
    {
        var full = Path.GetFullPath(path);
        if (!full.StartsWith(run.Directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("证据必须在当前任务目录内。");
        using var stream = File.OpenRead(full);
        var item = new Evidence { Id = "E" + (run.Evidence.Count + 1).ToString("D5"), Path = Path.GetRelativePath(run.Directory, full).Replace('\\', '/'), Kind = kind, StepId = step, Role = role, Device = device, Summary = summary, Width = width, Height = height, Sha256 = Convert.ToHexString(SHA256.HashData(stream)) };
        run.Evidence.Add(item);
        Save(run);
        return item;
    }
    public IEnumerable<RunRecord> History()
    {
        foreach (var path in Directory.EnumerateFiles(Path.Combine(Root, "runs"), "run.json", SearchOption.AllDirectories).OrderDescending())
        {
            RunRecord? run = null;
            try { run = Json.Read<RunRecord>(File.ReadAllText(path)); run.Directory = Path.GetDirectoryName(path)!; } catch { }
            if (run != null) yield return run;
        }
    }
    public void RecoverInterrupted()
    {
        foreach (var run in History().Where(r => r.Outcome is Outcome.Running or Outcome.Pending))
        {
            run.Outcome = Outcome.Error; run.Finished = DateTimeOffset.UtcNow; run.Message = "上次进程中断，未自动恢复设备动作。";
            foreach (var step in run.Steps.Where(s => s.Outcome is Outcome.Running or Outcome.Pending)) { step.Outcome = Outcome.Skipped; step.Actual = "进程中断，未完成验证。"; }
            Save(run); Report.Generate(run);
        }
    }
}

public static class Report
{
    public static string Label(Outcome o) => o switch { Outcome.Passed => "通过", Outcome.Failed => "失败", Outcome.Error => "执行异常", Outcome.Inconclusive => "无法判定", Outcome.Cancelled => "已停止", Outcome.Skipped => "未执行", Outcome.Running => "执行中", _ => "等待执行" };
    public static string Generate(RunRecord run)
    {
        static string H(string? s) => WebUtility.HtmlEncode(s ?? "");
        static string Link(string s) => H(string.Join('/', s.Replace('\\', '/').Split('/').Select(Uri.EscapeDataString)));
        var b = new StringBuilder("<!doctype html><html lang='zh-CN'><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'><title>AIclient 测试报告</title><style>body{background:#0d1422;color:#e8eef8;font:15px/1.7 system-ui,Microsoft YaHei;margin:0}main{max-width:1180px;margin:auto;padding:42px 24px}h1{font-size:32px}h2{font-size:22px}small,.muted{color:#9eafc7}.card{background:#162235;border:1px solid #2a3b54;border-radius:12px;padding:22px;margin:18px 0}.badge{color:#6ce0c6}pre{white-space:pre-wrap;overflow-wrap:anywhere;background:#0c1524;padding:14px;border-radius:6px}a{color:#71c6ff}img{max-width:100%;max-height:480px;border-radius:8px}.shots{display:flex;gap:18px;flex-wrap:wrap}.shot{max-width:48%}table{width:100%;border-collapse:collapse}td,th{text-align:left;padding:10px;border-bottom:1px solid #304158;vertical-align:top}summary{cursor:pointer}code{overflow-wrap:anywhere}@media(max-width:700px){.shot{max-width:100%}}</style><main>");
        b.Append($"<small>AIclient · 本地测试报告 · {H(run.Id)}</small><h1>{H(run.Plan?.Title ?? "测试任务")}</h1><div class='badge'>{H(Label(run.Outcome))}{(run.Demo ? " · 演示模拟数据，不代表真实硬件测试" : " · 真实设备模式")}</div><div class='card'><b>客户需求</b><p>{H(run.Request)}</p><b>本次覆盖范围</b><p>{H(run.Plan?.Scope)}</p><p>{H(run.Message)}</p><small>开始：{H(run.Started.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz"))}<br>结束：{H(run.Finished?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz"))}</small></div>");
        b.Append($"<div class='card'><b>设备绑定</b><p>ADB1：{H(run.SettingsSnapshot.Adb1Serial)}　ADB2：{H(run.SettingsSnapshot.Adb2Serial)}</p><p>ADB 编号不限定设备类型，支持两台手机等 Android 设备组合。</p><p>步骤：{run.Steps.Count}　通过：{run.Steps.Count(s => s.Outcome == Outcome.Passed)}　失败：{run.Steps.Count(s => s.Outcome == Outcome.Failed)}　证据：{run.Evidence.Count}</p></div>");
        if (run.Plan?.Uncovered.Count > 0) b.Append("<div class='card'><b>未覆盖 / 限制</b><ul>" + string.Concat(run.Plan.Uncovered.Select(s => $"<li>{H(s)}</li>")) + "</ul></div>");
        foreach (var step in run.Steps)
        {
            b.Append($"<section class='card'><h2>{H(step.Id)} · {H(step.Title)} <small>{H(Label(step.Outcome))}</small></h2><b>预期</b><p>{H(step.Expected)}</p><b>实际观察</b><p>{H(step.Actual)}</p><small>引用证据：{H(string.Join(", ", step.EvidenceIds))}</small><div class='shots'>");
            foreach (var ev in run.Evidence.Where(e => e.StepId == step.Id && e.Kind == "screenshot")) b.Append($"<div class='shot'><p>{H(ev.Id)} · {H(AdbSlots.Label(ev.Role))} · {H(ev.CapturedAt.ToLocalTime().ToString("HH:mm:ss.fff"))}</p><a href='{Link(ev.Path)}'><img loading='lazy' src='{Link(ev.Path)}' alt='{H(ev.Summary)}'></a></div>");
            b.Append("</div><details><summary>实际操作时间线</summary>");
            foreach (var a in run.Actions.Where(a => a.StepId == step.Id)) b.Append($"<p><b>{H(a.Started.ToLocalTime().ToString("HH:mm:ss.fff"))} · {H(a.Tool)} {H(AdbSlots.Label(a.Role))} · {(a.Success ? "操作成功" : "操作未完成")}</b></p><pre>{H(a.Arguments)}\n观察：{H(a.Observation)}\n结果：{H(a.Result)}</pre>");
            b.Append("</details></section>");
        }
        b.Append("<div class='card'><h2>证据清单</h2><p>原始文件使用相对路径；请保留整个任务文件夹。</p><table><tr><th>ID / 类型</th><th>文件 / 说明</th><th>SHA-256</th></tr>");
        foreach (var e in run.Evidence) b.Append($"<tr><td>{H(e.Id)}<br>{H(e.Kind)}</td><td><a href='{Link(e.Path)}'>{H(e.Path)}</a><br>{H(e.Summary)}</td><td><code>{H(e.Sha256)}</code></td></tr>");
        b.Append("</table><p><a href='run.json'>完整运行数据</a> · <a href='events.jsonl'>事件日志</a> · <a href='evidence.json'>证据索引</a></p></div><small>AI 判定应结合原始截图与设备日志复核；模拟数据始终以演示标识呈现。</small></main></html>");
        var path = Path.Combine(run.Directory, "report.html");
        File.WriteAllText(path, b.ToString(), new UTF8Encoding(false));
        return path;
    }
}

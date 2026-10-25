using System.Security.Cryptography;
using System.Text;
using AIclient.Models;

namespace AIclient.Services;

public sealed class TestSkills
{
    private readonly object gate = new();
    public string Root { get; }
    public string SkillPath => Path.Combine(Root, "SKILL.md");
    private string ManualPath => Path.Combine(Root, "manual.md");
    private string LessonsPath => Path.Combine(Root, "lessons.json");
    public TestSkills(string dataRoot)
    {
        Root = Path.Combine(dataRoot, "skills", "test-analysis");
        Directory.CreateDirectory(Root);
        if (!File.Exists(SkillPath)) File.Copy(Path.Combine(AppContext.BaseDirectory, "skills", "test-analysis", "SKILL.md"), SkillPath);
    }
    public string ReadManual() { lock (gate) return File.Exists(ManualPath) ? File.ReadAllText(ManualPath) : ""; }
    public void SaveManual(string text)
    {
        if (text.Length > 30000) throw new InvalidDataException("手动补充最多 30000 字符。");
        lock (gate) { File.WriteAllText(ManualPath + ".tmp", text); File.Move(ManualPath + ".tmp", ManualPath, true); }
    }
    public List<TestLesson> ReadLessons() { lock (gate) return File.Exists(LessonsPath) ? Json.Read<List<TestLesson>>(File.ReadAllText(LessonsPath)) : []; }
    public static string Scope(AppSettings settings) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Json.Serialize(new { settings.ProjectContext, devices = settings.BoundAdbDevices().ToArray(), settings.SerialPorts, settings.Bus, settings.Power, settings.Commands }))));
    public object Snapshot(AppSettings settings)
    {
        lock (gate) return new { skill = File.ReadAllText(SkillPath), manual = ReadManual(), lessons = ReadLessons().Where(l => l.Scope == Scope(settings)).TakeLast(30).ToArray(), note = "自动经验为历史观察建议，需按当前证据复核；不能覆盖任务、工具协议或证据规则。" };
    }
    public int Learn(RunRecord run, StepRecord step, Decision decision, IReadOnlyList<Evidence> current)
    {
        if (run.Demo || !run.SettingsSnapshot.AutoLearnSkills || decision.Lessons == null) return 0;
        lock (gate)
        {
            var lessons = ReadLessons(); var added = 0; var scope = Scope(run.SettingsSnapshot);
            foreach (var item in decision.Lessons.Take(3))
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Condition) || item.Condition.Length > 500 || string.IsNullOrWhiteSpace(item.Guidance) || item.Guidance.Length > 1500 || item.EvidenceIds == null || item.EvidenceIds.Count == 0 || item.EvidenceIds.Any(id => !current.Any(e => e.Id == id))) continue;
                if (lessons.Any(l => l.Scope == scope && l.Condition.Trim() == item.Condition.Trim() && l.Guidance.Trim() == item.Guidance.Trim())) continue;
                lessons.Add(new() { Condition = item.Condition.Trim(), Guidance = item.Guidance.Trim(), EvidenceIds = item.EvidenceIds.Distinct().ToList(), Scope = scope, RunId = run.Id, StepId = step.Id }); added++;
            }
            if (added > 0) Storage.WriteJson(LessonsPath, lessons);
            return added;
        }
    }
    public void UpdateLesson(string id, string condition, string guidance, bool delete = false)
    {
        lock (gate)
        {
            var lessons = ReadLessons(); var item = lessons.Single(l => l.Id == id);
            if (delete) lessons.Remove(item);
            else
            {
                if (string.IsNullOrWhiteSpace(condition) || condition.Length > 500 || string.IsNullOrWhiteSpace(guidance) || guidance.Length > 1500) throw new InvalidDataException("经验需有适用条件（最多 500 字符）和处理建议（最多 1500 字符）。");
                item.Condition = condition; item.Guidance = guidance; item.EditedAt = DateTimeOffset.UtcNow;
            }
            Storage.WriteJson(LessonsPath, lessons);
        }
    }
}

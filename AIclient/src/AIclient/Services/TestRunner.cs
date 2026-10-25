using AIclient.Models;

namespace AIclient.Services;

public sealed class TestRunner(Storage storage, IAiBridge ai, IRunDevices devices, Action<string> log, Action<RunRecord> changed, TestSkills? skills = null)
{
    private volatile bool paused;
    private int pauseVersion;
    public bool Paused => paused;
    public void TogglePause() { paused = !paused; Interlocked.Increment(ref pauseVersion); }
    private async Task WaitForResume(CancellationToken token) { while (paused) await Task.Delay(100, token); token.ThrowIfCancellationRequested(); }

    public async Task<RunRecord> RunAsync(string request, AppSettings settings, CancellationToken cancellationToken)
    {
        var run = storage.CreateRun(request, settings);
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(TimeSpan.FromMinutes(settings.TaskTimeoutMinutes));
        var token = budget.Token;
        var calls = 0;
        StepRecord? active = null;
        void Update() { storage.Save(run); changed(run); }
        void CallBudget() { if (++calls > settings.MaxAiCalls) throw new InvalidOperationException("已达到本任务的 AI 调用预算，停止执行。 "); }
        changed(run);
        try
        {
            log(settings.DemoMode ? "演示模式：使用模拟画面和判定，不连接设备、不消耗 Codex 额度。" : "开始检查真实设备与台架配置…");
            await devices.StartAsync(run, token);
            await WaitForResume(token);
            CallBudget();
            run.Plan = await ai.PlanAsync(run, token);
            token.ThrowIfCancellationRequested(); run.Plan.Validate();
            Storage.WriteJson(Path.Combine(run.Directory, "plan.json"), run.Plan);
            run.Steps = run.Plan.Steps.Select(s => new StepRecord { Id = s.Id, Title = s.Title, Expected = s.Expected }).ToList();
            Update(); log($"计划已生成：{run.Plan.Title}，共 {run.Steps.Count} 步，开始执行。");
            foreach (var step in run.Steps)
            {
                active = step;
                await WaitForResume(token);
                step.Outcome = Outcome.Running; step.Started = DateTimeOffset.UtcNow; Update();
                log($"{step.Id} · {step.Title}");
                var actionCount = 0;
                while (true)
                {
                    await WaitForResume(token);
                    var observedVersion = Volatile.Read(ref pauseVersion);
                    var evidence = await devices.ObserveAsync(run, step, token);
                    token.ThrowIfCancellationRequested();
                    step.EvidenceIds.AddRange(evidence.Select(e => e.Id));
                    CallBudget();
                    var decision = await ai.DecideAsync(run, step, evidence, token);
                    await WaitForResume(token);
                    if (observedVersion != Volatile.Read(ref pauseVersion)) { log("暂停后重新观察设备，丢弃暂停前的动作建议。"); continue; }
                    ValidateDecision(decision, evidence);
                    try
                    {
                        var learned = skills?.Learn(run, step, decision, evidence) ?? 0;
                        if (learned > 0) log($"已积累 {learned} 条有证据来源的测试经验，下次分析可用。");
                    }
                    catch (Exception ex) { log("测试经验未保存：" + ex.Message); }
                    step.Actual = decision.Observation; Update(); log(decision.Observation);
                    if (decision.Kind != "action")
                    {
                        step.Outcome = decision.Kind switch { "pass" => Outcome.Passed, "fail" => Outcome.Failed, _ => Outcome.Inconclusive };
                        break;
                    }
                    if (actionCount++ >= settings.MaxActionsPerStep) { step.Outcome = Outcome.Inconclusive; step.Actual += "\n达到本步骤动作上限，未继续操作。"; break; }
                    var action = new ActionRecord { StepId = step.Id, Tool = decision.Tool, Role = decision.Role, Arguments = decision.Arguments, Observation = decision.Observation };
                    run.Actions.Add(action); Storage.Event(run, new { kind = "action-start", action }); Update();
                    try
                    {
                        // 记录动作后、实际调用前再次检查停止状态，迟到的 AI 结果不会触发设备动作。
                        token.ThrowIfCancellationRequested();
                        action.Result = await devices.ExecuteAsync(run, step, decision, token);
                        action.Success = true;
                    }
                    catch (Exception ex) { action.Result = ex.Message; throw; }
                    finally { action.Finished = DateTimeOffset.UtcNow; Storage.Event(run, new { kind = "action-end", action }); Update(); }
                    log(action.Tool + " · " + action.Result);
                }
                step.Finished = DateTimeOffset.UtcNow; Update();
                if (step.Outcome != Outcome.Passed) { log("当前步骤未通过，保留证据并停止后续依赖步骤。"); break; }
            }
            run.Outcome = run.Steps.Any(s => s.Outcome == Outcome.Failed) ? Outcome.Failed : run.Steps.All(s => s.Outcome == Outcome.Passed) ? Outcome.Passed : Outcome.Inconclusive;
            run.Message = "本次结论仅适用于报告列明的覆盖范围。";
        }
        catch (OperationCanceledException)
        {
            run.Outcome = cancellationToken.IsCancellationRequested ? Outcome.Cancelled : Outcome.Error;
            run.Message = cancellationToken.IsCancellationRequested ? "用户停止任务。已发生的操作与证据保留。" : "达到任务总用时上限，已停止后续操作。";
            if (active?.Outcome == Outcome.Running) { active.Outcome = run.Outcome; active.Actual = run.Message; active.Finished = DateTimeOffset.UtcNow; }
            log(run.Message);
        }
        catch (Exception ex)
        {
            run.Outcome = Outcome.Error; run.Message = ex.Message;
            if (active?.Outcome == Outcome.Running) { active.Outcome = Outcome.Error; active.Actual = ex.Message; active.Finished = DateTimeOffset.UtcNow; }
            Storage.Event(run, new { kind = "error", message = ex.ToString() }); log("执行异常：" + ex.Message);
        }
        finally
        {
            log("正在停止监控、刷新原始数据并生成报告…");
            try { await devices.DisposeAsync(); } catch (Exception ex) { run.Message += "\n设备收尾异常：" + ex.Message; run.Outcome = Outcome.Error; }
            foreach (var step in run.Steps.Where(s => s.Outcome == Outcome.Pending)) { step.Outcome = Outcome.Skipped; step.Actual = "前置步骤未完成，未执行。"; }
            run.Finished = DateTimeOffset.UtcNow;
            foreach (var file in Directory.EnumerateFiles(Path.Combine(run.Directory, "logs"), "*", SearchOption.AllDirectories))
                if (!run.Evidence.Any(e => Path.GetFullPath(Path.Combine(run.Directory, e.Path)) == file)) storage.AddEvidence(run, file, "raw-log", "", summary: "原始设备数据 / 日志");
            var aiDir = Path.Combine(run.Directory, "ai");
            if (Directory.Exists(aiDir))
                foreach (var file in Directory.EnumerateFiles(aiDir, "response.json", SearchOption.AllDirectories)) storage.AddEvidence(run, file, "ai-response", "", summary: "Codex 最终结构化回答");
            Storage.Event(run, new { kind = "run-end", run.Outcome, run.Message });
            Update(); Report.Generate(run);
            log($"任务{Report.Label(run.Outcome)} · 报告已保存：{Path.Combine(run.Directory, "report.html")}");
        }
        return run;
    }

    public static void ValidateDecision(Decision decision, IReadOnlyList<Evidence> current)
    {
        if (decision.Kind is not ("action" or "pass" or "fail" or "inconclusive") || string.IsNullOrWhiteSpace(decision.Observation)) throw new InvalidDataException("AI 判定类型或观察说明无效。");
        if (decision.EvidenceIds.Any(id => !current.Any(e => e.Id == id))) throw new InvalidDataException("AI 引用了不属于当前观察的证据 ID。");
        if (decision.Kind is "pass" or "fail" && decision.EvidenceIds.Count == 0) throw new InvalidDataException("AI 功能结论没有引用当前证据，已拒绝该判定。");
        if (decision.Kind == "action" && string.IsNullOrWhiteSpace(decision.Tool)) throw new InvalidDataException("AI 动作没有指定工具。");
        if (decision.Kind == "action" && decision.Tool.StartsWith("adb.", StringComparison.Ordinal)) decision.Role = AdbSlots.Normalize(decision.Role);
        if (decision.Kind != "action" && (decision.Tool.Length > 0 || decision.Role.Length > 0)) throw new InvalidDataException("结论中不能同时携带设备动作。");
        using var doc = System.Text.Json.JsonDocument.Parse(decision.Arguments);
        if (doc.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object) throw new InvalidDataException("AI 动作参数不是 JSON 对象。");
    }
}

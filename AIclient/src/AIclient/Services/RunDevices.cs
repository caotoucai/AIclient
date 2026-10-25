using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AIclient.Models;

namespace AIclient.Services;

public interface IRunDevices : IAsyncDisposable
{
    Task StartAsync(RunRecord run, CancellationToken token);
    Task<IReadOnlyList<Evidence>> ObserveAsync(RunRecord run, StepRecord step, CancellationToken token);
    Task<string> ExecuteAsync(RunRecord run, StepRecord step, Decision decision, CancellationToken token);
}

public sealed class RunDevices(AppSettings settings, Storage storage, Action<string> log, Action<Evidence, string> screenshot) : IRunDevices
{
    private readonly AdbDevice adb = new(settings);
    private readonly Dictionary<string, SerialChannel> ports = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, (int width, int height)> sizes = [];
    private readonly ConcurrentQueue<MonitorRecord> recent = new();
    private readonly ConcurrentQueue<Exception> faults = new();
    private readonly ConcurrentDictionary<(uint id, bool extended), CanFrame> frames = new();
    private readonly object logLock = new();
    private readonly List<StreamWriter> writers = [];
    private DbcDatabase? dbc;
    private RunRecord? currentRun;
    private readonly StringBuilder canBuffer = new();
    private SerialChannel? canPort;
    private LegacyBusClient? legacyBus;
    private VisaInstrument? powerVisa, measurementVisa;

    private sealed record MonitorRecord(DateTimeOffset At, string Channel, string Direction, string Hex, string Text);

    public async Task StartAsync(RunRecord run, CancellationToken token)
    {
        currentRun = run;
        settings.Validate();
        if (string.IsNullOrWhiteSpace(settings.Adb1Serial) && string.IsNullOrWhiteSpace(settings.Adb2Serial) && !settings.SerialPorts.Any(s => s.Enabled) && !settings.Bus.Enabled && !settings.Power.Enabled) throw new InvalidOperationException("请先绑定至少一个 ADB 设备或启用真实设备通道，或选择演示模式。");
        foreach (var binding in settings.BoundAdbDevices())
        {
            var role = binding.Slot;
            await adb.CheckAsync(role, token);
            log($"{AdbSlots.Label(role)} 在线：{adb.Serial(role)}");
            var version = await adb.CommandAsync(role, ["shell", "getprop", "ro.build.fingerprint"], token);
            File.WriteAllText(Path.Combine(run.Directory, "logs", role + "-build.txt"), version);
            storage.AddEvidence(run, Path.Combine(run.Directory, "logs", role + "-build.txt"), "device-info", "", role, adb.Serial(role), "Android build fingerprint");
        }
        foreach (var config in settings.SerialPorts.Where(s => s.Enabled)) OpenSerial(run, config);
        if (settings.Power.Enabled && settings.Power.Transport == "visa")
        {
            powerVisa = await Task.Run(() => new VisaInstrument(settings.Power.Resource, settings.Power.TimeoutMs), token);
            if (!string.IsNullOrWhiteSpace(settings.Power.MeasurementResource) && settings.Power.MeasurementResource != settings.Power.Resource)
                measurementVisa = await Task.Run(() => new VisaInstrument(settings.Power.MeasurementResource, settings.Power.TimeoutMs), token);
            log("VISA 电源已打开；未自动切换输出或设置电压。");
        }
        if (settings.Bus.Enabled && settings.Bus.Adapter == "Legacy")
        {
            legacyBus = new LegacyBusClient(Path.Combine(run.Directory, "logs"), line =>
            {
                lock (logLock) File.AppendAllText(Path.Combine(run.Directory, "logs", "legacy-bus.jsonl"), line + "\n");
                using var doc = JsonDocument.Parse(line); var v = doc.RootElement;
                var record = new MonitorRecord(DateTimeOffset.UtcNow, "bus", v.GetProperty("direction").GetString() ?? "RX", v.GetProperty("data").GetString() ?? "", line);
                recent.Enqueue(record); while (recent.Count > 5000) recent.TryDequeue(out _);
                if (settings.Bus.Protocol is "CAN" or "CAN FD" && string.Equals(record.Direction, "rx", StringComparison.OrdinalIgnoreCase))
                {
                    var id = v.GetProperty("id").GetUInt32();
                    var extended = v.TryGetProperty("extended", out var ext) && ext.GetBoolean();
                    frames[(id, extended)] = new CanFrame(id, extended, Convert.FromHexString(record.Hex), record.At);
                }
            });
            await legacyBus.CallAsync("open", new { protocol = settings.Bus.Protocol, device = settings.Bus.DeviceName, database = settings.Bus.DbcPath, bitRate = settings.Bus.BitRate, dataRate = settings.Bus.DataRate, settings.Bus.LinMaster, settings.Bus.LinBaud, settings.Bus.LinVersion, settings.Bus.ErayFrame, settings.Bus.ColdFrame, settings.Bus.NodeName, settings.Bus.Timing }, token);
        }
        if (settings.Bus.Enabled && settings.Bus.Adapter == "SLCAN")
        {
            settings.Bus.Serial.Validate();
            if (settings.Bus.Serial.Enabled && ports.ContainsKey(settings.Bus.Serial.Name)) throw new InvalidOperationException("CAN 与普通串口的逻辑名称不能重复。");
            canPort = OpenSerial(run, settings.Bus.Serial, true);
            // 先关闭通道再设置速率，最后打开；接收路径在发送初始化命令前已经就绪。
            Send(canPort, Encoding.ASCII.GetBytes("C\r"));
            await Task.Delay(100, token);
            Send(canPort, Encoding.ASCII.GetBytes(Slcan.SpeedCommand(settings.Bus.BitRate) + "\r"));
            Send(canPort, Encoding.ASCII.GetBytes("O\r"));
            log("SLCAN 通道已打开；是否收到真实帧请以采集数据为准。");
        }
        if (settings.Bus.Enabled && !string.IsNullOrWhiteSpace(settings.Bus.DbcPath))
        {
            if (settings.Bus.Protocol is "CAN" or "CAN FD")
            {
                dbc = DbcDatabase.Load(settings.Bus.DbcPath);
                var path = Path.Combine(run.Directory, "logs", "database.dbc"); File.Copy(settings.Bus.DbcPath, path, true);
                storage.AddEvidence(run, path, "dbc", "", summary: $"{dbc.Signals.Count} 个基本信号；{dbc.Warnings.Count} 个不支持的条目");
                foreach (var warning in dbc.Warnings) log(warning);
            }
            else
            {
                var path = Path.Combine(run.Directory, "logs", "database" + Path.GetExtension(settings.Bus.DbcPath));
                File.Copy(settings.Bus.DbcPath, path, true); storage.AddEvidence(run, path, "bus-database", "", summary: settings.Bus.Protocol + " 数据库");
            }
        }
    }

    private SerialChannel OpenSerial(RunRecord run, SerialConfig config, bool isCan = false)
    {
        if (ports.ContainsKey(config.Name)) throw new InvalidOperationException("串口逻辑名重复：" + config.Name);
        if (ports.Values.Any(p => string.Equals(p.Config.Port, config.Port, StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("物理串口被重复配置：" + config.Port);
        var port = new SerialChannel(config);
        var path = Path.Combine(run.Directory, "logs", (isCan ? "bus" : "serial-" + Convert.ToHexString(Encoding.UTF8.GetBytes(config.Name))) + ".jsonl");
        var writer = new StreamWriter(path, false, new UTF8Encoding(false)) { AutoFlush = true };
        writers.Add(writer);
        var decoder = Encoding.GetEncoding(config.Encoding).GetDecoder();
        port.Received += bytes =>
        {
            var chars = new char[Encoding.GetEncoding(config.Encoding).GetMaxCharCount(bytes.Length)];
            var text = new string(chars, 0, decoder.GetChars(bytes, 0, bytes.Length, chars, 0));
            var record = new MonitorRecord(DateTimeOffset.UtcNow, config.Name, "RX", Convert.ToHexString(bytes), text);
            lock (logLock) { writer.WriteLine(System.Text.Json.JsonSerializer.Serialize(record)); }
            recent.Enqueue(record); while (recent.Count > 5000) recent.TryDequeue(out _);
            if (isCan)
            {
                foreach (var ch in text)
                {
                    if (ch == '\a') { faults.Enqueue(new IOException("SLCAN 适配器返回错误应答。")); continue; }
                    if (ch is '\r' or '\n')
                    {
                        var frame = Slcan.Parse(canBuffer.ToString()); canBuffer.Clear();
                        if (frame != null) frames[(frame.Id, frame.Extended)] = frame;
                    }
                    else if (canBuffer.Length < 256) canBuffer.Append(ch); else canBuffer.Clear();
                }
            }
        };
        port.Faulted += ex => { faults.Enqueue(ex); log($"设备采集异常：{config.Name} / {ex.Message}"); };
        try { port.Open(); ports.Add(config.Name, port); } catch { writer.Dispose(); throw; }
        log($"已打开串口 {config.Name}：{config.Port} @ {config.BaudRate}");
        return port;
    }

    private void CheckFaults() { legacyBus?.CheckHealthy(); if (faults.TryPeek(out var error)) throw new IOException("设备监控已异常，停止依赖当前数据的测试。", error); }

    public async Task<IReadOnlyList<Evidence>> ObserveAsync(RunRecord run, StepRecord step, CancellationToken token)
    {
        CheckFaults();
        var result = new List<Evidence>();
        foreach (var binding in settings.BoundAdbDevices())
        {
            var role = binding.Slot;
            token.ThrowIfCancellationRequested();
            var path = Path.Combine(run.Directory, "screenshots", $"{run.Evidence.Count + 1:D5}_{role}_{Guid.NewGuid():N}.png");
            var size = await adb.CaptureAsync(role, path, token);
            sizes[role] = size;
            var e = storage.AddEvidence(run, path, "screenshot", step.Id, role, adb.Serial(role), "当前设备原始截图", size.width, size.height);
            result.Add(e); screenshot(e, path);
        }
        if (ports.Count > 0 || legacyBus != null || powerVisa != null)
        {
            var entries = recent.Where(r => r.At >= step.Started).TakeLast(150).ToArray();
            var path = Path.Combine(run.Directory, "logs", "snapshot_" + Guid.NewGuid().ToString("N") + ".json");
            var values = dbc?.Signals.Select(s =>
            {
                if (!frames.TryGetValue((s.Id, s.Extended), out var f)) return $"{s.Name}：无数据";
                try { return $"{s.Name}={s.Decode(f.Data).ToString(CultureInfo.InvariantCulture)} {s.Unit} @ {f.At:O}"; } catch (Exception ex) { return s.Name + "：" + ex.Message; }
            }).ToArray() ?? [];
            Storage.WriteJson(path, new { from = step.Started, to = DateTimeOffset.UtcNow, records = entries, signals = values });
            var summary = Json.Serialize(new { records = entries.TakeLast(40), signals = values });
            if (summary.Length > 14000) summary = summary[..14000] + "\n（摘要截断，完整数据见证据文件）";
            result.Add(storage.AddEvidence(run, path, "monitor", step.Id, summary: summary));
        }
        CheckFaults();
        return result;
    }

    public async Task<string> ExecuteAsync(RunRecord run, StepRecord step, Decision d, CancellationToken token)
    {
        CheckFaults(); token.ThrowIfCancellationRequested();
        using var document = JsonDocument.Parse(d.Arguments);
        var a = document.RootElement;
        if (a.ValueKind != JsonValueKind.Object) throw new InvalidDataException("动作参数必须是 JSON 对象。");
        string S(string key) => a.GetProperty(key).GetString() ?? throw new InvalidDataException("动作参数缺失：" + key);
        int I(string key) => a.GetProperty(key).GetInt32();
        void Bounds(int x, int y)
        {
            if (!sizes.TryGetValue(d.Role, out var size) || x < 0 || y < 0 || x >= size.width || y >= size.height) throw new InvalidDataException("点击/滑动坐标超出最近截图尺寸。");
        }
        if (d.Tool.StartsWith("adb.", StringComparison.Ordinal)) { d.Role = AdbSlots.Normalize(d.Role); _ = adb.Serial(d.Role); }
        else if (d.Role.Length > 0) throw new InvalidDataException("非 ADB 工具的 role 应为空。");
        switch (d.Tool)
        {
            case "adb.capture": return "将在下一轮采集当前设备截图。";
            case "adb.tap":
                Bounds(I("x"), I("y"));
                await adb.CommandAsync(d.Role, ["shell", "input", "tap", AdbDevice.Number(I("x")), AdbDevice.Number(I("y"))], token);
                break;
            case "adb.swipe":
                Bounds(I("x1"), I("y1")); Bounds(I("x2"), I("y2")); var ms = I("durationMs");
                if (ms is < 50 or > 5000) throw new InvalidDataException("滑动时长需为 50–5000ms。");
                await adb.CommandAsync(d.Role, ["shell", "input", "swipe", AdbDevice.Number(I("x1")), AdbDevice.Number(I("y1")), AdbDevice.Number(I("x2")), AdbDevice.Number(I("y2")), AdbDevice.Number(ms)], token); break;
            case "adb.key":
                var key = S("key");
                if (key is "SETTINGS" or "BLUETOOTH_SETTINGS") await adb.CommandAsync(d.Role, ["shell", "am", "start", "-a", key == "SETTINGS" ? "android.settings.SETTINGS" : "android.settings.BLUETOOTH_SETTINGS"], token);
                else
                {
                    var code = key switch { "BACK" => "4", "HOME" => "3", "ENTER" => "66", "APP_SWITCH" => "187", _ => throw new InvalidDataException("不支持的 ADB 按键。") };
                    await adb.CommandAsync(d.Role, ["shell", "input", "keyevent", code], token);
                }
                break;
            case "adb.text":
                var text = S("text");
                if (!Regex.IsMatch(text, @"\A[A-Za-z0-9@._+:/ -]{1,200}\z")) throw new InvalidDataException("首版 ADB 输入只支持常用 ASCII 文本，不支持 Shell 特殊字符或中文输入。");
                await adb.CommandAsync(d.Role, ["shell", "input", "text", text.Replace(" ", "%s")], token); break;
            case "serial.send":
            case "bus.send":
                var command = settings.Commands.SingleOrDefault(c => c.Alias == S("alias")) ?? throw new InvalidDataException("命令别名未配置。");
                if (d.Tool == "bus.send")
                {
                    if (command.Transport != "bus") throw new InvalidDataException("总线命令传输类型不匹配。");
                    if (legacyBus != null)
                    {
                        var data = Convert.FromHexString(command.Payload.Replace(" ", ""));
                        var max = settings.Bus.Protocol switch { "CAN FD" => 64, "FlexRay" => 254, _ => 8 };
                        if (data.Length > max || (settings.Bus.Protocol == "LIN" && command.FrameId > 63) || (settings.Bus.Protocol is "CAN" or "CAN FD" && command.FrameId > 0x1FFFFFFF)) throw new InvalidDataException("配置报文 ID 或长度超出协议范围。");
                        var sent = await legacyBus.CallAsync("send", new { id = command.FrameId, frame = command.FrameName, data = Convert.ToHexString(data) }, token);
                        lock (logLock) File.AppendAllText(Path.Combine(run.Directory, "logs", "transmit.jsonl"), Json.Serialize(new { at = DateTimeOffset.UtcNow, command.Alias, command.FrameId, command.FrameName, data = Convert.ToHexString(data), result = sent }).Replace("\n", "") + "\n");
                        return sent.GetString() ?? "总线报文已发送。";
                    }
                    if (canPort == null) throw new InvalidDataException("CAN 设备未连接。");
                    var frame = Slcan.Parse(command.Payload) ?? throw new InvalidDataException("CAN 命令 payload 必须是有效 SLCAN 数据帧，如 t1232AABB。");
                    Send(canPort, Encoding.ASCII.GetBytes(Slcan.Format(frame) + "\r"));
                }
                else
                {
                    if (command.Transport != "serial") throw new InvalidDataException("命令传输类型不匹配。");
                    var port = Port(command.Channel);
                    Send(port, command.Hex ? Convert.FromHexString(command.Payload.Replace(" ", "")) : port.Encode(command.Payload));
                }
                return "已发送配置中的命令：" + command.Alias + "；功能效果仍需证据验证。";
            case "serial.expect":
                var channel = S("channel"); _ = Port(channel); var pattern = S("pattern");
                if (pattern.Length is < 1 or > 300) throw new InvalidDataException("串口关键字应为 1–300 字符。");
                await WaitUntilAsync(() => string.Concat(recent.Where(r => r.Channel == channel && r.Direction == "RX" && r.At >= step.Started).Select(r => r.Text)).Contains(pattern, StringComparison.Ordinal), I("timeoutMs"), token);
                return "当前步骤的串口接收日志已匹配字面关键字：" + pattern;
            case "bus.expect":
                var signal = dbc?.Signals.Where(s => s.Name == S("signal")).ToArray() ?? [];
                if (signal.Length != 1) throw new InvalidDataException("信号不存在或在多个报文中重名，请调整 DBC/项目配置。");
                var expected = a.GetProperty("expected").GetDouble(); var tolerance = a.GetProperty("tolerance").GetDouble();
                if (!double.IsFinite(expected) || !double.IsFinite(tolerance) || tolerance < 0) throw new InvalidDataException("信号预期或容差无效。");
                await WaitUntilAsync(() => frames.TryGetValue((signal[0].Id, signal[0].Extended), out var frame) && frame.At >= step.Started && Math.Abs(signal[0].Decode(frame.Data) - expected) <= tolerance, I("timeoutMs"), token);
                return "当前步骤的实时 CAN 信号已匹配预期值。";
            case "power.output":
                if (!settings.Power.Enabled) throw new InvalidDataException("电源未启用。");
                var on = a.GetProperty("on").GetBoolean();
                await PowerSendAsync(run, on ? settings.Power.OnCommand : settings.Power.OffCommand, token);
                return "已发送电源输出命令；无测量回读时实际输出状态未知。";
            case "power.voltage":
                if (!settings.Power.Enabled) throw new InvalidDataException("电源未启用。");
                var volts = a.GetProperty("volts").GetDouble();
                if (!double.IsFinite(volts) || volts < settings.Power.MinVoltage || volts > settings.Power.MaxVoltage || !settings.Power.VoltageCommand.Contains("{value}", StringComparison.Ordinal)) throw new InvalidDataException("电源电压越界或模板缺少 {value}。");
                await PowerSendAsync(run, settings.Power.VoltageCommand.Replace("{value}", volts.ToString(CultureInfo.InvariantCulture)), token);
                return "已发送设压命令；需设备回读才能验证实际电压。";
            case "power.measure":
                if (!settings.Power.Enabled || powerVisa == null) throw new InvalidDataException("电源测量需要启用 VISA 电源及真实回读命令。");
                var quantity = S("quantity");
                var query = quantity switch { "voltage" => settings.Power.VoltageQuery, "current" => settings.Power.CurrentQuery, _ => throw new InvalidDataException("测量量需为 voltage 或 current。") };
                var raw = await Task.Run(() => (measurementVisa ?? powerVisa).Query(query), token);
                token.ThrowIfCancellationRequested();
                if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var measured) || !double.IsFinite(measured)) throw new InvalidDataException("电源回读不是有效的单个数值：" + raw);
                var measurementPath = Path.Combine(run.Directory, "logs", "power_" + Guid.NewGuid().ToString("N") + ".json");
                Storage.WriteJson(measurementPath, new { quantity, query, raw, measured, at = DateTimeOffset.UtcNow, resource = string.IsNullOrWhiteSpace(settings.Power.MeasurementResource) ? settings.Power.Resource : settings.Power.MeasurementResource });
                storage.AddEvidence(run, measurementPath, "measurement", step.Id, summary: $"实测 {quantity}={raw}");
                recent.Enqueue(new(DateTimeOffset.UtcNow, "power", "RX", "", $"实测 {quantity}={raw}"));
                return $"VISA 实际回读 {quantity}={raw}，原始数据已保存。";
            case "wait":
                var duration = I("durationMs"); if (duration is < 100 or > 30000) throw new InvalidDataException("等待时间需为 100–30000ms。");
                await Task.Delay(duration, token); return "等待完成，期间设备数据持续采集。";
            default: throw new InvalidDataException("模型请求了未提供的工具：" + d.Tool);
        }
        await Task.Delay(400, token);
        return "ADB 命令已执行，下一轮将用新截图验证实际状态。";
    }

    private SerialChannel Port(string name) => ports.TryGetValue(name, out var port) && port != canPort ? port : throw new InvalidOperationException("串口通道未启用或类型不匹配：" + name);
    private async Task PowerSendAsync(RunRecord run, string command, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(command)) throw new InvalidDataException("电源命令未配置。");
        if (powerVisa == null) { var p = Port(settings.Power.Channel); Send(p, p.Encode(command)); }
        else
        {
            await Task.Run(() => powerVisa.Send(command), token);
            lock (logLock) File.AppendAllText(Path.Combine(run.Directory, "logs", "power-transmit.jsonl"), System.Text.Json.JsonSerializer.Serialize(new { at = DateTimeOffset.UtcNow, command, settings.Power.Resource }) + "\n");
        }
    }
    private void Send(SerialChannel port, byte[] data)
    {
        port.Send(data);
        var entry = new MonitorRecord(DateTimeOffset.UtcNow, port.Config.Name, "TX", Convert.ToHexString(data), Encoding.GetEncoding(port.Config.Encoding).GetString(data));
        recent.Enqueue(entry);
        if (currentRun != null) lock (logLock) File.AppendAllText(Path.Combine(currentRun.Directory, "logs", "transmit.jsonl"), System.Text.Json.JsonSerializer.Serialize(entry) + "\n");
    }
    private async Task WaitUntilAsync(Func<bool> predicate, int milliseconds, CancellationToken token)
    {
        if (milliseconds is < 100 or > 30000) throw new InvalidDataException("检查超时需为 100–30000ms。");
        var watch = System.Diagnostics.Stopwatch.StartNew();
        while (watch.ElapsedMilliseconds < milliseconds) { CheckFaults(); token.ThrowIfCancellationRequested(); if (predicate()) return; await Task.Delay(100, token); }
        throw new TimeoutException("指定时间内未观察到预期的设备数据。");
    }
    public async ValueTask DisposeAsync()
    {
        var errors = new List<Exception>();
        if (legacyBus != null) { try { await legacyBus.DisposeAsync(); } catch (Exception ex) { errors.Add(ex); } }
        try { measurementVisa?.Dispose(); } catch (Exception ex) { errors.Add(ex); }
        try { powerVisa?.Dispose(); } catch (Exception ex) { errors.Add(ex); }
        if (canPort != null) { try { Send(canPort, Encoding.ASCII.GetBytes("C\r")); } catch { } }
        foreach (var port in ports.Values) { try { await port.DisposeAsync(); } catch (Exception ex) { errors.Add(ex); } }
        foreach (var writer in writers) { try { writer.Dispose(); } catch (Exception ex) { errors.Add(ex); } }
        ports.Clear();
        if (errors.Count > 0) throw new AggregateException("部分设备关闭失败。", errors);
    }
}

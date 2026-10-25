using System.Globalization;
using System.Text.RegularExpressions;

namespace AIclient.Services;

public sealed record CanFrame(uint Id, bool Extended, byte[] Data, DateTimeOffset At);
public static class Slcan
{
    public static CanFrame? Parse(string line)
    {
        if (string.IsNullOrEmpty(line) || line[0] is not ('t' or 'T')) return null;
        var extended = line[0] == 'T'; var digits = extended ? 8 : 3;
        if (line.Length < 2 + digits || !uint.TryParse(line.AsSpan(1, digits), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var id) || id > (extended ? 0x1FFFFFFFu : 0x7FFu)) return null;
        if (!int.TryParse(line.AsSpan(1 + digits, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var dlc) || dlc > 8 || line.Length < 2 + digits + dlc * 2) return null;
        var expectedLength = 2 + digits + dlc * 2;
        if (line.Length != expectedLength && (line.Length != expectedLength + 4 || !ushort.TryParse(line.AsSpan(expectedLength), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))) return null;
        var data = new byte[dlc];
        for (var i = 0; i < dlc; i++) if (!byte.TryParse(line.AsSpan(2 + digits + i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out data[i])) return null;
        return new CanFrame(id, extended, data, DateTimeOffset.UtcNow);
    }
    public static string Format(CanFrame f)
    {
        if (f.Data.Length > 8 || f.Id > (f.Extended ? 0x1FFFFFFFu : 0x7FFu)) throw new InvalidDataException("SLCAN 首版只支持 classic CAN（最多 8 字节）。");
        return (f.Extended ? "T" : "t") + f.Id.ToString(f.Extended ? "X8" : "X3") + f.Data.Length.ToString("X1") + Convert.ToHexString(f.Data);
    }
    public static string SpeedCommand(int bitrate) => bitrate switch { 10000 => "S0", 20000 => "S1", 50000 => "S2", 100000 => "S3", 125000 => "S4", 250000 => "S5", 500000 => "S6", 800000 => "S7", 1000000 => "S8", _ => throw new InvalidDataException("SLCAN 波特率不支持。") };
}

public sealed record DbcSignal(uint Id, bool Extended, string Name, int Start, int Length, bool LittleEndian, bool Signed, double Factor, double Offset, string Unit)
{
    public double Decode(byte[] bytes)
    {
        if (Length is < 1 or > 64) throw new InvalidDataException("DBC 信号长度超出支持范围。");
        ulong raw = 0;
        var pos = Start;
        for (var i = 0; i < Length; i++)
        {
            if (pos < 0 || pos >= bytes.Length * 8) throw new InvalidDataException($"DBC 信号 {Name} 超出当前帧长度。");
            var bit = (ulong)((bytes[pos / 8] >> (pos % 8)) & 1);
            if (LittleEndian) { raw |= bit << i; pos++; }
            else { raw = (raw << 1) | bit; pos = pos % 8 == 0 ? pos + 15 : pos - 1; }
        }
        double value = Signed && (raw & (1UL << (Length - 1))) != 0 ? (Length == 64 ? unchecked((long)raw) : (long)(raw | (ulong.MaxValue << Length))) : raw;
        return value * Factor + Offset;
    }
}
public sealed class DbcDatabase
{
    public List<DbcSignal> Signals { get; } = [];
    public List<string> Warnings { get; } = [];
    public static DbcDatabase Load(string path)
    {
        var db = new DbcDatabase();
        uint? id = null; bool extended = false;
        foreach (var line in File.ReadLines(path))
        {
            var message = Regex.Match(line, @"^BO_\s+(\d+)\s+");
            if (message.Success) { var value = uint.Parse(message.Groups[1].Value, CultureInfo.InvariantCulture); extended = (value & 0x80000000) != 0; id = value & 0x1FFFFFFF; continue; }
            if (!line.TrimStart().StartsWith("SG_ ", StringComparison.Ordinal) || id == null) continue;
            var match = Regex.Match(line, "^\\s*SG_\\s+(\\w+)\\s*:\\s*(\\d+)\\|(\\d+)@([01])([+-])\\s*\\(([+-]?[\\d.eE]+),([+-]?[\\d.eE]+)\\).*?\\\"([^\\\"]*)\\\"");
            if (!match.Success) { db.Warnings.Add("未支持的复用或复杂信号：" + line.Trim()); continue; }
            var g = match.Groups;
            var signal = new DbcSignal(id.Value, extended, g[1].Value, int.Parse(g[2].Value), int.Parse(g[3].Value), g[4].Value == "1", g[5].Value == "-", double.Parse(g[6].Value, CultureInfo.InvariantCulture), double.Parse(g[7].Value, CultureInfo.InvariantCulture), g[8].Value);
            if (signal.Length is < 1 or > 64) db.Warnings.Add("未支持的信号长度：" + signal.Name); else db.Signals.Add(signal);
        }
        var text = File.ReadAllText(path);
        if (Regex.IsMatch(text, @"(?m)^\s*(?:SIG_VALTYPE_|SG_MUL_VAL_)\s+\d+")) throw new InvalidDataException("此 DBC 含浮点或扩展多路复用信号，首版不支持，不能按普通整数解释。");
        return db;
    }
}

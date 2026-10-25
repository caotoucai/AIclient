using System.Runtime.InteropServices;
using System.Text;

namespace AIclient.Services;

/// <summary>通过本机厂商 VISA Runtime 的标准 C API 调用，无需加载旧版 .NET VISA 程序集。</summary>
public sealed class VisaInstrument : IDisposable
{
    private nint library;
    private uint manager, session;
    private readonly Close close;
    private readonly Write write;
    private readonly Read read;
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int OpenManager(out uint session);
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)] private delegate int Open(uint manager, string resource, uint mode, uint timeout, out uint session);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int Close(uint session);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int Attribute(uint session, uint attribute, nuint value);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int Write(uint session, byte[] bytes, uint count, out uint written);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int Read(uint session, byte[] bytes, uint count, out uint read);
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)] private delegate int Find(uint manager, string expression, out uint list, out uint count, StringBuilder description);
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)] private delegate int Next(uint list, StringBuilder description);
    private T Api<T>(string name) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));
    private static void Check(int status) { if (status < 0) throw new IOException($"VISA 调用失败：0x{status:X8}。请检查本机 VISA Runtime、资源名称和设备连接。"); }
    public VisaInstrument(string resource = "", int timeoutMs = 5000)
    {
        if (timeoutMs is < 100 or > 30000) throw new InvalidDataException("VISA 超时需为 100–30000ms。");
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), Environment.Is64BitProcess ? "visa64.dll" : "visa32.dll");
        if (!NativeLibrary.TryLoad(path, out library)) throw new InvalidOperationException("未找到匹配进程位数的 VISA Runtime，请安装 NI/Keysight/RIGOL 等厂商的 VISA 驱动。");
        try
        {
            close = Api<Close>("viClose"); write = Api<Write>("viWrite"); read = Api<Read>("viRead");
            Check(Api<OpenManager>("viOpenDefaultRM")(out manager));
            if (resource.Length > 0) { Check(Api<Open>("viOpen")(manager, resource, 0, (uint)timeoutMs, out session)); Check(Api<Attribute>("viSetAttribute")(session, 0x3FFF001A, (nuint)timeoutMs)); }
        }
        catch { Dispose(); throw; }
    }
    public string[] Scan()
    {
        var description = new StringBuilder(1024); uint list = 0;
        try
        {
            var status = Api<Find>("viFindRsrc")(manager, "?*INSTR", out list, out var count, description);
            if (status == unchecked((int)0xBFFF0011)) return [];
            Check(status); var values = new List<string>();
            if (count > 0) values.Add(description.ToString());
            for (var i = 1; i < count; i++) { description.Clear(); Check(Api<Next>("viFindNext")(list, description)); values.Add(description.ToString()); }
            return values.ToArray();
        }
        finally { if (list != 0) close(list); }
    }
    public void Send(string command)
    {
        if (session == 0) throw new InvalidOperationException("VISA 设备未打开。");
        if (string.IsNullOrWhiteSpace(command)) throw new InvalidDataException("设备命令未配置。");
        var bytes = Encoding.ASCII.GetBytes(command.TrimEnd('\r', '\n') + "\n");
        Check(write(session, bytes, (uint)bytes.Length, out var count));
        if (count != bytes.Length) throw new IOException("VISA 命令未完整发送。");
    }
    public string Query(string command)
    {
        Send(command); var bytes = new byte[8192]; Check(read(session, bytes, (uint)bytes.Length, out var count));
        return Encoding.ASCII.GetString(bytes, 0, (int)count).Trim();
    }
    public void Dispose() { if (session != 0) { close(session); session = 0; } if (manager != 0) { close(manager); manager = 0; } if (library != 0) { NativeLibrary.Free(library); library = 0; } }
}

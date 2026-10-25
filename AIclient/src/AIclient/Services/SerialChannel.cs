using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using AIclient.Models;

namespace AIclient.Services;

/// <summary>Windows 串口，原始接收字节始终交给调用方保存。</summary>
public sealed class SerialChannel : IAsyncDisposable
{
    private SafeFileHandle? handle;
    private readonly CancellationTokenSource stop = new();
    private Task? receive;
    private readonly object writeLock = new();
    public SerialConfig Config { get; }
    public event Action<byte[]>? Received;
    public event Action<Exception>? Faulted;
    public SerialChannel(SerialConfig config) { Config = config; config.Validate(); }

    public void Open()
    {
        handle = CreateFile(@"\\.\" + Config.Port, 0xC0000000, 0, IntPtr.Zero, 3, 0, IntPtr.Zero);
        if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error(), $"无法打开串口 {Config.Port}");
        try
        {
            var dcb = new Dcb { Length = (uint)Marshal.SizeOf<Dcb>() };
            if (!GetCommState(handle, ref dcb)) throw new Win32Exception(Marshal.GetLastWin32Error());
            dcb.BaudRate = (uint)Config.BaudRate;
            dcb.Flags = 1u | (Config.Parity == "None" ? 0u : 2u) | (1u << 4) | (1u << 12);
            dcb.ByteSize = (byte)Config.DataBits;
            dcb.Parity = Config.Parity switch { "Odd" => 1, "Even" => 2, _ => 0 };
            dcb.StopBits = Config.StopBits == 2 ? (byte)2 : (byte)0;
            if (!SetCommState(handle, ref dcb)) throw new Win32Exception(Marshal.GetLastWin32Error());
            var timeouts = new CommTimeouts { ReadInterval = uint.MaxValue, ReadConstant = 100, WriteConstant = 2000 };
            if (!SetCommTimeouts(handle, ref timeouts)) throw new Win32Exception(Marshal.GetLastWin32Error());
            receive = Task.Run(ReadLoop);
        }
        catch { handle.Dispose(); handle = null; throw; }
    }

    private void ReadLoop()
    {
        var buffer = new byte[4096];
        try
        {
            while (!stop.IsCancellationRequested)
            {
                if (!ReadFile(handle!, buffer, (uint)buffer.Length, out var count, IntPtr.Zero)) throw new Win32Exception(Marshal.GetLastWin32Error());
                if (count > 0) Received?.Invoke(buffer[..(int)count]);
            }
        }
        catch (Exception ex) { if (!stop.IsCancellationRequested) Faulted?.Invoke(ex); }
    }

    public void Send(byte[] bytes)
    {
        lock (writeLock)
        {
            if (handle == null || handle.IsClosed || handle.IsInvalid) throw new InvalidOperationException($"串口 {Config.Name} 未连接。");
            if (!WriteFile(handle, bytes, (uint)bytes.Length, out var count, IntPtr.Zero) || count != bytes.Length) throw new Win32Exception(Marshal.GetLastWin32Error(), "串口发送未完整完成。");
        }
    }
    public byte[] Encode(string text) => Encoding.GetEncoding(Config.Encoding).GetBytes(text + Config.NewLine);
    public async ValueTask DisposeAsync()
    {
        stop.Cancel();
        if (handle is { IsClosed: false }) CancelIoEx(handle, IntPtr.Zero);
        if (receive != null) await receive;
        handle?.Dispose(); stop.Dispose();
    }
    [StructLayout(LayoutKind.Sequential)] private struct Dcb { public uint Length, BaudRate, Flags; public ushort Reserved, XonLim, XoffLim; public byte ByteSize, Parity, StopBits; public sbyte XonChar, XoffChar, ErrorChar, EofChar, EvtChar; public ushort Reserved1; }
    [StructLayout(LayoutKind.Sequential)] private struct CommTimeouts { public uint ReadInterval, ReadMultiplier, ReadConstant, WriteMultiplier, WriteConstant; }
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern SafeFileHandle CreateFile(string name, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetCommState(SafeFileHandle file, ref Dcb dcb);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetCommState(SafeFileHandle file, ref Dcb dcb);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetCommTimeouts(SafeFileHandle file, ref CommTimeouts timeouts);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ReadFile(SafeFileHandle file, byte[] buffer, uint length, out uint read, IntPtr overlapped);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool WriteFile(SafeFileHandle file, byte[] buffer, uint length, out uint written, IntPtr overlapped);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CancelIoEx(SafeFileHandle file, IntPtr overlapped);
}

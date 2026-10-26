using System.Runtime.InteropServices;

namespace AIclient.Services;

public sealed class AdbUsbDiscovery
{
    private readonly Func<string, CancellationToken, Task<string>> scan;
    private readonly Func<bool> usbPresent;
    private readonly Func<string, CancellationToken, Task<bool>> recover;
    private readonly Func<DateTimeOffset> now;
    private DateTimeOffset? emptySince;
    private string? currentPath;
    private bool attempted;
    public event Action<string, string>? Status;

    public AdbUsbDiscovery(Func<string, CancellationToken, Task<string>>? scan = null, Func<bool>? usbPresent = null,
        Func<string, CancellationToken, Task<bool>>? recover = null, Func<DateTimeOffset>? now = null)
    {
        this.scan = scan ?? ((p, t) => new AdbDevice(new() { AdbPath = p }).ScanAsync(t));
        this.usbPresent = usbPresent ?? HasPresentUsbInterface;
        this.recover = recover ?? ((p, t) => AdbServer.Shared.RecoverEmptyUsbAsync(AdbDevice.ResolveExecutable(p), t));
        this.now = now ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<string> ScanAsync(string path, CancellationToken token)
    {
        if (currentPath != path) { currentPath = path; emptySince = null; attempted = false; }
        var result = await scan(path, token);
        token.ThrowIfCancellationRequested();
        if (AdbServer.HasDevices(result) || !usbPresent()) { emptySince = null; attempted = false; return result; }
        emptySince ??= now();
        if (!attempted && now() - emptySince >= TimeSpan.FromSeconds(2))
        {
            attempted = true;
            Status?.Invoke(path, "检测到 USB 设备，正在恢复 ADB 连接…");
            var repaired = await recover(path, token);
            token.ThrowIfCancellationRequested();
            Status?.Invoke(path, repaired ? "ADB 连接已重建，正在重新检测设备…" : "正在等待设备完成 ADB 连接或授权…");
            return await scan(path, token);
        }
        return result;
    }

    public static bool HasPresentUsbInterface()
    {
        // The Android ADB interface class, not a general USB or phone/MTP device.
        var guid = new Guid("F72FE0D4-CBCB-407D-8814-9ED673D0DD6B");
        return CM_Get_Device_Interface_List_SizeW(out var size, ref guid, null, 0) == 0 && size > 1;
    }

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
    private static extern int CM_Get_Device_Interface_List_SizeW(out uint length, ref Guid interfaceClass, string? deviceId, uint flags);
}

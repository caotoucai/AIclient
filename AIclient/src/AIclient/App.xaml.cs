using System.IO;
using System.Windows;
using AIclient.Services;

namespace AIclient;

public partial class App : Application
{
    private Mutex? instanceMutex;
    private bool ownsMutex;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(args.Exception.Message, "AIclient 运行异常", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
        var root = FindRoot();
        var identity = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(Path.GetFullPath(root).ToUpperInvariant())))[..20];
        instanceMutex = new Mutex(true, @"Local\AIclient_" + identity, out ownsMutex);
        if (!ownsMutex) { MessageBox.Show("此数据目录的 AIclient 已经运行，请使用现有窗口。", "AIclient"); Shutdown(); return; }
        var window = new MainWindow(root);
        MainWindow = window;
        window.Show();
        if (e.Args.Contains("--smoke")) window.StartSmoke();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (ownsMutex) instanceMutex?.ReleaseMutex();
        instanceMutex?.Dispose();
        base.OnExit(e);
    }

    public static string FindRoot()
    {
        var explicitPath = Environment.GetEnvironmentVariable("AICLIENT_DATA_ROOT");
        if (!string.IsNullOrWhiteSpace(explicitPath)) return Path.GetFullPath(explicitPath);
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "DESIGN.md"))) return Path.Combine(dir.FullName, "data");
        return Path.Combine(AppContext.BaseDirectory, "data");
    }
}

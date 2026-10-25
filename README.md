# AIclient

Windows 本地 AI 自动化联调工作台，使用自然语言驱动 Android、串口、电源和车辆总线测试，保存逐步证据并生成 HTML 报告。

- 主程序：`AIclient/`，.NET 9 / WPF。
- 旧总线库依赖：`Support/CAN_Library/`，通过独立 .NET Framework 4.8 / x86 兼容进程使用。
- ADB 工具：`Resource/ADB/`。

保留上述目录结构。使用 Windows、.NET 9 SDK 和 .NET Framework 4.8 开发工具构建：

```powershell
dotnet build .\AIclient\AIclient.sln -c Release
.\AIclient\Start-AIclient.ps1
```

首次可使用演示模式。真实设备测试需配置本机 Codex CLI 登录、ADB、实际仪器及对应厂商驱动。

详见 [使用说明](AIclient/README.md)、[架构](AIclient/ARCHITECTURE.md)、[续接记录](AIclient/PROJECT_CONTEXT.md) 和 [验证记录](AIclient/VALIDATION.md)。

本地配置、运行数据、测试报告和编译输出不提交到仓库。自动化软件检查与真实硬件验收的范围以验证记录为准。

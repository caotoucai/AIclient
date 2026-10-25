# AIclient 项目续接记录

最后整理：2026-10-25。此文件用于下次会话快速恢复项目背景；详细实现以源码及 [ARCHITECTURE.md](ARCHITECTURE.md) 为准。

## 2026-10-25 最新续接：Skills 与设备配置

- 新增“测试 Skills”页：专用 SKILL.md、手动补充、自动经验开关、经验来源、编辑和删除。`TestSkills` 按台架指纹加载经验；规划/判断均显式注入最新资料并落盘 skills.json。决策新增 lessons；TestRunner 在有效、未取消、未因暂停丢弃的判定之后保存经验，不增加额外 AI 调用。演示不积累。
- 设备页改为可折叠分组，串口多通道、总线、Power、命令别名可直接编辑，与高级 JSON 同步。表格空白新增行用于新增配置，选中行按 Delete 删除。保存配置与运行前均校验。
- Power 支持 serial / visa；VISA 扫描资源、独立电源/测量绑定、输出、设压、电压/电流回读，工具为 power.output / power.voltage / power.measure。真实硬件兼容性待验收。
- 总线支持 SLCAN 或 Legacy。后者在独立 net48/x86 的 AIclient.DeviceHost 中编译复用 Support/CAN_Library 源码，通过 JSON stdin/stdout 通信，支持 CAN/CAN FD、LIN、FlexRay 的连接及配置报文发送和原始接收。界面含 CAN 速率/高级位时序、LIN 主从/速率/版本、数据库、节点名称和 FlexRay 项目启动帧。
- 兼容编译通过 scripts/Prepare-LegacySource.ps1 将 CAN_Channel.cs 生成到 obj，替换写死的 FlexRay 启动帧，并去掉连接后的自动唤醒；不修改旧 AT 工程文件。升级旧源码时脚本断言失败须复核。
- 一次仍为一个总线通道。bus.expect 仅支持 CAN/CAN FD 基本 DBC 标量，不宣称已覆盖旧 SDK 所有诊断、周期发送、E2E 等方法。设备页手动调用不调用真实 AI，生成独立报告且不自动判功能通过。
- 当前软件检查增至 47 项；最近产物在 artifacts/skills-devices-final-checks 与 artifacts/skills-devices-final-ui。UI 自检包含四个折叠分组、Skills 页和配置读取回写；硬件未验收。

以下 2026-10-24 记录为初版背景，涉及新增能力的描述以上述最新续接为准。

## 下次读取顺序

1. 本文件：用户要求、关键约定和当前进度。
2. [ARCHITECTURE.md](ARCHITECTURE.md)：完整软件架构、模块、协议、配置和维护说明。
3. [README.md](README.md)：启动、真实设备配置和构建方法。
4. [VALIDATION.md](VALIDATION.md)：已完成验证及尚未实机验证的内容。
5. [DESIGN.md](DESIGN.md)：原始需求与整体设计方向，规划内容需与实际代码区分。

## 项目目标与用户已确认的要求

- 在本仓库 `AIclient` 内开发 AI 自动化测试客户端，参考根目录的 `AT_Web_Client` 及其设备插件。用户所称 ATclient 对应这些旧工程。
- 面向汽车车机、手机及其他 Android 设备的联调；客户输入自然语言后，由 AI 拆解目标并循环分析截图、建议动作、验证实际结果。
- 客户端执行已配置设备 API，包括 ADB、串口、车辆总线和电源，保留逐步截图、原始数据和 evidence，生成浏览器可查看的报告。
- 首版本地运行，无需 Web 后端、远程任务或报告上传。
- AI 采用 PowerShell 调用本机 Codex CLI，沿用用户现有登录；当前未设计为必须额外配置 API Key。
- 常规实施选择由开发者继续处理，无需逐项人工确认；操作系统或宿主强制权限审批仍须遵守。

## 必须保留的 ADB 编号约定

- 配置和界面仅按 **ADB1 / ADB2** 绑定，不要求选择车机或手机类型。
- 工具协议和 evidence 的中性编号为 `adb1` / `adb2`，对应 `adb1Serial` / `adb2Serial`。
- 一般可将 ADB1 理解为车机、ADB2 理解为手机，但只是弱默认约定；用户需求、项目背景和真实画面优先。
- 两台均为手机、两台均为车机或其他 Android 组合都有效，不能仅因设备类型阻止测试。
- 真正缺失的业务能力应列为未覆盖或无法判定，不能为了通过测试虚构硬件能力或结果。
- 旧 `headunitSerial` / `phoneSerial` 可读入 ADB1 / ADB2；新字段优先，保存只输出新字段。历史 evidence 的旧标识只做显示映射，不改写原始证据文件。

## 当前软件组成

技术为 C# / .NET 9 Windows / WPF，主程序是 `src/AIclient/AIclient.csproj`，入口解决方案为 `AIclient.sln`。未引用原 AT 客户端程序集，无第三方业务 NuGet 依赖。

```text
MainWindow
  → TestRunner
      → IAiBridge → CodexBridge → ProcessRunner → PowerShell → codex exec
      → IRunDevices → RunDevices → AdbDevice / SerialChannel / SLCAN / DBC
      → Storage / Report
```

- `Models/Domain.cs`：配置、计划、动作、运行、证据与状态；包含 `AdbSlots` 及旧配置兼容。
- `Services/TestRunner.cs`：顺序步骤、观察/动作闭环、预算、暂停/停止、异常收尾。
- `Services/CodexBridge.cs`、`scripts/Invoke-Codex.ps1`：计划与截图决策、结构化 JSON、本机 CLI 调用。
- `Services/RunDevices.cs`：真实设备预检、取证与固定工具分发。
- `Services/AdbDevice.cs`：按编号映射到 serial，截图与 ADB 命令。
- `Services/SerialChannel.cs`、`Services/CanProtocol.cs`：Win32 串口、SLCAN classic CAN 与基本 DBC 标量。
- `Services/Storage.cs`：本地配置/运行文件、证据哈希、历史恢复和静态 HTML 报告。
- `Services/Demo.cs`：独立演示 AI 与模拟截图，不消耗真实 Codex 额度、不控制硬件。

当前是单任务顺序执行；计划校验后自动运行；每次动作后重新取证。没有自动重试和 CLI 会话续接。暂停恢复后会丢弃暂停前的动作建议并重新观察。

## 最近完成与验证状态

- 本地初版已实现并构建，设备设置页、观察页、AI 上下文、实际执行和报告已统一为 ADB1 / ADB2。
- 最近 Release 构建：0 错误、0 警告。
- 最近软件检查：**32 项全部通过**，含双手机设备替身的真实设备层预检、双端截图、不同 serial 点击、报告及旧配置兼容。
- UI 已实际启动并检查设置页和工作台截图。
- 此前已验证本机 CLI 为 ChatGPT 登录，真实 Codex 图片请求可通过 PowerShell 返回结构化结果。登录与设备状态可能变化，续接时按需要复核。
- 尚无真实车机、手机、串口、CAN 和电源的完整台架验收记录；不能把软件替身验证等同于真实蓝牙通过。

最近验证产物：

- `artifacts/adb-slots-checks/checks.json`
- `artifacts/adb-slots-ui/settings-smoke.png`
- `artifacts/adb-slots-ui/ui-smoke.png`
- `artifacts/adb-slots-ui/ui-smoke.json`
- `artifacts/codex-live/response.json`（此前真实 CLI 图像连通性）

## 启动、构建和数据

在仓库根目录执行：

```powershell
# 构建
dotnet build .\AIclient\AIclient.sln -c Release

# 启动；双击 AIclient/Start-AIclient.cmd 也可
.\AIclient\Start-AIclient.ps1

# 修改源码后构建并启动，避免复用旧 EXE
.\AIclient\Start-AIclient.ps1 -Build

# 软件检查，不连接真实硬件、不调用真实 Codex
.\AIclient\tests\AIclient.Checks\bin\Release\net9.0-windows\AIclient.Checks.exe .\AIclient\artifacts\adb-slots-checks
```

默认数据为 `AIclient/data`；环境变量 `AICLIENT_DATA_ROOT` 可指定其他位置。任务文件位于 `data/runs/<runId>`，包括 `run.json`、`plan.json`、`events.jsonl`、`evidence.json`、截图、日志、AI 输入输出和 `report.html`。

运行数据与构建/检查产物不入版本控制。不要清理用户已有配置、历史运行或证据；需要隔离测试时使用独立输出目录。

## 下次工作起点

本轮需求及 ADB 编号调整已完成，当前没有用户指定的下一项待开发任务。下次根据新指令读取相关模块后继续；如需实机联调，先检查设备是否连接、编号映射、真实蓝牙名称与串口/总线/电源协议。

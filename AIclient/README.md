# AIclient 本地版

Windows WPF 车机与手机 AI 自动化联调工作台。设计见 [DESIGN.md](DESIGN.md)。

当前软件的模块职责、执行时序、Codex/设备接口、配置字段、数据结构和维护方法见 [ARCHITECTURE.md](ARCHITECTURE.md)。

下次继续项目时先读取 [PROJECT_CONTEXT.md](PROJECT_CONTEXT.md)，其中记录当前进度、ADB1/ADB2 约定及架构入口；本目录的 [AGENTS.md](AGENTS.md) 也指向该续接记录。

## 启动

双击本目录的 **`Start-AIclient.cmd`**，或直接运行：

```powershell
.\AIclient\Start-AIclient.ps1
```

已经构建的应用位于 `src/AIclient/bin/Release/net9.0-windows/AIclient.exe`。需要本机 .NET 9 Desktop Runtime；当前开发机已安装。启动脚本在没有构建结果时调用本机 SDK 构建。

首次启动默认为**演示模式**，可直接点击“生成计划并运行”，查看双屏截图、步骤、证据和 HTML 报告。演示不调用真实 Codex、不操作设备，报告明确标记为模拟结果。

## 真实设备运行

1. 在“AI 设置 → Codex 配置”检查 Codex、PowerShell 路径、模型与执行预算，点击“保存 AI 设置”。
2. 在同页点击“检测 Codex 登录”。沿用本机现有登录；此前验证为 ChatGPT 登录。不需要在 AIclient 填 API Key，实际额度由登录账号权益决定。
3. 在“设备配置 → ADB 设备”检查 ADB 路径，连接已启用且已授权 ADB 的 Android 设备。设备页每秒自动更新可用列表，进入该页时立即检测，也可点击“刷新”。从下拉列表选择，分别绑定到 **ADB1 / ADB2**。未授权或离线会显示提示，已有绑定不因断开而清除。编号不限定设备类型，两台都可以是手机；支持单设备任务。通道显示只读，使用“解绑”移除绑定。
4. 在独立的“台架背景”页填写真实蓝牙名称、初始配对状态、设备用途、测试范围等，点击“保存台架背景”。例如填写“ADB1 和 ADB2 都是手机”，AI 将按实际需求和截图测试。
5. 在“设备配置”展开串口、电源或总线，刷新设备后选择并绑定，最后点击“保存设备配置”或设备分组下的“保存设置”。所有下拉框只允许选择列表项；未发现设备时不能绑定。串口选中通道后选择波特率等常用参数，总线只显示当前协议的参数；命令模板和位时序在折叠的高级参数内。
6. 回工作台关闭“演示模式”，输入任务并运行。计划生成后自动执行，期间无需逐步确认。暂停后会重新观察，停止后保留报告。

连接到本地 Codex CLI 不等于模型离线推理，仍需其实际使用的网络服务及额度可用。模型留空会继承用户当前配置，所选模型应支持截图输入及结构化输出。

设备、台架背景和 AI 设置分别保存各自字段，不会提前保存或清空其他页面的待保存内容。开始任务时统一读取并校验所有页面。完整 JSON 位于“AI 设置 → 高级配置”，点击“应用完整配置”会更新所有页面。

## 已实现

- 本地 WPF 工作台、配置持久化、ADB 扫描及中性 ADB1/ADB2 绑定、本地历史。旧版 `headunitSerial` / `phoneSerial` 配置和历史快照可继续读取，新保存配置使用 `adb1Serial` / `adb2Serial`。
- PowerShell 调用 `codex exec`；stdin 提示词、图片、JSON Schema、最终结果与诊断落盘。
- AI 生成计划，按当前截图持续判断、提出动作、执行并验证；动作数、调用数、总时长和单次超时限制。
- ADB 原始截图、像素坐标点击/滑动、指定按键、设置页跳转、常用 ASCII 文本输入。
- Windows 串口原始收发/监控、配置命令别名、字面关键字等待。
- SLCAN classic CAN，以及独立 .NET Framework 4.8 / x86 兼容进程复用旧 AT 总线库的 CAN/CAN FD、LIN、FlexRay 连接与配置报文收发；CAN/CAN FD 支持基本 DBC 标量断言。
- 串口电源输出/设压，或本机 VISA Runtime 资源扫描、输出/设压、电压/电流真实回读，支持独立测量仪器绑定。
- “测试 Skills”页面手动补充项目知识、开启/关闭自动经验积累、查看/编辑/删除经验；规划与每轮判断使用最新技能快照。
- 逐轮原图、AI 输入输出、操作时间线、串口/CAN 原始记录、SHA-256 索引，以及本地静态 HTML 报告。
- 失败、停止和上次进程异常中断的报告；单个任务内暂停/继续/取消。

## 首版限制

- 旧总线库和 VISA 接入的软件路径已实现，实际硬件仍待验收；外部相机和 iOS 尚未接入。旧库的所有高级诊断、E2E、周期报文等接口尚未全部暴露给 AI。
- SLCAN 只适用于协议兼容的硬件，不是所有 CAN 设备都能直接使用。硬件时间戳尚未同步；记录以 PC 采集时间为基准。
- DBC 支持普通整数标量（Intel/Motorola、符号、缩放、偏移）。复用信号跳过并提示，浮点/扩展复用数据定义拒绝加载。
- 串口电源不包含通用数值回读；可配置查询别名和关键字。VISA 的数值回读需按仪器协议配置命令，结果与单位由实际设备协议决定。
- 暂无通话对端与音频采集，不能验证真实通话音质或媒体声音。ADB 文本暂不支持中文输入和 Shell 特殊字符。
- 当前一次任务配置一个总线通道。LIN/FlexRay 保存原始帧，不提供 `bus.expect` 信号断言；电源测量目前要求 VISA 返回单个有限数值。没有连接真实设备时使用演示模式。
- 没有 Web 后端、任务上传或报告上传；浏览器本地查看报告无需服务器。

## 测试 Skills 与经验积累

在“AI 设置 → 测试 Skills”子页输入补充资料并保存，从下一次规划/判断生效，包括运行中的后续判断。可手动修改或删除自动经验，关闭自动积累开关在下次任务生效。

基础模板为 `skills/test-analysis/SKILL.md`。首次使用复制到数据根的 `skills/test-analysis/SKILL.md`，手动补充保存在同目录 `manual.md`，自动经验保存在 `lessons.json`。已有文件不被模板覆盖，也不修改本机全局 Codex 配置。

自动经验由决策 JSON 的 `lessons` 提供，客户端仅接收本轮有效证据支持、长度受限且未重复的条目。保存适用条件、建议、任务/步骤/证据 ID 和时间；模拟演示及无效、暂停后丢弃的判定不写入经验。经验只匹配相同设备绑定与台架配置，每次最多加载最近 30 条，仍须当前证据复核。完整技能快照写入每次请求的 `ai/<requestId>/skills.json`。

设备页参考旧 PowerModule / AdditionalDevice 的“设备列表 → 刷新 → 绑定/解绑”流程。串口使用“新增通道”和行内“解绑”管理；串口电源选择已配置的通道，VISA 则可分别绑定电源和测量仪。绑定只修改待保存配置，不立即操作硬件。电源手动控制、命令别名默认折叠。

设备页的扫描、连接检查、手动命令调用不消耗 Codex 额度。ADB刷新会取消旧的自动扫描重新检测，单次枚举等待最多3秒。项目旧默认ADB会切换到AIclient私有的Google官方37.0.1，启动时核对实际后台版本来源；首次缺包时启动脚本自动下载安装，离线可用 `scripts/Prepare-Adb.ps1 -ArchivePath <官方固定版本ZIP>` 安装。扫描、截图和点击统一使用该SDK，也保留其他明确指定的自定义路径。检测到Windows ADB接口但列表持续为空时会受控重建连接；未出现接口或设备正在等待授权时不反复重启。USB识别与设备授权未完成仍需等待，3秒不是物理重插上线保证。刷新耗时和状态变化保存在数据目录的 `device-discovery` 下。手动设备调用会操作真实硬件并生成独立运行记录；“检查完成”不等于被测功能通过。VISA 需安装匹配主进程位数的厂商 Runtime；Legacy 总线需本机 .NET Framework 4.8 及对应 x86 厂商驱动。复制应用时保留 `device-host` 子目录与其全部依赖。

FlexRay 的 FIBEX 与 E-Ray/Cold 启动帧由项目配置，不沿用旧库写死的车型帧名，也不在连接后自动发送旧库的唤醒报文。

## 数据与报告文件

默认保存在 `AIclient/data`，可用 `AICLIENT_DATA_ROOT` 指定其他位置。

```text
data/settings.json
data/runs/<runId>/run.json
data/runs/<runId>/plan.json
data/runs/<runId>/events.jsonl
data/runs/<runId>/evidence.json
data/runs/<runId>/screenshots/
data/runs/<runId>/logs/
data/runs/<runId>/ai/<requestId>/
data/runs/<runId>/report.html
```

点击“打开本地报告”即可预览；复制给他人时保留整个运行目录。报告图片和日志使用相对链接，不依赖外部 CDN。配置、数据、构建产物和测试证据已排除版本控制。

## 构建与检查

在仓库根目录执行：

```powershell
dotnet build .\AIclient\AIclient.sln -c Release
.\AIclient\tests\AIclient.Checks\bin\Release\net9.0-windows\AIclient.Checks.exe .\AIclient\artifacts\checks
```

检查涵盖完整演示、PowerShell 调用契约（使用假 CLI）、提示词不被当作脚本执行、证据与哈希、报告转义、过期证据拦截、取消后的迟到动作拦截、异常恢复、超时进程收尾、CAN 与 DBC 解析。检查程序不会操作真实设备或消耗 Codex 额度。

界面自动检查：设置独立 `AICLIENT_DATA_ROOT` 后用 `AIclient.exe --smoke` 启动，程序会完成演示、保存 `ui-smoke.png` 和 `ui-smoke.json` 后退出。

真实 CLI 图片连通性检查（会使用现有 Codex 额度）：

```powershell
.\AIclient\scripts\Test-LocalCodex.ps1 -ImagePath '<已有截图绝对路径>' -OutputDirectory '.\AIclient\artifacts\codex-live'
```

此检查只辨认截图中的演示标记，不能代替真实车机与手机验收。最终软件检查与实机验证状态见 [VALIDATION.md](VALIDATION.md)。

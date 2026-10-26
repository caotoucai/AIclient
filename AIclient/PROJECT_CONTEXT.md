# AIclient 项目续接记录

最后整理：2026-10-26。此文件用于下次会话快速恢复项目背景；详细实现以源码及 [ARCHITECTURE.md](ARCHITECTURE.md) 为准。

## 2026-10-26 最新续接：用户真实拔插日志定位到旧 ADB 后台

- 用户明确复现顺序：设备配置解绑ADB1、物理拔线、重插、点可用设备刷新。本次读取默认data/device-discovery/20261026.jsonl，20:24:40掉线，此后多次手动刷新31–36ms返回空列表，20:25:21.468才列出新transport_id。Windows ADB接口LastArrivalDate=20:24:44.077，因此接口已到达至ADB列出约37.391秒；当前实测延迟在ADB服务/USB传输上线阶段，不是刷新按钮排队。
- 原后台Resource/ADB/adb.exe为29.0.2。已从Google官方下载固定37.0.1到AIclient/tools/platform-tools，SHA256=45F4D63113E895EBDE0C90F194099A4676B6AC653BD28D54314A9E022BBC1A99，adb.exe Authenticode有效、发布者Google LLC；保留旧Resource文件。新旧均协议41，单纯换exe仍连旧后台：实测新版server-status返回unknown host service，故已停止旧后台并以私有37.0.1启动，server-status实际路径核对成功。
- 新增AdbServer：37+SDK启动时核对真实后台路径，旧/异路径后台才替换；初始化复用，刷新取消不打断stop/start收尾。新增AdbUsbDiscovery：cfgmgr32读取真正Present的Android ADB接口GUID；USB接口在而列表持续空2秒才恢复，每空列表阶段最多一次、后台30秒冷却，恢复前再查列表，任何在线/离线/待授权设备已列出均不重启。未拔线/未出现接口时不恢复。解绑不停止发现或重新自动绑定。
- 路径解析优先最近项目内tools/platform-tools；裸adb和本仓库旧默认Resource路径迁移，其他显式自定义路径保留。扫描/截图/点击使用同一解析与后台管理。Prepare-Adb.ps1固定版本/哈希安装并保留NOTICE及依赖，Start-AIclient.ps1首次缺包时调用；生成SDK目录不入库。
- Release0警告0错误，64项软件检查通过（旧后台协议兼容陷阱、实际路径、受控恢复、冷却、取消收尾、USB到达/拔出等）；实际WPF新增/累计16项ADB发现检查和原31项界面检查通过，包含解绑后模拟拔插再刷新。产物artifacts/adb-usb-hotplug-checks-final和artifacts/adb-usb-hotplug-ui。
- 实机确认新版后台、Present接口检测和1080×2400截图正常。尝试pnputil重启单个ADB接口遭Windows拒绝访问，尽管其ExitCode=0，并未观察到掉线；此尝试不算驱动重启成功，更不算物理拔插验收。用户原始日志保存在artifacts/adb-usb-hotplug/user-repro-before.jsonl，新版后的物理拔插时延尚未验收。保持用户默认绑定与手机设置。


## 2026-10-26 最新续接：ADB 拔插后手动刷新仍慢的二次修正

- 用户实测反馈上一轮轮询修复仍不能解决真实拔插后点击刷新等待。上一轮仅测试在线枚举/模拟接入，不能写为物理USB重插已解决。
- 定位客户端实际等待路径：手动刷新此前复用正在等待的自动扫描，AdbDevice.ScanAsync可等待20秒；错误后另有5秒退避。已改为手动刷新抢占并取消自动扫描，连续手动点击只合并新的手动扫描。设备枚举进程及界面等待均为3秒，界面WaitAsync不再因旧任务清理或忽略取消而无限等待；超时后自动检测1秒后重试，手动不等待退避。截图/点击等设备命令仍独立25秒超时。
- 用户建议统一根目录ADB进程。新增AdbDevice.ResolveExecutable：裸adb.exe从应用目录向上寻找根adb.exe或Resource/ADB/adb.exe，优先于PATH；显式指定的路径保持。发现/检查/截图/点击共用同一路径解析。本仓库实际文件为Resource/ADB/adb.exe，版本29.0.2；此次没有升级/替换该文件，也未重启共享ADB服务器。
- 新增data/device-discovery/yyyyMMdd.jsonl记录手动刷新、状态变化、慢响应和异常的路径/时刻/耗时/设备状态，区分应用等待与ADB尚未枚举。正常未变化的快速轮询不持续刷日志。
- 实际WPF隔离自检14项ADB发现检查通过，新增覆盖旧自动扫描卡住时手动抢占、旧连接结果拒绝、忽略取消时三秒上限及超时立即手动重试。最终UI产物artifacts/adb-reconnect-ui-final。51项软件检查通过，包含项目ADB路径优先级/显式路径保留；产物artifacts/adb-reconnect-checks。
- 实机服务链测试artifacts/adb-reconnect-live：只对RFCNC0RW4VB做adb reconnect，64毫秒命令返回，814毫秒恢复在线，期间观察到空列表再出现新transport_id；重连后1080×2400截图成功，1182毫秒。此为ADB传输逻辑重连，未做物理USB拔插计时，不能宣称用户原复现场景已实证修复。保留默认绑定和手机配置。


## 2026-10-26 最新续接：ADB 新设备刷新延迟

- 用户反馈插入ADB后设备列表更新慢。实机已连接状态下三次 adb devices -l 为30/18/14毫秒；未做物理拔插计时。旧界面仅手动单次扫描，允许重复请求并发，存在旧结果覆盖风险。
- 新增 MainWindow.Adb.cs：设备页可见、未最小化且未执行任务时每秒轮询；进页立即扫描；手动刷新共享在途请求，不排队重复扫描。切换页面、开始任务、更换路径、关闭窗口时取消旧扫描，并按版本/路径/可见状态拒绝迟到结果；关闭时停止定时器。
- 列表变化才刷新，保留按序列号选中的设备；授权/离线状态显示提示，只允许在线设备绑定，已有绑定和默认配置不因扫描改变。扫描失败时清空可选项并5秒后自动重试，手动刷新可立即重试。
- Release构建0警告0错误，48项软件检查通过。实际WPF隔离自检新增10项，含定时模拟接入、选择/提示保留、待授权/离线、请求合并、离页/改路径迟到拦截及任务期间暂停；原31项界面检查也通过。产物 artifacts/adb-discovery-ui/ui-smoke.json 与 artifacts/adb-discovery-checks/checks.json。自检为受控扫描替身，不宣称已完成真实USB热插拔端到端计时。重启使用新EXE后生效。


## 2026-10-26 最新续接：热点及自动 Skills 实机复验

- 实机仍为三星 SM-G9960 / Android 14、adb1=RFCNC0RW4VB。用户解锁后复用生产 CodexBridge/TestRunner/RunDevices/Storage/Report，通过真实 Codex 完成热点开启/关闭、新密码保存后退出重入回读、原密码恢复后退出重入回读。测试前备份原配置，未覆盖默认用户设备绑定。
- 首轮 20261026_170852_5bc51e 前五步 Passed；S06 因模型上下文没有此前高级选项基线而 Inconclusive，总状态保留。恢复补验 20261026_174753_edc145 对照已核对哈希的 S01 历史基线，仅打开/返回配置页，真实模型根据新证据 Passed。最终热点关闭、Wi-Fi开启、原密码及显示状态恢复，SSID、频段、安全类型和高级选项与基线一致。
- 首轮模型返回并由 TestSkills 自动保存 4 条真实经验：直接入口返回桌面及重新定位设置、密码清空后的最小长度校验及填写后校验恢复。已核对真实 response、当前证据来源；4 条都进入后续请求，补验新任务同台架快照也加载全部4条。没有事后伪装自动生成；新增经验不含密码或固定坐标。它们按隔离验收台架指纹保存，默认界面可查看所有台架；其他台架配置不会自动匹配。
- 实测暴露多页面重复核对：开发者在运行中向基础技能补充进度保留规则，下一轮快照加载后 S01 完成。此规则为人工改进，区别于4条自动经验。后续补充 CodexBridge.previousSteps，传递此前实际结论及证据来源，避免跨步骤遗漏；当前结果仍必须新证据，过期证据拦截保留。Release构建0警告0错误，48项软件检查通过，含前序结论传递且不混入当前/未来步骤的新增检查。未声称整轮热点任务已经在该代码修复后重跑。
- 汇总 artifacts/hotspot-live-20261026/report.html、summary.json，138条证据哈希通过；完整原始两轮报告、AI快照及输入输出保留。此次是实际手机服务执行链验收，未新增 WPF 全流程操作验证。单手机未验证第二设备密码认证、共享上网或性能。

## 2026-10-26 最新续接：特殊情况 Skills 补录与自动积累纠正

- 用户强调：实机出现确认弹窗等特殊情况，应主动沉淀 Skills，下次识别后优先处理，而不是每次从头探索。
- 上轮实机验收入口自行设置 AutoLearnSkills=false，且弹窗相关真实决策返回空 lessons，未发生自动积累。本次如实保留旧配置和返回，不将事后补录写成当时自动学习。
- 已按源步骤和 8 条证据哈希复核，将蓝牙“要关闭蓝牙吗？”二次确认、Wi-Fi“正在打开…”过渡状态两条经验写入默认 data/skills/test-analysis/lessons.json，保留源 runId、stepId、evidenceIds 及原台架指纹。为正常配置也可读取这些有条件知识，在同目录 manual.md 追加来源明确的复盘补充，保留已有内容，不改设备绑定。
- 强化 CodexBridge 和模板的经验规则：看到新确认弹窗/权限提示/阻塞/过渡态应记录已观察的识别条件与处理原则；发现时不提前声称处理生效，后续新证据再补充验证结果。遇到匹配经验先核对当前页面和目标并定位按钮，不沿用历史坐标。
- artifacts 内实机验收入口改为开启经验并共享默认知识库，运行产物仍隔离。正常配置自动积累当前已为开启。快照检查成功：普通配置读取条件补充，原台架读取匹配经验。产物 artifacts/phone-live-20261026/skills-review-check.json。未再次操作手机，也未声称验证了提速或新规则下真实模型一定生成经验。

## 2026-10-26 最新续接：单手机实机蓝牙/Wi-Fi

- 用户连接的实际设备为三星 SM-G9960 / Android 14（`RFCNC0RW4VB`），不是用户原配置保存的旧 serial。本次隔离绑定为 adb1，不覆盖默认用户配置。
- 通过 artifacts 内服务验收入口复用真实 CodexBridge/TestRunner/RunDevices/Storage/Report，实际执行蓝牙开启/关闭、Wi-Fi 关闭/重新开启；四项均有新截图及系统回读支持通过。处理了三星关闭蓝牙二次确认、开关过渡画面及等待稳定。
- 首轮因验收入口 20 分钟预算在扫描阶段超时，总状态保留 Error；随后以明确的验收目标调用真实 Codex 补验扫描和恢复，通过。当前 Wi-Fi 页面 4 个完整可见附近网络，最终 bluetooth_on=0、wifi_on=1，与初始状态一致。没有新配对或新 Wi-Fi 接入操作。
- 汇总 `artifacts/phone-live-20261026/report.html`，完整两轮报告与原始输入输出在 execution/runs；56 条证据哈希核对通过。真实 Codex 每轮约 61–78 秒，影响全自动任务总时长。本次是实际设备服务链路验收，不代表已覆盖 WPF 全流程手工界面操作。
- 单手机基础功能已有实证，蓝牙双端配对/音频、互联网/吞吐及其他台架硬件仍未验收；详细边界见 VALIDATION 最新节。

## 2026-10-26 最新续接：只选下拉框与页面拆分

- 用户明确要求所有 ComboBox 不可输入文本；设备界面参考根目录 Addins 对应模块。扫描列表与波特率/总线速率均改为 SelectedItem，空列表禁用绑定，刷新不将未扫描到的旧资源填回可选列表。历史项目的特殊速率保留为可选项，避免读取旧配置时丢失。
- 主导航为测试工作台、设备配置、台架背景、AI 设置、本地运行记录。ADB 在设备页单独分组，使用只读通道显示、扫描选择、绑定/解绑；拒绝重复与离线绑定。台架背景独立编辑。
- AI 设置包含 Codex 配置、测试 Skills、高级配置三个子页；Codex 路径、PowerShell、模型及执行预算集中配置。完整 JSON 在高级配置子页，应用会同步所有页面。
- 三个设置页面分别保存本页字段，不覆盖其他页的未保存编辑；Codex 登录检查、设备扫描/手动调用不依赖其他页未保存的非法输入。运行开始仍统一读取并校验全部设置；运行中禁用配置编辑，Skills 手动知识仍可更新。
- Release 构建通过（最近增量构建 0 警告/0 错误；全量兼容进程仍有旧库已知三条警告）。47 项软件检查通过，实际 WPF 自检包含 18 项设备交互和 13 项页面/保存检查，并完成演示、配置回写与标准/最小窗口截图检查。产物：`artifacts/ui-separation-checks`、`artifacts/ui-separation-ui`。隔离数据根，无真实硬件或真实 AI 调用。

以下 2026-10-25 记录为历史；界面结构及下拉框行为以上述记录为准。

## 2026-10-25 最新续接：设备设置简化

- 按用户要求参考旧 PowerModule、AdditionalDevice：设备列表、刷新、绑定/解绑；串口选中通道后显示常用参数，“新增通道”代替空白行新增。Power 的独立测量绑定仅在 VISA 下显示。
- 总线按协议显示 CAN/FD、LIN 或 FlexRay 参数；命令模板、位时序、手动控制及命令别名默认折叠。保留原 DTO、配置格式和可调用方法。
- 绑定操作只更新待保存配置，不连接硬件；保存前提交折叠字段与可编辑下拉框，非法数值阻止保存。解绑串口停用关联电源，保留命令别名；切换协议保留高级参数。
- Release 构建通过；47 项软件检查通过，WPF 自检新增 12 项实际界面绑定/解绑、校验与协议切换检查，配置回写通过。产物为 `artifacts/simple-device-checks` 和 `artifacts/simple-device-final-ui`，后者含正常/最小窗口截图。未连接真实硬件。

## 2026-10-25 上轮续接：Skills 与设备配置

- 新增“测试 Skills”页：专用 SKILL.md、手动补充、自动经验开关、经验来源、编辑和删除。`TestSkills` 按台架指纹加载经验；规划/判断均显式注入最新资料并落盘 skills.json。决策新增 lessons；TestRunner 在有效、未取消、未因暂停丢弃的判定之后保存经验，不增加额外 AI 调用。演示不积累。
- 设备页改为可折叠分组，串口多通道、总线、Power、命令别名与高级 JSON 同步。最新界面交互见上方“设备设置简化”；保存配置与运行前均校验。
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

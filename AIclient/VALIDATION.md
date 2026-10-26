# 初版本地验证记录

日期：2026-10-24。以下区分已完成的软件验证与尚未连接的实物台架。

## 2026-10-26 用户真实拔插日志与 ADB 后台修正

- 真实复现日志：解绑后拔出/重插，多个手动扫描仅31–36ms但返回空列表。Windows接口到达20:24:44.077，ADB重新列出20:25:21.468，间隔37.391s。原始文件已保留到artifacts/adb-usb-hotplug/user-repro-before.jsonl。证明当时等待位于ADB服务枚举/USB上线阶段，不能继续把UI定时器测试当成该故障修复证据。
- 旧29.0.2后台与新37.0.1都使用协议41；新版CLI不自动替换旧后台，server-status实测unknown host service。已保留旧二进制，安装Google官方37.0.1（固定包哈希45F4D63113E895EBDE0C90F194099A4676B6AC653BD28D54314A9E022BBC1A99，签名Google LLC有效），切换实际后台后路径/版本核验成功。安装脚本在隔离目录从同包复建通过。
- 修复不只换exe：初始化核对后台真正路径；USB ADB接口Present、持续空列表才受控恢复，重启前二次检查且冷却，已列出的其他设备/待授权设备不触发恢复；取消刷新仍完成启动收尾。未检测到接口时不重启。
- Release0警告0错误，64项软件检查通过，产物artifacts/adb-usb-hotplug-checks-final；实际WPF16项ADB发现和31项原设置/设备检查通过，产物artifacts/adb-usb-hotplug-ui。包含用户解绑→模拟拔出→重插→手动刷新流程、3秒等待和迟到结果拦截，以及后台替换/USB条件/冷却/取消的服务替身验证。
- 新SDK实机枚举、原生接口Present检测及1080×2400截图正常。pnputil接口重启实际被系统拒绝访问，其ExitCode=0不代表成功，未观察到断开；该尝试不算物理或驱动重连通过。新版之后尚未完成用户物理拔插端到端验收，不宣称37秒已在实测缩短。


## 2026-10-26 ADB 重插后手动刷新二次修正

- 用户报告上次修复仍慢，不能沿用上次在线枚举测试声称物理重插已解决。此次修正手动刷新等待旧自动扫描20秒的路径：手动抢占自动扫描，重复手动点击只合并新手动请求；枚举进程及UI等待均限制3秒。超时不额外等待5秒才检测，手动刷新立即重试，旧结果仍失效。
- 所有ADB动作统一项目可执行文件解析，裸adb.exe优先项目根adb.exe/Resource/ADB/adb.exe，显式路径不改。51项软件检查通过，新增3项路径优先级检查；artifacts/adb-reconnect-checks。最终实际WPF自检artifacts/adb-reconnect-ui-final中14项发现检查全部通过，包括挂起自动扫描抢占、忽略取消的硬等待上限及超时重试，原31项设备/设置检查也通过。Release构建0警告0错误。
- 实机artifacts/adb-reconnect-live/result.json：生产AdbDevice执行项目Resource/ADB/adb.exe，单手机adb reconnect逻辑重连命令64ms，814ms恢复在线；多次真实枚举从空列表到新transport_id，重连后截图1080×2400、1182ms。没有改用户绑定、热点或Wi-Fi设置，没有物理USB拔插端到端测试。后者仍待实际复现，不能标为已验收。
- 默认数据目录device-discovery下新增按天JSONL：记录每次手动刷新、状态变化、慢扫描与错误的时间/耗时/ADB路径和列表。用于后续真实拔插区分驱动/ADB未上线与客户端请求等待。


## 2026-10-26 ADB 发现刷新延迟修复验证

- 实机当前在线设备三次原生枚举30/18/14毫秒，支持该时点ADB返回较快；未进行物理USB拔插计时，不能推出所有驱动/授权场景耗时相同。
- 设备页改为每秒自动检测、进入页立即扫描，手动刷新复用在途请求；取消与版本/路径检查阻止旧结果回写。列表不变不重建、不覆盖操作提示，新设备出现保留用户选择；离线/待授权提示更新且保存的绑定不变。
- Release构建0警告0错误；48项软件检查通过（artifacts/adb-discovery-checks）。实际WPF --smoke隔离数据根，原31项设置/设备检查及新增10项ADB发现检查通过（artifacts/adb-discovery-ui/ui-smoke.json）。定时模拟设备接入在2.5秒检查窗口内出现，验证1秒定时器驱动而非手动刷新；同时覆盖并发合并、迟到结果、路径切换、授权状态和运行暂停。扫描替身不控制真实手机，不修改用户默认配置。设备页截图已查看。


## 2026-10-26 热点开关、密码及自动经验实机复验

- 单台三星 SM-G9960 / Android 14，通过生产服务链和真实 Codex 测试。热点开启有新页面及活动 SoftApManager支持；关闭有新页面及AP停止/活动管理器清零支持。新密码保存后退出重入回读一致；原密码也按同样流程恢复。没有第二台设备，因此没有宣称验证密码认证、共享上网或性能。
- 首轮 20261026_170852_5bc51e：S01–S05 Passed，最终S06因前序基线未传给模型而Inconclusive。未改写该状态。恢复补验 20261026_174753_edc145：验收入口给定单项目标和已复核源哈希的历史基线，真实Codex逐轮只读核对，通过。最终热点关闭、Wi-Fi开启、原密码/显示状态、SSID、首选5GHz、WPA2-Personal、随机MAC、20分钟无连接关闭、隐藏网络/WLAN6及三个附加开关与初始基线一致。
- 自动经验开启，写入应用默认实际知识目录。4条由首轮真实响应产生，来源证据校验通过，随后请求均加载，新任务同台架也加载全部4条；验证脚本还核对新增经验不含原密码/临时密码。原有两条复盘经验保留，未算作本次自动新增。
- 基础技能运行中补充多阶段进度规则后，实际后续快照读取并结束重复基线导航；该规则明确算人工改进。CodexBridge新增previousSteps以传递此前结论/证据，代码修复后的Release构建0警告0错误、48项软件检查通过（artifacts/hotspot-context-checks）。上下文修复的软件合约通过不等于整轮热点实测已重跑。
- 汇总 artifacts/hotspot-live-20261026/report.html、summary.json；verify.py核对两轮共138条证据SHA-256、真实经验响应及后续技能快照。保留完整原始报告和无法判定记录，默认用户绑定不变。此次是服务执行链验收，未增加WPF全流程实机界面验收。

## 2026-10-26 实机经验补录与读取检查

- 明确纠正：上轮验收入口关闭了自动积累，相关模型响应 lessons 为空；以下两条为复盘补录，并非当时自动生成。
- 复核真实运行 20261026_154239_b8c50e 的 S02/S04 已通过，以及 E00012–E00015、E00030–E00033 共 8 条源证据文件与哈希，保存蓝牙二次确认和 Wi-Fi 过渡状态经验，含条件、建议、来源和原台架指纹。
- 默认知识目录保留原有资料并追加条件补充。实际 Snapshot 检查确认默认配置读取条件知识、源台架读取对应两条经验；默认配置 AutoLearnSkills 为 true。结果 artifacts/phone-live-20261026/skills-review-check.json。
- CodexBridge/模板强化异常经验输出规则，实机验收入口改为开启经验并共享应用知识库；Release 构建通过，0 警告/0 错误。此次未再次操纵硬件，未测量提速，也未重新发起真实模型请求验证新规则下的经验生成率。

## 2026-10-26 单手机真实蓝牙/Wi-Fi 验收

- 实机为 USB ADB 的三星 SM-G9960 / Android 14，serial `RFCNC0RW4VB`，中性编号 adb1；测试前蓝牙关闭、Wi-Fi 开启。默认用户配置中保留的旧 serial 与本次设备不同，本次使用独立验收配置，不覆盖用户设置。
- 验收入口位于 `artifacts/phone-live-20261026/runner`，引用生产工程的 CodexBridge / TestRunner / RunDevices / Storage / Report，调用真实本机 Codex；这次验证的是服务执行链路，不是新增一轮 WPF 界面交互验收。每轮附真实手机截图和通过 ADB 只读采集的 bluetooth_on、wifi_on、cmd wifi status。
- 首轮 `20261026_154239_b8c50e`：蓝牙开启/关闭、Wi-Fi 关闭/重新开启四项目标均通过。三星关闭蓝牙出现二次确认，AI 根据新截图处理后确认关闭。开启后的过渡画面未立即判通过，等待稳定截图并与系统回读核对。
- 该验收入口设置了 20 分钟预算，Wi-Fi 扫描判定期间超时；首轮总状态真实保留为 Error，不能写成整轮自动执行通过。真实 Codex 每轮约 61–78 秒，完整任务的时长需考虑观察/动作调用次数。
- 补验 `20261026_160757_ff4bf7`：入口明确“附近网络与最终恢复状态”验收目标，仍由真实 Codex 根据新截图/系统状态判定，通过。WLAN 可用网络区域 4 个完整可见条目；bluetooth_on=0、wifi_on=1，恢复初始状态。重新开启 Wi-Fi 后系统自动回连原保存网络，没有发起新连接或蓝牙配对。
- 汇总报告：`artifacts/phone-live-20261026/report.html`；结构化汇总：`summary.json`；两轮完整原始报告、截图、AI 输入输出和系统状态在 execution/runs。56 条证据的文件与 SHA-256 已核对。
- 仅验证这台手机的基础开关和附近网络显示；未验证蓝牙双端配对、媒体/通话、Wi-Fi 新网络接入、互联网访问、吞吐或长期稳定性。车机、串口、VISA、CAN/CAN FD/LIN/FlexRay 的实物验收范围不变。下方“尚未实机验证”为初版历史，Android 基础截图/点击与本节列明能力现已有本机型实证。

## 2026-10-26 下拉框与设置页面拆分验证

- Release 构建通过，最近增量构建 0 警告、0 错误；首次重编译兼容进程出现旧库已有三条警告，未修改旧库。
- 47 项软件检查全部通过：`artifacts/ui-separation-checks/checks.json`。
- WPF 实际启动并完成演示、配置回写、18 项设备交互检查与 13 项页面/保存检查：所有设备 ComboBox 禁止编辑、列表选择回写速率、历史特殊速率保留、空 COM/VISA/总线扫描禁用绑定且保留原绑定、ADB 独立绑定/解绑/重复及离线拒绝、页面归属、分别保存与未保存编辑保留、非法 AI 预算拦截，以及运行期间的配置锁定和 Skills 可编辑。
- 自检结果：`artifacts/ui-separation-ui/ui-smoke.json`。标准与最小窗口截图覆盖 ADB、串口、总线、电源、台架背景、Codex、测试 Skills 和高级配置，已查看布局；小窗口可纵向滚动访问全部内容。
- 使用独立数据根与虚构扫描结果；没有修改用户实际配置、调用真实 Codex 或连接硬件。

## 2026-10-25 设备设置简化验证

- Release 构建通过，47 项已有软件检查全部通过：`artifacts/simple-device-checks/checks.json`。
- WPF 实际启动并完成演示、配置回写及新增 12 项界面操作检查：选中串口绑定、重复 COM 拒绝、无效波特率拒绝、串口电源绑定、VISA 电源/测量仪独立绑定与解绑、LIN/FlexRay 条件参数显示、协议切换保留高级参数、总线解绑、移除串口停用关联电源、最终配置完整校验。
- 产物为 `artifacts/simple-device-final-ui/ui-smoke.json`；`device-section-*.png` 与 `device-narrow-*.png` 覆盖标准窗口和最小窗口，已检查串口、电源、总线布局。小窗口通过纵向滚动查看完整内容。
- 使用隔离数据根与虚构扫描结果，不修改用户配置，不调用真实 AI 或硬件；真实台架验收范围不变。

## 2026-10-25 Skills 与设备配置补充验证

- 软件检查增至 **47 项全部通过**。新增规划/判断技能快照、带来源的自动经验、去重、无效证据拒绝、演示隔离、下一轮技能生效、台架隔离、经验编辑删除和手动补充持久化。
- 新增配置检查覆盖 SLCAN 错用 CAN FD、重复物理串口、VISA 缺少资源，以及 Legacy/LIN 与独立测量绑定回写。
- 实际启动 .NET Framework 4.8/x86 兼容进程，验证 ping JSON 通信及未连接设备时拒绝发送；没有把此项解释为硬件总线收发通过。
- WPF --smoke 已实际运行，演示通过，设备配置回写检查通过，并输出串口、总线、Power、命令别名四个展开分组和 Skills 页截图。视觉检查后修正浅色表格的标题及下拉值对比度。
- 产物：`artifacts/skills-devices-final-checks/checks.json`、`artifacts/skills-devices-final-ui/ui-smoke.json`、`device-section-0.png` 至 `device-section-3.png`、`skills-smoke.png`。
- 主工程及兼容进程构建通过。全量重新编译兼容进程时，旧库现有源码有三条警告（成员隐藏与不可达代码），主程序无新增编译警告。
- VISA 仪器、厂商总线适配器与真实 CAN/CAN FD/LIN/FlexRay 尚未连接验收；本轮没有额外调用真实 Codex 或向真实硬件发送命令。

## 已验证

- **Release 构建**：`dotnet build AIclient/AIclient.sln -c Release`，0 错误、0 警告；不依赖额外第三方 NuGet 包。
- **自动化流程检查**：21 项全部通过。检查完整演示、CLI 调用契约、含空格路径、提示词注入隔离、证据哈希、HTML 转义、无证据/过期证据结论拦截、取消后迟到动作拦截、暂停后重新观察、超时进程收尾、异常恢复、SLCAN 与 DBC。
- **WPF 实际启动**：`--smoke` 完成 3 步演示，保存 12 张模拟截图及监控证据，输出 HTML 报告后自动退出。截图已人工式视觉检查，主界面没有内容重叠。
- **浏览器预览**：使用本机 Edge 无界面模式打开 `file://` 本地报告并截图，已检查标题、状态、范围、限制及步骤布局。无需服务器和 CDN。
- **真实 Codex 安装**：本机 `codex-cli 0.163.0`；CLI 帮助支持 `exec`、`--image`、`--output-schema`、`--output-last-message`。
- **真实 Codex 登录**：`codex login status` 返回 `Logged in using ChatGPT`。
- **真实 Codex 图片调用**：使用现有登录，通过 PowerShell、固定 Bridge、stdin 提示词、PNG 输入和 JSON Schema 发起实际请求。返回 `visibleDemoLabel: true`，说明“画面明确标注演示模拟、演示画面及无真实设备交互，并非真实蓝牙测试”。没有新增 API Key。
- **ADB 服务**：仓库自带 `Resource/ADB/adb.exe` 成功启动本地服务；设备列表为空。

## 验证产物

- `artifacts/checks-final/checks.json`：流程检查清单与状态。
- `artifacts/ui-final/ui-smoke.png`：工作台截图。
- `artifacts/ui-final/ui-smoke.json`：演示结果及报告路径。
- `artifacts/ui-final/report-preview.png`：浏览器报告截图。
- `artifacts/ui-final/runs/`：完整演示报告和 evidence。
- `artifacts/codex-live/response.json`：真实 Codex 图片识别返回。

测试产物位于本机，未纳入版本控制。检查命令见 README，可重复生成。

## 尚未实机验证

当前没有扫描到真实车机或手机，也未连接串口设备、SLCAN 硬件和电源，因此不能宣称以下实测通过：

- 真实车机与手机的蓝牙开关、发现、配对、连接、断开、重连。
- 真实 ADB 固件上的截图、点击和设置入口兼容性。
- 特定串口协议、SLCAN 硬件应答、真实车辆 DBC、串口电源的实际输出。
- 通话、真实媒体声音、音频质量与专有总线 SDK。

真实台架接入后，应先确认角色绑定、项目蓝牙名称、串口/总线协议与电源命令，再关闭演示模式执行。程序已有对应的软件执行路径和证据机制，硬件兼容性仍以实际验证为准。

## ADB1 / ADB2 中性编号变更验证

本次将设备绑定、AI 动作、取证和报告统一改为 `adb1` / `adb2`。通常将 ADB1 用于车机、ADB2 用于手机仅是弱默认约定；两台均为手机等 Android 组合不会被设备类型条件拦截。

- Release 构建：0 错误、0 警告。
- 软件检查增至 **32 项，全部通过**；结果在 `artifacts/adb-slots-checks/checks.json`。
- 新增旧设置与历史快照兼容、新字段优先、仅输出新字段、AI 中性能力描述、旧类型路由拒绝检查。
- 使用受控 ADB 进程替身模拟两台不同 serial 的 Android 手机，真实 `RunDevices` / `AdbDevice` 完成预检、双端 PNG 拉取、分别点击、重新截图与报告；两端图像均为 420×760，分别使用正确的 serial。
- 受控替身流程验证的是软件路由、证据与流程，不代表实际双手机蓝牙功能已验收。
- 界面截图与演示检查输出到 `artifacts/adb-slots-ui/`；其中 `settings-smoke.png` 为设备设置页。

# 初版本地验证记录

日期：2026-10-24。以下区分已完成的软件验证与尚未连接的实物台架。

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

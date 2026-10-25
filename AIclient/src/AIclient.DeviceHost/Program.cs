using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web.Script.Serialization;
using CAN_Library.CAN_Interface;
using CAN_Library.CAN_Driver;

internal static class Program
{
    private static readonly JavaScriptSerializer json = new JavaScriptSerializer { MaxJsonLength = 2000000 };
    private static CAN_Interface manager;
    private static CAN_Channel channel;
    private static string protocol;
    private static readonly object gate = new object();
    private static void Write(object value) { lock (gate) Console.WriteLine(json.Serialize(value)); }
    private static string S(Dictionary<string, object> d, string key, string fallback = "") { return d.ContainsKey(key) ? Convert.ToString(d[key]) : fallback; }
    private static uint U(Dictionary<string, object> d, string key) { return Convert.ToUInt32(d[key]); }
    private static void Require(bool value) { if (!value) throw new InvalidOperationException("旧设备库返回失败，请检查驱动、设备映射和配置；详见 legacy-driver 日志。"); }
    [STAThread]
    private static void Main()
    {
        Console.InputEncoding = new System.Text.UTF8Encoding(false);
        Console.OutputEncoding = new System.Text.UTF8Encoding(false);
        // 厂商库可能写控制台，协议只使用保存下来的输出流。
        var output = Console.Out;
        Console.SetOut(new ProtocolWriter(output));
        try
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                Dictionary<string, object> request = null;
                try
                {
                    request = json.Deserialize<Dictionary<string, object>>(line);
                    var op = S(request, "operation");
                    object result;
                    if (op == "ping") result = "AIclient.DeviceHost/net48/x86";
                    else if (op == "scan") { EnsureManager(); result = manager.Scan_Channel_List().Select(c => c.FullName).ToArray(); }
                    else if (op == "open")
                    {
                        EnsureManager(); protocol = S(request, "protocol");
                        channel = manager.Scan_Channel_List().SingleOrDefault(c => c.FullName == S(request, "device"));
                        if (channel == null) throw new InvalidOperationException("未找到绑定的旧总线设备：" + S(request, "device"));
                        channel.setBenchName(S(request, "nodeName", "bench"));
                        var database = S(request, "database");
                        if (protocol == "LIN")
                        {
                            channel.LIN_Parameter.Baudrate = (LINBaudRate)Enum.Parse(typeof(LINBaudRate), S(request, "linBaud", "Baud_19200"));
                            channel.LIN_Parameter.LINversion = (LIN_Verison)Enum.Parse(typeof(LIN_Verison), S(request, "linVersion", "LIN_2_1"));
                            channel.ChannelLINmsgNotify += m => Write(new { kind = "frame", protocol, at = DateTimeOffset.UtcNow.ToString("O"), id = m.ID, data = BitConverter.ToString(m.Data ?? new byte[0]).Replace("-", ""), direction = m.TxRx.ToString() });
                            if (database.Length > 0) Require(channel.Import_LDF(database));
                            // 导入 LDF 之后再应用界面上的速率/版本，防止文件静默覆盖用户选择。
                            channel.LIN_Parameter.Baudrate = (LINBaudRate)Enum.Parse(typeof(LINBaudRate), S(request, "linBaud", "Baud_19200"));
                            channel.LIN_Parameter.LINversion = (LIN_Verison)Enum.Parse(typeof(LIN_Verison), S(request, "linVersion", "LIN_2_1"));
                            Require(channel.Connect_Lin(S(request, "linMaster") == "True"));
                        }
                        else if (protocol == "FlexRay")
                        {
                            channel.ChannelFlexRaymsgNotify += m => Write(new { kind = "frame", protocol, at = DateTimeOffset.UtcNow.ToString("O"), id = m.ID, name = m.Name, data = BitConverter.ToString(m.Data ?? new byte[0]).Replace("-", ""), direction = m.TxRx.ToString() });
                            if (string.IsNullOrWhiteSpace(database)) throw new InvalidOperationException("FlexRay 需要 FIBEX 数据库。");
                            AIclient.DeviceHost.LegacyOptions.ErayFrame = S(request, "erayFrame");
                            AIclient.DeviceHost.LegacyOptions.ColdFrame = S(request, "coldFrame");
                            if (string.IsNullOrWhiteSpace(AIclient.DeviceHost.LegacyOptions.ErayFrame) || string.IsNullOrWhiteSpace(AIclient.DeviceHost.LegacyOptions.ColdFrame)) throw new InvalidOperationException("请配置本项目的 FlexRay E-Ray / Cold 启动帧。");
                            Require(channel.Connect_FlexRay(database));
                        }
                        else
                        {
                            channel.CAN_Parameter.BaudRate = Rate(U(request, "bitRate"));
                            channel.CAN_Parameter.Arbitration_rate = Rate(U(request, "bitRate"));
                            channel.CAN_Parameter.Data_rate = Rate(U(request, "dataRate"));
                            if (request.ContainsKey("timing"))
                            {
                                var timing = (Dictionary<string, object>)request["timing"];
                                if (Convert.ToBoolean(timing["enabled"]))
                                {
                                    var clock = Convert.ToInt32(timing["clockMHz"]);
                                    if (!Enum.IsDefined(typeof(ClockFrequency), clock)) throw new InvalidOperationException("不支持的 CAN 时钟频率。");
                                    channel.CAN_Parameter.F_clock = (ClockFrequency)clock;
                                    channel.CAN_Parameter.Abr_brp = Convert.ToByte(timing["arbitrationBrp"]);
                                    channel.CAN_Parameter.Abr_tseg1 = Convert.ToByte(timing["arbitrationTseg1"]);
                                    channel.CAN_Parameter.Abr_tseg2 = Convert.ToByte(timing["arbitrationTseg2"]);
                                    channel.CAN_Parameter.Abr_sjw = Convert.ToByte(timing["arbitrationSjw"]);
                                    channel.CAN_Parameter.Data_brp = Convert.ToByte(timing["dataBrp"]);
                                    channel.CAN_Parameter.Data_tseg1 = Convert.ToByte(timing["dataTseg1"]);
                                    channel.CAN_Parameter.Data_tseg2 = Convert.ToByte(timing["dataTseg2"]);
                                    channel.CAN_Parameter.Data_sjw = Convert.ToByte(timing["dataSjw"]);
                                }
                            }
                            channel.ChannelMessageNotify += m => Write(new { kind = "frame", protocol, at = DateTimeOffset.UtcNow.ToString("O"), id = m.ID, extended = m.Type.ToString().Contains("Extended"), data = BitConverter.ToString(m.Data ?? new byte[0]).Replace("-", ""), direction = m.TxRx.ToString() });
                            Require(channel.Connect(protocol == "CAN FD"));
                            if (database.Length > 0) Require(channel.Import_DBC_WithoutSending(database));
                        }
                        result = "已连接：" + channel.FullName;
                    }
                    else if (op == "close") { Close(); result = "已关闭"; }
                    else
                    {
                        if (channel == null) throw new InvalidOperationException("总线尚未打开。");
                        if (op == "send")
                        {
                            var data = Hex(S(request, "data")); var id = U(request, "id");
                            if (protocol == "LIN") Require(channel.Send_LIN_Message(id, data, (uint)data.Length));
                            else if (protocol == "FlexRay") Require(channel.Send_FlexRay_Message(S(request, "frame"), data, (uint)data.Length));
                            else if (protocol == "CAN FD") Require(channel.Send_CANFD_message(id, (uint)data.Length, data));
                            else Require(channel.Send_CAN_message(id, (uint)data.Length, data));
                            result = "配置报文已发送；功能效果需新证据验证。";
                        }
                        else if (op == "getSignal") result = protocol == "LIN" ? channel.Get_LIN_Signal(S(request, "signal")) : protocol == "FlexRay" ? channel.Get_FlexRay_Signal(S(request, "signal")) : channel.Get_signal(S(request, "signal"));
                        else throw new InvalidOperationException("不支持的兼容方法：" + op);
                    }
                    Write(new { kind = "response", id = S(request, "requestId"), success = true, result });
                }
                catch (Exception ex) { Write(new { kind = "response", id = request == null ? "" : S(request, "requestId"), success = false, error = ex.Message }); }
            }
        }
        finally { Close(); }
    }
    private static void EnsureManager() { if (manager == null) manager = new CAN_Interface(); }
    private static void Close()
    {
        if (channel != null) { if (protocol == "LIN") channel.Disconnect_Lin(); else if (protocol == "FlexRay") channel.DisConnect_FlexRay(); else channel.DisConnect(); channel = null; }
        if (manager != null) { manager.Dispose(); manager = null; }
    }
    private static CanBaudRate Rate(uint value)
    {
        var rates = new Dictionary<uint, string> { {33333,"Baud_33Kbit"},{83333,"Baud_83Kbit"},{100000,"Baud_100Kbit"},{125000,"Baud_125Kbit"},{250000,"Baud_250Kbit"},{500000,"Baud_500Kbit"},{1000000,"Baud_1Mbit"},{2000000,"Baud_2Mbit"},{4000000,"Baud_4Mbit"},{5000000,"Baud_5Mbit"} };
        if (!rates.ContainsKey(value)) throw new InvalidOperationException("旧总线库不支持此波特率。");
        return (CanBaudRate)Enum.Parse(typeof(CanBaudRate), rates[value]);
    }
    private static byte[] Hex(string value) { value = value.Replace(" ", ""); if (value.Length % 2 != 0) throw new InvalidDataException("十六进制长度无效。"); return Enumerable.Range(0, value.Length / 2).Select(i => Convert.ToByte(value.Substring(i * 2, 2), 16)).ToArray(); }
    private sealed class ProtocolWriter : TextWriter
    {
        private readonly TextWriter output;
        public ProtocolWriter(TextWriter output) { this.output = output; }
        public override System.Text.Encoding Encoding { get { return System.Text.Encoding.UTF8; } }
        public override void WriteLine(string value) { if (value != null && value.StartsWith("{\"kind\":")) { output.WriteLine(value); output.Flush(); } else Console.Error.WriteLine(value); }
    }
}

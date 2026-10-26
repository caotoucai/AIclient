using System.Windows;
using System.Windows.Controls;
using AIclient.Models;

namespace AIclient;

public partial class DeviceSettingsView
{
    // Invoked only by --smoke, with isolated settings and fake scan results. No hardware calls.
    internal string[] CheckBindingWorkflow(AppSettings sample)
    {
        var passed = new List<string>();
        void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidDataException("设备界面自检失败：" + name);
            passed.Add(name);
        }
        static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        AppSettings Snapshot() { var result = new AppSettings(); ReadInto(result); return result; }
        try
        {
            Load(sample); ShowSection(0);
            ShowSerial(["COM17", "COM18"]);
            SerialGrid.SelectedIndex = 0;
            SerialBaudBox.SelectedItem = 57600;
            SerialAvailableBox.SelectedItem = "COM17"; Click(BindSerialButton);
            var saved = Snapshot();
            Check(saved.SerialPorts[0] is { Port: "COM17", BaudRate: 57600, Enabled: true } && saved.SerialPorts[1].Port == "COM4", "串口仅绑定选中通道并保留参数");
            SerialGrid.SelectedIndex = 1; SerialAvailableBox.SelectedItem = "COM17"; Click(BindSerialButton);
            Check(Snapshot().SerialPorts[1].Port == "COM4", "重复串口绑定被拒绝");
            Check(Controls(this).OfType<ComboBox>().All(c => !c.IsEditable), "全部设备下拉框禁止自由文本输入");
            SerialGrid.SelectedIndex = 0;
            SerialBaudBox.SelectedItem = 38400;
            Check(Snapshot().SerialPorts[0].BaudRate == 38400, "波特率选择直接写入配置");
            ShowSerial([]);
            Check(SerialAvailableBox.SelectedItem == null && !BindSerialButton.IsEnabled && Snapshot().SerialPorts[0].Port == "COM17", "空串口扫描清空选项并禁用绑定且保留已有绑定");

            ShowSection(2); PowerTransportBox.SelectedValue = "serial";
            PowerAvailableBox.SelectedItem = "serial1"; Click(BindPowerButton);
            Check(Snapshot().Power is { Channel: "serial1", Enabled: true }, "电源可绑定串口通道");
            PowerTransportBox.SelectedValue = "visa";
            ShowVisa(["USB::supply::INSTR", "USB::meter::INSTR"]);
            PowerAvailableBox.SelectedItem = "USB::supply::INSTR"; Click(BindPowerButton);
            PowerAvailableBox.SelectedItem = "USB::meter::INSTR"; Click(BindMeterButton);
            saved = Snapshot();
            Check(saved.Power is { Resource: "USB::supply::INSTR", MeasurementResource: "USB::meter::INSTR", Enabled: true }
                && saved.Power.VoltageCommand == sample.Power.VoltageCommand, "电源与测量仪独立绑定并保留命令模板");
            UnbindPowerClick(new Button { Tag = "meter" }, new());
            saved = Snapshot();
            Check(saved.Power.MeasurementResource == "" && saved.Power.Resource == "USB::supply::INSTR", "解绑测量仪保留电源");
            ShowVisa([]);
            Check(PowerAvailableBox.SelectedItem == null && !BindPowerButton.IsEnabled && !BindMeterButton.IsEnabled
                && Snapshot().Power.Resource == "USB::supply::INSTR", "空 VISA 扫描禁用绑定并保留已绑定资源");

            ShowSection(1); AdapterBox.SelectedValue = "Legacy";
            ShowBus(["fixture-channel"]); BusAvailableBox.SelectedItem = "fixture-channel"; Click(BindBusButton);
            ProtocolBox.SelectedItem = "LIN";
            Check(LinOptions.Visibility == Visibility.Visible && CanOptions.Visibility == Visibility.Collapsed && FlexRayOptions.Visibility == Visibility.Collapsed, "LIN 只显示对应参数");
            ProtocolBox.SelectedItem = "FlexRay";
            Check(FlexRayOptions.Visibility == Visibility.Visible && LinOptions.Visibility == Visibility.Collapsed, "FlexRay 只显示对应参数");
            ProtocolBox.SelectedItem = "CAN FD";
            saved = Snapshot();
            Check(FdOptions.Visibility == Visibility.Visible && saved.Bus is { Enabled: true, DeviceName: "fixture-channel" }
                && saved.Bus.DataRate == sample.Bus.DataRate && saved.Bus.Timing.ArbitrationTseg1 == sample.Bus.Timing.ArbitrationTseg1, "切换协议保留高级参数");
            CanBaudBox.SelectedItem = 250000; FdBaudBox.SelectedItem = 4000000;
            saved = Snapshot();
            Check(saved.Bus.BitRate == 250000 && saved.Bus.DataRate == 4000000, "CAN 与 FD 速率使用选择项保存");
            ShowBus([]);
            Check(BusAvailableBox.SelectedItem == null && !BindBusButton.IsEnabled && Snapshot().Bus.DeviceName == "fixture-channel", "空总线扫描禁用绑定并保留已有通道");
            UnbindBusClick(new Button(), new());
            Check(!Snapshot().Bus.Enabled, "解绑总线停用通道");
            PowerTransportBox.SelectedValue = "serial";
            UnbindSerialClick(new Button { DataContext = ports[0] }, new());
            saved = Snapshot();
            Check(saved.SerialPorts.Count == 1 && !saved.Power.Enabled && saved.Commands.Single().Alias == sample.Commands.Single().Alias, "移除串口停用关联电源并保留命令");
            saved.Validate();
            Check(true, "操作后的配置通过完整校验");
            var custom = Json.Read<AppSettings>(Json.Serialize(sample));
            custom.SerialPorts[0].BaudRate = 76800; custom.Bus.BitRate = 333333; custom.Bus.DataRate = 3000000;
            Load(custom);
            Check((int?)SerialBaudBox.SelectedItem == 76800 && (int?)CanBaudBox.SelectedItem == 333333 && (int?)FdBaudBox.SelectedItem == 3000000
                && Snapshot().SerialPorts[0].BaudRate == 76800, "历史项目速率保留为只选选项且回写不丢失");
        }
        finally { Load(sample); ShowSerial([]); ShowVisa([]); ShowBus([]); Message(""); }
        return passed.ToArray();
    }
}

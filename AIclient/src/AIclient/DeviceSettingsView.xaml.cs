using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AIclient.Models;
using Microsoft.Win32;

namespace AIclient;

public partial class DeviceSettingsView : UserControl
{
    public static IValueConverter EscapedNewLine { get; } = new NewLineConverter();
    private ObservableCollection<SerialConfig> ports = [];
    private ObservableCollection<DeviceCommand> commands = [];
    private string[] serialDevices = [], visaDevices = [], busDevices = [];
    private bool loading = true;
    private AppSettings Draft => (AppSettings)DataContext;
    private bool Visa => (string?)PowerTransportBox.SelectedValue == "visa";
    private bool Legacy => (string?)AdapterBox.SelectedValue == "Legacy";
    public event Action<string>? Requested;

    public DeviceSettingsView()
    {
        InitializeComponent();
        AdapterBox.ItemsSource = new[] { new Choice("串口 CAN", "SLCAN"), new Choice("厂商设备", "Legacy") };
        ProtocolBox.ItemsSource = new[] { "CAN", "CAN FD", "LIN", "FlexRay" };
        PowerTransportBox.ItemsSource = new[] { new Choice("串口设备", "serial"), new Choice("VISA 仪器", "visa") };
        SerialBaudBox.ItemsSource = new[] { 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };
        SerialDataBox.ItemsSource = new[] { 5, 6, 7, 8 };
        SerialParityBox.ItemsSource = new[] { "None", "Odd", "Even" };
        SerialStopBox.ItemsSource = new[] { 1, 2 };
        CommandTransportColumn.ItemsSource = new[] { "serial", "bus" };
        CanBaudBox.ItemsSource = new[] { 10000, 20000, 50000, 100000, 125000, 250000, 500000, 800000, 1000000 };
        FdBaudBox.ItemsSource = new[] { 500000, 1000000, 2000000, 4000000, 5000000, 8000000 };
        LinBaudBox.ItemsSource = new[] { "Baud_2400", "Baud_9600", "Baud_10400", "Baud_10417", "Baud_19200", "Baud_20000" };
        LinVersionBox.ItemsSource = new[] { "LIN_1_3", "LIN_2_0", "LIN_2_1", "J2602_0_0", "J2602_1_2", "J2602_1_1" };
    }

    public void Load(AppSettings settings)
    {
        loading = true;
        var draft = Json.Read<AppSettings>(Json.Serialize(settings));
        DataContext = draft;
        ports = new(draft.SerialPorts); commands = new(draft.Commands);
        SerialGrid.ItemsSource = ports; CommandsGrid.ItemsSource = commands;
        loading = false;
        RefreshPower(); RefreshBus();
        SerialGrid.SelectedIndex = ports.Count > 0 ? 0 : -1;
    }

    public void ReadInto(AppSettings target)
    {
        if (!CommandsGrid.CommitEdit(DataGridEditingUnit.Cell, true) || !CommandsGrid.CommitEdit(DataGridEditingUnit.Row, true))
            throw new InvalidDataException("请修正命令表格中的无效参数。");
        CommitBindings();
        Draft.SerialPorts = ports.ToList(); Draft.Commands = commands.ToList();
        var detached = Json.Read<AppSettings>(Json.Serialize(Draft));
        target.SerialPorts = detached.SerialPorts; target.Commands = detached.Commands; target.Bus = detached.Bus; target.Power = detached.Power;
    }

    // Include collapsed advanced fields and the selected serial editor, not just visible controls.
    private static IEnumerable<DependencyObject> Controls(DependencyObject root)
    {
        var pending = new Stack<DependencyObject>(); var seen = new HashSet<DependencyObject>(); pending.Push(root);
        while (pending.TryPop(out var node))
        {
            if (!seen.Add(node)) continue;
            yield return node;
            foreach (var child in LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>()) pending.Push(child);
            if (node is Visual)
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++) pending.Push(VisualTreeHelper.GetChild(node, i));
        }
    }
    private void CommitBindings()
    {
        var controls = Controls(this).ToArray();
        foreach (var node in controls)
        {
            if (node is TextBox box) box.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            if (node is ComboBox combo)
            {
                combo.GetBindingExpression(ComboBox.TextProperty)?.UpdateSource();
                combo.GetBindingExpression(ComboBox.SelectedItemProperty)?.UpdateSource();
                combo.GetBindingExpression(ComboBox.SelectedValueProperty)?.UpdateSource();
            }
        }
        if (controls.Any(Validation.GetHasError)) throw new InvalidDataException("设备参数存在无效数值，请修正后保存。");
    }
    private bool TryCommit()
    {
        try { CommitBindings(); return true; }
        catch (InvalidDataException ex) { Message(ex.Message); return false; }
    }

    private void SerialSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source != SerialGrid) return;
        // Keep invalid edits visible until corrected instead of discarding them on selection change.
        if (!loading && SerialEditor.DataContext is SerialConfig previous && ports.Contains(previous) && !TryCommit())
        {
            loading = true; SerialGrid.SelectedItem = previous; loading = false; return;
        }
        SerialEditor.DataContext = SerialGrid.SelectedItem;
        SerialEditor.Visibility = SerialGrid.SelectedItem == null ? Visibility.Collapsed : Visibility.Visible;
    }
    private SerialConfig NewSerial()
    {
        var index = 1;
        while (ports.Any(p => p.Name.Equals($"serial{index}", StringComparison.OrdinalIgnoreCase))) index++;
        var port = new SerialConfig { Name = $"serial{index}" }; ports.Add(port); SerialGrid.SelectedItem = port;
        return port;
    }
    private void AddSerialClick(object sender, RoutedEventArgs e)
    {
        if (!TryCommit()) return;
        NewSerial(); RefreshPower(); Message("已新增通道，选择串口后点击绑定通道。");
    }
    private static bool IsCom(string value) => System.Text.RegularExpressions.Regex.IsMatch(value, "^COM[1-9][0-9]*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    private void BindSerialClick(object sender, RoutedEventArgs e)
    {
        if (!TryCommit()) return;
        var value = SerialAvailableBox.Text.Trim().ToUpperInvariant();
        if (!IsCom(value)) { Message("请先刷新并选择串口，或输入 COM 口名称。"); return; }
        var selected = SerialGrid.SelectedItem as SerialConfig;
        if (ports.Any(p => p != selected && p.Enabled && p.Port.Equals(value, StringComparison.OrdinalIgnoreCase))) { Message("该串口已绑定其他启用的通道。"); return; }
        selected ??= NewSerial(); selected.Port = value; selected.Enabled = true;
        SerialEditor.DataContext = null; SerialEditor.DataContext = selected;
        SerialGrid.Items.Refresh(); RefreshPower(); Message($"{selected.Name} 已绑定 {value}，保存设置后生效。");
    }
    private void UnbindSerialClick(object sender, RoutedEventArgs e)
    {
        if (!TryCommit() || ((Button)sender).DataContext is not SerialConfig port) return;
        ports.Remove(port);
        if (Draft.Power.Transport == "serial" && Draft.Power.Channel == port.Name) Draft.Power.Enabled = false;
        RefreshPower(); Message($"已移除通道 {port.Name}，保存设置后生效。");
    }
    private void SerialDetailsChanged(object sender, RoutedEventArgs e)
    {
        if (loading) return;
        SerialGrid.Items.Refresh(); RefreshPower();
    }

    private void PowerTransportChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loading || e.Source != PowerTransportBox) return;
        PowerTransportBox.GetBindingExpression(ComboBox.SelectedValueProperty)?.UpdateSource();
        RefreshPower();
    }
    private void RefreshPower()
    {
        if (DataContext is not AppSettings) return;
        var power = Draft.Power;
        var channel = ports.FirstOrDefault(p => p.Name == power.Channel);
        var supply = Visa ? power.Resource : channel == null ? "" : $"{channel.Name} / {channel.Port}";
        var rows = new List<BindingRow> { new("supply", "电源", string.IsNullOrWhiteSpace(supply) ? "未绑定" : supply) };
        if (Visa) rows.Add(new("meter", "测量仪", string.IsNullOrWhiteSpace(power.MeasurementResource) ? "跟随电源" : power.MeasurementResource));
        PowerBindingsGrid.ItemsSource = rows;
        BindMeterButton.Visibility = Visa ? Visibility.Visible : Visibility.Collapsed;
        PowerEnabledToggle.GetBindingExpression(CheckBox.IsCheckedProperty)?.UpdateTarget();
        if (Visa) SetAvailable(PowerAvailableBox, visaDevices, power.Resource);
        else SetAvailable(PowerAvailableBox, ports.Select(p => p.Name).ToArray(), power.Channel);
    }
    private void ScanPowerClick(object sender, RoutedEventArgs e) => Requested?.Invoke(Visa ? "scan-visa" : "scan-serial");
    private void BindPowerClick(object sender, RoutedEventArgs e)
    {
        if (!TryCommit()) return;
        var value = PowerAvailableBox.Text.Trim(); var meter = (string)((Button)sender).Tag == "meter";
        if (value.Length == 0) { Message("请先选择设备。"); return; }
        if (Visa)
        {
            if (meter) Draft.Power.MeasurementResource = value;
            else { Draft.Power.Resource = value; Draft.Power.Enabled = true; }
        }
        else
        {
            var channel = ports.FirstOrDefault(p => p.Name == value);
            if (channel == null || meter) { Message("请先在串口设置中绑定通道，再选择通道名称。"); return; }
            Draft.Power.Channel = channel.Name; Draft.Power.Enabled = true; channel.Enabled = true;
            SerialEditor.DataContext = null; SerialEditor.DataContext = SerialGrid.SelectedItem;
        }
        RefreshPower(); Message(meter ? "已绑定测量仪，保存设置后生效。" : "已绑定电源，保存设置后生效。");
    }
    private void UnbindPowerClick(object sender, RoutedEventArgs e)
    {
        if (!TryCommit()) return;
        if ((string)((Button)sender).Tag == "meter") Draft.Power.MeasurementResource = "";
        else { Draft.Power.Enabled = false; if (Visa) Draft.Power.Resource = ""; else Draft.Power.Channel = ""; }
        RefreshPower(); Message("已解绑，保存设置后生效。");
    }

    private void BusTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loading || e.Source != AdapterBox) return;
        AdapterBox.GetBindingExpression(ComboBox.SelectedValueProperty)?.UpdateSource();
        if (!Legacy) ProtocolBox.SelectedItem = "CAN";
        RefreshBus();
    }
    private void ProtocolChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loading || e.Source != ProtocolBox) return;
        ProtocolBox.GetBindingExpression(ComboBox.SelectedItemProperty)?.UpdateSource(); RefreshBus();
    }
    private void RefreshBus()
    {
        if (DataContext is not AppSettings) return;
        var protocol = ProtocolBox.SelectedItem as string ?? Draft.Bus.Protocol;
        ProtocolBox.IsEnabled = Legacy;
        CanOptions.Visibility = protocol is "CAN" or "CAN FD" ? Visibility.Visible : Visibility.Collapsed;
        FdOptions.Visibility = protocol == "CAN FD" ? Visibility.Visible : Visibility.Collapsed;
        LinOptions.Visibility = protocol == "LIN" ? Visibility.Visible : Visibility.Collapsed;
        FlexRayOptions.Visibility = protocol == "FlexRay" ? Visibility.Visible : Visibility.Collapsed;
        TimingOptions.Visibility = Legacy && (protocol is "CAN" or "CAN FD") ? Visibility.Visible : Visibility.Collapsed;
        SlcanOptions.Visibility = Legacy ? Visibility.Collapsed : Visibility.Visible;
        DatabaseLabel.Text = protocol == "LIN" ? "LDF 数据库" : protocol == "FlexRay" ? "FIBEX 数据库" : "DBC 数据库";
        var device = Legacy ? Draft.Bus.DeviceName : Draft.Bus.Serial.Port;
        BusBindingsGrid.ItemsSource = new[] { new BindingRow("bus", protocol, Draft.Bus.Enabled && device.Length > 0 ? device : "未绑定") };
        BusEnabledToggle.GetBindingExpression(CheckBox.IsCheckedProperty)?.UpdateTarget();
        SetAvailable(BusAvailableBox, Legacy ? busDevices : serialDevices, device);
    }
    private void ScanBusClick(object sender, RoutedEventArgs e) => Requested?.Invoke(Legacy ? "scan-bus" : "scan-serial");
    private void BindBusClick(object sender, RoutedEventArgs e)
    {
        if (!TryCommit()) return;
        var value = BusAvailableBox.Text.Trim();
        if (value.Length == 0 || (!Legacy && !IsCom(value))) { Message("请刷新并选择设备，串口 CAN 也可直接输入 COM 口。"); return; }
        if (Legacy) Draft.Bus.DeviceName = value; else Draft.Bus.Serial.Port = value.ToUpperInvariant();
        Draft.Bus.Enabled = true; RefreshBus(); Message("已绑定总线通道，保存设置后生效。");
    }
    private void UnbindBusClick(object sender, RoutedEventArgs e)
    {
        if (!TryCommit()) return;
        Draft.Bus.Enabled = false; if (Legacy) Draft.Bus.DeviceName = "";
        RefreshBus(); Message("已解绑总线通道，保存设置后生效。");
    }
    private void BrowseDatabaseClick(object sender, RoutedEventArgs e)
    {
        var protocol = ProtocolBox.SelectedItem as string;
        var filter = protocol == "LIN" ? "LDF 数据库|*.ldf" : protocol == "FlexRay" ? "FIBEX 数据库|*.xml;*.fibex" : "DBC 数据库|*.dbc";
        var dialog = new OpenFileDialog { Filter = filter + "|所有文件|*.*", CheckFileExists = true };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true) DatabaseBox.Text = dialog.FileName;
    }
    private static void SetAvailable(ComboBox box, string[] values, string fallback)
    {
        var previous = box.Text;
        box.ItemsSource = values.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        box.Text = values.Contains(previous, StringComparer.OrdinalIgnoreCase) ? previous
            : values.Contains(fallback, StringComparer.OrdinalIgnoreCase) ? fallback : values.FirstOrDefault() ?? fallback;
    }
    public void ShowSerial(string[] values)
    {
        serialDevices = values; SetAvailable(SerialAvailableBox, values, SerialAvailableBox.Text);
        if (!Legacy) RefreshBus(); if (!Visa) RefreshPower();
    }
    public void ShowVisa(string[] values) { visaDevices = values; if (Visa) RefreshPower(); }
    public void ShowBus(string[] values) { busDevices = values; if (Legacy) RefreshBus(); }
    private void ActionClick(object sender, RoutedEventArgs e) => Requested?.Invoke((string)((Button)sender).Tag);
    public DeviceCommand? SelectedCommand => CommandsGrid.SelectedItem as DeviceCommand;
    public double ManualVoltage => double.Parse(ManualVoltageBox.Text, CultureInfo.InvariantCulture);
    public void Message(string text) => DeviceMessage.Text = text;
    public void ShowSection(int index)
    {
        var sections = new[] { SerialExpander, BusExpander, PowerExpander, CommandsExpander };
        for (var i = 0; i < sections.Length; i++) sections[i].IsExpanded = i == index;
        UpdateLayout(); sections[index].BringIntoView();
    }
    private sealed record Choice(string Label, string Value) { public override string ToString() => Label; }
    private sealed record BindingRow(string Key, string Purpose, string Device);
    private sealed class NewLineConverter : IValueConverter
    {
        public object Convert(object value, Type target, object parameter, CultureInfo culture) => ((string)value).Replace("\r", "\\r").Replace("\n", "\\n");
        public object ConvertBack(object value, Type target, object parameter, CultureInfo culture) => ((string)value).Replace("\\r", "\r").Replace("\\n", "\n");
    }
}

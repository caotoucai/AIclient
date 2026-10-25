using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AIclient.Models;

namespace AIclient;

public partial class DeviceSettingsView : UserControl
{
    public static IValueConverter EscapedNewLine { get; } = new NewLineConverter();
    private ObservableCollection<SerialConfig> ports = [];
    private ObservableCollection<DeviceCommand> commands = [];
    public event Action<string>? Requested;
    public DeviceSettingsView()
    {
        InitializeComponent();
        AdapterBox.ItemsSource = new[] { "SLCAN", "Legacy" }; ProtocolBox.ItemsSource = new[] { "CAN", "CAN FD", "LIN", "FlexRay" };
        PowerTransportBox.ItemsSource = new[] { "serial", "visa" }; ParityColumn.ItemsSource = new[] { "None", "Odd", "Even" };
        CommandTransportColumn.ItemsSource = new[] { "serial", "bus" };
        LinBaudBox.ItemsSource = new[] { "Baud_2400", "Baud_9600", "Baud_10400", "Baud_10417", "Baud_19200", "Baud_20000" };
        LinVersionBox.ItemsSource = new[] { "LIN_1_3", "LIN_2_0", "LIN_2_1", "J2602_0_0", "J2602_1_2", "J2602_1_1" };
    }
    public void Load(AppSettings settings)
    {
        var draft = Json.Read<AppSettings>(Json.Serialize(settings)); DataContext = draft;
        ports = new(draft.SerialPorts); commands = new(draft.Commands); SerialGrid.ItemsSource = ports; CommandsGrid.ItemsSource = commands;
    }
    public void ReadInto(AppSettings target)
    {
        if (!SerialGrid.CommitEdit(DataGridEditingUnit.Cell, true) || !SerialGrid.CommitEdit(DataGridEditingUnit.Row, true) || !CommandsGrid.CommitEdit(DataGridEditingUnit.Cell, true) || !CommandsGrid.CommitEdit(DataGridEditingUnit.Row, true)) throw new InvalidDataException("请修正表格中的无效参数。");
        CommitBindings(this);
        if (HasErrors(this)) throw new InvalidDataException("设备参数存在无效数值，请修正后保存。");
        var draft = (AppSettings)DataContext;
        draft.SerialPorts = ports.ToList(); draft.Commands = commands.ToList();
        var detached = Json.Read<AppSettings>(Json.Serialize(draft));
        target.SerialPorts = detached.SerialPorts; target.Commands = detached.Commands; target.Bus = detached.Bus; target.Power = detached.Power;
    }
    private static void CommitBindings(DependencyObject root)
    {
        if (root is TextBox box) box.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) CommitBindings(VisualTreeHelper.GetChild(root, i));
    }
    private static bool HasErrors(DependencyObject root) => Validation.GetHasError(root) || Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(root)).Any(i => HasErrors(VisualTreeHelper.GetChild(root, i)));
    private void ActionClick(object sender, RoutedEventArgs e) => Requested?.Invoke((string)((Button)sender).Tag);
    public DeviceCommand? SelectedCommand => CommandsGrid.SelectedItem as DeviceCommand;
    public double ManualVoltage => double.Parse(ManualVoltageBox.Text, CultureInfo.InvariantCulture);
    public void Message(string text) => DeviceMessage.Text = text;
    public void ShowVisa(string[] values) { PowerResourceBox.ItemsSource = values; MeasurementResourceBox.ItemsSource = values; }
    public void ShowBus(string[] values) => LegacyDeviceBox.ItemsSource = values;
    public void ShowSection(int index)
    {
        var sections = new[] { SerialExpander, BusExpander, PowerExpander, CommandsExpander };
        for (var i = 0; i < sections.Length; i++) sections[i].IsExpanded = i == index;
        UpdateLayout(); sections[index].BringIntoView();
    }
    private sealed class NewLineConverter : IValueConverter
    {
        public object Convert(object value, Type target, object parameter, CultureInfo culture) => ((string)value).Replace("\r", "\\r").Replace("\n", "\\n");
        public object ConvertBack(object value, Type target, object parameter, CultureInfo culture) => ((string)value).Replace("\\r", "\r").Replace("\\n", "\n");
    }
}

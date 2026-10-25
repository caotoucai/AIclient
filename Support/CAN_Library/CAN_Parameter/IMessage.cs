using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAN_Library.CAN_Parameter
{
    public interface ICAN_Message : IMessage
    {
        bool isFD { set; get; }
        Tx_Method Method { set; get; }
        Message_Type Type { set; get; }
        Message_State State { set; get; }
        ObservableCollection<CAN_Signal> Signals { set; get; }
    }

    public interface ILIN_Message : IMessage
    {
        uint FrameID { set; get; }
        LINDirection Direction { set; get; }
        LINChecksumType CheckSumType { set; get; }
        int CheckSum { set; get; }
        bool Cycle_Flag { set; get; }
        ulong Timespan { set; get; }
        ObservableCollection<CAN_Signal> Signals { set; get; }
    }

    public interface IMonitor_Message : IMessage
    {
        //ObservableCollection<ISignal> Signals { set; get; }
    }

    public interface IMessage: INotifyPropertyChanged
    {
        string Name { set; get; }
        string ECU { set; get; }
        TxRxType TxRx { set; get; }
        uint ID { set; get; }
        uint DLC { set; get; }
        byte[] Data { set; get; }
        string sData { set; get; }
        uint CycleTime { set; get; }
        long LastSendTimeMs { set; get; }
        long LastReceivedTimeMs { set; get; }
        string Error { set; get; }
        string ChannelName { set; get; }
        //ObservableCollection<ISignal> Signals { set; get; }
    }

    public enum Message_State
    {
        Active,
        Paused,
    }

    public enum Message_Type
    {
        CAN_Standard,
        CAN_Extended,
        CAN_FD_Standard,
        CAN_FD_Extended,
    }

    public enum Tx_Method
    {
        Cyclic,
        Spontan,
        CyclicAndSpontan,        
    }

    public enum FR_TX_Mode
    {
        Cyclic,
        SingleShot, //Event
        None,
    }

    public enum TxRxType
    {
        Tx,
        Rx,
        Err
    }

    public enum ValueGeneratorType
    {
        None,
        Toggle_Switch,
        Range_of_Value,
        Ramps_and_Pulses,
        Random,
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace CAN_Library.CAN_Parameter
{
    [Serializable]
    public class Diag_DID: IDiag_DID
    {
        private string _DIDname = "";
        public string DIDname
        {
            set
            {
                _DIDname = value;
                OnPropertyChanged("DIDname");
            }
            get
            {
                return _DIDname;
            }
        }

        private IDtype _IDtype = IDtype.DID;
        public IDtype IDtype
        {
            set
            {
                _IDtype = value;
                OnPropertyChanged("IDtype");
            }
            get
            {
                return _IDtype;
            }
        }

        private byte[] _id=new byte[] { 0};
        public byte[] id
        {
            set
            {
                _id = value;
                OnPropertyChanged("id");
            }
            get
            {
                return _id;
            }
        }

        private uint _readSID = 0;
        public uint readSID
        {
            set
            {
                _readSID = value;
                OnPropertyChanged("readSid");
            }
            get
            {
                return _readSID;
            }
        }

        private uint _writeSID = 0;
        public uint writeSID
        {
            set
            {
                _writeSID = value;
                OnPropertyChanged("writeSid");
            }
            get
            {
                return _writeSID;
            }
        }

        private uint _DLC = 0;
        public uint DLC
        {
            set
            {
                _DLC = value;
                OnPropertyChanged("DLC");
            }
            get
            {
                return _DLC;
            }
        }

        private DIDType _DIDType = DIDType.ReadWrite;
        public DIDType DIDType
        {
            set
            {
                _DIDType = value;
                OnPropertyChanged("DIDType");
            }
            get
            {
                return _DIDType;
            }
        }

        private RWStatus _RWStatus = RWStatus.Read;
        public RWStatus RWStatus
        {
            set
            {
                if (_DIDType == DIDType.Read)
                {
                    _RWStatus = RWStatus.Read;
                }
                else { _RWStatus = value; }                
                OnPropertyChanged("RWStatus");
            }
            get
            {
                return _RWStatus;
            }
        }

        private byte[] _Data = { 0 };
        public byte[] Data
        {
            set
            {
                if (value == null)
                {
                    _Data = value;
                }
                else//Data != null
                {
                    if (_Data == null)//Data != _Data
                    {
                        _Data = value;
                        if (did_signals != null && did_signals.Count > 0)
                        {
                            foreach (var item in did_signals)
                            {
                                Get_Signal(item);
                            }
                        }
                    }
                    else if (!Enumerable.SequenceEqual(value, _Data))//Data != null && _Data != null && /Data != _Data
                    {
                        _Data = value;
                        if (did_signals != null && did_signals.Count > 0)
                        {
                            foreach (var item in did_signals)
                            {
                                Get_Signal(item);
                            }
                        }
                    }
                }
                OnPropertyChanged("Data");
            }
            get
            {
                if (_Data != null)
                {
                    if (DLC != _Data.Length)
                    {
                        sData = "Data length error";
                    }
                    else
                    {
                        sData = BitConverter.ToString(_Data, 0, (int)DLC).Replace("-", " ").ToUpper();
                    }
                }
                else
                {
                    sData = "";
                }
                return _Data;
            }
        }

        private string _sData = "";
        public string sData
        {
            set
            {
                _sData = value;
                OnPropertyChanged("sData");
            }
            get
            {
                return _sData;
            }
        }

        private string _respond = "";
        public string respond
        {
            set
            {
                if (_respond != value)
                {
                    _respond = value;
                    if (_respond == "")
                    {
                        _respondStatus = respondStatus.noRespond;
                    }
                    else if (_respond.Split(' ')[0] == "7F")
                    {
                        _respondStatus = respondStatus.nagative;
                    }
                    else
                    {
                        _respondStatus = respondStatus.positive;
                    }                    
                }                                   
                OnPropertyChanged("respond");
            }
            get
            {
                return _respond;
            }
        }

        private respondStatus _respondStatus = respondStatus.noRespond;
        public respondStatus respondStatus
        {
            set
            {
                _respondStatus = value;
                OnPropertyChanged("respondStatus");
            }
            get
            {
                return _respondStatus;
            }
        }

        private List<Diag_Signal> _did_signals = new List<Diag_Signal>();
        public List<Diag_Signal> did_signals
        {
            set
            {
                _did_signals = value;
                OnPropertyChanged("did_signals");
            }
            get
            {
                return _did_signals;
            }
        }
        public Diag_DID()
        {
            did_signals = new List<Diag_Signal>();
        }
        public Diag_DID(byte[] ID, string DIDName, uint DLC, RWStatus RW)
        {
            this.id = ID;
            this.DIDname = DIDName;
            this.DLC = DLC;
            this.RWStatus = RW;
            Data = new byte[DLC];

            did_signals = new List<Diag_Signal>();
        }

        #region INotifyPropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public void Set_Signal(Diag_Signal Signal, long Value)
        {
            try
            {
                if (Signal.maxValue != Signal.minValue)
                {
                    if (Value > (Signal.maxValue - Signal.offset) / Signal.factor)
                        Value = (long)((Signal.maxValue - Signal.offset) / Signal.factor);
                }
                //little endian -> big endian
                int startbit = Signal.startBit;
                if (Signal.byteOrder == byteOrder.little_endian)
                {
                    for (int i = 0; i < Signal.bitLength-1; i++)
                    {
                        startbit++;
                        if (startbit % 8 == 0)
                            startbit -= 16;
                    }
                   
                }

                if (Signal.bitLength <= 64)
                {
                    ulong Signal_Value = (ulong)Value;
                    Signal.value = Signal_Value * Signal.factor + Signal.offset;                    
                    
                    int Start_BytePosition = startbit / 8;
                    int Start_BitPosition = 8 - startbit % 8;
                    int AvailableFill_Space = 9 - Start_BitPosition;
                    int FillLength = 0;

                    if (AvailableFill_Space >= Signal.bitLength)
                    {
                        int LeftMoveStep = AvailableFill_Space - Signal.bitLength;
                        ulong MaxValue = Get_MaxValue(Signal.bitLength) << LeftMoveStep;
                        ulong Clean_Value = Data[Start_BytePosition] & (~MaxValue & 0xFF);
                        Data[Start_BytePosition] = Convert.ToByte(Clean_Value | (Signal_Value << LeftMoveStep));
                    }
                    else
                    {
                        int Shift_Length = Signal.bitLength - AvailableFill_Space;
                        ulong Cut_Value = Signal_Value >> Shift_Length;
                        ulong Left_Value = Signal_Value & Get_MaxValue(Shift_Length);
                        ulong Clean_Value = Data[Start_BytePosition] & (~Get_MaxValue(AvailableFill_Space) & 0xFF);
                        Data[Start_BytePosition] = Convert.ToByte(Clean_Value | Cut_Value);
                        while (Shift_Length > 0)
                        {
                            Start_BytePosition += 1;
                            if (Shift_Length > 8)
                            {
                                FillLength = 8;
                                Shift_Length = Shift_Length - FillLength;
                                Cut_Value = Left_Value >> Shift_Length;
                                Left_Value = Left_Value & Get_MaxValue(Shift_Length);
                                Data[Start_BytePosition] = Convert.ToByte(Cut_Value);
                            }
                            else
                            {
                                FillLength = Shift_Length;
                                int LeftMoveStep = 8 - Shift_Length;
                                Clean_Value = Data[Start_BytePosition] & Get_MaxValue(LeftMoveStep);
                                Data[Start_BytePosition] = Convert.ToByte(Clean_Value | (Left_Value << LeftMoveStep));
                                Shift_Length -= FillLength;
                            }
                        }
                    }
                    //set data in signal
                    int byteNum = (int)Math.Ceiling(Signal.bitLength / 8.0);
                    string formatStr = "X" + (byteNum * 2).ToString();
                    string hexDatawithSpace = Value.ToString(formatStr);
                    for (int i = 0; i < byteNum; i++)
                    {
                        hexDatawithSpace = hexDatawithSpace.Insert((byteNum - i) * 2, " ");
                    }
                    Signal.hexData = hexDatawithSpace;
                }
                else
                {
                    Signal.value = Value * Signal.factor + Signal.offset;
                    int Start_BytePosition = startbit / 8;
                    int byteNum = (int)Math.Ceiling(Signal.bitLength / 8.0);
                    string formatStr = "X" + (byteNum * 2).ToString();
                    string hexData=Value.ToString(formatStr);
                    string hexDatawithSpace = hexData;
                    for (int i = 0; i < byteNum; i++)
                    {
                        Data[Start_BytePosition + i] = Convert.ToByte(hexData.Substring(i * 2, 2),16);
                        hexDatawithSpace = hexDatawithSpace.Insert((byteNum - i) * 2, " ");
                    }
                    Signal.hexData = hexDatawithSpace;
                }
                
            }
            catch (Exception ex)
            {
                //Log.Logger_Run("3.1.1-Set_Signal_RawValue() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        public void Set_signal_wripper(Diag_Signal didSignal, double signalValue)
        {
            long value = 0;
            switch (didSignal.dataFormat)
            {
                case dataFormat.physical:
                    value = (long)((signalValue - didSignal.offset) / didSignal.factor);
                    break;
                case dataFormat.hex:
                    value = (long)((signalValue - didSignal.offset) / didSignal.factor);
                    break;
                case dataFormat.dec:
                    value = (long)signalValue;
                    break;
                case dataFormat.text:
                    int i = 0;
                    foreach (char a in signalValue.ToString())
                    {
                        value += (long)((int)a * Math.Pow(256, i));
                        i++;
                    }
                    break;
            }
            Set_Signal(didSignal, value);//raw data
        }
        public void Set_signal_hex(Diag_Signal didSignal, string hex_signalData)
        {
            try
            {
                //text
                if (didSignal.dataFormat == dataFormat.text)
                {
                    char[] textarray=hex_signalData.ToCharArray();
                    hex_signalData = "";
                    foreach (var eachbyte in System.Text.Encoding.ASCII.GetBytes(textarray))
                    {
                        hex_signalData += eachbyte.ToString("X2");
                    }

                    didSignal.text = hex_signalData;
                    didSignal.hexData = hex_signalData;

                }
                //update data in signal
                hex_signalData = hex_signalData.Replace(" ", "");
                int bytelen = (int)Math.Ceiling(didSignal.bitLength / 8.0);

                hex_signalData = hex_signalData.PadLeft(bytelen * 2, '0');//不满整字节前面补0
                string subString = hex_signalData;

                //Data
                for (int i = 0; i < bytelen; i++)
                {
                    Data[didSignal.startBit / 8 + i] = Convert.ToByte(hex_signalData.Substring(i * 2, 2), 16);
                }

                //hexData
                for (int i = 1; i < bytelen; i++)
                {
                    hex_signalData = hex_signalData.Insert((bytelen - i) * 2, " ");
                }
                didSignal.hexData = hex_signalData;
                //value
                if (bytelen > 8)
                    subString = subString.Substring(bytelen * 2 - 16, 16);
                double rawvalue = ulong.Parse(subString, NumberStyles.AllowHexSpecifier);//大于8字节的数据转成rawdata会报错
                didSignal.value = rawvalue * didSignal.factor + didSignal.offset;
                
            }
            catch (Exception ex)
            {
                //Log.Logger_Run("Set_signal_hex() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        private ulong Get_MaxValue(int Length = 0)
        {
            int loop = Length;
            ulong value = 0;
            for (int i = 0; i < loop; i++)
            {
                value += (ulong)Math.Pow(2, Length - 1);
                Length -= 1;
            }
            return value;
        }
        private void Get_Signal(Diag_Signal Signal)
        {
            if (_Data == null || _Data.Length != DLC)
                return;
            //little endian -> big endian
            int startbit = Signal.startBit;
            if (Signal.byteOrder == byteOrder.little_endian)
            {
                for (int i = 0; i < Signal.bitLength - 1; i++)
                {
                    startbit++;
                    if (startbit % 8 == 0)
                        startbit -= 16;
                }

            }
            int Start_BytePosition = startbit / 8;
            int Start_BitPosition = 8 - startbit % 8;
            int AvailableFill_Space = 9 - Start_BitPosition;
            if (AvailableFill_Space >= Signal.bitLength)
            {
                int MoveStep = AvailableFill_Space - Signal.bitLength;
                ulong Get_Value = (ulong)(_Data[Start_BytePosition] >> MoveStep) & Get_MaxValue(Signal.bitLength);
                Signal.value = Get_Value * Signal.factor + Signal.offset;
                Signal.hexData = Get_Value.ToString("X2");
            }
            else
            {
                ulong Get_Value = 0;
                int MoveStep = Signal.bitLength - AvailableFill_Space;
                Get_Value = (_Data[Start_BytePosition] & Get_MaxValue(AvailableFill_Space)) << MoveStep;
                while (MoveStep > 0)
                {
                    Start_BytePosition += 1;
                    if (MoveStep > 8)
                    {
                        MoveStep -= 8;
                        Get_Value = Get_Value | ((ulong)_Data[Start_BytePosition] << MoveStep);
                    }
                    else
                    {
                        Get_Value = Get_Value | ((ulong)_Data[Start_BytePosition] >> (8 - MoveStep));
                        MoveStep -= MoveStep;
                    }
                }
                Signal.value = Get_Value * Signal.factor + Signal.offset;
                
                //get hexData   
                int byteNum = (int)Math.Ceiling(Signal.bitLength / 8.0);
                
                string TempDefaultvalueStr = "";
                if (byteNum <= 1)
                {
                    string formatStr = "X" + (byteNum * 2).ToString();
                    TempDefaultvalueStr = ((long)Get_Value).ToString(formatStr);
                    for (int i = 1; i < byteNum; i++)
                    {
                        TempDefaultvalueStr = TempDefaultvalueStr.Insert((byteNum - i) * 2, " ");
                    }
                }
                else
                {
                    for (int i = 0; i < byteNum; i++)
                    {
                        TempDefaultvalueStr += _Data[Signal.startBit / 8 + i].ToString("X2") + " ";
                    }
                }                                
                Signal.hexData = TempDefaultvalueStr;            
            }
            //get text
            if (Signal.dataFormat == dataFormat.text)
            {
                byte[] array = new byte[Signal.bitLength / 8];
                for (int i = 0; i < Signal.bitLength / 8; i++)
                {
                    array[i] = (byte)(Convert.ToInt32(Signal.hexData.Split(' ')[i], 16)); //ASCII码强制转换二进制                            
                }
                Signal.text = Convert.ToString(System.Text.Encoding.ASCII.GetString(array));

            }
        }
        
    }


    public interface IDiag_DID : INotifyPropertyChanged
    {
        string DIDname { set; get; }
        IDtype IDtype { set; get; }
        byte[] id { set; get; }
        uint readSID { set; get; }
        uint writeSID { set; get; }
        uint DLC { set; get; }
        DIDType DIDType { set; get; }
        RWStatus RWStatus { set; get; }
        byte[] Data { set; get; }
        string sData { set; get; }
        List<Diag_Signal> did_signals { set; get; }

    }
    public class Diag_Signal : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
        public string Diag_signalName { set; get; }
        public string DIDname { set; get; }
        public int startBit { set; get; }
        public int bitLength { set; get; }
        public byteOrder byteOrder { set; get; }

        public double factor { set; get; }
        public double offset { set; get; }
        public double minValue { set; get; }
        public double maxValue { set; get; }
        public string unit { set; get; }
        public dataFormat dataFormat { set; get; }
        public double defValue { set; get; }
        public double value { set; get; }
        public string hexData { set; get; }
        public string text { set; get; }
        public List<string> valueTable { set; get; }=new List<string>();

    }
    public enum IDtype
    {
        DID,
        PID,
        DPID,
        CPID,
        RID,
    }
    public enum DIDType
    {
        Read,
        ReadWrite,
        Read_Parameter_Identifier,
        Request,
        Send,
        TransferData,
        PID_list,
        Control,
        Start,//session
        StartStop,//routine start and stop
        StartResults,//routine start and result
        Setup,
        ReadOnceReadScheduledSlow,
        Reset
    }
    public enum RWStatus
    {
        Read,
        Write,
        Command,
    }
    public enum dataFormat
    {
        hex,
        dec,
        physical,
        text,
    }
    public enum byteOrder
    {
        big_endian,
        little_endian,
    }
    public enum respondStatus
    {
        positive,
        nagative,
        noRespond,
    }


}

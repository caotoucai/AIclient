using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CAN_Library.CAN_Driver;
using System.Linq;

namespace CAN_Library.CAN_Parameter
{
    [Serializable]
    public class LIN_Message : ILIN_Message
    {
        #region INotifyPropertyChanged
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

        private int CalculatorPID(int ID)
        {
            int ID0 = (ID & 0x01) == 0x01 ? 1 : 0;
            int ID1 = (ID & 0x02) == 0x02 ? 1 : 0;
            int ID2 = (ID & 0x04) == 0x04 ? 1 : 0;
            int ID3 = (ID & 0x08) == 0x08 ? 1 : 0;
            int ID4 = (ID & 0x10) == 0x10 ? 1 : 0;
            int ID5 = (ID & 0x20) == 0x20 ? 1 : 0;

            int ID6 = ID0 ^ ID1 ^ ID2 ^ ID4;
            int ID7 = ~(ID1 ^ ID3 ^ ID4 ^ ID5);
            ID = (byte)(ID | (ID6 << 6));
            ID = (byte)(ID | (ID7 << 7));
            return ID;
        }

        private uint _ID = 0x0;
        public uint ID
        {
            set
            {
                if (value > 0x3F)
                {
                    _ID = 0x3F;
                }
                else
                {
                    _ID = value;
                }

                _FrameID = (uint)CalculatorPID((int)_ID);
                OnPropertyChanged("ID");
            }
            get
            {
                return _ID;
            }
        }

        private string _Name = "";
        public string Name
        {
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
            get
            {
                return _Name;
            }
        }

        private String _ChannelName = "";
        public String ChannelName
        {
            set
            {
                _ChannelName = value;
                OnPropertyChanged("ChannelName");
            }
            get { return _ChannelName; }
        }

        private string _ECU = "";
        public string ECU
        {
            set
            {
                _ECU = value;
                OnPropertyChanged("ECU");
            }
            get
            {
                return _ECU;
            }
        }

        private uint _FrameID = 0x80;
        public uint FrameID
        {
            set
            {
                _FrameID = value;
                _ID = (uint)(value & 0x3F);
                OnPropertyChanged("FrameID");
            }
            get
            {
                return _FrameID;
            }
        }

        private uint _DLC = 8;  //DLC
        public uint DLC
        {
            set
            {
                if (value < 9 && value > 0)
                {
                    _DLC = value;
                }
                else if (value == 0)
                {
                    _DLC = 1;
                }
                else
                {
                    _DLC = 8;
                }
                OnPropertyChanged("DLC");
            }
            get
            {
                return _DLC;
            }
        }

        private TxRxType _TxRx = TxRxType.Rx;
        public TxRxType TxRx
        {
            set
            {
                _TxRx = value;
                OnPropertyChanged("TxRx");
            }
            get
            {
                return _TxRx;
            }
        }

        private LINDirection _Direction = LINDirection.Publisher;
        public LINDirection Direction
        {
            set
            {
                _Direction = value;
                OnPropertyChanged("Direction");
            }
            get
            {
                return _Direction;
            }
        }

        //default to 2.1
        private LINChecksumType _CheckSumType = LINChecksumType.Enhanced;
        public LINChecksumType CheckSumType
        {
            set
            {
                _CheckSumType = value;
                OnPropertyChanged("CheckSumType");
            }
            get
            {
                return _CheckSumType;
            }
        }

        private string _sData = "";
        public string sData
        {
            set
            {
                _sData = value;
                _Data = StringToByte(value);
                _LData = StringToByte(value);
                OnPropertyChanged("sData");
            }
            get
            {
                if (_Data == null)
                    return "no data";
                else
                    return BitConverter.ToString(_Data, 0).Replace("-", " ").ToUpper();
            }
        }

        //??????
        private byte[] _LData = new byte[8];
        public byte[] LData
        {
            set
            {
                _LData = value;
                OnPropertyChanged("LData");
            }
            get
            {
                return _LData;
            }
        }

        private byte[] _Data = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
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
                        if (Signals != null && Signals.Count > 0)
                        {
                            foreach (var item in Signals)
                            {
                                Get_Signal(item);
                            }
                        }
                    }
                    else if (!Enumerable.SequenceEqual(value, _Data))//Data != null && _Data != null && /Data != _Data
                    {
                        _Data = value;
                        if (Signals != null && Signals.Count > 0)
                        {
                            foreach (var item in Signals)
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
                //if (_Data != null)
                //{
                //    if (DLC != _Data.Length)
                //    {
                //        sData = "CAN Data length error";
                //    }
                //    else
                //    {
                //        sData = BitConverter.ToString(_Data, 0, (int)DLC).Replace("-", " ").ToUpper();
                //    }
                //}
                //else
                //{
                //    sData = "";
                //}
                return _Data;
            }
        }

        private int _CheckSum;
        public int CheckSum
        {
            set
            {
                _CheckSum = value;
                OnPropertyChanged("CheckSum");
            }
            get
            {
                return _CheckSum;
            }
        }

        private string _Error = "";
        public string Error
        {
            set
            {
                _Error = value;
                OnPropertyChanged("Error");
            }
            get
            {
                return _Error;
            }
        }

        private ulong _LastValue;

        private ulong _Timespan;
        public ulong Timespan
        {
            set
            {

                _Timespan = value - _LastValue;
                _Timespan = _Timespan / 1000;
                _LastValue = value;
                OnPropertyChanged("Timespan");
            }
            get
            {
                return _Timespan;
            }
        }

        //private long _Count;
        //public long Count
        //{
        //    set
        //    {
        //        _Count = value;
        //        OnPropertyChanged("Count");
        //    }
        //    get
        //    {
        //        return _Count;
        //    }
        //}

        //private int _Flag = 0;
        //public int Flag
        //{
        //    set
        //    {
        //        _Flag = value;
        //        OnPropertyChanged("Flag");
        //    }
        //    get
        //    {
        //        return _Flag;
        //    }
        //}

        private uint _CycleTime = 100;
        public uint CycleTime
        {
            set
            {
                _CycleTime = value;
                OnPropertyChanged("CycleTime");
            }
            get
            {
                return _CycleTime;
            }
        }

        private bool _Cycle_Flag = false;
        public bool Cycle_Flag
        {
            set
            {
                _Cycle_Flag = value;
                OnPropertyChanged("Cycle_Flag");
            }
            get
            {
                return _Cycle_Flag;
            }
        }

        private long _LastSendTimeMs = 0;  // for Publisher
        public long LastSendTimeMs
        {
            set
            {
                _LastSendTimeMs = value;
                OnPropertyChanged("LastSendTimeMs");
            }
            get
            {
                return _LastSendTimeMs;
            }
        }

        private long _LastReceivedTimeMs = 0;
        public long LastReceivedTimeMs
        {
            set
            {
                _LastReceivedTimeMs = value;
                OnPropertyChanged("LastReceivedTimeMs");
            }
            get
            {
                return _LastReceivedTimeMs;
            }
        }

        private Message_State _State = Message_State.Active;
        public Message_State State
        {
            set
            {
                _State = value;
                OnPropertyChanged("State");
            }
            get
            {
                return _State;
            }
        }

        private bool _E2E_Active = false;
        public bool E2E_Active
        {
            get
            {
                return _E2E_Active;
            }
            set
            {
                _E2E_Active = value;
            }
        }

        private ObservableCollection<CAN_Signal> _Signals = new ObservableCollection<CAN_Signal>();
        public ObservableCollection<CAN_Signal> Signals
        {
            set
            {
                _Signals = value;
                OnPropertyChanged("Signals");
            }
            get
            {
                return _Signals;
            }
        }

        /// <summary>
        /// <para>same CANID last 10 messages on bus</para>
        /// <para>CAN_Message_sampleType class contains time, tx/rx, ID, dlc, data</para>
        /// </summary>
        private List<CAN_Message_sampleType> _Recent_10_Messages = new List<CAN_Message_sampleType>();
        public List<CAN_Message_sampleType> Recent_10_Messages
        {
            set
            {
                _Recent_10_Messages = value;
                OnPropertyChanged("Recent_10_Messages");
            }
            get
            {
                return _Recent_10_Messages;
            }
        }

        public LIN_Message()
        {
            //Data = new byte [8];
        }

        public LIN_Message(uint frameID, uint DLC, byte[] Data, LINDirection direction)
        {
            this.FrameID = (Byte)frameID;
            this.DLC = Convert.ToByte(DLC);
            this.Data = Data;
            this.CheckSumType = LINChecksumType.Classic;
            this.Direction = direction;
        }

        public LIN_Message(uint frameID, uint DLC, byte[] Data)
        {
            this.FrameID = (Byte)frameID;
            this.DLC = Convert.ToByte(DLC);
        }

        private byte[] StringToByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            int byteLength = hexString.Length / 2;
            if ((hexString.Length % 2) != 0)
            {
                hexString += " ";
            }
            byte[] returnBytes = new byte[byteLength];
            for (int i = 0; i < byteLength; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }

        public void Set_Signal_RawValue_old(CAN_Signal Signal, ulong Value)
        {
            try
            {
                ulong Signal_Value = (ulong)Value;
                Signal.Value = Signal_Value;
                int Start_BytePosition = Signal.StartBit / 8;
                int Start_BitPosition = Signal.StartBit % 8;
                int AvailableFill_Space = 8 - Start_BitPosition;

                if (AvailableFill_Space >= Signal.Length)
                {
                    //int LeftMoveStep = AvailableFill_Space - Signal.Length;
                    ulong MaxValue = Get_MaxValue(Signal.Length);
                    ulong Original_Value = Data[Start_BytePosition] & (~(MaxValue << Start_BitPosition));
                    Data[Start_BytePosition] = Convert.ToByte(Original_Value | (Signal_Value << Start_BitPosition));
                }
                else
                {
                    ulong StartByte_Value = Signal_Value & 0xFF;
                    ulong MaxValue = Get_MaxValue(8);
                    ulong Original_Value = Data[Start_BytePosition] & (~(MaxValue << Start_BitPosition));
                    Data[Start_BytePosition] = Convert.ToByte(Original_Value | (StartByte_Value << Start_BitPosition));

                    int Remain_Length = Signal.Length - 8;
                    ulong NextByte_Value = (Signal_Value >> 8) & 0xFF;

                    while (Remain_Length > 0)
                    {
                        Start_BitPosition = 0;
                        Start_BytePosition += 1;
                        if (Remain_Length > 8)
                        {
                            //NextByte_Value = (NextByte_Value) & 0xFF;
                            //MaxValue = Get_MaxValue(8);
                            //Original_Value = Data[Start_BytePosition] & (~MaxValue & 0xFF);
                            //Data[Start_BytePosition] = Convert.ToByte(Original_Value | (NextByte_Value << Start_BitPosition));
                            //NextByte_Value = (NextByte_Value >> 8) & 0xFF;
                        }
                        else
                        {
                            MaxValue = Get_MaxValue(Remain_Length);
                            Original_Value = Data[Start_BytePosition] & (~(MaxValue << Start_BitPosition));
                            Data[Start_BytePosition] = Convert.ToByte(Original_Value | (NextByte_Value << Start_BitPosition));
                        }

                        Remain_Length = Remain_Length - 8;
                    }
                }

                string Temp = "";
                for (int i = 0; i < DLC; i++)
                {
                    Temp += Data[i].ToString("X2") + " ";
                }
                sData = Temp.Trim();
            }
            catch (Exception ex)
            {
                //Console.WriteLine("3.1.1-Set_Signal_RawValue() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        public void Set_Signal_RawValue(CAN_Signal Signal, double Value)
        {
            try
            {
                ulong Signal_Value = (ulong)Value;
                Signal.Value = Signal_Value;

                if (Signal.Value != Signal.olderValue)
                {
                    Signal.valueChangeList.Add(new CANSignalChangeInfo(DateTime.Now, Signal.olderValue, Signal.Value));
                    Signal.olderValue = Signal.Value;
                    if (Signal.valueChangeList.Count > 20)
                    {
                        Signal.valueChangeList.RemoveAt(0);
                    }
                }

                int Start_BytePosition = Signal.StartBit / 8;    //1
                int Start_BitPosition = Signal.StartBit % 8;     //6
                int AvailableFill_Space = 8 - Start_BitPosition;  //2
               
                if (AvailableFill_Space >= Signal.Length)
                {
                    ulong MaxValue = Get_MaxValue(Signal.Length);
                    ulong Original_Value = Data[Start_BytePosition] & (~(MaxValue << Start_BitPosition));
                    Data[Start_BytePosition] = Convert.ToByte(Original_Value | (Signal_Value << Start_BitPosition));
                }
                else
                {
                    ulong MaxValue = Get_MaxValue(AvailableFill_Space);
                    ulong StartByte_Value = Signal_Value & MaxValue ;
                    ulong Original_Value = Data[Start_BytePosition] & Get_MaxValue(Start_BitPosition);
                    Data[Start_BytePosition] = Convert.ToByte(Original_Value | (StartByte_Value << Start_BitPosition));

                    int Remain_Length = Signal.Length - AvailableFill_Space;  
                    ulong NextByte_Value = Signal_Value >> AvailableFill_Space;

                    while (Remain_Length > 0)
                    {
                        Start_BitPosition = 0;
                        Start_BytePosition += 1;
                        if (Remain_Length > 8)
                        {
                            //NextByte_Value = (NextByte_Value) & 0xFF;
                            //MaxValue = Get_MaxValue(8);
                            //Original_Value = Data[Start_BytePosition] & (~MaxValue & 0xFF);
                            //Data[Start_BytePosition] = Convert.ToByte(Original_Value | (NextByte_Value << Start_BitPosition));
                            //NextByte_Value = (NextByte_Value >> 8) & 0xFF;
                        }
                        else
                        {
                            MaxValue = Get_MaxValue(Remain_Length);

                            Original_Value = Data[Start_BytePosition] & (~(MaxValue << Start_BitPosition));
                            Data[Start_BytePosition] = Convert.ToByte(Original_Value | (NextByte_Value << Start_BitPosition ));
                        }

                        Remain_Length = Remain_Length - 8;// assume complete
                    }
                }

                string Temp = "";
                for (int i = 0; i < DLC; i++)
                {
                    Temp += Data[i].ToString("X2") + " ";
                }
                sData = Temp.Trim();
            }
            catch (Exception ex)
            {
                //Console.WriteLine("3.1.1-Set_Signal_RawValue() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }


        public void Get_Signal(CAN_Signal Signal)
        {
            if (_Data == null || _Data.Length != DLC)
                return;

            int Start_BytePosition = Signal.StartBit / 8;   // 1
            int Start_BitPosition = Signal.StartBit % 8;  //6

            int AvailableFill_Space = 8 - Start_BitPosition;  //2
            if (AvailableFill_Space >= Signal.Length)
            {
                ulong Get_Value = (ulong)(_Data[Start_BytePosition] >> Start_BitPosition) & Get_MaxValue(Signal.Length);
                Signal.Value = Get_Value * Signal.Factor + Signal.Offset;
            }
            else
            {
                ulong Get_Value = 0;
                int Remain_Length = Signal.Length - AvailableFill_Space;
                Get_Value = (ulong)(_Data[Start_BytePosition] >> Start_BitPosition);
                while (Remain_Length > 0)
                {
                    Start_BytePosition += 1;
                    if (Remain_Length > 8)
                    {
                        //Remain_Length -= 8;
                        //Get_Value = Get_Value | ((ulong)_Data[Start_BytePosition] << MoveStep);
                    }
                    else
                    {
                        ulong MaxValue = Get_MaxValue(Remain_Length);
                        ulong Next_Value = (((ulong)_Data[Start_BytePosition] & MaxValue ) << AvailableFill_Space);
                        Get_Value = Get_Value | Next_Value;
                        Remain_Length = 0; // assume complete
                    }
                }
                Signal.Value = Get_Value * Signal.Factor + Signal.Offset;
            }

            try
            {
                if (Signal.Value != Signal.olderValue)
                {
                    Signal.valueChangeList.Add(new CANSignalChangeInfo(DateTime.Now, Signal.olderValue, Signal.Value));
                    Signal.olderValue = Signal.Value;
                    if (Signal.valueChangeList.Count > 20)
                    {
                        Signal.valueChangeList.RemoveAt(0);
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(string.Format("[exception] Get_Signal(CAN_Signal Signal)\n{0}\n{1}", ex.Message, ex.StackTrace));
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

        public void CalculateChecksum()
        {
            int cs = 0;
            foreach (byte data in Data)
            {
                cs += data;
                if (cs > 0xFF)
                {
                    cs = (cs & 0xFF)+1;
                }
            }
            if (CheckSumType == LINChecksumType.Enhanced)
            {
                cs += (byte)FrameID;
                if (cs > 0xFF)
                {
                    cs = (cs & 0xFF) + 1;
                }
            }

            cs = (byte)(0xFF - cs);
            CheckSum = cs;
        }

        #region LIN E2E
        //byte[] Data_reservedBy1 = { 0 };
        //bool Data_reservedBy1_hasMake = false;
        public void Update_E2E()
        {
            try
            {
                //if (!Data_reservedBy1_hasMake)
                //{
                //    Data_reservedBy1 = dataFill1();
                //    Data_reservedBy1_hasMake = true;
                //}
                //if (Data_reservedBy1 == null)
                //{
                //    //Log.Logger_Run(string.Format("Data_reservedBy1 == null"), Log_Function.Log_Type.Info);
                //    return;
                //}

                foreach (var signal in Signals)
                {
                    if (signal.is_E2E_RollingCount)
                    {
                        double RollingCounter = signal.Value + 1;
                        if (RollingCounter > 14)
                        {
                            RollingCounter = 0;
                        }
                        Set_Signal_RawValue(signal, (ulong)RollingCounter);
                    }
                }
                foreach (var signal in Signals)
                {
                    if (signal.is_E2E_Checksum)
                    {
                        if (!signal.is_E2E_DataID_CRC_Done)
                        {
                            byte[] array01 = new byte[1];
                            array01[0] = signal.E2E_DataID_LByte;
                            byte byte01 = Calculate_CRC(array01, 0xFF, false);
                            array01[0] = signal.E2E_DataID_HByte;
                            signal.E2E_DataID_CRC_Value = Calculate_CRC(array01, byte01, false);
                            signal.is_E2E_DataID_CRC_Done = true;
                        }
                        byte[] array02 = new byte[signal.E2E_EndByte - signal.E2E_StartByte];
                        for (int i = 0; i < signal.E2E_EndByte - signal.E2E_StartByte; i++)
                        {
                            //array02[i] = (byte)(Data[signal.E2E_StartByte + 1 + i] | Data_reservedBy1[signal.E2E_StartByte + 1 + i]);
                            array02[i] = (byte)(Data[signal.E2E_StartByte + i]);
                        }
                        byte checksum = (byte)(Calculate_CRC(array02, signal.E2E_DataID_CRC_Value, false) ^ 0xFF);
                        Set_Signal_RawValue(signal, checksum);
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.Logger_Run("3.1.5-Update_E2E() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        byte Calculate_CRC(byte[] Data, byte value, bool IsFirstCall)
        {
            byte i;
            byte crc = 0xff;
            try
            {
                if (IsFirstCall != true)
                {
                    crc ^= value;
                }
                foreach (byte x in Data)              /* Step through bytes in memory */
                {
                    crc = (byte)(crc ^ (x));     /* Fetch byte from memory, XOR into CRC top byte*/
                    for (i = 0; i < 8; i++)             /* Prepare to rotate 8 bits */
                    {
                        if ((crc & 0x80) != 0)            /* b15 is set... */
                            crc = (byte)((crc << 1) ^ 0x1D);    /* rotate and XOR with polynomic */
                        else                          /* b15 is clear... */
                            crc <<= 1;                  /* just rotate */
                    }                             /* Loop for 8 bits */
                    crc &= 0xff;                  /* Ensure CRC remains 8-bit value */
                }                            /* Loop until num=0 */
                crc ^= 0xff;
                return (crc);                    /* Return updated CRC */
            }
            catch (Exception ex)
            {
                //Log.Logger_Run("3.1.6-Calculate_CRC() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return (crc);                    /* Return updated CRC */
            }
        }


        byte[] dataFill1()
        {
            try
            {
                byte[] dataAll = new byte[DLC];
                foreach (var signal in Signals)
                {
                    int Start_BytePosition = signal.StartBit / 8;
                    int Start_BitPosition = 8 - signal.StartBit % 8;
                    int AvailableFill_Space = 9 - Start_BitPosition;
                    int FillLength = 0;
                    ulong Signal_Value = (ulong)((signal.Max_Value - signal.Offset) / signal.Factor);
                    if (AvailableFill_Space >= signal.Length)
                    {
                        int LeftMoveStep = AvailableFill_Space - signal.Length;
                        ulong MaxValue = Get_MaxValue(signal.Length) << LeftMoveStep;
                        ulong Clean_Value = dataAll[Start_BytePosition] & (~MaxValue & 0xFF);
                        dataAll[Start_BytePosition] = Convert.ToByte(Clean_Value | (Signal_Value << LeftMoveStep));
                    }
                    else
                    {
                        int Shift_Length = signal.Length - AvailableFill_Space;
                        ulong Cut_Value = Signal_Value >> Shift_Length;
                        ulong Left_Value = Signal_Value & Get_MaxValue(Shift_Length);
                        ulong Clean_Value = dataAll[Start_BytePosition] & (~Get_MaxValue(AvailableFill_Space) & 0xFF);
                        dataAll[Start_BytePosition] = Convert.ToByte(Clean_Value | Cut_Value);
                        while (Shift_Length > 0)
                        {
                            Start_BytePosition += 1;
                            if (Shift_Length > 8)
                            {
                                FillLength = 8;
                                Shift_Length = Shift_Length - FillLength;
                                Cut_Value = Left_Value >> Shift_Length;
                                Left_Value = Left_Value & Get_MaxValue(Shift_Length);
                                dataAll[Start_BytePosition] = Convert.ToByte(Cut_Value);
                            }
                            else
                            {
                                FillLength = Shift_Length;
                                int LeftMoveStep = 8 - Shift_Length;
                                Clean_Value = dataAll[Start_BytePosition] & Get_MaxValue(LeftMoveStep);
                                dataAll[Start_BytePosition] = Convert.ToByte(Clean_Value | (Left_Value << LeftMoveStep));
                                Shift_Length -= FillLength;
                            }
                        }
                    }
                }
                for (int i = 0; i < dataAll.Length; i++)
                {
                    dataAll[i] = (byte)~dataAll[i];
                }
                return dataAll;
            }
            catch (Exception ex)
            {
                //Log.Logger_Run("3.1.7-dataFill1() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return null;
            }
        }
        #endregion
    }


    //[Serializable]
    //public class LIN_Signal : INotifyPropertyChanged
    //{
    //    private string _Name = "";
    //    public string Name
    //    {
    //        set
    //        {
    //            _Name = value;
    //            OnPropertyChanged("Name");
    //        }
    //        get
    //        {
    //            return _Name;
    //        }
    //    }

    //    private int _StartBit = 0;
    //    public int StartBit
    //    {
    //        set
    //        {
    //            _StartBit = value;
    //            OnPropertyChanged("StartBit");
    //        }
    //        get
    //        {
    //            return _StartBit;
    //        }
    //    }

    //    private int _Length = 0;
    //    public int Length
    //    {
    //        set
    //        {
    //            _Length = value;
    //            OnPropertyChanged("Length");
    //        }
    //        get
    //        {
    //            return _Length;
    //        }
    //    }

    //    private double _Init_Value = 0;//physical value
    //    public double Init_Value
    //    {
    //        set
    //        {
    //            _Init_Value = value;
    //            OnPropertyChanged("Init_Value");
    //        }
    //        get
    //        {
    //            return _Init_Value;
    //        }
    //    }

    //    private double _Minium = 0;//physical value
    //    public double Minimum
    //    {
    //        set
    //        {
    //            _Minium = value;
    //            OnPropertyChanged("Minimum");
    //        }
    //        get
    //        {
    //            return _Minium;
    //        }
    //    }

    //    private double _Maximum = 0;//physical value 信号有效值的最大值
    //    public double Maximum
    //    {
    //        set
    //        {
    //            _Maximum = value;
    //            OnPropertyChanged("Maximum");
    //        }
    //        get
    //        {
    //            return _Maximum;
    //        }
    //    }

    //    private string _Unit = "";
    //    public string Unit
    //    {
    //        set
    //        {
    //            _Unit = value;
    //            OnPropertyChanged("Unit");
    //        }
    //        get
    //        {
    //            return _Unit;
    //        }
    //    }

    //    private double _Value = 0;//physical value
    //    public double Value
    //    {
    //        set
    //        {
    //            if (value > Maximum)
    //            {
    //                _Value = Maximum;
    //            }
    //            else if (value < Minimum)
    //            {
    //                _Value = Minimum;
    //            }
    //            else
    //            {
    //                _Value = value;
    //            }
    //            OnPropertyChanged("Value");
    //        }
    //        get
    //        {
    //            return _Value;
    //        }
    //    }

    //    #region INotifyPropertyChanged
    //    [field: NonSerialized]
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    protected void OnPropertyChanged(string name)
    //    {
    //        PropertyChangedEventHandler handler = PropertyChanged;
    //        if (handler != null)
    //        {
    //            handler(this, new PropertyChangedEventArgs(name));
    //        }
    //    }
    //    #endregion
    //}

    public class LDF_Settings
    {
        #region INotifyPropertyChanged
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

        private string _Name = "";
        public string Name
        {
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
            get
            {
                return _Name;
            }
        }

        private LIN_Parameter _lin_parameter = new LIN_Parameter ();
        public LIN_Parameter LIN_Parameter
        {
            set
            {
                _lin_parameter = value;
                OnPropertyChanged("LIN_Parameter");
            }
            get
            {
                return _lin_parameter;
            }
        }

        private string _master = "";
        public string Master
        {
            set
            {
                _master = value;
                OnPropertyChanged("Master");
            }
            get
            {
                return _master;
            }
        }

        private ObservableCollection<String> _slaves = new ObservableCollection<String>();
        public ObservableCollection<String> Slaves
        {
            set
            {
                _slaves = value;
                OnPropertyChanged("Slaves");
            }
            get
            {
                return _slaves;
            }
        }
    }

    public class LIN_Node
    {
        #region INotifyPropertyChanged
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

        private string _Name = "";
        public string Name
        {
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
            get
            {
                return _Name;
            }
        }

        private LIN_Verison _Lin_Protocol = LIN_Verison.LIN_2_1;
        public LIN_Verison Lin_Protocol
        {
            set
            {
                _Lin_Protocol = value;
                OnPropertyChanged("Lin_Protocol");
            }
            get
            {
                return _Lin_Protocol;
            }
        }

        private int _configured_NAD = 0;
        public int Configured_NAD
        {
            set
            {
                _configured_NAD = value;
                OnPropertyChanged("Configured_NAD");
            }
            get
            {
                return _configured_NAD;
            }
        }

        private ObservableCollection<LIN_Message> _Frames = new ObservableCollection<LIN_Message>();
        public ObservableCollection<LIN_Message> Frames
        {
            set
            {
                _Frames = value;
                OnPropertyChanged("Frames");
            }
            get
            {
                return _Frames;
            }
        }
    }

    public enum LINDirection
    {
        Disabled = 0,
        Publisher = 1,
        Subscriber = 2,
        SubscriberAutoLength = 3,
    }


    public enum LINChecksumType : byte
    {
        /// <summary>
        /// Custom checksum
        /// </summary>
        //Custom = 0,
        /// <summary>
        /// Classic checksum (ver 1.x)
        /// </summary>
        Classic = 1,
        /// <summary>
        /// Enhanced checksum
        /// </summary>
        Enhanced = 2,
        /// <summary>
        /// Detect checksum
        /// </summary>
        Auto = 3,
    }

}

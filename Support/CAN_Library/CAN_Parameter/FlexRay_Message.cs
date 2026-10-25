using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;
using CAN_Library.CAN_Interface;

namespace CAN_Library.CAN_Parameter
{
    [Serializable]
    public class FlexRay_Message : INotifyPropertyChanged
    {
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

        private string _ECU = "";
        public string ECU//Bench Or Others
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

             

        private uint _ID = 0;
        public uint ID
        {
            set
            {
                _ID = value;
                OnPropertyChanged("ID");
            }
            get
            {
                return _ID;
            }
        }

        private uint _RXIndex = 0;
        public uint RXIndex
        {
            set
            {
                _RXIndex = value;
                OnPropertyChanged("RXIndex");
            }
            get
            {
                return _RXIndex;
            }
        }

        private uint _DLC = 32;
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


        private uint _OffSet = 0;
        public uint OffSet
        {
            set
            {
                _OffSet = value;
                OnPropertyChanged("OffSet");
            }
            get
            {
                return _OffSet;
            }
        }

        private uint _Repetition = 0;
        public uint Repetition
        {
            set
            {
                _Repetition = value;
                OnPropertyChanged("Repetition");
            }
            get
            {
                return _Repetition;
            }
        }

        private FR_TX_Mode _Method = FR_TX_Mode.Cyclic;
        public FR_TX_Mode Method
        {
            set
            {
                _Method = value;
                OnPropertyChanged("Method");
            }
            get
            {
                return _Method;
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
                if (_Data != null)
                {
                    if (DLC != _Data.Length)
                    {
                        sData = "CAN Data length error";
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
            get
            {
                if (_Data == null)
                    return "no data";
                else
                    return BitConverter.ToString(_Data, 0).Replace("-", " ").ToUpper();
            }
            set
            {
                _sData = value;
                _Data = StringToByte(value);
                OnPropertyChanged("sData");
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

        private int _CycleCount = 0;
        public int CycleCount
        {
            set
            {
                _CycleCount = value;
                OnPropertyChanged("CycleCount");
            }
            get
            {
                return _CycleCount;
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

        private long _LastSendTimeMs = 0;
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

#if SIMULATOR
        private bool _Need_Record = false;
#else
        private bool _Need_Record = true;
#endif
        public bool Need_Record
        {
            get
            {
                return _Need_Record;
            }
            set
            {
                _Need_Record = value;
            }
        }

        public int E2E_Method = 1;
        /// <summary>
        /// <para>same frame last 10 messages on bus</para>
        /// <para>CAN_Message_sampleType class contains time, tx/rx, ID, dlc, data</para>
        /// </summary>
        private List<Flexray_Message_sampleType> _Recent_10_Messages = new List<Flexray_Message_sampleType>();
        public List<Flexray_Message_sampleType> Recent_10_Messages
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

        public String PDUName = "";

        //checksum event for E2E
        public delegate byte[] E2EChecksumCallback(FlexRay_Message msg, CAN_Signal signal);
        public event E2EChecksumCallback E2EChecksumCallbackEvent;
        //

        public FlexRay_Message()
        {
            Signals = new ObservableCollection<CAN_Signal>();
        }

        public FlexRay_Message(string Name, uint DLC)
        {
            this.Name = Name;
            this.DLC = DLC;
            Data = new byte[DLC];
            Signals = new ObservableCollection<CAN_Signal>();
        }

        public FlexRay_Message(uint ID, string Name, uint DLC, string ECU)
        {
            this.ID = ID;
            this.Name = Name;
            this.DLC = DLC;
            this.ECU = ECU;
            Data = new byte[DLC];
            Signals = new ObservableCollection<CAN_Signal>();
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

        public void Set_Signal_RawValue(CAN_Signal Signal, double Value)
        {
            try
            {
                ulong Signal_Value = 0;
                if (Value < 0)
                {
                    ulong max = Get_MaxValue(Signal.Length);
                    //ulong r_Value = (ulong)(0 - Value);
                    //ulong orign_Value = (ulong)1 << (Signal.Length - 1) | r_Value;
                    //ulong v2 = (orign_Value ^ (max >> 1)) + 1;

                    ulong v2 = (ulong)Value & max;
                    Signal_Value = v2;
                    Signal.Value = Signal_Value;
                }
                else
                {
                    Signal_Value = (ulong)Value;
                    Signal.Value = Signal_Value * Signal.Factor + Signal.Offset;
                }
                if (Signal.Value != Signal.olderValue)
                {
                    Signal.valueChangeList.Add(new CANSignalChangeInfo(DateTime.Now, Signal.olderValue, Signal.Value));
                    Signal.olderValue = Signal.Value;
                    if (Signal.valueChangeList.Count > 20)
                    {
                        Signal.valueChangeList.RemoveAt(0);
                    }
                }

                int Start_BytePosition = Signal.StartBit / 8;
                int Start_BitPosition = 8 - Signal.StartBit % 8;
                int AvailableFill_Space = 9 - Start_BitPosition;
                int FillLength = 0;

                if (AvailableFill_Space >= Signal.Length)
                {
                    int LeftMoveStep = AvailableFill_Space - Signal.Length;
                    ulong MaxValue = Get_MaxValue(Signal.Length) << LeftMoveStep;
                    ulong Clean_Value = Data[Start_BytePosition] & (~MaxValue & 0xFF);
                    Data[Start_BytePosition] = Convert.ToByte(Clean_Value | (Signal_Value << LeftMoveStep));
                }
                else
                {
                    int Shift_Length = Signal.Length - AvailableFill_Space;
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
            }
            catch (Exception ex)
            {
                Log.Logger_Run("3.1.1-Set_Signal_RawValue() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        public void Set_Signal_UB(CAN_Signal Signal, ulong UB_Value)
        {
            try
            {
                //update UB signal if has
                if (Signal.UpdateBit != -1)
                {
                    int UB_Start_BytePosition = Signal.UpdateBit / 8;
                    int UB_Start_BitPosition = 8 - Signal.UpdateBit % 8;
                    int UB_AvailableFill_Space = 9 - UB_Start_BitPosition;
                    int LeftMoveStep = UB_AvailableFill_Space - 1;
                    ulong Clean_Value = Data[UB_Start_BytePosition] & (~UB_Value & 0xFF);
                    Data[UB_Start_BytePosition] = Convert.ToByte(Clean_Value | (UB_Value << LeftMoveStep));
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("3.1.1-Set_Signal_RawValue() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }
		
        //not used. always Set_Signal_RawValue now
        public void Set_Signal_PhysicalValue(CAN_Signal Signal, double Value)
        {
            try
            {
                double value1 = 0;
                if (Value < Signal.Min_Value)
                    value1 = Signal.Min_Value;
                else if (Value > Signal.Max_Value)
                    value1 = Signal.Max_Value;
                else
                    value1 = Value;                

                ulong Signal_Value = (ulong)Math.Round((value1 - Signal.Offset) / Signal.Factor);//raw value
                if (Signal_Value != Signal.Value)
                {
                    //update UB signal if has
                    Set_Signal_UB(Signal, 1);
                }

                Signal.Value = Signal_Value * Signal.Factor + Signal.Offset;
                int Start_BytePosition = Signal.StartBit / 8;
                int Start_BitPosition = 8 - Signal.StartBit % 8;
                int AvailableFill_Space = 9 - Start_BitPosition;
                int FillLength = 0;

                if (AvailableFill_Space >= Signal.Length)//>=7
                {
                    int LeftMoveStep = AvailableFill_Space - Signal.Length;
                    ulong MaxValue = Get_MaxValue(Signal.Length) << LeftMoveStep;
                    ulong Clean_Value = Data[Start_BytePosition] & (~MaxValue & 0xFF);
                    Data[Start_BytePosition] = Convert.ToByte(Clean_Value | (Signal_Value << LeftMoveStep));
                }
                else
                {
                    int Shift_Length = Signal.Length - AvailableFill_Space;
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
            }
            catch (Exception ex)
            {
                Log.Logger_Run("3.1.2-Set_Signal_PhysicalValue() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        public void Set_Signal_ValueByName(string name, double Value)
        {
            if (Signals.Any(p => p.Name == name))
            {
                Set_Signal_RawValue(Signals.First(p => p.Name == name), (ulong)Value);
            }
        }

        /// <summary>
        /// get signal's physic value
        /// </summary>
        /// <param name="Signal">signal object</param>
        /// <returns>double type signal's physic value</returns>
        public void Get_Signal(CAN_Signal Signal)
        {
            if (_Data == null || _Data.Length != DLC)
                return;
            int Start_BytePosition = Signal.StartBit / 8;
            int Start_BitPosition = 8 - Signal.StartBit % 8;
            int AvailableFill_Space = 9 - Start_BitPosition;
            if (AvailableFill_Space >= Signal.Length)
            {
                int MoveStep = AvailableFill_Space - Signal.Length;
                ulong Get_Value = (ulong)(_Data[Start_BytePosition] >> MoveStep) & Get_MaxValue(Signal.Length);
                Signal.Value = Get_Value * Signal.Factor + Signal.Offset;
            }
            else
            {
                ulong Get_Value = 0;
                int MoveStep = Signal.Length - AvailableFill_Space;
                Get_Value = (_Data[Start_BytePosition] & Get_MaxValue(AvailableFill_Space)) << MoveStep;
                while (MoveStep > 0)
                {
                    Start_BytePosition += 1;
                    if (MoveStep > 8)
                    {
                        MoveStep -= 8;
                        Get_Value = Get_Value |( ((ulong)_Data[Start_BytePosition] << (MoveStep-2))*4);
                    }
                    else
                    {
                        Get_Value = Get_Value | ((ulong)_Data[Start_BytePosition] >> (8 - MoveStep));
                        MoveStep -= MoveStep;
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

        #region FR E2E
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
                    //sapa callback is not realtime, use E2E_RollingCount instead of Value
                    if (signal.is_E2E_RollingCount)
                    {
                        double RollingCounter = signal.Value + 1;
                        if (RollingCounter > 14)
                        {
                            RollingCounter = 0;
                        }
                        Set_Signal_RawValue(signal, RollingCounter);
                    }

                    //if (signal.is_E2E_RollingCount)
                    //{
                    //    if (signal.E2E_RollingCount > 14)
                    //    {
                    //        signal.E2E_RollingCount = 0;
                    //    }                        
                    //    Set_Signal_RawValue(signal, signal.E2E_RollingCount);
                    //    signal.E2E_RollingCount++;
                    //}
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

                        byte[] array02 = signalGroupDataClean(signal.SignalGroup_Name);
                        //byte[] array02 = E2EChecksumCallbackEvent(this,signal);
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

        byte[] signalGroupDataClean_fail(string signalGroupName)
        {
            try
            {
                List<CAN_Signal> tmp_SignalList = Signals.ToList().FindAll(p => p.SignalGroup_Name == signalGroupName);
                byte[] dataAll = new byte[tmp_SignalList.Count - 1]; //execlude checksum signal
                CAN_Signal sg_Cntr = tmp_SignalList.First(p => p.is_E2E_RollingCount == true);
                dataAll[0] = (byte)sg_Cntr.Value; // Byte0 = rollingCount

                List<CAN_Signal> sortAscendingQuery =
                (from sg in tmp_SignalList
                 where sg.is_E2E_RollingCount == false && sg.is_E2E_Checksum == false
                 orderby sg.Name ascending
                 select sg).ToList();

                int i = 1;
                foreach (CAN_Signal sg in sortAscendingQuery)
                {
                    Console.WriteLine(sg.Name);
                    dataAll[i] = (byte)sg.Value;
                    i++;
                }

                return dataAll;
            }
            catch (Exception ex)
            {
                //Log.Logger_Run("3.1.7-dataFill1() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return null;
            }
        }

        byte[] signalGroupDataClean(string signalGroupName)
        {
            List<byte> data_lst = new List<byte> { };
            try
            {
                List<CAN_Signal> tmp_SignalList = Signals.ToList().FindAll(p => p.SignalGroup_Name == signalGroupName);
                CAN_Signal sg_Cntr = tmp_SignalList.First(p => p.is_E2E_RollingCount == true);
                data_lst.Add((byte)sg_Cntr.Value);

                List<CAN_Signal> sortAscendingQuery =
                (from sg in tmp_SignalList
                 where sg.is_E2E_RollingCount == false && sg.is_E2E_Checksum == false
                 orderby sg.Name ascending
                 select sg).ToList();
                
                foreach (CAN_Signal sg in sortAscendingQuery)
                {
                    if (sg.Length >8)
                    {
                        ulong Signal_Value = (ulong)sg.Value;
						data_lst.Add((byte)(Signal_Value & 0xFF)); //should low first
                        data_lst.Add((byte)(Signal_Value >> 8));  //high
                    }
                    else
                    {
                        data_lst.Add((byte)sg.Value);
                    }                    
                }
                byte[] dataAll = data_lst.ToArray();
                return dataAll;
            }
            catch (Exception ex)
            {
                //Log.Logger_Run("3.1.7-dataFill1() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return null;
            }
        }

        byte[] signalGroupDataClean_old(string signalGroupName, int DLC)
        {
            try
            {
                byte[] dataAll = new byte[DLC];
                List<CAN_Signal> tmp_SignalList = Signals.ToList().FindAll(p => p.SignalGroup_Name == signalGroupName);
                foreach (CAN_Signal sg in tmp_SignalList)
                {
                    int Start_BytePosition = sg.StartBit / 8;
                    int Start_BitPosition = 8 - sg.StartBit % 8;
                    int AvailableFill_Space = 9 - Start_BitPosition;
                    int FillLength = 0;
                    ulong Signal_Value = (ulong)sg.Value;
                    if (sg.NewValue != -1 && sg.NewValue != sg.Value)
                    {
                        Signal_Value = (ulong)sg.NewValue;
                        Set_Signal_RawValue(sg, Signal_Value);
                    }

                    if (AvailableFill_Space >= sg.Length)
                    {
                        int LeftMoveStep = AvailableFill_Space - sg.Length;
                        ulong MaxValue = Get_MaxValue(sg.Length) << LeftMoveStep;
                        ulong Clean_Value = dataAll[Start_BytePosition] & (~MaxValue & 0xFF);
                        dataAll[Start_BytePosition] = Convert.ToByte(Clean_Value | (Signal_Value << LeftMoveStep));
                    }
                    else
                    {
                        int Shift_Length = dataAll.Length - AvailableFill_Space;
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
                return dataAll;
            }
            catch (Exception ex)
            {
                //Log.Logger_Run("3.1.7-dataFill1() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return null;
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
    }

    [Serializable]
    public class Flexray_Message_sampleType : INotifyPropertyChanged
    {
        private DateTime _AbsTime = DateTime.Now;
        public DateTime AbsTime
        {
            set { _AbsTime = value; OnPropertyChanged("AbsTime"); }
            get { return _AbsTime; }
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

        private uint _DLC = 32;
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

        private byte[] _Data = { 0 };
        public byte[] Data
        {
            set { _Data = value; OnPropertyChanged("Data"); }
            get { return _Data; }
        }

        public Flexray_Message_sampleType(DateTime time, TxRxType txrx, string name, uint dlc, byte[] data)
        {
            AbsTime = time;
            TxRx = txrx;
            Name = name;
            DLC = dlc;
            Data = data;
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
    }
}

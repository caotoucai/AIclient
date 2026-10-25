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
    public class CAN_Message : ICAN_Message
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

        private Message_Type _Type = Message_Type.CAN_Standard;
        public Message_Type Type
        {
            set
            {
                _Type = value;
                OnPropertyChanged("Type");
            }
            get
            {
                return _Type;
            }
        }

        private String _ChannelName = "";
        public String ChannelName
        {
            set {
                _ChannelName = value;
                OnPropertyChanged("ChannelName");
            }
            get { return _ChannelName; }
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

        private uint _DLC = 8;
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

        private Tx_Method _Method = Tx_Method.Spontan;
        public Tx_Method Method
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

        private uint _CycleTime = 0;
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

        private bool _isFD;
        public bool isFD
        {
            get { return _isFD; }
            set { _isFD = value; }
        }


        ISignal flys = new CAN_Signal();

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

        private long _FirstReceivedTimeMs = 0;
        public long FirstReceivedTimeMs
        {
            set
            {
                _FirstReceivedTimeMs = value;
                OnPropertyChanged("FirstReceivedTimeMs");
            }
            get
            {
                return _FirstReceivedTimeMs;
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
        private bool _isSingleWireHighVoltage = false;
        public bool isSingleWireHighVoltage
        {
            set
            {
                _isSingleWireHighVoltage = value;
                OnPropertyChanged("isSingleWireHighVoltage");
            }
            get
            {
                return _isSingleWireHighVoltage;
            }
        }
        private bool _IsLoadBydbc = false;
        public bool IsLoadBydbc
        {
            get
            {
                return _IsLoadBydbc;
            }
            set
            {
                _IsLoadBydbc = value;
            }
        }

        private bool _IsNMAsrMesssage = false;
        public bool IsNMAsrMesssage
        {
            get { return _IsNMAsrMesssage; }
            set { _IsNMAsrMesssage = value; }
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

        public int E2E_Method = 1;

        public bool valueGenerator_Active = false;
        public bool rolling_Active = false;


        public CAN_Message()
        {
            Signals = new ObservableCollection<CAN_Signal>();
        }

        public CAN_Message(uint ID, string Name, uint DLC, string ECU)
        {
            this.ID = ID;
            this.Name = Name;
            this.DLC = DLC;
            this.ECU = ECU;
            Data = new byte[DLC];
            this.Method = Tx_Method.Spontan;
            Signals = new ObservableCollection<CAN_Signal>();
        }

        public CAN_Message(uint ID, uint DLC, byte[] Data, uint CycleTime, Message_State State, TxRxType TxRx)
        {
            this.ID = ID;
            this.DLC = DLC;
            this.Data = Data;
            this.CycleTime = CycleTime;
            this.State = State;
            this.TxRx = TxRx;
            if (TxRx == TxRxType.Tx)
            {
                this.ECU = "others";
            }
            else
            {
                this.ECU = CAN_Channel._benchName;
            }
            this.Method = Tx_Method.Cyclic;
            Signals = new ObservableCollection<CAN_Signal>();
        }

        public CAN_Message(uint ID, uint DLC, byte[] Data, TxRxType TxRx, bool IsFD)
        {
            this.ID = ID;
            this.DLC = DLC;
            this.Data = Data;
            this.TxRx = TxRx;
            this.Method = Tx_Method.Spontan;
            this.Type = ConvertToMsgType(ID, IsFD);
            Signals = new ObservableCollection<CAN_Signal>();
        }

        private Message_Type ConvertToMsgType(uint ID, bool IsFD)
        {
            if ((ID & 0x1FFF0000) > 0)
            {
                if (!IsFD)
                    return Message_Type.CAN_Extended;
                else
                    return Message_Type.CAN_FD_Extended;
            }
            else
            {
                if (!IsFD)
                    return Message_Type.CAN_Standard;
                else
                    return Message_Type.CAN_FD_Standard;
            }
        }

        public CAN_Message(uint ID, uint DLC, byte[] Data, Tx_Method method)
        {
            this.ID = ID;
            this.DLC = DLC;
            this.Data = Data;
            this.Method = method;
            Signals = new ObservableCollection<CAN_Signal>();
        }

        public void Set_Signal_RawValue(CAN_Signal Signal, ulong Value)
        {
            try
            {
                if (Value > (Signal.Max_Value - Signal.Offset) / Signal.Factor)
                    Value = (ulong)((Signal.Max_Value - Signal.Offset) / Signal.Factor);
                ulong Signal_Value = (ulong)Value;
                Signal.Value = Signal_Value * Signal.Factor + Signal.Offset;
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
                //Log.Logger_Run("3.1.1-Set_Signal_RawValue() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

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
                Signal.Value = value1;
                if (Signal.Value != Signal.olderValue)
                {
                    Signal.valueChangeList.Add(new CANSignalChangeInfo(DateTime.Now, Signal.olderValue, Signal.Value));
                    Signal.olderValue = Signal.Value;
                    if (Signal.valueChangeList.Count > 20)
                    {
                        Signal.valueChangeList.RemoveAt(0);
                    }
                }
                ulong Signal_Value = (ulong)Math.Round((value1 - Signal.Offset) / Signal.Factor);//raw value
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
                //Log.Logger_Run("3.1.2-Set_Signal_PhysicalValue() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        public void Set_Signal_ValueByName(string name, double Value)
        {
            if (Signals.Any(p => p.Name == name))
            {
                Set_Signal_PhysicalValue(Signals.First(p => p.Name == name), Value);
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
                        Get_Value = Get_Value | ((ulong)_Data[Start_BytePosition] << MoveStep);
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
                        Set_Signal_PhysicalValue(signal, RollingCounter);
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
                            array02[i] = (byte)(Data[signal.E2E_StartByte + 1 + i]);
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

        public void Update_Rolling()
        {
            try
            {
                foreach (var signal in Signals)
                {
                    if (signal.isRolling)
                    {
                        signal.rolling_Step_samples_index++;
                        if (signal.rolling_Step_samples_index >= signal.rolling_Step_samples)
                        {
                            double phy_value = signal.Value;
                            double current_step = signal.Rolling_Step_Frac + signal.Rolling_Step;
                            double rolling_step_int = (int)((current_step-signal.Offset)/signal.Factor)* signal.Factor+ signal.Offset;
                            signal.Rolling_Step_Frac = current_step - rolling_step_int;


                            if (phy_value + rolling_step_int < signal.Rolling_MinValue)
                                phy_value = phy_value + rolling_step_int + (signal.Rolling_MaxValue - signal.Rolling_MinValue + 1);
                            else if (phy_value + rolling_step_int > signal.Rolling_MaxValue)
                                phy_value = phy_value + rolling_step_int - (signal.Rolling_MaxValue - signal.Rolling_MinValue + 1);
                            else
                                phy_value += rolling_step_int;

                            signal.Rolling_all += rolling_step_int;//计算rolling累积总数
                            Set_Signal_PhysicalValue(signal, phy_value);
                            signal.rolling_Step_samples_index = 0;                           
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.Logger_Run("3.1.3-Rolling_Signal() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        public void Update_GeneratorValue()
        {
            try
            {
                foreach (var signal in Signals)
                {
                    if (signal.isValueGenerateActive && signal.isRolling==false && signal.is_E2E_Checksum == false && signal.is_E2E_RollingCount == false&& signal.valueGeneratorList.Count>0)
                    {
                        //先判断是否随机
                        if (signal.value_generator_type == ValueGeneratorType.Random)
                        {
                            Random rd = new Random();
                            Set_Signal_PhysicalValue(signal, rd.Next((int)signal.valueGeneratorList[0], (int)signal.valueGeneratorList[1]));
                            continue;
                        }      
                        else 
                        {
                            signal.valueGeneratorListIndex++;
                            //如果valueGeneratorList索引超出范围就归零
                            if (signal.valueGeneratorListIndex >= signal.valueGeneratorList.Count) { signal.valueGeneratorListIndex = 0; }
                            Set_Signal_PhysicalValue(signal, signal.valueGeneratorList[signal.valueGeneratorListIndex]);
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.Logger_Run("3.1.6-Update_GeneratorValue() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        
        }

        #region calculate generator value
        public void generateValue(CAN_Signal signal)
        {
            CAN_Channel canChannel = new CAN_Channel();


                List<double> values = new List<double>();
                double output = 0;//输出的值
                switch (signal.value_generator_type)
                {
                    case ValueGeneratorType.None:
                        return;
                    case ValueGeneratorType.Toggle_Switch:
                        values.Add(canChannel.hex2Decimal(signal.valueGenerator_Settings["Level1"]));
                        values.Add(canChannel.hex2Decimal(signal.valueGenerator_Settings["Level2"]));
                        break;
                    case ValueGeneratorType.Range_of_Value:                   
                        uint hold = uint.Parse(signal.valueGenerator_Settings["Hold"]);//每个值保持几zhen
                        double stepsize = canChannel.hex2Decimal(signal.valueGenerator_Settings["StepSize"]);//每个值的跨度
                        string direction = signal.valueGenerator_Settings["Direction"];//上升，下降还是上升加下降
                        var levels = (signal.Maximum - signal.Minimum) / stepsize+1;//一共有几个level值                      
                        if (direction == "Rising")
                        {
                            output = signal.Minimum;//升序从最小值开始
                            for (int j = 0; j < levels; j++)
                            {
                                for (int i = 0; i < hold; i++) { values.Add(output); }//加hold数量的samples
                                output += stepsize;//升到下一个level
                            }  
                        }
                        else if (direction == "Falling")
                        {
                            output = signal.Maximum;//降序从最大值开始
                            for (int j = 0; j < levels; j++)
                            {
                                for (int i = 0; i < hold; i++) { values.Add(output); }//加hold数量的samples
                                output -= stepsize;//降到下一个level
                            }
                        }
                        else if (direction == "Alternate")
                        {
                            output = signal.Minimum;//升序从最小值开始
                            for (int j = 0; j < levels-1; j++)//最后一个level不要，和下面重复了
                            {
                                for (int i = 0; i < hold; i++) { values.Add(output); }//加hold数量的samples
                                output += stepsize;//升到下一个level
                            }
                            output = signal.Maximum;//降序从最大值开始
                            for (int j = 0; j < levels; j++)
                            {
                                for (int i = 0; i < hold; i++) { values.Add(output); }//加hold数量的samples
                                output -= stepsize;//降到下一个level
                            }
                        }
                        break;
                    case ValueGeneratorType.Ramps_and_Pulses:
                        double level1 = canChannel.hex2Decimal(signal.valueGenerator_Settings["Level1"]);
                        double level2 = canChannel.hex2Decimal(signal.valueGenerator_Settings["Level2"]);
                        double stepsizeRise = (level2 - level1)/(uint.Parse(signal.valueGenerator_Settings["Rise"]) + 1);
                        double stepsizeFall = (level2 - level1) / (uint.Parse(signal.valueGenerator_Settings["Fall"]) + 1);
                        //1.level1保持pre帧
                        output = level1;
                        for (int i = 0; i < uint.Parse(signal.valueGenerator_Settings["PreDelay"]); i++) { values.Add(output); }
                        //2.level1到level2有rise帧
                        for (int i = 0; i < uint.Parse(signal.valueGenerator_Settings["Rise"]); i++) 
                        {
                            output+= stepsizeRise;
                            values.Add(output);
                        }
                        //3.level2保持hold帧
                        output = level2;
                        for (int i = 0; i < uint.Parse(signal.valueGenerator_Settings["Hold"]); i++) { values.Add(output); }
                        //4.level2到level1有fall帧
                        for (int i = 0; i < uint.Parse(signal.valueGenerator_Settings["Fall"]); i++)
                        {
                            output -= stepsizeFall;
                            values.Add(output);
                        }
                        //5.level1保持PostDelay帧
                        output = level1;
                        for (int i = 0; i < uint.Parse(signal.valueGenerator_Settings["PostDelay"]); i++) { values.Add(output); }
                        break;
                    case ValueGeneratorType.Random:
                        values.Add(canChannel.hex2Decimal(signal.valueGenerator_Settings["Min"]));
                        values.Add(canChannel.hex2Decimal(signal.valueGenerator_Settings["Max"]));
                        break;
                    default:
                        return;
                }
                if (values.Count > 0) { signal.valueGeneratorList = values; };
            
        }

        
        #endregion

        #region calculate crc
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
        #endregion

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
    public class CAN_Signal : ICAN_Signal
    {
	    #region Rolling
        public double Rolling_Step_Frac = 0;//上一轮rolling step累加完整数，剩下的的小数部分
        public int rolling_Step_samples = 1;//同一个值保持几帧
        public int rolling_Step_samples_index = 0;//同一个值保持几帧功能，发到第几帧了。
        public double Rolling_all=0.0;//raw value
        #endregion

        //Value generator param
        public ValueGeneratorType value_generator_type=new ValueGeneratorType();
        public Dictionary<string,string> valueGenerator_Settings=new Dictionary<string, string>();//eg.key="Predelay",value="1"
        public List<double> valueGeneratorList = new List<double> { };
        public int valueGeneratorListIndex = 0;
        public bool isValueGenerateActive = false;
        public string GUID = ""; //special for Flexray signal inner use.
        public int UpdateBit = -1; //special for Flexray signal inner use.
        public string UB_SignalName = "";  //special for Flexray signal inner use.
        public string SignalGroup_Name = "";  //special for Flexray signal inner use.

        //value table
        public List<string> valueTable = new List<string>();


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

        private string _longName = "";
        public string longName
        {
            set
            {
                _longName = value;
                OnPropertyChanged("longName");
            }
            get
            {
                return _longName;
            }
        }


        private int _StartBit = 0;
        public int StartBit
        {
            set
            {
                _StartBit = value;
                OnPropertyChanged("StartBit");
            }
            get
            {
                return _StartBit;
            }
        }

        private int _Length = 0;
        public int Length
        {
            set
            {
                _Length = value;
                OnPropertyChanged("Length");
            }
            get
            {
                return _Length;
            }
        }

        private double _Factor = 1;
        public double Factor
        {
            set
            {
                _Factor = value;
                OnPropertyChanged("Factor");
            }
            get
            {
                return _Factor;
            }
        }

        private double _Offset = 0;
        public double Offset
        {
            set
            {
                _Offset = value;
                OnPropertyChanged("Offset");
            }
            get
            {
                return _Offset;
            }
        }

        private double _Init_Value = 0;//physical value
        public double Init_Value
        {
            set
            {
                _Init_Value = value;
                OnPropertyChanged("Init_Value");
            }
            get
            {
                return _Init_Value;
            }
        }

        private double _Minium = 0;//physical value
        public double Minimum
        {
            set
            {
                _Minium = value;
                OnPropertyChanged("Minimum");
            }
            get
            {
                return _Minium;
            }
        }

        private double _Maximum = 0;//physical value 信号有效值的最大值
        public double Maximum
        {
            set
            {
                _Maximum = value;
                OnPropertyChanged("Maximum");
            }
            get
            {
                return _Maximum;
            }
        }

        private double _Min_Value = 0;
        public double Min_Value
        {
            get { return _Min_Value; }
            set {
                _Min_Value = value;
                OnPropertyChanged("Min_Value");
            }
        }

        private double _Max_Value = 0;
        public double Max_Value
        {
            get { return _Max_Value; }
            set {
                _Max_Value = value;
                OnPropertyChanged("Max_Value");
            }
        }

        private int _ByteOrder = 0;
        public int ByteOrder//0:Motorola|1:Intel
        {
            set
            {
                _ByteOrder = value;
                OnPropertyChanged("ByteOrder");
            }
            get
            {
                return _ByteOrder;
            }
        }

        private int _ValueType = 1;
        public int ValueType //0:Signed|1:Unsigned
        {
            set
            {
                _ValueType = value;
                OnPropertyChanged("ValueType");
            }
            get
            {
                return _ValueType;
            }
        }

        private string _Unit = "";
        public string Unit
        {
            set
            {
                _Unit = value;
                OnPropertyChanged("Unit");
            }
            get
            {
                return _Unit;
            }
        }

        private double _Value = 0;//physical value
        public double Value
        {
            set
            {
                if (value > Max_Value)
                {
                    _Value = Max_Value;
                }
                else if (value < Min_Value)
                {
                    _Value = Min_Value;
                }
                else
                {
                    _Value = value;
                }
                OnPropertyChanged("Value");
            }
            get
            {
                return _Value;
            }
        }

        private double _NewValue = -1;//for Flexray E2E 
        public double NewValue
        {
            set
            {
                _NewValue = value;
                OnPropertyChanged("NewValue");
            }
            get
            {
                return _NewValue;
            }
        }

        private bool _isRolling;
        public bool isRolling
        {
            get { return _isRolling; }
            set { _isRolling = value; }
        }

        private double _Rolling_Step;
        public double Rolling_Step
        {
            get { return _Rolling_Step; }
            set { _Rolling_Step = value; }
        }

        private double _Rolling_MaxValue;
        public double Rolling_MaxValue
        {
            get { return _Rolling_MaxValue; }
            set { _Rolling_MaxValue = value; }
        }

        private double _Rolling_MinValue;
        public double Rolling_MinValue
        {
            get { return _Rolling_MinValue; }
            set { _Rolling_MinValue = value; }
        }

        private bool _is_E2E_Checksum;
        public bool is_E2E_Checksum
        {
            get { return _is_E2E_Checksum; }
            set { _is_E2E_Checksum = value; }
        }

        private bool _is_E2E_RollingCount;
        public bool is_E2E_RollingCount
        {
            get { return _is_E2E_RollingCount; }
            set { _is_E2E_RollingCount = value; }
        }

        private double _E2E_RollingCount = 0;
        public double E2E_RollingCount
        {
            get { return _E2E_RollingCount; }
            set { _E2E_RollingCount = value; }
        }

        private bool _is_E2E_ReservedBit_Fill_One;
        public bool is_E2E_ReservedBit_Fill_One
        {
            get { return _is_E2E_ReservedBit_Fill_One; }
            set { _is_E2E_ReservedBit_Fill_One = value; }
        }

        private bool _is_E2E_DataID_CRC_Done;
        public bool is_E2E_DataID_CRC_Done
        {
            get { return _is_E2E_DataID_CRC_Done; }
            set { _is_E2E_DataID_CRC_Done = value; }
        }

        private int _E2E_StartByte;
        public int E2E_StartByte
        {
            get { return _E2E_StartByte; }
            set { _E2E_StartByte = value; }
        }

        private int _E2E_EndByte;
        public int E2E_EndByte
        {
            get { return _E2E_EndByte; }
            set { _E2E_EndByte = value; }
        }

        private int _E2E_Method;
        public int E2E_Method
        {
            get { return _E2E_Method; }
            set { _E2E_Method = value; }
        }

        private byte _E2E_DataID_HByte;
        public byte E2E_DataID_HByte
        {
            get { return _E2E_DataID_HByte; }
            set { _E2E_DataID_HByte = value; }
        }

        private byte _E2E_DataID_LByte;
        public byte E2E_DataID_LByte
        {
            get { return _E2E_DataID_LByte; }
            set { _E2E_DataID_LByte = value; }
        }

        private byte _E2E_DataID_CRC_Value;
        public byte E2E_DataID_CRC_Value
        {
            get { return _E2E_DataID_CRC_Value; }
            set { _E2E_DataID_CRC_Value = value; }
        }

        private double _olderValue = 0;
        public double olderValue
        {
            set
            {
                _olderValue = value;
                OnPropertyChanged("olderValue");
            }
            get
            {
                return _olderValue;
            }
        }

        public List<CANSignalChangeInfo> _valueChangeList = new List<CANSignalChangeInfo>();
        public List<CANSignalChangeInfo> valueChangeList
        {
            get { return _valueChangeList; }
            set
            {
                _valueChangeList = value;
                OnPropertyChanged("valueChangeList");
            }
        }


        private List<SignalValueRecord> _valueRecordList = new List<SignalValueRecord>();
        public List<SignalValueRecord> valueRecordList
        {
            get { return _valueRecordList; }
            set
            {
                _valueRecordList = value;
                OnPropertyChanged("valueRecordList");
            }
        }

        /// <summary>
        /// CAN ID which is this signal belongs to
        /// </summary>
        public uint CAN_ID_Ref = 0;
        public uint ValueChangeStep = 0;
        public uint ValueUpdateDataStep = 0;
        public string name_ref = "";//arxml ref name

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
    public class CAN_Message_sampleType : INotifyPropertyChanged
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

        private uint _DLC = 8;
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

        public CAN_Message_sampleType(DateTime time, TxRxType txrx, uint id, uint dlc, byte[] data)
        {
            AbsTime = time;
            TxRx = txrx;
            ID = id;
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

    [Serializable]
    public class CANSignalChangeInfo
    {
        public DateTime Abstime = DateTime.Now;
        public double olderValue = 0;
        public double currentVaule = 0;
        public CANSignalChangeInfo(DateTime time, double value_older, double value_new)
        {
            Abstime = time;
            olderValue = value_older;
            currentVaule = value_new;
        }
    }

    [Serializable]
    public class SignalValueRecord
    {
        public long absTime = DateTime.Now.Ticks / 10000;
        public double Value = 0;
        public SignalValueRecord(double value)
        {
            absTime = DateTime.Now.Ticks / 10000;
            Value = value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CAN_Library.CAN_Parameter;
using CAN_Library.CAN_Interface;
using System.ComponentModel;

namespace CAN_Library.CAN_Driver
{
    abstract class CAN_Driver : IDisposable
    {
        protected const string AppName = "ITV-ATDevelopTeam";
        public delegate void MessageDataCallBack(Pmessage Msg);
        public event MessageDataCallBack MessageEvent;

        protected abstract bool DoInitialize(CAN_Parameter can_param);
        protected abstract bool DoUninitialize();
        protected abstract bool DoSend(Pmessage msg);
        protected abstract bool DoSetBaudrate(CAN_Parameter can_param);

        public void Dispose()
        {
            Log.Logger_Run("1.4.1-CAN_Driver(): Dispose()");
            Uninitialize();
        }

        ~CAN_Driver()
        {
            Log.Logger_Run("1.4.1-CAN_Driver(): ~CAN_Driver()");
            //this.Dispose();
        }

        public bool Initialize(CAN_Parameter can_param)
        {
            return this.DoInitialize(can_param);
        }

        public bool Uninitialize()
        {
            Log.Logger_Run("1.4.1-CAN_Driver(): Uninitialize()");
            return this.DoUninitialize();
        }

        public bool SetBaudrate(CAN_Parameter can_param)
        {
            return this.DoSetBaudrate(can_param);
        }

        public bool SendMessage(Pmessage msg)
        {
            return this.DoSend(msg);
        }

        protected void NotifyMessageReceived(Pmessage Msg)
        {
            if (MessageEvent != null)
            {
                if (Msg.Data != null)
                {
                    if (Msg.DLC < Msg.Data.Length)
                        Msg.Data = Msg.Data.Take(Msg.DLC).ToArray();
                }
                this.MessageEvent(Msg);
            }
        }
    }

    class CANDriverFactory
    {
        public static CAN_Driver FilterDriver(Device_Type device_type, string device_name, byte channel_id, CAN_Parameter can_param)
        {
            try
            {
                CAN_Driver Result = null;
                switch (device_type)
                {
                    case Device_Type.Saint2:
                        Result = new CAN_Driver_Saint(device_name, channel_id, can_param);
                        break;
                    case Device_Type.Saint3:
                        Result = new CAN_Driver_Saint(device_name, channel_id, can_param);
                        break;
                    case Device_Type.Vector:
                        Result = new CAN_Driver_Vector(device_name, channel_id, can_param);
                        break;
                    case Device_Type.Spy3:
                        Result = new CAN_Driver_Spy3(device_name, channel_id, can_param);
                        break;
                    case Device_Type.ZLG:
                        Result = new CAN_Driver_ZLG(device_name, channel_id, can_param);
                        break;
                    case Device_Type.Sapa:
                        Result = new CAN_Driver_Sapa(device_name, channel_id, can_param);
                        break;
                    default:
                        break;
                }
                return Result;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.4.1-FilterDriver() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return null;
            }
        }
    }

    public class CAN_Parameter : INotifyPropertyChanged
    {
        ///channel physic wire type, forbid user to change value
        public readonly CANBus_PhysicType BusPhysicType;
        ///forbid user to change value
        public readonly bool IsFDSupport;

        ///channel protocol mode
        CANBus_ProtocolMode _BusProtocolMode;
        public CANBus_ProtocolMode BusProtocolMode
        {
            get { return _BusProtocolMode; }
            set { _BusProtocolMode = value; OnPropertyChanged("BusProtocolMode"); }
        }

        ///CAN 2.0B baud rate
        CanBaudRate _BaudRate;
        public CanBaudRate BaudRate
        {
            get { return _BaudRate; }
            set { _BaudRate = value; OnPropertyChanged("BaudRate"); }
        }

        //device clock
        ClockFrequency _F_clock;
        public ClockFrequency F_clock
        {
            get { return _F_clock; }
            set { _F_clock = value; OnPropertyChanged("F_clock"); }
        }

        //CAN FD arbitration phase rate
        CanBaudRate _Arbitration_rate = CanBaudRate.Unknown;
        public CanBaudRate Arbitration_rate
        {
            get {  return _Arbitration_rate; }
            set { _Arbitration_rate = value; OnPropertyChanged("Arbitration_rate"); }
        }

        byte _Abr_brp = 0xff;
        public byte Abr_brp
        {
            get { return _Abr_brp; }
            set { _Abr_brp = value; OnPropertyChanged("Abr_brp"); }
        }

        byte _Abr_tseg1 = 0xff;
        public byte Abr_tseg1
        {
            get { return _Abr_tseg1; }
            set { _Abr_tseg1 = value; OnPropertyChanged("Abr_tseg1"); }
        }

        byte _Abr_tseg2 = 0xff;
        public byte Abr_tseg2
        {
            get { return _Abr_tseg2; }
            set { _Abr_tseg2 = value; OnPropertyChanged("Abr_tseg2"); }
        }


        byte _Abr_sjw = 0xff;
        public byte Abr_sjw
        {
            get { return _Abr_sjw; }
            set { _Abr_sjw = value; OnPropertyChanged("Abr_sjw"); }
        }

        //CAN FD data phase rate
        CanBaudRate _Data_rate = CanBaudRate.Unknown;
        public CanBaudRate Data_rate
        {
            get { return _Data_rate; }
            set { _Data_rate = value; OnPropertyChanged("Data_rate"); }
        }

        byte _Data_brp = 0xff;
        public byte Data_brp
        {
            get { return _Data_brp; }
            set { _Data_brp = value; OnPropertyChanged("Data_brp"); }
        }

        byte _Data_tseg1 = 0xff;
        public byte Data_tseg1
        {
            get { return _Data_tseg1; }
            set { _Data_tseg1 = value; OnPropertyChanged("Data_tseg1"); }
        }

        byte _Data_tseg2 = 0xff;
        public byte Data_tseg2
        {
            get { return _Data_tseg2; }
            set { _Data_tseg2 = value; OnPropertyChanged("Data_tseg2"); }
        }

        public byte _Data_sjw = 0xff;
        public byte Data_sjw
        {
            get { return _Data_sjw; }
            set { _Data_sjw = value; OnPropertyChanged("Data_sjw"); }
        }

        //public bool BRS = true;//Bit rate switch

        public CAN_Parameter()
        {
            BusProtocolMode = CANBus_ProtocolMode.CAN;
            BaudRate = CanBaudRate.Unknown;
            F_clock = ClockFrequency.Freq_60MHZ;
            Arbitration_rate = CanBaudRate.Unknown;
            Abr_brp = 0xff;
            Abr_tseg1 = 0xff;
            Abr_tseg2 = 0xff;
            Abr_sjw = 0xff;
            Data_rate = CanBaudRate.Unknown;
            Data_brp = 0xff;
            Data_tseg1 = 0xff;
            Data_tseg2 = 0xff;
            Data_sjw = 0xff;
        }

        internal CAN_Parameter(CANBus_PhysicType type, bool fdSupport)
        {
            BusPhysicType = type;
            IsFDSupport = fdSupport;            
        }

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
    }

    public enum Device_Type
    {
        Saint2,
        Saint3,
        Vector,
        Spy3,
        ZLG,
        Sapa,
        none,
    }

    public enum CANBus_PhysicType
    {
        singleWire,
        lowSpeed,
        midSpeed,
        highSpeed,
        anyType,
    }

    public enum CANBus_ProtocolMode
    {
        CAN,
        ISO_CAN_FD,
        NON_ISO_CAN_FD,
    }

    public enum CanBaudRate
    {
        Unknown,
        Baud_33Kbit,//33333
        Baud_83Kbit,//83333
        Baud_100Kbit,
        Baud_125Kbit,
        Baud_250Kbit,
        Baud_500Kbit,
        Baud_1Mbit,
        Baud_2Mbit,
        Baud_4Mbit,
        Baud_5Mbit,
    }

    public enum ClockFrequency
    {
        Unknown,
        Freq_20MHZ = 20,
        Freq_24MHZ = 24,
        Freq_30MHZ = 30,
        Freq_40MHZ = 40,
        Freq_60MHZ = 60,
        Freq_80MHZ = 80,
    }
}

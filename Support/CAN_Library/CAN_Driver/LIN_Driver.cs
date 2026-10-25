using CAN_Library.CAN_Interface;
using CAN_Library.CAN_Parameter;
using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace CAN_Library.CAN_Driver
{
    abstract class LINDriver : IDisposable
    {
        public const string AppName = "ITV-ATDevelopTeam";
        internal LIN_Verison LINVersion = LIN_Verison.LIN_2_1;
        internal LINBaudRate BaudRate = LINBaudRate.Baud_19200;
        public delegate void LINMessageDataBack(Lmessage Msg);
        public event LINMessageDataBack MessageEvent;

        protected abstract bool DoInitialize(LIN_Parameter lin_param);
        protected abstract bool DoUninitialize();
        protected abstract bool DoSend(LIN_Message Msg);
        protected abstract bool DoSetBaudrate(LINBaudRate Rate = LINBaudRate.Baud_19200);

        protected abstract void DoClearPID(byte PID);
        protected abstract bool DoSet_Lin_Slavers(List<LIN_Node> slaves);
        protected abstract void DoSet_Lin_Slavers();
        protected abstract void DoSet_Lin_Master();
        protected abstract void DoSet_AutoChecksum_Mode();
        protected abstract void DoSet_Not_AutoChecksum_Mode();

        public bool Initialize(LIN_Parameter lin_param)
        {
            return this.DoInitialize(lin_param);
        }

        public bool Uninitialize()
        {
            Log.Logger_Run("1.4.1-LINDriver(): Uninitialize()");
            return this.DoUninitialize();
        }

        public bool Send(LIN_Message Msg)
        {
            if (Msg == null)
            {
                return false;
            }
            else
            {
                return this.DoSend(Msg);
            }
        }

        protected void NotifyMessageReceived(Lmessage Msg)
        {
            if (MessageEvent != null)
            {
                this.MessageEvent(Msg);
            }
        }

        internal void Clear_PID(byte PID)
        {
            DoClearPID(PID);
        }

        ~LINDriver()
        {
            Log.Logger_Run("1.4.1-LINDriver(): ~LINDriver()");
            //this.Dispose();
        }

        public void Dispose()
        {
            try
            {
                Log.Logger_Run("1.4.1-LINDriver(): Dispose()");
                this.Uninitialize();
            }
            catch
            {
            }
        }

        public bool LIN_Set_Slavers(List<LIN_Node> slaves)
        {
            return DoSet_Lin_Slavers(slaves);
        }

        public void LIN_Set_Slavers()
        {
            DoSet_Lin_Slavers();
        }

        public void LIN_Set_Master()
        {
            DoSet_Lin_Master();
        }

        public void LIN_AutoChecksum_Mode()
        {
            DoSet_AutoChecksum_Mode();
        }

        public void LIN_Not_AutoChecksum()
        {
            DoSet_Not_AutoChecksum_Mode();
        }
    }

    public class LIN_Parameter : INotifyPropertyChanged
    {
        private LINBaudRate _Baudrate = LINBaudRate.Baud_19200;
        public LINBaudRate Baudrate
        {
            set
            {
                _Baudrate = value;
                OnPropertyChanged("Baudrate");
            }
            get
            {
                return _Baudrate;
            }
        }

        private LIN_Verison _LINversion = LIN_Verison.LIN_2_1;
        public LIN_Verison LINversion
        {
            set
            {
                _LINversion = value;
                OnPropertyChanged("LINversion");
            }
            get
            {
                return _LINversion;
            }
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

    public enum LINBaudRate
    {
        Baud_2400 = 0,
        Baud_9600 = 1,
        Baud_10400 = 2,
        Baud_19200 = 3,
        Baud_20000 = 4,
        Baud_10417 = 5,
    }

    public enum ModeType
    {
        Slaver = 0,
        Master = 1,
    }

    public enum LIN_Verison
    {
        LIN_1_3,//LIN 1.3
        LIN_2_0,//LIN 2.0
        LIN_2_1,//LIN 2.1
        J2602_0_0,//SAE J2602 0.0
        J2602_1_2,//SAE J2602 1.2
        J2602_1_1,
    }

    class LINDriverFactory
    {
        public static LINDriver FilterDevice(Device_Type device_type, string device_name, byte channel_id, LIN_Parameter lin_param)
        {
            LINDriver Result = null;
            switch (device_type)
            {
                case Device_Type.Saint2:
                    Result = new LINDriver_Saint(device_name, channel_id, lin_param);
                    break;
                case Device_Type.Saint3:
                    Result = new LINDriver_Saint(device_name, channel_id, lin_param);
                    break;
                case Device_Type.Vector:
                    Result = new LINDriver_Vector(device_name, channel_id, lin_param);
                    break;
                case Device_Type.Spy3:
                    Result = new LINDriver_Spy3(device_name, channel_id, lin_param);
                    break;
                case Device_Type.Sapa:
                    Result = new LINDriver_Sapa(device_name, channel_id, lin_param);
                    break;
                default:
                    break;
            }
            return Result;
        }
    }

}

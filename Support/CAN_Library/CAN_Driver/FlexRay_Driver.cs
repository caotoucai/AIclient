using CAN_Library.CAN_Parameter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using CAN_Library.CAN_Interface;

namespace CAN_Library.CAN_Driver
{
    abstract class FlexRay_Driver : IDisposable
    {
        public const string AppName = "ITV-ATDevelopTeam";
        public delegate void FlexRayMessageDataBack(Fmessage Msg);
        public event FlexRayMessageDataBack MessageEvent;
        protected abstract bool DoInitialize(string flbex_file);
        protected abstract bool DoUninitialize();
        protected abstract bool DoSend(FlexRay_Message Msg);
        protected abstract bool DoSendRawData(FlexRay_Message Msg);
        protected abstract bool DoChangeSignalAndSend(FlexRay_Message Msg, string signal_name, long raw_value);

        protected abstract bool DoPause_FlexRay_Cyclic_Message(FlexRay_Message Msg);

        //protected abstract bool DoSend_WUP();
        protected abstract void DoInitStartupFrames(FlexRay_Message ErayMsg, FlexRay_Message ColdMsg);


        public bool Initialize(string flbex_file)
        {
            return this.DoInitialize(flbex_file);
        }

        public bool Uninitialize()
        {
            Log.Logger_Run("1.4.1-FlexRay_Driver(): Uninitialize()");
            return this.DoUninitialize();
        }

        public bool Send(FlexRay_Message Msg)
        {
             return this.DoSend(Msg);
        }

        public bool SendRawData(FlexRay_Message Msg)
        {
            return this.DoSendRawData(Msg);
        }

        public bool ChangeSignalAndSend(FlexRay_Message Msg, string signal_name, long raw_value)
        {
            return this.DoChangeSignalAndSend(Msg, signal_name, raw_value);
        }

        public bool Pause_FlexRay_Cyclic_Message(FlexRay_Message Msg)
        {
            return this.DoPause_FlexRay_Cyclic_Message(Msg);
        }

        public void InitStartupFrames(FlexRay_Message ErayMsg, FlexRay_Message ColdMsg)
        {
            this.DoInitStartupFrames(ErayMsg, ColdMsg);
        }

        protected void NotifyMessageReceived(Fmessage Msg)
        {
            if (MessageEvent != null)
            {
                this.MessageEvent(Msg);
            }
        }


        ~FlexRay_Driver()
        {
            Log.Logger_Run("1.4.1-FlexRay_Driver(): ~FlexRay_Driver()");
            //this.Dispose();
        }

        public void Dispose()
        {
            try
            {
                Log.Logger_Run("1.4.1-FlexRay_Driver(): Dispose()");
                this.Uninitialize();
            }
            catch
            {
            }
        }
    }

    public class FlexRay_Parameter : INotifyPropertyChanged
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
    }



    class FlexRayDriverFactory
    {
        public static FlexRay_Driver FilterDevice(Device_Type device_type, string device_name, byte channel_id, FlexRay_Parameter flex_param)
        {
            FlexRay_Driver Result = null;
            switch (device_type)
            {
                case Device_Type.Vector:
                    Result = new FlexRay_Driver_Vector(device_name, channel_id, flex_param);
                    break;
                case Device_Type.Sapa:
                    Result = new FlexRay_Driver_Sapa(device_name, channel_id, flex_param);
                    break;
                default:
                    break;
            }
            return Result;
        }
    }


    public class CodingStrcture
    {
        public uint bit_length { set; get; }
        public double offset { set; get; }
        public double factor { set; get; }
        public double minimum { set; get; }
        public double maximum { set; get; }
    }

    public class ScheduleTimingStrcture
    {
        public uint slot_id { set; get; }
        public uint offset { set; get; }
        public uint repetition { set; get; }
        public string frame_trigger_id { set; get; }
        //public string ecu  { set; get; }
        public TxRxType txrx { set; get; }
    }

    public enum ECUName
    {
        ASDM,
        BBM,
        CEM,
        ETC,
        IHU,
        SRS,
        VDDM,
        VGM
    }
}

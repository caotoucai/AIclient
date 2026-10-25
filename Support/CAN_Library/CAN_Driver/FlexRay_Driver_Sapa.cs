using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using vxlapi_NET;
using static vxlapi_NET.XLDefine;
using System.Runtime.InteropServices;
using CAN_Library.CAN_Interface;
using CAN_Library.CAN_Parameter;
using System.Diagnostics;
using Fibex_Parser;
using SapaWraper;
using System.Runtime.ExceptionServices;
using System.Security;

namespace CAN_Library.CAN_Driver
{
    class FlexRay_Driver_Sapa : FlexRay_Driver
    {
        SAPA_Device _myDevice;
        bool _driver_hasError = false;//device comes into a exception
        bool _driver_hasInit = false;
        Thread _rxThread;//to get standard messages in FR bus
        bool _isActive_RxThread;//to control RxThread

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WaitForSingleObject(int handle, int timeOut);

        public FlexRay_Driver_Sapa(string deviceName, byte channelID, FlexRay_Parameter flex_param)
        {
            try
            {
                if (CAN_Manager.SAPA_Device_List.Count > 0)
                {
                    if (CAN_Manager.SAPA_Device_List.Any(p => p.Display_Name == deviceName))
                    {
                        _myDevice = CAN_Manager.SAPA_Device_List.First(p => p.Display_Name == deviceName);
                    }
                    else
                    {
                        _driver_hasError = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("FlexRay_Driver_Sapa -- FlexRay_Driver_Sapa() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        protected override bool DoInitialize(string flbex_file)
        {
            Log.Logger_Run(string.Format("FlexRay_Driver_Sapa -- DoInitialize"));
            int r = 0;
            try
            {
                ///make sure driver is ok and device has not lost
                if (this._driver_hasError)
                {
                    Log.Logger_Run(string.Format("FlexRay_Driver_Sapa -- DoInitialize: driver_hasError"));
                    return false;
                }

                if (!CAN_Manager.SapaConnect.Load_fibex(flbex_file))
                    return false;

                if (!this._myDevice.IsOpen)
                {
                    if (!CAN_Manager.SapaConnect.OpenDevice(_myDevice.Name))
                    {
                        return false;
                    }                    
                }
                _myDevice.IsOpen = true;

                if (!this._driver_hasInit)
                {
                    CAN_Manager.SapaConnect.OnRecvFlexrayDataEvent += OnMessageRecvFuncton;
                    _driver_hasInit = true;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("FlexRay_Driver_Sapa -- DoInitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        private void OnMessageRecvFuncton(SAPAConnection.XL_FLEXRAY_FRAME_MESSAGE recv_msg)
        {
            Fmessage New_Received = new Fmessage();
            New_Received.FrameID = recv_msg.slotId;

            New_Received.CycleCount = recv_msg.cycleCount;
            byte[] rv_data = new byte[recv_msg.dataLength];
            Marshal.Copy(recv_msg.data, rv_data, 0, recv_msg.dataLength);

            New_Received.Data = rv_data;
            New_Received.DLC = (uint)recv_msg.dataLength;
            New_Received.TxRx = recv_msg.tx == 1 ? TxRxType.Tx : TxRxType.Rx;
            New_Received.LastReceivedTimeMs = (long)Convert.ToUInt64(DateTime.Now.Ticks / 10000L);
            base.NotifyMessageReceived(New_Received);
        }


        protected override bool DoUninitialize()
        {
            Log.Logger_Run(string.Format("FlexRay_Driver_Sapa -- DoUninitialize"));
            try
            {
                if (_driver_hasInit)
                {                   
                    _driver_hasInit = false;
                    CAN_Manager.SapaConnect.OnRecvFlexrayDataEvent -= OnMessageRecvFuncton;
                }

                //int r = SapaNativeWraper.CloseDevice(_myDevice.Name);
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("FlexRay_Driver_Sapa -- DoUninitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override void DoInitStartupFrames(FlexRay_Message ErayMsg, FlexRay_Message ColdMsg)
        {           
        }

        protected override bool DoSend(FlexRay_Message msg)
        {
            try
            {
                if (_driver_hasError)
                    return false;
                if (!_driver_hasInit)
                    return false;

                if (msg == null)
                {
                    return false;
                }

                bool r = Send_FR_Msg(msg);
                return r;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("FlexRay_Driver_Sapa -- DoSend() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override bool DoSendRawData(FlexRay_Message Msg)
        {
            SAPAConnection.XL_FLEXRAY_FRAME_MESSAGE fr_msg = new SAPAConnection.XL_FLEXRAY_FRAME_MESSAGE();
            int fr_size = Marshal.SizeOf(typeof(SAPAConnection.XL_FLEXRAY_FRAME_MESSAGE));
            fr_msg.slotId = Msg.ID;
            fr_msg.offset = (byte)Msg.OffSet;
            fr_msg.repetition = (byte)Msg.Repetition;
            fr_msg.dataLength = (int)Msg.DLC;
            fr_msg.txMode = (byte)(Msg.Method == FR_TX_Mode.Cyclic ? 1:0);  //0:event  1:time trigger

            IntPtr unmanagedPtr = Marshal.AllocHGlobal((int)Msg.DLC);
            Marshal.Copy(Msg.Data, 0, unmanagedPtr, (int)Msg.DLC);
            fr_msg.data = unmanagedPtr;

            int r = CAN_Manager.SapaConnect.Send_fr_raw_data(_myDevice.Name, fr_msg);
            Msg.RXIndex =(uint)r;
            Marshal.FreeHGlobal(unmanagedPtr);
            unmanagedPtr = IntPtr.Zero;
            return true;
        }

        protected override bool DoChangeSignalAndSend(FlexRay_Message Msg, string signal_name, long raw_value)
        {
            bool r = CAN_Manager.SapaConnect.Change_fr_signal_raw_value(_myDevice.Name, Msg.Name, signal_name, raw_value);  //can only delete spxl_send_set_fr_command message
            if (!r)
                return false;

            r = CAN_Manager.SapaConnect.Send_set_fr_command( _myDevice.Name, Msg.Name);            
            return r;
        }

        protected override bool DoPause_FlexRay_Cyclic_Message(FlexRay_Message msg)
        {
            int r = 0;
            if (msg.RXIndex != 0)
			{				
				int fr_size = Marshal.SizeOf(typeof(SAPAConnection.XL_FLEXRAY_FRAME_MESSAGE));
                SAPAConnection.XL_FLEXRAY_FRAME_MESSAGE fr_msg = new SAPAConnection.XL_FLEXRAY_FRAME_MESSAGE();
                fr_msg.slotId = msg.ID;
                fr_msg.offset = (byte)msg.OffSet;
                fr_msg.repetition = (byte)msg.Repetition;
                fr_msg.dataLength = (byte)msg.DLC;
                fr_msg.txMode = (byte)(msg.Method == FR_TX_Mode.Cyclic ? 1 : 0);  //0:event  1:time trigger
                fr_msg.index = (ushort)msg.RXIndex;
				r = CAN_Manager.SapaConnect.Send_del_fr_raw_data(_myDevice.Name, fr_msg);  //can only delete spxl_send_fr_msg_command message
            }
			else
			{
				bool ret = CAN_Manager.SapaConnect.Send_del_fr_command(_myDevice.Name, msg.Name);  //can only delete spxl_send_set_fr_command message
                return ret;
            }
			return true;
        }


        private bool Send_FR_Msg(FlexRay_Message msg)
        {
            bool r = CAN_Manager.SapaConnect.Send_set_fr_command(_myDevice.Name, msg.Name);
            return r;
        }        
    }
}

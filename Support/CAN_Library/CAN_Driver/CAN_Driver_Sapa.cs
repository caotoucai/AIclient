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
    class CAN_Driver_Sapa : CAN_Driver
    {
        SAPA_Device _myDevice;
        byte _channel_ID;
        CAN_Parameter _can_param;
        bool _driver_hasError = false;///channel driver comes into a exception
        bool _driver_hasInit = false;///channel driver is on open or not
        bool _sendByHighVol = false;

        public CAN_Driver_Sapa(string deviceName, byte channelID, CAN_Parameter can_param)
        {
            try
            {
                if (CAN_Manager.SAPA_Device_List.Any(p => p.Display_Name == deviceName))
                {
                    this._myDevice = CAN_Manager.SAPA_Device_List.First(p => p.Display_Name == deviceName);
                    this._channel_ID = channelID;
                    this._can_param = can_param;
                    _driver_hasError = false;
                }
                else
                {
                    _driver_hasError = true;
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.1.1-CAN_Driver_Saint() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        protected override bool DoInitialize(CAN_Parameter target_can_param)
        {
            try
            {
                ///make sure driver is ok and device has not lost
                if (this._driver_hasError)
                {
                    return false;
                }
                ///one saint device connect only one time
                if (!this._myDevice.IsOpen)
                {
                    Log.Logger_Run("to connect sapa device");
                    if (!CAN_Manager.SapaConnect.OpenDevice(_myDevice.Name))
                    {
                        return false;
                    }
                }

                _myDevice.IsOpen = true;

                _can_param.BusProtocolMode = target_can_param.BusProtocolMode;
                CAN_Manager.SapaConnect.Enable_CAN(_myDevice.Name, _channel_ID);

                if (!DoSetBaudrate(target_can_param))
                { return false; }
                
                if (!this._driver_hasInit)
                {
                    CAN_Manager.SapaConnect.OnRecvCanDataEvent += OnMessageRecvFuncton;
                    _driver_hasInit = true;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.1.2-DoInitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override bool DoSetBaudrate(CAN_Parameter target_can_param)
        {
            if (target_can_param.BusProtocolMode == CANBus_ProtocolMode.ISO_CAN_FD)
            {
                long Arbitration_rate = 0;
                long Data_rate = 0;
                if (target_can_param.Arbitration_rate == CanBaudRate.Unknown)
                    Arbitration_rate = 500000;
                else
                    Arbitration_rate = Convert_Bandrate(target_can_param.Arbitration_rate);
                if (target_can_param.Data_rate == CanBaudRate.Unknown)
                    Data_rate = 2000000;
                else
                    Arbitration_rate = Convert_Bandrate(target_can_param.Data_rate);

                CAN_Manager.SapaConnect.set_canfd_options(_myDevice.Name, _channel_ID, 1, 1, Arbitration_rate, Data_rate);
            }
            else
            {
                long baudrate = 0;
                if (target_can_param.BaudRate == CanBaudRate.Unknown)
                    baudrate = 500000;
                else
                    baudrate = Convert_Bandrate(target_can_param.BaudRate);
                CAN_Manager.SapaConnect.set_can_baudrate(_myDevice.Name, _channel_ID, baudrate);
            }

            return true;
        }

        private void OnMessageRecvFuncton(SAPAConnection.XL_CAN_FRAME_MESSAGE recv_msg)
        {
            Pmessage New_Received = new Pmessage();
            New_Received.CANID = recv_msg.arbid;
            New_Received.DLC = (byte)recv_msg.datalength;
            byte[] rv_data = new byte[recv_msg.datalength];
            Marshal.Copy(recv_msg.data, rv_data, 0, recv_msg.datalength);
            New_Received.Data = rv_data;


            New_Received.TxRx = TxRxType.Tx;
            New_Received.SingleWireHighVoltage = false;

            if (_can_param.BusProtocolMode == CANBus_ProtocolMode.ISO_CAN_FD)
                New_Received.FDorNot = true;
            else
                New_Received.FDorNot = false;

            New_Received.LastReceivedTimeMs = Convert.ToUInt64(DateTime.Now.Ticks / 10000L);            
            base.NotifyMessageReceived(New_Received);
        }

        protected override bool DoSend(Pmessage msg)
        {
            try
            {
                if (_driver_hasError)
                    return false;
                if (!_driver_hasInit)
                    return false;
                       
                bool r = CAN_Manager.SapaConnect.Send_CAN_Message(_myDevice.Name, _channel_ID, msg.FDorNot, msg.CANID, msg.DLC, msg.Data);
                return r;
                
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.1.4-DoSend() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override bool DoUninitialize()
        {
            try
            {
                if (_driver_hasInit)
                {
                    _driver_hasInit = false;
                    CAN_Manager.SapaConnect.OnRecvCanDataEvent -= OnMessageRecvFuncton;
                }

                //int r = SapaNativeWraper.spxl_close(ref device_info);
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.1.5-DoUninitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        private uint Convert_Bandrate(CanBaudRate rate)
        {
            switch (rate)
            {
                case CanBaudRate.Baud_100Kbit: return 100000;
                case CanBaudRate.Baud_125Kbit: return 125000;
                case CanBaudRate.Baud_250Kbit: return 250000;
                case CanBaudRate.Baud_500Kbit: return 500000;
                case CanBaudRate.Baud_1Mbit: return 1000000;
                case CanBaudRate.Baud_2Mbit: return 2000000;
                case CanBaudRate.Baud_4Mbit: return 4000000;
                case CanBaudRate.Baud_5Mbit: return 5000000;
                default: return 500000;
            }
        }
    }
}

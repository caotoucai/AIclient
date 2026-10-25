using System;
using System.Linq;
using System.Threading;
using CAN_Library;
using System.Runtime.InteropServices;
using CAN_Library.CAN_Interface;
using CAN_Library.CAN_Parameter;
using System.Collections.Generic;

namespace CAN_Library.CAN_Driver
{
    class LINDriver_Saint : LINDriver
    {
        Saint_Device _myDevice;
        byte _channel_ID;
        LIN_Parameter _lin_param;
        bool _driver_hasError = false;
        bool _driver_hasInit = false;
        bool _autoChecksum = true;
        bool _isMaster = true;

        public LINDriver_Saint(string deviceName, byte channelID, LIN_Parameter lin_param)
        {
            try
            {
                if (CAN_Manager.Saint_Device_List.Any(p => p.Display_Name == deviceName))
                {
                    this._myDevice = CAN_Manager.Saint_Device_List.First(p => p.Display_Name == deviceName);
                    this._channel_ID = channelID;
                    this._lin_param = lin_param;
                    _driver_hasError = false;
                }
                else
                {
                    _driver_hasError = true;
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("Exception.message: " + ex.Message + "\r\n              " + "Exception.StackTrace:" + ex.StackTrace);
            }

        }

        protected override bool DoInitialize(LIN_Parameter target_lin_param)
        {
            try
            {
                Log.Logger_Run("\r\nfunction: Saint DoInitialize");

                if (this._driver_hasError)
                {
                    Log.Logger_Run("saint has error");
                    return false;
                }
                ///one saint device connect only one time
                if (!this._myDevice.IsOpen)
                {
                    Log.Logger_Run("to connect saint device");
                    if (CAN_Manager.SaintConnect.ConnectToDevice(_myDevice.Name) == 0xFF)
                    {
                        Log.Logger_Run("connect saint device fail");
                        return false;
                    }
                }

                _myDevice.IsOpen = true;
                //init message
                //SendMessage2Device(_myDevice.Device_ID, string.Format("08 A3 B8 50 58 C0 C8"));
                SendMessage2Device(_myDevice.Device_ID, string.Format("08 A3 B8 50 58"));
                //LIN version
                SetLINVersion(target_lin_param.LINversion);
                Log.Logger_Run("set LIN version ok");

                //LIN baudrate
                if (!DoSetBaudrate(target_lin_param.Baudrate))
                {
                    Log.Logger_Run("set LIN Baud rate fail");
                    return false;
                }
                Log.Logger_Run("set LIN Baud rate ok");
                if (!this._driver_hasInit)
                {
                    CAN_Manager.SaintLINMessageCallbackEvent += new CAN_Manager.SaintLINMessageCallback(LINMessageCallback) ;
                    _driver_hasInit = true;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("Exception.message: " + ex.Message + "\r\n              " + "Exception.StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        private void LINMessageCallback(int deviceID, byte channelID, Lmessage linMessage)
        {
            try
            {
                if (_driver_hasError)
                    return;
                if (_myDevice == null)
                    return;
                if (_myDevice.Device_ID != deviceID)
                    return;
                if (_channel_ID != channelID)
                    return;
                base.NotifyMessageReceived(linMessage);
            }
            catch (Exception ex)
            {
                Log.Logger_Run("Exception.message: " + ex.Message + "\r\n              " + "Exception.StackTrace:" + ex.StackTrace);
            }
        }

        protected override bool DoSend(LIN_Message Msg)
        {
            try
            {
                //msg PID
                if (Msg.FrameID == 0x0 || Msg.FrameID == 0xFF)
                {
                    return false;
                }
                Msg.CalculateChecksum();
                //message make
                string commandToSend = "";
                if (_isMaster)
                {
                    commandToSend = "B8" + Msg.FrameID.ToString("X2").ToUpper() + BitConverter.ToString(Msg.Data, 0, (int)Msg.Data.Length).Replace("-", "").ToUpper();
                    SendMessage2Device(_myDevice.Device_ID, commandToSend);
                }
                else
                {
                    commandToSend = "BC10"+Msg.FrameID.ToString("X2").ToUpper() + BitConverter.ToString(Msg.Data, 0, (int)Msg.Data.Length).Replace("-", "").ToUpper() + Msg.CheckSum.ToString("X");
                    SendMessage2Device(_myDevice.Device_ID, commandToSend);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("Exception.message: " + ex.Message + "\r\n              " + "Exception.StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override bool DoUninitialize()
        {
            try
            {
                CAN_Manager.SaintLINMessageCallbackEvent -= LINMessageCallback;
                _driver_hasInit = false;             
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("Exception.message: " + ex.Message + "\r\n              " + "Exception.StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override void DoClearPID(byte PID)
        {
            try
            {
                SendMessage2Device(_myDevice.Device_ID, "BC11"+PID.ToString("X"));
            }
            catch (Exception ex)
            {
                Log.Logger_Run("Exception.message: " + ex.Message + "\r\n              " + "Exception.StackTrace:" + ex.StackTrace);
            }
        }

        void SetLINVersion(LIN_Verison version)
        {
            try
            {
                switch (version)
                {
                    case LIN_Verison.LIN_1_3:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 01 13"));
                        break;
                    case LIN_Verison.LIN_2_0:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 01 20"));
                        break;
                    case LIN_Verison.LIN_2_1:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 01 21"));
                        break;
                    case LIN_Verison.J2602_0_0:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 01 80"));
                        break;
                    case LIN_Verison.J2602_1_2:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 01 92"));
                        break;
                    case LIN_Verison.J2602_1_1:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 01 91"));
                        break;
                    default:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 01 20"));
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("Exception.message: " + ex.Message + "\r\n              " + "Exception.StackTrace:" + ex.StackTrace);
            }
        }

        protected override bool DoSetBaudrate(LINBaudRate Rate = LINBaudRate.Baud_19200)
        {
            try
            {
                switch (Rate)
                {
                    case LINBaudRate.Baud_2400:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 02 00 24"));
                        break;
                    case LINBaudRate.Baud_9600:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 02 00 96"));
                        break;
                    case LINBaudRate.Baud_10400:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 02 01 04"));
                        break;
                    case LINBaudRate.Baud_19200:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 02 01 92"));
                        break;
                    case LINBaudRate.Baud_20000:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 02 02 00"));
                        break;
                    case LINBaudRate.Baud_10417:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 02 01 05"));
                        break;
                    default:
                        SendMessage2Device(_myDevice.Device_ID, string.Format("BC 02 01 04"));
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("Exception.message: " + ex.Message + "\r\n              " + "Exception.StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override bool DoSet_Lin_Slavers(List<LIN_Node> slaves)
        {
            _isMaster = false;
            return true;
        }

        protected override void DoSet_Lin_Slavers()
        {
            _isMaster = false;
        }

        protected override void DoSet_Lin_Master()
        {
            _isMaster = true;
        }

        protected override void DoSet_AutoChecksum_Mode()
        {
            SendMessage2Device(_myDevice.Device_ID, string.Format("BC 03 01"));
            _autoChecksum = true;
        }

        protected override void DoSet_Not_AutoChecksum_Mode()
        {
            SendMessage2Device(_myDevice.Device_ID, string.Format("BC 03 00"));
            _autoChecksum = false;
        }

        private bool SendMessage2Device(int deviceId, IntPtr pData, UInt32 length)
        {
            return CAN_Manager.SaintConnect.SendDataToDevice(deviceId, pData, length);
        }

        private bool SendMessage2Device(int deviceID, string message)
        {
            return CAN_Manager.SaintConnect.SendDataToDevice(deviceID, message);
        }
    }
}

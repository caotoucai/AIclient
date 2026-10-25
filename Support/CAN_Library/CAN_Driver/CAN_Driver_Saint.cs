using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CAN_Library.CAN_Interface;
using CAN_Library.CAN_Parameter;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CAN_Library.CAN_Driver
{
    class CAN_Driver_Saint : CAN_Driver
    {
        Saint_Device _myDevice;
        byte _channel_ID;
        CAN_Parameter _can_param;
        bool _driver_hasError = false;///channel driver comes into a exception
        bool _driver_hasInit = false;///channel driver is on open or not
        bool _sendByHighVol = false;

        public CAN_Driver_Saint(string deviceName, byte channelID, CAN_Parameter can_param)
        {
            try
            {
                if (CAN_Manager.Saint_Device_List.Any(p => p.Display_Name == deviceName))
                {
                    this._myDevice = CAN_Manager.Saint_Device_List.First(p => p.Display_Name == deviceName);
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
                    if (CAN_Manager.SaintConnect.ConnectToDevice(_myDevice.Name) == 0xFF)
                    {
                        return false;
                    }
                    Thread.Sleep(1000);
                }

                ///check if here's a channel conflict
                if (!ChannelConflictCheck())
                    return false;
                _myDevice.IsOpen = true;

                SetS2Gateway(_channel_ID, target_can_param.BusProtocolMode);

                if (!DoSetBaudrate(target_can_param))
                { return false; }

                _can_param.BusProtocolMode = target_can_param.BusProtocolMode;

                if (!this._driver_hasInit)
                {
                    CAN_Manager.SaintMessageCallbackEvent += new CAN_Manager.SaintMessageCallback(CANMessageCallback);
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
            try
            {
                if (_driver_hasError)
                    return false;

                byte SaintHeader = 0;
                if (Convert.ToInt16(target_can_param.BusProtocolMode) == 0)
                {
                    if ((_channel_ID == 1) || (_channel_ID == 3))
                        SaintHeader = 0x54;
                    else if ((_channel_ID == 2) || (_channel_ID == 4))
                        SaintHeader = 0x5C;
                    else
                        return false;

                    if (target_can_param.BaudRate == CanBaudRate.Unknown)
                    {
                        if (_can_param.BusPhysicType == CANBus_PhysicType.singleWire)
                            target_can_param.BaudRate = CanBaudRate.Baud_33Kbit;
                        else
                            target_can_param.BaudRate = CanBaudRate.Baud_500Kbit;
                    }

                    if (SendBaudRateCommand(SaintHeader, target_can_param.BaudRate))
                    {
                        _can_param.BaudRate = target_can_param.BaudRate;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    byte SaintFDHeader = 0;
                    if (_channel_ID == 3)
                    {
                        SaintHeader = 0x54;
                        SaintFDHeader = 0xD4;
                    }
                    else if (_channel_ID == 4)
                    {
                        SaintHeader = 0x5C;
                        SaintFDHeader = 0xDC;
                    }
                    else
                        return false;

                    if (target_can_param.Arbitration_rate == CanBaudRate.Unknown)
                        target_can_param.Arbitration_rate = CanBaudRate.Baud_500Kbit;
                    if (target_can_param.Data_rate == CanBaudRate.Unknown)
                        target_can_param.Data_rate = CanBaudRate.Baud_2Mbit;

                    if (SendBaudRateCommand_FD(SaintHeader, SaintFDHeader, target_can_param.Arbitration_rate, target_can_param.Data_rate))
                    {
                        _can_param.Arbitration_rate = target_can_param.Arbitration_rate;
                        _can_param.Data_rate = target_can_param.Data_rate;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.1.3-DoSetBaudrate() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override bool DoSend(Pmessage msg)
        {
            try
            {
                if (_driver_hasError)
                    return false;
                if (!_driver_hasInit)
                    return false;

                SingleWireHighVoltage_Config(msg.SingleWireHighVoltage);
                return SendOneMessage(msg, msg.FDorNot);
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
                    CAN_Manager.SaintMessageCallbackEvent -= CANMessageCallback;
                    _driver_hasInit = false;
                }
                if (_channel_ID == 1 || _channel_ID == 3)
                {
                    _myDevice.CAN1_isOpen = false;
                }
                else
                {
                    _myDevice.CAN2_isOpen = false;
                }
                if (!_myDevice.CAN1_isOpen && !_myDevice.CAN2_isOpen)
                {
                    _myDevice.IsOpen = false;
                    CAN_Manager.SaintConnect.DisconnectFromDevice(_myDevice.Device_ID);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.1.5-DoUninitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        private void CANMessageCallback(int deviceID, byte channelID, Pmessage pMessage)
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
                base.NotifyMessageReceived(pMessage);
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.1.6-CANMessageCallback() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// has a channel conflict, return false;
        /// has no conflict, return true;
        /// </summary>
        /// <returns></returns>
        private bool ChannelConflictCheck()
        {
            try
            {
                switch (_channel_ID)
                {
                    ///swc1
                    case 1:
                        if (!_myDevice.CAN1_isOpen)
                        {
                            _myDevice.CAN1_SetHighSpeed = false;
                            _myDevice.CAN1_isOpen = true;
                            return true;
                        }
                        else
                        {
                            if (_myDevice.CAN1_SetHighSpeed)
                                return false;
                            else
                                return true;
                        }
                    ///swc2
                    case 2:
                        if (!_myDevice.CAN2_isOpen)
                        {
                            _myDevice.CAN2_SetHighSpeed = false;
                            _myDevice.CAN2_isOpen = true;
                            return true;
                        }
                        else
                        {
                            if (_myDevice.CAN2_SetHighSpeed)
                                return false;
                            else
                                return true;
                        }
                    ///hscan1
                    case 3:
                        if (!_myDevice.CAN1_isOpen)
                        {
                            _myDevice.CAN1_SetHighSpeed = true;
                            _myDevice.CAN1_isOpen = true;
                            return true;
                        }
                        else
                        {
                            if (_myDevice.CAN1_SetHighSpeed)
                                return true;
                            else
                                return false;
                        }
                    ///hscan2
                    case 4:
                        if (!_myDevice.CAN2_isOpen)
                        {
                            _myDevice.CAN2_SetHighSpeed = true;
                            _myDevice.CAN2_isOpen = true;
                            return true;
                        }
                        else
                        {
                            if (_myDevice.CAN2_SetHighSpeed)
                                return true;
                            else
                                return false;
                        }
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.1.7-ChannelConflictCheck() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        private bool SendMessage2Device(int deviceID, string message)
        {
            return CAN_Manager.SaintConnect.SendDataToDevice(deviceID, message);
        }

        private bool SendMessage2Device(int deviceId, IntPtr pData, UInt32 length)
        {
            return CAN_Manager.SaintConnect.SendDataToDevice(deviceId, pData, length);
        }

        private void SetS2Gateway(int ChannelID, CANBus_ProtocolMode mode)
        {
            byte Saint2Header = 0;
            if (Convert.ToInt16(mode) == 0)
            {
                if ((ChannelID == 1) || (ChannelID == 3)) { Saint2Header = 0x54; }
                else if ((ChannelID == 2) || (ChannelID == 4)) { Saint2Header = 0x5C; }
                SendMessage2Device(_myDevice.Device_ID, string.Format("08 A3 50 58"));
                Thread.Sleep(5);
                if (ChannelID == 1 || ChannelID == 2)
                    SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 04 02", Saint2Header.ToString("X02")));
                else
                    SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 04 00", Saint2Header.ToString("X02")));
            }
            else
            {
                if (ChannelID == 3) { Saint2Header = 0xD4; }
                else if (ChannelID == 4) { Saint2Header = 0xDC; }
                if (Convert.ToInt16(mode) == 1)
                    SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 0A 00", Saint2Header.ToString("X02")));
                else if (Convert.ToInt16(mode) == 2)
                    SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 0A 01", Saint2Header.ToString("X02")));
            }
        }

        private bool SendBaudRateCommand(byte saintHeader, CanBaudRate rate)
        {
            switch (rate)
            {
                case CanBaudRate.Baud_33Kbit:
                    return SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 8D AF", saintHeader.ToString("X02")));
                case CanBaudRate.Baud_83Kbit:
                    return SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 B1 2D", saintHeader.ToString("X02")));
                case CanBaudRate.Baud_100Kbit:
                    return SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 F1 39", saintHeader.ToString("X02")));
                case CanBaudRate.Baud_125Kbit:
                    return SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 DD 3E", saintHeader.ToString("X02")));
                case CanBaudRate.Baud_250Kbit:
                    return SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 CE 3E", saintHeader.ToString("X02")));
                case CanBaudRate.Baud_500Kbit:
                    return SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 C9 39", saintHeader.ToString("X02")));
                default:
                    return false;
            }
        }

        private bool SendBaudRateCommand_FD(byte saintHeader, byte saintFDHeader, CanBaudRate rate, CanBaudRate fdRate)
        {
            switch (rate)
            {
                case CanBaudRate.Baud_33Kbit:
                    SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 8D AF", saintHeader.ToString("X02")));
                    break;
                case CanBaudRate.Baud_100Kbit:
                    SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 F1 39", saintHeader.ToString("X02")));
                    break;
                case CanBaudRate.Baud_125Kbit:
                    SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 DD 3E", saintHeader.ToString("X02")));
                    break;
                case CanBaudRate.Baud_250Kbit:
                    SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 CE 3E", saintHeader.ToString("X02")));
                    break;
                case CanBaudRate.Baud_500Kbit:
                    SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 C9 39", saintHeader.ToString("X02")));
                    break;
                case CanBaudRate.Baud_1Mbit:
                    SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 84 2A", saintHeader.ToString("X02")));
                    break;
                default:
                    return false;
            }
            switch (fdRate)
            {
                case CanBaudRate.Baud_1Mbit:
                    return SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 00 09 0D 23", saintFDHeader.ToString("X02")));
                case CanBaudRate.Baud_2Mbit:
                    return SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 00 04 0D 23", saintFDHeader.ToString("X02")));
                case CanBaudRate.Baud_4Mbit:
                    return SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 00 02 0A 23", saintFDHeader.ToString("X02")));
                case CanBaudRate.Baud_5Mbit:
                    return SendMessage2Device(_myDevice.Device_ID, string.Format("{0} 01 00 01 0D 23", saintFDHeader.ToString("X02")));
                default:
                    return false;
            }
        }

        private void SingleWireHighVoltage_Config(bool singleWireHighVoltage)
        {
            if (_channel_ID == 1)
            {
                if (singleWireHighVoltage == true)
                {
                    if (_sendByHighVol == false)
                    {
                        SendMessage2Device(_myDevice.Device_ID, string.Format("54 02 02"));
                        _sendByHighVol = true;
                    }
                }
                else
                {
                    if (_sendByHighVol == true)
                    {
                        SendMessage2Device(_myDevice.Device_ID, string.Format("54 02 03"));
                        _sendByHighVol = false;
                    }
                }
            }
            else if (_channel_ID == 2)
            {
                if (singleWireHighVoltage == true)
                {
                    if (_sendByHighVol == false)
                    {
                        SendMessage2Device(_myDevice.Device_ID, string.Format("5C 02 02"));
                        _sendByHighVol = true;
                    }
                }
                else
                {
                    if (_sendByHighVol == true)
                    {
                        SendMessage2Device(_myDevice.Device_ID, string.Format("5C 02 03"));
                        _sendByHighVol = false;
                    }
                }
            }
        }

        private bool SendOneMessage(Pmessage msg, bool isFD)
        {
            bool status = false;
            byte[] pByteData = new byte[(msg.CANID & 0x1FFF0000) > 0 ? msg.DLC + 5 : msg.DLC + 3];
            if (!isFD)
            {
                if (_channel_ID == 1 || _channel_ID == 3)
                { pByteData[0] = 0x50; }
                else
                { pByteData[0] = 0x58; }
            }
            else
            {
                if (_channel_ID == 3)
                { pByteData[0] = 0xD0; }
                else
                { pByteData[0] = 0xD8; }
            }
            ///extended identifiers
            if ((msg.CANID & 0x1FFF0000) > 0)
            {
                pByteData[1] = Convert.ToByte((msg.CANID | 0x80000000) >> 24);
                pByteData[2] = Convert.ToByte((msg.CANID & 0x00FF0000) >> 16);
                pByteData[3] = Convert.ToByte((msg.CANID & 0x0000FF00) >> 8);
                pByteData[4] = Convert.ToByte(msg.CANID & 0x000000FF);
                for (byte i = 0; i < msg.DLC; i++)
                {
                    pByteData[5 + i] = msg.Data[0 + i];
                }
                IntPtr ptr = Marshal.AllocHGlobal(pByteData.Length);
                try
                {
                    Marshal.Copy(pByteData, 0, ptr, pByteData.Length);
                    status = SendMessage2Device(_myDevice.Device_ID, ptr, Convert.ToUInt16(msg.DLC + 1 + 4));
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            else ////11bit identifiers
            {
                pByteData[1] = Convert.ToByte((msg.CANID & 0xFF00) >> 8);
                pByteData[2] = Convert.ToByte(msg.CANID & 0x00FF);
                if (msg.DLC > msg.Data.Length)
                {
                    for (byte i = 0; i < msg.Data.Length; i++)
                    {
                        pByteData[3 + i] = msg.Data[0 + i];
                    }
                }
                else
                {
                    for (byte i = 0; i < msg.DLC; i++)
                    {
                        pByteData[3 + i] = msg.Data[0 + i];
                    }
                }
                IntPtr ptr = Marshal.AllocHGlobal(pByteData.Length);
                try
                {
                    Marshal.Copy(pByteData, 0, ptr, pByteData.Length);
                    status = SendMessage2Device(_myDevice.Device_ID, ptr, Convert.ToUInt16(msg.DLC + 1 + 2));

                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            return status;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSnet;
using System.Threading;
using CAN_Library.CAN_Interface;
using CAN_Library.CAN_Parameter;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CAN_Library.CAN_Driver
{
    class CAN_Driver_Spy3 : CAN_Driver
    {
        Spy3_Device _myDevice;
        int _networkID;
        CAN_Parameter _can_param;

        bool _driver_hasError = false;///channel driver comes into a exception
        bool _driver_hasInit = false;

        public CAN_Driver_Spy3(string deviceName, byte channelID, CAN_Parameter can_param)
        {
            try
            {
                if (CAN_Manager.Spy3_Device_List.Any(p => p.Display_Name == deviceName))
                {
                    this._myDevice = CAN_Manager.Spy3_Device_List.First(p => p.Display_Name == deviceName);
                    this._networkID = channelID;
                    this._can_param = can_param;
                    this._driver_hasError = false;
                }
                else
                {
                    _driver_hasError = true;
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.2.1-CAN_Driver_Spy3() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
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

                if (_myDevice.DoInitialization(ref target_can_param, this._networkID) && DoSetBaudrate(target_can_param))
                {
                    _myDevice.Spy3MessageEvent += _myDevice_Spy3MessageEvent;
                    _driver_hasInit = true;
                    return true;
                }
                else
                {
                    _driver_hasInit = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.2.2-DoInitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        private void _myDevice_Spy3MessageEvent(Pmessage Msg, int netid)
        {
            if(netid == this._networkID)
            {
                base.NotifyMessageReceived(Msg);
            }
        }

        protected override bool DoUninitialize()
        {
            try
            {
                if (_driver_hasInit)
                {
                    bool result = _myDevice.DoUninitialize(_networkID);
                    if (result)
                    {
                        _myDevice.Spy3MessageEvent -= _myDevice_Spy3MessageEvent;
                        _driver_hasInit = false;
                        return true;
                    }
                    else
                    { return false; }
                }
                else
                { return true; }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.2.3-DoUninitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override bool DoSend(Pmessage msg)
        {
            try
            {
                //ensure channel driver status is right and device has not lost
                if (_driver_hasError)
                    return false;
                if (!_driver_hasInit)
                    return false;

                return _myDevice.DoSend(msg, this._networkID);
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.2.4-DoSend() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override bool DoSetBaudrate(CAN_Parameter can_param)
        {
            if (_myDevice.DoSetBaudrate(can_param, _networkID))
            {
                if (can_param.BusProtocolMode == CANBus_ProtocolMode.CAN)
                {
                    _can_param.BaudRate = can_param.BaudRate;
                }
                else
                {
                    _can_param.Arbitration_rate = can_param.Arbitration_rate;
                    _can_param.Data_rate = can_param.Data_rate;
                }
                _can_param.BusProtocolMode = can_param.BusProtocolMode;
                return true;
            }
            else
            { return false; }
        }
    }
}

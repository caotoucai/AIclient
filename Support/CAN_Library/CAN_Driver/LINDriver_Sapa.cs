using System;
using System.Linq;
using System.Threading;
using CAN_Library;
using System.Runtime.InteropServices;
using CAN_Library.CAN_Interface;
using CAN_Library.CAN_Parameter;
using System.Collections.Generic;
using SapaWraper;

namespace CAN_Library.CAN_Driver
{
    class LINDriver_Sapa : LINDriver
    {
        SAPA_Device _myDevice;
        byte _channel_ID;
        LIN_Parameter _lin_param;
        bool _driver_hasError = false;
        bool _driver_hasInit = false;
        bool _autoChecksum = true;
        bool _isMaster = true;

        public LINDriver_Sapa(string deviceName, byte channelID, LIN_Parameter lin_param)
        {
            try
            {
                if (CAN_Manager.SAPA_Device_List.Any(p => p.Display_Name == deviceName))
                {
                    this._myDevice = CAN_Manager.SAPA_Device_List.First(p => p.Display_Name == deviceName);
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
                    Log.Logger_Run("to connect sapa device");
                    if (!CAN_Manager.SapaConnect.OpenDevice(_myDevice.Name))
                    {
                        return false;
                    }
                }

                _myDevice.IsOpen = true;
                CAN_Manager.SapaConnect.Enable_CAN(_myDevice.Name, _channel_ID);
                if (!this._driver_hasInit)
                {
                    CAN_Manager.SapaConnect.OnRecvLinDataEvent += OnMessageRecvFuncton;
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

        private void OnMessageRecvFuncton(SAPAConnection.XL_LIN_FRAME_MESSAGE recv_msg)
        {
            Lmessage New_Received = new Lmessage();
            base.NotifyMessageReceived(New_Received);
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
                    CAN_Manager.SapaConnect.Send_LIN_Message(_myDevice.Name, _channel_ID, Msg.ID, Msg.DLC, Msg.Data);
                }
                else
                {
                    if (_channel_ID == 0)
                    {
                        commandToSend = "B410" + Msg.FrameID.ToString("X2").ToUpper() + BitConverter.ToString(Msg.Data, 0, (int)Msg.Data.Length).Replace("-", "").ToUpper() + Msg.CheckSum.ToString("X");
                    }
                    else
                    {
                        commandToSend = "BC10" + Msg.FrameID.ToString("X2").ToUpper() + BitConverter.ToString(Msg.Data, 0, (int)Msg.Data.Length).Replace("-", "").ToUpper() + Msg.CheckSum.ToString("X");
                    }
                    CAN_Manager.SapaConnect.Send_Str_Command(_myDevice.Name, commandToSend);
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
                CAN_Manager.SapaConnect.OnRecvLinDataEvent -= OnMessageRecvFuncton;
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
        }

        void SetLINVersion(LIN_Verison version)
        {
            
        }

        protected override bool DoSetBaudrate(LINBaudRate Rate = LINBaudRate.Baud_19200)
        {
            return true;
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
            _autoChecksum = true;
        }

        protected override void DoSet_Not_AutoChecksum_Mode()
        {
            _autoChecksum = false;
        }
    }
}

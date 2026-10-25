using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using vxlapi_NET;
using static vxlapi_NET.XLDefine;
using System.Runtime.InteropServices;
using CAN_Library.CAN_Interface;
using CAN_Library.CAN_Parameter;


namespace CAN_Library.CAN_Driver
{
    class LINDriver_Vector : LINDriver
    {
        private Vector_Device _myDevice;
        uint _channelIndex;//channel index in system
        ulong _channelMask;//channel mask in system
        uint _hwChannel;//channel index in own device

        private LINBaudRate BaudRate = LINBaudRate.Baud_19200;
        Thread _rxThread;
        bool _isActive_RxThread;
        private int _portHandle = -1;
        private int _eventHandle = -1;

        bool _driver_hasError = false;//device comes into a exception
        bool _driver_hasInit = false;
        bool _isMaster = true;
        bool _autoChecksum = true;
        List<LIN_Node> _slaves;
        LIN_Parameter _lin_param;
        

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WaitForSingleObject(int handle, int timeOut);

        public LINDriver_Vector(string deviceName, byte channelID, LIN_Parameter lin_param)
        {
            try
            {
                if (CAN_Manager.Vector_Device_List.Any(p => p.Display_Name == deviceName))
                {
                    this._myDevice = CAN_Manager.Vector_Device_List.First(p => p.Display_Name == deviceName);
                    this._hwChannel = channelID;
                    this._lin_param = lin_param;
					this._channelIndex = CAN_Manager.Vector_driverConfig.channel.First(p => p.serialNumber.ToString() == deviceName.Split('-')[2] &&
                                p.hwChannel == channelID).channelIndex;
                    _driver_hasError = false;
                }
                else
                {
                    _driver_hasError = true;
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("LINDriver_Vector -- LINDriver_Vector() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        private bool getKicense()
        {
            XLDefine.XL_Status status;

            uint nbrOfBoxes = 0;

            UInt64[] licInfo = new UInt64[4];
            UInt64[] tmpLicInfo = new UInt64[4];
            uint boxMask = 0;
            uint boxSerial = 0;


            status = CAN_Manager.Vector_Driver.XL_GetKeymanBoxes(ref nbrOfBoxes);
            if (status == XLDefine.XL_Status.XL_SUCCESS)
            {
                for (uint i = 0; i < nbrOfBoxes; i++)
                {
                    status = CAN_Manager.Vector_Driver.XL_GetKeymanInfo(i, ref boxMask, ref boxSerial, ref tmpLicInfo);

                    if (status != XLDefine.XL_Status.XL_SUCCESS)
                        return false;

                    licInfo[0] |= tmpLicInfo[0];
                    licInfo[1] |= tmpLicInfo[1];
                    licInfo[2] |= tmpLicInfo[2];
                    licInfo[3] |= tmpLicInfo[3];
                }

            }
            return true;
        }

        protected override bool DoInitialize(LIN_Parameter lin_param)
        {
            Log.Logger_Run(string.Format("LINDriver_Vector -- DoInitialize"));
            try
            {
                if (this._driver_hasError)
                {
                    Log.Logger_Run(string.Format("LINDriver_Vector -- DoInitialize: driver_hasError"));
                    return false;
                }
                XL_Status status;

                //getKicense();

                uint hwIndex = 0;
                uint hwChannel = 0;
                status = CAN_Manager.Vector_Driver.XL_SetApplConfig(AppName, _channelIndex, _myDevice.HWType, _myDevice.HWIndex, _hwChannel, XL_BusTypes.XL_BUS_TYPE_LIN);
                if (status != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run(string.Format("LINDriver_Vector -- XL_SetApplConfig Error = {0}", status));
                    return false;
                }

                XL_HardwareType hwType = XL_HardwareType.XL_HWTYPE_NONE;
                status = CAN_Manager.Vector_Driver.XL_GetApplConfig(AppName, _channelIndex, ref hwType, ref hwIndex, ref hwChannel, XL_BusTypes.XL_BUS_TYPE_LIN);
                if (status != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run(string.Format("LINDriver_Vector -- XL_GetApplConfig Error = {0}", status));
                    return false;
                }

                int channelIndex = 0;
                ulong permissionMask = 0;
                if (hwType == _myDevice.HWType && hwIndex == _myDevice.HWIndex && hwChannel == _hwChannel)
                {
                    _channelMask = CAN_Manager.Vector_Driver.XL_GetChannelMask(hwType, (int)hwIndex, (int)hwChannel);
                    channelIndex = CAN_Manager.Vector_Driver.XL_GetChannelIndex(hwType, (int)hwIndex, (int)hwChannel);
                    if (channelIndex == _channelIndex)
                    {
                        permissionMask = _channelMask;
                    }
                    else
                    {
                        Log.Logger_Run(string.Format("LINDriver_Vector -- XL_GetChannelMask Error, return channelIndex = {0}, _channelIndex = {1}", channelIndex, _channelIndex));
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                status = CAN_Manager.Vector_Driver.XL_OpenPort(ref _portHandle, AppName, _channelMask, ref permissionMask, 256, XL_InterfaceVersion.XL_INTERFACE_VERSION_V3, XL_BusTypes.XL_BUS_TYPE_LIN);
                if (status != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run(string.Format("LINDriver_Vector -- XL_OpenPort Error = {0}", status));
                    return false;
                }

                if (_isMaster)
                {
                    linInitMaster();
                }
                else
                {
                    if (!linInitSlave(_slaves))
                    {
                        return false;
                    }
                }

                if (CAN_Manager.Vector_Driver.XL_ActivateChannel(_portHandle, _channelMask, XL_BusTypes.XL_BUS_TYPE_LIN, XL_AC_Flags.XL_ACTIVATE_NONE) != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run(string.Format("LINDriver_Vector -- XL_ActivateChannel Error = {0}", status));
                    return false;
                }

                if (CAN_Manager.Vector_Driver.XL_SetNotification(_portHandle, ref _eventHandle, 1) != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run(string.Format("LINDriver_Vector -- XL_SetNotification Error = {0}", status));
                    return false;
                }

                CAN_Manager.Vector_Driver.XL_FlushReceiveQueue(_portHandle);

                _isActive_RxThread = true;
                _rxThread = new Thread(new ThreadStart(RX_Thread));
                _rxThread.IsBackground = true;
                _rxThread.Start();
                _driver_hasInit = true;
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("LINDriver_Vector -- DoInitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected bool linInitSlave(byte linID)
        {
            Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- linInitSlave()"));

            XL_Status status;
            XLClass.xl_linStatPar linStatPar = new XLClass.xl_linStatPar();
            switch (BaudRate)
            {
                case LINBaudRate.Baud_10400:
                    linStatPar.baudrate = 10400;
                    break;
                case LINBaudRate.Baud_10417:
                    linStatPar.baudrate = 10417;
                    break;
                case LINBaudRate.Baud_19200:
                    linStatPar.baudrate = 19200;
                    break;
                case LINBaudRate.Baud_2400:
                    linStatPar.baudrate = 2400;
                    break;
                case LINBaudRate.Baud_9600:
                    linStatPar.baudrate = 9600;
                    break;
                case LINBaudRate.Baud_20000:
                    linStatPar.baudrate = 20000;
                    break;
                default:
                    linStatPar.baudrate = 19200;
                    break;
            }

            linStatPar.LINMode = XLDefine.XL_LIN_Mode.XL_LIN_SLAVE;

            linStatPar.LINVersion = XLDefine.XL_LIN_Version.XL_LIN_VERSION_2_1;
            status = CAN_Manager.Vector_Driver.XL_LinSetChannelParams(_portHandle, _channelMask, linStatPar);
            if (status != XL_Status.XL_SUCCESS)
            {
                Log.Logger_Run(string.Format("LINDriver_Vector -- XL_LinSetChannelParams Error = {0}", status));
                return false;
            }

            byte msgDLC = 8;
            byte id = linID;

            byte[] linData = { 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x00, 0xF0 };  //fake data
            status = CAN_Manager.Vector_Driver.XL_LinSetSlave(_portHandle, _channelMask, id, linData, msgDLC, XLDefine.XL_LIN_CalcChecksum.XL_LIN_CALC_CHECKSUM_ENHANCED);
            if (status != XL_Status.XL_SUCCESS)
            {
                Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- XL_LinSetDLC Error = {0}", status));
            }
            //if (CAN_Manager.Vector_Driver.XL_ActivateChannel(_portHandle, _channelMask, XL_BusTypes.XL_BUS_TYPE_LIN, XL_AC_Flags.XL_ACTIVATE_NONE) != XL_Status.XL_SUCCESS)
            //{
            //    return false;
            //}
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

        protected override bool DoSet_Lin_Slavers(List<LIN_Node> slaves)
        {
            _isMaster = false;
            _slaves = slaves;
            return true;
        }

        private bool linInitMaster()
        {
            Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- linInitMaster()"));
            XL_Status status;
            XLClass.xl_linStatPar linStatPar = new XLClass.xl_linStatPar();
            switch (_lin_param.Baudrate)
            {
                case LINBaudRate.Baud_10400:
                    linStatPar.baudrate = 10400;
                    break;
                case LINBaudRate.Baud_10417:
                    linStatPar.baudrate = 10417;
                    break;
                case LINBaudRate.Baud_19200:
                    linStatPar.baudrate = 19200;
                    break;
                case LINBaudRate.Baud_2400:
                    linStatPar.baudrate = 2400;
                    break;
                case LINBaudRate.Baud_9600:
                    linStatPar.baudrate = 9600;
                    break;
                case LINBaudRate.Baud_20000:
                    linStatPar.baudrate = 20000;
                    break;
                default:
                    linStatPar.baudrate = 19200;
                    break;
            }

            linStatPar.LINMode = XLDefine.XL_LIN_Mode.XL_LIN_MASTER;

            switch (_lin_param.LINversion)
            {
                case LIN_Verison.LIN_1_3:
                    linStatPar.LINVersion = XLDefine.XL_LIN_Version.XL_LIN_VERSION_1_3;
                    break;
                case LIN_Verison.LIN_2_0:
                    linStatPar.LINVersion = XLDefine.XL_LIN_Version.XL_LIN_VERSION_2_0;
                    break;
                case LIN_Verison.LIN_2_1:
                    linStatPar.LINVersion = XLDefine.XL_LIN_Version.XL_LIN_VERSION_2_1;
                    break;
                default:
                    linStatPar.LINVersion = XLDefine.XL_LIN_Version.XL_LIN_VERSION_2_1;
                    break;
            }

            status = CAN_Manager.Vector_Driver.XL_LinSetChannelParams(_portHandle, _channelMask, linStatPar);
            if (status != XL_Status.XL_SUCCESS)
            {
                Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- XL_LinSetChannelParams Error = {0}", status));
                return false;
            }

            //Defines the data length for all requested messages.
            //Set DLC for LIN message with ID 0-64 to 8
            byte[] DLC = new byte[64];
            for (int i = 0; i < 64; i++) DLC[i] = 8;
            status = CAN_Manager.Vector_Driver.XL_LinSetDLC(_portHandle, _channelMask, DLC);
            if (status != XL_Status.XL_SUCCESS)
            {
                Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- XL_LinSetDLC Error = {0}", status));
            }
            //if (CAN_Manager.Vector_Driver.XL_ActivateChannel(_portHandle, _channelMask, XL_BusTypes.XL_BUS_TYPE_LIN, XL_AC_Flags.XL_ACTIVATE_NONE) != XL_Status.XL_SUCCESS)
            //{
            //    return false;
            //}
            return true;
        }


        private bool linInitSlave(List<LIN_Node> slaves)
        {
            Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- linInitSlave()"));
            XL_Status status;
            XLClass.xl_linStatPar linStatPar = new XLClass.xl_linStatPar();
            switch (_lin_param.Baudrate)
            {
                case LINBaudRate.Baud_10400:
                    linStatPar.baudrate = 10400;
                    break;
                case LINBaudRate.Baud_10417:
                    linStatPar.baudrate = 10417;
                    break;
                case LINBaudRate.Baud_19200:
                    linStatPar.baudrate = 19200;
                    break;
                case LINBaudRate.Baud_2400:
                    linStatPar.baudrate = 2400;
                    break;
                case LINBaudRate.Baud_9600:
                    linStatPar.baudrate = 9600;
                    break;
                case LINBaudRate.Baud_20000:
                    linStatPar.baudrate = 20000;
                    break;
                default:
                    linStatPar.baudrate = 19200;
                    break;
            }

            linStatPar.LINMode = XLDefine.XL_LIN_Mode.XL_LIN_SLAVE;

            switch (_lin_param.LINversion)
            {
                case LIN_Verison.LIN_1_3:
                    linStatPar.LINVersion = XLDefine.XL_LIN_Version.XL_LIN_VERSION_1_3;
                    break;
                case LIN_Verison.LIN_2_0:
                    linStatPar.LINVersion = XLDefine.XL_LIN_Version.XL_LIN_VERSION_2_0;
                    break;
                case LIN_Verison.LIN_2_1:
                    linStatPar.LINVersion = XLDefine.XL_LIN_Version.XL_LIN_VERSION_2_1;
                    break;
                default:
                    linStatPar.LINVersion = XLDefine.XL_LIN_Version.XL_LIN_VERSION_2_1;
                    break;
            }

            status = CAN_Manager.Vector_Driver.XL_LinSetChannelParams(_portHandle, _channelMask, linStatPar);
            if (status != XL_Status.XL_SUCCESS)
            {
                Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- XL_LinSetChannelParams Error = {0}", status));
                return false;
            }

            foreach (LIN_Node _slave in slaves)
            {
                foreach (LIN_Message frame in _slave.Frames)
                {
                    XL_LIN_CalcChecksum CheckSumType = XL_LIN_CalcChecksum.XL_LIN_CALC_CHECKSUM_ENHANCED;
                    switch (frame.CheckSumType)
                    {
                        case LINChecksumType.Classic:
                            CheckSumType = XL_LIN_CalcChecksum.XL_LIN_CALC_CHECKSUM;
                            break;
                        case LINChecksumType.Enhanced:
                            CheckSumType = XL_LIN_CalcChecksum.XL_LIN_CALC_CHECKSUM_ENHANCED;
                            break;
                    }

                    status = CAN_Manager.Vector_Driver.XL_LinSetSlave(_portHandle, _channelMask, (byte)frame.ID, frame.Data, (byte)frame.DLC, CheckSumType);
                    if (status != XL_Status.XL_SUCCESS)
                    {
                        Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- XL_LinSetSlave Error = {0}", status));
                        return false;
                    }

                    //if (CAN_Manager.Vector_Driver.XL_ActivateChannel(_portHandle, _channelMask, XL_BusTypes.XL_BUS_TYPE_LIN, XL_AC_Flags.XL_ACTIVATE_NONE) != XL_Status.XL_SUCCESS)
                    //{
                    //    return false;
                    //}
                }
            }

            return true;
        }
               
        protected override bool DoUninitialize()
        {
            if (_driver_hasInit)
            {
                if (this._rxThread != null)
                {
                    if (_rxThread.IsAlive)
                    {
                        _isActive_RxThread = false;
                    }
                }
                CAN_Manager.Vector_Driver.XL_ClosePort(_portHandle);
                _driver_hasInit = false;
            }
            return true;
        }

        private void RX_Thread()
        {
            try
            {
                Thread.Sleep(100);

                XLClass.xl_event receivedEvent = new XLClass.xl_event();
                XL_Status xlStatus = XL_Status.XL_SUCCESS;
                WaitResults waitResult = new WaitResults();
                Lmessage New_Received = null;

                while (_isActive_RxThread)
                {
                    waitResult = (WaitResults)WaitForSingleObject(_eventHandle, 1000);
                    if (waitResult != WaitResults.WAIT_TIMEOUT)
                    {
                        xlStatus = XL_Status.XL_SUCCESS;
                        while (xlStatus != XL_Status.XL_ERR_QUEUE_IS_EMPTY)
                        {
                            xlStatus = CAN_Manager.Vector_Driver.XL_Receive(_portHandle, ref receivedEvent);
                            if (xlStatus == XL_Status.XL_SUCCESS)
                            {
                                switch (receivedEvent.tag)
                                {
                                    case XLDefine.XL_EventTags.XL_LIN_MSG:
                                        New_Received = new Lmessage(receivedEvent.tagData.linMsgApi.linMsg.id, receivedEvent.tagData.linMsgApi.linMsg.dlc,
                                            receivedEvent.tagData.linMsgApi.linMsg.data);
                                        New_Received.CheckSum = receivedEvent.tagData.linMsgApi.linMsg.crc;
                                        New_Received.LastReceivedTimeMs = (long)Convert.ToUInt64(DateTime.Now.Ticks / 10000L);
                                        if ((receivedEvent.tagData.linMsgApi.linMsg.flags & XLDefine.XL_MessageFlags.XL_LIN_MSGFLAG_TX)
                                         == XLDefine.XL_MessageFlags.XL_LIN_MSGFLAG_TX)
                                        {
                                            New_Received.Direction = LINDirection.Publisher;
                                        }
                                        else
                                        {
                                            New_Received.Direction = LINDirection.Subscriber;
                                        }

                                        base.NotifyMessageReceived(New_Received);
                                        break;

                                    case XLDefine.XL_EventTags.XL_LIN_ERRMSG:
                                        //Console.WriteLine("XL_LIN_ERRMSG");
                                        break;

                                    case XLDefine.XL_EventTags.XL_LIN_SYNCERR:
                                        //Console.WriteLine("XL_LIN_SYNCERR");
                                        break;

                                    case XLDefine.XL_EventTags.XL_LIN_NOANS:
                                        //Console.WriteLine("XL_LIN_NOANS");
                                        break;

                                    case XLDefine.XL_EventTags.XL_LIN_WAKEUP:
                                        //Console.WriteLine("XL_LIN_WAKEUP");
                                        break;

                                    case XLDefine.XL_EventTags.XL_LIN_SLEEP:
                                        //Console.WriteLine("XL_LIN_SLEEP");
                                        break;

                                    case XLDefine.XL_EventTags.XL_LIN_CRCINFO:
                                        //Console.WriteLine("XL_LIN_CRCINFO");
                                        break;
                                }
                            }
                            else
                            {
                                Thread.Sleep(1);
                            }
                        }
                    }
                    if (_driver_hasError)
                        return;
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("FlexRay_Driver_Vector -- RX_Thread() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        protected override bool DoSend(LIN_Message Msg)
        {
            if (_driver_hasError)
                return false;
            if (!_driver_hasInit)
                return false;

            XL_Status status = 0;
            XLDefine.XL_LIN_CalcChecksum CheckSumType = XLDefine.XL_LIN_CalcChecksum.XL_LIN_CALC_CHECKSUM_ENHANCED;
            switch (Msg.CheckSumType)
            {
                case LINChecksumType.Classic:
                    CheckSumType = XLDefine.XL_LIN_CalcChecksum.XL_LIN_CALC_CHECKSUM;
                    break;
                case LINChecksumType.Enhanced:
                    CheckSumType = XLDefine.XL_LIN_CalcChecksum.XL_LIN_CALC_CHECKSUM_ENHANCED;
                    break;
            }


            if (Msg.Direction == LINDirection.Publisher)
            {
                status = CAN_Manager.Vector_Driver.XL_LinSetSlave(_portHandle, _channelMask, (byte)Msg.ID, Msg.Data, (byte)Msg.DLC, CheckSumType);
            }

            if (status == XL_Status.XL_SUCCESS)
            {
                return true;
            }
            else
            {
                //Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- XL_LinSetSlave Error = {0}", status));
            }
            return false;
        }

        protected override bool DoSetBaudrate(LINBaudRate Rate = LINBaudRate.Baud_19200)
        {
            return true;
        }

        protected override void DoClearPID(byte PID)
        {
           // throw new NotImplementedException();
        }


        protected override void DoSet_AutoChecksum_Mode()
        {
            //throw new NotImplementedException();
        }

        protected override void DoSet_Not_AutoChecksum_Mode()
        {
            //throw new NotImplementedException();
        }
    }
}

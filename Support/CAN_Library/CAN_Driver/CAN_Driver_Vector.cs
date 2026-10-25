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

namespace CAN_Library.CAN_Driver
{
    class CAN_Driver_Vector : CAN_Driver
    {
        Vector_Device _myDevice;
        uint _channelIndex;//channel index in system
        ulong _channelMask;//channel mask in system
        uint _hwChannel;//channel index in own device
        CAN_Parameter _can_param;
        bool _sendByHighVol = false;

        Thread _rxThread;//to get standard messages in CAN bus
        bool _isActive_RxThread;//to control RxThread
        int _portHandle = -1;//standard CAN port handler.
        int _eventHandle = -1;//standard CAN event handler

        Thread _fd_rxThread;//to receive FD messages in CAN bus
        bool _isActive_fd_rxThread;//to control FD_RxThread
        int _fd_portHandle = -1;//FD CAN port handler
        int _fd_eventHandle = -1;//FD CAN event handler
        ulong messageTotalCounterSent = 0;
        bool _driver_hasError = false;//device comes into a exception
        bool _driver_hasInit = false;

        const ulong MAX_TransmitSend = 50000; 

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WaitForSingleObject(int handle, int timeOut);

        public CAN_Driver_Vector(string deviceName, byte channelID, CAN_Parameter can_param)
        {
            try
            {
                if (CAN_Manager.Vector_Device_List.Count > 0)
                {
                    if (CAN_Manager.Vector_Device_List.Any(p => p.Display_Name == deviceName))
                    {
                        this._myDevice = CAN_Manager.Vector_Device_List.First(p => p.Display_Name == deviceName);
                        this._hwChannel = channelID;
                        this._can_param = can_param;
                        if (CAN_Manager.Vector_driverConfig.channelCount > 0)
                        {
                            if (deviceName.IndexOf("Virtual") >= 0)
                            {
                                this._channelIndex = CAN_Manager.Vector_driverConfig.channel.First(p => p.serialNumber == 0 && p.hwChannel == channelID).channelIndex;
                                _driver_hasError = false;
                            }
                            else if (CAN_Manager.Vector_driverConfig.channel.Any(p => p.serialNumber.ToString() == deviceName.Split('-')[2]))
                            {
                                this._channelIndex = CAN_Manager.Vector_driverConfig.channel.First(p => p.serialNumber.ToString() == deviceName.Split('-')[2] &&
                                p.hwChannel == channelID).channelIndex;
                                _driver_hasError = false;
                            }
                            else
                            {
                                _driver_hasError = true;
                            }
                        }
                        else
                        {
                            _driver_hasError = true;
                        }
                    }
                    else
                    {
                        _driver_hasError = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("CAN_Driver_Vector -- CAN_Driver_Vector() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
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

        protected override bool DoInitialize(CAN_Parameter target_can_param)
        {
            Log.Logger_Run(string.Format("CAN_Driver_Vector -- DoInitialize"));
            try
            {
                ///make sure driver is ok and device has not lost
                if (this._driver_hasError)
                {
                    Log.Logger_Run(string.Format("CAN_Driver_Vector -- DoInitialize: driver_hasError"));
                    return false;
                }

                XL_Status status;
                //getKicense();
                //status = CAN_Manager.Vector_Driver.XL_OpenDriver();

                ///create a new application in the Vector Hardware Config tool if there's no one's name is Appname
                ///if there is such one, then set the app channel configuration in the existing application
                ///then create or set app channel which is the parameter "appChannel" in Vector Hardware Config tool, assign it to real device channel which parameters is HWType,HWIndex,HWChannel,BusTypes
                uint hwIndex = 0;
                uint hwChannel = 0;
                status = CAN_Manager.Vector_Driver.XL_SetApplConfig(AppName, _channelIndex, _myDevice.HWType, _myDevice.HWIndex, _hwChannel, XL_BusTypes.XL_BUS_TYPE_CAN);
                if (status != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_SetApplConfig Error = {0}", status));
                    return false;
                }
                ///get an app channel's info which is on XL_BUS_TYPE_CAN
                XL_HardwareType hwType = XL_HardwareType.XL_HWTYPE_NONE;
                status = CAN_Manager.Vector_Driver.XL_GetApplConfig(AppName, _channelIndex, ref hwType, ref hwIndex, ref hwChannel, XL_BusTypes.XL_BUS_TYPE_CAN);
                if (status != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_GetApplConfig Error = {0}", status));
                    return false;
                }
                ///ensure the assigned hardward channel is the target one
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
                        Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_GetChannelMask Error, return channelIndex = {0}, _channelIndex = {1}", channelIndex, _channelIndex));
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                if (target_can_param.BusProtocolMode == CANBus_ProtocolMode.ISO_CAN_FD)
                {
                    ///FD CAN
                    status = CAN_Manager.Vector_Driver.XL_OpenPort(ref _fd_portHandle, AppName, _channelMask, ref permissionMask, 16000, XL_InterfaceVersion.XL_INTERFACE_VERSION_V4, XL_BusTypes.XL_BUS_TYPE_CAN);
                    if (status != XL_Status.XL_SUCCESS)
                    {
                        Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_OpenPort Error = {0}", status));
                        return false;
                    }
                    permissionMask = _channelMask;

                    //to set baud rate
                    if (!SetFDBaudrate(target_can_param))
                    { return false; }

                    ///to active channel in device driver
                    status = CAN_Manager.Vector_Driver.XL_ActivateChannel(_fd_portHandle, _channelMask, XL_BusTypes.XL_BUS_TYPE_CAN, XL_AC_Flags.XL_ACTIVATE_NONE);
                    if (status != XL_Status.XL_SUCCESS)
                    {
                        Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_ActivateChannel Error = {0}", status));
                        return false;
                    }

                    ///set channel message callback
                    status = CAN_Manager.Vector_Driver.XL_SetNotification(_fd_portHandle, ref _fd_eventHandle, 1);
                    if (status != XL_Status.XL_SUCCESS)
                    {
                        Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_SetNotification Error = {0}", status));
                        return false;
                    }

                    status = CAN_Manager.Vector_Driver.XL_ResetClock(_fd_portHandle);
                    if (status != XL_Status.XL_SUCCESS)
                    {
                        Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_ResetClock Error = {0}", status));
                        return false;
                    }

                    this._isActive_fd_rxThread = true;
                    this._fd_rxThread = new Thread(new ThreadStart(FDRX_Thread));
                    this._fd_rxThread.IsBackground = true;

                    this._fd_rxThread.Start();
                }
                else if (target_can_param.BusProtocolMode == CANBus_ProtocolMode.CAN)
                {
                    status = CAN_Manager.Vector_Driver.XL_OpenPort(ref _portHandle, AppName, _channelMask, ref permissionMask, 1024, XL_InterfaceVersion.XL_INTERFACE_VERSION, XL_BusTypes.XL_BUS_TYPE_CAN);
                    if (status != XL_Status.XL_SUCCESS)
                    {
                        Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_OpenPort Error = {0}", status));
                        return false;
                    }

                    //to set baud rate
                    if (!SetNormBaudrate(target_can_param))
                    { return false; }

                    status = CAN_Manager.Vector_Driver.XL_ActivateChannel(_portHandle, _channelMask, XL_BusTypes.XL_BUS_TYPE_CAN, XL_AC_Flags.XL_ACTIVATE_NONE);
                    if (status != XL_Status.XL_SUCCESS)
                    {
                        Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_ActivateChannel Error = {0}", status));
                        return false;
                    }

                    status = CAN_Manager.Vector_Driver.XL_SetNotification(_portHandle, ref _eventHandle, 1);
                    if (status != XL_Status.XL_SUCCESS)
                    {
                        Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_SetNotification Error = {0}", status));
                        return false;
                    }

                    status = CAN_Manager.Vector_Driver.XL_ResetClock(_portHandle);
                    if (status != XL_Status.XL_SUCCESS)
                    {
                        Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_ResetClock Error = {0}", status));
                        return false;
                    }
                    this._isActive_RxThread = true;
                    this._rxThread = new Thread(new ThreadStart(RX_Thread));
                    this._rxThread.IsBackground = true;
                    this._rxThread.Start();
                }
                //_myDevice.IsOpen = true;
                _can_param.BusProtocolMode = target_can_param.BusProtocolMode;
                this._driver_hasInit = true;
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("CAN_Driver_Vector -- DoInitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override bool DoUninitialize()
        {
            Log.Logger_Run(string.Format("CAN_Driver_Vector -- DoUninitialize"));
            try
            {
                XL_Status status;
                if (_driver_hasInit)
                {
                    if (this._fd_rxThread != null)
                    {
                        if (_fd_rxThread.IsAlive)
                        {
                            _isActive_fd_rxThread = false;
                            //_fd_rxThread.Join();
                        }
                    }
                    if (this._rxThread != null)
                    {
                        if (_rxThread.IsAlive)
                        {
                            _isActive_RxThread = false;
                            //_rxThread.Join();
                        }
                    }

                    if (_can_param.BusProtocolMode == CANBus_ProtocolMode.CAN)
                    {
                        status = CAN_Manager.Vector_Driver.XL_ClosePort(_portHandle);
                        if (status != XL_Status.XL_SUCCESS)
                        {
                            Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_ClosePort Error = {0}", status));
                        }
                    }
                    else
                    {
                        status = CAN_Manager.Vector_Driver.XL_ClosePort(_fd_portHandle);
                        if (status != XL_Status.XL_SUCCESS)
                        {
                            Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_ClosePort Error = {0}", status));
                        }
                        status = CAN_Manager.Vector_Driver.XL_ClosePort(_portHandle);
                        if (status != XL_Status.XL_SUCCESS)
                        {
                            Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_ClosePort Error = {0}", status));
                        }
                    }

                    _driver_hasInit = false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("CAN_Driver_Vector -- DoUninitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override bool DoSetBaudrate(CAN_Parameter target_can_param)
        {
            try
            {
                if (_driver_hasError)
                {
                    Log.Logger_Run(string.Format("CAN_Driver_Vector -- DoSetBaudrate: driver_hasError"));
                    return false;
                }

                XL_Status status;
                if (target_can_param.BusProtocolMode == CANBus_ProtocolMode.CAN)
                {
                    if (target_can_param.BaudRate == CanBaudRate.Unknown)
                    {
                        if (target_can_param.BusPhysicType == CANBus_PhysicType.singleWire)
                            target_can_param.BaudRate = CanBaudRate.Baud_33Kbit;
                        else
                            target_can_param.BaudRate = CanBaudRate.Baud_500Kbit;
                    }
                    status = CAN_Manager.Vector_Driver.XL_CanSetChannelBitrate(_portHandle, _channelMask, Convert_Bandrate(target_can_param.BaudRate));
                    if (status == XL_Status.XL_SUCCESS)
                    {
                        _can_param.BaudRate = target_can_param.BaudRate;
                        return true;
                    }
                    else
                    {
                        Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_CanSetChannelBitrate Error = {0}", status));
                        return false;
                    }
                }
                else
                {
                    if (target_can_param.Arbitration_rate == CanBaudRate.Unknown)
                        target_can_param.Arbitration_rate = CanBaudRate.Baud_500Kbit;
                    if (target_can_param.Data_rate == CanBaudRate.Unknown)
                        target_can_param.Data_rate = CanBaudRate.Baud_2Mbit;
                    if (target_can_param.Abr_sjw == 0xff || target_can_param.Abr_tseg1 == 0xff || target_can_param.Abr_tseg2 == 0xff || target_can_param.Data_sjw == 0xff || target_can_param.Data_tseg1 == 0xff || target_can_param.Data_tseg2 == 0xff)
                    {
                        // target_can_param.Abr_sjw = 1;
                        // target_can_param.Abr_tseg1 = 31;
                        // target_can_param.Abr_tseg2 = 8;
                        // target_can_param.Data_sjw = 1;
                        // target_can_param.Data_tseg1 = 7;
                        // target_can_param.Data_tseg2 = 2;
                        target_can_param.Abr_sjw = 2;
                        target_can_param.Abr_tseg1 = 63;
                        target_can_param.Abr_tseg2 = 16;
                        target_can_param.Data_sjw = 2;
                        target_can_param.Data_tseg1 = 14;
                        target_can_param.Data_tseg2 = 5;
                    }
                    XLClass.XLcanFdConf canFdConf = new XLClass.XLcanFdConf()
                    {
                        arbitrationBitRate = Convert_Bandrate(target_can_param.Arbitration_rate),
                        sjwAbr = target_can_param.Abr_sjw,
                        tseg1Abr = target_can_param.Abr_tseg1,
                        tseg2Abr = target_can_param.Abr_tseg2,
                        dataBitRate = Convert_Bandrate(target_can_param.Data_rate),
                        sjwDbr = target_can_param.Data_sjw,
                        tseg1Dbr = target_can_param.Data_tseg1,
                        tseg2Dbr = target_can_param.Data_tseg2,
                    };
                    status = CAN_Manager.Vector_Driver.XL_CanFdSetConfiguration(_fd_portHandle, _channelMask, canFdConf);
                    if (status == XL_Status.XL_SUCCESS)
                    {
                        _can_param.Arbitration_rate = target_can_param.Arbitration_rate;
                        _can_param.Abr_sjw = target_can_param.Abr_sjw;
                        _can_param.Abr_tseg1 = target_can_param.Abr_tseg1;
                        _can_param.Abr_tseg2 = target_can_param.Abr_tseg2;
                        _can_param.Data_rate = target_can_param.Data_rate;
                        _can_param.Data_sjw = target_can_param.Data_sjw;
                        _can_param.Abr_tseg1 = target_can_param.Data_tseg1;
                        _can_param.Abr_tseg2 = target_can_param.Data_tseg2;
                        return true;
                    }
                    else
                    {
                        Log.Logger_Run(string.Format("CAN_Driver_Vector -- XL_CanFdSetConfiguration Error = {0}", status));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("CAN_Driver_Vector -- DoSetBaudrate() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override bool DoSend(Pmessage msg)
        {
            try
            {
                if (_driver_hasError)
                {
                    Log.Logger_Run(string.Format("CAN_Driver_Vector -- DoSend: driver_hasError"));
                    return false;
                }
                if (!_driver_hasInit)
                {
                    return false;
                }
                if (!msg.FDorNot)
                {
                    return Send_CAN2_0_Msg(msg, msg.SingleWireHighVoltage);
                }
                else
                {
                    return Send_CAN_FD_Msg(msg);
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("CAN_Driver_Vector -- DoSend() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        private void FDRX_Thread()
        {
            try
            {
                Thread.Sleep(100);

                XLClass.XLcanRxEvent receivedEvent = new XLClass.XLcanRxEvent();
                XL_Status xlStatus = XL_Status.XL_SUCCESS;
                WaitResults waitResult = new WaitResults();
                while (_isActive_fd_rxThread)
                {
                    waitResult = (WaitResults)WaitForSingleObject(_fd_eventHandle, 1000);
                    if (waitResult != WaitResults.WAIT_TIMEOUT)
                    {
                        xlStatus = XL_Status.XL_SUCCESS;
                        while (xlStatus != XL_Status.XL_ERR_QUEUE_IS_EMPTY)
                        {
                            xlStatus = CAN_Manager.Vector_Driver.XL_CanReceive(_fd_portHandle, ref receivedEvent);
                            if (xlStatus == XL_Status.XL_SUCCESS)
                            {
                                build_Message(receivedEvent);
                            }
                            else
                            {
                                Thread.Sleep(1);
                            }
                        }
                    }
                    if (_driver_hasError)
                    {
                        Log.Logger_Run(string.Format("CAN_Driver_Vector -- FDRX_Thread: driver_hasError"));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("CAN_Driver_Vector -- FDRX_Thread() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        private void RX_Thread()
        {
            try
            {
                Thread.Sleep(100);

                XLClass.xl_event receivedEvent = new XLClass.xl_event();
                XL_Status xlStatus = XL_Status.XL_SUCCESS;
                WaitResults waitResult = new WaitResults();
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
                                build_Message(receivedEvent);
                            }
                            else
                            {
                                Thread.Sleep(1);
                            }
                        }
                    }
                    if (_driver_hasError)
                    {
                        Log.Logger_Run(string.Format("CAN_Driver_Vector -- RX_Thread: driver_hasError"));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("CAN_Driver_Vector -- RX_Thread() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        //CAN build
        private void build_Message(XLClass.xl_event Event)
        {
            //get right CAN ID
            uint can_id;
            if ((Event.tagData.can_Msg.id & 0x80000000) > 0)
            {
                can_id = Event.tagData.can_Msg.id & 0x7FFFFFFF;
            }
            else
            {
                can_id = Event.tagData.can_Msg.id;
            }
            switch (Event.tag)
            {
                case XL_EventTags.XL_RECEIVE_MSG:
                    //tx message
                    if (Event.tagData.can_Msg.flags == XL_MessageFlags.XL_CAN_MSG_FLAG_TX_COMPLETED)
                    {
                        base.NotifyMessageReceived(new Pmessage(can_id,
                                                                    Convert.ToByte(Event.tagData.can_Msg.dlc),
                                                                      Event.tagData.can_Msg.data, TxRxType.Tx, false, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)));
                    }
                    //rx message
                    else if (Event.tagData.can_Msg.flags == XL_MessageFlags.XL_CAN_MSG_FLAG_NONE)
                    {
                        base.NotifyMessageReceived(new Pmessage(can_id,
                                                                    Convert.ToByte(Event.tagData.can_Msg.dlc),
                                                                      Event.tagData.can_Msg.data, TxRxType.Rx, false, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)));
                    }
                    //error message
                    else if (Event.tagData.can_Msg.flags == XL_MessageFlags.XL_CAN_MSG_FLAG_ERROR_FRAME)
                    {
                        base.NotifyMessageReceived(new Pmessage(0, 0, new byte[1], TxRxType.Err, false, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)));
                    }
                    break;
                case XL_EventTags.XL_TRANSMIT_MSG:
                    base.NotifyMessageReceived(new Pmessage(can_id,
                                                                Convert.ToByte(Event.tagData.can_Msg.dlc),
                                                                  Event.tagData.can_Msg.data, TxRxType.Tx, false, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)));
                    break;
                default:
                    break;
            }
        }

        //FD CAN build
        private void build_Message(XLClass.XLcanRxEvent Event)
        {
            //get right CAN ID
            bool isFD = false;
            switch (Event.tag)
            {
                //rx message
                case XLDefine.XL_CANFD_RX_EventTags.XL_CAN_EV_TAG_RX_OK:
                    //get right CAN ID
                    uint can_id = (Event.tagData.canRxOkMsg.canId & 0x80000000) > 0 ? (Event.tagData.canRxOkMsg.canId & 0x7FFFFFFF) : Event.tagData.canRxOkMsg.canId;

                    if (Event.tagData.canRxOkMsg.msgFlags == XL_CANFD_RX_MessageFlags.XL_CAN_RXMSG_FLAG_NONE)
                    {
                        isFD = false;
                    }
                    else if ((Event.tagData.canRxOkMsg.msgFlags & XL_CANFD_RX_MessageFlags.XL_CAN_RXMSG_FLAG_EDL) > 0)
                    {
                        isFD = true;
                    }
                    base.NotifyMessageReceived(new Pmessage(can_id,
                                                                BuildDLC(Event.tagData.canRxOkMsg.dlc),
                                                                  Event.tagData.canRxOkMsg.data, TxRxType.Rx, isFD, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)));
                    break;
                //tx message
                case XLDefine.XL_CANFD_RX_EventTags.XL_CAN_EV_TAG_TX_OK:
                    //get right CAN ID
                    uint can_id2 = (Event.tagData.canTxOkMsg.canId & 0x80000000) > 0 ? (Event.tagData.canTxOkMsg.canId & 0x7FFFFFFF) : Event.tagData.canTxOkMsg.canId;

                    if (Event.tagData.canTxOkMsg.msgFlags == XL_CANFD_RX_MessageFlags.XL_CAN_RXMSG_FLAG_NONE)
                    {
                        isFD = false;
                    }
                    else if ((Event.tagData.canTxOkMsg.msgFlags & XL_CANFD_RX_MessageFlags.XL_CAN_RXMSG_FLAG_EDL) > 0)
                    {
                        isFD = true;
                    }
                    base.NotifyMessageReceived(new Pmessage(can_id2,
                                                                    BuildDLC(Event.tagData.canTxOkMsg.dlc),
                                                                      Event.tagData.canTxOkMsg.data, TxRxType.Tx, isFD, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)));
                    break;
                //error message
                case XLDefine.XL_CANFD_RX_EventTags.XL_CAN_EV_TAG_TX_ERROR:
                case XLDefine.XL_CANFD_RX_EventTags.XL_CAN_EV_TAG_RX_ERROR:
                    base.NotifyMessageReceived(new Pmessage(0, 0, new byte[1], TxRxType.Err, false, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)));
                    break;
                default:
                    break;
            }
        }

        private byte BuildDLC(XL_CANFD_DLC DLC)
        {
            switch (DLC)
            {
                case XL_CANFD_DLC.DLC_CAN_CANFD_0_BYTES: return 0;
                case XL_CANFD_DLC.DLC_CAN_CANFD_1_BYTES: return 1;
                case XL_CANFD_DLC.DLC_CAN_CANFD_2_BYTES: return 2;
                case XL_CANFD_DLC.DLC_CAN_CANFD_3_BYTES: return 3;
                case XL_CANFD_DLC.DLC_CAN_CANFD_4_BYTES: return 4;
                case XL_CANFD_DLC.DLC_CAN_CANFD_5_BYTES: return 5;
                case XL_CANFD_DLC.DLC_CAN_CANFD_6_BYTES: return 6;
                case XL_CANFD_DLC.DLC_CAN_CANFD_7_BYTES: return 7;
                case XL_CANFD_DLC.DLC_CAN_CANFD_8_BYTES: return 8;
                case XL_CANFD_DLC.DLC_CANFD_12_BYTES: return 12;
                case XL_CANFD_DLC.DLC_CANFD_16_BYTES: return 16;
                case XL_CANFD_DLC.DLC_CANFD_20_BYTES: return 20;
                case XL_CANFD_DLC.DLC_CANFD_24_BYTES: return 24;
                case XL_CANFD_DLC.DLC_CANFD_32_BYTES: return 32;
                case XL_CANFD_DLC.DLC_CANFD_48_BYTES: return 48;
                case XL_CANFD_DLC.DLC_CANFD_64_BYTES: return 64;
                default: return 8;
            }
        }

        private uint Convert_Bandrate(CanBaudRate rate)
        {
            switch (rate)
            {
                case CanBaudRate.Baud_33Kbit: return 33333;
                case CanBaudRate.Baud_83Kbit: return 83333;
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

        private bool Send_CAN2_0_Msg(Pmessage msg, bool singleWireHighVoltage)
        {
            if (_can_param.BusProtocolMode == CANBus_ProtocolMode.CAN)
            {
                SingleWireHighVoltage_Config(singleWireHighVoltage);

                XLClass.xl_event canTxEvent = new XLClass.xl_event();

                canTxEvent.tag = XLDefine.XL_EventTags.XL_TRANSMIT_MSG;
                if ((msg.CANID & 0x1FFF0000) > 0)
                    canTxEvent.tagData.can_Msg.id = msg.CANID | 0x80000000;
                else
                    canTxEvent.tagData.can_Msg.id = msg.CANID;
                byte dlc = 0;
                if (msg.DLC > 8)
                    dlc = 8;
                else
                    dlc = msg.DLC;
                canTxEvent.tagData.can_Msg.dlc = (ushort)dlc;
                if (dlc > 0)
                {
                    for (int i = 0; i < dlc; i++)
                    {
                        canTxEvent.tagData.can_Msg.data[i] = msg.Data[i];
                    }
                }

                XLDefine.XL_Status xlStatus = XLDefine.XL_Status.XL_SUCCESS;
                xlStatus = CAN_Manager.Vector_Driver.XL_CanTransmit(_portHandle, _channelMask, canTxEvent);
                delayUs(0.2);
                if (xlStatus == XL_Status.XL_SUCCESS)
                {
                    messageTotalCounterSent += 1;
					/*
                    if (messageTotalCounterSent == MAX_TransmitSend)
                    {
                        Log.Logger_Run(string.Format("Send_CAN_FD_Msg -- XL_CanFlushTransmitQueue manually"));
                        Thread.Sleep(2);
                        CAN_Manager.Vector_Driver.XL_CanFlushTransmitQueue(_fd_portHandle, _channelMask);
                        Thread.Sleep(3);
                        messageTotalCounterSent = 0;
                    }
					*/
                    return true;
                }
                else
                {
                    Log.Logger_Run(string.Format("Send_CAN2_0_Msg -- XL_CanTransmit Error = {0}, {1}, {2}", xlStatus, msg.CANID.ToString("X"),messageTotalCounterSent));
                    Thread.Sleep(2);
                    messageTotalCounterSent = 0;
                    CAN_Manager.Vector_Driver.XL_CanFlushTransmitQueue(_portHandle, _channelMask);
                    Thread.Sleep(1);
                    xlStatus = CAN_Manager.Vector_Driver.XL_CanTransmit(_portHandle, _channelMask, canTxEvent);
                    if (xlStatus != XL_Status.XL_SUCCESS)
                    {
                        Log.Logger_Run(string.Format("Retry -- XL_CanTransmit Error = {0}, {1}", xlStatus, msg.CANID.ToString("X")));
                    }
                    else
                    { messageTotalCounterSent += 1; }
                }
                return false;
            }
            else
            {
                XLClass.XLcanTxEvent canTxEvent = new XLClass.XLcanTxEvent();
                canTxEvent.tag = XL_CANFD_TX_EventTags.XL_CAN_EV_TAG_TX_MSG;
                if ((msg.CANID & 0x1FFF0000) > 0)
                    canTxEvent.tagData.canId = msg.CANID | 0x80000000;
                else
                    canTxEvent.tagData.canId = msg.CANID;
                canTxEvent.tagData.dlc = GetDLCFromLength(msg.DLC);
                canTxEvent.tagData.msgFlags = XLDefine.XL_CANFD_TX_MessageFlags.XL_CAN_TXMSG_FLAG_NONE;
                for (int i = 0; i < msg.DLC; i++)
                {
                    canTxEvent.tagData.data[i] = msg.Data[i];
                }
                uint messageCounterSent = 0;
                XL_Status status;
                status = CAN_Manager.Vector_Driver.XL_CanTransmitEx(_fd_portHandle, _channelMask, ref messageCounterSent, canTxEvent);
                delayUs(0.2);
                if (status == XL_Status.XL_SUCCESS)
                {
                    messageTotalCounterSent += 1;
					/*
                    if (messageTotalCounterSent == MAX_TransmitSend)
                    {
                        Log.Logger_Run(string.Format("Send_CAN_FD_Msg -- XL_CanFlushTransmitQueue manually"));
                        Thread.Sleep(2);
                        CAN_Manager.Vector_Driver.XL_CanFlushTransmitQueue(_fd_portHandle, _channelMask);
                        Thread.Sleep(3);
                        messageTotalCounterSent = 0;
                    }
					*/
                    return true;
                }
                else
                {
                    Log.Logger_Run(string.Format("Send_CAN2_0_Msg -- XL_CanTransmitEx Error = {0}, {1}, {2}", status, msg.CANID.ToString("X"), messageTotalCounterSent));                    
                    Thread.Sleep(2);
                    messageTotalCounterSent = 0;
                    CAN_Manager.Vector_Driver.XL_CanFlushTransmitQueue(_fd_portHandle, _channelMask);
                    Thread.Sleep(1);
                    status = CAN_Manager.Vector_Driver.XL_CanTransmitEx(_fd_portHandle, _channelMask, ref messageCounterSent, canTxEvent);
                    if (status != XL_Status.XL_SUCCESS)
                    {
                        Log.Logger_Run(string.Format("Retry -- XL_CanTransmitEx Error = {0}, {1}", status, msg.CANID.ToString("X")));
                    }
                    else
                    { messageTotalCounterSent += 1; }
                }
                return false;
            }
        }

        private void SingleWireHighVoltage_Config(bool singleWireHighVoltage)
        {
            if (singleWireHighVoltage == true)
            {
                if (_sendByHighVol == false)
                {
                    CAN_Manager.Vector_Driver.XL_CanSetChannelTransceiver(_portHandle, _channelMask, XL_Transceiver_Types.XL_TRANSCEIVER_TYPE_CAN_SWC, XL_Transceiver_OperationModes.XL_TRANSCEIVER_LINEMODE_SWC_WAKEUP, 0);
                    _sendByHighVol = true;
                }
            }
            else
            {
                if (_sendByHighVol == true)
                {
                    CAN_Manager.Vector_Driver.XL_CanSetChannelTransceiver(_portHandle, _channelMask, XL_Transceiver_Types.XL_TRANSCEIVER_TYPE_CAN_SWC, XL_Transceiver_OperationModes.XL_TRANSCEIVER_LINEMODE_SWC_NORMAL, 0);
                    _sendByHighVol = false;
                }
            }
        }

        private double delayUs(double time)
        {
            System.Diagnostics.Stopwatch stopTime = new System.Diagnostics.Stopwatch();

            stopTime.Start();
            while (stopTime.Elapsed.TotalMilliseconds < time) { }
            stopTime.Stop();

            return stopTime.Elapsed.TotalMilliseconds;
        }

        private bool Send_CAN_FD_Msg(Pmessage msg)
        {
            XLClass.XLcanTxEvent canTxEvent = new XLClass.XLcanTxEvent();
            canTxEvent.tag = XL_CANFD_TX_EventTags.XL_CAN_EV_TAG_TX_MSG;
            if ((msg.CANID & 0x1FFF0000) > 0)
                canTxEvent.tagData.canId = msg.CANID | 0x80000000;
            else
                canTxEvent.tagData.canId = msg.CANID;
            canTxEvent.tagData.dlc = GetDLCFromLength(msg.DLC);
            canTxEvent.tagData.msgFlags = XLDefine.XL_CANFD_TX_MessageFlags.XL_CAN_TXMSG_FLAG_BRS | XLDefine.XL_CANFD_TX_MessageFlags.XL_CAN_TXMSG_FLAG_EDL;
            for (int i = 0; i < msg.DLC; i++)
            {
                canTxEvent.tagData.data[i] = msg.Data[i];
            }
            uint messageCounterSent = 1;
            XL_Status status;
            status = CAN_Manager.Vector_Driver.XL_CanTransmitEx(_fd_portHandle, _channelMask, ref messageCounterSent, canTxEvent);
            delayUs(0.2); //DO NOT CHANGE TIME DELAY
            if (status == XL_Status.XL_SUCCESS)
            {
                messageTotalCounterSent += 1;
				/*
                if (messageTotalCounterSent == MAX_TransmitSend)
                {
                    Log.Logger_Run(string.Format("Send_CAN_FD_Msg -- XL_CanFlushTransmitQueue manually"));
                    Thread.Sleep(2);
                    CAN_Manager.Vector_Driver.XL_CanFlushTransmitQueue(_fd_portHandle, _channelMask);
                    Thread.Sleep(3);
                    messageTotalCounterSent = 0;
                }*/
                return true;
            }
            else
            {
                Log.Logger_Run(string.Format("Send_CAN_FD_Msg -- XL_CanTransmitEx Error = {0}, {1}, {2}", status, msg.CANID.ToString("X"), messageTotalCounterSent));
                Thread.Sleep(2);
                messageTotalCounterSent = 0;
                CAN_Manager.Vector_Driver.XL_CanFlushTransmitQueue(_fd_portHandle, _channelMask);
                Thread.Sleep(1);
                status = CAN_Manager.Vector_Driver.XL_CanTransmitEx(_fd_portHandle, _channelMask, ref messageCounterSent, canTxEvent);
                if (status != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run(string.Format("Retry -- XL_CanTransmitEx Error = {0}, {1}", status, msg.CANID.ToString("X")));
                }
                else
                { messageTotalCounterSent += 1; }
            }
            return false;
        }

        private bool SetNormBaudrate(CAN_Parameter target_can_param)
        {
            try
            {
                if (_driver_hasError)
                    return false;

                XL_Status status;
                if (target_can_param.BaudRate == CanBaudRate.Unknown)
                {
                    if (target_can_param.BusPhysicType == CANBus_PhysicType.singleWire)
                        target_can_param.BaudRate = CanBaudRate.Baud_33Kbit;
                    else
                        target_can_param.BaudRate = CanBaudRate.Baud_500Kbit;
                }
                status = CAN_Manager.Vector_Driver.XL_CanSetChannelBitrate(_portHandle, _channelMask, Convert_Bandrate(target_can_param.BaudRate));
                if (status == XL_Status.XL_SUCCESS)
                {
                    _can_param.BaudRate = target_can_param.BaudRate;
                    return true;
                }
                else
                {
					Log.Logger_Run(string.Format("SetNormBaudrate -- XL_CanSetChannelBitrate Error = {0}", status));
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("CAN_Driver_Vector -- SetNormBaudrate() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        private bool SetFDBaudrate(CAN_Parameter target_can_param)
        {
            try
            {
                if (_driver_hasError)
                    return false;

                XL_Status status;
                if (target_can_param.Arbitration_rate == CanBaudRate.Unknown)
                    target_can_param.Arbitration_rate = CanBaudRate.Baud_500Kbit;
                if (target_can_param.Data_rate == CanBaudRate.Unknown)
                    target_can_param.Data_rate = CanBaudRate.Baud_2Mbit;
                if (target_can_param.Abr_sjw == 0xff || target_can_param.Abr_tseg1 == 0xff || target_can_param.Abr_tseg2 == 0xff || target_can_param.Data_sjw == 0xff || target_can_param.Data_tseg1 == 0xff || target_can_param.Data_tseg2 == 0xff)
                {
                    //target_can_param.Abr_sjw = 1;
                    //target_can_param.Abr_tseg1 = 3;
                    //target_can_param.Abr_tseg2 = 8;
                    target_can_param.Abr_sjw = 2;
                    target_can_param.Abr_tseg1 = 63;
                    target_can_param.Abr_tseg2 = 16;
                    //target_can_param.Data_sjw = 1;
                    //target_can_param.Data_tseg1 = 2;
                    //target_can_param.Data_tseg2 = 1;
                    target_can_param.Data_sjw = 2;
                    target_can_param.Data_tseg1 = 14;
                    target_can_param.Data_tseg2 = 5;
                }
                XLClass.XLcanFdConf canFdConf = new XLClass.XLcanFdConf()
                {
                    arbitrationBitRate = Convert_Bandrate(target_can_param.Arbitration_rate),
                    sjwAbr = target_can_param.Abr_sjw,
                    tseg1Abr = target_can_param.Abr_tseg1,
                    tseg2Abr = target_can_param.Abr_tseg2,
                    dataBitRate = Convert_Bandrate(target_can_param.Data_rate),
                    sjwDbr = target_can_param.Data_sjw,
                    tseg1Dbr = target_can_param.Data_tseg1,
                    tseg2Dbr = target_can_param.Data_tseg2,
                };
                status = CAN_Manager.Vector_Driver.XL_CanFdSetConfiguration(_fd_portHandle, _channelMask, canFdConf);
                if (status == XL_Status.XL_SUCCESS)
                {
                    _can_param.Arbitration_rate = target_can_param.Arbitration_rate;
                    _can_param.Abr_sjw = target_can_param.Abr_sjw;
                    _can_param.Abr_tseg1 = target_can_param.Abr_tseg1;
                    _can_param.Abr_tseg2 = target_can_param.Abr_tseg2;
                    _can_param.Data_rate = target_can_param.Data_rate;
                    _can_param.Data_sjw = target_can_param.Data_sjw;
                    _can_param.Data_tseg1 = target_can_param.Data_tseg1;
                    _can_param.Data_tseg2 = target_can_param.Data_tseg2;
                    return true;
                }
                else
                {	
			        Log.Logger_Run(string.Format("SetFDBaudrate -- XL_CanFdSetConfiguration Error = {0}", status));
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("CAN_Driver_Vector -- SetFDBaudrate() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        private XLDefine.XL_CANFD_DLC GetDLCFromLength(uint Length)
        {
            switch (Length)
            {
                case 0: return XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_0_BYTES;
                case 1: return XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_1_BYTES;
                case 2: return XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_2_BYTES;
                case 3: return XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_3_BYTES;
                case 4: return XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_4_BYTES;
                case 5: return XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_5_BYTES;
                case 6: return XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_6_BYTES;
                case 7: return XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_7_BYTES;
                case 8: return XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_8_BYTES;
                case 12: return XLDefine.XL_CANFD_DLC.DLC_CANFD_12_BYTES;
                case 16: return XLDefine.XL_CANFD_DLC.DLC_CANFD_16_BYTES;
                case 20: return XLDefine.XL_CANFD_DLC.DLC_CANFD_20_BYTES;
                case 24: return XLDefine.XL_CANFD_DLC.DLC_CANFD_24_BYTES;
                case 32: return XLDefine.XL_CANFD_DLC.DLC_CANFD_32_BYTES;
                case 48: return XLDefine.XL_CANFD_DLC.DLC_CANFD_48_BYTES;
                case 64: return XLDefine.XL_CANFD_DLC.DLC_CANFD_64_BYTES;
                default: return XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_8_BYTES;
            }
        }
    }
}

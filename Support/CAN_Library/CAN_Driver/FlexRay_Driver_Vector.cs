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
using System.Linq;
using Fibex_Parser;

namespace CAN_Library.CAN_Driver
{
    class FlexRay_Driver_Vector : FlexRay_Driver
    {
        Vector_Device _myDevice;
        uint _channelIndex;//channel index in system
        ulong _channelMask;//channel mask in system
        uint _hwChannel;//channel index in own device
        FlexRay_Parameter _flex_param;

        Thread _rxThread;//to get standard messages in CAN bus
        bool _isActive_RxThread;//to control RxThread
        int _portHandle = -1;//standard CAN port handler
        int _eventHandle = -1;//standard CAN event handler

        Thread _fd_rxThread;//to receive FD messages in CAN bus
        bool _isActive_fd_rxThread;//to control FD_RxThread
        int _fd_portHandle = -1;//FD CAN port handler
        int _fd_eventHandle = -1;//FD CAN event handler
        ulong send_conter = 0;
        ulong stop_conter = 0;
		
        bool _driver_hasError = false;//device comes into a exception
        bool _driver_hasInit = false;
        //const
        private const uint RX_FIFO_SIZE = 32768;
        private const uint PAYLOAD_LENGTH = 32;
        //
        FlexRay_Message _ErayMsg;  // used as startup frame for eray
        FlexRay_Message _ColdMsg;  // used as startup frame for cold

        private XLClass.xl_fr_cluster_configuration frClusterConfig = new XLClass.xl_fr_cluster_configuration();

        public object XL_Flexray_EventTags { get; private set; }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WaitForSingleObject(int handle, int timeOut);

        public FlexRay_Driver_Vector(string deviceName, byte channelID, FlexRay_Parameter flex_param)
        {
            try
            {
                if (CAN_Manager.Vector_Device_List.Count > 0)
                {
                    if (CAN_Manager.Vector_Device_List.Any(p => p.Display_Name == deviceName))
                    {
                        this._myDevice = CAN_Manager.Vector_Device_List.First(p => p.Display_Name == deviceName);
                        this._hwChannel = channelID;
                        this._flex_param = flex_param;
                        if (CAN_Manager.Vector_driverConfig.channelCount > 0)
                        {
                            if (CAN_Manager.Vector_driverConfig.channel.Any(p => p.serialNumber.ToString() == deviceName.Split('-')[2]))
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
                Log.Logger_Run("FlexRay_Driver_Vector -- FlexRay_Driver_Vector() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
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

        protected override bool DoInitialize(string flbex_file)
        {
            Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- DoInitialize"));
            try
            {
                ///make sure driver is ok and device has not lost
                if (this._driver_hasError)
                {
                    Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- DoInitialize: driver_hasError"));
                    return false;
                }

                getKicense();

                XL_Status status;
                //status = CAN_Manager.Vector_Driver.XL_OpenDriver();

                ///create a new application in the Vector Hardware Config tool if there's no one's name is Appname
                ///if there is such one, then set the app channel configuration in the existing application
                ///then create or set app channel which is the parameter "appChannel" in Vector Hardware Config tool, assign it to real device channel which parameters is HWType,HWIndex,HWChannel,BusTypes
                uint hwIndex = 0;
                uint hwChannel = 0;
                status = CAN_Manager.Vector_Driver.XL_SetApplConfig(AppName, _channelIndex, _myDevice.HWType, _myDevice.HWIndex, _hwChannel, XL_BusTypes.XL_BUS_TYPE_FLEXRAY);
                if (status != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- XL_SetApplConfig Error = {0}", status));
                    return false;
                }
                ///get an app channel's info which is on XL_BUS_TYPE_FLEXRAY
                XL_HardwareType hwType = XL_HardwareType.XL_HWTYPE_NONE;
                status = CAN_Manager.Vector_Driver.XL_GetApplConfig(AppName, _channelIndex, ref hwType, ref hwIndex, ref hwChannel, XL_BusTypes.XL_BUS_TYPE_FLEXRAY);
                if (status != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- XL_GetApplConfig Error = {0}", status));
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
                        Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- XL_GetChannelMask Error, return channelIndex = {0}, _channelIndex = {1}", channelIndex, _channelIndex));
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                status = CAN_Manager.Vector_Driver.XL_OpenPort(ref _portHandle, AppName, _channelMask, ref permissionMask, RX_FIFO_SIZE, XL_InterfaceVersion.XL_INTERFACE_VERSION_V4, XL_BusTypes.XL_BUS_TYPE_FLEXRAY);
                if (status != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run("FlexRay_Driver_Vector: Fail to XL_OpenPort ");
                    return false;
                }


                //Set FlexRay configuration    
                if (InitFlexRayConfiguration(flbex_file, ref frClusterConfig) != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run("FlexRay_Driver_Vector: Fail to InitFlexRayConfiguration ");
                    return false;
                }


                // Define startup and sync frame
                if (InitStartupNormal() != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run("FlexRay_Driver_Vector: Fail to InitStartupNormal ");
                    return false;
                }

                // Get RX event handle
                if (CAN_Manager.Vector_Driver.XL_SetNotification(_portHandle, ref _eventHandle, 1) != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run("FlexRay_Driver_Vector: Fail to XL_SetNotification ");
                    return false;
                }

                //if (CAN_Manager.Vector_Driver.XL_ResetClock(_portHandle) != XL_Status.XL_SUCCESS)
                //{ return false; }

                status = CAN_Manager.Vector_Driver.XL_ActivateChannel(_portHandle, _channelMask, XL_BusTypes.XL_BUS_TYPE_FLEXRAY, XL_AC_Flags.XL_ACTIVATE_RESET_CLOCK);
                if (status != XL_Status.XL_SUCCESS)
                {
                    Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- XL_ActivateChannel Error = {0}", status));
                    return false;
                }


                this._isActive_RxThread = true;
                this._rxThread = new Thread(new ThreadStart(RX_Thread));
                this._rxThread.IsBackground = true;
                this._rxThread.Start();

                _myDevice.IsOpen = true;
                this._driver_hasInit = true;
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("FlexRay_Driver_Vector -- DoInitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        protected override bool DoUninitialize()
        {
            Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- DoUninitialize"));
            try
            {
                if (_driver_hasInit)
                {
                    if (this._rxThread != null)
                    {
                        if (_rxThread.IsAlive)
                        {
                            _isActive_RxThread = false;
                            //_rxThread.Join();
                        }
                    }

                    XL_Status status = CAN_Manager.Vector_Driver.XL_ClosePort(_portHandle);
                    if (status != XL_Status.XL_SUCCESS)
                    {
                        Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- XL_ClosePort Error = {0}", status));
                        return false;
                    }
                    _driver_hasInit = false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("FlexRay_Driver_Vector -- DoUninitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Configures the FlexRay node
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        private XLDefine.XL_Status InitFlexRayConfiguration(string flbex_file, ref XLClass.xl_fr_cluster_configuration xlConf)
        {
            FlexRayConfiguration frConfig = new FlexRayConfiguration();
            FibexConfiguration fibex = new FibexConfiguration();
            string result = fibex.GetConfig_Vector(flbex_file, ref xlConf).ToString();
            return CAN_Manager.Vector_Driver.XL_FrSetConfiguration(_portHandle, _channelMask, ref xlConf);
        }
        // -

        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// setup the startup and sync frames for the E-Ray
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        private XLDefine.XL_Status InitStartupNormal()
        {

            XLClass.xl_fr_set_modes xlFrMode = new XLClass.xl_fr_set_modes();
            XLClass.xl_fr_event xlFrConfigFrame = new XLClass.xl_fr_event();
            XLDefine.XL_Status xlStatus = XLDefine.XL_Status.XL_ERROR;

            // setup the startup and sync frames for the E-Ray
            xlFrConfigFrame.tag = XLDefine.XL_FlexRay_EventTags.XL_FR_TX_FRAME;
            xlFrConfigFrame.flagsChip = XLDefine.XL_FlexRay_FlagsChip.XL_FR_CHANNEL_A;
            xlFrConfigFrame.size = 0;
            xlFrConfigFrame.userHandle = 0;
            xlFrConfigFrame.tagData.frTxFrame.flags = XLDefine.XL_FlexRay_FrameFlags.XL_FR_FRAMEFLAG_REQ_TXACK | XLDefine.XL_FlexRay_FrameFlags.XL_FR_FRAMEFLAG_STARTUP | XLDefine.XL_FlexRay_FrameFlags.XL_FR_FRAMEFLAG_SYNC;
            xlFrConfigFrame.tagData.frTxFrame.offset = (byte)_ErayMsg.OffSet;
            xlFrConfigFrame.tagData.frTxFrame.repetition = (byte)_ErayMsg.Repetition;
            xlFrConfigFrame.tagData.frTxFrame.payloadLength = (byte)frClusterConfig.gPayloadLengthStatic;
            xlFrConfigFrame.tagData.frTxFrame.slotID = (byte)_ErayMsg.ID;
            xlFrConfigFrame.tagData.frTxFrame.txMode = XLDefine.XL_FlexRay_TXMode.XL_FR_TX_MODE_CYCLIC;
            xlFrConfigFrame.tagData.frTxFrame.incrementOffset = 0;
            xlFrConfigFrame.tagData.frTxFrame.incrementSize = 0;

            // copy payload into event buffer to be sent
            for (int i = 0; i < _ErayMsg.Data.Length; i++) xlFrConfigFrame.tagData.frTxFrame.data[i] = 0; // _ErayMsg.Data[i];

            // initialize startup frame
            xlStatus = CAN_Manager.Vector_Driver.XL_FrInitStartupAndSync(_portHandle, _channelMask, xlFrConfigFrame);
            if (xlStatus != XLDefine.XL_Status.XL_SUCCESS)
            {
                Log.Logger_Run(string.Format("FlexRay_Driver_Vector: Fail to XL_FrInitStartupAndSync for Eray CC,  return = {0}", xlStatus));
                return xlStatus;
            }

            // setup the mode for the E-Ray CC
            xlFrMode.frMode = XLDefine.XL_FlexRay_Mode.XL_FR_MODE_NORMAL;
            xlFrMode.frStartupAttributes = XLDefine.XL_FlexRay_StartupAttributes.XL_FR_MODE_COLDSTART_LEADING;
            xlStatus = CAN_Manager.Vector_Driver.XL_FrSetMode(_portHandle, _channelMask, xlFrMode);
            if (xlStatus != XLDefine.XL_Status.XL_SUCCESS)
            {
                Log.Logger_Run(string.Format("FlexRay_Driver_Vector: Fail to XL_FrSetMode for Eray CC,  return = {0}", xlStatus));
                return xlStatus;
            }
            // setup the startup and sync frames for the COLD CC
            // NEEDS advanced LICENSE!
            xlFrConfigFrame.tag = XLDefine.XL_FlexRay_EventTags.XL_FR_TX_FRAME;
            xlFrConfigFrame.flagsChip = XLDefine.XL_FlexRay_FlagsChip.XL_FR_CC_COLD_A;
            xlFrConfigFrame.size = 0;
            xlFrConfigFrame.userHandle = 0;
            xlFrConfigFrame.tagData.frTxFrame.flags = XLDefine.XL_FlexRay_FrameFlags.XL_FR_FRAMEFLAG_REQ_TXACK | XLDefine.XL_FlexRay_FrameFlags.XL_FR_FRAMEFLAG_STARTUP | XLDefine.XL_FlexRay_FrameFlags.XL_FR_FRAMEFLAG_SYNC;
            xlFrConfigFrame.tagData.frTxFrame.offset = (byte)_ColdMsg.OffSet;
            xlFrConfigFrame.tagData.frTxFrame.repetition = (byte)_ColdMsg.Repetition;
            xlFrConfigFrame.tagData.frTxFrame.payloadLength = (byte)frClusterConfig.gPayloadLengthStatic;// (byte)(payload.Length / 2);
            xlFrConfigFrame.tagData.frTxFrame.slotID = (byte)_ColdMsg.ID;
            xlFrConfigFrame.tagData.frTxFrame.txMode = XLDefine.XL_FlexRay_TXMode.XL_FR_TX_MODE_CYCLIC;
            xlFrConfigFrame.tagData.frTxFrame.incrementOffset = 0;
            xlFrConfigFrame.tagData.frTxFrame.incrementSize = 0;

            for (int i = 0; i < _ColdMsg.Data.Length; i++) xlFrConfigFrame.tagData.frTxFrame.data[i] = 0; // _ColdMsg.Data[i];

            // setup the mode for the COLD CC
            xlStatus = CAN_Manager.Vector_Driver.XL_FrInitStartupAndSync(_portHandle, _channelMask, xlFrConfigFrame);

            // check COLD CC license
            if (xlStatus == XLDefine.XL_Status.XL_ERR_NO_LICENSE)
            {
                Log.Logger_Run(string.Format("FlexRay_Driver_Vector: Fail to XL_FrInitStartupAndSync for COLD CC,  return = {0}", xlStatus));
                return xlStatus;
            }

            if (xlStatus != XLDefine.XL_Status.XL_SUCCESS)
            {
                Log.Logger_Run(string.Format("FlexRay_Driver_Vector: Fail to XL_FrInitStartupAndSync for COLD CC,  return = {0}", xlStatus));
                return xlStatus;
            }

            // setup the mode for the COLD CC
            xlFrMode.frMode = XLDefine.XL_FlexRay_Mode.XL_FR_MODE_COLD_NORMAL;
            xlFrMode.frStartupAttributes = XLDefine.XL_FlexRay_StartupAttributes.XL_FR_MODE_COLDSTART_LEADING;
            xlStatus = CAN_Manager.Vector_Driver.XL_FrSetMode(_portHandle, _channelMask, xlFrMode);
            return xlStatus;
        }

        protected override void DoInitStartupFrames(FlexRay_Message ErayMsg, FlexRay_Message ColdMsg)
        {
            _ErayMsg = ErayMsg;
            _ColdMsg = ColdMsg;
        }

        protected override bool DoSend(FlexRay_Message msg)
        {
            try
            {
                if (_driver_hasError)
                {
                    Log.Logger_Run(string.Format("FlexRay_Driver_Vector -- DoSend: driver_hasError"));
                    return false;
                }
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
                Log.Logger_Run("FlexRay_Driver_Vector -- DoSend() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }


        protected override bool DoSendRawData(FlexRay_Message Msg)
        {
            return true;
        }

        protected override bool DoPause_FlexRay_Cyclic_Message(FlexRay_Message msg)
        {
            if (_driver_hasError)
                return false;
            if (!_driver_hasInit)
                return false;

            if (msg == null)
            {
                return false;
            }
            if (msg.Name == _ErayMsg.Name || msg.Name == _ColdMsg.Name)
            {
                return true;
            }

            msg.Method = FR_TX_Mode.SingleShot;
            bool r = Send_FR_Msg(msg);

            msg.Method = FR_TX_Mode.Cyclic;  //revert back to default cycle
            return r;
        }

        protected override bool DoChangeSignalAndSend(FlexRay_Message Msg, string signal_name, long raw_value)
        {
            return true;
        }

        private void RX_Thread()
        {
            try
            {
                Thread.Sleep(100);

                XLClass.xl_fr_event receivedEvent = new XLClass.xl_fr_event();
                XL_Status xlStatus = XL_Status.XL_SUCCESS;
                WaitResults waitResult = new WaitResults();
                while (_isActive_RxThread)
                {
                    int queuelevel = 0;
                    CAN_Manager.Vector_Driver.XL_GetReceiveQueueLevel(_portHandle, ref queuelevel);
                    if (queuelevel == 0)
                    {
                        // Wait for hardware events - we only get informed once if the queue level crosses the queue-notify-level!
                        waitResult = (XLDefine.WaitResults)WaitForSingleObject(_eventHandle, 1000);
                    }
                    else
                    {
                        // Some elements are already in queue
                        waitResult = XLDefine.WaitResults.WAIT_OBJECT_0;
                    }

                    if (waitResult != WaitResults.WAIT_TIMEOUT)
                    {
                        xlStatus = XL_Status.XL_SUCCESS;
                        while (xlStatus != XL_Status.XL_ERR_QUEUE_IS_EMPTY)
                        {
                            xlStatus = CAN_Manager.Vector_Driver.XL_FrReceive(_portHandle, ref receivedEvent);
                            if (xlStatus == XL_Status.XL_SUCCESS)
                            {
                                //Console.WriteLine(CAN_Manager.Vector_Driver.XL_GetFREventString(receivedEvent));
                                build_Message(receivedEvent);
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

        //callback message  build
        private void build_Message(XLClass.xl_fr_event Event)
        {
            Fmessage New_Received = null;

            switch (Event.tag)
            {
                case XL_FlexRay_EventTags.XL_FR_RX_FRAME:
                    //Console.WriteLine("###############################################RX:: XL_FR_RX_FRAME");
                    //Console.WriteLine(string.Format("SlotID: {0}, Data: {1}", Event.tagData.frRxFrame.slotID,
                    //        BitConverter.ToString(Event.tagData.frRxFrame.data, 0, 32).Replace("-", " ").ToUpper()));
                    if (Event.tagData.frRxFrame.flags != XLDefine.XL_FlexRay_FrameFlags.XL_FR_FRAMEFLAG_NULLFRAME && Event.tagData.frRxFrame.payloadLength != 0)
                    {  
                        New_Received = new Fmessage();
                        New_Received.FrameID = Event.tagData.frRxFrame.slotID;
                        New_Received.CycleCount = Event.tagData.frRxFrame.cycleCount;
                        New_Received.DLC = (uint)Event.tagData.frRxFrame.payloadLength * 2;
                        New_Received.Data = Event.tagData.frRxFrame.data.Take((int)New_Received.DLC).ToArray();
                        New_Received.TxRx = TxRxType.Rx;
                        New_Received.LastReceivedTimeMs = (long)Convert.ToUInt64(DateTime.Now.Ticks / 10000L);
                        base.NotifyMessageReceived(New_Received);
                    }
                    break;

                case XL_FlexRay_EventTags.XL_FR_TXACK_FRAME:
                    if (Event.tagData.frTxAckFrame.flags != XLDefine.XL_FlexRay_FrameFlags.XL_FR_FRAMEFLAG_NULLFRAME && Event.tagData.frTxAckFrame.payloadLength != 0)
                    {
                        New_Received = new Fmessage();
                        New_Received.FrameID = Event.tagData.frTxAckFrame.slotID;
                        New_Received.CycleCount = Event.tagData.frTxAckFrame.cycleCount;
                        New_Received.DLC = (uint)Event.tagData.frTxAckFrame.payloadLength * 2;
                        New_Received.Data = Event.tagData.frTxAckFrame.data.Take((int)New_Received.DLC).ToArray();
                        New_Received.TxRx = TxRxType.Tx;
                        New_Received.LastReceivedTimeMs = (long)Convert.ToUInt64(DateTime.Now.Ticks / 10000L);
                        base.NotifyMessageReceived(New_Received);
                    }

                    break;

                case XL_FlexRay_EventTags.XL_FR_TX_FRAME:
                    //Console.WriteLine(":: XL_FR_TX_FRAME");
                    break;
                case XL_FlexRay_EventTags.XL_FR_INVALID_FRAME:
                    //Console.WriteLine("INVALID-FRAME: XL_FR_INVALID_FRAME");
                    break;

                case XL_FlexRay_EventTags.XL_FR_START_CYCLE:
                    //Console.WriteLine("::XL_FR_START_CYCLE");
                    break;

                case XL_FlexRay_EventTags.XL_FR_STATUS:
                    //Console.WriteLine("::XL_FR_STATUS");
                    break;

                case XL_FlexRay_EventTags.XL_FR_SPY_FRAME:
                    //Console.WriteLine("::XL_FR_SPY_FRAME");
                    break;

                default:
                    //Console.WriteLine(String.Format("unknown xl_fr_event event tag: {0}", Event.tag));
                    break;
            }
        }


        private bool Send_FR_Msg(FlexRay_Message msg)
        {

            XLClass.xl_fr_event xlFrEvent = new XLClass.xl_fr_event();

            xlFrEvent.tag = XLDefine.XL_FlexRay_EventTags.XL_FR_TX_FRAME;
            xlFrEvent.flagsChip = XLDefine.XL_FlexRay_FlagsChip.XL_FR_CHANNEL_A;
            xlFrEvent.size = 0; // Calculated inside XL-API DLL - In case of event used in xlFrTransmit: Parameter not used
            xlFrEvent.userHandle = 0; // Internal use
            xlFrEvent.tagData.frTxFrame.flags = XLDefine.XL_FlexRay_FrameFlags.XL_FR_FRAMEFLAG_REQ_TXACK;

            xlFrEvent.tagData.frTxFrame.offset = (byte)msg.OffSet;
            xlFrEvent.tagData.frTxFrame.repetition = (byte)msg.Repetition;

            xlFrEvent.tagData.frTxFrame.slotID = (ushort)msg.ID;
            xlFrEvent.tagData.frTxFrame.payloadLength = (byte)(msg.DLC / 2);

            if (msg.Method == FR_TX_Mode.Cyclic)
            {
                xlFrEvent.tagData.frTxFrame.txMode = XLDefine.XL_FlexRay_TXMode.XL_FR_TX_MODE_CYCLIC;
                send_conter++;
            }
            else if (msg.Method == FR_TX_Mode.SingleShot)
            {
                xlFrEvent.tagData.frTxFrame.txMode = XLDefine.XL_FlexRay_TXMode.XL_FR_TX_MODE_SINGLE_SHOT;
            }
            else if (msg.Method == FR_TX_Mode.None)
            {
                xlFrEvent.tagData.frTxFrame.txMode = XLDefine.XL_FlexRay_TXMode.XL_FR_TX_MODE_NONE;
                stop_conter++;
            }

            xlFrEvent.tagData.frTxFrame.incrementOffset = 0;
            xlFrEvent.tagData.frTxFrame.incrementSize = 0;  

            // copy payload into event buffer to be sent
            for (int i = 0; i < msg.DLC; i++) xlFrEvent.tagData.frTxFrame.data[i] = msg.Data[i];

            XLDefine.XL_Status status = CAN_Manager.Vector_Driver.XL_FrTransmit(_portHandle, _channelMask, ref xlFrEvent);
            if (status == XL_Status.XL_SUCCESS)
            {
                return true;
            }
            else
            {
                Log.Logger_Run(string.Format("Send_FR_Msg -- XL_FrTransmit Error = {0}, {1}, {2}, {3}", status, msg.ID.ToString("X"), send_conter, stop_conter));
            }
            return false;
        }

    
    }
}

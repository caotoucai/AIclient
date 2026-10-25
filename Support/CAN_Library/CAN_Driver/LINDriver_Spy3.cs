using System;
using System.Linq;
using System.Threading;
using CAN_Library;
using System.Runtime.InteropServices;
using CAN_Library.CAN_Interface;
using CAN_Library.CAN_Parameter;
using System.Collections.Generic;
using CSnet;

namespace CAN_Library.CAN_Driver
{
    class LINDriver_Spy3 : LINDriver
    {
        Spy3_Device _myDevice;
        byte _channel_ID;
        LIN_Parameter _lin_param;
        bool _driver_hasError = false;
        bool _driver_hasInit = false;
        bool _autoChecksum = true;
        bool _isMaster = true;

        public LINDriver_Spy3(string deviceName, byte channelID, LIN_Parameter lin_param)
        {
            try
            {
                if (CAN_Manager.Spy3_Device_List.Any(p => p.Display_Name == deviceName))
                {
                    this._myDevice = CAN_Manager.Spy3_Device_List.First(p => p.Display_Name == deviceName);
                    this._channel_ID = channelID;
                    this._lin_param = lin_param;
                    HW_Device = this._myDevice.HW_Device;
                    m_hObject = this._myDevice.m_hObject;
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
        public byte[] bNetwork = new byte[255];//network channels reference
        public NeoDevice HW_Device;
        public IntPtr m_hObject;//device handler
        System.Threading.Thread _rxThread;
        bool _isActive_rxThread = false;
        List<int> InitID = new List<int>();
        icsSpyMessage[] _stMessages = new icsSpyMessage[20000];

        protected override bool DoInitialize(LIN_Parameter lin_param)
        {
            try
            {
                ///make sure driver is ok and device has not lost
                if (this._driver_hasError)
                {
                    return false;
                }
                if (!CAN_Manager.Spy3_Device_List.First(p => p.Display_Name == _myDevice.Display_Name).IsOpen)
                {
                    for (int iCount = 0; iCount < 255; iCount++)
                    {
                        bNetwork[iCount] = Convert.ToByte(iCount);
                    }
                    
                    _myDevice.pNumberOfErrors = icsNeoDll.icsneoOpenNeoDevice(ref HW_Device, ref m_hObject, ref bNetwork[0], 1, 0);
                    //Console.WriteLine("LIN open:" + m_hObject + "    error code:" + _myDevice.pNumberOfErrors);
                    if (_myDevice.pNumberOfErrors != 1)
                    {
                        return false;
                    }
                    CAN_Manager.Spy3_Device_List.First(p => p.Display_Name == _myDevice.Display_Name).m_hObject = m_hObject;
                    CAN_Manager.Spy3_Device_List.First(p => p.Display_Name == _myDevice.Display_Name).IsOpen = true;
                   
                }
                if (CAN_Manager.Spy3_Device_List.First(p => p.Display_Name == _myDevice.Display_Name).IsOpen)
                {
                    m_hObject = CAN_Manager.Spy3_Device_List.First(p => p.Display_Name == _myDevice.Display_Name).m_hObject;
                    if (!DoSetBaudrate(lin_param.Baudrate))
                    { return false; }

                    ///start messages callback thread
                    this._rxThread = new System.Threading.Thread(new System.Threading.ThreadStart(Rx_Thread));
                    this._isActive_rxThread = true;
                    this._rxThread.Start();
                    if (InitID.IndexOf(_channel_ID) < 0)
                    {
                        InitID.Add(_channel_ID);
                    }


                    _myDevice.Spy3MessageEvent_lin += LINMessageCallback;
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
        private void Rx_Thread()
        {
            try
            {
                while (this._isActive_rxThread)
                {
                    this.ReadMessages();
                    System.Threading.Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.11-Rx_Thread() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }
        private void ReadMessages()
        {
            try
            {
                int lNumberOfMessages = 0;
                int lNumberOfErrors = 0;
                int status = 0;
                Lmessage New_Received = null;

                if (_isActive_rxThread)
                {
                    status = icsNeoDll.icsneoGetMessages(m_hObject, ref _stMessages[0], ref lNumberOfMessages, ref lNumberOfErrors);
                }
                else
                {
                    int iNumberOfErrors = 0;
                    if (icsNeoDll.icsneoClosePort(m_hObject, ref iNumberOfErrors) == 1)
                    {
                        icsNeoDll.icsneoFreeObject(m_hObject);
                    }
                    return;
                }
                if (status == 1)
                {
                    if (lNumberOfMessages > 0)
                    {
                        for (long count = 0; count < lNumberOfMessages; count++)
                        {
                            TxRxType type = TxRxType.Rx;
                            if ((_stMessages[count].StatusBitField & Convert.ToInt32(eDATA_STATUS_BITFIELD_1.SPY_STATUS_TX_MSG)) > 0)
                            {
                                type = TxRxType.Tx;
                            }

                            if (_stMessages[count].Protocol == Convert.ToByte(ePROTOCOL.SPY_PROTOCOL_LIN))
                            {
                                if (_stMessages[count].NumberBytesData > 0)
                                {
                                    byte[] tempMsg = new byte[8];
                                    tempMsg[0] = _stMessages[count].Data1;
                                    tempMsg[1] = _stMessages[count].Data2;
                                    tempMsg[2] = _stMessages[count].Data3;
                                    tempMsg[3] = _stMessages[count].Data4;
                                    tempMsg[4] = _stMessages[count].Data5;
                                    tempMsg[5] = _stMessages[count].Data6;
                                    tempMsg[6] = _stMessages[count].Data7;
                                    tempMsg[7] = _stMessages[count].Data8;
                                    byte[] targetMsg = new byte[_stMessages[count].NumberBytesData];
                                    for (byte i = 0; i < _stMessages[count].NumberBytesData; i++)
                                    {
                                        targetMsg[i] = tempMsg[i];
                                    }

                                    //build lin msg
                                    New_Received = new Lmessage();
                                    New_Received.CheckSum = _stMessages[count].Data8;
                                    New_Received.ID = (uint)_stMessages[count].ArbIDOrHeader;
                                    New_Received.DLC = _stMessages[count].NumberBytesData;
                                    New_Received.Data = targetMsg;
                                    New_Received.TxRx = type;

                                    LINMessageCallback(_stMessages[count].NetworkID, New_Received);
                                }
                                else
                                {
                                    //Spy3MessageEvent_lin(new Pmessage((uint)_stMessages[count].ArbIDOrHeader, 0, null, type, false, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)), _stMessages[count].NetworkID);
                                }
                            }
 
                        }
                    }
                }
                else
                {
                    ///remind manager to scan device lost!!!
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.12-Rx_Thread() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }
        private void LINMessageCallback(byte channelID, Lmessage linMessage)
        {
            try
            {
                if (_driver_hasError)
                    return;
                if (_myDevice == null)
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

        protected override bool DoSend(LIN_Message msg)
        {
            try
            {
                if (_driver_hasError)
                    return false;
                if (!_driver_hasInit)
                    return false;
                //msg PID
                if (msg.FrameID == 0x0 || msg.FrameID == 0xFF)
                {
                    return false;
                }
                //msg.CalculateChecksum();
                //message make
   
                byte[] _dataToSend = new byte[8];
                if (msg.DLC > 0)
                {
                    for (int i = 0; i < msg.DLC; i++)
                    {
                        _dataToSend[i] = msg.Data[i];
                    }
                }

                icsSpyMessageJ1850 stJMsg=new icsSpyMessageJ1850();
                stJMsg.NetworkID = _channel_ID;
                //Set the LIN ID, use the Protected ID
                stJMsg.Header1 = (byte)msg.FrameID;
                //set the data data3-8 copy to data1-6
                stJMsg.Data1= _dataToSend[2];
                stJMsg.Data2 = _dataToSend[3];
                stJMsg.Data3 = _dataToSend[4];
                stJMsg.Data4 = _dataToSend[5];
                stJMsg.Data5 = _dataToSend[6];
                stJMsg.Data6 = _dataToSend[7];

                bool UseEnhanced = true;
                if (_isMaster)
                {//Set the Init Message flag. This tells the
                 //Hardware that this is a Master frame
                    stJMsg.StatusBitField = (int)eDATA_STATUS_BITFIELD_1.SPY_STATUS_INIT_MESSAGE;
                    stJMsg.StatusBitField2 = 0;
                }
                else//slave
                {
                    //Set message as Slave frame
                    stJMsg.StatusBitField = 0;
                }

                switch (msg.DLC)
                {
                    case 0:
                        //Make a header only type frame
                        stJMsg.NumberBytesHeader = 1;
                        stJMsg.NumberBytesData = 0;
                        break;
                    case 1:
                        //Only 1 byte of data
                        stJMsg.NumberBytesHeader = 2;
                        stJMsg.Header2 =_dataToSend[0];
                        stJMsg.NumberBytesData = 0;
                        //checksum goes into header byte 3 for this instance
                        stJMsg.Header3 = CalculateLINCheckSum(UseEnhanced, ref stJMsg);
                        stJMsg.NumberBytesHeader = 3;
                        break;
                    default:
                        //2 or more 
                        stJMsg.NumberBytesHeader = 3;
                        stJMsg.Header2 = _dataToSend[0];
                        stJMsg.Header3 = _dataToSend[1];
                        stJMsg.NumberBytesData = Convert.ToByte(msg.DLC-2);

                        //header byte
                        if (msg.DLC == 2) stJMsg.Data1 = CalculateLINCheckSum(UseEnhanced, ref stJMsg);
                        if (msg.DLC == 3) stJMsg.Data2 = CalculateLINCheckSum(UseEnhanced, ref stJMsg);
                        if (msg.DLC == 4) stJMsg.Data3 = CalculateLINCheckSum(UseEnhanced, ref stJMsg);
                        if (msg.DLC == 5) stJMsg.Data4 = CalculateLINCheckSum(UseEnhanced, ref stJMsg);
                        if (msg.DLC == 6) stJMsg.Data5 = CalculateLINCheckSum(UseEnhanced, ref stJMsg);
                        if (msg.DLC == 7) stJMsg.Data6 = CalculateLINCheckSum(UseEnhanced, ref stJMsg);
                        if (msg.DLC == 8) stJMsg.Data7 = CalculateLINCheckSum(UseEnhanced, ref stJMsg);
                        stJMsg.NumberBytesData = Convert.ToByte(msg.DLC - 1);
                        break;
                }

                _myDevice.pNumberOfErrors = icsNeoDll.icsneoTxMessages(m_hObject, ref stJMsg, stJMsg.NetworkID, 1);
                if (_myDevice.pNumberOfErrors == 1)
                {
                    return true;
                }
                //remind manager to scan device lost!!!
                return false;

            }
            catch (Exception ex)
            {
                Log.Logger_Run("Exception.message: " + ex.Message + "\r\n              " + "Exception.StackTrace:" + ex.StackTrace);
                return false;
            }
        }
        private byte CalculateLINCheckSum(bool UseEnhanced, ref icsSpyMessageJ1850 SpyMessageJ1850)
        {
            int ChecksumCalculation = 0;

            //Add a little error checking
            if (SpyMessageJ1850.NumberBytesHeader == 0 || SpyMessageJ1850.NumberBytesHeader == 1) return (0);

            //Get sum of all the bytes
            if (SpyMessageJ1850.NumberBytesHeader >= 2) ChecksumCalculation = ChecksumCalculation + SpyMessageJ1850.Header2;
            if (SpyMessageJ1850.NumberBytesHeader >= 3) ChecksumCalculation = ChecksumCalculation + SpyMessageJ1850.Header3;
            if (SpyMessageJ1850.NumberBytesData >= 1) ChecksumCalculation = ChecksumCalculation + SpyMessageJ1850.Data1;
            if (SpyMessageJ1850.NumberBytesData >= 2) ChecksumCalculation = ChecksumCalculation + SpyMessageJ1850.Data2;
            if (SpyMessageJ1850.NumberBytesData >= 3) ChecksumCalculation = ChecksumCalculation + SpyMessageJ1850.Data3;
            if (SpyMessageJ1850.NumberBytesData >= 4) ChecksumCalculation = ChecksumCalculation + SpyMessageJ1850.Data4;
            if (SpyMessageJ1850.NumberBytesData >= 5) ChecksumCalculation = ChecksumCalculation + SpyMessageJ1850.Data5;
            if (SpyMessageJ1850.NumberBytesData >= 6) ChecksumCalculation = ChecksumCalculation + SpyMessageJ1850.Data6;

            //If enhanced add in the protected ID
            if (UseEnhanced == true) ChecksumCalculation = ChecksumCalculation + SpyMessageJ1850.Header1;

            //Get number of carries and add to LSB
            ChecksumCalculation = ((ChecksumCalculation / 0x100) + ChecksumCalculation) & 0xFF;
            //Invert to get checksum
            return Convert.ToByte(0xFF ^ ChecksumCalculation);
        }

        protected override bool DoUninitialize()
        {
            try
            {
                bool anyConnect = false;
                foreach (var channel in CAN_Manager.Available_Channel_List)
                {
                    if (channel.DeviceName == _myDevice.Display_Name && channel.IsConnected == true && channel._ChannelID!= _channel_ID)
                    {
                        anyConnect = true;
                    }
                }
                if (!anyConnect)//close the device
                {
                    //Console.WriteLine("lin close:" + m_hObject + "    error code:" + _myDevice.pNumberOfErrors);
                    icsNeoDll.icsneoClosePort(m_hObject, ref _myDevice.pNumberOfErrors);
                    CAN_Manager.Spy3_Device_List.First(p => p.Display_Name == _myDevice.Display_Name).IsOpen = false;                   
                }
                //icsNeoDll.icsneoClosePort(m_hObject, ref pNumberOfErrors);
                _myDevice.Spy3MessageEvent_lin -= LINMessageCallback;
                _driver_hasInit = false;             
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("Exception.message: " + ex.Message + "\r\n              " + "Exception.StackTrace:" + ex.StackTrace);
                return false;
            }
        }



        private int Convert_Bandrate(LINBaudRate rate)
        {
            switch (rate)
            {
                case LINBaudRate.Baud_2400:
                    return 2400;
                case LINBaudRate.Baud_9600:
                    return 9600;
                case LINBaudRate.Baud_10400:
                    return 10400;
                case LINBaudRate.Baud_10417:
                    return 10417;
                case LINBaudRate.Baud_19200:
                    return 19200;
                case LINBaudRate.Baud_20000:
                    return 20000;
                default:
                    return 19200;
            }
        }

        protected override bool DoSetBaudrate(LINBaudRate Rate = LINBaudRate.Baud_19200)
        {
            try
            {
                _myDevice.pNumberOfErrors = icsNeoDll.icsneoSetBitRate(m_hObject, Convert_Bandrate(Rate), _channel_ID);
                if (_myDevice.pNumberOfErrors == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
        }
        protected override void DoSet_Not_AutoChecksum_Mode()
        {
        }
        protected override void DoClearPID(byte PID)
        {
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using vxlapi_NET;
using CSnet;
using SBE2ConnectionWrapper;
using CAN_Library.CAN_Driver;
using CAN_Library.CAN_Parameter;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using ZLGCAN;
using SapaWraper;
//using System.Threading;

namespace CAN_Library.CAN_Interface
{
    class CAN_Manager
    {
        public static XLClass.xl_driver_config Vector_driverConfig = new XLClass.xl_driver_config();
        public static XLDriver Vector_Driver = new XLDriver();
        static uint Vector_serialNumber = 0xFF;
        public static NeoDeviceEx[] Spy3_ndNeoToOpen = new NeoDeviceEx[4];//neoVIFire device info array
        public static int iNumberOfNeoDevices = 4;//set max 4 spy3 neoVIFire device
        public static SBE2Connection SaintConnect = new SBE2Connection();
        public static SAPAConnection SapaConnect = new SAPAConnection();

        static bool SaintEngine_IsConnected = false;
        static bool SaintDataBackSet = false;

        internal static readonly object _locker_Vector_driverConfig = new object();
        internal static readonly object _locker_Spy3_ndNeoToOpen = new object();
        internal static readonly object _locker_SaintList = new object();
        internal static readonly object _locker_Spy3List = new object();
        internal static readonly object _locker_VectorList = new object();
        internal static readonly object _locker_channelList = new object();

        public static ObservableCollection<Saint_Device> Saint_Device_List = new ObservableCollection<Saint_Device>();
        static List<Vector_Device> temp_vector_device_list = new List<Vector_Device>();
        public static ObservableCollection<Vector_Device> Vector_Device_List = new ObservableCollection<Vector_Device>();
        public static ObservableCollection<Spy3_Device> Spy3_Device_List = new ObservableCollection<Spy3_Device>();

        public static ObservableCollection<ZLG_Device> ZLG_Device_List = new ObservableCollection<ZLG_Device>();
        public static ObservableCollection<SAPA_Device> SAPA_Device_List = new ObservableCollection<SAPA_Device>();


        static List<CAN_Channel> Channel_List_temp1 = new List<CAN_Channel>();
        public static ObservableCollection<CAN_Channel> Available_Channel_List = new ObservableCollection<CAN_Channel>();

        // public delegate void ExceptionCallback(uint errorCode, string Description);
        // public static event ExceptionCallback ManagerExceptionEvent;
        public delegate void SaintMessageCallback(int deviceID, byte channelID, Pmessage pMessage);
        public static event SaintMessageCallback SaintMessageCallbackEvent;
        public delegate void SaintLINMessageCallback(int deviceID, byte channelID, Lmessage linMessage);
        public static event SaintLINMessageCallback SaintLINMessageCallbackEvent;

        public static string[] LINerrCode = new string[8]
        {
        "Bit 0 (0x01) - Slave Not Responding Timeout",
        "Bit 1 (0x02) – Tframe Error",
        "Bit 2 (0x04) - Checksum Error",
        "Bit 3 (0x08) - Parity Error",
        "Bit 4 (0x10) – Break/Sync Error",
        "Bit 5 (0x20) – LoopBack Error (AKA Data Error)",
        "Bit 6 (0x40) - Receive Framing Error",
        "Bit 7 (0x80) - Receive Buffer Overflow Error"
        };

        #region saint device and channel functions
        internal static void Update_Saint_Device_List()
        {
            try
            {
                if (SaintConnect.Connect("localhost", 8000, "APTIV"))
                    SaintEngine_IsConnected = true;
                if (!SaintDataBackSet)
                {
                    CAN_Manager.SaintConnect.DataCallbackEvent += new SBE2Connection.DataCallbackEventHandler(Saint_dataCallbackHander);
                    SaintDataBackSet = true;
                    Timer Timer_CheckDeviceList = new Timer(10000);
                    Timer_CheckDeviceList.AutoReset = true;
                    Timer_CheckDeviceList.Elapsed += new ElapsedEventHandler(TimerHandler_CheckDeviceList);
                    Timer_CheckDeviceList.Start();
                }
                else
                {
                    CAN_Manager.SaintConnect.DataCallbackEvent -= Saint_dataCallbackHander;
                    CAN_Manager.SaintConnect.DataCallbackEvent += new SBE2Connection.DataCallbackEventHandler(Saint_dataCallbackHander);
                }

                //SaintConnect.OpenAllDevices();
                string deviceListTemp = SaintConnect.RequestDeviceList();
                if (deviceListTemp != null && deviceListTemp != "ERROR")
                {
                    List<Saint_Device> needToRemoveDeviceList = new List<Saint_Device>();
                    if (Saint_Device_List.Count > 0)
                    {
                        foreach (Saint_Device temp_device in Saint_Device_List)
                        {
                            if (deviceListTemp.IndexOf(temp_device.Name) < 0)
                            {
                                needToRemoveDeviceList.Add(temp_device);
                            }
                        }
                    }
                    if (needToRemoveDeviceList.Count > 0)
                    {
                        foreach (Saint_Device temp_device_toRemove in needToRemoveDeviceList)
                        {
                            Saint_Device_List.Remove(temp_device_toRemove);
                        }
                        needToRemoveDeviceList.Clear();
                    }

                    string[] SaintDeviceList = deviceListTemp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (byte i = 0; i < SaintDeviceList.Length; i += 2)
                    {
                        if (Saint_Device_List.All(p => p.Name != SaintDeviceList[i]))
                        {
                            Saint_Device new_one = new Saint_Device(SaintDeviceList[i], SaintDeviceList[i + 1]);
                            if (new_one.Display_Name != null)
                                Saint_Device_List.Add(new_one);
                        }
                    }
                }
                else
                {
                    Saint_Device_List.Clear();
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.1-Update_Saint_Device_List() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }
        static void Saint_dataCallbackHander(int DeviceID, IntPtr pData, uint Length, DateTime absTime)
        {
            try
            {
                byte[] temp_pdata = new byte[Length];
                Marshal.Copy(pData, temp_pdata, 0, Convert.ToInt32(Length));

                if (!Saint_Device_List.Any(p => p.Device_ID == DeviceID))
                    return;
                if (temp_pdata[0] == 0x57 || temp_pdata[0] == 0x5F || temp_pdata[0] == 0x08 || temp_pdata[0] == 0xBF)//Ignore Configure msg
                    return;
                //lIN
                if (temp_pdata[0] == 0xB9)
                {
                    if (SaintLINMessageCallbackEvent != null)
                    {
                        Lmessage linMsg = new Lmessage();
                        int dlc;
                        if (temp_pdata.Length >= 6)
                        {
                           dlc = temp_pdata.Length - 6;
                           linMsg.DLC = Convert.ToByte(dlc);
                           linMsg.Data = new byte[dlc];
                           linMsg.CheckSum = temp_pdata[6];
                        }
                        else 
                        { 
                           dlc = 0;
                           linMsg.DLC = 0;
                        }
                                               
                        for (int i = 0; i < dlc; i++)
                        { 
                            linMsg.Data[i] = temp_pdata[i+2];
                        }
                        linMsg.ID = (uint)temp_pdata[1] & 0x3F;
                        
                        //get error
                        byte completionCode = temp_pdata[temp_pdata.Length - 3];
                        string completionCodeBits = Convert.ToString(completionCode, 2);

                        for (int i = 0; i < completionCodeBits.Length; i++)
                        {
                            if (completionCodeBits[i] == '1')
                            {
                                linMsg.Error += LINerrCode[i];
                                linMsg.TxRx = TxRxType.Err;
                            }

                        }
                        linMsg.LastReceivedTimeMs = (long)Convert.ToUInt64(DateTime.Now.Ticks / 10000L);
                        SaintLINMessageCallbackEvent(DeviceID, 5, linMsg);
                    }
                    return;
                }

                else if (temp_pdata[0] == 0x08)
                {
                    if (temp_pdata[1] == 0x80)
                    {
                        for (int i = 0; i < Saint_Device_List.Count; i++)
                        {
                            if (Saint_Device_List[i].Device_ID == DeviceID)
                            {
                                SaintConnect.ConnectToDevice(Saint_Device_List[i].Name);
                                break;
                            }
                        }
                        return;
                    }
                }
                byte channelID = 0;
                ///CAN1
                if ((temp_pdata[0] & 8) == 0)
                {
                    if (Saint_Device_List.First(p => p.Device_ID == DeviceID).CAN1_SetHighSpeed)
                        channelID = 3;
                    else
                        channelID = 1;
                }
                ///CAN2
                else if ((temp_pdata[0] & 8) > 0)
                {
                    if (Saint_Device_List.First(p => p.Device_ID == DeviceID).CAN2_SetHighSpeed)
                        channelID = 4;
                    else
                        channelID = 2;
                }
                if (temp_pdata[Length - 3] != 0x08 && temp_pdata[Length - 3] != 0x10)
                {
                    if (SaintMessageCallbackEvent != null)
                    {
                        string completionCode = "";
                        switch (temp_pdata[Length - 3])
                        {
                            case 0x1:
                                completionCode = "stuff error";
                                break;
                            case 0x02:
                                completionCode = "form error";
                                break;
                            case 0x03:
                                completionCode = "ACK error";
                                break;
                            case 0x4:
                                completionCode = "bit1 error";
                                break;
                            case 0x5:
                                completionCode = "bit0 error";
                                break;
                            case 0x6:
                                completionCode = "CRC error";
                                break;
                            default:
                                completionCode = "CAN frame error";
                                break;
                        }
                        Pmessage pMessage = new Pmessage
                        {
                            CANID = 0,
                            DLC = 0,
                            Data = null,
                            TxRx = TxRxType.Err,
                            FDorNot = (temp_pdata[0] & 0xD0) == 0xD0 ? true : false,
                            LastReceivedTimeMs = Convert.ToUInt64(DateTime.Now.Ticks / 10000L),
                            Error = completionCode,
                        };
                        SaintMessageCallbackEvent(DeviceID, channelID, pMessage);
                    }
                }
                else if (temp_pdata[0] == 0x53 || temp_pdata[0] == 0x5B || temp_pdata[0] == 0x51 || temp_pdata[0] == 0x59
                      || temp_pdata[0] == 0xD3 || temp_pdata[0] == 0xDB || temp_pdata[0] == 0xD1 || temp_pdata[0] == 0xD9)
                {
                    if (SaintMessageCallbackEvent != null)
                    {
                        Pmessage pMessage = SaintPmessageBuild(temp_pdata, Length);
                        SaintMessageCallbackEvent(DeviceID, channelID, pMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.2-Saint_dataCallbackHander() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }
        static Pmessage SaintPmessageBuild(byte[] pData, uint length)
        {
            try
            {
                ///for tx frames, bit1 of byte0 must be set
                TxRxType TxRx = ((pData[0] & 1 << 1) != 0) ? TxRxType.Tx : TxRxType.Rx;
                ///for extended identifiers, bit7 of byte1 must be set
                ///remove 1 in 6th bit in first byte of 11bit data
                uint ID = ((pData[1] & 1 << 7) != 0) ? Convert.ToUInt32(((pData[1] & 0x7F) << 24) + (pData[2] << 16) + (pData[3] << 8) + pData[4]) :
                                                        (Convert.ToUInt32((pData[1] << 8) + pData[2]) & 0x7FF);

                ///dlc = length - 1(header) - 2~4(ID) - 1(completion code) - 2(1ms resolution Time Stamp)
                uint DLC = ((pData[1] & 1 << 7) != 0) ? (length - 8) : (length - 6);
                byte[] Data = new byte[0];
                if (DLC != 0)
                {
                    Data = new byte[DLC];
                    ///extended identifiers
                    if ((pData[1] & 1 << 7) != 0)
                    {
                        for (byte i = 0; i < DLC; i++)
                        {
                            Data[0 + i] = pData[5 + i];
                        }
                    }
                    ///11bit identifiers
                    else
                    {
                        for (byte i = 0; i < DLC; i++)
                        {
                            Data[0 + i] = pData[3 + i];
                        }
                    }
                }
                bool isFD = (pData[0] & 0xD0) == 0xD0 ? true : false;
                Pmessage pMessage = new Pmessage(ID, Convert.ToByte(DLC), Data, TxRx, isFD, Convert.ToUInt64(DateTime.Now.Ticks / 10000L));
                return pMessage;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.3-SaintPmessageBuild() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return null;
            }
        }
        static void Saint_Channel_Create()
        {
            foreach (Saint_Device device in Saint_Device_List)
            {
                if (device.Display_Name.IndexOf("Saint2") >= 0)
                {
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Saint2,
                        _ChannelID = 1,
                        _Name = "CAN1_SingleWire",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-CAN1_SingleWire",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.singleWire, false),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Saint2,
                        _ChannelID = 2,
                        _Name = "CAN2_SingleWire",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-CAN2_SingleWire",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.singleWire, false),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Saint2,
                        _ChannelID = 3,
                        _Name = "CAN1_HighSpeed",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-CAN1_HighSpeed",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, false),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Saint2,
                        _ChannelID = 4,
                        _Name = "CAN2_HighSpeed",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-CAN2_HighSpeed",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, false),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Saint2,
                        _ChannelID = 5,
                        _Name = "LIN",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-LIN",
                        _IsConnected = false,
                        _LIN_Parameter = new LIN_Parameter(),
                    });
                }
                else
                {
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Saint3,
                        _ChannelID = 1,
                        _Name = "CAN1_SingleWire",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-CAN1_SingleWire",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.singleWire, false),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Saint3,
                        _ChannelID = 2,
                        _Name = "CAN2_SingleWire",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-CAN2_SingleWire",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.singleWire, false),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Saint3,
                        _ChannelID = 3,
                        _Name = "CAN1_HighSpeed",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-CAN1_HighSpeed",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, true),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Saint3,
                        _ChannelID = 4,
                        _Name = "CAN2_HighSpeed",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-CAN2_HighSpeed",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, true),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Saint3,
                        _ChannelID = 5,
                        _Name = "LIN",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-LIN",
                        _IsConnected = false,
                        _LIN_Parameter = new LIN_Parameter(),
                    });
                }
            }
        }

        static void TimerHandler_CheckDeviceList(object sender, EventArgs e)
        {
            if (!SaintEngine_IsConnected)
            {
                return;
            }
            string returnedDevices = "";
            if (SaintConnect.IsConnected())
            {
                returnedDevices = SaintConnect.RequestDeviceList();
            }
            else
            {
                //Log.Logger_Run("Saint-----------loss-----------socket connection!");
                CAN_Manager.SaintConnect.DataCallbackEvent -= Saint_dataCallbackHander;
                SaintConnect.Connect("localhost", 8000, "APTIV");
                CAN_Manager.SaintConnect.DataCallbackEvent += Saint_dataCallbackHander;
                if (SaintConnect.IsConnected())
                {
                    //Log.Logger_Run("Saint-----------start-----------socket connection!");
                    returnedDevices = SaintConnect.RequestDeviceList();
                }
                else
                {
                    //Log.Logger_Run("Saint-----------try fail-----------socket connection!");
                }
            }
            if (returnedDevices != "" && returnedDevices != "ERROR" && Saint_Device_List.Count > 0)
            {
                for (int i = 0; i < Saint_Device_List.Count; i++)
                {
                    if (returnedDevices.IndexOf(Saint_Device_List[i].Name) >= 0)
                    {
                        if (Saint_Device_List[i].IsOpen)
                        {
                            SaintConnect.ConnectToDevice(Saint_Device_List[i].Name);
                            System.Threading.Thread.Sleep(1000);
                            //CAN_Manager.SaintConnect.DataCallbackEvent += Saint_dataCallbackHander;
                            //Log.Logger_Run(string.Format("Saint-----------try device connect in engine tool-----------{0}!", Saint_Device_List[i].Display_Name));
                        }
                    }
                }
            }
        }
        #endregion

        #region vector device and channel functions
        public static void Update_Vector_Devcie_List()
        {
            try
            {
                Vector_serialNumber = 0xFF;
                if (Vector_Driver.XL_OpenDriver() == XLDefine.XL_Status.XL_SUCCESS)
                {
                    if (Vector_Driver.XL_GetDriverConfig(ref Vector_driverConfig) == XLDefine.XL_Status.XL_SUCCESS)
                    {
                        temp_vector_device_list = new List<Vector_Device>();
                        for (int i = 0; i < Vector_driverConfig.channelCount; i++)
                        {
                            Vector_Device new_one = Build_Vector_Device(Vector_driverConfig.channel[i]);
                            if (new_one != null)
                            {
                                temp_vector_device_list.Add(new_one);
                            }
                        }
                        if (Vector_Device_List.Count == 0)
                        {
                            if (temp_vector_device_list.Count > 0)
                            {
                                foreach (Vector_Device temp_device in temp_vector_device_list)
                                {
                                    Vector_Device_List.Add(temp_device);
                                }
                            }
                        }
                        else
                        {
                            //old lost device remove from device_list
                            List<Vector_Device> needToRemoveDeviceList = new List<Vector_Device>();
                            foreach (Vector_Device temp_device in Vector_Device_List)
                            {
                                if (temp_vector_device_list.All(p => p.Name != temp_device.Name))
                                {
                                    needToRemoveDeviceList.Add(temp_device);
                                }
                            }
                            if (needToRemoveDeviceList.Count > 0)
                            {
                                foreach (Vector_Device temp_device_toRemove in needToRemoveDeviceList)
                                {
                                    Vector_Device_List.Remove(temp_device_toRemove);
                                }
                                needToRemoveDeviceList.Clear();
                            }
                            foreach (Vector_Device tempOneDevice in temp_vector_device_list)
                            {
                                if (Vector_Device_List.All(p => p.Name != tempOneDevice.Name))
                                {
                                    Vector_Device_List.Add(tempOneDevice);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.4-Update_Vector_Devcie_List() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        private static Vector_Device Build_Vector_Device(XLClass.xl_channel_config channelConfig)
        {
            try
            {
                if (temp_vector_device_list.Count == 0 || temp_vector_device_list.All(p => p.SerialNumber != channelConfig.serialNumber))
                {
                    Vector_Device new_one = new Vector_Device();
                    new_one.SerialNumber = channelConfig.serialNumber;
                    new_one.HWType = channelConfig.hwType;
                    new_one.HWIndex = channelConfig.hwIndex;
                    if (channelConfig.hwType == XLDefine.XL_HardwareType.XL_HWTYPE_VIRTUAL)
                    {
                        new_one.Display_Name = "Vector - Virtual";
                        new_one.Name = "Virtual";
                    }
                    else
                    {
                        new_one.Name = channelConfig.name.Replace("\0", "").Substring(0, channelConfig.name.IndexOf("Chan") - 1) + "-" + channelConfig.serialNumber.ToString();
                        new_one.Display_Name = "Vector - " + new_one.Name;
                    }
                    return new_one;
                }
                else
                { return null; }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.5-Build_Vector_Device() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return null;
            }
        }
        private static void Vector_Channel_Create()
        {
            try
            {
                for (int i = 0; i < Vector_driverConfig.channelCount; i++)
                {
                    if (Vector_driverConfig.channel[i].transceiverName.IndexOf("CAN") < 0 && Vector_driverConfig.channel[i].transceiverName.IndexOf("LIN") < 0 && Vector_driverConfig.channel[i].transceiverName.IndexOf("FR") < 0 && Vector_driverConfig.channel[i].transceiverName.IndexOf("Unknown Transceiver") < 0)
                        continue;

                    if (Vector_Device_List.Any(p => p.SerialNumber == Vector_driverConfig.channel[i].serialNumber))
                    {
                        Vector_Device temp_device = Vector_Device_List.First(p => p.SerialNumber == Vector_driverConfig.channel[i].serialNumber);
                        if (temp_device.Name != "Virtual")
                        {
                            if (Vector_driverConfig.channel[i].busParams.busType == XLDefine.XL_BusTypes.XL_BUS_TYPE_LIN)
                            {
                                CAN_Channel newOne = new CAN_Channel()
                                {
                                    _device_type = Device_Type.Vector,
                                    _ChannelID = Vector_driverConfig.channel[i].hwChannel,
                                    _Name = "Ch" + (Vector_driverConfig.channel[i].hwChannel + 1).ToString() + " " + Vector_driverConfig.channel[i].transceiverName.Replace("\0", ""),
                                    _DeviceName = temp_device.Display_Name,
                                    _FullName = temp_device.Display_Name + "-" + "Ch" + (Vector_driverConfig.channel[i].hwChannel + 1).ToString() + " " + Vector_driverConfig.channel[i].transceiverName.Replace("\0", ""),
                                    _IsConnected = false,
                                    _LIN_Parameter = new LIN_Parameter(),
                                };
                                Channel_List_temp1.Add(newOne);
                            }
                            else if (Vector_driverConfig.channel[i].busParams.busType == XLDefine.XL_BusTypes.XL_BUS_TYPE_FLEXRAY)
                            {
                                CAN_Channel newOne = new CAN_Channel()
                                {
                                    _device_type = Device_Type.Vector,
                                    _ChannelID = Vector_driverConfig.channel[i].hwChannel,
                                    _Name = "Ch" + (Vector_driverConfig.channel[i].hwChannel + 1).ToString() + " " + Vector_driverConfig.channel[i].transceiverName.Replace("\0", ""),
                                    _DeviceName = temp_device.Display_Name,
                                    _FullName = temp_device.Display_Name + "-" + "Ch" + (Vector_driverConfig.channel[i].hwChannel + 1).ToString() + " " + Vector_driverConfig.channel[i].transceiverName.Replace("\0", ""),
                                    _IsConnected = false,
                                    _LIN_Parameter = new LIN_Parameter(),
                                };
                                Channel_List_temp1.Add(newOne);
                            }
                            else
                            {
                                int firstIndex = Vector_driverConfig.channel[i].transceiverName.IndexOf("(");
                                int length = Vector_driverConfig.channel[i].transceiverName.IndexOf(")") - Vector_driverConfig.channel[i].transceiverName.IndexOf("(") - 1;
                                if (length <= 0)
                                    continue;

                                CAN_Channel newOne = new CAN_Channel()
                                {
                                    _device_type = Device_Type.Vector,
                                    _ChannelID = Vector_driverConfig.channel[i].hwChannel,
                                    _Name = "Ch" + (Vector_driverConfig.channel[i].hwChannel + 1).ToString() + " " + Vector_driverConfig.channel[i].transceiverName.Replace("\0", "").Substring(firstIndex + 1, length),
                                    _DeviceName = temp_device.Display_Name,
                                    _FullName = temp_device.Display_Name + "-" + "Ch" + (Vector_driverConfig.channel[i].hwChannel + 1).ToString() + " " + Vector_driverConfig.channel[i].transceiverName.Replace("\0", "").Substring(firstIndex + 1, length),
                                    _IsConnected = false,
                                };

                                if (Vector_driverConfig.channel[i].transceiverName.IndexOf("High") >= 0)
                                {
                                    newOne._CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, (Vector_driverConfig.channel[i].channelCapabilities & XLDefine.XL_ChannelCapabilities.XL_CHANNEL_FLAG_CANFD_ISO_SUPPORT) == XLDefine.XL_ChannelCapabilities.XL_CHANNEL_FLAG_CANFD_ISO_SUPPORT ? true : false);
                                }
                                else if (Vector_driverConfig.channel[i].transceiverName.IndexOf("Low") >= 0)
                                {
                                    newOne._CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.lowSpeed, false);
                                }
                                else if (Vector_driverConfig.channel[i].transceiverName.IndexOf("Single") >= 0)
                                {
                                    newOne._CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.singleWire, false);
                                }
                                Channel_List_temp1.Add(newOne);
                            }
                        }
                        else
                        {
                            CAN_Channel newOne = new CAN_Channel()
                            {
                                _device_type = Device_Type.Vector,
                                _ChannelID = Vector_driverConfig.channel[i].hwChannel,
                                _Name = "Ch" + (Vector_driverConfig.channel[i].hwChannel + 1).ToString(),
                                _DeviceName = temp_device.Display_Name,
                                _FullName = temp_device.Display_Name + "-" + "Ch" + (Vector_driverConfig.channel[i].hwChannel + 1).ToString(),
                                _IsConnected = false,
                                //_CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.anyType, (Vector_driverConfig.channel[i].channelCapabilities & XLDefine.XL_ChannelCapabilities.XL_CHANNEL_FLAG_CANFD_ISO_SUPPORT) == XLDefine.XL_ChannelCapabilities.XL_CHANNEL_FLAG_CANFD_ISO_SUPPORT ? true : false),
                                _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.anyType, true),
                            };
                            Channel_List_temp1.Add(newOne);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.6-Vector_Channel_Create() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }
        #endregion

        #region neoVI device and channel functions
        public static void Update_Spy3_Device_List()
        {
            try
            {
                OptionsNeoEx neoDeviceOption = new OptionsNeoEx();
                if (icsNeoDll.icsneoFindDevices(ref Spy3_ndNeoToOpen[0], ref iNumberOfNeoDevices, 0, 0, ref neoDeviceOption, 0) == 1)
                {
                    lock (Spy3_Device_List)
                    {
                        if (iNumberOfNeoDevices > 0)
                        {
                            for (byte n = 0; n < iNumberOfNeoDevices; n++)
                            {
                                //Define Fire or fire 2
                                string temp_device_name = "";
                                if (Spy3_ndNeoToOpen[n].neoDevice.DeviceType == Convert.ToInt32(eHardwareTypes.NEODEVICE_FIRE2))
                                {
                                    temp_device_name = "neoVI Fire2-" + Spy3_ndNeoToOpen[n].neoDevice.SerialNumber.ToString();
                                }
                                else if (Spy3_ndNeoToOpen[n].neoDevice.DeviceType == Convert.ToInt32(eHardwareTypes.NEODEVICE_FIRE))
                                {
                                    temp_device_name = "neoVI Fire-" + Spy3_ndNeoToOpen[n].neoDevice.SerialNumber.ToString();
                                }
                                //Build List
                                if (temp_device_name != "")
                                {
                                    if (Spy3_Device_List.All(p => p.Name != temp_device_name))
                                    {
                                        Spy3_Device new_one = new Spy3_Device(temp_device_name, Spy3_ndNeoToOpen[n].neoDevice);
                                        Spy3_Device_List.Add(new_one);
                                    }
                                }
                            }
                            //remove all non-existing device from spy3 device list
                            List<Spy3_Device> needToRemoveDeviceList = new List<Spy3_Device>();
                            if (Spy3_Device_List.Count > 0)
                            {
                                foreach (Spy3_Device temp_device in Spy3_Device_List)
                                {
                                    if (Spy3_ndNeoToOpen.All(p => p.neoDevice.SerialNumber.ToString() != temp_device.Name.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)[1]))
                                    {
                                        needToRemoveDeviceList.Add(temp_device);
                                    }
                                }
                                if (needToRemoveDeviceList.Count > 0)
                                {
                                    foreach (Spy3_Device temp_device_toRemove in needToRemoveDeviceList)
                                    {
                                        Spy3_Device_List.Remove(temp_device_toRemove);
                                    }
                                    needToRemoveDeviceList.Clear();
                                }
                            }
                        }
                        else
                        {
                            Spy3_Device_List.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.7-Update_Spy3_Device_List() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }
        private static void SYP3_Channel_Create()
        {
            foreach (Spy3_Device device in Spy3_Device_List)
            {
                if (device.HW_Device.DeviceType == Convert.ToInt32(eHardwareTypes.NEODEVICE_FIRE2))
                {
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 1,
                        _Name = "HSCAN",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-HSCAN",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, true),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 2,
                        _Name = "MSCAN",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-MSCAN",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.midSpeed, true),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 3,
                        _Name = "SWCAN",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-SWCAN",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.singleWire, false),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 68,
                        _Name = "SWCAN2",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-SWCAN2",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.singleWire, false),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 42,
                        _Name = "HSCAN2",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-HSCAN2",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, true),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 44,
                        _Name = "HSCAN3",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-HSCAN3",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, true),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 61,
                        _Name = "HSCAN4",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-HSCAN4",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, true),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 62,
                        _Name = "HSCAN5",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-HSCAN5",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, true),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 96,
                        _Name = "HSCAN6",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-HSCAN6",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, true),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 97,
                        _Name = "HSCAN7",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-HSCAN7",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, true),
                    });
                }
                else
                {
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 1,
                        _Name = "HSCAN",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-HSCAN",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, false),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 2,
                        _Name = "MSCAN",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-MSCAN",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, false),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 2,
                        _Name = "MSCAN",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-MSCAN",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.midSpeed, false),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 3,
                        _Name = "SWCAN",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-SWCAN",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.singleWire, false),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 42,
                        _Name = "HSCAN2",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-HSCAN2",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, false),
                    });
                    Channel_List_temp1.Add(new CAN_Channel()
                    {
                        _device_type = Device_Type.Spy3,
                        _ChannelID = 44,
                        _Name = "HSCAN3",
                        _DeviceName = device.Display_Name,
                        _FullName = device.Display_Name + "-HSCAN3",
                        _IsConnected = false,
                        _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, false),
                    });
                }

                string nameLIN = "LIN";
                string nameLIN2 = "LIN2";
                string nameLIN3 = "LIN3";
                string nameLIN4 = "LIN4";
                Channel_List_temp1.Add(new CAN_Channel()
                {
                    _device_type = Device_Type.Spy3,
                    _Name = nameLIN,
                    _ChannelID = (Byte)icsNeoDll.GetNetworkIDfromString(ref nameLIN),                   
                    _DeviceName = device.Display_Name,
                    _FullName = device.Display_Name + "-"+ nameLIN,
                    _IsConnected = false,
                    _LIN_Parameter = new LIN_Parameter()
                });

                Channel_List_temp1.Add(new CAN_Channel()
                {
                    _device_type = Device_Type.Spy3,
                    _Name = nameLIN2,
                    _ChannelID = (Byte)icsNeoDll.GetNetworkIDfromString(ref nameLIN2),
                    _DeviceName = device.Display_Name,
                    _FullName = device.Display_Name + "-"+ nameLIN2,
                    _IsConnected = false,
                });

                Channel_List_temp1.Add(new CAN_Channel()
                {
                    _device_type = Device_Type.Spy3,
                    _Name = nameLIN3,
                    _ChannelID = (Byte)icsNeoDll.GetNetworkIDfromString(ref nameLIN3),
                    _DeviceName = device.Display_Name,
                    _FullName = device.Display_Name + "-" + nameLIN3,
                    _IsConnected = false,
                });

                Channel_List_temp1.Add(new CAN_Channel()
                {
                    _device_type = Device_Type.Spy3,
                    _Name = nameLIN4,
                    _ChannelID = (Byte)icsNeoDll.GetNetworkIDfromString(ref nameLIN4),
                    _DeviceName = device.Display_Name,
                    _FullName = device.Display_Name + "-" + nameLIN4,
                    _IsConnected = false,
                    
                });
            }
        }
        #endregion

        #region ZLG device and channel functions
               
        public static void Update_ZLG_Device_List()
        {                       
            try
            {
                List<IntPtr> handle_list = new List<IntPtr> { };
                foreach (ZLGCAN_Warpper.DeviceType devicetype in ZLGCAN_Warpper.Support_CANFD_Device)
                {
                    for (int device_index = 0; device_index < ZLGCAN_Warpper.MAX_DEVICE; device_index++)
                    {
                        IntPtr device_handle = Method.ZCAN_OpenDevice((uint)devicetype.type, (uint)device_index, 0);
                        if (IntPtr.Zero == device_handle)
                        {
                            continue; 
                        }


                        ZLGCAN_Warpper.ZCAN_DEVICE_INFO info = new ZLGCAN_Warpper.ZCAN_DEVICE_INFO();
                        IntPtr pInfo = Marshal.AllocHGlobal(Marshal.SizeOf(info));
                        uint result = ZLGCAN_Warpper.ZCAN_GetDeviceInf(device_handle, pInfo);

                        if (result != Define.STATUS_OK)
                        {
                            Method.ZCAN_CloseDevice(device_handle);
                            continue;
                        }

                        info = (ZLGCAN_Warpper.ZCAN_DEVICE_INFO)Marshal.PtrToStructure((IntPtr)((UInt32)pInfo), typeof(ZLGCAN_Warpper.ZCAN_DEVICE_INFO));
                        Marshal.FreeHGlobal(pInfo);

                        handle_list.Add(device_handle);
                        
                        string temp_device_name = new string(info.str_hw_Type).Replace("\0","") + "_" + device_index.ToString();
                        if (ZLG_Device_List.All(p => p.Name != temp_device_name))
                        {
                            ZLG_Device new_one = new ZLG_Device(devicetype, temp_device_name, device_index);
                            new_one.SerialNumber = new string(info.str_Serial_Num).Replace("\0", "");
                            ZLG_Device_List.Add(new_one);
                        }

                        //do not closedevice because one hardware device may be opened by many device type.
                        //ZCAN_USBCANFD_100U device can also be opened with ZCAN_USBCANFD_200U / ZCAN_USBCANFD_MINI type.
                        //Method.ZCAN_CloseDevice(device_handle); 
                    }
                }

                // close after scan all device
                foreach(IntPtr handle in handle_list)
                {
                    Method.ZCAN_CloseDevice(handle);
                }
                //remove all non-existing device from device list  -- later implemented               

            }
            catch (Exception ex)
            {
                Log.Logger_Run("Update_ZLG_Device_List() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        private static void ZLG_Channel_Create()
        {
            try
            {
                foreach (ZLG_Device device in ZLG_Device_List)
                {
                    for (int iChannelID = 0; iChannelID < device.Device_Info.channel_count; iChannelID ++)
                    {
                        Channel_List_temp1.Add(new CAN_Channel()
                        {
                            _device_type = Device_Type.ZLG,
                            _ChannelID = (byte)iChannelID,
                            _Name = "ZLGCAN",
                            _DeviceName = device.Display_Name,
                            _FullName = device.Display_Name + "-Ch" + iChannelID.ToString (),
                            _IsConnected = false,
                            _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, true),
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("ZLG_Channel_Create() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }
        #endregion

        #region Sapa device and channel functions
        public static void Update_Sapa_Device_List()
        {
			IntPtr ptr = IntPtr.Zero;
			try
            {               
                bool r = SapaConnect.RequestDeviceList();
                if (!r)
                {
                    Log.Logger_Run("2.3.7- fail to RequestDeviceList()");
                    return;
                }

                int i = 0;
                foreach (SAPAConnection.XL_DEVICE_T device in SapaConnect.device_list)
                { 
                    SAPA_Device new_device = new SAPA_Device(device.str_Serial_Num, i);
                    SAPA_Device_List.Add(new_device);
                    i++;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static void Sapa_Channel_Create()
        {
            try
            {
                foreach (SAPA_Device device in SAPA_Device_List)
                {
                    if (device.device_index % 2 == 0)
                    {
                        Channel_List_temp1.Add(
                        new CAN_Channel()
                        {
                            _device_type = Device_Type.Sapa,
                            _ChannelID = 0,
                            _Name = "SAPA_FlexRay",
                            _DeviceName = device.Display_Name,
                            _FullName = device.Display_Name + "_FR",
                            _IsConnected = false,
                        });
                        Channel_List_temp1.Add(
                        new CAN_Channel()
                        {
                            _device_type = Device_Type.Sapa,
                            _ChannelID = 0,
                            _Name = "SAPA_CAN1",
                            _DeviceName = device.Display_Name,
                            _FullName = device.Display_Name + "_CAN1",
                            _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, true),
                            _IsConnected = false,
                        });
                        //Channel_List_temp1.Add(
                        //new CAN_Channel()
                        //{
                        //    _device_type = Device_Type.Sapa,
                        //    _ChannelID = 0,
                        //    _Name = "SAPA_LIN1",
                        //    _DeviceName = device.Display_Name,
                        //    _FullName = device.Display_Name + "_LIN1",
                        //    _IsConnected = false,
                        //});
                    }
                    else
                    {
                        Channel_List_temp1.Add(
                        new CAN_Channel()
                        {
                            _device_type = Device_Type.Sapa,
                            _ChannelID = 1,
                            _Name = "SAPA_CAN2",
                            _DeviceName = device.Display_Name,
                            _FullName = device.Display_Name + "_CAN2",
                            _CAN_Parameter = new CAN_Driver.CAN_Parameter(CANBus_PhysicType.highSpeed, true),
                            _IsConnected = false,
                        });
                        //Channel_List_temp1.Add(
                        //new CAN_Channel()
                        //{
                        //    _device_type = Device_Type.Sapa,
                        //    _ChannelID = 1,
                        //    _Name = "SAPA_LIN2",
                        //    _DeviceName = device.Display_Name,
                        //    _FullName = device.Display_Name + "_LIN2",
                        //    _IsConnected = false,
                        //});
                    }    
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("ZLG_Channel_Create() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }
  #endregion

        public static void Update_CANChannel()
        {
            if (Available_Channel_List.Count == 0)
            {
                Channel_List_temp1.Clear();
                SYP3_Channel_Create();
                Vector_Channel_Create();
                Saint_Channel_Create();
                ZLG_Channel_Create();
                Sapa_Channel_Create();
                foreach (CAN_Channel channel in Channel_List_temp1)
                {
                    Available_Channel_List.Add(channel);
                }
            }
            else
            {
                Channel_List_temp1.Clear();
                SYP3_Channel_Create();
                Vector_Channel_Create();
                Saint_Channel_Create();
                ZLG_Channel_Create();
                Sapa_Channel_Create();
                foreach (CAN_Channel channel in Channel_List_temp1)
                {
                    if (Available_Channel_List.All(p => p.FullName != channel.FullName))
                    {
                        Available_Channel_List.Add(channel);
                    }
                }
                List<CAN_Channel> needToRemoveChannel = new List<CAN_Channel>();
                foreach (CAN_Channel channel in Available_Channel_List)
                {
                    if (Channel_List_temp1.All(p => p.FullName != channel.FullName))
                    {
                        needToRemoveChannel.Add(channel);
                    }
                }
                foreach (CAN_Channel channel in needToRemoveChannel)
                {
                    Available_Channel_List.Remove(channel);
                }
                needToRemoveChannel.Clear();
            }
        }

        internal static void ClearList()
        {
            try
            {
                Channel_List_temp1.Clear();
                Available_Channel_List.Clear();
                Saint_Device_List.Clear();
                temp_vector_device_list.Clear();
                Vector_Device_List.Clear();
                Spy3_Device_List.Clear();
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.8-ClearList() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }
        internal static void SaintDispose()
        {
            if (SaintEngine_IsConnected)
            {
                SaintConnect.Disconnect();
                SaintEngine_IsConnected = false;
                if (SaintDataBackSet)
                {
                    CAN_Manager.SaintConnect.DataCallbackEvent -= Saint_dataCallbackHander;
                    SaintDataBackSet = false;
                }
            }
        }
    }

    internal class Saint_Device
    {
        public string Name = "";
        public int Device_ID = 255;
        public bool IsOpen = false;
        public string Display_Name;//full name
        public bool CAN1_SetHighSpeed = false;//CAN1 is SWC at first
        public bool CAN2_SetHighSpeed = false;//CAN2 is SWC at first
        public bool CAN1_isOpen = false;
        public bool CAN2_isOpen = false;

        public Saint_Device(string name, string device_id)
        {
            this.Name = name;
            int.TryParse(device_id, out int Saint_ID);
            this.Device_ID = Saint_ID;
            this.IsOpen = false;
            if (name.Length == 6 && name.Substring(0, 2) == "SZ")
            {
                this.Display_Name = "Saint2-" + name;
            }
            else
            { this.Display_Name = name; }
        }
    }

    internal class Vector_Device
    {
        public string Name;
        public XLDefine.XL_HardwareType HWType;
        public byte HWIndex;
        public bool IsOpen;
        public string Display_Name;//full name
        public uint SerialNumber;
    }

    internal class ZLG_Device
    {
        public string Name;
        public bool IsOpen;
        public string Display_Name;//full name
        public string SerialNumber;

        public int device_index; 
        public ZLGCAN_Warpper.DeviceType Device_Info;
        public bool isCANFD;
        /// <summary>
        /// 
        /// </summary>       

        public ZLG_Device(ZLGCAN_Warpper.DeviceType device_info, string name, int device_indexD)
        {
            this.Device_Info = device_info;
            this.Name = name;
            this.device_index = device_index;
            this.IsOpen = false;
            this.isCANFD = isCANFD;
            this.Display_Name = "ZLG-" + name;
        }  
    }

    internal class SAPA_Device
    {
        public string Name;
        public bool IsOpen;
        public string Display_Name;//full name
        public int device_index;
        /// <summary>
        /// 
        /// </summary>       

        public SAPA_Device(string name, int device_index)
        {
            this.Name = name;
            this.device_index = device_index;
            this.IsOpen = false;
            this.Display_Name = "SAPA-" + name;
        }
    }

    class Spy3_Device
    {
        public delegate void MessageDataCallBack(Pmessage Msg, int netid);
        public event MessageDataCallBack Spy3MessageEvent;

        public delegate void MessageDataCallBack_LIN(byte netid, Lmessage linMessage);
        public event MessageDataCallBack_LIN Spy3MessageEvent_lin;

        public string Name;
        public NeoDevice HW_Device;
        public bool IsOpen;
        public string Display_Name;//full name
        public IntPtr m_hObject;//device handler
        public byte[] bNetwork = new byte[255];//network channels reference
        private int _channel_ID;
        public Int32 pNumberOfErrors;
        icsSpyMessage[] _stMessages = new icsSpyMessage[20000];
        System.Threading.Thread _rxThread;

        bool _isActive_rxThread = false;

        List<int> InitID = new List<int>();

        public Spy3_Device(string name, NeoDevice hw_device)
        {
            this.Name = name;
            this.HW_Device = hw_device;
            IsOpen = false;
            this.Display_Name = "Spy3-" + name;
        }


        public bool DoInitialization(ref CAN_Driver.CAN_Parameter target_can_param, int _networkID)
        {
            try
            {               
                if (!CAN_Manager.Spy3_Device_List.First(p => p.Display_Name == Display_Name).IsOpen)
                {
                    for (int iCount = 0; iCount < 255; iCount++)
                    {
                        bNetwork[iCount] = Convert.ToByte(iCount);
                    }
                    _channel_ID = _networkID;

                    pNumberOfErrors=icsNeoDll.icsneoOpenNeoDevice(ref HW_Device, ref m_hObject, ref bNetwork[0], 1, 0);
                    //Console.WriteLine("CAN open:" + m_hObject + "    error code:" + pNumberOfErrors);
                    if (pNumberOfErrors != 1)
                    {
                        return false;
                    }
                    CAN_Manager.Spy3_Device_List.First(p => p.Display_Name == Display_Name).IsOpen = true;
                }
                
                if (!DoSetBaudrate(target_can_param, _networkID))
                { return false; }

                ///start messages callback thread
                this._rxThread = new System.Threading.Thread(new System.Threading.ThreadStart(Rx_Thread));
                this._isActive_rxThread = true;
                this._rxThread.Start();
                if (InitID.IndexOf(_networkID) < 0)
                {
                    InitID.Add(_networkID);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.9-DoInitialization() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        public bool DoUninitialize(int _networkID)
        {
            try
            {
                bool anyConnect = false;
                foreach (var channel in CAN_Manager.Available_Channel_List)
                {
                    if (channel.DeviceName== Display_Name && channel.IsConnected==true && channel._ChannelID != _channel_ID)
                    {
                        anyConnect = true;
                    }                  
                }
                if (!anyConnect)//close the device
                {
                    //Console.WriteLine("CAN close:" + m_hObject + "    error code:" + pNumberOfErrors);
                    icsNeoDll.icsneoClosePort(m_hObject, ref pNumberOfErrors);
                    CAN_Manager.Spy3_Device_List.First(p => p.Display_Name == Display_Name).IsOpen = false;
                    
                }
                

                if (InitID.IndexOf(_networkID) >= 0)
                {
                    InitID.Remove(_networkID);

                    if (InitID.Count == 0)
                    {
                        if (this._rxThread != null)
                        {
                            this._isActive_rxThread = false;
                            this._rxThread.Join();
                        }
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.10-DoUninitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
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

                            if (_stMessages[count].Protocol == Convert.ToByte(ePROTOCOL.SPY_PROTOCOL_CAN))
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
                                    Spy3MessageEvent(new Pmessage((uint)_stMessages[count].ArbIDOrHeader, _stMessages[count].NumberBytesData, targetMsg, type, false, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)), _channel_ID);
                                }
                                else
                                {
                                    Spy3MessageEvent(new Pmessage((uint)_stMessages[count].ArbIDOrHeader, 0, null, type, false, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)), _channel_ID);
                                }
                            }
                            else if (_stMessages[count].Protocol == Convert.ToByte(ePROTOCOL.SPY_PROTOCOL_CANFD))
                            {
                                if (_stMessages[count].NumberBytesData > 0)
                                {
                                    int CANID = _stMessages[count].ArbIDOrHeader;
                                    byte dlc = _stMessages[count].NumberBytesData;
                                    int netid = _stMessages[count].NetworkID;
                                    try
                                    {
                                        if (dlc <= 8)
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
                                            byte[] targetMsg = new byte[dlc];
                                            for (byte i = 0; i < dlc; i++)
                                            {
                                                targetMsg[i] = tempMsg[i];
                                            }
                                            Spy3MessageEvent(new Pmessage((uint)CANID, dlc, targetMsg, type, true, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)), netid);
                                        }
                                        else
                                        {
                                            byte[] tempMsg = new byte[dlc];
                                            Marshal.Copy(_stMessages[count].iExtraDataPtr, tempMsg, 0, _stMessages[count].NumberBytesData);
                                            Spy3MessageEvent(new Pmessage((uint)CANID, dlc, tempMsg, type, true, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)), netid);
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
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

        public bool DoSetBaudrate(CAN_Driver.CAN_Parameter target_can_param, int _networkID)
        {
            try
            {
                ///unfd set is applied for neoVI Fire and neoVI Fire2
                if (target_can_param.BusProtocolMode == CANBus_ProtocolMode.CAN)
                {
                    if (icsNeoDll.icsneoSetBitRate(m_hObject, Convert_Bandrate(target_can_param.BaudRate), _networkID) == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                ///fd is only applied for neoVI Fire2
                else
                {
                    SFIRE2Settings Fire2ReadSettings = new SFIRE2Settings();
                    int iNumberOfBytes;
                    ///Get the Fire2 settings
                    iNumberOfBytes = System.Runtime.InteropServices.Marshal.SizeOf(Fire2ReadSettings);
                    if (icsNeoDll.icsneoGetFire2Settings(m_hObject, ref Fire2ReadSettings, iNumberOfBytes) == 1)
                    {
                        switch (_networkID)
                        {
                            case 1:
                                Fire2ReadSettings.Canfd1.FDMode = Covert_FDMode(true, target_can_param.BusProtocolMode);
                                Fire2ReadSettings.Canfd1.FDBaudrate = Covert_Settings_FDBaudrate(target_can_param.Data_rate);
                                break;
                            case 2:
                                Fire2ReadSettings.Canfd2.FDMode = Covert_FDMode(true, target_can_param.BusProtocolMode);
                                Fire2ReadSettings.Canfd2.FDBaudrate = Covert_Settings_FDBaudrate(target_can_param.Data_rate);
                                break;
                            case 42:
                                Fire2ReadSettings.Canfd3.FDMode = Covert_FDMode(true, target_can_param.BusProtocolMode);
                                Fire2ReadSettings.Canfd3.FDBaudrate = Covert_Settings_FDBaudrate(target_can_param.Data_rate);
                                break;
                            case 44:
                                Fire2ReadSettings.Canfd4.FDMode = Covert_FDMode(true, target_can_param.BusProtocolMode);
                                Fire2ReadSettings.Canfd4.FDBaudrate = Covert_Settings_FDBaudrate(target_can_param.Data_rate);
                                break;
                            case 61:
                                Fire2ReadSettings.Canfd5.FDMode = Covert_FDMode(true, target_can_param.BusProtocolMode);
                                Fire2ReadSettings.Canfd5.FDBaudrate = Covert_Settings_FDBaudrate(target_can_param.Data_rate);
                                break;
                            case 62:
                                Fire2ReadSettings.Canfd6.FDMode = Covert_FDMode(true, target_can_param.BusProtocolMode);
                                Fire2ReadSettings.Canfd6.FDBaudrate = Covert_Settings_FDBaudrate(target_can_param.Data_rate);
                                break;
                            case 96:
                                Fire2ReadSettings.Canfd7.FDMode = Covert_FDMode(true, target_can_param.BusProtocolMode);
                                Fire2ReadSettings.Canfd7.FDBaudrate = Covert_Settings_FDBaudrate(target_can_param.Data_rate);
                                break;
                            case 97:
                                Fire2ReadSettings.Canfd8.FDMode = Covert_FDMode(true, target_can_param.BusProtocolMode);
                                Fire2ReadSettings.Canfd8.FDBaudrate = Covert_Settings_FDBaudrate(target_can_param.Data_rate);
                                break;
                            default:
                                return false;
                        }
                        if (icsNeoDll.icsneoSetFire2Settings(m_hObject, ref Fire2ReadSettings, iNumberOfBytes, 1) == 0)
                        {
                            ///remind manager to scan device lost!!!
                            return false;
                        }
                        if (icsNeoDll.icsneoSetBitRate(m_hObject, Convert_Bandrate(target_can_param.Arbitration_rate), _networkID) == 1)
                        {
                            return true;
                        }
                        else
                        {
                            ///remind manager to scan device lost!!!
                            return false;
                        }
                    }
                    ///remind manager to scan device lost!!!
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.13-DoSetBaudrate() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        public bool DoSend(Pmessage msg, int _networkID)
        {
            try
            {
                if (!msg.FDorNot)
                    return Send_NormalMessage(msg, msg.SingleWireHighVoltage, _networkID);
                else
                    return Send_FDMessage(msg, _networkID);
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.14-DoSend() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        private bool Send_FDMessage(Pmessage msg, int _networkID)
        {
            try
            {
                icsSpyMessage stMessagesTx = new icsSpyMessage();
                uint iNumberTxed = 0;
                stMessagesTx.NetworkID = Convert.ToByte(_networkID);///need to debug, not same as the icsnAPI.chm
                stMessagesTx.ArbIDOrHeader = (int)msg.CANID;
                stMessagesTx.NumberBytesData = Convert.ToByte(msg.DLC);
                if (msg.DLC <= 8)
                {
                    byte[] _dataToSend = new byte[8];
                    if (msg.DLC > 0)
                    {
                        for (int i = 0; i < msg.DLC; i++)
                        {
                            _dataToSend[i] = msg.Data[i];
                        }
                    }
                    stMessagesTx.Data1 = _dataToSend[0];
                    stMessagesTx.Data2 = _dataToSend[1];
                    stMessagesTx.Data3 = _dataToSend[2];
                    stMessagesTx.Data4 = _dataToSend[3];
                    stMessagesTx.Data5 = _dataToSend[4];
                    stMessagesTx.Data6 = _dataToSend[5];
                    stMessagesTx.Data7 = _dataToSend[6];
                    stMessagesTx.Data8 = _dataToSend[7];
                }
                byte[] SendData = new byte[64];
                if (msg.DLC > 8)
                {
                    for (int i = 0; i < msg.DLC; i++)
                    {
                        SendData[i] = msg.Data[i];
                    }
                }
                System.Runtime.InteropServices.GCHandle gcHandle = System.Runtime.InteropServices.GCHandle.Alloc(SendData, System.Runtime.InteropServices.GCHandleType.Pinned);
                IntPtr CanFDptr = gcHandle.AddrOfPinnedObject();
                stMessagesTx.iExtraDataPtr = CanFDptr;
                stMessagesTx.Protocol = Convert.ToByte(ePROTOCOL.SPY_PROTOCOL_CANFD);
                if ((msg.CANID & 0x1FFF0000) > 0)
                    stMessagesTx.StatusBitField = Convert.ToInt32(eDATA_STATUS_BITFIELD_1.SPY_STATUS_XTD_FRAME);
                else
                    stMessagesTx.StatusBitField = 0;
                stMessagesTx.StatusBitField2 = 0;
                stMessagesTx.StatusBitField3 = 16; ///Enable bitrate switch, same as the icsnAPI.chm but need to debug
                stMessagesTx.ExtraDataPtrEnabled = 1;

                if (icsNeoDll.icsneoTxMessagesEx(m_hObject, ref stMessagesTx, stMessagesTx.NetworkID, 1, ref iNumberTxed, 0) == 1)
                {
                    gcHandle.Free();
                    return true;
                }
                gcHandle.Free();
                ///remind manager to scan device lost!!!
                return false;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.15-Send_FDMessage() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        private bool Send_NormalMessage(Pmessage msg, bool singleWireHighVoltage, int _networkID)
        {
            try
            {
                icsSpyMessage stMessagesTx = new icsSpyMessage();
                stMessagesTx.NetworkID = Convert.ToByte(_networkID);
                if ((msg.CANID & 0x1FFF0000) > 0)
                    stMessagesTx.StatusBitField = Convert.ToInt32(eDATA_STATUS_BITFIELD_1.SPY_STATUS_XTD_FRAME);
                else
                    stMessagesTx.StatusBitField = 0;
                if (_networkID == 3 || _networkID == 68)
                {
                    if (singleWireHighVoltage)
                        stMessagesTx.StatusBitField2 = Convert.ToInt32(eDATA_STATUS_BITFIELD_2.SPY_STATUS2_HIGH_VOLTAGE);
                    else
                        stMessagesTx.StatusBitField2 = 0;
                }
                stMessagesTx.ArbIDOrHeader = (int)msg.CANID;
                stMessagesTx.NumberBytesData = Convert.ToByte(msg.DLC);
                byte[] _dataToSend = new byte[8];
                if (msg.DLC > 0)
                {
                    for (int i = 0; i < msg.DLC; i++)
                    {
                        _dataToSend[i] = msg.Data[i];
                    }
                }
                stMessagesTx.Data1 = _dataToSend[0];
                stMessagesTx.Data2 = _dataToSend[1];
                stMessagesTx.Data3 = _dataToSend[2];
                stMessagesTx.Data4 = _dataToSend[3];
                stMessagesTx.Data5 = _dataToSend[4];
                stMessagesTx.Data6 = _dataToSend[5];
                stMessagesTx.Data7 = _dataToSend[6];
                stMessagesTx.Data8 = _dataToSend[7];

                if (icsNeoDll.icsneoTxMessages(m_hObject, ref stMessagesTx, stMessagesTx.NetworkID, 1) == 1)
                {
                    return true;
                }
                //remind manager to scan device lost!!!
                return false;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.3.16-Send_NormalMessage() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }



        private int Convert_Bandrate(CanBaudRate rate)
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

        private byte Covert_Settings_FDBaudrate(CanBaudRate rate)
        {
            switch (rate)
            {
                case CanBaudRate.Baud_33Kbit: return 1;
                case CanBaudRate.Baud_83Kbit: return 4;
                case CanBaudRate.Baud_100Kbit: return 5;
                case CanBaudRate.Baud_125Kbit: return 6;
                case CanBaudRate.Baud_250Kbit: return 7;
                case CanBaudRate.Baud_500Kbit: return 8;
                case CanBaudRate.Baud_1Mbit: return 10;
                case CanBaudRate.Baud_2Mbit: return 12;
                case CanBaudRate.Baud_4Mbit: return 13;
                case CanBaudRate.Baud_5Mbit: return 14;
                default: return 8;
            }
        }

        private byte Covert_FDMode(bool brs, CANBus_ProtocolMode mode)
        {
            if (brs == false && mode == CANBus_ProtocolMode.NON_ISO_CAN_FD)
                return 1;
            else if (brs == true && mode == CANBus_ProtocolMode.NON_ISO_CAN_FD)
                return 2;
            else if (brs == false && mode == CANBus_ProtocolMode.ISO_CAN_FD)
                return 3;
            else if (brs == true && mode == CANBus_ProtocolMode.ISO_CAN_FD)
                return 4;
            else
                return 0;
        }

    }

    public class Pmessage
    {
        public uint CANID;
        public byte DLC;
        public byte[] Data;
        public TxRxType TxRx;
        public bool FDorNot;
        public UInt64 LastReceivedTimeMs;
        public bool SingleWireHighVoltage;
        public string Error = "";

        public Pmessage()
        {
            TxRx = TxRxType.Tx;
            LastReceivedTimeMs = 0;
            SingleWireHighVoltage = false;
        }

        internal Pmessage(uint canID, byte dlc, byte[] data, TxRxType txRx, bool fdOrNot, ulong lastReceiveTimeMs, bool singleWireHighVoltage = false)
        {
            //get right data
            if (dlc < data.Length)
                data = data.Take(dlc).ToArray();
            else if (dlc > data.Length)
            {
                int a = dlc - data.Length;
                List<byte> b = data.ToList();
                for (int i = 0; i < a; i++)
                {
                    b.Add(0);
                }
                data = b.ToArray();
            }

            this.CANID = canID;
            this.DLC = dlc;
            this.Data = data;
            this.TxRx = txRx;
            this.FDorNot = fdOrNot;
            this.LastReceivedTimeMs = lastReceiveTimeMs;
            this.SingleWireHighVoltage = singleWireHighVoltage;
        }
    }

    public class Lmessage
    {
        public uint ID;
        public byte DLC;
        public byte[] Data;
        public TxRxType TxRx;
        public long LastReceivedTimeMs;
        public int CheckSum;
        public LINDirection Direction;
        public string Error = "";

        public Lmessage()
        {
            TxRx = TxRxType.Tx;
            LastReceivedTimeMs = 0;
        }

        public Lmessage(uint ID, byte dlc, byte[] data)
        {
            //get right data
            if (dlc < data.Length)
                data = data.Take(dlc).ToArray();
            else if (dlc > data.Length)
            {
                int a = dlc - data.Length;
                List<byte> b = data.ToList();
                for (int i = 0; i < a; i++)
                {
                    b.Add(0);
                }
                data = b.ToArray();
            }

            this.ID = ID;
            this.DLC = dlc;
            this.Data = data;
        }
    }

    public class Fmessage
    {
        public uint FrameID;
        public uint DLC;
        public byte[] Data = { 0 };
        public TxRxType TxRx;
        public int CycleCount;
        public long LastReceivedTimeMs;
        public string Error = "";

        public Fmessage()
        {
            TxRx = TxRxType.Tx;
            LastReceivedTimeMs = 0;
        }

        internal Fmessage(uint frameID, uint dlc, byte[] data, TxRxType txRx, int cycleCount, long lastReceiveTimeMs)
        {
            //get right data
            if (dlc < data.Length)
                data = data.Take((int)dlc).ToArray();
            else if (dlc > data.Length)
            {
                int a = (int)dlc - data.Length;
                List<byte> b = data.ToList();
                for (int i = 0; i < a; i++)
                {
                    b.Add(0);
                }
                data = b.ToArray();
            }

            this.FrameID = frameID;
            this.DLC = dlc;
            this.Data = data;
            this.TxRx = txRx;
            this.CycleCount = cycleCount;
            this.LastReceivedTimeMs = lastReceiveTimeMs;
        }
    }
}

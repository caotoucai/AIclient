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
using ZLGCAN;

namespace CAN_Library.CAN_Driver
{
    
    class ZLGCAN_Warpper
    {        

        public struct DeviceType
        {
            public int type;  
            public bool isCANFD;
            public uint channel_count;
            public DeviceType(int device_type, uint count, bool is_canfd)
            {
                type = device_type;
                channel_count = count;
                isCANFD = is_canfd;
            }
        };

        public static int MAX_DEVICE = 2;
        public static DeviceType[] Support_CANFD_Device = {
            new DeviceType(Define.ZCAN_USBCANFD_100U, 1,true) ,
            new DeviceType(Define.ZCAN_USBCANFD_200U,2,true) ,
            new DeviceType(Define.ZCAN_USBCANFD_MINI,1,true)
        };
        //NOTE: ZCAN_USBCANFD_100U device can also open with ZCAN_USBCANFD_200U/ZCAN_USBCANFD_MINI type.  

        public enum CanFD_Standard
        {
            CANFD_ISO,
            NON_ISO,
        }

        public enum Resistance_Enable
        {
            ENABLE_RESISTANCE,
            DISABLE_RESISTANCE,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ZCAN_DEVICE_INFO
        {
            public UInt16 hw_Version;
            public UInt16 fw_Version;
            public UInt16 dr_Version;
            public UInt16 in_Version;
            public UInt16 irq_Num;
            public byte can_Num;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] str_Serial_Num;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public char[] str_hw_Type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UInt16[] reserved;
        }
       
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_GetDeviceInf(IntPtr device_handle, IntPtr pdeviceInf);
    }

    class CAN_Driver_ZLG : CAN_Driver
    {
        ZLG_Device _myDevice;
        int _networkID;
        CAN_Parameter _can_param;
        
        IntPtr device_handle;
        IntPtr channel_handle;

        //
        IntPtr ptr_handle;
        IProperty property_;
        //
        recvdatathread recv_data_thread_ = null;  //recv        

        const CanBaudRate default_abaudrate = CanBaudRate.Baud_500Kbit;
        const CanBaudRate default_dbaudrate = CanBaudRate.Baud_2Mbit;


        public CAN_Driver_ZLG(string deviceName, byte channelID, CAN_Parameter can_param)
        {
            try
            {
                if (CAN_Manager.ZLG_Device_List.Any(p => p.Display_Name == deviceName))
                {
                    this._myDevice = CAN_Manager.ZLG_Device_List.First(p => p.Display_Name == deviceName);
                    this._networkID = channelID;
                    this._can_param = can_param;
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("CAN_Driver_ZLG() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        protected override bool DoInitialize(CAN_Parameter target_can_param)
        {
            try
            {
                if (_myDevice.IsOpen == true)
                {
                    return false;
                }

                device_handle = Method.ZCAN_OpenDevice((uint)_myDevice.Device_Info.type, (uint)_myDevice.device_index, 0);
                if (IntPtr.Zero == device_handle)
                {
                    return false;
                }

                      
                ptr_handle = Method.GetIProperty(device_handle);
                if (IntPtr.Zero == ptr_handle)
                {
                    return false;
                }

                property_ = (IProperty)Marshal.PtrToStructure((IntPtr)((UInt32)ptr_handle), typeof(IProperty));

                if (target_can_param.IsFDSupport == true)
                {
                    if (!setCANFDStandard((int)ZLGCAN_Warpper.CanFD_Standard.CANFD_ISO)) //设置CANFD标准
                    {
                        return false;
                    }                                     
                }

                if (!DoSetBaudrate(target_can_param))
                {
                    return false;
                }

                ZCAN_CHANNEL_INIT_CONFIG config_ = new ZCAN_CHANNEL_INIT_CONFIG();

                config_.canfd.mode = 0;  //Normal
                if (target_can_param.IsFDSupport == true)
                {
                    config_.can_type = Define.TYPE_CANFD;
                }
                else
                {
                    config_.can_type = Define.TYPE_CAN;
                    config_.can.filter = 0;
                    config_.can.acc_code = 0;
                    config_.can.acc_mask = 0xFFFFFFFF;
                }

                IntPtr pConfig = Marshal.AllocHGlobal(Marshal.SizeOf(config_));
                Marshal.StructureToPtr(config_, pConfig, true);

                //int size = sizeof(ZCAN_CHANNEL_INIT_CONFIG);
                //IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(size);
                //System.Runtime.InteropServices.Marshal.StructureToPtr(config_, ptr, true);
                channel_handle = Method.ZCAN_InitCAN(device_handle, (uint)_networkID, pConfig);
                Marshal.FreeHGlobal(pConfig);

                if (!setResistanceEnable((int)ZLGCAN_Warpper.Resistance_Enable.ENABLE_RESISTANCE))
                {
                    return false;
                }

                // 
                //if (!setFilter())
                //{
                //    return false;
                //}

                if (Method.ZCAN_StartCAN(channel_handle) != Define.STATUS_OK)
                {
                    return false;
                }

                _myDevice.IsOpen = true;                
                recv_data_thread_ = new recvdatathread();
                recv_data_thread_.setChannelHandle(channel_handle);
                recv_data_thread_.setStart(true);
                recv_data_thread_.RecvCANData += this.CANRecv_Thread;
                recv_data_thread_.RecvFDData += this.CANFDRecv_Thread;
               
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("DoInitialization() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }           
        }


        protected override bool DoUninitialize()
        {
            try
            {
                recv_data_thread_.RecvCANData -= this.CANRecv_Thread;
                recv_data_thread_.RecvFDData -= this.CANFDRecv_Thread;
                //stop recv thread
                recv_data_thread_.setStart(false);

                Method.ReleaseIProperty(ptr_handle);

                if (IntPtr.Zero != device_handle)
                {
                    Method.ZCAN_CloseDevice(device_handle);
                    device_handle = IntPtr.Zero;
                    channel_handle = IntPtr.Zero;
                    _myDevice.IsOpen = false;
                }
               
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run("1.2.3-DoUninitialize() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }


        private void CANRecv_Thread(ZCAN_Receive_Data[] data, uint len)
        {
            for (uint i = 0; i < len; ++i)
            {
                ZCAN_Receive_Data can = data[i];
                uint can_id = data[i].frame.can_id; 

                base.NotifyMessageReceived(new Pmessage(can_id, can.frame.can_dlc, can.frame.data, TxRxType.Rx, false, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)));
                                               
            }       

        }

        private void CANFDRecv_Thread(ZCAN_ReceiveFD_Data[] data, uint len)
        {
            for (uint i = 0; i < len; ++i)
            {
                ZCAN_ReceiveFD_Data canfd = data[i];
                uint can_id = data[i].frame.can_id;
                base.NotifyMessageReceived(new Pmessage(can_id, canfd.frame.len, canfd.frame.data, TxRxType.Rx, true, Convert.ToUInt64(DateTime.Now.Ticks / 10000L)));
            }
        }

        public uint MakeCanId(uint id, int rtr, int err)
        {
            int eff = (id & 0x10000000) > 0 ? 1 : 0; //1:extend frame 0:standard frame
            uint ueff = (uint)(!!(Convert.ToBoolean(eff)) ? 1 : 0);
            uint urtr = (uint)(!!(Convert.ToBoolean(rtr)) ? 1 : 0);
            uint uerr = (uint)(!!(Convert.ToBoolean(err)) ? 1 : 0);
            return id | ueff << 31 | urtr << 30 | uerr << 29;
        }

        protected override bool DoSend(Pmessage msg)
        {
            try
            {
                if (!_myDevice.IsOpen)
                    return false;

                if (!msg.FDorNot)
                {
                    return Send_CAN_Msg(msg);
                }
                else
                {
                    return Send_CAN_FD_Msg(msg);
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run("DoSend() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                return false;
            }
        }

        private bool Send_CAN_Msg(Pmessage msg)
        {
            byte[] By_Raw_Data = new byte[8];        //raw data
            for (int i = 0; i<8; i++)
            {
                if (i < msg.DLC)                
                    By_Raw_Data[i] = msg.Data[i];                
                else
                    By_Raw_Data[i] = 0;
            }      
            //By_Raw_Data = msg.Data;
            ZCAN_Transmit_Data can_data = new ZCAN_Transmit_Data();
            can_data.frame.can_id = MakeCanId(msg.CANID, 0, 0); 
            can_data.frame.data = By_Raw_Data;
            can_data.frame.can_dlc = msg.DLC;
            can_data.transmit_type = 0;  //Normal send
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(can_data));
            Marshal.StructureToPtr(can_data, ptr, true);
            uint result = Method.ZCAN_Transmit(channel_handle, ptr, 1);
            Marshal.FreeHGlobal(ptr);
            return true;
        }

        private bool Send_CAN_FD_Msg(Pmessage msg)
        {
            byte[] By_Raw_Data = new byte[64];        //raw data            
            for (int i = 0; i<64; i++)
            {
                if (i < msg.DLC)                
                    By_Raw_Data[i] = msg.Data[i];                
                else
                    By_Raw_Data[i] = 0;
            }            

            ZCAN_TransmitFD_Data canfd_data = new ZCAN_TransmitFD_Data();
            canfd_data.frame.can_id = MakeCanId(msg.CANID, 0, 0);
            canfd_data.frame.data = By_Raw_Data;
            canfd_data.frame.len = msg.DLC;
            canfd_data.transmit_type = 0;  //Normal send
            canfd_data.frame.flags = 0; //Exp speed
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(canfd_data));
            Marshal.StructureToPtr(canfd_data, ptr, true);
            uint result = Method.ZCAN_TransmitFD(channel_handle, ptr, 1);
            Marshal.FreeHGlobal(ptr);
            return true;
        }

        private bool setResistanceEnable(int enable)
        {
            string path = _networkID + "/initenal_resistance";
            string value = enable.ToString();
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            return 1 == property_.SetValue(path, Encoding.ASCII.GetBytes(value));
        }

        private bool setFilter()
        {
            string path = _networkID + "/filter_clear";//清除滤波
            string value = "0";
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            if (0 == property_.SetValue(path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }

            path = _networkID + "/filter_mode";
            value = "0";
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            if (0 == property_.SetValue(path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }

            path = _networkID + "/filter_start";
            value = "0";
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            if (0 == property_.SetValue(path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }

            path = _networkID + "/filter_end";
            value = "1";
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            if (0 == property_.SetValue(path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }

            path = _networkID + "/filter_ack";//滤波生效
            value = "0";
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            if (0 == property_.SetValue(path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }

            //如果要设置多条滤波，在清除滤波和滤波生效之间设置多条滤波即可
            return true;
        }

        //设置CANFD标准
        private bool setCANFDStandard(int canfd_standard)
        {
            string path = _networkID + "/canfd_standard";
            string value = canfd_standard.ToString();
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            return 1 == property_.SetValue(path, Encoding.ASCII.GetBytes(value));
        }


        protected override bool DoSetBaudrate(CAN_Parameter can_param)
        {
            if (can_param.BusProtocolMode == CANBus_ProtocolMode.CAN)
            {
                string path = _networkID + "/baud_rate";
                string value = Convert_Bandrate(can_param.BaudRate).ToString();
                return 1 == property_.SetValue(path, Encoding.ASCII.GetBytes(value));
            }
            else
            {
                string path = _networkID + "/canfd_abit_baud_rate";
                string value = Convert_Bandrate(can_param.Arbitration_rate).ToString();
                if (1 != property_.SetValue(path, Encoding.ASCII.GetBytes(value)))
                {
                    return false;
                }
                path = _networkID + "/canfd_dbit_baud_rate";
                value = Convert_Bandrate(can_param.Data_rate).ToString();
                if (1 != property_.SetValue(path, Encoding.ASCII.GetBytes(value)))
                {
                    return false;
                }
            }
            return true; 
        }


        protected bool DoSetBaudrate(CanBaudRate abaud, CanBaudRate dbaud)
        {           
            string path = _networkID + "/canfd_abit_baud_rate";
            string value = Convert_Bandrate(abaud).ToString();
            if (1 != property_.SetValue(path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }
            path = _networkID + "/canfd_dbit_baud_rate";
            value = Convert_Bandrate(dbaud).ToString();
            if (1 != property_.SetValue(path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }
            
            return true;
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

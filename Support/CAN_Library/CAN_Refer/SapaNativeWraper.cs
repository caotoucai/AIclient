using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using CAN_Library;
using System.Linq;
using System.Threading;
using System.IO;
using System.Collections.ObjectModel;

namespace SapaWraper
{
    public partial class SAPAConnection : IDisposable //Partial class to allow plug-in specific code
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct XL_DEVICE_T
        {
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public IntPtr device_handle;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string str_Serial_Num;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XL_Name_FIELD
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string str_Name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XL_STR_20
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string str_Node;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XL_STR_16
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public char[] value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XL_STR_8
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public Byte[] value;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct XL_FLEXRAY_FRAME_INFO
        {
            public UInt32 slotId;
            public UInt16 baseCycle;
            public UInt16 cycleRepetition;

            public UInt16 cycleCounter;
            public UInt32 cycleTime;
            public byte isTimeTriggered;
            public XL_Name_FIELD name;
            public XL_STR_20 transmitter;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public XL_STR_20[] receivers;
            public Byte receiversNum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public XL_FLEXRAY_SIGNAL_INFO[] signalsList;
            public Byte signalsNum;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct XL_FLEXRAY_SIGNAL_INFO
        {
            public XL_Name_FIELD name;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
            public string description;
            public double defaultValue;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public XL_STR_100[] valueRange;
            public Byte valueRangeNum;
            public double minPhyValue;
            public double maxPhyValue;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
            public string unit;
            public double factor;
            public double offset;
            public XL_STR_20 str_transmitter;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public XL_STR_20[] receivers;
            public Byte receiversNum;
        }

        //[StructLayout(LayoutKind.Sequential)]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct XL_FLEXRAY_FRAME_MESSAGE
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UInt32[] flag;
            public UInt64 timestamp;
            public UInt64 timestampReserved;  //32
            public Byte netProtocol;          //33
            public Byte netInstance;            //34
            public UInt32 type;                 //38
            public UInt32 slotId;               //42
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            //public Byte[] data;
            public IntPtr data;             //46
            public int dataLength;          //50
            public Byte bus;
            public Byte tx;
            public Byte offset;
            public Byte repetition;
            public Byte cycleCount;
            public Byte channel;
            public Byte txMode;   //57
            public UInt16 index; //59
            public IntPtr name;  //63
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            //public char[] name;
            public Byte nameLength;  //64
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
            public Byte[] reserved;
        }

        //[StructLayout(LayoutKind.Sequential)]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct XL_CAN_FRAME_MESSAGE
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UInt32[] flag;              //16
            public UInt64 timestamp;           //8  
            public UInt64 timestampReserved;  //8
            public Byte netProtocol;          //1 - 33
            public Byte netInstance;            //1 - 34
            public UInt32 type;                 //4 - 38
            public UInt32 arbid;               //CANID, 4 - 42
            public IntPtr data;             //4
            public int datalength;          //4
            public Byte bus;
            public Byte tx;
            public Byte dlc;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 29)]   //7
            public Byte[] reserved;
        }

        //[StructLayout(LayoutKind.Sequential)]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct XL_LIN_FRAME_MESSAGE
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UInt32[] flag;              //16
            public UInt64 timestamp;           //8  
            public UInt64 timestampReserved;  //8
            public Byte netProtocol;          //1 - 33
            public Byte netInstance;            //1 - 34
            public UInt32 type;                 //4 - 38
            public UInt32 id;               //4 - 42
            public IntPtr data;             //4
            public int datalength;          //4
            public Byte bus;
            public Byte tx;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public Byte[] reserved;
        }

        //[StructLayout(LayoutKind.Sequential)]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct XL_COMMON_FRAME_MESSAGE
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UInt32[] flag;              //16
            public UInt64 timestamp;           //8  
            public UInt64 timestampReserved;  //8
            public Byte netProtocol;          //1 - 33
            public Byte netInstance;            //1 - 34
            public UInt32 type;                 //4 - 38
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public Byte[] header;               //4 - 42
            public IntPtr data;             //4
            public int datalength;          //4
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public Byte[] reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XL_STR_100
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string str_value;
        }

        public enum SPXL_PROTOCOL
        {
            SPXL_PROTOCOL_INVALID = 0,
            SPXL_PROTOCOL_CAN = 1,
            SPXL_PROTOCOL_CANFD = 2,
            SPXL_PROTOCOL_ISO15765_2 = 3,
            SPXL_PROTOCOL_LIN = 4,
            SPXL_PROTOCOL_ETH = 5,
            SPXL_PROTOCOL_FR = 6,
            SPXL_PROTOCOL_GBLSETTINGS = 31,
        };

        bool m_bStart = false;
        string _device_name = "";
        XL_DEVICE_T device_9;
        IntPtr channel_handle_ = IntPtr.Zero;
        Thread recv_thread_ = null;
        public const string DLL_NAME = "spxlib.dll";
        public const string ENGINE_SERVER_IP = "127.0.0.1";
        public const int ENGINE_SERVER_PORT = 8001;
        public ObservableCollection<XL_DEVICE_T> device_list = new ObservableCollection<XL_DEVICE_T>();
        public ObservableCollection<XL_DEVICE_T> opened_device_list = new ObservableCollection<XL_DEVICE_T>();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void RecvCanDataEventHandler(XL_CAN_FRAME_MESSAGE msg);//CAN数据接收事件委托

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void RecvLinDataEventHandler(XL_LIN_FRAME_MESSAGE msg);//LIN数据接收事件委托

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void RecvFlexrayDataEventHandler(XL_FLEXRAY_FRAME_MESSAGE msg);//LIN数据接收事件委托

        public event RecvCanDataEventHandler OnRecvCanDataEvent;
        public event RecvLinDataEventHandler OnRecvLinDataEvent;
        public event RecvFlexrayDataEventHandler OnRecvFlexrayDataEvent;      

        internal static class SapaNativeWraper
        {
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_init();

            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_init_debug();

            //int SPXLIBSHARED_EXPORT spxl_destroy(void);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_destroy();

            //int SPXLIBSHARED_EXPORT spxl_get_libversion(char* version, size_t length);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_get_libversion(string filePath, int length);

            //int SPXLIBSHARED_EXPORT spxl_find_alldevs(xl_device_t *devices, size_t *count);
            //size_t = uint
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_find_alldevs(IntPtr devices, ref Int32 count);

            //int SPXLIBSHARED_EXPORT spxl_find_alldevs(xl_device_t *devices, size_t *count);
            //size_t = uint
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_find_alldevs_ex(IntPtr devices, ref Int32 count, string ip, Int16 port);

            //int spxl_load_fibex(const char* filePath);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_load_fibex(string filePath);

            //int spxl_open(const xl_device_t *device);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_open(ref XL_DEVICE_T device);

            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_close(ref XL_DEVICE_T device);

            //int spxl_change_fr_signal_raw_value(const xl_device_t *device, const char* frame_name, const char* signal_name, uint64_t raw_value);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_change_fr_signal_raw_value(ref XL_DEVICE_T device, string frame_name, string signal_name, ulong raw_value);

            //SPXLIBSHARED_EXPORT int   spxl_change_fr_signal_raw_sighed_value(const xl_device_t *device, const char *frame_name, const char *signal_name, int64_t raw_value);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_change_fr_signal_raw_sighed_value(ref XL_DEVICE_T device, string frame_name, string signal_name, long raw_value);

            //int spxl_send_set_fr_command(const xl_device_t* device, const char* frame_name);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_send_set_fr_command(ref XL_DEVICE_T device, string frame_name);

            //int spxl_send_fr_msg_command(const xl_device_t* device, xl_msg_t *msg);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_send_fr_msg_command(ref XL_DEVICE_T device, IntPtr msg);

            //int SPXLIBSHARED_EXPORT spxl_send_del_fr_msg_command(const xl_device_t *device, xl_msg_fr_t *fr_msg);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_send_del_fr_msg_command(ref XL_DEVICE_T device, IntPtr msg);

            // int spxl_transmit_one(const xl_device_t *device, xl_msg_t *msg, xl_uint8_t *data, size_t data_length);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_transmit_one(ref XL_DEVICE_T device, IntPtr msg, byte[] data, int data_length);

            //int SPXLIBSHARED_EXPORT spxl_get_all_fr_msgs_name_List(char msgsName[][50], int &length);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_get_all_fr_msgs_name_List(IntPtr msgsNames, ref Int32 length);

            //int SPXLIBSHARED_EXPORT spxl_get_all_fr_signals_name_List(char sigsName[][50], int &length);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_get_all_fr_signals_name_List(IntPtr sigsName, ref Int32 length);

            //int SPXLIBSHARED_EXPORT spxl_get_fr_signals_name_List(const char *frame_name, char sigsName[][50], int &length);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_get_fr_signals_name_List(string frame_name, IntPtr sigsName, ref Int32 length);

            //int SPXLIBSHARED_EXPORT spxl_get_fr_msg_info(const char *frame_name, FibexFrameInfo &msgInfo);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            //public static extern int spxl_get_fr_msg_info(String frame_name, ref XL_FLEXRAY_FRAME_INFO msgInfo);
            public static extern int spxl_get_fr_msg_info(String frame_name, IntPtr msgInfo);

            //int SPXLIBSHARED_EXPORT spxl_get_fr_signal_info(const char* signal_name, FibexSignalInfo &sigInfo);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            //public static extern int spxl_get_fr_msg_info(String frame_name, ref XL_FLEXRAY_FRAME_INFO msgInfo);
            public static extern int spxl_get_fr_signal_info(String frame_name, IntPtr sigInfo);

            //int spxl_send_command(const xl_device_t *device, const char *cmd, int cmdlen);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_send_command(ref XL_DEVICE_T device, string cmd, UInt32 cmdlen);

            //int spxl_enable_can(const xl_device_t* device, xl_uint8_t instance);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_enable_can(ref XL_DEVICE_T device, byte instance);

            //int spxl_disable_can(const xl_device_t* device, xl_uint8_t instance);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_disable_can(ref XL_DEVICE_T device, byte instance);

            //int spxl_set_baudrate(const xl_device_t* device, int instance, xl_int64_t baudrate);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_set_can_baudrate(ref XL_DEVICE_T device, int instance, Int64 baudrate);

            //int spxl_set_can_transceiver(const xl_device_t* device, int instance, xl_uint8_t txvr, xl_uint8_t mode);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_set_can_transceiver(ref XL_DEVICE_T device, int instance, byte txvr, byte mode);

            //int spxl_set_canfd_options(const xl_device_t* device,
            //                  int instance,
            //                  xl_uint8_t enable,
            //                  xl_uint8_t brs,
            //                  xl_int64_t arbi_rate,
            //                  xl_int64_t data_rate);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_set_canfd_options(ref XL_DEVICE_T device, int instance, byte enable, byte brs, Int64 arbi_rate, Int64 data_rate);

             //int SPXLIBSHARED_EXPORT spxl_receive(const xl_device_t *device, xl_msg_t* msgs, xl_uint32_t *count, xl_uint32_t timeout);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_receive(ref XL_DEVICE_T device, IntPtr msgs, ref UInt32 count, UInt32 timeout);

            //void SPXLIBSHARED_EXPORT spxl_enable_rx_polling(const xl_device_t *device);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_enable_rx_polling(ref XL_DEVICE_T device);

            //void SPXLIBSHARED_EXPORT spxl_disable_rx_polling(const xl_device_t* device);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern void spxl_disable_rx_polling(ref XL_DEVICE_T device);

            //int SPXLIBSHARED_EXPORT spxl_flush_rx_polling(const xl_device_t* device);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_flush_rx_polling(ref XL_DEVICE_T device);

            //int SPXLIBSHARED_EXPORT spxl_send_del_all_fr_command(const xl_device_t* device);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_send_del_all_fr_command(ref XL_DEVICE_T device);

            //int SPXLIBSHARED_EXPORT spxl_send_del_fr_command(const xl_device_t* device, const char* frame_name);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_send_del_fr_command(ref XL_DEVICE_T device, string frame_name);

            //int SPXLIBSHARED_EXPORT spxl_close(const xl_device_t *device);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int spxl_close(IntPtr device);
        }

        ~SAPAConnection()
        {
        }

        /// <summary>
        /// Releases all resources used by the SBE2ConnectionWrapper.SBE2Connection
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (recv_thread_ != null && recv_thread_.IsAlive)
            {
                recv_thread_.Abort();
                recv_thread_ = null;
                m_bStart = false;
            }
        }

        public bool RequestDeviceList()
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int r = SapaNativeWraper.spxl_init();
                // 再定义定义结构体数组
                int count = 8;
                device_list.Clear();
                // 再分配内存空间
                int size = Marshal.SizeOf(typeof(XL_DEVICE_T));
                ptr = Marshal.AllocHGlobal((int)count * size);

                r = SapaNativeWraper.spxl_find_alldevs_ex(ptr, ref count, ENGINE_SERVER_IP, ENGINE_SERVER_PORT);

                // 复制数据到结构体中
                for (int i = 0; i < count; ++i)
                {
                    XL_DEVICE_T device = (XL_DEVICE_T)Marshal.PtrToStructure(
                        (IntPtr)((Int64)ptr + i * size), typeof(XL_DEVICE_T));
                    device_list.Add(device);
                }
                // 释放内存空间
                Marshal.FreeHGlobal(ptr);
                ptr = IntPtr.Zero;
                return true;
            }
            catch (Exception ex)
            {
                Marshal.FreeHGlobal(ptr);
                ptr = IntPtr.Zero;
                return false;
            }

        }

        public bool OpenDevice(string device_name)
        {
            int r = 0;
            XL_DEVICE_T device;
            if (device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            //int count = 8;
            //int size = Marshal.SizeOf(typeof(XL_DEVICE_T));
            //IntPtr ptr = Marshal.AllocHGlobal((int)count * size);
            //r = SapaNativeWraper.spxl_find_alldevs_ex(ptr, ref count, ENGINE_SERVER_IP, ENGINE_SERVER_PORT);

            //// 复制数据到结构体中
            //for (int i = 0; i < count; ++i)
            //{ 
            //    XL_DEVICE_T d = (XL_DEVICE_T)Marshal.PtrToStructure(
            //        (IntPtr)((Int64)ptr + i * size), typeof(XL_DEVICE_T));

            //    if (device_name == d.str_Serial_Num)
            //    {
            //        device = d;
            //        break;
            //    }
            //}
            //// 释放内存空间
            //Marshal.FreeHGlobal(ptr);
            //ptr = IntPtr.Zero;

            r = SapaNativeWraper.spxl_open(ref device);
            if (r != 0)
            {
                Log.Logger_Run("SAPAConnection: Fail to spxl_open.");
                return false;
            }

            opened_device_list.Add(device);

            bool ret = Enable_rx_polling(device_name);
            
            return true;
        }

        public bool CloseDevice(string device_name)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            int r = SapaNativeWraper.spxl_close(ref device);
            if (r != 0)
            {
                Log.Logger_Run("SAPAConnection: Fail to spxl_open.");
                return false;
            }
            opened_device_list.Remove(device);
            return true;
        }

        public bool Load_fibex(string filename)
        {
            int r = SapaNativeWraper.spxl_load_fibex(filename);
            if (r != 0)
            {
                Log.Logger_Run("SAPAConnection: Fail to spxl_load_fibex.");
                return false;
            }
            return true;
        }

        public bool Change_fr_signal_raw_value(string device_name, string frame_name, string signal_name, long raw_value)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            int r = SapaNativeWraper.spxl_change_fr_signal_raw_sighed_value(ref device, frame_name, signal_name, raw_value);  
            if (r != 0)
            {
                Log.Logger_Run("SAPAConnection: Change_fr_signal_raw_value.");
                return false;
            }
            return true;
        }

        public bool Send_set_fr_command(string device_name, string frame_name)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            int r = SapaNativeWraper.spxl_send_set_fr_command(ref device, frame_name);
            if (r != 0)
            {
                Log.Logger_Run("SAPAConnection: spxl_send_set_fr_command.");
                return false;
            }
            return true;
        }

        public bool Send_del_fr_command(string device_name, string frame_name)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            int r = SapaNativeWraper.spxl_send_del_fr_command(ref device, frame_name);  //can only delete spxl_send_set_fr_command message
            if (r != 0)
            {
                Log.Logger_Run("SAPAConnection: Delete_fr_command.");
                return false;
            }
            return true;
        }

        public int Send_fr_raw_data(string device_name, XL_FLEXRAY_FRAME_MESSAGE fr_msg)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return -1;
            }

            ///hardcode here for flexray
            fr_msg.netProtocol = 6;
            fr_msg.netInstance = 0;
            fr_msg.channel = 2;
            fr_msg.index = 0xFFFF;

            int fr_size = Marshal.SizeOf(typeof(XL_FLEXRAY_FRAME_MESSAGE));
            IntPtr ptr = Marshal.AllocHGlobal(fr_size);
            Marshal.StructureToPtr(fr_msg, ptr, true);
            int r = SapaNativeWraper.spxl_send_fr_msg_command(ref device, ptr);
            XL_FLEXRAY_FRAME_MESSAGE recv_msg = (XL_FLEXRAY_FRAME_MESSAGE)Marshal.PtrToStructure((IntPtr)((UInt32)ptr), typeof(XL_FLEXRAY_FRAME_MESSAGE));
            r = recv_msg.index;
            Marshal.FreeHGlobal(ptr);
            ptr = IntPtr.Zero;
            return r;
        }

        public int Send_del_fr_raw_data(string device_name, XL_FLEXRAY_FRAME_MESSAGE fr_msg)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return -1;
            }

            ///hardcode here for flexray
            fr_msg.netProtocol = 6;
            fr_msg.netInstance = 0;
            fr_msg.channel = 2;

            int fr_size = Marshal.SizeOf(typeof(XL_FLEXRAY_FRAME_MESSAGE));
            IntPtr ptr = Marshal.AllocHGlobal(fr_size);
            Marshal.StructureToPtr(fr_msg, ptr, true);
            int r = SapaNativeWraper.spxl_send_del_fr_msg_command(ref device, ptr);
            Marshal.FreeHGlobal(ptr);
            ptr = IntPtr.Zero;
            return r;
        }

        public bool Enable_rx_polling(string device_name)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            int r = SapaNativeWraper.spxl_enable_rx_polling(ref device);
            if (r != 0)
            {
                Log.Logger_Run("SAPAConnection: Fail to spxl_enable_rx_polling.");
                return false;
            }
            m_bStart = true;
            recv_thread_ = new Thread(new ParameterizedThreadStart(RecvDataFunc));
            recv_thread_.IsBackground = true;
            recv_thread_.Start(device_name);
            return true;
        }

        public bool Send_Str_Command(string device_name, string cmd)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            int r = SapaNativeWraper.spxl_send_command(ref device, cmd, (uint)cmd.Length);
            return true;
        }

        public bool Send_CAN_Message(string device_name,int instance_id, bool FDorNot, uint CANid, uint dlc, byte[] data)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            XL_CAN_FRAME_MESSAGE can_msg = new XL_CAN_FRAME_MESSAGE();
            can_msg.arbid = CANid;
            can_msg.datalength = (int)dlc;
            can_msg.dlc = (byte)dlc;

            if (FDorNot)
                can_msg.netProtocol = 2;
            else
                can_msg.netProtocol = 1;

            can_msg.netInstance = (byte)instance_id;

            IntPtr unmanagedPtr = Marshal.AllocHGlobal((int)dlc);
            Marshal.Copy(data, 0, unmanagedPtr, (int)dlc);
            can_msg.data = unmanagedPtr;

            int fr_size = Marshal.SizeOf(typeof(XL_CAN_FRAME_MESSAGE));
            IntPtr ptr = Marshal.AllocHGlobal(fr_size);
            Marshal.StructureToPtr(can_msg, ptr, true);

            int r = SapaNativeWraper.spxl_transmit_one(ref device, ptr, data, (int)dlc);

            Marshal.FreeHGlobal(unmanagedPtr);
            unmanagedPtr = IntPtr.Zero;
            Marshal.FreeHGlobal(ptr);
            ptr = IntPtr.Zero;
            return true;
        }

        public bool Send_LIN_Message(string device_name, int instance_id, uint LINid, uint dlc, byte[] data)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            XL_LIN_FRAME_MESSAGE can_msg = new XL_LIN_FRAME_MESSAGE();
            can_msg.id = LINid;
            can_msg.datalength = (int)dlc; 
            can_msg.netProtocol = 4;          

            can_msg.netInstance = (byte)instance_id;

            IntPtr unmanagedPtr = Marshal.AllocHGlobal((int)dlc);
            Marshal.Copy(data, 0, unmanagedPtr, (int)dlc);
            can_msg.data = unmanagedPtr;

            int fr_size = Marshal.SizeOf(typeof(XL_LIN_FRAME_MESSAGE));
            IntPtr ptr = Marshal.AllocHGlobal(fr_size);
            Marshal.StructureToPtr(can_msg, ptr, true);

            int r = SapaNativeWraper.spxl_transmit_one(ref device, ptr, data, (int)dlc);

            Marshal.FreeHGlobal(unmanagedPtr);
            unmanagedPtr = IntPtr.Zero;
            Marshal.FreeHGlobal(ptr);
            ptr = IntPtr.Zero;
            return true;
        }

        public bool Enable_CAN(string device_name, int can_instance)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            int r = SapaNativeWraper.spxl_enable_can(ref device, (byte)can_instance);
            return true;
        }

        public bool Disable_CAN(string device_name, int can_instance)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            int r = SapaNativeWraper.spxl_disable_can(ref device, (byte)can_instance);
            return true;
        }


        public bool set_can_baudrate(string device_name, int can_instance, long baudrate)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            int r = SapaNativeWraper.spxl_set_can_baudrate(ref device, can_instance, (Int64)baudrate);
            return true;
        }

        public bool set_can_transceiver(string device_name, int can_instance, byte txvr, byte mode)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            int r = SapaNativeWraper.spxl_set_can_transceiver(ref device, can_instance, txvr, mode);
            return true;
        }

        //# 参数:deviceChannel:设备通道,instance:设备实例号,enable:FD开关,brs:波特率开关,arbi_rate:arbi速率,data_rate:数据传输速率
        public bool set_canfd_options(string device_name, int can_instance, byte enable, byte brs, long arbi_rate, long data_rate)
        {
            XL_DEVICE_T device;
            if (opened_device_list.Any(p => p.str_Serial_Num == device_name))
            {
                device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            }
            else
            {
                return false;
            }

            int r = SapaNativeWraper.spxl_set_canfd_options(ref device, can_instance, enable, brs, (Int64)arbi_rate, (Int64)data_rate);
            return true;
        }

        //数据接收函数
        protected void RecvDataFunc(object obj)
        {
            string device_name = obj.ToString();
            XL_DEVICE_T device = opened_device_list.First(p => p.str_Serial_Num == device_name);
            int rx_size = Marshal.SizeOf(typeof(XL_COMMON_FRAME_MESSAGE));

            while (m_bStart)
            {
                IntPtr ptr = Marshal.AllocHGlobal(rx_size);
                uint rCount = 1;
                int r = SapaNativeWraper.spxl_receive(ref device, ptr, ref rCount, 0);
                if (r == 0 && rCount != 0)
                {
                    XL_COMMON_FRAME_MESSAGE recv_msg = (XL_COMMON_FRAME_MESSAGE)Marshal.PtrToStructure((IntPtr)((UInt32)ptr), typeof(XL_COMMON_FRAME_MESSAGE));

                    switch (recv_msg.netProtocol)
                    {
                        case (byte)SPXL_PROTOCOL.SPXL_PROTOCOL_CANFD:
                        case (byte)SPXL_PROTOCOL.SPXL_PROTOCOL_CAN:
                            XL_CAN_FRAME_MESSAGE can_msg = (XL_CAN_FRAME_MESSAGE)Marshal.PtrToStructure((IntPtr)((UInt32)ptr), typeof(XL_CAN_FRAME_MESSAGE));
                            //string timeStr = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                            //Console.WriteLine(string.Format("CAN: {0} -- {1}", timeStr, can_msg.arbid.ToString("X4")));
                            if (OnRecvCanDataEvent != null)
                            {
                                OnRecvCanDataEvent(can_msg);
                            }
                            break;
                        case (byte)SPXL_PROTOCOL.SPXL_PROTOCOL_LIN:
                            XL_LIN_FRAME_MESSAGE lin_msg = (XL_LIN_FRAME_MESSAGE)Marshal.PtrToStructure((IntPtr)((UInt32)ptr), typeof(XL_LIN_FRAME_MESSAGE));
                            //string timeStr2 = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                            //Console.WriteLine(string.Format("LIN: {0} -- {1}", timeStr2, lin_msg.id.ToString("X2")));
                            if (OnRecvLinDataEvent != null)
                            {
                                OnRecvLinDataEvent(lin_msg);
                            }
                            break;
                        case (byte)SPXL_PROTOCOL.SPXL_PROTOCOL_FR:
                            XL_FLEXRAY_FRAME_MESSAGE fr_msg = (XL_FLEXRAY_FRAME_MESSAGE)Marshal.PtrToStructure((IntPtr)((UInt32)ptr), typeof(XL_FLEXRAY_FRAME_MESSAGE));
                            //string timeStr1 = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                            //Console.WriteLine(string.Format("Flexray: {0} -- {1}", timeStr1, fr_msg.slotId));
                            if (OnRecvFlexrayDataEvent != null)
                            {
                                OnRecvFlexrayDataEvent(fr_msg);
                            }
                            break;
                        default:
                            break;
                    }

                }
                Marshal.FreeHGlobal(ptr);
                ptr = IntPtr.Zero;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using CAN_Library;

namespace SBE2ConnectionWrapper
{
    /// <summary>
    /// Wrapper for the SBE2Connection Dll Imported functions
    /// *Uses SBE2Connection.dll*
    /// </summary>
    public partial class SBE2Connection : IDisposable //Partial class to allow plug-in specific code
    {
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private struct ABS_TIMESTAMP
        {
            public uint years;
            public byte months;
            public byte days;
            public byte hours;
            public byte minutes;
            public byte seconds;
            public ushort milliseconds;
        }

        private const string DLL_NAME = "SBE2Connection.dll";
        private const string MIN_SBE2_VERSION = "v1.10.1.0";
        private const string MIN_DLL_VERSION = "1, 0, 0, 5";
        //functions
        internal static class NativeMethods
        {
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2Connect(string Address, Int32 port, string AppName);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2ConnectLocal(string AppName);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2IsConnected();
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2Disconnect();
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2RequestDeviceList(StringBuilder pDevList);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2GetEngineVersion(StringBuilder pEngVer);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2ConnectToDevice(string pDeviceName);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2DisconnectFromDevice(Int32 DeviceID);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2SetDataCallback(InternalDataCallbackEventHandler DataCallback);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2SendDataToDeviceString(Int32 DeviceID, string DataString);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2SendDataToDeviceRawString(Int32 DeviceID, string DataString);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2SendDataToDeviceData(int DeviceID, IntPtr pData, UInt32 Length);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2SendDataToDeviceRawData(int DeviceID, IntPtr pData, UInt32 Length);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2OpenAllDevices();
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2CloseAllDevices();
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2OpenDeviceByName(string Name);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2CloseDeviceByName(string Name);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2OpenDeviceByID(int DeviceID);
            [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            internal static extern Int32 SBE2CloseDeviceByID(int DeviceID);
        }

        private const int MAX_DEVICES = 32;
        private bool disposed;

        // DataCallback Event Variables
        public delegate void InternalDataCallbackEventHandler(int DeviceID, IntPtr pData, UInt32 Length, IntPtr pAbsTime);
        private InternalDataCallbackEventHandler pDataCallback;
        public delegate void DataCallbackEventHandler(int DeviceID, IntPtr pData, UInt32 Length, DateTime absTime);

        // DeviceListChange Event Variables
        public delegate void DeviceListChangeEventHandler(string deviceListStr);

        /// <summary>
        /// Create an SBE2Connection object.
        /// </summary>
        /// <param name="enableDeviceListChangeEvent">Enables the device list change event</param>
        public SBE2Connection()
        {
            // Get path to the SBE2Connection DLL.
#if SIMULATOR
            string dllPath = @".\SBE2Connection.dll";
#else
            string dllPath = @".\Component\SBE2Connection.dll";
#endif

            // Check if the SBE2Connection DLL is installed on the computer.
            if (System.IO.File.Exists(dllPath))
            {
                // Get the version number of the SBE2Connection DLL.
                string version = FileVersionInfo.GetVersionInfo(dllPath).FileVersion;
                // Check the version number.
                if (version.CompareTo(MIN_DLL_VERSION) >= 0)
                {
                    // Create function pointer for use by the DLL.
                    pDataCallback = new InternalDataCallbackEventHandler(DataCallbackDo);
                }
                else
                {
                    Log.Logger_Run("***** SBE2Connection DLL version is too low *****");
                    Log.Logger_Run("Please update '" + DLL_NAME + "'.\n\r- Minimum version: " + MIN_DLL_VERSION + "\n\r- Current version: "
                      + version);
                    Environment.Exit(1);
                }
            }
            else
            {
                Log.Logger_Run("***** SBE2Connection DLL not found *****");
                Log.Logger_Run("File '" + DLL_NAME + "' could not be found. Please install saint software.");
            }
        }

        ~SBE2Connection()
        {
        }

        /// <summary>
        /// Releases all resources used by the SBE2ConnectionWrapper.SBE2Connection
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Get the maximum number of devices.
        /// </summary>
        public int MaxDevices
        {
            get { return MAX_DEVICES; }
        }


        #region DataCallback Event Pitcher
        /// <summary>
        /// Event is thrown each time a connected device gets a new message from the SBE.
        /// </summary>
        public event DataCallbackEventHandler DataCallbackEvent;

        private unsafe void DataCallbackDo(int DeviceID, IntPtr pData, UInt32 Length, IntPtr pAbsTime)
        {
            ABS_TIMESTAMP* pAbsTimestamp = (ABS_TIMESTAMP*)pAbsTime;

            DateTime absTime;

            try
            {
                absTime = new DateTime((int)pAbsTimestamp->years, pAbsTimestamp->months, pAbsTimestamp->days, pAbsTimestamp->hours,
                  pAbsTimestamp->minutes, pAbsTimestamp->seconds).AddMilliseconds(pAbsTimestamp->milliseconds);
            }
            catch (ArgumentOutOfRangeException)
            {
                absTime = DateTime.Now;
            }

            if (DataCallbackEvent != null)
            {
                DataCallbackEvent(DeviceID, pData, Length, absTime);
            }
        }
        #endregion

        private bool tryConnect()
        {
            // Get the version number of the SBE2.
            string version = GetEngineVersion();
            // Check the version number.
            if (version.CompareTo(MIN_SBE2_VERSION) >= 0)
            {
                // Set data callback function pointer.
                NativeMethods.SBE2SetDataCallback(pDataCallback);
                // Start check device list timer if needed.
                //if (CheckDeviceList != null)
                //    CheckDeviceList.Start();

                return true;
            }
            else // Version number is too low.
            {
                Log.Logger_Run("***** Saint Bus Engine 2 version is too low *****");
                // Disconnect from SBE2.
                Disconnect();
                Log.Logger_Run("Please update Saint Bus Engine 2.\n\r- Minimum version: " + MIN_SBE2_VERSION + "\n\r- Current version: "
                  + version);
                return false;
            }
        }

        #region SBEConnection Functions
        /// <summary>
        /// Attempts to connect to the Saint Bus Engine using the given port and IP address. 
        /// Starts the DeviceListChange event timer if the event was enabled in the constructor
        /// </summary>
        /// <param name="address">Can be an IP (XXX.XXX.XXX.XXX), url or computer name</param>
        /// <param name="port">Saint Bus Engine port number</param>
        /// <param name="appName">Name of the calling plug-in</param>
        /// <returns>Returns true when the client successfully connects to the SBE2</returns>
        public bool Connect(string address, Int32 port, string appName)
        {
            bool status = false;
            if (NativeMethods.SBE2Connect(address, port, appName) == 1)
            {
                status = true;
            }
            status = status && tryConnect();
            return status;
            //return ((NativeMethods.SBE2Connect(address, port, appName) == 1) && tryConnect());
        }

        /// <summary>
        /// Attempts to connect to the Saint Bus Engine using the default port (8000) and IP (localhost)
        /// Starts the DeviceListChange event timer if the event was enabled in the constructor
        /// </summary>
        /// <param name="appName">Name of the calling plug-in</param>
        /// <returns>Returns true when the client successfully connects to the SBE2</returns>
        public bool ConnectLocal(string appName)
        {
            return ((NativeMethods.SBE2ConnectLocal(appName) == 1) && tryConnect());
        }

        /// <summary>
        /// Checks if the plug-in is connected to SBE
        /// </summary>
        /// <returns>Returns true when the client is connected to the SBE2</returns>
        public bool IsConnected()
        {
            return (NativeMethods.SBE2IsConnected() == 1);
        }

        /// <summary>
        /// Disconnects the client from the SBE2
        /// </summary>
        public void Disconnect()
        {
            //if (CheckDeviceList != null)
            //{
            //    // Stop check device list timer if needed.
            //    CheckDeviceList.Stop();
            //    deviceListStr = " ";
            //    // Reset timer interval throw event sooner
            //    CheckDeviceList.Interval = 10;
            //    fixTime = true;
            //}
            NativeMethods.SBE2Disconnect();
        }

        /// <summary>
        /// Request open devices from the SBE.
        /// </summary>
        /// <returns>Returns a string of open devices, seperated by commas</returns>
        public string RequestDeviceList()
        {
            StringBuilder tempStr = new StringBuilder(512);
            NativeMethods.SBE2RequestDeviceList(tempStr);
            return tempStr.ToString().Trim(','); //Trims the last comma off the device list
        }

        /// <summary>
        /// Request the Saint Bus Engine version
        /// </summary>
        /// <returns>The Saint Bus Engine version number</returns>
        public string GetEngineVersion()
        {
            StringBuilder tempStr = new StringBuilder(512);
            NativeMethods.SBE2GetEngineVersion(tempStr);
            return tempStr.ToString();
        }

        /// <summary>
        /// Connects to the given device name
        /// </summary>
        /// <param name="deviceName">Device Name</param>
        /// <returns>Returns the device ID if successful, 0xFF if a failure has occurred</returns>
        public int ConnectToDevice(string deviceName)
        {
            return (NativeMethods.SBE2ConnectToDevice(deviceName));
        }

        /// <summary>
        /// Disconnects from the given device ID
        /// </summary>
        /// <param name="deviceId">Device ID to disconnect from</param>
        public void DisconnectFromDevice(Int32 deviceId)
        {
            NativeMethods.SBE2DisconnectFromDevice(deviceId);
        }

        /// <summary>
        /// Sends data to the given device, but converts the hex string to binary. This function packs 0xFFs.
        /// </summary>
        /// <param name="deviceId">Device to send data to</param>
        /// <param name="data">Data to send</param>
        public bool SendDataToDevice(Int32 deviceId, string data)
        {
            int ii = NativeMethods.SBE2SendDataToDeviceString(deviceId, data);
            if (ii == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sends binary data to the given device. This function packs 0xFFs.
        /// </summary>
        /// <param name="deviceId">Device to send data to</param>
        /// <param name="pData">Pointer to data</param>
        /// <param name="length">Length of data</param>
        public bool SendDataToDevice(int deviceId, IntPtr pData, UInt32 length)
        {
            int ii = NativeMethods.SBE2SendDataToDeviceData(deviceId, pData, length);
            if (ii == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sends data to the given device, but converts the hex string to binary. This function does not pack 0xFFs.
        /// </summary>
        /// <param name="deviceId">Device to send data to</param>
        /// <param name="data">Data to send</param>
        public void SendDataToDeviceRaw(Int32 deviceId, string data)
        {
            NativeMethods.SBE2SendDataToDeviceRawString(deviceId, data);
        }

        /// <summary>
        /// Sends binary data to the given device. This function does not pack 0xFFs.
        /// </summary>
        /// <param name="deviceId">Device to send data to</param>
        /// <param name="pData">Pointer to data</param>
        /// <param name="length">Length of data</param>
        public void SendDataToDeviceRaw(int deviceId, IntPtr pData, UInt32 length)
        {
            NativeMethods.SBE2SendDataToDeviceRawData(deviceId, pData, length);
        }

        /// <summary>
        /// Opens all enumerated devices connected to the SBE
        /// </summary>
        public void OpenAllDevices()
        {
            NativeMethods.SBE2OpenAllDevices();
        }

        /// <summary>
        /// Closes all enumerated devices
        /// </summary>
        public void CloseAllDevices()
        {
            NativeMethods.SBE2CloseAllDevices();
        }

        /// <summary>
        /// Opens the device with the given name.
        /// </summary>
        /// <param name="name">Name of device to open</param>
        /// <returns>Returns true if device was opened, false otherwise.</returns>
        public bool OpenDeviceByName(string name)
        {
            return (NativeMethods.SBE2OpenDeviceByName(name) == 1);
        }

        /// <summary>
        /// Closes the device with the given name
        /// </summary>
        /// <param name="name">Name of device to close</param>
        public void CloseDeviceByName(string name)
        {
            NativeMethods.SBE2CloseDeviceByName(name);
        }

        /// <summary>
        /// Opens the device with the given device ID
        /// </summary>
        /// <param name="deviceId">Device ID to open</param>
        /// <returns>Returns true if device was opened, false otherwise</returns>
        public bool OpenDeviceById(int deviceId)
        {
            return (NativeMethods.SBE2OpenDeviceByID(deviceId) == 1);
        }

        /// <summary>
        /// Closes the device with the given device ID
        /// </summary>
        /// <param name="deviceId">Device ID to close</param>
        public void CloseDeviceById(int deviceId)
        {
            NativeMethods.SBE2CloseDeviceByID(deviceId);
        }
        #endregion
    }
}
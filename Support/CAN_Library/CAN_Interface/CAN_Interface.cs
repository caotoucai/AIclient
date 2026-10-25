using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CAN_Library.CAN_Driver;
using System.IO;
using System.Threading;

namespace CAN_Library.CAN_Interface
{
    public class CAN_Interface : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        //public delegate void ExceptionCallback(uint errorCode, string Description);
        //public event ExceptionCallback ExceptionEvent;

        //public delegate void Fast_MessageCallback(CAN_Message Msg);
        //public static event Fast_MessageCallback ChannelMessageNotify;

        ObservableCollection<CAN_Channel> _CAN_Channel_List = new ObservableCollection<CAN_Channel>();
        public ObservableCollection<CAN_Channel> CAN_Channel_List
        {
            get { return _CAN_Channel_List; }
            set { _CAN_Channel_List = value; OnPropertyChanged("CAN_Channel_List"); }
        }

        public CAN_Interface()
        {
            if (false == Directory.Exists(Log._RunlogFolderPath))
            {
                Directory.CreateDirectory(Log._RunlogFolderPath);
            }

            if (Directory.Exists(Log._RunlogFolderPath))
            {
                Log._RunlogFilePath = string.Format(Log._RunlogFolderPath + @"\{0}.log", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            }
            else  //in case fail to create _RunlogFolderPath folder
            {
                Log._RunlogFilePath = string.Format(Log._RunlogFolderPath + @"-{0}.log", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            }
        }

        public void Dispose()
        {
            try
            {
                foreach (var channel in CAN_Manager.Available_Channel_List)
                {
                    if (channel.IsConnected)
                    {
                        channel.DisConnect();
                    }
                }

                CAN_Manager.SaintDispose();
                CAN_Channel_List.Clear();
                CAN_Manager.ClearList();
            }
            catch (Exception ex)
            {
                Log.Logger_Run("2.2.1-Dispose() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        public ObservableCollection<CAN_Channel> Scan_Channel_List()
        {
            Log.Logger_Run("\r\nfunction: Scan_Channel_List");
            CAN_Manager.Update_Spy3_Device_List();
            CAN_Manager.Update_Vector_Devcie_List();
            CAN_Manager.Update_Saint_Device_List();
            CAN_Manager.Update_ZLG_Device_List();
            CAN_Manager.Update_Sapa_Device_List();
            CAN_Manager.Update_CANChannel();
            CAN_Channel_List = CAN_Manager.Available_Channel_List;
            return CAN_Manager.Available_Channel_List;
        }
    }
}

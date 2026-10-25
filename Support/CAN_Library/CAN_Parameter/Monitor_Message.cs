using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;

namespace CAN_Library.CAN_Parameter
{
    public class Monitor_Message : IMonitor_Message
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

        private uint _ID = 0x0;
        public uint ID
        {
            get { return _ID; }
            set
            {
                if (_ID != value)
                {
                    _ID = value;
                }
                OnPropertyChanged("ID");
            }
        }

        private string _ECU = "";
        public string ECU
        {
            get { return _ECU; }
            set
            {
                if (_ECU != value)
                {
                    _ECU = value;
                }
                OnPropertyChanged("ECU");
            }
        }

        private string _ChannelName = "";
        public string ChannelName
        {
            get { return _ChannelName; }
            set
            {
                if (_ChannelName != value)
                {
                    _ChannelName = value;
                }
                OnPropertyChanged("ChannelName");
            }
        }



        private string _Name = "";
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                }
                OnPropertyChanged("Name");
            }
        }

        private uint _DLC = 0;
        public uint DLC
        {
            get { return _DLC; }
            set
            {
                if (_DLC != value)
                {
                    _DLC = value;
                }
                OnPropertyChanged("DLC");
            }
        }

        private string _sData = "";
        public string sData
        {
            get
            {
                return _sData;
            }
            set
            {
                if (_sData != value)
                {
                    _sData = value;
                }
                OnPropertyChanged("sData");
            }
        }

        private byte[] _Data = {0};
        public byte[] Data
        {
            get { return _Data; }
            set
            {
                if (_Data != value)
                {
                    _Data = value;
                }
            }
        }
               
        private ObservableCollection<CAN_Signal> _Signals = new ObservableCollection<CAN_Signal>();
        public ObservableCollection<CAN_Signal> Signals
        {
            get { return _Signals; }
            set
            {
                if (_Signals != value)
                {
                    _Signals = value;
                }
                OnPropertyChanged("Signals");
            }
        }

        private long _TimeSpan = 0;
        public long TimeSpan
        {
            get { return _TimeSpan; }
            set
            {
                if (_TimeSpan != value)
                {
                    _TimeSpan = value;
                    OnPropertyChanged("TimeSpan");
                }
            }
        }

        private long _LastSendTimeMs = 0;  // for Publisher
        public long LastSendTimeMs
        {
            set
            {
                _LastSendTimeMs = value;
                OnPropertyChanged("LastSendTimeMs");
            }
            get
            {
                return _LastSendTimeMs;
            }
        }

        private long _LastReceivedTimeMs = 0;
        public long LastReceivedTimeMs
        {
            set
            {
                _LastReceivedTimeMs = value;
                OnPropertyChanged("LastReceivedTimeMs");
            }
            get
            {
                return _LastReceivedTimeMs;
            }
        }

        private uint _CycleTime = 100;
        public uint CycleTime
        {
            set
            {
                _CycleTime = value;
                OnPropertyChanged("CycleTime");
            }
            get
            {
                return _CycleTime;
            }
        }

        TxRxType _TxRx;
        public TxRxType TxRx
        {
            get { return _TxRx; }
            set
            {
                if (_TxRx != value)
                {
                    _TxRx = value;
                    OnPropertyChanged("TxRx");
                }
            }
        }

        private string _Error = "";
        public string Error
        {
            set
            {
                _Error = value;
                OnPropertyChanged("Error");
            }
            get
            {
                return _Error;
            }
        }

        private long _RevCount = 0;
        public long RevCount
        {
            set
            {
                _RevCount = value;
                OnPropertyChanged("RevCount");
            }
            get
            {
                return _RevCount;
            }
        }
    }
}

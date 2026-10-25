using CAN_Library.CAN_Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CAN_Library.CAN_Parameter
{
    public class Eth_Message
    {  
        private L4_Protocol _Protocol = L4_Protocol.UDP;
        public L4_Protocol Protocol
        {
            set
            {
                _Protocol = value;
            }
            get { return _Protocol; }
        }

        private Transmitter _Transmitter = Transmitter.Server;
        public Transmitter Transmitter
        {
            set
            {
                _Transmitter = value;
            }
            get
            {
                return _Transmitter;
            }
        }

        private IPEndPoint _SourceEndPoint = new IPEndPoint(IPAddress.Any, 0);
        public IPEndPoint SourceEndPoint
        {
            set
            {
                _SourceEndPoint = value;
            }
            get
            {
                return _SourceEndPoint;
            }
        }

        private IPEndPoint _DestEndPoint = new IPEndPoint(IPAddress.Any, 0);
        public IPEndPoint DestEndPoint
        {
            set
            {
                _DestEndPoint = value;
            }
            get
            {
                return _DestEndPoint;
            }
        }        

        private long _TimestampMicroS = 0;
        public long TimestampMicroS
        {
            set
            {
                _TimestampMicroS = value;
            }
            get
            {
                return _TimestampMicroS;
            }
        }

        private string _sPayload = "";//TCP or UDP payload
        public string sPayload
        {
            get
            {
                if (_Payload == null)
                    return "no data";
                else
                    return BitConverter.ToString(_Payload, 0).Replace("-", " ").ToUpper();
            }
            set
            {
                _sPayload = value;
            }
        }

        private byte[] _Payload;//TCP or UDP payload
        public byte[] Payload
        {
            set
            {
                _Payload = value;                
            }
            get
            {
                return _Payload;
            }
        }

        public Eth_Message(byte[] payload, IPEndPoint sourceEndPoint, IPEndPoint destEndPoint, L4_Protocol protocol = L4_Protocol.TCP)
        {
            this.Payload = payload;
            this.SourceEndPoint = sourceEndPoint;
            this.DestEndPoint = destEndPoint;
        }        
    }


    public class DoipMessage
    {
        // doip protocol version
        private byte _protocol_version = 0x02;
        public byte Protocol_Version
        {
            set
            {
                _protocol_version = value;
            }
            get
            {
                return _protocol_version;
            }
        }


        // doip protocol inverse version
        private byte _protocol_version_inv = 0xfd;
        public byte Protocol_Version_INV
        {
            set
            {
                _protocol_version_inv = value;
            }
            get
            {
                return _protocol_version_inv;
            }
        }

        // doip payload type
        private UInt16 _payload_type = 0x8001;
        public UInt16 Payload_Type
        {
            set
            {
                _payload_type = value;
            }
            get
            {
                return _payload_type;
            }
        }

        // doip payload length, including two bytes SourceLA and two bytes TargetLA
        private UInt32 _payload_length = 0;
        public UInt32 Payload_Length
        {
            set
            {
                _payload_length = value;
            }
            get
            {
                return _payload_length;
            }
        }

        // doip payload Source LA
        private UInt16 _sourceLA = 0;
        public UInt16 SourceLA
        {
            set
            {
                _sourceLA = value;
            }
            get
            {
                return _sourceLA;
            }
        }

        // doip payload Target LA
        private UInt16 _targetLA = 0;
        public UInt16 TargetLA
        {
            set
            {
                _targetLA = value;
            }
            get
            {
                return _targetLA;
            }
        }

        private byte[] _Payload = { 0 };
        public byte[] Payload
        {
            set
            {
                _Payload = value;
                //_data = new byte[12 + Payload.Length];
                //_data[0] = _protocol_version;
                //_data[1] = _protocol_version_inv;
                //_data[2] = (byte)(_payload_type >> 8 & 0xff);
                //_data[3] = (byte)(_payload_type & 0xff);
                //_data[4] = (byte)(_payload_length >> 24 & 0xff);
                //_data[5] = (byte)(_payload_length >> 16 & 0xff);
                //_data[6] = (byte)(_payload_length >> 8 & 0xff);
                //_data[7] = (byte)(_payload_length & 0xff);
                //_data[8] = (byte)(_sourceLA >> 8 & 0xff);
                //_data[9] = (byte)(_sourceLA & 0xff);
                //_data[10] = (byte)(_targetLA >> 8 & 0xff);
                //_data[11] = (byte)(_targetLA & 0xff);

                //for (int i = 0; i < _payload_length; i++)
                //    _data[12 + i] = _Payload[i];
            }
            get
            {
                return _Payload;
            }
        }

        private byte[] _data = { 0 };
        public byte[] Data
        {
            get
            {
                try
                {
                    //02 fd 80 01 00 00 00 06 0e 00 12 01 10 03
                    _data = new byte[8 + _payload_length];
                    _data[0] = _protocol_version;
                    _data[1] = _protocol_version_inv;
                    _data[2] = (byte)(_payload_type >> 8 & 0xff);
                    _data[3] = (byte)(_payload_type & 0xff);

                    _data[4] = (byte)(_payload_length >> 24 & 0xff);
                    _data[5] = (byte)(_payload_length >> 16 & 0xff);
                    _data[6] = (byte)(_payload_length >> 8 & 0xff);
                    _data[7] = (byte)(_payload_length & 0xff);

                    _data[8] = (byte)(_sourceLA >> 8 & 0xff);
                    _data[9] = (byte)(_sourceLA & 0xff);

                    _data[10] = (byte)(_targetLA >> 8 & 0xff);
                    _data[11] = (byte)(_targetLA & 0xff);

                    for (int i = 0; i < _payload_length - 4; i++)
                        _data[12 + i] = _Payload[i];
                }
                catch (Exception ex)
                {

                }
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        public DoipMessage(byte[] payload, UInt16 sourceLA, UInt16 targetLA)
        {
            Payload = payload;
            Payload_Length = (UInt32)(payload.Length + 4);
            SourceLA = sourceLA;
            TargetLA = targetLA;
        }
    }

    public enum L4_Protocol
    {
        ICMP= 0x01,
        TCP = 0x06,
        UDP = 0x11,
        Others = 0xFF,
    }

    public enum Transmitter
    {
        Server = 0,
        Client = 1,
    }

}

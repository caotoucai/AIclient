using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CAN_Library.CAN_Parameter;

namespace CAN_Library.CAN_Interface
{
    public class Ether_Channel
    {
        //message tx/rx event
        public delegate void EthMessageCallback(Eth_Message Msg);
        public event EthMessageCallback ChannelMessageNotify;

        #region TCP
        private Conn conn = null;  //Only one now
        /// <summary>
        /// 将Socket定义为全局变量
        /// </summary>
        private Socket serverSocket = null;
        private bool isStartTCPServer = false;
        public bool startTCPServer(IPEndPoint ipServerPoint)
        {
            if (isStartTCPServer)
            {
                Log.Logger_Run("TCPServer instance already start.");
                return false;
            }

            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(ipServerPoint);//绑定IP和端口号
                serverSocket.Listen(10);//开始监听端口,0为监听无限个客户端
                isStartTCPServer = true;
                Log.Logger_Run("TCPServer Start listenning ...");
                //开始调用异步连接
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger_Run(string.Format("[exception] startTCPServer()\n{0}\n{1}", ex.Message, ex.StackTrace));
                return false;
            }
        }

        public void stopTCPServer()
        {
            isStartTCPServer = false;
            Thread.Sleep(500);
            try
            {
                if (serverSocket != null)
                {
                    serverSocket.Close();
                    serverSocket = null;
                }

                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }
            catch (Exception ex)
            {
                Log.Logger_Run(string.Format("[exception] stopTCPServer()\n{0}\n{1}", ex.Message, ex.StackTrace));
            }
        }

        /// <summary>
        /// Accept回调
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallBack(IAsyncResult ar)
        {
            if (serverSocket == null)
            {
                Log.Logger_Run("AcceptCallBack: Opps! serverSocket already closed.");
                return;
            }

            try
            {
                Socket socket = serverSocket.EndAccept(ar);//尝试进行异步连接
                IPEndPoint SourceEndPoint = socket.RemoteEndPoint as IPEndPoint;
                //if (SourceEndPoint.Address.ToString() != "198.18.34.1")
                //{
                //    Log.Logger_Run("Not the expected one.");
                //    socket.Close();
                //}
                //else
                //{
                    conn = new Conn();
                    conn.Init(socket);
                    string adr = conn.GetAddress();
                    Log.Logger_Run("Client Connected: [" + adr + "]");
                    //conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, new AsyncCallback(ReceiveCb), conn);
                    conn.socket.BeginReceive(conn.readBuff, 0, 1024, SocketFlags.None, new AsyncCallback(ReceiveCallBack), conn);
                //}
                serverSocket.BeginAccept(AcceptCallBack, null);
            }
            catch (Exception ex)
            {
                Log.Logger_Run(string.Format("[exception] AcceptCallBack()\n{0}\n{1}", ex.Message, ex.StackTrace));
            }
        }
        /// <summary>
        /// 接收回调
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            Conn conn = (Conn)ar.AsyncState;
            if (conn.socket == null)
            {
                Log.Logger_Run("ReceiveCallBack: Opps! client already disconnect");
                return;
            }

            try
            {
                int count = conn.socket.EndReceive(ar);
                //关闭信号
                if (count <= 0)
                {
                    Log.Logger_Run("ReceiveCallBack: maybe already disconnect");
                    conn.Close();
                    return;
                }
                //数据处理

                byte[] data = conn.readBuff.Take(count).ToArray();
                //Console.WriteLine($"TCP Message received: {conn.GetAddress()}, Data: {BitConverter.ToString(data, 0, count).Replace("-", " ").ToUpper()}");
                if (ChannelMessageNotify != null && count != 0)
                {
                    Eth_Message ether_Message = new Eth_Message(data, null, null, L4_Protocol.TCP);
                    ether_Message.SourceEndPoint = conn.socket.RemoteEndPoint as IPEndPoint;
                    ether_Message.DestEndPoint = serverSocket.LocalEndPoint as IPEndPoint;
                    ether_Message.TimestampMicroS = (long)Convert.ToUInt64(DateTime.Now.Ticks / 10000L);
                    ChannelMessageNotify(ether_Message);
                }

                //继续接收
                //conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, new AsyncCallback(ReceiveCb), conn);
                conn.socket.BeginReceive(conn.readBuff, 0, 1024, SocketFlags.None, new AsyncCallback(ReceiveCallBack), conn);

            }
            catch (Exception ex)
            {
                Log.Logger_Run(string.Format("[exception] ReceiveCallBack()\n{0}\n{1}", ex.Message, ex.StackTrace));
                conn.Close();
            }
        }

        public void sendMessage(byte[] data)
        {
            if (conn == null)
                return;

            conn.socket.Send(data);
        }

        public void sendTCP_message(byte[] sendData, EndPoint endPoint)
        {
            try
            {
                if (conn == null)
                    return;
                conn.socket.SendTo(sendData, sendData.Length, SocketFlags.None, endPoint);
            }
            catch (Exception Exception)      // For catching Exception throwed from .BeginReceive()
            {
                Log.Logger_Run(string.Format("[exception] sendTCP_message()\n{0}\n{1}", Exception.Message, Exception.StackTrace));
            }
        }

        public void sendDOIPMessage(byte[] data, UInt16 sourceLA=0x0E00, UInt16 targetLA=0x1201)
        {
            if (conn == null)
                return;
            DoipMessage msg = new DoipMessage(data, sourceLA, targetLA);
            conn.socket.Send(msg.Data);
        }
        #endregion

        #region UDP
        public static string targetServiceID { get; set; } = "1001";
        private Socket udpServer_Multi;
        private Socket udpServer;
        public IPEndPoint MultiUDPPort;
        private bool isStartUDPServer = false;
        public void StartUDPServer(IPEndPoint source_endPoint)
        {
            try
            {
                isStartUDPServer = true;
                //1.Socket creat          
                udpServer = new Socket
                    (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                udpServer_Multi = new Socket
                   (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //2.Bind ip and port监听端口
                udpServer.Bind(source_endPoint);
                udpServer_Multi.Bind(new IPEndPoint(source_endPoint.Address, MultiUDPPort.Port));

                //3.receive data
                new Thread(ReceiveUDPMessage)
                {
                    IsBackground = true
                }.Start();

                new Thread(ReceiveMultiUDPMessage)
                {
                    IsBackground = true
                }.Start();
            }
            catch (Exception Exception)      // For catching Exception throwed from .BeginReceive()
            {
                Log.Logger_Run(string.Format("[exception] StartUDPServer()\n{0}\n{1}", Exception.Message, Exception.StackTrace));
            }
            
        }

        public void StopUDPServer()
        {
            try
            {
                isStartUDPServer = false;

                Thread.Sleep(500);
                if (udpServer != null)
                    udpServer.Close();
                if (udpServer_Multi != null)
                    udpServer_Multi.Close();
            }
            catch (Exception Exception)      // For catching Exception throwed from .BeginReceive()
            {
                Log.Logger_Run(string.Format("[exception] StartUDPServer()\n{0}\n{1}", Exception.Message, Exception.StackTrace));
            }           
        }        

        public void sendUDP_message(byte[] sendData, EndPoint endPoint)//server
        {
            try
            {
                IPEndPoint IPendpoint = endPoint as IPEndPoint;
                if (IPendpoint.Port == MultiUDPPort.Port)
                {
                    if (udpServer_Multi != null)
                        udpServer_Multi.SendTo(sendData, sendData.Length, SocketFlags.None, endPoint);
                }
                else
                {
                    if (udpServer != null)
                        udpServer.SendTo(sendData, sendData.Length, SocketFlags.None, endPoint);
                }
            }
            catch (Exception Exception)      // For catching Exception throwed from .BeginReceive()
            {
                Log.Logger_Run(string.Format("[exception] StartUDPServer()\n{0}\n{1}", Exception.Message, Exception.StackTrace));
            }
        }

        public void ReceiveUDPMessage()
        {
            try
            {
                while (isStartUDPServer)
                {
                    byte[] data = new byte[1460];
                    EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0); ;
                    int length = udpServer.ReceiveFrom(data, ref endPoint);
                    data = data.Take(length).ToArray();
                    if (ChannelMessageNotify != null && length != 0)
                    {
                        Eth_Message ether_Message = new Eth_Message(data, null, null, L4_Protocol.UDP);
                        ether_Message.SourceEndPoint = endPoint as IPEndPoint;
                        ether_Message.DestEndPoint = udpServer.LocalEndPoint as IPEndPoint;
                        ChannelMessageNotify(ether_Message);
                    }

                }
            }
            catch (Exception Exception)
            {
                Log.Logger_Run(string.Format("[exception] StartUDPServer()\n{0}\n{1}", Exception.Message, Exception.StackTrace));
            }


        }
        public void ReceiveMultiUDPMessage()//多播的socket receive
        {
            try
            {
                while (isStartUDPServer)
                {
                    byte[] data = new byte[1460];
                    EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0); ;
                    int length = udpServer_Multi.ReceiveFrom(data, ref endPoint);//此方法把数据来源ip、port放到第二个参数中
                                                                                 //string message = Encoding.UTF8.GetString(data, 0, length);
                    data = data.Take(length).ToArray();
                    if (ChannelMessageNotify != null && data.Length != 0)
                    {
                        Eth_Message ether_Message = new Eth_Message(data, null, null, L4_Protocol.UDP);
                        ether_Message.SourceEndPoint = endPoint as IPEndPoint;
                        ether_Message.DestEndPoint = udpServer_Multi.LocalEndPoint as IPEndPoint;
                        ChannelMessageNotify(ether_Message);
                    }
                }
            }
            catch (Exception Exception)
            {
                Log.Logger_Run(string.Format("[exception] StartUDPServer()\n{0}\n{1}", Exception.Message, Exception.StackTrace));
            }
        }
        #endregion

        public void send_message(Eth_Message ethe_msg)//server
        {
            try
            {
                if (ethe_msg.Protocol == L4_Protocol.UDP)
                    sendUDP_message(ethe_msg.Payload, ethe_msg.DestEndPoint);
                else if (ethe_msg.Protocol == L4_Protocol.TCP)
                    sendTCP_message(ethe_msg.Payload, ethe_msg.DestEndPoint);
            }
            catch (Exception Exception)      // For catching Exception throwed from .BeginReceive()
            {
                Log.Logger_Run(string.Format("[exception] StartUDPServer()\n{0}\n{1}", Exception.Message, Exception.StackTrace));
            }
        }
    }



    public class Conn
    {
        //定义数据最大长度
        public const int MAX_LENGTH = 1024;
        //Socket
        public Socket socket = null;

        //Buff
        public byte[] readBuff = new byte[MAX_LENGTH];

        public int buffCount = 0;
        //构造函数
        public Conn()
        {
            readBuff = new byte[MAX_LENGTH];
        }

        ////缓冲区剩余的字节数
        //public int BuffRemain()
        //{
        //    return MAX_LENGTH - buffCount;
        //}

        //初始化
        public void Init(Socket socket)
        {
            this.socket = socket;
            buffCount = 0;
        }

        //获取客户端地址
        public string GetAddress()
        {
            if (socket == null)
                return "Fail to get Address";
            return socket.RemoteEndPoint.ToString();
        }

        //关闭
        public void Close()
        {
            if (socket == null)
                return;
            Console.WriteLine("disconnected:" + GetAddress());
            socket.Close();
            socket = null;
        }
    }


}

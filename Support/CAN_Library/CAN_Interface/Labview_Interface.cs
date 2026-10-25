using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CAN_Library.CAN_Driver;
using CAN_Library.CAN_Parameter;

namespace CAN_Library.CAN_Interface
{
    public class LabView_Interface : IDisposable
    {
        public LabView_Interface()
        {
            if (false == Directory.Exists(Log._RunlogFolderPath))
            {
                Directory.CreateDirectory(Log._RunlogFolderPath);
            }
            if (Directory.Exists(Log._RunlogFolderPath))
            {
                Log._RunlogFilePath = string.Format(Log._RunlogFolderPath + @"\{0}.log", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            }
            else
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
                Log.Logger_Run("2.4.1-Dispose() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        public void CANLib_Dispose()
        {
            Dispose();
        }

        public delegate void msgWatch(string channelName, UInt32 MsgID, string MsgData);
        public event msgWatch CANMessageWatch_ForLabview;

        List<CAN_Channel> CAN_Channel_List = new List<CAN_Channel>();

        public string Scan_Channnels()
        {
            Log.Logger_Run("\r\nfunction: Scan_Channnels");

            CAN_Manager.Update_Spy3_Device_List();
            CAN_Manager.Update_Vector_Devcie_List();
            CAN_Manager.Update_Saint_Device_List();
            CAN_Manager.Update_CANChannel();
            CAN_Channel_List = CAN_Manager.Available_Channel_List.ToList();

            string channels = "";
            if (CAN_Channel_List.Count > 0)
            {
                foreach (var channel in CAN_Channel_List)
                {
                    channels = channels + channel.FullName + ",";
                }
            }
            Log.Logger_Run(channels);
            return channels;
        }

        public bool If_FDCAN_Supported(string channelName)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).CAN_Parameter.IsFDSupport;
            }
            else
                return false;
        }

        public bool Connect_CAN_Mode(string channelName, string BaudRate)
        {
            Log.Logger_Run("\r\nfunction: Connect_CAN_Mode");
            if (channelName.IndexOf("LIN") >= 0)
            {
                Log.Logger_Run("this channel is not CAN channel");
                return false;
            }
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {

                CAN_Channel_List.First(p => p.FullName == channelName).CAN_Parameter.BusProtocolMode = CANBus_ProtocolMode.CAN;
                CAN_Channel_List.First(p => p.FullName == channelName).CAN_Parameter.BaudRate = ConvertBaudRate(BaudRate);
                if (CAN_Channel_List.First(p => p.FullName == channelName).Connect())
                {
                    CAN_Channel_List.First(p => p.FullName == channelName).ChannelMessageNotify += OnChannelMessageNotify_EventHandler;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public bool Connect_FDCAN_Mode(string channelName, string arbitration_rate, string data_rate)
        {
            if (channelName.IndexOf("LIN") >= 0)
            {
                return false;
            }
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).CAN_Parameter.BusProtocolMode = CANBus_ProtocolMode.ISO_CAN_FD;
                CAN_Channel_List.First(p => p.FullName == channelName).CAN_Parameter.Arbitration_rate = ConvertBaudRate(arbitration_rate);
                CAN_Channel_List.First(p => p.FullName == channelName).CAN_Parameter.Data_rate = ConvertBaudRate(data_rate);
                if (CAN_Channel_List.First(p => p.FullName == channelName).Connect())
                {
                    CAN_Channel_List.First(p => p.FullName == channelName).ChannelMessageNotify += OnChannelMessageNotify_EventHandler;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public bool Connect_LIN_Mode(string channelName, string BaudRate, string LIN_Version)
        {
            Log.Logger_Run("\r\nfunction: Connect_LIN_Mode");
            Log.Logger_Run("channel Name: " + channelName);
            Log.Logger_Run("Baud Rate: " + BaudRate);
            Log.Logger_Run("LIN Version: " + LIN_Version);

            if (channelName.IndexOf("LIN") >= 0)
            {
                if (CAN_Channel_List.Any(p => p.FullName == channelName))
                {
                    CAN_Channel_List.First(p => p.FullName == channelName).LIN_Parameter.Baudrate = ConvertLINBaudRate(BaudRate);
                    CAN_Channel_List.First(p => p.FullName == channelName).LIN_Parameter.LINversion = ConvertLINversion(LIN_Version);
                    if (CAN_Channel_List.First(p => p.FullName == channelName).Connect())
                    {
                        Log.Logger_Run("LIN channel connect ok!");
                        return true;
                    }
                    else
                    {
                        Log.Logger_Run("LIN channel connect fail!");
                        return false;
                    }
                }
                else
                {
                    Log.Logger_Run("this channel can not find in device");
                    return false;
                }
            }
            else
            {
                Log.Logger_Run("this channel is not LIN channel");
                return false;
            }
        }

        public bool DisConnect(string channelName)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).DisConnect();
            }
            else
                return false;
        }

        public bool LIN_Set_Slaver_Mode(string channelName)
        {
            Log.Logger_Run("\r\nfunction: LIN_Set_Slaver_Mode");
            Log.Logger_Run("channel Name: " + channelName);
            if (channelName.IndexOf("LIN") >= 0)
            {
                if (CAN_Channel_List.Any(p => p.FullName == channelName))
                {
                    if (CAN_Channel_List.First(p => p.FullName == channelName).LIN_Set_Slaver_Mode())
                    {
                        Log.Logger_Run("LIN channel Set Slaver Mode ok!");
                        return true;
                    }
                    else
                    {
                        Log.Logger_Run("LIN channel Set Slaver Mode fail!");
                        return false;
                    }
                }
                else
                {
                    Log.Logger_Run("this channel can not find in device");
                    return false;
                }
            }
            else
            {
                Log.Logger_Run("this channel is not LIN channel");
                return false;
            }
        }

        public bool LIN_Set_Master_Mode(string channelName)
        {
            Log.Logger_Run("\r\nfunction: LIN_Set_Master_Mode");
            Log.Logger_Run("channel Name: " + channelName);

            if (channelName.IndexOf("LIN") >= 0)
            {
                if (CAN_Channel_List.Any(p => p.FullName == channelName))
                {
                    if (CAN_Channel_List.First(p => p.FullName == channelName).LIN_Set_Master_Mode())
                    {
                        Log.Logger_Run("LIN channel Set Master Mode ok!");
                        return true;
                    }
                    else
                    {
                        Log.Logger_Run("LIN channel Set Master Mode fail!");
                        return false;
                    }
                }
                else
                {
                    Log.Logger_Run("this channel can not find in device");
                    return false;
                }
            }
            else
            {
                Log.Logger_Run("this channel is not LIN channel");
                return false;
            }
        }

        public bool LIN_Set_AutoChecksum(string channelName)
        {
            Log.Logger_Run("\r\nfunction: LIN_Set_AutoChecksum");
            Log.Logger_Run("channel Name: " + channelName);

            if (channelName.IndexOf("LIN") >= 0)
            {
                if (CAN_Channel_List.Any(p => p.FullName == channelName))
                {
                    if (CAN_Channel_List.First(p => p.FullName == channelName).LIN_Set_AutoChecksum())
                    {
                        Log.Logger_Run("LIN channel Set AutoChecksum ok!");
                        return true;
                    }
                    else
                    {
                        Log.Logger_Run("LIN channel Set AutoChecksum fail!");
                        return false;
                    }
                }
                else
                {
                    Log.Logger_Run("this channel can not find in device");
                    return false;
                }
            }
            else
            {
                Log.Logger_Run("this channel is not LIN channel");
                return false;
            }
        }

        public bool LIN_Not_AutoChecksum(string channelName)
        {
            Log.Logger_Run("\r\nfunction: LIN_Set_AutoChecksum");
            Log.Logger_Run("channel Name: " + channelName);

            if (channelName.IndexOf("LIN") >= 0)
            {
                if (CAN_Channel_List.Any(p => p.FullName == channelName))
                {
                    if (CAN_Channel_List.First(p => p.FullName == channelName).LIN_Set_Not_AutoChecksum())
                    {
                        Log.Logger_Run("LIN channel Set not AutoChecksum mode ok!");
                        return true;
                    }
                    else
                    {
                        Log.Logger_Run("LIN channel Set not AutoChecksum mode fail!");
                        return false;
                    }
                }
                else
                {
                    Log.Logger_Run("this channel can not find in device");
                    return false;
                }
            }
            else
            {
                Log.Logger_Run("this channel is not LIN channel");
                return false;
            }
        }

        public bool Send_LINmessage(string channelName, string LIN_Message)
        {
            if (channelName.IndexOf("LIN") < 0)
                return false;
            if (LIN_Message == null || LIN_Message == "" || (LIN_Message.Replace(" ", "").Length % 2) != 0)
                return false;
            string linmsg = LIN_Message.Replace(" ", "");
            int stringlength = linmsg.Length;
            int bytelength = stringlength / 2;
            byte[] mdata = StringToByte(linmsg, (uint)bytelength);
            uint PID = mdata[0];
            byte dlc = 0;
            if (bytelength - 1 > 0)
            {
                dlc = (byte)(bytelength - 1);
            }
            LIN_Message Message = new LIN_Message();
            if (dlc > 0)
            {
                byte[] data = new byte[dlc];
                for (int i = 0; i < dlc; i++)
                {
                    data[i] = mdata[i + 1];
                }
                Message.Data = data;
            }
            else
                Message.Data = null;
            Message.FrameID = PID;
            Message.DLC = dlc;
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).Send_LIN_Message(Message);
                return true;
            }
            else
                return false;
        }

        public void Clear_SlaverPID(string channelName, byte PID)
        {
            if (channelName.IndexOf("LIN") < 0)
                return;
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).Clear_LIN_PID(PID);
            }
        }


        public bool ReSetBaudrate_CAN_Mode(string channelName, string BaudRate)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).ReSetBaudrate(ConvertBaudRate(BaudRate));
            }
            else
                return false;
        }

        public bool ReSetBaudrate_FDCAN_Mode(string channelName, string arbitration_rate, string data_rate)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).ReSetFDBaudrate(ConvertBaudRate(arbitration_rate), ConvertBaudRate(data_rate));
            }
            else
                return false;
        }

        public bool Import_DBC(string channelName, string DBC_Path)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).Import_DBC(DBC_Path);
            }
            else
                return false;
        }

        public bool Delete_DBC(string channelName)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).Delete_DBC();
            }
            else
                return false;
        }

        public bool Set_Signal_PhysicalValue(string channelName, string signalName, double signal_PhsicalValue, bool isSendNow = false)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).Set_Signal(signalName, signal_PhsicalValue, isSendNow);
            }
            else
                return false;
        }

        public bool Set_Signal_PhysicalValue(string channelName, uint CANid, string signalName, double signal_PhsicalValue, bool isSendNow = false)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).Set_Signal(signalName, CANid, signal_PhsicalValue, isSendNow);
            }
            else
                return false;
        }

        public bool Send_Signal_ByRawValue(string channelName, string signalName, ulong Raw_Value)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).Send_Signal_RawValue(signalName, Raw_Value);
            }
            else
                return false;
        }

        public bool Send_Signal_ByRawValue(string channelName, uint CANid, string signalName, ulong Raw_Value)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).Send_Signal_RawValue(signalName, CANid, Raw_Value);
            }
            else
                return false;
        }

        public double Get_Signal_PhysicalValue(string channelName, string signalName)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).Get_signal(signalName);
            }
            else
                return -1;
        }

        public bool Send_CAN_message(string channelName, uint CANid, uint dlc, string data, bool singleWireHighVoltage = false)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).Send_CAN_message(CANid, dlc, data, singleWireHighVoltage);
            }
            else
                return false;
        }

        public bool Send_CANFD_message(string channelName, uint CANid, uint dlc, string data)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).Send_CANFD_message(CANid, dlc, data);
            }
            else
                return false;
        }

        public void Send_cyclic_CAN_message(string channelName, uint CANid, uint dlc, string data, uint cycleTime)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).Send_cyclic_CAN_message(CANid, dlc, data, cycleTime);
            }
        }

        public void Send_cyclic_CANFD_message(string channelName, uint CANid, uint dlc, string data, uint cycleTime)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).Send_cyclic_CANFD_message(CANid, dlc, data, cycleTime);
            }
        }

        public bool Send_pits_message(string channelName, uint CANid, uint dlc, string data, bool singleWireHighVoltage = false)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).Send_pits_message(CANid, dlc, data, singleWireHighVoltage);
            }
            else
                return false;
        }

        public bool Send_pitsFD_message(string channelName, uint CANid, uint dlc, string data)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                return CAN_Channel_List.First(p => p.FullName == channelName).Send_pitsFD_message(CANid, dlc, data);
            }
            else
                return false;
        }

        public void Send_cyclic_pits_message(string channelName, uint CANid, uint dlc, string data, uint cycleTime)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).Send_cyclic_pits_message(CANid, dlc, data, cycleTime);
            }
        }

        public void Send_cyclic_pitsFD_message(string channelName, uint CANid, uint dlc, string data, uint cycleTime)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).Send_cyclic_pitsFD_message(CANid, dlc, data, cycleTime);
            }
        }

        public void Pause_cyclic_message(string channelName, uint CANid)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).Pause_cyclic_message(CANid);
            }
        }

        public void Resume_cyclic_message(string channelName, uint CANid)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).Resume_cyclic_message(CANid);
            }
        }

        public void Pause_cyclic_pits_message(string channelName, uint CANid, string data)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).Pause_cyclic_pits_message(CANid, data);
            }
        }

        public void Resume_cyclic_pits_message(string channelName, uint CANid, string data)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).Resume_cyclic_pits_message(CANid, data);
            }
        }

        public void Pause_allCyclic_message(string channelName)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).Pause_allCyclic_message();
            }
        }

        public void Resume_allCyclic_message(string channelName)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).Resume_allCyclic_message();
            }
        }

        public void Wakeup_SWC(string channelName)
        {
            if (CAN_Channel_List.Any(p => p.FullName == channelName))
            {
                CAN_Channel_List.First(p => p.FullName == channelName).Wakeup_SWC();
            }
        }

        void OnChannelMessageNotify_EventHandler(CAN_Parameter.CAN_Message Msg)
        {
            if (CANMessageWatch_ForLabview != null)
            {
                string str = "";
                if (Msg.Data != null)
                {
                    str = BitConverter.ToString(Msg.Data, 0, Msg.Data.Length).Replace("-", " ").ToUpper();
                }
                else
                {
                    str = "";
                }
                CANMessageWatch_ForLabview(Msg.ChannelName, Msg.ID, str);
            }
        }

        #region internal functions
        CanBaudRate ConvertBaudRate(string rate)
        {
            switch (rate)
            {
                case "33K":
                    return CanBaudRate.Baud_33Kbit;
                case "83K":
                    return CanBaudRate.Baud_83Kbit;
                case "100K":
                    return CanBaudRate.Baud_100Kbit;
                case "125K":
                    return CanBaudRate.Baud_125Kbit;
                case "250K":
                    return CanBaudRate.Baud_250Kbit;
                case "500K":
                    return CanBaudRate.Baud_500Kbit;
                case "1M":
                    return CanBaudRate.Baud_1Mbit;
                case "2M":
                    return CanBaudRate.Baud_2Mbit;
                case "4M":
                    return CanBaudRate.Baud_4Mbit;
                case "5M":
                    return CanBaudRate.Baud_5Mbit;
                default:
                    return CanBaudRate.Baud_500Kbit;
            }
        }

        LINBaudRate ConvertLINBaudRate(string rate)
        {
            switch (rate)
            {
                case "2400":
                    return LINBaudRate.Baud_2400;
                case "9600":
                    return LINBaudRate.Baud_9600;
                case "10400":
                    return LINBaudRate.Baud_10400;
                case "19200":
                    return LINBaudRate.Baud_19200;
                case "20000":
                    return LINBaudRate.Baud_20000;
                case "10417":
                    return LINBaudRate.Baud_10417;
                default:
                    return LINBaudRate.Baud_19200;
            }
        }

        LIN_Verison ConvertLINversion(string version)
        {
            switch (version)
            {
                case "LIN_1.3":
                    return LIN_Verison.LIN_1_3;
                case "LIN_2.0":
                    return LIN_Verison.LIN_2_0;
                case "LIN_2.1":
                    return LIN_Verison.LIN_2_1;
                case "SAE_J2602_0.0":
                    return LIN_Verison.J2602_0_0;
                case "SAE_J2602_1.0":
                    return LIN_Verison.J2602_1_1;
                case "SAE_J2602_1.2":
                    return LIN_Verison.J2602_1_2;
                default:
                    return LIN_Verison.LIN_2_0;
            }
        }

        byte[] StringToByte(string hexString, uint DLC)
        {
            hexString = hexString.Replace(" ", "");
            int byteLength = hexString.Length / 2;
            if ((hexString.Length % 2) != 0)
            {
                hexString += " ";
            }
            byte[] returnBytes = new byte[DLC];
            for (int i = 0; i < byteLength; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }
        #endregion
    }
}

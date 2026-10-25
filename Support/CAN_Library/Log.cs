using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace CAN_Library
{
    internal class Log
    {
        //internal static MyLog Logger = new MyLog();

        public static string _CANlogFolderPath = @"C:\Users\Public\Documents\CANLib_CANlog";
        public static string _CANlogFilePath = @"C:\Users\Public\Documents\CANLib_CANlog.log";

        public static string _RunlogFolderPath = @"C:\Users\Public\Documents\CANLib_Runlog";
        public static string _RunlogFilePath = @"C:\Users\Public\Documents\CANLib_Runlog.log";

        public static int LineCount = 0;

        public static List<MsgInfo> MsgLoggingList = new List<MsgInfo>();

        static readonly object _lock1 = new object();

        internal static Thread logThread;
        public static bool logThreadStarted = false;
        static int logLineCount = 0;
        internal static bool logThreadEnable = false;

        public static void AddLogToList(string time, string chan, CAN_Interface.Pmessage msg)
        {
            MsgInfo temp = new MsgInfo() { Msg = msg, Channel = chan, Time = time };
            lock (_lock1)
            {
                MsgLoggingList.Add(temp);
            }
        }

        public static void StartLogThread()
        {
            if (!logThreadStarted)
            {
                logThreadEnable = true;
                logThread = new Thread(new ThreadStart(logThreadHandler));
                logThread.IsBackground = true;
                logThread.Start();
                logThreadStarted = true;
            }
        }

        public static void StopLogThread()
        {
            if (logThreadStarted)
            {
                logThreadEnable = false;
                logThreadStarted = false;
            }
        }

        private static void logThreadHandler()
        {
            while (logThreadEnable)
            {
                try
                {
                    if (logLineCount > 5000)
                    {
                        lock (_lock1)
                        {
                            MsgLoggingList.RemoveRange(0, 5000);
                        }
                        logLineCount = 0;
                    }
                    if (logLineCount < MsgLoggingList.Count)
                    {
                        string str = "";
                        MsgInfo temp = MsgLoggingList[logLineCount];
                        if (MsgLoggingList[logLineCount].Msg.Data != null)
                        {
                            str = temp.Time + "   " + temp.Channel + "   " + temp.Msg.TxRx.ToString() + "   " +
                                temp.Msg.CANID.ToString("X") + "   " + BitConverter.ToString(temp.Msg.Data, 0, temp.Msg.DLC > temp.Msg.Data.Length ? (int)temp.Msg.Data.Length : (int)temp.Msg.DLC).Replace("-", " ").ToUpper();
                        }
                        else
                        {
                            str = temp.Time + "   " + temp.Channel + "   " + temp.Msg.TxRx.ToString() + "   " +
                                temp.Msg.CANID.ToString("X") + "   " + "no data";
                        }
                        Logger_CAN(str);
                        logLineCount++;
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    Logger_Run("2.5.1-logThreadHandler() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
                }
            }
        }

        public static void Logger_CAN(string str)
        {
            try
            {
                FileStream we = new FileStream(_CANlogFilePath, FileMode.Append, FileAccess.Write);
                string message = str + "\r\n";
                byte[] by = Encoding.Default.GetBytes(message);
                we.Write(by, 0, by.Length);

                we.Close();
                we.Dispose();

                if (LineCount++ >= 10000)
                {
                    _CANlogFilePath = string.Format(_CANlogFolderPath + @"\{0}.log", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
                    LineCount = 0;
                }
            }
            catch (Exception ex)
            {
                Logger_Run("2.5.2-Logger_CAN() has an exception-message: " + ex.Message + "\r\n              " + "StackTrace:" + ex.StackTrace);
            }
        }

        public static void Logger_Run(string str)
        {
            try
            {
                lock (_lock1)
                {
                    using (FileStream we = new FileStream(_RunlogFilePath, FileMode.Append, FileAccess.Write))
                    {
                        string message = DateTime.Now.ToString("MM-dd HH:mm:ss.fff") + "   " + str + "\r\n";
                        byte[] by = Encoding.Default.GetBytes(message);
                        we.Write(by, 0, by.Length);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static int GetRows(string FilePath)
        {
            using (StreamReader read = new StreamReader(FilePath, Encoding.Default))
            {
                return read.ReadToEnd().Split('\n').Length;
            }
        }

        public static int ReadFileLines(string FilePath)
        {
            int lines = 0;
            using (StreamReader sr = new StreamReader(FilePath, Encoding.Default))
            {
                while (sr.ReadLine() != null)
                {
                    lines++;
                }
            }
            return lines;
        }
    }

    internal class MsgInfo
    {
        public CAN_Interface.Pmessage Msg;
        public string Time;
        public string Channel;
    }
}


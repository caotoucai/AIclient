using System;
using System.IO;
namespace CAN_Library
{
    // 将旧库日志限制在本次任务目录，不使用旧库的 Public Documents 路径。
    internal static class Log
    {
        internal static string _RunlogFolderPath = Path.Combine(Environment.CurrentDirectory, "legacy-driver");
        internal static string _RunlogFilePath = Path.Combine(_RunlogFolderPath, "run.log");
        private static readonly object gate = new object();
        internal static void Logger_Run(string message) { lock (gate) { Directory.CreateDirectory(_RunlogFolderPath); File.AppendAllText(_RunlogFilePath, message + Environment.NewLine); } }
        internal static void Logger_CAN(string message) { Logger_Run(message); }
        internal static void StartLogThread() { }
        internal static void StopLogThread() { }
        internal static void AddLogToList(string time, string channel, CAN_Library.CAN_Interface.Pmessage message) { }
    }
}

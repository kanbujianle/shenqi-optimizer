using System;
using System.IO;

namespace ShenqiOptimizer
{
    /// <summary>
    /// 日志系统 - 记录系统运行状态和错误信息
    /// </summary>
    public static class Logger
    {
        private static string logPath;

        static Logger()
        {
            logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        public static void Log(string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logMessage = $"[{timestamp}] {message}";

                // 输出到控制台
                Console.WriteLine(logMessage);

                // 保存到文件
                string logFile = Path.Combine(logPath, $"log_{DateTime.Now:yyyyMMdd}.txt");
                File.AppendAllText(logFile, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[日志错误] {ex.Message}");
            }
        }

        /// <summary>
        /// 记录错误
        /// </summary>
        public static void Error(string message, Exception ex = null)
        {
            string fullMessage = ex != null 
                ? $"[错误] {message}\r\n{ex.GetType().Name}: {ex.Message}\r\n{ex.StackTrace}"
                : $"[错误] {message}";
            Log(fullMessage);
        }
    }
}
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ShenqiOptimizer.Core
{
    /// <summary>
    /// 游戏内存操作类 - 负责直接访问游戏进程内存
    /// 模拟参考项目中 asm.dll 的功能
    /// </summary>
    public partial class GameMemory
    {
        private Process targetProcess;
        private IntPtr processHandle;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;

        /// <summary>
        /// 附加到游戏进程
        /// </summary>
        public bool AttachProcess(Process process)
        {
            try
            {
                targetProcess = process;
                processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);

                if (processHandle == IntPtr.Zero)
                {
                    Logger.Log("[内存] 无法打开游戏进程，请确保以管理员权限运行");
                    return false;
                }

                Logger.Log($"[内存] 已附加到游戏进程: {process.ProcessName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 附加进程失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 读取内存
        /// </summary>
        public byte[] ReadMemory(IntPtr address, int size)
        {
            try
            {
                byte[] buffer = new byte[size];
                if (ReadProcessMemory(processHandle, address, buffer, size, out int bytesRead))
                {
                    Logger.Log($"[内存] 成功读取 {bytesRead} 字节 from {address.ToString("X8")}");
                    return buffer;
                }
                Logger.Log($"[错误] 读取内存失败 at {address.ToString("X8")}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 读取内存异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 写入内存
        /// </summary>
        public bool WriteMemory(IntPtr address, byte[] data)
        {
            try
            {
                if (WriteProcessMemory(processHandle, address, data, data.Length, out int bytesWritten))
                {
                    Logger.Log($"[内存] 成功写入 {bytesWritten} 字节 to {address.ToString("X8")}");
                    return true;
                }
                Logger.Log($"[错误] 写入内存失败 at {address.ToString("X8")}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 写入内存异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (processHandle != IntPtr.Zero)
                {
                    CloseHandle(processHandle);
                    Logger.Log("[内存] 已关闭游戏进程句柄");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 释放内存资源失败: {ex.Message}");
            }
        }
    }
}

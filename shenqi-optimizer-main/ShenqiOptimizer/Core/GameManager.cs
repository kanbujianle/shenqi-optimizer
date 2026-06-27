using System;
using System.Diagnostics;

namespace ShenqiOptimizer.Core
{
    /// <summary>
    /// 游戏管理器 - 负责游戏进程的检测、控制和事件管理
    /// </summary>
    public class GameManager
    {
        private ConfigManager configManager;
        private GameMemory gameMemory;
        private Process gameProcess;
        private const string GAME_PROCESS_NAME = "shenqi"; // 游戏进程名称

        public GameManager(ConfigManager config)
        {
            configManager = config;
            gameMemory = new GameMemory();
        }

        /// <summary>
        /// 检测游戏是否运行
        /// </summary>
        public bool IsGameRunning()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(GAME_PROCESS_NAME);
                if (processes.Length > 0)
                {
                    gameProcess = processes[0];
                    Logger.Log($"[游戏] 检测到游戏进程: PID={gameProcess.Id}");
                    return true;
                }
                Logger.Log("[游戏] 未检测到游戏进程");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 检测游戏进程失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取游戏进程
        /// </summary>
        public Process GetGameProcess()
        {
            return gameProcess;
        }

        /// <summary>
        /// 初始化游戏内存操作
        /// </summary>
        public bool InitializeGameMemory()
        {
            try
            {
                if (gameProcess == null)
                {
                    Logger.Log("[错误] 游戏进程未初始化");
                    return false;
                }

                return gameMemory.AttachProcess(gameProcess);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 初始化游戏内存失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 应用内存补丁
        /// </summary>
        public bool ApplyMemoryPatch(string patchName)
        {
            try
            {
                string address = configManager.GetValue($"补丁_{patchName}", "地址");
                string data = configManager.GetValue($"补丁_{patchName}", "数据");

                if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(data))
                {
                    Logger.Log($"[警告] 补丁配置不完整: {patchName}");
                    return false;
                }

                // 转换十六进制字符串为字节数组
                byte[] patchData = HexStringToByteArray(data);
                IntPtr patchAddress = new IntPtr(Convert.ToInt32(address, 16));

                return gameMemory.WriteMemory(patchAddress, patchData);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 应用补丁失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 读取游戏内存
        /// </summary>
        public byte[] ReadMemory(string addressHex, int size)
        {
            try
            {
                IntPtr address = new IntPtr(Convert.ToInt32(addressHex, 16));
                return gameMemory.ReadMemory(address, size);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 读取内存失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 写入游戏内存
        /// </summary>
        public bool WriteMemory(string addressHex, byte[] data)
        {
            try
            {
                IntPtr address = new IntPtr(Convert.ToInt32(addressHex, 16));
                return gameMemory.WriteMemory(address, data);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 写入内存失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 十六进制字符串转字节数组
        /// </summary>
        private byte[] HexStringToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] buffer = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                buffer[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return buffer;
        }
    }
}
using System;
using System.Collections.Generic;
using ShenqiOptimizer.Core;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// BUFF 管理器 - 管理增益效果、自动维持等
    /// </summary>
    public class BuffManager
    {
        private ConfigManager configManager;
        private Dictionary<string, long> buffCooldowns;
        private bool isRunning = false;

        public BuffManager(ConfigManager config)
        {
            configManager = config;
            buffCooldowns = new Dictionary<string, long>();
        }

        /// <summary>
        /// 启动 BUFF 管理
        /// </summary>
        public void StartBuffManagement()
        {
            isRunning = true;
            Logger.Log("[BUFF] BUFF 管理已启动");
        }

        /// <summary>
        /// 停止 BUFF 管理
        /// </summary>
        public void StopBuffManagement()
        {
            isRunning = false;
            Logger.Log("[BUFF] BUFF 管理已停止");
        }

        /// <summary>
        /// 应用 BUFF
        /// </summary>
        public bool ApplyBuff(string buffName)
        {
            try
            {
                // 检查冷却
                if (IsBuffOnCooldown(buffName))
                {
                    Logger.Log($"[BUFF] BUFF 冷却中: {buffName}");
                    return false;
                }

                Logger.Log($"[BUFF] 应用 BUFF: {buffName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 应用 BUFF 失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查 BUFF 是否在冷却
        /// </summary>
        private bool IsBuffOnCooldown(string buffName)
        {
            if (!buffCooldowns.ContainsKey(buffName))
                return false;

            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return buffCooldowns[buffName] > currentTime;
        }

        /// <summary>
        /// 获取运行状态
        /// </summary>
        public bool IsRunning() => isRunning;
    }
}
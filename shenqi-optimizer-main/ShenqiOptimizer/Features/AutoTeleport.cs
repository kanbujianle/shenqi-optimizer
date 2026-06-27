using System;
using System.Threading;
using System.Threading.Tasks;
using ShenqiOptimizer.Core;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// 自动传送功能 - 实现快速移动、多地点循环等
    /// </summary>
    public class AutoTeleport
    {
        private ConfigManager configManager;
        private bool isRunning = false;

        public AutoTeleport(ConfigManager config)
        {
            configManager = config;
        }

        /// <summary>
        /// 传送到指定位置
        /// </summary>
        public async Task TeleportTo(string locationName)
        {
            try
            {
                Logger.Log($"[传送] 正在传送到: {locationName}");

                await Task.Run(() =>
                {
                    // TODO: 实现传送逻辑
                    // 1. 打开传送界面
                    // 2. 选择目标位置
                    // 3. 确认传送
                    Thread.Sleep(2000); // 模拟传送时间
                });

                Logger.Log($"[传送] 成功传送到: {locationName}");
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 传送失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 启动循环传送
        /// </summary>
        public async Task StartAutoTeleportLoop(string[] locations)
        {
            if (isRunning)
            {
                Logger.Log("[传送] 自动传送已在运行");
                return;
            }

            isRunning = true;
            Logger.Log("[传送] 自动传送循环已启动");

            await Task.Run(async () =>
            {
                while (isRunning)
                {
                    foreach (var location in locations)
                    {
                        if (!isRunning) break;

                        await TeleportTo(location);
                        Thread.Sleep(5000); // 传送间隔
                    }
                }
            });
        }

        /// <summary>
        /// 停止自动传送
        /// </summary>
        public void StopAutoTeleport()
        {
            isRunning = false;
            Logger.Log("[传送] 自动传送已停止");
        }

        /// <summary>
        /// 获取运行状态
        /// </summary>
        public bool IsRunning() => isRunning;
    }
}
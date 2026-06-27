using System;
using System.Threading;
using System.Threading.Tasks;
using ShenqiOptimizer.Core;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// 改进的自动打怪系统 - 集成所有战斗子模块
    /// </summary>
    public class AutoFarmSystem
    {
        private ConfigManager configManager;
        private GameMemory gameMemory;
        private MonsterDetector monsterDetector;
        private TargetManager targetManager;
        private SkillManager skillManager;
        private PlayerStatus playerStatus;
        private CombatEngine combatEngine;
        private BuffManager buffManager;
        private AutoPickup autoPickup;
        private SellSystem sellSystem;

        private bool isRunning = false;
        private int mainLoopInterval; // 主循环间隔（毫秒）
        private int scanInterval;     // 怪物扫描间隔（毫秒）
        private long lastScanTime = 0;

        public AutoFarmSystem(ConfigManager config, GameMemory memory)
        {
            configManager = config;
            gameMemory = memory;

            // 初始化所有子系统
            monsterDetector = new MonsterDetector(config, memory);
            targetManager = new TargetManager(config);
            skillManager = new SkillManager(config);
            playerStatus = new PlayerStatus(config);
            var combatSys = new CombatSystem(config);
            combatEngine = new CombatEngine(config, monsterDetector, targetManager, skillManager, playerStatus, combatSys);
            buffManager = new BuffManager(config);
            autoPickup = new AutoPickup(config);
            sellSystem = new SellSystem(config);

            // 读取配置
            mainLoopInterval = configManager.GetIntValue("全局", "全局_编辑框_战斗_循环间隔", 100);
            scanInterval = configManager.GetIntValue("全局", "全局_编辑框_战斗_怪物扫描间隔", 1000);

            Logger.Log("[自动打怪] 系统已初始化");
        }

        /// <summary>
        /// 启动自动打怪
        /// </summary>
        public async Task Start()
        {
            if (isRunning)
            {
                Logger.Log("[自动打怪] 系统已在运行");
                return;
            }

            isRunning = true;
            Logger.Log("[自动打怪] 系统已启动");

            await Task.Run(async () =>
            {
                while (isRunning)
                {
                    try
                    {
                        // 执行主循环
                        await MainLoop();

                        // 等待下一个循环
                        await Task.Delay(mainLoopInterval);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"[错误] 主循环异常: {ex.Message}");
                        await Task.Delay(1000);
                    }
                }
            });
        }

        /// <summary>
        /// 停止自动打怪
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            targetManager.ClearTarget();
            Logger.Log("[自动打怪] 系统已停止");
        }

        /// <summary>
        /// 主循环
        /// </summary>
        private async Task MainLoop()
        {
            try
            {
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                // 1. 定期扫描怪物（不必每帧都扫描以提高性能）
                if (currentTime - lastScanTime >= scanInterval)
                {
                    monsterDetector.ScanMonsters();
                    lastScanTime = currentTime;
                }

                // 2. 执行战斗循环
                await combatEngine.ExecuteCombatLoop();

                // 3. 检查并使用BUFF
                if (buffManager.IsRunning())
                {
                    // TODO: 实现BUFF维持逻辑
                }

                // 4. 检查是否需要捡物
                if (autoPickup.IsRunning())
                {
                    // TODO: 实现捡物逻辑
                }

                // 5. 检查是否需要卖出
                if (sellSystem.IsRunning())
                {
                    // TODO: 实现卖出逻辑
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 主循环执行失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取系统运行状态
        /// </summary>
        public bool IsRunning() => isRunning;

        /// <summary>
        /// 获取当前目标
        /// </summary>
        public MonsterDetector.MonsterInfo GetCurrentTarget()
        {
            return targetManager.GetCurrentTarget();
        }

        /// <summary>
        /// 获取玩家状态
        /// </summary>
        public PlayerStatus.PlayerStatusInfo GetPlayerStatus()
        {
            return playerStatus.GetStatus();
        }

        /// <summary>
        /// 获取怪物检测器
        /// </summary>
        public MonsterDetector GetMonsterDetector() => monsterDetector;

        /// <summary>
        /// 获取技能管理器
        /// </summary>
        public SkillManager GetSkillManager() => skillManager;

        /// <summary>
        /// 获取战斗引擎
        /// </summary>
        public CombatEngine GetCombatEngine() => combatEngine;
    }
}
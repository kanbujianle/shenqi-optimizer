using System;
using System.Collections.Generic;
using System.Linq;
using ShenqiOptimizer.Core;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// 怪物检测和管理系统
    /// </summary>
    public class MonsterDetector
    {
        private ConfigManager configManager;
        private GameMemory gameMemory;
        private List<MonsterInfo> detectedMonsters;

        public MonsterDetector(ConfigManager config, GameMemory memory)
        {
            configManager = config;
            gameMemory = memory;
            detectedMonsters = new List<MonsterInfo>();
        }

        /// <summary>
        /// 怪物信息结构
        /// </summary>
        public class MonsterInfo
        {
            public int MonsterId { get; set; }          // 怪物ID
            public string MonsterName { get; set; }     // 怪物名称
            public int Health { get; set; }             // 血量
            public int MaxHealth { get; set; }          // 最大血量
            public float Distance { get; set; }         // 距离（米）
            public float X { get; set; }                // X坐标
            public float Y { get; set; }                // Y坐标
            public float Z { get; set; }                // Z坐标
            public int Level { get; set; }              // 等级
            public bool IsElite { get; set; }           // 是否精英怪
            public bool IsInCombat { get; set; }        // 是否在战斗中
            public long LastDetectTime { get; set; }    // 最后检测时间
        }

        /// <summary>
        /// 扫描周围怪物
        /// </summary>
        public List<MonsterInfo> ScanMonsters()
        {
            try
            {
                detectedMonsters.Clear();

                // TODO: 实现内存读取逻辑
                // 1. 读取游戏内存中的怪物列表基址
                // 2. 遍历怪物数组
                // 3. 读取每个怪物的属性（ID、名称、位置、血量等）
                // 4. 过滤无效怪物

                Logger.Log($"[怪物检测] 扫描完成，发现 {detectedMonsters.Count} 个怪物");
                return detectedMonsters;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 怪物扫描失败: {ex.Message}");
                return new List<MonsterInfo>();
            }
        }

        /// <summary>
        /// 获取有效的可攻击怪物列表（已过滤）
        /// </summary>
        public List<MonsterInfo> GetValidTargets()
        {
            try
            {
                // 读取过滤配置
                string ignoreMonsters = configManager.GetValue("共用", "全局_过滤物品数组");
                string[] ignoreList = string.IsNullOrEmpty(ignoreMonsters) 
                    ? new string[0] 
                    : ignoreMonsters.Split('|');

                // 读取距离限制
                int maxDistance = configManager.GetIntValue("全局", "全局_编辑框_打怪距离", 15);

                // 过滤怪物
                var validTargets = detectedMonsters
                    .Where(m => 
                        m.Health > 0 &&                                    // 血量 > 0
                        m.Distance <= maxDistance &&                       // 在距离范围内
                        !ignoreList.Any(ignore => m.MonsterName.Contains(ignore)) &&  // 不在忽略列表中
                        !m.MonsterName.Contains("BOSS") &&                 // 排除BOSS
                        m.Level >= 1                                       // 等级有效
                    )
                    .OrderBy(m => m.Distance)  // 按距离排序（优先最近的）
                    .ToList();

                Logger.Log($"[怪物过滤] 有效目标: {validTargets.Count} 个");
                return validTargets;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 怪物过滤失败: {ex.Message}");
                return new List<MonsterInfo>();
            }
        }

        /// <summary>
        /// 选择最优目标
        /// </summary>
        public MonsterInfo SelectBestTarget()
        {
            var validTargets = GetValidTargets();

            if (validTargets.Count == 0)
            {
                Logger.Log("[目标选择] 没有有效目标");
                return null;
            }

            // 优先级: 距离最近 > 血量最少 > 等级最低
            var bestTarget = validTargets
                .OrderBy(m => m.Distance)           // 首先按距离排序
                .ThenBy(m => m.Health)              // 然后按血量排序
                .ThenBy(m => m.Level)               // 最后按等级排序
                .First();

            Logger.Log($"[目标选择] 已选择目标: {bestTarget.MonsterName} (ID:{bestTarget.MonsterId}, 距离:{bestTarget.Distance}m, 血量:{bestTarget.Health}/{bestTarget.MaxHealth})");
            return bestTarget;
        }

        /// <summary>
        /// 获取所有检测到的怪物
        /// </summary>
        public List<MonsterInfo> GetAllMonsters()
        {
            return new List<MonsterInfo>(detectedMonsters);
        }

        /// <summary>
        /// 获取怪物总数
        /// </summary>
        public int GetMonsterCount()
        {
            return detectedMonsters.Count;
        }

        /// <summary>
        /// 清空怪物列表
        /// </summary>
        public void Clear()
        {
            detectedMonsters.Clear();
        }
    }
}
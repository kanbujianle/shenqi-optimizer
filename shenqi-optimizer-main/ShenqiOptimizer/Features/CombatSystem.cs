using System;
using System.Collections.Generic;
using ShenqiOptimizer.Core;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// 战斗系统 - 管理技能释放、BUFF、负面状态处理等
    /// </summary>
    public class CombatSystem
    {
        private ConfigManager configManager;
        private Dictionary<string, long> skillCooldowns; // 技能冷却时间

        public CombatSystem(ConfigManager config)
        {
            configManager = config;
            skillCooldowns = new Dictionary<string, long>();
        }

        /// <summary>
        /// 检查是否需要解除负面状态
        /// </summary>
        public bool CheckDebuff()
        {
            // TODO: 实现负面状态检测
            // 从配置中读取需要驱散的状态列表
            string debuffList = configManager.GetValue("共用", "全局_被MS技能驱散的数组");
            Logger.Log("[战斗] 检查负面状态");
            return false;
        }

        /// <summary>
        /// 自动吃药
        /// </summary>
        public bool UsePotion(string potionType)
        {
            try
            {
                // 读取配置
                int hpThreshold = configManager.GetIntValue("全局", "全局_编辑框_我的HP阈值", 50);
                int mpThreshold = configManager.GetIntValue("全局", "全局_编辑框_我的MP阈值", 20);

                Logger.Log($"[战斗] 使用药物: {potionType}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 吃药失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 释放技能
        /// </summary>
        public bool CastSkill(string skillName)
        {
            try
            {
                // 检查冷却
                if (IsSkillOnCooldown(skillName))
                {
                    Logger.Log($"[战斗] 技能冷却中: {skillName}");
                    return false;
                }

                // TODO: 实现技能释放逻辑
                Logger.Log($"[战斗] 释放技能: {skillName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 释放技能失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查技能是否在冷却
        /// </summary>
        private bool IsSkillOnCooldown(string skillName)
        {
            if (!skillCooldowns.ContainsKey(skillName))
                return false;

            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return skillCooldowns[skillName] > currentTime;
        }

        /// <summary>
        /// 设置技能冷却
        /// </summary>
        private void SetSkillCooldown(string skillName, long cooldownMs)
        {
            long cooldownTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + cooldownMs;
            skillCooldowns[skillName] = cooldownTime;
        }
    }
}
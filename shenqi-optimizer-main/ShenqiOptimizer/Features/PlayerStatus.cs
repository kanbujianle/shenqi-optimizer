using System;
using System.Collections.Generic;
using ShenqiOptimizer.Core;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// 玩家状态管理系统
    /// </summary>
    public class PlayerStatus
    {
        private ConfigManager configManager;

        public class PlayerStatusInfo
        {
            public int Health { get; set; }             // 当前血量
            public int MaxHealth { get; set; }          // 最大血量
            public int Mana { get; set; }               // 当前魔法
            public int MaxMana { get; set; }            // 最大魔法
            public int Stamina { get; set; }            // 当前怒气/灵力
            public int MaxStamina { get; set; }         // 最大怒气/灵力
            public List<string> ActiveBuffs { get; set; }    // 活跃BUFF
            public List<string> ActiveDebuffs { get; set; }  // 活跃DEBUFF
            public float HealthPercent { get; set; }   // 血量百分比
            public float ManaPercent { get; set; }     // 魔法百分比
            public float StaminaPercent { get; set; }  // 怒气百分比
        }

        private PlayerStatusInfo status;

        public PlayerStatus(ConfigManager config)
        {
            configManager = config;
            status = new PlayerStatusInfo
            {
                ActiveBuffs = new List<string>(),
                ActiveDebuffs = new List<string>()
            };
        }

        /// <summary>
        /// 更新玩家状态（从内存读取）
        /// </summary>
        public void UpdateStatus()
        {
            try
            {
                // TODO: 实现从游戏内存读取玩家状态
                // 读取: 血量、魔法、怒气等属性
                // 读取: BUFF/DEBUFF列表
                // 计算: 百分比数值

                // 示例数据（实际需要从内存读取）
                status.HealthPercent = (status.Health / (float)status.MaxHealth) * 100;
                status.ManaPercent = (status.Mana / (float)status.MaxMana) * 100;
                status.StaminaPercent = (status.Stamina / (float)status.MaxStamina) * 100;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 更新玩家状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取玩家状态
        /// </summary>
        public PlayerStatusInfo GetStatus()
        {
            UpdateStatus();
            return status;
        }

        /// <summary>
        /// 获取血量百分比
        /// </summary>
        public float GetHealthPercent()
        {
            return status.HealthPercent;
        }

        /// <summary>
        /// 获取魔法百分比
        /// </summary>
        public float GetManaPercent()
        {
            return status.ManaPercent;
        }

        /// <summary>
        /// 检查血量是否低于阈值
        /// </summary>
        public bool IsHealthLow()
        {
            int threshold = configManager.GetIntValue("全局", "全局_编辑框_我的HP阈值", 50);
            return status.HealthPercent < threshold;
        }

        /// <summary>
        /// 检查魔法是否低于阈值
        /// </summary>
        public bool IsManaLow()
        {
            int threshold = configManager.GetIntValue("全局", "全局_编辑框_我的MP阈值", 20);
            return status.ManaPercent < threshold;
        }

        /// <summary>
        /// 检查是否有指定BUFF
        /// </summary>
        public bool HasBuff(string buffName)
        {
            return status.ActiveBuffs.Contains(buffName);
        }

        /// <summary>
        /// 检查是否有指定DEBUFF
        /// </summary>
        public bool HasDebuff(string debuffName)
        {
            return status.ActiveDebuffs.Contains(debuffName);
        }

        /// <summary>
        /// 添加BUFF
        /// </summary>
        public void AddBuff(string buffName)
        {
            if (!status.ActiveBuffs.Contains(buffName))
            {
                status.ActiveBuffs.Add(buffName);
                Logger.Log($"[BUFF] 获得增益: {buffName}");
            }
        }

        /// <summary>
        /// 移除BUFF
        /// </summary>
        public void RemoveBuff(string buffName)
        {
            if (status.ActiveBuffs.Contains(buffName))
            {
                status.ActiveBuffs.Remove(buffName);
                Logger.Log($"[BUFF] 失去增益: {buffName}");
            }
        }
    }
}
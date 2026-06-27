using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ShenqiOptimizer.Core;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// 技能管理系统 - 管理技能冷却、优先级、执行顺序
    /// </summary>
    public class SkillManager
    {
        private ConfigManager configManager;
        private Dictionary<string, SkillInfo> skills;
        private Queue<string> skillExecutionQueue;

        public class SkillInfo
        {
            public string SkillName { get; set; }       // 技能名称
            public int SkillId { get; set; }             // 技能ID
            public int Cooldown { get; set; }            // 冷却时间（毫秒）
            public int ManaCost { get; set; }            // 魔法消耗
            public int Priority { get; set; }            // 优先级（1-10，数字越大优先级越高）
            public float Range { get; set; }             // 施法范围（米）
            public bool IsAOE { get; set; }              // 是否群体攻击
            public long LastCastTime { get; set; }       // 最后施法时间
        }

        public SkillManager(ConfigManager config)
        {
            configManager = config;
            skills = new Dictionary<string, SkillInfo>();
            skillExecutionQueue = new Queue<string>();
            InitializeSkills();
        }

        /// <summary>
        /// 初始化技能配置
        /// </summary>
        private void InitializeSkills()
        {
            try
            {
                // TODO: 从配置文件加载技能列表
                // 示例技能配置
                skills["普攻"] = new SkillInfo
                {
                    SkillName = "普攻",
                    SkillId = 1001,
                    Cooldown = 500,
                    ManaCost = 0,
                    Priority = 1,
                    Range = 3f,
                    IsAOE = false,
                    LastCastTime = 0
                };

                skills["剑舞"] = new SkillInfo
                {
                    SkillName = "剑舞",
                    SkillId = 1002,
                    Cooldown = 3000,
                    ManaCost = 50,
                    Priority = 8,
                    Range = 5f,
                    IsAOE = true,
                    LastCastTime = 0
                };

                skills["挥剑斩"] = new SkillInfo
                {
                    SkillName = "挥剑斩",
                    SkillId = 1003,
                    Cooldown = 2000,
                    ManaCost = 30,
                    Priority = 7,
                    Range = 4f,
                    IsAOE = false,
                    LastCastTime = 0
                };

                Logger.Log($"[技能] 已初始化 {skills.Count} 个技能");
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 技能初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查技能是否可用
        /// </summary>
        public bool IsSkillAvailable(string skillName)
        {
            try
            {
                if (!skills.ContainsKey(skillName))
                {
                    Logger.Log($"[技能] 技能不存在: {skillName}");
                    return false;
                }

                var skill = skills[skillName];
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                long elapsed = currentTime - skill.LastCastTime;

                return elapsed >= skill.Cooldown;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 技能检查失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取技能冷却剩余时间（毫秒）
        /// </summary>
        public long GetSkillCooldownRemaining(string skillName)
        {
            if (!skills.ContainsKey(skillName))
                return 0;

            var skill = skills[skillName];
            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long elapsed = currentTime - skill.LastCastTime;
            long remaining = skill.Cooldown - elapsed;

            return Math.Max(0, remaining);
        }

        /// <summary>
        /// 标记技能已施法
        /// </summary>
        public void MarkSkillCasted(string skillName)
        {
            if (skills.ContainsKey(skillName))
            {
                skills[skillName].LastCastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                Logger.Log($"[技能] 已施法: {skillName}");
            }
        }

        /// <summary>
        /// 根据优先级获取下一个可用技能
        /// </summary>
        public SkillInfo GetNextAvailableSkill(int playerMana = int.MaxValue)
        {
            try
            {
                var availableSkills = skills.Values
                    .Where(s => IsSkillAvailable(s.SkillName) && s.ManaCost <= playerMana)
                    .OrderByDescending(s => s.Priority)      // 优先级从高到低
                    .ThenBy(s => s.Cooldown)                 // 冷却时间从短到长
                    .ToList();

                if (availableSkills.Count == 0)
                {
                    Logger.Log("[技能] 没有可用技能");
                    return null;
                }

                var nextSkill = availableSkills.First();
                Logger.Log($"[技能] 下一个技能: {nextSkill.SkillName} (优先级:{nextSkill.Priority})");
                return nextSkill;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 获取下一个技能失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取所有技能
        /// </summary>
        public Dictionary<string, SkillInfo> GetAllSkills()
        {
            return new Dictionary<string, SkillInfo>(skills);
        }

        /// <summary>
        /// 添加自定义技能
        /// </summary>
        public void AddSkill(SkillInfo skill)
        {
            skills[skill.SkillName] = skill;
            Logger.Log($"[技能] 已添加技能: {skill.SkillName}");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShenqiOptimizer.Core;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// 战斗执行引擎 - 处理实际的战斗逻辑执行
    /// </summary>
    public class CombatEngine
    {
        private ConfigManager configManager;
        private MonsterDetector monsterDetector;
        private TargetManager targetManager;
        private SkillManager skillManager;
        private PlayerStatus playerStatus;
        private CombatSystem combatSystem;

        private bool isInCombat = false;
        private int combatLoopInterval; // 战斗循环间隔（毫秒）

        public CombatEngine(
            ConfigManager config,
            MonsterDetector detector,
            TargetManager targetMgr,
            SkillManager skillMgr,
            PlayerStatus playerStat,
            CombatSystem combatSys)
        {
            configManager = config;
            monsterDetector = detector;
            targetManager = targetMgr;
            skillManager = skillMgr;
            playerStatus = playerStat;
            combatSystem = combatSys;
            combatLoopInterval = configManager.GetIntValue("全局", "全局_编辑框_战斗_循环间隔", 100);
        }

        /// <summary>
        /// 执行单次战斗循环
        /// </summary>
        public async Task ExecuteCombatLoop()
        {
            try
            {
                isInCombat = true;

                // 1. 更新玩家状态
                playerStatus.UpdateStatus();

                // 2. 检查是否需要喝药
                await HandleHealthAndMana();

                // 3. 检查并移除负面状态
                await HandleDebuffs();

                // 4. 如果没有当前目标或目标已死亡，选择新目标
                if (!targetManager.HasValidTarget())
                {
                    await SelectNewTarget();
                }

                // 5. 如果有有效目标，执行战斗
                if (targetManager.HasValidTarget())
                {
                    await ExecuteAttack();
                }

                // 6. 检查是否应该继续战斗
                if (targetManager.GetCurrentTarget() == null)
                {
                    isInCombat = false;
                    Logger.Log("[战斗] 本轮战斗结束");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 战斗循环执行失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理血量和魔法
        /// </summary>
        private async Task HandleHealthAndMana()
        {
            try
            {
                // 检查血量
                if (playerStatus.IsHealthLow())
                {
                    Logger.Log($"[战斗] 血量过低 ({playerStatus.GetHealthPercent():F1}%), 使用治疗物品");
                    await combatSystem.UsePotion("HP药");
                    await Task.Delay(500); // 等待药物生效
                }

                // 检查魔法
                if (playerStatus.IsManaLow())
                {
                    Logger.Log($"[战斗] 魔法过低 ({playerStatus.GetManaPercent():F1}%), 使用魔法药");
                    await combatSystem.UsePotion("MP药");
                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 处理血量和魔法失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理负面状态（DEBUFF）
        /// </summary>
        private async Task HandleDebuffs()
        {
            try
            {
                var status = playerStatus.GetStatus();

                if (status.ActiveDebuffs.Count > 0)
                {
                    foreach (var debuff in status.ActiveDebuffs)
                    {
                        Logger.Log($"[战斗] 检测到负面状态: {debuff}, 尝试驱散");
                        // TODO: 实现驱散DEBUFF的逻辑
                        await Task.Delay(100);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 处理负面状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 选择新目标
        /// </summary>
        private async Task SelectNewTarget()
        {
            try
            {
                // 扫描周围怪物
                monsterDetector.ScanMonsters();

                // 选择最优目标
                var bestTarget = monsterDetector.SelectBestTarget();

                if (bestTarget != null)
                {
                    targetManager.SetTarget(bestTarget);
                    Logger.Log($"[战斗] 选择新目标: {bestTarget.MonsterName}");
                }
                else
                {
                    Logger.Log("[战斗] 没有可用目标，战斗结束");
                    isInCombat = false;
                }

                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 选择目标失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行攻击
        /// </summary>
        private async Task ExecuteAttack()
        {
            try
            {
                var target = targetManager.GetCurrentTarget();
                if (target == null)
                    return;

                Logger.Log($"[战斗] 攻击目标: {target.MonsterName} (血量:{target.Health}/{target.MaxHealth})");

                // 1. 获取下一个可用技能
                var nextSkill = skillManager.GetNextAvailableSkill((int)playerStatus.GetStatus().Mana);

                if (nextSkill != null)
                {
                    // 2. 施放技能
                    await CastSkill(nextSkill);

                    // 3. 等待技能释放时间
                    await Task.Delay(200);
                }
                else
                {
                    Logger.Log("[战斗] 没有可用技能");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 执行攻击失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 施放技能
        /// </summary>
        private async Task CastSkill(SkillManager.SkillInfo skill)
        {
            try
            {
                Logger.Log($"[技能] 施放: {skill.SkillName}");

                // TODO: 实现实际的技能释放逻辑
                // 1. 向游戏发送技能释放命令
                // 2. 标记技能冷却
                // 3. 更新玩家魔法值

                skillManager.MarkSkillCasted(skill.SkillName);
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 施放技能失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取战斗状态
        /// </summary>
        public bool IsInCombat() => isInCombat;
    }
}
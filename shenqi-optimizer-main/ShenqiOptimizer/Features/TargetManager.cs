using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShenqiOptimizer.Core;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// 目标管理系统 - 管理当前攻击目标的状态
    /// </summary>
    public class TargetManager
    {
        private ConfigManager configManager;
        private MonsterDetector.MonsterInfo currentTarget;
        private Stopwatch targetLockTimer;
        private long targetLockDuration; // 目标锁定时间（毫秒）

        public TargetManager(ConfigManager config)
        {
            configManager = config;
            targetLockTimer = new Stopwatch();
            targetLockDuration = configManager.GetIntValue("全局", "全局_编辑框_自动换目标间隔", 3000);
        }

        /// <summary>
        /// 设置当前目标
        /// </summary>
        public void SetTarget(MonsterDetector.MonsterInfo target)
        {
            if (target == null)
            {
                ClearTarget();
                return;
            }

            currentTarget = target;
            targetLockTimer.Restart();
            Logger.Log($"[目标] 已锁定目标: {target.MonsterName}");
        }

        /// <summary>
        /// 获取当前目标
        /// </summary>
        public MonsterDetector.MonsterInfo GetCurrentTarget()
        {
            return currentTarget;
        }

        /// <summary>
        /// 检查是否有有效目标
        /// </summary>
        public bool HasValidTarget()
        {
            return currentTarget != null && currentTarget.Health > 0;
        }

        /// <summary>
        /// 检查目标是否在锁定时间内
        /// </summary>
        public bool IsTargetLocked()
        {
            if (!HasValidTarget())
                return false;

            return targetLockTimer.ElapsedMilliseconds < targetLockDuration;
        }

        /// <summary>
        /// 检查目标是否死亡
        /// </summary>
        public bool IsTargetDead()
        {
            return currentTarget != null && currentTarget.Health <= 0;
        }

        /// <summary>
        /// 清除当前目标
        /// </summary>
        public void ClearTarget()
        {
            if (currentTarget != null)
            {
                Logger.Log($"[目标] 已清除目标: {currentTarget.MonsterName}");
                currentTarget = null;
            }
            targetLockTimer.Stop();
        }

        /// <summary>
        /// 获取目标距离
        /// </summary>
        public float GetTargetDistance()
        {
            return currentTarget?.Distance ?? float.MaxValue;
        }

        /// <summary>
        /// 获取目标血量百分比
        /// </summary>
        public float GetTargetHealthPercent()
        {
            if (currentTarget == null || currentTarget.MaxHealth <= 0)
                return 0;

            return (currentTarget.Health / (float)currentTarget.MaxHealth) * 100;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// 经验金币速率计算器
    /// </summary>
    public class RewardCalculator
    {
        private List<RewardRecord> rewardHistory;
        private DateTime sessionStartTime;
        private const int MaxHistorySize = 10000;

        public class RewardRecord
        {
            public DateTime Timestamp { get; set; }
            public long Experience { get; set; }    // 获得的经验
            public long Gold { get; set; }           // 获得的金币
            public string SourceName { get; set; }   // 来源（怪物名）
        }

        public RewardCalculator()
        {
            rewardHistory = new List<RewardRecord>(MaxHistorySize);
            sessionStartTime = DateTime.Now;
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            rewardHistory.Clear();
            sessionStartTime = DateTime.Now;
            Logger.Log("[奖励] 奖励计算已启动");
        }

        /// <summary>
        /// 记录奖励
        /// </summary>
        public void RecordReward(long experience, long gold, string sourceName)
        {
            try
            {
                var record = new RewardRecord
                {
                    Timestamp = DateTime.Now,
                    Experience = experience,
                    Gold = gold,
                    SourceName = sourceName
                };

                rewardHistory.Add(record);

                if (rewardHistory.Count > MaxHistorySize)
                    rewardHistory.RemoveAt(0);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 记录奖励失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取总经验
        /// </summary>
        public long GetTotalExperience()
        {
            return rewardHistory.Sum(r => r.Experience);
        }

        /// <summary>
        /// 获取总金币
        /// </summary>
        public long GetTotalGold()
        {
            return rewardHistory.Sum(r => r.Gold);
        }

        /// <summary>
        /// 获取经验速率 (每小时)
        /// </summary>
        public float GetExperiencePerHour()
        {
            var elapsed = DateTime.Now - sessionStartTime;
            if (elapsed.TotalHours < 0.016667) // 至少运行 1 分钟
                return 0;

            return (float)(GetTotalExperience() / elapsed.TotalHours);
        }

        /// <summary>
        /// 获取金币速率 (每小时)
        /// </summary>
        public float GetGoldPerHour()
        {
            var elapsed = DateTime.Now - sessionStartTime;
            if (elapsed.TotalHours < 0.016667)
                return 0;

            return (float)(GetTotalGold() / elapsed.TotalHours);
        }

        /// <summary>
        /// 获取运行时间
        /// </summary>
        public TimeSpan GetRuntime()
        {
            return DateTime.Now - sessionStartTime;
        }

        /// <summary>
        /// 生成奖励报告
        /// </summary>
        public string GenerateReport()
        {
            try
            {
                var runtime = GetRuntime();
                var totalExp = GetTotalExperience();
                var totalGold = GetTotalGold();
                var expPerHour = GetExperiencePerHour();
                var goldPerHour = GetGoldPerHour();

                string report = $@"
╔══════════════════════════════════════════════════════════════╗
║                    奖励收获统计                              ║
╠══════════════════════════════════════════════════════════════╣
║ 运行时间:     {runtime:hh\:mm\:ss}
║ 总经验:       {totalExp:N0}
║ 经验/小时:    {expPerHour:N0}
║ 总金币:       {totalGold:N0}
║ 金币/小时:    {goldPerHour:N0}
║ 战斗次数:     {rewardHistory.Count}
╠══════════════════════════════════════════════════════════════╣
║ 按来源统计:
";

                var bySource = rewardHistory
                    .GroupBy(r => r.SourceName)
                    .OrderByDescending(g => g.Count())
                    .Take(10);

                foreach (var source in bySource)
                {
                    var totalExp2 = source.Sum(r => r.Experience);
                    var totalGold2 = source.Sum(r => r.Gold);
                    report += $"║   {source.Key,-20} EXP: {totalExp2:N0} | Gold: {totalGold2:N0} ({source.Count()} 次)\n";
                }

                report += "╚══════════════════════════════════════════════════════════════╝";
                return report;
            }
            catch { return "[错误] 生成奖励报告失败"; }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            Logger.Log($"[奖励] 奖励计算已停止 (总经验: {GetTotalExperience():N0}, 总金币: {GetTotalGold():N0})");
        }
    }
}
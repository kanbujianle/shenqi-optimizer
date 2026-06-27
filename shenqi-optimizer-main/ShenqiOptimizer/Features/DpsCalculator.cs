using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// DPS 计算器 - 计算每秒伤害输出
    /// </summary>
    public class DpsCalculator
    {
        private List<DamageRecord> damageHistory;
        private Stopwatch sessionTimer;
        private const int MaxHistorySize = 1000;

        public class DamageRecord
        {
            public DateTime Timestamp { get; set; }
            public float Damage { get; set; }
            public string SkillName { get; set; }
            public string TargetName { get; set; }
        }

        public DpsCalculator()
        {
            damageHistory = new List<DamageRecord>(MaxHistorySize);
            sessionTimer = new Stopwatch();
        }

        /// <summary>
        /// 启动 DPS 计算
        /// </summary>
        public void Start()
        {
            damageHistory.Clear();
            sessionTimer.Restart();
            Logger.Log("[DPS] DPS 计算已启动");
        }

        /// <summary>
        /// 记录伤害
        /// </summary>
        public void RecordDamage(float damage, string skillName, string targetName)
        {
            try
            {
                var record = new DamageRecord
                {
                    Timestamp = DateTime.Now,
                    Damage = damage,
                    SkillName = skillName,
                    TargetName = targetName
                };

                damageHistory.Add(record);

                if (damageHistory.Count > MaxHistorySize)
                    damageHistory.RemoveAt(0);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 记录伤害失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取总伤害
        /// </summary>
        public float GetTotalDamage()
        {
            return damageHistory.Sum(d => d.Damage);
        }

        /// <summary>
        /// 获取 DPS (每秒伤害)
        /// </summary>
        public float GetDps()
        {
            if (sessionTimer.ElapsedMilliseconds < 1000) // 至少运行 1 秒
                return 0;

            float totalDamage = GetTotalDamage();
            double seconds = sessionTimer.Elapsed.TotalSeconds;
            return (float)(totalDamage / seconds);
        }

        /// <summary>
        /// 获取最高单次伤害
        /// </summary>
        public float GetMaxDamage()
        {
            return damageHistory.Count > 0 ? damageHistory.Max(d => d.Damage) : 0;
        }

        /// <summary>
        /// 获取平均单次伤害
        /// </summary>
        public float GetAverageDamage()
        {
            return damageHistory.Count > 0 ? damageHistory.Average(d => d.Damage) : 0;
        }

        /// <summary>
        /// 获取伤害分布（按技能）
        /// </summary>
        public Dictionary<string, float> GetDamageBySkill()
        {
            return damageHistory
                .GroupBy(d => d.SkillName)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Damage));
        }

        /// <summary>
        /// 获取伤害分布（按目标）
        /// </summary>
        public Dictionary<string, float> GetDamageByTarget()
        {
            return damageHistory
                .GroupBy(d => d.TargetName)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Damage));
        }

        /// <summary>
        /// 获取运行时间
        /// </summary>
        public TimeSpan GetRuntime()
        {
            return sessionTimer.Elapsed;
        }

        /// <summary>
        /// 生成 DPS 报告
        /// </summary>
        public string GenerateReport()
        {
            try
            {
                var runtime = GetRuntime();
                var totalDamage = GetTotalDamage();
                var dps = GetDps();
                var maxDamage = GetMaxDamage();
                var avgDamage = GetAverageDamage();
                var skillDamage = GetDamageBySkill();

                string report = $@"
╔══════════════════════════════════════════════════════════════╗
║                    DPS 统计报告                              ║
╠══════════════════════════════════════════════════════════════╣
║ 运行时间:   {runtime:hh\:mm\:ss}
║ 总伤害:     {totalDamage:F0}
║ DPS:        {dps:F2} (每秒)
║ 最高伤害:   {maxDamage:F0}
║ 平均伤害:   {avgDamage:F2}
║ 伤害次数:   {damageHistory.Count}
╠══════════════════════════════════════════════════════════════╣
║ 按技能统计:
";
                foreach (var skill in skillDamage.OrderByDescending(s => s.Value))
                {
                    var skillRecords = damageHistory.Where(d => d.SkillName == skill.Key).ToList();
                    float skillDps = (float)(skill.Value / runtime.TotalSeconds);
                    report += $"║   {skill.Key,-20} {skill.Value,10:F0} ({skillRecords.Count,3} 次) DPS: {skillDps,7:F2}\n";
                }

                report += "╚══════════════════════════════════════════════════════════════╝";
                return report;
            }
            catch { return "[错误] 生成 DPS 报告失败"; }
        }

        /// <summary>
        /// 停止计算
        /// </summary>
        public void Stop()
        {
            sessionTimer.Stop();
            Logger.Log($"[DPS] DPS 计算已停止 (总伤害: {GetTotalDamage():F0}, DPS: {GetDps():F2})");
        }
    }
}
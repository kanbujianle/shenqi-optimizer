using System;
using System.Diagnostics;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// 战斗监控系统 - 记录战斗统计数据
    /// </summary>
    public class CombatMonitor
    {
        private int killCount = 0;
        private long startTime = 0;
        private long lastUpdateTime = 0;
        private float cpuUsage = 0;
        private float memoryUsage = 0;
        private Process currentProcess;
        private PerformanceCounter cpuCounter;

        public CombatMonitor()
        {
            try
            {
                currentProcess = Process.GetCurrentProcess();
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
            }
            catch (Exception ex)
            {
                Logger.Log($"[性能监控] 初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 启动监控
        /// </summary>
        public void Start()
        {
            startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            killCount = 0;
            Logger.Log("[监控] 战斗监控已启动");
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        public void Stop()
        {
            Logger.Log($"[监控] 战斗监控已停止 (击杀: {killCount})");
        }

        /// <summary>
        /// 增加击杀计数
        /// </summary>
        public void IncrementKillCount()
        {
            killCount++;
            Logger.Log($"[监控] 击杀数: {killCount}");
        }

        /// <summary>
        /// 获取击杀数
        /// </summary>
        public int GetKillCount() => killCount;

        /// <summary>
        /// 获取运行时间
        /// </summary>
        public TimeSpan GetRuntime()
        {
            long elapsed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime;
            return TimeSpan.FromMilliseconds(elapsed);
        }

        /// <summary>
        /// 获取 CPU 使用率
        /// </summary>
        public float GetCpuUsage()
        {
            try
            {
                cpuUsage = cpuCounter?.NextValue() ?? 0;
            }
            catch { }
            return cpuUsage;
        }

        /// <summary>
        /// 获取内存使用量 (MB)
        /// </summary>
        public float GetMemoryUsage()
        {
            try
            {
                currentProcess?.Refresh();
                memoryUsage = (currentProcess?.WorkingSet64 ?? 0) / 1024f / 1024f;
            }
            catch { }
            return memoryUsage;
        }

        /// <summary>
        /// 获取每分钟击杀数 (KPM)
        /// </summary>
        public float GetKillsPerMinute()
        {
            var runtime = GetRuntime();
            if (runtime.TotalMinutes < 0.016667) // 少于 1 秒
                return 0;
            return killCount / (float)runtime.TotalMinutes;
        }

        /// <summary>
        /// 销毁资源
        /// </summary>
        public void Dispose()
        {
            cpuCounter?.Dispose();
        }
    }
}
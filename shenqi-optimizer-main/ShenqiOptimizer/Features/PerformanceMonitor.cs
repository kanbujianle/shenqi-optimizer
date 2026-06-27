using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// 性能监控系统 - 实时监控游戏助手的性能指标
    /// </summary>
    public class PerformanceMonitor
    {
        private Process currentProcess;
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private Dictionary<string, Stopwatch> operationTimers;
        private Dictionary<string, long> operationCounts;
        private Queue<PerformanceSnapshot> performanceHistory;
        private const int MaxHistorySize = 600; // 保持最近 600 个快照 (60秒@100ms)

        public class PerformanceSnapshot
        {
            public DateTime Timestamp { get; set; }
            public float CpuUsage { get; set; }              // CPU 使用率 (%)
            public float MemoryUsage { get; set; }           // 内存使用 (MB)
            public int ThreadCount { get; set; }             // 线程数
            public float AverageCombatLoopTime { get; set; } // 平均战斗循环时间 (ms)
            public float GarbageCollections { get; set; }    // 垃圾回收次数
        }

        public PerformanceMonitor()
        {
            try
            {
                currentProcess = Process.GetCurrentProcess();
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                operationTimers = new Dictionary<string, Stopwatch>();
                operationCounts = new Dictionary<string, long>();
                performanceHistory = new Queue<PerformanceSnapshot>(MaxHistorySize);

                Logger.Log("[性能监控] 系统已初始化");
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 性能监控初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 开始计时一个操作
        /// </summary>
        public void StartOperation(string operationName)
        {
            try
            {
                if (!operationTimers.ContainsKey(operationName))
                    operationTimers[operationName] = new Stopwatch();
                else
                    operationTimers[operationName].Restart();

                operationTimers[operationName].Start();
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 启动操作计时失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 停止计时一个操作
        /// </summary>
        public long StopOperation(string operationName)
        {
            try
            {
                if (!operationTimers.ContainsKey(operationName))
                    return 0;

                var timer = operationTimers[operationName];
                timer.Stop();
                long elapsed = timer.ElapsedMilliseconds;

                // 累计操作计数
                if (!operationCounts.ContainsKey(operationName))
                    operationCounts[operationName] = 0;
                operationCounts[operationName]++;

                return elapsed;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 停止操作计时失败: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 获取操作平均执行时间
        /// </summary>
        public float GetAverageOperationTime(string operationName)
        {
            try
            {
                if (!operationTimers.ContainsKey(operationName))
                    return 0;

                var timer = operationTimers[operationName];
                long count = operationCounts.ContainsKey(operationName) ? operationCounts[operationName] : 1;

                if (count == 0) return 0;
                return timer.ElapsedMilliseconds / (float)count;
            }
            catch { return 0; }
        }

        /// <summary>
        /// 获取 CPU 使用率 (%)
        /// </summary>
        public float GetCpuUsage()
        {
            try
            {
                return cpuCounter?.NextValue() ?? 0;
            }
            catch { return 0; }
        }

        /// <summary>
        /// 获取内存使用量 (MB)
        /// </summary>
        public float GetMemoryUsage()
        {
            try
            {
                currentProcess?.Refresh();
                return (currentProcess?.WorkingSet64 ?? 0) / 1024f / 1024f;
            }
            catch { return 0; }
        }

        /// <summary>
        /// 获取线程数
        /// </summary>
        public int GetThreadCount()
        {
            try
            {
                currentProcess?.Refresh();
                return currentProcess?.Threads.Count ?? 0;
            }
            catch { return 0; }
        }

        /// <summary>
        /// 记录性能快照
        /// </summary>
        public void RecordSnapshot(float combatLoopTime)
        {
            try
            {
                var snapshot = new PerformanceSnapshot
                {
                    Timestamp = DateTime.Now,
                    CpuUsage = GetCpuUsage(),
                    MemoryUsage = GetMemoryUsage(),
                    ThreadCount = GetThreadCount(),
                    AverageCombatLoopTime = combatLoopTime,
                    GarbageCollections = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2)
                };

                if (performanceHistory.Count >= MaxHistorySize)
                    performanceHistory.Dequeue();

                performanceHistory.Enqueue(snapshot);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 记录性能快照失败: {ex.Message}");
            }
        }

        /// <summary>
        /// ��取性能历史
        /// </summary>
        public List<PerformanceSnapshot> GetPerformanceHistory()
        {
            return performanceHistory.ToList();
        }

        /// <summary>
        /// 获取平均性能指标
        /// </summary>
        public PerformanceSnapshot GetAverageMetrics()
        {
            try
            {
                if (performanceHistory.Count == 0)
                    return new PerformanceSnapshot();

                var snapshots = performanceHistory.ToList();
                return new PerformanceSnapshot
                {
                    Timestamp = DateTime.Now,
                    CpuUsage = snapshots.Average(s => s.CpuUsage),
                    MemoryUsage = snapshots.Average(s => s.MemoryUsage),
                    ThreadCount = (int)snapshots.Average(s => s.ThreadCount),
                    AverageCombatLoopTime = snapshots.Average(s => s.AverageCombatLoopTime),
                    GarbageCollections = (float)snapshots.Average(s => s.GarbageCollections)
                };
            }
            catch { return new PerformanceSnapshot(); }
        }

        /// <summary>
        /// 获取峰值性能指标
        /// </summary>
        public PerformanceSnapshot GetPeakMetrics()
        {
            try
            {
                if (performanceHistory.Count == 0)
                    return new PerformanceSnapshot();

                var snapshots = performanceHistory.ToList();
                return new PerformanceSnapshot
                {
                    Timestamp = DateTime.Now,
                    CpuUsage = snapshots.Max(s => s.CpuUsage),
                    MemoryUsage = snapshots.Max(s => s.MemoryUsage),
                    ThreadCount = snapshots.Max(s => s.ThreadCount),
                    AverageCombatLoopTime = snapshots.Max(s => s.AverageCombatLoopTime),
                    GarbageCollections = snapshots.Max(s => s.GarbageCollections)
                };
            }
            catch { return new PerformanceSnapshot(); }
        }

        /// <summary>
        /// 清空性能历史
        /// </summary>
        public void ClearHistory()
        {
            performanceHistory.Clear();
            operationCounts.Clear();
            foreach (var timer in operationTimers.Values)
                timer.Reset();
        }

        /// <summary>
        /// 生成性能报告
        /// </summary>
        public string GenerateReport()
        {
            try
            {
                var current = new PerformanceSnapshot
                {
                    CpuUsage = GetCpuUsage(),
                    MemoryUsage = GetMemoryUsage(),
                    ThreadCount = GetThreadCount()
                };

                var average = GetAverageMetrics();
                var peak = GetPeakMetrics();

                return $@"
╔══════════════════════════════════════════════════════════════╗
║                  性能监控报告                                ║
╠══════════════════════════════════════════════════════════════╣
║ 当前性能:
║   CPU 使用率: {current.CpuUsage,6:F2}%
║   内存使用量: {current.MemoryUsage,6:F0} MB
║   线程数量:   {current.ThreadCount,6} 个
╠══════════════════════════════════════════════════════════════╣
║ 平均性能 (过去 60 秒):
║   CPU 使用率: {average.CpuUsage,6:F2}%
║   内存使用量: {average.MemoryUsage,6:F0} MB
║   线程数量:   {average.ThreadCount,6} 个
║   战斗循环:   {average.AverageCombatLoopTime,6:F2} ms
╠══════════════════════════════════════════════════════════════╣
║ 峰值性能:
║   CPU 使用率: {peak.CpuUsage,6:F2}%
║   内存使用量: {peak.MemoryUsage,6:F0} MB
║   线程数量:   {peak.ThreadCount,6} 个
║   战斗循环:   {peak.AverageCombatLoopTime,6:F2} ms
╠══════════════════════════════════════════════════════════════╣
║ 操作计时:
";
                foreach (var op in operationTimers)
                {
                    var avgTime = GetAverageOperationTime(op.Key);
                    var count = operationCounts.ContainsKey(op.Key) ? operationCounts[op.Key] : 0;
                    return current + $"║   {op.Key,-30} 平均: {avgTime,7:F2}ms (共 {count} 次)\n";
                }

                return current + "╚══════════════════════════════════════════════════════════════╝";
            }
            catch { return "[错误] 生成性能报告失败"; }
        }

        /// <summary>
        /// 资源清理
        /// </summary>
        public void Dispose()
        {
            try
            {
                cpuCounter?.Dispose();
                foreach (var timer in operationTimers.Values)
                    timer?.Stop();
                performanceHistory.Clear();
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 清理资源失败: {ex.Message}");
            }
        }
    }
}
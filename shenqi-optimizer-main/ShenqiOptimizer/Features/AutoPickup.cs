using System;
using System.Collections.Generic;
using ShenqiOptimizer.Core;

namespace ShenqiOptimizer.Features
{
    /// <summary>
    /// 自动捡物功能 - 自动拾取掉落物品、智能过滤等
    /// </summary>
    public class AutoPickup
    {
        private ConfigManager configManager;
        private bool isRunning = false;
        private List<string> filterList; // 过滤列表

        public AutoPickup(ConfigManager config)
        {
            configManager = config;
            filterList = new List<string>();
            LoadFilterList();
        }

        /// <summary>
        /// 加载过滤列表
        /// </summary>
        private void LoadFilterList()
        {
            try
            {
                string filterData = configManager.GetValue("共用", "全局_过滤物品数组");
                if (!string.IsNullOrEmpty(filterData))
                {
                    string[] items = filterData.Split('|');
                    filterList.AddRange(items);
                    Logger.Log($"[捡物] 已加载 {filterList.Count} 个过滤物品");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 加载过滤列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 启动自动捡物
        /// </summary>
        public void StartAutoPickup()
        {
            isRunning = true;
            Logger.Log("[捡物] 自动捡物已启动");
        }

        /// <summary>
        /// 停止自动捡物
        /// </summary>
        public void StopAutoPickup()
        {
            isRunning = false;
            Logger.Log("[捡物] 自动捡物已停止");
        }

        /// <summary>
        /// 检查物品是否应该被捡取
        /// </summary>
        private bool ShouldPickup(string itemName)
        {
            // 检查是否在过滤列表中
            foreach (var filter in filterList)
            {
                if (itemName.Contains(filter))
                {
                    Logger.Log($"[捡物] 物品被过滤: {itemName}");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取运行状态
        /// </summary>
        public bool IsRunning() => isRunning;
    }
}
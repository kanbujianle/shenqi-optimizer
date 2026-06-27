using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShenqiOptimizer.Core
{
    /// <summary>
    /// 配置管理器 - 负责加载和管理 INI 配置文件
    /// 基于参考项目的配置格式
    /// </summary>
    public class ConfigManager
    {
        private Dictionary<string, Dictionary<string, string>> configs;
        private string configPath;

        public ConfigManager()
        {
            configs = new Dictionary<string, Dictionary<string, string>>();
            configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            InitializeConfigs();
        }

        private void InitializeConfigs()
        {
            try
            {
                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                // 加载所有 INI 配置文件
                LoadConfigFile(Path.Combine(configPath, "default.ini"));
                LoadConfigFile(Path.Combine(configPath, "patches.ini"));

                Logger.Log("[配置] 配置文件加载完成");
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 配置初始化失败: {ex.Message}");
            }
        }

        public void LoadConfigFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Logger.Log($"[警告] 配置文件不存在: {filePath}");
                    return;
                }

                string[] lines = File.ReadAllLines(filePath);
                string currentSection = "";

                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();

                    // 跳过空行和注释
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                        continue;

                    // 识别 Section [xxx]
                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                        if (!configs.ContainsKey(currentSection))
                            configs[currentSection] = new Dictionary<string, string>();
                    }
                    // 解析键值对
                    else if (trimmedLine.Contains("=") && !string.IsNullOrEmpty(currentSection))
                    {
                        var parts = trimmedLine.Split('=', 2);
                        string key = parts[0].Trim();
                        string value = parts.Length > 1 ? parts[1].Trim() : "";
                        configs[currentSection][key] = value;
                    }
                }

                Logger.Log($"[配置] 已加载配置文件: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 加载配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取配置值
        /// </summary>
        public string GetValue(string section, string key, string defaultValue = "")
        {
            try
            {
                if (configs.ContainsKey(section) && configs[section].ContainsKey(key))
                    return configs[section][key];
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 获取配置值失败: {ex.Message}");
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取整数配置值
        /// </summary>
        public int GetIntValue(string section, string key, int defaultValue = 0)
        {
            try
            {
                string value = GetValue(section, key);
                if (int.TryParse(value, out int result))
                    return result;
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 获取整数配置值失败: {ex.Message}");
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取布尔配置值
        /// </summary>
        public bool GetBoolValue(string section, string key, bool defaultValue = false)
        {
            try
            {
                string value = GetValue(section, key).ToLower();
                return value == "true" || value == "是" || value == "1" || value == "yes";
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 获取布尔配置值失败: {ex.Message}");
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取数组配置值
        /// </summary>
        public string[] GetArrayValue(string section, string key, char separator = '|')
        {
            try
            {
                string value = GetValue(section, key);
                if (!string.IsNullOrEmpty(value))
                    return value.Split(separator);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 获取数组配置值失败: {ex.Message}");
            }
            return new string[0];
        }

        /// <summary>
        /// 获取所有 Section
        /// </summary>
        public IEnumerable<string> GetSections()
        {
            return configs.Keys;
        }

        /// <summary>
        /// 获取指定 Section 的所有键
        /// </summary>
        public IEnumerable<string> GetKeys(string section)
        {
            if (configs.ContainsKey(section))
                return configs[section].Keys;
            return new List<string>();
        }
    }
}
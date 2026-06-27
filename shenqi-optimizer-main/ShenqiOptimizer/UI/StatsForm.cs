using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ShenqiOptimizer.Core;
using ShenqiOptimizer.Features;

namespace ShenqiOptimizer.UI
{
    public partial class StatsForm : Form
    {
        private PerformanceMonitor performanceMonitor;
        private DpsCalculator dpsCalculator;
        private RewardCalculator rewardCalculator;
        private System.Windows.Forms.Timer refreshTimer;

        public StatsForm()
        {
            InitializeComponent();
            performanceMonitor = new PerformanceMonitor();
            dpsCalculator = new DpsCalculator();
            rewardCalculator = new RewardCalculator();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "性能统计 - Shenqi Optimizer";
            this.Width = 1000;
            this.Height = 700;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.ForeColor = System.Drawing.Color.White;
            this.Font = new System.Drawing.Font("微软雅黑", 10);

            // 创建选项卡
            TabControl tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(45, 45, 48),
                ForeColor = System.Drawing.Color.White
            };

            // 性能选项卡
            TabPage performanceTab = new TabPage("📊 性能监控")
            {
                BackColor = System.Drawing.Color.FromArgb(30, 30, 30)
            };
            var perfPanel = CreatePerformancePanel();
            performanceTab.Controls.Add(perfPanel);
            tabControl.TabPages.Add(performanceTab);

            // DPS 选项卡
            TabPage dpsTab = new TabPage("⚔️ DPS 统计")
            {
                BackColor = System.Drawing.Color.FromArgb(30, 30, 30)
            };
            var dpsPanel = CreateDpsPanel();
            dpsTab.Controls.Add(dpsPanel);
            tabControl.TabPages.Add(dpsTab);

            // 奖励选项卡
            TabPage rewardTab = new TabPage("🎁 奖励速率")
            {
                BackColor = System.Drawing.Color.FromArgb(30, 30, 30)
            };
            var rewardPanel = CreateRewardPanel();
            rewardTab.Controls.Add(rewardPanel);
            tabControl.TabPages.Add(rewardTab);

            this.Controls.Add(tabControl);

            // 启动刷新计时器
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 500; // 500ms 刷新一次
            refreshTimer.Tick += (s, e) => RefreshStats();
            refreshTimer.Start();
        }

        private Control CreatePerformancePanel()
        {
            TextBox txtPerf = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Courier New", 9),
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.LimeGreen,
                Tag = "PerfText"
            };
            return txtPerf;
        }

        private Control CreateDpsPanel()
        {
            TextBox txtDps = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Courier New", 9),
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.Cyan,
                Tag = "DpsText"
            };
            return txtDps;
        }

        private Control CreateRewardPanel()
        {
            TextBox txtReward = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Courier New", 9),
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.Yellow,
                Tag = "RewardText"
            };
            return txtReward;
        }

        private void RefreshStats()
        {
            try
            {
                // 刷新性能面板
                var perfTexts = this.Controls.OfType<TabControl>()
                    .SelectMany(tc => tc.TabPages.OfType<TabPage>())
                    .SelectMany(tp => tp.Controls.OfType<TextBox>())
                    .Where(tb => tb.Tag?.ToString() == "PerfText");

                foreach (var txt in perfTexts)
                {
                    txt.Text = performanceMonitor.GenerateReport();
                }

                // 刷新 DPS 面板
                var dpsTexts = this.Controls.OfType<TabControl>()
                    .SelectMany(tc => tc.TabPages.OfType<TabPage>())
                    .SelectMany(tp => tp.Controls.OfType<TextBox>())
                    .Where(tb => tb.Tag?.ToString() == "DpsText");

                foreach (var txt in dpsTexts)
                {
                    txt.Text = dpsCalculator.GenerateReport();
                }

                // 刷新奖励面板
                var rewardTexts = this.Controls.OfType<TabControl>()
                    .SelectMany(tc => tc.TabPages.OfType<TabPage>())
                    .SelectMany(tp => tp.Controls.OfType<TextBox>())
                    .Where(tb => tb.Tag?.ToString() == "RewardText");

                foreach (var txt in rewardTexts)
                {
                    txt.Text = rewardCalculator.GenerateReport();
                }
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            performanceMonitor?.Dispose();
            dpsCalculator?.Stop();
            rewardCalculator?.Stop();
            base.OnFormClosing(e);
        }
    }
}
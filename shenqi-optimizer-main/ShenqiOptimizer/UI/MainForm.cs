using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using ShenqiOptimizer.Core;
using ShenqiOptimizer.Features;

namespace ShenqiOptimizer.UI
{
    public partial class MainForm : Form
    {
        private GameManager gameManager;
        private ConfigManager configManager;
        private AutoFarmSystem autoFarmSystem;
        private CombatMonitor combatMonitor;
        private bool isAutoFarmRunning = false;
        private System.Windows.Forms.Timer updateTimer;

        public MainForm()
        {
            InitializeComponent();
            InitializeGameSystem();
        }

        private void InitializeGameSystem()
        {
            try
            {
                configManager = new ConfigManager();
                gameManager = new GameManager(configManager);
                combatMonitor = new CombatMonitor();

                Logger.Log("[UI] 游戏系统已初始化");
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 初始化失败: {ex.Message}");
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.Text = "神泣优化助手 - Shenqi Optimizer v1.0.0";
            this.Width = 1200;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new System.Drawing.Font("微软雅黑", 10);
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.ForeColor = System.Drawing.Color.White;

            // 创建菜单栏
            CreateMenuBar();

            // 创建主容器
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60f));

            // 左侧：功能面板
            mainLayout.Controls.Add(CreateFunctionPanel(), 0, 0);

            // 中间：战斗日志
            mainLayout.Controls.Add(CreateCombatLogPanel(), 1, 0);

            // 右侧：实时信息
            mainLayout.Controls.Add(CreateInfoPanel(), 2, 0);

            // 底部：状态栏
            mainLayout.Controls.Add(CreateStatusBar(), 0, 1);
            mainLayout.SetColumnSpan(mainLayout.Controls[mainLayout.Controls.Count - 1], 3);

            this.Controls.Add(mainLayout);

            // 启动定时器更新 UI
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 100; // 100ms 更新一次
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private void CreateMenuBar()
        {
            MenuStrip menuStrip = new MenuStrip
            {
                BackColor = System.Drawing.Color.FromArgb(45, 45, 48),
                ForeColor = System.Drawing.Color.White
            };

            // 文件菜单
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("文件(&F)");
            fileMenu.DropDownItems.Add("退出(&X)", null, (s, e) => this.Close());
            menuStrip.Items.Add(fileMenu);

            // 帮助菜单
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("帮助(&H)");
            helpMenu.DropDownItems.Add("关于(&A)", null, (s, e) => ShowAbout());
            menuStrip.Items.Add(helpMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private Control CreateFunctionPanel()
        {
            GroupBox gbFunctions = new GroupBox
            {
                Text = "🎮 功能控制",
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
                ForeColor = System.Drawing.Color.White,
                Padding = new Padding(10)
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7,
                Padding = new Padding(5)
            };

            // 自动打怪按钮
            Button btnAutoFarm = CreateStyledButton("▶ 开启自动打怪", (s, e) => BtnAutoFarm_Click());
            Button btnStopFarm = CreateStyledButton("⏹ 停止打怪", (s, e) => BtnStopFarm_Click());
            btnStopFarm.BackColor = System.Drawing.Color.FromArgb(192, 57, 43);

            Button btnAutoPickup = CreateStyledButton("📦 自动捡物", (s, e) => BtnAutoPickup_Click());
            Button btnAutoSell = CreateStyledButton("💰 自动售出", (s, e) => BtnAutoSell_Click());
            Button btnBuffManager = CreateStyledButton("🛡️ BUFF管理", (s, e) => BtnBuffManager_Click());
            Button btnSettings = CreateStyledButton("⚙️ 设置", (s, e) => BtnSettings_Click());
            Button btnClear = CreateStyledButton("🗑️ 清空日志", (s, e) => BtnClear_Click());

            layout.Controls.Add(btnAutoFarm, 0, 0);
            layout.Controls.Add(btnStopFarm, 0, 1);
            layout.Controls.Add(btnAutoPickup, 0, 2);
            layout.Controls.Add(btnAutoSell, 0, 3);
            layout.Controls.Add(btnBuffManager, 0, 4);
            layout.Controls.Add(btnSettings, 0, 5);
            layout.Controls.Add(btnClear, 0, 6);

            for (int i = 0; i < 7; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 14.28f));

            gbFunctions.Controls.Add(layout);
            return gbFunctions;
        }

        private Control CreateCombatLogPanel()
        {
            GroupBox gbLog = new GroupBox
            {
                Text = "📊 战斗日志",
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
                ForeColor = System.Drawing.Color.White,
                Padding = new Padding(10)
            };

            TextBox txtLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Courier New", 9),
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.LimeGreen,
                Tag = "CombatLog"
            };

            txtLog.Text = "[系统] 神泣优化助手已启动\r\n" +
                         "[系统] 配置文件已加载\r\n" +
                         "[系统] 等待用户操作...\r\n\r\n";

            gbLog.Controls.Add(txtLog);
            return gbLog;
        }

        private Control CreateInfoPanel()
        {
            GroupBox gbInfo = new GroupBox
            {
                Text = "ℹ️ 实时信息",
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
                ForeColor = System.Drawing.Color.White,
                Padding = new Padding(10)
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8,
                Padding = new Padding(5),
                AutoSize = true
            };

            // 玩家信息
            Label lblPlayerStatus = CreateInfoLabel("👤 玩家状态", "就绪");
            lblPlayerStatus.Tag = "PlayerStatus";
            layout.Controls.Add(lblPlayerStatus, 0, 0);

            // 当前目标
            Label lblTarget = CreateInfoLabel("🎯 当前目标", "无");
            lblTarget.Tag = "CurrentTarget";
            layout.Controls.Add(lblTarget, 0, 1);

            // 目标血量
            Label lblTargetHP = CreateInfoLabel("❤️ 目标血量", "0/0");
            lblTargetHP.Tag = "TargetHP";
            layout.Controls.Add(lblTargetHP, 0, 2);

            // 当前技能
            Label lblSkill = CreateInfoLabel("⚡ 当前技能", "无");
            lblSkill.Tag = "CurrentSkill";
            layout.Controls.Add(lblSkill, 0, 3);

            // 战斗状态
            Label lblCombatStatus = CreateInfoLabel("⚔️ 战斗状态", "非战斗状态");
            lblCombatStatus.Tag = "CombatStatus";
            layout.Controls.Add(lblCombatStatus, 0, 4);

            // 击杀数
            Label lblKillCount = CreateInfoLabel("💀 击杀数", "0");
            lblKillCount.Tag = "KillCount";
            layout.Controls.Add(lblKillCount, 0, 5);

            // 运行时间
            Label lblRuntime = CreateInfoLabel("⏱️ 运行时间", "00:00:00");
            lblRuntime.Tag = "Runtime";
            layout.Controls.Add(lblRuntime, 0, 6);

            // 性能信息
            Label lblPerformance = CreateInfoLabel("📈 性能", "CPU: 0% | 内存: 0MB");
            lblPerformance.Tag = "Performance";
            layout.Controls.Add(lblPerformance, 0, 7);

            for (int i = 0; i < 8; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5f));

            gbInfo.Controls.Add(layout);
            return gbInfo;
        }

        private Control CreateStatusBar()
        {
            Panel statusBar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(45, 45, 48),
                Height = 50,
                Padding = new Padding(10)
            };

            Label lblStatus = new Label
            {
                Text = "状态: 就绪",
                Dock = DockStyle.Left,
                ForeColor = System.Drawing.Color.White,
                AutoSize = true,
                Tag = "StatusText"
            };

            Label lblVersion = new Label
            {
                Text = "v1.0.0 | 神泣优化助手",
                Dock = DockStyle.Right,
                ForeColor = System.Drawing.Color.Gray,
                AutoSize = true
            };

            statusBar.Controls.Add(lblStatus);
            statusBar.Controls.Add(lblVersion);

            return statusBar;
        }

        private Button CreateStyledButton(string text, EventHandler clickHandler)
        {
            Button btn = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("微软雅黑", 11, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.FromArgb(46, 125, 50),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(2)
            };
            btn.Click += clickHandler;
            return btn;
        }

        private Label CreateInfoLabel(string title, string value)
        {
            Label label = new Label
            {
                Text = $"{title}: {value}",
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 25,
                ForeColor = System.Drawing.Color.LimeGreen,
                Font = new System.Drawing.Font("Courier New", 10)
            };
            return label;
        }

        // 按钮事件处理
        private void BtnAutoFarm_Click()
        {
            if (isAutoFarmRunning)
            {
                MessageBox.Show("自动打怪已在运行中", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                autoFarmSystem = new AutoFarmSystem(configManager, gameManager.GetGameMemory() ?? new GameMemory());
                isAutoFarmRunning = true;
                combatMonitor.Start();

                _ = autoFarmSystem.Start(); // 异步启动

                UpdateLog("[系统] 自动打怪已启动");
                UpdateStatus("正在自动打怪...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStopFarm_Click()
        {
            if (!isAutoFarmRunning || autoFarmSystem == null)
            {
                MessageBox.Show("自动打怪未运行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            autoFarmSystem.Stop();
            isAutoFarmRunning = false;
            combatMonitor.Stop();

            UpdateLog("[系统] 自动打怪已停止");
            UpdateStatus("就绪");
        }

        private void BtnAutoPickup_Click()
        {
            MessageBox.Show("自动捡物功能开发中...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAutoSell_Click()
        {
            MessageBox.Show("自动售出功能开发中...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnBuffManager_Click()
        {
            MessageBox.Show("BUFF管理功能开发中...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSettings_Click()
        {
            MessageBox.Show("设置面板开发中...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnClear_Click()
        {
            var logControls = this.Controls.OfType<GroupBox>()
                .SelectMany(gb => gb.Controls.OfType<TextBox>())
                .Where(tb => tb.Tag?.ToString() == "CombatLog");

            foreach (var log in logControls)
            {
                log.Clear();
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (isAutoFarmRunning && autoFarmSystem != null)
            {
                UpdateUIWithCombatData();
            }
        }

        private void UpdateUIWithCombatData()
        {
            try
            {
                // 更新玩家信息
                var playerStatus = autoFarmSystem.GetPlayerStatus();
                var currentTarget = autoFarmSystem.GetCurrentTarget();

                // 更新当前目标
                UpdateLabel("CurrentTarget", currentTarget?.MonsterName ?? "无");

                // 更新目标血量
                if (currentTarget != null)
                {
                    UpdateLabel("TargetHP", $"{currentTarget.Health}/{currentTarget.MaxHealth} ({(currentTarget.Health / (float)currentTarget.MaxHealth * 100):F1}%)");
                }

                // 更新战斗状态
                UpdateLabel("CombatStatus", autoFarmSystem.GetCombatEngine().IsInCombat() ? "战斗中" : "等待中");

                // 更新击杀数
                UpdateLabel("KillCount", combatMonitor.GetKillCount().ToString());

                // 更新运行时间
                UpdateLabel("Runtime", combatMonitor.GetRuntime().ToString(@"hh\:mm\:ss"));

                // 更新性能信息
                UpdateLabel("Performance", $"CPU: {combatMonitor.GetCpuUsage():F1}% | 内存: {combatMonitor.GetMemoryUsage():F0}MB");
            }
            catch (Exception ex)
            {
                Logger.Log($"[UI更新错误] {ex.Message}");
            }
        }

        private void UpdateLabel(string tag, string value)
        {
            var labels = this.Controls.OfType<GroupBox>()
                .SelectMany(gb => gb.Controls.OfType<TableLayoutPanel>())
                .SelectMany(tlp => tlp.Controls.OfType<Label>())
                .Where(l => l.Tag?.ToString() == tag);

            foreach (var label in labels)
            {
                label.Text = label.Text.Substring(0, label.Text.IndexOf(':') + 2) + value;
            }
        }

        private void UpdateLog(string message)
        {
            var logControls = this.Controls.OfType<GroupBox>()
                .SelectMany(gb => gb.Controls.OfType<TextBox>())
                .Where(tb => tb.Tag?.ToString() == "CombatLog");

            foreach (var log in logControls)
            {
                log.AppendText($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}\r\n");
            }
        }

        private void UpdateStatus(string status)
        {
            var statusLabels = this.Controls.OfType<Panel>()
                .SelectMany(p => p.Controls.OfType<Label>())
                .Where(l => l.Tag?.ToString() == "StatusText");

            foreach (var label in statusLabels)
            {
                label.Text = $"状态: {status}";
            }
        }

        private void ShowAbout()
        {
            MessageBox.Show(
                "神泣优化助手 v1.0.0\r\n\r\n" +
                "功能: 自动打怪、自动传送、自动捡物等\r\n" +
                "作者: Copilot\r\n" +
                "免责声明: 仅供学习研究使用\r\n",
                "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
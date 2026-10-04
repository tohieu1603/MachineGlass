using AF.Common;
using AF.Data;

namespace Autoframe.UI
{
    partial class Form_Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Main));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_StackLight = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_State = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_SafetyDoor = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripDropDownButton_Speed = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_SpeedLow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_SpeedMiddle = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_SpeedHigh = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton_Process = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem_Recipe = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_AdjustMode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_Produce = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_DryRun = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Calib = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_GRR = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Others = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList_Button = new System.Windows.Forms.ImageList(this.components);
            this.imageList_Hive = new System.Windows.Forms.ImageList(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.userControl_Notice1 = new AF.Common.UI.UserControl_Message();
            this.imageList_User = new System.Windows.Forms.ImageList(this.components);
            this.panel_main = new System.Windows.Forms.Panel();
            this.tableLayoutPanel_Main = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.RoundButton_User = new AF.Common.UI.RoundButton();
            this.RoundButton_ProjectName = new AF.Common.UI.RoundButton();
            this.RoundButton_Stop = new AF.Common.UI.RoundButton();
            this.RoundButton_Manual = new AF.Common.UI.RoundButton();
            this.RoundButton_Pause = new AF.Common.UI.RoundButton();
            this.RoundButton_Data = new AF.Common.UI.RoundButton();
            this.RoundButton_Start = new AF.Common.UI.RoundButton();
            this.RoundButton_Setting = new AF.Common.UI.RoundButton();
            this.RoundButton_Alarm = new AF.Common.UI.RoundButton();
            this.RoundButton_Auto = new AF.Common.UI.RoundButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox_Quiet = new System.Windows.Forms.PictureBox();
            this.ucChooseLan1 = new AF.Data.Language.ucc.ucChooseLan();
            this.statusStrip1.SuspendLayout();
            this.tableLayoutPanel_Main.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Quiet)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.Transparent;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_StackLight,
            this.toolStripStatusLabel_State,
            this.toolStripStatusLabel_SafetyDoor,
            this.toolStripDropDownButton_Speed,
            this.toolStripDropDownButton_Process});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 1023);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 23, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1540, 25);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_StackLight
            // 
            this.toolStripStatusLabel_StackLight.ActiveLinkColor = System.Drawing.Color.Black;
            this.toolStripStatusLabel_StackLight.AutoSize = false;
            this.toolStripStatusLabel_StackLight.BackColor = System.Drawing.Color.DarkRed;
            this.toolStripStatusLabel_StackLight.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabel_StackLight.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripStatusLabel_StackLight.ForeColor = System.Drawing.SystemColors.Info;
            this.toolStripStatusLabel_StackLight.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripStatusLabel_StackLight.Name = "toolStripStatusLabel_StackLight";
            this.toolStripStatusLabel_StackLight.Size = new System.Drawing.Size(130, 25);
            this.toolStripStatusLabel_StackLight.Text = "急停";
            this.toolStripStatusLabel_StackLight.Click += new System.EventHandler(this.toolStripStatusLabel_StackLight_Click);
            // 
            // toolStripStatusLabel_State
            // 
            this.toolStripStatusLabel_State.AutoSize = false;
            this.toolStripStatusLabel_State.BackColor = System.Drawing.Color.DarkGreen;
            this.toolStripStatusLabel_State.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabel_State.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripStatusLabel_State.ForeColor = System.Drawing.Color.White;
            this.toolStripStatusLabel_State.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripStatusLabel_State.Name = "toolStripStatusLabel_State";
            this.toolStripStatusLabel_State.Size = new System.Drawing.Size(150, 25);
            this.toolStripStatusLabel_State.Text = " 状态: 手动 ";
            // 
            // toolStripStatusLabel_SafetyDoor
            // 
            this.toolStripStatusLabel_SafetyDoor.ActiveLinkColor = System.Drawing.Color.Black;
            this.toolStripStatusLabel_SafetyDoor.AutoSize = false;
            this.toolStripStatusLabel_SafetyDoor.BackColor = System.Drawing.Color.DarkGreen;
            this.toolStripStatusLabel_SafetyDoor.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabel_SafetyDoor.DoubleClickEnabled = true;
            this.toolStripStatusLabel_SafetyDoor.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripStatusLabel_SafetyDoor.ForeColor = System.Drawing.Color.White;
            this.toolStripStatusLabel_SafetyDoor.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripStatusLabel_SafetyDoor.Name = "toolStripStatusLabel_SafetyDoor";
            this.toolStripStatusLabel_SafetyDoor.Size = new System.Drawing.Size(130, 25);
            this.toolStripStatusLabel_SafetyDoor.Text = "安全门";
            this.toolStripStatusLabel_SafetyDoor.Click += new System.EventHandler(this.toolStripStatusLabel_SafetyDoor_Click);
            this.toolStripStatusLabel_SafetyDoor.DoubleClick += new System.EventHandler(this.toolStripStatusLabel_SafetyDoor_DoubleClick);
            // 
            // toolStripDropDownButton_Speed
            // 
            this.toolStripDropDownButton_Speed.AutoSize = false;
            this.toolStripDropDownButton_Speed.BackColor = System.Drawing.Color.DarkGreen;
            this.toolStripDropDownButton_Speed.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_Speed.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.toolStripMenuItem_SpeedLow,
            this.toolStripMenuItem_SpeedMiddle,
            this.toolStripMenuItem_SpeedHigh});
            this.toolStripDropDownButton_Speed.Font = new System.Drawing.Font("宋体", 10F);
            this.toolStripDropDownButton_Speed.ForeColor = System.Drawing.SystemColors.Control;
            this.toolStripDropDownButton_Speed.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_Speed.Image")));
            this.toolStripDropDownButton_Speed.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_Speed.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripDropDownButton_Speed.Name = "toolStripDropDownButton_Speed";
            this.toolStripDropDownButton_Speed.Size = new System.Drawing.Size(150, 25);
            this.toolStripDropDownButton_Speed.Text = "速度";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(313, 6);
            // 
            // toolStripMenuItem_SpeedLow
            // 
            this.toolStripMenuItem_SpeedLow.Font = new System.Drawing.Font("宋体", 20F);
            this.toolStripMenuItem_SpeedLow.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.toolStripMenuItem_SpeedLow.Name = "toolStripMenuItem_SpeedLow";
            this.toolStripMenuItem_SpeedLow.Size = new System.Drawing.Size(316, 48);
            this.toolStripMenuItem_SpeedLow.Text = "   低速   ";
            this.toolStripMenuItem_SpeedLow.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.toolStripMenuItem_SpeedLow.Click += new System.EventHandler(this.toolStripMenuItem_Speed_Click);
            // 
            // toolStripMenuItem_SpeedMiddle
            // 
            this.toolStripMenuItem_SpeedMiddle.Font = new System.Drawing.Font("宋体", 20F);
            this.toolStripMenuItem_SpeedMiddle.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.toolStripMenuItem_SpeedMiddle.Name = "toolStripMenuItem_SpeedMiddle";
            this.toolStripMenuItem_SpeedMiddle.Size = new System.Drawing.Size(316, 48);
            this.toolStripMenuItem_SpeedMiddle.Text = "   中速   ";
            this.toolStripMenuItem_SpeedMiddle.Click += new System.EventHandler(this.toolStripMenuItem_Speed_Click);
            // 
            // toolStripMenuItem_SpeedHigh
            // 
            this.toolStripMenuItem_SpeedHigh.Font = new System.Drawing.Font("宋体", 20F);
            this.toolStripMenuItem_SpeedHigh.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.toolStripMenuItem_SpeedHigh.Name = "toolStripMenuItem_SpeedHigh";
            this.toolStripMenuItem_SpeedHigh.Size = new System.Drawing.Size(316, 48);
            this.toolStripMenuItem_SpeedHigh.Text = "   高速   ";
            this.toolStripMenuItem_SpeedHigh.Click += new System.EventHandler(this.toolStripMenuItem_Speed_Click);
            // 
            // toolStripDropDownButton_Process
            // 
            this.toolStripDropDownButton_Process.AutoSize = false;
            this.toolStripDropDownButton_Process.BackColor = System.Drawing.Color.DarkGreen;
            this.toolStripDropDownButton_Process.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_Process.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_Recipe,
            this.toolStripSeparator2,
            this.toolStripMenuItem_Produce,
            this.toolStripMenuItem_DryRun,
            this.toolStripMenuItem_Calib,
            this.toolStripMenuItem_GRR,
            this.toolStripMenuItem_Others});
            this.toolStripDropDownButton_Process.Font = new System.Drawing.Font("宋体", 10F);
            this.toolStripDropDownButton_Process.ForeColor = System.Drawing.SystemColors.Control;
            this.toolStripDropDownButton_Process.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_Process.Image")));
            this.toolStripDropDownButton_Process.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_Process.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripDropDownButton_Process.Name = "toolStripDropDownButton_Process";
            this.toolStripDropDownButton_Process.Size = new System.Drawing.Size(160, 25);
            this.toolStripDropDownButton_Process.Text = "流程：生产";
            this.toolStripDropDownButton_Process.DropDownOpening += new System.EventHandler(this.toolStripDropDownButton_Process_DropDownOpening);
            // 
            // toolStripMenuItem_Recipe
            // 
            this.toolStripMenuItem_Recipe.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_AdjustMode,
            this.toolStripSeparator3});
            this.toolStripMenuItem_Recipe.Font = new System.Drawing.Font("宋体", 20F);
            this.toolStripMenuItem_Recipe.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.toolStripMenuItem_Recipe.Name = "toolStripMenuItem_Recipe";
            this.toolStripMenuItem_Recipe.Size = new System.Drawing.Size(276, 48);
            this.toolStripMenuItem_Recipe.Text = "[ 配方 ]";
            this.toolStripMenuItem_Recipe.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            // 
            // toolStripMenuItem_AdjustMode
            // 
            this.toolStripMenuItem_AdjustMode.Name = "toolStripMenuItem_AdjustMode";
            this.toolStripMenuItem_AdjustMode.Size = new System.Drawing.Size(284, 48);
            this.toolStripMenuItem_AdjustMode.Text = "调试模式";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(281, 6);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(273, 6);
            // 
            // toolStripMenuItem_Produce
            // 
            this.toolStripMenuItem_Produce.Font = new System.Drawing.Font("宋体", 20F);
            this.toolStripMenuItem_Produce.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.toolStripMenuItem_Produce.Name = "toolStripMenuItem_Produce";
            this.toolStripMenuItem_Produce.Size = new System.Drawing.Size(276, 48);
            this.toolStripMenuItem_Produce.Text = "  生产";
            this.toolStripMenuItem_Produce.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            // 
            // toolStripMenuItem_DryRun
            // 
            this.toolStripMenuItem_DryRun.Font = new System.Drawing.Font("宋体", 20F);
            this.toolStripMenuItem_DryRun.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.toolStripMenuItem_DryRun.Name = "toolStripMenuItem_DryRun";
            this.toolStripMenuItem_DryRun.Size = new System.Drawing.Size(276, 48);
            this.toolStripMenuItem_DryRun.Text = "  空跑";
            this.toolStripMenuItem_DryRun.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            // 
            // toolStripMenuItem_Calib
            // 
            this.toolStripMenuItem_Calib.Font = new System.Drawing.Font("宋体", 20F);
            this.toolStripMenuItem_Calib.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.toolStripMenuItem_Calib.Name = "toolStripMenuItem_Calib";
            this.toolStripMenuItem_Calib.Size = new System.Drawing.Size(276, 48);
            this.toolStripMenuItem_Calib.Text = "  标定";
            this.toolStripMenuItem_Calib.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            // 
            // toolStripMenuItem_GRR
            // 
            this.toolStripMenuItem_GRR.Font = new System.Drawing.Font("宋体", 20F);
            this.toolStripMenuItem_GRR.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.toolStripMenuItem_GRR.Name = "toolStripMenuItem_GRR";
            this.toolStripMenuItem_GRR.Size = new System.Drawing.Size(276, 48);
            this.toolStripMenuItem_GRR.Text = "  GRR";
            this.toolStripMenuItem_GRR.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            // 
            // toolStripMenuItem_Others
            // 
            this.toolStripMenuItem_Others.Font = new System.Drawing.Font("宋体", 20F);
            this.toolStripMenuItem_Others.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.toolStripMenuItem_Others.Name = "toolStripMenuItem_Others";
            this.toolStripMenuItem_Others.Size = new System.Drawing.Size(276, 48);
            this.toolStripMenuItem_Others.Text = "  其他";
            this.toolStripMenuItem_Others.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            // 
            // imageList_Button
            // 
            this.imageList_Button.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_Button.ImageStream")));
            this.imageList_Button.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_Button.Images.SetKeyName(0, "Home_Energized.png");
            this.imageList_Button.Images.SetKeyName(1, "Home_De_energized.png");
            this.imageList_Button.Images.SetKeyName(2, "Home_Disabled.png");
            this.imageList_Button.Images.SetKeyName(3, "Alarm_Energized.png");
            this.imageList_Button.Images.SetKeyName(4, "Alarm_De_energized.png");
            this.imageList_Button.Images.SetKeyName(5, "Alarm_Disabled.png");
            this.imageList_Button.Images.SetKeyName(6, "Alarm_Alerted.png");
            this.imageList_Button.Images.SetKeyName(7, "Config_Energized.png");
            this.imageList_Button.Images.SetKeyName(8, "Config_De_energized.png");
            this.imageList_Button.Images.SetKeyName(9, "Config_Disabled.png");
            this.imageList_Button.Images.SetKeyName(10, "Data_Energized.png");
            this.imageList_Button.Images.SetKeyName(11, "Data_De_energized.png");
            this.imageList_Button.Images.SetKeyName(12, "Data_Disabled.png");
            this.imageList_Button.Images.SetKeyName(13, "Vision_Energized.png");
            this.imageList_Button.Images.SetKeyName(14, "Vision_De_energized.png");
            this.imageList_Button.Images.SetKeyName(15, "Vision_Disabled.png");
            this.imageList_Button.Images.SetKeyName(16, "Setting_Energized.png");
            this.imageList_Button.Images.SetKeyName(17, "Setting_De_energized.png");
            this.imageList_Button.Images.SetKeyName(18, "Setting_Disabled.png");
            this.imageList_Button.Images.SetKeyName(19, "Running_Energized.png");
            this.imageList_Button.Images.SetKeyName(20, "Running_De_energized.png");
            this.imageList_Button.Images.SetKeyName(21, "Running_Disabled.png");
            this.imageList_Button.Images.SetKeyName(22, "Paused_Energized.png");
            this.imageList_Button.Images.SetKeyName(23, "Paused_De_energized.png");
            this.imageList_Button.Images.SetKeyName(24, "Paused_Disabled.png");
            this.imageList_Button.Images.SetKeyName(25, "Stopped_Energized.png");
            this.imageList_Button.Images.SetKeyName(26, "Stopped_De_energized.png");
            this.imageList_Button.Images.SetKeyName(27, "Stopped_Disabled.png");
            // 
            // imageList_Hive
            // 
            this.imageList_Hive.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_Hive.ImageStream")));
            this.imageList_Hive.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_Hive.Images.SetKeyName(0, "HIVE_Disabled.png");
            this.imageList_Hive.Images.SetKeyName(1, "HIVE_Running.png");
            this.imageList_Hive.Images.SetKeyName(2, "HIVE_Idle.png");
            this.imageList_Hive.Images.SetKeyName(3, "HIVE_Engineering.png");
            this.imageList_Hive.Images.SetKeyName(4, "HIVE_Plan_DT.png");
            this.imageList_Hive.Images.SetKeyName(5, "HIVE_DT.png");
            this.imageList_Hive.Images.SetKeyName(6, "No_login.png");
            this.imageList_Hive.Images.SetKeyName(7, "Level1.png");
            this.imageList_Hive.Images.SetKeyName(8, "Level2.png");
            this.imageList_Hive.Images.SetKeyName(9, "Level3.png");
            this.imageList_Hive.Images.SetKeyName(10, "Normal(E_M).png");
            this.imageList_Hive.Images.SetKeyName(11, "E-SKU.png");
            this.imageList_Hive.Images.SetKeyName(12, "M-SKU.png");
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // userControl_Notice1
            // 
            this.userControl_Notice1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userControl_Notice1.Location = new System.Drawing.Point(4, 94);
            this.userControl_Notice1.Margin = new System.Windows.Forms.Padding(4);
            this.userControl_Notice1.Name = "userControl_Notice1";
            this.userControl_Notice1.Size = new System.Drawing.Size(1532, 132);
            this.userControl_Notice1.TabIndex = 21;
            // 
            // imageList_User
            // 
            this.imageList_User.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_User.ImageStream")));
            this.imageList_User.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_User.Images.SetKeyName(0, "User.png");
            this.imageList_User.Images.SetKeyName(1, "User_OP.png");
            this.imageList_User.Images.SetKeyName(2, "User_AD.png");
            this.imageList_User.Images.SetKeyName(3, "User_ENG.png");
            // 
            // panel_main
            // 
            this.panel_main.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_main.Location = new System.Drawing.Point(0, 230);
            this.panel_main.Margin = new System.Windows.Forms.Padding(0);
            this.panel_main.Name = "panel_main";
            this.panel_main.Size = new System.Drawing.Size(1540, 793);
            this.panel_main.TabIndex = 20;
            // 
            // tableLayoutPanel_Main
            // 
            this.tableLayoutPanel_Main.ColumnCount = 1;
            this.tableLayoutPanel_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_Main.Controls.Add(this.panel_main, 0, 2);
            this.tableLayoutPanel_Main.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tableLayoutPanel_Main.Controls.Add(this.userControl_Notice1, 0, 1);
            this.tableLayoutPanel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_Main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_Main.Name = "tableLayoutPanel_Main";
            this.tableLayoutPanel_Main.RowCount = 3;
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_Main.Size = new System.Drawing.Size(1540, 1023);
            this.tableLayoutPanel_Main.TabIndex = 19;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 12;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel1.Controls.Add(this.RoundButton_User, 10, 0);
            this.tableLayoutPanel1.Controls.Add(this.RoundButton_ProjectName, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.RoundButton_Stop, 9, 0);
            this.tableLayoutPanel1.Controls.Add(this.RoundButton_Manual, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.RoundButton_Pause, 8, 0);
            this.tableLayoutPanel1.Controls.Add(this.RoundButton_Data, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.RoundButton_Start, 7, 0);
            this.tableLayoutPanel1.Controls.Add(this.RoundButton_Setting, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.RoundButton_Alarm, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.RoundButton_Auto, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 11, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1540, 90);
            this.tableLayoutPanel1.TabIndex = 17;
            // 
            // RoundButton_User
            // 
            this.RoundButton_User.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RoundButton_User.BackColor = System.Drawing.Color.Transparent;
            this.RoundButton_User.BaseColor = System.Drawing.Color.Transparent;
            this.RoundButton_User.BaseColorEnd = System.Drawing.Color.Transparent;
            this.RoundButton_User.FlatAppearance.BorderSize = 0;
            this.RoundButton_User.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RoundButton_User.ImageHeight = 75;
            this.RoundButton_User.ImageIndex = 0;
            this.RoundButton_User.ImageList = this.imageList_User;
            this.RoundButton_User.ImageWidth = 75;
            this.RoundButton_User.Location = new System.Drawing.Point(1280, 0);
            this.RoundButton_User.Margin = new System.Windows.Forms.Padding(0);
            this.RoundButton_User.Name = "RoundButton_User";
            this.RoundButton_User.nImageIndex = 0;
            this.RoundButton_User.Radius = 24;
            this.RoundButton_User.Size = new System.Drawing.Size(128, 90);
            this.RoundButton_User.SpliteButtonWidth = 18;
            this.RoundButton_User.TabIndex = 23;
            this.RoundButton_User.UseVisualStyleBackColor = false;
            this.RoundButton_User.Click += new System.EventHandler(this.roundButton_User_Click);
            // 
            // RoundButton_ProjectName
            // 
            this.RoundButton_ProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RoundButton_ProjectName.AutoSize = true;
            this.RoundButton_ProjectName.BackColor = System.Drawing.Color.Transparent;
            this.RoundButton_ProjectName.BaseColor = System.Drawing.Color.Transparent;
            this.RoundButton_ProjectName.BaseColorEnd = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.SetColumnSpan(this.RoundButton_ProjectName, 2);
            this.RoundButton_ProjectName.FlatAppearance.BorderSize = 0;
            this.RoundButton_ProjectName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RoundButton_ProjectName.Font = new System.Drawing.Font("Consolas", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RoundButton_ProjectName.ForeColor = System.Drawing.Color.DarkGray;
            this.RoundButton_ProjectName.ImageHeight = 78;
            this.RoundButton_ProjectName.ImageWidth = 248;
            this.RoundButton_ProjectName.Location = new System.Drawing.Point(640, 0);
            this.RoundButton_ProjectName.Margin = new System.Windows.Forms.Padding(0);
            this.RoundButton_ProjectName.Name = "RoundButton_ProjectName";
            this.RoundButton_ProjectName.nImageIndex = -1;
            this.RoundButton_ProjectName.Radius = 24;
            this.RoundButton_ProjectName.Size = new System.Drawing.Size(256, 90);
            this.RoundButton_ProjectName.SpliteButtonWidth = 18;
            this.RoundButton_ProjectName.TabIndex = 20;
            this.RoundButton_ProjectName.Text = "T008";
            this.RoundButton_ProjectName.UseVisualStyleBackColor = true;
            this.RoundButton_ProjectName.Click += new System.EventHandler(this.roundButton_MachineName_Click);
            // 
            // RoundButton_Stop
            // 
            this.RoundButton_Stop.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RoundButton_Stop.BackColor = System.Drawing.Color.Transparent;
            this.RoundButton_Stop.BaseColor = System.Drawing.Color.Transparent;
            this.RoundButton_Stop.BaseColorEnd = System.Drawing.Color.Transparent;
            this.RoundButton_Stop.FlatAppearance.BorderSize = 0;
            this.RoundButton_Stop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RoundButton_Stop.ImageHeight = 75;
            this.RoundButton_Stop.ImageIndex = 26;
            this.RoundButton_Stop.ImageList = this.imageList_Button;
            this.RoundButton_Stop.ImageWidth = 75;
            this.RoundButton_Stop.Location = new System.Drawing.Point(1152, 0);
            this.RoundButton_Stop.Margin = new System.Windows.Forms.Padding(0);
            this.RoundButton_Stop.Name = "RoundButton_Stop";
            this.RoundButton_Stop.nImageIndex = 26;
            this.RoundButton_Stop.Radius = 24;
            this.RoundButton_Stop.Size = new System.Drawing.Size(128, 90);
            this.RoundButton_Stop.SpliteButtonWidth = 18;
            this.RoundButton_Stop.TabIndex = 14;
            this.RoundButton_Stop.UseVisualStyleBackColor = false;
            this.RoundButton_Stop.Click += new System.EventHandler(this.RoundButton_Stop_Click);
            // 
            // RoundButton_Manual
            // 
            this.RoundButton_Manual.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RoundButton_Manual.BackColor = System.Drawing.Color.Transparent;
            this.RoundButton_Manual.BaseColor = System.Drawing.Color.Transparent;
            this.RoundButton_Manual.BaseColorEnd = System.Drawing.Color.Transparent;
            this.RoundButton_Manual.FlatAppearance.BorderSize = 0;
            this.RoundButton_Manual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RoundButton_Manual.ImageHeight = 75;
            this.RoundButton_Manual.ImageIndex = 8;
            this.RoundButton_Manual.ImageList = this.imageList_Button;
            this.RoundButton_Manual.ImageWidth = 75;
            this.RoundButton_Manual.Location = new System.Drawing.Point(256, 0);
            this.RoundButton_Manual.Margin = new System.Windows.Forms.Padding(0);
            this.RoundButton_Manual.Name = "RoundButton_Manual";
            this.RoundButton_Manual.nImageIndex = 8;
            this.RoundButton_Manual.Radius = 24;
            this.RoundButton_Manual.Size = new System.Drawing.Size(128, 90);
            this.RoundButton_Manual.SpliteButtonWidth = 18;
            this.RoundButton_Manual.TabIndex = 20;
            this.RoundButton_Manual.UseVisualStyleBackColor = true;
            this.RoundButton_Manual.Click += new System.EventHandler(this.RoundButton_Click);
            // 
            // RoundButton_Pause
            // 
            this.RoundButton_Pause.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RoundButton_Pause.BackColor = System.Drawing.Color.Transparent;
            this.RoundButton_Pause.BaseColor = System.Drawing.Color.Transparent;
            this.RoundButton_Pause.BaseColorEnd = System.Drawing.Color.Transparent;
            this.RoundButton_Pause.FlatAppearance.BorderSize = 0;
            this.RoundButton_Pause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RoundButton_Pause.ImageHeight = 75;
            this.RoundButton_Pause.ImageIndex = 23;
            this.RoundButton_Pause.ImageList = this.imageList_Button;
            this.RoundButton_Pause.ImageWidth = 75;
            this.RoundButton_Pause.Location = new System.Drawing.Point(1024, 0);
            this.RoundButton_Pause.Margin = new System.Windows.Forms.Padding(0);
            this.RoundButton_Pause.Name = "RoundButton_Pause";
            this.RoundButton_Pause.nImageIndex = 23;
            this.RoundButton_Pause.Radius = 24;
            this.RoundButton_Pause.Size = new System.Drawing.Size(128, 90);
            this.RoundButton_Pause.SpliteButtonWidth = 18;
            this.RoundButton_Pause.TabIndex = 13;
            this.RoundButton_Pause.UseVisualStyleBackColor = true;
            this.RoundButton_Pause.Click += new System.EventHandler(this.RoundButton_Pause_Click);
            // 
            // RoundButton_Data
            // 
            this.RoundButton_Data.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RoundButton_Data.BackColor = System.Drawing.Color.Transparent;
            this.RoundButton_Data.BaseColor = System.Drawing.Color.Transparent;
            this.RoundButton_Data.BaseColorEnd = System.Drawing.Color.Transparent;
            this.RoundButton_Data.FlatAppearance.BorderSize = 0;
            this.RoundButton_Data.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RoundButton_Data.ImageHeight = 75;
            this.RoundButton_Data.ImageIndex = 11;
            this.RoundButton_Data.ImageList = this.imageList_Button;
            this.RoundButton_Data.ImageWidth = 75;
            this.RoundButton_Data.Location = new System.Drawing.Point(384, 0);
            this.RoundButton_Data.Margin = new System.Windows.Forms.Padding(0);
            this.RoundButton_Data.Name = "RoundButton_Data";
            this.RoundButton_Data.nImageIndex = 11;
            this.RoundButton_Data.Radius = 24;
            this.RoundButton_Data.Size = new System.Drawing.Size(128, 90);
            this.RoundButton_Data.SpliteButtonWidth = 18;
            this.RoundButton_Data.TabIndex = 10;
            this.RoundButton_Data.UseVisualStyleBackColor = true;
            this.RoundButton_Data.Click += new System.EventHandler(this.RoundButton_Click);
            // 
            // RoundButton_Start
            // 
            this.RoundButton_Start.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RoundButton_Start.BackColor = System.Drawing.Color.Transparent;
            this.RoundButton_Start.BaseColor = System.Drawing.Color.Transparent;
            this.RoundButton_Start.BaseColorEnd = System.Drawing.Color.Transparent;
            this.RoundButton_Start.FlatAppearance.BorderSize = 0;
            this.RoundButton_Start.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RoundButton_Start.ImageHeight = 75;
            this.RoundButton_Start.ImageIndex = 19;
            this.RoundButton_Start.ImageList = this.imageList_Button;
            this.RoundButton_Start.ImageWidth = 75;
            this.RoundButton_Start.Location = new System.Drawing.Point(896, 0);
            this.RoundButton_Start.Margin = new System.Windows.Forms.Padding(0);
            this.RoundButton_Start.Name = "RoundButton_Start";
            this.RoundButton_Start.nImageIndex = 19;
            this.RoundButton_Start.Radius = 24;
            this.RoundButton_Start.Size = new System.Drawing.Size(128, 90);
            this.RoundButton_Start.SpliteButtonWidth = 18;
            this.RoundButton_Start.TabIndex = 12;
            this.RoundButton_Start.UseVisualStyleBackColor = true;
            this.RoundButton_Start.Click += new System.EventHandler(this.RoundButton_Start_Click);
            // 
            // RoundButton_Setting
            // 
            this.RoundButton_Setting.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RoundButton_Setting.BackColor = System.Drawing.Color.Transparent;
            this.RoundButton_Setting.BaseColor = System.Drawing.Color.Transparent;
            this.RoundButton_Setting.BaseColorEnd = System.Drawing.Color.Transparent;
            this.RoundButton_Setting.FlatAppearance.BorderSize = 0;
            this.RoundButton_Setting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RoundButton_Setting.ImageHeight = 75;
            this.RoundButton_Setting.ImageIndex = 17;
            this.RoundButton_Setting.ImageList = this.imageList_Button;
            this.RoundButton_Setting.ImageWidth = 75;
            this.RoundButton_Setting.Location = new System.Drawing.Point(512, 0);
            this.RoundButton_Setting.Margin = new System.Windows.Forms.Padding(0);
            this.RoundButton_Setting.Name = "RoundButton_Setting";
            this.RoundButton_Setting.nImageIndex = 17;
            this.RoundButton_Setting.Radius = 24;
            this.RoundButton_Setting.Size = new System.Drawing.Size(128, 90);
            this.RoundButton_Setting.SpliteButtonWidth = 18;
            this.RoundButton_Setting.TabIndex = 7;
            this.RoundButton_Setting.UseVisualStyleBackColor = true;
            this.RoundButton_Setting.Click += new System.EventHandler(this.RoundButton_Click);
            // 
            // RoundButton_Alarm
            // 
            this.RoundButton_Alarm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RoundButton_Alarm.BackColor = System.Drawing.Color.Transparent;
            this.RoundButton_Alarm.BaseColor = System.Drawing.Color.Transparent;
            this.RoundButton_Alarm.BaseColorEnd = System.Drawing.Color.Transparent;
            this.RoundButton_Alarm.FlatAppearance.BorderSize = 0;
            this.RoundButton_Alarm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RoundButton_Alarm.ImageHeight = 75;
            this.RoundButton_Alarm.ImageIndex = 4;
            this.RoundButton_Alarm.ImageList = this.imageList_Button;
            this.RoundButton_Alarm.ImageWidth = 75;
            this.RoundButton_Alarm.Location = new System.Drawing.Point(128, 0);
            this.RoundButton_Alarm.Margin = new System.Windows.Forms.Padding(0);
            this.RoundButton_Alarm.Name = "RoundButton_Alarm";
            this.RoundButton_Alarm.nImageIndex = 4;
            this.RoundButton_Alarm.Radius = 24;
            this.RoundButton_Alarm.Size = new System.Drawing.Size(128, 90);
            this.RoundButton_Alarm.SpliteButtonWidth = 18;
            this.RoundButton_Alarm.TabIndex = 9;
            this.RoundButton_Alarm.UseVisualStyleBackColor = true;
            this.RoundButton_Alarm.Click += new System.EventHandler(this.RoundButton_Click);
            // 
            // RoundButton_Auto
            // 
            this.RoundButton_Auto.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RoundButton_Auto.BackColor = System.Drawing.Color.Transparent;
            this.RoundButton_Auto.BaseColor = System.Drawing.Color.Transparent;
            this.RoundButton_Auto.BaseColorEnd = System.Drawing.Color.Transparent;
            this.RoundButton_Auto.FlatAppearance.BorderSize = 0;
            this.RoundButton_Auto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RoundButton_Auto.ImageHeight = 75;
            this.RoundButton_Auto.ImageIndex = 1;
            this.RoundButton_Auto.ImageList = this.imageList_Button;
            this.RoundButton_Auto.ImageWidth = 75;
            this.RoundButton_Auto.Location = new System.Drawing.Point(0, 0);
            this.RoundButton_Auto.Margin = new System.Windows.Forms.Padding(0);
            this.RoundButton_Auto.Name = "RoundButton_Auto";
            this.RoundButton_Auto.nImageIndex = 1;
            this.RoundButton_Auto.Radius = 24;
            this.RoundButton_Auto.Size = new System.Drawing.Size(128, 90);
            this.RoundButton_Auto.SpliteButtonWidth = 18;
            this.RoundButton_Auto.TabIndex = 6;
            this.RoundButton_Auto.UseVisualStyleBackColor = true;
            this.RoundButton_Auto.Click += new System.EventHandler(this.RoundButton_Click);
            // 
            // panel1
            // 
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.panel1.Controls.Add(this.pictureBox_Quiet);
            this.panel1.Controls.Add(this.ucChooseLan1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(1411, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(126, 84);
            this.panel1.TabIndex = 24;
            // 
            // pictureBox_Quiet
            // 
            this.pictureBox_Quiet.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox_Quiet.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_Quiet.Image")));
            this.pictureBox_Quiet.Location = new System.Drawing.Point(0, 0);
            this.pictureBox_Quiet.Name = "pictureBox_Quiet";
            this.pictureBox_Quiet.Size = new System.Drawing.Size(86, 84);
            this.pictureBox_Quiet.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_Quiet.TabIndex = 26;
            this.pictureBox_Quiet.TabStop = false;
            this.pictureBox_Quiet.Visible = false;
            this.pictureBox_Quiet.Click += new System.EventHandler(this.pictureBox_Quiet_Click);
            // 
            // ucChooseLan1
            // 
            this.ucChooseLan1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ucChooseLan1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ucChooseLan1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.ucChooseLan1.Location = new System.Drawing.Point(94, 61);
            this.ucChooseLan1.Margin = new System.Windows.Forms.Padding(5);
            this.ucChooseLan1.Name = "ucChooseLan1";
            this.ucChooseLan1.Size = new System.Drawing.Size(39, 21);
            this.ucChooseLan1.TabIndex = 25;
            // 
            // Form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1540, 1048);
            this.Controls.Add(this.tableLayoutPanel_Main);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Name = "Form_Main";
            this.Text = "";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Main_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_Main_FormClosed);
            this.Load += new System.EventHandler(this.Form_Main_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tableLayoutPanel_Main.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Quiet)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_State;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_SafetyDoor;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_StackLight;

        private System.Windows.Forms.ImageList imageList_Button;
        private System.Windows.Forms.ImageList imageList_Hive;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_Process;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_Speed;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_SpeedLow;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_SpeedMiddle;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_SpeedHigh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Produce;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_DryRun;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Calib;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_GRR;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Others;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Recipe;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_AdjustMode;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private AF.Common.UI.UserControl_Message userControl_Notice1;
        private System.Windows.Forms.Panel panel_main;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Main;
        private System.Windows.Forms.ImageList imageList_User;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private AF.Common.UI.RoundButton RoundButton_User;
        private AF.Common.UI.RoundButton RoundButton_ProjectName;
        private AF.Common.UI.RoundButton RoundButton_Stop;
        private AF.Common.UI.RoundButton RoundButton_Manual;
        private AF.Common.UI.RoundButton RoundButton_Pause;
        private AF.Common.UI.RoundButton RoundButton_Data;
        private AF.Common.UI.RoundButton RoundButton_Start;
        private AF.Common.UI.RoundButton RoundButton_Setting;
        private AF.Common.UI.RoundButton RoundButton_Alarm;
        private AF.Common.UI.RoundButton RoundButton_Auto;
        private System.Windows.Forms.Panel panel1;
        private AF.Data.Language.ucc.ucChooseLan ucChooseLan1;
        private System.Windows.Forms.PictureBox pictureBox_Quiet;
    }
}
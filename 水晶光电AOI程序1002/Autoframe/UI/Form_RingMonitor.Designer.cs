namespace Autoframe.UI
{
    partial class Form_RingMonitor
    {
        /// <summary>
        /// 设计器支持所需的变量。
        /// 上部统计区（topPanel/statPanel）与右侧明细栏（rightPanel）均为 Designer 设计固定布局，
        /// 在 Visual Studio 窗体设计器中可直接拖拽修改（名称、位置、颜色、字体等）。
        /// 注意：gridHost 为 Dock=Fill 的料盘容器，必须保持在 Controls.Add 顺序的最后（Dock 布局按 z-order 从底到顶处理）。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>清理所有正在使用的资源</summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.topPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRing = new System.Windows.Forms.Label();
            this.lblMode = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.statPanel = new System.Windows.Forms.TableLayoutPanel();
            this.kpiTotal = new System.Windows.Forms.Panel();
            this.lblTotalValue = new System.Windows.Forms.Label();
            this.lblTotalTitle = new System.Windows.Forms.Label();
            this.kpiOK = new System.Windows.Forms.Panel();
            this.lblOKValue = new System.Windows.Forms.Label();
            this.lblOKTitle = new System.Windows.Forms.Label();
            this.kpiNG = new System.Windows.Forms.Panel();
            this.lblNGValue = new System.Windows.Forms.Label();
            this.lblNGTitle = new System.Windows.Forms.Label();
            this.kpiRate = new System.Windows.Forms.Panel();
            this.lblRateValue = new System.Windows.Forms.Label();
            this.lblRateTitle = new System.Windows.Forms.Label();
            this.totalGroup = new System.Windows.Forms.Panel();
            this.barTotal = new System.Windows.Forms.ProgressBar();
            this.lblTotalNG = new System.Windows.Forms.Label();
            this.consecGroup = new System.Windows.Forms.Panel();
            this.barConsec = new System.Windows.Forms.ProgressBar();
            this.lblConsec = new System.Windows.Forms.Label();
            this.rightPanel = new System.Windows.Forms.Panel();
            this.rightTable = new System.Windows.Forms.TableLayoutPanel();
            this.btnRow = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAck = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.lbAlarm = new System.Windows.Forms.ListBox();
            this.lblDtl = new System.Windows.Forms.Label();
            this.dgv = new System.Windows.Forms.DataGridView();
            this.hintLabel = new System.Windows.Forms.Label();
            this.gridHost = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.topPanel.SuspendLayout();
            this.statPanel.SuspendLayout();
            this.kpiTotal.SuspendLayout();
            this.kpiOK.SuspendLayout();
            this.kpiNG.SuspendLayout();
            this.kpiRate.SuspendLayout();
            this.totalGroup.SuspendLayout();
            this.consecGroup.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.rightTable.SuspendLayout();
            this.btnRow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // topPanel
            // 
            this.topPanel.BackColor = System.Drawing.Color.White;
            this.topPanel.Controls.Add(this.lblRing);
            this.topPanel.Controls.Add(this.lblMode);
            this.topPanel.Controls.Add(this.lblTime);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 96);
            this.topPanel.Name = "topPanel";
            this.topPanel.Padding = new System.Windows.Forms.Padding(12, 8, 12, 4);
            this.topPanel.Size = new System.Drawing.Size(1931, 52);
            this.topPanel.TabIndex = 4;
            // 
            // lblRing
            // 
            this.lblRing.AutoSize = true;
            this.lblRing.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblRing.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.lblRing.Location = new System.Drawing.Point(16, 14);
            this.lblRing.Margin = new System.Windows.Forms.Padding(4, 6, 18, 0);
            this.lblRing.Name = "lblRing";
            this.lblRing.Size = new System.Drawing.Size(175, 31);
            this.lblRing.TabIndex = 0;
            this.lblRing.Text = "当前钢环：#—";
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.lblMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.lblMode.Location = new System.Drawing.Point(213, 17);
            this.lblMode.Margin = new System.Windows.Forms.Padding(4, 9, 18, 0);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(94, 27);
            this.lblMode.TabIndex = 1;
            this.lblMode.Text = "规格：—";
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.lblTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.lblTime.Location = new System.Drawing.Point(329, 17);
            this.lblTime.Margin = new System.Windows.Forms.Padding(4, 9, 18, 0);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(94, 27);
            this.lblTime.TabIndex = 2;
            this.lblTime.Text = "开始：—";
            // 
            // statPanel
            // 
            this.statPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.statPanel.ColumnCount = 8;
            this.statPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.statPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.statPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.statPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.statPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.statPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.statPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.statPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.statPanel.Controls.Add(this.kpiTotal, 0, 0);
            this.statPanel.Controls.Add(this.kpiOK, 1, 0);
            this.statPanel.Controls.Add(this.kpiNG, 2, 0);
            this.statPanel.Controls.Add(this.kpiRate, 3, 0);
            this.statPanel.Controls.Add(this.totalGroup, 6, 0);
            this.statPanel.Controls.Add(this.consecGroup, 4, 0);
            this.statPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.statPanel.Location = new System.Drawing.Point(0, 0);
            this.statPanel.Name = "statPanel";
            this.statPanel.Padding = new System.Windows.Forms.Padding(12, 8, 12, 4);
            this.statPanel.RowCount = 1;
            this.statPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.statPanel.Size = new System.Drawing.Size(1931, 96);
            this.statPanel.TabIndex = 5;
            // 
            // kpiTotal
            // 
            this.kpiTotal.BackColor = System.Drawing.Color.White;
            this.kpiTotal.Controls.Add(this.lblTotalValue);
            this.kpiTotal.Controls.Add(this.lblTotalTitle);
            this.kpiTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiTotal.Location = new System.Drawing.Point(12, 8);
            this.kpiTotal.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
            this.kpiTotal.Name = "kpiTotal";
            this.kpiTotal.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.kpiTotal.Size = new System.Drawing.Size(230, 80);
            this.kpiTotal.TabIndex = 0;
            // 
            // lblTotalValue
            // 
            this.lblTotalValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTotalValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.lblTotalValue.Location = new System.Drawing.Point(10, 24);
            this.lblTotalValue.Name = "lblTotalValue";
            this.lblTotalValue.Size = new System.Drawing.Size(210, 50);
            this.lblTotalValue.TabIndex = 0;
            this.lblTotalValue.Text = "0";
            this.lblTotalValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTotalTitle
            // 
            this.lblTotalTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTotalTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.lblTotalTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(116)))), ((int)(((byte)(124)))));
            this.lblTotalTitle.Location = new System.Drawing.Point(10, 6);
            this.lblTotalTitle.Name = "lblTotalTitle";
            this.lblTotalTitle.Size = new System.Drawing.Size(210, 18);
            this.lblTotalTitle.TabIndex = 1;
            this.lblTotalTitle.Text = "总检测";
            // 
            // kpiOK
            // 
            this.kpiOK.BackColor = System.Drawing.Color.White;
            this.kpiOK.Controls.Add(this.lblOKValue);
            this.kpiOK.Controls.Add(this.lblOKTitle);
            this.kpiOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiOK.Location = new System.Drawing.Point(250, 8);
            this.kpiOK.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
            this.kpiOK.Name = "kpiOK";
            this.kpiOK.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.kpiOK.Size = new System.Drawing.Size(230, 80);
            this.kpiOK.TabIndex = 1;
            // 
            // lblOKValue
            // 
            this.lblOKValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOKValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblOKValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(157)))), ((int)(((byte)(85)))));
            this.lblOKValue.Location = new System.Drawing.Point(10, 24);
            this.lblOKValue.Name = "lblOKValue";
            this.lblOKValue.Size = new System.Drawing.Size(210, 50);
            this.lblOKValue.TabIndex = 0;
            this.lblOKValue.Text = "0";
            this.lblOKValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblOKTitle
            // 
            this.lblOKTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblOKTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.lblOKTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(116)))), ((int)(((byte)(124)))));
            this.lblOKTitle.Location = new System.Drawing.Point(10, 6);
            this.lblOKTitle.Name = "lblOKTitle";
            this.lblOKTitle.Size = new System.Drawing.Size(210, 18);
            this.lblOKTitle.TabIndex = 1;
            this.lblOKTitle.Text = "OK";
            // 
            // kpiNG
            // 
            this.kpiNG.BackColor = System.Drawing.Color.White;
            this.kpiNG.Controls.Add(this.lblNGValue);
            this.kpiNG.Controls.Add(this.lblNGTitle);
            this.kpiNG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiNG.Location = new System.Drawing.Point(488, 8);
            this.kpiNG.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
            this.kpiNG.Name = "kpiNG";
            this.kpiNG.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.kpiNG.Size = new System.Drawing.Size(230, 80);
            this.kpiNG.TabIndex = 2;
            // 
            // lblNGValue
            // 
            this.lblNGValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNGValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblNGValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(69)))), ((int)(((byte)(69)))));
            this.lblNGValue.Location = new System.Drawing.Point(10, 24);
            this.lblNGValue.Name = "lblNGValue";
            this.lblNGValue.Size = new System.Drawing.Size(210, 50);
            this.lblNGValue.TabIndex = 0;
            this.lblNGValue.Text = "0";
            this.lblNGValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblNGTitle
            // 
            this.lblNGTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblNGTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.lblNGTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(116)))), ((int)(((byte)(124)))));
            this.lblNGTitle.Location = new System.Drawing.Point(10, 6);
            this.lblNGTitle.Name = "lblNGTitle";
            this.lblNGTitle.Size = new System.Drawing.Size(210, 18);
            this.lblNGTitle.TabIndex = 1;
            this.lblNGTitle.Text = "NG";
            // 
            // kpiRate
            // 
            this.kpiRate.BackColor = System.Drawing.Color.White;
            this.kpiRate.Controls.Add(this.lblRateValue);
            this.kpiRate.Controls.Add(this.lblRateTitle);
            this.kpiRate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiRate.Location = new System.Drawing.Point(726, 8);
            this.kpiRate.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
            this.kpiRate.Name = "kpiRate";
            this.kpiRate.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.kpiRate.Size = new System.Drawing.Size(230, 80);
            this.kpiRate.TabIndex = 3;
            // 
            // lblRateValue
            // 
            this.lblRateValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRateValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblRateValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.lblRateValue.Location = new System.Drawing.Point(10, 24);
            this.lblRateValue.Name = "lblRateValue";
            this.lblRateValue.Size = new System.Drawing.Size(210, 50);
            this.lblRateValue.TabIndex = 0;
            this.lblRateValue.Text = "—";
            this.lblRateValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRateTitle
            // 
            this.lblRateTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRateTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.lblRateTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(116)))), ((int)(((byte)(124)))));
            this.lblRateTitle.Location = new System.Drawing.Point(10, 6);
            this.lblRateTitle.Name = "lblRateTitle";
            this.lblRateTitle.Size = new System.Drawing.Size(210, 18);
            this.lblRateTitle.TabIndex = 1;
            this.lblRateTitle.Text = "良率";
            // 
            // totalGroup
            // 
            this.totalGroup.BackColor = System.Drawing.Color.White;
            this.statPanel.SetColumnSpan(this.totalGroup, 2);
            this.totalGroup.Controls.Add(this.barTotal);
            this.totalGroup.Controls.Add(this.lblTotalNG);
            this.totalGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.totalGroup.Location = new System.Drawing.Point(1440, 8);
            this.totalGroup.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
            this.totalGroup.Name = "totalGroup";
            this.totalGroup.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.totalGroup.Size = new System.Drawing.Size(471, 80);
            this.totalGroup.TabIndex = 5;
            // 
            // barTotal
            // 
            this.barTotal.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barTotal.Location = new System.Drawing.Point(0, 62);
            this.barTotal.Margin = new System.Windows.Forms.Padding(10, 0, 10, 10);
            this.barTotal.Maximum = 50;
            this.barTotal.Name = "barTotal";
            this.barTotal.Size = new System.Drawing.Size(471, 14);
            this.barTotal.TabIndex = 0;
            // 
            // lblTotalNG
            // 
            this.lblTotalNG.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTotalNG.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTotalNG.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(76)))), ((int)(((byte)(84)))));
            this.lblTotalNG.Location = new System.Drawing.Point(0, 0);
            this.lblTotalNG.Margin = new System.Windows.Forms.Padding(10, 4, 10, 0);
            this.lblTotalNG.Name = "lblTotalNG";
            this.lblTotalNG.Size = new System.Drawing.Size(471, 23);
            this.lblTotalNG.TabIndex = 1;
            this.lblTotalNG.Text = "累计NG /50：0";
            // 
            // consecGroup
            // 
            this.consecGroup.BackColor = System.Drawing.Color.White;
            this.statPanel.SetColumnSpan(this.consecGroup, 2);
            this.consecGroup.Controls.Add(this.barConsec);
            this.consecGroup.Controls.Add(this.lblConsec);
            this.consecGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consecGroup.Location = new System.Drawing.Point(964, 8);
            this.consecGroup.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
            this.consecGroup.Name = "consecGroup";
            this.consecGroup.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.consecGroup.Size = new System.Drawing.Size(468, 80);
            this.consecGroup.TabIndex = 4;
            // 
            // barConsec
            // 
            this.barConsec.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barConsec.Location = new System.Drawing.Point(0, 62);
            this.barConsec.Margin = new System.Windows.Forms.Padding(10, 0, 10, 10);
            this.barConsec.Maximum = 30;
            this.barConsec.Name = "barConsec";
            this.barConsec.Size = new System.Drawing.Size(468, 14);
            this.barConsec.TabIndex = 0;
            // 
            // lblConsec
            // 
            this.lblConsec.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblConsec.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblConsec.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(76)))), ((int)(((byte)(84)))));
            this.lblConsec.Location = new System.Drawing.Point(0, 0);
            this.lblConsec.Margin = new System.Windows.Forms.Padding(10, 4, 10, 0);
            this.lblConsec.Name = "lblConsec";
            this.lblConsec.Size = new System.Drawing.Size(468, 23);
            this.lblConsec.TabIndex = 1;
            this.lblConsec.Text = "连续NG /30：0";
            // 
            // rightPanel
            // 
            this.rightPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.rightPanel.Controls.Add(this.rightTable);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Location = new System.Drawing.Point(1167, 3);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Padding = new System.Windows.Forms.Padding(6, 8, 12, 6);
            this.rightPanel.Size = new System.Drawing.Size(761, 791);
            this.rightPanel.TabIndex = 3;
            // 
            // rightTable
            // 
            this.rightTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.rightTable.ColumnCount = 1;
            this.rightTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rightTable.Controls.Add(this.btnRow, 0, 0);
            this.rightTable.Controls.Add(this.lbAlarm, 0, 1);
            this.rightTable.Controls.Add(this.lblDtl, 0, 2);
            this.rightTable.Controls.Add(this.dgv, 0, 3);
            this.rightTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightTable.Location = new System.Drawing.Point(6, 8);
            this.rightTable.Name = "rightTable";
            this.rightTable.RowCount = 4;
            this.rightTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.rightTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.rightTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.rightTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rightTable.Size = new System.Drawing.Size(743, 777);
            this.rightTable.TabIndex = 0;
            // 
            // btnRow
            // 
            this.btnRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.btnRow.Controls.Add(this.btnAck);
            this.btnRow.Controls.Add(this.btnReset);
            this.btnRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRow.Location = new System.Drawing.Point(3, 3);
            this.btnRow.Name = "btnRow";
            this.btnRow.Padding = new System.Windows.Forms.Padding(2);
            this.btnRow.Size = new System.Drawing.Size(737, 38);
            this.btnRow.TabIndex = 0;
            // 
            // btnAck
            // 
            this.btnAck.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.btnAck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAck.ForeColor = System.Drawing.Color.White;
            this.btnAck.Location = new System.Drawing.Point(5, 5);
            this.btnAck.Name = "btnAck";
            this.btnAck.Size = new System.Drawing.Size(128, 32);
            this.btnAck.TabIndex = 0;
            this.btnAck.Text = "确认报警并复位";
            this.btnAck.UseVisualStyleBackColor = false;
            this.btnAck.Click += new System.EventHandler(this.btnAck_Click);
            // 
            // btnReset
            // 
            this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReset.Location = new System.Drawing.Point(139, 5);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(96, 32);
            this.btnReset.TabIndex = 1;
            this.btnReset.Text = "重置统计";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // lbAlarm
            // 
            this.lbAlarm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbAlarm.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lbAlarm.ItemHeight = 24;
            this.lbAlarm.Location = new System.Drawing.Point(3, 47);
            this.lbAlarm.Name = "lbAlarm";
            this.lbAlarm.Size = new System.Drawing.Size(737, 104);
            this.lbAlarm.TabIndex = 1;
            // 
            // lblDtl
            // 
            this.lblDtl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDtl.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblDtl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(76)))), ((int)(((byte)(84)))));
            this.lblDtl.Location = new System.Drawing.Point(3, 154);
            this.lblDtl.Name = "lblDtl";
            this.lblDtl.Size = new System.Drawing.Size(737, 24);
            this.lblDtl.TabIndex = 2;
            this.lblDtl.Text = "单片结果明细（最新在前，可上下滚动）";
            this.lblDtl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgv
            // 
            this.dgv.AllowUserToAddRows = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.AllowUserToResizeRows = false;
            this.dgv.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgv.BackgroundColor = System.Drawing.Color.White;
            this.dgv.ColumnHeadersHeight = 34;
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.dgv.Location = new System.Drawing.Point(3, 181);
            this.dgv.MultiSelect = false;
            this.dgv.Name = "dgv";
            this.dgv.ReadOnly = true;
            this.dgv.RowHeadersVisible = false;
            this.dgv.RowHeadersWidth = 62;
            this.dgv.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv.Size = new System.Drawing.Size(737, 593);
            this.dgv.TabIndex = 3;
            // 
            // hintLabel
            // 
            this.hintLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.hintLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hintLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.hintLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(116)))), ((int)(((byte)(124)))));
            this.hintLabel.Location = new System.Drawing.Point(0, 945);
            this.hintLabel.Name = "hintLabel";
            this.hintLabel.Padding = new System.Windows.Forms.Padding(14, 4, 0, 0);
            this.hintLabel.Size = new System.Drawing.Size(1931, 24);
            this.hintLabel.TabIndex = 1;
            this.hintLabel.Text = "综合结果 = IR ∩ AR，任一 NG 即 NG；连续30片或累计50片NG自动报警（空跑模式下无NG）。";
            // 
            // gridHost
            // 
            this.gridHost.AutoScroll = true;
            this.gridHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.gridHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridHost.Location = new System.Drawing.Point(3, 3);
            this.gridHost.Name = "gridHost";
            this.gridHost.Padding = new System.Windows.Forms.Padding(6);
            this.gridHost.Size = new System.Drawing.Size(1158, 791);
            this.gridHost.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60.27965F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 39.72035F));
            this.tableLayoutPanel1.Controls.Add(this.gridHost, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.rightPanel, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 148);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1931, 797);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // Form_RingMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(1931, 969);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.hintLabel);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.statPanel);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.MinimumSize = new System.Drawing.Size(720, 520);
            this.Name = "Form_RingMonitor";
            this.Text = "钢环检测监控";
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.statPanel.ResumeLayout(false);
            this.kpiTotal.ResumeLayout(false);
            this.kpiOK.ResumeLayout(false);
            this.kpiNG.ResumeLayout(false);
            this.kpiRate.ResumeLayout(false);
            this.totalGroup.ResumeLayout(false);
            this.consecGroup.ResumeLayout(false);
            this.rightPanel.ResumeLayout(false);
            this.rightTable.ResumeLayout(false);
            this.btnRow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        // ===== 上部统计区（Designer 设计固定）=====
        private System.Windows.Forms.FlowLayoutPanel topPanel;
        private System.Windows.Forms.Label lblRing;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.TableLayoutPanel statPanel;
        private System.Windows.Forms.Panel kpiTotal;
        private System.Windows.Forms.Label lblTotalTitle;
        private System.Windows.Forms.Label lblTotalValue;
        private System.Windows.Forms.Panel kpiOK;
        private System.Windows.Forms.Label lblOKTitle;
        private System.Windows.Forms.Label lblOKValue;
        private System.Windows.Forms.Panel kpiNG;
        private System.Windows.Forms.Label lblNGTitle;
        private System.Windows.Forms.Label lblNGValue;
        private System.Windows.Forms.Panel kpiRate;
        private System.Windows.Forms.Label lblRateTitle;
        private System.Windows.Forms.Label lblRateValue;
        private System.Windows.Forms.Panel consecGroup;
        private System.Windows.Forms.ProgressBar barConsec;
        private System.Windows.Forms.Label lblConsec;
        private System.Windows.Forms.Panel totalGroup;
        private System.Windows.Forms.ProgressBar barTotal;
        private System.Windows.Forms.Label lblTotalNG;

        // ===== 右侧明细栏（Designer 设计固定）=====
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.TableLayoutPanel rightTable;
        private System.Windows.Forms.FlowLayoutPanel btnRow;
        private System.Windows.Forms.Button btnAck;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.ListBox lbAlarm;
        private System.Windows.Forms.Label lblDtl;
        private System.Windows.Forms.DataGridView dgv;

        // ===== 底部说明条 =====
        private System.Windows.Forms.Label hintLabel;

        // ===== 中部料盘（Dock=Fill，最后 Add；自绘控件 GridCanvas 在代码中创建加入）=====
        private System.Windows.Forms.Panel gridHost;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}

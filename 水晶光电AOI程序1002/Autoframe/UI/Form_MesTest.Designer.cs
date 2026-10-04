namespace Autoframe.UI
{
    partial class Form_MesTest
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        private void InitializeComponent()
        {
            this.tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            this.gbCfg = new System.Windows.Forms.GroupBox();
            this.tlpCfg = new System.Windows.Forms.TableLayoutPanel();
            this.lblGetUrl = new System.Windows.Forms.Label();
            this.txtGetUrl = new System.Windows.Forms.TextBox();
            this.lblSaveUrl = new System.Windows.Forms.Label();
            this.txtSaveUrl = new System.Windows.Forms.TextBox();
            this.lblDomain = new System.Windows.Forms.Label();
            this.cboDomain = new System.Windows.Forms.ComboBox();
            this.lblEqp = new System.Windows.Forms.Label();
            this.txtEqp = new System.Windows.Forms.TextBox();
            this.lblWo = new System.Windows.Forms.Label();
            this.txtWo = new System.Windows.Forms.TextBox();
            this.lblModel = new System.Windows.Forms.Label();
            this.cboModel = new System.Windows.Forms.ComboBox();
            this.chkAuto = new System.Windows.Forms.CheckBox();
            this.btnSaveCfg = new System.Windows.Forms.Button();
            this.gbOps = new System.Windows.Forms.GroupBox();
            this.flpOps = new System.Windows.Forms.FlowLayoutPanel();
            this.btnGetModel = new System.Windows.Forms.Button();
            this.btnTestUpload = new System.Windows.Forms.Button();
            this.btnUploadRing = new System.Windows.Forms.Button();
            this.lblModelResult = new System.Windows.Forms.Label();
            this.gbLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.tlpRoot.SuspendLayout();
            this.gbCfg.SuspendLayout();
            this.tlpCfg.SuspendLayout();
            this.gbOps.SuspendLayout();
            this.flpOps.SuspendLayout();
            this.gbLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpRoot
            // 
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.gbCfg, 0, 0);
            this.tlpRoot.Controls.Add(this.gbOps, 0, 1);
            this.tlpRoot.Controls.Add(this.gbLog, 0, 2);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Location = new System.Drawing.Point(0, 0);
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.RowCount = 3;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Size = new System.Drawing.Size(1104, 600);
            this.tlpRoot.TabIndex = 0;
            // 
            // gbCfg
            // 
            this.gbCfg.AutoSize = true;
            this.gbCfg.Controls.Add(this.tlpCfg);
            this.gbCfg.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbCfg.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.gbCfg.Location = new System.Drawing.Point(3, 3);
            this.gbCfg.Name = "gbCfg";
            this.gbCfg.Padding = new System.Windows.Forms.Padding(10, 4, 10, 8);
            this.gbCfg.Size = new System.Drawing.Size(1098, 166);
            this.gbCfg.TabIndex = 0;
            this.gbCfg.TabStop = false;
            this.gbCfg.Text = "MES 配置（接口地址可在设备侧修改，保存后写入 exe.config）";
            // 
            // tlpCfg
            // 
            this.tlpCfg.AutoSize = true;
            this.tlpCfg.ColumnCount = 4;
            this.tlpCfg.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCfg.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCfg.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCfg.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCfg.Controls.Add(this.lblGetUrl, 0, 0);
            this.tlpCfg.Controls.Add(this.txtGetUrl, 1, 0);
            this.tlpCfg.Controls.Add(this.lblSaveUrl, 2, 0);
            this.tlpCfg.Controls.Add(this.txtSaveUrl, 3, 0);
            this.tlpCfg.Controls.Add(this.lblDomain, 0, 1);
            this.tlpCfg.Controls.Add(this.cboDomain, 1, 1);
            this.tlpCfg.Controls.Add(this.lblEqp, 2, 1);
            this.tlpCfg.Controls.Add(this.txtEqp, 3, 1);
            this.tlpCfg.Controls.Add(this.lblWo, 0, 2);
            this.tlpCfg.Controls.Add(this.txtWo, 1, 2);
            this.tlpCfg.Controls.Add(this.lblModel, 2, 2);
            this.tlpCfg.Controls.Add(this.cboModel, 3, 2);
            this.tlpCfg.Controls.Add(this.chkAuto, 0, 3);
            this.tlpCfg.Controls.Add(this.btnSaveCfg, 3, 3);
            this.tlpCfg.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpCfg.Location = new System.Drawing.Point(10, 22);
            this.tlpCfg.Name = "tlpCfg";
            this.tlpCfg.RowCount = 4;
            this.tlpCfg.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpCfg.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpCfg.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpCfg.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpCfg.Size = new System.Drawing.Size(1078, 136);
            this.tlpCfg.TabIndex = 0;
            // 
            // lblGetUrl
            // 
            this.lblGetUrl.AutoSize = true;
            this.lblGetUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGetUrl.Location = new System.Drawing.Point(3, 0);
            this.lblGetUrl.Name = "lblGetUrl";
            this.lblGetUrl.Size = new System.Drawing.Size(491, 34);
            this.lblGetUrl.TabIndex = 0;
            this.lblGetUrl.Text = "获取机种接口:";
            this.lblGetUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtGetUrl
            // 
            this.txtGetUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtGetUrl.Location = new System.Drawing.Point(500, 4);
            this.txtGetUrl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGetUrl.Name = "txtGetUrl";
            this.txtGetUrl.Size = new System.Drawing.Size(226, 25);
            this.txtGetUrl.TabIndex = 1;
            this.txtGetUrl.Text = "http://10.18.80.21:8081/api/getwomodel";
            // 
            // lblSaveUrl
            // 
            this.lblSaveUrl.AutoSize = true;
            this.lblSaveUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSaveUrl.Location = new System.Drawing.Point(732, 0);
            this.lblSaveUrl.Name = "lblSaveUrl";
            this.lblSaveUrl.Size = new System.Drawing.Size(111, 34);
            this.lblSaveUrl.TabIndex = 2;
            this.lblSaveUrl.Text = "上传数据接口:";
            this.lblSaveUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtSaveUrl
            // 
            this.txtSaveUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSaveUrl.Location = new System.Drawing.Point(849, 4);
            this.txtSaveUrl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtSaveUrl.Name = "txtSaveUrl";
            this.txtSaveUrl.Size = new System.Drawing.Size(226, 25);
            this.txtSaveUrl.TabIndex = 3;
            this.txtSaveUrl.Text = "http://10.18.80.21:8081/api/savetestdata";
            // 
            // lblDomain
            // 
            this.lblDomain.AutoSize = true;
            this.lblDomain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDomain.Location = new System.Drawing.Point(3, 34);
            this.lblDomain.Name = "lblDomain";
            this.lblDomain.Size = new System.Drawing.Size(491, 34);
            this.lblDomain.TabIndex = 4;
            this.lblDomain.Text = "domainId:";
            this.lblDomain.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboDomain
            // 
            this.cboDomain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboDomain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDomain.Items.AddRange(new object[] {
            "VNLP",
            "VNML"});
            this.cboDomain.Location = new System.Drawing.Point(500, 38);
            this.cboDomain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cboDomain.Name = "cboDomain";
            this.cboDomain.Size = new System.Drawing.Size(226, 28);
            this.cboDomain.TabIndex = 5;
            // 
            // lblEqp
            // 
            this.lblEqp.AutoSize = true;
            this.lblEqp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEqp.Location = new System.Drawing.Point(732, 34);
            this.lblEqp.Name = "lblEqp";
            this.lblEqp.Size = new System.Drawing.Size(111, 34);
            this.lblEqp.TabIndex = 6;
            this.lblEqp.Text = "设备编号 eqpId:";
            this.lblEqp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtEqp
            // 
            this.txtEqp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEqp.Location = new System.Drawing.Point(849, 38);
            this.txtEqp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtEqp.Name = "txtEqp";
            this.txtEqp.Size = new System.Drawing.Size(226, 25);
            this.txtEqp.TabIndex = 7;
            this.txtEqp.Text = "AOI-001";
            // 
            // lblWo
            // 
            this.lblWo.AutoSize = true;
            this.lblWo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWo.Location = new System.Drawing.Point(3, 68);
            this.lblWo.Name = "lblWo";
            this.lblWo.Size = new System.Drawing.Size(491, 34);
            this.lblWo.TabIndex = 8;
            this.lblWo.Text = "工单号 woId:";
            this.lblWo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtWo
            // 
            this.txtWo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtWo.Location = new System.Drawing.Point(500, 72);
            this.txtWo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtWo.Name = "txtWo";
            this.txtWo.Size = new System.Drawing.Size(226, 25);
            this.txtWo.TabIndex = 9;
            // 
            // lblModel
            // 
            this.lblModel.AutoSize = true;
            this.lblModel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblModel.Location = new System.Drawing.Point(732, 68);
            this.lblModel.Name = "lblModel";
            this.lblModel.Size = new System.Drawing.Size(111, 34);
            this.lblModel.TabIndex = 10;
            this.lblModel.Text = "机种 model:";
            this.lblModel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboModel
            // 
            this.cboModel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboModel.Location = new System.Drawing.Point(849, 72);
            this.cboModel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cboModel.Name = "cboModel";
            this.cboModel.Size = new System.Drawing.Size(226, 28);
            this.cboModel.TabIndex = 11;
            // 
            // chkAuto
            // 
            this.chkAuto.AutoSize = true;
            this.chkAuto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkAuto.Location = new System.Drawing.Point(3, 108);
            this.chkAuto.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.chkAuto.Name = "chkAuto";
            this.chkAuto.Size = new System.Drawing.Size(491, 22);
            this.chkAuto.TabIndex = 12;
            this.chkAuto.Text = "☑ 每完成一个钢环自动上传生产/质量数据（失败不影响生产，仅记日志）";
            this.chkAuto.UseVisualStyleBackColor = true;
            this.chkAuto.CheckedChanged += new System.EventHandler(this.chkAuto_CheckedChanged);
            // 
            // btnSaveCfg
            // 
            this.btnSaveCfg.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnSaveCfg.Location = new System.Drawing.Point(996, 105);
            this.btnSaveCfg.Name = "btnSaveCfg";
            this.btnSaveCfg.Size = new System.Drawing.Size(79, 28);
            this.btnSaveCfg.TabIndex = 13;
            this.btnSaveCfg.Text = "保存配置";
            this.btnSaveCfg.UseVisualStyleBackColor = true;
            this.btnSaveCfg.Click += new System.EventHandler(this.btnSaveCfg_Click);
            // 
            // gbOps
            // 
            this.gbOps.AutoSize = true;
            this.gbOps.Controls.Add(this.flpOps);
            this.gbOps.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbOps.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.gbOps.Location = new System.Drawing.Point(3, 175);
            this.gbOps.Name = "gbOps";
            this.gbOps.Padding = new System.Windows.Forms.Padding(10, 4, 10, 8);
            this.gbOps.Size = new System.Drawing.Size(1098, 64);
            this.gbOps.TabIndex = 1;
            this.gbOps.TabStop = false;
            this.gbOps.Text = "操作";
            // 
            // flpOps
            // 
            this.flpOps.Controls.Add(this.btnGetModel);
            this.flpOps.Controls.Add(this.btnTestUpload);
            this.flpOps.Controls.Add(this.btnUploadRing);
            this.flpOps.Controls.Add(this.lblModelResult);
            this.flpOps.Dock = System.Windows.Forms.DockStyle.Top;
            this.flpOps.Location = new System.Drawing.Point(10, 22);
            this.flpOps.Name = "flpOps";
            this.flpOps.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.flpOps.Size = new System.Drawing.Size(1078, 34);
            this.flpOps.TabIndex = 0;
            // 
            // btnGetModel
            // 
            this.btnGetModel.Location = new System.Drawing.Point(3, 5);
            this.btnGetModel.Name = "btnGetModel";
            this.btnGetModel.Size = new System.Drawing.Size(140, 30);
            this.btnGetModel.TabIndex = 0;
            this.btnGetModel.Text = "1. 获取工单机种";
            this.btnGetModel.UseVisualStyleBackColor = true;
            this.btnGetModel.Click += new System.EventHandler(this.btnGetModel_Click);
            // 
            // btnTestUpload
            // 
            this.btnTestUpload.Location = new System.Drawing.Point(149, 5);
            this.btnTestUpload.Name = "btnTestUpload";
            this.btnTestUpload.Size = new System.Drawing.Size(130, 30);
            this.btnTestUpload.TabIndex = 1;
            this.btnTestUpload.Text = "2. 测试上传(示例)";
            this.btnTestUpload.UseVisualStyleBackColor = true;
            this.btnTestUpload.Click += new System.EventHandler(this.btnTestUpload_Click);
            // 
            // btnUploadRing
            // 
            this.btnUploadRing.Location = new System.Drawing.Point(285, 5);
            this.btnUploadRing.Name = "btnUploadRing";
            this.btnUploadRing.Size = new System.Drawing.Size(150, 30);
            this.btnUploadRing.TabIndex = 2;
            this.btnUploadRing.Text = "3. 上传当前钢环";
            this.btnUploadRing.UseVisualStyleBackColor = true;
            this.btnUploadRing.Click += new System.EventHandler(this.btnUploadRing_Click);
            // 
            // lblModelResult
            // 
            this.lblModelResult.AutoSize = true;
            this.lblModelResult.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblModelResult.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(157)))), ((int)(((byte)(85)))));
            this.lblModelResult.Location = new System.Drawing.Point(441, 10);
            this.lblModelResult.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.lblModelResult.Name = "lblModelResult";
            this.lblModelResult.Size = new System.Drawing.Size(41, 19);
            this.lblModelResult.TabIndex = 3;
            this.lblModelResult.Text = "机种:";
            // 
            // gbLog
            // 
            this.gbLog.Controls.Add(this.txtLog);
            this.gbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbLog.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.gbLog.Location = new System.Drawing.Point(3, 245);
            this.gbLog.Name = "gbLog";
            this.gbLog.Padding = new System.Windows.Forms.Padding(10, 4, 10, 10);
            this.gbLog.Size = new System.Drawing.Size(1098, 352);
            this.gbLog.TabIndex = 2;
            this.gbLog.TabStop = false;
            this.gbLog.Text = "调用日志（接口地址、请求/响应、结果均记录于此）";
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.White;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.txtLog.Location = new System.Drawing.Point(10, 22);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(1078, 320);
            this.txtLog.TabIndex = 0;
            this.txtLog.WordWrap = false;
            // 
            // Form_MesTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(1104, 600);
            this.Controls.Add(this.tlpRoot);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.Name = "Form_MesTest";
            this.Text = "MES 上传与测试";
            this.tlpRoot.ResumeLayout(false);
            this.tlpRoot.PerformLayout();
            this.gbCfg.ResumeLayout(false);
            this.gbCfg.PerformLayout();
            this.tlpCfg.ResumeLayout(false);
            this.tlpCfg.PerformLayout();
            this.gbOps.ResumeLayout(false);
            this.flpOps.ResumeLayout(false);
            this.flpOps.PerformLayout();
            this.gbLog.ResumeLayout(false);
            this.gbLog.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpRoot;
        private System.Windows.Forms.GroupBox gbCfg;
        private System.Windows.Forms.TableLayoutPanel tlpCfg;
        private System.Windows.Forms.Label lblGetUrl;
        private System.Windows.Forms.TextBox txtGetUrl;
        private System.Windows.Forms.Label lblSaveUrl;
        private System.Windows.Forms.TextBox txtSaveUrl;
        private System.Windows.Forms.Label lblDomain;
        private System.Windows.Forms.ComboBox cboDomain;
        private System.Windows.Forms.Label lblEqp;
        private System.Windows.Forms.TextBox txtEqp;
        private System.Windows.Forms.Label lblWo;
        private System.Windows.Forms.TextBox txtWo;
        private System.Windows.Forms.Label lblModel;
        private System.Windows.Forms.ComboBox cboModel;
        private System.Windows.Forms.CheckBox chkAuto;
        private System.Windows.Forms.Button btnSaveCfg;
        private System.Windows.Forms.GroupBox gbOps;
        private System.Windows.Forms.FlowLayoutPanel flpOps;
        private System.Windows.Forms.Button btnGetModel;
        private System.Windows.Forms.Button btnTestUpload;
        private System.Windows.Forms.Button btnUploadRing;
        private System.Windows.Forms.Label lblModelResult;
        private System.Windows.Forms.GroupBox gbLog;
        private System.Windows.Forms.TextBox txtLog;
    }
}

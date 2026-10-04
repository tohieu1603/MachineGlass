namespace Autoframe.UI
{
    partial class Form_MesTest2
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
            this.pnlUrl = new System.Windows.Forms.TableLayoutPanel();
            this.lblUrl = new System.Windows.Forms.Label();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.gbRequest = new System.Windows.Forms.GroupBox();
            this.txtRequest = new System.Windows.Forms.TextBox();
            this.gbResponse = new System.Windows.Forms.GroupBox();
            this.txtResponse = new System.Windows.Forms.TextBox();
            this.pnlBottom = new System.Windows.Forms.TableLayoutPanel();
            this.lblSave = new System.Windows.Forms.Label();
            this.btnSend = new System.Windows.Forms.Button();
            this.tlpRoot.SuspendLayout();
            this.pnlUrl.SuspendLayout();
            this.gbRequest.SuspendLayout();
            this.gbResponse.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpRoot
            // 
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.pnlUrl, 0, 0);
            this.tlpRoot.Controls.Add(this.gbRequest, 0, 1);
            this.tlpRoot.Controls.Add(this.gbResponse, 0, 2);
            this.tlpRoot.Controls.Add(this.pnlBottom, 0, 3);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Location = new System.Drawing.Point(0, 0);
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.RowCount = 4;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tlpRoot.Size = new System.Drawing.Size(1100, 700);
            this.tlpRoot.TabIndex = 0;
            // 
            // pnlUrl
            // 
            this.pnlUrl.ColumnCount = 3;
            this.pnlUrl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.pnlUrl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlUrl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 162F));
            this.pnlUrl.Controls.Add(this.lblUrl, 0, 0);
            this.pnlUrl.Controls.Add(this.txtUrl, 1, 0);
            this.pnlUrl.Controls.Add(this.comboBox1, 2, 0);
            this.pnlUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlUrl.Location = new System.Drawing.Point(3, 3);
            this.pnlUrl.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.pnlUrl.Name = "pnlUrl";
            this.pnlUrl.RowCount = 1;
            this.pnlUrl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlUrl.Size = new System.Drawing.Size(1094, 43);
            this.pnlUrl.TabIndex = 0;
            // 
            // lblUrl
            // 
            this.lblUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUrl.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.lblUrl.Location = new System.Drawing.Point(3, 0);
            this.lblUrl.Name = "lblUrl";
            this.lblUrl.Size = new System.Drawing.Size(94, 43);
            this.lblUrl.TabIndex = 0;
            this.lblUrl.Text = "接口地址:";
            this.lblUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtUrl
            // 
            this.txtUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtUrl.Font = new System.Drawing.Font("Consolas", 10.5F);
            this.txtUrl.Location = new System.Drawing.Point(103, 4);
            this.txtUrl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(826, 32);
            this.txtUrl.TabIndex = 1;
            this.txtUrl.Text = "http://10.18.80.21:8081/api/getwomodel";
            // 
            // comboBox1
            // 
            this.comboBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "获取工单机种",
            "生产、质量数据上传"});
            this.comboBox1.Location = new System.Drawing.Point(935, 3);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(156, 32);
            this.comboBox1.TabIndex = 2;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // gbRequest
            // 
            this.gbRequest.Controls.Add(this.txtRequest);
            this.gbRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbRequest.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.gbRequest.Location = new System.Drawing.Point(3, 49);
            this.gbRequest.Name = "gbRequest";
            this.gbRequest.Padding = new System.Windows.Forms.Padding(10, 6, 10, 10);
            this.gbRequest.Size = new System.Drawing.Size(1094, 295);
            this.gbRequest.TabIndex = 1;
            this.gbRequest.TabStop = false;
            this.gbRequest.Text = "Request（JSON，可编辑）";
            // 
            // txtRequest
            // 
            this.txtRequest.BackColor = System.Drawing.Color.White;
            this.txtRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRequest.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtRequest.Location = new System.Drawing.Point(10, 33);
            this.txtRequest.Multiline = true;
            this.txtRequest.Name = "txtRequest";
            this.txtRequest.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRequest.Size = new System.Drawing.Size(1074, 252);
            this.txtRequest.TabIndex = 0;
            this.txtRequest.Text = "{\r\n    \"domainId\": \"VNLP\",\r\n    \"woId\": \"CY-SF02-2608010001\"\r\n}";
            this.txtRequest.WordWrap = false;
            // 
            // gbResponse
            // 
            this.gbResponse.Controls.Add(this.txtResponse);
            this.gbResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbResponse.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.gbResponse.Location = new System.Drawing.Point(3, 350);
            this.gbResponse.Name = "gbResponse";
            this.gbResponse.Padding = new System.Windows.Forms.Padding(10, 6, 10, 10);
            this.gbResponse.Size = new System.Drawing.Size(1094, 295);
            this.gbResponse.TabIndex = 2;
            this.gbResponse.TabStop = false;
            this.gbResponse.Text = "Response（服务器返回）";
            // 
            // txtResponse
            // 
            this.txtResponse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.txtResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtResponse.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtResponse.Location = new System.Drawing.Point(10, 33);
            this.txtResponse.Multiline = true;
            this.txtResponse.Name = "txtResponse";
            this.txtResponse.ReadOnly = true;
            this.txtResponse.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResponse.Size = new System.Drawing.Size(1074, 252);
            this.txtResponse.TabIndex = 0;
            this.txtResponse.WordWrap = false;
            // 
            // pnlBottom
            // 
            this.pnlBottom.ColumnCount = 2;
            this.pnlBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.pnlBottom.Controls.Add(this.lblSave, 0, 0);
            this.pnlBottom.Controls.Add(this.btnSend, 1, 0);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBottom.Location = new System.Drawing.Point(3, 651);
            this.pnlBottom.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.RowCount = 1;
            this.pnlBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlBottom.Size = new System.Drawing.Size(1094, 49);
            this.pnlBottom.TabIndex = 3;
            // 
            // lblSave
            // 
            this.lblSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSave.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
            this.lblSave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(122)))), ((int)(((byte)(183)))));
            this.lblSave.Location = new System.Drawing.Point(3, 0);
            this.lblSave.Name = "lblSave";
            this.lblSave.Size = new System.Drawing.Size(938, 49);
            this.lblSave.TabIndex = 0;
            this.lblSave.Text = "测试记录将保存到: D:\\exe\\Mes\\yyyyMMdd.txt";
            this.lblSave.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSend
            // 
            this.btnSend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSend.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnSend.Location = new System.Drawing.Point(947, 5);
            this.btnSend.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(144, 39);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "发送测试";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // Form_MesTest2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.tlpRoot);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.Name = "Form_MesTest2";
            this.Text = "MES 接口测试";
            this.tlpRoot.ResumeLayout(false);
            this.pnlUrl.ResumeLayout(false);
            this.pnlUrl.PerformLayout();
            this.gbRequest.ResumeLayout(false);
            this.gbRequest.PerformLayout();
            this.gbResponse.ResumeLayout(false);
            this.gbResponse.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpRoot;
        private System.Windows.Forms.TableLayoutPanel pnlUrl;
        private System.Windows.Forms.Label lblUrl;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.GroupBox gbRequest;
        private System.Windows.Forms.TextBox txtRequest;
        private System.Windows.Forms.GroupBox gbResponse;
        private System.Windows.Forms.TextBox txtResponse;
        private System.Windows.Forms.TableLayoutPanel pnlBottom;
        private System.Windows.Forms.Label lblSave;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

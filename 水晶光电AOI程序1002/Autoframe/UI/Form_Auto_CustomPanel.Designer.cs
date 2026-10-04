
namespace Autoframe.UI
{
    partial class Form_Auto_CustomPanel
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox_DI = new System.Windows.Forms.GroupBox();
            this.userControl_DI = new AF.Card.UI.UserControl_IO();
            this.groupBox_DO = new System.Windows.Forms.GroupBox();
            this.userControl_DO = new AF.Card.UI.UserControl_IO();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.IR需要清料 = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.groupBox_DI.SuspendLayout();
            this.groupBox_DO.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox_DI
            // 
            this.groupBox_DI.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_DI.Controls.Add(this.userControl_DI);
            this.groupBox_DI.Location = new System.Drawing.Point(422, 7);
            this.groupBox_DI.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_DI.Name = "groupBox_DI";
            this.groupBox_DI.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_DI.Size = new System.Drawing.Size(211, 177);
            this.groupBox_DI.TabIndex = 0;
            this.groupBox_DI.TabStop = false;
            this.groupBox_DI.Text = "DI";
            // 
            // userControl_DI
            // 
            this.userControl_DI.BackColor = System.Drawing.SystemColors.Control;
            this.userControl_DI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userControl_DI.Location = new System.Drawing.Point(2, 16);
            this.userControl_DI.Margin = new System.Windows.Forms.Padding(1);
            this.userControl_DI.Name = "userControl_DI";
            this.userControl_DI.Size = new System.Drawing.Size(207, 159);
            this.userControl_DI.TabIndex = 0;
            // 
            // groupBox_DO
            // 
            this.groupBox_DO.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_DO.Controls.Add(this.userControl_DO);
            this.groupBox_DO.Location = new System.Drawing.Point(643, 7);
            this.groupBox_DO.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_DO.Name = "groupBox_DO";
            this.groupBox_DO.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_DO.Size = new System.Drawing.Size(211, 177);
            this.groupBox_DO.TabIndex = 1;
            this.groupBox_DO.TabStop = false;
            this.groupBox_DO.Text = "DO";
            // 
            // userControl_DO
            // 
            this.userControl_DO.BackColor = System.Drawing.SystemColors.Control;
            this.userControl_DO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userControl_DO.Location = new System.Drawing.Point(2, 16);
            this.userControl_DO.Margin = new System.Windows.Forms.Padding(1);
            this.userControl_DO.Name = "userControl_DO";
            this.userControl_DO.Size = new System.Drawing.Size(207, 159);
            this.userControl_DO.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.RosyBrown;
            this.button1.Location = new System.Drawing.Point(21, 38);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(96, 32);
            this.button1.TabIndex = 2;
            this.button1.Text = "IR载具清料";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(159, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 26);
            this.label1.TabIndex = 5;
            this.label1.Text = "强制OK";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Enabled = false;
            this.checkBox2.Location = new System.Drawing.Point(21, 108);
            this.checkBox2.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(96, 16);
            this.checkBox2.TabIndex = 7;
            this.checkBox2.Text = "摆臂允许清料";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Enabled = false;
            this.checkBox4.Location = new System.Drawing.Point(21, 89);
            this.checkBox4.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(96, 16);
            this.checkBox4.TabIndex = 7;
            this.checkBox4.Text = "扩膜允许清料";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // IR需要清料
            // 
            this.IR需要清料.AutoSize = true;
            this.IR需要清料.Enabled = false;
            this.IR需要清料.Location = new System.Drawing.Point(21, 127);
            this.IR需要清料.Margin = new System.Windows.Forms.Padding(2);
            this.IR需要清料.Name = "IR需要清料";
            this.IR需要清料.Size = new System.Drawing.Size(84, 16);
            this.IR需要清料.TabIndex = 8;
            this.IR需要清料.Text = "IR需要清料";
            this.IR需要清料.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(147, 131);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(96, 23);
            this.button2.TabIndex = 9;
            this.button2.Text = "写入数据";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(147, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "OK覆膜";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(147, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "NG覆膜";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(192, 88);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(51, 21);
            this.numericUpDown1.TabIndex = 14;
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(192, 107);
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(51, 21);
            this.numericUpDown2.TabIndex = 15;
            // 
            // Form_Auto_CustomPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGray;
            this.Controls.Add(this.numericUpDown2);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.IR需要清料);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox_DO);
            this.Controls.Add(this.groupBox_DI);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form_Auto_CustomPanel";
            this.Size = new System.Drawing.Size(865, 195);
            this.Load += new System.EventHandler(this.Form_Auto_CustomPanel_Load);
            this.groupBox_DI.ResumeLayout(false);
            this.groupBox_DO.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_DI;
        private AF.Card.UI.UserControl_IO userControl_DI;
        private System.Windows.Forms.GroupBox groupBox_DO;
        private AF.Card.UI.UserControl_IO userControl_DO;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox IR需要清料;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
    }
}

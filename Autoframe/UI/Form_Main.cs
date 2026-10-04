using AF.Card;
using AF.Card.UI;
using AF.Common;
using AF.Common.UI;
using AF.Data;
using AF.Data.Language;
using AF.Data.Permission;
using AF.Data.Permission.UI;
using AF.Station;
using AF.Station.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AF.Station.StationBase;
using AF.Communicate;
using AF.File;
using Autoframe;


namespace Autoframe.UI
{
    /// <summary>
    /// 主界面
    /// </summary>
    public partial class Form_Main : AF.Data.Language.BaseForm
    {
        ToolStripMenuItem_Slider toolStripMenuItem_SpeedAdjust;

        /// <summary>
        /// 翻译黑名单
        /// </summary>
        public List<string> __blacklist { get; set; } = new List<string>() {            
            "RoundButton_ProjectName","RoundButton_Vision",
            "RoundButton_Alarm","RoundButton_Auto","RoundButton_Data",
            "RoundButton_Manual","RoundButton_Pause","RoundButton_Setting",
            "RoundButton_Start","RoundButton_Stop","RoundButton_User",
            "toolStripStatusLabel_StackLight","toolStripStatusLabel_State",
            "toolStripDropDownButton_Process","toolStripDropDownButton_Speed",
            "statusStrip1"
        };

        /// <summary>
        /// 界面字典
        /// </summary>
        public static Dictionary<RoundButton, Form> m_dicForm = new Dictionary<RoundButton, Form>();
        /// <summary>
        /// 当前界面
        /// </summary>
        public static Form m_currentForm = null;
        /// <summary>
        /// 当前界面按钮
        /// </summary>
        public static RoundButton m_currentButton = null;

        private Color blinkColor = Color.White;
        private Form_SafetyDoor formSafetyDoor = new Form_SafetyDoor();
        private Dictionary<string, ToolStripStatusLabel> dictCommunicate = new Dictionary<string, ToolStripStatusLabel>();

        /// <summary>
        /// 
        /// </summary>
        public Form_Main()
        {
            InitializeComponent();            
        }

        private void LoadToolStripMenu()
        {
            this.toolStripMenuItem_SpeedLow.Tag = 20;
            this.toolStripMenuItem_SpeedMiddle.Tag = 50;
            this.toolStripMenuItem_SpeedHigh.Tag = 100;

            this.toolStripMenuItem_SpeedAdjust = new ToolStripMenuItem_Slider("- 速度 +");
            this.toolStripMenuItem_SpeedAdjust.Click += ToolStripMenuItem_SpeedAdjust_Click;
            this.toolStripDropDownButton_Speed.DropDownItems.Insert(0, toolStripMenuItem_SpeedAdjust);

            foreach (var item in Parameter.dictRecipe)
            {
                ToolStripMenuItem tsmi = new ToolStripMenuItem(item.Key.txtTrans(TransType.Trans));
                tsmi.Tag = item.Key;
                tsmi.Font = new Font("宋体", 20);
                tsmi.Margin = new Padding(0, 5, 0, 5);
                tsmi.Click += ToolStripRecipeItem_Click;
                tsmi.Checked = item.Key == Parameter.CurrentRecipe;
                this.toolStripMenuItem_Recipe.DropDownItems.Add(tsmi);
            }
            this.toolStripMenuItem_AdjustMode.Click += ToolStripMenuItem_AdjustMode_Click;
            ToolStripRecipeItem_Click(null, null);

            this.toolStripMenuItem_Produce.Tag = AutoProcess.生产;
            this.toolStripMenuItem_Produce.Click += ToolStripProcessItem_Click;
            this.toolStripMenuItem_DryRun.Tag = AutoProcess.空跑;
            this.toolStripMenuItem_DryRun.Click += ToolStripProcessItem_Click;

            foreach (var item in Enum.GetValues(typeof(AutoProcess)))
            {
                ToolStripMenuItem tsmi = new ToolStripMenuItem(item.ToString().txtTrans(TransType.Trans));
                tsmi.Tag = item;
                tsmi.Font = new Font("宋体", 20);
                tsmi.Margin = new Padding(0, 5, 0, 5);
                tsmi.Click += ToolStripProcessItem_Click;

                int nProcess = (int)item;
                if (nProcess <= 1)
                    continue;
                else if (nProcess >= 100 && nProcess < 200)
                    this.toolStripMenuItem_Calib.DropDownItems.Add(tsmi);
                else if (nProcess >= 200 && nProcess < 300)
                    this.toolStripMenuItem_GRR.DropDownItems.Add(tsmi);
                else
                    this.toolStripMenuItem_Others.DropDownItems.Add(tsmi);
            }

            if (this.toolStripMenuItem_Calib.DropDownItems.Count == 0)
                this.toolStripDropDownButton_Process.DropDownItems.Remove(this.toolStripMenuItem_Calib);
            if (this.toolStripMenuItem_GRR.DropDownItems.Count == 0)
                this.toolStripDropDownButton_Process.DropDownItems.Remove(this.toolStripMenuItem_GRR);
            if (this.toolStripMenuItem_Others.DropDownItems.Count == 0)
                this.toolStripDropDownButton_Process.DropDownItems.Remove(this.toolStripMenuItem_Others);

            foreach (var item in TcpServerMgr.listServer)
            {
                if (item.bLight)
                {
                    NewToolStripStatusLabel(item.Name);

                    item.ServerStateChangedEvent += OnTcpServerStateChanged;
                    OnTcpServerStateChanged(item, null);
                }
            }
            foreach (var item in TcpClientMgr.listTcpClient)
            {
                if (item.bLight)
                {
                    NewToolStripStatusLabel(item.m_strName);
                    item.StateChangedEvent += OnTcpClientStateChanged;
                    OnTcpClientStateChanged(item);
                }
            }
            foreach (var item in ComMgr.listComPort)
            {
                if (item.bLight)
                {
                    NewToolStripStatusLabel(item.m_strName);
                    item.StateChangedEvent += OnComStateChanged;
                    OnComStateChanged(item);
                }
            }
        }

        private void ToolStripMenuItem_SpeedAdjust_Click(object sender, EventArgs e)
        {
            UpdateSpeed(this.toolStripMenuItem_SpeedAdjust.Value);
        }

        private void ToolStripMenuItem_AdjustMode_Click(object sender, EventArgs e)
        {
            this.toolStripMenuItem_AdjustMode.Checked = !this.toolStripMenuItem_AdjustMode.Checked;
            AF.Station.Statistics.IsAdjust = this.toolStripMenuItem_AdjustMode.Checked;
        }

        private void OnTcpServerStateChanged(object sender, EventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                TcpServer server = sender as TcpServer;
                if (!this.dictCommunicate.ContainsKey(server.Name))
                    return;
                this.dictCommunicate[server.Name].BackColor = server.IsRunning ? Color.DarkGreen : Color.Yellow;
                this.dictCommunicate[server.Name].ForeColor = server.IsRunning ? Color.White : Color.Black;
            });
        }

        private void OnTcpClientStateChanged(TcpClient tcp)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                if (!this.dictCommunicate.ContainsKey(tcp.m_strName))
                    return;
                this.dictCommunicate[tcp.m_strName].BackColor = tcp.IsOpen ? Color.DarkGreen : Color.Yellow;
                this.dictCommunicate[tcp.m_strName].ForeColor = tcp.IsOpen ? Color.White : Color.Black;
            });
        }

        private void OnComStateChanged(ComPort com)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                if (!this.dictCommunicate.ContainsKey(com.m_strName))
                    return;
                this.dictCommunicate[com.m_strName].BackColor = com.IsOpen ? Color.DarkGreen : Color.Yellow;
                this.dictCommunicate[com.m_strName].ForeColor = com.IsOpen ? Color.White : Color.Black;
            });
        }

        private void NewToolStripStatusLabel(string strText,int nWidth = 130)
        {
            ToolStripStatusLabel tssl = new ToolStripStatusLabel();
            tssl.AutoSize = false;
            tssl.BackColor = System.Drawing.Color.DarkGreen;
            tssl.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            tssl.DoubleClickEnabled = true;
            tssl.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            tssl.ForeColor = System.Drawing.Color.White;
            tssl.Margin = new System.Windows.Forms.Padding(0);
            tssl.Size = new System.Drawing.Size(nWidth, 25);
            tssl.Tag = strText;
            tssl.Text = tssl.Tag.ToString().txtTrans(TransType.Trans);
            this.statusStrip1.Items.Add(tssl);
            this.dictCommunicate.Add(strText, tssl);
            LanguageMgr.Instance.eventChangeLanguage += () =>
            {
                tssl.Text = tssl.Tag.ToString().txtTrans(TransType.Trans);
            };
        }

        /// <summary>
        /// 软件版本
        /// </summary>
        public static string SoftwareVersion = "";

        private void Form_Main_Load(object sender, EventArgs e)
        {
            //语言切换按钮，如果不需要可设置为false
            this.ucChooseLan1.Visible = true;
            this.ucChooseLan1.Height = (int)(this.tableLayoutPanel1.GetRowHeights()[0] * 0.25);
            this.ucChooseLan1.Width = (int)(this.tableLayoutPanel1.GetColumnWidths()[0] * 0.2);

            SoftwareVersion = $"{ProgramTool.g_FrameVersion}.{Ini.GetStringValue(Program.g_IniFile, "Software", "Version", "")}";
            this.Text = $"Autoframe V{SoftwareVersion}";
            this.RoundButton_ProjectName.Text = Ini.GetStringValue(Program.g_IniFile, "Info", "Project", "");

            //关联页面和站位
            m_dicForm.Add(RoundButton_Auto, new Form_Auto());
            m_dicForm.Add(RoundButton_Alarm, new Form_Alarm());
            m_dicForm.Add(RoundButton_Data, new Form_Data());
            m_dicForm.Add(RoundButton_Setting, new Form_Setting());
            m_dicForm.Add(RoundButton_Manual, new Form_Manual());

            //初始化页面属性
            foreach (KeyValuePair<RoundButton, Form> pair in m_dicForm)
            {
                pair.Value.TopLevel = false;
                pair.Value.Parent = this.panel_main;
                pair.Value.Dock = DockStyle.Fill;
            }

            LoadToolStripMenu();            

            //显示主页面
            RoundButton_Auto.PerformClick();

            //添加响应函数   
            PermissionMgr.Instance.eventChangePermission += OnChangePermission;
            OnChangePermission();
            MessageMgr.NewMessage += OnNewMessage;
            OnNewMessage(null, null);
            SystemIO.StackLightChangedEvent += OnStackLightChanged;
            SystemIO.OnNewEvent(null, null);
            OnStackLightChanged(SystemIO.CurrentLightState);

            SystemIO.SafetyDoorChangedEvent += OnSafetyDoorChanged;
            OnSafetyDoorChanged();
            StationEx.StationProcessChanged += OnProcessChanged;
            OnProcessChanged();
            ProgramTool.StationStateChangedEvent += OnStationStateChanged;
            OnStationStateChanged(StationMgr.State, StationMgr.State);
            UpdateSpeed();

            SystemIO.BuzzerOn += SystemIO_BuzzerOn;

            LanguageMgr.Instance.eventChangeLanguage += OnChangeLanguage;
            OnChangeLanguage();

            this.toolStripStatusLabel_SafetyDoor.Visible = !SystemIO.IsSafetyDoorEmpty();
            SystemIO.CheckSystemDI();

            SetPermission();
            Logger.Manual("Autoframe startup.");

            foreach (var item in m_dicForm)
            {
                item.Value.Show();
                item.Value.Hide();
            }
            m_dicForm[this.RoundButton_Auto].Show();
        }

        private void SystemIO_BuzzerOn()
        {
            if (SystemIO.doBuzzer == null)
                return;

            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                this.pictureBox_Quiet.Visible = SystemIO.doBuzzer.State == DoHandleEx.DoState.Off ? false : true;
            });
        }

        private void OnNewMessage(object sender, EventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                this.tableLayoutPanel_Main.RowStyles[1].Height = MessageMgr.HasMessage ? 120F : 0F;
            });
        }

        private void OnChangeLanguage()
        {
            try
            {
                this.toolStripDropDownButton_Process.Text = "流程: ".txtTrans(TransType.Trans) + $"{StationEx.Process}".txtTrans(TransType.Trans);
                this.toolStripDropDownButton_Speed.Text = "速度: ".txtTrans(TransType.Trans) + AxisHandle.SpeedPercent + "%";
                if (this.toolStripStatusLabel_StackLight.Tag != null)
                    this.toolStripStatusLabel_StackLight.Text = this.toolStripStatusLabel_StackLight.Tag.ToString().txtTrans(TransType.Trans);

                this.toolStripMenuItem_Produce.Text = "  "+ AutoProcess.生产.ToString().txtTrans(TransType.Trans);
                this.toolStripMenuItem_DryRun.Text = "  " + AutoProcess.空跑.ToString().txtTrans(TransType.Trans);
                if (this.toolStripStatusLabel_State.Tag != null)
                    this.toolStripStatusLabel_State.Text = this.toolStripStatusLabel_State.Tag.ToString().txtTrans(TransType.Trans);

                foreach (var item in this.toolStripMenuItem_Recipe.DropDownItems)
                {
                    if(item is ToolStripMenuItem)
                    {
                        ToolStripMenuItem tsm = item as ToolStripMenuItem;
                        if (tsm.Tag != null)
                            tsm.Text = tsm.Tag.ToString().txtTrans(TransType.Trans);
                    }
                }

                foreach (ToolStripMenuItem item in this.toolStripMenuItem_Calib.DropDownItems)
                {
                    if (item is ToolStripMenuItem)
                    {
                        ToolStripMenuItem tsm = item as ToolStripMenuItem;
                        if (tsm.Tag != null)
                            tsm.Text = tsm.Tag.ToString().txtTrans(TransType.Trans);
                    }
                }

                foreach (ToolStripMenuItem item in this.toolStripMenuItem_GRR.DropDownItems)
                {
                    if (item is ToolStripMenuItem)
                    {
                        ToolStripMenuItem tsm = item as ToolStripMenuItem;
                        if (tsm.Tag != null)
                            tsm.Text = tsm.Tag.ToString().txtTrans(TransType.Trans);
                    }
                }

                foreach (ToolStripMenuItem item in this.toolStripMenuItem_Others.DropDownItems)
                {
                    if (item is ToolStripMenuItem)
                    {
                        ToolStripMenuItem tsm = item as ToolStripMenuItem;
                        if (tsm.Tag != null)
                            tsm.Text = tsm.Tag.ToString().txtTrans(TransType.Trans);
                    }
                }
            }
            catch (Exception ex)
            {
                EventMgr.Warn(ex.Message);
            }
        }

        private void OnChangePermission()
        {
            switch(PermissionMgr.Instance.GetName(SwitchName.RoleName))
            {
                case "Administrator":
                case "Engineer":
                    this.RoundButton_User.nImageIndex = 3;
                    break;
                case "Adjustor":
                    this.RoundButton_User.nImageIndex = 2;
                    break;
                case "Operator":
                    this.RoundButton_User.nImageIndex = 1;
                    SwitchWnd(RoundButton_Auto);
                    break;
                default:
                    this.RoundButton_User.nImageIndex = 0;
                    SwitchWnd(RoundButton_Auto);
                    break;
            }          
        }

        /// <summary>
        /// 设置Control权限
        /// </summary>
        private void SetPermission()
        {
            //导航栏控件
            RoundButton_Manual.SetPermission(eRoles.Adjustor, eControlProp.Enable);
            RoundButton_Setting.SetPermission(eRoles.FAE, eControlProp.Enable);

            //生产菜单
            toolStripMenuItem_Recipe.SetPermission(eRoles.Adjustor, eControlProp.Enable);//配方
            toolStripMenuItem_AdjustMode.SetPermission(eRoles.FAE, eControlProp.Enable);
            toolStripMenuItem_Produce.SetPermission(eRoles.FAE, eControlProp.Enable);
            toolStripMenuItem_DryRun.SetPermission(eRoles.FAE, eControlProp.Enable);
            toolStripMenuItem_Calib.SetPermission(eRoles.Adjustor, eControlProp.Enable);
            toolStripMenuItem_GRR.SetPermission(eRoles.Adjustor, eControlProp.Enable);
            toolStripMenuItem_Others.SetPermission(eRoles.Adjustor, eControlProp.Enable);

            //速度菜单
            toolStripMenuItem_SpeedAdjust.SetPermission(eRoles.Adjustor, eControlProp.Enable);
            toolStripMenuItem_SpeedLow.SetPermission(eRoles.Adjustor, eControlProp.Enable);
            toolStripMenuItem_SpeedMiddle.SetPermission(eRoles.Adjustor, eControlProp.Enable);
            toolStripMenuItem_SpeedHigh.SetPermission(eRoles.Engineer, eControlProp.Enable);
        }

        private void ToolStripProcessItem_Click(object sender, EventArgs e)
        {
            if (StationMgr.State == StationState.Manual)
                StationEx.Process = (AutoProcess)((ToolStripMenuItem)sender).Tag;
            else
                MessageBox.Show("非手动模式，无法切换流程！".txtTrans(TransType.Trans));
        }

        private void ToolStripRecipeItem_Click(object sender, EventArgs e)
        {
            this.toolStripMenuItem_AdjustMode.Checked = AF.Station.Statistics.IsAdjust;
            if (sender == null)
                return;

            Parameter.CurrentRecipe = ((ToolStripMenuItem)sender).Text;
            Logger.Manual($"切换配方：{Parameter.CurrentRecipe}");
            foreach (var item in this.toolStripMenuItem_Recipe.DropDownItems)
            {
                if ((item is ToolStripMenuItem))
                    ((ToolStripMenuItem)item).Checked = false;
            }
            ((ToolStripMenuItem)sender).Checked = true;            
        }

        void OnProcessChanged()
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                Logger.Manual($"切换流程：{StationEx.Process}");
                this.toolStripDropDownButton_Process.Text = $"{"流程".txtTrans(TransType.Trans)}: {StationEx.Process.ToString().txtTrans(TransType.Trans)}";
                switch (StationEx.Process)
                {
                    case AutoProcess.生产:
                        this.toolStripDropDownButton_Process.BackColor = Color.DarkGreen;
                        this.toolStripDropDownButton_Process.ForeColor = Color.White;
                        break;
                    default:
                        this.toolStripDropDownButton_Process.BackColor = Color.LightGray;
                        this.toolStripDropDownButton_Process.ForeColor = Color.Black;
                        break;
                }

                foreach (var item in this.toolStripDropDownButton_Process.DropDownItems)
                {
                    if (!(item is ToolStripMenuItem))
                        continue;
                    var tag = ((ToolStripMenuItem)item).Tag;
                    ((ToolStripMenuItem)item).Checked = (tag == null) ? false : ((AutoProcess)tag == StationEx.Process);
                }                

                foreach (var item in this.toolStripMenuItem_Calib.DropDownItems)
                {
                    if (!(item is ToolStripMenuItem))
                        continue;
                    var tag = ((ToolStripMenuItem)item).Tag;
                    if (tag == null)
                        continue;

                    bool bChecked = (AutoProcess)tag == StationEx.Process;
                    ((ToolStripMenuItem)item).Checked = bChecked;
                    if (bChecked)
                        this.toolStripMenuItem_Calib.Checked = true;
                }
                foreach (var item in this.toolStripMenuItem_GRR.DropDownItems)
                {
                    if (!(item is ToolStripMenuItem))
                        continue;
                    var tag = ((ToolStripMenuItem)item).Tag;
                    if (tag == null)
                        continue;

                    bool bChecked = (AutoProcess)tag == StationEx.Process;
                    ((ToolStripMenuItem)item).Checked = bChecked;
                    if (bChecked)
                        this.toolStripMenuItem_GRR.Checked = true;
                }
                foreach (var item in this.toolStripMenuItem_Others.DropDownItems)
                {
                    if (!(item is ToolStripMenuItem))
                        continue;
                    var tag = ((ToolStripMenuItem)item).Tag;
                    if (tag == null)
                        continue;
                    ((ToolStripMenuItem)item).Checked = (AutoProcess)tag == StationEx.Process;

                    bool bChecked = (AutoProcess)tag == StationEx.Process;
                    ((ToolStripMenuItem)item).Checked = bChecked;
                    if (bChecked)
                        this.toolStripMenuItem_Others.Checked = true;
                }
            });
        }

        void OnSafetyDoorChanged()
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                toolStripStatusLabel_SafetyDoor.BackColor = SystemIO.IsSafetyDoorClosed ? Color.DarkGreen : Color.Yellow;
                toolStripStatusLabel_SafetyDoor.ForeColor = SystemIO.IsSafetyDoorClosed ? Color.White : Color.Black;
            });
        }

        void UpdateSpeed(int nSpeed = -1)
        {
            if(nSpeed > 0)
                AxisHandle.SpeedPercent = nSpeed;

            this.toolStripDropDownButton_Speed.Text = "速度: ".txtTrans(TransType.Trans) + AxisHandle.SpeedPercent + "%";
            this.toolStripMenuItem_SpeedLow.Checked = false;
            this.toolStripMenuItem_SpeedMiddle.Checked = false;
            this.toolStripMenuItem_SpeedHigh.Checked = false;
            this.toolStripDropDownButton_Speed.BackColor = Color.LightGray;
            this.toolStripDropDownButton_Speed.ForeColor = Color.Black;

            if (AxisHandle.SpeedPercent <= 20)
            {
                this.toolStripMenuItem_SpeedLow.Checked = true;
            }
            else if (AxisHandle.SpeedPercent <= 70)
            {
                this.toolStripMenuItem_SpeedMiddle.Checked = true;
            }
            else
            {
                this.toolStripMenuItem_SpeedHigh.Checked = true;
                this.toolStripDropDownButton_Speed.BackColor = Color.DarkGreen;
                this.toolStripDropDownButton_Speed.ForeColor = Color.White;
            }
        }

        private void OnStackLightChanged(SystemIO.StackLightState state)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                this.blinkColor = Color.White;

                if (SystemIO.IsEMG)
                {
                    this.blinkColor = Color.Red;
                    toolStripStatusLabel_StackLight.ForeColor = Color.White;
                    toolStripStatusLabel_StackLight.BackColor = Color.Red;
                    toolStripStatusLabel_StackLight.Text = "设备：急停".txtTrans(TransType.Trans);
                    toolStripStatusLabel_StackLight.Tag = "设备：急停";
                    return;
                }
                switch (state)
                {
                    case SystemIO.StackLightState.Run:
                        toolStripStatusLabel_StackLight.ForeColor = Color.White;
                        toolStripStatusLabel_StackLight.BackColor = Color.DarkGreen;
                        toolStripStatusLabel_StackLight.Text = "设备：运行".txtTrans(TransType.Trans);
                        toolStripStatusLabel_StackLight.Tag = "设备：运行";
                        break;
                    case SystemIO.StackLightState.Idel:
                        toolStripStatusLabel_StackLight.ForeColor = Color.Gray;
                        toolStripStatusLabel_StackLight.BackColor = Color.LightGreen;
                        toolStripStatusLabel_StackLight.Text = "设备：空闲".txtTrans(TransType.Trans);
                        toolStripStatusLabel_StackLight.Tag = "设备：空闲";
                        break;
                    case SystemIO.StackLightState.Wait:
                        toolStripStatusLabel_StackLight.ForeColor = Color.Black;
                        toolStripStatusLabel_StackLight.BackColor = Color.LightBlue;
                        toolStripStatusLabel_StackLight.Text = "设备：缺料".txtTrans(TransType.Trans);
                        toolStripStatusLabel_StackLight.Tag = "设备：缺料";
                        break;
                    case SystemIO.StackLightState.Notice:
                        toolStripStatusLabel_StackLight.ForeColor = Color.Black;
                        toolStripStatusLabel_StackLight.BackColor = Color.LightBlue;
                        toolStripStatusLabel_StackLight.Text = "设备：提示".txtTrans(TransType.Trans);
                        toolStripStatusLabel_StackLight.Tag = "设备：提示";
                        break;
                    case SystemIO.StackLightState.Adjust:
                        toolStripStatusLabel_StackLight.ForeColor = Color.Black;
                        toolStripStatusLabel_StackLight.BackColor = Color.LightGray;
                        toolStripStatusLabel_StackLight.Text = "设备：调试".txtTrans(TransType.Trans);
                        toolStripStatusLabel_StackLight.Tag = "设备：调试";
                        break;
                    case SystemIO.StackLightState.Alarm:
                        SwitchWnd(this.RoundButton_Alarm);
                        this.blinkColor = Color.Yellow;
                        toolStripStatusLabel_StackLight.ForeColor = Color.Black;
                        toolStripStatusLabel_StackLight.BackColor = Color.Yellow;
                        toolStripStatusLabel_StackLight.Text = "设备：报警".txtTrans(TransType.Trans);
                        toolStripStatusLabel_StackLight.Tag = "设备：报警";
                        break;
                    default:
                        break;
                }
            });
        }

        private void OnStationStateChanged(StationState OldState, StationState NewState)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            this.BeginInvoke((Action)delegate
            {
                if(StationMgr.IsPartPause || (OldState == StationState.Emg && NewState == StationState.Pause))
                {
                    RoundButton_Start.nImageIndex = 20;
                    RoundButton_Pause.nImageIndex = 23;
                    RoundButton_Stop.nImageIndex = 26;                    
                    toolStripStatusLabel_State.BackColor = Color.Yellow;
                    toolStripStatusLabel_State.ForeColor = Color.Black;
                    toolStripStatusLabel_State.Text = "站位: 部分暂停".txtTrans(TransType.Trans);
                    toolStripStatusLabel_State.Tag = "站位: 部分暂停";
                    return;
                }

                switch (NewState)
                {
                    case StationState.Manual:  //手动状态
                        RoundButton_Start.nImageIndex = 20;
                        RoundButton_Pause.nImageIndex = 23;
                        RoundButton_Stop.nImageIndex = 25;
                        toolStripStatusLabel_State.BackColor = Color.LightGray;
                        toolStripStatusLabel_State.ForeColor = Color.Black;
                        toolStripStatusLabel_State.Text = "站位: 停止".txtTrans(TransType.Trans);
                        toolStripStatusLabel_State.Tag = "站位: 停止";
                        break;
                    case StationState.Auto:   //自动运行状态
                        RoundButton_Start.nImageIndex = 19;
                        RoundButton_Pause.nImageIndex = 23;
                        RoundButton_Stop.nImageIndex = 26;                        
                        toolStripStatusLabel_State.BackColor = Color.DarkGreen;
                        toolStripStatusLabel_State.ForeColor = Color.White;
                        toolStripStatusLabel_State.Text = "站位: 运行".txtTrans(TransType.Trans);
                        toolStripStatusLabel_State.Tag = "站位: 运行";
                        break;
                    case StationState.Init:   //初始化状态
                        RoundButton_Start.nImageIndex = 19;
                        RoundButton_Pause.nImageIndex = 23;
                        RoundButton_Stop.nImageIndex = 26;                        
                        toolStripStatusLabel_State.BackColor = Color.DarkGreen;
                        toolStripStatusLabel_State.ForeColor = Color.White;
                        toolStripStatusLabel_State.Text = "站位: 初始化".txtTrans(TransType.Trans);
                        toolStripStatusLabel_State.Tag = "站位: 初始化";
                        break;
                    case StationState.Ready:  //等待开始
                        RoundButton_Start.nImageIndex = 19;
                        RoundButton_Pause.nImageIndex = 23;
                        RoundButton_Stop.nImageIndex = 26;                        
                        toolStripStatusLabel_State.BackColor = Color.DarkGreen;
                        toolStripStatusLabel_State.ForeColor = Color.White;
                        toolStripStatusLabel_State.Text = "站位: 待机".txtTrans(TransType.Trans);
                        toolStripStatusLabel_State.Tag = "站位: 待机";
                        break;
                    case StationState.Emg:         //急停状态
                        RoundButton_Start.nImageIndex = 20;
                        RoundButton_Pause.nImageIndex = 23;
                        RoundButton_Stop.nImageIndex = 25;
                        toolStripStatusLabel_State.BackColor = Color.Red;
                        toolStripStatusLabel_State.ForeColor = Color.White;
                        toolStripStatusLabel_State.Text = "站位: 急停".txtTrans(TransType.Trans);
                        toolStripStatusLabel_State.Tag = "站位: 急停";
                        break;
                    case StationState.Pause:       //暂停状态
                        RoundButton_Start.nImageIndex = 20;
                        RoundButton_Pause.nImageIndex = 22;
                        RoundButton_Stop.nImageIndex = 26;
                        toolStripStatusLabel_State.BackColor = Color.Yellow;
                        toolStripStatusLabel_State.ForeColor = Color.Black;
                        toolStripStatusLabel_State.Text = "站位: 暂停".txtTrans(TransType.Trans);
                        toolStripStatusLabel_State.Tag = "站位: 暂停";
                        break;
                    default:
                        break;
                }
            });
        }



        private void RoundButton_Click(object sender, EventArgs e)
        {
            SwitchWnd((RoundButton)sender);
        }

        /// <summary>
        /// 开始自动流程,如果是暂停则恢复运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoundButton_Start_Click(object sender, EventArgs e)
        {
            this.formSafetyDoor.Hide();
            Logger.Manual("点击启动按钮");
            StationMgr.StartButtonPressed();
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoundButton_Pause_Click(object sender, EventArgs e)
        {
            Logger.Manual("点击暂停按钮");
            StationMgr.Pause();
        }

        /// <summary>
        /// 结束自动流程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoundButton_Stop_Click(object sender, EventArgs e)
        {
            Logger.Manual("点击停止按钮");
            StationMgr.Stop();
        }

        /// <summary>
        /// 关闭程序,清空报警类数据
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">附带数据的对象</param>
        private void Form_Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            StationMgr.Stop();
            Logger.Manual("Autoframe shutdown.");

            foreach (KeyValuePair<RoundButton, Form> kp in m_dicForm)
                kp.Value.Close();
            Application.Exit();
        }

        /// <summary>
        /// 根据权限来登录点击的界面
        /// </summary>
        /// <param name="btn"></param>
        /// <returns></returns>
        public static void SwitchWnd(RoundButton btn)
        {
            if (m_currentButton == btn || !m_dicForm.ContainsKey(btn))
                return;


            if (m_currentButton != null /*&& m_currentButton.Enabled*/)
                m_currentButton.nImageIndex++;

            m_currentButton = btn;
            m_currentButton.nImageIndex--;


            if (m_currentForm != null)
                m_currentForm.Hide();
            if (m_currentForm != m_dicForm[btn])
            {
                m_currentForm = m_dicForm[btn];
                m_currentForm.Show();
            }
        }

        private void Form_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            switch (e.CloseReason)
            {
                //应用程序要求关闭窗口
                case CloseReason.ApplicationExitCall:
                    //e.Cancel = false; //不拦截，响应操作
                    break;
                //自身窗口上的关闭按钮
                case CloseReason.FormOwnerClosing:
                //MDI窗体关闭事件
                case CloseReason.MdiFormClosing:
                //用户通过UI关闭窗口或者通过Alt+F4关闭窗口
                case CloseReason.UserClosing:
                    if (DialogResult.No == MessageBox.Show("确认退出程序？".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
                        e.Cancel = true;//拦截，不响应操作
                    break;
                //不明原因的关闭
                case CloseReason.None:
                    break;
                //任务管理器关闭进程
                case CloseReason.TaskManagerClosing:
                    //e.Cancel = false;//不拦截，响应操作
                    break;

                //操作系统准备关机
                case CloseReason.WindowsShutDown:
                    //e.Cancel = false;//不拦截，响应操作
                    break;
                default:
                    break;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ProgramTool.CheckNoOperation();

            if(SystemIO.IsSafetyDoorEmpty())
            {
                if(this.blinkColor == Color.White)
                    this.blinkColor = Color.Yellow;
            }
            else
            {
                if(this.blinkColor == Color.White && !SystemIO.IsSafetyDoorClosed)
                    this.blinkColor = Color.Yellow;
            }

            if (this.tableLayoutPanel1.BackColor != this.blinkColor)
                this.tableLayoutPanel1.BackColor = this.blinkColor;
            else
                this.tableLayoutPanel1.BackColor = Color.White;
        }

        private void toolStripStatusLabel_SafetyDoor_Click(object sender, EventArgs e)
        {
            SystemIO.LockSafetyDoor();
            Logger.Manual("点击关闭所有安全门按钮");
        }

        private void toolStripStatusLabel_SafetyDoor_DoubleClick(object sender, EventArgs e)
        {
            switch (ProgramTool.stationState)
            {
                case StationState.Emg:
                case StationState.Manual:
                case StationState.Pause:
                    this.formSafetyDoor.Show();
                    break;
                default:
                    break;
            }            
        }

        private void toolStripDropDownButton_Process_DropDownOpening(object sender, EventArgs e)
        {
            switch (StationMgr.State)
            {
                case StationState.Auto:
                case StationState.Init:
                case StationState.Ready:
                case StationState.Emg:
                    foreach (var item in this.toolStripDropDownButton_Process.DropDownItems)
                        ((ToolStripItem)item).Visible = false;
                    break;
                case StationState.Pause:
                    foreach (var item in this.toolStripDropDownButton_Process.DropDownItems)
                        ((ToolStripItem)item).Visible = false;
                    this.toolStripMenuItem_Recipe.Visible = true;
                    break;
                case StationState.Manual:
                    foreach (var item in this.toolStripDropDownButton_Process.DropDownItems)
                        ((ToolStripItem)item).Visible = true;
                    break;
                default:
                    break;
            }
        }

        private void toolStripMenuItem_Speed_Click(object sender, EventArgs e)
        {
            UpdateSpeed((int)((ToolStripMenuItem)sender).Tag);
        }

        private void roundButton_MachineName_Click(object sender, EventArgs e)
        {
            if (PermissionMgr.existsPermition(eRoles.Administrator))
            {
                Form_Test test = new Form_Test();
                test.Show();
            }            
        }

        private void toolStripStatusLabel_StackLight_Click(object sender, EventArgs e)
        {
            if(EventMgr.IsAlarm)
                Form_Alarm_Current.ClearAlarm?.Invoke();
        }

        private void roundButton_User_Click(object sender, EventArgs e)
        {
            frmLogin fl = new frmLogin();
            fl.ShowDialog();
            Logger.Manual("登录用户：".txtTrans(TransType.Trans) + $"{PermissionMgr.Instance.GetName(SwitchName.AliasName)}[{PermissionMgr.Instance.GetName(SwitchName.UserName)}] -> {PermissionMgr.Instance.GetName(SwitchName.RoleName)}");
        }

        private void pictureBox_Quiet_Click(object sender, EventArgs e)
        {
            SystemIO.Quiet();
            this.pictureBox_Quiet.Visible = false;
        }
    }
}


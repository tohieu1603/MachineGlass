using AF.Common;
using AF.Data;
using AF.File;
using AF.Station;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Autoframe.UI
{
    public partial class Form_Auto_CustomPanel : UserControl
    {
        private object[] arrayDI = null;
        private object[] arrayDO = null;

        public Form_Auto_CustomPanel()
        {
            InitializeComponent();
        }

        public void Init(object[] arrayDI, object[] arrayDO)
        {
            this.arrayDI = arrayDI;
            this.arrayDO = arrayDO;
        }

        private void Form_Auto_CustomPanel_Load(object sender, EventArgs e)
        {

            if (arrayDI == null)
                this.groupBox_DI.Visible = false;
            else
                this.userControl_DI.Init(arrayDI, 0, Color.LightGray, SystemColors.Control);

            if (arrayDO == null)
                this.groupBox_DO.Visible = false;
            else
                this.userControl_DO.Init(arrayDO, 1, Color.LightGray, SystemColors.Control);

            ProgramTool.StationStateChangedEvent += OnStationStateChanged;
            OnStationStateChanged(StationState.Manual, StationMgr.State);
            Register.BitChangedEvent += OnRegBitChanged;
            IntRegBit();
        }

        private void OnStationStateChanged(StationState oldState, StationState newState)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                switch (newState)
                {
                    case StationState.Auto:
                    case StationState.Init:
                    case StationState.Ready:
                        this.userControl_DO.Enabled = false;
                        break;
                    case StationState.Emg:
                    case StationState.Pause:
                    case StationState.Manual:
                        this.userControl_DO.Enabled = true;
                        break;
                    default:
                        break;
                }
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (RegBit.扩膜允许清料.Get() && RegBit.摆臂允许清料.Get() && RegBit.IR检测需要清料.Get())
            {
                RegBit.IR载具清料.Set(true);
            }
        }


        /// <summary>寄存器位变化事件回调：只处理 IR载具清料 这一位</summary>
        private void OnRegBitChanged(int nIndex, bool bBit)
        {
            if (nIndex != (int)RegBit.强制OK && nIndex != (int)RegBit.扩膜允许清料 && nIndex != (int)RegBit.摆臂允许清料 && nIndex != (int)RegBit.IR载具清料 && nIndex != (int)RegBit.IR检测需要清料)
                return;
            else
            {
                if (this.IsDisposed || !this.IsHandleCreated)
                    return;
                this.BeginInvoke((Action)delegate
                {
                    switch (nIndex)
                    {
                        case (int)RegBit.强制OK:
                            if (bBit)
                            {
                                label1.BackColor = Color.Red;
                            }
                            else
                            {
                                label1.BackColor = Color.LightGray;
                            }
                            break;
                        case (int)RegBit.扩膜允许清料:
                            if (bBit)
                            {
                                checkBox4.Checked = true;
                                checkBox4.BackColor = Color.Green;
                            }
                            else
                            {
                                checkBox4.Checked = false;
                                checkBox4.BackColor = Color.LightGray;
                            }
                            break;
                        case (int)RegBit.摆臂允许清料:
                            if (bBit)
                            {
                                checkBox2.Checked = true;
                                checkBox2.BackColor = Color.Green;
                            }
                            else
                            {
                                checkBox2.Checked = false;
                                checkBox2.BackColor = Color.LightGray;
                            }
                            break;
                        case (int)RegBit.IR载具清料:
                            if (bBit)
                            {
                                button1.BackColor = Color.Red;
                            }
                            else
                            {
                                button1.BackColor = Color.LightGray;
                            }
                            break;
                        case (int)RegBit.IR检测需要清料:
                            if (bBit)
                            {
                                IR需要清料.Checked = true;
                                IR需要清料.BackColor = Color.Red;
                            }
                            else
                            {
                                IR需要清料.Checked = false;
                                IR需要清料.BackColor = Color.LightGray;
                            }
                            break;
                        default:
                            break;
                    }

                });
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {
            if (RegBit.强制OK.Get())
            {
                RegBit.强制OK.Set(false);
            }
            else
            {
                RegBit.强制OK.Set(true);
            }

        }
        private void IntRegBit()
        {

            if (RegBit.强制OK.Get())
            {
                label1.BackColor = Color.Red;
            }
            else
            {
                label1.BackColor = Color.LightGray;
            }
            if (RegBit.扩膜允许清料.Get())
            {
                checkBox4.Checked = true;
                checkBox4.BackColor = Color.Green;
            }
            else
            {
                checkBox4.Checked = false;
                checkBox4.BackColor = Color.LightGray;
            }
            if (RegBit.摆臂允许清料.Get())

            {
                checkBox2.Checked = true;
                checkBox2.BackColor = Color.Green;
            }
            else
            {
                checkBox2.Checked = false;
                checkBox2.BackColor = Color.LightGray;
            }
            if (RegBit.IR载具清料.Get())
            {
                button1.BackColor = Color.Red;
            }
            else
            {
                button1.BackColor = Color.LightGray;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult d = (MessageBox.Show($"之前数据\n\rOK: {Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount","")}" +
                $"\n\rNG: {Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", "")}\n\r更新? Yes: 写入 No: 取消", "Warning!", MessageBoxButtons.YesNo,MessageBoxIcon.Warning));
            if (d==DialogResult.Yes)
            {
                Ini.WriteValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount", numericUpDown1.Value.ToString());
                Ini.WriteValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", numericUpDown2.Value.ToString());
            }
        }
    }
}


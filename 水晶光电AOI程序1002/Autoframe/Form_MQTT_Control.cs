using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolEx;

namespace AutoFrame
{
    public partial class Form_MQTT_Control : Form
    {
        private static Logger _logger = LogManager.GetLogger("ChangeUserMode");
        DateTime _DT;
        public Form_MQTT_Control()
        {
            InitializeComponent();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            _DT = DateTime.Now;
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            DateTime KEYUP = DateTime.Now;

            TimeSpan TS = KEYUP.Subtract(_DT);
            if (TS.TotalMilliseconds > 50)
            {
                MessageBox.Show("请勿手动输入，请刷卡！！！");
                textBox1.Clear();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string text = textBox1.Text.Trim();
            if (text != "" && text.Length >= 10)
            {
                MQTT.DisConnectMqttServerAsync1();
                DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["AutoReconnect"].m_dbStandardValue = false;
                _logger.Error($"用户{textBox1.Text}关闭MQTT自动链接功能");
                this.Dispose();
                this.Close();
            }
            else
            {
                MessageBox.Show("工号不能为空！");
                return;
                    
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }
    }
}

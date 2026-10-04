using AutoFrame;
using AutoFrameDll;
using CommonTool;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using MQTTnet.Packets;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
//using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Shapes;
//using TCPLib;
using ToolEx;
using static AutoFrame.MQTT;
using static System.Windows.Forms.AxHost;

namespace AutoFrame
{
    public partial class Form_MqttClient : Form
    {
        private MqttClient mqttClient = null;
        private string filepath = SystemMgr.GetInstance().GetSystemParamString("MqttLogSavePath");
        private string filepath1 = SystemMgr.GetInstance().GetSystemParamString("Admin_MqttLogSavePath");
        public Form_MqttClient()
        {
            InitializeComponent();

        }


        /// <summary>
        /// 服务器连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MqttClient_Connected(object sender, EventArgs e)
        {
            Invoke((new Action(() =>
            {
                txtReceiveMessage.AppendText("已连接到MQTT服务器！" + Environment.NewLine);
            })));
        }

        /// <summary>
        /// 断开服务器连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MqttClient_Disconnected(object sender, EventArgs e)
        {
            Invoke((new Action(() =>
            {
                txtReceiveMessage.AppendText("已断开MQTT连接！" + Environment.NewLine);
            })));
        }

        /// <summary>
        /// 接收到消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MqttClient_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {

            Invoke((new Action(() =>
            {
                string s = ($"{Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}{Environment.NewLine}");
                textBox1.AppendText($"{s}");
            })));


        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSubscribe_ClickAsync(object sender, EventArgs e)
        {
            string topic = comboBox2.Text.Trim();

            if (string.IsNullOrEmpty(topic))
            {
                MessageBox.Show("订阅主题不能为空！");
                return;
            }

            if (!mqttClient.IsConnected)
            {
                MessageBox.Show("MQTT客户端尚未连接！");
                return;
            }

            //mqttClient.SubscribeAsync(new List<TopicFilter> {
            //    new TopicFilter(topic, MqttQualityOfServiceLevel.AtMostOnce)
            //});

            txtReceiveMessage.AppendText($"已订阅[{topic}]主题" + Environment.NewLine);
            //comboBox2.Enabled = false;
            //btnSubscribe.Enabled = false;
        }

        /// <summary>
        /// 发布主题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPublish_Click(object sender, EventArgs e)
        {
            string topic = comboBox1.Text;

            if (string.IsNullOrEmpty(topic))
            {
                MessageBox.Show("发布主题不能为空！");
                return;
            }

            string inputString = txtSendMessage.Text.Trim();
            MqttApplicationMessage appMsg = new MqttApplicationMessage();
            appMsg.Topic = topic;
            appMsg.PayloadSegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(inputString));
            appMsg.QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce;
            appMsg.Retain = false;
            try
            {
                mqttClient.PublishAsync(appMsg);
                //MachineDataTwe machineDataTwe = JsonConvert.DeserializeObject<MachineDataTwe>(message);
                //_logger.Info($"已发布消息{machineDataTwe.msg_id}");
            }
            catch
            {
                // StationEx.SaveMQTTInteractionLog(inputString + Environment.NewLine);
            }
        }

        private void FmMqttClient_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string message = "";
            string Data = "";

            double oldValue = 0;
            double newValue = 0;
            MQTT.ChangParam(oldValue, newValue, out message);

            txtSendMessage.AppendText(message);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ProductData productData = new ProductData();
            string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AssemInfo.ini");
            string Login = IniOperation.GetStringValue(iniPath, "systemdata", "Login", "");
            if(string.IsNullOrEmpty(Login))
            {
                MessageBox.Show("账号为空，请输入账号后在报工");
                return; 
            }
            string StrPutParam =SystemMgr.GetInstance().GetParamString("PutParam");//1,2,3,4
            switch(StrPutParam)
            {
                case "1,4":
                    productData.ProductInfo4[0].runCardNo = textBox14.Text.ToString().Trim();  //流程卡号
                    productData.ProductInfo4[0].TrayCode = textBox15.Text.ToString().Trim();  //清洗蓝码
                    productData.SteelRing.SteelSN = textBox16.Text.ToString().Trim();  //钢环码
                    productData.ProductInfo4[0].SNCode = textBox17.Text.ToString().Trim(); //中片码1

                    productData.ProductInfo4[3].runCardNo = textBox14.Text.ToString().Trim();  //流程卡号
                    productData.ProductInfo4[3].TrayCode = textBox15.Text.ToString().Trim();  //清洗蓝码
                    productData.SteelRing.SteelSN = textBox16.Text.ToString().Trim();  //钢环码
                    productData.ProductInfo4[3].SNCode = textBox18.Text.ToString().Trim(); //中片码2
                    productData.steelringcode = textBox16.Text.ToString().Trim();
                    break;
                case "2,3":
                    productData.ProductInfo4[1].runCardNo = textBox14.Text.ToString().Trim();  //流程卡号
                    productData.ProductInfo4[1].TrayCode = textBox15.Text.ToString().Trim();  //清洗蓝码
                    productData.SteelRing.SteelSN = textBox16.Text.ToString().Trim();  //钢环码
                    productData.ProductInfo4[1].SNCode = textBox17.Text.ToString().Trim(); //中片码1

                    productData.ProductInfo4[2].runCardNo = textBox14.Text.ToString().Trim();  //流程卡号
                    productData.ProductInfo4[2].TrayCode = textBox15.Text.ToString().Trim();  //清洗蓝码
                    productData.SteelRing.SteelSN = textBox16.Text.ToString().Trim();  //钢环码
                    productData.ProductInfo4[2].SNCode = textBox18.Text.ToString().Trim(); //中片码2
                    productData.steelringcode = textBox16.Text.ToString().Trim();
                    
                    break;
            }
            string Temp;
            MQTT.Changworkorder(productData, out Temp);

            txtSendMessage.AppendText(Temp);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string message = "";
            string Data = "";
            MQTT.ChangeState();
            txtSendMessage.AppendText(message);

        }

        private void button6_Click(object sender, EventArgs e)
        {
            string message = "";
            string Data = "";
            MQTT.ChangTechnology(Data, out message);

            txtSendMessage.AppendText(message);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string message = "";
            string Data = "";
            MQTT.ChangEMapping(Data, out message);

            txtSendMessage.AppendText(message);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            txtReceiveMessage.Clear();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            txtSendMessage.Clear();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //Task.Run(async () => { await ConnectMqttServerAsync(); });


        }

        private void button10_Click(object sender, EventArgs e)
        {
            Task.Run(async () => { await mqttClient.DisconnectAsync(); });
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string topic = comboBox2.Text.Trim();

            if (string.IsNullOrEmpty(topic))
            {
                MessageBox.Show("订阅主题不能为空！");
                return;
            }

            if (!mqttClient.IsConnected)
            {
                MessageBox.Show("MQTT客户端尚未连接！");
                return;
            }

            mqttClient.UnsubscribeAsync(topic);
            txtReceiveMessage.AppendText($"已取消订阅[{topic}]主题" + Environment.NewLine);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (textBox13.Text.ToString() == "")
            {
                MessageBox.Show("请输入测试二维码");
                return;
            }
            #region
            string message = "";
            ProductInfo data = new ProductInfo();
            //data.fileId = "成品AOI载具报工202402221319";
            //data.COLLECT_PALLET_SERIAL = "1";
            //data.m_dtOutPutTime = DateTime.Now ;
            //data.m_dtInputTime = DateTime.Now;
            //data.MOUTH_NO = "2";
            //data.Code = textBox13.Text.ToString();
            //data.FEED_SERIAL = "5";
            //data.COLLECT_BOX_CODE = "TVRABF4ZEQ0U61";
            //data.COLLECT_SERIAL = "3";
            //data.Testing106 = "7";
            //data.Reason = "良品";
            //data.m_bResult = "ok";
            //data.FEED_PALLET_SERIAL = "1";
            //data.Testing1 = "1";
            //data.Testing2 = "243";
            //data.Testing3 = "1";
            //data.Testing4 = "66.7";
            //data.Testing5 = "1";
            //data.Testing6 = "";
            //data.Testing7 = "";
            //data.Testing8 = "";
            //data.Testing9 = "";
            //data.Testing10 = "";

            //data.Testing11 = "";
            //data.Testing12 = "";
            //data.Testing13 = "";
            //data.Testing14 = "";
            //data.Testing15 = "";
            //data.Testing16 = "";
            //data.Testing17 = "2";
            //data.Testing18 = "790.4";
            //data.Testing19 = "";
            //data.Testing20 = "";

            //data.Testing21 = "";
            //data.Testing22 = "";
            //data.Testing23 = "";
            //data.Testing24 = "";
            //data.Testing25 = "";
            //data.Testing26 = "";
            //data.Testing27 = "";
            //data.Testing28 = "";
            //data.Testing29 = "1";
            //data.Testing30 = "653.1";
            //data.Testing31 = "";
            //data.Testing32 = "";
            //data.Testing33 = "";
            //data.Testing34 = "";
            //data.Testing35 = "";
            //data.Testing36 = "";
            //data.Testing37 = "";
            //data.Testing38 = "";

            //data.Testing39 = "";
            //data.Testing40 = "";
            //data.Testing41 = "";
            //data.Testing42 = "";
            //data.Testing43 = "";
            //data.Testing44 = "";
            //data.Testing45 = "";
            //data.Testing46 = "";
            //data.Testing47 = "";
            //data.Testing48 = "";
            //data.Testing49 = "";
            //data.Testing50 = "";
            //data.Testing51 = "";
            //data.Testing52 = "";
            //data.Testing53 = "";
            //data.Testing54 = "";
            //data.Testing55 = "1";
            //data.Testing56 = "432";

            //data.Testing57 = "";
            //data.Testing58 = "";
            //data.Testing59 = "3";
            //data.Testing60 = "1129";
            //data.Testing61 = "";
            //data.Testing62 = "";
            //data.Testing63 = "";
            //data.Testing64 = "";
            //data.Testing65 = "";
            //data.Testing66 = "";

            //data.Testing67 = "";
            //data.Testing68 = "";
            //data.Testing69 = "";
            //data.Testing70 = "";
            //data.Testing71 = "";
            //data.Testing72 = "";
            //data.Testing73 = "";
            //data.Testing74 = "";
            //data.Testing75 = "1";
            //data.Testing76 = "894.1";

            //data.Testing77 = "";
            //data.Testing78 = "";
            //data.Testing79 = "";
            //data.Testing80 = "";

            //data.Testing81 = "";
            //data.Testing82 = "";
            //data.Testing83 = "";
            //data.Testing84 = "";
            //data.Testing85 = "";
            //data.Testing86 = "";
            //data.Testing87 = "";
            //data.Testing88 = "";

            //data.Testing89 = "";
            //data.Testing90 = "";
            //data.Testing91 = "1";
            //data.Testing92 = "631.9";
            //data.Testing93 = "";
            //data.Testing94 = "";
            //data.Testing95 = "";
            //data.Testing96 = "";

            //data.Testing97 = "";
            //data.Testing98 = "";
            //data.Testing99 = "";
            //data.Testing100 = "";

            //data.Testing101 = "";
            //data.Testing102 = "";
            //data.Testing103 = "";
            //data.Testing104 = "";

            //data.Testing105 = "1";
            //data.Testing106 = "57";
            //data.Testing107 = "";
            //data.Testing108 = "";
            //data.Testing109 = "";
            //data.Testing110 = "";
            //data.Testing111 = "";
            //data.Testing112 = "";
            //data.Testing113 = "";
            //data.Testing114 = "";
            //data.Testing115 = "";
            //data.Testing116 = "";
            //data.Testing117 = "2";
            //data.Testing118 = "89751";
            //data.Testing119 = "";
            //data.Testing120 = "";

            //data.Testing121 = "1.2";
            //data.Testing122 = "2.01";
            //data.Testing123 = "1.32";
            //data.Testing124 = "4.5";
            //data.Testing125 = "6.1";
            //data.Testing126 = "11.1";
            //data.Testing127 = "14.2";
            //data.Testing128 = "1.01";
            //data.Testing129 = "4.31";
            //data.Testing130 = "1.02";
            //data.Testing131 = "2.33";
            //data.Testing132 = "4.54";
            //data.Testing133 = "2.346";
            //data.Testing134 = "1.97";
            //data.Testing135 = "1.25";
            #endregion

            if (checkBox1.Checked)
            {
                MQTT.ChangAOI(data, out message);
            }
            else if (checkBox2.Checked)
            {
                //MQTTEx.Admin_ChangAOI(data, out message);
            }
            else
            {
                MQTT.ChangAOI(data, out message);
            }


            txtSendMessage.AppendText(message);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string message = "";
            string Data = "";
            MQTT.ChangHttpPost(Data, out message);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            RootObject2 message;
            string SN = "U39C-2310120939-003";
            //MQTT.HttpGetPieceId(data,out message);
            MQTT.HttpGetRunCardNo(SN, out message);
        }

        private void roundButton1_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                MQTT.ConnectMqttServerAsync1();
            });
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] DecryptAes(string encryptedData, string key)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;

                    byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
                    byte[] keyBytes = Encoding.UTF8.GetBytes(key);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(keyBytes, aes.IV), CryptoStreamMode.Write))
                        {
                            cs.Write(encryptedBytes, 0, encryptedBytes.Length);
                            cs.Close();
                        }

                        return ms.ToArray();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        private void roundButton2_Click(object sender, EventArgs e)
        {
            try
            {
                string encryptedData = textBox1.Text; // 替换为实际的加密内容
                string key = "sjgd2023gyhlwiot"; // 替换为实际的密钥


                byte[] decryptedData = DecryptAes(encryptedData, key);


                //byte[] decryptedData1 = AesDecrypt(encryptedData, key);

                string decryptedText = Encoding.UTF8.GetString(decryptedData);

                txtSendMessage.AppendText(decryptedText);
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void button14_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }
        private void button15_Click(object sender, EventArgs e)
        {
            string message = "";
            string data = "CSLC0720003-020-02";
            MQTT.HttpGGetAoiBadPiece(out message);
        }
        private void button9_Click_1(object sender, EventArgs e)
        {
            string message = "";
            string pieceIdSN = "7cd302652f650f19";
            //strReturnMsg.Data[0].PIECE_ID,小件ID或小件二维码,strReturnMsg.Data[0].PIECE_STATUS:小件状态0正常1报废2异常3黑名单;
            //strReturnMsg.Data[0].RUN_CARD_NO:流程卡号;strReturnMsg.Data[0].GRADE:等级;
            //strReturnMsg.Data[0].LOCATION:单片位置;strReturnMsg.Data[0].DEFECT_CODE_ID:缺陷代码;
            //strReturnMsg.Data[0].DEFECT_OP_ID:站点名称;strReturnMsg.Data[0].MART_PART_NO:料号;
            MQTT.HttpGetPieceId(textBox17.Text.Trim(), out RootObject7 strReturnMsg);
        }

        private void button16_Click(object sender, EventArgs e)
        {

            string param = "?pieceId=7cd302652f650f19" + "&opId=1010" + "&flag=Y";
            List<string> strReturnMsg1 = new List<string>();
            string result = "\"{\\\"ErrCode\\\":20000,\\\"ErrMSG\\\":\\\"OK\\\",\\\"Data\\\":[{\\\"DOMAIN_ID\\\":\\\"T12\\\",\\\"PIECE_ID\\\":\\\"7cd302652f650f19\\\",\\\"PIECE_STATUS\\\":0,\\\"RUN_CARD_NO\\\":\\\"U39C-2310180016-006\\\",\\\"MART_PART_NO\\\":\\\"1511020000080\\\",\\\"GRADE\\\":\\\"A\\\",\\\"LOCATION\\\":\\\"4\\\",\\\"DEFECT_CODE_ID\\\":null,\\\"DEFECT_OP_ID\\\":null,\\\"LAST_OP_ID\\\":\\\"成品AOI\\\",\\\"PARENT_PIECE_ID\\\":\\\"U39C-2310060009-015-26-08\\\"}]}\"";
            //string result = "{\"ErrCode\":20000,\"ErrMSG\":\"OK\",\"Data\":[{\"DOMAIN_ID\":\"T12\",\"PIECE_ID\":\"7cd302652f650f19\",\"PIECE_STATUS\":0,\"RUN_CARD_NO\":\"U39C-2310180016-006\",\"MART_PART_NO\":\"1511020000080\",\"GRADE\":\"A\",\"LOCATION\":\"4\\\",\"DEFECT_CODE_ID\":null,\"DEFECT_OP_ID\":null,\"LAST_OP_ID\":\"成品AOI\",\"PARENT_PIECE_ID\":\"U39C-2310060009-015-26-08\"}]}";
            string unescapedJsonString = JsonConvert.DeserializeObject<string>(result);
            RootObject deserializedObject = JsonConvert.DeserializeObject<RootObject>(unescapedJsonString);
            foreach (Piece pipe in deserializedObject.Data)
            {

                string PIECE_ID = pipe.PIECE_ID;
                string PIECE_STATUS = pipe.PIECE_STATUS;
                string RUN_CARD_NO = pipe.RUN_CARD_NO;
                string GRADE = pipe.GRADE;
                string LOCATION = pipe.LOCATION;
                string DEFECT_CODE_ID = pipe.DEFECT_CODE_ID;
                string DEFECT_OP_ID = pipe.DEFECT_OP_ID;
                strReturnMsg1.Add(PIECE_ID);
                strReturnMsg1.Add(PIECE_STATUS);
                strReturnMsg1.Add(RUN_CARD_NO);
                strReturnMsg1.Add(GRADE);
                strReturnMsg1.Add(LOCATION);
                strReturnMsg1.Add(DEFECT_CODE_ID);
                strReturnMsg1.Add(DEFECT_OP_ID);
            }
            List<string> str = strReturnMsg1;
            string ss = str[3];
        }
        /// <summary>
        /// 4.7机台流程卡号校验测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button17_Click(object sender, EventArgs e)
        {
            string result = "{\"success\":true,\"message\":\"SUCCESS\",\"bizErrCode\":20000,\"bizErrMsg\":\"OK\",\"bizData\":{\"runCardNo\":\"U39C-2310120939-003\",\"woId\":\"U39C-2310120939\",\"domainId\":\"T12\",\"martPartNo\":\"1511020000081\",\"routeId\":\"MP-小件分厂-disco及端面丝印制程\",\"currentOpId\":\"成品AOI\",\"cardStatus\":1,\"runCartQty\":10000,\"pieceList\":null}}";
            RootObject2 RootObject = new RootObject2();

            var data = JsonConvert.DeserializeObject<RootObject2>(result);
            //RootObject.success = data.success;
            //RootObject.message = data.message;
            //string s = RootObject.Data[0];
            var b = data.bizData.domainId;
        }

        /// <summary>
        /// 补报工数据查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button20_Click(object sender, EventArgs e)
        {
            comboBox3.Items.Clear();
            DirectoryInfo folder = new DirectoryInfo(filepath);

            foreach (DirectoryInfo filepathname in folder.GetDirectories())
            {
                //Console.WriteLine(filepathname);
                comboBox3.Items.Add(filepathname);

            }
            comboBox3.SelectedIndex = 0;
            this.button36.Enabled = true;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if (StationMgr.GetInstance().IsAutoRunning() || StationMgr.GetInstance().IsPause())
            {
                return;
            }
            else
            {
                string filename = comboBox3.SelectedItem.ToString();
                Task.Run(() => { LoadingDate(filename); });
            }

        }
        /// <summary>
        /// 自动行号显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            e.Row.HeaderCell.Value = string.Format("{0}", e.Row.Index + 1);
        }

        private void dataGridView2_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            e.Row.HeaderCell.Value = string.Format("{0}", e.Row.Index + 1);
        }

        private void dataGridView3_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            e.Row.HeaderCell.Value = string.Format("{0}", e.Row.Index + 1);
        }
        //s
        private void button19_Click(object sender, EventArgs e)
        {
            if (StationMgr.GetInstance().IsAutoRunning() || StationMgr.GetInstance().IsPause())
            {
                return;
            }
            else
            {
                string filename = comboBox3.SelectedItem.ToString();
                Task.Run(() => { LoadingDate(filename); });
            }

            //MessageBox.Show("刷新成功！");
        }

        /// <summary>
        /// 加载报工数据
        /// </summary>
        private void LoadingDate(string filename)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                try
                {
                    DateTime DT=DateTime.Now;
                    List<string> str = new List<string>();
                    string OKfilepath = null;
                    string NGfilepath = null;
                    string Bufferpath = null;

                    if (string.IsNullOrEmpty(filename))
                    {
                        MessageBox.Show("请选择需要处理的工单号");
                        return;
                    }
                    string IDCarpath = System.IO.Path.Combine(filepath, filename);
                    DirectoryInfo folder = new DirectoryInfo(IDCarpath);
                    foreach (DirectoryInfo filepathname in folder.GetDirectories())
                    {
                        str.Add(filepathname.Name);
                    }
                    for (int i = 0; i < str.Count; i++)
                    {
                        if (str[i] == "OK")
                        {
                            OKfilepath = str[i];
                        }
                        if (str[i] == "NG")
                        {
                            NGfilepath = str[i];
                        }
                        if (str[i] == "Buffer")
                        {
                            Bufferpath = str[i];
                        }
                    }
                    this.dataGridView2.Rows.Clear();
                    int f = 0;
                    if (!string.IsNullOrEmpty(OKfilepath))
                    {
                        string OKfilepathname = System.IO.Path.Combine(IDCarpath, OKfilepath);
                        DirectoryInfo filepath = new DirectoryInfo(OKfilepathname);
                        textBox6.Text = filepath.GetFiles("*").Length.ToString();
                       
                        for (int i = 0; i < filepath.GetFiles("*").Length; i++)
                        {
                            var a = filepath.GetFiles("*")[i].Name.Split('.')[0];
                            Console.WriteLine(a);
                            if(a!=string.Empty)
                            {
                                int index = this.dataGridView2.Rows.Add();
                                dataGridView2.Rows[i].Cells[0].Value = a;
                                dataGridView2.Rows[i].Cells[1].Value = "已完成";

                                string SAS = filepath.GetFiles("*")[i].Name;
                                if (File.Exists(SAS))
                                {
                                    DT = File.GetLastWriteTimeUtc(SAS);
                                    
                                }
                                dataGridView2.Rows[i].Cells[2].Value = DT.ToString("yyyy-MM-dd HH:mm:ss");//SubString(filepath.GetFiles("*")[i].Name.Split('.')[1]);
                            }
                        }
                        f = filepath.GetFiles("*").Length;
                    }

                    if (!string.IsNullOrEmpty(NGfilepath))
                    {
                        string NGfilepathname = System.IO.Path.Combine(IDCarpath, NGfilepath);
                        DirectoryInfo filepath = new DirectoryInfo(NGfilepathname);
                        textBox3.Text = filepath.GetFiles("*").Length.ToString();
                        //this.dataGridView2.Rows.Clear();
                        for (int i = 0; i < filepath.GetFiles("*").Length; i++)
                        {
                          
                            string a = filepath.GetFiles("*")[i].Name.Split('.')[0];
                            if(a!=string.Empty)
                            {
                                this.dataGridView2.Rows.Add();
                                dataGridView2.Rows[f].Cells[0].Value = a;
                              
                                dataGridView2.Rows[f].Cells[1].Value = "报工失败";
                                string SAS = filepath.GetFiles("*")[i].Name;
                                if (File.Exists(SAS))
                                {
                                    DT = File.GetLastWriteTimeUtc(SAS);
                                }
                                dataGridView2.Rows[f].Cells[2].Value = DT.ToString("yyyy-MM-dd HH:mm:ss");//SubString(filepath.GetFiles("*")[i].Name.Split('-')[1]);
                                f++;
                            }
                        }
                        
                    }
                    if (!string.IsNullOrEmpty(Bufferpath))
                    {
                        string BUFFERfilepathname = System.IO.Path.Combine(IDCarpath, Bufferpath);
                        DirectoryInfo filepath = new DirectoryInfo(BUFFERfilepathname);
                        textBox4.Text = filepath.GetFiles("*").Length.ToString();
                         //this.dataGridView2.Rows.Clear();
                        for (int i = 0; i < filepath.GetFiles("*").Length; i++)
                        {
                            string a = filepath.GetFiles("*")[i].Name.Split('.')[0];
                            if(a != string.Empty) 
                            {
                                this.dataGridView2.Rows.Add();
                                dataGridView2.Rows[i + f].Cells[0].Value = a;
                                dataGridView2.Rows[i + f].Cells[1].Value = "缓存中";
                                string SAS = filepath.GetFiles("*")[i].Name;
                                if (File.Exists(SAS))
                                {
                                    DT = File.GetCreationTime(SAS);

                                }
                                dataGridView2.Rows[i + f].Cells[2].Value = DT.ToString("yyyy-MM-dd HH:mm:ss");//SubString(filepath.GetFiles("*")[i].Name.Split('-')[1]);
                                f++;
                            }
                        }
                    }
                }
                catch (Exception)
                {

                    //throw;
                }

            });

        }
        /// <summary>
        /// 单元格被选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int CIndex = e.RowIndex;
            if (CIndex >= 0 && CIndex < dataGridView2.Rows.Count)
            {
                if (dataGridView2.Rows[CIndex].Cells[0].Value != null)
                {
                    textBox5.Text = dataGridView2.Rows[CIndex].Cells[0].Value.ToString();
                    button21.Enabled = true;
                }
                else
                {
                    textBox5.Text = string.Empty;
                }

            }
            else
            {
                textBox5.Text = string.Empty;
                button21.Enabled = false;
            }
        }


        private string SubString(string sub)
        {
            string starttime = sub.Substring(0, 14);
            string dtime = starttime.Substring(0, 4) + "-" + starttime.Substring(4, 2) + "-" + starttime.Substring(6, 2) + " " + starttime.Substring(8, 2) + ":" + starttime.Substring(10, 2) + ":" + starttime.Substring(12, 2);
            //return DateTime.ParseExact(starttime, "yyyyMMddhhmmss", System.Globalization.CultureInfo.CurrentCulture);
            return dtime;
        }
        /// <summary>
        /// 单片产品补报工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button21_Click(object sender, EventArgs e)
        {
            string LogID = comboBox3.Text.ToString();
            string CCODE = this.textBox5.Text.Trim();
            MQTT.GetInstance().Returnpass(LogID, CCODE);
        }
        /// <summary>
        ///  所有报工失败物料补报工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button22_Click(object sender, EventArgs e)
        {
            string code = comboBox3.Text.ToString();
            MQTT.GetInstance().Returnpass(code);
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (Convert.ToInt32(textBox3.Text) > 0)
            {
                button22.Enabled = true;
            }
            else
            {
                button22.Enabled = false;
            }
        }
        private void button23_Click(object sender, EventArgs e)
        {

            MQTT.GetInstance().StartMonitor();
        }

        private void button24_Click(object sender, EventArgs e)
        {
            // 创建文件选择器对话框
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBox8.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void button25_Click(object sender, EventArgs e)
        {
            OpenFileDialog ss = new OpenFileDialog();
            DialogResult ssd = ss.ShowDialog();
            textBox7.Text = ss.FileName;
        }

        private void button26_Click(object sender, EventArgs e)
        {
            try
            {
                string[] Files1 = File.ReadAllLines(textBox7.Text.ToString());
                string[] Files2 = Directory.GetDirectories(textBox8.Text.ToString());
                textBox9.Text = textBox8.Text.ToString() + "\\NG1";
                if (!Directory.Exists(textBox9.Text)) Directory.CreateDirectory(textBox9.Text);
                if (textBox7.Text != null && textBox8.Text != null)
                {

                    for (int i = 0; i < Files2.Length; i++)
                    {
                        string[] file3 = Directory.GetFiles(Files2[i]);
                        if (Files2[i].ToString().Trim().Contains("OK") || Files2[i].ToString().Trim().Contains("NG") || Files2[i].ToString().Trim().Contains("Buffer"))
                        {
                            if (file3.Length != 0)
                            {
                                for (int j = 0; j < file3.Length; j++)
                                {
                                    for (int k = 0; k < Files1.Length; k++)
                                    {
                                        String A = file3[j].ToString().Trim();
                                        String B = Files1[k].ToString().Trim();
                                        if (A.Contains(B) && B != "")
                                        {
                                            //File.Copy(file3[j], file3[j].Replace("NG", "qqq"));
                                            if (A.Contains("OK"))
                                            {

                                                File.Copy(A, A.Replace("OK", "NG1"));
                                                File.Delete(A);
                                            }
                                            if (A.Contains("NG"))
                                            {
                                                File.Copy(A, A.Replace("NG", "NG1"));
                                                File.Delete(A);
                                            }
                                            if (A.Contains("Buffer"))
                                            {
                                                File.Copy(A, A.Replace("Buffer", "NG1"));
                                                File.Delete(A);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception EX)
            {

                MessageBox.Show("检索失败" + EX.ToString());
            }

        }
        /// <summary>
        /// 二维码补传
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button27_Click(object sender, EventArgs e)
        {
            string[] Files = Directory.GetFiles(textBox9.Text.ToString());

            MessageBox.Show($"当前路径共{Files.Length}颗物料,点击确定补报工");
            string LogID = comboBox3.Text.ToString();
            MQTT.GetInstance().Returnpass1(LogID);
        }

        private void button28_Click(object sender, EventArgs e)
        {
            RootObject3 res;
            string martPartNo = "1511020000080";
            string pieceTraySN = "QXBZ-04-113005-230313-2873";
            string strReturnMsg;
            HttpGetAoiFileId(pieceTraySN, out res);
            string a = res.bizData.fileId;
            string b = res.bizData.vehicleNo;
            string c = res.bizData.expire;
            string d = res.bizData.alertMsg;

        }

        private void button28_Click_1(object sender, EventArgs e)
        {
            //string devicenumber = textBox10.Text.ToString().Trim();
            //string msg_id = textBox11.Text.ToString().Trim();
            //string lot_id = textBox12.Text.ToString().Trim();
            //MQTT.GetInstance().aaa(lot_id, devicenumber, msg_id);
            //string Temp;
            ////OP模式
            //ProductData data=new ProductData();
            //MQTT.Changworkorder(data, out Temp);
        }

        private void button29_Click(object sender, EventArgs e)
        {
            string code = comboBox3.Text.ToString();
            Task.Run(() =>
            {

                MQTT.GetInstance().Returnpass(code);
            });
        }

        private void button30_Click(object sender, EventArgs e)
        {
            //String MESSAGE;
            //MQTT.ReportAOINGEnd(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), out MESSAGE);
        }

        private void button31_Click(object sender, EventArgs e)
        {
            //String MESSAGE;
            //MQTT.ReportAOIOKEnd("TAAAAJ9M1CAH41", "0", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),out MESSAGE);
        }

        private void button32_Click(object sender, EventArgs e)
        {

            CotMesTask.GetInstance().StartMonitor();
            MessageBox.Show("启动成功！");

        }



        private void button36_Click(object sender, EventArgs e)
        {
            string filepath_last = filepath + "";
            comboBox3.Items.Clear();
            DirectoryInfo folder = new DirectoryInfo(filepath_last);
            foreach (DirectoryInfo filepathname in folder.GetDirectories())
            {
                comboBox3.Items.Add(filepathname);

            }
            comboBox3.SelectedIndex = 0;

            this.button18.Enabled = true;
            this.button19.Enabled = true;
        }

        private void button37_Click(object sender, EventArgs e)
        {
            MQTT.RootObject5 rootObject5 = null;
            MQTT.HttpGGetbyPiece(textBox13.Text, out rootObject5);
            MessageBox.Show(JsonConvert.SerializeObject(rootObject5));
        }

        private void button20_Click_1(object sender, EventArgs e)
        {
            RootObject3 res;
            string martPartNo = textBox_runCardNo.Text.Trim();
            string pieceTraySN = textBox16.Text.Trim();
            string strReturnMsg;
            HttpGetAoiFileId(pieceTraySN, out res);
            //string a = res.bizData.fileId;
            //string b = res.bizData.vehicleNo;
            //string c = res.bizData.expire;
            //string d = res.bizData.alertMsg;
        }

        private void button34_Click(object sender, EventArgs e)
        {

            MQTT.HttpGetSteelSNId(textBox14.Text.Trim(), textBox_runCardNo.Text.Trim(), out RootObject2 strReturnMsg);
        }

        private void button33_Click(object sender, EventArgs e)
        {
            //RootObject4 rootObject4 = new RootObject4();
            //MQTT.HttpGetEQSoftVersion(out rootObject4);
            //MessageBox.Show(rootObject4.bizErrMsg.ToString());
            RootObject4 rootObject4 = new RootObject4();
            MQTT.HttpGetEQSoftVersion(out rootObject4);
            MessageBox.Show(rootObject4.bizErrMsg.ToString());
        }

        private void button35_Click(object sender, EventArgs e)
        {
            List<string> list = new List<string>();
            string da ="";
            for (int i = 1; i < 5; i++)
            {
                da = DataMgr.GetInstance().m_dictDataGroup[$"前移载调整平台{i}上下限"].m_dictData["X"].m_dbOffset.S;
                list.Add(da); 
                da = DataMgr.GetInstance().m_dictDataGroup[$"前移载调整平台{i}上下限"].m_dictData["Y"].m_dbOffset.S;
                list.Add(da);
            }
            for (int i = 1; i < 5; i++)
            {
                da = DataMgr.GetInstance().m_dictDataGroup[$"后移载调整平台{i}补偿"].m_dictData["X"].m_dbOffset.S;
                list.Add(da);
                da = DataMgr.GetInstance().m_dictDataGroup[$"后移载调整平台{i}补偿"].m_dictData["Y"].m_dbOffset.S;
                list.Add(da);
            }
            RootObject6 rootObject6 = new RootObject6();
            MQTT.HttpGetMachineParamCheckBefore(list, out rootObject6);
            MessageBox.Show(rootObject6.bizErrMsg.ToString());
        }

        private void button38_Click(object sender, EventArgs e)
        {
            string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AssemInfo.ini");
            string Login = IniOperation.GetStringValue(iniPath, "systemdata", "Login", "");
            if (string.IsNullOrEmpty(Login))
            {
                MessageBox.Show("账号为空，请输入账号后在报工");
                return;
            }
            ProductData productData = new ProductData();
            productData.ProductInfo4[1].runCardNo = textBox14.Text.ToString().Trim();  //流程卡号
            productData.ProductInfo4[1].TrayCode = textBox15.Text.ToString().Trim();  //清洗蓝码
            productData.SteelRing.SteelSN = textBox16.Text.ToString().Trim();  //钢环码
            productData.ProductInfo4[1].SNCode = textBox17.Text.ToString().Trim(); //中片码1

            productData.ProductInfo4[2].runCardNo = textBox14.Text.ToString().Trim();  //流程卡号
            productData.ProductInfo4[2].TrayCode = textBox15.Text.ToString().Trim();  //清洗蓝码
            productData.SteelRing.SteelSN = textBox16.Text.ToString().Trim();  //钢环码
            productData.ProductInfo4[2].SNCode = textBox18.Text.ToString().Trim(); //中片码2
            productData.steelringcode = textBox16.Text.ToString().Trim();
            string Temp;
            MQTT.SelectMode(productData, out Temp);

            txtSendMessage.AppendText(Temp);
        }
    }
}
using AutoFrameDll;
using CommonTool;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Packets;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using ToolEx;
using static AutoFrame.MQTT;
using static AutoFrame.MQTT.MachineDataNotOpMode;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AutoFrame
{
    enum ChineseToEnglish
    {
        AssembleTheCCDZ_axis = 0,
        PatchX_Axis,
        PatchY_Axis,
        PatchZ_Axis,
        PatchU_Axis,
        SteelRingX_Axis,
        SteelRingY_Axis,
        SteelRingZ_Axis,
        SteelRingU_Axis,
        LiftingSteelRingY_Axis,
        LaydownSteelRingY_Axis,
        SteelRringTransplantationLeftX_Axis,
        SteelRringTransplantationRightX_Axis,
        FilmCoatedRingExcisionZ_Axis,
        FilmCoatedRingExcisionR_Axis,
        LabelingModuleY_Axis,
        LabelingModuleZ_Axis,
        LabelingModuleU_Axis,
        RemoveBackingFilmU_Axis,
        CollectWasteFilmU_Axis,
        FilmdrawingY_Axis,
        PressedFilmY_Axis,
        AssembleX1_Axis,
        AssembleY1_Axis,
        AssembleZ1_Axis,
        AssembleX2_Axis,
        AssembleY2_Axis,
        AssembleZ2_Axis,
        AssembleX3_Axis,
        AssembleY3_Axis,
        AssembleZ3_Axis,
        AssembleX4_Axish,
        AssembleY4_Axis,
        AssembleZ4_Axis,
        ClipUnlockU_Axis,
        CleaningBasketTransplantationY_Axis,
        CleaningBasketLoadingZ_Axis,
        CleaningBasketPickuUpX_Axis,
        CleaningBasketPickuUpY_Axis,
        CleaningBasketPickuUpZ_Axis,
        CleaningBasketPickuUpU_Axis,
        MiddleTransferLatformU_Axis,
    }


    class MQTT : SingletonTemplate<MQTT>
    {
        public static readonly object lockO = new object();
        public static readonly object lockSend = new object();
        public static readonly object lockReceiver = new object();
        public static readonly object lockConn = new object();
        public static readonly object lockParam = new object();
        private static Logger mqtt_logger = LogManager.GetLogger("mqtt");
        private static Logger http_logger = LogManager.GetLogger("http");
        private static Logger status_logger = LogManager.GetLogger("status");
        private static Logger Admin_mqtt_logger = LogManager.GetLogger("Admin_mqtt");
        /// <summary>
        /// 服务是否连接
        /// </summary>
        public static bool ConStatue = false;
        public static MqttClient mqttClient = null;
        public List<string> TimeList = new List<string> { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24" };
        /// <summary>
        /// 显示Log改变委托
        /// </summary>
        public delegate void ShowLogChangeHander(string log);

        /// <summary>
        /// 显示数据改变事件
        /// </summary>
        public event ShowLogChangeHander SendLogChangeEvent;

        /// <summary>
        /// MQTT改变委托
        /// </summary>
        public delegate void MqttChangeHander();
        /// <summary>
        ///MQTT改变事件
        /// </summary>
        public static event MqttChangeHander Mqtt1ChangeEvent;
        /// <summary>
        ///报工成功改变事件
        /// </summary>
        public static event MqttChangeHander Mqtt2ChangeEvent;
        /// <summary>
        ///报工失败改变事件
        /// </summary>
        public static event MqttChangeHander Mqtt3ChangeEvent;
        /// <summary>
        ///报工未响应改变事件
        /// </summary>
        public static event MqttChangeHander Mqtt4ChangeEvent;
        /// <summary>
        /// 保存数据改变事件
        /// </summary>
        public event ShowLogChangeHander ReceiverLogChangeEvent;
        /// <summary>
        /// 接受到消息的集合
        /// </summary>
        public static ConcurrentDictionary<string, MqttReceiver> DicReceiver = new ConcurrentDictionary<string, MqttReceiver>();

        /// <summary>
        /// 正在等待回复的消息集合
        /// </summary>
        public static ConcurrentDictionary<string, WaitReceiver> msg_idlist = new ConcurrentDictionary<string, WaitReceiver>();

        /// <summary>
        /// 非操作者模式接受到消息的集合
        /// </summary>
        public static ConcurrentDictionary<string, MqttReceiver> Admin_DicReceiver = new ConcurrentDictionary<string, MqttReceiver>();
        /// <summary>
        /// 非操作者模式正在等待回复的消息集合
        /// </summary>
        public static ConcurrentDictionary<string, WaitReceiver> Admin_msg_idlist = new ConcurrentDictionary<string, WaitReceiver>();


        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <returns></returns>
        public static bool ConnectMqttServerAsync1()
        {

            lock (lockConn)
            {
                try
                {
                    if (mqttClient == null || !mqttClient.IsConnected)
                    {
                        mqttClient = new MqttFactory().CreateMqttClient() as MqttClient;
                        mqttClient.ConnectedAsync += MqttClient_Connected();
                        mqttClient.DisconnectedAsync += MqttClient_Disconnected();
                        mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceived();
                    }
                    if (mqttClient.IsConnected) return true;
                    MqttClientOptions options = new MqttClientOptions();
                    options.ChannelOptions = new MqttClientTcpOptions
                    {
                        Server = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["ip"].m_dbStandardValue.ToString(),
                        Port = Convert.ToInt32(DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["port"].m_dbStandardValue.ToString()),

                    };
                    options.ClientId = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString() + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString();
                    options.Credentials = new MqttClientCredentials(
                       DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["Username"].m_dbStandardValue.ToString(),
                       Encoding.UTF8.GetBytes(DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["Psw"].m_dbStandardValue.ToString())
                    );
                    options.CleanSession = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["CleanSession"].m_dbStandardValue.B;
                    options.KeepAlivePeriod = TimeSpan.FromSeconds(DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["KeepAliveInterval"].m_dbStandardValue.D);
                    switch (DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["ProtocolVersion"].m_dbStandardValue.ToString())
                    {
                        case "V310":
                            options.ProtocolVersion = MqttProtocolVersion.V310;
                            break;
                        case "V311":
                            options.ProtocolVersion = MqttProtocolVersion.V311;
                            break;
                        case "V500":
                            options.ProtocolVersion = MqttProtocolVersion.V500;
                            break;
                        default:
                            options.ProtocolVersion = MqttProtocolVersion.V311;
                            break;
                    }
                    //options.CleanSession = true;
                    //options.Timeout = TimeSpan.FromSeconds(5);
                    //byte[] Temp = Encoding.Default.GetBytes(ClientId + "与服务器断开连接");
                    //options.WillMessage = new MqttApplicationMessage("", Temp, MqttQualityOfServiceLevel.AtMostOnce,false);
                    status_logger.Trace("向MQTT发起了连接请求！");
                    mqttClient.ConnectAsync(options).Wait(DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["ConnectionTimeout"].m_dbStandardValue.I);
                    return mqttClient.IsConnected;
                }
                catch (Exception ex)
                {

                    status_logger.Trace($"连接到MQTT服务器失败！");
                    return false;

                }
            }

        }



        public static bool DisConnectMqttServerAsync1()
        {

            if (mqttClient != null)
            {
                Task t = mqttClient.DisconnectAsync();
                t.Wait();
            }


            return mqttClient.IsConnected;

        }

        /// <summary>
        /// 服务器连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static Func<MqttClientConnectedEventArgs, Task> MqttClient_Connected()
        {
            return async (args) =>
            {
                await Task.Delay(500);
                ConStatue = true;
                status_logger.Trace("已连接MQTT服务器！" + Environment.NewLine);
                Mqtt1ChangeEvent();
            };
        }

        /// <summary>
        /// 断开服务器连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static Func<MqttClientDisconnectedEventArgs, Task> MqttClient_Disconnected()
        {
            return async (args) =>
            {
                await Task.Delay(500);
                ConStatue = false;
                if (args.Exception != null)
                {
                    status_logger.Trace("MQTT异常断开：" + $"{args.Exception.Message}");
                    status_logger.Trace("MQTT异常断开：" + $"{args.Reason}");
                    if (args.Exception.InnerException != null)
                    {
                        status_logger.Trace("MQTT异常断开：" + args.Exception.InnerException.Message);
                    }
                }
                else
                {
                    status_logger.Trace("MQTT正常断开：" + args.Reason);
                }
                Mqtt1ChangeEvent();
            };

        }

        /// <summary>
        /// 接收到消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static Func<MqttApplicationMessageReceivedEventArgs, Task> MqttClient_ApplicationMessageReceived()
        {
            return async (args) =>
            {
                await Task.Delay(10);
                string strReceiver = "";
                ArraySegment<byte> payloadSegment = args.ApplicationMessage.PayloadSegment;
                strReceiver = $"{Encoding.UTF8.GetString(payloadSegment.Array, payloadSegment.Offset, payloadSegment.Count)}{Environment.NewLine}";
                MqttReceiver mqttReceiver = JsonConvert.DeserializeObject<MqttReceiver>(strReceiver);
                try
                {



                    //如果是要监听的信息就缓存起来
                    if (msg_idlist.Keys.Contains(mqttReceiver.msg_id))
                    {
                        DicReceiver.TryAdd(mqttReceiver.msg_id, mqttReceiver);
                        mqtt_logger.Info($"收到正常模式的报工回复:MSG_ID:{JsonConvert.SerializeObject(mqttReceiver)}");
                    }
                    else if (Admin_msg_idlist.Keys.Contains(mqttReceiver.msg_id))
                    {
                        Admin_DicReceiver.TryAdd(mqttReceiver.msg_id, mqttReceiver);
                        Admin_mqtt_logger.Info($"接收到非操作员模式下的报工回复:MSG_ID:{JsonConvert.SerializeObject(mqttReceiver)}");
                    }
                }
                catch (Exception ex)
                {
                    mqtt_logger.Info($"mqtt报工回复异常，注意排查,:MSG_ID:{mqttReceiver.msg_id}_{mqttReceiver.res_msg}" + ex.ToString());
                }
            };
        }
        /// <summary>
        /// 保存Log数据
        /// </summary>
        /// <param name="wReceiver"></param>
        /// <param name="Rulte"></param>
        public void SaveMQTTInteractionLog(WaitReceiver wReceiver, bool Rulte)
        {
            lock (lockO)
            {
                try
                {
                    string orderSN = wReceiver.Lot_ID;
                    string hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("MqttLogSavePath"), orderSN.Substring(0, 8), "OK");
                    if (!Rulte)
                    {
                        hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("MqttLogSavePath"), orderSN.Substring(0, 8), "NG");

                    }
                    if (!Directory.Exists(hiveDirectory)) Directory.CreateDirectory(hiveDirectory);
                    string fileNamelog = wReceiver.fileNamelog;
                    fileNamelog = wReceiver.ProductCode + ".txt";
                    fileNamelog = Path.Combine(hiveDirectory, fileNamelog);
                    File.AppendAllText(fileNamelog, wReceiver.Send_msg + "@" + "\r\n" + JsonConvert.SerializeObject(wReceiver.MqttReceiver));
                    //删除缓存的数据
                    if (wReceiver.fileNamelog != null)
                    {

                        hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("MqttLogSavePath"), orderSN.Substring(0, 8), "Buffer");
                        fileNamelog = wReceiver.fileNamelog;
                        fileNamelog = Path.Combine(hiveDirectory, fileNamelog);
                        File.Delete(fileNamelog);
                    }
                }
                catch (Exception e)
                {
                    mqtt_logger.Warn("报工数据记录失败");
                    mqtt_logger.Warn(e.Message);
                    //MessageBox.Show(e.Message);
                    //WarningMgr.GetInstance().Warning("报工数据记录失败");
                }
            }


        }
        /// <summary>
        /// 报工内容缓存
        /// </summary>
        /// <param name="wReceiver"></param>
        public static void SaveMQTTBufferlog(WaitReceiver wReceiver)
        {
            try
            {
                string orderSN = wReceiver.Lot_ID;
                string hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("MqttLogSavePath"), orderSN.Substring(0, 8), "Buffer");
                if (!Directory.Exists(hiveDirectory)) Directory.CreateDirectory(hiveDirectory);
                string fileNamelog = wReceiver.fileNamelog;
                fileNamelog = Path.Combine(hiveDirectory, fileNamelog);

                File.AppendAllText(fileNamelog, wReceiver.Send_msg + "@" + "\r\n" + JsonConvert.SerializeObject(wReceiver.MqttReceiver));
            }
            catch (Exception e)
            {
                //WarningMgr.GetInstance().Warning("报工数据缓存失败");
                mqtt_logger.Warn("报工数据记录失败");
                mqtt_logger.Warn(e.Message);
            }
        }
        /// <summary>
        /// 报工内容缓存
        /// </summary>
        /// <param name="wReceiver"></param>
        public static void SaveMQTTStemplog(WaitReceiver wReceiver)
        {
            try
            {
                string orderSN = wReceiver.Lot_ID;
                string hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("MqttLogSavePath"), DateTime.Now.ToString("yyyy-MM-dd"), "NG");
                if (!Directory.Exists(hiveDirectory)) Directory.CreateDirectory(hiveDirectory);
                string fileNamelog = wReceiver.fileNamelog;
                fileNamelog = Path.Combine(hiveDirectory, fileNamelog);

                File.AppendAllText(fileNamelog, wReceiver.Send_msg + "@" + "\r\n" + JsonConvert.SerializeObject(wReceiver.MqttReceiver));
            }
            catch (Exception e)
            {
                //WarningMgr.GetInstance().Warning("报工数据缓存失败");
                mqtt_logger.Warn("报工数据记录失败");
                mqtt_logger.Warn(e.Message);
            }
        }
        /// <summary>
        /// 补报工缓存内容
        /// </summary>
        /// <param name="wReceiver"></param>
        public static void ReSaveMQTTBufferlog(WaitReceiver wReceiver, string LogID)
        {
            try
            {
                //string orderSN = SystemMgr.GetInstance().GetRegString((int)SysStrReg.OrderSN) + "_" + SystemMgr.GetInstance().GetRegString((int)SysStrReg.OrderCreatTime);
                string hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("MqttLogSavePath"), "MQTT", "Buffer");
                if (!Directory.Exists(hiveDirectory)) Directory.CreateDirectory(hiveDirectory);
                string fileNamelog = wReceiver.fileNamelog;
                fileNamelog = Path.Combine(hiveDirectory, fileNamelog);

                File.AppendAllText(fileNamelog, wReceiver.Send_msg + "@" + "\r\n" + JsonConvert.SerializeObject(wReceiver.MqttReceiver));
            }
            catch (Exception e)
            {
                //WarningMgr.GetInstance().Warning("报工数据缓存失败");
                mqtt_logger.Warn("报工数据记录失败");
                mqtt_logger.Warn(e.Message);
            }
        }
        /// <summary>
        /// 补传所有报工失败物料数据
        /// </summary>
        /// <param name="Lot_ID">工单号</param>
        public void Returnpass(string LogID)
        {

            try
            {
                for (int j = 0; j < 24; j++)
                {
                    string hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("MqttLogSavePath"), LogID, LogID + TimeList[j], "NG");
                    if (Directory.Exists(hiveDirectory))
                    {
                        string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString() + ".reply";
                        string topic1 = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString();
                        string[] Files = Directory.GetFiles(hiveDirectory);
                        if (Files.Length > 0)
                        {
                            List<string> strlist = new List<string>();
                            for (int i = 0; i < Files.Length; i++)
                            {
                                if (ConStatue)
                                {
                                    string strData = File.ReadAllText(Files[i]);

                                    string code = Path.GetFileNameWithoutExtension(Files[i]);
                                    //截取文件名中的产品码
                                    string strCoed = Path.GetFileNameWithoutExtension(Files[i]).Substring(0, 16);
                                    string Send_msg = strData.Substring(0, strData.IndexOf("@"));
                                    MachineDataTwe MS = JsonConvert.DeserializeObject<MachineDataTwe>(Send_msg);
                                    MS.data = DESEncrypt(JsonConvert.SerializeObject(MS.data), "sjgd2023gyhlwiot");

                                    File.Copy(Files[i], Files[i].Replace("NG", "Buffer"));
                                    File.Delete(Files[i]);
                                    Thread.Sleep(10);
                                    WaitReceiver wReceiver = new WaitReceiver();
                                    MqttReceiver mqttRceeiver = new MqttReceiver();
                                    wReceiver.Msg_id = MS.msg_id;
                                    wReceiver.Lot_ID = LogID + TimeList[j];
                                    wReceiver.ProductCode = strCoed;
                                    wReceiver.fileNamelog = Path.GetFileName(Files[i]);
                                    wReceiver.Send_msg = Send_msg;
                                    wReceiver.fileNamelog = Path.GetFileName(Files[i]);
                                    wReceiver.Wait_time = 0;
                                    msg_idlist.TryAdd(MS.msg_id, wReceiver);
                                    Mqtt_Subscribe(topic);
                                    Send_msg = JsonConvert.SerializeObject(MS);
                                    Mqtt_Send(topic1, Send_msg);
                                    Mqtt4ChangeEvent();
                                    Thread.Sleep(200);
                                }
                                else
                                {
                                    MessageBox.Show("补报工失败，MQTT已断开连接，请检查后重新进行补报！！！");
                                    return;
                                }
                            }
                        }

                    }
                    else
                    {
                        mqtt_logger.Info($"补传数据文件{LogID}不存在，无需补传！" + Environment.NewLine);
                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
                //row;
            }

        }
        public void Returnpass1(string LogID)
        {

            string hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("MqttLogSavePath"), "MQTT", LogID, "NG1");
            //  string hiveDirectory1 = Path.Combine(SystemMgr.GetInstance().GetParamString("MqttLogSavePath"), "MQTT", Lot_ID, "Buffer");
            if (Directory.Exists(hiveDirectory))
            {
                string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString() + ".reply";
                string topic1 = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString();
                string[] Files = Directory.GetFiles(hiveDirectory);
                List<string> strlist = new List<string>();
                for (int i = 0; i < Files.Length; i++)
                {
                    if (ConStatue)
                    {
                        string strData = File.ReadAllText(Files[i]);
                        File.Copy(Files[i], Files[i].Replace("NG1", "Buffer"));
                        //补报时删除NG文件夹内的数据
                        File.Delete(Files[i]);
                        string code = Path.GetFileNameWithoutExtension(Files[i]);
                        //截取文件名中的产品码
                        string strCoed = Path.GetFileNameWithoutExtension(Files[i]).Substring(0, 16);
                        string Send_msg = strData.Substring(0, strData.IndexOf("@"));
                        MachineDataTwe MS = JsonConvert.DeserializeObject<MachineDataTwe>(Send_msg);
                        WaitReceiver wReceiver = new WaitReceiver();
                        MqttReceiver mqttRceeiver = new MqttReceiver();
                        wReceiver.Msg_id = MS.msg_id;
                        wReceiver.Lot_ID = LogID;
                        wReceiver.Loation_flow_card = LogID;
                        wReceiver.ProductCode = strCoed;
                        wReceiver.Send_msg = Send_msg;
                        wReceiver.fileNamelog = Path.GetFileName(Files[i]);
                        wReceiver.Wait_time = 0;
                        msg_idlist.TryAdd(MS.msg_id, wReceiver);
                        Mqtt_Subscribe(topic);
                        Mqtt_Send(topic1, Send_msg);
                        Mqtt4ChangeEvent();
                        Thread.Sleep(200);
                    }
                    else
                    {
                        MessageBox.Show("补报工失败，MQTT已断开连接，请检查后重新进行补报！！！");
                    }
                }
            }
        }
        /// <summary>
        /// 3.8单片补报工
        /// </summary>
        /// <param name="Lot_ID">工单号</param>
        /// <param name="SNcode">二维码</param>
        public void Returnpass(string Lot_ID, string SNcode)
        {
            string dicpath, path;
            string hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetSystemParamString("MqttLogSavePath"), Lot_ID, "NG");
            //string hiveDirectory1 = Path.Combine(SystemMgr.GetInstance().GetParamString("MqttLogSavePath"), "MQTT", Lot_ID, "Buffer");
            //if (Directory.Exists(hiveDirectory))
            {
                string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["workorderreply"].m_dbStandardValue.ToString() + ".reply";
                string topic1 = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["workorder"].m_dbStandardValue.ToString();
                string[] Files = Directory.GetFiles(hiveDirectory);
                List<string> strlist = new List<string>();
                for (int i = 0; i < Files.Length; i++)
                {
                    if (Files[i].ToString().Contains(SNcode))
                    {
                        if (ConStatue)
                        {
                            string strData = File.ReadAllText(Files[i]);

                            dicpath = Files[i].Replace("NG", "Buffer");
                            path = hiveDirectory.Replace("NG", "Buffer");
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            File.Copy(Files[i], Files[i].Replace("NG", "Buffer"));
                            //补报时删除NG文件夹内的数据
                            File.Delete(Files[i]);
                            string code = Path.GetFileNameWithoutExtension(Files[i]);
                            //截取文件名中的产品码
                            string strCoed = Path.GetFileNameWithoutExtension(Files[i]).Substring(0, 19);
                            string Send_msg = strData.Substring(0, strData.IndexOf("@"));
                            MachineDataTwe MS = JsonConvert.DeserializeObject<MachineDataTwe>(Send_msg);
                            MS.data = DESEncrypt(JsonConvert.SerializeObject(MS.data), "sjgd2023gyhlwiot");

                            WaitReceiver wReceiver = new WaitReceiver();
                            MqttReceiver mqttRceeiver = new MqttReceiver();
                            wReceiver.Msg_id = MS.msg_id;
                            wReceiver.Lot_ID = Lot_ID;
                            wReceiver.Loation_flow_card = Lot_ID;
                            wReceiver.ProductCode = strCoed;
                            wReceiver.Send_msg = Send_msg;
                            wReceiver.fileNamelog = Path.GetFileName(Files[i]);
                            wReceiver.Wait_time = 0;
                            msg_idlist.TryAdd(MS.msg_id, wReceiver);
                            Mqtt_Subscribe(topic);
                            Send_msg = JsonConvert.SerializeObject(MS);
                            Mqtt_Send(topic1, Send_msg);
                            Thread.Sleep(200);
                        }
                        else
                        {
                            MessageBox.Show("补报工失败，MQTT已断开连接，请检查后重新进行补报！！！");
                            return;
                        }
                    }

                    // Directory.Move(Files[i], hiveDirectory1);


                }
            }
        }
        /// <summary>
        /// 自动补报TEMP数据
        /// </summary>
        /// <param name="Lot_ID"></param>
        public void AutoReturnpass(string Lot_ID)
        {
            try
            {
                for (int j = 1; j < 25; j++)
                {
                    string hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("LogSavePath"), DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("yyyyMMdd") + TimeList[j], "NG");
                    if (Directory.Exists(hiveDirectory))
                    {
                        string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString() + ".reply";
                        string topic1 = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString();
                        string[] Files = Directory.GetFiles(hiveDirectory);
                        List<string> strlist = new List<string>();
                        for (int i = 0; i < Files.Length; i++)
                        {
                            if (ConStatue)
                            {
                                string strData = File.ReadAllText(Files[i]);
                                File.Copy(Files[i], Files[i].Replace("NG", "Buffer"));
                                File.Delete(Files[i]);
                                string code = Path.GetFileNameWithoutExtension(Files[i]);
                                //截取文件名中的产品码
                                string strCoed = Path.GetFileNameWithoutExtension(Files[i]).Substring(0, 16);
                                string Send_msg = strData.Substring(0, strData.IndexOf("@"));
                                MachineDataTwe MS = JsonConvert.DeserializeObject<MachineDataTwe>(Send_msg);
                                MS.data = DESEncrypt(JsonConvert.SerializeObject(MS.data), "sjgd2023gyhlwiot");

                                Thread.Sleep(10);
                                WaitReceiver wReceiver = new WaitReceiver();
                                MqttReceiver mqttRceeiver = new MqttReceiver();
                                wReceiver.Msg_id = MS.msg_id;
                                wReceiver.Lot_ID = Lot_ID;
                                wReceiver.ProductCode = strCoed;
                                wReceiver.fileNamelog = Path.GetFileName(Files[i]);
                                wReceiver.Send_msg = Send_msg;
                                wReceiver.fileNamelog = Path.GetFileName(Files[i]);
                                wReceiver.Wait_time = 0;
                                msg_idlist.TryAdd(MS.msg_id, wReceiver);
                                Mqtt_Subscribe(topic);
                                Send_msg = JsonConvert.SerializeObject(MS);
                                Mqtt_Send(topic1, Send_msg);
                                Thread.Sleep(200);
                            }
                            else
                            {
                                MessageBox.Show("补报工失败，MQTT已断开连接，请检查后重新进行补报！！！");
                                return;
                            }
                        }
                    }
                    else
                    {
                        mqtt_logger.Info(DateTime.Now.ToString("yyyyMMddhhmmssfff") + $"补传数据文件{Lot_ID + TimeList[j]}不存在，无需补传！" + Environment.NewLine);
                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
                //row;
            }

        }

        /// <summary>
        /// 获取反馈主题
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public string GetRetopic(string topic)
        {
            string Retopic = "";
            switch (topic)
            {
                case "data.device.workorder.report":
                    Retopic = "device.workreport.reply.twe - porro";
                    break;
                case "data.device.aoi":
                    Retopic = "data.device.aoi.reply";
                    break;
                case "data.device.emap":
                    Retopic = "data.device.emap.reply";
                    break;
                default:
                    Retopic = "未定义反馈主题";
                    break;

            }

            return Retopic;

        }

        public override void ThreadMonitor()
        {
            while (this.m_bRunThread)
            {
                Thread.Sleep(500);
                try
                {
                    ConcurrentDictionary<string, WaitReceiver> msg_idlistTemp = new ConcurrentDictionary<string, WaitReceiver>(msg_idlist);

                    foreach (var item in msg_idlistTemp.Keys)
                    {

                        WaitReceiver wReceiver = msg_idlistTemp[item];
                        if (DicReceiver.ContainsKey(wReceiver.Msg_id))
                        {

                            wReceiver.MqttReceiver = DicReceiver[item];
                            DicReceiver.TryRemove(item, out MqttReceiver Receiver1);
                            msg_idlist.TryRemove(item, out WaitReceiver Receiver2);
                            if (wReceiver.MqttReceiver.res_code == "20000")
                            {
                                SaveMQTTInteractionLog(wReceiver, true);
                                //Mqtt2ChangeEvent();
                            }
                            else
                            {
                                SaveMQTTInteractionLog(wReceiver, false);
                                //Mqtt3ChangeEvent();
                            }
                            // ReceiverLogChangeEvent(JsonConvert.SerializeObject(wReceiver.mqttReceiver));
                        }
                        else
                        {
                            wReceiver.Wait_time += 1000;
                        }
                        if (wReceiver.Wait_time > 16000)
                        {
                            msg_idlist.TryRemove(item, out WaitReceiver Receiver3);
                            SaveMQTTInteractionLog(wReceiver, false);
                            //Mqtt3ChangeEvent();
                        }

                    }
                }
                catch (Exception ex)
                {
                    //ShowLog(ex.ToString());
                    mqtt_logger.Warn("MQTT线程监控捕获异常，注意排查" + ex.ToString());
                }
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        internal static void Mqtt_Send(string topic, string message)
        {
            if (!ConStatue)
            {
                mqtt_logger.Warn("MQTT服务器已断开，请重新连接！");
            }

            MqttApplicationMessage appMsg = new MqttApplicationMessage();
            appMsg.Topic = topic;
            appMsg.PayloadSegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            switch (DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["QOS"].m_dbStandardValue.ToString())
            {
                case "0":
                    appMsg.QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce;
                    break;
                case "1":
                    appMsg.QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce;
                    break;
                case "2":
                    appMsg.QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce;
                    break;
                default:
                    appMsg.QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce;
                    break;
            }
            appMsg.Retain = false;
            MachineDataTwe machineDataTwe = JsonConvert.DeserializeObject<MachineDataTwe>(message);
            try
            {
                mqttClient.PublishAsync(appMsg);

                mqtt_logger.Info($"已发布消息{machineDataTwe.msg_id}");
            }
            catch (Exception ex)
            {
                mqtt_logger.Info($"发布消息失败：{machineDataTwe.msg_id}" + ex.ToString());
            }

        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        internal static void Mqtt_Subscribe(string topic)
        {
            try
            {
                MqttTopicFilter TopicFilter = new MqttTopicFilterBuilder().WithTopic(topic).Build();
                mqttClient.SubscribeAsync(TopicFilter, CancellationToken.None);
            }
            catch (Exception)
            {
                mqtt_logger.Info($"订阅主题失败:{topic}");
            }

        }

        /// <summary>
        /// 取消订阅主题
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        internal static void Mqtt_Cancel(string topic)
        {
            mqttClient.UnsubscribeAsync(topic);
            mqtt_logger.Info($"已取消订阅[{topic}]主题" + Environment.NewLine);

        }

        /// <summary>  
        /// 将c# DateTime时间格式转换为13位时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        private static long ConvertDateTimeToInt(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }
        /// <summary>
        /// 从配置文件读取轴配置参数
        /// </summary>
        /// <returns></returns>
        public static List<string[]> RedAxisConfigSave()
        {
            List<string[]> strlist = new List<string[]>();
            string strParams = "";
            string strParamsValues = "";
            string strParamsSn = "";
            List<string> st = DataMgr.GetInstance().m_dictDataGroup["轴速度参数管理"].m_dictData.Keys.ToList();
            int SystemSpeed = SystemMgr.GetInstance().GetParamInt("SystemSpeed");
            for (int i = 0; i < st.Count; i++)
            {
                if (i < 3)
                {
                    strParams += st[i] + ",";
                    strParamsSn += DataMgr.GetInstance().m_dictDataGroup["轴速度参数管理"].
                    m_dictData[st[i]].m_dbOffset.S + ",";
                    strParamsValues += (DataMgr.GetInstance().m_dictDataGroup["轴速度参数管理"].
                        m_dictData[st[i]].m_dbStandardValue.I).ToString() + ",";
                    strParamsValues += DataMgr.GetInstance().m_dictDataGroup["轴速度参数管理"].
                      m_dictData[st[i]].m_dbOffset.S + ",";
                    continue;
                }
                if (i != st.Count - 1)
                {
                    strParams += st[i] + ",";
                    strParamsValues += (DataMgr.GetInstance().m_dictDataGroup["轴速度参数管理"].
                        m_dictData[st[i]].m_dbLimitU.D / 100 / 0.9 * SystemSpeed).ToString() + ",";

                    strParamsSn += DataMgr.GetInstance().m_dictDataGroup["轴速度参数管理"].
                        m_dictData[st[i]].m_dbOffset.S + ",";

                    DataMgr.GetInstance().m_dictDataGroup["轴速度参数管理"].m_dictData[st[i]].m_dbStandardValue =
                       (DataMgr.GetInstance().m_dictDataGroup["轴速度参数管理"].m_dictData[st[i]].m_dbLimitU.D / 100 / 0.9 * SystemSpeed).ToString() + ",";
                }
                else
                {
                    strParams += st[i];
                    strParamsSn += DataMgr.GetInstance().m_dictDataGroup["轴速度参数管理"].
                        m_dictData[st[i]].m_dbOffset.S;
                    strParamsValues += DataMgr.GetInstance().m_dictDataGroup["轴速度参数管理"].
                        m_dictData[st[i]].m_dbStandardValue.S;
                }
            }
            strlist.Add(strParams.Split(','));
            strlist.Add(strParamsSn.Split(','));
            strlist.Add(strParamsValues.Split(','));
            return strlist;
        }
        /// <summary>
        /// 从配置文件读取相机配置参数
        /// </summary>
        /// <returns></returns>
        public static List<string> RedCamerConfigSave()
        {
            List<string> strlist = new List<string>();
            #region
            string strParamsName = "";
            string strParamsValues = "";
            List<string> st = DataMgr.GetInstance().m_dictDataGroup["相机曝光参数管理"].m_dictData.Keys.ToList();

            for (int i = 0; i < st.Count; i++)
            {
                strParamsName = st[i];
                strParamsValues = DataMgr.GetInstance().m_dictDataGroup["相机曝光参数管理"].m_dictData[strParamsName].m_dbStandardValue.ToString();
                strlist.Add(strParamsName);
                strlist.Add(strParamsValues);
            }

            //strlist.Add(strParamsSn.Split(','));
            //strlist.Add(strParamsValues.Split(','));
            #endregion
            //MotionMgr.m_dictAxisCfg;
            return strlist;
        }
        /// <summary>
        /// 3.1设备参数上报
        /// </summary>      
        public static void ChangParam(object oldValue, object newValue, out string message)
        {
            lock (lockParam)
            {
                string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["parameters"].m_dbStandardValue.ToString();
                MachineDataparam MS = new MachineDataparam();
                DateTime time = DateTime.Now;
                long Time = ConvertDateTimeToInt(time);
                MS.msg_id = "ps-" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString() + "-" + Time;
                MS.deviceno = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString();
                MS.dataType = "MES-ParamSetting";//""MES-ParamSetting//ParamEditRecord
                MS.deviceType = "";
                List<MachineDataparam.DataItem> data = new List<MachineDataparam.DataItem>();
                
                for (int i = 1; i < 5; i++) 
                {
                    MachineDataparam.DataItem dt0 = new MachineDataparam.DataItem();
                    dt0.o_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    dt0.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                    dt0.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();
                    dt0.paramName = $"前移载调整平台{i}上下限X补偿";
                    dt0.paramSn = $"MQBeforeGapX{i}";
                    dt0.paramValue = DataMgr.GetInstance().m_dictDataGroup[$"前移载调整平台{i}上下限"].m_dictData["X"].m_dbOffset.ToString();
                    dt0.timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    data.Add(dt0);
                }

                for (int i = 1; i <5; i++)
                {
                    MachineDataparam.DataItem dt5 = new MachineDataparam.DataItem();
                    dt5.o_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    dt5.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                    dt5.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();
                    dt5.paramName = $"前移载调整平台{i}上下限Y补偿";
                    dt5.paramSn = $"MQBeforeGapY{i}";
                    dt5.paramValue = DataMgr.GetInstance().m_dictDataGroup[$"前移载调整平台{i}上下限"].m_dictData["Y"].m_dbOffset.ToString();
                    dt5.timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    data.Add(dt5);
                }

                for (int i = 1; i < 5; i++)
                {
                    MachineDataparam.DataItem dt6 = new MachineDataparam.DataItem();
                    dt6.o_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    dt6.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                    dt6.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();
                    dt6.paramName = $"后移载调整平台{i}上下限X补偿";
                    dt6.paramSn = $"MQBehandGapX{i}";
                    dt6.paramValue = DataMgr.GetInstance().m_dictDataGroup[$"后移载调整平台{i}补偿"].m_dictData["X"].m_dbOffset.ToString();
                    dt6.timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    data.Add(dt6);
                }
             

                for(int i = 1; i < 5; i++)
                {
                    MachineDataparam.DataItem dt7 = new MachineDataparam.DataItem();

                    dt7.o_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    dt7.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                    dt7.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();
                    dt7.paramName = $"后移载调整平台{i}上下限Y补偿";
                    dt7.paramSn = $"MQBehandGapY{i}";
                    dt7.paramValue = DataMgr.GetInstance().m_dictDataGroup[$"后移载调整平台{i}补偿"].m_dictData["Y"].m_dbOffset.ToString();
                    dt7.timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    data.Add(dt7);
                }





                MachineDataparam.DataItem dt = new MachineDataparam.DataItem();

                dt.o_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dt.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                dt.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();
                dt.paramName = $"覆膜环切站环切U轴速度";
                dt.paramSn = "MQRoundUSpeed";
                dt.paramValue = DataMgr.GetInstance().m_dictDataGroup[$"覆膜环切站"].m_dictData["环切U轴速度"].m_dbStandardValue.ToString();
                dt.timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                data.Add(dt);



                MachineDataparam.DataItem dt1 = new MachineDataparam.DataItem();

                dt1.o_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dt1.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                dt1.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();
                dt1.paramName = $"覆膜环切站开UV灯的百分比";
                dt1.paramSn = "MQUVLightP";
                dt1.paramValue = DataMgr.GetInstance().m_dictDataGroup[$"覆膜环切站"].m_dictData["开UV灯的百分比"].m_dbStandardValue.ToString();
                dt1.timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                data.Add(dt1);
                MachineDataparam.DataItem dt2 = new MachineDataparam.DataItem();
                dt2.o_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dt2.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                dt2.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();
                dt2.paramName = $"覆膜环切站环切道具压力";
                dt2.paramSn = "MQRoundPress";
                dt2.paramValue = DataMgr.GetInstance().m_dictDataGroup[$"覆膜环切站"].m_dictData["环切道具压力"].m_dbStandardValue.ToString();
                dt2.timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                data.Add(dt2);

                MachineDataparam.DataItem dt3 = new MachineDataparam.DataItem();
                dt3.o_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dt3.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                dt3.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();
                dt3.paramName = $"覆膜站辊压压力前移载";
                dt3.paramSn = "MQBeforePress";
                dt3.paramValue = DataMgr.GetInstance().m_dictDataGroup[$"覆膜站辊压压力"].m_dictData["前移载"].m_dbOffset.ToString();
                dt3.timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                data.Add(dt3);

                MachineDataparam.DataItem dt4 = new MachineDataparam.DataItem();

                dt4.o_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dt4.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                dt4.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();
                dt4.paramName = $"覆膜站辊压压力后移载";
                dt4.paramSn = "MQBehandPress";
                dt4.paramValue = DataMgr.GetInstance().m_dictDataGroup[$"覆膜站辊压压力"].m_dictData["后移载"].m_dbOffset.ToString();
                dt4.timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                data.Add(dt4);

                SaveParamCsv(data);
                MS.data = DESEncrypt(JsonConvert.SerializeObject(data), "sjgd2023gyhlwiot");
                message = JsonConvert.SerializeObject(MS);
                if (string.IsNullOrEmpty(topic))
                {
                    System.Windows.Forms.MessageBox.Show("发布主题不能为空！");
                    return;
                }
                Mqtt_Send(topic, message);
            }
        }

        /// <summary>
        /// 加工工艺上报
        /// </summary>      
        public static void ChangTechnology(string Data, out string message)
        {

            //string process = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["process"].m_dbStandardValue.ToString() + ".reply";

            //MQTT.Mqtt_Subscribe(process);

            string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["process"].m_dbStandardValue.ToString();

            MachineDataTechnology MS = new MachineDataTechnology();
            DateTime time = DateTime.Now;
            long Time = ConvertDateTimeToInt(time);
            MS.msg_id = "per-" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString() + "-" + Time;
            MS.deviceno = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString();
            MS.dataType = "ParamEditRecord";
            MS.deviceType = "";

            List<MachineDataTechnology.DataItem> data = new List<MachineDataTechnology.DataItem>();
            MachineDataTechnology.DataItem dt = new MachineDataTechnology.DataItem();
            dt.o_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");           //记录时间
            dt.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();            //机台号  
            dt.o_workStation = "";            //工站编号 
            dt.paramName = "";           //参数名称
            dt.paramSn = "";         //参数序号
            dt.paramValue = "";            //参数值
            dt.timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");            //参数值            
            data.Add(dt);
            MS.data = DESEncrypt(JsonConvert.SerializeObject(data), "sjgd2023gyhlwiot");
            message = JsonConvert.SerializeObject(MS);
            //HiveMgr.GetInstance().UploadInformation(MS, out message, CreateFile);
            Mqtt_Send(topic, message);
        }

        /// <summary>
        /// 工具采集数据上报
        /// </summary>      
        public static void ChangEMapping(string Data, out string message)
        {
            //string emap = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["emap"].m_dbStandardValue.ToString() + ".reply";

            //MQTT.Mqtt_Subscribe(emap);

            string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["emap"].m_dbStandardValue.ToString();
            MachineDataEMapping MS = new MachineDataEMapping();
            DateTime time = DateTime.Now;
            long Time = ConvertDateTimeToInt(time);
            MS.msg_id = "emap-" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString() + "-" + Time;
            MS.deviceno = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString();
            MS.dataType = "EMAP";
            MS.deviceType = "";
            List<MachineDataEMapping.DataItem> data = new List<MachineDataEMapping.DataItem>();
            MachineDataEMapping.DataItem dt = new MachineDataEMapping.DataItem();
            dt.o_workStation = "P2130";          //工站编号
            dt.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();          //机台号  
            dt.w_vehicleNo = "";          //载具二维码  
            dt.runCardId = "";              //流程卡号
            dt.pieceId = "";          //单片二维码
            dt.w_location = "";            //单片位置
            dt.level = "";              //等级
            dt.curtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");            //生成日期
            data.Add(dt);
            MS.data = DESEncrypt(JsonConvert.SerializeObject(data), "sjgd2023gyhlwiot");
            message = JsonConvert.SerializeObject(MS);
            Mqtt_Send(topic, message);
        }
        /// <summary>
        /// 自动贴片机设备报工
        /// </summary>      
        public static void Changworkorder(ProductData Data, out string message)
        {
            string workorder = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["workorderreply"].m_dbStandardValue.ToString();
            Mqtt_Subscribe(workorder);
            string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["workorder"].m_dbStandardValue.ToString();
            MachineDataworkorder MS = new MachineDataworkorder();
            DateTime time = DateTime.Now;
            long Time = ConvertDateTimeToInt(time);

            MS.msg_id = "wpd-" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString() + "-" + Time;
            MS.deviceno = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString();
            MS.dataType = "WorkReportData";
            MS.deviceType = workorder;
            string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AssemInfo.ini");
            string Login = IniOperation.GetStringValue(iniPath, "systemdata", "Login", "");
            List<MachineDataworkorder.DataItem> data = new List<MachineDataworkorder.DataItem>();

            MachineDataworkorder.DataItem dt1 = new MachineDataworkorder.DataItem();
            MachineDataworkorder.DataItem dt2 = new MachineDataworkorder.DataItem();

            List<治具数组> remark1 = new List<治具数组>();

            List<治具数组> remark2 = new List<治具数组>();
            治具数组 piece1 = new 治具数组();
            治具数组 piece2 = new 治具数组();
            治具数组 piece3 = new 治具数组();
            治具数组 piece4 = new 治具数组();

            if (SystemMgr.GetInstance().GetParamString("PutParam") == "1,4")
            {
                #region 中片一
                //清洗蓝进入时间辅助载具
                piece1.createTime = Data.m_dtCarrierBeginTime.ToString("yyyy-MM-dd HH:mm:ss");
                //载具类型1进入
                piece1.subVehicleType = "1";
                //进入载具码
                piece1.subVehicleCode = Data.ProductInfo4[0].TrayCode;
                piece1.vehicleCode = Data.SteelRing.SteelSN;
                piece1.location = "";
                piece1.qty = "";
                remark1.Add(piece1);

                //钢环时间主载具（回收）
                piece2.createTime = Data.m_dtCarrierEndTime.ToString("yyyy-MM-dd HH:mm:ss");
                //载具类型2（回收）
                piece2.subVehicleType = "2";
                //回收载具码（回收）
                piece2.subVehicleCode = Data.ProductInfo4[0].TrayCode;
                piece2.vehicleCode = Data.SteelRing.SteelSN;
                piece2.location = "";
                piece2.qty = "";
                remark1.Add(piece2);


                dt1.runCardId = Data.ProductInfo4[0].runCardNo;        //流程卡号    非必填
                dt1.pieceId = Data.ProductInfo4[0].SNCode;       //单片二维码    非必填
                dt1.w_location = "1";   //单片位置    非必填
                dt1.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();        //工站编号
                dt1.w_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");        //生成日期
                dt1.w_vehicleNo = Data.SteelRing.SteelSN;        //载具二维码    非必填
                dt1.goodProductNumber = "1";        //良品数量
                dt1.defectCode = "OK";        //物料结果
                dt1.o_operator = Login;        //作业人员工号
                dt1.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();        //机台号
                dt1.coatingTrackingNumber = "";       //镀膜跟踪号    非必填
                dt1.p1RunCardId = "";//p1流程卡    非必填
                dt1.p3RunCardId = "";//p3流程卡    非必填
                dt1.level = "";        //等级    非必填
                dt1.w_judge = "";
                dt1.remark1 = "";
                dt1.remark2 = remark1;
                dt1.remark3 = "";
                dt1.parentPieceId = "";    //父单片码
                dt1.fileId = Data.FileID;
                if(SystemMgr.GetInstance().GetParamBool("ProductWithOutScannerCode"))
                {
                    data.Add(dt1);
                }
                else
                {
                    if (dt1.pieceId != string.Empty)
                    {
                        data.Add(dt1);
                    }
                }
              

                #endregion
                #region 中片四
                //清洗蓝进入时间辅助载具
                piece3.createTime = Data.m_dtCarrierBeginTime.ToString("yyyy-MM-dd HH:mm:ss");
                //载具类型1进入
                piece3.subVehicleType = "1";
                //进入载具码
                piece3.subVehicleCode = Data.ProductInfo4[3].TrayCode;
                piece3.vehicleCode = Data.SteelRing.SteelSN;
                piece3.location = "";
                piece3.qty = "";
                remark2.Add(piece3);

                //钢环时间主载具（回收）
                piece4.createTime = Data.m_dtCarrierEndTime.ToString("yyyy-MM-dd HH:mm:ss");
                //载具类型2（回收）
                piece4.subVehicleType = "2";
                //回收载具码（回收）
                piece4.subVehicleCode = Data.ProductInfo4[3].TrayCode;
                piece4.vehicleCode = Data.SteelRing.SteelSN;
                piece4.location = "";
                piece4.qty = "";
                remark2.Add(piece4);


                dt2.runCardId = Data.ProductInfo4[3].runCardNo;        //流程卡号    非必填
                dt2.pieceId = Data.ProductInfo4[3].SNCode;       //单片二维码    非必填
                dt2.w_location = "4";   //单片位置    非必填
                dt2.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();        //工站编号
                dt2.w_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");        //生成日期
                dt2.w_vehicleNo = Data.SteelRing.SteelSN;        //载具二维码    非必填
                dt2.goodProductNumber = "1";        //良品数量
                dt2.defectCode = "OK";        //物料结果
                dt2.o_operator = Login;        //作业人员工号
                dt2.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();        //机台号
                dt2.coatingTrackingNumber = "";       //镀膜跟踪号    非必填
                dt2.p1RunCardId = "";//p1流程卡    非必填
                dt2.p3RunCardId = "";//p3流程卡    非必填
                dt2.level = "";        //等级    非必填
                dt2.w_judge = "";
                dt2.remark1 = "";
                dt2.remark2 = remark2;
                dt2.remark3 = "";
                dt2.parentPieceId = "";    //父单片码
                dt2.fileId = Data.FileID;
                if (SystemMgr.GetInstance().GetParamBool("ProductWithOutScannerCode"))
                {
                    data.Add(dt2);
                }
                else
                {
                    if (dt2.pieceId != string.Empty)
                    {
                        data.Add(dt2);
                    }
                }
             
                #endregion
            }
            else
            {
                //Data.ProductInfo4[1].TrayCode = "QXL202506DD180240";
                //Data.ProductInfo4[2].TrayCode = "QXL202506DD180240";
                //Data.ProductInfo4[1].runCardNo = "U39C-2506180018-001";
                //Data.ProductInfo4[2].runCardNo = "U39C-2506180018-001";
                #region 中片二
                //清洗蓝进入时间辅助载具
                piece1.createTime = Data.m_dtCarrierBeginTime.ToString("yyyy-MM-dd HH:mm:ss");
                //载具类型1进入
                piece1.subVehicleType = "1";
                //进入载具码
                piece1.subVehicleCode = Data.ProductInfo4[1].TrayCode;
                piece1.vehicleCode = Data.SteelRing.SteelSN;
                piece1.location = "";
                piece1.qty = "";
                remark1.Add(piece1);

                //钢环时间主载具（回收）
                piece2.createTime = Data.m_dtCarrierEndTime.ToString("yyyy-MM-dd HH:mm:ss");
                //载具类型2（回收）
                piece2.subVehicleType = "2";
                //回收载具码（回收）
                piece2.subVehicleCode = Data.ProductInfo4[1].TrayCode;
                piece2.vehicleCode = Data.SteelRing.SteelSN;
                piece2.location = "";
                piece2.qty = "";
                remark1.Add(piece2);


                dt1.runCardId = Data.ProductInfo4[1].runCardNo;        //流程卡号    非必填
                dt1.pieceId = Data.ProductInfo4[1].SNCode;       //单片二维码    非必填
                dt1.w_location = "2";   //单片位置    非必填
                dt1.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();        //工站编号
                dt1.w_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");        //生成日期
                dt1.w_vehicleNo = Data.SteelRing.SteelSN;        //载具二维码    非必填
                dt1.goodProductNumber = "1";        //良品数量
                dt1.defectCode = "OK";        //物料结果
                dt1.o_operator = Login;        //作业人员工号
                dt1.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();        //机台号
                dt1.coatingTrackingNumber = "";       //镀膜跟踪号    非必填
                dt1.p1RunCardId = "";//p1流程卡    非必填
                dt1.p3RunCardId = "";//p3流程卡    非必填
                dt1.level = "";        //等级    非必填
                dt1.w_judge = "";
                dt1.remark1 = "";
                dt1.remark2 = remark1;
                dt1.remark3 = "";
                dt1.parentPieceId = "";    //父单片码
                dt1.fileId = Data.FileID;
                if (SystemMgr.GetInstance().GetParamBool("ProductWithOutScannerCode"))
                {
                    data.Add(dt1);
                }
                else
                {
                    if (dt2.pieceId != string.Empty)
                    {
                        data.Add(dt1);
                    }
                }
               
                #endregion
                #region 中片三
                //清洗蓝进入时间辅助载具
                piece3.createTime = Data.m_dtCarrierBeginTime.ToString("yyyy-MM-dd HH:mm:ss");
                //载具类型1进入
                piece3.subVehicleType = "1";
                //进入载具码
                piece3.subVehicleCode = Data.ProductInfo4[2].TrayCode;
                piece3.vehicleCode = Data.SteelRing.SteelSN;
                piece3.location = "";
                piece3.qty = "";
                remark2.Add(piece3);

                //钢环时间主载具（回收）
                piece4.createTime = Data.m_dtCarrierEndTime.ToString("yyyy-MM-dd HH:mm:ss");
                //载具类型2（回收）
                piece4.subVehicleType = "2";
                //回收载具码（回收）
                piece4.subVehicleCode = Data.ProductInfo4[2].TrayCode;
                piece4.vehicleCode = Data.SteelRing.SteelSN;
                piece4.location = "";
                piece4.qty = "";
                remark2.Add(piece4);


                dt2.runCardId = Data.ProductInfo4[2].runCardNo;        //流程卡号    非必填
                dt2.pieceId = Data.ProductInfo4[2].SNCode;       //单片二维码    非必填
                dt2.w_location = "3";   //单片位置    非必填
                dt2.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();        //工站编号
                dt2.w_createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");        //生成日期
                dt2.w_vehicleNo = Data.SteelRing.SteelSN;        //载具二维码    非必填
                dt2.goodProductNumber = "1";        //良品数量
                dt2.defectCode = "OK";        //物料结果
                dt2.o_operator = Login;        //作业人员工号
                dt2.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();        //机台号
                dt2.coatingTrackingNumber = "";       //镀膜跟踪号    非必填
                dt2.p1RunCardId = "";//p1流程卡    非必填
                dt2.p3RunCardId = "";//p3流程卡    非必填
                dt2.level = "";        //等级    非必填
                dt2.w_judge = "";
                dt2.remark1 = "";
                dt2.remark2 = remark2;
                dt2.remark3 = "";
                dt2.parentPieceId = "";    //父单片码
                dt2.fileId = Data.FileID;
                if (SystemMgr.GetInstance().GetParamBool("ProductWithOutScannerCode"))
                {
                    data.Add(dt2);
                }
                else
                {
                    if (dt2.pieceId != string.Empty)
                    {
                        data.Add(dt2);
                    }
                }
                #endregion
            }


            MS.data = JsonConvert.SerializeObject(data);
            mqtt_logger.Trace(JsonConvert.SerializeObject(MS));
            MS.data = DESEncrypt(JsonConvert.SerializeObject(data), "sjgd2023gyhlwiot");
            message = JsonConvert.SerializeObject(MS);

            WaitReceiver WReceiver = new WaitReceiver();
            WReceiver.Msg_id = MS.msg_id;
            //流程卡号
            WReceiver.Lot_ID = DateTime.Now.ToString("yyyyMMddHH");
            WReceiver.ProductCode = Data.steelringcode;
            WReceiver.fileNamelog = WReceiver.ProductCode + ".txt";
            WReceiver.Send_msg = message;
            //WReceiver.Loation_flow_card = SystemMgr.GetInstance().GetRegString((int)SysStrReg.OrderSN) + "_" + SystemMgr.GetInstance().GetRegString((int)SysStrReg.OrderCreatTime);
            if (ConStatue)
            {
                msg_idlist.TryAdd(MS.msg_id, WReceiver);
                SaveMQTTBufferlog(WReceiver);
            }
            else
            {
                SaveMQTTStemplog(WReceiver);
            }


            Mqtt_Send(topic, message);
        }



        /// <summary>
        /// 3.15 非操作员模式数据上报
        /// </summary>
        /// <returns></returns>
        public static bool SelectMode(ProductData Data, out string message)
        {
            try
            {
                string workorder = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["非作业员报工主题"].m_dbStandardValue.ToString();
                Mqtt_Subscribe(workorder);
                string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["非作业员报工主题"].m_dbStandardValue.ToString();
                MachineDataNotOpMode MNM = new MachineDataNotOpMode();
                DateTime time = DateTime.Now;
                long Time = ConvertDateTimeToInt(time);

                MNM.msg_id = "aoi-" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString() + "-" + Time;
                MNM.deviceno = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString();
                MNM.dataType = "aoi";
                MNM.deviceType = "data.device.extFour.reply.fish290";
                string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AssemInfo.ini");
                string Login = IniOperation.GetStringValue(iniPath, "systemdata", "Login", "");
                List<MachineDataNotOpMode.DataItem1> data = new List<DataItem1>();
                MachineDataNotOpMode.DataItem1 data1 = new MachineDataNotOpMode.DataItem1();
                MachineDataNotOpMode.DataItem1 data2 = new MachineDataNotOpMode.DataItem1();


                if (SystemMgr.GetInstance().GetParamString("PutParam") == "1,4")
                {
                    data1.CREATE_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    data1.O_OPERATOR = Login;
                    data1.OP_MODE = "1";
                    data1.RUN_CARD_ID = Data.ProductInfo4[0].runCardNo;
                    data1.W_VEHICLE_NO = Data.SteelRing.SteelSN;
                    data1.PIECE_ID = Data.ProductInfo4[0].SNCode;
                    data1.LOCATION = "1";
                    data1.GOODPRODUCTNUMBER = "1";
                    data1.DEFECTCODE = "OK";
                    data1.MACHINE_NO = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                    data1.O_WORKSTATION = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();
                    data.Add(data1);

                    data2.CREATE_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    data2.O_OPERATOR = Login;
                    data2.OP_MODE = "1";
                    data2.RUN_CARD_ID = Data.ProductInfo4[3].runCardNo;
                    data2.W_VEHICLE_NO = Data.SteelRing.SteelSN;
                    data2.PIECE_ID = Data.ProductInfo4[3].SNCode;
                    data2.LOCATION = "1";
                    data2.GOODPRODUCTNUMBER = "1";
                    data2.DEFECTCODE = "OK";
                    data2.MACHINE_NO = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                    data2.O_WORKSTATION = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();
                    data.Add(data2);
                }
                else
                {
                    //2,3
                    data1.CREATE_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    data1.O_OPERATOR = Login;
                    data1.OP_MODE = "1";
                    data1.RUN_CARD_ID = Data.ProductInfo4[1].runCardNo;
                    data1.W_VEHICLE_NO = Data.SteelRing.SteelSN;
                    data1.PIECE_ID = Data.ProductInfo4[1].SNCode;
                    data1.LOCATION = "1";
                    data1.GOODPRODUCTNUMBER = "1";
                    data1.DEFECTCODE = "OK";
                    data1.MACHINE_NO = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                    data1.O_WORKSTATION = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();
                    data.Add(data1);

                    data2.CREATE_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    data2.O_OPERATOR = Login;
                    data2.OP_MODE = "1";
                    data2.RUN_CARD_ID = Data.ProductInfo4[2].runCardNo;
                    data2.W_VEHICLE_NO = Data.SteelRing.SteelSN;
                    data2.PIECE_ID = Data.ProductInfo4[2].SNCode;
                    data2.LOCATION = "1";
                    data2.GOODPRODUCTNUMBER = "1";
                    data2.DEFECTCODE = "OK";
                    data2.MACHINE_NO = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                    data2.O_WORKSTATION = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();
                    data.Add(data2);
                }


                MNM.data = JsonConvert.SerializeObject(data);
                mqtt_logger.Trace(JsonConvert.SerializeObject(MNM));
                MNM.data = DESEncrypt(JsonConvert.SerializeObject(data), "sjgd2023gyhlwiot");
                message = JsonConvert.SerializeObject(MNM);

                WaitReceiver WReceiver = new WaitReceiver();
                WReceiver.Msg_id = MNM.msg_id;
                //流程卡号
                WReceiver.Lot_ID = DateTime.Now.ToString("yyyyMMddHH");
                WReceiver.ProductCode = Data.steelringcode;
                WReceiver.fileNamelog = WReceiver.ProductCode + ".txt";
                WReceiver.Send_msg = message;
                //WReceiver.Loation_flow_card = SystemMgr.GetInstance().GetRegString((int)SysStrReg.OrderSN) + "_" + SystemMgr.GetInstance().GetRegString((int)SysStrReg.OrderCreatTime);
                if (ConStatue)
                {
                    msg_idlist.TryAdd(MNM.msg_id, WReceiver);
                    SaveMQTTBufferlog(WReceiver);
                }
                else
                {
                    SaveMQTTStemplog(WReceiver);
                }
                Mqtt_Send(topic, message);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                message = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// Rundata 3.3机况上报
        /// </summary>
        /// <param name="state">机台状态</param>
        /// <param name="message"></param>
        /// <param name="CreateFile"></param>
        /// <param name="previous_state"></param>
        /// <param name="StateChangeReason"></param>
        public static void ChangeState(string StateNumber, string StateName)
        {
            //string status = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["status"].m_dbStandardValue.ToString() + ".reply";

            //MQTT.Mqtt_Subscribe(status);
            string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["status"].m_dbStandardValue.ToString();
            MachineStateData MS = new MachineStateData();
            //string oldstate = GetErrorInfo.GetInstance().ReadMachineStateXML().Statecode;
            //string MSG = GetErrorInfo.GetInstance().ReadMachineStateXML().StatestrMsg;
            //string o_alertCode = GetErrorInfo.GetInstance().ReadMachineStateXML().code;
            //string o_alertInfo = GetErrorInfo.GetInstance().ReadMachineStateXML().strMsg;
            DateTime time = DateTime.Now;
            long Time = ConvertDateTimeToInt(time);

            MS.msg_id = "rd-" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString() + "-" + Time;
            MS.deviceno = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString();
            MS.dataType = "RunData";
            MS.deviceType = "";
            List<MachineStateData.DataItem> data = new List<MachineStateData.DataItem>();
            MachineStateData.DataItem dt = new MachineStateData.DataItem();

            dt.o_workStation = "290";        //工站编号
            dt.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();       //机台号
            //dt.o_machineState = oldstate;       //状态码
            //dt.o_stateName = MSG;        //状态名称
            //dt.o_stateStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");        //状态运行开始时间
            //dt.o_alertCode = o_alertCode;        //错误代码     非必填
            //dt.o_alertInfo = o_alertInfo;       //填写错误信息   非必填   
            data.Add(dt);
            SaveStateCsv(data);


            MS.data = DESEncrypt(JsonConvert.SerializeObject(data), "sjgd2023gyhlwiot");



            string message = JsonConvert.SerializeObject(MS);
            Mqtt_Send(topic, message);
            //HiveMgr.GetInstance().UploadInformation(MS, out message, CreateFile);
        }
        /// <summary>
        /// Rundata 机况上报
        /// </summary>
        public static void ChangeState()
        {
            string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["status"].m_dbStandardValue.ToString();
            MachineStateData MS = new MachineStateData();
            //string oldstate = GetErrorInfo.GetInstance().ReadMachineStateXML().Statecode;
            //string MSG = GetErrorInfo.GetInstance().ReadMachineStateXML().StatestrMsg;
            //string o_alertCode = GetErrorInfo.GetInstance().ReadMachineStateXML().code;
            //string o_alertInfo = GetErrorInfo.GetInstance().ReadMachineStateXML().strMsg;
            DateTime time = DateTime.Now;
            long Time = ConvertDateTimeToInt(time);

            MS.msg_id = "rd-" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString() + "-" + Time;
            MS.deviceno = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString();
            MS.dataType = "RunData";
            MS.deviceType = "";
            List<MachineStateData.DataItem> data = new List<MachineStateData.DataItem>();
            MachineStateData.DataItem dt = new MachineStateData.DataItem();

            dt.o_workStation = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString();        //工站编号
            dt.o_machineNo = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();       //机台号
            //dt.o_machineState = oldstate;       //状态码
            //dt.o_stateName = MSG;        //状态名称
            //dt.o_stateStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");        //状态运行开始时间
            //dt.o_alertCode = o_alertCode;        //错误代码     非必填
            //dt.o_alertInfo = o_alertInfo;       //填写错误信息   非必填   
            data.Add(dt);
            SaveStateCsv(data);


            MS.data = DESEncrypt(JsonConvert.SerializeObject(data), "sjgd2023gyhlwiot");

            string message = JsonConvert.SerializeObject(MS);

            Mqtt_Send(topic, message);
            MS.data = JsonConvert.SerializeObject(data);
            mqtt_logger.Info(JsonConvert.SerializeObject(MS));
        }
        /// <summary>
        /// AOI正常报工明细(25年新报工方式)
        /// </summary>      
        public static void ChangAOI(ProductInfo data1, out string message)
        {
            //if (data1.Code.Length>17|| data1.Code.Length < 15)
            //{
            message = "";
            //    return;
            //}
            ////lock (lockSend)
            ////{
            //    string parameters = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString() + ".reply";

            //    MQTT.Mqtt_Subscribe(parameters);

            //    string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString();
            //    MachineDataTwe MS = new MachineDataTwe();
            //    MachineDataTwe.DataItem MS1 = new MachineDataTwe.DataItem();
            //    DateTime time = DateTime.Now;
            //    long Time = ConvertDateTimeToInt(time);

            //    //List<string[]> DataList = ExcelNPOI.test1();

            //    MS.msg_id = "ps-"+ DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString() + "-" + Time;
            //    MS.deviceno = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString();
            //    MS.dataType = "aoi";
            //    //MS.origntime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //    MS.deviceType= "data.device.aoi.reply";
            //    List<MachineDataTwe.DataItem> data = new List<MachineDataTwe.DataItem>();

            //    MachineDataTwe.DataItem dt = new MachineDataTwe.DataItem();
            //    List<MachineDataTwe.DataItem.DataItem1> ItemList = new List<MachineDataTwe.DataItem.DataItem1>();
            //    dt.LOCATION = "";
            //    dt.O_WORKSTATION = "1010";        //工站编号
            //    dt.COLLECT_PALLET_SERIAL = data1.COLLECT_PALLET_SERIAL;
            //    dt.MACHINE_NO =  DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
            //    dt.O_OPERATOR = "123";
            //    dt.TEST_END_TIME = data1.m_dtOutPutTime.ToString("yyyy-MM-dd HH:mm:ss");
            //    dt.FLOW_CARD_ID = data1.runCardId;   
            //    dt.FILE_ID = data1.fileId;
            //    dt.VEHICLE_CODE = "";        
            //    dt.POSIONT_COLUMN = "";
            //    dt.POSIONT_ROW = "";
            //    dt.DOWN_MOUTH_NO = data1.MOUTH_NO;
            //    dt.TEST_START_TIME =data1.m_dtInputTime.ToString("yyyy-MM-dd HH:mm:ss");
            //    dt.FLOW_QR_CODE =data1.Code;
            //    //dt.FLOW_QR_CODE = "7cd32164ac5c1b35";
            //    dt.FEED_SERIAL = data1.FEED_SERIAL;
            //    dt.COLLECT_BOX_CODE =data1.COLLECT_BOX_CODE;
            //    dt.FEED_PALLET_QR_CODE = data1.COLLECT_PALLET_SERIAL;
            //    dt.COLLECT_SERIAL =data1. COLLECT_SERIAL;
            //    dt.QR_GRADE ="";
            //    dt.PORRO_NO = "";
            //    dt.SCAN_NO = "";
            //    dt.TEST_ITEM_FAULTY = data1.Reason;
            //    dt.TEST_ITEM_RESULT = data1.m_bResult;
            //    dt.MOUTH_NO = "";
            //    dt.FEED_PALLET_SERIAL = data1.FEED_PALLET_SERIAL;
            //    //dt.boxNo = "";
            //    #region MyRegion
            //    MachineDataTwe.DataItem.DataItem1 dtList = new MachineDataTwe.DataItem.DataItem1();
            //    dtList.MODEL_SIDE = "A透明";
            //    dtList.TEST_ITEM_NAME = "1001";
            //    dtList.TEST_ITEM_VALUE = data1.Testing2;
            //    dtList.TEST_ITEM_RESULT = data1.Testing1;
            //    ItemList.Add(dtList);
            //    MachineDataTwe.DataItem.DataItem1 dtList1 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList1.MODEL_SIDE = "A透明";
            //    dtList1.TEST_ITEM_NAME = "1004";
            //    dtList1.TEST_ITEM_VALUE = data1.Testing4;
            //    dtList1.TEST_ITEM_RESULT = data1.Testing3;
            //    ItemList.Add(dtList1);
            //    MachineDataTwe.DataItem.DataItem1 dtList2 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList2.MODEL_SIDE = "A透明";
            //    dtList2.TEST_ITEM_NAME = "1005";
            //    dtList2.TEST_ITEM_VALUE = data1.Testing6;
            //    dtList2.TEST_ITEM_RESULT = data1.Testing5;
            //    ItemList.Add(dtList2);
            //    MachineDataTwe.DataItem.DataItem1 dtList3 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList3.MODEL_SIDE = "A透明";
            //    dtList3.TEST_ITEM_NAME = "3016";
            //    dtList3.TEST_ITEM_VALUE = data1.Testing8;
            //    dtList3.TEST_ITEM_RESULT = data1.Testing7;
            //    ItemList.Add(dtList3);
            //    MachineDataTwe.DataItem.DataItem1 dtList4 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList4.MODEL_SIDE = "A透明";
            //    dtList4.TEST_ITEM_NAME = "1000";
            //    dtList4.TEST_ITEM_VALUE = data1.Testing10;
            //    dtList4.TEST_ITEM_RESULT = data1.Testing9;
            //    ItemList.Add(dtList4);
            //    MachineDataTwe.DataItem.DataItem1 dtList89 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList89.MODEL_SIDE = "A透明";
            //    dtList89.TEST_ITEM_NAME = "1002";
            //    dtList89.TEST_ITEM_VALUE = data1.Testing235;
            //    dtList89.TEST_ITEM_RESULT = data1.Testing234;
            //    ItemList.Add(dtList89);
            //    MachineDataTwe.DataItem.DataItem1 dtList5 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList5.MODEL_SIDE = "C透明";
            //    dtList5.TEST_ITEM_NAME = "1001";
            //    dtList5.TEST_ITEM_VALUE = data1.Testing12;
            //    dtList5.TEST_ITEM_RESULT = data1.Testing11;
            //    ItemList.Add(dtList5);
            //    MachineDataTwe.DataItem.DataItem1 dtList6 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList6.MODEL_SIDE = "C透明";
            //    dtList6.TEST_ITEM_NAME = "1004";
            //    dtList6.TEST_ITEM_VALUE = data1.Testing14;
            //    dtList6.TEST_ITEM_RESULT = data1.Testing13;
            //    ItemList.Add(dtList6);
            //    MachineDataTwe.DataItem.DataItem1 dtList7 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList7.MODEL_SIDE = "C透明";
            //    dtList7.TEST_ITEM_NAME = "1005";
            //    dtList7.TEST_ITEM_VALUE = data1.Testing16;
            //    dtList7.TEST_ITEM_RESULT = data1.Testing15;
            //    ItemList.Add(dtList7);
            //    MachineDataTwe.DataItem.DataItem1 dtList8 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList8.MODEL_SIDE = "C透明";
            //    dtList8.TEST_ITEM_NAME = "3016";
            //    dtList8.TEST_ITEM_VALUE = data1.Testing18;
            //    dtList8.TEST_ITEM_RESULT = data1.Testing17;
            //    ItemList.Add(dtList8);
            //    MachineDataTwe.DataItem.DataItem1 dtList9 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList9.MODEL_SIDE = "C透明";
            //    dtList9.TEST_ITEM_NAME = "1000";
            //    dtList9.TEST_ITEM_VALUE = data1.Testing20;
            //    dtList9.TEST_ITEM_RESULT = data1.Testing19;
            //    ItemList.Add(dtList9);
            //    MachineDataTwe.DataItem.DataItem1 dtList90 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList90.MODEL_SIDE = "C透明";
            //    dtList90.TEST_ITEM_NAME = "1002";
            //    dtList90.TEST_ITEM_VALUE = data1.Testing237;
            //    dtList90.TEST_ITEM_RESULT = data1.Testing236;
            //    ItemList.Add(dtList90);
            //    MachineDataTwe.DataItem.DataItem1 dtList10 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList10.MODEL_SIDE = "A油墨区";
            //    dtList10.TEST_ITEM_NAME = "3077";
            //    dtList10.TEST_ITEM_VALUE = data1.Testing22;
            //    dtList10.TEST_ITEM_RESULT = data1.Testing21;
            //    ItemList.Add(dtList10);
            //    MachineDataTwe.DataItem.DataItem1 dtList11 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList11.MODEL_SIDE = "A油墨区";
            //    dtList11.TEST_ITEM_NAME = "3076";
            //    dtList11.TEST_ITEM_VALUE = data1.Testing24;
            //    dtList11.TEST_ITEM_RESULT = data1.Testing23;
            //    ItemList.Add(dtList11);
            //    MachineDataTwe.DataItem.DataItem1 dtList12 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList12.MODEL_SIDE = "A油墨区";
            //    dtList12.TEST_ITEM_NAME = "1011";
            //    dtList12.TEST_ITEM_VALUE = data1.Testing26;
            //    dtList12.TEST_ITEM_RESULT = data1.Testing25;
            //    ItemList.Add(dtList12);
            //    MachineDataTwe.DataItem.DataItem1 dtList13 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList13.MODEL_SIDE = "A油墨区";
            //    dtList13.TEST_ITEM_NAME = "1511";
            //    dtList13.TEST_ITEM_VALUE = data1.Testing28;
            //    dtList13.TEST_ITEM_RESULT = data1.Testing27;
            //    ItemList.Add(dtList13);
            //    MachineDataTwe.DataItem.DataItem1 dtList14 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList14.MODEL_SIDE = "A油墨区";
            //    dtList14.TEST_ITEM_NAME = "1510";
            //    dtList14.TEST_ITEM_VALUE = data1.Testing30;
            //    dtList14.TEST_ITEM_RESULT = data1.Testing29;
            //    ItemList.Add(dtList14);
            //    MachineDataTwe.DataItem.DataItem1 dtList15 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList15.MODEL_SIDE = "A油墨区";
            //    dtList15.TEST_ITEM_NAME = "1507";
            //    dtList15.TEST_ITEM_VALUE = data1.Testing32;
            //    dtList15.TEST_ITEM_RESULT = data1.Testing31;
            //    ItemList.Add(dtList15);
            //    MachineDataTwe.DataItem.DataItem1 dtList16 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList16.MODEL_SIDE = "A油墨区";
            //    dtList16.TEST_ITEM_NAME = "1508";
            //    dtList16.TEST_ITEM_VALUE = data1.Testing34;
            //    dtList16.TEST_ITEM_RESULT = data1.Testing33;
            //    ItemList.Add(dtList16);
            //    MachineDataTwe.DataItem.DataItem1 dtList17 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList17.MODEL_SIDE = "A油墨区";
            //    dtList17.TEST_ITEM_NAME = "3063";
            //    dtList17.TEST_ITEM_VALUE = data1.Testing36;
            //    dtList17.TEST_ITEM_RESULT = data1.Testing35;
            //    ItemList.Add(dtList17);
            //    MachineDataTwe.DataItem.DataItem1 dtList18 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList18.MODEL_SIDE = "A油墨区";
            //    dtList18.TEST_ITEM_NAME = "3012";
            //    dtList18.TEST_ITEM_VALUE = data1.Testing38;
            //    dtList18.TEST_ITEM_RESULT = data1.Testing37;
            //    ItemList.Add(dtList18);
            //    MachineDataTwe.DataItem.DataItem1 dtList19 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList19.MODEL_SIDE = "C油墨区";
            //    dtList19.TEST_ITEM_NAME = "3077";
            //    dtList19.TEST_ITEM_VALUE = data1.Testing40;
            //    dtList19.TEST_ITEM_RESULT = data1.Testing39;
            //    ItemList.Add(dtList19);
            //    MachineDataTwe.DataItem.DataItem1 dtList20 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList20.MODEL_SIDE = "C油墨区";
            //    dtList20.TEST_ITEM_NAME = "3076";
            //    dtList20.TEST_ITEM_VALUE = data1.Testing42;
            //    dtList20.TEST_ITEM_RESULT = data1.Testing41;
            //    ItemList.Add(dtList20);
            //    MachineDataTwe.DataItem.DataItem1 dtList21 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList21.MODEL_SIDE = "C油墨区";
            //    dtList21.TEST_ITEM_NAME = "1011";
            //    dtList21.TEST_ITEM_VALUE = data1.Testing44;
            //    dtList21.TEST_ITEM_RESULT = data1.Testing43;
            //    ItemList.Add(dtList21);
            //    MachineDataTwe.DataItem.DataItem1 dtList22 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList22.MODEL_SIDE = "C油墨区";
            //    dtList22.TEST_ITEM_NAME = "1511";
            //    dtList22.TEST_ITEM_VALUE = data1.Testing46;
            //    dtList22.TEST_ITEM_RESULT = data1.Testing45;
            //    ItemList.Add(dtList22);
            //    MachineDataTwe.DataItem.DataItem1 dtList23 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList23.MODEL_SIDE = "C油墨区";
            //    dtList23.TEST_ITEM_NAME = "1510";
            //    dtList23.TEST_ITEM_VALUE = data1.Testing48;
            //    dtList23.TEST_ITEM_RESULT = data1.Testing47;
            //    ItemList.Add(dtList23);
            //    MachineDataTwe.DataItem.DataItem1 dtList24 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList24.MODEL_SIDE = "C油墨区";
            //    dtList24.TEST_ITEM_NAME = "1507";
            //    dtList24.TEST_ITEM_VALUE = data1.Testing50;
            //    dtList24.TEST_ITEM_RESULT = data1.Testing49;
            //    ItemList.Add(dtList24);
            //    MachineDataTwe.DataItem.DataItem1 dtList25 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList25.MODEL_SIDE = "C油墨区";
            //    dtList25.TEST_ITEM_NAME = "1508";
            //    dtList25.TEST_ITEM_VALUE = data1.Testing52;
            //    dtList25.TEST_ITEM_RESULT = data1.Testing51;
            //    ItemList.Add(dtList25);
            //    MachineDataTwe.DataItem.DataItem1 dtList26 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList26.MODEL_SIDE = "C油墨区";
            //    dtList26.TEST_ITEM_NAME = "3063";
            //    dtList26.TEST_ITEM_VALUE = data1.Testing54;
            //    dtList26.TEST_ITEM_RESULT = data1.Testing53;
            //    ItemList.Add(dtList26);
            //    MachineDataTwe.DataItem.DataItem1 dtList27 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList27.MODEL_SIDE = "C油墨区";
            //    dtList27.TEST_ITEM_NAME = "3012";
            //    dtList27.TEST_ITEM_VALUE = data1.Testing56;
            //    dtList27.TEST_ITEM_RESULT = data1.Testing55;
            //    ItemList.Add(dtList27);

            //    MachineDataTwe.DataItem.DataItem1 dtList28 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList28.MODEL_SIDE = "B油墨区";
            //    dtList28.TEST_ITEM_NAME = "3076";
            //    dtList28.TEST_ITEM_VALUE = data1.Testing58;
            //    dtList28.TEST_ITEM_RESULT = data1.Testing57;
            //    ItemList.Add(dtList28);
            //    MachineDataTwe.DataItem.DataItem1 dtList29 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList29.MODEL_SIDE = "B油墨区";
            //    dtList29.TEST_ITEM_NAME = "1011";
            //    dtList29.TEST_ITEM_VALUE = data1.Testing60;
            //    dtList29.TEST_ITEM_RESULT = data1.Testing59;
            //    ItemList.Add(dtList29);
            //    MachineDataTwe.DataItem.DataItem1 dtList30 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList30.MODEL_SIDE = "B油墨区";
            //    dtList30.TEST_ITEM_NAME = "1511";
            //    dtList30.TEST_ITEM_VALUE = data1.Testing62;
            //    dtList30.TEST_ITEM_RESULT = data1.Testing61;
            //    ItemList.Add(dtList30);
            //    MachineDataTwe.DataItem.DataItem1 dtList31 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList31.MODEL_SIDE = "B油墨区";
            //    dtList31.TEST_ITEM_NAME = "1510";
            //    dtList31.TEST_ITEM_VALUE = data1.Testing64;
            //    dtList31.TEST_ITEM_RESULT = data1.Testing63;
            //    ItemList.Add(dtList31);
            //    MachineDataTwe.DataItem.DataItem1 dtList32 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList32.MODEL_SIDE = "B油墨区";
            //    dtList32.TEST_ITEM_NAME = "1507";
            //    dtList32.TEST_ITEM_VALUE = data1.Testing66;
            //    dtList32.TEST_ITEM_RESULT = data1.Testing65;
            //    ItemList.Add(dtList32);
            //    MachineDataTwe.DataItem.DataItem1 dtList33 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList33.MODEL_SIDE = "B油墨区";
            //    dtList33.TEST_ITEM_NAME = "1508";
            //    dtList33.TEST_ITEM_VALUE = data1.Testing68;
            //    dtList33.TEST_ITEM_RESULT = data1.Testing67;
            //    ItemList.Add(dtList33);
            //    MachineDataTwe.DataItem.DataItem1 dtList34 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList34.MODEL_SIDE = "B油墨区";
            //    dtList34.TEST_ITEM_NAME = "3063";
            //    dtList34.TEST_ITEM_VALUE = data1.Testing70;
            //    dtList34.TEST_ITEM_RESULT = data1.Testing69;
            //    ItemList.Add(dtList34);

            //    MachineDataTwe.DataItem.DataItem1 dtList35 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList35.MODEL_SIDE = "D油墨区";
            //    dtList35.TEST_ITEM_NAME = "3076";
            //    dtList35.TEST_ITEM_VALUE = data1.Testing72;
            //    dtList35.TEST_ITEM_RESULT = data1.Testing71;
            //    ItemList.Add(dtList35);
            //    MachineDataTwe.DataItem.DataItem1 dtList36 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList36.MODEL_SIDE = "D油墨区";
            //    dtList36.TEST_ITEM_NAME = "1011";
            //    dtList36.TEST_ITEM_VALUE = data1.Testing74;
            //    dtList36.TEST_ITEM_RESULT = data1.Testing73;
            //    ItemList.Add(dtList36);
            //    MachineDataTwe.DataItem.DataItem1 dtList37 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList37.MODEL_SIDE = "D油墨区";
            //    dtList37.TEST_ITEM_NAME = "1511";
            //    dtList37.TEST_ITEM_VALUE = data1.Testing76;
            //    dtList37.TEST_ITEM_RESULT = data1.Testing75;
            //    ItemList.Add(dtList37);
            //    MachineDataTwe.DataItem.DataItem1 dtList38 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList38.MODEL_SIDE = "D油墨区";
            //    dtList38.TEST_ITEM_NAME = "1510";
            //    dtList38.TEST_ITEM_VALUE = data1.Testing78;
            //    dtList38.TEST_ITEM_RESULT = data1.Testing77;
            //    ItemList.Add(dtList38);

            //    MachineDataTwe.DataItem.DataItem1 dtList39 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList39.MODEL_SIDE = "D油墨区";
            //    dtList39.TEST_ITEM_NAME = "1507";
            //    dtList39.TEST_ITEM_VALUE = data1.Testing80;
            //    dtList39.TEST_ITEM_RESULT = data1.Testing79;
            //    ItemList.Add(dtList39);

            //    MachineDataTwe.DataItem.DataItem1 dtList40 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList40.MODEL_SIDE = "D油墨区";
            //    dtList40.TEST_ITEM_NAME = "1508";
            //    dtList40.TEST_ITEM_VALUE = data1.Testing82;
            //    dtList40.TEST_ITEM_RESULT = data1.Testing81;
            //    ItemList.Add(dtList40);
            //    MachineDataTwe.DataItem.DataItem1 dtList41 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList41.MODEL_SIDE = "D油墨区";
            //    dtList41.TEST_ITEM_NAME = "3063";
            //    dtList41.TEST_ITEM_VALUE = data1.Testing84;
            //    dtList41.TEST_ITEM_RESULT = data1.Testing83;
            //    ItemList.Add(dtList41);

            //    MachineDataTwe.DataItem.DataItem1 dtList42 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList42.MODEL_SIDE = "E油墨区";
            //    dtList42.TEST_ITEM_NAME = "1011";
            //    dtList42.TEST_ITEM_VALUE = data1.Testing86;
            //    dtList42.TEST_ITEM_RESULT = data1.Testing85;
            //    ItemList.Add(dtList42);
            //    MachineDataTwe.DataItem.DataItem1 dtList43 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList43.MODEL_SIDE = "E油墨区";
            //    dtList43.TEST_ITEM_NAME = "1511";
            //    dtList43.TEST_ITEM_VALUE = data1.Testing88;
            //    dtList43.TEST_ITEM_RESULT = data1.Testing87;
            //    ItemList.Add(dtList43);
            //    MachineDataTwe.DataItem.DataItem1 dtList44 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList44.MODEL_SIDE = "E油墨区";
            //    dtList44.TEST_ITEM_NAME = "1510";
            //    dtList44.TEST_ITEM_VALUE = data1.Testing90;
            //    dtList44.TEST_ITEM_RESULT = data1.Testing89;
            //    ItemList.Add(dtList44);
            //    MachineDataTwe.DataItem.DataItem1 dtList45 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList45.MODEL_SIDE = "E油墨区";
            //    dtList45.TEST_ITEM_NAME = "1507";
            //    dtList45.TEST_ITEM_VALUE = data1.Testing92;
            //    dtList45.TEST_ITEM_RESULT = data1.Testing91;
            //    ItemList.Add(dtList45);
            //    MachineDataTwe.DataItem.DataItem1 dtList46 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList46.MODEL_SIDE = "E油墨区";
            //    dtList46.TEST_ITEM_NAME = "1508";
            //    dtList46.TEST_ITEM_VALUE = data1.Testing94;
            //    dtList46.TEST_ITEM_RESULT = data1.Testing93;
            //    ItemList.Add(dtList46);

            //    MachineDataTwe.DataItem.DataItem1 dtList47 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList47.MODEL_SIDE = "F油墨区";
            //    dtList47.TEST_ITEM_NAME = "1011";
            //    dtList47.TEST_ITEM_VALUE = data1.Testing96;
            //    dtList47.TEST_ITEM_RESULT = data1.Testing95;
            //    ItemList.Add(dtList47);
            //    MachineDataTwe.DataItem.DataItem1 dtList48 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList48.MODEL_SIDE = "F油墨区";
            //    dtList48.TEST_ITEM_NAME = "1511";
            //    dtList48.TEST_ITEM_VALUE = data1.Testing98;
            //    dtList48.TEST_ITEM_RESULT = data1.Testing97;
            //    ItemList.Add(dtList48);
            //    MachineDataTwe.DataItem.DataItem1 dtList49 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList49.MODEL_SIDE = "F油墨区";
            //    dtList49.TEST_ITEM_NAME = "1510";
            //    dtList49.TEST_ITEM_VALUE = data1.Testing100;
            //    dtList49.TEST_ITEM_RESULT = data1.Testing99;
            //    ItemList.Add(dtList49);
            //    MachineDataTwe.DataItem.DataItem1 dtList50 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList50.MODEL_SIDE = "F油墨区";
            //    dtList50.TEST_ITEM_NAME = "1507";
            //    dtList50.TEST_ITEM_VALUE = data1.Testing102;
            //    dtList50.TEST_ITEM_RESULT = data1.Testing101;
            //    ItemList.Add(dtList50);
            //    MachineDataTwe.DataItem.DataItem1 dtList51 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList51.MODEL_SIDE = "F油墨区";
            //    dtList51.TEST_ITEM_NAME = "1508";
            //    dtList51.TEST_ITEM_VALUE = data1.Testing104;
            //    dtList51.TEST_ITEM_RESULT = data1.Testing103;
            //    ItemList.Add(dtList51);

            //    MachineDataTwe.DataItem.DataItem1 dtList52 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList52.MODEL_SIDE = "B镜面";
            //    dtList52.TEST_ITEM_NAME = "1011";
            //    dtList52.TEST_ITEM_VALUE = data1.Testing106;
            //    dtList52.TEST_ITEM_RESULT = data1.Testing105;
            //    ItemList.Add(dtList52);
            //    MachineDataTwe.DataItem.DataItem1 dtList53 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList53.MODEL_SIDE = "B镜面";
            //    dtList53.TEST_ITEM_NAME = "1004";
            //    dtList53.TEST_ITEM_VALUE = data1.Testing108;
            //    dtList53.TEST_ITEM_RESULT = data1.Testing107;
            //    ItemList.Add(dtList53);
            //    MachineDataTwe.DataItem.DataItem1 dtList54 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList54.MODEL_SIDE = "B镜面";
            //    dtList54.TEST_ITEM_NAME = "1005";
            //    dtList54.TEST_ITEM_VALUE = data1.Testing110;
            //    dtList54.TEST_ITEM_RESULT = data1.Testing109;
            //    ItemList.Add(dtList54);
            //    MachineDataTwe.DataItem.DataItem1 dtList55 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList55.MODEL_SIDE = "B镜面";
            //    dtList55.TEST_ITEM_NAME = "3091";
            //    dtList55.TEST_ITEM_VALUE = data1.Testing112;
            //    dtList55.TEST_ITEM_RESULT = data1.Testing111;
            //    ItemList.Add(dtList55);
            //    MachineDataTwe.DataItem.DataItem1 dtList56 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList56.MODEL_SIDE = "B镜面";
            //    dtList56.TEST_ITEM_NAME = "1000";
            //    dtList56.TEST_ITEM_VALUE = data1.Testing114;
            //    dtList56.TEST_ITEM_RESULT = data1.Testing113;
            //    ItemList.Add(dtList56);

            //    MachineDataTwe.DataItem.DataItem1 dtList57 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList57.MODEL_SIDE = "D镜面";
            //    dtList57.TEST_ITEM_NAME = "1011";
            //    dtList57.TEST_ITEM_VALUE = data1.Testing116;
            //    dtList57.TEST_ITEM_RESULT = data1.Testing115;
            //    ItemList.Add(dtList57);
            //    MachineDataTwe.DataItem.DataItem1 dtList58 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList58.MODEL_SIDE = "D镜面";
            //    dtList58.TEST_ITEM_NAME = "1004";
            //    dtList58.TEST_ITEM_VALUE = data1.Testing118;
            //    dtList58.TEST_ITEM_RESULT = data1.Testing117;
            //    ItemList.Add(dtList58);
            //    MachineDataTwe.DataItem.DataItem1 dtList59 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList59.MODEL_SIDE = "D镜面";
            //    dtList59.TEST_ITEM_NAME = "1005";
            //    dtList59.TEST_ITEM_VALUE = data1.Testing120;
            //    dtList59.TEST_ITEM_RESULT = data1.Testing119;
            //    ItemList.Add(dtList59);
            //    MachineDataTwe.DataItem.DataItem1 dtList60 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList60.MODEL_SIDE = "D镜面";
            //    dtList60.TEST_ITEM_NAME = "3091";
            //    dtList60.TEST_ITEM_VALUE = data1.Testing122;
            //    dtList60.TEST_ITEM_RESULT = data1.Testing121;
            //    ItemList.Add(dtList60);
            //    MachineDataTwe.DataItem.DataItem1 dtList61 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList61.MODEL_SIDE = "D镜面";
            //    dtList61.TEST_ITEM_NAME = "1000";
            //    dtList61.TEST_ITEM_VALUE = data1.Testing124;
            //    dtList61.TEST_ITEM_RESULT = data1.Testing123;
            //    ItemList.Add(dtList61);

            //    MachineDataTwe.DataItem.DataItem1 dtList62 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList62.MODEL_SIDE = "BL1";
            //    dtList62.TEST_ITEM_NAME = "1022";
            //    dtList62.TEST_ITEM_VALUE = data1.Testing126;
            //    dtList62.TEST_ITEM_RESULT = data1.Testing125;
            //    ItemList.Add(dtList62);
            //    MachineDataTwe.DataItem.DataItem1 dtList63 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList63.MODEL_SIDE = "BL1";
            //    dtList63.TEST_ITEM_NAME = "1022";
            //    dtList63.TEST_ITEM_VALUE = data1.Testing128;
            //    dtList63.TEST_ITEM_RESULT = data1.Testing127;
            //    ItemList.Add(dtList63);
            //    MachineDataTwe.DataItem.DataItem1 dtList64 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList64.MODEL_SIDE = "BL2";
            //    dtList64.TEST_ITEM_NAME = "1022";
            //    dtList64.TEST_ITEM_VALUE = data1.Testing130;
            //    dtList64.TEST_ITEM_RESULT = data1.Testing129;
            //    ItemList.Add(dtList64);
            //    MachineDataTwe.DataItem.DataItem1 dtList65 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList65.MODEL_SIDE = "BL2";
            //    dtList65.TEST_ITEM_NAME = "1022";
            //    dtList65.TEST_ITEM_VALUE = data1.Testing132;
            //    dtList65.TEST_ITEM_RESULT = data1.Testing131;
            //    ItemList.Add(dtList65);

            //    MachineDataTwe.DataItem.DataItem1 dtList66 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList66.MODEL_SIDE = "其他不良";
            //    dtList66.TEST_ITEM_NAME = "1513";
            //    dtList66.TEST_ITEM_VALUE = data1.Testing134;
            //    dtList66.TEST_ITEM_RESULT = data1.Testing133;
            //    ItemList.Add(dtList66);
            //    MachineDataTwe.DataItem.DataItem1 dtList67 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList67.MODEL_SIDE = "其他不良";
            //    dtList67.TEST_ITEM_NAME = "1501";
            //    dtList67.TEST_ITEM_VALUE = data1.Testing136;
            //    dtList67.TEST_ITEM_RESULT = data1.Testing135;
            //    ItemList.Add(dtList67);
            //    MachineDataTwe.DataItem.DataItem1 dtList68 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList68.MODEL_SIDE = "其他不良";
            //    dtList68.TEST_ITEM_NAME = "4232";
            //    dtList68.TEST_ITEM_VALUE = data1.Testing138;
            //    dtList68.TEST_ITEM_RESULT = data1.Testing137;
            //    ItemList.Add(dtList68);
            //    MachineDataTwe.DataItem.DataItem1 dtList69 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList69.MODEL_SIDE = "其他不良";
            //    dtList69.TEST_ITEM_NAME = "4230";
            //    dtList69.TEST_ITEM_VALUE = data1.Testing140;
            //    dtList69.TEST_ITEM_RESULT = data1.Testing139;
            //    ItemList.Add(dtList69);
            //    MachineDataTwe.DataItem.DataItem1 dtList70 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList70.MODEL_SIDE = "其他不良";
            //    dtList70.TEST_ITEM_NAME = "2000";
            //    dtList70.TEST_ITEM_VALUE = data1.Testing142;
            //    dtList70.TEST_ITEM_RESULT = data1.Testing141;
            //    ItemList.Add(dtList70);
            //    MachineDataTwe.DataItem.DataItem1 dtList71 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList71.MODEL_SIDE = "其他不良";
            //    dtList71.TEST_ITEM_NAME = "6013";
            //    dtList71.TEST_ITEM_VALUE = data1.Testing144;
            //    dtList71.TEST_ITEM_RESULT = data1.Testing143;
            //    ItemList.Add(dtList71);
            //    MachineDataTwe.DataItem.DataItem1 dtList72 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList72.MODEL_SIDE = "其他不良";
            //    dtList72.TEST_ITEM_NAME = "1022";
            //    dtList72.TEST_ITEM_VALUE = data1.Testing146;
            //    dtList72.TEST_ITEM_RESULT = data1.Testing145;
            //    ItemList.Add(dtList72);
            //    MachineDataTwe.DataItem.DataItem1 dtList73 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList73.MODEL_SIDE = "其他不良";
            //    dtList73.TEST_ITEM_NAME = "6999";
            //    dtList73.TEST_ITEM_VALUE = data1.Testing148;
            //    dtList73.TEST_ITEM_RESULT = data1.Testing147;
            //    ItemList.Add(dtList73);

            //    MachineDataTwe.DataItem.DataItem1 dtList74= new MachineDataTwe.DataItem.DataItem1();
            //    dtList74.MODEL_SIDE = "尺寸缺陷";
            //    dtList74.TEST_ITEM_NAME = "FAI2";
            //    dtList74.TEST_ITEM_VALUE = data1.Testing149;
            //    dtList74.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList74);
            //    MachineDataTwe.DataItem.DataItem1 dtList75 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList75.MODEL_SIDE = "尺寸缺陷";
            //    dtList75.TEST_ITEM_NAME = "FAI4";
            //    dtList75.TEST_ITEM_VALUE = data1.Testing150;
            //    dtList75.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList75);
            //    MachineDataTwe.DataItem.DataItem1 dtList76 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList76.MODEL_SIDE = "尺寸缺陷";
            //    dtList76.TEST_ITEM_NAME = "FAI6";
            //    dtList76.TEST_ITEM_VALUE = data1.Testing151;
            //    dtList76.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList76);
            //    MachineDataTwe.DataItem.DataItem1 dtList77 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList77.MODEL_SIDE = "尺寸缺陷";
            //    dtList77.TEST_ITEM_NAME = "FAI10";
            //    dtList77.TEST_ITEM_VALUE = data1.Testing152;
            //    dtList77.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList77);
            //    MachineDataTwe.DataItem.DataItem1 dtList78 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList78.MODEL_SIDE = "尺寸缺陷";
            //    dtList78.TEST_ITEM_NAME = "FAI12";
            //    dtList78.TEST_ITEM_VALUE = data1.Testing153;
            //    dtList78.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList78);
            //    MachineDataTwe.DataItem.DataItem1 dtList79 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList79.MODEL_SIDE = "尺寸缺陷";
            //    dtList79.TEST_ITEM_NAME = "FAI13";
            //    dtList79.TEST_ITEM_VALUE = data1.Testing154;
            //    dtList79.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList79);
            //    MachineDataTwe.DataItem.DataItem1 dtList80 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList80.MODEL_SIDE = "尺寸缺陷";
            //    dtList80.TEST_ITEM_NAME = "FAI14";
            //    dtList80.TEST_ITEM_VALUE = data1.Testing155;
            //    dtList80.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList80);
            //    MachineDataTwe.DataItem.DataItem1 dtList81 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList81.MODEL_SIDE = "尺寸缺陷";
            //    dtList81.TEST_ITEM_NAME = "FAI18";
            //    dtList81.TEST_ITEM_VALUE = data1.Testing156;
            //    dtList81.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList81);
            //    MachineDataTwe.DataItem.DataItem1 dtList82 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList82.MODEL_SIDE = "尺寸缺陷";
            //    dtList82.TEST_ITEM_NAME = "FAI32";
            //    dtList82.TEST_ITEM_VALUE = data1.Testing157;
            //    dtList82.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList82);
            //    MachineDataTwe.DataItem.DataItem1 dtList83 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList83.MODEL_SIDE = "尺寸缺陷";
            //    dtList83.TEST_ITEM_NAME = "FAI36-1";
            //    dtList83.TEST_ITEM_VALUE = data1.Testing158;
            //    dtList83.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList83);
            //    MachineDataTwe.DataItem.DataItem1 dtList84 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList84.MODEL_SIDE = "尺寸缺陷";
            //    dtList84.TEST_ITEM_NAME = "FAI36-2";
            //    dtList84.TEST_ITEM_VALUE = data1.Testing159;
            //    dtList84.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList84);
            //    MachineDataTwe.DataItem.DataItem1 dtList85 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList85.MODEL_SIDE = "尺寸缺陷";
            //    dtList85.TEST_ITEM_NAME = "FAI26";
            //    dtList85.TEST_ITEM_VALUE = data1.Testing160;
            //    dtList85.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList85);
            //    MachineDataTwe.DataItem.DataItem1 dtList86 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList86.MODEL_SIDE = "尺寸缺陷";
            //    dtList86.TEST_ITEM_NAME = "FAI27";
            //    dtList86.TEST_ITEM_VALUE = data1.Testing161;
            //    dtList86.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList86);
            //    MachineDataTwe.DataItem.DataItem1 dtList87 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList87.MODEL_SIDE = "尺寸缺陷";
            //    dtList87.TEST_ITEM_NAME = "FAI28";
            //    dtList87.TEST_ITEM_VALUE = data1.Testing162;
            //    dtList87.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList87);
            //    MachineDataTwe.DataItem.DataItem1 dtList88 = new MachineDataTwe.DataItem.DataItem1();
            //    dtList88.MODEL_SIDE = "尺寸缺陷";
            //    dtList88.TEST_ITEM_NAME = "FAI33";
            //    dtList88.TEST_ITEM_VALUE = data1.Testing163;
            //    dtList88.TEST_ITEM_RESULT = "";
            //    ItemList.Add(dtList88);
            //    dt.ItemList = ItemList;
            //#endregion

            ////dt.reportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //data.Add(dt);

            //MS.data = DESEncrypt(JsonConvert.SerializeObject(data), "sjgd2023gyhlwiot");

            //message = JsonConvert.SerializeObject(MS);
            //MS.data = JsonConvert.SerializeObject(data);
            //string Savemessage = JsonConvert.SerializeObject(MS);
            //WaitReceiver WReceiver = new WaitReceiver();
            //WReceiver.Msg_id = MS.msg_id;
            // //流程卡号
            //WReceiver.Lot_ID = DateTime.Now.ToString("yyyyMMddHH");
            //WReceiver.ProductCode = data1.Code;
            //WReceiver.fileNamelog = WReceiver.ProductCode +"-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            //WReceiver.Send_msg = Savemessage;
            //WReceiver.Loation_flow_card = SystemMgr.GetInstance().GetRegString((int)SysStrReg.OrderSN) + "_" + SystemMgr.GetInstance().GetRegString((int)SysStrReg.OrderCreatTime);
            //if (ConStatue)
            //{
            //    msg_idlist.TryAdd(MS.msg_id, WReceiver);
            //    SaveMQTTBufferlog(WReceiver);
            //}
            //else
            //{
            //    SaveMQTTStemplog(WReceiver);
            //}

            //if (string.IsNullOrEmpty(topic))
            //{
            //    MessageBox.Show("发布主题不能为空！");
            //    return;
            //}
            //MQTT.GetMqtt();
            //Mqtt_Send(topic, message);
            //}

        }

        /// <summary>
        /// AOI OK料盘结工单报工
        /// </summary>    
        /// <param name="message"></param>
        public static void ReportAOIOKEnd(string Barcode, string FileID, string StartTime, out string message)
        {

            string parameters = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString() + ".reply";
            //MQTT.Mqtt_Subscribe(parameters);

            string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString();
            MachineDataTwe MS = new MachineDataTwe();
            MachineDataTwe.DataItem MS1 = new MachineDataTwe.DataItem();
            DateTime time = DateTime.Now;
            long Time = ConvertDateTimeToInt(time);

            //List<string[]> DataList = ExcelNPOI.test1();

            MS.msg_id = "ps-" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString() + "-" + Time;    ///车间内使用 101016000171          车间外使用   101016000201
            MS.deviceno = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString();
            MS.dataType = "aoi";
            //MS.origntime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            MS.deviceType = "data.device.aoi.reply";
            List<MachineDataTwe.DataItem> data = new List<MachineDataTwe.DataItem>();

            MachineDataTwe.DataItem dt = new MachineDataTwe.DataItem();
            List<MachineDataTwe.DataItem.DataItem1> ItemList = new List<MachineDataTwe.DataItem.DataItem1>();

            //工站编号
            dt.O_WORKSTATION = "1010";
            //机台号
            dt.MACHINE_NO = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
            //作业员
            dt.O_OPERATOR = "123";
            //流程卡号
            dt.FLOW_CARD_ID = DataMgr.GetInstance().m_dictDataGroup["生产料号"].m_dictData["料号"].m_dbStandardValue.ToString();
            //FileID
            dt.FILE_ID = FileID;
            //回收托盘二维码
            dt.COLLECT_BOX_CODE = Barcode;
            //测试开始时间
            dt.TEST_START_TIME = StartTime;
            //结卡标志位
            dt.TEST_ITEM_RESULT = "END";
            dt.VEHICLE_CODE = "";
            dt.POSIONT_COLUMN = "";
            dt.POSIONT_ROW = "";
            dt.DOWN_MOUTH_NO = "";
            dt.LOCATION = "";
            dt.FLOW_QR_CODE = "";
            dt.FEED_SERIAL = "";
            dt.COLLECT_PALLET_SERIAL = "";
            dt.FEED_PALLET_QR_CODE = "";
            dt.COLLECT_SERIAL = "";
            dt.QR_GRADE = "";
            dt.PORRO_NO = "";
            dt.SCAN_NO = "";
            dt.TEST_ITEM_FAULTY = "";
            dt.MOUTH_NO = "";
            dt.FEED_PALLET_SERIAL = "";
            dt.TEST_END_TIME = "";
            //dt.boxNo = "";
            #region
            MachineDataTwe.DataItem.DataItem1 dtList = new MachineDataTwe.DataItem.DataItem1();
            dtList.MODEL_SIDE = "A透明";
            dtList.TEST_ITEM_NAME = "1001";
            dtList.TEST_ITEM_VALUE = "";
            dtList.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList);
            MachineDataTwe.DataItem.DataItem1 dtList1 = new MachineDataTwe.DataItem.DataItem1();
            dtList1.MODEL_SIDE = "A透明";
            dtList1.TEST_ITEM_NAME = "1004";
            dtList1.TEST_ITEM_VALUE = "";
            dtList1.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList1);
            MachineDataTwe.DataItem.DataItem1 dtList2 = new MachineDataTwe.DataItem.DataItem1();
            dtList2.MODEL_SIDE = "A透明";
            dtList2.TEST_ITEM_NAME = "1005";
            dtList2.TEST_ITEM_VALUE = "";
            dtList2.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList2);
            MachineDataTwe.DataItem.DataItem1 dtList3 = new MachineDataTwe.DataItem.DataItem1();
            dtList3.MODEL_SIDE = "A透明";
            dtList3.TEST_ITEM_NAME = "3016";
            dtList3.TEST_ITEM_VALUE = "";
            dtList3.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList3);
            MachineDataTwe.DataItem.DataItem1 dtList4 = new MachineDataTwe.DataItem.DataItem1();
            dtList4.MODEL_SIDE = "A透明";
            dtList4.TEST_ITEM_NAME = "1000";
            dtList4.TEST_ITEM_VALUE = "";
            dtList4.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList4);
            MachineDataTwe.DataItem.DataItem1 dtList5 = new MachineDataTwe.DataItem.DataItem1();
            dtList5.MODEL_SIDE = "C透明";
            dtList5.TEST_ITEM_NAME = "1001";
            dtList5.TEST_ITEM_VALUE = "";
            dtList5.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList5);
            MachineDataTwe.DataItem.DataItem1 dtList6 = new MachineDataTwe.DataItem.DataItem1();
            dtList6.MODEL_SIDE = "C透明";
            dtList6.TEST_ITEM_NAME = "1004";
            dtList6.TEST_ITEM_VALUE = "";
            dtList6.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList6);
            MachineDataTwe.DataItem.DataItem1 dtList7 = new MachineDataTwe.DataItem.DataItem1();
            dtList7.MODEL_SIDE = "C透明";
            dtList7.TEST_ITEM_NAME = "1005";
            dtList7.TEST_ITEM_VALUE = "";
            dtList7.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList7);
            MachineDataTwe.DataItem.DataItem1 dtList8 = new MachineDataTwe.DataItem.DataItem1();
            dtList8.MODEL_SIDE = "C透明";
            dtList8.TEST_ITEM_NAME = "3016";
            dtList8.TEST_ITEM_VALUE = "";
            dtList8.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList8);
            MachineDataTwe.DataItem.DataItem1 dtList9 = new MachineDataTwe.DataItem.DataItem1();
            dtList9.MODEL_SIDE = "C透明";
            dtList9.TEST_ITEM_NAME = "1000";
            dtList9.TEST_ITEM_VALUE = "";
            dtList9.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList9);
            MachineDataTwe.DataItem.DataItem1 dtList10 = new MachineDataTwe.DataItem.DataItem1();
            dtList10.MODEL_SIDE = "A油墨区";
            dtList10.TEST_ITEM_NAME = "3077";
            dtList10.TEST_ITEM_VALUE = "";
            dtList10.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList10);
            MachineDataTwe.DataItem.DataItem1 dtList11 = new MachineDataTwe.DataItem.DataItem1();
            dtList11.MODEL_SIDE = "A油墨区";
            dtList11.TEST_ITEM_NAME = "3076";
            dtList11.TEST_ITEM_VALUE = "";
            dtList11.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList11);
            MachineDataTwe.DataItem.DataItem1 dtList12 = new MachineDataTwe.DataItem.DataItem1();
            dtList12.MODEL_SIDE = "A油墨区";
            dtList12.TEST_ITEM_NAME = "1011";
            dtList12.TEST_ITEM_VALUE = "";
            dtList12.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList12);
            MachineDataTwe.DataItem.DataItem1 dtList13 = new MachineDataTwe.DataItem.DataItem1();
            dtList13.MODEL_SIDE = "A油墨区";
            dtList13.TEST_ITEM_NAME = "1511";
            dtList13.TEST_ITEM_VALUE = "";
            dtList13.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList13);
            MachineDataTwe.DataItem.DataItem1 dtList14 = new MachineDataTwe.DataItem.DataItem1();
            dtList14.MODEL_SIDE = "A油墨区";
            dtList14.TEST_ITEM_NAME = "1510";
            dtList14.TEST_ITEM_VALUE = "";
            dtList14.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList14);
            MachineDataTwe.DataItem.DataItem1 dtList15 = new MachineDataTwe.DataItem.DataItem1();
            dtList15.MODEL_SIDE = "A油墨区";
            dtList15.TEST_ITEM_NAME = "1507";
            dtList15.TEST_ITEM_VALUE = "";
            dtList15.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList15);
            MachineDataTwe.DataItem.DataItem1 dtList16 = new MachineDataTwe.DataItem.DataItem1();
            dtList16.MODEL_SIDE = "A油墨区";
            dtList16.TEST_ITEM_NAME = "1508";
            dtList16.TEST_ITEM_VALUE = "";
            dtList16.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList16);
            MachineDataTwe.DataItem.DataItem1 dtList17 = new MachineDataTwe.DataItem.DataItem1();
            dtList17.MODEL_SIDE = "A油墨区";
            dtList17.TEST_ITEM_NAME = "3063";
            dtList17.TEST_ITEM_VALUE = "";
            dtList17.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList17);
            MachineDataTwe.DataItem.DataItem1 dtList18 = new MachineDataTwe.DataItem.DataItem1();
            dtList18.MODEL_SIDE = "A油墨区";
            dtList18.TEST_ITEM_NAME = "3012";
            dtList18.TEST_ITEM_VALUE = "";
            dtList18.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList18);
            MachineDataTwe.DataItem.DataItem1 dtList19 = new MachineDataTwe.DataItem.DataItem1();
            dtList19.MODEL_SIDE = "C油墨区";
            dtList19.TEST_ITEM_NAME = "3077";
            dtList19.TEST_ITEM_VALUE = "";
            dtList19.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList19);
            MachineDataTwe.DataItem.DataItem1 dtList20 = new MachineDataTwe.DataItem.DataItem1();
            dtList20.MODEL_SIDE = "C油墨区";
            dtList20.TEST_ITEM_NAME = "3076";
            dtList20.TEST_ITEM_VALUE = "";
            dtList20.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList20);
            MachineDataTwe.DataItem.DataItem1 dtList21 = new MachineDataTwe.DataItem.DataItem1();
            dtList21.MODEL_SIDE = "C油墨区";
            dtList21.TEST_ITEM_NAME = "1011";
            dtList21.TEST_ITEM_VALUE = "";
            dtList21.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList21);
            MachineDataTwe.DataItem.DataItem1 dtList22 = new MachineDataTwe.DataItem.DataItem1();
            dtList22.MODEL_SIDE = "C油墨区";
            dtList22.TEST_ITEM_NAME = "1511";
            dtList22.TEST_ITEM_VALUE = "";
            dtList22.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList22);
            MachineDataTwe.DataItem.DataItem1 dtList23 = new MachineDataTwe.DataItem.DataItem1();
            dtList23.MODEL_SIDE = "C油墨区";
            dtList23.TEST_ITEM_NAME = "1510";
            dtList23.TEST_ITEM_VALUE = "";
            dtList23.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList23);
            MachineDataTwe.DataItem.DataItem1 dtList24 = new MachineDataTwe.DataItem.DataItem1();
            dtList24.MODEL_SIDE = "C油墨区";
            dtList24.TEST_ITEM_NAME = "1507";
            dtList24.TEST_ITEM_VALUE = "";
            dtList24.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList24);
            MachineDataTwe.DataItem.DataItem1 dtList25 = new MachineDataTwe.DataItem.DataItem1();
            dtList25.MODEL_SIDE = "C油墨区";
            dtList25.TEST_ITEM_NAME = "1508";
            dtList25.TEST_ITEM_VALUE = "";
            dtList25.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList25);
            MachineDataTwe.DataItem.DataItem1 dtList26 = new MachineDataTwe.DataItem.DataItem1();
            dtList26.MODEL_SIDE = "C油墨区";
            dtList26.TEST_ITEM_NAME = "3063";
            dtList26.TEST_ITEM_VALUE = "";
            dtList26.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList26);
            MachineDataTwe.DataItem.DataItem1 dtList27 = new MachineDataTwe.DataItem.DataItem1();
            dtList27.MODEL_SIDE = "C油墨区";
            dtList27.TEST_ITEM_NAME = "3012";
            dtList27.TEST_ITEM_VALUE = "";
            dtList27.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList27);

            MachineDataTwe.DataItem.DataItem1 dtList28 = new MachineDataTwe.DataItem.DataItem1();
            dtList28.MODEL_SIDE = "B油墨区";
            dtList28.TEST_ITEM_NAME = "3076";
            dtList28.TEST_ITEM_VALUE = "";
            dtList28.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList28);
            MachineDataTwe.DataItem.DataItem1 dtList29 = new MachineDataTwe.DataItem.DataItem1();
            dtList29.MODEL_SIDE = "B油墨区";
            dtList29.TEST_ITEM_NAME = "1011";
            dtList29.TEST_ITEM_VALUE = "";
            dtList29.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList29);
            MachineDataTwe.DataItem.DataItem1 dtList30 = new MachineDataTwe.DataItem.DataItem1();
            dtList30.MODEL_SIDE = "B油墨区";
            dtList30.TEST_ITEM_NAME = "1511";
            dtList30.TEST_ITEM_VALUE = "";
            dtList30.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList30);
            MachineDataTwe.DataItem.DataItem1 dtList31 = new MachineDataTwe.DataItem.DataItem1();
            dtList31.MODEL_SIDE = "B油墨区";
            dtList31.TEST_ITEM_NAME = "1510";
            dtList31.TEST_ITEM_VALUE = "";
            dtList31.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList31);
            MachineDataTwe.DataItem.DataItem1 dtList32 = new MachineDataTwe.DataItem.DataItem1();
            dtList32.MODEL_SIDE = "B油墨区";
            dtList32.TEST_ITEM_NAME = "1507";
            dtList32.TEST_ITEM_VALUE = "";
            dtList32.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList32);
            MachineDataTwe.DataItem.DataItem1 dtList33 = new MachineDataTwe.DataItem.DataItem1();
            dtList33.MODEL_SIDE = "B油墨区";
            dtList33.TEST_ITEM_NAME = "1508";
            dtList33.TEST_ITEM_VALUE = "";
            dtList33.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList33);
            MachineDataTwe.DataItem.DataItem1 dtList34 = new MachineDataTwe.DataItem.DataItem1();
            dtList34.MODEL_SIDE = "B油墨区";
            dtList34.TEST_ITEM_NAME = "3063";
            dtList34.TEST_ITEM_VALUE = "";
            dtList34.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList34);

            MachineDataTwe.DataItem.DataItem1 dtList35 = new MachineDataTwe.DataItem.DataItem1();
            dtList35.MODEL_SIDE = "D油墨区";
            dtList35.TEST_ITEM_NAME = "3076";
            dtList35.TEST_ITEM_VALUE = "";
            dtList35.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList35);
            MachineDataTwe.DataItem.DataItem1 dtList36 = new MachineDataTwe.DataItem.DataItem1();
            dtList36.MODEL_SIDE = "D油墨区";
            dtList36.TEST_ITEM_NAME = "1011";
            dtList36.TEST_ITEM_VALUE = "";
            dtList36.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList36);
            MachineDataTwe.DataItem.DataItem1 dtList37 = new MachineDataTwe.DataItem.DataItem1();
            dtList37.MODEL_SIDE = "D油墨区";
            dtList37.TEST_ITEM_NAME = "1511";
            dtList37.TEST_ITEM_VALUE = "";
            dtList37.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList37);
            MachineDataTwe.DataItem.DataItem1 dtList38 = new MachineDataTwe.DataItem.DataItem1();
            dtList38.MODEL_SIDE = "D油墨区";
            dtList38.TEST_ITEM_NAME = "1510";
            dtList38.TEST_ITEM_VALUE = "";
            dtList38.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList38);

            MachineDataTwe.DataItem.DataItem1 dtList39 = new MachineDataTwe.DataItem.DataItem1();
            dtList39.MODEL_SIDE = "D油墨区";
            dtList39.TEST_ITEM_NAME = "1507";
            dtList39.TEST_ITEM_VALUE = "";
            dtList39.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList39);

            MachineDataTwe.DataItem.DataItem1 dtList40 = new MachineDataTwe.DataItem.DataItem1();
            dtList40.MODEL_SIDE = "D油墨区";
            dtList40.TEST_ITEM_NAME = "1508";
            dtList40.TEST_ITEM_VALUE = "";
            dtList40.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList40);
            MachineDataTwe.DataItem.DataItem1 dtList41 = new MachineDataTwe.DataItem.DataItem1();
            dtList41.MODEL_SIDE = "D油墨区";
            dtList41.TEST_ITEM_NAME = "3063";
            dtList41.TEST_ITEM_VALUE = "";
            dtList41.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList41);

            MachineDataTwe.DataItem.DataItem1 dtList42 = new MachineDataTwe.DataItem.DataItem1();
            dtList42.MODEL_SIDE = "E油墨区";
            dtList42.TEST_ITEM_NAME = "1011";
            dtList42.TEST_ITEM_VALUE = "";
            dtList42.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList42);
            MachineDataTwe.DataItem.DataItem1 dtList43 = new MachineDataTwe.DataItem.DataItem1();
            dtList43.MODEL_SIDE = "E油墨区";
            dtList43.TEST_ITEM_NAME = "1511";
            dtList43.TEST_ITEM_VALUE = "";
            dtList43.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList43);
            MachineDataTwe.DataItem.DataItem1 dtList44 = new MachineDataTwe.DataItem.DataItem1();
            dtList44.MODEL_SIDE = "E油墨区";
            dtList44.TEST_ITEM_NAME = "1510";
            dtList44.TEST_ITEM_VALUE = "";
            dtList44.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList44);
            MachineDataTwe.DataItem.DataItem1 dtList45 = new MachineDataTwe.DataItem.DataItem1();
            dtList45.MODEL_SIDE = "E油墨区";
            dtList45.TEST_ITEM_NAME = "1507";
            dtList45.TEST_ITEM_VALUE = "";
            dtList45.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList45);
            MachineDataTwe.DataItem.DataItem1 dtList46 = new MachineDataTwe.DataItem.DataItem1();
            dtList46.MODEL_SIDE = "E油墨区";
            dtList46.TEST_ITEM_NAME = "1508";
            dtList46.TEST_ITEM_VALUE = "";
            dtList46.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList46);

            MachineDataTwe.DataItem.DataItem1 dtList47 = new MachineDataTwe.DataItem.DataItem1();
            dtList47.MODEL_SIDE = "F油墨区";
            dtList47.TEST_ITEM_NAME = "1011";
            dtList47.TEST_ITEM_VALUE = "";
            dtList47.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList47);
            MachineDataTwe.DataItem.DataItem1 dtList48 = new MachineDataTwe.DataItem.DataItem1();
            dtList48.MODEL_SIDE = "F油墨区";
            dtList48.TEST_ITEM_NAME = "1511";
            dtList48.TEST_ITEM_VALUE = "";
            dtList48.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList48);
            MachineDataTwe.DataItem.DataItem1 dtList49 = new MachineDataTwe.DataItem.DataItem1();
            dtList49.MODEL_SIDE = "F油墨区";
            dtList49.TEST_ITEM_NAME = "1510";
            dtList49.TEST_ITEM_VALUE = "";
            dtList49.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList49);
            MachineDataTwe.DataItem.DataItem1 dtList50 = new MachineDataTwe.DataItem.DataItem1();
            dtList50.MODEL_SIDE = "F油墨区";
            dtList50.TEST_ITEM_NAME = "1507";
            dtList50.TEST_ITEM_VALUE = "";
            dtList50.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList50);
            MachineDataTwe.DataItem.DataItem1 dtList51 = new MachineDataTwe.DataItem.DataItem1();
            dtList51.MODEL_SIDE = "F油墨区";
            dtList51.TEST_ITEM_NAME = "1508";
            dtList51.TEST_ITEM_VALUE = "";
            dtList51.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList51);

            MachineDataTwe.DataItem.DataItem1 dtList52 = new MachineDataTwe.DataItem.DataItem1();
            dtList52.MODEL_SIDE = "B镜面";
            dtList52.TEST_ITEM_NAME = "1011";
            dtList52.TEST_ITEM_VALUE = "";
            dtList52.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList52);
            MachineDataTwe.DataItem.DataItem1 dtList53 = new MachineDataTwe.DataItem.DataItem1();
            dtList53.MODEL_SIDE = "B镜面";
            dtList53.TEST_ITEM_NAME = "1004";
            dtList53.TEST_ITEM_VALUE = "";
            dtList53.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList53);
            MachineDataTwe.DataItem.DataItem1 dtList54 = new MachineDataTwe.DataItem.DataItem1();
            dtList54.MODEL_SIDE = "B镜面";
            dtList54.TEST_ITEM_NAME = "1005";
            dtList54.TEST_ITEM_VALUE = "";
            dtList54.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList54);
            MachineDataTwe.DataItem.DataItem1 dtList55 = new MachineDataTwe.DataItem.DataItem1();
            dtList55.MODEL_SIDE = "B镜面";
            dtList55.TEST_ITEM_NAME = "3091";
            dtList55.TEST_ITEM_VALUE = "";
            dtList55.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList55);
            MachineDataTwe.DataItem.DataItem1 dtList56 = new MachineDataTwe.DataItem.DataItem1();
            dtList56.MODEL_SIDE = "B镜面";
            dtList56.TEST_ITEM_NAME = "1000";
            dtList56.TEST_ITEM_VALUE = "";
            dtList56.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList56);

            MachineDataTwe.DataItem.DataItem1 dtList57 = new MachineDataTwe.DataItem.DataItem1();
            dtList57.MODEL_SIDE = "D镜面";
            dtList57.TEST_ITEM_NAME = "1011";
            dtList57.TEST_ITEM_VALUE = "";
            dtList57.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList57);
            MachineDataTwe.DataItem.DataItem1 dtList58 = new MachineDataTwe.DataItem.DataItem1();
            dtList58.MODEL_SIDE = "D镜面";
            dtList58.TEST_ITEM_NAME = "1004";
            dtList58.TEST_ITEM_VALUE = "";
            dtList58.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList58);
            MachineDataTwe.DataItem.DataItem1 dtList59 = new MachineDataTwe.DataItem.DataItem1();
            dtList59.MODEL_SIDE = "D镜面";
            dtList59.TEST_ITEM_NAME = "1005";
            dtList59.TEST_ITEM_VALUE = "";
            dtList59.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList59);
            MachineDataTwe.DataItem.DataItem1 dtList60 = new MachineDataTwe.DataItem.DataItem1();
            dtList60.MODEL_SIDE = "D镜面";
            dtList60.TEST_ITEM_NAME = "3091";
            dtList60.TEST_ITEM_VALUE = "";
            dtList60.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList60);
            MachineDataTwe.DataItem.DataItem1 dtList61 = new MachineDataTwe.DataItem.DataItem1();
            dtList61.MODEL_SIDE = "D镜面";
            dtList61.TEST_ITEM_NAME = "1000";
            dtList61.TEST_ITEM_VALUE = "";
            dtList61.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList61);

            MachineDataTwe.DataItem.DataItem1 dtList62 = new MachineDataTwe.DataItem.DataItem1();
            dtList62.MODEL_SIDE = "BL1";
            dtList62.TEST_ITEM_NAME = "1022";
            dtList62.TEST_ITEM_VALUE = "";
            dtList62.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList62);
            MachineDataTwe.DataItem.DataItem1 dtList63 = new MachineDataTwe.DataItem.DataItem1();
            dtList63.MODEL_SIDE = "BL1";
            dtList63.TEST_ITEM_NAME = "1022";
            dtList63.TEST_ITEM_VALUE = "";
            dtList63.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList63);
            MachineDataTwe.DataItem.DataItem1 dtList64 = new MachineDataTwe.DataItem.DataItem1();
            dtList64.MODEL_SIDE = "BL2";
            dtList64.TEST_ITEM_NAME = "1022";
            dtList64.TEST_ITEM_VALUE = "";
            dtList64.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList64);
            MachineDataTwe.DataItem.DataItem1 dtList65 = new MachineDataTwe.DataItem.DataItem1();
            dtList65.MODEL_SIDE = "BL2";
            dtList65.TEST_ITEM_NAME = "1022";
            dtList65.TEST_ITEM_VALUE = "";
            dtList65.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList65);

            MachineDataTwe.DataItem.DataItem1 dtList66 = new MachineDataTwe.DataItem.DataItem1();
            dtList66.MODEL_SIDE = "其他不良";
            dtList66.TEST_ITEM_NAME = "1513";
            dtList66.TEST_ITEM_VALUE = "";
            dtList66.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList66);
            MachineDataTwe.DataItem.DataItem1 dtList67 = new MachineDataTwe.DataItem.DataItem1();
            dtList67.MODEL_SIDE = "其他不良";
            dtList67.TEST_ITEM_NAME = "1501";
            dtList67.TEST_ITEM_VALUE = "";
            dtList67.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList67);
            MachineDataTwe.DataItem.DataItem1 dtList68 = new MachineDataTwe.DataItem.DataItem1();
            dtList68.MODEL_SIDE = "其他不良";
            dtList68.TEST_ITEM_NAME = "4232";
            dtList68.TEST_ITEM_VALUE = "";
            dtList68.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList68);
            MachineDataTwe.DataItem.DataItem1 dtList69 = new MachineDataTwe.DataItem.DataItem1();
            dtList69.MODEL_SIDE = "其他不良";
            dtList69.TEST_ITEM_NAME = "4230";
            dtList69.TEST_ITEM_VALUE = "";
            dtList69.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList69);
            MachineDataTwe.DataItem.DataItem1 dtList70 = new MachineDataTwe.DataItem.DataItem1();
            dtList70.MODEL_SIDE = "其他不良";
            dtList70.TEST_ITEM_NAME = "2000";
            dtList70.TEST_ITEM_VALUE = "";
            dtList70.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList70);
            MachineDataTwe.DataItem.DataItem1 dtList71 = new MachineDataTwe.DataItem.DataItem1();
            dtList71.MODEL_SIDE = "其他不良";
            dtList71.TEST_ITEM_NAME = "6013";
            dtList71.TEST_ITEM_VALUE = "";
            dtList71.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList71);
            MachineDataTwe.DataItem.DataItem1 dtList72 = new MachineDataTwe.DataItem.DataItem1();
            dtList72.MODEL_SIDE = "其他不良";
            dtList72.TEST_ITEM_NAME = "1022";
            dtList72.TEST_ITEM_VALUE = "";
            dtList72.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList72);
            MachineDataTwe.DataItem.DataItem1 dtList73 = new MachineDataTwe.DataItem.DataItem1();
            dtList73.MODEL_SIDE = "其他不良";
            dtList73.TEST_ITEM_NAME = "6999";
            dtList73.TEST_ITEM_VALUE = "";
            dtList73.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList73);

            MachineDataTwe.DataItem.DataItem1 dtList74 = new MachineDataTwe.DataItem.DataItem1();
            dtList74.MODEL_SIDE = "尺寸缺陷";
            dtList74.TEST_ITEM_NAME = "FAI2";
            dtList74.TEST_ITEM_VALUE = "";
            dtList74.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList74);
            MachineDataTwe.DataItem.DataItem1 dtList75 = new MachineDataTwe.DataItem.DataItem1();
            dtList75.MODEL_SIDE = "尺寸缺陷";
            dtList75.TEST_ITEM_NAME = "FAI4";
            dtList75.TEST_ITEM_VALUE = "";
            dtList75.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList75);
            MachineDataTwe.DataItem.DataItem1 dtList76 = new MachineDataTwe.DataItem.DataItem1();
            dtList76.MODEL_SIDE = "尺寸缺陷";
            dtList76.TEST_ITEM_NAME = "FAI6";
            dtList76.TEST_ITEM_VALUE = "";
            dtList76.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList76);
            MachineDataTwe.DataItem.DataItem1 dtList77 = new MachineDataTwe.DataItem.DataItem1();
            dtList77.MODEL_SIDE = "尺寸缺陷";
            dtList77.TEST_ITEM_NAME = "FAI10";
            dtList77.TEST_ITEM_VALUE = "";
            dtList77.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList77);
            MachineDataTwe.DataItem.DataItem1 dtList78 = new MachineDataTwe.DataItem.DataItem1();
            dtList78.MODEL_SIDE = "尺寸缺陷";
            dtList78.TEST_ITEM_NAME = "FAI12";
            dtList78.TEST_ITEM_VALUE = "";
            dtList78.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList78);
            MachineDataTwe.DataItem.DataItem1 dtList79 = new MachineDataTwe.DataItem.DataItem1();
            dtList79.MODEL_SIDE = "尺寸缺陷";
            dtList79.TEST_ITEM_NAME = "FAI13";
            dtList79.TEST_ITEM_VALUE = "";
            dtList79.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList79);
            MachineDataTwe.DataItem.DataItem1 dtList80 = new MachineDataTwe.DataItem.DataItem1();
            dtList80.MODEL_SIDE = "尺寸缺陷";
            dtList80.TEST_ITEM_NAME = "FAI14";
            dtList80.TEST_ITEM_VALUE = "";
            dtList80.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList80);
            MachineDataTwe.DataItem.DataItem1 dtList81 = new MachineDataTwe.DataItem.DataItem1();
            dtList81.MODEL_SIDE = "尺寸缺陷";
            dtList81.TEST_ITEM_NAME = "FAI18";
            dtList81.TEST_ITEM_VALUE = "";
            dtList81.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList81);
            MachineDataTwe.DataItem.DataItem1 dtList82 = new MachineDataTwe.DataItem.DataItem1();
            dtList82.MODEL_SIDE = "尺寸缺陷";
            dtList82.TEST_ITEM_NAME = "FAI32";
            dtList82.TEST_ITEM_VALUE = "";
            dtList82.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList82);
            MachineDataTwe.DataItem.DataItem1 dtList83 = new MachineDataTwe.DataItem.DataItem1();
            dtList83.MODEL_SIDE = "尺寸缺陷";
            dtList83.TEST_ITEM_NAME = "FAI36-1";
            dtList83.TEST_ITEM_VALUE = "";
            dtList83.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList83);
            MachineDataTwe.DataItem.DataItem1 dtList84 = new MachineDataTwe.DataItem.DataItem1();
            dtList84.MODEL_SIDE = "尺寸缺陷";
            dtList84.TEST_ITEM_NAME = "FAI36-2";
            dtList84.TEST_ITEM_VALUE = "";
            dtList84.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList84);
            MachineDataTwe.DataItem.DataItem1 dtList85 = new MachineDataTwe.DataItem.DataItem1();
            dtList85.MODEL_SIDE = "尺寸缺陷";
            dtList85.TEST_ITEM_NAME = "FAI26";
            dtList85.TEST_ITEM_VALUE = "";
            dtList85.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList85);
            MachineDataTwe.DataItem.DataItem1 dtList86 = new MachineDataTwe.DataItem.DataItem1();
            dtList86.MODEL_SIDE = "尺寸缺陷";
            dtList86.TEST_ITEM_NAME = "FAI27";
            dtList86.TEST_ITEM_VALUE = "";
            dtList86.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList86);
            MachineDataTwe.DataItem.DataItem1 dtList87 = new MachineDataTwe.DataItem.DataItem1();
            dtList87.MODEL_SIDE = "尺寸缺陷";
            dtList87.TEST_ITEM_NAME = "FAI28";
            dtList87.TEST_ITEM_VALUE = "";
            dtList87.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList87);
            MachineDataTwe.DataItem.DataItem1 dtList88 = new MachineDataTwe.DataItem.DataItem1();
            dtList88.MODEL_SIDE = "尺寸缺陷";
            dtList88.TEST_ITEM_NAME = "FAI33";
            dtList88.TEST_ITEM_VALUE = "";
            dtList88.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList88);

            dt.ItemList = ItemList;

            #endregion
            //dt.reportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.Add(dt);

            MS.data = DESEncrypt(JsonConvert.SerializeObject(data), "sjgd2023gyhlwiot");

            message = JsonConvert.SerializeObject(MS);
            WaitReceiver WReceiver = new WaitReceiver();
            WReceiver.Msg_id = MS.msg_id;
            WReceiver.Lot_ID = DateTime.Now.ToString("yyyyMMddHH");
            //if (Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHH").Substring(8, 2)) % 2 == 0)
            //{
            //    WReceiver.Lot_ID = DateTime.Now.ToString("yyyyMMddHH");
            //}
            //else
            //{
            //    WReceiver.Lot_ID = (Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHH")) - 1).ToString();
            //}
            WReceiver.ProductCode = FileID;
            WReceiver.fileNamelog = WReceiver.ProductCode + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            WReceiver.Send_msg = message;

            if (ConStatue)
            {
                msg_idlist.TryAdd(MS.msg_id, WReceiver);
                SaveMQTTBufferlog(WReceiver);
            }
            else
            {
                SaveMQTTStemplog(WReceiver);
            }

            if (string.IsNullOrEmpty(topic))
            {
                MessageBox.Show("发布主题不能为空！");
                return;
            }
            //MQTT.GetMqtt();
            Mqtt_Send(topic, message);
        }
        /// <summary>
        /// AOI NG料盘结工单报工
        /// </summary>    
        /// <param name="message"></param>
        public static void ReportAOINGEnd(string starttime, out string message)
        {

            string parameters = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString() + ".reply";
            //MQTT.Mqtt_Subscribe(parameters);

            string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString();
            MachineDataTwe MS = new MachineDataTwe();
            MachineDataTwe.DataItem MS1 = new MachineDataTwe.DataItem();
            DateTime time = DateTime.Now;
            long Time = ConvertDateTimeToInt(time);

            //List<string[]> DataList = ExcelNPOI.test1();

            MS.msg_id = "ps-" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString() + "-" + Time;    ///车间内使用 101016000171          车间外使用   101016000201
            MS.deviceno = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString();
            MS.dataType = "aoi";
            //MS.origntime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            MS.deviceType = "data.device.aoi.reply";
            List<MachineDataTwe.DataItem> data = new List<MachineDataTwe.DataItem>();

            MachineDataTwe.DataItem dt = new MachineDataTwe.DataItem();
            List<MachineDataTwe.DataItem.DataItem1> ItemList = new List<MachineDataTwe.DataItem.DataItem1>();

            //工站编号
            dt.O_WORKSTATION = "1010";
            //机台号
            dt.MACHINE_NO = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
            //作业员
            dt.O_OPERATOR = "123";
            //流程卡号
            dt.FLOW_CARD_ID = DataMgr.GetInstance().m_dictDataGroup["生产料号"].m_dictData["料号"].m_dbStandardValue.ToString();
            //FileID
            dt.FILE_ID = "0";
            //回收托盘二维码
            dt.COLLECT_BOX_CODE = "";
            //测试开始时间
            dt.TEST_START_TIME = starttime;
            //结卡标志位
            dt.TEST_ITEM_RESULT = "END";


            dt.VEHICLE_CODE = "";
            dt.POSIONT_COLUMN = "";
            dt.POSIONT_ROW = "";
            dt.DOWN_MOUTH_NO = "";
            dt.LOCATION = "";
            dt.FLOW_QR_CODE = "";
            dt.FEED_SERIAL = "";
            dt.COLLECT_PALLET_SERIAL = "";
            dt.FEED_PALLET_QR_CODE = "";
            dt.COLLECT_SERIAL = "";
            dt.QR_GRADE = "";
            dt.PORRO_NO = "";
            dt.SCAN_NO = "";
            dt.TEST_ITEM_FAULTY = "";
            dt.MOUTH_NO = "";
            dt.FEED_PALLET_SERIAL = "";
            dt.TEST_END_TIME = "";
            //dt.boxNo = "";

            MachineDataTwe.DataItem.DataItem1 dtList = new MachineDataTwe.DataItem.DataItem1();
            dtList.MODEL_SIDE = "A透明";
            dtList.TEST_ITEM_NAME = "1001";
            dtList.TEST_ITEM_VALUE = "";
            dtList.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList);
            MachineDataTwe.DataItem.DataItem1 dtList1 = new MachineDataTwe.DataItem.DataItem1();
            dtList1.MODEL_SIDE = "A透明";
            dtList1.TEST_ITEM_NAME = "1004";
            dtList1.TEST_ITEM_VALUE = "";
            dtList1.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList1);
            MachineDataTwe.DataItem.DataItem1 dtList2 = new MachineDataTwe.DataItem.DataItem1();
            dtList2.MODEL_SIDE = "A透明";
            dtList2.TEST_ITEM_NAME = "1005";
            dtList2.TEST_ITEM_VALUE = "";
            dtList2.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList2);
            MachineDataTwe.DataItem.DataItem1 dtList3 = new MachineDataTwe.DataItem.DataItem1();
            dtList3.MODEL_SIDE = "A透明";
            dtList3.TEST_ITEM_NAME = "3016";
            dtList3.TEST_ITEM_VALUE = "";
            dtList3.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList3);
            MachineDataTwe.DataItem.DataItem1 dtList4 = new MachineDataTwe.DataItem.DataItem1();
            dtList4.MODEL_SIDE = "A透明";
            dtList4.TEST_ITEM_NAME = "1000";
            dtList4.TEST_ITEM_VALUE = "";
            dtList4.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList4);
            MachineDataTwe.DataItem.DataItem1 dtList5 = new MachineDataTwe.DataItem.DataItem1();
            dtList5.MODEL_SIDE = "C透明";
            dtList5.TEST_ITEM_NAME = "1001";
            dtList5.TEST_ITEM_VALUE = "";
            dtList5.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList5);
            MachineDataTwe.DataItem.DataItem1 dtList6 = new MachineDataTwe.DataItem.DataItem1();
            dtList6.MODEL_SIDE = "C透明";
            dtList6.TEST_ITEM_NAME = "1004";
            dtList6.TEST_ITEM_VALUE = "";
            dtList6.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList6);
            MachineDataTwe.DataItem.DataItem1 dtList7 = new MachineDataTwe.DataItem.DataItem1();
            dtList7.MODEL_SIDE = "C透明";
            dtList7.TEST_ITEM_NAME = "1005";
            dtList7.TEST_ITEM_VALUE = "";
            dtList7.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList7);
            MachineDataTwe.DataItem.DataItem1 dtList8 = new MachineDataTwe.DataItem.DataItem1();
            dtList8.MODEL_SIDE = "C透明";
            dtList8.TEST_ITEM_NAME = "3016";
            dtList8.TEST_ITEM_VALUE = "";
            dtList8.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList8);
            MachineDataTwe.DataItem.DataItem1 dtList9 = new MachineDataTwe.DataItem.DataItem1();
            dtList9.MODEL_SIDE = "C透明";
            dtList9.TEST_ITEM_NAME = "1000";
            dtList9.TEST_ITEM_VALUE = "";
            dtList9.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList9);
            MachineDataTwe.DataItem.DataItem1 dtList10 = new MachineDataTwe.DataItem.DataItem1();
            dtList10.MODEL_SIDE = "A油墨区";
            dtList10.TEST_ITEM_NAME = "3077";
            dtList10.TEST_ITEM_VALUE = "";
            dtList10.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList10);
            MachineDataTwe.DataItem.DataItem1 dtList11 = new MachineDataTwe.DataItem.DataItem1();
            dtList11.MODEL_SIDE = "A油墨区";
            dtList11.TEST_ITEM_NAME = "3076";
            dtList11.TEST_ITEM_VALUE = "";
            dtList11.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList11);
            MachineDataTwe.DataItem.DataItem1 dtList12 = new MachineDataTwe.DataItem.DataItem1();
            dtList12.MODEL_SIDE = "A油墨区";
            dtList12.TEST_ITEM_NAME = "1011";
            dtList12.TEST_ITEM_VALUE = "";
            dtList12.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList12);
            MachineDataTwe.DataItem.DataItem1 dtList13 = new MachineDataTwe.DataItem.DataItem1();
            dtList13.MODEL_SIDE = "A油墨区";
            dtList13.TEST_ITEM_NAME = "1511";
            dtList13.TEST_ITEM_VALUE = "";
            dtList13.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList13);
            MachineDataTwe.DataItem.DataItem1 dtList14 = new MachineDataTwe.DataItem.DataItem1();
            dtList14.MODEL_SIDE = "A油墨区";
            dtList14.TEST_ITEM_NAME = "1510";
            dtList14.TEST_ITEM_VALUE = "";
            dtList14.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList14);
            MachineDataTwe.DataItem.DataItem1 dtList15 = new MachineDataTwe.DataItem.DataItem1();
            dtList15.MODEL_SIDE = "A油墨区";
            dtList15.TEST_ITEM_NAME = "1507";
            dtList15.TEST_ITEM_VALUE = "";
            dtList15.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList15);
            MachineDataTwe.DataItem.DataItem1 dtList16 = new MachineDataTwe.DataItem.DataItem1();
            dtList16.MODEL_SIDE = "A油墨区";
            dtList16.TEST_ITEM_NAME = "1508";
            dtList16.TEST_ITEM_VALUE = "";
            dtList16.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList16);
            MachineDataTwe.DataItem.DataItem1 dtList17 = new MachineDataTwe.DataItem.DataItem1();
            dtList17.MODEL_SIDE = "A油墨区";
            dtList17.TEST_ITEM_NAME = "3063";
            dtList17.TEST_ITEM_VALUE = "";
            dtList17.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList17);
            MachineDataTwe.DataItem.DataItem1 dtList18 = new MachineDataTwe.DataItem.DataItem1();
            dtList18.MODEL_SIDE = "A油墨区";
            dtList18.TEST_ITEM_NAME = "3012";
            dtList18.TEST_ITEM_VALUE = "";
            dtList18.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList18);
            MachineDataTwe.DataItem.DataItem1 dtList19 = new MachineDataTwe.DataItem.DataItem1();
            dtList19.MODEL_SIDE = "C油墨区";
            dtList19.TEST_ITEM_NAME = "3077";
            dtList19.TEST_ITEM_VALUE = "";
            dtList19.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList19);
            MachineDataTwe.DataItem.DataItem1 dtList20 = new MachineDataTwe.DataItem.DataItem1();
            dtList20.MODEL_SIDE = "C油墨区";
            dtList20.TEST_ITEM_NAME = "3076";
            dtList20.TEST_ITEM_VALUE = "";
            dtList20.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList20);
            MachineDataTwe.DataItem.DataItem1 dtList21 = new MachineDataTwe.DataItem.DataItem1();
            dtList21.MODEL_SIDE = "C油墨区";
            dtList21.TEST_ITEM_NAME = "1011";
            dtList21.TEST_ITEM_VALUE = "";
            dtList21.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList21);
            MachineDataTwe.DataItem.DataItem1 dtList22 = new MachineDataTwe.DataItem.DataItem1();
            dtList22.MODEL_SIDE = "C油墨区";
            dtList22.TEST_ITEM_NAME = "1511";
            dtList22.TEST_ITEM_VALUE = "";
            dtList22.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList22);
            MachineDataTwe.DataItem.DataItem1 dtList23 = new MachineDataTwe.DataItem.DataItem1();
            dtList23.MODEL_SIDE = "C油墨区";
            dtList23.TEST_ITEM_NAME = "1510";
            dtList23.TEST_ITEM_VALUE = "";
            dtList23.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList23);
            MachineDataTwe.DataItem.DataItem1 dtList24 = new MachineDataTwe.DataItem.DataItem1();
            dtList24.MODEL_SIDE = "C油墨区";
            dtList24.TEST_ITEM_NAME = "1507";
            dtList24.TEST_ITEM_VALUE = "";
            dtList24.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList24);
            MachineDataTwe.DataItem.DataItem1 dtList25 = new MachineDataTwe.DataItem.DataItem1();
            dtList25.MODEL_SIDE = "C油墨区";
            dtList25.TEST_ITEM_NAME = "1508";
            dtList25.TEST_ITEM_VALUE = "";
            dtList25.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList25);
            MachineDataTwe.DataItem.DataItem1 dtList26 = new MachineDataTwe.DataItem.DataItem1();
            dtList26.MODEL_SIDE = "C油墨区";
            dtList26.TEST_ITEM_NAME = "3063";
            dtList26.TEST_ITEM_VALUE = "";
            dtList26.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList26);
            MachineDataTwe.DataItem.DataItem1 dtList27 = new MachineDataTwe.DataItem.DataItem1();
            dtList27.MODEL_SIDE = "C油墨区";
            dtList27.TEST_ITEM_NAME = "3012";
            dtList27.TEST_ITEM_VALUE = "";
            dtList27.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList27);

            MachineDataTwe.DataItem.DataItem1 dtList28 = new MachineDataTwe.DataItem.DataItem1();
            dtList28.MODEL_SIDE = "B油墨区";
            dtList28.TEST_ITEM_NAME = "3076";
            dtList28.TEST_ITEM_VALUE = "";
            dtList28.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList28);
            MachineDataTwe.DataItem.DataItem1 dtList29 = new MachineDataTwe.DataItem.DataItem1();
            dtList29.MODEL_SIDE = "B油墨区";
            dtList29.TEST_ITEM_NAME = "1011";
            dtList29.TEST_ITEM_VALUE = "";
            dtList29.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList29);
            MachineDataTwe.DataItem.DataItem1 dtList30 = new MachineDataTwe.DataItem.DataItem1();
            dtList30.MODEL_SIDE = "B油墨区";
            dtList30.TEST_ITEM_NAME = "1511";
            dtList30.TEST_ITEM_VALUE = "";
            dtList30.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList30);
            MachineDataTwe.DataItem.DataItem1 dtList31 = new MachineDataTwe.DataItem.DataItem1();
            dtList31.MODEL_SIDE = "B油墨区";
            dtList31.TEST_ITEM_NAME = "1510";
            dtList31.TEST_ITEM_VALUE = "";
            dtList31.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList31);
            MachineDataTwe.DataItem.DataItem1 dtList32 = new MachineDataTwe.DataItem.DataItem1();
            dtList32.MODEL_SIDE = "B油墨区";
            dtList32.TEST_ITEM_NAME = "1507";
            dtList32.TEST_ITEM_VALUE = "";
            dtList32.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList32);
            MachineDataTwe.DataItem.DataItem1 dtList33 = new MachineDataTwe.DataItem.DataItem1();
            dtList33.MODEL_SIDE = "B油墨区";
            dtList33.TEST_ITEM_NAME = "1508";
            dtList33.TEST_ITEM_VALUE = "";
            dtList33.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList33);
            MachineDataTwe.DataItem.DataItem1 dtList34 = new MachineDataTwe.DataItem.DataItem1();
            dtList34.MODEL_SIDE = "B油墨区";
            dtList34.TEST_ITEM_NAME = "3063";
            dtList34.TEST_ITEM_VALUE = "";
            dtList34.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList34);

            MachineDataTwe.DataItem.DataItem1 dtList35 = new MachineDataTwe.DataItem.DataItem1();
            dtList35.MODEL_SIDE = "D油墨区";
            dtList35.TEST_ITEM_NAME = "3076";
            dtList35.TEST_ITEM_VALUE = "";
            dtList35.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList35);
            MachineDataTwe.DataItem.DataItem1 dtList36 = new MachineDataTwe.DataItem.DataItem1();
            dtList36.MODEL_SIDE = "D油墨区";
            dtList36.TEST_ITEM_NAME = "1011";
            dtList36.TEST_ITEM_VALUE = "";
            dtList36.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList36);
            MachineDataTwe.DataItem.DataItem1 dtList37 = new MachineDataTwe.DataItem.DataItem1();
            dtList37.MODEL_SIDE = "D油墨区";
            dtList37.TEST_ITEM_NAME = "1511";
            dtList37.TEST_ITEM_VALUE = "";
            dtList37.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList37);
            MachineDataTwe.DataItem.DataItem1 dtList38 = new MachineDataTwe.DataItem.DataItem1();
            dtList38.MODEL_SIDE = "D油墨区";
            dtList38.TEST_ITEM_NAME = "1510";
            dtList38.TEST_ITEM_VALUE = "";
            dtList38.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList38);

            MachineDataTwe.DataItem.DataItem1 dtList39 = new MachineDataTwe.DataItem.DataItem1();
            dtList39.MODEL_SIDE = "D油墨区";
            dtList39.TEST_ITEM_NAME = "1507";
            dtList39.TEST_ITEM_VALUE = "";
            dtList39.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList39);

            MachineDataTwe.DataItem.DataItem1 dtList40 = new MachineDataTwe.DataItem.DataItem1();
            dtList40.MODEL_SIDE = "D油墨区";
            dtList40.TEST_ITEM_NAME = "1508";
            dtList40.TEST_ITEM_VALUE = "";
            dtList40.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList40);
            MachineDataTwe.DataItem.DataItem1 dtList41 = new MachineDataTwe.DataItem.DataItem1();
            dtList41.MODEL_SIDE = "D油墨区";
            dtList41.TEST_ITEM_NAME = "3063";
            dtList41.TEST_ITEM_VALUE = "";
            dtList41.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList41);

            MachineDataTwe.DataItem.DataItem1 dtList42 = new MachineDataTwe.DataItem.DataItem1();
            dtList42.MODEL_SIDE = "E油墨区";
            dtList42.TEST_ITEM_NAME = "1011";
            dtList42.TEST_ITEM_VALUE = "";
            dtList42.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList42);
            MachineDataTwe.DataItem.DataItem1 dtList43 = new MachineDataTwe.DataItem.DataItem1();
            dtList43.MODEL_SIDE = "E油墨区";
            dtList43.TEST_ITEM_NAME = "1511";
            dtList43.TEST_ITEM_VALUE = "";
            dtList43.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList43);
            MachineDataTwe.DataItem.DataItem1 dtList44 = new MachineDataTwe.DataItem.DataItem1();
            dtList44.MODEL_SIDE = "E油墨区";
            dtList44.TEST_ITEM_NAME = "1510";
            dtList44.TEST_ITEM_VALUE = "";
            dtList44.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList44);
            MachineDataTwe.DataItem.DataItem1 dtList45 = new MachineDataTwe.DataItem.DataItem1();
            dtList45.MODEL_SIDE = "E油墨区";
            dtList45.TEST_ITEM_NAME = "1507";
            dtList45.TEST_ITEM_VALUE = "";
            dtList45.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList45);
            MachineDataTwe.DataItem.DataItem1 dtList46 = new MachineDataTwe.DataItem.DataItem1();
            dtList46.MODEL_SIDE = "E油墨区";
            dtList46.TEST_ITEM_NAME = "1508";
            dtList46.TEST_ITEM_VALUE = "";
            dtList46.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList46);

            MachineDataTwe.DataItem.DataItem1 dtList47 = new MachineDataTwe.DataItem.DataItem1();
            dtList47.MODEL_SIDE = "F油墨区";
            dtList47.TEST_ITEM_NAME = "1011";
            dtList47.TEST_ITEM_VALUE = "";
            dtList47.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList47);
            MachineDataTwe.DataItem.DataItem1 dtList48 = new MachineDataTwe.DataItem.DataItem1();
            dtList48.MODEL_SIDE = "F油墨区";
            dtList48.TEST_ITEM_NAME = "1511";
            dtList48.TEST_ITEM_VALUE = "";
            dtList48.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList48);
            MachineDataTwe.DataItem.DataItem1 dtList49 = new MachineDataTwe.DataItem.DataItem1();
            dtList49.MODEL_SIDE = "F油墨区";
            dtList49.TEST_ITEM_NAME = "1510";
            dtList49.TEST_ITEM_VALUE = "";
            dtList49.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList49);
            MachineDataTwe.DataItem.DataItem1 dtList50 = new MachineDataTwe.DataItem.DataItem1();
            dtList50.MODEL_SIDE = "F油墨区";
            dtList50.TEST_ITEM_NAME = "1507";
            dtList50.TEST_ITEM_VALUE = "";
            dtList50.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList50);
            MachineDataTwe.DataItem.DataItem1 dtList51 = new MachineDataTwe.DataItem.DataItem1();
            dtList51.MODEL_SIDE = "F油墨区";
            dtList51.TEST_ITEM_NAME = "1508";
            dtList51.TEST_ITEM_VALUE = "";
            dtList51.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList51);

            MachineDataTwe.DataItem.DataItem1 dtList52 = new MachineDataTwe.DataItem.DataItem1();
            dtList52.MODEL_SIDE = "B镜面";
            dtList52.TEST_ITEM_NAME = "1011";
            dtList52.TEST_ITEM_VALUE = "";
            dtList52.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList52);
            MachineDataTwe.DataItem.DataItem1 dtList53 = new MachineDataTwe.DataItem.DataItem1();
            dtList53.MODEL_SIDE = "B镜面";
            dtList53.TEST_ITEM_NAME = "1004";
            dtList53.TEST_ITEM_VALUE = "";
            dtList53.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList53);
            MachineDataTwe.DataItem.DataItem1 dtList54 = new MachineDataTwe.DataItem.DataItem1();
            dtList54.MODEL_SIDE = "B镜面";
            dtList54.TEST_ITEM_NAME = "1005";
            dtList54.TEST_ITEM_VALUE = "";
            dtList54.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList54);
            MachineDataTwe.DataItem.DataItem1 dtList55 = new MachineDataTwe.DataItem.DataItem1();
            dtList55.MODEL_SIDE = "B镜面";
            dtList55.TEST_ITEM_NAME = "3091";
            dtList55.TEST_ITEM_VALUE = "";
            dtList55.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList55);
            MachineDataTwe.DataItem.DataItem1 dtList56 = new MachineDataTwe.DataItem.DataItem1();
            dtList56.MODEL_SIDE = "B镜面";
            dtList56.TEST_ITEM_NAME = "1000";
            dtList56.TEST_ITEM_VALUE = "";
            dtList56.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList56);

            MachineDataTwe.DataItem.DataItem1 dtList57 = new MachineDataTwe.DataItem.DataItem1();
            dtList57.MODEL_SIDE = "D镜面";
            dtList57.TEST_ITEM_NAME = "1011";
            dtList57.TEST_ITEM_VALUE = "";
            dtList57.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList57);
            MachineDataTwe.DataItem.DataItem1 dtList58 = new MachineDataTwe.DataItem.DataItem1();
            dtList58.MODEL_SIDE = "D镜面";
            dtList58.TEST_ITEM_NAME = "1004";
            dtList58.TEST_ITEM_VALUE = "";
            dtList58.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList58);
            MachineDataTwe.DataItem.DataItem1 dtList59 = new MachineDataTwe.DataItem.DataItem1();
            dtList59.MODEL_SIDE = "D镜面";
            dtList59.TEST_ITEM_NAME = "1005";
            dtList59.TEST_ITEM_VALUE = "";
            dtList59.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList59);
            MachineDataTwe.DataItem.DataItem1 dtList60 = new MachineDataTwe.DataItem.DataItem1();
            dtList60.MODEL_SIDE = "D镜面";
            dtList60.TEST_ITEM_NAME = "3091";
            dtList60.TEST_ITEM_VALUE = "";
            dtList60.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList60);
            MachineDataTwe.DataItem.DataItem1 dtList61 = new MachineDataTwe.DataItem.DataItem1();
            dtList61.MODEL_SIDE = "D镜面";
            dtList61.TEST_ITEM_NAME = "1000";
            dtList61.TEST_ITEM_VALUE = "";
            dtList61.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList61);

            MachineDataTwe.DataItem.DataItem1 dtList62 = new MachineDataTwe.DataItem.DataItem1();
            dtList62.MODEL_SIDE = "BL1";
            dtList62.TEST_ITEM_NAME = "1022";
            dtList62.TEST_ITEM_VALUE = "";
            dtList62.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList62);
            MachineDataTwe.DataItem.DataItem1 dtList63 = new MachineDataTwe.DataItem.DataItem1();
            dtList63.MODEL_SIDE = "BL1";
            dtList63.TEST_ITEM_NAME = "1022";
            dtList63.TEST_ITEM_VALUE = "";
            dtList63.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList63);
            MachineDataTwe.DataItem.DataItem1 dtList64 = new MachineDataTwe.DataItem.DataItem1();
            dtList64.MODEL_SIDE = "BL2";
            dtList64.TEST_ITEM_NAME = "1022";
            dtList64.TEST_ITEM_VALUE = "";
            dtList64.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList64);
            MachineDataTwe.DataItem.DataItem1 dtList65 = new MachineDataTwe.DataItem.DataItem1();
            dtList65.MODEL_SIDE = "BL2";
            dtList65.TEST_ITEM_NAME = "1022";
            dtList65.TEST_ITEM_VALUE = "";
            dtList65.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList65);

            MachineDataTwe.DataItem.DataItem1 dtList66 = new MachineDataTwe.DataItem.DataItem1();
            dtList66.MODEL_SIDE = "其他不良";
            dtList66.TEST_ITEM_NAME = "1513";
            dtList66.TEST_ITEM_VALUE = "";
            dtList66.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList66);
            MachineDataTwe.DataItem.DataItem1 dtList67 = new MachineDataTwe.DataItem.DataItem1();
            dtList67.MODEL_SIDE = "其他不良";
            dtList67.TEST_ITEM_NAME = "1501";
            dtList67.TEST_ITEM_VALUE = "";
            dtList67.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList67);
            MachineDataTwe.DataItem.DataItem1 dtList68 = new MachineDataTwe.DataItem.DataItem1();
            dtList68.MODEL_SIDE = "其他不良";
            dtList68.TEST_ITEM_NAME = "4232";
            dtList68.TEST_ITEM_VALUE = "";
            dtList68.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList68);
            MachineDataTwe.DataItem.DataItem1 dtList69 = new MachineDataTwe.DataItem.DataItem1();
            dtList69.MODEL_SIDE = "其他不良";
            dtList69.TEST_ITEM_NAME = "4230";
            dtList69.TEST_ITEM_VALUE = "";
            dtList69.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList69);
            MachineDataTwe.DataItem.DataItem1 dtList70 = new MachineDataTwe.DataItem.DataItem1();
            dtList70.MODEL_SIDE = "其他不良";
            dtList70.TEST_ITEM_NAME = "2000";
            dtList70.TEST_ITEM_VALUE = "";
            dtList70.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList70);
            MachineDataTwe.DataItem.DataItem1 dtList71 = new MachineDataTwe.DataItem.DataItem1();
            dtList71.MODEL_SIDE = "其他不良";
            dtList71.TEST_ITEM_NAME = "6013";
            dtList71.TEST_ITEM_VALUE = "";
            dtList71.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList71);
            MachineDataTwe.DataItem.DataItem1 dtList72 = new MachineDataTwe.DataItem.DataItem1();
            dtList72.MODEL_SIDE = "其他不良";
            dtList72.TEST_ITEM_NAME = "1022";
            dtList72.TEST_ITEM_VALUE = "";
            dtList72.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList72);
            MachineDataTwe.DataItem.DataItem1 dtList73 = new MachineDataTwe.DataItem.DataItem1();
            dtList73.MODEL_SIDE = "其他不良";
            dtList73.TEST_ITEM_NAME = "6999";
            dtList73.TEST_ITEM_VALUE = "";
            dtList73.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList73);

            MachineDataTwe.DataItem.DataItem1 dtList74 = new MachineDataTwe.DataItem.DataItem1();
            dtList74.MODEL_SIDE = "尺寸缺陷";
            dtList74.TEST_ITEM_NAME = "FAI2";
            dtList74.TEST_ITEM_VALUE = "";
            dtList74.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList74);
            MachineDataTwe.DataItem.DataItem1 dtList75 = new MachineDataTwe.DataItem.DataItem1();
            dtList75.MODEL_SIDE = "尺寸缺陷";
            dtList75.TEST_ITEM_NAME = "FAI4";
            dtList75.TEST_ITEM_VALUE = "";
            dtList75.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList75);
            MachineDataTwe.DataItem.DataItem1 dtList76 = new MachineDataTwe.DataItem.DataItem1();
            dtList76.MODEL_SIDE = "尺寸缺陷";
            dtList76.TEST_ITEM_NAME = "FAI6";
            dtList76.TEST_ITEM_VALUE = "";
            dtList76.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList76);
            MachineDataTwe.DataItem.DataItem1 dtList77 = new MachineDataTwe.DataItem.DataItem1();
            dtList77.MODEL_SIDE = "尺寸缺陷";
            dtList77.TEST_ITEM_NAME = "FAI10";
            dtList77.TEST_ITEM_VALUE = "";
            dtList77.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList77);
            MachineDataTwe.DataItem.DataItem1 dtList78 = new MachineDataTwe.DataItem.DataItem1();
            dtList78.MODEL_SIDE = "尺寸缺陷";
            dtList78.TEST_ITEM_NAME = "FAI12";
            dtList78.TEST_ITEM_VALUE = "";
            dtList78.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList78);
            MachineDataTwe.DataItem.DataItem1 dtList79 = new MachineDataTwe.DataItem.DataItem1();
            dtList79.MODEL_SIDE = "尺寸缺陷";
            dtList79.TEST_ITEM_NAME = "FAI13";
            dtList79.TEST_ITEM_VALUE = "";
            dtList79.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList79);
            MachineDataTwe.DataItem.DataItem1 dtList80 = new MachineDataTwe.DataItem.DataItem1();
            dtList80.MODEL_SIDE = "尺寸缺陷";
            dtList80.TEST_ITEM_NAME = "FAI14";
            dtList80.TEST_ITEM_VALUE = "";
            dtList80.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList80);
            MachineDataTwe.DataItem.DataItem1 dtList81 = new MachineDataTwe.DataItem.DataItem1();
            dtList81.MODEL_SIDE = "尺寸缺陷";
            dtList81.TEST_ITEM_NAME = "FAI18";
            dtList81.TEST_ITEM_VALUE = "";
            dtList81.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList81);
            MachineDataTwe.DataItem.DataItem1 dtList82 = new MachineDataTwe.DataItem.DataItem1();
            dtList82.MODEL_SIDE = "尺寸缺陷";
            dtList82.TEST_ITEM_NAME = "FAI32";
            dtList82.TEST_ITEM_VALUE = "";
            dtList82.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList82);
            MachineDataTwe.DataItem.DataItem1 dtList83 = new MachineDataTwe.DataItem.DataItem1();
            dtList83.MODEL_SIDE = "尺寸缺陷";
            dtList83.TEST_ITEM_NAME = "FAI36-1";
            dtList83.TEST_ITEM_VALUE = "";
            dtList83.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList83);
            MachineDataTwe.DataItem.DataItem1 dtList84 = new MachineDataTwe.DataItem.DataItem1();
            dtList84.MODEL_SIDE = "尺寸缺陷";
            dtList84.TEST_ITEM_NAME = "FAI36-2";
            dtList84.TEST_ITEM_VALUE = "";
            dtList84.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList84);
            MachineDataTwe.DataItem.DataItem1 dtList85 = new MachineDataTwe.DataItem.DataItem1();
            dtList85.MODEL_SIDE = "尺寸缺陷";
            dtList85.TEST_ITEM_NAME = "FAI26";
            dtList85.TEST_ITEM_VALUE = "";
            dtList85.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList85);
            MachineDataTwe.DataItem.DataItem1 dtList86 = new MachineDataTwe.DataItem.DataItem1();
            dtList86.MODEL_SIDE = "尺寸缺陷";
            dtList86.TEST_ITEM_NAME = "FAI27";
            dtList86.TEST_ITEM_VALUE = "";
            dtList86.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList86);
            MachineDataTwe.DataItem.DataItem1 dtList87 = new MachineDataTwe.DataItem.DataItem1();
            dtList87.MODEL_SIDE = "尺寸缺陷";
            dtList87.TEST_ITEM_NAME = "FAI28";
            dtList87.TEST_ITEM_VALUE = "";
            dtList87.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList87);
            MachineDataTwe.DataItem.DataItem1 dtList88 = new MachineDataTwe.DataItem.DataItem1();
            dtList88.MODEL_SIDE = "尺寸缺陷";
            dtList88.TEST_ITEM_NAME = "FAI33";
            dtList88.TEST_ITEM_VALUE = "";
            dtList88.TEST_ITEM_RESULT = "";
            ItemList.Add(dtList88);


            dt.ItemList = ItemList;


            //dt.reportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.Add(dt);

            MS.data = DESEncrypt(JsonConvert.SerializeObject(data), "sjgd2023gyhlwiot");

            message = JsonConvert.SerializeObject(MS);
            WaitReceiver WReceiver = new WaitReceiver();
            WReceiver.Msg_id = MS.msg_id;
            WReceiver.Lot_ID = DateTime.Now.ToString("yyyyMMddHH");
            //if (Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHH").Substring(8, 2)) % 2 == 0)
            //{
            //    WReceiver.Lot_ID = DateTime.Now.ToString("yyyyMMddHH");
            //}
            //else
            //{
            //    WReceiver.Lot_ID = (Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHH")) - 1).ToString();
            //}
            WReceiver.ProductCode = DateTime.Now.ToString("yyyyMMddHHmmssff");
            WReceiver.fileNamelog = WReceiver.ProductCode + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            WReceiver.Send_msg = message;

            if (ConStatue)
            {
                msg_idlist.TryAdd(MS.msg_id, WReceiver);
                SaveMQTTBufferlog(WReceiver);
            }
            else
            {
                SaveMQTTStemplog(WReceiver);
            }

            if (string.IsNullOrEmpty(topic))
            {
                MessageBox.Show("发布主题不能为空！");
                return;
            }
            //MQTT.GetMqtt();
            Mqtt_Send(topic, message);
        }
        /// <summary>
        /// Post发送数据
        /// </summary>      
        public static void ChangHttpPost(string Data, out string message)
        {

            DateTime time = DateTime.Now;
            long Time = ConvertDateTimeToInt(time);

            List<MachineDataPost.DataItem> data = new List<MachineDataPost.DataItem>();
            MachineDataPost.DataItem dt = new MachineDataPost.DataItem();
            dt.workType = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dt.actionType = "L1-965-";
            dt.machineNo = "965";
            dt.Operator = "";
            dt.runcardId = "";
            dt.vehicleNo = "";
            dt.location = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dt.pieceId = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.Add(dt);

            //MS.data = DESEncrypt(JsonConvert.SerializeObject(data), "sjgd2023gyhlwiot");
            message = JsonConvert.SerializeObject(data);
            string log = "";
            HttpPost(message, out log);

        }
        /// <summary>
        /// 4.3 小件单片信息获取Get 小件单片信息获取
        /// </summary>
        /// <param name="strSN"></param>
        /// <param name="strReturnMsg"></param>
        /// <returns></returns>
        public static bool HttpGGetAoiBadPiece(out string strReturnMsg)
        {
            try
            {
                string Http = SystemMgr.GetInstance().GetParamString("MesUrl");
                string PROCESS_MES_URL = $"http://{Http}/api/Mes/GetBostonAoiBadPiece";
                string param = "?runcardId=U39C-2301280002-014" + "&opld=1010";
                HttpWebRequest get_request = (HttpWebRequest)WebRequest.Create(PROCESS_MES_URL + param);
                get_request.ProtocolVersion = HttpVersion.Version11;
                get_request.Timeout = 60000;
                get_request.Method = "GET";
                get_request.Accept = "application/json";
                WebResponse wr = get_request.GetResponse();
                StreamReader reader = new StreamReader(wr.GetResponseStream());
                string result = reader.ReadToEnd();
                process_control_responce res = JsonConvert.DeserializeObject<process_control_responce>(result);
                strReturnMsg = result;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {

                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string text = reader.ReadToEnd();
                            error_responceget errJSON =
                            JsonConvert.DeserializeObject<error_responceget>(text);  //錯誤訊息
                            strReturnMsg = "呼叫API上传资料失败，错误原因︰「" + errJSON.error + "」，请联系「" + errJSON.contact + "」排除异常!";
                            //return false;
                        }
                    }

                }
                else
                {
                    strReturnMsg = ex.Message;

                }

                return false;
            }
            return true;
        }

        /// <summary>
        /// 4.13 单片获取载具、卡号接口
        /// </summary>
        /// <param name="SNcode">中片二维码</param>
        /// <param name="strReturnMsg"></param>
        /// <returns></returns>
        public static bool HttpGGetbyPiece(string SNcode, out RootObject5 strReturnMsg)
        {
            try
            {
                string Http = SystemMgr.GetInstance().GetParamString("MesUrl");
                string PROCESS_MES_URL = $"http://{Http}/api/Mes/wip/byPiece";
                string param = "?pieceId=" + SNcode +
                    "&martPartNo = " + DataMgr.GetInstance().m_dictDataGroup["料号"].m_dictData["成品料号"].m_dbStandardValue.ToString() +
                    "&opld=" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString() +
                    "&machineNo=" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();


                HttpWebRequest get_request = (HttpWebRequest)WebRequest.Create(PROCESS_MES_URL + param);
                get_request.ProtocolVersion = HttpVersion.Version11;
                get_request.Timeout = 60000;
                get_request.Method = "GET";
                get_request.Accept = "application/json";
                WebResponse wr = get_request.GetResponse();
                StreamReader reader = new StreamReader(wr.GetResponseStream());
                string result = reader.ReadToEnd();
                http_logger.Trace(PROCESS_MES_URL + param + ":" + result);
                RootObject5 res = JsonConvert.DeserializeObject<RootObject5>(result);
                strReturnMsg = res;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {

                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string text = reader.ReadToEnd();
                            error_responceget errJSON =
                            JsonConvert.DeserializeObject<error_responceget>(text);  //錯誤訊息
                            strReturnMsg = new RootObject5();
                            strReturnMsg.bizErrMsg = "呼叫API上传资料失败，错误原因︰「" + errJSON.error + "」，请联系「" + errJSON.contact + "」排除异常!";

                            return false;
                        }
                    }

                }
                else
                {
                    strReturnMsg = new RootObject5();

                    strReturnMsg.bizErrMsg = ex.Message;

                }

                return false;
            }
            return true;
        }





        /// <summary>
        /// 4.16 载具获取fileid
        /// </summary>
        /// <param name="strSN"></param>
        /// <param name="strReturnMsg"></param>
        /// <param name="martPartNo"></param>成品料号
        /// <param name="pieceTraySN"></param>载具号
        /// <param name="machineNo"></param>机台号
        /// <returns></returns>
        public static bool HttpGetAoiFileId(string pieceTraySN, out RootObject3 ressult)
        {
            try
            {
                string Http = SystemMgr.GetInstance().GetParamString("MesUrl");
                string PROCESS_MES_URL = $"http://{Http}/api/mes/wip/getFileIdByVehicleNo";
                string param;

                param = "?martPartNo=" + DataMgr.GetInstance().m_dictDataGroup["料号"].m_dictData["成品料号"].m_dbStandardValue.ToString() + "&vehicleNo=" + pieceTraySN + "&opId=290" + "&machineNo=" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();

                HttpWebRequest get_request = (HttpWebRequest)WebRequest.Create(PROCESS_MES_URL + param);
                get_request.ProtocolVersion = HttpVersion.Version11;
                get_request.Timeout = 60000;
                get_request.Method = "GET";
                get_request.Accept = "application/json";
                WebResponse wr = get_request.GetResponse();
                StreamReader reader = new StreamReader(wr.GetResponseStream());
                string result = reader.ReadToEnd();
                SaveHiveLog(pieceTraySN, PROCESS_MES_URL + param + result);
                RootObject3 res = JsonConvert.DeserializeObject<RootObject3>(result);
                ressult = res;
            }
            catch (WebException ex)
            {
                RootObject3 res = new RootObject3();
                if (ex.Response != null)
                {

                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string text = reader.ReadToEnd();
                            error_responceget errJSON =
                            JsonConvert.DeserializeObject<error_responceget>(text);  //錯誤訊息
                            res.bizErrMsg = text;
                            ressult = res;
                            return false;
                        }
                    }

                }
                else
                {
                    res.bizErrMsg = ex.Message;

                }
                ressult = res;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 保存信息到本地文档
        /// </summary>
        /// <param name="Message">需要保存的信息</param>
        /// <returns></returns>
        public static bool SaveHiveLog(string SN, string Message)
        {
            http_logger.Warn(SN + ":" + Message);
            return true;

        }

        /// <summary>
        /// 4.21 按单片获取单片信息
        /// </summary>
        /// <param name="strSN"></param>
        /// <param name="strReturnMsg"></param>
        /// <returns></returns>
        public static bool HttpGetPieceId(string pieceIdSN, out RootObject7 strReturnMsg)
        {
            try
            {
                string Http = SystemMgr.GetInstance().GetParamString("MesUrl");
                string PROCESS_MES_URL = $"http://{Http}/api/Mes/wip/byDefectPiece";
                List<string> strReturnMsg1 = new List<string>();
                string param;

                param = "?pieceId=" + pieceIdSN + "&machineNo=" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                HttpWebRequest get_request = (HttpWebRequest)WebRequest.Create(PROCESS_MES_URL + param);
                get_request.ProtocolVersion = HttpVersion.Version11;
                get_request.Timeout = 60000;
                get_request.Method = "GET";
                get_request.Accept = "application/json";
                WebResponse wr = get_request.GetResponse();
                StreamReader reader = new StreamReader(wr.GetResponseStream());
                string result = reader.ReadToEnd();
                SaveHiveLog(pieceIdSN, PROCESS_MES_URL + param + result);

                //RootObject2 deserializedObject = JsonConvert.DeserializeObject<RootObject2>(result);
                RootObject7 deserializedObject = JsonConvert.DeserializeObject<RootObject7>(result);

                // BizData_LionFish bizlf = JsonConvert.DeserializeObject<BizData_LionFish>(deserializedObject.bizData);
                //BizData_LionFish biz = JsonConvert.DeserializeObject<BizData_LionFish>(deserializedObject.bizData);
                //strReturnMsg = biz;
                strReturnMsg = deserializedObject;

            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {

                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string text = reader.ReadToEnd();
                            error_responceget errJSON =
                            JsonConvert.DeserializeObject<error_responceget>(text);  //錯誤訊息
                            //strReturnMsg = "呼叫API上传资料失败，错误原因︰「" + errJSON.error + "」，请联系「" + errJSON.contact + "」排除异常!";
                            //return false;
                        }
                    }

                }
                else
                {
                    //strReturnMsg = ex.Message;

                }
                strReturnMsg = new RootObject7();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 4.7 后端流程卡
        /// </summary>
        /// <param name="runCardNo">后站流程卡，手动扫码获取</param>
        /// <param name="martPartNo"></param>
        /// <param name="strReturnMsg"></param>
        /// <returns></returns>
        public static bool HttpGetSteelSNId(string runCardNo, string martPartNo, out RootObject2 strReturnMsg)
        {
            try
            {
                string Http = SystemMgr.GetInstance().GetParamString("MesUrl");
                string PROCESS_MES_URL = $"http://{Http}/api/Mes/wip/cardInfo";
                List<string> strReturnMsg1 = new List<string>();
                string param;

                param = "?runCardNo=" + runCardNo + "&opId=" + "290" + "&machineNo=" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString() + "&martPartNo=" + martPartNo;
                HttpWebRequest get_request = (HttpWebRequest)WebRequest.Create(PROCESS_MES_URL + param);
                get_request.ProtocolVersion = HttpVersion.Version11;
                get_request.Timeout = 60000;
                get_request.Method = "GET";
                get_request.Accept = "application/json";
                WebResponse wr = get_request.GetResponse();
                StreamReader reader = new StreamReader(wr.GetResponseStream());
                string result = reader.ReadToEnd();
                SaveHiveLog(runCardNo, PROCESS_MES_URL + param + result);
                RootObject2 deserializedObject = JsonConvert.DeserializeObject<RootObject2>(result);

                strReturnMsg = deserializedObject;

            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {

                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string text = reader.ReadToEnd();
                            error_responceget errJSON =
                            JsonConvert.DeserializeObject<error_responceget>(text);  //錯誤訊息
                            //strReturnMsg = "呼叫API上传资料失败，错误原因︰「" + errJSON.error + "」，请联系「" + errJSON.contact + "」排除异常!";
                            //return false;
                        }
                    }

                }
                else
                {
                    //strReturnMsg = ex.Message;

                }
                strReturnMsg = new RootObject2();
                return false;
            }
            return true;
        }








        /// <summary>
        /// Get 机台流程卡号校验
        /// </summary>
        /// <param name="strSN"></param>
        /// <param name="strReturnMsg"></param>
        /// <returns></returns>
        public static bool HttpGetRunCardNo(string SN, out RootObject2 strReturnMsg)
        {
            try
            {
                string Http = SystemMgr.GetInstance().GetParamString("MesHttpUrl");

                string PROCESS_MES_URL = $"http://{Http}/api/mes/wip/cardInfo";
                string param = "?runCardNo=" + SN + "&opId=1010" + "&machineNo=" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
                HttpWebRequest get_request = (HttpWebRequest)WebRequest.Create(PROCESS_MES_URL + param);
                get_request.ProtocolVersion = HttpVersion.Version11;
                get_request.Timeout = 60000;
                get_request.Method = "GET";
                get_request.Accept = "application/json";
                WebResponse wr = get_request.GetResponse();
                StreamReader reader = new StreamReader(wr.GetResponseStream());
                string result = reader.ReadToEnd();
                SaveHiveLog(SN, PROCESS_MES_URL + param + result);
                //process_control_responce res = JsonConvert.DeserializeObject<process_control_responce>(result);
                RootObject2 data = JsonConvert.DeserializeObject<RootObject2>(result);
                strReturnMsg = data;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {

                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string text = reader.ReadToEnd();
                            error_responceget errJSON =
                            JsonConvert.DeserializeObject<error_responceget>(text);  //錯誤訊息
                            //strReturnMsg = "呼叫API上传资料失败，错误原因︰「" + errJSON.error + "」，请联系「" + errJSON.contact + "」排除异常!";
                            //return false;
                        }
                    }

                }
                else
                {
                    //strReturnMsg = ex.Message;

                }
                strReturnMsg = new RootObject2();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 4.22获取设备软件版本校验
        /// </summary>
        /// <param name="strReturnMsg"></param>
        /// <returns></returns>
        public static bool HttpGetEQSoftVersion(out RootObject4 strReturnMsg)
        {
            try
            {
                string Http = "172.16.10.161:83";

                string PROCESS_MES_URL = $"http://{Http}/api/mes/wip/getEQSoftVersion";
                string param = "?opId=290" + "&machineNo=" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString() + "&version=" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["软件版本号"].m_dbStandardValue.ToString() + "&deviceNo="
                    + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString() + "&lineId=" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["线别"].m_dbStandardValue.ToString();
                HttpWebRequest get_request = (HttpWebRequest)WebRequest.Create(PROCESS_MES_URL + param);
                get_request.ProtocolVersion = HttpVersion.Version11;
                get_request.Timeout = 6000;
                get_request.Method = "GET";
                get_request.Accept = "application/json";
                WebResponse wr = get_request.GetResponse();
                StreamReader reader = new StreamReader(wr.GetResponseStream());
                string result = reader.ReadToEnd();
                SaveHiveLog(DateTime.Now.ToString("HHmmss"), PROCESS_MES_URL + param + result);
                //process_control_responce res = JsonConvert.DeserializeObject<process_control_responce>(result);
                RootObject4 data = JsonConvert.DeserializeObject<RootObject4>(result);
                strReturnMsg = data;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {

                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string text = reader.ReadToEnd();
                            error_responceget errJSON =
                            JsonConvert.DeserializeObject<error_responceget>(text);  //錯誤訊息
                            JsonSerializer serializer = new JsonSerializer();
                            StringWriter textWriter = new StringWriter();
                            JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                            {
                                Formatting = Formatting.Indented,
                                Indentation = 4,
                                IndentChar = ' '
                            };
                            serializer.Serialize(jsonWriter, errJSON);
                            MessageBox.Show($"MES软件版本校验反馈：\r\n{textWriter}\r\n点击确认重试");

                            //strReturnMsg = "呼叫API上传资料失败，错误原因︰「" + errJSON.error + "」，请联系「" + errJSON.contact + "」排除异常!";
                            //return false;
                        }
                    }

                }
                strReturnMsg = new RootObject4();
                return false;
            }

            return true;
        }
        /// <summary>
        /// 4.26设备参数管控接口(加工前) 
        /// </summary>
        /// <returns></returns>
        public static bool HttpGetMachineParamCheckBefore(List<string> parames, out RootObject6 strReturnMsg)
        {
            try
            {
                string Http = "172.16.10.161:83";

                string PROCESS_MES_URL = $"http://{Http}/api/mes/wip/machineParamCheckBefore";
                string param = "?machineNo=" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString()
                    + "&vehicleNo=" + ""
                    + "&opid=" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["OPID"].m_dbStandardValue.ToString()
                    + "&BeforeGapX1=" + parames[0] + "&BeforeGapY1=" + parames[1]
                    + "&BeforeGapX2=" + parames[2] + "&BeforeGapY2=" + parames[3]
                    + "&BeforeGapX3=" + parames[4] + "&BeforeGapY3=" + parames[5]
                    + "&BeforeGapX4=" + parames[6] + "&BeforeGapY4=" + parames[7]
                    + "&BeHandGapX1=" + parames[8] + "&BeHandGapY1=" + parames[9]
                    + "&BeHandGapX2=" + parames[10] + "&BeHandGapY2=" + parames[11]
                    + "&BeHandGapX3=" + parames[12] + "&BeHandGapY3=" + parames[13]
                    + "&BeHandGapX4=" + parames[14] + "&BeHandGapY4=" + parames[15]
                    + "&martPartNo=" + "1510312001616";
                HttpWebRequest get_request = (HttpWebRequest)WebRequest.Create(PROCESS_MES_URL + param);
                get_request.ProtocolVersion = HttpVersion.Version11;
                get_request.Timeout = 6000;
                get_request.Method = "GET";
                get_request.Accept = "application/json";
                WebResponse wr = get_request.GetResponse();
                StreamReader reader = new StreamReader(wr.GetResponseStream());
                string result = reader.ReadToEnd();
                SaveHiveLog(DateTime.Now.ToString("HHmmss"), PROCESS_MES_URL + param + result);
                //process_control_responce res = JsonConvert.DeserializeObject<process_control_responce>(result);
                RootObject6 data = JsonConvert.DeserializeObject<RootObject6>(result);
                strReturnMsg = data;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {

                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string text = reader.ReadToEnd();
                            error_responceget errJSON =
                            JsonConvert.DeserializeObject<error_responceget>(text);  //錯誤訊息
                            JsonSerializer serializer = new JsonSerializer();
                            StringWriter textWriter = new StringWriter();
                            JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                            {
                                Formatting = Formatting.Indented,
                                Indentation = 4,
                                IndentChar = ' '
                            };
                            serializer.Serialize(jsonWriter, errJSON);
                            MessageBox.Show($"MES设备参数管控反馈：\r\n{textWriter}\r\n点击确认重试");

                            //strReturnMsg = "呼叫API上传资料失败，错误原因︰「" + errJSON.error + "」，请联系「" + errJSON.contact + "」排除异常!";
                            //return false;
                        }
                    }

                }
                strReturnMsg = new RootObject6();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 参数校验接口(用于平均值上抛)
        /// </summary>
        /// <param name="strReturnMsg"></param>
        /// <returns></returns>
        public static bool HttpGetParamAvgCheck(string[] list, out RootObject4 strReturnMsg)
        {
            try
            {
                string Http = SystemMgr.GetInstance().GetParamString("MesHttpUrl");

                string PROCESS_MES_URL = $"http://{Http}/api/mes/wip/coatParamAvgCheck";
                string param = "?opId=1010" + "&machineNo=" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString() + "&HTruanjianbanbenhao=" + list[0] + "&HTchicun2=" + list[1] + "&HTchicun3=" + list[2] + "&HTchicun6=" + list[3] + "&HTchicun7=" + list[4];
                HttpWebRequest get_request = (HttpWebRequest)WebRequest.Create(PROCESS_MES_URL + param);
                get_request.ProtocolVersion = HttpVersion.Version11;
                get_request.Timeout = 3000;
                get_request.Method = "GET";
                get_request.Accept = "application/json";
                WebResponse wr = get_request.GetResponse();
                StreamReader reader = new StreamReader(wr.GetResponseStream());
                string result = reader.ReadToEnd();
                SaveHiveLog(DateTime.Now.ToString("HHmmss"), PROCESS_MES_URL + param + result);
                //process_control_responce res = JsonConvert.DeserializeObject<process_control_responce>(result);
                RootObject4 data = JsonConvert.DeserializeObject<RootObject4>(result);
                strReturnMsg = data;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {

                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string text = reader.ReadToEnd();
                            error_responceget errJSON =
                            JsonConvert.DeserializeObject<error_responceget>(text);  //錯誤訊息
                            //strReturnMsg = "呼叫API上传资料失败，错误原因︰「" + errJSON.error + "」，请联系「" + errJSON.contact + "」排除异常!";
                            //return false;
                        }
                    }

                }
                strReturnMsg = new RootObject4();
                return false;
            }

            return true;
        }


        public class process_control_responce1
        {
            public string ErrCode { get; set; }
            public string ErrMSG { get; set; }
            public List<Piece> Data { get; set; }
        }
        public class Piece
        {
            public string DOMAIN_ID { get; set; }
            /// <summary>
            /// PIECE_ID
            /// </summary>
            public string PIECE_ID { get; set; }
            /// <summary>
            ///小件状态0正常1报废2异常3黑名单
            /// </summary>
            public string PIECE_STATUS { get; set; }
            /// <summary>
            /// 流程卡号
            /// </summary>
            public string RUN_CARD_NO { get; set; }
            public string MART_PART_NO { get; set; }
            /// <summary>
            /// 等级
            /// </summary>
            public string GRADE { get; set; }
            /// <summary>
            /// 单片位置
            /// </summary>
            public string LOCATION { get; set; }
            /// <summary>
            /// 缺陷代码
            /// </summary>
            public string DEFECT_CODE_ID { get; set; }
            /// <summary>
            /// 站点名称
            /// </summary>
            public string DEFECT_OP_ID { get; set; }
            public string LAST_OP_ID { get; set; }
            public string PARENT_PIECE_ID { get; set; }
        }

        public class RootObject
        {
            public string ErrCode { get; set; }
            public string ErrMSG { get; set; }
            public List<Piece> Data { get; set; }
        }


        public class RootObject2
        {
            public bool success { get; set; }
            public string message { get; set; }
            public int bizErrCode { get; set; }
            public string bizErrMsg { get; set; }
            public BizData bizData { get; set; }
        }

        public class RootObject3
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string bizErrCode { get; set; }
            public string bizErrMsg { get; set; }
            public BizData1 bizData { get; set; }
        }
        public class RootObject4
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string bizErrCode { get; set; }
            public string bizErrMsg { get; set; }
            public BizData2 bizData { get; set; }
        }

        public class RootObject5
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string bizErrCode { get; set; }
            public string bizErrMsg { get; set; }
            public BizData3 bizData { get; set; }
        }
        public class RootObject6
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string bizErrCode { get; set; }
            public string bizErrMsg { get; set; }
            //根据实际需求写入BizData数据
            public BizData bizData { get; set; }
        }


        public class RootObject7
        {
            public bool success { get; set; }
            public string message { get; set; }
            public int bizErrCode { get; set; }
            public string bizErrMsg { get; set; }
            public BizDataff bizData { get; set; }
        }
        public class BizData
        {
            public string runCardNo { get; set; }
            public string woId { get; set; }
            public string domainId { get; set; }
            public string martPartNo { get; set; }
            public string routeId { get; set; }
            public string currentOpId { get; set; }
            public int cardStatus { get; set; }
            public int runCartQty { get; set; }
            public object pieceList { get; set; }
        }
        public class BizData1
        {
            public string fileId { get; set; }
            public string vehicleNo { get; set; }
            public string expire { get; set; }
            public string alertMsg { get; set; }
            public string maxUsable { get; set; }
            public string surplusUsable { get; set; }
            public string usableNow { get; set; }
            public string maxMaintainInterval { get; set; }
            public string surplusMaintainUsable { get; set; }
            public string maxCleanUsable { get; set; }
            public string surplusCleanUsable { get; set; }
        }
        public class BizData2
        {
            public string opId { get; set; }
            public string machineNo { get; set; }
            public string version { get; set; }
            public string deviceNo { get; set; }
            public string lineId { get; set; }
            public string createUser { get; set; }
            public string createTime { get; set; }
            public string lmTime { get; set; }
        }

        public class BizData_LionFish
        {
            public string runCardNo { get; set; }
            public string vehicleNo { get; set; }

            public string pieceId { get; set; }
            public string location { get; set; }
            public string lastOpId { get; set; }
            public string nextOpId { get; set; }
            public int pieceStatus { get; set; }
            public string operatorUser { get; set; }
            public string machineNo { get; set; }

            public string lmTime { get; set; }

        }

        public class BizDataff
        {
            public string runCardNo { get; set; }
            public string vehicleNo { get; set; }
            public string pieceId { get; set; }
            public string location { get; set; }
            public string lastOpId { get; set; }
            public string nextOpId { get;set; }
            public int pieceStatus { get; set; }
            public string operatorUser { get; set; }
            public string machineNo { get; set; }
            public string lmTime { get; set; }

        }


        public class BizData3
        {
            public string runCardNo { get; set; }
            public string domainId { get; set; }
            public string woId { get; set; }
            public string routeId { get; set; }

            public int cardStatus { get; set; }
            public string currentOpId { get; set; }

            public int runCartQty { get; set; }
            public List<TEMP> pieceList { get; set; }



        }
        public class TEMP
        {
            public string runCardNo { get; set; }
            public string vehicleNo { get; set; }
            public string pieceId { get; set; }
            public string location { get; set; }
            public string lastOpId { get; set; }
            public string nextOpId { get; set; }
            public string pieceStatus { get; set; }
            public string operatorUser { get; set; }
            public string machineNo { get; set; }
            public string lmTime { get; set; }

        }
        public class process_control_responce
        {
            public bool pass { get; set; }
            public List<string> choice_ids { get; set; }
            public string control_id { get; set; }
            public List<Process> processes { get; set; }
        }
        public class error_responceget
        {
            public string error { get; set; }  //<log uuid>
            public string contact { get; set; }  //<log uuid>

        }
        public class error_responcepost
        {
            public int code { get; set; }  //<log uuid>
            public string data { get; set; }  //<log uuid>
            public string msg { get; set; }
        }
        /// <summary>
        /// Post数据
        /// </summary>
        /// <param name="strData"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public static string HttpPost(string strData, out string strMsg)//自动上传trace
        {
            //string username = "nJUFwmTDVeI=";
            //string password = "FZ4w7LYS8NYSjYxekz9rQw==";
            //string userpass = username + ":" + password;
            string url = @"http://60.191.133.236:5773/api/mes/wip/vehicleInfo";  //多系统用的
            HttpWebRequest post_request = (HttpWebRequest)WebRequest.Create(url);
            post_request.ProtocolVersion = HttpVersion.Version11;
            post_request.Timeout = 60000;
            post_request.Method = "POST";
            post_request.ContentType = "application/json";
            post_request.Accept = "application/json";
            //byte[] credentialsAuth = new UTF8Encoding().GetBytes(username + ":" + password);
            //post_request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(credentialsAuth);
            byte[] payload;
            payload = Encoding.UTF8.GetBytes(strData);
            try
            {
                Stream writer;
                writer = post_request.GetRequestStream();
                writer.Write(payload, 0, payload.Length);
                writer.Close();//關閉請求流

                using (WebResponse wr = post_request.GetResponse())             //接受返回值：
                {
                    string result = "";
                    StreamReader reader = new StreamReader(wr.GetResponseStream());
                    result = reader.ReadToEnd();
                    WarningMgr.GetInstance().Info("Receive OK Response from API, data=「" + result + "」");
                    strMsg = result;
                    return "ok";
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using (Stream data1 = response.GetResponseStream())
                        using (var reader = new StreamReader(data1))
                        {
                            string text = reader.ReadToEnd();
                            error_responcepost errJSON = JsonConvert.DeserializeObject<error_responcepost>(text);
                            WarningMgr.GetInstance().Info("Receive Error Response from API, Message = " + ex.Message + ", Type = " + httpResponse.StatusCode + ", Response = " + text);
                            strMsg = "呼叫API上传资料失败，错误原因︰「" + errJSON.code + "」，请联系「" + errJSON.msg + "」排除异常!";
                            return "ng";
                        }
                    }
                }
                else
                {
                    WarningMgr.GetInstance().Info("POST DATA to API ERROR, Message = " + ex.Message);
                    MessageBox.Show("ERROR", "呼叫API失败，错误原因︰「" + ex.Message + "」，请联系资讯检查URI及Proxy Agent!");
                    strMsg = "呼叫API失败，错误原因︰「" + ex.Message + "」，请联系资讯检查URI及Proxy Agent!";
                    return "error";
                }
            }
        }
        /// <summary>
        /// ECB加密
        /// </summary>
        /// <param name="Data">加密字符串</param>
        /// <param name="key">秘钥</param>
        public static string DESEncrypt(string Data, string key)
        {
            if (string.IsNullOrEmpty(Data)) return null;
            byte[] ToEncrytArray = Encoding.UTF8.GetBytes(Data);
            string s = "";
            RijndaelManaged rm = new RijndaelManaged
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            if (key.Length == 16)
            {
                rm.Key = Encoding.UTF8.GetBytes(key);
            }
            ICryptoTransform cTransform = rm.CreateEncryptor();
            byte[] resutArray = cTransform.TransformFinalBlock(ToEncrytArray, 0, ToEncrytArray.Length);
            return Convert.ToBase64String(resutArray, 0, resutArray.Length);
        }
        /// <summary>
        /// ECB解密密
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DESDecrypt(string Data, string key)
        {
            if (string.IsNullOrEmpty(Data)) return null;
            byte[] ToDecryptArray = Convert.FromBase64String(Data);
            string s = "";
            RijndaelManaged rm = new RijndaelManaged
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            if (key.Length == 16)
            {
                rm.Key = Encoding.UTF8.GetBytes(key);
            }
            ICryptoTransform cTransform = rm.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(ToDecryptArray, 0, ToDecryptArray.Length);
            return Encoding.UTF8.GetString(resultArray);
        }
        /// <summary>
        /// 设备参数上报
        /// </summary>
        public class MachineDataparam
        {
            public string msg_id { get; set; }
            public string deviceno { get; set; }
            public string dataType { get; set; }
            public string deviceType { get; set; }

            public string data { get; set; }

            public class DataItem
            {
                /// <summary>
                /// 
                /// </summary>
                public string o_createTime { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string o_machineNo { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string o_workStation { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string paramName { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string paramSn { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string paramValue { get; set; }


                /// <summary>
                /// 
                /// </summary>
                public string timeStamp { get; set; }

            }
        }
        /// <summary>
        /// 加工工艺
        /// </summary>
        public class MachineDataTechnology
        {
            public string msg_id { get; set; }
            public string deviceno { get; set; }
            public string dataType { get; set; }
            public string deviceType { get; set; }

            public string data { get; set; }
            public class DataItem
            {
                /// <summary>
                /// 记录时间
                /// </summary>
                public string o_createTime { get; set; }
                /// <summary>
                /// 机台号
                /// </summary>
                public string o_machineNo { get; set; }
                /// <summary>
                /// 工站编号
                /// </summary>
                public string o_workStation { get; set; }
                /// <summary>
                /// 参数名称
                /// </summary>
                public string paramName { get; set; }
                /// <summary>
                /// 参数序号
                /// </summary>
                public string paramSn { get; set; }
                /// <summary>
                /// 参数值
                /// </summary>
                public string paramValue { get; set; }
                /// <summary>
                /// 参数值  
                /// </summary>
                public string timeStamp { get; set; }
            }
        }
        /// <summary>
        /// 工具采集
        /// </summary>
        public class MachineDataEMapping
        {
            //public string unit_sn { get; set; }
            public string msg_id = "";
            public string deviceno = "";
            public string dataType = "";
            public string deviceType = "";
            public string data { get; set; }
            private string guid = "";

            public class DataItem
            {
                /// <summary>
                /// 工站编号
                /// </summary>
                public string o_workStation { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string machineNo { get; set; }
                /// <summary>
                /// 机台号 
                /// </summary>
                public string o_machineNo { get; set; }
                /// <summary>
                /// 载具二维码
                /// </summary>
                public string w_vehicleNo { get; set; }
                /// <summary>
                /// 流程卡号
                /// </summary>
                public string runCardId { get; set; }
                /// <summary>
                /// 单片二维码
                /// </summary>
                public string pieceId { get; set; }
                /// <summary>
                /// 单片位置
                /// </summary>
                public string w_location { get; set; }
                /// <summary>
                /// 等级
                /// </summary>
                public string level { get; set; }
                /// <summary>
                /// 生成日期 
                /// </summary>
                public string curtime { get; set; }

            }

        }
        /// <summary>
        /// 设备报工
        /// </summary>
        public class MachineDataworkorder
        {
            public string msg_id = "";
            public string deviceno = "";
            public string dataType = "";
            public string deviceType = "";
            public string data { get; set; }
            private string guid = "";

            public class DataItem
            {
                /// <summary>
                /// 工站编号
                /// </summary>
                public string o_workStation { get; set; }
                /// <summary>
                /// 生成日期
                /// </summary>
                public string w_createTime { get; set; }
                /// <summary>
                /// 流程卡号
                /// </summary>
                public string runCardId { get; set; }
                /// <summary>
                /// 载具二维码
                /// </summary>
                public string w_vehicleNo { get; set; }
                /// <summary>
                /// 单片二维码
                /// </summary>
                public string pieceId { get; set; }
                /// <summary>
                /// 单片位置
                /// </summary>
                public string w_location { get; set; }
                /// <summary>
                /// 良品数量
                /// </summary>
                public string goodProductNumber { get; set; }
                /// <summary>
                /// 物料结果
                /// </summary>
                public string defectCode { get; set; }
                /// <summary>
                /// 作业人员工号
                /// </summary>
                public string o_operator { get; set; }
                /// <summary>
                /// 设备编码
                /// </summary>
                public string o_machineNo { get; set; }
                /// <summary>
                /// 镀膜跟踪号
                /// </summary>
                public string coatingTrackingNumber { get; set; }
                /// <summary>
                /// p1流程卡
                /// </summary>
                public string p1RunCardId { get; set; }
                /// <summary>
                /// p3流程卡
                /// </summary>
                public string p3RunCardId { get; set; }
                /// <summary>
                /// 等级
                /// </summary>
                public string level { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string w_judge { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string remark1 { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<治具数组> remark2 { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string remark3 { get; set; }
                /// <summary>
                /// 父单片码
                /// </summary>
                public string parentPieceId { get; set; }
                /// <summary>
                /// FileID
                /// </summary>
                public string fileId { get; set; }
            }
        }
        /// <summary>
        /// Twe/AOI报工明细
        /// </summary>
        public class MachineDataTwe
        {
            public string msg_id = "";
            public string deviceno = "";
            public string dataType = "";
            //public string origntime = "";
            public string deviceType = "";
            //public List<DataItem> data { get; set; }
            public string data { get; set; }

            public class DataItem
            {
                /// <summary>
                ///  载具位置
                /// </summary>
                public string LOCATION { get; set; }
                /// <summary>
                /// 工站编号
                /// </summary>
                public string O_WORKSTATION { get; set; }
                /// <summary>
                /// 回收托盘号
                /// </summary>
                public string COLLECT_PALLET_SERIAL { get; set; }
                /// <summary>
                /// 机台号
                /// </summary>
                public string MACHINE_NO { get; set; }
                /// <summary>
                /// 操作员
                /// </summary>
                public string O_OPERATOR { get; set; }
                /// <summary>
                /// 结束时间
                /// </summary>
                public string TEST_END_TIME { get; set; }
                /// <summary>
                /// 流程卡号
                /// </summary>
                public string FLOW_CARD_ID { get; set; }
                /// <summary>
                /// 载具号
                /// </summary>
                public string VEHICLE_CODE { get; set; }
                /// <summary>
                /// 单片ID绑定的托盘号
                /// </summary>
                public string FILE_ID { get; set; }
                /// <summary>
                /// 小件Y位置
                /// </summary>
                public string POSIONT_COLUMN { get; set; }
                /// <summary>
                /// 小件X位置
                /// </summary>
                public string POSIONT_ROW { get; set; }
                /// <summary>
                /// 下料吸嘴编号
                /// </summary>
                public string DOWN_MOUTH_NO { get; set; }
                /// <summary>
                /// 开始时间
                /// </summary>
                public string TEST_START_TIME { get; set; }
                /// <summary>
                /// 小片二维码
                /// </summary>
                public string FLOW_QR_CODE { get; set; }
                /// <summary>
                /// 供给顺序
                /// </summary>
                public string FEED_SERIAL { get; set; }
                /// <summary>
                /// 回收托盘二维码
                /// </summary>
                public string COLLECT_BOX_CODE { get; set; }
                /// <summary>
                /// 托盘二维码
                /// </summary>
                public string FEED_PALLET_QR_CODE { get; set; }
                /// <summary>
                /// 回收顺序
                /// </summary>
                public string COLLECT_SERIAL { get; set; }
                /// <summary>
                /// 码的等级
                /// </summary>
                public string QR_GRADE { get; set; }
                /// <summary>
                /// porro编号
                /// </summary>
                public string PORRO_NO { get; set; }
                /// <summary>
                /// 扫码器标号
                /// </summary>
                public string SCAN_NO { get; set; }
                /// <summary>
                /// 未定义
                /// </summary>
                //public string boxNo { get; set; }
                public List<DataItem1> ItemList { get; set; }
                public class DataItem1
                {
                    /// <summary>
                    /// 测试面分类
                    /// </summary>
                    public string MODEL_SIDE { get; set; }
                    /// <summary>
                    /// 测试项目名称
                    /// </summary>
                    public string TEST_ITEM_NAME { get; set; }
                    /// <summary>
                    /// 测试项目值
                    /// </summary>
                    public string TEST_ITEM_VALUE { get; set; }
                    /// <summary>
                    /// 测试结果
                    /// </summary>
                    public string TEST_ITEM_RESULT { get; set; }
                }
                /// <summary>
                /// 错误代码
                /// </summary>
                public string TEST_ITEM_FAULTY { get; set; }
                /// <summary>
                /// 物料结果
                /// </summary>
                public string TEST_ITEM_RESULT { get; set; }
                /// <summary>
                /// 吸嘴编号
                /// </summary>
                public string MOUTH_NO { get; set; }
                /// <summary>
                /// 托盘号
                /// </summary>
                public string FEED_PALLET_SERIAL { get; set; }
                /// <summary>
                /// 发送时间
                /// </summary>
                //public string reportTime { get; set; }

            }
        }
        /// <summary>
        /// 非操作员模式报工专用
        /// </summary>
        public class MachineDataTwe1
        {
            public string msg_id { get; set; }
            public string deviceno { get; set; }
            public string dataType { get; set; }
            public string deviceType { get; set; }
            public string data { get; set; }
            //public List<DataItem1> data = new List<DataItem1>();
            public class DataItem1
            {
                /// <summary>
                ///  载具位置
                /// </summary>
                public string LOCATION { get; set; }
                /// <summary>
                /// 工站编号
                /// </summary>
                public string O_WORKSTATION { get; set; }
                /// <summary>
                /// 机台号
                /// </summary>
                public string MACHINE_NO { get; set; }
                /// <summary>
                /// 操作员
                /// </summary>
                public string O_OPERATOR { get; set; }
                /// <summary>
                /// 流程卡号
                /// </summary>
                public string RUN_CARD_ID { get; set; }
                /// <summary>
                /// 回收托盘二维码
                /// </summary>
                public string W_VEHICLE_NO { get; set; }
                public string CREATE_TIME { get; set; }
                public string OP_MODE { get; set; }
                public string PIECE_ID { get; set; }
                public string GOODPRODUCTNUMBER { get; set; }
                public string DEFECTCODE { get; set; }

            }
        }
        public class MachineDataTwe2
        {
            public string msg_id { get; set; }
            public string deviceno { get; set; }
            public string dataType { get; set; }
            public string deviceType { get; set; }
            //public string data { get; set; }
            public List<MachineDataTwe1.DataItem1> data = new List<MachineDataTwe1.DataItem1>();
        }



        /// <summary>
        /// 非操作员模式数据上报   黑生虎
        /// </summary>
        public class MachineDataNotOpMode
        {
            public string msg_id { get; set; }
            public string deviceno { get; set; }
            public string dataType { get; set; }
            public string deviceType { get; set; }
            public string data { get; set; }

            public class DataItem1
            {
                public string CREATE_TIME { get; set; }
                public string O_OPERATOR { get; set; }
                public string OP_MODE { get; set; }
                public string RUN_CARD_ID { get; set; }
                public string W_VEHICLE_NO { get; set; }
                public string PIECE_ID { get; set; }
                public string LOCATION { get; set; }
                public string GOODPRODUCTNUMBER { get; set; }
                public string DEFECTCODE { get; set; }
                public string MACHINE_NO { get; set; }
                public string O_WORKSTATION { get; set; }
            }
        }

        /// <summary>
        /// 机况状态数据
        /// </summary>
        public class MachineStateData
        {

            public string msg_id = "";
            public string deviceno = "";
            public string dataType = "";
            public string deviceType = "";
            public string data { get; set; }
            //public Dictionary<string, object> data = null;
            private string guid = "";
            public class DataItem
            {
                /// <summary>
                /// 工站编号
                /// </summary>
                public string o_workStation { get; set; }
                /// <summary>
                /// 机台号
                /// </summary>
                public string o_machineNo { get; set; }
                /// <summary>
                /// 状态码
                /// </summary>
                public string o_machineState { get; set; }
                /// <summary>
                /// 状态名称
                /// </summary>
                public string o_stateName { get; set; }
                /// <summary>
                /// 状态运行开始时间
                /// </summary>
                public string o_stateStartTime { get; set; }
                /// <summary>
                /// 错误代码
                /// </summary>
                public string o_alertCode { get; set; }
                /// <summary>
                /// 填写错误信息
                /// </summary>
                public string o_alertInfo { get; set; }

            }
            //public MachineStateData()
            //{
            //    guid = System.Guid.NewGuid().ToString();
            //    data = new Dictionary<string, object>();
            //}


            public enum MachineState
            {
                Running = 1,
                Idle = 2,
                Engineering = 3,
                PlannedDown = 4,
                UnplannedDown = 5,
            }

        }
        /// <summary>
        /// Post数据
        /// </summary>
        public class MachineDataPost
        {
            public class DataItem
            {
                /// <summary>
                /// 站点名称
                /// </summary>
                public string workType { get; set; }
                /// <summary>
                /// 工作面
                /// </summary>
                public string actionType { get; set; }
                /// <summary>
                /// 机台号
                /// </summary>
                public string machineNo { get; set; }
                /// <summary>
                ///操作员
                /// </summary>
                public string Operator { get; set; }
                /// <summary>
                /// 流程卡号
                /// </summary>
                public string runcardId { get; set; }
                /// <summary>
                /// 治具号
                /// </summary>
                public string vehicleNo { get; set; }
                /// <summary>
                /// 穴位
                /// </summary>
                public string location { get; set; }
                /// <summary>
                /// 单片码
                /// </summary>
                public string pieceId { get; set; }

            }
        }
        /// <summary>
        /// 保存状态文件
        /// </summary>
        /// <param name=""></param>
        public static void SaveStateCsv(List<MachineStateData.DataItem> datas)
        {
            int row = 0, col = 0;
            CsvOperation csv = new CsvOperation(@"D:\ProBsd", "RD" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
            if (!File.Exists(Path.Combine(@"D:\ProBsd", "RD" + DateTime.Now.ToString("yyyyMMdd") + ".csv")))
            {
                //File.SetAttributes(csv.FileName, FileAttributes.Normal);
                col = 0;
                csv[0, col++] = "工站编号";
                csv[0, col++] = "机台号";
                csv[0, col++] = "状态码";//1:正常运行 2：报警 3：待机 4：检修
                csv[0, col++] = "状态名称";
                csv[0, col++] = "开始时间";
                csv[0, col++] = "错误代码";
                csv[0, col++] = "关键错误信息";
                row = 1;
                col = 0;
                foreach (var data in datas)
                {
                    //if (row == datas.Count)
                    //{
                    //    break;
                    //}
                    csv[row, col++] = data.o_workStation; //工站编号
                    csv[row, col++] = data.o_machineNo;//机台号
                    csv[row, col++] = data.o_machineState; //状态码
                    csv[row, col++] = data.o_stateName;//状态名称                      
                    csv[row, col++] = data.o_stateStartTime;//开始时间
                    csv[row, col++] = data.o_alertCode;//错误代码
                    csv[row, col++] = data.o_alertInfo; //"关键错误信息
                    row++;
                    col = 0;
                }
                csv.Save();
                File.SetAttributes(csv.FileName, FileAttributes.ReadOnly);
            }
            else
            {
                foreach (var data in datas)
                {
                    //if (row == datas.Count-1)
                    //{
                    //    break;
                    //}
                    File.SetAttributes(csv.FileName, FileAttributes.Normal);
                    col = 0;
                    csv[row, col++] = data.o_workStation; //工站编号
                    csv[row, col++] = data.o_machineNo;//机台号
                    csv[row, col++] = data.o_machineState; //状态码
                    csv[row, col++] = data.o_stateName;//状态名称                      
                    csv[row, col++] = data.o_stateStartTime;//开始时间
                    csv[row, col++] = data.o_alertCode;//错误代码
                    csv[row, col++] = data.o_alertInfo; //"关键错误信息
                    row++;
                    col = 0;
                }
                csv.Save();
                File.SetAttributes(csv.FileName, FileAttributes.ReadOnly);
            }

        }
        /// <summary>
        /// 保存状态文件
        /// </summary>
        /// <param name=""></param>
        public static void SaveParameCsv(List<MachineDataparam.DataItem> datas)
        {

            int row = 0, col = 0;
            int i = 0;
            CsvOperation csv = new CsvOperation(@"D:\ProBsd", "PA" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
            if (!File.Exists(Path.Combine(@"D:\ProBsd", "PA" + DateTime.Now.ToString("yyyyMMdd") + ".csv")))
            {
                col = 0;
                csv[0, col++] = "工站编号";
                csv[0, col++] = "机台号";
                csv[0, col++] = "记录时间";
                csv[0, col++] = "参数序号";
                csv[0, col++] = "参数名称";
                csv[0, col++] = "参数值";
                row = 1;
                col = 0;
                foreach (var data in datas)
                {
                    if (row == datas.Count)
                    {
                        break;
                    }
                    csv[row, col++] = data.o_workStation; //工站编号
                    csv[row, col++] = data.o_machineNo;//机台号
                    csv[row, col++] = data.o_createTime; //记录时间
                    //csv[row, col++] = data.paramSn;//参数序号                     
                    //csv[row, col++] = data.paramName;//参数名称
                    //csv[row, col++] = data.paramValue;//参数

                    row++;
                    col = 0;
                }
                csv.Save();

                File.SetAttributes(csv.FileName, FileAttributes.ReadOnly);
            }
            else
            {
                File.SetAttributes(csv.FileName, FileAttributes.Normal);
                foreach (var data in datas)
                {

                    if (row == datas.Count - 1)
                    {
                        break;
                    }
                    csv[row, col++] = data.o_workStation; //工站编号
                    csv[row, col++] = data.o_machineNo;//机台号
                    csv[row, col++] = data.o_createTime; //记录时间
                    //csv[row, col++] = data.paramSn;//参数序号                     
                    //csv[row, col++] = data.paramName;//参数名称
                    //csv[row, col++] = data.paramValue;//参数
                    row++;
                    col = 0;
                }
                csv.Save();
                File.SetAttributes(csv.FileName, FileAttributes.ReadOnly);
            }

        }
        /// <summary>
        /// Mqtt返回对象
        /// </summary>
        public class MqttReceiver
        {
            public string msg_id { set; get; }
            public string res_time { set; get; }
            public string rpt_time { set; get; }
            public string res_code { set; get; }
            public string res_msg { set; get; }
            public string deviceno { set; get; }
        }
        public class WaitReceiver
        {
            public MqttReceiver MqttReceiver { set; get; }
            public int Wait_time { set; get; }
            public string Msg_id { set; get; }
            public string ProductCode { set; get; }
            /// <summary>
            /// 年-月-日-时
            /// </summary>
            public string Lot_ID { set; get; }
            public string Send_msg { set; get; }
            public string fileNamelog { set; get; }
            public string Loation_flow_card { set; get; }

        }

        public class 治具数组
        {
            public string createTime { set; get; }
            public string subVehicleType { set; get; }
            /// <summary>
            /// 辅助治具（入料）清洗蓝
            /// </summary>
            public string subVehicleCode { set; get; }
            /// <summary>
            /// 主治具（出料）钢环
            /// </summary>
            public string vehicleCode { set; get; }
            public string location { set; get; }
            public string qty { set; get; }

        }

        /// <summary>
        /// 资产编号与机台号错误时使用
        /// </summary>
        /// <param name="LogID">流程卡号</param>
        /// <param name="a">资产编号</param>
        /// <param name="b">机台号</param>
        public void aaa(string LogID, string a, string b)
        {
            string hiveDirectory = System.IO.Path.Combine(SystemMgr.GetInstance().GetParamString("MqttLogSavePath"), "MQTT", LogID, "NG");
            //  string hiveDirectory1 = Path.Combine(SystemMgr.GetInstance().GetParamString("MqttLogSavePath"), "MQTT", Lot_ID, "Buffer");
            if (Directory.Exists(hiveDirectory))
            {
                string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString() + ".reply";
                string topic1 = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["aoi"].m_dbStandardValue.ToString();
                string[] Files = Directory.GetFiles(hiveDirectory);
                List<string> strlist = new List<string>();
                for (int i = 0; i < Files.Length; i++)
                {
                    if (ConStatue)
                    {
                        string strData = File.ReadAllText(Files[i]);
                        //File.Copy(Files[i], Files[i].Replace("NG", "Buffer"));
                        //补报时删除NG文件夹内的数据

                        //string code = Path.GetFileNameWithoutExtension(Files[i]);
                        //截取文件名中的产品码
                        string strCoed = System.IO.Path.GetFileNameWithoutExtension(Files[i]).Substring(0, 16);
                        string Send_msg = strData.Substring(0, strData.IndexOf("}") + 1);

                        MachineDataTwe MS = JsonConvert.DeserializeObject<MachineDataTwe>(Send_msg);

                        string encryptedData = MS.data; // 替换为实际的加密内容
                        string key = "sjgd2023gyhlwiot"; // 替换为实际的密钥
                        byte[] decryptedData;
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

                                decryptedData = ms.ToArray();
                            }
                        }
                        List<MachineDataTwe.DataItem> data = new List<MachineDataTwe.DataItem>();
                        string decryptedText = Encoding.UTF8.GetString(decryptedData);
                        data = JsonConvert.DeserializeObject<List<MachineDataTwe.DataItem>>(decryptedText);
                        data[0].MACHINE_NO = "L2-1010-009";

                        MS.data = DESEncrypt(JsonConvert.SerializeObject(data), "sjgd2023gyhlwiot");
                        MS.msg_id = a + MS.msg_id.Substring(15);
                        MS.deviceno = a;
                        Send_msg = JsonConvert.SerializeObject(MS);
                        WaitReceiver wReceiver = new WaitReceiver();
                        MqttReceiver mqttRceeiver = new MqttReceiver();
                        wReceiver.Msg_id = MS.msg_id;
                        wReceiver.Lot_ID = LogID;
                        wReceiver.Loation_flow_card = LogID;
                        wReceiver.ProductCode = strCoed;
                        wReceiver.Send_msg = Send_msg;
                        wReceiver.fileNamelog = System.IO.Path.GetFileName(Files[i]);
                        wReceiver.Wait_time = 0;
                        msg_idlist.TryAdd(MS.msg_id, wReceiver);
                        ReSaveMQTTBufferlog(wReceiver, LogID);
                        File.Delete(Files[i]);
                        Mqtt_Subscribe(topic);
                        Mqtt_Send(topic1, Send_msg);
                        Thread.Sleep(50);
                    }
                    else
                    {
                        MessageBox.Show("补报工失败，MQTT已断开连接，请检查后重新进行补报！！！");
                    }

                }
            }
        }

        /// <summary>
        /// 保存状态文件
        /// </summary>
        /// <param name=""></param>
        public static void SaveParamCsv(List<MachineDataparam.DataItem> datas)
        {
            int row = 0, col = 0;
            CsvOperation csv = new CsvOperation(@"D:\ProBsd", "Param" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
            if (!File.Exists(Path.Combine(@"D:\ProBsd", "Param" + DateTime.Now.ToString("yyyyMMdd") + ".csv")))
            {
                //File.SetAttributes(csv.FileName, FileAttributes.Normal);
                col = 0;
                csv[0, col++] = "记录时间";
                csv[0, col++] = "机台号";
                csv[0, col++] = "工站编号";
                csv[0, col++] = "前移载1X补偿";
                csv[0, col++] = "前移载1Y补偿";
                csv[0, col++] = "前移载2X补偿";
                csv[0, col++] = "采集时间";
                row = 1;
                col = 0;
                foreach (var data in datas)
                {
                    //if (row == datas.Count)
                    //{
                    //    break;
                    //}
                    csv[row, col++] = data.o_createTime; //记录时间
                    csv[row, col++] = data.o_machineNo;//机台号
                    csv[row, col++] = data.o_workStation; //工站号
                    csv[row, col++] = data.paramName;//中文名称                 
                    csv[row, col++] = datas.IndexOf(data).ToString();//英文名称
                    csv[row, col++] = data.paramValue;//参数值
                    csv[row, col++] = data.timeStamp; //采集时间
                    row++;
                    col = 0;
                }
                csv.Save();
                File.SetAttributes(csv.FileName, FileAttributes.ReadOnly);
            }
            else
            {
                if (datas != null)
                {
                    foreach (var data in datas)
                    {
                        //if (row == datas.Count-1)
                        //{
                        //    break;
                        //}
                        File.SetAttributes(csv.FileName, FileAttributes.Normal);
                        col = 0;
                        csv[row, col++] = data.o_createTime; //记录时间
                        csv[row, col++] = data.o_machineNo;//机台号
                        csv[row, col++] = data.o_workStation; //工站号
                        csv[row, col++] = data.paramName;//中文名称                         
                        csv[row, col++] = datas.IndexOf(data).ToString();//英文名称
                        csv[row, col++] = data.paramValue;//参数值
                        csv[row, col++] = data.timeStamp; //采集时间
                        row++;
                        col = 0;
                    }
                    csv.Save();
                }

                File.SetAttributes(csv.FileName, FileAttributes.ReadOnly);
            }

        }
    }
}

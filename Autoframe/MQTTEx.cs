using CommonTool;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using ToolEx;
using static AutoFrame.MQTT;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace AutoFrame
{
    class MQTTEx : SingletonTemplate<MQTTEx>
    {



        
        /// <summary>
        /// MQTT改变委托
        /// </summary>
        public delegate void MqttChangeHander();
        /// <summary>
        ///报工失败改变事件
        /// </summary>
        public static event MqttChangeHander Mqtt3ChangeEvent;
        /// <summary>
        /// 线程监视器
        /// </summary>
        public override void ThreadMonitor()
        {
            while (this.m_bRunThread)
            {
                Thread.Sleep(500);
                try
                {
                    ConcurrentDictionary<string, WaitReceiver> msg_idlistTemp = new ConcurrentDictionary<string, WaitReceiver>(Admin_msg_idlist);

                    foreach (var item in msg_idlistTemp.Keys)
                    {

                        WaitReceiver wReceiver = msg_idlistTemp[item];
                        if (Admin_DicReceiver.ContainsKey(wReceiver.Msg_id))
                        {

                            wReceiver.MqttReceiver = Admin_DicReceiver[item];
                            Admin_DicReceiver.TryRemove(item, out MqttReceiver Receiver1);
                            Admin_msg_idlist.TryRemove(item, out WaitReceiver Receiver2);
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
                            Admin_msg_idlist.TryRemove(item, out WaitReceiver Receiver3);
                            SaveMQTTInteractionLog(wReceiver, false);
                            //Mqtt3ChangeEvent();
                        }

                    }
                }
                catch (Exception ex)
                {
                    //ShowLog(ex.ToString());
                    //mqtt_logger.Warn("MQTT线程监控捕获异常，注意排查" + ex.ToString());
                    MessageBox.Show("MQTT线程监控捕获异常，注意排查" + ex.ToString());
                }


            }
        }

        /// <summary>
        /// 保存Log数据
        /// </summary>
        /// <param name="wReceiver"></param>
        /// <param name="Rulte"></param>
        public static void SaveMQTTInteractionLog(WaitReceiver wReceiver, bool Rulte)
        {
            lock (lockO)
            {
                try
                {

                    string hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("Admin_MqttLogSavePath"), wReceiver.Lot_ID.Substring(0, 10), "OK");
                    if (!Rulte)
                    {
                        hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("Admin_MqttLogSavePath"), wReceiver.Lot_ID.Substring(0, 10), "NG");

                    }
                    if (!Directory.Exists(hiveDirectory)) Directory.CreateDirectory(hiveDirectory);
                    string fileNamelog = wReceiver.fileNamelog;
                    fileNamelog = wReceiver.ProductCode + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                    fileNamelog = Path.Combine(hiveDirectory, fileNamelog);
                    File.AppendAllText(fileNamelog, wReceiver.Send_msg + "@" + "\r\n" + JsonConvert.SerializeObject(wReceiver.MqttReceiver));
                    //删除缓存的数据
                    if (wReceiver.fileNamelog != null)
                    {

                        hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("Admin_MqttLogSavePath"), wReceiver.Lot_ID.Substring(0, 10),  "Buffer");
                        fileNamelog = wReceiver.fileNamelog;
                        fileNamelog = Path.Combine(hiveDirectory, fileNamelog);
                        File.Delete(fileNamelog);
                    }
                }
                catch (Exception e)
                {
                    //mqtt_logger.Warn("报工数据记录失败");
                    //mqtt_logger.Warn(e.Message);
                    //MessageBox.Show(e.Message);
                    MessageBox.Show("报工数据记录失败" + e.Message);
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
                //string orderSN = wReceiver.Lot_ID;
                string hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("Admin_MqttLogSavePath"), wReceiver.Lot_ID.Substring(0,10), "Buffer");
                if (!Directory.Exists(hiveDirectory)) Directory.CreateDirectory(hiveDirectory);
                string fileNamelog = wReceiver.fileNamelog;
                fileNamelog = Path.Combine(hiveDirectory, fileNamelog);

                File.AppendAllText(fileNamelog, wReceiver.Send_msg + "@" + "\r\n" + JsonConvert.SerializeObject(wReceiver.MqttReceiver));
            }
            catch (Exception e)
            {
                //WarningMgr.GetInstance().Warning("报工数据缓存失败");
                //mqtt_logger.Warn("报工数据记录失败");
                //mqtt_logger.Warn(e.Message);
                MessageBox.Show("报工数据缓存失败" + e.Message);
            }
        }

        /// <summary>
        /// 3.15 AOI非操作员模式下报工
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="message"></param>
        public static void Admin_ChangAOI(ProductData data1, out string message)
        {
            //if (data1.Code.Length > 17 || data1.Code.Length < 15)
            //{
            message = "";
            //    return;
            //}
            //lock (lockSend)
            //{
            //    string parameters = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["非作业员报工主题"].m_dbStandardValue.ToString() + ".reply";
            //    string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["非作业员报工主题"].m_dbStandardValue.ToString();
            //    MachineDataTwe1 MS = new MachineDataTwe1();
            //    MachineDataTwe.DataItem MS1 = new MachineDataTwe.DataItem();
            //    DateTime time = DateTime.Now;
            //    long Time = ConvertDateTimeToInt(time);
            //    #region
            //    MS.msg_id = "ps-" + DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString() + "-" + Time;    ///车间内使用 101016000171          车间外使用   101016000201
            //    MS.deviceno = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["资产编号"].m_dbStandardValue.ToString();
            //    MS.dataType = "aoi";
            //    //MS.origntime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //    MS.deviceType = parameters;
            //    List<MachineDataTwe1.DataItem1> data = new List<MachineDataTwe1.DataItem1>();

            //    MachineDataTwe1.DataItem1 dt = new MachineDataTwe1.DataItem1(); 
            //    //加工时间 
            //    dt.CREATE_TIME = data1.m_dtOutPutTime.ToString("yyyy-MM-dd HH:mm:ss");
            //    //作业员
            //    dt.O_OPERATOR = "123";
            //    //加工设备模式  状态
            //    dt.OP_MODE = SystemMgr.GetInstance().GetRegString((int)SysStrReg.OP_MODE);
            //    //流程卡号
            //    dt.RUN_CARD_ID = DataMgr.GetInstance().m_dictDataGroup["生产料号"].m_dictData["料号"].m_dbStandardValue.ToString();
            //    //回收托盘二维码
            //    dt.W_VEHICLE_NO = data1.COLLECT_BOX_CODE;
            //    //单片ID 
            //    dt.PIECE_ID = data1.Code;
            //    //包装盒位置
            //    dt.LOCATION = data1.COLLECT_SERIAL;
            //    //数量 
            //    dt.GOODPRODUCTNUMBER = "1";
            //    //判定结果 
            //    dt.DEFECTCODE = "NG";
            //    //机台号
            //    dt.MACHINE_NO = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["机台号"].m_dbStandardValue.ToString();
            //    //工站编号
            //    dt.O_WORKSTATION = "1010";
            //    data.Add(dt);
            //    #endregion
            //    MS.data = DESEncrypt(JsonConvert.SerializeObject(data), "sjgd2023gyhlwiot");
            //    message = JsonConvert.SerializeObject(MS);

            //    MachineDataTwe2 MS_data = new MachineDataTwe2();
            //    MS_data.msg_id = MS.msg_id;
            //    MS_data.deviceno = MS.deviceno;
            //    MS_data.dataType = MS.dataType;
            //    MS_data.deviceType = MS.deviceType;
            //    MS_data.data = data;

            //    WaitReceiver WReceiver = new WaitReceiver();
            //    WReceiver.Msg_id = MS.msg_id;
            //    WReceiver.Lot_ID = DateTime.Now.ToString("yyyy-MM-ddHH");
            //    WReceiver.ProductCode = data1.Code;
            //    WReceiver.fileNamelog = WReceiver.ProductCode + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            //    JsonSerializer serializer = new JsonSerializer();
            //    StringWriter textWriter = new StringWriter();
            //    JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
            //    {
            //        Formatting = Formatting.Indented,
            //        Indentation = 4,
            //        IndentChar = ' '
            //    };
            //    serializer.Serialize(jsonWriter, MS_data);
            //    WReceiver.Send_msg = textWriter.ToString();

            //    if (ConStatue)
            //    {
            //        Admin_msg_idlist.TryAdd(MS.msg_id, WReceiver);
            //        SaveMQTTBufferlog(WReceiver);
            //    }
            //    else
            //    {
            //        SaveMQTTBufferlog(WReceiver);
            //    }

            //    if (string.IsNullOrEmpty(parameters))
            //    {
            //        MessageBox.Show("发布主题不能为空！");
            //        return;
            //    }
            //    Mqtt_Subscribe(parameters);
            //    Mqtt_Send(topic, message);
            //}
        }
        public static string ConvertJsonString(object obj)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return "";
            }
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

        public void Returnpass1(string LogID)
        {

            string hiveDirectory = Path.Combine(SystemMgr.GetInstance().GetParamString("Admin_MqttLogSavePath"), LogID, "NG");
            if (Directory.Exists(hiveDirectory))
            {
                string topic = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["非作业员报工主题"].m_dbStandardValue.ToString() + ".reply";
                string topic1 = DataMgr.GetInstance().m_dictDataGroup["MQTT数据"].m_dictData["非作业员报工主题"].m_dbStandardValue.ToString();
                string[] Files = Directory.GetFiles(hiveDirectory);
                //List<string> strlist = new List<string>();
                for (int i = 0; i < Files.Length; i++)
                {
                    if (ConStatue)
                    {
                        string strData = File.ReadAllText(Files[i]);
                        File.Copy(Files[i], Files[i].Replace("NG", "Buffer"));
                        //补报时删除NG文件夹内的数据
                        File.Delete(Files[i]);
                        string code = Path.GetFileNameWithoutExtension(Files[i]);
                        //截取文件名中的产品码
                        string strCoed = Path.GetFileNameWithoutExtension(Files[i]).Substring(0, 16);
                        string Send_msg = strData.Substring(0, strData.IndexOf("@"));

                        MachineDataTwe MS = JsonConvert.DeserializeObject<MachineDataTwe>(Send_msg);
                        MS.data = DESEncrypt(JsonConvert.SerializeObject(MS.data), "sjgd2023gyhlwiot");

                        WaitReceiver wReceiver = new WaitReceiver();
                        wReceiver.Msg_id = MS.msg_id;
                        wReceiver.Lot_ID = LogID;
                        wReceiver.Loation_flow_card = LogID;
                        wReceiver.ProductCode = strCoed;
                        wReceiver.Send_msg = Send_msg;
                        wReceiver.fileNamelog = Path.GetFileName(Files[i]);
                        wReceiver.Wait_time = 0;
                        Admin_msg_idlist.TryAdd(MS.msg_id, wReceiver);
                        Mqtt_Subscribe(topic);
                        Send_msg = JsonConvert.SerializeObject(MS);
                        Mqtt_Send(topic1, Send_msg);
                        Thread.Sleep(200);
                    }
                    else
                    {
                        MessageBox.Show("补报工失败，MQTT已断开连接，请检查后重新进行补报！！！");
                    }
                }
            }
        }
    }
}

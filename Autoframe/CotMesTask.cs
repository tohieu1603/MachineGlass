using CommonTool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolEx;
using static AutoFrame.MQTT;
using NLog;
//using NLog.Fluent;

namespace AutoFrame
{
    internal class CotMesTask: SingletonTemplate<CotMesTask>
    {
        public ConcurrentQueue<ProductInfo> QUEUE = new ConcurrentQueue<ProductInfo>();

        private static Logger _logger = LogManager.GetLogger("CotMesTask");
        ProductInfo m_data = new ProductInfo();
        public int GetQueueCount()
        {
            return QUEUE.Count;
        }
        public bool AddQueue(ProductData data,int index)
        {
            if (QUEUE.Count > 100)
            {
                return false;
            }
            else
            {
                QUEUE.Enqueue(data.ProductInfo4[index]);
                return true;
            }
           
        }
        public bool AddQueue(ProductInfo data)
        {
            if (QUEUE.Count > 100)
            {
                return false;
            }
            else
            {
                QUEUE.Enqueue(data);
                return true;
            }

        }
        public override void ThreadMonitor()
        {
            while (this.m_bRunThread)
            {
                Thread.Sleep(1000);
                try
                {
                   
                    
                    if (GetQueueCount() > 0)
                    {
                        DateTime start = DateTime.Now;
                        if (QUEUE.TryDequeue(out m_data))
                        {
                            string message;
                            //if (SystemMgr.GetInstance().GetRegString((int)SysStrReg.str_WorkMode) != WorkMode.测试模式.ToString())
                            if(false)
                            {
                                MQTT.ChangAOI(m_data, out message);
                                //MQTTEx.Admin_ChangAOI(m_data, out message);
                            }
                            else
                            {
                                if (SystemMgr.GetInstance().GetParamBool("EnableMES"))
                                {
                                    MQTT.ChangAOI(m_data, out message);
                                    //MQTTEx.Admin_ChangAOI(m_data, out message);
                                }
                                else
                                {
                                    //MQTTEx.Admin_ChangAOI(m_data, out message);
                                }
                                
                            }
                        }
                        _logger.Trace($"报工开始时间：{start.ToString("yyyy-MM-dd HH:mm:ss:fff")}，耗时：{(DateTime.Now - start).TotalMilliseconds}");
                    }
                }
                catch (Exception ex)
                {
                    //ShowLog(ex.ToString());
                    WarningMgr.GetInstance().Warning("", "MQTT报工线程监控捕获异常，请排查后请新启动" + ex.ToString());
                }


            }
        }
    }
}

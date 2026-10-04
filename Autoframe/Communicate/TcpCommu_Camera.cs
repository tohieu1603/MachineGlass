using AF.Communicate;
using AF.Station;
using Autoframe;
using System;
using System.Collections.Generic;
using AF.Common;
using AF.Data;
using Foundation;
using System.Threading;
using System.Xml.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.VisualBasic.Devices;

namespace Autoframe
{
    public class TcpCommu_Camera : TcpCommunicate
    {
        private AutoProcess autoProcess = AutoProcess.生产;

        /// <summary>
        /// 初始化锁
        /// </summary>
        private readonly object m_InitLock = new object();

        /// <summary>
        /// 创建TCP连接
        /// </summary>
        public TcpClient tcpLink = null;

        /// <summary>
        /// 接收相机返回的数据(已分割)
        /// </summary>
        private string[] strReceive = null;

        /// <summary>
        /// 接收相机返回的数据(未分割)
        /// </summary>
        protected string strRcvOrg;

        /// <summary>
        /// 发送数据的命令头
        /// </summary>
        string strStartCmd;

        //通讯变量
        public double d上料引导位置X;
        public double d上料引导位置Y;
        public double d上料引导位置U;

        public double[] d下料引导位置X = new double[] { };
        public double[] d下料引导位置Y = new double[] { };
        public double[] d下料引导位置U = new double[] { };

        public bool[] b上料产品定位结果 = new bool[] { };
        /// <summary>
        /// 结果
        /// </summary>
        public bool result;


        // 命令枚举
        public enum Command
        {
            CCD1_上料排片引导,

            CCD2_上料产品引导,

            CCD3_IR面检测_拍照,
            CCD3_IR面检测_获取结果,
            CCD4_翻转上料产品定位,

            CCD5_翻转上料产品定位,

            CCD6_AR面检测_拍照,
            CCD6_AR面检测_获取结果,

            CCD7_下料产品引导,

            CCD8_下料产品引导,


            CCD1_九点标定,
            CCD1_旋转标定,
            CCD1_注册首个产品模板,

            CCD2_九点标定,
            CCD2_旋转标定,
            CCD2_注册取料模板,

            CCD4_注册物料模板,

            CCD5_注册物料模板,

            CCD7_九点标定,
            CCD7_旋转标定,
            CCD7_注册物料模板,

            CCD8_九点标定,
            CCD8_旋转标定,
            CCD8_注册物料模板,

            CCD1_静态,
            CCD1_动态,
            CCD2_静态,
            CCD2_动态,
            CCD3_静态,
            CCD3_动态,
            CCD4_静态,
            CCD4_动态,
            CCD5_静态,
            CCD5_动态,
            CCD6_静态,
            CCD6_动态,
            CCD7_静态,
            CCD7_动态,
            CCD8_静态,
            CCD8_动态,
        }

        public TcpCommu_Camera() : base()
        {
            //绑定网口
            this.tcplink = TCP_Client.CCD1.GetTcp();
            this.typeCommand = typeof(Command);
        }


        /// <summary>
        /// 初始化相机连接
        /// </summary>
        /// <param name="nIpIndex">网口索引</param>
        /// <returns></returns>
        public bool Init(StationEx station, int nIpIndex)
        {
            lock (m_InitLock)
            {
                try
                {
                    this.station = station;
                    if (!Parameter.GetBit("VisionEnable"))
                    {
                        Logger.Process(station.Name, "相机未启用");
                        return true;
                    }

                    this.tcpLink = TcpClientMgr.GetClient(nIpIndex);

                    //if (tcpLink.IsOpen)
                    //{
                    //    return true;
                    //}

                    this.tcpLink.Close();
                    for (int i = 0; i < 3; i++)
                    {
                        if (this.tcpLink.Open())
                            break;
                        Thread.Sleep(100);

                    }
                    if (!this.tcpLink.IsOpen)
                    {
                        EventMgr.Warn(ErrCommunicate.报警, this.tcpLink.m_strName, "连接失败".txtTrans(), this.station == null ? "" : this.station.Name);
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    EventMgr.Warn(ErrCommunicate.报警, this.tcpLink.m_strName, "连接失败".txtTrans(), this.station == null ? "" : this.station.Name);
                    return false;
                }
            }

        }


        /// <summary>
        /// 获取发送指令文本
        /// </summary>
        /// <param name="nCmdIdx">命令索引</param>
        /// <param name="param">命令参数</param>
        /// <returns></returns>
        public override string GetSendString(object nCmdIdx, params object[] param)
        {
            if (!(nCmdIdx is Command))
            {
                EventMgr.Error("参数类型错误".txtTrans(TransType.Mark));
                return "";
            }

            if (param.Length==0)
                param = new string[] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };


            switch ((Command)nCmdIdx)
            {
                case Command.CCD1_上料排片引导://LCR，CCD1/2，ProcessID，X,Y,R，UserString;  ProcessID：流程ID，1/2; X,Y,R：轴系实时位置
                case Command.CCD2_上料产品引导:
                    if ((Command)nCmdIdx == Command.CCD1_上料排片引导)
                        this.CmdSend = $"LCR,CCD1,1,{param[0]},{param[1]},{param[2]},";
                    else
                        this.CmdSend = $"LCR,CCD2,2,{param[0]},{param[1]},{param[2]},";
                    break;
                case Command.CCD3_IR面检测_拍照://单次拍照 AOI，CCD3，ProcessID,ProductID,Barcode; ProcessID: 流程ID，3;ProductID：1~4;Barcode: 时间戳 / 二维码                                           
                                           // this.CmdSend = $"AOI,CCD3,3,{param[0]},{DateTime.Now.ToString("yyyyMMddHHmmss")}";
                    this.CmdSend = $"T3,{param[0]},{param[1]}"; //T3，n,SN
                    break;
                case Command.CCD3_IR面检测_获取结果://AOIResult，CCD3，Barcode1,Barcode2,Barcode3,Barcode4
                    //this.CmdSend = $"AOIResult,CCD3,Barcode1,Barcode2,Barcode3,Barcode4";
                    this.CmdSend = $"SR,{param[0]}";  //SR,id
                    break;
                case Command.CCD4_翻转上料产品定位://LCR，CCD4/5,ProcessID,X,Y,R,UserString;ProcessID: 流程ID,4/5;X,Y,R：轴系实时位置
                case Command.CCD5_翻转上料产品定位:
                    if ((Command)nCmdIdx == Command.CCD4_翻转上料产品定位)
                        this.CmdSend = $"LCR,CCD4,4,{param[0]},{param[1]},{param[2]},0";
                    else
                        this.CmdSend = $"LCR,CCD5,5,{param[0]},{param[1]},{param[2]},0";
                    break;
                case Command.CCD6_AR面检测_拍照://单次拍照 AOI，CCD6，ProcessID,ProductID,Barcode; ProcessID: 流程ID，6;ProductID：1~4;Barcode: 时间戳 / 二维码                                           
                    //this.CmdSend = $"AOI,CCD6,6,{param[0]},{DateTime.Now.ToString("yyyyMMddHHmmssfff")}";
                    this.CmdSend = $"T6,{param[0]},{param[1]}"; //T3，n,SN
                    break;
                case Command.CCD6_AR面检测_获取结果://AOIResult，CCD6，Barcode1,Barcode2,Barcode3,Barcode4
                    //this.CmdSend = $"AOIResult,CCD6,Barcode1,Barcode2,Barcode3,Barcode4";
                    this.CmdSend = $"SR,{param[0]}";//SR,id
                    break;
                case Command.CCD7_下料产品引导://LCR，CCD7/8,ProcessID,X,Y,R,UserString;ProcessID: 流程ID,7/8;X,Y,R：轴系实时位置
                case Command.CCD8_下料产品引导:
                    if ((Command)nCmdIdx == Command.CCD4_翻转上料产品定位)
                        this.CmdSend = $"LCR,CCD7,7,{param[0]},{param[1]},{param[2]},0";
                    else
                        this.CmdSend = $"LCR,CCD8,8,{param[0]},{param[1]},{param[2]},0";
                    break;
                case Command.CCD1_九点标定://CALS，CCD1,ProcessID,PosID，X，Y，R，UserString；ProcessID: 流程ID;PosID：1~9，九个点的顺序，5号点位为九点标定基准点，需要上位机在界面中开放出来，其余8个点围绕5号点走等分间距X, Y, R:轴的实时位置
                case Command.CCD2_九点标定:
                case Command.CCD7_九点标定:
                case Command.CCD8_九点标定:
                    if ((Command)nCmdIdx == Command.CCD1_九点标定)
                        this.CmdSend = $"CALS,CCD1,1,{param[0]},{param[1]},{param[2]},0";
                    else if ((Command)nCmdIdx == Command.CCD2_九点标定)
                        this.CmdSend = $"LCR,CCD2,4,{param[0]},{param[1]},{param[2]},0";
                    else if ((Command)nCmdIdx == Command.CCD7_九点标定)
                        this.CmdSend = $"LCR,CCD7,9,{param[0]},{param[1]},{param[2]},0";
                    else
                        this.CmdSend = $"LCR,CCD8,12,{param[0]},{param[1]},{param[2]},0";
                    break;
                case Command.CCD1_旋转标定://CCS，CCD1，ProcessID，PosID，X，Y，R，UserString；ProcessID：流程ID，2;PosID：1~5,5个点的顺序，3号点位为旋转标定基准点，通常情况可与九点标定基准点为同一个，其余4个点，围绕3号点，旋转固定角度，XY不变X, Y, R:轴的实时位置
                case Command.CCD2_旋转标定:
                case Command.CCD7_旋转标定:
                case Command.CCD8_旋转标定:
                    if ((Command)nCmdIdx == Command.CCD1_旋转标定)
                        this.CmdSend = $"CCS,CCD1,2,{param[0]},{param[1]},{param[2]},0";
                    else if ((Command)nCmdIdx == Command.CCD2_旋转标定)
                        this.CmdSend = $"CCS,CCD2,3,{param[0]},{param[1]},{param[2]},0";
                    else if ((Command)nCmdIdx == Command.CCD7_旋转标定)
                        this.CmdSend = $"CCS,CCD7,10,{param[0]},{param[1]},{param[2]},0";
                    else
                        this.CmdSend = $"CCS,CCD8,13,{param[0]},{param[1]},{param[2]},0";
                    break;
                case Command.CCD1_注册首个产品模板://TA，CCD1，ProcessID，X,Y,R,UserString;ProcessID：流程ID，3;X,Y,R: 轴的实时位置
                case Command.CCD2_注册取料模板:
                case Command.CCD4_注册物料模板:
                case Command.CCD5_注册物料模板:
                case Command.CCD7_注册物料模板:
                case Command.CCD8_注册物料模板:
                    if ((Command)nCmdIdx == Command.CCD1_注册首个产品模板)
                        this.CmdSend = $"TA,CCD1,3,{param[0]},{param[1]},{param[2]},0";
                    else if ((Command)nCmdIdx == Command.CCD2_注册取料模板)
                        this.CmdSend = $"TA,CCD2,6,{param[0]},{param[1]},{param[2]},0";
                    else if ((Command)nCmdIdx == Command.CCD4_注册物料模板)
                        this.CmdSend = $"TA,CCD4,7,{param[0]},{param[1]},{param[2]},0";
                    else if ((Command)nCmdIdx == Command.CCD5_注册物料模板)
                        this.CmdSend = $"TA,CCD5,8,{param[0]},{param[1]},{param[2]},0";
                    else if ((Command)nCmdIdx == Command.CCD7_注册物料模板)
                        this.CmdSend = $"TA,CCD7,11,{param[0]},{param[1]},{param[2]},0";
                    else
                        this.CmdSend = $"TA,CCD6,14,{param[0]},{param[1]},{param[2]},0";
                    break;
                case Command.CCD1_静态:
                    this.CmdSend = $"Static,CCD1";
                    break;
                case Command.CCD1_动态:
                    this.CmdSend = $"Dynamic,CCD1";
                    break;
                case Command.CCD2_静态:
                    this.CmdSend = $"Static,CCD2";
                    break;
                case Command.CCD2_动态:
                    this.CmdSend = $"Dynamic,CCD2";
                    break;
                case Command.CCD3_静态:
                    this.CmdSend = $"Static,CCD3";
                    break;
                case Command.CCD3_动态:
                    this.CmdSend = $"Dynamic,CCD3";
                    break;
                case Command.CCD4_静态:
                    this.CmdSend = $"Static,CCD4";
                    break;
                case Command.CCD4_动态:
                    this.CmdSend = $"Dynamic,CCD4";
                    break;
                case Command.CCD5_静态:
                    this.CmdSend = $"Static,CCD5";
                    break;
                case Command.CCD5_动态:
                    this.CmdSend = $"Dynamic,CCD5";
                    break;
                case Command.CCD6_静态:
                    this.CmdSend = $"Static,CCD6";
                    break;
                case Command.CCD6_动态:
                    this.CmdSend = $"Dynamic,CCD6";
                    break;
                case Command.CCD7_静态:
                    this.CmdSend = $"Static,CCD7";
                    break;
                case Command.CCD7_动态:
                    this.CmdSend = $"Dynamic,CCD7";
                    break;
                case Command.CCD8_静态:
                    this.CmdSend = $"Static,CCD8";
                    break;
                case Command.CCD8_动态:
                    this.CmdSend = $"Dynamic,CCD8";
                    break;
                default:
                    this.CmdSend = "";
                    break;
            }

            return base.GetSendString(nCmdIdx, param); ;
        }


        /// <summary>
        /// 发送相机命令
        /// </summary>
        /// <param name="strRcv"></param>
        /// <param name="nCmdIdx"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool SendCmd(out string[] strRcv, int nCmdIdx, bool bWait, params object[] param)
        {
            strRcv = new string[] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
            if (!Parameter.GetBit("VisionEnable"))
            {
                return true;
            }

            if (!this.tcpLink.IsOpen)
                this.tcpLink.Open();
            this.tcpLink.ClearBuffer();

            string strSend = GetSendString(nCmdIdx, param);
            this.tcpLink.WriteLine(strSend);
            Logger.Process(this.station == null ? "" : this.station.Name, $"->[{this.tcpLink.m_strName}] {strSend}");
            switch ((Command)nCmdIdx)
            {
                case Command.CCD1_上料排片引导:
                case Command.CCD1_九点标定:
                case Command.CCD1_旋转标定:
                case Command.CCD1_注册首个产品模板:
                    DO.CCD1触发.Set(true);
                    Thread.Sleep(20);
                    DO.CCD1触发.Set(false);
                    break;
                case Command.CCD2_上料产品引导:
                case Command.CCD2_注册取料模板:
                case Command.CCD2_九点标定:
                case Command.CCD2_旋转标定:
                    DO.CCD2触发.Set(true);
                    Thread.Sleep(20);
                    DO.CCD2触发.Set(false);
                    break;
                case Command.CCD3_IR面检测_拍照:
                case Command.CCD3_IR面检测_获取结果:
                    //DO.CCD3触发.Set(true);
                    //Thread.Sleep(20);
                    //DO.CCD3触发.Set(false);
                    break;
                case Command.CCD4_翻转上料产品定位:
                case Command.CCD4_注册物料模板:
                    DO.CCD4触发.Set(true);
                    Thread.Sleep(20);
                    DO.CCD4触发.Set(false);
                    break;
                case Command.CCD5_翻转上料产品定位:
                case Command.CCD5_注册物料模板:
                    DO.CCD4触发.Set(true);
                    Thread.Sleep(20);
                    DO.CCD4触发.Set(false);
                    break;
                case Command.CCD6_AR面检测_拍照:
                case Command.CCD6_AR面检测_获取结果:
                    //DO.CCD6触发.Set(true);
                    //Thread.Sleep(20);
                    //DO.CCD6触发.Set(false);
                    break;
                case Command.CCD7_下料产品引导:
                case Command.CCD7_九点标定:
                case Command.CCD7_旋转标定:
                case Command.CCD7_注册物料模板:
                    DO.CCD7触发.Set(true);
                    Thread.Sleep(20);
                    DO.CCD7触发.Set(false);
                    break;
                case Command.CCD8_下料产品引导:
                case Command.CCD8_九点标定:
                case Command.CCD8_旋转标定:
                case Command.CCD8_注册物料模板:
                    DO.CCD7触发.Set(true);
                    Thread.Sleep(20);
                    DO.CCD7触发.Set(false);
                    break;
                default:
                    break;
            }
            Delay(100);
            if (!bWait)
                return true;

            bool bResult = WaitReplyAndProcess(out strRcv, nCmdIdx);

            if (!bResult)
                return false;
            return true;

        }

        /// <summary>
        /// 等待回复并处理
        /// </summary>
        /// <param name="strDatas"></param>
        /// <param name="nCmdIdx"></param>
        /// <param name="data"></param>
        /// <param name="bCheckRcv"></param>
        /// <param name="respond"></param>
        /// <returns></returns>
        public bool WaitReplyAndProcess(out string[] strDatas, int nCmdIdx)
        {
            strDatas = new string[] { };
            if (Parameter.GetBit("VisionEnable"))
            {
                strDatas = new string[] { "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
                return true;
            }

            if (!Recevie(out strDatas, strStartCmd))
                return false;

            AnalyzeRcvString(nCmdIdx);
            return true;

        }

        /// <summary>
        /// 接收回复
        /// </summary>
        /// <param name="strDatas"></param>
        /// <param name="strStartWith"></param>
        /// <param name="strEndWith"></param>
        /// <param name="nTimeOut"></param>
        /// <param name="nCmdIdx"></param>
        /// <returns></returns>
        private bool Recevie(out string[] strDatas, string strStartWith, string strEndWith = "", int nTimeOut = 0, int nCmdIdx = -1)
        {
            string strTemp = "";
            strReceive = null;
            strDatas = new string[] { };
            if (this.station.WaitTcp(tcpLink, out strTemp, strStartWith, "", nTimeOut, false))
            {
                strRcvOrg = strTemp;
                strReceive = strTemp.Split(',');
                for (int i = 0; i < strReceive.Length; ++i)
                    strReceive[i] = strReceive[i].Trim();
                strDatas = strReceive;
                return true;
            }
            else
            {
                for (int i = 0; i < strReceive.Length; ++i)
                    strReceive[i] = "";
                strDatas = strReceive;
                strRcvOrg = "";
                return false;
            }
        }


        /// <summary>
        /// 接受数据解析
        /// </summary>
        /// <param name="nCmdIdx">命令索引</param>
        public override bool AnalyzeRcvString(object nCmdIdx)
        {
            if (!((Command)nCmdIdx is Command))
            {
                EventMgr.Error("参数类型错误".txtTrans(TransType.Mark));
                return false;
            }

            switch ((Command)nCmdIdx)
            {
                case Command.CCD1_上料排片引导:
                case Command.CCD2_上料产品引导:
                    result = GetRcvInt(2) == 1;
                    d上料引导位置X = GetRcvDouble(3);
                    d上料引导位置Y = GetRcvDouble(4);
                    d上料引导位置U = GetRcvDouble(5);
                    break;
                case Command.CCD4_翻转上料产品定位:
                    result = GetRcvInt(1) == 1;
                    b上料产品定位结果[0] = GetRcvBit(3);
                    b上料产品定位结果[1] = GetRcvBit(4);
                    break;
                case Command.CCD5_翻转上料产品定位:
                    result = GetRcvInt(1) == 1;
                    b上料产品定位结果[2] = GetRcvBit(3);
                    b上料产品定位结果[3] = GetRcvBit(4);
                    break;
                case Command.CCD7_下料产品引导:
                    result = GetRcvInt(1) == 1;
                    d下料引导位置X[0] = GetRcvDouble(3);
                    d下料引导位置Y[0] = GetRcvDouble(4);
                    d下料引导位置U[0] = GetRcvDouble(5);
                    d下料引导位置X[1] = GetRcvDouble(6);
                    d下料引导位置Y[1] = GetRcvDouble(7);
                    d下料引导位置U[1] = GetRcvDouble(8);
                    break;
                case Command.CCD8_下料产品引导:
                    result = GetRcvInt(1) == 1;
                    d下料引导位置X[2] = GetRcvDouble(3);
                    d下料引导位置Y[2] = GetRcvDouble(4);
                    d下料引导位置U[2] = GetRcvDouble(5);
                    d下料引导位置X[3] = GetRcvDouble(6);
                    d下料引导位置Y[3] = GetRcvDouble(7);
                    d下料引导位置U[3] = GetRcvDouble(8);
                    break;
            }
            if (autoProcess == AutoProcess.空跑)
            {
                result = true;
            }
            return true;
        }
    }
}
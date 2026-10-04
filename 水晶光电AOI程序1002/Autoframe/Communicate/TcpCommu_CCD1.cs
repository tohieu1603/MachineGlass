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
    public class TcpCommu_CCD1 : TcpCommunicate
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
        string strStartCmd="";

        //通讯变量
        public double d上料引导1位置X;
        public double d上料引导1位置Y;
        public double d上料引导2位置X;
        public double d上料引导2位置Y;
        public double d上料引导3位置X;
        public double d上料引导3位置Y;
        public double d上料引导位置U;
        public int d上料行数;
        public int d上料列数;

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

            CCD1_九点标定,
            CCD1_旋转标定,
            CCD1_注册首个产品模板,

            CCD1_静态,
            CCD1_动态,
 
        }

        public TcpCommu_CCD1() : base()
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
            if (!((Command)nCmdIdx is Command))
            {
                EventMgr.Error("参数类型错误".txtTrans(TransType.Mark));
                return "";
            }

            if (param.Length==0)
                param = new string[] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };


            switch ((Command)nCmdIdx)
            {
                case Command.CCD1_上料排片引导://LCR，CCD1，Status,X1,Y1,X2,Y2,X3,Y3,R，Rows，Columns;LCR，CCD1，ProcessID，X,Y,R，UserString;
                    this.CmdSend = $"LCR,CCD1,1,{param[0]},{param[1]},{param[2]},0";
                    break;
                case Command.CCD1_九点标定://CALS，CCD1,ProcessID,PosID，X，Y，R，UserString；ProcessID: 流程ID;PosID：1~9，九个点的顺序，5号点位为九点标定基准点，需要上位机在界面中开放出来，其余8个点围绕5号点走等分间距X, Y, R:轴的实时位置
                        this.CmdSend = $"CALS,CCD1,1,{param[0]},{param[1]},{param[2]},{param[3]},0";
                    break;
                case Command.CCD1_旋转标定://CCS，CCD1，ProcessID，PosID，X，Y，R，UserString；ProcessID：流程ID，2;PosID：1~5,5个点的顺序，3号点位为旋转标定基准点，通常情况可与九点标定基准点为同一个，其余4个点，围绕3号点，旋转固定角度，XY不变X, Y, R:轴的实时位置
                    if ((Command)nCmdIdx == Command.CCD1_旋转标定)
                        this.CmdSend = $"CCS,CCD1,2,{param[0]},{param[1]},{param[2]},{param[3]},0";
                    break;
                case Command.CCD1_注册首个产品模板://TA，CCD1，ProcessID，X,Y,R,UserString;ProcessID：流程ID，3;X,Y,R: 轴的实时位置
                    if ((Command)nCmdIdx == Command.CCD1_注册首个产品模板)
                        this.CmdSend = $"TA,CCD1,3,{param[0]},{param[1]},{param[2]},0";
                    break;
                case Command.CCD1_静态:
                    this.CmdSend = $"Static,CCD1";
                    break;
                case Command.CCD1_动态:
                    this.CmdSend = $"Dynamic,CCD1";
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
            result= false;
            strRcv = new string[] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
            if (!Parameter.GetBit("VisionEnable"))
            {
                result = true;
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
                case Command.CCD1_静态:
                case Command.CCD1_动态:
                    DO.CCD1触发.Set(true);
                    Thread.Sleep(20);
                    DO.CCD1触发.Set(false);
                    break;
                default:
                    break;
            }
            if (!bWait)
                return true;

            bool bResult = WaitReplyAndProcess(out strRcv, nCmdIdx);
            result= bResult;
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
            if (!Parameter.GetBit("VisionEnable"))
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

                strRcv = strTemp.Split(',');
                strReceive = strTemp.Split(',');
                for (int i = 0; i < strReceive.Length; ++i)
                {
                    strRcv[i] = strRcv[i].Trim();
                    strReceive[i] = strReceive[i].Trim();
                }
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
                    result = GetRcvInt(2) == 1;
                    d上料引导1位置X = GetRcvDouble(3);
                    d上料引导1位置Y = GetRcvDouble(4);
                    d上料引导2位置X = GetRcvDouble(5);
                    d上料引导2位置Y = GetRcvDouble(6);
                    d上料引导3位置X = GetRcvDouble(7);
                    d上料引导3位置Y = GetRcvDouble(8);
                    d上料引导位置U = GetRcvDouble(9);
                    d上料行数 = (int)GetRcvDouble(10);
                    d上料列数 = (int)GetRcvDouble(11);
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
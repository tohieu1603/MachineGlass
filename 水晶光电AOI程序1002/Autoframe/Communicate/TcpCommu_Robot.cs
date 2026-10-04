using AF.Common;
using AF.Communicate;
using AF.Data;
using AF.Station;
using Autoframe;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Autoframe
{
    public class TcpCommu_Robot : TcpCommunicate
    {
        //通讯变量
        public double fX;
        public double fY;
        public double fZ;
        public double fU;
        public string strRobotRec;

        /// <summary>
        /// 结果
        /// </summary>
        public bool result;
        /// <summary>
        /// 创建TCP连接
        /// </summary>
        public TcpClient tcpLink = null;

        /// <summary>
        /// 初始化锁
        /// </summary>
        private readonly object m_InitLock = new object();

        private string[] RePlay = null;
        // 命令枚举
        public enum Command
        {
            初始化,
            待机位,
            过度位,
            回过度位,
            取料,
            拍照,
            吸嘴1OK覆膜放料,
            吸嘴2OK覆膜放料,
            吸嘴3OK覆膜放料,
            吸嘴4OK覆膜放料,
            吸嘴1NG覆膜放料,
            吸嘴2NG覆膜放料,
            吸嘴3NG覆膜放料,
            吸嘴4NG覆膜放料,
            CCD7_九点标定,
            CCD8九点标定,
            CCD7旋转标定,
            CCD8旋转标定,
            基准标定,
            获取取料位,
            获取拍照位,
            DO控制,

        }

        public TcpCommu_Robot() : base()
        {
            //绑定网口
            this.tcplink = TCP_Client.机械手.GetTcp();
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
                    //if (!Parameter.GetBit("VisionEnable"))
                    //{
                    //    Logger.Process(station.Name, "Robot未启用");
                    //    return true;
                    //}

                    this.tcpLink = TcpClientMgr.GetClient(nIpIndex);

                    //if (tcpLink.IsOpen)
                    //{
                    //    return true;
                    //}

                    this.tcpLink.Close();
                    Delay(500);
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
            switch ((Command)nCmdIdx)
            {
                case Command.初始化:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = $"Init,{param[0]}";
                    else
                        this.CmdSend = "Init,0";
                    break;

                case Command.待机位:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = $"Home,{param[0]},{param[1]}";
                    else
                        this.CmdSend = "Home,速度";
                    break;
                case Command.过度位:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = $"WaitP,{param[0]}";
                    else
                        this.CmdSend = "WaitP,速度";
                    break;
                case Command.回过度位:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = $"backWait,{param[0]}";
                    else
                        this.CmdSend = "backWait,速度";
                    break;
                case Command.取料:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = $"Pick,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]}";
                    else
                        this.CmdSend = "Pick,速度";
                    break;

                case Command.拍照:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = $"Snap,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]}";
                    else
                        this.CmdSend = "Snap,20";
                    break;

                case Command.吸嘴1OK覆膜放料:
                    if (param != null && param.Length >= 4)
                        this.CmdSend = $"PassPut1,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]}";
                    else
                        this.CmdSend = "PassPut1,N,X,Y,R,1";//0代表有料,1无料
                    break;

                case Command.吸嘴2OK覆膜放料:
                    if (param != null && param.Length >= 4)
                        this.CmdSend = $"PassPut2,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]}";
                    else
                        this.CmdSend = "PassPut2,N,X,Y,R";
                    break;

                case Command.吸嘴3OK覆膜放料:
                    if (param != null && param.Length >= 4)
                        this.CmdSend = $"PassPut3,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]}";
                    else
                        this.CmdSend = "PassPut3,N,X,Y,R";
                    break;

                case Command.吸嘴4OK覆膜放料:
                    if (param != null && param.Length >= 4)
                        this.CmdSend = $"PassPut4,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]}";
                    else
                        this.CmdSend = "PassPut4,N,X,Y,R";
                    break;

                case Command.吸嘴1NG覆膜放料:
                    if (param != null && param.Length >= 4)
                        this.CmdSend = $"FailPut1,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]}";
                    else
                        this.CmdSend = "FailPut1,N,X,Y,R";
                    break;

                case Command.吸嘴2NG覆膜放料:
                    if (param != null && param.Length >= 4)
                        this.CmdSend = $"FailPut2,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]}";
                    else
                        this.CmdSend = "FailPut2,N,X,Y,R";
                    break;

                case Command.吸嘴3NG覆膜放料:
                    if (param != null && param.Length >= 4)
                        this.CmdSend = $"FailPut3,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]}";
                    else
                        this.CmdSend = "FailPut3,N,X,Y,R";
                    break;


                case Command.吸嘴4NG覆膜放料:
                    if (param != null && param.Length >= 4)
                        this.CmdSend = $"FailPut4,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]}";
                    else
                        this.CmdSend = "FailPut4,N,X,Y,R";
                    break;

                case Command.CCD7_九点标定:
                    if (param != null && param.Length >= 2)
                        this.CmdSend = $"LeftCCDCalib,{param[0]},{param[1]}";
                    else
                        this.CmdSend = "LeftCCDCalib,N,";
                    break;

                case Command.CCD7旋转标定:
                    if (param != null && param.Length >= 2)
                        this.CmdSend = $"LeftCCDRotationCalib,{param[0]},{param[1]}";
                    else
                        this.CmdSend = "LeftCCDRotationCalib,N";
                    break;

                case Command.CCD8九点标定:
                    if (param != null && param.Length >= 2)
                        this.CmdSend = $"RightCCDCalib,{param[0]},{param[1]}"; 
                    else
                        this.CmdSend = "RightCCDCalib,N";
                    break;

                case Command.CCD8旋转标定:
                    if (param != null && param.Length >= 2)
                        this.CmdSend = $"RightCCDRotationCalib,{param[0]},{param[1]}";
                    else
                        this.CmdSend = "RightCCDRotationCalib,N";
                    break;

                case Command.基准标定:
                    if (param != null && param.Length >= 0)
                        this.CmdSend = $"GlodenCalib";
                    else
                        this.CmdSend = "GlodenCalib";
                    break;

                case Command.获取取料位:
                    if (param != null && param.Length >= 0)
                        this.CmdSend = $"GetPick,0";
                    else
                        this.CmdSend = "GetPick,0";
                    break;

                case Command.获取拍照位:
                    if (param != null && param.Length >= 0)
                        this.CmdSend = $"GetSnap,0";
                    else
                        this.CmdSend = "GetSnap,0";
                    break;

                case Command.DO控制:
                    if (param != null && param.Length >= 2)
                        this.CmdSend = $"SingleIO,{param[0]},{param[1]}";
                    else
                        this.CmdSend = "SingleIO,0,0";
                    break;

                default:
                    this.CmdSend = "";
                    break;
            }

            return base.GetSendString(nCmdIdx, param); ;
        }
        /// <summary>
        /// 机械手发送指令接口
        /// </summary>
        /// <param name="strRcv">接收数据</param>
        /// <param name="nCmdIdx">机械手指令</param>
        /// <param name="bWait">是否等待接收数据</param>
        /// <param name="param">机械手参数</param>
        /// <returns></returns>
        public bool SendCmd(out string[] strRcv,int nCmdIdx,bool bWait,params object[] param)
        {
            strRcv = new string[] { "", "", "", "", "", "" };

            if (!this.tcplink.IsOpen)
            {
                this.tcplink.Open();
            }
            this.tcplink.ClearBuffer();

            string strSend = GetSendString((Command)nCmdIdx, param);
            this.tcplink.WriteLine(strSend);
            Logger.Process(this.station == null ? "" : this.station.Name, $"->[{this.tcplink.m_strName}] {strSend}");


            if (!bWait)
            {
                return true;
            }
            bool bResult = WaitReplyAndProcess(out strRcv, nCmdIdx);
            if (!bResult)
            {
                return false;
            }
            return true;

        }
        /// <summary>
        /// 等待机械手回复并解析
        /// </summary>
        /// <param name="strDatas"></param>
        /// <param name="nCmdIdx"></param>
        /// <returns></returns>
        public bool WaitReplyAndProcess(out string[] strDatas,int nCmdIdx)
        {
            strDatas = new string[] { "", "", "", "", "", "" };
            if (!Recevie(out strDatas,""))
            {
                return false;
            }
            AnalyzeRcvString((Command)nCmdIdx);
            return true;
        }
        /// <summary>
        /// TCP接收数据
        /// </summary>
        /// <param name="strDatas"></param>
        /// <param name="strStartWish"></param>
        /// <param name="strEndWish"></param>
        /// <param name="nTimeOut"></param>
        /// <returns></returns>
        private bool Recevie(out string[] strDatas,string strStartWish,string strEndWish="",int nTimeOut=0)
        {
            string strTemp = "";
            strDatas = new string[] { };

            if (this.station.WaitTcp (tcplink ,out strTemp,strStartWish,strEndWish,nTimeOut,false))
            {
                string strRcvOrg = strTemp;
                strRobotRec = strTemp;
                string[] strReceive = strTemp.Split(',');
                RePlay = strReceive;
                for (int i = 0; i < strReceive.Length; i++)
                {
                    strReceive[i] = strReceive[i].Trim();
                }
                strDatas = strReceive;
                return true; 
            }
            else
            {
                strDatas = new string[] { "", "", "", "","","" };
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
                case Command.初始化:
                case Command.取料:
                case Command.拍照:
                    result = GetRcvInt(1) == 1;
                    break;

            }
            if (!GetSendString(nCmdIdx).Contains(RePlay[0]))
            {
                EventMgr.Error($"发送指令{GetSendString(nCmdIdx)}和回复{strRobotRec}，不一致！".txtTrans(TransType.Mark));
            }
            return true;
        }


    }
}
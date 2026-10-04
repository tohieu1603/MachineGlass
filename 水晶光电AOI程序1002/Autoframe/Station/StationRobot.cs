using AF.Card;
using AF.Common;
using AF.Data;
using AF.Station;
using Microsoft.VisualBasic.Logging;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using AF.Communicate;
using AF.File;
using System.Xml.Linq;
using static AF.Data.Register;
using System.Collections;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Windows.Interop;
using ToolEx;

namespace Autoframe
{
    public class StationRobot : StationEx
    {
        TcpCommu_Robot tcpCommu_Robot = new TcpCommu_Robot();
        InoRobot LinkRobot = new InoRobot(TCP_Client.机械手.GetTcp().m_strIP);
        TcpCommu_CCD7 CCD7 = new TcpCommu_CCD7();
        TcpCommu_CCD8 CCD8 = new TcpCommu_CCD8();
        double CCD7ToRobot = 0;
        int CoverCount = 0;
        int CoverCountFlie = 0;
        string[] RobotRcv = new string[] { };
        string[] VisionRcv7 = new string[] { };
        string[] VisionRcv8 = new string[] { };
        int P;
        private bool RobotResut = false;
        private bool RingChanged = false;
        private int RobotputNUM = 0;
        private int RobotputNUMng = 0;
        Double X, Y, U;
        ProductData productRobot = new ProductData();
        /// <summary>
        /// 连续NG数量
        /// </summary>
        private int ContinuousNG = 0;
        /// <summary>
        /// 连续NG原因
        /// </summary>
        private List<string> NGReasons = new List<string>();
        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public StationRobot(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<NEST, ProductData>();
            //LoadContainer<NEST, ProductInfo>();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { DI.机械手运行, DI.机械手报警, DI.机械手启动完成, DI.机械手停止 };
            //配置站位界面显示输出
            arrayDO = new object[] { DO.机械手停止, DO.机械手启动, DO.机械手复位, DO.机械手清除报警 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { };
            //配置站位轴
            arrayAxis = new object[] { };
        }

        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {

        }

        /// <summary>
        /// 产品穴位，与产品是一对一
        /// </summary>
        public enum NEST
        {
            吸嘴1,
            吸嘴2,
            吸嘴3,
            吸嘴4
        }
        public override void OnStop()
        {
        }
        public override void OnPause()
        {
        }
        public override void OnResume()
        {
        }

        /// <summary>
        /// 初始化时设置站位安全状态，用来设备初始化时各站位之间相互卡站，
        /// 控制各站位先后动作顺序用，比如流水线组装，肯定需要组装的Z轴升
        /// 起后，流水线才能动作这种情况
        /// </summary>
        public override void Preinit()
        {
            foreach (RegBit item in Enum.GetValues(typeof(RegBit)))
            {
                item.Set(false);
            }
            RegBit.强制OK.Set(false);
            RegBit.机械手初始化完成.Set(false);
            RegBit.机械手通知AR检测取料完成.Set(false);
            RegBit.机械手通知OK覆膜放料完成.Set(false);
            RegBit.机械手通知NG覆膜放料完成.Set(false);
            switch (Process)
            {
                case AutoProcess.生产:
                case AutoProcess.空跑:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "生产流程开始";
                    break;
                case AutoProcess.CCD7九点标定:
                case AutoProcess.CCD8九点标定:
                case AutoProcess.CCD7旋转标定:
                case AutoProcess.CCD8旋转标定:
                case AutoProcess.机械手下相机基准标定:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "标定选择入口";
                    break;
                case AutoProcess.机械手下相机动态:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "相机动态流程开始";
                    break;
                case AutoProcess.机械手下相机静态:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "相机静态流程开始";
                    break;
                case AutoProcess.CCD4标定:
                case AutoProcess.CCD5标定:
                case AutoProcess.CCD6静态:
                case AutoProcess.CCD6动态:
                case AutoProcess.AR2测试:
                case AutoProcess.AR1测试:
                case AutoProcess.OK覆膜一键出料:
                case AutoProcess.NG覆膜一键出料:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = StepEmpty;
                    break;
                default:
                    this.InitEntry = StepExit;
                    this.AutoEntry = StepEmpty;
                    break;
            }
        }

        //读取机械手点位
        List<RobotPosItem> RotPos = new List<RobotPosItem>();
        private int 单行数量 = 0;

        ProductData productData = new ProductData();
        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case "初始化流程开始":

                    //RingMonitor.ResetAll();
                    //RingMonitor.SetRingLayout(17, 19, 23, IsDryRun);
                    //for (int i = 0; i <100; i++)
                    //{

                    //    RingMonitor.OnResult(i, false, "");
                    //    Delay(100);
                    //}
                    //return StepExit;


                    //productData.Result = false;

                    //productData.ShowData();
                    //this[(NEST)0] = productData;
                    //this[(NEST)0].ShowData();
                    #region test
                    //ProductData productData1 = new ProductData();
                    //productData1.Result = true;
                    //this[(NEST)0].CT = "2123.3";
                    //this[(NEST)0].CCD3Sit = "2";
                    //this[(NEST)0].CCD6Sit = "2";
                    //this[(NEST)0] = productData1;
                    //this[(NEST)0].SN=DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    //this[(NEST)0].ShowData();
                    //this[(NEST)0].Result= false;
                    //this[(NEST)0].CT = "2123.3";
                    //this[(NEST)0].CCD3Sit = "1";
                    //this[(NEST)0].CCD6Sit= "1";
                    //this[(NEST)0].SN = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    //this[(NEST)0].ShowData();

                    //Statistics.UpdateMaterial(Material.Chassis, ThrowReason.Chassis_AOI检测结果NG);
                    //return StepExit;
                    #endregion
                    RingChanged = false;
                    //NGReasons = new List<string>() { "CCD3检测NG_12_tyuyty_CCD6检测NG_jjj_fff_", "CCD3检测NG_12_tyuyty_CCD6检测NG_jjj_fff_",
                    //    "CCD3检测NG_12_tyuyty_CCD6检测NG_jjj_fff_", "CCD3检测NG_12_tyuyty_CCD6检测NG_jjj_fff_", "CCD3检测NG_12_tyuyty_CCD6检测NG_4444_fff_" };
                    //string NGReasonStr = "CCD3检测NG_12_tyuyty_CCD6检测NG_jjj_fff_".Replace("CCD3检测NG", "").Replace("CCD6检测NG", "");
                    //string[] strAlls = NGReasonStr.Split('_');
                    //List<string> backStr = new List<string>();
                    //for (int i = 0; i < strAlls.Length; i++)
                    //{
                    //    for (int j = NGReasons.Count - Parameter.GetInt("NGNums"); j < NGReasons.Count; j++)
                    //    {
                    //        if (!NGReasons[j].Contains(strAlls[i]) || strAlls[i]=="")
                    //        {
                    //            break;
                    //        }
                    //        if (j >= NGReasons.Count - 1)
                    //        {
                    //            backStr.Add(strAlls[i]);
                    //        }
                    //    }
                    //}
                    //if (backStr.Count > 0)
                    //{
                    //    string msg = string.Join(",", backStr);
                    //}
                    //StationMgr.Stop();
                    //return StepExit;
                    //bool[] aaa=new bool[] {false, true,false, true };
                    //List<int> OKIndex = new List<int>();
                    //int OKindexs = 0;

                    //List<int> NGIndex = new List<int>();
                    //int NGindexs = 0;

                    //for (int i = 0; i < 4; i++)
                    //{
                    //    if (aaa[i])
                    //    {
                    //        OKIndex.Add(i);
                    //        OKindexs++;
                    //    }
                    //    else
                    //    {
                    //        NGIndex.Add(i);
                    //        NGindexs++;
                    //    }
                    //}
                    //for (int i = 0; i < OKIndex.Count; i++)
                    //{
                    //    NGIndex.Add( OKIndex[i]);
                    //}


                    RobotputNUM = 0;
                    单行数量 = Parameter.GetInt("覆膜单行数量");
                    Logger.Process(this.Name, $"int");
                    this.tcpCommu_Robot.Init(this);
                    //if (!this.tcpCommu_Robot.Init(this, 0))
                    //    return "初始化流程开始";
                    //else
                    //    Logger.Process(Name, "Robot连接成功".txtTrans(TransType.Trans));

                    Delay(3000);
                    Logger.Process(this.Name, $"connect");
                    for (int i = 0; i < 2; i++)
                    {
                        if (LinkRobot.Connect())
                            break;
                    }


                    Logger.Process(this.Name, $"ClaerError");
                    Delay(300);
                    LinkRobot.ResetError();

                    Delay(200);
                    Logger.Process(this.Name, $"reset");
                    LinkRobot.ResetError();

                    Delay(200);
                    Logger.Process(this.Name, $"servoOn");
                    LinkRobot.servoON();

                    Delay(200);
                    Logger.Process(this.Name, $"switchAuto");
                    LinkRobot.switchAutoMode();

                    Delay(200);
                    Logger.Process(this.Name, $"start");
                    LinkRobot.start();
                    Delay(2000);

                    RotPos = new List<RobotPosItem>();

                    //给机械手发送初始化指令，空跑发0，正常流程发1
                    if (this.tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.初始化, true, IsDryRun ? 0 : Parameter.GetInt("产品类型")))
                    {

                        return "点位校验";
                    }
                    else
                        RegBit.机械手初始化完成.Set(false);
                    ShowMessage("机械手初始化失败，请检查连接!".txtTrans(TransType.Trans));
                    return StepExit;

                case "点位校验":
                    if (!IsDryRun)
                    {
                        //读取csv文件点位
                        List<RobotPosItem> csvPos = new List<RobotPosItem>();
                        csvPos = GetCsvPos(Program.csvRobotPos_Path);

                        ////读取机械手点位
                        //List<RobotPosItem> RotPos = new List<RobotPosItem>();
                        for (int i = 0; i < 2; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    this.tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.获取取料位, true);
                                    RotPos.Add(new RobotPosItem(RobotRcv[0], Convert.ToDouble(RobotRcv[1]), Convert.ToDouble(RobotRcv[2]), Convert.ToDouble(RobotRcv[3])));
                                    break;
                                case 1:
                                    this.tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.获取拍照位, true);
                                    RotPos.Add(new RobotPosItem(RobotRcv[0], Convert.ToDouble(RobotRcv[1]), Convert.ToDouble(RobotRcv[2]), Convert.ToDouble(RobotRcv[3])));
                                    break;
                                default:
                                    break;
                            }
                        }
                        //比较点位
                        if (!compareRobotPos(RotPos, csvPos))
                        {
                            int Rotsel = ShowMessage("机械手点位与本地文件不一致，请检查".txtTrans(TransType.Trans), "报警提示".txtTrans(TransType.Trans), new string[] { "退出".txtTrans(TransType.Trans), "已经确认，重新写入本地文件".txtTrans(TransType.Trans) });
                            if (Rotsel == 0)
                            {
                                RegBit.机械手初始化完成.Set(false);
                            }
                            else
                            {
                                //重写文件
                                overWriteRotPos(RotPos, Program.csvRobotPos_Path);
                            }
                        }
                        return "回待机位";
                    }  //判断是不是空跑模式
                    else
                        return "回待机位";

                case "回待机位":
                    tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.待机位, true, AxisHandle.SpeedPercent, 单行数量);
                    string[] RobotIO = new string[] {"","",};
                    switch (RobotRcv[1])
                    {
                        case "0":
                            ShowMessage("手动检查机械手气缸缩回状态".txtTrans(TransType.Trans));
                            return "回待机位";
                            break;
                        case "1":
                            if (this[NEST.吸嘴1] != null && RobotRcv[2]=="1")
                            {
                                RegBit.机械手初始化吸嘴1有料.Set(true);
                            }
                            else
                            {
                                //this[NEST.吸嘴1] = null;
                                tcpCommu_Robot.SendCmd(out RobotIO, (int)TcpCommu_Robot.Command.DO控制, true, 4, 0);
                                tcpCommu_Robot.SendCmd(out RobotIO, (int)TcpCommu_Robot.Command.DO控制, true, 8, 1);
                                Delay(200);
                                tcpCommu_Robot.SendCmd(out RobotIO, (int)TcpCommu_Robot.Command.DO控制, true, 8, 0);
                            }
                            if (this[NEST.吸嘴2] != null && RobotRcv[3] == "1")
                            {
                                RegBit.机械手初始化吸嘴2有料.Set(true);
                            }
                            else
                            {
                                //this[NEST.吸嘴2] = null;
                                tcpCommu_Robot.SendCmd(out RobotIO, (int)TcpCommu_Robot.Command.DO控制, true, 5, 0);
                                tcpCommu_Robot.SendCmd(out RobotIO, (int)TcpCommu_Robot.Command.DO控制, true, 9, 1);
                                Delay(200);
                                tcpCommu_Robot.SendCmd(out RobotIO, (int)TcpCommu_Robot.Command.DO控制, true, 9, 0);
                            }
                            if (this[NEST.吸嘴3] != null && RobotRcv[4] == "1")
                            {
                                RegBit.机械手初始化吸嘴3有料.Set(true);
                            }
                            else
                            {
                                //this[NEST.吸嘴3] = null;
                                tcpCommu_Robot.SendCmd(out RobotIO, (int)TcpCommu_Robot.Command.DO控制, true, 6, 0);
                                tcpCommu_Robot.SendCmd(out RobotIO, (int)TcpCommu_Robot.Command.DO控制, true, 10, 1);
                                Delay(200);
                                tcpCommu_Robot.SendCmd(out RobotIO, (int)TcpCommu_Robot.Command.DO控制, true, 10, 0);
                            }
                            if (this[NEST.吸嘴4] != null && RobotRcv[5] == "1")
                            {
                                RegBit.机械手初始化吸嘴4有料.Set(true);
                            }
                            else
                            {
                                //this[NEST.吸嘴4] = null;
                                tcpCommu_Robot.SendCmd(out RobotIO, (int)TcpCommu_Robot.Command.DO控制, true, 7, 0);
                                tcpCommu_Robot.SendCmd(out RobotIO, (int)TcpCommu_Robot.Command.DO控制, true, 11, 1);
                                Delay(200);
                                tcpCommu_Robot.SendCmd(out RobotIO, (int)TcpCommu_Robot.Command.DO控制, true, 11, 0);
                            }
                            break;
                        //case "2":
                        //    if(this[NEST.吸嘴1]!=null && this[NEST.吸嘴2] != null&&this[NEST.吸嘴3] != null && this[NEST.吸嘴4] != null)
                        //    {
                        //        RegBit.机械手初始化有料.Set(true);
                        //        return "连接相机";
                        //    }
                        //    ShowMessage("手动取下机械手上物料".txtTrans(TransType.Trans));
                        //    tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.DO控制, true, 4, 0);
                        //    tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.DO控制, true, 5, 0);
                        //    tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.DO控制, true, 6, 0);
                        //    tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.DO控制, true, 7, 0);
                        //    break;
                        case "3":
                            ShowMessage("检查机械手上吸嘴真空".txtTrans(TransType.Trans));
                            break;
                        default:
                            break;
                    }
                    return "连接相机";

                case "连接相机":
                    CCD7.Init(this, 7);
                    CCD8.Init(this, 8);
                    return "初始化完成";



                case "初始化完成":
                    单行数量 = Parameter.GetInt("覆膜单行数量");
                    //CCD7ToRobot = Parameter.GetDouble("机械手视觉引导极限值");
                    RegBit.机械手初始化完成.Set(true);
                    //ResetCarrierCounter();
                    CT_TimeStart = DateTime.Now;
                    //结束初始化
                    return StepExit;

                default:
                    return null;
            }
        }
        /// <summary>
        /// 时间开始
        /// </summary>
        private DateTime CT_TimeStart = DateTime.Now;
        /// <summary>
        /// 时间结束
        /// </summary>
        private DateTime CT_TimeEnd = DateTime.Now;
        private double 取料时长 = 0;
        List<int> OKIndex = new List<int>();
        List<int> NGIndex = new List<int>();
        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case "生产流程开始":
                    if (RegBit.机械手初始化吸嘴1有料.Get() || RegBit.机械手初始化吸嘴2有料.Get() || RegBit.机械手初始化吸嘴3有料.Get() || RegBit.机械手初始化吸嘴4有料.Get())
                    {

                        return "初始化机械手到拍照位";
                    }
                    //AddCarrier();
                    //ReduceCarrier();
                    return "等待AR面可取料";

                case "等待AR面可取料":
                    WaitRegBit(RegBit.AR检测通知机械手有料, true);
                    RegBit.AR检测通知机械手有料.Set(false);
                    取料时长 = 0;
                    return "机械手取料";

                case "机械手取料":

                    bool RobotPic = this.tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.取料, true, 100, Convert.ToInt32(RegBit.AR通知机械手穴位1有料.Get()),
                        Convert.ToInt32(RegBit.AR通知机械手穴位2有料.Get()), Convert.ToInt32(RegBit.AR通知机械手穴位3有料.Get()), Convert.ToInt32(RegBit.AR通知机械手穴位4有料.Get()));


                    if (RobotPic)
                    {
                        if (RobotRcv[1] == "1")
                        {

                            RegBit.机械手通知AR关闭真空.Set(true);
                            return "机械手到拍照位";
                        }
                        //if (RegBit.IR载具清料.Get())
                        //{
                        //    取料时长 += 3;
                        //    RegBit.机械手通知AR检测取料完成.Set(true);
                        //    return "机械手到拍照位";
                        //}
                        //将AR面四个产品的数据传递过来
                        //this[0].GetProductData=
                        int ret = ShowMessage("取料失败！".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "重新取料".txtTrans(TransType.Trans), "放料".txtTrans(TransType.Trans) });
                        取料时长 += Duration;
                        if (ret == 0)
                        {
                            return "机械手取料";
                        }

                        return "机械手到拍照位";
                    }
                    else
                    {
                        int ret = ShowMessage("取料失败！".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "确认".txtTrans(TransType.Trans), "重新取料".txtTrans(TransType.Trans) });
                        取料时长 += Duration;
                        if (ret == 0)
                        {
                            return StepEmpty;
                        }
                        return "机械手取料";
                    }
                case "初始化机械手到拍照位":
                    bool RObot0 = this.tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.拍照, true, 100,Convert.ToInt32(RegBit.机械手初始化吸嘴1有料.Get()), 
                        Convert.ToInt32(RegBit.机械手初始化吸嘴2有料.Get()), Convert.ToInt32(RegBit.机械手初始化吸嘴3有料.Get()), Convert.ToInt32(RegBit.机械手初始化吸嘴4有料.Get()));
                    RegBit.机械手初始化吸嘴1有料.Set(false);
                    RegBit.机械手初始化吸嘴2有料.Set(false);
                    RegBit.机械手初始化吸嘴3有料.Set(false);
                    RegBit.机械手初始化吸嘴4有料.Set(false);
                    if (RObot0)
                    {
                        if (RobotRcv[1] == "1")
                        {
                            return "拍照定位";
                        }
                        else
                        {
                            int ret = ShowMessage("机械手取料吸真空失败！".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "再次判断".txtTrans(TransType.Trans), "忽略".txtTrans(TransType.Trans) });
                            取料时长 += Duration;
                            if (ret == 0)
                            {
                                return "初始化机械手到拍照位";
                            }
                            return "拍照定位";
                        }
                    }
                    else
                    {
                        int ret = ShowMessage("机械手到拍照位失败！".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "确认".txtTrans(TransType.Trans), "再次发送".txtTrans(TransType.Trans) });
                        取料时长 += Duration;
                        if (ret == 0)
                        {
                            return StepEmpty;
                        }
                        return "初始化机械手到拍照位";
                    }

                case "机械手到拍照位":
                    WaitRegBit(RegBit.AR检测通知机械手真空已关闭, true);
                    RegBit.AR检测通知机械手真空已关闭.Set(false);
                    bool RObot = this.tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.拍照, true, 100, Convert.ToInt32(RegBit.AR通知机械手穴位1有料.Get()),
                        Convert.ToInt32(RegBit.AR通知机械手穴位2有料.Get()), Convert.ToInt32(RegBit.AR通知机械手穴位3有料.Get()), Convert.ToInt32(RegBit.AR通知机械手穴位4有料.Get()));
                    if (RObot)
                    {
                        if (RobotRcv[1] == "1")
                        {
                            RegBit.机械手通知AR检测取料完成.Set(true);
                            RegBit.AR通知机械手穴位1有料.Set(false);
                            RegBit.AR通知机械手穴位2有料.Set(false);
                            RegBit.AR通知机械手穴位3有料.Set(false);
                            RegBit.AR通知机械手穴位4有料.Set(false);
                            return "拍照定位";
                        }
                        else
                        {
                            int ret = ShowMessage("机械手取料吸真空失败！".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "重取".txtTrans(TransType.Trans), "忽略".txtTrans(TransType.Trans) });
                            取料时长 += Duration;
                            if (ret == 0)
                            {
                                RegBit.AR检测通知机械手真空已关闭.Set(true);               //=================重取用===============
                                return "机械手取料";
                            }
                            RegBit.机械手通知AR检测取料完成.Set(true);
                            RegBit.AR通知机械手穴位1有料.Set(false);
                            RegBit.AR通知机械手穴位2有料.Set(false);
                            RegBit.AR通知机械手穴位3有料.Set(false);
                            RegBit.AR通知机械手穴位4有料.Set(false);
                            return "拍完照去过度位";
                        }
                    }
                    else
                    {
                        int ret = ShowMessage("机械手到拍照位失败！".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "确认".txtTrans(TransType.Trans), "再次发送".txtTrans(TransType.Trans) });
                        取料时长 += Duration;
                        if (ret == 0)
                        {
                            return StepEmpty;
                        }
                        return "机械手到拍照位";
                    }

                case "拍照定位":
                    if (!IsDryRun)
                    {
                        CCD7.SendCmd(out VisionRcv7, (int)TcpCommu_CCD7.Command.CCD7_下料产品引导, false, RotPos[1].X, RotPos[1].Y, RotPos[1].U);
                        CCD8.SendCmd(out VisionRcv8, (int)TcpCommu_CCD8.Command.CCD8_下料产品引导, false, RotPos[1].X, RotPos[1].Y, RotPos[1].U);
                        DO.CCD7触发.Set(true);
                        //Thread.Sleep(20);

                        string[] CCDresult = new string[] { };
                        CCD7.WaitReplyAndProcess(out CCDresult, (int)TcpCommu_CCD7.Command.CCD7_下料产品引导);
                        DO.CCD7触发.Set(false);
                        if (!AllNestCheck())
                        {
                            if (CCD7.resultn.Contains(false))
                            {
                                if (RegBit.IR载具清料.Get())
                                {
                                    return "拍完照去过度位";
                                }

                                string CCD7Error = null;
                                for (int i = 0; i < 4; i++)
                                {
                                    if (!CCD7.resultn[i])
                                    {
                                        CCD7Error += $"吸嘴{i + 1}NG；";
                                    }
                                }
                                int ret = ShowMessage(("CCD7拍照：" + CCD7Error).txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "忽略".txtTrans(TransType.Trans), "重拍".txtTrans(TransType.Trans) });
                                取料时长 += Duration;
                                if (ret == 0)
                                {
                                    return "拍完照去过度位";
                                }
                                return "拍照定位";
                            }
                            //bool isOutLimit = AllNestCheck();
                            //if (isOutLimit)
                            //{
                            //    int sel = ShowMessage("吸嘴偏移超出限制,请检查确认！", "报警", new string[] { "确认OK，继续组装", "再次拍照" });
                            //    if (sel == 0) return "计算判断";
                            //    return "拍照定位";
                            //}

                            return "拍完照去过度位";
                        }
                        else
                        {
                            int ret = ShowMessage("拍照定位引导超限或者拍照失败！".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "忽略".txtTrans(TransType.Trans), "重拍".txtTrans(TransType.Trans) });
                            取料时长 += Duration;
                            if (ret == 0)
                            {
                                return "拍完照去过度位";
                            }
                            return "拍照定位";

                        }
                    }
                    else
                        Delay(100);
                        return "拍完照去过度位";

                case "拍完照去过度位":
                    RingChanged = false;
                    OKIndex = new List<int>();
                    NGIndex = new List<int>();
                    for (int i = 0; i < 4; i++)
                    {
                        if (this[(NEST)i].UpRingTime != RegString.UpRingTime.Get())
                        {
                            RingChanged = true;
                        }
                        if (this[(NEST)i].Result || RegBit.强制OK.Get())//here
                        {
                            OKIndex.Add(i);
                        }
                        else
                        {
                            NGIndex.Add(i);
                        }
                    }
                    for (int i = 0; i < NGIndex.Count; i++)
                    {
                        OKIndex.Add(NGIndex[i]);
                    }
                    this.tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.过度位, true, 100);
                    return "计算判断";
                case "计算判断":
                    //机械手空跑流程
                    //if (IsDryRun)
                    //{
                    //    for (int i = 0; i < 2; i++)
                    //    {
                    //        int OKCount = (int.Parse(Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount", "")) % 单行数量) + 1;
                    //        int OKCountFile = (int.Parse(Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount", ""))) + 1;
                    //        WaitRegBit(RegBit.OK覆膜通知机械手可放料, true);
                    //        RegBit.OK覆膜通知机械手可放料.Set(false);
                    //        // 随便取吸嘴1指令，空跑不区分视觉坐标
                    //        tcpCommu_Robot.SendCmd(out RobotRcv, i == 0 ? (int)TcpCommu_Robot.Command.吸嘴1OK覆膜放料 : (int)TcpCommu_Robot.Command.吸嘴2OK覆膜放料, true, 100, OKCount, 0, 0, 0);
                    //        Ini.WriteValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount", OKCountFile.ToString());
                    //        RegBit.机械手通知OK覆膜放料完成.Set(true);
                    //        Delay(500);
                    //    }
                    //    for (int i = 0; i < 2; i++)
                    //    {
                    //        int NGCount = (int.Parse(Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", "")) % 单行数量) + 1;
                    //        int NGCountFile = (int.Parse(Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", ""))) + 1;
                    //        WaitRegBit(RegBit.NG覆膜通知机械手可放料, true);
                    //        RegBit.NG覆膜通知机械手可放料.Set(false);
                    //        tcpCommu_Robot.SendCmd(out RobotRcv, i == 0 ? (int)TcpCommu_Robot.Command.吸嘴3NG覆膜放料 : (int)TcpCommu_Robot.Command.吸嘴4NG覆膜放料, true, 100, NGCount, 0, 0, 0);
                    //        Ini.WriteValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", NGCountFile.ToString());
                    //        RegBit.机械手通知NG覆膜放料完成.Set(true);
                    //        Delay(500);
                    //    }
                    //    return "生产流程结束";
                    //}

                    单行数量 = Parameter.GetInt("覆膜单行数量");

                    if (this[(NEST)OKIndex[RobotputNUM]].Result || RegBit.强制OK.Get())     //判断产品的结果  this[(NEST)RobotputNUM].Result//here
                    {

                        //this[(NEST)i].DtEndTime = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss,fff"));

                        //从文件获取OK覆膜当前产品的个数，将个数除以一排放的产品数量，得到余数就是当前的列
                        CoverCount = (int.Parse(Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount", "")) % 单行数量) + 1;
                        Logger.Process(this.Name, $"4{DateTime.Now.ToString("yyyyMMdd_HH_mm_ss.fff")}");
                        if (CoverCount != 1)
                        {
                            WaitRegBit(RegBit.OK覆膜通知机械手可放料, true);
                            RegBit.OK覆膜通知机械手可放料.Set(false);
                        }

                        //去OK覆膜
                        switch (OKIndex[RobotputNUM])    // 判断具体的吸嘴
                        {
                            case 0:
                                P = (int)TcpCommu_Robot.Command.吸嘴1OK覆膜放料;
                                X = CCD7.d下料引导位置X[OKIndex[RobotputNUM]];
                                Y = CCD7.d下料引导位置Y[OKIndex[RobotputNUM]];
                                U = CCD7.d下料引导位置U[OKIndex[RobotputNUM]];
                                break;
                            case 1:
                                P = (int)TcpCommu_Robot.Command.吸嘴2OK覆膜放料;
                                X = CCD7.d下料引导位置X[OKIndex[RobotputNUM]];
                                Y = CCD7.d下料引导位置Y[OKIndex[RobotputNUM]];
                                U = CCD7.d下料引导位置U[OKIndex[RobotputNUM]];
                                break;
                            case 2:
                                P = (int)TcpCommu_Robot.Command.吸嘴3OK覆膜放料;
                                X = CCD7.d下料引导位置X[OKIndex[RobotputNUM]];
                                Y = CCD7.d下料引导位置Y[OKIndex[RobotputNUM]];
                                U = CCD7.d下料引导位置U[OKIndex[RobotputNUM]];
                                break;
                            case 3:
                                P = (int)TcpCommu_Robot.Command.吸嘴4OK覆膜放料;
                                X = CCD7.d下料引导位置X[OKIndex[RobotputNUM]];
                                Y = CCD7.d下料引导位置Y[OKIndex[RobotputNUM]];
                                U = CCD7.d下料引导位置U[OKIndex[RobotputNUM]];
                                break;
                        }



                    }
                    else    //产品NG，机械手到NG覆膜工站
                    {


                        //this[(NEST)i].DtEndTime = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss.fff"));
                        switch (OKIndex[RobotputNUM])
                        {
                            case 0:
                                P = (int)TcpCommu_Robot.Command.吸嘴1NG覆膜放料;
                                X = CCD7.d下料引导位置X[OKIndex[RobotputNUM]];
                                Y = CCD7.d下料引导位置Y[OKIndex[RobotputNUM]];
                                U = CCD7.d下料引导位置U[OKIndex[RobotputNUM]];
                                break;
                            case 1:
                                P = (int)TcpCommu_Robot.Command.吸嘴2NG覆膜放料;
                                X = CCD7.d下料引导位置X[OKIndex[RobotputNUM]];
                                Y = CCD7.d下料引导位置Y[OKIndex[RobotputNUM]];
                                U = CCD7.d下料引导位置U[OKIndex[RobotputNUM]];
                                break;
                            case 2:
                                P = (int)TcpCommu_Robot.Command.吸嘴3NG覆膜放料;
                                X = CCD7.d下料引导位置X[OKIndex[RobotputNUM]];
                                Y = CCD7.d下料引导位置Y[OKIndex[RobotputNUM]];
                                U = CCD7.d下料引导位置U[OKIndex[RobotputNUM]];
                                break;
                            case 3:
                                P = (int)TcpCommu_Robot.Command.吸嘴4NG覆膜放料;
                                X = CCD7.d下料引导位置X[OKIndex[RobotputNUM]];
                                Y = CCD7.d下料引导位置Y[OKIndex[RobotputNUM]];
                                U = CCD7.d下料引导位置U[OKIndex[RobotputNUM]];
                                break;
                        }
                        //将机械手放产品的个数除以一排放的产品数量，得到余数就是当前的列
                        CoverCount = (int.Parse(Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", "")) % 单行数量) + 1;
                        if (CoverCount != 1)
                        {
                            WaitRegBit(RegBit.NG覆膜通知机械手可放料, true);
                            RegBit.NG覆膜通知机械手可放料.Set(false);
                        }


                    }

                    return "机械手去放料位";
                case "机械手去放料位":
                    if (CoverCount == 1)
                    {

                        if (this[(NEST)OKIndex[RobotputNUM]].Result || RegBit.强制OK.Get())
                        {
                            WaitRegBit(RegBit.OK覆膜通知机械手可放料, true);
                            RegBit.OK覆膜通知机械手可放料.Set(false);
                        }
                        else
                        {
                            WaitRegBit(RegBit.NG覆膜通知机械手可放料, true);
                            RegBit.NG覆膜通知机械手可放料.Set(false);
                        }
                        RobotResut = tcpCommu_Robot.SendCmd(out RobotRcv, P, true, 100, CoverCount, X, Y, U, 0);
                    }
                    else
                    {
                        RobotResut = tcpCommu_Robot.SendCmd(out RobotRcv, P, false, 100, CoverCount, X, Y, U, 0);
                        if (CoverCount < 单行数量 && CoverCount != 1)
                        {
                            if (this[(NEST)OKIndex[RobotputNUM]].Result || RegBit.强制OK.Get())//here
                            {
                                RegBit.机械手通知OK覆膜放料完成.Set(true);
                            }
                            else
                            {
                                RegBit.机械手通知NG覆膜放料完成.Set(true);
                            }
                        }
                        RobotResut = tcpCommu_Robot.WaitReplyAndProcess(out RobotRcv, P);
                    }

                    if (RobotResut)
                    {
                        if (RobotRcv[1] == "2")
                        {
                            int ret = ShowMessage($"机械手回复，去{this[(NEST)RobotputNUM].Result}覆膜{RobotputNUM + 1}0,吸嘴有料".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "继续".txtTrans(TransType.Trans), "重放".txtTrans(TransType.Trans) });
                            取料时长 += Duration;
                            if (ret == 1)
                            {
                                return "机械手去放料位";
                            }

                        }
                        if (!RingChanged)
                        {
                            RingMonitor.OnResult(this[(NEST)OKIndex[RobotputNUM]].PianPos, this[(NEST)OKIndex[RobotputNUM]].Result, this[(NEST)OKIndex[RobotputNUM]].NGReason);
                        }
                        if (this[(NEST)OKIndex[RobotputNUM]].Result || RegBit.强制OK.Get())//here
                        {
                            ContinuousNG = 0;
                            NGReasons = new List<string>();
                            CoverCountFlie = (int.Parse(Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount", ""))) + 1;
                            Ini.WriteValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount", CoverCountFlie.ToString()); //将当前产品的OKPos写入文件
                            this[(NEST)OKIndex[RobotputNUM]].OKPos = CoverCountFlie; //将当前产品的OKPos写入产品数据

                            if (CoverCount >= 单行数量 || CoverCount == 1)
                                RegBit.机械手通知OK覆膜放料完成.Set(true);
                        }
                        else
                        {
                            ContinuousNG++;
                            NGReasons.Add(this[(NEST)OKIndex[RobotputNUM]].NGReason);
                            CoverCountFlie = (int.Parse(Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", ""))) + 1;
                            Ini.WriteValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", CoverCountFlie.ToString()); //将当前产品的NGPos写入文件
                            this[(NEST)(OKIndex[RobotputNUM])].NGPos = CoverCountFlie;      //将当前产品的NGPos写入产品数据
                            int pos = this[(NEST)OKIndex[RobotputNUM]].PianPos;

                            if (CoverCount >= 单行数量 || CoverCount == 1)
                                RegBit.机械手通知NG覆膜放料完成.Set(true);
                            if (ContinuousNG >= Parameter.GetInt("NGNums"))
                            {
                                bool CheckRst = CheckNGreason(this[(NEST)OKIndex[RobotputNUM]].NGReason, out string msg);
                                if (CheckRst)
                                {
                                    int ret = ShowMessage($"连续NG超过{ContinuousNG}个，原因：{msg}，请检查".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans),
                                               new string[] { "忽略".txtTrans(TransType.Trans), "清载具".txtTrans(TransType.Trans) });
                                    取料时长 += Duration;
                                    ContinuousNG = 0;
                                    NGReasons = new List<string>();
                                    if (ret == 1)
                                    {
                                        RegBit.机械手申请清载具.Set(true);
                                    }
                                }
                            }
                        }
                        RobotputNUM++;
                        if (RobotputNUM < 4)
                            return "计算判断";
                        RobotputNUM = 0;
                        //if (this[(NEST)OKIndex[3]].Result || !RegBit.AR检测通知机械手有料.Get())
                        //{
                        //    return "机械手去待机位";
                        //}
                        //return "生产流程结束";
                        return "机械手去待机位";

                    }
                    else
                    {
                        int ret = ShowMessage($"机械手回复，去{this[(NEST)RobotputNUM].Result}覆膜{RobotputNUM + 1}0,失败".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans),
                            new string[] { "忽略".txtTrans(TransType.Trans), "回待机位".txtTrans(TransType.Trans) });
                        取料时长 += Duration;
                        if (ret == 1)
                        {
                            return "机械手去待机位";
                        }
                    }
                    return "生产流程结束";
                case "机械手去待机位":
                    RobotResut = this.tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.回过度位, true, 100);
                    if (!RobotResut)
                    {
                        int ret = ShowMessage($"机械手回待机位,失败".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "退出".txtTrans(TransType.Trans), "重新发送".txtTrans(TransType.Trans) });
                        取料时长 += Duration;
                        if (ret == 1)
                        {
                            return StepExit;
                        }
                        else
                        {
                            return "机械手去待机位";
                        }
                    }
                    return "数据传递";
                case "数据传递":
                    if(!RegBit.DataUpdata向机械手要料.Get())
                        return "数据传递";
                    RegBit.DataUpdata向机械手要料.Set(false);
                    this[NEST.吸嘴1].DtEndTime = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss.fff"));
                    this[NEST.吸嘴2].DtEndTime = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss.fff"));
                    this[NEST.吸嘴3].DtEndTime = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss.fff"));
                    this[NEST.吸嘴4].DtEndTime = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss.fff"));
                    "数据更新".GetStation<DataUpdata>()[DataUpdata.NEST.穴位1] = this[NEST.吸嘴1];
                    "数据更新".GetStation<DataUpdata>()[DataUpdata.NEST.穴位2] = this[NEST.吸嘴2];
                    "数据更新".GetStation<DataUpdata>()[DataUpdata.NEST.穴位3] = this[NEST.吸嘴3];
                    "数据更新".GetStation<DataUpdata>()[DataUpdata.NEST.穴位4] = this[NEST.吸嘴4];
                    Logger.Process(this.Name, $"{Name},穴位1，结果：{this[NEST.吸嘴1].Result}，NG原因：{this[NEST.吸嘴1].NGReason}，BreakDown:{this[NEST.吸嘴1].BreakDownTime}");
                    Logger.Process(this.Name, $"{Name},穴位2，结果：{this[NEST.吸嘴2].Result}，NG原因：{this[NEST.吸嘴2].NGReason}，BreakDown:{this[NEST.吸嘴2].BreakDownTime}");
                    Logger.Process(this.Name, $"{Name},穴位3，结果：{this[NEST.吸嘴3].Result}，NG原因：{this[NEST.吸嘴3].NGReason}，BreakDown:{this[NEST.吸嘴3].BreakDownTime}");
                    Logger.Process(this.Name, $"{Name},穴位4，结果：{this[NEST.吸嘴4].Result}，NG原因：{this[NEST.吸嘴4].NGReason}，BreakDown:{this[NEST.吸嘴4].BreakDownTime}");
                    this[NEST.吸嘴1] = null;
                    this[NEST.吸嘴2] = null;
                    this[NEST.吸嘴3] = null;
                    this[NEST.吸嘴4] = null;
                    return "生产流程结束";
                case "生产流程结束":
                    RegBit.机械手通知DataUpdata有料.Set(true);
                    //CT_TimeEnd = DateTime.Now;
                    //this[NEST.吸嘴1].CT = ((CT_TimeEnd - CT_TimeStart).TotalSeconds / 4).ToString();
                    //this[NEST.吸嘴2].CT = ((CT_TimeEnd - CT_TimeStart).TotalSeconds / 4).ToString();
                    //this[NEST.吸嘴3].CT = ((CT_TimeEnd - CT_TimeStart).TotalSeconds / 4).ToString();
                    //this[NEST.吸嘴4].CT = ((CT_TimeEnd - CT_TimeStart).TotalSeconds / 4).ToString();
                    //CT_TimeStart = DateTime.Now;
                    //RegDouble.CT.Set((CT_TimeEnd - CT_TimeStart).TotalSeconds / 4);
                    //this[NEST.吸嘴1].BreakDownTime += 取料时长;
                    //this[NEST.吸嘴2].BreakDownTime += 取料时长;
                    //this[NEST.吸嘴3].BreakDownTime += 取料时长;
                    //this[NEST.吸嘴4].BreakDownTime += 取料时长;
                    //bool only1 = true;
                    //for (int i = 0; i < 4; i++)
                    //{
                    //    if (RingChanged)
                    //    {
                    //        RingMonitor.ResetAll();
                    //        RingMonitor.SetRingLayout(RegInt.RowCCD1.Get(), RegInt.ColCCD1.Get(), RegInt.PickRingSite.Get(), IsDryRun);
                    //    }
                    //    else if (this[NEST.吸嘴4].PianPos >= this[NEST.吸嘴4].CCD1Row * this[NEST.吸嘴4].CCD1Col)
                    //    {
                    //        this[NEST.吸嘴1] = null;
                    //        this[NEST.吸嘴2] = null;
                    //        this[NEST.吸嘴3] = null;
                    //        this[NEST.吸嘴4] = null;
                    //        return "等待重新上料";
                    //    }
                    //    this[NEST.吸嘴1] = null;
                    //    this[NEST.吸嘴2] = null;
                    //    this[NEST.吸嘴3] = null;
                    //    this[NEST.吸嘴4] = null;
                    //    return "生产流程开始";
                    //case "等待重新上料":
                    //    if (RegBit.扩膜CCD1拍照完成状态.Get())
                    //    {
                    //        RegBit.扩膜CCD1拍照完成状态.Set(false) ;
                    //        RingMonitor.ResetAll();
                    //        RingMonitor.SetRingLayout(RegInt.RowCCD1.Get(), RegInt.ColCCD1.Get(),  RegInt.PickRingSite.Get(), IsDryRun);
                    //        return "生产流程开始";
                    //    }
                    //}


                    //Logger.Process(this.Name, $"{Name},穴位1，结果：{this[NEST.吸嘴1].Result}，NG原因：{this[NEST.吸嘴1].NGReason}，BreakDown:{this[NEST.吸嘴1].BreakDownTime}");
                    //Logger.Process(this.Name, $"{Name},穴位2，结果：{this[NEST.吸嘴2].Result}，NG原因：{this[NEST.吸嘴2].NGReason}，BreakDown:{this[NEST.吸嘴2].BreakDownTime}");
                    //Logger.Process(this.Name, $"{Name},穴位3，结果：{this[NEST.吸嘴3].Result}，NG原因：{this[NEST.吸嘴3].NGReason}，BreakDown:{this[NEST.吸嘴3].BreakDownTime}");
                    //Logger.Process(this.Name, $"{Name},穴位4，结果：{this[NEST.吸嘴4].Result}，NG原因：{this[NEST.吸嘴4].NGReason}，BreakDown:{this[NEST.吸嘴4].BreakDownTime}");
                    //if (this[(NEST)OKIndex[3]].NGReason.Contains("清料"))
                    //{
                    //    RegBit.IR载具清料.Set(false);
                    //    StationMgr.Pause();
                    //    Delay(200);
                    //    StationMgr.Stop();
                    //}
                    //this[NEST.吸嘴1] = null;
                    //this[NEST.吸嘴2] = null;
                    //this[NEST.吸嘴3] = null;
                    //this[NEST.吸嘴4] = null;
                    return "生产流程开始";
            }
            return null;
        }





        protected override string CalibRun(string strStep)
        {
            switch (strStep)
            {
                case "标定选择入口":
                    string tip = "";
                    switch (Process)
                    {
                        case AutoProcess.CCD7九点标定: tip = "当前：CCD7九点标定"; break;
                        case AutoProcess.CCD8九点标定: tip = "当前：CCD8九点标定"; break;
                        case AutoProcess.CCD7旋转标定: tip = "当前：CCD7旋转标定"; break;
                        case AutoProcess.CCD8旋转标定: tip = "当前：CCD8旋转标定"; break;
                        case AutoProcess.机械手下相机基准标定: tip = "当前：基准标定"; break;
                    }
                    ShowMessage(tip);
                    // 根据当前流程跳转对应标定起始步骤
                    switch (Process)
                    {
                        case AutoProcess.CCD7九点标定:
                            return "CCD7九点标定开始";
                        case AutoProcess.CCD8九点标定:
                            return "CCD8九点标定开始";
                        case AutoProcess.CCD7旋转标定:
                            return "CCD7旋转标定开始";
                        case AutoProcess.CCD8旋转标定:
                            return "CCD8旋转标定开始";
                        case AutoProcess.机械手下相机基准标定:
                            return "基准标定开始";
                        default:
                            return StepExit;
                    }


                case "CCD7九点标定开始":
                    Delay(10);
                    return "提示放标定片";

                case "提示放标定片":
                    for (int i = 4; i < 8; i++)
                    {
                        tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.DO控制, true, i, 1);
                    }
                    ShowMessage("请在吸嘴上放好标定片".txtTrans(TransType.Trans));
                    // ShowMessage("请将标定片放到指定位置");
                    return "机械手九点标定";

                case "机械手九点标定":
                    for (int i = 1; i <= 9; i++)
                    {
                        tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.CCD7_九点标定, true, AxisHandle.SpeedPercent, i);
                        Delay(200);
                        CCD7.SendCmd(out VisionRcv7, (int)TcpCommu_CCD7.Command.CCD7_九点标定, true, i.ToString(), Convert.ToDouble(RobotRcv[2]), Convert.ToDouble(RobotRcv[3]));
                    }
                    return "机械手到待机位";

                case "机械手到待机位":
                    tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.待机位, true, AxisHandle.SpeedPercent, "");
                    ShowMessage("CCD7九点标定完成".txtTrans(TransType.Trans));
                    int sel1 = ShowMessage("CCD7九点标定完成，是否继续做其他标定？".txtTrans(TransType.Trans), "标定完成".txtTrans(TransType.Trans), new string[] { "CCD7旋转标定".txtTrans(TransType.Trans), "结束标定流程".txtTrans(TransType.Trans) });
                    if (sel1 == 0)
                    {
                        // 选继续：回到标定选择入口，不重新初始化
                        return "CCD7旋转标定开始";
                    }
                    else
                    {
                        // 选结束：直接退出标定流程
                        return StepExit;
                    }


                case "CCD8九点标定开始":
                    Delay(10);
                    return "CCD8提示放标定片";

                case "CCD8提示放标定片":
                    for (int i = 4; i < 8; i++)
                    {
                        tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.DO控制, true, i, 1);
                    }
                    ShowMessage("请在吸嘴上放好标定片".txtTrans(TransType.Trans));
                    return "CCD8机械手九点标定";

                case "CCD8机械手九点标定":
                    for (int i = 1; i <= 9; i++)
                    {
                        tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.CCD8九点标定, true, AxisHandle.SpeedPercent, i);
                        Delay(200);
                        CCD8.SendCmd(out VisionRcv7, (int)TcpCommu_CCD8.Command.CCD8_九点标定, true, i.ToString(), Convert.ToDouble(RobotRcv[2]), Convert.ToDouble(RobotRcv[3]));
                    }
                    return "CCD8机械手到待机位";

                case "CCD8机械手到待机位":
                    tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.待机位, true, "");
                    ShowMessage("CCD8九点标定完成".txtTrans(TransType.Trans));
                    int sel2 = ShowMessage("CCD8九点标定完成，是否继续做其他标定？".txtTrans(TransType.Trans), "标定完成".txtTrans(TransType.Trans), new string[] { "CCD8旋转标定".txtTrans(TransType.Trans), "结束标定流程".txtTrans(TransType.Trans) });
                    if (sel2 == 0)
                        return "CCD8旋转标定开始";
                    else
                        return StepExit;

                case "CCD7旋转标定开始":
                    Delay(10);
                    return "CCD7旋转标定放标定片";

                case "CCD7旋转标定放标定片":
                    for (int i = 4; i < 8; i++)
                    {
                        tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.DO控制, true, i, 1);
                    }
                    ShowMessage("请在吸嘴上放好标定片");
                    return "CCD7旋转标定";

                case "CCD7旋转标定":
                    for (int i = 1; i <= 5; i++)
                    {
                        tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.CCD7旋转标定, true, AxisHandle.SpeedPercent, i);
                        Delay(200);
                        CCD7.SendCmd(out VisionRcv7, (int)TcpCommu_CCD7.Command.CCD7_旋转标定, true, i.ToString(), Convert.ToDouble(RobotRcv[2]), Convert.ToDouble(RobotRcv[3]));
                    }
                    return "CCD7旋转标定机械手到待机位";

                case "CCD7旋转标定机械手到待机位":
                    tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.待机位, true, "");
                    ShowMessage("CCD7旋转标定完成");
                    int sel3 = ShowMessage("CCD7旋转标定完成，是否继续做其他标定？", "标定完成", new string[] { "CCD8九点标定", "结束标定流程" });
                    if (sel3 == 0)
                        return "CCD8九点标定开始";
                    else
                        return StepExit;

                case "CCD8旋转标定开始":
                    Delay(10);
                    return "CCD8旋转标定放标定片";

                case "CCD8旋转标定放标定片":
                    for (int i = 4; i < 8; i++)
                    {
                        tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.DO控制, true, i, 1);
                    }
                    ShowMessage("请在吸嘴上放好标定片");
                    return "CCD8旋转标定";

                case "CCD8旋转标定":
                    for (int i = 1; i <= 5; i++)
                    {
                        tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.CCD8旋转标定, true, AxisHandle.SpeedPercent, i);
                        Delay(200);
                        CCD8.SendCmd(out VisionRcv7, (int)TcpCommu_CCD8.Command.CCD8_旋转标定, true, i.ToString(), Convert.ToDouble(RobotRcv[2]), Convert.ToDouble(RobotRcv[3]));
                    }
                    return "CCD8旋转标定机械手到待机位";

                case "CCD8旋转标定机械手到待机位":
                    tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.待机位, true, "");
                    ShowMessage("CCD8旋转标定完成");
                    int sel4 = ShowMessage("CCD8旋转标定完成，是否继续做其他标定？", "标定完成", new string[] { "继续其他标定", "结束标定流程" });
                    if (sel4 == 0)
                        return "标定选择入口";
                    else
                        return StepExit;

                //基准标定
                case "基准标定开始":
                    Delay(10);
                    return "基准标定放标定片";

                case "基准标定放标定片":
                    ShowMessage("请将标定片放到指定位置");
                    return "基准标定";

                case "基准标定":

                    tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.基准标定, true, "");
                    tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.拍照, true, "");
                    CCD7.SendCmd(out VisionRcv7, (int)TcpCommu_CCD7.Command.CCD7_注册物料模板, true, "");
                    CCD8.SendCmd(out VisionRcv7, (int)TcpCommu_CCD8.Command.CCD8_注册物料模板, true, "");

                    return "基准标定机械手到待机位";

                case "基准标定机械手到待机位":
                    tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.待机位, true, AxisHandle.SpeedPercent);
                    ShowMessage("基准标定完成");
                    int sel5 = ShowMessage("基准标定完成，是否继续做其他标定？", "标定完成", new string[] { "继续其他标定", "结束标定流程" });
                    if (sel5 == 0)
                        return "标定选择入口";
                    else
                        return StepExit;





                default:
                    return null;
            }
        }

        protected override string GrrRun(string strStep)
        {
            switch (strStep)
            {
                //相机静态流程开始
                case "相机静态流程开始":
                    Delay(10);

                    return "静态测试提示";

                case "静态测试提示":
                    for (int i = 4; i < 8; i++)
                    {
                        tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.DO控制, true, i, 1);
                    }
                    ShowMessage("请在吸嘴上放好物料");
                    return "机械手取静态标定片";

                case "机械手取静态标定片":
                    //tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.基准标定, true, "");
                    return "机械手静态测试";

                case "机械手静态测试":
                    tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.拍照, true, AxisHandle.SpeedPercent);
                    for (int i = 0; i < 300; i++)      //静态的次数
                    {
                        CCD7.SendCmd(out VisionRcv7, (int)TcpCommu_CCD7.Command.CCD7_静态, true);
                        CCD8.SendCmd(out VisionRcv8, (int)TcpCommu_CCD8.Command.CCD8_静态, true);
                        Delay(500);
                        VisionRcv8 = null;
                    }
                    return StepExit;

                //相机动态流程开始
                case "相机动态流程开始":
                    Delay(10);
                    return "动态测试提示";

                case "动态测试提示":
                    for (int i = 4; i < 8; i++)
                    {
                        tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.DO控制, true, i, 1);
                    }
                    ShowMessage("请在吸嘴上放好物料");
                    return "机械手取动态标定片";

                case "机械手取动态标定片":
                    //tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.基准标定, true, "");
                    return "机械手动态测试";

                case "机械手动态测试":
                    for (int i = 0; i < 200; i++)      //动态的次数
                    {
                        tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.拍照, true, AxisHandle.SpeedPercent);
                        Delay(500);
                        CCD7.SendCmd(out VisionRcv7, (int)TcpCommu_CCD7.Command.CCD7_动态, true);
                        CCD8.SendCmd(out VisionRcv8, (int)TcpCommu_CCD8.Command.CCD8_动态, true);
                        tcpCommu_Robot.SendCmd(out RobotRcv, (int)TcpCommu_Robot.Command.取料, true, AxisHandle.SpeedPercent);
                    }
                    return StepEntry;
                default:
                    return null;
            }
        }
        private bool VisionDataLimit(Double X, double Y, double U, double LimitX = 5, double LimitY = 5, double LimitU = 5)
        {
            bool overX = Math.Abs(X) > LimitX;
            bool overY = Math.Abs(Y) > LimitY;
            bool overU = Math.Abs(U) > LimitU;
            if (overX || overY || overU)
            {
                StringBuilder s1 = new StringBuilder();
                s1.AppendLine("视觉坐标超限：");
                if (overX) s1.AppendLine($"X={X:F3},超出±{LimitX}");
                if (overY) s1.AppendLine($"Y={Y:F3},超出±{LimitY}");
                if (overU) s1.AppendLine($"U={U:F3},超出±{LimitU}");
                ShowMessage(s1.ToString());
            }
            return (overX || overY || overU);

        }

        private bool AllNestCheck()
        {
            bool overLimit = false;

            for (int i = 0; i < 4; i++)
            {
                bool over = VisionDataLimit(CCD7.d下料引导位置X[i], CCD7.d下料引导位置Y[i], CCD7.d下料引导位置U[i], 5, 5, 5);
                if (over) overLimit = true;
            }
            //for (int i = 2; i < 4; i++)
            //{
            //    bool over = VisionDataLimit(CCD7.d下料引导位置X[i], CCD7.d下料引导位置Y[i], CCD7.d下料引导位置U[i], 5, 5, 5);
            //    if (over) overLimit = true;
            //}
            return overLimit;

        }
        /// <summary>
        /// 定义机械手点位数据格式
        /// </summary>
        public struct RobotPosItem
        {
            public string PosName;
            public double X;
            public double Y;
            public double U;

            public RobotPosItem(string name, double x, double y, double u)
            {
                PosName = name;
                X = x;
                Y = y;
                U = u;
            }
        }

        /// <summary>
        /// 相同NG原因判断，
        /// </summary>
        /// <param name="NGReasonStr">接收到的数据</param>
        /// <param name="msg">返回的消息</param>
        /// <returns></returns>
        private bool CheckNGreason(string NGReasonStr, out string msg)
        {
            NGReasonStr = NGReasonStr.Replace("CCD3检测NG", "").Replace("CCD6检测NG", "");
            string[] strAlls = NGReasonStr.Split('_');
            List<string> backStr = new List<string>();
            for (int i = 1; i < strAlls.Length; i++)
            {
                for (int j = NGReasons.Count - Parameter.GetInt("NGNums"); j < NGReasons.Count; j++)
                {
                    if (!NGReasons[j].Contains(strAlls[i]) && strAlls[i] != "")
                    {
                        break;
                    }
                    if (j >= NGReasons.Count - 1)
                    {
                        backStr.Add(strAlls[i]);
                    }
                }
            }
            if (backStr.Count > 0)
            {
                msg = string.Join(",", backStr);
                return true;
            }
            msg = "";
            return false;
        }
        /// <summary>
        /// 解析机械手初始化收到的点位数据
        /// </summary>
        /// <param name="RobotRcvStr">接收到的数据</param>
        /// <returns></returns>
        private List<RobotPosItem> ParseRobotPos(string RobotRcvStr)
        {
            if (string.IsNullOrWhiteSpace(RobotRcvStr))
            {
                ShowMessage("机械手返回点位数据为空，解析失败");
                return null;
            }
            string[] strAlls = RobotRcvStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (strAlls.Length <= 1)
            {
                ShowMessage("机械手数据无点位信息，格式异常");
                return null;
            }
            List<RobotPosItem> posList = new List<RobotPosItem>();
            for (int i = 1; i < strAlls.Length; i++)
            {
                string strTemp = strAlls[i].Trim();
                string[] strPos = strTemp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (strPos.Length < 4)
                {
                    ShowMessage("点位信息不全，请检查！");
                    return null;
                }
                string posName = strPos[0].Trim();
                if (!double.TryParse(strPos[1], out double x) || !double.TryParse(strPos[2], out double y) || !double.TryParse(strPos[3], out double u))
                {
                    ShowMessage($"点位{posName}包含非数字，请检查！");
                    return null;
                }
                posList.Add(new RobotPosItem(posName, x, y, u));
            }
            return posList;
        }

        /// <summary>
        /// 写入标准点位到csv文件
        /// </summary>
        /// <param name="robotPos">点位信息</param>
        /// <param name="csvPath">csv路径</param>
        private void overWriteRotPos(List<RobotPosItem> robotPos, string csvPath)
        {
            List<string> content = new List<string>();
            // 表头，和结构体字段对应
            content.Add("点位名称,X,Y,U");
            foreach (var item in robotPos)
            {
                // 按顺序：名称,X,Y,U 拼接一行
                string line = $"{item.PosName},{item.X},{item.Y},{item.U}";
                content.Add(line);
            }
            // 一次性覆盖写入文件
            File.WriteAllLines(csvPath, content, Encoding.Default);
        }


        /// <summary>
        /// 比较机械手点位与保存的csv文件点位数据是否一致
        /// </summary>
        /// <param name="robotPos"></param>
        /// <param name="csvPos"></param>
        /// <returns></returns>
        private bool compareRobotPos(List<RobotPosItem> robotPos, List<RobotPosItem> csvPos)
        {
            if (robotPos.Count != csvPos.Count)
            {
                return false;
            }
            for (int i = 0; i < csvPos.Count; i++)
            {
                if (robotPos[i].PosName == csvPos[i].PosName)
                {
                    if (Math.Abs(robotPos[i].X - csvPos[i].X) > 0.1 || Math.Abs(robotPos[i].Y - csvPos[i].Y) > 0.1 || Math.Abs(robotPos[i].U - csvPos[i].U) > 0.1)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            return true;
        }
        /// <summary>
        /// 获取csv文件里面的机械手点位并保存到list里面
        /// </summary>
        /// <param name="csvPath">标准点位文件路径</param>
        /// <returns></returns>
        private List<RobotPosItem> GetCsvPos(string csvPath)
        {
            List<RobotPosItem> csvPos = new List<RobotPosItem>();
            if (!File.Exists(csvPath))
            {
                ShowMessage("标准点位文件不存在");
                return null;
            }

            string[] allLines = File.ReadAllLines(csvPath, Encoding.Default);
            for (int i = 1; i < allLines.Length; i++)
            {
                string LineData = allLines[i].Trim();
                if (string.IsNullOrEmpty(LineData) || LineData.StartsWith("#"))
                    continue;

                string[] colData = LineData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (colData.Length < 4)
                {
                    ShowMessage($"csv文件第{i + 1}行数据不全");
                    return null;
                }
                string name = colData[0].Trim();
                if (!double.TryParse(colData[1], out double sx) || !double.TryParse(colData[2], out double sy) || !double.TryParse(colData[3], out double su))
                {
                    ShowMessage($"csv第{i + 1}行数据解析失败");
                    return null;
                }
                csvPos.Add(new RobotPosItem(name, sx, sy, su));

            }
            return csvPos;

        }


        protected override string OtherRun(string strStep)
        {
            switch (strStep)
            {
                default:
                    return null;
            }
        }

        #region
        /// <summary>
        /// 获取点位
        /// </summary>
        /// <param name="pt">点位枚举</param>
        /// <returns></returns>
        public PointInfo this[POINT pt]
        {
            get { return this.dictPoint[(int)pt]; }
        }


        /// <summary>
        /// 获取轴点位
        /// </summary>
        /// <param name="pt">点位枚举</param>
        /// <param name="axis">轴枚举</param>
        /// <returns></returns>
        public double this[POINT pt, AXIS axis]
        {
            get { return this.dictPoint[(int)pt][$"a{(int)axis}"]; }
        }

        /// <summary>
        /// 获取容器
        /// </summary>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        public ProductData this[NEST nIndex]
        {
            get { return this.stationSpace[(int)nIndex] as ProductData; }
            set { this.stationSpace[(int)nIndex] = value; }
        }
        #endregion
    }
    public static partial class HandleEx
    {
        public static double GetPos(this StationRobot.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this StationRobot.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}

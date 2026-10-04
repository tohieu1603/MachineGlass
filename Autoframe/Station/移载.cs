using AF.Card;
using AF.Common;
using AF.Data;
using AF.Station;
using Autoframe;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Autoframe
{
    public class 移载 : StationEx
    {
        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public 移载(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<NEST, ProductData>();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { DI.移载吸真空到位 };
            //配置站位界面显示输出
            arrayDO = new object[] { DO.CCD4触发, DO.移载吸真空, DO.移载破真空 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { };
            //配置站位轴
            arrayAxis = new object[] { AXIS.移载X轴, AXIS.移载Y轴, AXIS.移载Z轴 };
        }

        ProductData productData = new ProductData();
        /// <summary>
        /// 4:CCD4   5:CCD5
        /// </summary>
        int 所标定的相机 = 0; //4:CCD4。5:CCD5
        int n标定移动次数;
        double d九点标定步长 = 3;
        private double d拍照基准点X = 0;
        private double d拍照基准点Y = 0;
        private string[] str相机返回数据 = null;
        TcpCommu_CCD4 camera4 = new TcpCommu_CCD4();
        TcpCommu_CCD5 camera5 = new TcpCommu_CCD5();
        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            待机位,
            上料位,
            拍照位,
            下料位

        }

        /// <summary>
        /// 产品穴位，与产品是一对一
        /// </summary>
        public enum NEST
        {
            穴位1,
            穴位2,
            穴位3,
            穴位4
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
            RegBit.移载初始化吸嘴有料.Set(false);
            RegBit.移载初始化完成.Set(false);
            RegBit.移载通知AR检测放料完成.Set(false);
            RegBit.移载通知翻转取料完成.Set(false);
            switch (Process)
            {
                case AutoProcess.生产:
                case AutoProcess.空跑:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "生产流程开始";
                    break;
                case AutoProcess.CCD2标定注册:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = StepEmpty;
                    break;
                case AutoProcess.CCD4标定:
                    this.InitEntry = "初始化流程开始";
                    所标定的相机 = 4;
                    this.AutoEntry = "相机标定开始";
                    break;
                case AutoProcess.CCD5标定:
                    this.InitEntry = "初始化流程开始";
                    所标定的相机 = 5;
                    this.AutoEntry = "相机标定开始";
                    break;
                case AutoProcess.CCD4CCD5静态:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "CCD4CCD5静态";
                    break;
                case AutoProcess.CCD4CCD5动态:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "CCD4CCD5动态";
                    break;

                case AutoProcess.CCD3静态:
                case AutoProcess.CCD3动态:
                case AutoProcess.CCD6静态:
                case AutoProcess.CCD6动态:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = StepExit;
                    break;
                case AutoProcess.AR2测试:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = StepEmpty;
                    break;
                case AutoProcess.AR1测试:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = StepEmpty;
                    break;
                default:
                    this.InitEntry = StepExit;
                    this.AutoEntry = StepEmpty;
                    break;
            }
        }



        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case "初始化流程开始":
                    //string[] rcv4 = new string[] { };
                    //string[] rcv5 = new string[] { };
                    //// 发送CCD4定位引导

                    //camera4.SendCmd(out rcv4, (int)TcpCommu_CCD4.Command.CCD4_翻转上料产品定位, false, "0", "0", "0");
                    //// 发送CCD5定位引导
                    //camera5.SendCmd(out rcv5, (int)TcpCommu_CCD5.Command.CCD5_翻转上料产品定位, false, "0", "0", "0");
                    //DO.CCD4触发.Set(true);
                    //Thread.Sleep(20);
                    //DO.CCD4触发.Set(false);
                    //// 读取CCD4结果
                    //string[] res4Buf = new string[] { };
                    //bool ccd4Result = camera4.WaitReplyAndProcess(out res4Buf, (int)TcpCommu_CCD4.Command.CCD4_翻转上料产品定位);

                    //// 读取CCD5结果
                    //string[] res5Buf = new string[] { };
                    //bool ccd5Result = camera5.WaitReplyAndProcess(out res5Buf, (int)TcpCommu_CCD5.Command.CCD5_翻转上料产品定位);
                    //return StepExit;
                    //第一步：打开吸嘴吸真空=T
                    DO.移载吸真空.Set(true);
                    DO.移载破真空.Set(false);
                    return "连接相机";
                case "连接相机":
                    if (!camera4.Init(this, 4))
                        return "连接相机4";
                    else
                        Logger.Process(Name, "相机4连接成功".txtTrans(TransType.Trans));
                    if (!camera5.Init(this, 5))
                        return "连接相机5";
                    else
                        Logger.Process(Name, "相机5连接成功".txtTrans(TransType.Trans));
                    return "轴上使能";
                case "轴上使能":
                    AXIS.移载X轴.ServoOn(true);
                    AXIS.移载Y轴.ServoOn(true);
                    AXIS.移载Z轴.ServoOn(true);
                    Delay(1000);
                    return "Z轴回原点";
                case "Z轴回原点":
                    AXIS.移载Z轴.Home();
                    WaitAxis(AXIS.移载Z轴);
                    return "Z轴到待机位";
                case "Z轴到待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    return "X轴到待机位";

                case "X轴到待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载X轴 }, 100, true);
                    WaitAxis(AXIS.移载X轴);
                    return "Y轴到待机位";

                case "Y轴到待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Y轴 }, 100, true);
                    WaitAxis(AXIS.移载Y轴);
                    return "吸嘴真空判断";

                case "吸嘴真空判断":
                    if (DI.移载吸真空到位.Get())
                    {

                        if (this[NEST.穴位1] != null && this[NEST.穴位2] != null && this[NEST.穴位3] != null && this[NEST.穴位4] != null)
                        {

                            //真空到位：移载初始化吸嘴有料=T
                            RegBit.移载初始化吸嘴有料.Set(true);
                        }
                        else
                        {
                            this[NEST.穴位1] = "翻转".GetStation<翻转>()[翻转.NEST.穴位1];
                            this[NEST.穴位2] = "翻转".GetStation<翻转>()[翻转.NEST.穴位2];
                            this[NEST.穴位3] = "翻转".GetStation<翻转>()[翻转.NEST.穴位3];
                            this[NEST.穴位4] = "翻转".GetStation<翻转>()[翻转.NEST.穴位4];
                            "翻转".GetStation<翻转>()[翻转.NEST.穴位1] = null;
                            "翻转".GetStation<翻转>()[翻转.NEST.穴位2] = null;
                            "翻转".GetStation<翻转>()[翻转.NEST.穴位3] = null;
                            "翻转".GetStation<翻转>()[翻转.NEST.穴位4] = null;
                            if (this[NEST.穴位1] != null && this[NEST.穴位2] != null && this[NEST.穴位3] != null && this[NEST.穴位4] != null)
                            {
                                //真空到位：移载初始化吸嘴有料=T
                                RegBit.移载初始化吸嘴有料.Set(true);
                            }
                        }
                            return "置位初始化完成";
                    }
                    else
                    {
                        this[NEST.穴位1] = null;
                        this[NEST.穴位2] = null;
                        this[NEST.穴位3] = null;
                        this[NEST.穴位4] = null;
                        DO.移载吸真空.Set(false);
                        DO.移载破真空.Set(true);
                        Delay(1000);
                        DO.移载破真空.Set(false);
                        //真空不到位：移载初始化吸嘴有料=F，然后破真空
                        RegBit.移载初始化吸嘴有料.Set(false);

                        //DO.移载破真空.Set(true);
                        return "置位初始化完成";
                    }

                case "置位初始化完成":
                    //无论有无料，最后都置位 移载初始化完成=T
                    RegBit.移载初始化完成.Set(true);

                    return StepExit;

                default:
                    return null;
            }
        }

        // 类内全局变量
        string CCD4Time = "";
        string CCD5Time = "";
        DateTime TakeinTime = DateTime.Now;
        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case "生产流程开始":


                    CCD4Time = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}";
                    CCD5Time = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}";
                    return "判断初始化吸嘴结果";

                case "判断初始化吸嘴结果":
                    // 流程图第一个判断：移载初始化吸嘴有料？
                    if (RegBit.移载初始化吸嘴有料.Get())
                    {
                        // 吸嘴带料，先走下料给AR检测流程
                        return "模组到下料位";
                    }
                    // 无料，直接等待翻转来料
                    return "等待翻转通知移载有料";

                #region 分支：吸嘴有料 → 下料给AR检测站
                case "模组到下料位":
                    //Logger.Process(this.Name, $"{Name},穴位1，结果：{this[NEST.穴位1].Result}，NG原因：{this[NEST.穴位1].NGReason}");
                    //Logger.Process(this.Name, $"{Name},穴位2，结果：{this[NEST.穴位2].Result}，NG原因：{this[NEST.穴位2].NGReason}");
                    //Logger.Process(this.Name, $"{Name},穴位3，结果：{this[NEST.穴位3].Result}，NG原因：{this[NEST.穴位3].NGReason}");
                    //Logger.Process(this.Name, $"{Name},穴位4，结果：{this[NEST.穴位4].Result}，NG原因：{this[NEST.穴位4].NGReason}");
                    AxisGoto(POINT.下料位, new AXIS[] { AXIS.移载X轴, AXIS.移载Y轴 }, 100, true);
                    WaitAxis(AXIS.移载X轴);
                    WaitAxis(AXIS.移载Y轴);
                    if (RegBit.移载初始化吸嘴有料.Get())
                    {
                        RegBit.移载初始化吸嘴有料.Set(false);
                        return "等待AR检测接料就绪";
                    }
                    RegBit.移载通知翻转取料完成.Set(true);
                    // 置位通知AR检测准备接料
                    return "等待数据传递完成";

                case "等待数据传递完成":
                    if (RegBit.移载通知翻转取料完成.Get())
                        return "等待数据传递完成";
                    return "等待AR检测接料就绪";
                case "等待AR检测接料就绪":
                    if (RegBit.AR检测通知移载接料准备OK.Get())
                    {
                        RegBit.AR检测通知移载接料准备OK.Set(false);
                        return "模组Z到下料放料位";
                    }
                    return "等待AR检测接料就绪";

                case "模组Z到下料放料位":
                    AxisGoto(POINT.下料位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    return "破真空释放物料";

                case "破真空释放物料":
                    // 流程图：破真空0.2S，吸嘴真空=F
                    DO.移载破真空.Set(true);
                    DO.移载吸真空.Set(false);
                    Thread.Sleep(100);
                    DO.移载破真空.Set(false);
                    TakeinTime = DateTime.Now;
                    return "AR检测真空";
                case "AR检测真空":
                    // 空跑跳过真空
                    if (IsDryRun)
                    {
                        Delay(1000);
                        return "Z轴回待机位";
                    }
                    bool[] vacArr = new bool[]
                    {
                        this[NEST.穴位1].CCD3Sit != "NULL" && DI.AR面吸真空1到位.Get(),
                        this[NEST.穴位2].CCD3Sit != "NULL" && DI.AR面吸真空2到位.Get(),
                        this[NEST.穴位3].CCD3Sit != "NULL" && DI.AR面吸真空3到位.Get(),
                        this[NEST.穴位4].CCD3Sit != "NULL" && DI.AR面吸真空4到位.Get()
                    };
                    bool[] vacArr2 = new bool[]
                    {
                        this[NEST.穴位1].CCD3Sit != "NULL" && DI.AR面吸真空5到位.Get(),
                        this[NEST.穴位2].CCD3Sit != "NULL" && DI.AR面吸真空6到位.Get(),
                        this[NEST.穴位3].CCD3Sit != "NULL" && DI.AR面吸真空7到位.Get(),
                        this[NEST.穴位4].CCD3Sit != "NULL" && DI.AR面吸真空8到位.Get()
                    };

                    if (vacArr.All(a => a))
                    {
                        Delay(500);
                        return "Z轴回待机位";
                    }
                    if (vacArr2.All(a => a))
                    {
                        Delay(500);
                        return "Z轴回待机位";
                    }
                    if ((DateTime.Now - TakeinTime).TotalMilliseconds > 3000)
                    {
                        this[NEST.穴位1].BreakDownTime += 3;
                        this[NEST.穴位2].BreakDownTime += 3;
                        this[NEST.穴位3].BreakDownTime += 3;
                        this[NEST.穴位4].BreakDownTime += 3;
                        if (RegBit.IR载具清料.Get())
                        {
                            return "Z轴回待机位";
                        }
                        int ret = ShowMessage("AR吸真空异常，确认‘下一片’还是‘重新放料’", "提示", new string[] { "下一片", "重新放料", "忽略" });
                        this[NEST.穴位1].BreakDownTime += Duration;
                        this[NEST.穴位2].BreakDownTime += Duration;
                        this[NEST.穴位3].BreakDownTime += Duration;
                        this[NEST.穴位4].BreakDownTime += Duration;
                        if (ret == 0)
                        {

                            AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                            WaitAxis(AXIS.移载Z轴);
                            AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Y轴 }, 100, true);
                            WaitAxis(AXIS.移载Y轴);
                            DO.移载破真空.Set(false);
                            RegBit.移载初始化吸嘴有料.Set(false);
                            RegBit.AR检测通知移载接料准备OK.Set(true);
                            ShowMessage($"请拿走和AR模组上的料".txtTrans(TransType.Trans));
                            return "生产流程开始";
                        }
                        else if (ret == 2)
                        {
                            return "Z轴回待机位";
                        }
                        else
                        {
                            DO.移载破真空.Set(false);
                            DO.移载吸真空.Set(true);
                            Thread.Sleep(500);
                            AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                            WaitAxis(AXIS.移载Z轴);
                            if (!RegBit.IR载具清料.Get())
                            {
                                ShowMessage($"请确认效果".txtTrans(TransType.Trans));
                            }

                            TakeinTime = DateTime.Now;
                            return "模组Z到下料放料位";
                        }

                    }
                    return "AR检测真空";
                case "Z轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    // 标记放料完成，通知AR检测
                    RegBit.移载通知AR检测放料完成.Set(true);
                    RegBit.移载初始化吸嘴有料.Set(false);
                    if (RegBit.翻转通知移载有料.Get())
                    {
                        RegBit.翻转通知移载有料.Set(false);
                        return "模组到翻转取料上料位";
                    }
                    return "Y轴回待机位";

                case "Y轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Y轴 }, 100, true);
                    WaitAxis(AXIS.移载Y轴);

                    // 下料流程走完，进入等待翻转来料
                    return "等待翻转通知移载有料";
                #endregion

                #region 公共等待：等待翻转站来料信号
                case "等待翻转通知移载有料":
                    if (RegBit.翻转通知移载有料.Get())
                    {
                        RegBit.翻转通知移载有料.Set(false);
                        return "模组到翻转取料上料位";
                    }
                    return "等待翻转通知移载有料";
                #endregion

                #region 翻转取料流程
                case "模组到翻转取料上料位":
                    AxisGoto(POINT.上料位, new AXIS[] { AXIS.移载X轴, AXIS.移载Y轴 }, 100, true);
                    WaitAxis(AXIS.移载X轴);
                    WaitAxis(AXIS.移载Y轴);
                    AxisGoto(POINT.上料位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    return "打开移载吸真空";

                case "打开移载吸真空":
                    DO.移载吸真空.Set(true);
                    DO.移载破真空.Set(false);
                    RegBit.移载通知翻转吸真空.Set(true);

                    TakeinTime = DateTime.Now;
                    Delay(200);
                    return "判断移载吸真空到位";

                case "判断移载吸真空到位":
                    // 空跑模式跳过真空检测
                    if (Process == AutoProcess.空跑)
                    {
                        Logger.Process(Name, "空跑模式：跳过真空校验".txtTrans(TransType.Trans));
                        return "取料后Z轴回待机位";
                    }
                    // 真空到位校验，超时失败则重新取料
                    if (DI.移载吸真空到位.Get() && !DI.翻转吸真空到位.Get())
                    {
                        Delay(500);
                        //RegBit.移载初始化吸嘴有料.Set(true);
                        return "取料后Z轴回待机位";
                    }
                    if ((DateTime.Now - TakeinTime).TotalMilliseconds > 3000)
                    {
                        if (0 == ShowMessage("移载吸真空异常，确认‘下一片’还是‘等待’", "提示", new string[] { "下一片", "等待" }))
                        {

                            AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                            WaitAxis(AXIS.移载Z轴);
                            AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Y轴 }, 100, true);
                            WaitAxis(AXIS.移载Y轴);
                            DO.移载吸真空.Set(false);
                            RegBit.移载通知翻转取料完成.Set(true);
                            return "生产流程开始";
                        }
                        else
                        {
                            TakeinTime = DateTime.Now;
                        }

                    }
                    return "判断移载吸真空到位";

                case "取料后Z轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    // 通知翻转取料完成
                    return "取料后到拍照位";
                case "取料后到拍照位":
                    AxisGoto(POINT.拍照位, new AXIS[] { AXIS.移载X轴, AXIS.移载Y轴, AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    WaitAxis(AXIS.移载Y轴);
                    WaitAxis(AXIS.移载X轴);
                    //Delay(100);
                    return "CCD4_CCD5发送定位拍照指令";
                case "CCD4_CCD5发送定位拍照指令":
                    // 获取移载实时X/Y坐标，U固定0
                    double xNow = POINT.拍照位.GetPos(AXIS.移载X轴, this);
                    double yNow = POINT.拍照位.GetPos(AXIS.移载Y轴, this);
                    double uNow = 0;
                    string[] rcv4 = new string[] { };
                    string[] rcv5 = new string[] { };
                    // 发送CCD4定位引导

                    camera4.SendCmd(out rcv4, (int)TcpCommu_CCD4.Command.CCD4_翻转上料产品定位, false, xNow.ToString(), yNow.ToString(), uNow.ToString());
                    // 发送CCD5定位引导
                    camera5.SendCmd(out rcv5, (int)TcpCommu_CCD5.Command.CCD5_翻转上料产品定位, false, xNow.ToString(), yNow.ToString(), uNow.ToString());
                    DO.CCD4触发.Set(true);
                    Thread.Sleep(20);
                    DO.CCD4触发.Set(false);
                    Thread.Sleep(50);
                    return "获取CCD4_CCD5视觉检测结果";

                case "获取CCD4_CCD5视觉检测结果":
                    // 空跑模式直接强制OK，忽略视觉返回
                    if (Process == AutoProcess.空跑)
                    {
                        //this[NEST.穴位1].Result = true;
                        //this[NEST.穴位1].NGReason = "空跑模式忽略视觉";
                        //this[NEST.穴位2].Result = true;
                        //this[NEST.穴位2].NGReason = "空跑模式忽略视觉";
                        //this[NEST.穴位3].Result = true;
                        //this[NEST.穴位3].NGReason = "空跑模式忽略视觉";
                        //this[NEST.穴位4].Result = true;
                        //this[NEST.穴位4].NGReason = "空跑模式忽略视觉";
                        return "模组到下料位";
                    }
                    // 读取CCD4结果
                    string[] res4Buf = new string[] { };
                    bool ccd4Result = camera4.WaitReplyAndProcess(out res4Buf, (int)TcpCommu_CCD4.Command.CCD4_翻转上料产品定位);

                    // 读取CCD5结果
                    string[] res5Buf = new string[] { };
                    bool ccd5Result = camera5.WaitReplyAndProcess(out res5Buf, (int)TcpCommu_CCD5.Command.CCD5_翻转上料产品定位);
                    if (ccd4Result && ccd5Result)
                    {
                        if (RegBit.IR载具清料.Get())
                        {
                            return "模组到下料位";
                        }
                        if (res4Buf[2] == "1" && res4Buf[3] == "1" && res5Buf[2] == "1" && res5Buf[3] == "1")
                        {
                            return "模组到下料位";
                        }


                        int ret = ShowMessage("移载拍照存在缺料", "提示", new string[] { "重取", "重拍", "忽略" });
                        if (ret == 0)
                        {
                            AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                            WaitAxis(AXIS.移载Z轴);
                            return "模组到翻转取料上料位";
                        }
                        else if (ret == 2)
                        {
                            return "模组到下料位";
                        }
                    }
                    else
                    {
                        int ret = ShowMessage("移载拍照失败", "提示", new string[] { "重取", "重拍" });
                        if (ret == 0)
                        {
                            AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                            WaitAxis(AXIS.移载Z轴);
                            return "模组到翻转取料上料位";
                        }
                    }
                    return "CCD4_CCD5发送定位拍照指令";

                #endregion

                case "生产流程结束":
                    return "生产流程开始";
            }
            return null;
        }

        protected override string CalibRun(string strStep)
        {
            switch (strStep)
            {
                case "相机标定开始":
                    Delay(10);
                    RegInt.相机功能选择.Set(
                    ShowMessage($"{Process.ToString()}开始，请在扩膜平台上放一片钢环，选择相机功能后，按‘启动’按钮",
                    "提示", new string[] { "注册", "九点标定" }));
                    n标定移动次数 = 1;
                    WaitDI(DI.启动, true, -1);
                    if (RegInt.相机功能选择.Get() == 0)
                    {
                        return "等待AR检测1到上料位";
                    }
                    ShowMessage($"{Process.ToString()}开始，请确定移载吸嘴有料");
                    return "取料后到拍照位";
                case "等待AR检测1到上料位":
                    if (RegBit.AR检测通知移载接料准备OK.Get())
                    {
                        RegBit.AR检测通知移载接料准备OK.Set(false);
                        return "模组到下料位";
                    }
                    return "等待AR检测1到上料位";
                case "模组到下料位":
                    AxisGoto(POINT.下料位, new AXIS[] { AXIS.移载X轴, AXIS.移载Y轴 }, 100, true);
                    WaitAxis(AXIS.移载X轴);
                    WaitAxis(AXIS.移载Y轴);
                    // 置位通知AR检测准备接料
                    return "模组Z到下料放料位";
                case "模组Z到下料放料位":
                    AxisGoto(POINT.下料位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    return "吸真空";
                case "吸真空":
                    DO.移载吸真空.Set(true);
                    DO.移载破真空.Set(false);
                    Thread.Sleep(50);
                    TakeinTime = DateTime.Now;
                    return "判断移载吸真空到位";
                case "判断移载吸真空到位":
                    // 真空到位校验，超时失败则重新取料
                    if (DI.移载吸真空到位.Get())
                    {
                        RegBit.移载初始化吸嘴有料.Set(true);
                        return "取料后Z轴回待机位";
                    }
                    if ((DateTime.Now - TakeinTime).TotalMilliseconds > 3000)
                    {
                        ShowMessage($"{Name}吸嘴真空异常，请检查".txtTrans(TransType.Trans));
                        TakeinTime = DateTime.Now;
                    }
                    return "判断移载吸真空到位";
                case "取料后Z轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    return "取料后到拍照位";
                case "取料后到拍照位":
                    AxisGoto(POINT.拍照位, new AXIS[] { AXIS.移载X轴, AXIS.移载Y轴 }, 100, true);
                    WaitAxis(AXIS.移载Y轴);
                    WaitAxis(AXIS.移载X轴);
                    AxisGoto(POINT.拍照位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    d拍照基准点X = AXIS.移载X轴.GetPos();
                    d拍照基准点Y = AXIS.移载Y轴.GetPos();
                    Delay(100);
                    if (RegInt.相机功能选择.Get() == 0)
                        return "发送注册拍照指令";
                    return "移动到九点标定点";
                case "发送注册拍照指令":
                    // 获取移载实时X/Y坐标，U固定0
                    double xNow = POINT.拍照位.GetPos(AXIS.移载X轴, this);
                    double yNow = POINT.拍照位.GetPos(AXIS.移载Y轴, this);
                    double uNow = 0;
                    string[] rcv4 = new string[] { };
                    string[] rcv5 = new string[] { };
                    // 发送CCD4定位引导
                    if (所标定的相机 == 4)
                        camera4.SendCmd(out rcv4, (int)TcpCommu_CCD4.Command.CCD4_注册物料模板, false, xNow.ToString(), yNow.ToString(), uNow.ToString());
                    // 发送CCD5定位引导
                    if (所标定的相机 == 5)
                        camera5.SendCmd(out rcv5, (int)TcpCommu_CCD5.Command.CCD5_注册物料模板, false, xNow.ToString(), yNow.ToString(), uNow.ToString());

                    if (所标定的相机 == 4)
                    {
                        string[] res4Buf = new string[] { };
                        camera4.WaitReplyAndProcess(out res4Buf, (int)TcpCommu_CCD4.Command.CCD4_注册物料模板);
                        if (res4Buf[3] == "NG")
                            Logger.Process(Name, "CCD4标定失败".txtTrans(TransType.Trans));
                        else
                            Logger.Process(Name, "CCD4标定成功".txtTrans(TransType.Trans));
                    }

                    // 读取CCD5结果
                    if (所标定的相机 == 5)
                    {
                        string[] res5Buf = new string[] { };
                        camera5.WaitReplyAndProcess(out res5Buf, (int)TcpCommu_CCD5.Command.CCD5_注册物料模板);
                        if (res5Buf[3] == "NG")
                            Logger.Process(Name, "CCD5标定失败".txtTrans(TransType.Trans));
                        else
                            Logger.Process(Name, "CCD5标定成功".txtTrans(TransType.Trans));
                    }

                    return "注册标定结束";

                case "移动到九点标定点":
                    double dX, dY;
                    d九点标定步长 = 1;

                    switch (n标定移动次数)
                    {
                        case 1:
                            dX = d拍照基准点X + d九点标定步长; ;
                            dY = d拍照基准点Y + d九点标定步长;
                            break;
                        case 2:
                            dX = d拍照基准点X;
                            dY = d拍照基准点Y + d九点标定步长;
                            break;
                        case 3:
                            dX = d拍照基准点X - d九点标定步长;
                            dY = d拍照基准点Y + d九点标定步长;
                            break;
                        case 4:
                            dX = d拍照基准点X - d九点标定步长;
                            dY = d拍照基准点Y;
                            break;
                        case 5:
                            dX = d拍照基准点X;
                            dY = d拍照基准点Y;
                            break;
                        case 6:
                            dX = d拍照基准点X + d九点标定步长;
                            dY = d拍照基准点Y;
                            break;
                        case 7:
                            dX = d拍照基准点X + d九点标定步长;
                            dY = d拍照基准点Y - d九点标定步长;
                            break;
                        case 8:
                            dX = d拍照基准点X;
                            dY = d拍照基准点Y - d九点标定步长;
                            break;
                        case 9:
                            dX = d拍照基准点X - d九点标定步长;
                            dY = d拍照基准点Y - d九点标定步长;
                            break;
                        default:
                            return "九点标定完成";
                    }
                    AXIS.移载X轴.AbsMove(dX, 30);
                    AXIS.移载Y轴.AbsMove(dY, 30);
                    WaitAxis(AXIS.移载X轴);
                    WaitAxis(AXIS.移载Y轴);
                    Delay(1000);
                    n标定移动次数++;
                    return "相机拍照并判断结果";
                case "相机拍照并判断结果":
                    bool CCD4拍照结果 = true;
                    if (所标定的相机 == 4)
                    {
                        CCD4拍照结果 = this.camera4.SendCmd(out str相机返回数据, (int)TcpCommu_CCD4.Command.CCD4_九点标定, true, n标定移动次数 - 1,
                          AXIS.移载X轴.GetPos().ToString(), AXIS.移载Y轴.GetPos().ToString(), "0");
                    }

                    bool CCD5拍照结果 = true;
                    if (所标定的相机 == 5)
                    {
                        CCD5拍照结果 = this.camera5.SendCmd(out str相机返回数据, (int)TcpCommu_CCD5.Command.CCD5_九点标定, true, n标定移动次数 - 1,
                         AXIS.移载X轴.GetPos().ToString(), AXIS.移载Y轴.GetPos().ToString(), "0");
                    }

                    if (!CCD4拍照结果 || !CCD5拍照结果)
                    {
                        ShowMessage($"{Process.ToString()}失败，请检查处理后按‘确定’");
                        return "移载模组回到待机位";
                    }
                    return "取料后到拍照位";
                case "移载模组回到待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    return "X轴到待机位";

                case "X轴到待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载X轴 }, 100, true);
                    WaitAxis(AXIS.移载X轴);
                    return "Y轴到待机位";

                case "Y轴到待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载Y轴 }, 100, true);
                    WaitAxis(AXIS.移载Y轴);
                    return "相机标定开始";
                case "九点标定完成":
                case "注册标定结束":
                    return "相机标定开始";
                default:
                    return null;
            }
        }
        int CCDTimes = 0;
        protected override string GrrRun(string strStep)
        {
            switch (strStep)
            {
                #region 静态
                case "CCD4CCD5静态":
                    DO.移载破真空.Set(false);
                    DO.移载吸真空.Set(true);
                    Delay(1000);
                    ShowMessage($"{Name}提示放料".txtTrans(TransType.Trans));

                    return "运动到拍照位";
                case "运动到拍照位":
                    CCDTimes = 0;
                    AxisGoto(POINT.拍照位, new AXIS[] { AXIS.移载X轴, AXIS.移载Y轴, AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    WaitAxis(AXIS.移载Y轴);
                    WaitAxis(AXIS.移载X轴);
                    Delay(100);
                    return "CCD4_CCD5发送定位拍照指令";
                case "CCD4_CCD5发送定位拍照指令":
                    // 获取移载实时X/Y坐标，U固定0
                    double xNow = POINT.拍照位.GetPos(AXIS.移载X轴, this);
                    double yNow = POINT.拍照位.GetPos(AXIS.移载Y轴, this);
                    double uNow = 0;
                    string[] rcv4 = new string[] { };
                    string[] rcv5 = new string[] { };
                    // 发送CCD4定位引导
                    camera4.SendCmd(out rcv4, (int)TcpCommu_CCD4.Command.CCD4_静态, false, xNow.ToString(), yNow.ToString(), uNow.ToString());
                    // 发送CCD5定位引导
                    camera5.SendCmd(out rcv5, (int)TcpCommu_CCD5.Command.CCD5_静态, false, xNow.ToString(), yNow.ToString(), uNow.ToString());
                    Thread.Sleep(100);
                    return "获取CCD4_CCD5视觉检测结果";

                case "获取CCD4_CCD5视觉检测结果":
                    // 读取CCD4结果
                    //string[] res4Buf = new string[] { };
                    //camera4.WaitReplyAndProcess(out res4Buf, (int)TcpCommu_Camera.Command.CCD4_静态);
                    //if (res4Buf[2] == "NG")
                    //{
                    //    Logger.Process(Name, "CCD4标定失败".txtTrans(TransType.Trans));
                    //    return null;
                    //}
                    //// 读取CCD5结果
                    //string[] res5Buf = new string[] { };
                    //camera4.WaitReplyAndProcess(out res5Buf, (int)TcpCommu_Camera.Command.CCD5_静态);
                    //if (res5Buf[2] == "NG")
                    //{
                    //    Logger.Process(Name, "CCD5标定失败".txtTrans(TransType.Trans));
                    //    return null;
                    //}
                    CCDTimes++;
                    if (CCDTimes < Parameter.GetInt("GRRNums"))
                    {
                        Delay(200);
                        return "CCD4_CCD5发送定位拍照指令";
                    }
                    CCDTimes = 0;
                    return "CCD4CCD5静态去待机位";
                case "CCD4CCD5静态去待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载X轴, AXIS.移载Y轴, AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    WaitAxis(AXIS.移载Y轴);
                    WaitAxis(AXIS.移载X轴);
                    return StepExit;
                #endregion

                #region 动态
                case "CCD4CCD5动态":
                    DO.移载破真空.Set(false);
                    DO.移载吸真空.Set(true);
                    Delay(1000);
                    ShowMessage($"{Name}提示放料".txtTrans(TransType.Trans));
                    CCDTimes = 0;
                    return "运动到DT拍照位";
                case "运动到DT拍照位":
                    AxisGoto(POINT.拍照位, new AXIS[] { AXIS.移载X轴, AXIS.移载Y轴, AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    WaitAxis(AXIS.移载Y轴);
                    WaitAxis(AXIS.移载X轴);
                    Delay(100);
                    return "CCD4CCD5动态发送定位拍照指令";
                case "CCD4CCD5动态发送定位拍照指令":
                    // 获取移载实时X/Y坐标，U固定0
                    double xNowD = POINT.拍照位.GetPos(AXIS.移载X轴, this);
                    double yNowD = POINT.拍照位.GetPos(AXIS.移载Y轴, this);
                    double uNowD = 0;
                    string[] rcv4D = new string[] { };
                    string[] rcv5D = new string[] { };
                    // 发送CCD4定位引导
                    camera4.SendCmd(out rcv4D, (int)TcpCommu_CCD4.Command.CCD4_动态, false, xNowD.ToString(), yNowD.ToString(), uNowD.ToString());
                    // 发送CCD5定位引导
                    camera5.SendCmd(out rcv5D, (int)TcpCommu_CCD5.Command.CCD5_动态, false, xNowD.ToString(), yNowD.ToString(), uNowD.ToString());
                    Thread.Sleep(100);
                    return "获取CCD4CCD5动态视觉检测结果";

                case "获取CCD4CCD5动态视觉检测结果":
                    // 读取CCD4结果
                    //string[] res4BufD = new string[] { };
                    //camera4.WaitReplyAndProcess(out res4BufD, (int)TcpCommu_Camera.Command.CCD4_静态);
                    //if (res4BufD[2] == "NG")
                    //{
                    //    Logger.Process(Name, "CCD4标定失败".txtTrans(TransType.Trans));
                    //    return null;
                    //}
                    //// 读取CCD5结果
                    //string[] res5BufD = new string[] { };
                    //camera4.WaitReplyAndProcess(out res5BufD, (int)TcpCommu_Camera.Command.CCD5_静态);
                    //if (res5BufD[2] == "NG")
                    //{
                    //    Logger.Process(Name, "CCD5标定失败".txtTrans(TransType.Trans));
                    //    return null;
                    //}
                    return "CCD4CCD5动态去待机位";
                case "CCD4CCD5动态去待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.移载X轴, AXIS.移载Y轴, AXIS.移载Z轴 }, 100, true);
                    WaitAxis(AXIS.移载Z轴);
                    WaitAxis(AXIS.移载Y轴);
                    WaitAxis(AXIS.移载X轴);
                    Delay(100);
                    CCDTimes++;
                    if (CCDTimes < Parameter.GetInt("GRRNums"))
                    {
                        Delay(200);
                        return "运动到DT拍照位";
                    }
                    CCDTimes = 0;
                    return StepExit;
                #endregion
                default:
                    return null;
            }
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

        /// <summary>
        /// 将源穴位的数据，传给目标穴位。传送完后源穴位清空。
        /// </summary>
        /// <param name="sourceNest">源穴位枚举</param>
        /// <param name="tagetNest">目标穴位</param>
        public void TransferProduct(NEST sourceNest, ref ProductData tagetNest)
        {
            tagetNest = this[sourceNest];
            this[sourceNest] = null;
        }

        /// <summary>
        /// 将源穴位的数据，传给目标穴位。传送完后源穴位清空。
        /// </summary>
        /// <param name="sourceNest">源穴位</param>
        /// <param name="targetNest">目标穴位枚举</param>
        public void TransferProduct(ref ProductData sourceNest, NEST targetNest)
        {
            this[targetNest] = sourceNest;
            sourceNest = null;
        }
        #endregion
    }
    public static partial class HandleEx
    {
        public static double GetPos(this 移载.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this 移载.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}

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
    public class 翻转 : StationEx
    {
        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public 翻转(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<NEST, ProductData>();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { DI.翻转吸真空到位 };
            //配置站位界面显示输出
            arrayDO = new object[] { DO.CCD4触发,  DO.翻转吸真空, DO.翻转破真空 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { };
            //配置站位轴
            arrayAxis = new object[] { AXIS.翻转U轴 };
        }


        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            待机位,
            上料位,
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
            RegBit.翻转初始化完成.Set(false);
            RegBit.翻转吸嘴有料.Set(false);
            RegBit.翻转通知IR检测上料.Set(false);
            RegBit.翻转通知IR检测上料完成.Set(false);
            RegBit.翻转通知移载有料.Set(false);
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
                case AutoProcess.CCD3静态:
                case AutoProcess.CCD3动态:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = StepExit;
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
                    //Thread.Sleep(2000);
                    //bool Uresult = this[NEST.穴位1].Result;

                    //this[NEST.穴位1].Product = new ProductData();
                    //this[NEST.穴位2].Product = new ProductData();
                    //this[NEST.穴位3].Product = new ProductData();
                    //this[NEST.穴位4].Product = new ProductData();
                    //WorkSpace.PresentFixtures[0].Carriers = new Carrier[1];
                    //return "初始化完成";
                    //return StepExit;
                    //Delay(10);
                    //camera.Init(this, 3);
                    //camera.DeInit();
                    //string[] strTemp = new string[] { };
                    //camera.SendCmd(out strTemp, (int)TcpCommu_Camera.Command.CCD1_上料排片引导, false);
                    //camera.WaitReplyAndProcess(out strTemp, (int)TcpCommu_Camera.Command.CCD1_上料排片引导);
                    return "轴上使能";


                case "轴上使能":
                    AXIS.翻转U轴.ServoOn(true);
                    Delay(1000);
                    return "等待移载初始化完成";

                case "等待移载初始化完成":
                    if (RegBit.移载初始化完成.Get())
                    {
                        return "吸嘴吸真空";
                    }
                    return "等待移载初始化完成";
                case "吸嘴吸真空":
                    DO.翻转吸真空.Set(true);
                    DO.翻转破真空.Set(false);
                    return "U轴回待机位";
                case "U轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.翻转U轴 }, 100, true);
                    WaitAxis(AXIS.翻转U轴);
                    return "判断吸真空";
                case "判断吸真空":
                    if (DI.翻转吸真空到位.Get() && this[NEST.穴位1] != null && this[NEST.穴位2] != null && this[NEST.穴位3] != null && this[NEST.穴位4] != null)
                    {
                        RegBit.翻转吸嘴有料.Set(true);
                    }
                    else
                    {
                        this[NEST.穴位1] = null;
                        this[NEST.穴位2] = null;
                        this[NEST.穴位3] = null;
                        this[NEST.穴位4] = null;
                        DO.翻转吸真空.Set(false);
                        DO.翻转破真空.Set(true);
                        Delay(1000);
                        DO.翻转破真空.Set(false);
                        RegBit.翻转吸嘴有料.Set(false);
                    }
                    return "初始化完成";
                case "初始化完成":
                    RegBit.翻转初始化完成.Set(true);
                    return StepExit;
                default:
                    return null;
            }
        }
        int UploadSit = 0;
        int CCDSit = 0;
        string CCDTime = null;
        DateTime TakeinTime = DateTime.Now;
        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case "生产流程开始":
                    // 判断：翻转吸嘴有料=F
                    if (!RegBit.翻转吸嘴有料.Get())
                    {
                        return "轴运动到上料位";
                    }
                    else
                    {
                        RegBit.翻转吸嘴有料.Set(false) ;
                        // 吸嘴有料，直接走到下料工序
                        return "轴运动到下料位";
                    }

                case "轴运动到上料位":
                    AxisGoto(POINT.上料位, new AXIS[] { AXIS.翻转U轴 }, 100, true);
                    WaitAxis(AXIS.翻转U轴);
                    // 翻转通知IR检测上料：T
                    RegBit.翻转通知IR检测上料.Set(true);
                    return "等待IR检测通知翻转有料";

                case "等待IR检测通知翻转有料":
                    if (RegBit.IR检测通知翻转有料.Get())
                    {
                        // IR信号收到，复位信号，打开真空
                        RegBit.IR检测通知翻转有料.Set(false);
                        DO.翻转吸真空.Set(true);
                        DO.翻转破真空.Set(false);
                        TakeinTime = DateTime.Now;
                        return "等待上料位吸嘴真空";
                    }
                    return "等待IR检测通知翻转有料";

                case "等待上料位吸嘴真空":
                    // 空跑模式：跳过真空检测
                    if (Process == AutoProcess.空跑 || DI.翻转吸真空到位.Get())
                    {
                        Delay(200);
                        RegBit.翻转通知IR检测上料完成.Set(true);
                        return "轴运动到下料位";
                    }
                    if ((DateTime.Now - TakeinTime).TotalMilliseconds > 3000)
                    {
                        if (RegBit.IR载具清料.Get())
                        {
                            RegBit.翻转通知IR检测上料完成.Set(true);
                            return "轴运动到下料位";
                        }
                        if (0 == ShowMessage("确认翻转取料效果，确认‘继续等待’还是‘重新上料’", "提示", new string[] { "继续等待", "重新上料" }))
                        {
                            TakeinTime = DateTime.Now;
                            return "等待上料位吸嘴真空";
                        }
                        DO.翻转吸真空.Set(false);
                        DO.翻转破真空.Set(false);
                        RegBit.翻转通知IR检测上料完成.Set(true);
                        TakeinTime = DateTime.Now;
                        return "生产流程开始";
                    }
                    return "等待上料位吸嘴真空";

                case "轴运动到下料位":
                    AxisGoto(POINT.下料位, new AXIS[] { AXIS.翻转U轴 }, 100, true);
                    WaitAxis(AXIS.翻转U轴);
                    if (Parameter.GetBit("调试模式"))
                    {
                        if (0 == ShowMessage("人工确认翻转接料效果，确认‘重新接料’还是‘下一步’", "提示", new string[] { "重新接料", "下一步" }))
                        {
                            return "轴运动到上料位";
                        }
                    }
                   
                    // 翻转通知移载有料：T
                    RegBit.翻转通知移载有料.Set(true);
                    WaitRegBit(RegBit.移载通知翻转吸真空, true);
                    RegBit.移载通知翻转吸真空.Set(false);
                    DO.翻转吸真空.Set(false);
                    DO.翻转破真空.Set(false);
                    return "等待移载通知翻转取料完成";

                case "等待移载通知翻转取料完成":

                    if (RegBit.移载通知翻转取料完成.Get())
                    {
                        if (RegBit.IR载具清料.Get())
                        {
                            this[NEST.穴位1].BreakDownTime += 3;
                            this[NEST.穴位2].BreakDownTime += 3;
                            this[NEST.穴位3].BreakDownTime += 3;
                            this[NEST.穴位4].BreakDownTime += 3;
                        }
                        //数据赋值给移载
                        Logger.Process(this.Name, $"{Name},穴位1，结果：{this[NEST.穴位1].Result}，NG原因：{this[NEST.穴位1].NGReason}");
                        Logger.Process(this.Name, $"{Name},穴位2，结果：{this[NEST.穴位2].Result}，NG原因：{this[NEST.穴位2].NGReason}");
                        Logger.Process(this.Name, $"{Name},穴位3，结果：{this[NEST.穴位3].Result}，NG原因：{this[NEST.穴位3].NGReason}");
                        Logger.Process(this.Name, $"{Name},穴位4，结果：{this[NEST.穴位4].Result}，NG原因：{this[NEST.穴位4].NGReason}");
                        "移载".GetStation<移载>()[移载.NEST.穴位1] = this[NEST.穴位1];
                        "移载".GetStation<移载>()[移载.NEST.穴位2] = this[NEST.穴位2];
                        "移载".GetStation<移载>()[移载.NEST.穴位3] = this[NEST.穴位3];
                        "移载".GetStation<移载>()[移载.NEST.穴位4] = this[NEST.穴位4];

                        this[NEST.穴位1] = null;
                        this[NEST.穴位2] = null;
                        this[NEST.穴位3] = null;
                        this[NEST.穴位4] = null;
                        // 收到移载完成信号，复位握手位
                        RegBit.移载通知翻转取料完成.Set(false);
                        // 再次打开吸真空，为下一轮循环做准备
                        DO.翻转吸真空.Set(true);
                        DO.翻转破真空.Set(false);
                        // 回到流程起点循环执行
                        return "轴运动到上料位";
                    }
                    return "等待移载通知翻转取料完成";

                default:
                    return null;
            }
        }

        protected override string CalibRun(string strStep)
        {
            switch (strStep)
            {
                case "相机标定开始":
                    Delay(10);
                    return "相机标定开始";
                default:
                    return null;
            }
        }

        protected override string GrrRun(string strStep)
        {
            switch (strStep)
            {
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
        #endregion
    }
    public static partial class HandleEx
    {
        public static double GetPos(this 翻转.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this 翻转.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}

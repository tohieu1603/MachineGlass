using AF.Card;
using AF.Common;
using AF.Data;
using AF.Station;
using Autoframe;
using Inovance.InoMotionCotrollerShop.InoServiceContract.EtherCATConfigApi;
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
    public class 摆臂站 : StationEx
    {

        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public 摆臂站(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<NEST, ProductData>();
        }
        Stopwatch sw = Stopwatch.StartNew();
        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { DI.摆臂吸真空感应 };
            //配置站位界面显示输出
            arrayDO = new object[] { DO.摆臂取料吸真空, DO.摆臂取料破真空 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { };
            //配置站位轴
            arrayAxis = new object[] { AXIS.摆臂上下Z轴, AXIS.摆臂旋转U轴, AXIS.顶针上下Z轴 };
        }

        private double 空跑取料位 = 0;

        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            待机位,
            取料位,
            取料上抬位,
            放料位,
            放料上抬位,
            顶针伸出位,
            顶针缩回位,
            CCD2标定从IR1取料位
        }

        /// <summary>
        /// 产品穴位，与产品是一对一
        /// </summary>
        public enum NEST
        {
            穴位
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
            switch (Process)
            {
                case AutoProcess.生产:
                case AutoProcess.空跑:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "生产流程开始";
                    break;
                case AutoProcess.CCD1标定注册:
                case AutoProcess.CCD2标定注册:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = StepEmpty;
                    break;
                case AutoProcess.扩膜站相机动静态:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = StepExit;
                    break;
                case AutoProcess.CCD3静态:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = StepExit;
                    break;
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

        int aaa=0; int bbb=0;

        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case "初始化流程开始":
                    //if (!this[NEST.穴位].Result)
                    //{
                    //    aaa++;
                    //    return "初始化流程开始";
                    //}

                    //ProductData sss = this[NEST.穴位];
                    //return StepExit;
                    return "轴上使能";

                case "轴上使能":
                    AXIS.摆臂旋转U轴.ServoOn(false);
                    AXIS.顶针上下Z轴.ServoOn(false);
                    Delay(1000);
                    AXIS.摆臂上下Z轴.ServoOn(true);
                    AXIS.摆臂旋转U轴.ServoOn(true);
                    AXIS.顶针上下Z轴.ServoOn(true);
                    Delay(1000);

                    UpdatePtpTgtPos(AXIS.摆臂旋转U轴, AXIS.摆臂旋转U轴.GetPos());
                    //AXIS.摆臂旋转U轴.AbsMove(POINT.待机位.GetPos(AXIS.摆臂旋转U轴, this), 100);
                    //Delay(100);
                    //UpdatePtpTgtPos(AXIS.摆臂旋转U轴, POINT.取料位.GetPos(AXIS.摆臂旋转U轴, this));
                    //AXIS.摆臂旋转U轴.GetHandle().TargetPos = POINT.取料位.GetPos(AXIS.摆臂旋转U轴, this);
                    //WaitAxis(AXIS.摆臂旋转U轴);
                    //return StepExit;

                    return "摆臂上下Z轴运动到待机位";


                case "摆臂上下Z轴运动到待机位":
                    AxisSingleTo(POINT.待机位, AXIS.摆臂上下Z轴, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.摆臂上下Z轴);
                    RegBit.摆臂站初始化Z轴已抬起.Set(true);
                    return "等待扩膜站初始化完成";

                case "等待扩膜站初始化完成":
                    if (RegBit.扩膜初始化通知摆臂回原.Get())
                        return "摆臂吸嘴真空打开";
                    return "等待扩膜站初始化完成";

                case "摆臂吸嘴真空打开":
                    AXIS.顶针上下Z轴.Home();
                    WaitAxis(AXIS.顶针上下Z轴);
                    AxisSingleTo(POINT.顶针缩回位, AXIS.顶针上下Z轴);
                    WaitAxis(AXIS.顶针上下Z轴);
                    DO.摆臂取料破真空.Set(false);
                    DO.摆臂取料吸真空.Set(true);
                    Delay(500);
                    return "摆臂旋转U轴运动到待机位";

                case "摆臂旋转U轴运动到待机位":
                    AxisSingleTo(POINT.待机位, AXIS.摆臂旋转U轴, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.摆臂旋转U轴);
                    return "检查吸嘴是否有残余物料";

                case "检查吸嘴是否有残余物料":
                    Delay(1000);
                    if (DI.摆臂吸真空感应.Get())
                    {
                        if(!(this[NEST.穴位] == null))
                        {
                            RegBit.摆臂站初始化有料.Set(true);
                            return "初始化完成";
                        }
                        return "提示人工清理吸嘴残余物料";
                    }
                    DO.摆臂取料吸真空.Set(false);
                    return "初始化完成";

                case "提示人工清理吸嘴残余物料":
                    ShowMessage("摆臂吸嘴有残余物料，请拿走");
                    return "检查吸嘴是否有残余物料";

                case "初始化完成":
                    RegBit.摆臂站初始化完成.Set(true);
                    //结束初始化
                    return StepExit;
                default:
                    return null;
            }
        }

        DateTime TakeinTime = DateTime.Now;
        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case "生产流程开始":
                    if (RegBit.摆臂站初始化有料.Get())
                    {
                        RegBit.摆臂站初始化有料.Set(false);
                        AxisSingleTo(POINT.放料位, AXIS.摆臂旋转U轴);
                        WaitAxis(AXIS.摆臂旋转U轴);
                        return "放料前检查物料是否取起";
                    }
                    AxisSingleTo(POINT.待机位, AXIS.摆臂旋转U轴);
                    return "等待扩膜站通知取料";

                case "等待扩膜站通知取料":
                    if (RegBit.机械手申请清载具.Get())
                    {
                        WaitAxis(AXIS.摆臂旋转U轴);
                        RegBit.IR载具清料.Set(true);
                        return StepExit;
                    }
                    if (RegBit.扩膜通知摆臂取料.Get())
                    {
                        RegBit.扩膜允许清料.Set(false);
                        if (RegBit.摆臂停止上料.Get())
                            return StepExit;
                        RegBit.扩膜通知摆臂取料.Set(false);
                        UpdatePtpTgtPos(AXIS.摆臂旋转U轴, POINT.取料位.GetPos(AXIS.摆臂旋转U轴, this));
                        AXIS.摆臂旋转U轴.GetHandle().TargetPos = POINT.取料位.GetPos(AXIS.摆臂旋转U轴, this);
                        //AxisSingleTo(POINT.顶针伸出位, AXIS.顶针上下Z轴, 100);
                        WaitAxis(AXIS.摆臂旋转U轴);
                        return "摆臂吸嘴打开真空";
                    }
                    RegBit.摆臂允许清料.Set(true);
                    return "等待扩膜站通知取料";

                case "摆臂吸嘴打开真空":
                    //return StepExit;
                    //DO.摆臂取料吸真空.Set(true);
                    return "摆臂上下Z轴下降取料";

                case "摆臂上下Z轴下降取料":

                    //WaitAxis(AXIS.顶针上下Z轴);
                    Delay(150);
                    sw.Start();
                    if (IsDryRun)
                    {
                        空跑取料位 = POINT.取料位.GetPos(AXIS.摆臂上下Z轴, this) - 45;
                        AXIS.摆臂上下Z轴.AbsMove(空跑取料位, 100);
                    }
                    else
                    {
                        AxisSingleTo(POINT.取料位, AXIS.摆臂上下Z轴);
                    }
                    WaitAxis(AXIS.摆臂上下Z轴);
                    Delay(30);
                    AxisSingleTo(POINT.顶针伸出位, AXIS.顶针上下Z轴, 100);
                    WaitAxis(AXIS.顶针上下Z轴);
                    DO.摆臂取料吸真空.Set(true);
                    Delay(50);
                    TakeinTime = DateTime.Now;
                    sw.Stop();
                    double ddd = sw.Elapsed.TotalMilliseconds;
                    Logger.Process(this.Name, $"摆臂下降取料{ddd:F3}ms");
                    return "摆臂取料真空检测";

                case "摆臂取料真空检测":
                    if (IsDryRun)
                    {
                        return "摆臂Z轴取料后到上抬位";
                    }
                    if (DI.摆臂吸真空感应.Get())
                    {
                        Delay(100);
                        sw.Stop();
                        double dddd = sw.Elapsed.TotalMilliseconds;
                        Logger.Process(this.Name, $"摆臂下降取料{dddd:F3}ms");
                        return "摆臂Z轴取料后到上抬位";
                    }
                    if ((DateTime.Now - TakeinTime).TotalMilliseconds > 3000)
                    {
                        if (0 == ShowMessage("确认摆臂取料效果，确认‘继续等待’还是‘下一片’", "提示", new string[] { "继续等待", "下一片" }))
                        {
                            TakeinTime = DateTime.Now;
                            return "摆臂取料真空检测";
                        }
                        DO.摆臂取料吸真空.Set(false);
                        AxisSingleTo(POINT.待机位, AXIS.摆臂上下Z轴, 100);
                        AxisSingleTo(POINT.顶针缩回位, AXIS.顶针上下Z轴, 100);
                        WaitAxis(AXIS.摆臂上下Z轴);
                        WaitAxis(AXIS.顶针上下Z轴);
                        TakeinTime = DateTime.Now;
                        return "摆臂旋转U轴运动到待机位";
                    }
                    return "摆臂取料真空检测";

                case "摆臂Z轴取料后到上抬位":
                    sw.Start();
                    AxisSingleTo(POINT.取料上抬位, AXIS.摆臂上下Z轴, 15);
                    WaitAxis(AXIS.摆臂上下Z轴);
                    sw.Stop();
                    Logger.Process(this.Name, $"摆臂取料上抬{sw.Elapsed.TotalMilliseconds:F3}ms");
                    return "摆臂上下Z轴取料后到待机位";

                case "摆臂上下Z轴取料后到待机位":
                    sw.Start();
                    AxisSingleTo(POINT.待机位, AXIS.摆臂上下Z轴, 100);
                    WaitAxis(AXIS.摆臂上下Z轴);
                    Delay(50);
                    AxisSingleTo(POINT.顶针缩回位, AXIS.顶针上下Z轴, 100);
                    WaitAxis(AXIS.顶针上下Z轴);


 
                    sw.Stop();
                    Logger.Process(this.Name, $"摆臂取料上抬{sw.Elapsed.TotalMilliseconds:F3}ms");
                    return "检查物料是否取起";

                case "检查物料是否取起":
                    if (!DI.摆臂吸真空感应.Get() && !IsDryRun)
                    {
                        int Sw = ShowMessage("摆臂取物料失败，请检查真空异常还是未取其，然后点击‘继续’或者‘下一片’", "提示", new string[] { "放料", "取料", "下一片" });
                        "扩膜站".GetStation<扩膜站>()[扩膜站.NEST.穴位].BreakDownTime += Duration;
                        if (2 == Sw)
                        {
                            return "摆臂旋转U轴运动到待机位";
                        }
                        else if (1 == Sw)
                        {
                            //AxisSingleTo(POINT.顶针伸出位, AXIS.顶针上下Z轴, 100);
                            DO.摆臂取料吸真空.Set(false);
                            return "摆臂上下Z轴下降取料";
                        }

                    }

                    return "摆臂旋转U轴运动到放料位";

                case "摆臂旋转U轴运动到待机位":
                    AxisSingleTo(POINT.待机位, AXIS.摆臂旋转U轴);
                    WaitAxis(AXIS.摆臂旋转U轴);
                    RegBit.摆臂通知扩膜取料完成.Set(true);
                    ShowMessage("摆臂吸嘴有残余物料，请拿走");
                    DO.摆臂取料吸真空.Set(false);
                    return "等待扩膜站通知取料";

                case "摆臂旋转U轴运动到放料位":
                    AxisSingleTo(POINT.放料位, AXIS.摆臂旋转U轴);
                    //Delay(30);
                    RegBit.摆臂通知扩膜取料完成.Set(true);
                    WaitAxis(AXIS.摆臂旋转U轴);
                    return "等待扩膜数据传递完成";
                case "等待扩膜数据传递完成":
                    if (RegBit.摆臂通知扩膜取料完成.Get())
                        return "等待扩膜数据传递完成";
                    return "放料前检查物料是否取起";

                case "放料前检查物料是否取起":
                    if (!DI.摆臂吸真空感应.Get() && !IsDryRun)
                    {
                        int Sw = ShowMessage("摆臂取物料失败，请检查真空异常还是未取其", "提示", new string[] { "放料",  "下一片" });
                        "扩膜站".GetStation<扩膜站>()[扩膜站.NEST.穴位].BreakDownTime += Duration;
                        if (1 == Sw)
                        {
                            AxisSingleTo(POINT.待机位, AXIS.摆臂旋转U轴);
                            return "生产流程结束";
                        }
                    }
                    return "等待IR检测通知摆臂接料准备OK";


                case "等待IR检测通知摆臂接料准备OK":
                    if (RegBit.IR检测1通知摆臂接料准备OK.Get())
                    {
                        RegBit.IR检测1通知摆臂接料准备OK.Set(false);
                        return "摆臂上下Z轴下降放料";
                    }
                    if (RegBit.IR检测2通知摆臂接料准备OK.Get())
                    {
                        RegBit.IR检测2通知摆臂接料准备OK.Set(false);
                        return "摆臂上下Z轴下降放料";
                    }
                    return "等待IR检测通知摆臂接料准备OK";

                case "摆臂上下Z轴下降放料":
                    sw.Start();
                    AxisSingleTo(POINT.放料位, AXIS.摆臂上下Z轴, 100);
                    WaitAxis(AXIS.摆臂上下Z轴);
                    sw.Stop();
                    RegBit.摆臂通知IR检测到位放料.Set(true);
                    Logger.Process(this.Name, $"摆臂放料下降{sw.Elapsed.TotalMilliseconds:F3}ms");
                    return "摆臂关闭吸真空并打开破真空";

                case "摆臂关闭吸真空并打开破真空":
                    DO.摆臂取料吸真空.Set(false);
                    DO.摆臂取料破真空.Set(true);
                    Delay(70);
                    DO.摆臂取料破真空.Set(false);
                    return "等待摆臂吸嘴没有真空";
                case "等待摆臂吸嘴没有真空":
                    if (!DI.摆臂吸真空感应.Get())
                    {
                        RegBit.摆臂通知IR检测到位放料.Set(true);
                        return "摆臂上下Z轴放料上抬位";
                    }
                    return "等待摆臂吸嘴没有真空";

                case "摆臂上下Z轴放料上抬位":
                    sw.Start();
                    AxisSingleTo(POINT.放料上抬位, AXIS.摆臂上下Z轴, 15);
                    DO.摆臂取料破真空.Set(false);
                    WaitAxis(AXIS.摆臂上下Z轴);
                    sw.Stop();
                    Logger.Process(this.Name, $"摆臂放料上抬{sw.Elapsed.TotalMilliseconds:F3}ms");
                    return "摆臂上下Z轴放料后抬起";

                case "摆臂上下Z轴放料后抬起":
                    sw.Start();
                    AxisSingleTo(POINT.待机位, AXIS.摆臂上下Z轴, 100);
                    DO.摆臂取料破真空.Set(false);
                    WaitAxis(AXIS.摆臂上下Z轴);

                    sw.Stop();
                    Logger.Process(this.Name, $"摆臂放料上抬{sw.Elapsed.TotalMilliseconds:F3}ms");
                    DO.摆臂取料破真空.Set(false);
                    return "摆臂站放料完成";

                case "摆臂站放料完成":
                    RegBit.摆臂通知IR检测放料完成.Set(true);
                    if (RegBit.扩膜通知摆臂取料.Get())
                        return "生产流程结束";
                    AxisSingleTo(POINT.待机位, AXIS.摆臂旋转U轴);
                    return "生产流程结束";

                case "生产流程结束":
                    return "等待扩膜站通知取料";
            }
            return null;
        }

        protected override string CalibRun(string strStep)
        {
            switch (strStep)
            {
                case "等待开始标定":
                    if (RegBit.扩膜开始标定.Get())
                        return "判断是否为CCD2基准标定";
                    return "等待开始标定";
                case "判断是否为CCD2基准标定":
                    if (RegInt.相机功能选择.Get() == 2)
                        return "摆臂旋转U轴运动到放料位";
                    return StepExit;

                case "摆臂旋转U轴运动到放料位":
                    AxisSingleTo(POINT.放料位, AXIS.摆臂旋转U轴, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.摆臂旋转U轴);
                    //RegBit.摆臂通知扩膜取料完成.Set(true);
                    return "等待IR检测通知摆臂接料准备OK";
                case "等待IR检测通知摆臂接料准备OK":
                    WaitRegBit(RegBit.IR检测1通知摆臂接料准备OK, true);
                    RegBit.IR检测1通知摆臂接料准备OK.Set(false);
                    return "摆臂上下Z轴下降放料";

                case "摆臂上下Z轴下降放料":
                    sw.Start();
                    AxisSingleTo(POINT.放料位, AXIS.摆臂上下Z轴, 100);
                    WaitAxis(AXIS.摆臂上下Z轴);
                    sw.Stop();
                    Logger.Process(this.Name, $"摆臂放料下降{sw.Elapsed.TotalMilliseconds:F3}ms");
                    return "摆臂关闭吸真空并打开破真空";

                case "摆臂关闭吸真空并打开破真空":
                    DO.摆臂取料吸真空.Set(true);
                    DO.摆臂取料破真空.Set(false);
                    Delay(50);
                    return "摆臂上下Z轴放料后抬起";

                case "摆臂上下Z轴放料后抬起":
                    sw.Start();
                    AxisSingleTo(POINT.待机位, AXIS.摆臂上下Z轴, 100);
                    WaitAxis(AXIS.摆臂上下Z轴);
                    sw.Stop();
                    Logger.Process(this.Name, $"摆臂放料上抬{sw.Elapsed.TotalMilliseconds:F3}ms");
                    return "等待扩膜标定允许摆臂放料";
                case "等待扩膜标定允许摆臂放料":
                    if (RegBit.扩膜标定允许摆臂放料.Get())
                        return "摆臂上下U轴到取料位";
                    return "等待扩膜标定允许摆臂放料";
                case "摆臂上下U轴到取料位":
                    AxisSingleTo(POINT.取料位, AXIS.摆臂旋转U轴, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.摆臂旋转U轴);
                    return "摆臂上下Z轴下降取料位";
                case "摆臂上下Z轴下降取料位":
                    sw.Start();
                    AxisSingleTo(POINT.取料位, AXIS.摆臂上下Z轴, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.摆臂上下Z轴);
                    sw.Stop();
                    double ddd = sw.Elapsed.TotalMilliseconds;
                    Logger.Process(this.Name, $"摆臂下降取料{ddd:F3}ms");
                    DO.摆臂取料吸真空.Set(false);
                    DO.摆臂取料破真空.Set(true);
                    Delay(50);
                    return "摆臂上下Z轴取料后抬起";
                case "摆臂上下Z轴取料后抬起":
                    sw.Start();
                    AxisSingleTo(POINT.待机位, AXIS.摆臂上下Z轴, 100);
                    WaitAxis(AXIS.摆臂上下Z轴);
                    sw.Stop();
                    Logger.Process(this.Name, $"摆臂取料上抬{sw.Elapsed.TotalMilliseconds:F3}ms");
                    return "摆臂旋转U轴运动到待机位";
                case "摆臂旋转U轴运动到待机位":
                    AxisSingleTo(POINT.待机位, AXIS.摆臂旋转U轴, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.摆臂旋转U轴);
                    DO.摆臂取料吸真空.Set(false);
                    DO.摆臂取料破真空.Set(false);
                    RegBit.摆臂标定通知扩膜放料完成.Set(true);
                    return StepExit;
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
        public static double GetPos(this 摆臂站.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this 摆臂站.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}


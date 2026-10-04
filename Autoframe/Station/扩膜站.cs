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
using static Autoframe.扩膜站;

namespace Autoframe
{
    public class 扩膜站 : StationEx
    {
        private TcpCommu_CCD1 camera拍钢环相机;

        private TcpCommu_CCD2 camera拍钢环内产品相机 = new TcpCommu_CCD2();

        private double d列间距 = 0.4; // 列间距 (mm)
        private double d行间距 = 0.2; // 行间距 (mm)
        private const int n最大列数 = 4;      // 最大列数
        private const int n最大行数 = 3;      // 最大行数

        private int n物料总数 = 12;

        private string[] str相机返回数据 = null;

        private int n拍照次数 = 0;
        private int n摆臂取料 = 0;

        private int n拍照NG次数 = 0;

        private double d拍照基准点X = 0;
        private double d拍照基准点Y = 0;
        private double d拍照基准点U = 0;

        private PointD P00 = new PointD(0, 0);
        private PointD P0End = new PointD(0, 0);
        private PointD PLast0 = new PointD(0, 0);
        private List<PointD> SnapPoints = new List<PointD>();
        private bool b第一次拍物料 = false;
        private bool 走ccd2调整点 = false;
        private PointD Pnow = new PointD(0, 0);
        Location 物料位置 = new Location();
        private double 弹窗时长 = 0;

        /// <summary>
        /// 相机连接结果
        /// </summary>
        bool bConnectResult = true;
        /// <summary>
        /// 下一个拍照位U轴
        /// </summary>
        double UCCD2Site = 0;

        int n标定移动次数;
        double d九点标定步长 = 3;
        double d旋转标定角度 = 10;

        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public 扩膜站(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<NEST, ProductData>();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { DI.顶针吸真空感应 };
            //配置站位界面显示输出
            arrayDO = new object[] { DO.顶针吸真空, DO.顶针破真空 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { Cylinder.钢环固定气缸1, Cylinder.钢环固定气缸2 };
            //配置站位轴
            arrayAxis = new object[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜Z轴, AXIS.扩膜U轴, AXIS.顶针模组Z轴 };
        }

        ProductData productData = new ProductData();


        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            换钢环位,
            钢环拍照位,
            物料拍照位,
            扩膜位,
            吸产品位,
            待机位,
            料盘点1,
            料盘点2,
            料盘点3,
            CCD1动静态位,
            CCD2动静态位,
            CCD2标定9点中心,
            CCD1标定9点中心,
            CCD2标定基准点,
            刚环临时测试点
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
                    this.AutoEntry = "相机标定注册开始";
                    break;
                case AutoProcess.扩膜站相机动静态:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "相机动静态开始";
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



        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case "初始化流程开始":
                    //for (int i = 0; i < 2; i++)
                    //{
                    //    this[NEST.穴位] = new ProductData();
                    //    this[NEST.穴位].DtStartTime = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss.fff"));
                    //    this[NEST.穴位].Result = i==1;
                    //    "摆臂站".GetStation<摆臂站>()[摆臂站.NEST.穴位] = this[NEST.穴位];
                    //    this[NEST.穴位] = null;
                    //}

                    //return StepExit;
                    n摆臂取料 = 0;
                    bConnectResult = true;
                    n物料总数 = n最大列数 * n最大行数;
                    str相机返回数据 = new string[] { };
                    n拍照次数 = 0;
                    n拍照NG次数 = 0;
                    物料位置 = new Location();
                    d拍照基准点X = 0;
                    d拍照基准点Y = 0;
                    d拍照基准点U = 0;
                    d行间距 = 计算料盘行列间距()[0];
                    d列间距 = 计算料盘行列间距()[1];
                    n标定移动次数 = 0;
                    d九点标定步长 = 5;
                    d旋转标定角度 = 10;
                    b第一次拍物料 = true;
                    return "连接CCD1";

                case "连接CCD1":
                    camera拍钢环相机 = new TcpCommu_CCD1();
                    camera拍钢环相机.Init(this, 1);

                    return "检查CCD1相机是否连接成功";

                case "检查CCD1相机是否连接成功":
                    if (!bConnectResult)
                    {
                        ShowMessage("CCD1连接失败，请检查处理后点‘确定’重新连接 ".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans));
                        return "连接CCD1";
                    }
                    return "连接CCD2";

                case "连接CCD2":
                    camera拍钢环内产品相机 = new TcpCommu_CCD2();
                    camera拍钢环内产品相机.Init(this, 2);

                    return "检查CCD2相机是否连接成功";

                case "检查CCD2相机是否连接成功":
                    if (!bConnectResult)
                    {
                        ShowMessage("CCD2连接失败，请检查处理后点‘确定’重新连接 ".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans));
                        return "连接CCD2";
                    }
                    return "等待拉料站和摆臂站初始化完成";

                case "等待拉料站和摆臂站初始化完成":
                    if (RegBit.拉料站初始化完成.Get() && RegBit.摆臂站初始化Z轴已抬起.Get())
                        return "关闭顶针吸真空";
                    return "等待拉料站和摆臂站初始化完成";

                case "关闭顶针吸真空":
                    DO.顶针吸真空.Set(false);
                    DO.顶针破真空.Set(false);
                    return "轴上使能";

                case "轴上使能":
                    AllServoOn(this, true);
                    Delay(1000);
                    return "顶针模组Z轴运动到待机位";

                case "顶针模组Z轴运动到待机位":
                    AxisSingleTo(POINT.待机位, AXIS.顶针模组Z轴);
                    WaitAxis(AXIS.顶针模组Z轴);
                    return "扩膜Z轴运动到扩膜位";



                case "扩膜Z轴运动到扩膜位":
                    AxisSingleTo(POINT.扩膜位, AXIS.扩膜Z轴);
                    WaitAxis(AXIS.扩膜Z轴);
                    return "扩膜XY轴回原";

                case "扩膜XY轴回原":
                    AXIS.扩膜X轴.Home();
                    AXIS.扩膜Y轴.Home();
                    WaitAxis(AXIS.扩膜X轴);
                    WaitAxis(AXIS.扩膜Y轴);
                    return "扩膜平台XYU移动到换钢环位";

                case "扩膜平台XYU移动到换钢环位":
                    AxisGoto(POINT.换钢环位, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜U轴 });
                    WaitAxis(AXIS.扩膜X轴);
                    WaitAxis(AXIS.扩膜Y轴);
                    WaitAxis(AXIS.扩膜U轴);
                    RegBit.扩膜初始化通知摆臂回原.Set(true);
                    WaitRegBit(RegBit.摆臂站初始化完成, true);

                    return "钢环固定气缸缩回";


                case "钢环固定气缸缩回":
                    AxisGoto(POINT.换钢环位, new AXIS[] { AXIS.扩膜Z轴 });
                    WaitAxis(AXIS.扩膜Z轴);
                    CylinderOut(Cylinder.钢环固定气缸1);
                    CylinderOut(Cylinder.钢环固定气缸2);
                    return "提示人工检查扩膜平台是否有钢环";

                case "提示人工检查扩膜平台是否有钢环":
                    ShowMessage("请检查扩膜平台是否有钢环，如有，请拿走".txtTrans(TransType.Trans));
                    return "初始化完成";

                case "初始化完成":
                    弹窗时长 = 0;
                    RegBit.扩膜站初始化完成.Set(true);
                    //结束初始化
                    return StepExit;
                default:
                    return null;
            }
        }

        /// <summary>
        /// CC2拍照失败次数
        /// </summary>
        private int CCD2NG = 0;
        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case "生产流程开始":
                    n拍照次数 = 0;
                    return "扩膜站通知拉料站上钢环";

                case "扩膜站通知拉料站上钢环":
                    AxisGoto(POINT.换钢环位, new AXIS[] { AXIS.扩膜Z轴 });
                    WaitAxis(AXIS.扩膜Z轴);
                    if (RegBit.IR载具清料.Get())
                    {
                        return StepExit;
                    }
                    RegBit.扩膜通知拉料站上钢环.Set(true);
                    return "等待拉料站上钢环完成";

                case "等待拉料站上钢环完成":
                    WaitRegBit(RegBit.拉料通知扩膜上料完成, true);
                    RegBit.拉料通知扩膜上料完成.Set(false);
                    //return "钢环固定气缸伸出";
                    return "扩膜Z轴移动到扩膜点";

                //case "钢环固定气缸伸出":
                //    Cylinder.钢环固定气缸1.Out();
                //    Cylinder.钢环固定气缸2.Out();
                //    WaitCylOut(Cylinder.钢环固定气缸1);
                //    WaitCylOut(Cylinder.钢环固定气缸2);
                //    return "扩膜Z轴移动到扩膜点";

                case "扩膜Z轴移动到扩膜点":
                    AxisSingleTo(POINT.扩膜位, AXIS.扩膜Z轴);
                    WaitAxis(AXIS.扩膜Z轴);
                    return "扩膜平台移动到钢环拍照位";

                case "扩膜平台移动到钢环拍照位":
                    AxisGoto(POINT.钢环拍照位, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜U轴 });
                    WaitAxis(AXIS.扩膜X轴);
                    WaitAxis(AXIS.扩膜Y轴);
                    WaitAxis(AXIS.扩膜U轴);
                    return "CCD1拍钢环";

                case "CCD1拍钢环":
                    RegBit.扩膜允许清料.Set(false);
                    if (IsDryRun)
                    {
                        return "判断拍钢环结果";
                    }
                    #region 二次拍照
                    //--------------二次拍照-------------
                    for (int i = 0; i < 2; i++)
                    {
                        camera拍钢环相机.SendCmd(out str相机返回数据, (int)TcpCommu_CCD1.Command.CCD1_上料排片引导, true, AXIS.扩膜X轴.GetPos().ToString(),
                       AXIS.扩膜Y轴.GetPos().ToString(), AXIS.扩膜U轴.GetPos().ToString());
                        if (this.camera拍钢环相机.d上料引导位置U < AXIS.扩膜U轴.GetPos() - 10 || this.camera拍钢环相机.d上料引导位置U > AXIS.扩膜U轴.GetPos() + 10)
                        {
                            if (0 == ShowMessage("CCD1引导角度超限".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "重拍".txtTrans(TransType.Trans), "退出".txtTrans(TransType.Trans) }))
                                return "CCD1拍钢环";
                            return StepExit;
                        }
                        else if (i == 0)
                        {
                            d拍照基准点U = this.camera拍钢环相机.d上料引导位置U;
                            AXIS.扩膜U轴.AbsMove(d拍照基准点U);
                            WaitAxis(AXIS.扩膜U轴);
                            Delay(500);
                        }
                    }
                    #endregion

                    //camera拍钢环相机.SendCmd(out str相机返回数据, (int)TcpCommu_CCD1.Command.CCD1_上料排片引导, true, AXIS.扩膜X轴.GetPos().ToString(),
                    //    AXIS.扩膜Y轴.GetPos().ToString(), AXIS.扩膜U轴.GetPos().ToString());

                    return "判断拍钢环结果";

                case "判断拍钢环结果":
                    走ccd2调整点 = false;
                    if (IsNormalRun)
                    {
                        if (!camera拍钢环相机.result)
                        {
                            if (0 == ShowMessage("CCD1拍钢环失败，请检查后，确认‘重拍’还是‘换料’".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "重拍".txtTrans(TransType.Trans), "换料".txtTrans(TransType.Trans) }))
                            {
                                return "CCD1拍钢环";
                            }
                            else
                            {
                                return "扩膜平台移动到换钢环位";
                            }

                        }
                        P00 = new PointD(this.camera拍钢环相机.d上料引导1位置X, this.camera拍钢环相机.d上料引导1位置Y);
                        P0End = new PointD(this.camera拍钢环相机.d上料引导2位置X, this.camera拍钢环相机.d上料引导2位置Y);
                        PLast0 = new PointD(this.camera拍钢环相机.d上料引导3位置X, this.camera拍钢环相机.d上料引导3位置Y);
                        UCCD2Site = this.camera拍钢环相机.d上料引导位置U;
                        int RowD = this.camera拍钢环相机.d上料行数;
                        int ColD = this.camera拍钢环相机.d上料列数;
                        RegInt.RowCCD1.Set(RowD);
                        RegInt.ColCCD1.Set(ColD);
                        RegBit.扩膜CCD1拍照完成状态.Set(true);
                        b第一次拍物料 = true;
                        //if (b第一次拍物料)
                        //{
                        //    b第一次拍物料 = false;
                        //    RingMonitor.ResetAll();
                        //    RingMonitor.SetRingLayout(RegInt.RowCCD1.Get(), RegInt.ColCCD1.Get(),  RegInt.PickRingSite.Get(), IsDryRun);
                        //}
                        //else
                        //{
                        //    Delay(100);
                        //    RegBit.扩膜CCD1拍照完成状态.Set(true);
                        //}
                        if (IsNormalRun)
                            n物料总数 = RowD * ColD;
                        SnapPoints = GenerateTraySnakePickList(P00, P0End, PLast0, RowD, ColD);
                        if (P00.X < 130 && P0End.X < 130 && PLast0.X < 130 && -19 < P00.X && -19 < P0End.X && -19 < PLast0.X &&
                            P00.Y > 20 && P0End.Y > 20 && PLast0.Y > 20 && P00.Y < 147 && P0End.Y < 147 && PLast0.Y < 147)
                        {
                        }
                        else
                        {
                            if (0 == ShowMessage("CCD1拍钢环引导超限，请检查后，确认‘重拍’还是‘换料’".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "重拍".txtTrans(TransType.Trans), "换料".txtTrans(TransType.Trans) }))
                            {
                                //n拍照次数--;
                                return "CCD1拍钢环";
                            }
                            else
                            {
                                return "扩膜平台移动到换钢环位";
                            }
                        }
                        return "获取拍钢环数据";
                    }
                    return "获取拍钢环数据";

                case "获取拍钢环数据":
                    if (IsDryRun)
                    {
                        d拍照基准点X = POINT.物料拍照位.GetPos(AXIS.扩膜X轴, this);
                        d拍照基准点Y = POINT.物料拍照位.GetPos(AXIS.扩膜Y轴, this);
                        d拍照基准点U = POINT.物料拍照位.GetPos(AXIS.扩膜U轴, this);
                    }
                    else
                    {
                        d拍照基准点X = P00.X;
                        d拍照基准点Y = P00.Y;
                        d拍照基准点U = this.camera拍钢环相机.d上料引导位置U;
                    }

                    //接收相机返回钢环物料列数，
                    return "钢环位置矫正";

                case "钢环位置矫正":

                    if (d拍照基准点X < 130 && -19 < d拍照基准点X && d拍照基准点Y > 23 && d拍照基准点Y < 147)
                    {
                        AXIS.扩膜X轴.AbsMove(d拍照基准点X, AxisHandle.SpeedPercent);
                        AXIS.扩膜Y轴.AbsMove(d拍照基准点Y, AxisHandle.SpeedPercent);
                        AXIS.扩膜U轴.AbsMove(d拍照基准点U, AxisHandle.SpeedPercent);
                        WaitAxis(AXIS.扩膜X轴);
                        WaitAxis(AXIS.扩膜Y轴);
                        WaitAxis(AXIS.扩膜U轴);
                    }
                    else
                    {
                        if (0 == ShowMessage("CCD1拍钢环引导超限，请检查后，确认‘重拍’还是‘换料’".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "重拍".txtTrans(TransType.Trans), "换料".txtTrans(TransType.Trans) }))
                        {
                            return "CCD1拍钢环";
                        }
                        else
                        {
                            return "扩膜平台移动到换钢环位";
                        }
                    }


                    return "顶针模组吸真空打开";

                case "顶针模组吸真空打开":
                    //DO.顶针吸真空.Set(true);
                    return "顶针模组运动到吸产品位";

                case "顶针模组运动到吸产品位":
                    AxisSingleTo(POINT.吸产品位, AXIS.顶针模组Z轴);
                    WaitAxis(AXIS.顶针模组Z轴);
                    DO.顶针吸真空.Set(false);
                    Delay(5000);

                    return "CCD2拍钢环内物料";


                case "CCD2拍钢环内物料":
                    Duration = 0;

                    this.camera拍钢环内产品相机.SendCmd(out str相机返回数据, (int)TcpCommu_CCD2.Command.CCD2_上料产品引导, true, AXIS.扩膜X轴.GetPos().ToString(),
                        AXIS.扩膜Y轴.GetPos().ToString(), AXIS.扩膜U轴.GetPos().ToString());
                    n拍照次数++;
                    //RingMonitor.TakeOut(n拍照次数);   // 记录当前取出料的环内料位号（蛇形顺序）
                    if (!IsNormalRun)
                    {
                        物料位置 = GetLocationByIndex(n拍照次数);
                        Logger.Process(this.Name, $"CCD2拍第{n拍照次数}片物料,此物料在：第{物料位置.Row}行，第{物料位置.Col}列");
                    }

                    return "判断拍钢环内物料结果";

                case "判断拍钢环内物料结果":
                    if (!camera拍钢环内产品相机.result && !IsDryRun)
                    {
                        CCD2NG++;
                        if (CCD2NG > 10)
                        {
                            CCD2NG = 0;
                            if (0 == ShowMessage("CCD2拍钢环内物料失败，请检查后，确认‘重拍’还是‘下一片’".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "重拍".txtTrans(TransType.Trans), "下一片".txtTrans(TransType.Trans) }))
                            {
                                弹窗时长 += Duration;
                                n拍照次数--;
                                return "CCD2拍钢环内物料";
                            }
                            else
                            {
                                return "检查钢环内物料是否全部做完";
                            }
                        }
                        return "检查钢环内物料是否全部做完";
                        //n拍照NG次数++;
                        //if (n拍照NG次数 >= 3)
                        //{
                        //    n拍照NG次数 = 0;
                        //    ShowMessage("连续三次拍物料失败，请检查", "提示");
                        //}
                        //return "扩膜平台移动到下一片物料拍照点";

                    }
                    this[NEST.穴位] = new ProductData();                  // 创建产品数据对象
                    this[NEST.穴位].Ringsite = RegInt.PickRingSite.Get();
					this[NEST.穴位].UpRingTime= RegString.UpRingTime.Get();

                    this[NEST.穴位].PianPos = n拍照次数;
                    this[NEST.穴位].BreakDownTime += 弹窗时长;
                    弹窗时长 = 0;
                    return "获取物料数据并矫正物料位置";

                case "获取物料数据并矫正物料位置":
                    if (IsDryRun)
                    {
                        d拍照基准点X = POINT.物料拍照位.GetPos(AXIS.扩膜X轴, this);
                        d拍照基准点Y = POINT.物料拍照位.GetPos(AXIS.扩膜Y轴, this);
                        d拍照基准点U = POINT.物料拍照位.GetPos(AXIS.扩膜U轴, this);
                    }
                    else
                    {
                        d拍照基准点X = this.camera拍钢环内产品相机.d上料引导位置X;
                        d拍照基准点Y = this.camera拍钢环内产品相机.d上料引导位置Y;
                        d拍照基准点U = this.camera拍钢环内产品相机.d上料引导位置U;
                        //if (b第一次拍物料)
                        //{
                        //    b第一次拍物料 = false;
                        //    d拍照基准点X = this.camera拍钢环相机.d上料引导1位置X;
                        //    d拍照基准点Y = this.camera拍钢环相机.d上料引导1位置Y;
                        //    d拍照基准点U = this.camera拍钢环相机.d上料引导位置U;
                        //}
                        if (d拍照基准点X < 130 && -19 < d拍照基准点X && d拍照基准点Y > 23 && d拍照基准点Y < 150)
                        {
                            AXIS.扩膜X轴.AbsMove(d拍照基准点X);
                            AXIS.扩膜Y轴.AbsMove(d拍照基准点Y);
                            AXIS.扩膜U轴.AbsMove(d拍照基准点U);
                            WaitAxis(AXIS.扩膜X轴);
                            WaitAxis(AXIS.扩膜Y轴);
                            WaitAxis(AXIS.扩膜U轴);
                        }
                        else
                        {
                            if (0 == ShowMessage("CCD2引导超限，请检查后，确认‘重拍’".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "重拍".txtTrans(TransType.Trans) }))
                            {
                                if (!(n拍照次数 == 0))
                                    n拍照次数--;
                                this[NEST.穴位].BreakDownTime += Duration;
                                return "CCD2拍钢环内物料";
                            }
                            else
                            {
                                AxisGoto(POINT.换钢环位, new AXIS[] { AXIS.顶针模组Z轴 });
                                WaitAxis(AXIS.顶针模组Z轴);
                                return "扩膜平台移动到换钢环位";
                            }
                        }

                    }
                    Delay(110);
                    DO.顶针吸真空.Set(true);
                    //DO.顶针破真空.Set(false);
                    //return "顶针模组吸真空打开";
                    return "扩膜站通知摆臂站取料";



                case "扩膜站通知摆臂站取料":
                    RegBit.扩膜通知摆臂取料.Set(true);
                    if (IsNormalRun)
                    {
                        UCCD2Site = d拍照基准点U;
                        var p = SnapPoints[n拍照次数 - 1];
                        double offsetX = d拍照基准点X - p.X;
                        double offsetY = d拍照基准点Y - p.Y;
                        if (n拍照次数 < n物料总数)
                        {
                            p = SnapPoints[n拍照次数];
                            Pnow = new PointD(p.X + offsetX, p.Y + offsetY);
                            走ccd2调整点 = true;
                        }
                    }



                    CCD2NG = 0;
                    return "等待摆臂站取料完成";

                case "等待摆臂站取料完成":
                    WaitRegBit(RegBit.摆臂通知扩膜取料完成, true);
                    this[NEST.穴位].DtStartTime = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss.fff"));
                    "摆臂站".GetStation<摆臂站>()[摆臂站.NEST.穴位] = this[NEST.穴位];
                    this[NEST.穴位] = null;
                    RegBit.摆臂通知扩膜取料完成.Set(false);
                    return "顶针缩回";

                case "顶针缩回":  //顶针上下到摆臂里了。
                    DO.顶针吸真空.Set(false);
                    DO.顶针破真空.Set(false);//摆臂站取料时会关闭吸真空，打开破真空       //---------------------------------------------------------------------------------------------
                    n摆臂取料++;
                    Logger.Process(this.Name, $"{Name},取料次数{n拍照次数}".txtTrans(TransType.Trans));
                    if (n摆臂取料 > Parameter.GetInt("TakeNums") - 1 && Parameter.GetInt("TakeNums") != 9999)
                    {
                        RegBit.扩膜允许清料.Set(true);
                        if (0 == ShowMessage($"取料到达{Parameter.GetInt("TakeNums")}设置次".txtTrans(TransType.Trans),
                            "提示".txtTrans(TransType.Trans), new string[] { "继续".txtTrans(TransType.Trans), "清料".txtTrans(TransType.Trans) }))
                        {
                            RegBit.扩膜允许清料.Set(false);
                            n摆臂取料 = 0;
                        }
                        else
                        {
                            RegBit.IR载具清料.Set(true);
                            n摆臂取料 = 0;
                            AxisGoto(POINT.换钢环位, new AXIS[] { AXIS.顶针模组Z轴 });
                            WaitAxis(AXIS.顶针模组Z轴);
                            return "扩膜平台移动到换钢环位";
                        }
                    }
                    return "检查钢环内物料是否全部做完";

                //case "顶针模组运动到待机位":
                //    AxisSingleTo(POINT.待机位, AXIS.顶针模组Z轴, SpeedPercent);
                //    return "检查钢环内物料是否全部做完";

                case "检查钢环内物料是否全部做完":
                    if (n拍照次数 >= n物料总数)
                    {
                        RegBit.扩膜允许清料.Set(true);
                        return "顶针模组运动到待机位";
                    }

                    return "扩膜平台移动到下一片物料拍照点";

                case "扩膜平台移动到下一片物料拍照点":
                    if (IsNormalRun)
                    {
                        if (!走ccd2调整点)
                        {
                            Pnow = SnapPoints[n拍照次数];
                        }
                        else
                        {
                            走ccd2调整点 = false;
                        }
                        if (Pnow.X < 130 && -19 < Pnow.X && Pnow.Y > 20 && Pnow.Y < 147)
                        {
                            AXIS.扩膜X轴.AbsMove(Pnow.X);
                            AXIS.扩膜Y轴.AbsMove(Pnow.Y);
                            AXIS.扩膜U轴.AbsMove(UCCD2Site);
                        }
                        else
                        {
                            if (0 == ShowMessage("CCD1拍钢环引导超限，请检查后，确认‘重拍’还是‘换料’".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "下一个".txtTrans(TransType.Trans) }))
                            {
                                n拍照次数++;
                                return "检查钢环内物料是否全部做完";
                            }
                            else
                            {
                                AxisGoto(POINT.换钢环位, new AXIS[] { AXIS.顶针模组Z轴 });
                                WaitAxis(AXIS.顶针模组Z轴);
                                return "扩膜平台移动到换钢环位";
                            }
                        }

                    }
                    else
                    {
                        AXIS.扩膜X轴.AbsMove(d拍照基准点X + (物料位置.Coord.X), AxisHandle.SpeedPercent);
                        AXIS.扩膜Y轴.AbsMove(d拍照基准点Y - (物料位置.Coord.Y), AxisHandle.SpeedPercent);
                        AXIS.扩膜U轴.AbsMove(d拍照基准点U, AxisHandle.SpeedPercent);
                    }

                    WaitAxis(AXIS.扩膜X轴);
                    WaitAxis(AXIS.扩膜Y轴);
                    WaitAxis(AXIS.扩膜U轴);
                    return "CCD2拍钢环内物料";

                case "顶针模组运动到待机位":
                    AxisSingleTo(POINT.待机位, AXIS.顶针模组Z轴);
                    WaitAxis(AXIS.顶针模组Z轴);
                    return "扩膜平台移动到换钢环位";

                case "扩膜平台移动到换钢环位":
                    AxisGoto(POINT.换钢环位, new AXIS[] { AXIS.顶针模组Z轴 });
                    WaitAxis(AXIS.顶针模组Z轴);
                    AxisGoto(POINT.换钢环位, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜U轴 });
                    WaitAxis(AXIS.扩膜X轴);
                    WaitAxis(AXIS.扩膜Y轴);
                    WaitAxis(AXIS.扩膜U轴);
                    //return "钢环固定气缸缩回";
                    return "扩膜站通知拉料站下钢环";

                //case "钢环固定气缸缩回":
                //    CylinderBack(Cylinder.钢环固定气缸1);
                //    CylinderBack(Cylinder.钢环固定气缸2);
                //    return "扩膜站通知拉料站下钢环";

                case "扩膜站通知拉料站下钢环":
                    RegBit.扩膜通知拉料站下钢环.Set(true);

                    return "生产流程结束";

                //case "等待拉料站下钢环完成":
                //    WaitRegBit(RegBit.拉料通知扩膜取料完成, true);
                //    RegBit.拉料通知扩膜取料完成.Set(false);
                //    return "生产流程结束";

                case "生产流程结束":
                    return "生产流程开始";
            }
            return null;
        }

        protected override string CalibRun(string strStep)
        {
            switch (strStep)
            {
                case "相机标定注册开始":
                    TcpCommu_CCD1.Command command = TcpCommu_CCD1.Command.CCD1_九点标定;
                    TcpCommu_CCD2.Command command2 = TcpCommu_CCD2.Command.CCD2_九点标定;
                    RegInt.相机功能选择.Set(
                    ShowMessage($"{Process.ToString()}开始，请在扩膜平台上放一片钢环，选择相机功能后，按‘启动’按钮".txtTrans(TransType.Trans),
                    "提示".txtTrans(), new string[] { "九点标定".txtTrans(TransType.Trans), "旋转标定".txtTrans(TransType.Trans), "注册".txtTrans(TransType.Trans) }));
                    WaitDI(DI.启动, true, -1);
                    RegBit.扩膜开始标定.Set(true);
                    return "扩膜Z轴移动到扩膜点";

                //case "钢环固定气缸伸出":
                //    Cylinder.钢环固定气缸1.Out();
                //    Cylinder.钢环固定气缸2.Out();
                //    WaitCylOut(Cylinder.钢环固定气缸1);
                //    WaitCylOut(Cylinder.钢环固定气缸2);
                //    return "扩膜Z轴移动到扩膜点";

                case "扩膜Z轴移动到扩膜点":
                    AxisSingleTo(POINT.扩膜位, AXIS.扩膜Z轴);
                    WaitAxis(AXIS.扩膜Z轴);
                    return "扩膜平台移动到拍照位";
                case "扩膜平台移动到拍照位":
                    n标定移动次数 = 1;
                    if (Process == AutoProcess.CCD1标定注册)
                    {
                        if (RegInt.相机功能选择.Get() == 2)
                            AxisGoto(POINT.钢环拍照位, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜U轴 });
                        else
                            AxisGoto(POINT.CCD1标定9点中心, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜U轴 });
                    }
                    else if (RegInt.相机功能选择.Get() == 2)
                    {
                        AxisGoto(POINT.CCD2标定基准点, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜U轴 });
                    }
                    else
                    {
                        AxisGoto(POINT.CCD2标定9点中心, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜U轴 });
                    }
                    WaitAxis(AXIS.扩膜X轴);
                    WaitAxis(AXIS.扩膜Y轴);
                    WaitAxis(AXIS.扩膜U轴);
                    d拍照基准点X = AXIS.扩膜X轴.GetPos();
                    d拍照基准点Y = AXIS.扩膜Y轴.GetPos();
                    d拍照基准点U = AXIS.扩膜U轴.GetPos();
                    switch (RegInt.相机功能选择.Get())
                    {
                        case 0:
                            return "移动到九点标定点";

                        case 1:
                            return "移动到旋转标定点";

                        case 2:
                            return "相机拍照并判断结果";
                        default:
                            break;
                    }
                    return "相机拍照并判断结果";

                case "相机拍照并判断结果":
                    bool b拍照结果 = true;
                    if (Process == AutoProcess.CCD1标定注册)
                    {
                        if (RegInt.相机功能选择.Get() == 0)
                        {
                            command = TcpCommu_CCD1.Command.CCD1_九点标定;
                        }
                        else if (RegInt.相机功能选择.Get() == 1)
                        {
                            command = TcpCommu_CCD1.Command.CCD1_旋转标定;
                        }
                        else
                        {
                            command = TcpCommu_CCD1.Command.CCD1_注册首个产品模板;
                        }
                        b拍照结果 = this.camera拍钢环相机.SendCmd(out str相机返回数据, (int)command, true, n标定移动次数 - 1,
                              AXIS.扩膜X轴.GetPos().ToString(), AXIS.扩膜Y轴.GetPos().ToString(), AXIS.扩膜U轴.GetPos().ToString());
                        //b拍照结果 = this.camera拍钢环相机.result;
                    }
                    else
                    {
                        if (RegInt.相机功能选择.Get() == 0)
                        {
                            command2 = TcpCommu_CCD2.Command.CCD2_九点标定;
                        }
                        else if (RegInt.相机功能选择.Get() == 1)
                        {
                            command2 = TcpCommu_CCD2.Command.CCD2_旋转标定;
                        }
                        else
                        {
                            RegBit.扩膜标定允许摆臂放料.Set(true);
                            command2 = TcpCommu_CCD2.Command.CCD2_注册取料模板;
                            WaitRegBit(RegBit.摆臂标定通知扩膜放料完成, true);
                        }
                        this.camera拍钢环内产品相机.SendCmd(out str相机返回数据, (int)command2, true, n标定移动次数 - 1,
                                AXIS.扩膜X轴.GetPos().ToString(), AXIS.扩膜Y轴.GetPos().ToString(), AXIS.扩膜U轴.GetPos().ToString());
                        b拍照结果 = this.camera拍钢环内产品相机.result;
                    }
                    if (!b拍照结果)
                    {
                        ShowMessage($"{Process.ToString()}失败，请检查处理后按‘确定’".txtTrans(TransType.Trans));
                        return "扩膜平台移动到换钢环位";
                    }

                    if (RegInt.相机功能选择.Get() == 0)
                    {
                        return "移动到九点标定点";
                    }
                    else if (RegInt.相机功能选择.Get() == 1)
                    {
                        return "移动到旋转标定点";
                    }
                    else
                    {
                        return "扩膜平台移动到换钢环位";
                    }
                case "扩膜平台移动到换钢环位":
                    AxisGoto(POINT.换钢环位, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜Z轴, AXIS.扩膜U轴 });
                    WaitAxis(AXIS.扩膜X轴);
                    WaitAxis(AXIS.扩膜Y轴);
                    WaitAxis(AXIS.扩膜Z轴);
                    WaitAxis(AXIS.扩膜U轴);
                    return "相机标定注册开始";


                case "移动到九点标定点":
                    double dX, dY;
                    d九点标定步长 = 50;

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
                    AXIS.扩膜X轴.AbsMove(dX, 30);
                    AXIS.扩膜Y轴.AbsMove(dY, 30);
                    WaitAxis(AXIS.扩膜X轴);
                    WaitAxis(AXIS.扩膜Y轴);
                    Delay(1000);
                    n标定移动次数++;
                    return "相机拍照并判断结果";

                case "移动到旋转标定点":
                    double dU;
                    d旋转标定角度 = 10;
                    switch (n标定移动次数)
                    {
                        case 1:
                            dU = d拍照基准点U + d旋转标定角度;
                            break;
                        case 2:
                            dU = d拍照基准点U + d旋转标定角度 * 2;
                            break;
                        case 3:
                            dU = d拍照基准点U;
                            break;
                        case 4:
                            dU = d拍照基准点U - d旋转标定角度;
                            break;
                        case 5:
                            dU = d拍照基准点U - d旋转标定角度 * 2;
                            break;
                        default:
                            return "旋转标定完成";
                    }
                    AXIS.扩膜U轴.AbsMove(dU, 30);
                    WaitAxis(AXIS.扩膜U轴);
                    Delay(1000);
                    n标定移动次数++;
                    return "相机拍照并判断结果";

                case "九点标定完成":
                case "旋转标定完成":
                    return "相机标定注册开始";
                default:
                    return null;
            }
        }

        protected override string GrrRun(string strStep)
        {
            switch (strStep)
            {
                case "相机动静态开始":
                    AxisGoto(POINT.换钢环位, new AXIS[] { AXIS.扩膜Z轴 });
                    WaitAxis(AXIS.扩膜Z轴);
                    ShowMessage($"{Process.ToString()}开始，请在扩膜平台上放一片钢环，按‘启动’按钮", "提示".txtTrans(TransType.Trans));
                    WaitDI(DI.启动, true, -1);
                    return "钢环固定气缸伸出";

                case "钢环固定气缸伸出":
                    Cylinder.钢环固定气缸1.Out();
                    Cylinder.钢环固定气缸2.Out();
                    WaitCylOut(Cylinder.钢环固定气缸1);
                    WaitCylOut(Cylinder.钢环固定气缸2);
                    return "扩膜Z轴移动到扩膜点";

                case "扩膜Z轴移动到扩膜点":
                    AxisSingleTo(POINT.扩膜位, AXIS.扩膜Z轴);
                    WaitAxis(AXIS.扩膜Z轴);

                    return "流程选择";

                case "流程选择":
                    if (0 == ShowMessage($"{Process.ToString()}开始，相机动静态选择，按‘启动’按钮".txtTrans(), "提示".txtTrans(), new string[] { "CCD1", "CCD2" }))
                    {
                        return "扩膜平台移动到钢环拍照位";
                    }

                    return "扩膜平台移动到钢环物料拍照位";

                case "扩膜平台移动到钢环拍照位":
                    AxisGoto(POINT.CCD1动静态位, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜U轴 });
                    WaitAxis(AXIS.扩膜X轴);
                    WaitAxis(AXIS.扩膜Y轴);
                    WaitAxis(AXIS.扩膜U轴);
                    return "CCD1流程选择";

                case "CCD1流程选择":
                    if (0 == ShowMessage($"{Process.ToString()}开始，相机动静态选择，按‘启动’按钮", "提示".txtTrans(), new string[] { "静态".txtTrans(TransType.Trans), "动态".txtTrans(TransType.Trans) }))
                    {
                        return "CCD1静态开始";
                    }

                    return "CCD1动态开始";

                case "CCD1静态开始":
                    for (int i = 0; i < 200; i++)
                    {
                        this.camera拍钢环相机.SendCmd(out str相机返回数据, (int)TcpCommu_CCD1.Command.CCD1_静态, false);
                    }
                    return "CCD1静态结束";

                case "CCD1静态结束":
                    if (0 == ShowMessage("CCD1静态结束,是否开始CCD1动态".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "是".txtTrans(TransType.Trans), "跳过".txtTrans(TransType.Trans) }))
                    {
                        return "CCD1动态开始";
                    }
                    else
                    {
                        return "CCD1动态结束";
                    }

                case "CCD1动态开始":
                    for (int i = 0; i < 200; i++)
                    {
                        AxisGoto(POINT.CCD1动静态位, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜U轴 });
                        WaitAxis(AXIS.扩膜X轴);
                        WaitAxis(AXIS.扩膜Y轴);
                        WaitAxis(AXIS.扩膜U轴);
                        this.camera拍钢环相机.SendCmd(out str相机返回数据, (int)TcpCommu_CCD1.Command.CCD1_动态, false);
                        AxisGoto(POINT.物料拍照位, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜U轴 });
                        WaitAxis(AXIS.扩膜X轴);
                        WaitAxis(AXIS.扩膜Y轴);
                        WaitAxis(AXIS.扩膜U轴);
                        Delay(100);
                    }
                    return "CCD1动态结束";

                case "CCD1动态结束":
                    return "流程选择";


                case "扩膜平台移动到钢环物料拍照位":
                    AxisGoto(POINT.CCD2动静态位, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜U轴 });
                    WaitAxis(AXIS.扩膜X轴);
                    WaitAxis(AXIS.扩膜Y轴);
                    WaitAxis(AXIS.扩膜U轴);
                    return "CCD2流程选择";

                case "CCD2流程选择":
                    if (0 == ShowMessage($"{Process.ToString()}开始，CCD2动静态选择，按‘启动’按钮".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "静态".txtTrans(TransType.Trans), "动态".txtTrans(TransType.Trans) }))
                    {
                        return "CCD2静态开始";
                    }

                    return "CCD2动态开始";

                case "CCD2静态开始":
                    for (int i = 0; i < 200; i++)
                    {
                        this.camera拍钢环内产品相机.SendCmd(out str相机返回数据, (int)TcpCommu_CCD2.Command.CCD2_静态, false);
                    }
                    return "CCD2静态结束";

                case "CCD2静态结束":
                    if (0 == ShowMessage("CCD1静态结束,是否开始CCD1动态".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "是".txtTrans(TransType.Trans), "跳过".txtTrans(TransType.Trans) }))
                    {
                        return "CCD2动态开始";
                    }
                    else
                    {
                        return "CCD2动态结束";
                    }

                case "CCD2动态开始":
                    for (int i = 0; i < 200; i++)
                    {
                        AxisGoto(POINT.CCD2动静态位, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜U轴 });
                        WaitAxis(AXIS.扩膜X轴);
                        WaitAxis(AXIS.扩膜Y轴);
                        WaitAxis(AXIS.扩膜U轴);
                        this.camera拍钢环内产品相机.SendCmd(out str相机返回数据, (int)TcpCommu_CCD2.Command.CCD2_动态, false);
                        AxisGoto(POINT.待机位, new AXIS[] { AXIS.扩膜X轴, AXIS.扩膜Y轴, AXIS.扩膜U轴 });
                        WaitAxis(AXIS.扩膜X轴);
                        WaitAxis(AXIS.扩膜Y轴);
                        WaitAxis(AXIS.扩膜U轴);
                        Delay(100);
                    }
                    return "CCD2动态结束";

                case "CCD2动态结束":
                    return null;

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

        /// <summary>
        /// 定义物理坐标结构体
        /// </summary>
        public struct Coord
        {
            public double X;
            public double Y;
        }

        /// <summary>
        /// 定义逻辑位置结构体 (行列信息)
        /// </summary>
        public struct Location
        {
            /// <summary>
            /// 行
            /// </summary>
            public int Row;

            /// <summary>
            /// 列
            /// </summary>
            public int Col;

            /// <summary>
            /// 对应的物理坐标
            /// </summary>
            public Coord Coord;
        }

        /// <summary>
        /// 根据行号、列号计算物理坐标 (X, Y)
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="col">列</param>
        /// <returns></returns>
        private Coord GetCoordinate(int row, int col)
        {

            if (row < 1 || row > n最大行数+1 || col < 1 || col > n最大列数)
                ShowMessage("行或列超出料盘范围".txtTrans(TransType.Trans));


            double x = (col - 1) * d列间距;
            double y = (row - 1) * d行间距;

            return new Coord { X = x, Y = y };
        }

        /// <summary>
        /// 根据物料序号(Index)获取位置信息 (行, 列, 坐标)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Location GetLocationByIndex(int index)
        {
            if (index < 0)
                ShowMessage("Index 不能为负数".txtTrans(TransType.Trans));

            // 计算行号
            int row = (index / n最大列数) + 1;

            // 计算在当前行的偏移量
            int offsetInRow = index % n最大列数;

            // 3. 计算列号 蛇形路径
            int col=0;
            if (row % 2 != 0)
            {
                // 奇数行 (1, 3, 5...): 从左向右 (1 -> 10)
                col = offsetInRow + 1;
            }
            else
            {
                // 偶数行 (2, 4, 6...): 从右向左 (10 -> 1)
                col = n最大列数 - offsetInRow;
            }

            // 4. 组合结果
            return new Location
            {
                Row = row,
                Col = col,
                Coord = GetCoordinate(row, col)
            };
        }

        /// <summary>
        /// 计算料盘间距
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public double[] 计算料盘行列间距()
        {

            //  计算列间距
            double totalWidth = POINT.料盘点2.GetPos(AXIS.扩膜X轴, this) - POINT.料盘点1.GetPos(AXIS.扩膜X轴, this);
            double cellWidth = totalWidth / n最大列数;

            // 计算行间距
            double totalHeight;
            totalHeight = POINT.料盘点1.GetPos(AXIS.扩膜Y轴, this) - POINT.料盘点3.GetPos(AXIS.扩膜X轴, this);

            double cellHeight = totalHeight / n最大行数;

            // 数值合法性校验（避免负数间距）
            if (cellWidth <= 0 || cellHeight <= 0)
            {
                throw new ArgumentException("坐标或行列数错误，计算出负间距，请检查坐标顺序");
            }

            return new double[] { cellHeight, cellWidth };
        }
        public struct PointD
        {
            public double X { get; set; }
            public double Y { get; set; }
            public PointD(double x, double y)
            {
                X = x;
                Y = y;
            }
            public override string ToString()
            {
                return $"X:{X:F3},Y:{Y:F3}";
            }
        }
        /// <summary>
        /// n行m列料盘 蛇形取料
        /// 行顺序：0→1→2→3…→n
        /// 偶数行(r%2==0)：列 0 → m 左→右
        /// 奇数行(r%2==1)：列 m → 0 右→左
        /// </summary>
        /// <param name="p00">第0行第0列</param>
        /// <param name="p0End">第0行第m列</param>
        /// <param name="pLastRow0">第n行第0列</param>
        /// <param name="Row">行</param>
        /// <param name="Col">列</param>
        /// <returns>按取料先后顺序的坐标列表，总数量 n*m</returns>
        public List<PointD> GenerateTraySnakePickList(PointD p00, PointD p0End, PointD pLastRow0, int Row, int Col)
        {
            List<PointD> pickList = new List<PointD>();
            double stepColX = 0;
            double stepColY = 0;
            double stepRowX = 0;
            double stepRowY = 0;
            // 列方向单步增量（同一行相邻两个孔位）
            if (Col == 1)
            {
                stepColX = p0End.X - p00.X;
                stepColY = p0End.Y - p00.Y;
            }
            else
            {
                stepColX = (p0End.X - p00.X) / (Col - 1);
                stepColY = (p0End.Y - p00.Y) / (Col - 1);
            }


            // 行方向单步增量（同一列相邻两行）
            if (Row == 1)
            {
                stepRowX = pLastRow0.X - p00.X;
                stepRowY = pLastRow0.Y - p00.Y;
            }
            else
            {
                stepRowX = (pLastRow0.X - p00.X) / (Row - 1);
                stepRowY = (pLastRow0.Y - p00.Y) / (Row - 1);
            }


            // 逐行向下遍历
            for (int r = 0; r < Row; r++)
            {
                if (r % 2 == 0)
                {
                    // 偶数行：左 → 右
                    for (int c = 0; c < Col; c++)
                    {
                        double x = p00.X + r * stepRowX + c * stepColX;
                        double y = p00.Y + r * stepRowY + c * stepColY;
                        pickList.Add(new PointD(x, y));
                    }
                }
                else
                {
                    // 奇数行：右 → 左
                    for (int c = Col - 1; c >= 0; c--)
                    {
                        double x = p00.X + r * stepRowX + c * stepColX;
                        double y = p00.Y + r * stepRowY + c * stepColY;
                        pickList.Add(new PointD(x, y));
                    }
                }
            }
            return pickList;
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
        public static double GetPos(this 扩膜站.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this 扩膜站.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}


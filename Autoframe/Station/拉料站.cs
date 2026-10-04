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

namespace Autoframe
{
    public class 拉料站 : StationEx
    {


        private bool b拉料模组是否在取料或者拍照位 = false;

        private bool b拉料模组夹爪是否夹紧 = false;

        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public 拉料站(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<NEST, ProductData>();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { DI.拉料升降伸出位, DI.拉料升降缩回位, DI.拉料夹爪夹紧位, DI.拉料夹爪松开位, DI.钢环有料感应 };
            //配置站位界面显示输出
            arrayDO = new object[] { DO.拉料升降伸出, DO.拉料升降缩回, DO.拉料夹爪夹紧, DO.拉料夹爪松开 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { Cylinder.拉料升降气缸, Cylinder.拉料夹爪气缸 , Cylinder.钢环固定气缸1, Cylinder.钢环固定气缸2};
            //配置站位轴
            arrayAxis = new object[] { AXIS.拉料Y轴 };
        }

        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            /// <summary>
            /// 等待取钢环的位置
            /// </summary>
            待机位,
            取料位,
            过渡位,
            换料位,
            /// <summary>
            /// 等待从扩膜取钢环的位置
            /// </summary>
            等待位,

        }

        /// <summary>
        /// 产品穴位，与产品是一对一
        /// </summary>
        public enum NEST
        {

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
            RegBit.拉料通知上料换料.Set(false);
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
                    this.AutoEntry = "空步";
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



        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case "初始化流程开始":
                    AXIS.拉料Y轴.ServoOn(true);
                    Delay(500);
                    return "拉料模组运动到待机位";

                case "拉料模组运动到待机位":
                    AxisSingleTo(POINT.待机位, AXIS.拉料Y轴);
                    WaitAxis(AXIS.拉料Y轴);
                    return "待机位检查拉料模组夹爪检测光电是否有料";
                case "待机位检查拉料模组夹爪检测光电是否有料":
                    Cylinder.拉料夹爪气缸.Back();
                    if (DI.钢环有料感应.Get())
                    {
                        ShowMessage("拉料模组上有夹爪，请拿走");
                        return "待机位检查拉料模组夹爪检测光电是否有料";
                    }
                    else
                    {
                        return "拉料模组升降缩回";
                    }
                case "拉料模组升降缩回":
                    Cylinder.拉料升降气缸.Back();
                    WaitCylBack(Cylinder.拉料升降气缸);
                    return "检查拉料模组夹爪检测光电是否有料";

                case "检查拉料模组夹爪检测光电是否有料":
                    Cylinder.拉料夹爪气缸.Out();
                    WaitCylOut(Cylinder.拉料夹爪气缸);
                    if (DI.钢环有料感应.Get())
                    {
                        return "提示人工拿走钢环";
                    }
                    else
                    {
                        return "拉料模组夹爪张开";
                    }

                case "拉料模组夹爪张开":
                    Cylinder.拉料夹爪气缸.Back();
                    WaitCylBack(Cylinder.拉料夹爪气缸);
                    //return "拉料站允许上料站初始化";
                    return "初始化完成";

                case "提示人工拿走钢环":
                    ShowMessage("拉料模组上有夹爪，请拿走");
                    return "检查拉料模组夹爪检测光电是否有料";

                //case "拉料站允许上料站初始化":
                //    RegBit.拉料站初始化完成.Set(true);
                //    return "等待上料站初始化完成";

                //case "等待上料站初始化完成":
                //    WaitRegBit(RegBit.上料站初始化完成, true);
                //    return "初始化完成";

                case "初始化完成":
                    Cylinder.钢环固定气缸1.Out();
                    Cylinder.钢环固定气缸2.Out();
                    RegBit.拉料通知上料换料.Set(true);
                    RegBit.拉料站初始化完成.Set(true);
                    //结束初始化
                    return StepExit;
                default:
                    return null;
            }
        }


        //protected override string InitRun(string strStep)
        //{
        //    switch (strStep)
        //    {
        //        case "初始化流程开始":
        //            AXIS.拉料Y轴.ServoOn(true);
        //            Delay(500);
        //            b拉料模组是否在取料或者拍照位 = false;
        //            b拉料模组夹爪是否夹紧 = false;
        //            return "检查拉料模组是否在取料位或者拍照位";

        //        case "检查拉料模组是否在取料位或者拍照位":
        //            double d取料位 = POINT.取料位.GetPos(AXIS.拉料Y轴, this);
        //            double d上料位 = POINT.拍照位.GetPos(AXIS.拉料Y轴, this);
        //            if (Math.Abs(AXIS.拉料Y轴.GetPos() - d取料位) >= 0.2 || Math.Abs(AXIS.拉料Y轴.GetPos() - d上料位) >= 0.2)
        //            {
        //                b拉料模组是否在取料或者拍照位 = true;
        //                return "拉料模组夹爪张开";
        //            }
        //            b拉料模组是否在取料或者拍照位 = false;
        //            return "拉料模组运动到待机位";

        //        case "拉料模组夹爪张开":
        //            Cylinder.拉料夹爪气缸.Back();
        //            WaitCylBack(Cylinder.拉料夹爪气缸);
        //            if (b拉料模组夹爪是否夹紧)
        //            {
        //                return "记录拉料模组初始化无料";
        //            }
        //            return "拉料模组运动到待机位";

        //        case "拉料模组运动到待机位":
        //            AxisSingleTo(POINT.待机位, AXIS.拉料Y轴);
        //            WaitAxis(AXIS.拉料Y轴);
        //            return "拉料模组升降缩回";

        //        case "拉料模组升降缩回":
        //            Cylinder.拉料升降气缸.Back();
        //            WaitCylBack(Cylinder.拉料升降气缸);
        //            if (!b拉料模组是否在取料或者拍照位)
        //            {
        //                return "检查拉料模组夹爪是否夹紧";
        //            }
        //            return "记录拉料模组初始化无料";

        //        case "检查拉料模组夹爪是否夹紧":
        //            if (DI.拉料夹爪夹紧位.Get())
        //            {
        //                b拉料模组夹爪是否夹紧 = true;
        //            }
        //            b拉料模组夹爪是否夹紧 = false;
        //            return "检查拉料模组夹爪检测光电是否有料";

        //        case "检查拉料模组夹爪检测光电是否有料":
        //            if (b拉料模组夹爪是否夹紧)
        //            {
        //                if (DI.钢环有料感应.Get())
        //                {
        //                    return "记录拉料模组初始化有料";
        //                }
        //                else
        //                {
        //                    return "拉料模组夹爪张开";
        //                }
        //            }
        //            else
        //            {
        //                if (DI.钢环有料感应.Get())
        //                {
        //                    return "报警提示拉料模组检测光电异常";
        //                }
        //                else
        //                {
        //                    return "记录拉料模组初始化无料";
        //                }
        //            }

        //        case "报警提示拉料模组检测光电异常":
        //            ShowMessage("拉料模组夹爪检测光电异常，请检查");
        //            return "检查拉料模组夹爪检测光电是否有料";

        //        case "记录拉料模组初始化有料":
        //            RegBit.拉料站初始化有料.Set(true);
        //            return "拉料站允许上料站初始化";


        //        case "记录拉料模组初始化无料":
        //            RegBit.拉料站初始化有料.Set(false);
        //            return "拉料站允许上料站初始化";

        //        case "拉料站允许上料站初始化":
        //            RegBit.拉料站初始化完成.Set(true);
        //            return "等待上料站初始化完成";

        //        case "等待上料站初始化完成":
        //            WaitRegBit(RegBit.上料站初始化完成, true);
        //            return "初始化完成";

        //        case "初始化完成":
        //            //结束初始化
        //            return StepExit;
        //        default:
        //            return null;
        //    }
        //}

        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case "生产流程开始":
                    return "等待扩膜站通知拉料站上钢环";

                case "等待扩膜站通知拉料站上钢环":
                    WaitRegBit(RegBit.扩膜通知拉料站上钢环, true);
                    //RegBit.扩膜通知拉料站上钢环.Set(false);
                    return "拉料模组运动到取料位";

                case "拉料模组运动到取料位":
                    Cylinder.拉料升降气缸.Out();
                    Delay(200);
                    AxisSingleTo(POINT.取料位, AXIS.拉料Y轴);
                    WaitAxis(AXIS.拉料Y轴);
                    RegBit.拉料通知上料下降3.Set(true);
                    return "等待上料下降3毫米到位";
                case "等待上料下降3毫米到位":
                    WaitRegBit(RegBit.上料站通知拉料下降3毫米到位, true);
                    RegBit.上料站通知拉料下降3毫米到位.Set(false);
                    return "拉料模组夹爪夹紧";
                case "拉料模组夹爪夹紧":
                    Cylinder.拉料夹爪气缸.Out();
                    WaitCylOut(Cylinder.拉料夹爪气缸);
                    if ((Parameter.GetBit("DryRunWithMgz") && IsDryRun) || IsNormalRun)
                    {
                        return "检查是否夹到钢环";
                    }
                    return "拉料模组运动到换料位";

                case "检查是否夹到钢环":
                    if (!DI.钢环有料感应.Get())
                    {
                        if (1 == ShowMessage("夹爪没有检测到钢环，请检查后确认‘继续’ 还是‘取下一片’，", "提示", new string[] { "继续", "取下一片" }))
                        {
                            //return "拉料模组通知上料站换料";
                            return "拉料模组去上料站放空钢环";
                        }
                    }
                    return "拉料模组运动到换料位";

                case "拉料模组运动到换料位":
                    AxisSingleTo(POINT.换料位, AXIS.拉料Y轴);
                    //if ((Parameter.GetBit("DryRunWithMgz") && IsDryRun) || IsNormalRun)
                    //{
                    //    //while (!StationMgr.Stop())
                    //    //    if (DI.料盘对齐检测.Get())
                    //    //    {
                    //    //        break;
                    //    //    }
                    //    if (IsInPos(AXIS.拉料Y轴))
                    //    {
                    //        if (0 == ShowMessage("拉料过程中没检测到从料仓中拉出钢环，请检查", "提示", new string[] { "继续", "下一片" }))
                    //        {
                    //            break;
                    //        }
                    //        return "拉料模组上料";
                    //    }
                    //}

                    WaitAxis(AXIS.拉料Y轴);
                    return "拉料模组上料";

                case "拉料模组上料":
                    //Cylinder.拉料升降气缸.Out();
                    //WaitCylOut(Cylinder.拉料升降气缸);
                    Cylinder.拉料夹爪气缸.Back();
                    WaitCylBack(Cylinder.拉料夹爪气缸);
                    return "拉料模组到等待位";

                case "拉料模组到等待位":
                    RegBit.扩膜通知拉料站上钢环.Set(false);
                    AxisSingleTo(POINT.等待位, AXIS.拉料Y轴);
                    WaitAxis(AXIS.拉料Y轴);
                    Cylinder.拉料升降气缸.Back();
                    WaitCylBack(Cylinder.拉料升降气缸);
                    return "拉料模组到待机位上料完成";

                case "拉料模组到待机位上料完成":
                    AxisSingleTo(POINT.待机位, AXIS.拉料Y轴);
                    WaitAxis(AXIS.拉料Y轴);
                    RegBit.拉料通知扩膜上料完成.Set(true);
                    return "等待扩膜站通知拉料站下料";

                case "等待扩膜站通知拉料站下料":
                    if (RegBit.扩膜通知拉料站下钢环.Get())
                    {
                        RegBit.扩膜通知拉料站下钢环.Set(false);
                        return "下料到等到位";
                    }
                    return "等待扩膜站通知拉料站下料";

                case "下料到等到位":
                    AxisSingleTo(POINT.等待位, AXIS.拉料Y轴);
                    WaitAxis(AXIS.拉料Y轴);
                    return "拉料模组下料";

                case "拉料模组下料":
                    Cylinder.拉料升降气缸.Out();
                    WaitCylOut(Cylinder.拉料升降气缸);
                    AxisSingleTo(POINT.换料位, AXIS.拉料Y轴);
                    WaitAxis(AXIS.拉料Y轴);
                    Cylinder.拉料夹爪气缸.Out();
                    Delay(300);
                    return "拉料模组去上料站放空钢环";

                case "拉料模组去上料站放空钢环":
                    //AxisSingleTo(POINT.待机位, AXIS.拉料Y轴);
                    //WaitAxis(AXIS.拉料Y轴);
                    //Cylinder.拉料升降气缸.Back();
                    //WaitCylBack(Cylinder.拉料升降气缸);
                    AxisSingleTo(POINT.取料位, AXIS.拉料Y轴);
                    WaitAxis(AXIS.拉料Y轴);
                    Cylinder.拉料夹爪气缸.Back();
                    WaitCylBack(Cylinder.拉料夹爪气缸);
                    RegBit.拉料通知上料上升3.Set(true);
                    return "等待上料上升3毫米到位";

                case "等待上料上升3毫米到位":
                    WaitRegBit(RegBit.上料站通知拉料上升3毫米到位, true);
                    RegBit.上料站通知拉料上升3毫米到位.Set(false);
                    return "拉料模组运动到待机位";

                case "拉料模组运动到待机位":
                    AxisSingleTo(POINT.待机位, AXIS.拉料Y轴);
                    WaitAxis(AXIS.拉料Y轴);
                    return "拉料模组通知上料站换料";

                case "拉料模组通知上料站换料":
                    RegBit.拉料通知上料换料.Set(true);
                    return "等待上料站换料完成";

                case "等待上料站换料完成":
                    WaitRegBit(RegBit.上料站通知拉料换料完成, true);
                    RegBit.上料站通知拉料换料完成.Set(false);
                    return "生产流程结束";

                case "生产流程结束":
                    return "生产流程开始";
            }
            return null;
        }

        protected override string CalibRun(string strStep)
        {
            switch (strStep)
            {
                case "空步":
                    Delay(10);
                    return "空步";
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
    }
    public static partial class HandleEx
    {
        public static double GetPos(this 拉料站.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this 拉料站.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}

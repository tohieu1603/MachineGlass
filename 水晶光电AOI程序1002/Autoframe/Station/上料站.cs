using AF.Card;
using AF.Common;
using AF.Data;
using AF.File;
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
    public class 上料站 : StationEx
    {

        ProductData productData = new ProductData();

        private int n取到第几层钢环 = 0;

        private double dZ轴位置 = 0;

        private double d钢环间距 = 12.26;//实际值12.26

        private string IniFile = AppDomain.CurrentDomain.BaseDirectory + "Config" + "\\" + "StationParam.ini";
        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public 上料站(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<NEST, ProductData>();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { DI.料盘上料按钮, DI.料盘超限检测, DI.料盘有无检测 };
            //配置站位界面显示输出
            arrayDO = new object[] { DO.料盘上料按钮灯 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { };
            //配置站位轴
            arrayAxis = new object[] { AXIS.料仓升降Z轴 };
        }




        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            待机位,
            上料位,
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
            switch (Process)
            {
                case AutoProcess.生产:
                case AutoProcess.空跑:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "生产流程开始";
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
                    DO.料盘上料按钮灯.Set(false);
                    d钢环间距 = 12.26;
                    dZ轴位置 = 0;
                    IniFile = AppDomain.CurrentDomain.BaseDirectory + "Config" + "\\" + "StationParam.ini";
                    return "轴上使能";
                case "轴上使能":
                    AXIS.料仓升降Z轴.ServoOn(true);
                    Delay(1000);
                    return "等待拉料站初始化完成";

                case "等待拉料站初始化完成":
                    WaitRegBit(RegBit.拉料站初始化完成, true);
                    return "检查料仓有无弹夹";

                case "检查料仓有无弹夹":

                    if (DI.料盘有无检测.Get() && (Parameter.GetBit("DryRunWithMgz") || IsDryRun))
                    {
                        return "读取弹夹刚环数量";
                    }
                    return "料仓Z轴运动到待机位";

                case "读取弹夹刚环数量":
                    int.TryParse(Ini.GetStringValue(IniFile, "CountParam", "UpSiloCount", ""), out n取到第几层钢环);
                    return "检查钢环是否正确放在弹夹中";

                case "料仓Z轴运动到待机位":
                    AxisSingleTo(POINT.待机位, AXIS.料仓升降Z轴);
                    WaitAxis(AXIS.料仓升降Z轴);
                    return "提示人工上弹夹";

                case "提示人工上弹夹":
                    DO.料盘上料按钮灯.Set(true);
                    if (Parameter.GetBit("DryRunWithMgz") || (IsNormalRun && !DI.料盘有无检测.Get()))
                    {
                        ShowMessage("料仓没有弹夹，请放置弹夹");
                    }
                    return "检查弹夹是否放好";

                case "检查弹夹是否放好":
                    n取到第几层钢环 = 0;
                    if (Parameter.GetBit("DryRunWithMgz") || IsNormalRun)
                    {
                        WaitDI(DI.料盘上料按钮, true, -1);
                        //DO.料盘上料按钮灯.Set(false);
                        if (!DI.料盘有无检测.Get())
                        {
                            ShowMessage("弹夹没放好，请检查！！");
                            return "检查弹夹是否放好";
                        }
                        DO.料盘上料按钮灯.Set(false);
                        return "检查钢环是否正确放在弹夹中";
                    }
                    DO.料盘上料按钮灯.Set(false);
                    //n取到第几层钢环 = 0;
                    return "检查钢环是否正确放在弹夹中";

                case "检查钢环是否正确放在弹夹中":
                    if (DI.料盘超限检测.Get() && (Parameter.GetBit("DryRunWithMgz") || IsNormalRun))
                    {
                        ShowMessage("钢环没有放好，请检查！！");
                        return "检查钢环是否正确放在弹夹中";
                    }
                    return "料仓Z轴移动到上料位";

                case "料仓Z轴移动到上料位":
                    dZ轴位置 = POINT.上料位.GetPos(AXIS.料仓升降Z轴, this) - (d钢环间距 * n取到第几层钢环);
                    AXIS.料仓升降Z轴.AbsMove(dZ轴位置, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.料仓升降Z轴);
                    return "初始化完成";

                case "初始化完成":
                    RegBit.上料站初始化完成.Set(true);
                    //结束初始化
                    return StepExit;
                default:
                    return null;
            }
        }

        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case "生产流程开始":
                    return "等待拉料站通知换料";
                
                case "等待拉料站通知换料":
                    WaitRegBit(RegBit.拉料通知上料换料, true);
                    RegBit.拉料通知上料换料.Set(false);
                    //if (IsDryRun)
                    //{
                    //    return "料仓Z轴运动到上料位";
                    //}
                    return "检查钢环是否全部做完";

                case "检查钢环是否全部做完":
                    if (n取到第几层钢环 >= 13)
                    {
                        return "料仓Z轴运动到待机位";
                    }
                    RegInt.PickRingSite.Set(n取到第几层钢环);
                    RegString.UpRingTime.Set(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return "料仓Z轴运动到上料位";

                case "料仓Z轴运动到待机位":
                    AxisSingleTo(POINT.待机位, AXIS.料仓升降Z轴);
                    WaitAxis(AXIS.料仓升降Z轴);
                    return "提示人工换弹夹";

                case "提示人工换弹夹":
                    DO.料盘上料按钮灯.Set(true);
                    if (Parameter.GetBit("DryRunWithMgz") || IsNormalRun)
                    {
                        ShowMessage("弹夹已做完，请更换弹夹");
                    }
                    return "检查弹夹是否放好";

                case "检查弹夹是否放好":
                    if (Parameter.GetBit("DryRunWithMgz") || IsNormalRun)
                    {
                        WaitDI(DI.料盘上料按钮, true, -1);
                        if (!DI.料盘有无检测.Get())
                        {
                            ShowMessage("弹夹没放好，请检查！！");
                            return "检查弹夹是否放好";
                        }
                    }
                    DO.料盘上料按钮灯.Set(false);
                    n取到第几层钢环 = 0;
                    return "检查钢环是否正确放在弹夹中";

                case "检查钢环是否正确放在弹夹中":
                    if (DI.料盘超限检测.Get() && (Parameter.GetBit("DryRunWithMgz") || IsNormalRun))
                    {
                        ShowMessage("钢环没有放好，请检查！！");
                        return "检查钢环是否正确放在弹夹中";
                    }
                    return "料仓Z轴运动到上料位";

                case "料仓Z轴运动到上料位":
                    dZ轴位置 = POINT.上料位.GetPos(AXIS.料仓升降Z轴, this) - (d钢环间距 * n取到第几层钢环);
                    AXIS.料仓升降Z轴.AbsMove(dZ轴位置, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.料仓升降Z轴);
                    RegBit.上料站通知拉料下降3毫米到位.Set(false);
                    RegBit.上料站通知拉料换料完成.Set(true);
                    return "检查钢环在弹夹中是否超限";
                case "检查钢环在弹夹中是否超限":
                    if (DI.料盘超限检测.Get() && (Parameter.GetBit("DryRunWithMgz") || IsNormalRun))
                    {
                        ShowMessage("钢环没有放好，请检查！！");
                        return "检查钢环在弹夹中是否超限";
                    }
                    return "等待下降3毫米";
                case "等待下降3毫米":
                    WaitRegBit(RegBit.拉料通知上料下降3, true);
                    RegBit.拉料通知上料下降3.Set(false);
                    dZ轴位置 = POINT.上料位.GetPos(AXIS.料仓升降Z轴, this) - (d钢环间距 * n取到第几层钢环)+3;
                    AXIS.料仓升降Z轴.AbsMove(dZ轴位置, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.料仓升降Z轴);
                    RegBit.上料站通知拉料下降3毫米到位.Set(true);
                    return "等待上升3毫米";
                case "等待上升3毫米":
                    if(RegBit.拉料通知上料上升3.Get())
                        return "上升3毫米";
                    return "等待上升3毫米";
                case "上升3毫米":
                    RegBit.拉料通知上料上升3.Set(false);
                    dZ轴位置 = POINT.上料位.GetPos(AXIS.料仓升降Z轴, this) - (d钢环间距 * n取到第几层钢环) - 3;
                    AXIS.料仓升降Z轴.AbsMove(dZ轴位置, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.料仓升降Z轴);
                    RegBit.上料站通知拉料上升3毫米到位.Set(true);
                    return "通知拉料站换料完成";
                case "通知拉料站换料完成":

                    n取到第几层钢环++;
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
    }
    public static partial class HandleEx
    {
        public static double GetPos(this 上料站.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this 上料站.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}

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
    public class OK下料站 : StationEx
    {
        DI 下料吸真空到位;
        DO 下料吸真空; DO 下料破真空;
        AXIS 下料Y轴; AXIS 下料Z轴;
        POINT 待机位; POINT 取料等待位; POINT 放料位; POINT 取料位;
        RegBit 下料初始化完成; RegBit 下料通知覆膜取料完成; RegBit 料仓通知下料可放料; RegBit 下料通知料仓放料完成; RegBit 覆膜通知下料有料;

        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public OK下料站(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<NEST, ProductData>();
        }

        void HardwareConfig()
        {
            if (this.Name == "OK下料")
            {
                下料吸真空到位 = DI.OK下料吸真空到位;

                下料吸真空 = DO.OK下料吸真空;
                下料破真空 = DO.OK下料破真空;

                待机位 = POINT.待机位;
                取料等待位 = POINT.取料等待位;
                放料位 = POINT.放料位;
                取料位 = POINT.取料位;

                下料Y轴 = AXIS.OK下料Y轴;
                下料Z轴 = AXIS.OK下料Z轴;

                下料初始化完成 = RegBit.OK下料初始化完成;
                下料通知覆膜取料完成 = RegBit.OK下料通知OK覆膜取料完成;
                料仓通知下料可放料 = RegBit.OK料仓通知OK下料可放料;
                下料通知料仓放料完成 = RegBit.OK下料通知OK料仓放料完成;
                覆膜通知下料有料 = RegBit.OK覆膜通知OK下料有料;
            }
            else 
            {
                下料吸真空到位 = DI.NG下料吸真空到位;

                下料吸真空 = DO.NG下料吸真空;
                下料破真空 = DO.NG下料破真空;

                待机位 = POINT.待机位;
                取料等待位 = POINT.取料等待位;
                放料位 = POINT.放料位;
                取料位 = POINT.取料位;

                下料Y轴 = AXIS.NG下料Y轴;
                下料Z轴 = AXIS.NG下料Z轴;

                下料初始化完成 = RegBit.NG下料初始化完成;
                下料通知覆膜取料完成 = RegBit.NG下料通知OK覆膜取料完成;
                料仓通知下料可放料 = RegBit.NG料仓通知NG下料可放料;
                下料通知料仓放料完成 = RegBit.NG下料通知NG料仓放料完成;
                覆膜通知下料有料 = RegBit.NG覆膜通知NG下料有料;
            }
            
            //配置站位界面显示输入
            arrayDI = new object[] { 下料吸真空到位 };
            //配置站位界面显示输出
            arrayDO = new object[] { 下料吸真空, 下料破真空 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] {  };
            //配置站位轴
            arrayAxis = new object[] {下料Y轴,下料Z轴};
        }

        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            待机位,
            取料等待位,
            取料位,
            放料位,
        }

        /// <summary>
        /// 产品穴位，与产品是一对一
        /// </summary>
        public enum NEST
        {
            
        }

        public override void OnStop()
        {
            //base.OnStop() 不能删，且需要放在第一行。
            base.OnStop();
            下料Y轴.ServoOn(false);
            下料Z轴.ServoOn(false);
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
            下料初始化完成.Set(false);
            下料通知覆膜取料完成.Set(false);
            下料通知料仓放料完成.Set(false);
            覆膜通知下料有料.Set(false) ;
            switch (Process)
            {
                case AutoProcess.生产:
                case AutoProcess.空跑:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "生产流程开始";
                    break;
                case AutoProcess.OK覆膜一键出料:
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
                    Delay(3000);

                    return "轴上使能";

                case "轴上使能":
                    下料Y轴.ServoOn(true);
                    下料Z轴.ServoOn(true);
                    Delay(2000);
                    if (!IsDryRun)
                    {
                        下料Z轴.Home();
                        WaitAxis(下料Z轴);
                    }
                    
                    return "检测吸嘴是否有料";

                case "检测吸嘴是否有料":
                    if (!下料吸真空到位.Get()||IsDryRun)
                    {
                        return "移动到取料等待位";
                    }
                    else
                    {
                        return "等待料仓可放料";
                    }
                        
                //-----------------无料----------------------------
                case "移动到取料等待位":
                    if (!IsDryRun)
                    {
                        AxisSingleTo(取料等待位, 下料Z轴, 30);
                        WaitAxis(下料Z轴);
                    }
                    
                    AxisSingleTo(取料等待位, 下料Y轴, 30);
                    WaitAxis(下料Y轴);
                    return "移动到待机位";
                //-----------------有料----------------------------
                case "等待料仓可放料":
                    if (料仓通知下料可放料.Get())
                    {
                        料仓通知下料可放料.Set(false);
                        return "移动到放料位";
                    }
                    else
                    {
                        return "等待料仓可放料";
                    }

                case "移动到放料位":
                    AxisSingleTo(放料位, 下料Y轴, 30);
                    WaitAxis(下料Y轴);
                    if (!IsDryRun)
                    {
                        AxisSingleTo(放料位, 下料Z轴, 30);
                        WaitAxis(下料Z轴);
                    }
                    
                    return "关闭吸真空放料";

                case "关闭吸真空放料":
                    下料吸真空.Set(false);
                    下料破真空.Set(true);
                    Thread.Sleep(50);
                    下料破真空.Set(false);
                    return "移动到待机位";

                case "移动到待机位":
                    if (!IsDryRun)
                    {
                        AxisSingleTo(待机位, 下料Z轴, 30);
                        WaitAxis(下料Z轴);
                    }
                    AxisSingleTo(待机位, 下料Y轴, 30);
                    WaitAxis(下料Y轴);
                    return "初始化完成";

                case "初始化完成":
                    下料通知覆膜取料完成.Set(true);
                    下料初始化完成.Set(true);

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
                    return "检测流线有料";

                case "检测流线有料":
                    if (覆膜通知下料有料.Get())
                    {
                        覆膜通知下料有料.Set(false);
                        return "移动到取料位";
                    }
                    else
                    {
                        return "检测流线有料";
                    }

                case "移动到取料位":
                    AxisSingleTo(取料位, 下料Y轴, 100);
                    WaitAxis(下料Y轴);
                    if (!IsDryRun)
                    {
                        AxisSingleTo(取料位, 下料Z轴, 100);
                        WaitAxis(下料Z轴);
                    }
                    return "打开吸真空";

                case "打开吸真空":
                    下料吸真空.Set(true);
                    下料破真空.Set(false);
                    if (!IsDryRun)
                    {
                        WaitDI(下料吸真空到位, true, 2);
                        //if (下料吸真空到位.Get())
                        //{
                        //    覆膜通知下料有料.Set(false);
                        //    return "Z轴运动到待机位";
                        //}
                        //else
                        //{
                        //    return "打开吸真空";
                        //}
                    }

                    return "Z轴运动到待机位";

                case "Z轴运动到待机位":
                    if (!IsDryRun)
                    {
                        AxisSingleTo(待机位, 下料Z轴, 100);
                        WaitAxis(下料Z轴);
                    }
                    下料通知覆膜取料完成.Set(true);
                    return "运动到放料位";

                case "运动到放料位":
                    AxisSingleTo(放料位, 下料Y轴, 100);
                    WaitAxis(下料Y轴);

                    return "等待料仓可放料";

                case "等待料仓可放料":
                    if (料仓通知下料可放料.Get())
                    {
                        料仓通知下料可放料.Set(false);
                        return "Z轴运动到放料位";
                    }
                    else
                    {
                        return "等待料仓可放料";
                    }

                case "Z轴运动到放料位":
                    if (!IsDryRun)
                    {
                        if (!下料吸真空到位.Get())
                        {
                            int ret = ShowMessage($"{this.Name},吸真空异常".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "忽略".txtTrans(TransType.Trans), "重检".txtTrans(TransType.Trans) });
                            if (ret == 1)
                            {
                                return "Z轴运动到放料位";
                            }
                        }
                        AxisSingleTo(放料位, 下料Z轴, 100);
                        WaitAxis(下料Z轴);
                    }

                    return "关闭吸真空";

                case "关闭吸真空":
                    下料吸真空.Set(false);
                    下料破真空.Set(true);
                    Delay(1000);
                    下料破真空.Set(false);
                    return "向料仓发送放料完成";

                case "向料仓发送放料完成":

                    return "运动到待机位";

                case "运动到待机位":
                    if (!IsDryRun)
                    {
                        AxisSingleTo(待机位, 下料Z轴, 100);
                        WaitAxis(下料Z轴);
                    }

                    AxisSingleTo(待机位, 下料Y轴, 100);
                    WaitAxis(下料Y轴);
                    下料通知料仓放料完成.Set(true);
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
                //case "相机标定开始":
                //    Delay(10);
                //    return "相机标定开始";
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
                case "生产流程开始":
                    return "检测流线有料";

                case "检测流线有料":
                    if (覆膜通知下料有料.Get())
                    {
                        覆膜通知下料有料.Set(false);
                        return "移动到取料位";
                    }
                    else
                    {
                        return "检测流线有料";
                    }

                case "移动到取料位":
                    AxisSingleTo(取料位, 下料Y轴, 100);
                    WaitAxis(下料Y轴);
                    if (!IsDryRun)
                    {
                        AxisSingleTo(取料位, 下料Z轴, 100);
                        WaitAxis(下料Z轴);
                    }
                    return "打开吸真空";

                case "打开吸真空":
                    下料吸真空.Set(true);
                    if (!IsDryRun)
                    {
                        WaitDI(下料吸真空到位, true, 2);
                        //if (下料吸真空到位.Get())
                        //{
                        //    覆膜通知下料有料.Set(false);
                        //    return "Z轴运动到待机位";
                        //}
                        //else
                        //{
                        //    return "打开吸真空";
                        //}
                    }

                    return "Z轴运动到待机位";

                case "Z轴运动到待机位":
                    if (!IsDryRun)
                    {
                        AxisSingleTo(待机位, 下料Z轴, 100);
                        WaitAxis(下料Z轴);
                    }
                    下料通知覆膜取料完成.Set(true);
                    return "运动到放料位";

                case "运动到放料位":
                    AxisSingleTo(放料位, 下料Y轴, 100);
                    WaitAxis(下料Y轴);
                    return "等待料仓可放料";

                case "等待料仓可放料":
                    if (料仓通知下料可放料.Get())
                    {
                        料仓通知下料可放料.Set(false);
                        return "Z轴运动到放料位";
                    }
                    else
                    {
                        return "等待料仓可放料";
                    }

                case "Z轴运动到放料位":
                    if (!IsDryRun)
                    {
                        if (!下料吸真空到位.Get())
                        {
                            int ret = ShowMessage($"{this.Name},吸真空异常".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "忽略".txtTrans(TransType.Trans), "重检".txtTrans(TransType.Trans) });
                            if (ret == 1)
                            {
                                return "Z轴运动到放料位";
                            }
                        }
                        AxisSingleTo(放料位, 下料Z轴, 100);
                        WaitAxis(下料Z轴);
                    }

                    return "关闭吸真空";

                case "关闭吸真空":
                    下料吸真空.Set(false);
                    下料破真空.Set(true);
                    Delay(1000);
                    下料破真空.Set(false);
                    return "向料仓发送放料完成";

                case "向料仓发送放料完成":

                    return "运动到待机位";

                case "运动到待机位":
                    if (!IsDryRun)
                    {
                        AxisSingleTo(待机位, 下料Z轴, 100);
                        WaitAxis(下料Z轴);
                    }

                    AxisSingleTo(待机位, 下料Y轴, 100);
                    WaitAxis(下料Y轴);
                    下料通知料仓放料完成.Set(true);
                    return "生产流程结束";

                case "生产流程结束":
                    return "生产流程开始";
            }
            return null;
        }
    }
    public static partial class HandleEx
    {
        public static double GetPos(this OK下料站.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this OK下料站.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}

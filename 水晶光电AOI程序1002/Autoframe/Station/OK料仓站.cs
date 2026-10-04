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
    public class OK料仓站 : StationEx
    {
        DI 料仓有料; DI 料仓满料; DI 料仓到位; DI 料仓解锁到位; DI 料仓锁止到位; DI 下料按钮;
        DO 料仓解锁; DO 料仓锁定;DO 下料按钮灯;
        Cylinder 料仓解锁气缸;
        RegBit 料仓初始化完成; RegBit 料仓通知下料可放料; RegBit 下料通知料仓放料完成;

        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public OK料仓站(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<NEST, ProductData>();
        }

        void HardwareConfig()
        {
            if (this.Name == "OK料仓")
            {
                下料按钮 = DI.OK下料按钮;
                料仓有料 = DI.OK料仓有料;
                料仓满料 = DI.OK料仓满料;
                料仓到位 = DI.OK料仓到位;
                料仓解锁到位 = DI.OK料仓解锁到位;
                料仓锁止到位 = DI.OK料仓锁止到位;

                料仓解锁 = DO.OK料仓解锁;
                料仓锁定 = DO.OK料仓锁定;
                下料按钮灯 = DO.OK下料按钮灯;

                料仓解锁气缸 = Cylinder.OK料仓解锁气缸;

                料仓初始化完成 = RegBit.OK料仓初始化完成;
                料仓通知下料可放料 = RegBit.OK料仓通知OK下料可放料;
                下料通知料仓放料完成 = RegBit.OK下料通知OK料仓放料完成;
            }
            else 
            {
                下料按钮 = DI.NG下料按钮;
                料仓有料 = DI.NG料仓有料;
                料仓满料 = DI.NG料仓满料;
                料仓到位 = DI.NG料仓到位;
                料仓解锁到位 = DI.NG料仓解锁到位;
                料仓锁止到位 = DI.NG料仓锁止到位;

                料仓解锁 = DO.NG料仓解锁;
                料仓锁定 = DO.NG料仓锁定;
                下料按钮灯 = DO.NG下料按钮灯;

                料仓解锁气缸 = Cylinder.NG料仓解锁气缸;

                料仓初始化完成 = RegBit.NG料仓初始化完成;
                料仓通知下料可放料 = RegBit.NG料仓通知NG下料可放料;
                下料通知料仓放料完成 = RegBit.NG下料通知NG料仓放料完成;
            }
            
            //配置站位界面显示输入
                arrayDI = new object[] { 下料按钮, 料仓有料 , 料仓满料 , 料仓到位 ,料仓解锁到位 ,料仓锁止到位 };
            //配置站位界面显示输出
            arrayDO = new object[] { 料仓解锁, 料仓锁定 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { 料仓解锁气缸 };
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
            料仓初始化完成.Set(false);
            料仓通知下料可放料.Set(false);
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
                    Delay(1000);
                    return "检查料仓状态";

                case "检查料仓状态":
                    if ((!料仓满料.Get()&& 料仓到位.Get())||IsDryRun)
                    {
                        CylinderBack(料仓解锁气缸);
                        return "发送料仓放料信号";
                    }
                    else
                        return "发送料仓不能放料信号";
                    
                //-----------------非满料/到位-----------------------
                case "发送料仓放料信号":
                    料仓通知下料可放料.Set(true);
                    return "初始化完成";
                //-----------------满料/未到位----------------------------
                case "发送料仓不能放料信号":
                    料仓通知下料可放料.Set(false);
                    return "料仓解锁";
                case "料仓解锁":
                    CylinderOut(料仓解锁气缸);
                    return "人工处理";
                case "人工处理":
                    ShowMessage($"{this.Name}料仓状态异常,请人工处理".txtTrans(TransType.Trans));
                    return "检查料仓状态";

                case "初始化完成":
                    料仓初始化完成.Set(true);

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
                    return "检测料仓是否到位";

                case "检测料仓是否到位":
                    if (料仓到位.Get())
                    {
                        return "检测料仓状态";
                    }
                    else
                        return "满料或异常处理";

                case "检测料仓状态":
                    if (!料仓满料.Get()||IsDryRun)
                    {
                        下料按钮灯.Set(false);
                        料仓通知下料可放料.Set(true);
                        return "等待下料放料完成";
                    }
                    else//满料或异常处理
                    {
                        料仓通知下料可放料.Set(false) ;
                        return "满料或异常处理";
                    }

                case "等待下料放料完成":
                    if (下料通知料仓放料完成.Get())
                    {
                        下料通知料仓放料完成.Set(false);
                        return "检测料仓状态";
                    }
                    else
                    {
                        return "等待下料放料完成";
                    }

                case "满料或异常处理":
                    下料按钮灯.Set(true);
                    return "料仓气缸解锁";

                case "料仓气缸解锁":
                    CylinderOut(料仓解锁气缸);
                    return "人工处理";

                case "人工处理":
                    ShowMessage($"{this.Name}料仓已满或未到位,请人工处理".txtTrans(TransType.Trans));
                    return "料仓按钮检测";

                case "料仓按钮检测":
                    if (下料按钮.Get())
                    {
                        return "料仓气缸锁止";
                    }
                    else
                    {
                        return "料仓按钮检测";
                    }

                case "料仓气缸锁止":
                    CylinderBack(料仓解锁气缸);
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
                    return "检测料仓是否到位";

                case "检测料仓是否到位":
                    if (料仓到位.Get())
                    {
                        return "检测料仓状态";
                    }
                    else
                        return "满料或异常处理";

                case "检测料仓状态":
                    if (料仓满料.Get())//满料或异常处理
                    {
                        
                        料仓通知下料可放料.Set(false);
                        return "满料或异常处理";
                    }
                    else
                    { 
                        下料按钮灯.Set(false);
                        料仓通知下料可放料.Set(true);
                        return "等待下料放料完成";
                    }

                case "等待下料放料完成":
                    if (下料通知料仓放料完成.Get())
                    {
                        下料通知料仓放料完成.Set(false);
                        return "检测料仓状态";
                    }
                    else
                    {
                        return "等待下料放料完成";
                    }

                case "满料或异常处理":
                    下料按钮灯.Set(true);
                    return "料仓气缸解锁";

                case "料仓气缸解锁":

                    CylinderOut(料仓解锁气缸);

                    return "人工处理";

                case "人工处理":
                    ShowMessage($"{this.Name}料仓已满或未到位,请人工处理".txtTrans(TransType.Trans));
                    return "料仓按钮检测";

                case "料仓按钮检测":
                    if (下料按钮.Get())
                    {
                        return "料仓气缸锁止";
                    }
                        return "料仓按钮检测";

                case "料仓气缸锁止":
                    CylinderBack(料仓解锁气缸);
                    return "生产流程结束";

                case "生产流程结束":
                    return "生产流程开始";
            }
            return null;
        }
    }
    public static partial class HandleEx
    {
        public static double GetPos(this OK料仓站.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this OK料仓站.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}

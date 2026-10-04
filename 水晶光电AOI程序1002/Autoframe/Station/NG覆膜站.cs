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

namespace Autoframe
{
    public class NG覆膜站 : StationEx
    {

        DI 上覆膜有料检测; DI 下覆膜有料检测; DI 覆膜下压伸出位; DI 覆膜下压缩回位; DI 覆膜切刀伸出位; DI 覆膜切刀缩回位; DI 覆膜超限信号1; DI 覆膜超限信号2; DI 流线感应光电;
        DO 覆膜下压伸出; DO 覆膜下压缩回; DO 覆膜切刀伸出; DO 覆膜切刀缩回; DO 步进流线正转; DO 气胀轴; DO 步进流线反转; DO 下料吸真空; DO 下料破真空;
        Cylinder 覆膜切刀气缸; Cylinder 覆膜下压气缸; Cylinder 料仓解锁气缸;
        AXIS 覆膜调整X轴; AXIS 覆膜调整Z轴;
        POINT 覆膜待机位; POINT 覆膜工作位;
        RegBit 覆膜初始化完成; RegBit 覆膜通知机械手可放料; RegBit 覆膜通知下料有料; RegBit 下料通知覆膜取料完成; RegBit 机械手放料完成;

        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public NG覆膜站(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<NEST, ProductData>();
        }

        void HardwareConfig()
        {
            if (this.Name == "OK覆膜")
            {
                上覆膜有料检测 = DI.OK上覆膜有料检测;
                下覆膜有料检测 = DI.OK下覆膜有料检测;
                覆膜下压伸出位 = DI.OK覆膜下压伸出位;
                覆膜下压缩回位 = DI.OK覆膜下压缩回位;
                覆膜切刀伸出位 = DI.OK覆膜切刀伸出位;
                覆膜切刀缩回位 = DI.OK覆膜切刀缩回位;
                覆膜超限信号1 = DI.OK覆膜超限信号1;
                覆膜超限信号2 = DI.OK覆膜超限信号2;
                流线感应光电 = DI.OK流线感应光电;

                覆膜下压伸出 = DO.OK覆膜下压伸出;
                覆膜下压缩回 = DO.OK覆膜下压缩回;
                覆膜切刀伸出 = DO.OK覆膜切刀伸出;
                覆膜切刀缩回 = DO.OK覆膜切刀缩回;
                步进流线正转 = DO.OK流道正转;
                步进流线反转 = DO.OK流道反转;
                气胀轴 = DO.OK气胀轴;
                下料吸真空 = DO.OK下料吸真空;
                下料破真空 = DO.OK下料破真空;

                覆膜切刀气缸 = Cylinder.OK覆膜切刀气缸;
                覆膜下压气缸 = Cylinder.OK覆膜下压气缸;
                料仓解锁气缸 = Cylinder.OK料仓解锁气缸;

                覆膜调整X轴 = AXIS.OK覆膜调整X轴;
                覆膜调整Z轴 = AXIS.OK覆膜调整Z轴;

                覆膜待机位 = POINT.覆膜待机位;
                覆膜工作位 = POINT.覆膜工作位;

                覆膜初始化完成 = RegBit.OK覆膜初始化完成;
                覆膜通知机械手可放料 = RegBit.OK覆膜通知机械手可放料;
                覆膜通知下料有料 = RegBit.OK覆膜通知OK下料有料;
                下料通知覆膜取料完成 = RegBit.OK下料通知OK覆膜取料完成;
                机械手放料完成 = RegBit.机械手通知OK覆膜放料完成;

            }
            else
            {
                上覆膜有料检测 = DI.NG上覆膜有料检测;
                下覆膜有料检测 = DI.NG下覆膜有料检测;
                覆膜下压伸出位 = DI.NG覆膜下压伸出位;
                覆膜下压缩回位 = DI.NG覆膜下压缩回位;
                覆膜切刀伸出位 = DI.NG覆膜切刀伸出位;
                覆膜切刀缩回位 = DI.NG覆膜切刀缩回位;
                覆膜超限信号1 = DI.NG覆膜超限信号1;
                覆膜超限信号2 = DI.NG覆膜超限信号2;
                流线感应光电 = DI.NG流线感应光电;

                覆膜下压伸出 = DO.NG覆膜下压伸出;
                覆膜下压缩回 = DO.NG覆膜下压缩回;
                覆膜切刀伸出 = DO.NG覆膜切刀伸出;
                覆膜切刀缩回 = DO.NG覆膜切刀缩回;
                步进流线正转 = DO.NG流道正转;
                步进流线反转 = DO.NG流道反转;
                气胀轴 = DO.NG气胀轴;
                下料吸真空 = DO.NG下料吸真空;
                下料破真空 = DO.NG下料破真空;

                覆膜切刀气缸 = Cylinder.NG覆膜切刀气缸;
                覆膜下压气缸 = Cylinder.NG覆膜下压气缸;
                料仓解锁气缸 = Cylinder.NG料仓解锁气缸;

                覆膜调整X轴 = AXIS.NG覆膜调整X轴;
                覆膜调整Z轴 = AXIS.NG覆膜调整Z轴;

                覆膜待机位 = POINT.覆膜待机位;
                覆膜工作位 = POINT.覆膜工作位;

                覆膜初始化完成 = RegBit.NG覆膜初始化完成;
                覆膜通知机械手可放料 = RegBit.NG覆膜通知机械手可放料;
                覆膜通知下料有料 = RegBit.NG覆膜通知NG下料有料;
                下料通知覆膜取料完成 = RegBit.NG下料通知OK覆膜取料完成;
                机械手放料完成 = RegBit.机械手通知NG覆膜放料完成;

            }
            //配置站位界面显示输入
            arrayDI = new object[] { 上覆膜有料检测, 下覆膜有料检测, 覆膜下压伸出位, 覆膜下压缩回位, 覆膜切刀伸出位, 覆膜切刀缩回位, 覆膜超限信号1, 覆膜超限信号2, 流线感应光电 };
            //配置站位界面显示输出
            arrayDO = new object[] { 覆膜下压伸出, 覆膜下压缩回, 覆膜切刀伸出, 覆膜切刀缩回, 气胀轴, 步进流线正转 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { 覆膜切刀气缸, 覆膜下压气缸, 料仓解锁气缸 };
            //配置站位轴
            arrayAxis = new object[] { 覆膜调整X轴, 覆膜调整Z轴 };
        }
        /// <summary>
        /// OutCliEnable
        /// </summary>
        private bool OutCliEnable = false;
        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            覆膜待机位,
            覆膜工作位,
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

            覆膜调整X轴.ServoOn(false);
            覆膜调整Z轴.ServoOn(false);
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
            覆膜初始化完成.Set(false);
            覆膜通知机械手可放料.Set(false);
            覆膜通知下料有料.Set(false);
            下料通知覆膜取料完成.Set(false);
            switch (Process)
            {
                case AutoProcess.生产:
                case AutoProcess.空跑:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "生产流程开始";
                    break;
                case AutoProcess.NG覆膜一键出料:
                    this.InitEntry = "初始化流程开始";
                    下料通知覆膜取料完成.Set(true);
                    this.AutoEntry = "生产流程开始";
                    break;
                default:
                    this.InitEntry = StepExit;
                    this.AutoEntry = StepEmpty;
                    break;
            }
        }


        private int 单行数量 = 0;
        private int 行数 = 0;
        private int 切膜次数 = 0;
        private bool 第一次 = false;
        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case "初始化流程开始":
                    第一次 = true;
                    切膜次数 = 0;
                    //int reac_num1 = 0;
                    //步进流线正转.Set(true);
                    //while (reac_num1 <= Parameter.GetInt("流线感应次数") - 1 && currentState != StationState.Pause && currentState != StationState.Emg)
                    //{
                    //    if (流线感应光电.Get())
                    //    {
                    //        reac_num1++;
                    //        RegInt.TestCount.Set(reac_num1);
                    //        while (流线感应光电.Get() && currentState != StationState.Pause && currentState != StationState.Emg)
                    //        {

                    //Delay(3000);
                    气胀轴.Set(true);
                    return "等待机械手初始化完成";

                case "等待机械手初始化完成":
                    if (RegBit.机械手初始化完成.Get())
                    {
                        return "轴上使能";
                    }
                    else
                    {
                        return "等待机械手初始化完成";
                    }

                case "轴上使能":
                    单行数量 = Parameter.GetInt("覆膜单行数量");
                    行数 = Parameter.GetInt("覆膜行数");
                    覆膜调整X轴.ServoOn(true);
                    覆膜调整Z轴.ServoOn(true);
                    //double cc = Parameter.GetDouble("满盘转动位移");
                    //覆膜调整X轴.RelativeMove(Parameter.GetDouble("满盘转动位移"), AxisHandle.SpeedPercent);
                    //WaitAxis(覆膜调整X轴);
                    Delay(1000);
                    //Home35(覆膜调整X轴);
                    覆膜调整X轴.Home(true, 35);
                    return "气缸伸出";

                case "气缸伸出":
                    CylinderBack(覆膜下压气缸);
                    CylinderOut(覆膜切刀气缸);
                    return "有料检测";

                case "有料检测":
                    if (true)
                    //if ((上覆膜有料检测.Get() && 下覆膜有料检测.Get() && !覆膜超限信号1.Get() && !覆膜超限信号2.Get())||IsDryRun)
                    {
                        return "判断产品数量";
                    }
                    return "人工处理";

                case "人工处理":
                    ShowMessage($"{this.Name}覆膜已满,请人工更换后按按钮启动".txtTrans(TransType.Trans));
                    //if (ShowMessage($"{this.Name}覆膜有料,请检查", "人工处理有料", new string[] { "确认OK" }) == 0)
                    //{
                    //    return "有料检测";
                    //}
                    //else
                    //    return "人工处理";
                    return "按钮检测";
                case "按钮检测":
                    if (DI.覆膜上料按钮.Get())
                    {
                        return "有料检测";
                    }
                    else
                    {
                        return "按钮检测";
                    }

                case "判断产品数量":
                    string num = "0";
                    if (this.Name == "OK覆膜")
                    {
                        num = Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount", "");
                    }
                    else
                    {
                        num = Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", "");
                    }

                    if (Convert.ToInt32(num) % 单行数量 != 0)
                    {
                        return "允许放料";
                    }
                    return "不允许放料";
                //-------------------------OK放料-------------------------------
                case "允许放料":
                    覆膜通知机械手可放料.Set(true);
                    //结束初始化
                    return "初始化完成";
                //-------------------------NG放料---------------------------------
                case "不允许放料":
                    覆膜通知机械手可放料.Set(false);
                    return "调整X轴覆膜";

                case "调整X轴覆膜":
                    步进流线正转.Set(true);
                    //AxisSingleTo(覆膜工作位, 覆膜调整X轴);
                    覆膜调整X轴.RelativeMove(Parameter.GetDouble("满行转动位移"), AxisHandle.SpeedPercent);
                    WaitAxis(覆膜调整X轴);
                    步进流线正转.Set(false);
                    return "判断下料完成";

                case "判断下料完成":
                    if (下料通知覆膜取料完成.Get())
                    {
                        下料通知覆膜取料完成.Set(false);
                        return "流线出料";
                    }
                    else
                    {
                        return "判断下料完成";
                    }

                case "流线出料":
                    步进流线正转.Set(true);
                    return "流线有料检测";

                case "流线有料检测":
                    if (!IsDryRun)
                    {
                        int reac_num = 0;
                        while (reac_num <= Parameter.GetInt("流线感应次数") - 1 && currentState != StationState.Pause && currentState != StationState.Emg)
                        {
                            if (流线感应光电.Get())
                            {
                                reac_num++;
                                RegInt.TestCount.Set(reac_num);
                                while (流线感应光电.Get() && currentState != StationState.Pause && currentState != StationState.Emg)
                                {

                                }
                            }
                        }
                    }
                    else
                    {
                        Delay(1000);
                    }
                    步进流线正转.Set(false);


                    return "允许放料";
                case "初始化完成":
                    AxisGoto(POINT.覆膜工作位, new AXIS[] { 覆膜调整Z轴 }, 100, true);
                    WaitAxis(覆膜调整Z轴);
                    return StepExit;
                default:
                    return null;
            }
        }
        string num = "0";
        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case "生产流程开始":
                    return "超限检测";
                case "超限检测":
                    if ((!覆膜超限信号1.Get() && !覆膜超限信号2.Get()) || IsDryRun)
                    {
                        return "有料/满料检测";
                    }
                    ShowMessage($"{this.Name}覆膜超限,请人工确认".txtTrans(TransType.Trans));
                    return "超限检测";
                case "有料/满料检测":
                    //if (true || IsDryRun)
                    if ((上覆膜有料检测.Get() && 下覆膜有料检测.Get()) || IsDryRun)
                    {
                        AxisGoto(POINT.覆膜工作位, new AXIS[] { 覆膜调整Z轴 }, 100, true);
                        WaitAxis(覆膜调整Z轴);
                        return "判断物料个数";
                    }
                    AxisGoto(POINT.覆膜待机位, new AXIS[] { 覆膜调整Z轴 }, 100, true);
                    WaitAxis(覆膜调整Z轴);
                    ShowMessage($"{this.Name}覆膜已满,请人工更换".txtTrans(TransType.Trans));
                    return "有料/满料检测";

                case "判断物料个数":

                    if (this.Name == "OK覆膜")
                    {
                        num = Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount", "");
                    }
                    else
                    {
                        num = Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", "");
                    }
                    if (单行数量 == 0)
                    {
                        单行数量 = Parameter.GetInt("覆膜单行数量");
                    }
                    if (Convert.ToInt32(num) % 单行数量 == 0)
                    {
                        覆膜通知机械手可放料.Set(false);//只有不满行，机械手才准放
                        if (Convert.ToInt32(num) / 单行数量 == 9)
                        {
                            //if (第一次)
                            //{
                            //    第一次 = false;
                            //    return "调整X轴单步运动";
                            //}
                            return "切膜动作";
                        }
                        //满行
                        if (Convert.ToInt32(num) / 单行数量 >= 行数)
                        {
                            num = "0";
                            切膜次数++;
                            //满盘
                            return "换盘";
                        }
                        else
                        {
                            //下一行
                            return "调整X轴单步运动";
                        }

                    }
                    else
                    {
                        //不满行
                        return "通知机械手可放料";
                    }
                //----------------------下一行----------------------------
                case "调整X轴单步运动":
                    步进流线正转.Set(true);
                    覆膜调整X轴.RelativeMove(Parameter.GetDouble("满行转动位移"), AxisHandle.SpeedPercent);
                    WaitAxis(覆膜调整X轴);
                    步进流线正转.Set(false);
                    return "通知机械手可放料";
                //----------------------不满行----------------------------
                case "通知机械手可放料":
                    覆膜通知机械手可放料.Set(true);
                    return "等待机械手放料完成";
                //----------------------满盘----------------------------
                case "换盘":
                    if (this.Name == "OK覆膜")
                    {
                        Ini.WriteValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount", "0");
                    }
                    else
                    {
                        Ini.WriteValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", "0");
                    }
                    return "调整X轴运动";
                case "调整X轴运动":
                    步进流线正转.Set(true);
                    覆膜调整X轴.RelativeMove(Parameter.GetDouble("满盘转动位移"), AxisHandle.SpeedPercent);
                    WaitAxis(覆膜调整X轴);
                    步进流线正转.Set(false);
                    return "通知机械手可放料";

                case "切膜动作":
                    CylinderOut(覆膜下压气缸);
                    Delay(500);
                    CylinderBack(覆膜切刀气缸);
                    return "切膜返回";

                case "切膜返回":
                    CylinderBack(覆膜下压气缸);
                    CylinderOut(覆膜切刀气缸);

                    return "流线出料";

                case "流线出料":
                    步进流线正转.Set(true);
                    return "发送出料有料";

                case "发送出料有料":

                    int reac_num = 0;
                    while (reac_num <= Parameter.GetInt("流线感应次数") - 1 && currentState != StationState.Pause && currentState != StationState.Emg)
                    {
                        if (流线感应光电.Get())
                        {
                            reac_num++;
                            RegInt.TestCount.Set(reac_num);
                            while (流线感应光电.Get() && currentState != StationState.Pause && currentState != StationState.Emg)
                            {

                            }
                        }
                    }

                    步进流线正转.Set(false);
                    覆膜通知下料有料.Set(true);
                    WaitRegBit(下料通知覆膜取料完成, true);
                    下料通知覆膜取料完成.Set(false);
                    return "调整X轴单步运动";

                case "等待机械手放料完成":

                    if (机械手放料完成.Get())
                    {
                        机械手放料完成.Set(false);
                        return "生产流程结束";
                    }
                    else
                    {
                        return "等待机械手放料完成";
                    }

                case "生产流程结束":
                    return "生产流程开始";

                default:
                    return null;

            }
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
                    return "超限检测";
                case "超限检测":
                    if ((!覆膜超限信号1.Get() && !覆膜超限信号2.Get()) || IsDryRun)
                    {
                        return "有料/满料检测";
                    }
                    ShowMessage($"{this.Name}覆膜超限,请人工确认".txtTrans(TransType.Trans));
                    return "超限检测";
                case "有料/满料检测":
                    //if (true || IsDryRun)
                    if ((上覆膜有料检测.Get() && 下覆膜有料检测.Get()) || IsDryRun)
                    {
                        AxisGoto(POINT.覆膜工作位, new AXIS[] { 覆膜调整Z轴 }, 100, true);
                        WaitAxis(覆膜调整Z轴);
                        return "判断物料个数";
                    }
                    AxisGoto(POINT.覆膜待机位, new AXIS[] { 覆膜调整Z轴 }, 100, true);
                    WaitAxis(覆膜调整Z轴);
                    ShowMessage($"{this.Name}：膜用完,请人工更换".txtTrans(TransType.Trans));
                    return "有料/满料检测";


                case "判断物料个数":

                    if (this.Name == "OK覆膜")
                    {
                        num = Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount", "");
                    }
                    else
                    {
                        num = Ini.GetStringValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", "");
                    }
                    num = (单行数量 - Convert.ToInt32(num) % 单行数量 + Convert.ToInt32(num)).ToString();
                    if (this.Name == "OK覆膜")
                    {
                        Ini.WriteValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount", num);
                    }
                    else
                    {
                        Ini.WriteValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", num);
                    }
                    if (Convert.ToInt32(num) % 单行数量 == 0)
                    {
                        覆膜通知机械手可放料.Set(false);//只有不满行，机械手才准放
                        if (Convert.ToInt32(num) / 单行数量 == 9)
                        {
                            return "切膜动作";
                        }
                        //满行
                        if (Convert.ToInt32(num) / 单行数量 >= 行数)
                        {
                            num = "0";
                            切膜次数++;
                            //满盘
                            return "换盘";
                        }
                        else
                        {
                            //下一行
                            return "调整X轴单步运动";
                        }

                    }
                    else
                    {
                        //不满行
                        return "通知机械手可放料";
                    }
                //----------------------下一行----------------------------
                case "调整X轴单步运动":
                    步进流线正转.Set(true);
                    覆膜调整X轴.RelativeMove(Parameter.GetDouble("满行转动位移"), AxisHandle.SpeedPercent);
                    WaitAxis(覆膜调整X轴);
                    步进流线正转.Set(false);
                    return "生产流程结束";
                ////----------------------不满行----------------------------
                //case "通知机械手可放料":
                //    覆膜通知机械手可放料.Set(true);
                //    return "等待机械手放料完成";
                //----------------------满盘----------------------------
                case "换盘":
                    if (this.Name == "OK覆膜")
                    {
                        Ini.WriteValue(Program.StationsParam_IniFile, "CountParam", "OKFilmCount", "0");
                    }
                    else
                    {
                        Ini.WriteValue(Program.StationsParam_IniFile, "CountParam", "NGFilmCount", "0");
                    }
                    return "调整X轴运动";
                case "调整X轴运动":
                    步进流线正转.Set(true);
                    覆膜调整X轴.RelativeMove(Parameter.GetDouble("满盘转动位移"), AxisHandle.SpeedPercent);
                    WaitAxis(覆膜调整X轴);
                    步进流线正转.Set(false);
                    return "生产流程结束";

                case "切膜动作":
                    CylinderOut(覆膜下压气缸);
                    Delay(500);
                    CylinderBack(覆膜切刀气缸);
                    return "切膜返回";

                case "切膜返回":
                    CylinderBack(覆膜下压气缸);
                    CylinderOut(覆膜切刀气缸);

                    return "流线出料";

                case "流线出料":
                    步进流线正转.Set(true);
                    return "发送出料有料";

                case "发送出料有料":

                    int reac_num = 0;
                    while (reac_num <= Parameter.GetInt("流线感应次数") - 1 && currentState != StationState.Pause && currentState != StationState.Emg)
                    {
                        if (流线感应光电.Get())
                        {
                            reac_num++;
                            RegInt.TestCount.Set(reac_num);
                            while (流线感应光电.Get() && currentState != StationState.Pause && currentState != StationState.Emg)
                            {

                            }
                        }
                    }

                    步进流线正转.Set(false);
                    覆膜通知下料有料.Set(true);
                    WaitRegBit(下料通知覆膜取料完成, true);
                    下料通知覆膜取料完成.Set(false);
                    return "判断满盘切膜次数";

                case "判断满盘切膜次数":
                    if (切膜次数 >= 2)
                    {
                        int Sw = ShowMessage("满盘切膜次数达到2次", "提示", new string[] { "继续", "拿料退出" });
                        // "扩膜站".GetStation<扩膜站>()[扩膜站.NEST.穴位].BreakDownTime += Duration;
                        if (1 == Sw)
                        {

                            CylinderOut(料仓解锁气缸);
                            StationMgr.Pause();
                            Delay(200);
                            StationMgr.Stop();
                        }
                        切膜次数 = 0;
                    }
                    else
                        Delay(8000); 
                    return "调整X轴单步运动";
                case "生产流程结束":
                    return "生产流程开始";

                default:
                    return null;

            }
        }

        private bool Func_ReadOKData(string path, ref string OutData)
        {
            if (string.IsNullOrEmpty(path)) return false;
            bool bIsExist = File.Exists(path);

            if (bIsExist)
            {
                using (StreamReader sw = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.Default))
                {
                    OutData = sw.ReadLine();
                }
                return true;
            }
            else
                return false;
        }



        private bool Single_RelativeMove(double fpos, AXIS enumAxis, int nSpeedPercent = 100, double dbAcc = 0, double dbDec = 0, bool bPause = false)
        {

            if (!enumAxis.GetType().IsEnum)
                throw new Exception("轴枚举错误".txtTrans(TransType.Mark) + $"：{enumAxis}");

            if (nSpeedPercent > 100 || nSpeedPercent < 1)
            {
                throw new Exception("速度百分比超限".txtTrans(TransType.Mark));
            }

            AxisHandle axisHandle = CardMgr.GetAxis((int)enumAxis);
            if (axisHandle == null)
                throw new Exception("未获取到轴".txtTrans(TransType.Mark) + $"：{enumAxis}");
            return axisHandle.RelativeMove(fpos, nSpeedPercent, dbAcc, dbDec);
        }
    }

    //private bool Func_ReadData(string path, ref string OutData) 

    public static partial class HandleEx
    {
        public static double GetPos(this NG覆膜站.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this NG覆膜站.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}

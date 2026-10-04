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
    public class IR检测2 : StationEx
    {
        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public IR检测2(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<NEST, ProductData>();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { DI.IR面吸真空5到位, DI.IR面吸真空6到位, DI.IR面吸真空7到位, DI.IR面吸真空8到位 };
            //配置站位界面显示输出
            arrayDO = new object[] { DO.CCD6触发, DO.IR面2吸真空, DO.IR面2破真空, DO.IR面吸破真空5, DO.IR面吸破真空6, DO.IR面吸破真空7, DO.IR面吸破真空8 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { };
            //配置站位轴
            arrayAxis = new object[] { AXIS.IR面检测X2轴, AXIS.IR面检测Y2轴, AXIS.IR面检测Z2轴 };
        }

        ProductData productData = new ProductData();

        TcpCommu_CCD3 camera = new TcpCommu_CCD3();
        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            待机位,
            上料位1,
            上料位2,
            上料位3,
            上料位4,
            拍照位11,
            拍照位12,
            拍照位13,
            拍照位21,
            拍照位22,
            拍照位23,
            拍照位31,
            拍照位32,
            拍照位33,
            拍照位41,
            拍照位42,
            拍照位43,
            下料位,
            下料顶升位
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
            RegBit.IR检测2上料位.Set(false);
            RegBit.IR检测2下料位.Set(false);
            RegBit.IR检测2下料等待位.Set(false);
            RegBit.IR检测2初始化吸嘴有料.Set(false);
            RegBit.IR检测2初始化完成.Set(false);
            RegBit.IR检测2待机位.Set(false);
            RegBit.IR检测2拍照位.Set(false);
            RegBit.IR检测2拍照等待位.Set(false);
            RegBit.IR检测2缩回位.Set(false);
            RegBit.IR检测2通知摆臂接料准备OK.Set(false);
            RegBit.IR检测通知翻转有料.Set(false);
            switch (Process)
            {
                case AutoProcess.生产:
                case AutoProcess.空跑:
                    if (Parameter.GetBit("IR检测2Enable"))
                    {
                        this.InitEntry = "初始化流程开始";
                        this.AutoEntry = "生产流程开始";
                    }
                    break;
                case AutoProcess.CCD2标定注册:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = StepEmpty;
                    break;
                case AutoProcess.CCD3静态:
                case AutoProcess.CCD3动态:
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
                    //if (Parameter.GetBit("IR检测1Enable"))
                    //{
                    //    ShowMessage($"IR检测2:确保IR检测1模组在安全位。".txtTrans(TransType.Trans));
                    //}
                    //return StepExit;
                    //Delay(10);
                    //camera.Init(this, 3);
                    //camera.DeInit();
                    //string[] strTemp = new string[] { };
                    //camera.SendCmd(out strTemp, (int)TcpCommu_Camera.Command.CCD1_上料排片引导, false);
                    //camera.WaitReplyAndProcess(out strTemp, (int)TcpCommu_Camera.Command.CCD1_上料排片引导);
                    return "连接相机";

                case "连接相机":
                    if (!IsDryRun)
                    {
                        if (!camera.Init(this, 3))
                            return "连接相机";
                        else
                            Logger.Process(Name, "相机连接成功".txtTrans(TransType.Trans));

                        if (!Parameter.GetBit("IR检测1Enable"))
                        {
                            Delay(1000);
                            string[] strccd3 = new string[] { };
                            camera.SendCmd(out strccd3, (int)TcpCommu_CCD3.Command.CCD3_复位, true);
                            if (strccd3[1] == "OK")
                            {
                                return "轴上使能";
                            }

                            EventMgr.Error($"{Name}CCD3初始化失败".txtTrans(TransType.Mark));
                            return StepExit;
                        }
                        
                    }

                    return "轴上使能";

                case "轴上使能":
                    AXIS.IR面检测Z2轴.ServoOn(false);
                    Delay(1000);
                    AXIS.IR面检测X2轴.ServoOn(true);
                    AXIS.IR面检测Y2轴.ServoOn(true);
                    AXIS.IR面检测Z2轴.ServoOn(true);
                    Delay(1000);
                    return "等待翻转和摆臂初始化完成";

                case "等待翻转和摆臂初始化完成":
                    if (RegBit.翻转初始化完成.Get() && RegBit.摆臂站初始化完成.Get())
                    {
                        return "吸嘴吸真空";
                    }
                    return "等待翻转和摆臂初始化完成";
                case "吸嘴吸真空":
                    DO.IR面2吸真空.Set(true);
                    DO.IR面2破真空.Set(false);
                    DO.IR面吸破真空5.Set(true);
                    DO.IR面吸破真空6.Set(true);
                    DO.IR面吸破真空7.Set(true);
                    DO.IR面吸破真空8.Set(true);
                    return "Y轴回原点";

                case "Y轴回原点":
                    AXIS.IR面检测Y2轴.Home();
                    WaitAxis(AXIS.IR面检测Y2轴);
                    return "X轴回原点";

                case "X轴回原点":
                    AXIS.IR面检测X2轴.Home();
                    WaitAxis(AXIS.IR面检测X2轴);
                    return "Y轴回待机位";
                case "Y轴回待机位":
                    //StationBase 翻转ex = new 翻转("翻转");
                    //POINT.待机位.GetPos(AXIS.IR面检测Y1轴, 翻转ex);
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.IR面检测Y2轴 }, 100, true);
                    WaitAxis(AXIS.IR面检测Y2轴);
                    //AXIS.IR面检测Y1轴.AbsMove(POINT.待机位.GetPos(AXIS.IR面检测Y1轴, this), 100);
                    return "X轴回待机位";

                case "X轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.IR面检测X2轴 }, 100, true);
                    WaitAxis(AXIS.IR面检测X2轴);
                    //AXIS.IR面检测Y1轴.AbsMove(POINT.待机位.GetPos(AXIS.IR面检测Y1轴,this), 100);
                    return "Z轴回原点";
                case "Z轴回原点":
                    AXIS.IR面检测Z2轴.Home();
                    WaitAxis(AXIS.IR面检测Z2轴);
                    return "Z轴回待机位";
                case "Z轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.IR面检测Z2轴 }, 100, true);
                    WaitAxis(AXIS.IR面检测Z2轴);
                    //AXIS.IR面检测Y1轴.AbsMove(POINT.待机位.GetPos(AXIS.IR面检测Y1轴,this), 100);
                    return "判断吸真空";
                case "判断吸真空":
                    Delay(2000);
                    if (!IsDryRun)
                    {
                        bool[] IR1SuckIn = new bool[4];
                        IR1SuckIn[0] = DI.IR面吸真空5到位.Get();
                        IR1SuckIn[1] = DI.IR面吸真空6到位.Get();
                        IR1SuckIn[2] = DI.IR面吸真空7到位.Get();
                        IR1SuckIn[3] = DI.IR面吸真空8到位.Get();
                        if (!IR1SuckIn.Contains(true))
                        {
                            RegBit.IR检测2初始化吸嘴有料.Set(false);

                            DO.IR面2吸真空.Set(false);
                            DO.IR面2破真空.Set(true);
                            Delay(500);
                            DO.IR面2吸真空.Set(false);
                            DO.IR面2破真空.Set(false);
                            DO.IR面吸破真空5.Set(false);
                            DO.IR面吸破真空6.Set(false);
                            DO.IR面吸破真空7.Set(false);
                            DO.IR面吸破真空8.Set(false);
                            this[NEST.穴位1] = null;
                            this[NEST.穴位2] = null;
                            this[NEST.穴位3] = null;
                            this[NEST.穴位4] = null;
                        }
                        else if (!IR1SuckIn.Contains(false))
                        {
                            int ret = ShowMessage($"{this.Name},确认是否有料".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "有料".txtTrans(TransType.Trans), "无料".txtTrans(TransType.Trans) });
                            if (ret == 1)
                            {
                                this[NEST.穴位1] = null;
                                this[NEST.穴位2] = null;
                                this[NEST.穴位3] = null;
                                this[NEST.穴位4] = null;
                                RegBit.IR检测2初始化吸嘴有料.Set(false);
                            }
                            else
                                RegBit.IR检测2初始化吸嘴有料.Set(true);
                        }
                        else
                        {
                            ShowMessage($"{this.Name}请取走吸嘴上的料".txtTrans(TransType.Trans));
                            return "判断吸真空";
                        }
                    }
                    else
                    {
                        RegBit.IR检测2初始化吸嘴有料.Set(false);
                    }
                    return "初始化完成";
                case "初始化完成":
                    RegBit.IR检测2待机位.Set(true);
                    RegBit.IR检测2初始化完成.Set(true);
                    //结束初始化
                    return StepExit;
                default:
                    return null;
            }
        }
        int UploadSit = 0;
        int CCDSit = 0;
        string CCDTime = null;

        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case "生产流程开始":
                    //ProductData tempData = new ProductData();

                    //New 载具
                    //Carrier newCarrier = new Carrier() { CarrierID = $"IR检测1" };
                    //newCarrier.Nests = new Nest[] { new Nest("穴位1"), new Nest("穴位2"), new Nest("穴位3"), new Nest("穴位4") };//载具只有一个穴位，如有多个穴位，Nests数量New 多个
                    return "判断初始化吸嘴结果";
                case "判断初始化吸嘴结果":
                    if (RegBit.IR检测2初始化吸嘴有料.Get())
                    {
                        if (!RegBit.IR检测1上料位.Get() && !RegBit.IR检测1待机位.Get())
                        {
                            RegBit.IR检测2初始化吸嘴有料.Set(false);
                            return "等待IR检测1不在拍照位";
                        }
                        return "判断初始化吸嘴结果";
                    }
                    //this[NEST.穴位1] = new ProductData();
                    //this[NEST.穴位2] = new ProductData();
                    //this[NEST.穴位3] = new ProductData();
                    //this[NEST.穴位4] = new ProductData();
                    return "打开吸真空";
                case "打开吸真空":
                    DO.IR面2吸真空.Set(true);
                    DO.IR面2破真空.Set(false);
                    //DO.IR面吸破真空5.Set(true);
                    //DO.IR面吸破真空6.Set(true);
                    //DO.IR面吸破真空7.Set(true);
                    //DO.IR面吸破真空8.Set(true);
                    return "等待IR检测1不在上料位";

                case "等待IR检测1不在上料位":
                    if (!RegBit.IR检测1上料位.Get() && !RegBit.IR检测1待机位.Get())
                    {
                        UploadSit = 1;
                        RegBit.IR检测2上料位.Set(true);
                        return "去上料位";
                    }
                    return "等待IR检测1不在上料位";
                case "去上料位":
                    switch (UploadSit)
                    {
                        case 1:
                            AxisGoto(POINT.上料位1, new AXIS[] { AXIS.IR面检测Z2轴, AXIS.IR面检测Y2轴, AXIS.IR面检测X2轴 }, 100, true);
                            break;
                        case 2:
                            AxisGoto(POINT.上料位2, new AXIS[] { AXIS.IR面检测Z2轴, AXIS.IR面检测Y2轴, AXIS.IR面检测X2轴 }, 100, true);
                            break;
                        case 3:
                            AxisGoto(POINT.上料位3, new AXIS[] { AXIS.IR面检测Z2轴, AXIS.IR面检测Y2轴, AXIS.IR面检测X2轴 }, 100, true);
                            break;
                        case 4:
                            AxisGoto(POINT.上料位4, new AXIS[] { AXIS.IR面检测Z2轴, AXIS.IR面检测Y2轴, AXIS.IR面检测X2轴 }, 100, true);
                            break;
                        default:
                            ShowMessage($"{this.Name}上料{UploadSit}，没有该点位".txtTrans(TransType.Trans));
                            return null;
                    }
                    WaitAxis(AXIS.IR面检测Z2轴);
                    WaitAxis(AXIS.IR面检测Y2轴);
                    WaitAxis(AXIS.IR面检测X2轴);

                    RegBit.IR检测2待机位.Set(false);
                    RegBit.IR检测2通知摆臂接料准备OK.Set(true);
                    //    //New 物料
                    return "等待摆臂到IR放料位";
                case "等待摆臂到IR放料位":
                    if (RegBit.摆臂通知IR检测到位放料.Get())
                    {
                        RegBit.摆臂通知IR检测到位放料.Set(false);
                        switch (UploadSit)
                        {
                            case 1:
                                DO.IR面吸破真空5.Set(true);
                                break;
                            case 2:
                                DO.IR面吸破真空6.Set(true);
                                break;
                            case 3:
                                DO.IR面吸破真空7.Set(true);
                                break;
                            case 4:
                                DO.IR面吸破真空8.Set(true);
                                break;
                            default:
                                ShowMessage($"{this.Name}上料{UploadSit}，没有".txtTrans(TransType.Trans));
                                return null;
                        }
                        RegBit.IR检测需要清料.Set(true);
                        return "等待摆臂通知IR检测放料完成";
                    }
                    return "等待摆臂到IR放料位";
                case "等待摆臂通知IR检测放料完成":
                    if (RegBit.摆臂通知IR检测放料完成.Get())
                    {
                        RegBit.摆臂通知IR检测放料完成.Set(false);
                        RegBit.IR检测需要清料.Set(false);
                        this[(NEST)(UploadSit - 1)] = "摆臂站".GetStation<摆臂站>()[摆臂站.NEST.穴位];
                        "摆臂站".GetStation<摆臂站>()[摆臂站.NEST.穴位] = null;
                        //RingMonitor.AssignNextPos(this[(NEST)(UploadSit - 1)]);
                        return "检测真空";
                    }
                    if (RegBit.IR载具清料.Get() && UploadSit >=2 )
                    {
                        RegBit.IR检测需要清料.Set(false);
                        for (int i = UploadSit; i < 5; i++)
                        {
                            this[(NEST)(i - 1)] = new ProductData();
                            this[(NEST)(i - 1)].Result = false;
                            this[(NEST)(i - 1)].NGReason = "清料";
                        }
                        return "等待IR检测1不在拍照位";
                    }
                    return "等待摆臂通知IR检测放料完成";

                case "检测真空":
                    Delay(150);
                    if (!IsDryRun)
                    {
                        switch (UploadSit)
                        {
                            case 1:
                                WaitDI(DI.IR面吸真空5到位, true);
                                break;
                            case 2:
                                WaitDI(DI.IR面吸真空6到位, true);
                                break;
                            case 3:
                                WaitDI(DI.IR面吸真空7到位, true);
                                break;
                            case 4:
                                WaitDI(DI.IR面吸真空8到位, true);
                                break;
                            default:
                                ShowMessage($"{this.Name}上料{UploadSit}，没有该点位".txtTrans(TransType.Trans));
                                return null;
                        }

                    }
                    this[(NEST)(UploadSit - 1)].BreakDownTime += Duration;
                    UploadSit++;
                    if (UploadSit < 5)
                        return "去上料位";
                    UploadSit = 0;
                    if (Parameter.GetBit("调试模式"))
                    {
                        if (0 == ShowMessage("人工确认IR检测2料效果，确认‘重新接料’还是‘下一步’".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "重新接料", "下一步".txtTrans(TransType.Trans) }))
                        {
                            return "生产流程开始";
                        }
                    }
                    return "等待IR检测1不在拍照位";
                case "等待IR检测1不在拍照位":
                    if (!RegBit.IR检测1拍照位.Get() && (!RegBit.IR检测1待机位.Get()|| RegBit.IR检测2上料位.Get()) && (RegBit.IR检测2上料位.Get()|| RegBit.IR检测2待机位.Get()))
                    {
                        RegBit.IR检测2初始化吸嘴有料.Set(false);
                        CCDSit = 1;
                        RegBit.IR检测2拍照位.Set(true);
                        return "到拍照位";
                    }
                    return "等待IR检测1不在拍照位";
                case "到拍照位":
                    POINT photoPt;
                    switch (CCDSit)
                    {
                        case 1:
                            photoPt = POINT.拍照位11;
                            break;
                        case 2:
                            photoPt = POINT.拍照位12;
                            break;
                        case 3:
                            photoPt = POINT.拍照位13;
                            break;
                        case 4:
                            photoPt = POINT.拍照位13;
                            break;
                        case 5:
                            WaitRegBit(RegBit.IR检测1下料位, false);
                            photoPt = POINT.拍照位21;
                            break;
                        case 6:
                            photoPt = POINT.拍照位22;
                            break;
                        case 7:
                            photoPt = POINT.拍照位23;
                            break;
                        case 8:
                            photoPt = POINT.拍照位23;
                            break;
                        case 9:
                            photoPt = POINT.拍照位31;
                            break;
                        case 10:
                            photoPt = POINT.拍照位32;
                            break;
                        case 11:
                            photoPt = POINT.拍照位33;
                            break;
                        case 12:
                            photoPt = POINT.拍照位33;
                            break;
                        case 13:
                            photoPt = POINT.拍照位41;
                            break;
                        case 14:
                            photoPt = POINT.拍照位42;
                            break;
                        case 15:
                            photoPt = POINT.拍照位43;
                            break;
                        case 16:
                            photoPt = POINT.拍照位43;
                            break;
                        default:
                            ShowMessage($"{this.Name}上料{UploadSit}，没有该点位".txtTrans(TransType.Trans));
                            return null;
                    }
                    AxisGoto(photoPt, new AXIS[] { AXIS.IR面检测Y2轴, AXIS.IR面检测X2轴 }, 100, true);
                    if (CCDSit == 5 || CCDSit == 9 || CCDSit == 13)
                    {
                        //Delay(100);
                        string[] strResult = new string[] { };
                        if (Parameter.GetBit("VisionEnable"))
                        {
                            camera.WaitReplyAndProcess(out strResult, (int)TcpCommu_CCD3.Command.CCD3_IR面检测_获取结果);
                            //if (strResult[1] != CCDTime || ((CCDSit - 1) / 4).ToString() != strResult[2])
                            //{
                            //    int rret = ShowMessage(this.Name, $"CCD3发送SN：{CCDTime},穴位：{((CCDSit - 1) / 4).ToString()};CCD3回复SN：{strResult[1]},穴位：{strResult[2]},有误，请检查后，确认‘重新收取’还是‘忽略’".txtTrans(TransType.Trans),
                            //        new string[] { "重新收取".txtTrans(TransType.Trans), "忽略".txtTrans(TransType.Trans) });
                            //    this[(NEST)((CCDSit - 1) / 4 - 1)].BreakDownTime += Duration;
                            //    if (0 == rret)
                            //    {
                            //        WaitAxis(AXIS.IR面检测Y2轴);
                            //        WaitAxis(AXIS.IR面检测X2轴);
                            //        return "到拍照位";
                            //    }
                            //    else
                            //    {
                            //        this[(NEST)((CCDSit - 1) / 4 - 1)].Result = false;
                            //        this[(NEST)((CCDSit - 1) / 4 - 1)].NGReason += $"CCD3检测NG{camera.RecString}；";
                            //    }
                            //}
                        }

                        if (IsDryRun)
                            this[(NEST)((CCDSit - 1) / 4 - 1)].Result = true;
                        else
                        {
                            this[(NEST)((CCDSit - 1) / 4) - 1].CCD3Sit = $"IR_2:{(CCDSit - 1) / 4}:{strResult[3]}";
                            this[(NEST)((CCDSit - 1) / 4 - 1)].Result = strResult[3].Contains("OK");
                            if (!this[(NEST)((CCDSit - 1) / 4 - 1)].Result)
                                this[(NEST)((CCDSit - 1) / 4 - 1)].NGReason += $"CCD3检测NG{strResult[4]}；";

                            if (strResult[4].Contains("NULL"))
                            {
                                var list = strResult.ToList();
                                list.Insert(0, DateTime.Now.ToString("yyyyMMddyyyyMMdd_HH_mm_ss.fff"));   // 在索引0插入
                                var arr = list.ToArray();
                                SaveCsv("D:\\exe\\Log\\ZH\\Others\\CCD3CCD6NULL", DateTime.Now.ToString("yyyyMMdd"), "Time,CMD,SN,Slot,Result,NGreason", arr);
                            }
                        }
                    }
                    WaitAxis(AXIS.IR面检测Y2轴);
                    WaitAxis(AXIS.IR面检测X2轴);
                    AxisGoto(photoPt, new AXIS[] { AXIS.IR面检测Z2轴 }, 100, true);
                    WaitAxis(AXIS.IR面检测Z2轴);
                    RegBit.IR检测2上料位.Set(false);
                    RegBit.IR检测2待机位.Set(false);
                    return "触发拍照";
                case "触发拍照":
                    if (CCDSit == 1 || CCDSit == 5 || CCDSit == 9 || CCDSit == 13)
                    {
                        string[] strTemp = new string[] { };
                        CCDTime = $"{DateTime.Now.ToString("yyyyMMdd_HH_mm_ss.fff")}";
                        this[(NEST)((CCDSit - 1) / 4)].SN = CCDTime;
                        if (Parameter.GetBit("VisionEnable"))
                            camera.SendCmd(out strTemp, (int)TcpCommu_CCD3.Command.CCD3_IR面检测_拍照, false, CCDTime, $"{(CCDSit + 3) / 4}" );
                    }
                    else
                    {
                        Delay(50);
                    }
                    DO.CCD3触发.Set(true);
                    Thread.Sleep(20);
                    DO.CCD3触发.Set(false);
                    CCDSit++;
                    if (CCDSit < 17)
                        return "到拍照位";
                    CCDSit = 0;
                    return "等待IR检测1不在下料位";

                case "等待IR检测1不在下料位":
                    if (!RegBit.IR检测1下料位.Get())
                    {
                        RegBit.IR检测2下料位.Set(true);
                        return "到下料位";
                    }
                    return "等待IR检测1不在下料位";
                case "到下料位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.IR面检测Z2轴 }, 100, true);
                    WaitAxis(AXIS.IR面检测Z2轴);
                    AxisGoto(POINT.下料位, new AXIS[] { AXIS.IR面检测Z2轴, AXIS.IR面检测Y2轴, AXIS.IR面检测X2轴 }, 100, true);
                    //Thread.Sleep(150);
                    string[] strResult2 = new string[] { };
                    if (Parameter.GetBit("VisionEnable"))
                        camera.WaitReplyAndProcess(out strResult2, (int)TcpCommu_CCD3.Command.CCD3_IR面检测_获取结果);
                    if (IsDryRun)
                        this[NEST.穴位4].Result = true;
                    else
                    {
                        this[NEST.穴位4].CCD3Sit = $"IR_2:4:{strResult2[3]}";
                        this[NEST.穴位4].Result = strResult2[3].Contains("OK");
                        if (!this[NEST.穴位4].Result)
                            this[NEST.穴位4].NGReason += $"CCD3检测NG{strResult2[4]}；";

                        if (strResult2[4].Contains("NULL"))
                        {
                            var list = strResult2.ToList();
                            list.Insert(0, DateTime.Now.ToString("yyyyMMddyyyyMMdd_HH_mm_ss.fff"));   // 在索引0插入
                            var arr = list.ToArray();
                            SaveCsv("D:\\exe\\Log\\ZH\\Others\\CCD3CCD6NULL", DateTime.Now.ToString("yyyyMMdd"), "Time,CMD,SN,Slot,Result,NGreason", arr);
                        }
                    }
                    WaitAxis(AXIS.IR面检测Z2轴);
                    WaitAxis(AXIS.IR面检测Y2轴);
                    WaitAxis(AXIS.IR面检测X2轴);
                    RegBit.IR检测2拍照位.Set(false);
                    RegBit.IR检测通知翻转有料.Set(true);
                    return "等待翻转通知IR检测上料";

                case "等待翻转通知IR检测上料":
                    if (RegBit.翻转通知IR检测上料.Get())
                    {
                        RegBit.翻转通知IR检测上料.Set(false);
                        return "去下料顶升位";
                    }
                    return "等待翻转通知IR检测上料";
                case "去下料顶升位":
                    AxisGoto(POINT.下料顶升位, new AXIS[] { AXIS.IR面检测Z2轴 }, 100, true);
                    DO.IR面2吸真空.Set(false);
                    DO.IR面吸破真空5.Set(false);
                    DO.IR面吸破真空6.Set(false);
                    DO.IR面吸破真空7.Set(false);
                    DO.IR面吸破真空8.Set(false);
                    WaitAxis(AXIS.IR面检测Z2轴);
                    return "等待翻转通知IR检测上料完成";

                case "等待翻转通知IR检测上料完成":
                    if (RegBit.翻转通知IR检测上料完成.Get())
                    {
                        RegBit.翻转通知IR检测上料完成.Set(false);
                        return "检测结果赋值给翻转吸嘴";
                    }
                    return "等待翻转通知IR检测上料完成";
                case "检测结果赋值给翻转吸嘴":
                    Logger.Process(this.Name, $"{Name},穴位1，结果：{this[NEST.穴位1].Result}，NG原因：{this[NEST.穴位1].NGReason}，BreakDown:{this[NEST.穴位1].BreakDownTime}");
                    Logger.Process(this.Name, $"{Name},穴位2，结果：{this[NEST.穴位2].Result}，NG原因：{this[NEST.穴位2].NGReason}，BreakDown:{this[NEST.穴位2].BreakDownTime}");
                    Logger.Process(this.Name, $"{Name},穴位3，结果：{this[NEST.穴位3].Result}，NG原因：{this[NEST.穴位3].NGReason}，BreakDown:{this[NEST.穴位3].BreakDownTime}");
                    Logger.Process(this.Name, $"{Name},穴位4，结果：{this[NEST.穴位4].Result}，NG原因：{this[NEST.穴位4].NGReason}，BreakDown:{this[NEST.穴位4].BreakDownTime}");

                    "翻转".GetStation<翻转>()[翻转.NEST.穴位1] = this[NEST.穴位1];
                    "翻转".GetStation<翻转>()[翻转.NEST.穴位2] = this[NEST.穴位2];
                    "翻转".GetStation<翻转>()[翻转.NEST.穴位3] = this[NEST.穴位3];
                    "翻转".GetStation<翻转>()[翻转.NEST.穴位4] = this[NEST.穴位4];
                    this[NEST.穴位1] = null;
                    this[NEST.穴位2] = null;
                    this[NEST.穴位3] = null;
                    this[NEST.穴位4] = null;

                    RegBit.IR检测2待机位.Set(true);
                    return "Z轴回待机位";

                case "Z轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.IR面检测Z2轴 }, 100, true);
                    WaitAxis(AXIS.IR面检测Z2轴);
                    return "Y轴回待机位";
                case "Y轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.IR面检测Y2轴 }, 100, true);
                    WaitAxis(AXIS.IR面检测Y2轴);
                    DO.IR面2吸真空.Set(false);
                    DO.IR面2破真空.Set(true);
                    DO.IR面吸破真空5.Set(true);
                    DO.IR面吸破真空6.Set(true);
                    DO.IR面吸破真空7.Set(true);
                    DO.IR面吸破真空8.Set(true);
                    return "X轴回待机位";
                case "X轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.IR面检测X2轴 }, 100, true);
                    WaitAxis(AXIS.IR面检测X2轴);

                    RegBit.IR检测2下料位.Set(false);
                    DO.IR面2吸真空.Set(false);
                    DO.IR面2破真空.Set(false);
                    DO.IR面吸破真空5.Set(false);
                    DO.IR面吸破真空6.Set(false);
                    DO.IR面吸破真空7.Set(false);
                    DO.IR面吸破真空8.Set(false);
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
        public static double GetPos(this IR检测2.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this IR检测2.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}

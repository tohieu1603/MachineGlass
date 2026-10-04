using AF.Card;
using AF.Common;
using AF.Data;
using AF.Station;
using Autoframe;
using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Autoframe
{
    public class AR检测1 : StationEx
    {
        public AR检测1(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<NEST, ProductData>();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[]
            {
                DI.AR面吸真空1到位,
                DI.AR面吸真空2到位,
                DI.AR面吸真空3到位,
                DI.AR面吸真空4到位
            };
            //配置站位界面显示输出
            arrayDO = new object[]
            {
                DO.CCD6触发,
                DO.AR面1吸真空,
                DO.AR面1破真空,
                DO.AR面吸破真空1,
                DO.AR面吸破真空2,
                DO.AR面吸破真空3,
                DO.AR面吸破真空4
            };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { };
            //配置站位轴
            arrayAxis = new object[]
            {
                AXIS.AR面检测X1轴,
                AXIS.AR面检测Y1轴,
                AXIS.AR面检测Z1轴
            };
        }

        //ProductData productData = new ProductData();
        TcpCommu_CCD6 camera = new TcpCommu_CCD6();

        public enum POINT
        {
            待机位,
            上料位,
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
            下料位

        }

        public enum NEST
        {
            穴位1, 穴位2, 穴位3, 穴位4
        }

        public override void OnStop()
        {
            //DO.AR面1吸真空.Set(false);
            //DO.AR面1破真空.Set(true);
        }

        public override void OnPause()
        {
        }
        public override void OnResume()
        {
        }

        public override void Preinit()
        {
            RegBit.AR检测1上料位.Set(false);
            RegBit.AR检测1下料位.Set(false);
            RegBit.AR检测1下料等待位.Set(false);
            RegBit.AR检测1初始化吸嘴有料.Set(false);
            RegBit.AR检测1初始化完成.Set(false);
            RegBit.AR检测1待机位.Set(false);
            RegBit.AR检测1拍照位.Set(false);
            RegBit.AR检测1拍照等待位.Set(false);
            RegBit.AR检测1缩回位.Set(false);
            RegBit.AR检测通知移载接料准备OK.Set(false);
            RegBit.AR检测通知机械手有料.Set(false);
            switch (Process)
            {
                case AutoProcess.生产:
                case AutoProcess.空跑:
                    if (Parameter.GetBit("AR检测1Enable"))
                    {
                        this.InitEntry = "初始化流程开始";
                        this.AutoEntry = "生产流程开始";
                    }
                    break;
                case AutoProcess.CCD4标定:
                case AutoProcess.CCD5标定:
                    InitEntry = "初始化流程开始";
                    AutoEntry = "相机标定开始";
                    break;
                case AutoProcess.CCD6静态:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "CCD6静态";
                    break;
                case AutoProcess.CCD6动态:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "CCD6动态";
                    break;
                case AutoProcess.AR1测试:
                    this.InitEntry = "初始化流程开始";
                    this.AutoEntry = "开始检测";
                    break;
                case AutoProcess.AR2测试:
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
                    CCDTimes = 0;
                    //if (Parameter.GetBit("AR检测2Enable"))
                    //    ShowMessage($"{this.Name}:确保AR检测2模组在安全位。".txtTrans(TransType.Trans));
                    return "连接相机";
                case "连接相机":
                    //if (!camera.Init(this, 6))
                    //    return "连接相机";
                    //Logger.Process(Name, "CCD6 AR相机连接成功".txtTrans(TransType.Trans));
                    if (!IsDryRun)
                    {
                        if (!camera.Init(this, 6))
                            return "连接相机";
                        else
                            Logger.Process(Name, "相机连接成功".txtTrans(TransType.Trans));
                        Delay(1000);
                        string[] strccd6 = new string[] { };
                        camera.SendCmd(out strccd6, (int)TcpCommu_CCD6.Command.CCD6_复位, true);
                        if (strccd6[1] == "OK")
                        {
                            return "轴上使能";
                        }

                        EventMgr.Error($"{Name}CCD6初始化失败".txtTrans(TransType.Mark));
                        return StepExit;
                    }

                    //string[] strTemp = new string[] { };
                    //CCDSit = 1;
                    //CCDTime = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}";
                    //if (Parameter.GetBit("VisionEnable"))
                    //    camera.SendCmd(out strTemp, (int)TcpCommu_CCD6.Command.CCD6_AR面检测_拍照, false, CCDTime, $"{(CCDSit + 1) / 2}");
                    //DO.CCD6触发.Set(true);
                    //Thread.Sleep(20);
                    //DO.CCD6触发.Set(false);
                    //Thread.Sleep(1000);
                    //DO.CCD6触发.Set(true);
                    //Thread.Sleep(20);
                    //DO.CCD6触发.Set(false);
                    //camera.WaitReplyAndProcess(out strTemp, (int)TcpCommu_CCD6.Command.CCD6_AR面检测_拍照);
                    return "轴上使能";
                case "轴上使能":
                    AXIS.AR面检测Z1轴.ServoOn(false);
                    Delay(1000);
                    AXIS.AR面检测X1轴.ServoOn(true);
                    AXIS.AR面检测Y1轴.ServoOn(true);
                    AXIS.AR面检测Z1轴.ServoOn(true);
                    Delay(1000);
                    return "等待移载和机械手初始化完成";
                case "等待移载和机械手初始化完成":
                    if (RegBit.移载初始化完成.Get() && RegBit.机械手初始化完成.Get())
                        return "吸嘴吸真空";
                    return "等待移载和机械手初始化完成";
                case "吸嘴吸真空":
                    DO.AR面1破真空.Set(false);
                    DO.AR面吸破真空1.Set(true);
                    DO.AR面吸破真空2.Set(true);
                    DO.AR面吸破真空3.Set(true);
                    DO.AR面吸破真空4.Set(true);
                    DO.AR面1吸真空.Set(true);
                    return "Y轴回原点";
                case "Y轴回原点":
                    AXIS.AR面检测Y1轴.Home();
                    WaitAxis(AXIS.AR面检测Y1轴);
                    return "X轴回原点";

                case "X轴回原点":
                    AXIS.AR面检测X1轴.Home();
                    WaitAxis(AXIS.AR面检测X1轴);
                    return "Y轴回待机位";
                case "Y轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.AR面检测Y1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测Y1轴);
                    return "X轴回待机位";

                case "X轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.AR面检测X1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测X1轴);
                    return "Z轴回原点";

                case "Z轴回原点":
                    AXIS.AR面检测Z1轴.Home();
                    WaitAxis(AXIS.AR面检测Z1轴);
                    return "Z轴回待机位";
                case "Z轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.AR面检测Z1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测Z1轴);
                    return "判断吸真空";
                case "判断吸真空":
                    if (!IsDryRun)
                    {
                        bool[] vacArr = new bool[]
                            {
                        DI.AR面吸真空1到位.Get(),
                        DI.AR面吸真空2到位.Get(),
                        DI.AR面吸真空3到位.Get(),
                        DI.AR面吸真空4到位.Get()
                             };
                        if (!vacArr.Any(b => b))
                        {
                            RegBit.AR检测1初始化吸嘴有料.Set(false);
                            DO.AR面1吸真空.Set(false);
                            DO.AR面1破真空.Set(true);
                            Delay(500);
                            DO.AR面1吸真空.Set(false);
                            DO.AR面1破真空.Set(false);
                            DO.AR面吸破真空1.Set(false);
                            DO.AR面吸破真空2.Set(false);
                            DO.AR面吸破真空3.Set(false);
                            DO.AR面吸破真空4.Set(false);
                            this[NEST.穴位1] = null;
                            this[NEST.穴位2] = null;
                            this[NEST.穴位3] = null;
                            this[NEST.穴位4] = null;

                        }
                        else if (vacArr.All(b => b))
                        {
                            int ret = ShowMessage($"{this.Name},确认是否有料".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans), new string[] { "有料".txtTrans(TransType.Trans), "无料".txtTrans(TransType.Trans) });
                            if (ret == 1)
                            {
                                this[NEST.穴位1] = null;
                                this[NEST.穴位2] = null;
                                this[NEST.穴位3] = null;
                                this[NEST.穴位4] = null;
                                RegBit.AR检测1初始化吸嘴有料.Set(false);
                            }
                            else
                                RegBit.AR检测1初始化吸嘴有料.Set(true);
                        }
                        else
                        {
                            ShowMessage($"{Name}吸嘴残留物料，请手动取走".txtTrans(TransType.Trans));
                            return "判断吸真空";
                        }
                    }
                    else
                    {
                        RegBit.AR检测1初始化吸嘴有料.Set(false);
                    }
                    return "初始化完成";
                case "初始化完成":

                    RegBit.AR检测1待机位.Set(true);
                    RegBit.AR检测1初始化完成.Set(true);
                    Logger.Process(Name, "AR检测1初始化完成".txtTrans(TransType.Trans));
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
                    return "判断初始化吸嘴结果";
                case "判断初始化吸嘴结果":
                    if (RegBit.AR检测1初始化吸嘴有料.Get())
                    {

                        // 吸嘴带料，等待AR2空闲才能下料给移载
                        if (!RegBit.AR检测2上料位.Get())
                            return "等待AR检测2不在拍照位";
                    }
                    else if (RegBit.AR检测2初始化吸嘴有料.Get())
                    {
                        return "判断初始化吸嘴结果";
                    }
                    return "打开吸真空";
                case "打开吸真空":
                    DO.AR面1破真空.Set(false);
                    DO.AR面1吸真空.Set(true);
                    DO.AR面吸破真空1.Set(true);
                    DO.AR面吸破真空2.Set(true);
                    DO.AR面吸破真空3.Set(true);
                    DO.AR面吸破真空4.Set(true);
                    return "等待AR检测2不在上料位";
                case "等待AR检测2不在上料位":
                    if (!RegBit.AR检测2上料位.Get())
                    {
                        UploadSit = 1;
                        RegBit.AR检测1上料位.Set(true);
                        return "去上料位";
                    }
                    return "等待AR检测2不在上料位";
                case "去上料位":
                    AxisGoto(POINT.上料位, new AXIS[] { AXIS.AR面检测Z1轴, AXIS.AR面检测Y1轴, AXIS.AR面检测X1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测Z1轴);
                    WaitAxis(AXIS.AR面检测Y1轴);
                    WaitAxis(AXIS.AR面检测X1轴);
                    RegBit.AR检测1待机位.Set(false);
                    // 通知移载准备接料
                    RegBit.AR检测通知移载接料准备OK.Set(true);
                    return "等待移载通知AR检测放料完成";
                case "等待移载通知AR检测放料完成":
                    if (RegBit.移载通知AR检测放料完成.Get())
                    {
                        RegBit.移载通知AR检测放料完成.Set(false);
                        this[NEST.穴位1] = "移载".GetStation<移载>()[移载.NEST.穴位1];
                        this[NEST.穴位2] = "移载".GetStation<移载>()[移载.NEST.穴位2];
                        this[NEST.穴位3] = "移载".GetStation<移载>()[移载.NEST.穴位3];
                        this[NEST.穴位4] = "移载".GetStation<移载>()[移载.NEST.穴位4];
                        "移载".GetStation<移载>()[移载.NEST.穴位1] = null;
                        "移载".GetStation<移载>()[移载.NEST.穴位2] = null;
                        "移载".GetStation<移载>()[移载.NEST.穴位3] = null;
                        "移载".GetStation<移载>()[移载.NEST.穴位4] = null;
                        return "检测真空";
                    }
                    TakeinTime = DateTime.Now;
                    return "等待移载通知AR检测放料完成";
                case "检测真空":
                    // 空跑跳过真空
                    if (IsDryRun)
                    {
                        Delay(1000);
                        return "等待AR检测2不在拍照位";
                    }
                    bool[] vacArr = new bool[]
                    {
                        !this[NEST.穴位1].CCD3Sit.Contains("NULL") && DI.AR面吸真空1到位.Get(),
                        !this[NEST.穴位2].CCD3Sit.Contains("NULL") && DI.AR面吸真空2到位.Get(),
                        !this[NEST.穴位3].CCD3Sit.Contains("NULL") && DI.AR面吸真空3到位.Get(),
                        !this[NEST.穴位4].CCD3Sit.Contains("NULL") && DI.AR面吸真空4到位.Get()
                    };


                    if (vacArr.All(a => a))
                    {
                        return "等待AR检测2不在拍照位";
                    }
                    else if ((DateTime.Now - TakeinTime).TotalMilliseconds > 3000)
                    {
                        this[NEST.穴位1].BreakDownTime += 3;
                        this[NEST.穴位2].BreakDownTime += 3;
                        this[NEST.穴位3].BreakDownTime += 3;
                        this[NEST.穴位4].BreakDownTime += 3;
                        if (RegBit.IR载具清料.Get())
                        {
                            return "等待AR检测2不在拍照位";
                        }
                        int ret = ShowMessage($"{Name}吸嘴真空异常，请检查".txtTrans(TransType.Trans), "提示", new string[] { "重新检测", "忽略" });
                        this[NEST.穴位1].BreakDownTime += Duration;
                        this[NEST.穴位2].BreakDownTime += Duration;
                        this[NEST.穴位3].BreakDownTime += Duration;
                        this[NEST.穴位4].BreakDownTime += Duration;
                        if (ret != 0)
                        {
                            return "等待AR检测2不在拍照位";
                        }
                        TakeinTime = DateTime.Now;
                    }
                    return "检测真空";
                case "等待AR检测2不在拍照位":
                    if (!RegBit.AR检测2拍照位.Get())
                    {
                        CCDSit = 1;
                        RegBit.AR检测1拍照位.Set(true);
                        //Delay(2000);  //--------------------------------------------------------------------
                        return "到拍照位";
                    }
                    return "等待AR检测2不在拍照位";
                case "到拍照位":
                    POINT photoPt;
                    switch (CCDSit)
                    {
                        case 1:
                            photoPt = POINT.拍照位11;
                            RegBit.AR检测1初始化吸嘴有料.Set(false);
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
                            throw new Exception($"无效拍照点位{CCDSit}");
                    }

                    AxisGoto(photoPt, new AXIS[] { AXIS.AR面检测Z1轴, AXIS.AR面检测Y1轴, AXIS.AR面检测X1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测Z1轴);
                    WaitAxis(AXIS.AR面检测Y1轴);
                    WaitAxis(AXIS.AR面检测X1轴);
                    RegBit.AR检测1上料位.Set(false);
                    RegBit.AR检测1待机位.Set(false);
                    // 偶数点位读取上一组视觉结果
                    if (new[] { 5, 9, 13 }.Contains(CCDSit))
                    {
                        //Delay(100);
                        string[] resBuf = new string[] { };
                        if (Parameter.GetBit("VisionEnable"))
                        {
                            camera.WaitReplyAndProcess(out resBuf, (int)TcpCommu_CCD6.Command.CCD6_AR面检测_获取结果);
                            //if (resBuf[1] != CCDTime || ((CCDSit - 1) / 4).ToString() != resBuf[2])
                            //{
                            //    int rret = ShowMessage(this.Name, $"CCD6发送SN：{CCDTime},穴位：{((CCDSit - 1) / 4).ToString()};CCD6回复SN：{resBuf[1]},穴位：{resBuf[2]},有误，请检查后，确认‘重新收取’还是‘忽略’".txtTrans(TransType.Trans),
                            //        new string[] { "重新收取".txtTrans(TransType.Trans), "忽略".txtTrans(TransType.Trans) });
                            //    this[(NEST)((CCDSit - 1) / 4 - 1)].BreakDownTime += Duration;
                            //    if (0 == rret)
                            //    {
                            //        WaitAxis(AXIS.AR面检测Y1轴);
                            //        WaitAxis(AXIS.AR面检测X1轴);
                            //        return "到拍照位";
                            //    }
                            //    else
                            //    {
                            //        this[(NEST)((CCDSit - 1) / 4 - 1)].Result = false;
                            //        this[(NEST)((CCDSit - 1) / 4 - 1)].NGReason += $"CCD6检测NG{camera.RecString}；";
                            //    }
                            //}
                        }

                        if (IsDryRun)
                        {
                            this[(NEST)((CCDSit - 1) / 4 - 1)].Result = true;
                            this[(NEST)((CCDSit - 1) / 4 - 1)].NGReason = "NULL";
                        }

                        else
                        {
                            this[(NEST)((CCDSit - 1) / 4) - 1].CCD6Sit = $"AR_1:{(CCDSit - 1) / 4}:{resBuf[3]}";
                            this[(NEST)((CCDSit - 1) / 4 - 1)].Result = resBuf[3] == "OK" ? this[(NEST)((CCDSit - 1) / 4 - 1)].Result : false;
                            if (!(resBuf[3] == "OK"))
                                this[(NEST)((CCDSit - 1) / 4 - 1)].NGReason += $"CCD6检测NG{resBuf[4]}";

                            if (resBuf[4].Contains("NULL"))
                            {
                                var list = resBuf.ToList();
                                list.Insert(0, DateTime.Now.ToString("yyyyMMddyyyyMMdd_HH_mm_ss.fff"));   // 在索引0插入
                                var arr = list.ToArray();
                                SaveCsv("D:\\exe\\Log\\ZH\\Others\\CCD3CCD6NULL", DateTime.Now.ToString("yyyyMMdd"), "Time,CMD,SN,Slot,Result,NGreason", arr);
                            }
                        }
                    }
                    return "触发拍照";
                case "触发拍照":
                    // 奇数点位触发CCD6拍照
                    if (new[] { 1, 5, 9, 13 }.Contains(CCDSit))
                    {
                        CCDTime = this[(NEST)((CCDSit - 1) / 4)].SN;
                        //CCDTime = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}";
                        string[] sendBuf = new string[] { };
                        if (Parameter.GetBit("VisionEnable"))
                            camera.SendCmd(out sendBuf, (int)TcpCommu_CCD6.Command.CCD6_AR面检测_拍照, false, CCDTime, $"{(CCDSit + 3) / 4}");
                    }
                    else
                    {
                        Delay(50);
                    }
                    DO.CCD6触发.Set(true);
                    Thread.Sleep(20);
                    DO.CCD6触发.Set(false);
                    CCDSit++;
                    if (CCDSit < 17)
                        return "到拍照位";
                    CCDSit = 0;
                    return "等待AR检测2不在下料位";
                case "等待AR检测2不在下料位":
                    if (!RegBit.AR检测2下料位.Get())
                    {
                        RegBit.AR检测1下料位.Set(true);
                        return "到下料位";
                    }
                    return "等待AR检测2不在下料位";
                case "到下料位":
                    //AxisGoto(POINT.待机位, new AXIS[] { AXIS.AR面检测Z1轴 }, 100, true);
                    //WaitAxis(AXIS.AR面检测Z1轴);
                    AxisGoto(POINT.下料位, new AXIS[] { AXIS.AR面检测Z1轴, AXIS.AR面检测Y1轴, AXIS.AR面检测X1轴 }, 100, true);
                    //Thread.Sleep(150);
                    // 读取第4穴位结果
                    string[] finalRes = new string[] { };
                    camera.WaitReplyAndProcess(out finalRes, (int)TcpCommu_Camera.Command.CCD6_AR面检测_获取结果);
                    if (IsDryRun)
                    {
                        this[NEST.穴位4].Result = true;
                        this[NEST.穴位4].NGReason = "NULL";
                    }
                    else
                    {
                        this[NEST.穴位4].CCD6Sit = $"AR_1:4:{finalRes[3]}";
                        this[NEST.穴位4].Result = finalRes[3].Contains("OK") ? this[(NEST)3].Result : false;
                        if (!finalRes[3].Contains("OK"))
                            this[NEST.穴位4].NGReason += $"CCD6检测NG{finalRes[4]}";

                        if (finalRes[4].Contains("NULL"))
                        {
                            var list = finalRes.ToList();
                            list.Insert(0, DateTime.Now.ToString("yyyyMMddyyyyMMdd_HH_mm_ss.fff"));   // 在索引0插入
                            var arr = list.ToArray();
                            SaveCsv("D:\\exe\\Log\\ZH\\Others\\CCD3CCD6NULL", DateTime.Now.ToString("yyyyMMdd"), "Time,CMD,SN,Slot,Result,NGreason", arr);
                        }
                    }
                    RegBit.AR通知机械手穴位1有料.Set(!this[NEST.穴位1].NGReason.Contains("NULL"));
                    RegBit.AR通知机械手穴位2有料.Set(!this[NEST.穴位2].NGReason.Contains("NULL"));
                    RegBit.AR通知机械手穴位3有料.Set(!this[NEST.穴位3].NGReason.Contains("NULL"));
                    RegBit.AR通知机械手穴位4有料.Set(!this[NEST.穴位4].NGReason.Contains("NULL"));
                    WaitAxis(AXIS.AR面检测Z1轴);
                    WaitAxis(AXIS.AR面检测Y1轴);
                    WaitAxis(AXIS.AR面检测X1轴);
                    RegBit.AR检测1拍照位.Set(false);
                    // 告知移载本站有料可取

                    RegBit.AR检测通知机械手有料.Set(true);
                    return "等待机械手通知AR关闭真空";
                case "等待机械手通知AR关闭真空":
                    if (RegBit.机械手通知AR关闭真空.Get())
                    {
                        RegBit.机械手通知AR关闭真空.Set(false);
                        DO.AR面1破真空.Set(false);
                        DO.AR面1吸真空.Set(false);
                        RegBit.AR检测通知机械手真空已关闭.Set(true);
                        return "等待机械手通知AR检测取料完成";
                    }
                    return "等待机械手通知AR关闭真空";
                case "等待机械手通知AR检测取料完成":
                    if (RegBit.机械手通知AR检测取料完成.Get())
                    {
                        RegBit.机械手通知AR检测取料完成.Set(false);
                        return "检测结果赋值给机器人吸嘴";
                    }
                    return "等待机械手通知AR检测取料完成";
                case "检测结果赋值给机器人吸嘴":
                    // 将4个穴位产品数据传递给机器人站
                    //StationEx moveStation = StationMgr.GetStation("移载") as StationEx;
                    //this[NEST.穴位1].DtEndTime = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss.fff"));
                    //this[NEST.穴位2].DtEndTime = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss.fff"));
                    //this[NEST.穴位3].DtEndTime = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss.fff"));
                    //this[NEST.穴位4].DtEndTime = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss.fff"));
                    Logger.Process(this.Name, $"{Name},穴位1，结果：{this[NEST.穴位1].Result}，NG原因：{this[NEST.穴位1].NGReason}，BreakDown:{this[NEST.穴位1].BreakDownTime}");
                    Logger.Process(this.Name, $"{Name},穴位2，结果：{this[NEST.穴位2].Result}，NG原因：{this[NEST.穴位2].NGReason}，BreakDown:{this[NEST.穴位2].BreakDownTime}");
                    Logger.Process(this.Name, $"{Name},穴位3，结果：{this[NEST.穴位3].Result}，NG原因：{this[NEST.穴位3].NGReason}，BreakDown:{this[NEST.穴位3].BreakDownTime}");
                    Logger.Process(this.Name, $"{Name},穴位4，结果：{this[NEST.穴位4].Result}，NG原因：{this[NEST.穴位4].NGReason}，BreakDown:{this[NEST.穴位4].BreakDownTime}");
                    "机械手站".GetStation<StationRobot>()[StationRobot.NEST.吸嘴1] = this[NEST.穴位1];
                    "机械手站".GetStation<StationRobot>()[StationRobot.NEST.吸嘴2] = this[NEST.穴位2];
                    "机械手站".GetStation<StationRobot>()[StationRobot.NEST.吸嘴3] = this[NEST.穴位3];
                    "机械手站".GetStation<StationRobot>()[StationRobot.NEST.吸嘴4] = this[NEST.穴位4];
                    this[NEST.穴位1] = null;
                    this[NEST.穴位2] = null;
                    this[NEST.穴位3] = null;
                    this[NEST.穴位4] = null;


                    RegBit.AR检测1待机位.Set(true);
                    //Thread.Sleep(100);
                    return "Z轴回待机位";
                case "Z轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.AR面检测Z1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测Z1轴);
                    return "Y轴回待机位";
                case "Y轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.AR面检测Y1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测Y1轴);
                    DO.AR面1破真空.Set(true);
                    DO.AR面1吸真空.Set(false);
                    DO.AR面吸破真空1.Set(true);
                    DO.AR面吸破真空2.Set(true);
                    DO.AR面吸破真空3.Set(true);
                    DO.AR面吸破真空4.Set(true);

                    return "X轴回待机位";
                case "X轴回待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.AR面检测X1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测X1轴);
                    RegBit.AR检测1下料位.Set(false);
                    DO.AR面1破真空.Set(false);
                    DO.AR面1吸真空.Set(false);
                    DO.AR面吸破真空1.Set(false);
                    DO.AR面吸破真空2.Set(false);
                    DO.AR面吸破真空3.Set(false);
                    DO.AR面吸破真空4.Set(false);

                    return "生产流程结束";

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
                case "相机标定开始":
                    ShowMessage($"{Name}手动上料".txtTrans(TransType.Trans));
                    return "打开吸真空";
                case "打开吸真空":
                    DO.AR面1破真空.Set(false);
                    DO.AR面吸破真空1.Set(true);
                    DO.AR面吸破真空2.Set(true);
                    DO.AR面吸破真空3.Set(true);
                    DO.AR面吸破真空4.Set(true);
                    DO.AR面1吸真空.Set(true);
                    return "检测真空";
                case "检测真空":
                    bool[] vacArr = new bool[]
                    {
                        DI.AR面吸真空1到位.Get(),
                        DI.AR面吸真空2到位.Get(),
                        DI.AR面吸真空3到位.Get(),
                        DI.AR面吸真空4到位.Get()
                    };
                    if (vacArr.All(a => a))
                    {
                        //return "等待AR检测2不在拍照位";
                    }
                    else if ((DateTime.Now - TakeinTime).TotalMilliseconds > 3000)
                    {
                        ShowMessage($"{Name}吸嘴真空异常，请检查".txtTrans(TransType.Trans));
                        TakeinTime = DateTime.Now;
                        return "检测真空";
                    }
                    return "去上料位";
                case "去上料位":
                    AxisGoto(POINT.上料位, new AXIS[] { AXIS.AR面检测Z1轴, AXIS.AR面检测Y1轴, AXIS.AR面检测X1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测Z1轴);
                    WaitAxis(AXIS.AR面检测Y1轴);
                    WaitAxis(AXIS.AR面检测X1轴);
                    DO.AR面1破真空.Set(false);
                    DO.AR面1吸真空.Set(false);
                    // 通知移载准备接料
                    RegBit.AR检测通知移载接料准备OK.Set(true);
                    return StepExit;
                default:
                    return null;
            }
        }
        // 直接追加到你已有的AR检测1类内部，放在CalibRun下方、OtherRun上方即可
        int CCDTimes = 0;
        protected override string GrrRun(string strStep)
        {
            switch (strStep)
            {
                #region CCD6静态标定
                case "CCD6静态":
                    DO.AR面1破真空.Set(false);
                    DO.AR面吸破真空1.Set(true);
                    DO.AR面吸破真空2.Set(true);
                    DO.AR面吸破真空3.Set(true);
                    DO.AR面吸破真空4.Set(true);
                    DO.AR面1吸真空.Set(true);
                    ShowMessage($"{Name}放物料".txtTrans(TransType.Trans));
                    AxisGoto(POINT.拍照位11, new AXIS[] { AXIS.AR面检测X1轴, AXIS.AR面检测Y1轴, AXIS.AR面检测Z1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测X1轴);
                    WaitAxis(AXIS.AR面检测Y1轴);
                    WaitAxis(AXIS.AR面检测Z1轴);
                    Delay(100);
                    return "CCD6发送定位拍照指令";
                case "CCD6发送定位拍照指令":
                    // 获取当前拍照位XY坐标，U固定0
                    double xNow = POINT.拍照位11.GetPos(AXIS.AR面检测X1轴, this);
                    double yNow = POINT.拍照位11.GetPos(AXIS.AR面检测Y1轴, this);
                    double uNow = 0;
                    string[] rcvBuf = new string[] { };
                    // 发送CCD6静态标定拍照指令
                    camera.SendCmd(out rcvBuf, (int)TcpCommu_CCD6.Command.CCD6_静态, false, xNow.ToString(), yNow.ToString(), uNow.ToString());
                    Thread.Sleep(50);
                    DO.CCD6触发.Set(true);
                    Thread.Sleep(20);
                    DO.CCD6触发.Set(false);
                    Thread.Sleep(200);
                    return "获取CCD6视觉检测结果";

                case "获取CCD6视觉检测结果":
                    //string[] resBuf = new string[] { };
                    //camera.WaitReplyAndProcess(out resBuf, (int)TcpCommu_Camera.Command.CCD6_静态);
                    //if (resBuf[2] == "NG")
                    //{
                    //    Logger.Process(Name, "CCD6静态GRR标定失败".txtTrans(TransType.Trans));
                    //    return null;
                    //}
                    CCDTimes++;
                    if (CCDTimes < Parameter.GetInt("GRRNums"))
                    {
                        Delay(200);
                        return "CCD6发送定位拍照指令";
                    }
                    CCDTimes = 0;
                    return "CCD6静态去待机位";
                case "CCD6静态去待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.AR面检测X1轴, AXIS.AR面检测Y1轴, AXIS.AR面检测Z1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测X1轴);
                    WaitAxis(AXIS.AR面检测Y1轴);
                    WaitAxis(AXIS.AR面检测Z1轴);
                    return StepEmpty;
                #endregion

                #region CCD6动态标定
                case "CCD6动态":
                    DO.AR面1破真空.Set(false);
                    DO.AR面吸破真空1.Set(true);
                    DO.AR面吸破真空2.Set(true);
                    DO.AR面吸破真空3.Set(true);
                    DO.AR面吸破真空4.Set(true);
                    DO.AR面1吸真空.Set(true);
                    ShowMessage($"{Name}放物料".txtTrans(TransType.Trans));
                    return "去拍照位";
                case "去拍照位":
                    AxisGoto(POINT.拍照位11, new AXIS[] { AXIS.AR面检测X1轴, AXIS.AR面检测Y1轴, AXIS.AR面检测Z1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测X1轴);
                    WaitAxis(AXIS.AR面检测Y1轴);
                    WaitAxis(AXIS.AR面检测Z1轴);
                    Delay(100);
                    return "CCD6动态发送定位拍照指令";
                case "CCD6动态发送定位拍照指令":
                    double xNowD = POINT.拍照位11.GetPos(AXIS.AR面检测X1轴, this);
                    double yNowD = POINT.拍照位11.GetPos(AXIS.AR面检测Y1轴, this);
                    double uNowD = 0;
                    string[] rcvBufD = new string[] { };
                    camera.SendCmd(out rcvBufD, (int)TcpCommu_CCD6.Command.CCD6_动态, false, xNowD.ToString(), yNowD.ToString(), uNowD.ToString());
                    Thread.Sleep(50);
                    DO.CCD6触发.Set(true);
                    Thread.Sleep(20);
                    DO.CCD6触发.Set(false);
                    Thread.Sleep(200);
                    return "获取CCD6动态视觉检测结果";

                case "获取CCD6动态视觉检测结果":
                    string[] resBufD = new string[] { };
                    //camera.WaitReplyAndProcess(out resBufD, (int)TcpCommu_Camera.Command.CCD6_动态);
                    //if (resBufD[2] == "NG")
                    //{
                    //    Logger.Process(Name, "CCD6动态GRR标定失败".txtTrans(TransType.Trans));
                    //    return null;
                    //}
                    return "CCD6动态去待机位";
                case "CCD6动态去待机位":
                    AxisGoto(POINT.待机位, new AXIS[] { AXIS.AR面检测X1轴, AXIS.AR面检测Y1轴, AXIS.AR面检测Z1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测X1轴);
                    WaitAxis(AXIS.AR面检测Y1轴);
                    WaitAxis(AXIS.AR面检测Z1轴);
                    Delay(100);
                    CCDTimes++;
                    if (CCDTimes < Parameter.GetInt("GRRNums"))
                    {
                        Delay(200);
                        return "去拍照位";
                    }
                    CCDTimes = 0;
                    return StepEmpty;
                #endregion
                default:
                    return null;
            }
        }
        int TestNum = 0;
        protected override string OtherRun(string strStep)
        {
            switch (strStep)
            {
                case "开始检测":
                    TestNum++;
                    if (TestNum > 100)
                    {
                        TestNum = 0;
                        return StepEmpty;
                    }
                    return "等待AR检测2不在拍照位";
                case "等待AR检测2不在拍照位":
                    if (!RegBit.AR检测2拍照位.Get())
                    {
                        CCDSit = 1;
                        RegBit.AR检测1拍照位.Set(true);
                        return "到拍照位";
                    }
                    return "等待AR检测2不在拍照位";
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
                            photoPt = POINT.拍照位21;
                            break;
                        case 4:
                            photoPt = POINT.拍照位22;
                            break;
                        case 5:
                            photoPt = POINT.拍照位31;
                            break;
                        case 6:
                            photoPt = POINT.拍照位32;
                            break;
                        case 7:
                            photoPt = POINT.拍照位41;
                            break;
                        case 8:
                            photoPt = POINT.拍照位42;
                            break;
                        default:
                            throw new Exception($"无效拍照点位{CCDSit}");
                    }

                    AxisGoto(photoPt, new AXIS[] { AXIS.AR面检测Z1轴, AXIS.AR面检测Y1轴, AXIS.AR面检测X1轴 }, 100, true);
                    WaitAxis(AXIS.AR面检测Z1轴);
                    WaitAxis(AXIS.AR面检测Y1轴);
                    WaitAxis(AXIS.AR面检测X1轴);
                    RegBit.AR检测1上料位.Set(false);
                    RegBit.AR检测1待机位.Set(false);
                    //偶数点位读取上一组视觉结果
                    if (new[] { 3, 5, 7 }.Contains(CCDSit))
                    {
                        Thread.Sleep(150);
                        string[] resBuf = new string[] { };
                        if (Parameter.GetBit("VisionEnable"))
                            camera.WaitReplyAndProcess(out resBuf, (int)TcpCommu_CCD6.Command.CCD6_AR面检测_拍照);
                        if (IsDryRun)
                            this[(NEST)((CCDSit - 1) / 2 - 1)].Result = true;
                        else
                        {
                            this[(NEST)((CCDSit - 1) / 2 - 1)].Result = resBuf[1].Contains("OK");
                            if (this[(NEST)((CCDSit - 1) / 2 - 1)].Result)
                                this[(NEST)((CCDSit - 1) / 2 - 1)].NGReason += "CCD6检测NG";
                        }
                    }
                    return "触发拍照";
                case "触发拍照":
                    // 奇数点位触发CCD6拍照
                    if (new[] { 1, 3, 5, 7 }.Contains(CCDSit))
                    {
                        CCDTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        string[] sendBuf = new string[] { };
                        if (Parameter.GetBit("VisionEnable"))
                            camera.SendCmd(out sendBuf, (int)TcpCommu_CCD6.Command.CCD6_AR面检测_拍照, false, CCDTime, $"{(CCDSit + 1) / 2}");
                    }
                    DO.CCD6触发.Set(true);
                    Thread.Sleep(20);
                    DO.CCD6触发.Set(false);
                    CCDSit++;
                    if (CCDSit < 9)
                        return "到拍照位";
                    CCDSit = 0;
                    return "开始检测";
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
        public static double GetPos(this AR检测1.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
        public static double GetPos(this AR检测1.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            return station?.dictPoint[(int)pt][$"a{(int)axis}"] ?? 0d;
        }
    }
}
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
using System.Xml.Linq;
using static AF.Data.Register;
using System.Collections;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Windows.Interop;
using ToolEx;

namespace Autoframe
{
    public class DataUpdata : StationEx
    {
        /// <summary>
        /// 时间开始
        /// </summary>
        private DateTime CT_TimeStart = DateTime.Now;
        /// <summary>
        /// 时间结束
        /// </summary>
        private DateTime CT_TimeEnd = DateTime.Now;
        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public DataUpdata(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<NEST, ProductData>();
            //LoadContainer<NEST, ProductInfo>();
        }

        void HardwareConfig()
        {

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
                    //this[NEST.穴位1] = new ProductData();
                    //this[NEST.穴位1].NGReason = "清料";
                    //for (int i = 0; i < 4; i++)
                    //{
                    //    if (this[(NEST)i].NGReason.Contains("清料"))
                    //    {
                    //        this[NEST.穴位1] = null;
                    //        this[NEST.穴位2] = null;
                    //        this[NEST.穴位3] = null;
                    //        this[NEST.穴位4] = null;
                    //        RegBit.IR载具清料.Set(false);
                    //        StationMgr.Pause();
                    //        Delay(200);
                    //        StationMgr.Stop();
                    //        break;
                    //    }
                    //}
                    if (this[NEST.穴位1] != null && this[NEST.穴位2] != null && this[NEST.穴位3] != null && this[NEST.穴位4] != null)
                    {
                        RegBit.DataUpdata初始化有料.Set(true);
                    }
                    else
                    {
                        RegBit.DataUpdata初始化有料.Set(false);
                        this[NEST.穴位1] = null;
                        this[NEST.穴位2] = null;
                        this[NEST.穴位3] = null;
                        this[NEST.穴位4] = null;
                    }
                    return "初始化完成";
                case "初始化完成":
                    CT_TimeStart = DateTime.Now;
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
                    if (RegBit.DataUpdata初始化有料.Get())
                    {
                        RegBit.DataUpdata初始化有料.Set(false);
                        return "数据更新";
                    }
                    RegBit.DataUpdata向机械手要料.Set(true);
                    return "等待机械手有料";
                case "等待机械手有料":
                    if (RegBit.机械手通知DataUpdata有料.Get())
                    {
                        RegBit.机械手通知DataUpdata有料.Set(false);
                        return "数据更新";
                    }
                    return "等待机械手有料";

                case "数据更新":
                    Delay(200);
                    CT_TimeEnd = DateTime.Now;
                    this[NEST.穴位1].CT = ((CT_TimeEnd - CT_TimeStart).TotalSeconds / 4).ToString();
                    this[NEST.穴位2].CT = ((CT_TimeEnd - CT_TimeStart).TotalSeconds / 4).ToString();
                    this[NEST.穴位3].CT = ((CT_TimeEnd - CT_TimeStart).TotalSeconds / 4).ToString();
                    this[NEST.穴位4].CT = ((CT_TimeEnd - CT_TimeStart).TotalSeconds / 4).ToString();
                    CT_TimeStart = DateTime.Now;
                    RegDouble.CT.Set((CT_TimeEnd - CT_TimeStart).TotalSeconds / 4);
                    bool only1 = true;
                    for (int i = 0; i < 4; i++)
                    {
                        if (this[(NEST)i].UpRingTime != RegString.UpRingTime.Get())
                        {
                            if (this[(NEST)i].UpRingTime == RegString.UpRingTime.Get() && only1 && RegBit.扩膜CCD1拍照完成状态.Get())
                            {
                                RingMonitor.SaveCurrentRingImage();
                                RingMonitor.ResetAll();
                                RingMonitor.SetRingLayout(RegInt.RowCCD1.Get(), RegInt.ColCCD1.Get(), RegInt.PickRingSite.Get(), IsDryRun);
                                only1 = false;
                                RegBit.扩膜CCD1拍照完成状态.Set(false);
                            }
                            RingMonitor.OnResult(this[(NEST)i].PianPos, this[(NEST)i].Result, this[(NEST)i].NGReason);
                        }
                        else
                        {
                            if (this[(NEST)i].UpRingTime == RegString.UpRingTime.Get() && RegBit.扩膜CCD1拍照完成状态.Get())
                            {
                                RingMonitor.SaveCurrentRingImage();
                                RingMonitor.ResetAll();
                                RingMonitor.SetRingLayout(RegInt.RowCCD1.Get(), RegInt.ColCCD1.Get(), RegInt.PickRingSite.Get(), IsDryRun);
                                RegBit.扩膜CCD1拍照完成状态.Set(false);
                            }
                        }

                        if (this[(NEST)i].Result)
                        {
                            this[(NEST)i].ShowData();
                            Statistics.UpdateMaterial(Material.Chassis);
                        }
                        else if (!this[(NEST)i].NGReason.Contains("NULL"))
                        {
                            this[(NEST)i].ShowData();
                            Statistics.UpdateMaterial(Material.Chassis, ThrowReason.Chassis_AOI检测结果NG);
                        }
                    }


                    Logger.Process(this.Name, $"{Name},穴位1，结果：{this[NEST.穴位1].Result}，NG原因：{this[NEST.穴位1].NGReason}，BreakDown:{this[NEST.穴位1].BreakDownTime}");
                    Logger.Process(this.Name, $"{Name},穴位2，结果：{this[NEST.穴位2].Result}，NG原因：{this[NEST.穴位2].NGReason}，BreakDown:{this[NEST.穴位2].BreakDownTime}");
                    Logger.Process(this.Name, $"{Name},穴位3，结果：{this[NEST.穴位3].Result}，NG原因：{this[NEST.穴位3].NGReason}，BreakDown:{this[NEST.穴位3].BreakDownTime}");
                    Logger.Process(this.Name, $"{Name},穴位4，结果：{this[NEST.穴位4].Result}，NG原因：{this[NEST.穴位4].NGReason}，BreakDown:{this[NEST.穴位4].BreakDownTime}");
                    for (int i = 0; i < 4; i++)
                    {
                        if (this[(NEST)i].NGReason.Contains("清料"))
                        {
                            this[NEST.穴位1] = null;
                            this[NEST.穴位2] = null;
                            this[NEST.穴位3] = null;
                            this[NEST.穴位4] = null;
                            RegBit.IR载具清料.Set(false);
                            StationMgr.Pause();
                            Delay(1000);
                            StationMgr.Stop();
                            break;
                        }
                    }

                    this[NEST.穴位1] = null;
                    this[NEST.穴位2] = null;
                    this[NEST.穴位3] = null;
                    this[NEST.穴位4] = null;

                    return "生产流程开始";
            }
            return null;
        }





        protected override string CalibRun(string strStep)
        {
            switch (strStep)
            {
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
        public static double GetPos(this DataUpdata.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this DataUpdata.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}

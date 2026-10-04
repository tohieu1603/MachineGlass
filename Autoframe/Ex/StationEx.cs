using AF.Card;
using AF.Common;
using AF.Communicate;
using AF.Data;
using AF.File;
using AF.Station;
using Inovance.InoMotionCotrolCard2;
using Inovance.InoMotionCotrollerShop.InoServiceContract.EtherCATConfigApi;
using MySqlX.XDevAPI;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Autoframe
{
    /// <summary>
    /// 站位
    /// </summary>
    public class StationEx : StationBase
    {
        private const int nScanTime = 10;//ms
        private static AutoProcess autoProcess = AutoProcess.生产;
        /// <summary>
        /// 自动流程改变
        /// </summary>
        public static event Action StationProcessChanged = null;
        /// <summary>
        /// ShowMsg所需要的时长
        /// </summary>
        public double Duration { get; set; } = 0;
        /// <summary>
        /// 是否空跑流程
        /// </summary>
        public bool IsDryRun { get { return autoProcess == AutoProcess.空跑; } }

        public bool IsNormalRun { get { return autoProcess == AutoProcess.生产; } }

        //public int SpeedPercent { get { return Parameter.GetInt("SpeedPercent"); } }

        /// <summary>
        /// 自动流程
        /// </summary>
        public  readonly object m_FileLock = new object();
        public static AutoProcess Process
        {
            get { return autoProcess; }
            set
            {
                autoProcess = value;
                if (StationProcessChanged != null)
                    StationProcessChanged();
            }
        }

        /// <summary>
        /// 加载当站Container信息
        /// </summary>
        /// <typeparam name="EContainer">容器枚举</typeparam>
        /// <typeparam name="TContainer">容器类型</typeparam>
        protected void LoadContainer<EContainer, TContainer>() where EContainer : Enum where TContainer:class
        {
            int nLength = Enum.GetValues(typeof(EContainer)).Length;
            if (nLength < 1)
                return;

            this.stationSpace = new StationSpace(this.Name);
            foreach (var item in Enum.GetNames(typeof(EContainer)))
                this.stationSpace.Add(item);

            this.stationSpace.Load<TContainer>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strName"></param>
        public StationEx(string strName) : base(strName)
        {

        }

        /// <summary>
        /// 响应流程步改变
        /// </summary>
        /// <param name="strStep"></param>
        protected override void OnStepChanged(string strStep)
        {
            base.OnStepChanged($"{Process}>{strStep}");
        }

        public const int ProcessCalib = 100;
        public const int ProcessGRR = 200;
        public const int ProcessOthers = 300;

        /// <summary>
        /// 流程步运行
        /// </summary>
        /// <param name="strStep">步名</param>
        /// <param name="bInit">是否是初始化步</param>
        /// <returns></returns>
        protected override string StepRun(string strStep, bool bInit)
        {
            if (bInit)
                return InitRun(strStep);
            
            int nProcess = (int)Process;
            if (nProcess < ProcessCalib)
                return ProduceRun(strStep);              
            else if (nProcess >= ProcessCalib && nProcess < ProcessGRR)
                return CalibRun(strStep);
            else if (nProcess >= ProcessGRR && nProcess < ProcessOthers)
                return GrrRun(strStep);
            else
                return OtherRun(strStep);
        }

        /// <summary>
        /// 初始化流程运行
        /// </summary>
        /// <param name="strStep"></param>
        /// <returns></returns>
        protected virtual string InitRun(string strStep)
        {
            return null;
        }
        /// <summary>
        /// 生产流程运行
        /// </summary>
        /// <param name="strStep"></param>
        /// <returns></returns>
        protected virtual string ProduceRun(string strStep)
        {
            return null;
        }
        /// <summary>
        /// 标定流程运行
        /// </summary>
        /// <param name="strStep"></param>
        /// <returns></returns>
        protected virtual string CalibRun(string strStep)
        {
            return null;
        }
        /// <summary>
        /// GRR流程运行
        /// </summary>
        /// <param name="strStep"></param>
        /// <returns></returns>
        protected virtual string GrrRun(string strStep)
        {
            return null;
        }
        /// <summary>
        /// 其他流程运行
        /// </summary>
        /// <param name="strStep"></param>
        /// <returns></returns>
        protected virtual string OtherRun(string strStep)
        {
            return null;
        }

        #region 寄存器
        /// <summary>
        /// 等待Bit寄存器有效
        /// </summary>
        /// <param name="regBit">Bit寄存器枚举</param>
        /// <param name="bValue">有效值</param>
        /// <param name="nTimeOutS">超时时间(以秒为单位)，-1:一直等待， 0：使用系统参数设置时间， 其它：特定时间</param>
        /// <param name="bPause">是否响应暂停状态</param>
        public void WaitRegBit(RegBit regBit, bool bValue, int nTimeOutS = -1, bool bPause = true)
        {
            string str1 = $"等待 寄存器[{regBit}] = {bValue}";
            Logger.Process(this.Name, str1, 1);

            int nTimeCount = 0;
            if (nTimeOutS == 0)
                nTimeOutS = ProgramTool.g_WaitRegTime;

            while (true)
            {
                CheckContinue(bPause);
                bool bBit = Register.GetBit((int)regBit);
                if (bBit == bValue)//到位
                    return;

                Thread.Sleep(nScanTime);
                nTimeCount += nScanTime;
                if (nTimeOutS != -1 && nTimeCount > nTimeOutS * 1000)
                {
                    EventMgr.Notice(Register.ErrReg.超时, regBit.ToString(), $"{"等待".txtTrans(TransType.Mark)} {"寄存器".txtTrans(TransType.Mark)}[{regBit.ToString().txtTrans(TransType.Mark)}] = {bValue} {"超时".txtTrans(TransType.Mark)}", this.Name);
                    nTimeCount = 0;
                }
            }
        }

        /// <summary>
        /// 等待任意Bit寄存器值有效
        /// </summary>
        /// <param name="regBit">Bit寄存器枚举数组</param>
        /// <param name="bValue">有效值</param>
        /// <param name="nTimeOutS">超时时间(以秒为单位)，-1:一直等待， 0：使用系统参数设置时间， 其它：特定时间</param>
        /// <param name="bPause">是否响应暂停</param>
        /// <returns>有效的寄存器</returns>
        public RegBit WaitRegBit(RegBit[] regBit, bool bValue, int nTimeOutS = -1, bool bPause = true)
        {
            string str1 = "";
            for (int i = 0; i < regBit.Length; i++)
            {
                str1 += regBit[i];
                if (i < regBit.Length - 1)
                    str1 += ",";
            }
            Logger.Process(this.Name, $"等待任意 寄存器[{str1}] = {bValue}", 1);

            int nTimeCount = 0;
            if (nTimeOutS == 0)
                nTimeOutS = ProgramTool.g_WaitRegTime;

            while (true)
            {
                CheckContinue(bPause);
                //只要任意一个系统位寄存器满足条件就跳出
                foreach (var index in regBit)
                {
                    if (Register.GetBit((int)index) == bValue)
                    {
                        Logger.Process(this.Name, $"等到寄存器[{index}]={bValue}", 1);
                        return index;
                    }
                }

                Thread.Sleep(nScanTime);
                nTimeCount += nScanTime;
                if (nTimeOutS != -1 && nTimeCount > nTimeOutS * 1000)
                {
                    EventMgr.Notice(Register.ErrReg.超时, regBit[0].ToString(), $"{"等待任意".txtTrans(TransType.Mark)} {"寄存器".txtTrans(TransType.Mark)}[{str1.txtTrans(TransType.Mark)}] = {bValue} {"超时".txtTrans(TransType.Mark)}", this.Name);
                    nTimeCount = 0;
                }
            }
        }

        /// <summary>
        /// 等待所有Bit寄存器值有效
        /// </summary>
        /// <param name="regBit">Bit寄存器枚举数组</param>
        /// <param name="bValue">有效值</param>
        /// <param name="nTimeOutS">超时时间(以秒为单位)，-1:一直等待， 0：使用系统参数设置时间， 其它：特定时间</param>
        /// <param name="bPause">是否响应暂停</param>
        public void WaitRegBitAll(RegBit[] regBit, bool bValue, int nTimeOutS = 0, bool bPause = true)
        {
            string str1 = "";
            for (int i = 0; i < regBit.Length; i++)
            {
                str1 += regBit[i];
                if (i < regBit.Length - 1)
                    str1 += ",";
            }
            Logger.Process(this.Name, $"等待所有 寄存器[{str1}] = {bValue}", 1);

            int nTimeCount = 0;
            if (nTimeOutS == 0)
                nTimeOutS = ProgramTool.g_WaitRegTime;

            while (true)
            {
                CheckContinue(bPause);

                bool b = true;
                foreach (var item in regBit)
                    b = b && AF.Data.Register.GetBit((int)item) == bValue;
                if (b)
                    return;

                Thread.Sleep(nScanTime);
                nTimeCount += nScanTime;
                if (nTimeOutS != -1 && nTimeCount > nTimeOutS * 1000)
                {
                    EventMgr.Notice(Register.ErrReg.超时, regBit[0].ToString(), $"{"等待所有".txtTrans(TransType.Mark)} {"寄存器".txtTrans(TransType.Mark)}[{str1.txtTrans(TransType.Mark)}] = {bValue} {"超时".txtTrans(TransType.Mark)}", this.Name);
                    nTimeCount = 0;
                }
            }
        }
        #endregion

        #region DI
        /// <summary>
        /// 等待IO输入点有效
        /// </summary>
        /// <param name="enumDI">DI枚举</param>
        /// <param name="bValue">有效值</param>
        /// <param name="nTimeOutS">超时时间(以秒为单位)，-1:一直等待， 0：使用系统参数设置时间， 其它：特定时间</param>
        /// <param name="bTimeOutReturn">设置为true 超时返回，跳出循环；设置为false 超时不返回，卡在循环里</param>
        /// <param name="bPause">是否响应暂停</param>
        /// <returns>true:未超时  false:超时  </returns>
        public bool WaitDI(DI enumDI, bool bValue, int nTimeOutS = 0, bool bTimeOutReturn = false, bool bPause = true)
        {
            string str1 = $"等待 DI[{enumDI}] = {bValue}";
            Logger.Process(this.Name, str1, 1);

            int nTimeCount = 0;
            if (nTimeOutS == 0)
                nTimeOutS = ProgramTool.g_WaitDiTime;

            while (true)
            {
                CheckContinue(bPause);
                bool bBit = CardMgr.DI[(int)enumDI].Value;
                if (bBit == bValue)
                    return true;

                Thread.Sleep(nScanTime);
                nTimeCount += nScanTime;
                if (nTimeOutS != -1 && nTimeCount > nTimeOutS * 1000)
                {
                    DateTime dtNow = DateTime.Now;
                    EventMgr.Notice(DiHandle.ErrDI.超时, enumDI.ToString(), $"{"等待".txtTrans(TransType.Mark)} DI[{enumDI.ToString().txtTrans(TransType.Mark)}] = {bValue} {"超时".txtTrans(TransType.Mark)}", this.Name);
                    nTimeCount = 0;
                    Duration = Math.Round((DateTime.Now - dtNow).TotalSeconds, 3)+ nTimeOutS;
                    if (bTimeOutReturn)
                        return false;
                }
            }
        }


        /// <summary>
        /// 等待任意DI有效
        /// </summary>
        /// <param name="enumDI">DI枚举数组</param>
        /// <param name="bValue">有效值</param>
        /// <param name="nTimeOutS">超时时间(以秒为单位)，-1:一直等待， 0：使用系统参数设置时间， 其它：特定时间</param>
        /// <param name="bPause">是否响应暂停</param>
        /// <returns>有效的DI</returns>
        public DI WaitDI(DI[] enumDI, bool bValue, int nTimeOutS = 0, bool bPause = true)
        {
            string str1 = "";
            for (int i = 0; i < enumDI.Length; i++)
            {
                str1 += enumDI[i];
                if (i < enumDI.Length - 1)
                    str1 += ",";
            }
            Logger.Process(this.Name, $"等待任意 DI[{str1}] = {bValue}", 1);

            DI? diRtn = null;
            int nTimeCount = 0;
            if (nTimeOutS == 0)
                nTimeOutS = ProgramTool.g_WaitDiTime;//待确认

            while (true)
            {
                CheckContinue(bPause);
                //只要任意一个DI信号满足条件就跳出
                foreach (DI name in enumDI)
                {
                    if (CardMgr.DI[(int)name].Value == bValue)
                    {
                        diRtn = name;
                        break;
                    }
                }
                if (diRtn != null)
                    return (DI)diRtn;

                Thread.Sleep(nScanTime);
                nTimeCount += nScanTime;
                if (nTimeOutS != -1 && nTimeCount > nTimeOutS * 1000)
                {
                    EventMgr.Notice(DiHandle.ErrDI.超时, enumDI[0].ToString(), $"{"等待任意".txtTrans(TransType.Mark)} DI[{str1.ToString().txtTrans(TransType.Mark)}] = {bValue} {"超时".txtTrans(TransType.Mark)}", this.Name);
                    nTimeCount = 0;
                }
            }
        }


        /// <summary>
        /// 等待所有DI有效
        /// </summary>
        /// <param name="enumDI">DI枚举数组</param>
        /// <param name="bValue">有效值</param>
        /// <param name="nTimeOutS">超时时间(以秒为单位)，-1:一直等待， 0：使用系统参数设置时间， 其它：特定时间</param>
        /// <param name="bPause">是否响应暂停</param>
        public void WaitDiAll(DI[] enumDI, bool bValue, int nTimeOutS = 0, bool bPause = true)
        {
            string str1 = "";
            for (int i = 0; i < enumDI.Length; i++)
            {
                str1 += enumDI[i];
                if (i < enumDI.Length - 1)
                    str1 += ",";
            }
            Logger.Process(this.Name, $"等待所有 DI[{str1}] = {bValue}", 1);

            int nTimeCount = 0;
            if (nTimeOutS == 0)
                nTimeOutS = ProgramTool.g_WaitDiTime;

            while (true)
            {
                CheckContinue(bPause);
                if (enumDI.Where(n => CardMgr.DI[(int)n].Value == bValue).Count() == enumDI.Count())
                    return;

                Thread.Sleep(nScanTime);
                nTimeCount += nScanTime;
                if (nTimeOutS != -1 && nTimeCount > nTimeOutS * 1000)
                {
                    EventMgr.Notice(DiHandle.ErrDI.超时, enumDI[0].ToString(), $"{"等待所有".txtTrans(TransType.Mark)} DI[{str1.ToString().txtTrans(TransType.Mark)}] = {bValue} {"超时".txtTrans(TransType.Mark)}", this.Name);
                    nTimeCount = 0;
                }
            }
        }

        /// <summary>
        /// 等待启动
        /// </summary>
        public override void WaitBegin()
        {
            if (StationMgr.WaitBeginButton < 0)
                return;

            if (SystemIO.diStart == null)
                return;

            WaitDI((DI)StationMgr.WaitBeginButton, true, -1);
        }

        #endregion

        #region 气缸
        /// <summary>
        /// 气缸伸出
        /// </summary>
        /// <param name="cylinder">气缸枚举</param>
        /// <param name="wait">是否等待气缸到位信号</param>
        /// <param name="nTimeOutS">等待到位超时时间(以秒为单位)，-1:一直等待， 0：使用系统参数设置时间， 其它：特定时间</param>
        /// <param name="bPause">是否响应暂停</param>
        public void CylinderOut(Cylinder cylinder, bool wait = true, int nTimeOutS = 0, bool bPause = true)
        {
            var cyl = CylinderMgr.GetCyLinder((int)cylinder);
            if (cyl == null)
                throw new Exception("未获取到气缸".txtTrans(TransType.Mark) + $"{cylinder}");

            if (cyl.DI_SafetyCurtain != null)
                WaitDI((DI)cyl.DI_SafetyCurtain.nTotalIndex, true, -1, false, bPause);

            cyl.Out();
            if (wait)
                WaitCylOut(cylinder, nTimeOutS, bPause);
        }

        /// <summary>
        /// 气缸缩回
        /// </summary>
        /// <param name="cylinder">气缸枚举</param>
        /// <param name="wait">是否等待气缸到位信号</param>
        /// <param name="nTimeOutS">等待到位超时时间(以秒为单位)，-1:一直等待， 0：使用系统参数设置时间， 其它：特定时间</param>
        /// <param name="bPause">是否响应暂停</param>
        public void CylinderBack(Cylinder cylinder, bool wait = true, int nTimeOutS = 0, bool bPause = true)
        {
            var cyl = CylinderMgr.GetCyLinder((int)cylinder);
            if (cyl == null)
                throw new Exception("未获取到气缸".txtTrans(TransType.Mark) + $"{cylinder}");

            if (cyl.DI_SafetyCurtain != null)
                WaitDI((DI)cyl.DI_SafetyCurtain.nTotalIndex, true, -1, false, bPause);

            cyl.Back();
            if (wait)
                WaitCylBack(cylinder, nTimeOutS, bPause);
        }

        /// <summary>
        /// 等待气缸伸出到位
        /// </summary>
        /// <param name="cylinder">气缸枚举</param>
        /// <param name="nTimeOutS">超时时间(以秒为单位)，-1:一直等待， 0：使用系统参数设置时间， 其它：特定时间</param>
        /// <param name="bPause">是否响应暂停</param>
        public void WaitCylOut(Cylinder cylinder, int nTimeOutS = 0, bool bPause = true)
        {
            string str1 = $"等待 气缸[{cylinder}] 伸出";
            Logger.Process(this.Name, str1, 1);

            var cyl = CylinderMgr.GetCyLinder((int)cylinder);
            if (cyl == null)
                throw new Exception("未获取到气缸".txtTrans(TransType.Mark) + $"{cylinder}");

            int nTimeCount = 0;
            if (nTimeOutS == 0)
                nTimeOutS = ProgramTool.g_WaitDiTime;

            while (true)
            {
                CheckContinue(bPause);
                if (cyl.State == CylinderState.Out)
                    return;

                Thread.Sleep(nScanTime);
                nTimeCount += nScanTime;
                if (nTimeOutS != -1 && nTimeCount > nTimeOutS * 1000)
                {
                    EventMgr.Notice(CylinderHandle.ErrCylinder.超时, cylinder.ToString(), $"{"等待".txtTrans(TransType.Mark)} {"气缸".txtTrans(TransType.Mark)}[{cylinder.ToString().txtTrans(TransType.Mark)}] {"伸出".txtTrans(TransType.Mark)} {"超时".txtTrans(TransType.Mark)}", this.Name);
                    nTimeCount = 0;
                }
            }
        }

        /// <summary>
        /// 等待气缸缩回到位
        /// </summary>
        /// <param name="cylinder">气缸枚举</param>
        /// <param name="nTimeOutS">超时时间(以秒为单位)，-1:一直等待， 0：使用系统参数设置时间， 其它：特定时间</param>
        /// <param name="bPause">是否响应暂停</param>
        public void WaitCylBack(Cylinder cylinder, int nTimeOutS = 0, bool bPause = true)
        {
            string str1 = $"等待 气缸[{cylinder}] 缩回";
            Logger.Process(this.Name, str1, 1);

            var cyl = CylinderMgr.GetCyLinder((int)cylinder);
            if (cyl == null)
                throw new Exception("未获取到气缸".txtTrans(TransType.Mark) + $"{cylinder}");

            int nTimeCount = 0;
            if (nTimeOutS == 0)
                nTimeOutS = ProgramTool.g_WaitDiTime;

            while (true)
            {
                CheckContinue(bPause);
                if (cyl.State == CylinderState.Back)
                    return;

                Thread.Sleep(nScanTime);
                nTimeCount += nScanTime;
                if (nTimeOutS != -1 && nTimeCount > nTimeOutS * 1000)
                {
                    EventMgr.Notice(CylinderHandle.ErrCylinder.超时, cylinder.ToString(), $"{"等待".txtTrans(TransType.Mark)} {"气缸".txtTrans(TransType.Mark)}[{cylinder.ToString().txtTrans(TransType.Mark)}] {"缩回".txtTrans(TransType.Mark)} {"超时".txtTrans(TransType.Mark)}", this.Name);
                    nTimeCount = 0;
                }
            }
        }
        #endregion


        #region 轴
        /// <summary>
        /// 当前站位所有轴上使能
        /// </summary>
        /// <param name="station"></param>
        /// <param name="bServoOn"></param>
        public void AllServoOn(StationEx station, bool bServoOn)
        {
            foreach (AXIS item in station.arrayAxis)
            {
                item.ServoOn(bServoOn);
            }
        }

        /// <summary>
        /// 单轴跑点位
        /// </summary>
        /// <param name="enumAxis">轴枚举</param>
        /// <param name="enumPoint">点位枚举</param>
        /// <param name="nSpeedPercent">速度百分比</param>
        /// <param name="dbAcc">加速度：s，设为0 使用配置文档</param>
        /// <param name="dbDec">减速度：s，设为0 使用配置文档</param>
        /// <param name="bPause">是否响应暂停</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool AxisSingleTo<T>( T enumPoint, AXIS enumAxis, int nSpeedPercent = 100, double dbAcc = 0, double dbDec = 0, bool bPause = true) where T : Enum
        {
            int nPointIdx = Convert.ToInt32(enumPoint);
            if (!this.dictPoint.ContainsKey(nPointIdx))
                throw new Exception("点位获取错误".txtTrans(TransType.Mark) + $"：{enumPoint}");
            if (nSpeedPercent > 100 || nSpeedPercent < 1)
                throw new Exception("速度百分比超限".txtTrans(TransType.Mark));

            AxisHandle axisHandle = CardMgr.GetAxis((int)enumAxis);
            if (axisHandle == null)
                throw new Exception("未获取到轴".txtTrans(TransType.Mark) + $"：{enumAxis}");
            if (!this.dictPoint[nPointIdx].HasAxis(axisHandle.TotalIndex))
                throw new Exception("点位获取错误".txtTrans(TransType.Mark) + $"：{enumPoint}");
            return axisHandle.AbsMove(this.dictPoint[nPointIdx][axisHandle.Alias], nSpeedPercent, dbAcc, dbDec, bPause);
        }

        /// <summary>
        /// 单轴跑位置
        /// </summary>
        /// <param name="enumAxis">轴枚举</param>
        /// <param name="fPos">目标位置</param>
        /// <param name="nSpeedPercent">速度百分比</param>
        /// <param name="dbAcc">加速度：s，设为0 使用配置文档</param>
        /// <param name="dbDec">减速度：s，设为0 使用配置文档</param>
        /// <param name="bPause">是否响应暂停</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool AxisSingleTo( double fPos, AXIS enumAxis, int nSpeedPercent = 100, double dbAcc = 0, double dbDec = 0, bool bPause = true)
        {
            if (nSpeedPercent > 100 || nSpeedPercent < 1)
                throw new Exception("速度百分比超限".txtTrans(TransType.Mark));

            AxisHandle axisHandle = CardMgr.GetAxis((int)enumAxis);
            if (axisHandle == null)
                throw new Exception("未获取到轴".txtTrans(TransType.Mark) + $"：{enumAxis}");
            return axisHandle.AbsMove(fPos, nSpeedPercent, dbAcc, dbDec, bPause);
        }

        /// <summary>
        /// 更新轴目标位置
        /// </summary>
        /// <param name="enumAxis"></param>
        /// <param name="enumPoint"></param>
        /// <exception cref="Exception"></exception>
        public void UpdatePtpTgtPos(AXIS enumAxis, double enumPoint)
        {
            AxisHandle axisHandle = CardMgr.GetAxis((int)enumAxis);
            if (axisHandle == null)
                throw new Exception("未获取到轴".txtTrans(TransType.Mark) + $"：{enumAxis}");
            //public ulong m_hCardHandle = 4294967295uL;
            //double Tagetpos = this.dictPoint[(int)enumPoint][axisHandle.Alias] * enumAxis.GetHandle().config.dbGearRatio;
            //ImcApi.IMC_UpdatePtpTgtPos(0, (short)enumAxis, enumPoint);
            double Tagetpos= enumPoint* enumAxis.GetHandle().config.dbGearRatio;
            Imc60.IMC_UpdatePtpTgtPos(0, (short)enumAxis, Tagetpos);
            //Imc60.IMC_UpdatePtpTgtPos(0, (short)enumAxis, this.dictPoint[(int)enumPoint][axisHandle.Alias]);

        }
        /// <summary>
        /// 多轴跑点位
        /// </summary>
        /// <param name="enumAxis">要参与运动的轴 数组，如果为 null 所有轴一起跑</param>
        /// <param name="enumPoint">点位枚举</param>
        /// <param name="nSpeedPercent">速度百分比</param>
        /// <param name="bPause">是否响应暂停</param>
        /// <returns></returns>
        public bool AxisGoto<T>( T enumPoint, AXIS[] enumAxis, int nSpeedPercent = 100, bool bPause = true) where T : Enum
        {
            bool bRtn = true;
            if (enumAxis == null)
            {
                foreach (var item in this.arrayAxis)
                    bRtn = bRtn && AxisSingleTo( enumPoint, (AXIS)item, nSpeedPercent, 0, 0, bPause);
            }
            else
            {
                foreach (var item in enumAxis)
                    bRtn = bRtn && AxisSingleTo( enumPoint, item, nSpeedPercent, 0, 0, bPause);
            }
            return bRtn;
        }

        /// <summary>
        /// 多轴跑位置
        /// </summary>
        /// <param name="enumAxis">要参与运动的轴 数组，如果为 null 所有轴一起跑</param>
        /// <param name="fPos">目标位置数组</param>
        /// <param name="nSpeedPercent">速度百分比</param>
        /// <param name="bPause">是否响应暂停</param>
        /// <returns></returns>
        public bool AxisGoto( double[] fPos, AXIS[] enumAxis, int nSpeedPercent = 100, bool bPause = false)
        {
            int n = enumAxis == null ? this.arrayAxis.Length : enumAxis.Length;
            if (n != fPos.Length)
                throw new Exception("点位长度不匹配".txtTrans(TransType.Mark));

            bool bRtn = true;
            int i = 0;
            if (enumAxis == null)
            {
                foreach (var item in this.arrayAxis)
                    bRtn = bRtn && AxisSingleTo( fPos[i++], (AXIS)item, nSpeedPercent, 0, 0, bPause);
            }
            else
            {
                foreach (var item in enumAxis)
                    bRtn = bRtn && AxisSingleTo( fPos[i++], item, nSpeedPercent, 0, 0, bPause);
            }
            return bRtn;
        }

        /// <summary>
        /// 等轴待运动完成
        /// </summary>
        /// <param name="enumAxis">轴枚举</param>
        /// <param name="nInPosError"> 到位误差[脉冲值]：小于0 使用系统配置，大于0 实际误差值</param>
        /// <param name="nTimeOutS">超时时间(以秒为单位)，-1:一直等待， 0：使用系统参数设置时间， 其它：特定时间</param>
        /// <param name="bPause">是否响应暂停</param>
        public void WaitAxis(AXIS enumAxis, int nInPosError = -1, int nTimeOutS = 0, bool bPause = true)
        {
            AxisHandle axisHandle = CardMgr.GetAxis((int)enumAxis);
            if (axisHandle == null)
                throw new Exception("未获取到轴".txtTrans(TransType.Mark) + $"：{enumAxis}");

            if(axisHandle.IsHoming)
                Logger.Process(this.Name, $"等待 轴[{axisHandle.Name}] 回原", 1);
            else
                Logger.Process(this.Name, $"等待 轴[{axisHandle.Name}] 到位", 1);

            if (nInPosError < 0)
                nInPosError = axisHandle.config.nInPosError;

            int nTimeCount = 0;
            if (nTimeOutS == 0)
                nTimeOutS = ProgramTool.g_WaitMotionTime;

            while (true)
            {
                CheckContinue(bPause);
                if (axisHandle.IsInPos(nInPosError))
                    return;

                //未到位
                Thread.Sleep(nScanTime);
                nTimeCount += nScanTime;
                if (nTimeOutS != -1 && nTimeCount > nTimeOutS * 1000)
                {
                    EventMgr.Warn(AxisHandle.ErrAxis.到位超时, axisHandle.Name, "到位超时".txtTrans(TransType.Mark));
                    nTimeCount = 0;
                }
            }
        }
        /// <summary>
        /// 判断轴是否到位
        /// </summary>
        /// <param name="enumAxis"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool IsInPos(AXIS enumAxis)
        {
            if (!enumAxis.GetType().IsEnum)
                throw new Exception("轴枚举错误".txtTrans(TransType.Mark) + $"：{enumAxis}");
            AxisHandle axisHandle = CardMgr.GetAxis((int)enumAxis);
            if (axisHandle == null)
                throw new Exception("未获取到轴".txtTrans(TransType.Mark) + $"：{enumAxis}");

            bool nRtn = axisHandle.IsInPos(axisHandle.config.nInPosError);
            return nRtn;
        }
        /// <summary>
        /// 等待插补运动完成
        /// </summary>
        /// <param name="nCardIndex">卡号</param>
        /// <param name="nPointTableIndex">插补缓存表序号</param>
        /// <param name="nTimeOutS">超时时间(以秒为单位)，-1:一直等待， 0：使用系统参数设置时间， 其它：特定时间</param>
        /// <param name="bPause">是否响应暂停</param>
        public void WaitAxisInterpolation(int nCardIndex, int nPointTableIndex, int nTimeOutS = 0, bool bPause = true)
        {
            string str1 = "等待插补到位";
            Logger.Process(this.Name, str1, 1);
            int nTimeCount = 0;
            if (nTimeOutS == 0)
                nTimeOutS = ProgramTool.g_WaitMotionTime;

            while (true)
            {
                CheckContinue(bPause);
                int nRtn = CardMgr.PointTable_IsStop(nCardIndex, nPointTableIndex);
                if (nRtn == -1)//未停止
                {
                    Thread.Sleep(nScanTime);
                    nTimeCount += nScanTime;

                    if (nTimeOutS != -1 && nTimeCount > nTimeOutS * 1000)
                    {
                        EventMgr.Warn($"{str1.txtTrans(TransType.Mark)} {"超时".txtTrans(TransType.Mark)}", Name);
                        nTimeCount = 0;
                    }
                }
                else if (nRtn == 0) //已完成插补运动
                {
                    return;
                }
                else //异常停止
                {
                    string[] strError = { "急停".txtTrans(TransType.Mark), "报警".txtTrans(TransType.Mark), "未励磁".txtTrans(TransType.Mark), "正限位".txtTrans(TransType.Mark), "负限位".txtTrans(TransType.Mark), "停止位置超限".txtTrans(TransType.Mark) };
                    if (nRtn < 7)
                        str1 = "插补运动异常".txtTrans(TransType.Mark) + $"：{strError[nRtn - 1]}";
                    else
                        str1 = "插补运动异常，代码".txtTrans(TransType.Mark) + $"：{nRtn - 10}";
                    if (StationMgr.State != StationState.Emg &&
                        StationMgr.State != StationState.Manual)
                        throw new Exception(str1);
                }
            }
        }

        public void Home35(AXIS enumAxis)
        {
            string errorMessage = null;
            //【1】更新轴号
            short nTempAxNo = (short)enumAxis; //实时读取轴选择
            //【2】写入回原参数
            Imc60.THomingPara tHomingPara = new Imc60.THomingPara();
            tHomingPara.homeMethod = 35;
            tHomingPara.offset = 0;
            tHomingPara.highVel = 0;
            tHomingPara.lowVel = 0;
            tHomingPara.acc = 0;
            short nHomingStatus = 0;
            //【3】开始回原
            UInt32 ret = Imc60.IMC_StartHoming(0, nTempAxNo, ref tHomingPara);//StartHoming指令可以下发回零参数，并且执行开始回零动作
            bool isJudgeHm = true;
            if (ret != 0)
            {
                throw new Exception($"{enumAxis}开始回原失败,错误代码为0x" + ret.ToString("x8"));
                return;
            }
            else
            {

                Task task_HmMovSts = new Task(() =>
                {
                    while (isJudgeHm)
                    {
                        Thread.Sleep(5);
                        Imc60.IMC_GetHomingStatus(0, nTempAxNo, ref nHomingStatus);
                        if (nHomingStatus == 3)
                        {
                            errorMessage=( $"{enumAxis}回零状态值： 3， 回零完成");
                            break;
                        }
                        if (nHomingStatus == 0)
                        {
                            errorMessage = ($"{enumAxis}回零状态值： 0， 正在回零...");
                        }
                        else
                        {
                            errorMessage = ($"{enumAxis}回零状态值： " + nHomingStatus.ToString() + "," + "   回零错误");
                            break;
                        }
                        throw new Exception(errorMessage);
                    }
                });

                task_HmMovSts.Start();
            }
        }
        #endregion


        #region 通讯

        /// <summary>
        /// 等待网络回复以某字符串开头和某字符串结尾的字符串数据
        /// </summary>
        /// <param name="tcplink">TCP对象</param>
        /// <param name="strReceive">接收到的数据</param>
        /// <param name="strStartWith">开头字符串</param>
        /// <param name="strEndWith">结尾字符串</param>
        /// <param name="nTimeOutS">超时时间，单位秒 0:系统时间 -1:一直等待 其他:其他时间</param>
        /// <param name="bTimeOutReturn">设置为true 超时返回，跳出循环；设置为false 超时不返回，卡在循环里</param>
        /// <returns>true:未超时  false:超时</returns>
        public override bool WaitTcp(TcpClient tcplink, out string strReceive, string strStartWith = "", string strEndWith = "", int nTimeOutS = 0, bool bTimeOutReturn = true)
        {
            strReceive = "";

            if (strStartWith.Length > 0 || strEndWith.Length > 0)
                Logger.Process(this.Name, $"等待 TCP[{tcplink.m_strName}] 回复，格式“{strStartWith}...{strEndWith}”", 1);
            else
                Logger.Process(this.Name, $"等待 TCP[{tcplink.m_strName}] 回复", 1);

            string sRead = "", sTemp = "";
            int nTimeCount = 0;
            while (true)
            {
                CheckContinue(false);
                tcplink.ReadLine(out sRead, true);

                sTemp += sRead;
                if (strStartWith.Length > 0 && strEndWith.Length > 0
                    && sTemp.StartsWith(strStartWith)
                    && sTemp.EndsWith(strEndWith))
                {
                    //等待开头和结尾
                    Logger.Process(this.Name, $"TCP[{tcplink.m_strName}] <- {sTemp}");
                    strReceive = sTemp;
                    return true;
                }
                else if (strStartWith.Length == 0 && strEndWith.Length > 0
                    && sTemp.EndsWith(strEndWith))
                {
                    //等待结尾
                    Logger.Process(this.Name, $"TCP[{tcplink.m_strName}] <- {sTemp}");
                    strReceive = sTemp;
                    return true;
                }
                else if (strStartWith.Length > 0 && strEndWith.Length == 0
                    && sTemp.StartsWith(strStartWith))
                {
                    //等待开头
                    Logger.Process(this.Name, $"TCP[{tcplink.m_strName}] <- {sTemp}");
                    strReceive = sTemp;
                    return true;
                }
                else if (sTemp.Length > 0)
                {
                    Logger.Process(this.Name, $"TCP[{tcplink.m_strName}] <- {sTemp}");
                    strReceive = sTemp;
                    return true;
                }

                Thread.Sleep(nScanTime);
                nTimeCount += nScanTime;
                if (nTimeCount > ProgramTool.g_WaitCommuTime * 1000)
                {
                    if (bTimeOutReturn)
                        return false;
                    EventMgr.Notice(ErrCommunicate.超时, tcplink.m_strName, $"{"等待".txtTrans(TransType.Mark)} TCP[{tcplink.m_strName.txtTrans(TransType.Mark)}] {"回复".txtTrans(TransType.Mark)} {"超时".txtTrans(TransType.Mark)}", this.Name);
                    nTimeCount = 0;
                }
            }
        }

        /// <summary>
        /// 等待串口回复以某字符串开头和某字符串结尾的字符串数据
        /// </summary>
        /// <param name="comLink">TCP对象</param>
        /// <param name="strReceive">接收到的数据</param>
        /// <param name="strStartWith">开头字符串</param>
        /// <param name="strEndWith">结尾字符串</param>
        /// <param name="nTimeOutS">超时时间，单位秒 0:系统时间 -1:一直等待 其他:其他时间</param>
        /// <param name="bTimeOutReturn">设置为true 超时返回，跳出循环；设置为false 超时不返回，卡在循环里</param>
        /// <returns>true:未超时  false:超时  </returns>
        public bool WaitCom(ComPort comLink, out string strReceive, string strStartWith = "", string strEndWith = "", int nTimeOutS = 0, bool bTimeOutReturn = false)
        {
            strReceive = "";

            if (strStartWith.Length > 0 || strEndWith.Length > 0)
                Logger.Process(this.Name, $"等待 COM[{comLink.m_strName}] 回复，格式“{strStartWith}...{strEndWith}”", 1);
            else
                Logger.Process(this.Name, $"等待 COM[{comLink.m_strName}] 回复", 1);

            string sRead = "", sTemp = "";
            int nTimeCount = 0;
            while (true)
            {
                CheckContinue(false);
                comLink.ReadLine(out sRead);

                sTemp += sRead;
                if (strStartWith.Length > 0 && strEndWith.Length > 0
                    && sTemp.StartsWith(strStartWith)
                    && sTemp.EndsWith(strEndWith))
                {
                    //等待开头和结尾
                    Logger.Process(this.Name, $"COM[{comLink.m_strName}] <- {sTemp}", 1);
                    strReceive = sTemp;
                    return true;
                }
                else if (strStartWith.Length == 0 && strEndWith.Length > 0
                    && sTemp.EndsWith(strEndWith))
                {
                    //等待结尾
                    Logger.Process(this.Name, $"COM[{comLink.m_strName}] <- {sTemp}", 1);
                    strReceive = sTemp;
                    return true;
                }
                else if (strStartWith.Length > 0 && strEndWith.Length == 0
                    && sTemp.StartsWith(strStartWith))
                {
                    //等待开头
                    Logger.Process(this.Name, $"COM[{comLink.m_strName}] <- {sTemp}", 1);
                    strReceive = sTemp;
                    return true;
                }
                else if (sTemp.Length > 0)
                {
                    Logger.Process(this.Name, $"COM[{comLink.m_strName}] <- {sTemp}", 1);
                    strReceive = sTemp;
                    return true;
                }

                Thread.Sleep(nScanTime);
                nTimeCount += nScanTime;
                if (nTimeCount > ProgramTool.g_WaitCommuTime * 1000)
                {
                    if (bTimeOutReturn)
                        return false;
                    EventMgr.Notice(ErrCommunicate.超时, comLink.m_strName, $"{"等待".txtTrans(TransType.Mark)} COM[{comLink.m_strName.txtTrans(TransType.Mark)}] {"回复".txtTrans(TransType.Mark)} {"超时".txtTrans(TransType.Mark)}", this.Name);
                    nTimeCount = 0;
                }
            }
        }

        #endregion

        /// <summary>
        /// 保存CSV文件
        /// </summary>
        /// <param name="FolderName">文件夹名称</param>
        /// <param name="strFileName">文件名称</param>
        /// <param name="strHead">表头</param>
        /// <param name="strWriteData">要写入的数据</param>
        public void SaveCsv(string FolderName, string strFileName, string strHead, params string[] strWriteData)
        {
            lock (m_FileLock)
            {
                try
                {
                    string strPath;
                    strPath = FolderName;
                    if (!Directory.Exists(strPath))
                        Directory.CreateDirectory(strPath);
                    string s1 = strHead;
                    string s2 = "";
                    foreach (string item in strWriteData)
                    {
                        s2 += item + ",";
                    }
                    string strFile = strPath;
                    strFile = strFile + strFileName + ".csv";
                    bool bIsExist = File.Exists(strFile);

                    using (StreamWriter sw = new StreamWriter(new FileStream(strFile, FileMode.Append, FileAccess.Write), Encoding.UTF8))
                    {
                        if (!bIsExist)
                            sw.WriteLine(s1);
                        sw.WriteLine(s2);
                        sw.Flush();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

        }

        //
        // 摘要:
        //     弹窗提示 人机交互
        //
        // 参数:
        //   strMsg:
        //     内容
        //
        //   strTitle:
        //     标题
        //
        //   strButton:
        //     自定义按钮
        //
        //   nBindDI:
        //     绑定按钮的信号，数量和按钮数相同
        //
        //   bBlock:
        //     是否卡流程
        //
        // 返回结果:
        //     返回选择的按钮Index
        public int ShowMessage(string strMsg, string strTitle = null, string[] strButton = null, object[] nBindDI = null, bool bBlock = true)
        {
            DateTime dtNow = DateTime.Now;
            if (strButton == null)
            {
                strButton = new string[1] { "确认" };
            }

            for (int i = 0; i < strButton.Length; i++)
            {
                if (nBindDI == null)
                {
                    break;
                }

                if (i < nBindDI.Length)
                {
                    string text = nBindDI[i]?.ToString() ?? "";
                    if (text.Length >= 1)
                    {
                        ref string reference = ref strButton[i];
                        reference = reference + "\r\n[" + text + "]";
                    }
                }
            }

            MessageMgr.MessageInfo messageInfo = MessageMgr.Show(strMsg, strTitle, strButton, Name);
            while (bBlock)
            {
                Delay(10);
                if (messageInfo.nReturn >= 0)
                {
                    break;
                }

                if (nBindDI == null)
                {
                    continue;
                }

                for (int j = 0; j < nBindDI.Length; j++)
                {
                    if (j < strButton.Length)
                    {
                        int num = (int)nBindDI[j];
                        if (num >= 0 && num < CardMgr.DI.Count && CardMgr.DI[num].Value)
                        {
                            messageInfo.nReturn = j;
                            break;
                        }
                    }
                }
            }

            if (messageInfo.nReturn >= 0 && messageInfo.nReturn < strButton.Length)
            {
                messageInfo.Clear();
                MessageMgr.PostNewMessage(messageInfo);
                Logger.Manual(Name + "弹窗：" + strMsg.txtParse(TransType.Org) + " ----- 选择按钮:" + strButton[messageInfo.nReturn]);
            }
            Duration=Math.Round((DateTime.Now - dtNow).TotalSeconds, 3);
            return messageInfo.nReturn;
        }
        public void WaitMaterial(Material material, string strMsg = null,DI? di = null)
        {
            EventMgr.WaitMaterial(material, true);
            object[] nBindDI = null;
            if (di != null)
                nBindDI = new object[] { di };
            ShowMessage(strMsg, "缺料".txtTrans(TransType.Mark), new string[] { "确认".txtTrans(TransType.Mark) }, nBindDI);
            EventMgr.WaitMaterial(material, false);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AF.Card;
using AF.Common;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using AF.Station;
using AF.Data;
using AF.Communicate;

namespace Autoframe
{
    public static partial class HandleEx
    {
        #region Register

        public static bool Get(this RegBit regBit)
        {
            return Register.GetBit((int)regBit);
        }

        public static void Set(this RegBit regBit, bool bValue, bool bNotify = true)
        {
            Register.SetBit((int)regBit, bValue, bNotify);
        }

        public static int Get(this RegInt regInt)
        {
            return Register.GetInt((int)regInt);
        }

        public static void Set(this RegInt regInt, int nValue, bool bNotify = true)
        {
            Register.SetInt((int)regInt, nValue, bNotify);
        }

        public static double Get(this RegDouble regDouble)
        {
            return Register.GetDouble((int)regDouble);
        }

        public static void Set(this RegDouble regDouble, double fValue, bool bNotify = true)
        {
            Register.SetDouble((int)regDouble, fValue, bNotify);
        }

        public static string Get(this RegString regString)
        {
            return Register.GetString((int)regString);
        }

        public static void Set(this RegString regString, string strValue, bool bNotify = true)
        {
            Register.SetString((int)regString, strValue, bNotify);
        }

        #endregion

        #region MemoryMap

        public static bool Get(this MemoryMapBit n)
        {
            bool b = false;
            MemoryMap.Read((int)n, ref b);
            return b;
        }

        public static void Set(this MemoryMapBit n, bool bValue)
        {
            MemoryMap.Write((int)n, bValue);
        }

        public static int Get(this MemoryMapInt n)
        {
            int nRtn = 0;
            MemoryMap.Read((int)n, ref nRtn);
            return nRtn;
        }

        public static void Set(this MemoryMapInt n, int nValue)
        {
            MemoryMap.Write((int)n, nValue);
        }

        public static double Get(this MemoryMapDouble n)
        {
            double f = 0;
            MemoryMap.Read((int)n, ref f);
            return f;
        }

        public static void Set(this MemoryMapDouble n, double fValue)
        {
            MemoryMap.Write((int)n, fValue);
        }

        public static string Get(this MemoryMapString n)
        {
            string s = "";
            MemoryMap.Read((int)n, ref s);
            return s;
        }

        public static void Set(this MemoryMapString n, string strValue)
        {
            MemoryMap.Write((int)n, strValue);
        }

        #endregion


        #region IO
        /// <summary>
        /// 获取输入信号状态
        /// </summary>
        /// <param name="enumDI">DI枚举中成员</param>
        /// <returns>DI当前状态</returns>
        public static bool Get(this DI enumDI)
        {
            return CardMgr.DI[(int)enumDI].Value;
        }

        public static bool Read(this DI enumDI)
        {
            return CardMgr.DI[(int)enumDI].Read();
        }


        /// <summary>
        /// 获取输出信号状态
        /// </summary>
        /// <param name="enumDO">DO枚举中成员</param>
        /// <returns>DO当前状态</returns>
        public static bool Get(this DO enumDO)
        {
            return CardMgr.DO[(int)enumDO].Value;
        }
        /// <summary>
        /// 设置输出信号状态
        /// </summary>
        /// <param name="enumDO">DO枚举中成员</param>
        /// <param name="bBit">输出状态</param>
        public static void Set(this DO enumDO, bool bBit)
        {
            CardMgr.DO[(int)enumDO].Value = bBit;
        }

        public static double Get(this AI enumAI)
        {
            return CardMgr.AI[(int)enumAI].Value;
        }
        #endregion


        #region Cylinder

        /// <summary>
        /// 气缸伸出
        /// </summary>
        /// <returns>其它：执行时间 0:不等待到位 -1：超时</returns>
        public static bool Out(this Cylinder cylinder)
        {
            var cyl = CylinderMgr.GetCyLinder((int)cylinder);
            if (cyl == null)
                return false;
            return cyl.Out();
        }

        /// <summary>
        /// 气缸缩回
        /// </summary>
        /// <returns>其它：执行时间 0:不等待到位 -1：超时</returns>
        public static bool Back(this Cylinder cylinder)
        {
            var cyl = CylinderMgr.GetCyLinder((int)cylinder);
            if (cyl == null)
                return false;

            return cyl.Back();
        }

        #endregion


        #region Axis
        public static AxisHandle GetHandle(this AXIS axis)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return CardMgr.dictAxis[(int)axis];
            return null;
        }

        /// <summary>
        /// 获取轴位置
        /// </summary>
        /// <returns></returns>
        public static double GetPos(this AXIS axis)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return axis.GetHandle().GetPos();
            return 0;
        }

        /// <summary>
        /// 伺服使能
        /// </summary>
        /// <param name="bServoOn">使能或断使能</param>
        public static bool ServoOn(this AXIS axis, bool bServoOn)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return axis.GetHandle().ServoOn(bServoOn);
            return false;
        }
        /// <summary>
        /// 伺服正常停止
        /// </summary>
        public static void Stop(this AXIS axis)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                axis.GetHandle().Stop();
        }

        /// <summary>
        /// 设置单轴的某一运动参数(浮点)
        /// </summary>
        /// <param name="nParam">参数:自定义扩展</param>
        /// <param name="fData">参数值</param>
        /// <returns></returns>
        public static bool SetParam(this AXIS axis, int nParam, double fData)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return axis.GetHandle().SetParam(nParam, fData);
            return false;
        }

        /// <summary>
        /// 绝对位置移动：不受系统速度影响
        /// </summary>
        /// <param name="nPos">目标点位：mm</param>
        /// <param name="dbSpeed">速度：mm/s</param>
        /// <param name="dbAcc">加速度：s，设为0 使用配置文档</param>
        /// <param name="dbDec">减速度：s，设为0 使用配置文档</param>
        /// <param name="bPause">是否响应暂停</param>
        /// <returns></returns>
        public static bool AbsMove(this AXIS axis, double nPos, double dbSpeed, double dbAcc = 0, double dbDec = 0, bool bPause = true)//按照自定义速度，不受系统影响
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return axis.GetHandle().AbsMove(nPos, dbSpeed, dbAcc, dbDec, bPause );
            return false;
        }
        /// <summary>
        /// 绝对位置移动：受系统速度影响
        /// </summary>
        /// <param name="fPos">位置：mm</param> 
        /// <param name="nSpeedPercent">速度百分比</param>
        /// <param name="dbAcc">加速度：s，设为0 使用配置文档</param>
        /// <param name="dbDec">减速度：s，设为0 使用配置文档</param>
        /// <param name="bPause">是否响应暂停</param>
        /// <returns></returns>
        public static bool AbsMove(this AXIS axis, double fPos, int nSpeedPercent = 100, double dbAcc = 0, double dbDec = 0, bool bPause = true)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return axis.GetHandle().AbsMove(fPos, nSpeedPercent, dbAcc, dbDec, bPause );
            return false;
        }

        /// <summary>
        /// 相对位置移动：不受系统速度影响
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="fPos">相对位置：mm</param>
        /// <param name="dbSpeed">绝对值速度</param>
        /// <param name="dbAcc">加速度：s，设为0 使用配置文档</param>
        /// <param name="dbDec">减速度：s，设为0 使用配置文档</param>
        /// <param name="bPause">是否响应暂停</param>
        /// <returns></returns>
        public static bool RelativeMove(this AXIS axis, double fPos, double dbSpeed, double dbAcc = 0, double dbDec = 0, bool bPause = false)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return axis.GetHandle().RelativeMove(fPos, dbSpeed, dbAcc, dbDec, bPause );
            return false;
        }
        /// <summary>
        /// 相对位置移动：受系统速度影响
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="fPos">相对位置：mm</param>
        /// <param name="nSpeedPercent">速度百分比</param>
        /// <param name="dbAcc">加速度：s，设为0 使用配置文档</param>
        /// <param name="dbDec">减速度：s，设为0 使用配置文档</param>
        /// <param name="bPause">是否响应暂停</param>
        /// <returns></returns>
        public static bool RelativeMove(this AXIS axis, double fPos, int nSpeedPercent, double dbAcc = 0, double dbDec = 0, bool bPause = true)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return axis.GetHandle().RelativeMove(fPos, nSpeedPercent, dbAcc, dbDec, bPause );
            return false;
        }

        /// <summary>
        /// 回原点
        /// </summary>
        /// <param name="axis">自动运行</param>
        /// <param name="bPause">是否响应暂停</param>
        /// <param name="nHomeMode">缺省值-1：使用轴配置</param>
        /// <returns></returns>
        public static bool Home(this AXIS axis, bool bPause = true, int nHomeMode = -1)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return axis.GetHandle().Home(nHomeMode, bPause );
            return false;
        }



        /// <summary>
        /// 轴是否正常停止
        /// </summary>
        /// <returns></returns>
        public static AxisHandle.StopType IsStop(this AXIS axis)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return axis.GetHandle().IsStop;
            return AxisHandle.StopType.Alarm;
        }

        /// <summary>
        /// 是否在正限位
        /// </summary>
        /// <returns></returns>
        public static int IsPEL(this AXIS axis)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return axis.GetHandle().IsPEL;
            return -1;
        }
        /// <summary>
        /// 是否在负限位
        /// </summary>
        /// <returns></returns>
        public static int IsMEL(this AXIS axis)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return axis.GetHandle().IsMEL;
            return -1;
        }

        /// <summary>
        /// 是否在原点
        /// </summary>
        /// <returns></returns>
        public static int IsOrg(this AXIS axis)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return axis.GetHandle().IsOrg;
            return -1;
        }

        public static bool SetPosZero(this AXIS axis)
        {
            if (CardMgr.dictAxis.ContainsKey((int)axis))
                return axis.GetHandle().SetPosZero();
            return false;
        }
        #endregion

        #region Port
        public static TcpClient GetTcp(this TCP_Client tcp)
        {
            return TcpClientMgr.GetClient((int)tcp);
        }
        public static TcpServer GetTcp(this TCP_Server tcp)
        {
            return TcpServerMgr.GetServer((int)tcp);
        }
        public static ComPort GetPort(this COM com)
        {
            return ComMgr.GetComPort((int)com);
        }
        #endregion

        /// <summary>
        /// 获取站位
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T GetStation<T>(this string str) where T: class
        {
            return StationMgr.GetStation(str) as T;
        }
    }
}

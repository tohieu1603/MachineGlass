using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using AF.Card;
using AF.Common;
using Inovance.InoMotionCotrollerShop.InoServiceContract.EtherCATConfigApi;
using TrioMotion.TrioPC_NET;

namespace AF.CardEx
{
	/// <summary>
	/// 汇川EtherCAT运动控制卡封装,类名必须以"Card_"前导，否则加载不到
	/// </summary>
	/// <remarks>
	/// 总线卡
	/// </remarks>
	public class Card_TrioEcat : CardBase, IMotion, IIo
	{
		
		/// <summary>
		/// TRIO控制器IP
		/// </summary>
		public string IpAddress = "192.168.0.250";
		public double m_dbUnits = 0.0;

		public TrioPC trio = new TrioPC();  //实例化一个TrioPC对象

		private static Card_TrioEcat instance = null;
		private static readonly object m_lock = new object();
		private bool m_bInited = false;

		public static int n_TrioEtherCat_DI_Group = ReadCfgFromXml_("DI");
		public static int n_TrioEtherCat_DO_Group = ReadCfgFromXml_("DO");
		public uint[] m_IOCardStatusIn;
		public uint[] m_IOCardStatusOut;

		/// <summary>
		/// 获取Trio配置IO组数
		/// </summary>
		/// <returns></returns>
		public static int ReadCfgFromXml_(string IoState)
		{
			XmlDocument doc = new XmlDocument();
			//D:\新框架trio类测试\autoframe_release\exe\Config\Hardware.xml
			doc.Load(AppDomain.CurrentDomain.BaseDirectory + "\\" + "Config" + "\\" + "Hardware.xml");
			XmlNodeList xnl = doc.SelectNodes("/HardwareCfg/" + "Card");

			string CardNo = "-1";
			int count_InoEcat = 0;
			if (xnl.Count > 0)
			{
				xnl = xnl.Item(0).ChildNodes;

				if (xnl.Count > 0)
				{
					foreach (XmlNode xn in xnl)
					{
						if ((xn.NodeType == XmlNodeType.Element) && (((XmlElement)xn).GetAttribute("卡类型").Trim() == "TRIOEcat"))
						{
							CardNo = ((XmlElement)xn).GetAttribute("卡号").Trim();
						}
					}
				}
			}

			XmlNodeList xtl = doc.SelectNodes("/HardwareCfg/" + $"{IoState}");

			if (xtl.Count > 0)
			{
				xtl = xtl.Item(0).ChildNodes;

				if (xtl.Count > 0)
				{
					foreach (XmlNode xn in xtl)
					{
						if ((xn.NodeType == XmlNodeType.Element) && (((XmlElement)xn).GetAttribute("卡号").Trim() == CardNo))
						{
							count_InoEcat++;
						}
					}
				}
			}
			return count_InoEcat;
		}


		/// <summary>
		/// 构造函数：板卡类应该只初始化一次
		/// </summary>
		/// <param name="nCardTotalIndex"></param>
		/// <param name="nCardIndex"></param>
		/// <param name="strName"></param>
		public Card_TrioEcat(int nCardTotalIndex, int nCardIndex, string strName)
			: base(nCardTotalIndex, nCardIndex, strName)
		{
			bEnable = false;

			m_IOCardStatusIn = new uint[n_TrioEtherCat_DI_Group/32];
			for (int i = 0; i < n_TrioEtherCat_DI_Group / 32; i++)
			{
				m_IOCardStatusIn[i] = 0;
			}
			m_IOCardStatusOut = new uint[n_TrioEtherCat_DO_Group];
			for (int i = 0; i < n_TrioEtherCat_DO_Group/32; i++)
			{
				m_IOCardStatusOut[i] = 0;

			}



			//m_homePara.homeMethod = 0;
			//m_homePara.offset = 0;
			//m_homePara.highVel = 10000;
			//m_homePara.lowVel = 1000;
			//m_homePara.acc = 50000;
			//m_homePara.overtime = 10000;
			AppendHomeMode();
		}
		/// <summary>
		/// 加载卡,TRIO复位控制器功能=此处加载卡的功能，包括重启总线的，一般操作在Trio中操作
		/// </summary>
		/// <returns></returns>
		public bool Load()
		{
			try
			{
				trio.SetHost(IpAddress);
				if (!trio.IsOpen(PortId.EthernetREMOTE)) //判断程序是否与控制器之间建立了远程同步连接，没有则建立连接
				{
					trio.Open(PortType.Ethernet, PortId.EthernetREMOTE);// 打开程序与控制器的远程连接（同步模式）
				}
				if (!trio.IsOpen(PortId.EthernetREMOTE))
				{

					return false;
				}

				//如果需要触发trio重新扫描总线大复位，改为true。一般如果总线没有报错，上位机仅执行建立同步连接，不强行处理大复位。
				if (false)
				{
					try
					{
						if (trio.Execute("halt"))// 停止所有程序
						{
							return trio.Run("_20_STARTUP", 0);//运行引导程序，TRIO配合总线扫描入口程序
						}
						return true;
					}
					catch (Exception e)
					{
						return false;
					}

				}
				return true;
			}
			catch (Exception ex)
			{
				EventMgr.Error(ex.Message);
				return false;
			}
		}

		/// <summary>
		/// 控制卡初始化
		/// </summary>
		/// <param name="listDI">配置的输入IO集合</param>
		/// <param name="listDO">配置的输出IO集合</param>
		/// <returns></returns>
		public override bool Init(List<DiHandle> listDI, List<DoHandle> listDO)
		{
			try
			{
				//定义存放IO数组的长度
				arrayDI = new bool[listDI.Count];
				for (int i = 0; i < arrayDI.Length; i++)
				{
					arrayDI[i] = false;
				}

				arrayDO = new bool[listDO.Count];
				for (int i = 0; i < arrayDO.Length; i++)
				{
					arrayDO[i] = false;
				}

				//加载卡
				bEnable = Load();

				trio.SetVariable("WDOG", 1);
				trio.GetVariable("WDOG", out double value);

				if (value != 1)// 若设置失败，可能是EC总线异常，此时重新初始化EC
				{
					trio.Close();
					Thread.Sleep(100);
					EventMgr.Error("TRIO controller connection failed");
					return false;
				}
				if (bEnable)
				{
					if (this is IIo && (listDI.Count > 0 || listDO.Count > 0))
						InitIO();
					if (this is IMotion)
						InitMotion();
					bEnable = true;
					return true;
				}
				else
				{
					bEnable = false;
					return false;
				}
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				bEnable = false;
				EventMgr.Error(ex.Message);
				return false;

			}
		}

		/// <summary>
		/// 关闭控制卡
		/// </summary>
		/// <returns></returns>
		public override bool DeInit()
		{
			if (bEnable)
			{
				bEnable = false;

				try
				{
					trio.Close();
					m_bInited = false;
					instance = null;
					return true;
				}
				catch (Exception e)
				{
					return false;
				}
			}
			return false;
		}


		/// <summary>
		/// 根据EtherCAT站号，写EtherCAT从站的PDO数据
		/// </summary>
		/// <param name="nEcatID"></param>
		/// <param name="pdo"></param>
		/// <returns></returns>
		public override bool SetPDO(Int16 nEcatID, PdoData pdo)
		{
			return true;

			//pdo.nReturn = Imc60.IMC_SetEcatSlavePdoData((short)nCardIndex, nEcatID, pdo.nIndex, pdo.nSubIndex, pdo.data);
			//if (pdo.nReturn == Imc60.EXE_SUCCESS)
			//    return true;

			//EventMgr.Error($"SetEcatPDO()出错! result = {pdo.nReturn.ToString("x8")}");
			//return false;
		}

		/// <summary>
		/// 根据EtherCAT站号，读EtherCAT从站的PDO数据
		/// </summary>
		/// <param name="nEcatID"></param>
		/// <param name="pdo"></param>
		/// <returns></returns>
		public override bool GetPDO(Int16 nEcatID, ref PdoData pdo)
		{
			return true;

			//pdo.nReturn = Imc60.IMC_GetEcatSlavePdoData((short)nCardIndex, nEcatID, pdo.nIndex, pdo.nSubIndex, pdo.data);
			//if (pdo.nReturn == Imc60.EXE_SUCCESS)
			//    return true;

			//EventMgr.Error($"GetEcatPDO()出错! result = {pdo.nReturn.ToString("x8")}");
			//return false;
		}

		/// <summary>
		/// 根据EtherCAT站号，写EtherCAT从站的SDO数据
		/// </summary>
		/// <param name="nEcatID"></param>
		/// <param name="sdo"></param>
		/// <returns></returns>
		public override bool SetSDO(Int16 nEcatID, SdoData sdo)
		{
			return true;

			//sdo.nReturn = Imc60.IMC_SetEcatSlaveSdo((short)nCardIndex, nEcatID, sdo.nIndex, sdo.nSubIndex, sdo.data, (uint)sdo.data.Length, ref sdo.nAbortCode);
			//if (sdo.nReturn == Imc60.EXE_SUCCESS)
			//    return true;

			//EventMgr.Error($"SetEcatSDO()出错! result = {sdo.nReturn.ToString("x8")}");
			//return false;
		}

		/// <summary>
		/// 根据EtherCAT站号写EtherCAT从站的SDO数据
		/// </summary>
		/// <param name="nEcatID"></param>
		/// <param name="sdo"></param>
		/// <returns></returns>
		public override bool GetSDO(Int16 nEcatID, ref SdoData sdo)
		{
			return true;

			//uint nResultSize = 0;
			//sdo.nReturn = Imc60.IMC_GetEcatSlaveSdo((short)nCardIndex, nEcatID, sdo.nIndex, sdo.nSubIndex, sdo.data, (uint)sdo.data.Length, ref nResultSize, ref sdo.nAbortCode);
			//if (sdo.nReturn == Imc60.EXE_SUCCESS)
			//    return true;

			//EventMgr.Error($"GetEcatSDO()出错! result = {sdo.nReturn.ToString("x8")}");
			//return false;
		}


		#region Motion
		/// <summary>
		/// 根据轴号，获取 Ethcat 从站号
		/// </summary>
		/// <param name="nSlaveIndex"></param>
		/// <returns></returns>
		public bool GetEcatIndex(int nAxisNo, out Int16 nSlaveIndex)
		{
			nSlaveIndex = -1;
			return false;

			//nSlaveIndex = -1;
			//try
			//{
			//	if (!bEnable)
			//		return false;

			//	Int16 nSlotIndex = -1;
			//	uint nRtn = ImcApi.IMC_GetAxEcatStation(m_hCardHandle, (ushort)nAxisNo, ref nSlaveIndex, ref nSlotIndex);
			//	if (nRtn == ImcApi.EXE_SUCCESS)
			//		return true;

			//	EventMgr.Error($"GetEcatIndex()出错! result = {nRtn.ToString("x8")}");
			//	return false;
			//}
			//catch (Exception ex)
			//{
			//	EventMgr.Error(ex.Message);
			//	return false;
			//}
		}

		public Dictionary<string, HomeMode> dictHomeMode { get; set; } = new Dictionary<string, HomeMode>();

		public Dictionary<int, int[]> m_dicBoard { get; set; } = new Dictionary<int, int[]>();

		public Dictionary<int, int> m_dictCrdAxis { get; set; } = new Dictionary<int, int>();

		public void AppendHomeMode()
		{
			//ORG_P 23;ORG_N 27;PEL 18;MEL 17；EZ_PEL 34;EZ_MEL 33;ORG_P_EZ 7;ORG_N_EZ 11;PEL_EZ 2;MEL_EZ 1;
			dictHomeMode.Clear();
			//添加常用回原模式，如有不同可在Program更新或添加
			//HomeMode homeMode = new HomeMode { nHomeIndex = 23, strModeName = "ORG_P", strDescription = "正向回原，以原点信号为原点".txtTrans(TransType.Trans) };
			//DicHomeMode.Add(homeMode.strModeName, homeMode);
			//homeMode = new HomeMode { nHomeIndex = 27, strModeName = "ORG_N", strDescription = "负向回原，以原点信号为原点".txtTrans(TransType.Trans) };
			//DicHomeMode.Add(homeMode.strModeName, homeMode);
			//homeMode = new HomeMode { nHomeIndex = 18, strModeName = "PEL", strDescription = "正向回原，以正限位信号为原点".txtTrans(TransType.Trans) };
			//DicHomeMode.Add(homeMode.strModeName, homeMode);
			//homeMode = new HomeMode { nHomeIndex = 17, strModeName = "MEL", strDescription = "负向回原，以负限位信号为原点".txtTrans(TransType.Trans) };
			//DicHomeMode.Add(homeMode.strModeName, homeMode);
			//homeMode = new HomeMode { nHomeIndex = 34, strModeName = "EZ_PEL", strDescription = "正向回原，以EZ信号为原点".txtTrans(TransType.Trans) };
			//DicHomeMode.Add(homeMode.strModeName, homeMode);
			//homeMode = new HomeMode { nHomeIndex = 33, strModeName = "EZ_MEL", strDescription = "负向回原，以EZ信号为原点".txtTrans(TransType.Trans) };
			//DicHomeMode.Add(homeMode.strModeName, homeMode);
			//homeMode = new HomeMode { nHomeIndex = 7, strModeName = "ORG_P_EZ", strDescription = "正向回原，以原点信号为原点，带EZ".txtTrans(TransType.Trans) };
			//DicHomeMode.Add(homeMode.strModeName, homeMode);
			//homeMode = new HomeMode { nHomeIndex = 11, strModeName = "ORG_N_EZ", strDescription = "负向回原，以原点信号为原点，带EZ".txtTrans(TransType.Trans) };
			//DicHomeMode.Add(homeMode.strModeName, homeMode);
			//homeMode = new HomeMode { nHomeIndex = 2, strModeName = "PEL_EZ", strDescription = "正向回原，以正限位信号为原点，带EZ".txtTrans(TransType.Trans) };
			//DicHomeMode.Add(homeMode.strModeName, homeMode);
			//homeMode = new HomeMode { nHomeIndex = 1, strModeName = "MEL_EZ", strDescription = "负向回原，以负限位信号为原点，带EZ".txtTrans(TransType.Trans) };
			//DicHomeMode.Add(homeMode.strModeName, homeMode);

		}
		public bool InitMotion()
		{
			return true;
		}

		/// <summary>
		/// 给予使能
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <returns></returns>
		public bool ServoOn(int nAxisNo)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = trio.SetVariable("WDOG", 1);
				ret &= trio.GetVariable("WDOG", out double value);
				if (ret && value == 1)
				{
					ret = trio.SetAxisVariable("AXIS_ENABLE", nAxisNo, 1);
					if (!ret)
					{
						throw new AlarmException(AlarmType.Error, $"TRIO Card Axis "/*.txtTrans(TransType.Mark)*/ + $"{nAxisNo}" + " servo on Error");
					}
					return ret;
				}


				else
				{
					throw new AlarmException(AlarmType.Error, $"TRIO Card Axis "/*.txtTrans(TransType.Mark)*/ + $"{nAxisNo}" + " servo on Error");
				}
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		/// 断开使能
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <returns></returns>
		public bool ServoOff(int nAxisNo)
		{
			try
			{
				if (!bEnable)
					return false;
				bool ret = false;
				ret = trio.Cancel(2, nAxisNo);
				if (ret)
				{
					ret = trio.SetAxisVariable("AXIS_ENABLE", nAxisNo, 0);
					ret &= trio.GetAxisVariable("AXIS_ENABLE", nAxisNo, out double value);
					if (ret && value == 0)
						return true;
					return false;
				}
				else
					return false;


			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}
		}

		/// <summary>
		/// 读取伺服使能状态
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <returns></returns>
		public bool IsServoOn(int nAxisNo)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = false;
				ret = trio.GetAxisVariable("AXIS_ENABLE", nAxisNo, out double value);
				if (ret && value == 1)
					return true;
				else
					return false;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		/// 设置回原模式
		/// </summary>
		/// <param name="nAxisNo"></param>
		/// <param name="nMode"></param>
		/// <param name="vm"></param>
		/// <param name="vo"></param>
		/// <param name="acc"></param>
		/// <param name="dec"></param>
		/// <param name="offset"></param>
		/// <param name="sFac"></param>
		/// <returns></returns>
		public bool SetHomeMode(int nAxisNo, int nMode, double vm, double vo, double acc, double dec, double offset = 0, double sFac = 0)
		{
			if (!bEnable)
				return false;

			return true;
		}

		/// <summary>
		/// 回原点
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <param name="nMode">回原点参数, 对于8254，此参数代表回原点的方向</param>
		/// <returns></returns>
		public bool Home(int nAxisNo, int nMode)
		{

			return true;
			//try
			//{
			//    if (!bEnable)
			//        return false;

			//    if (IsHomeMode(nAxisNo) > 0)
			//    {
			//        ImcApi.IMC_FinishHoming(m_hCardHandle, (short)nAxisNo);
			//    }


			//    ImcApi.THomingPara homePara = m_homePara;

			//    homePara.homeMethod = (short)nMode;

			//    uint ret = ImcApi.IMC_StartHoming(m_hCardHandle, (short)nAxisNo, ref homePara);

			//    if (ret == ImcApi.EXE_SUCCESS)
			//    {
			//        Thread.Sleep(100);
			//        //short homingSts = 0;
			//        return true;
			//    }
			//    else
			//    {
			//        throw new AlarmException(AlarmType.Error, $"IMC30G-E Card Axis ".txtTrans(TransType.Mark) + $"{nAxisNo}" + " Home Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//    }
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    throw new AlarmException(AlarmType.Error, ex.Message);
			//}

		}

		/// <summary>
		/// 回原点
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <param name="nMode">模式</param>
		/// <param name="vm">最大速度</param>
		/// <param name="vo">最小速度</param>
		/// <param name="acc">加速时间</param>
		/// <param name="dec">减速时间</param>
		/// <param name="offset">原点偏移</param>
		/// <param name="sFac"></param>
		/// <returns></returns>
		public bool Home(int nAxisNo, int nMode, double vMax, double vMin, double acc, double dec, double offset = 0, double sFac = 0)
		{
			try
			{
				if (!bEnable)
					return false;



				bool ret = trio.DriveHoming(
					(short)nMode,      //回原方式（0-35种标准方式）。
					(short)offset,     //设置位置零点与机械原点之间的偏移量。轴回原完成后，实际位置等于设定值。
					(uint)vMax,          //设置寻找原点开关的速度
					(uint)vMin,          //设置寻找零点位置的速度
					(uint)(vMax / acc),  //设置回原动作中的加速度
					(short)nAxisNo);

				if (!ret)
				{
					throw new AlarmException(AlarmType.Error, $"TRIO Card Axis " + $"{nAxisNo}" + " Home Error,result = ");

				}
				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}
		}

		/// <summary>
		/// 位置对齐
		/// </summary>
		/// <param name="nAxisNo"></param>
		/// <param name="nMode"></param>
		/// <returns></returns>
		public bool PositionAlign(int nAxisNo)
		{
			if (!bEnable)
				return false;
			return true;
		}


		/// <summary>
		/// 退出回原模式
		/// </summary>
		/// <param name="nAxisNo"></param>
		/// <param name="nMode"></param>
		/// <returns></returns>
		public bool ExitHomeMode(int nAxisNo)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = false;
				ret = trio.Cancel(2, nAxisNo);

				if (!ret)
					throw new AlarmException(AlarmType.Error, $"TRIO Card IMC_FinishHoming Error");
				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}
		}

		/// <summary>
		/// 以绝对位置移动
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <param name="nPos">位置</param>
		/// <param name="nSpeed">速度</param>
		/// <returns></returns>
		public bool AbsMove(int nAxisNo, int nPos, int nSpeed)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = trio.SetAxisVariable("SPEED", nAxisNo, nSpeed);    // 设置轴速度
				ret &= trio.SetAxisVariable("ACCEL", nAxisNo, nSpeed * 10);   // 设置加速度
				ret &= trio.SetAxisVariable("DECEL", nAxisNo, nSpeed * 10);   // 设置减速度
				if (!ret)
				{
					throw new AlarmException(AlarmType.Error, $"TRIO Card IMC_FinishHoming Error");
					
				}

				double[] dist = new double[] { nPos };
				ret = trio.MoveAbs(dist, nAxisNo);//执行运动 
				if (!ret)
				{
					
					return false;
				}
				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		/// 以绝对位置移动
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <param name="fPos">位置</param>
		/// <param name="vm">速度</param>
		/// <param name="acc">加速时间</param>
		/// <param name="dec">减速时间</param>
		/// <param name="vs"></param>
		/// <param name="ve"></param>
		/// <param name="sFac">速度规划类型</param>
		/// <returns></returns>
		public bool AbsMove(int nAxisNo, double fPos, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0.99)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = trio.SetAxisVariable("SPEED", nAxisNo, vm);    // 设置轴速度
				ret &= trio.SetAxisVariable("ACCEL", nAxisNo, vm / acc);  // 设置加速度
				ret &= trio.SetAxisVariable("DECEL", nAxisNo, vm / dec);  // 设置减速度
				if (!ret)
				{					
					return false;
				}
				double[] dist = new double[] { fPos };
				ret = trio.MoveAbs(dist, nAxisNo);
				if (!ret)
				{
					
					return false;
				}
				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}
		}
		/// <summary>
		/// 以当前位置为起始点进行多轴直线插补
		/// </summary>
		/// <param name="nAxisArray">轴号数组</param>
		/// <param name="nPosArray">目标位置</param>
		/// <param name="vm">速度</param>
		/// <param name="acc">加速时间</param>
		/// <param name="dec">减速时间</param>
		/// <param name="vs"></param>
		/// <param name="ve"></param>
		/// <param name="sFac"></param>
		/// <returns></returns>
		public bool AbsLinearMove(ref int[] nAxisArray, ref double[] nPosArray, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0)
		{
			try
			{
				if (!bEnable)
					return false;
				
				double[] dbAixsArray = new double[nAxisArray.Length];
				for (int i = 0; i < nAxisArray.Length; i++)
				{
					dbAixsArray[i] = nAxisArray[i];
				}
				trio.Base(nAxisArray.Length, dbAixsArray);
				bool ret = trio.MoveAbs(nPosArray, nAxisArray[0]); //基准轴为轴0
				if (!ret)
				{
					
					return false;
				}
				return true;


			
			}
			catch (Exception ex)
			{
				EventMgr.Error(ex.Message);
				return false;
			}

		}

		/// <summary>
		/// 以当前位置为起始点进行多轴直线插补
		/// </summary>
		/// <param name="nAxisArray"></param>
		/// <param name="fPosOffsetArray"></param>
		/// <param name="vm"></param>
		/// <param name="acc"></param>
		/// <param name="dec"></param>
		/// <param name="vs"></param>
		/// <param name="ve"></param>
		/// <param name="sFac"></param>
		/// <returns></returns>
		public bool RelativeLinearMove(ref int[] nAxisArray, ref double[] fPosOffsetArray, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0)
		{
			try
			{
				if (!bEnable)
					return false;

				double[] dbAixsArray = new double[nAxisArray.Length];
				for (int i = 0; i < nAxisArray.Length; i++)
				{
					dbAixsArray[i] = nAxisArray[i];
				}
				trio.Base(nAxisArray.Length, dbAixsArray);
				bool ret = trio.MoveRel(fPosOffsetArray, nAxisArray[0]); //基准轴为轴0  
				if (!ret)
				{
					
					return false;
				}
				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				EventMgr.Error(ex.Message);
				return false;
			}

		}

		/// <summary>
		/// 以当前点为起点做圆弧插补运动
		/// </summary>
		/// <param name="nAxisArray"></param>
		/// <param name="fCenterArray"></param>
		/// <param name="fEndArray"></param>
		/// <param name="Dir"></param>
		/// <param name="vm"></param>
		/// <param name="acc"></param>
		/// <param name="dec"></param>
		/// <param name="vs"></param>
		/// <param name="ve"></param>
		/// <param name="sFac"></param>
		/// <returns></returns>
		public bool AbsArcMove(ref int[] nAxisArray, ref double[] fCenterArray, ref double[] fEndArray, int Dir, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0)
		{
			try
			{
				if (!bEnable)
					return false;

				if (nAxisArray.Length != 2)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion,
					//    string.Format("TRIO controller Aixs {0} AbsArcMove Error,Axis Number is over range", nAixsArray[0]));
					return false;
				}
				double[] dbAixsArray = new double[nAxisArray.Length];
				for (int i = 0; i < nAxisArray.Length; i++)
				{
					dbAixsArray[i] = nAxisArray[i];
				}
				trio.Base(nAxisArray.Length, dbAixsArray);

				double[] dbPos = new double[nAxisArray.Length];
				for (int i = 0; i < nAxisArray.Length; i++)
				{
					trio.GetAxisVariable("MPOS", nAxisArray[i], out dbPos[i]);
				}

				bool ret = trio.MoveCirc(fEndArray[0] - dbPos[0], fEndArray[1] - dbPos[1], fCenterArray[0], fCenterArray[1], Dir, nAxisArray[0]); // 轴0和轴1进行插补运动，分别运动距离5、10，基准轴为轴0
				if (!ret)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion,
					//    string.Format("TRIO controller Aixs {0} AbsArcMove Error,result = {1}", nAixsArray[0], ret.ToString()));
					return false;
				}
				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				EventMgr.Error(ex.Message);
				return false;
			}

		}

		/// <summary>
		/// 以当前点为起点做相对圆弧插补运动
		/// </summary>
		/// <param name="nAxisArray"></param>
		/// <param name="fCenterOffsetArray"></param>
		/// <param name="fEndArray"></param>
		/// <param name="Dir"></param>
		/// <param name="vm"></param>
		/// <param name="acc"></param>
		/// <param name="dec"></param>
		/// <param name="vs"></param>
		/// <param name="ve"></param>
		/// <param name="sFac"></param>
		/// <returns></returns>
		public bool RelativeArcMove(ref int[] nAxisArray, ref double[] fCenterOffsetArray, ref double[] fEndArray, int Dir, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0)
		{
			try
			{
				if (!bEnable)
					return false;

				if (nAxisArray.Length > 2)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion,
					//    string.Format("TRIO controller Aixs {0} RelativeArcMove Error,Axis Number is over rang", nAixsArray[0]));
					return false;
				}
				double[] dbAixsArray = new double[nAxisArray.Length];
				for (int i = 0; i < nAxisArray.Length; i++)
				{
					dbAixsArray[i] = nAxisArray[i];
				}
				trio.Base(nAxisArray.Length, dbAixsArray);//基准轴为轴0
				bool ret = trio.MoveCirc(fEndArray[0], fEndArray[1], fCenterOffsetArray[0], fCenterOffsetArray[1], Dir, nAxisArray[0]);
				if (!ret)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion,
					//    string.Format("TRIO controller Aixs {0} RelativeArcMove Error,result = {1}", nAxisArray[0], ret.ToString()));
					return false;
				}
				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				EventMgr.Error(ex.Message);
				return false;
			}

		}

		/// <summary>
		/// 初始化多轴插补运动
		/// </summary>
		/// <param name="nMtSysNo"></param>
		/// <param name="nAxisArray"></param>
		/// <param name="vm"></param>
		/// <param name="acc"></param>
		/// <returns></returns>
		protected bool InitMultMove(int nMtSysNo, int[] nAxisArray, double vm, double acc)
		{

			return false;
			//try
			//{
			//    if (!bEnable)
			//        return false;

			//    //轴映射 将插补轴0,1,2号轴映射到单轴号0,1,2上
			//    short[] pMaskAxNo = new short[3];
			//    for (int i = 0; i < 3; i++)
			//    {
			//        if (nAxisArray.Length > i)
			//        {
			//            pMaskAxNo[i] = (short)nAxisArray[i];
			//            ServoOn(nAxisArray[i]);
			//        }
			//        else
			//        {
			//            pMaskAxNo[i] = 31;
			//        }
			//    }

			//    //0号位建立插补坐标系，前瞻数3000段，5000.0的急停减加速度
			//    uint ret = ImcApi.IMC_CrdSetMtSys(m_hCardHandle, (short)nMtSysNo, pMaskAxNo, 3000, 500000.0);
			//    if ((ret & 0xffff) == 0x0075)
			//    {
			//        m_nMultInit = 1;
			//    }
			//    else if (ret == ImcApi.EXE_SUCCESS)
			//    {
			//        m_nMultInit = 1;
			//    }
			//    else
			//    {
			//        m_nMultInit = 0; ;
			//    }
			//    ret = ImcApi.IMC_CrdSetTrajVel(m_hCardHandle, (short)nMtSysNo, vm);
			//    ret = ImcApi.IMC_CrdSetTrajAcc(m_hCardHandle, (short)nMtSysNo, acc);//设置插补进给速度和加速度
			//    ret = ImcApi.IMC_CrdSetZeroFlag(m_hCardHandle, (short)nMtSysNo, 0);  //每段末速度不强制为0
			//    ret = ImcApi.IMC_CrdSetIncMode(m_hCardHandle, (short)nMtSysNo, 0); //插补编程方式为绝对编程方式
			//    ImcApi.TCrdAdvParam CrdAdvParam = new ImcApi.TCrdAdvParam();
			//    CrdAdvParam.userVelMode = 0;  //系统规划模式
			//    CrdAdvParam.transMode = 1; //过渡模式
			//    CrdAdvParam.noDataProtect = 0; //数据断流无保护
			//    CrdAdvParam.noCoplaneCircOptm = 0; //异面过渡无处理
			//    CrdAdvParam.turnCoef = 1.0; //拐角系数1.0
			//    CrdAdvParam.tol = 0.1; //轨迹精度0.1 unit
			//    ret = ImcApi.IMC_CrdSetAdvParam(m_hCardHandle, (short)nMtSysNo, ref CrdAdvParam);  //设置插补高级参数
			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        EventMgr.Error($"IMC30G-E Card IMC_CrdSetAdvParam Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//        return false;
			//    }

			//    return true;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return false;
			//}

		}

		/// <summary>
		/// 相对位置移动
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <param name="nPos">位置</param>
		/// <param name="nSpeed">速度</param>
		/// <returns></returns>
		public bool RelativeMove(int nAxisNo, int nPos, int nSpeed)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = trio.SetAxisVariable("SPEED", nAxisNo, nSpeed);    // 设置轴速度
				ret &= trio.SetAxisVariable("ACCEL", nAxisNo, nSpeed * 10);   // 设置加速度
				ret &= trio.SetAxisVariable("DECEL", nAxisNo, nSpeed * 10);   // 设置减速度
				if (!ret)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion_SetParam,
					//    string.Format("TRIO controller Aixs {0} set single axis moving parameter Error,result = {1}", nAxisNo, ret.ToString()));
					return false;
				}

				double[] dist = new double[] { nPos };
				ret = trio.MoveRel(dist, nAxisNo); //执行运动 
				if (!ret)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Rel,
					//    string.Format("TRIO controller Aixs {0} abs move Error,result = {1}", nAxisNo, ret.ToString()));
					return false;
				}
				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		/// 相对位置移动
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <param name="fOffset">相对位置</param>
		/// <param name="vm">速度</param>
		/// <param name="acc">加速时间</param>
		/// <param name="dec">减速时间</param>
		/// <param name="vs"></param>
		/// <param name="ve"></param>
		/// <param name="sFac"></param>
		/// <returns></returns>
		public bool RelativeMove(int nAxisNo, double fOffset, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0.99)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = trio.SetAxisVariable("SPEED", nAxisNo, vm);    // 设置轴速度
				ret &= trio.SetAxisVariable("ACCEL", nAxisNo, vm / acc);  // 设置加速度
				ret &= trio.SetAxisVariable("DECEL", nAxisNo, vm / dec);  // 设置减速度
				if (!ret)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion_SetParam,
					//    string.Format("TRIO controller Aixs {0} set single axis moving parameter Error,result = {1}", nAxisNo, ret.ToString()));
					return false;
				}

				double[] dist = new double[] { fOffset };
				ret = trio.MoveRel(dist, nAxisNo); //执行运动 
				if (!ret)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Rel,
					//    string.Format("TRIO controller Aixs {0} abs move Error,result = {1}", nAxisNo, ret.ToString()));
					return false;
				}
				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}
		}

		/// <summary>
		/// JOG运动
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <param name="bPositive">方向</param>
		/// <param name="bStart">开始标志</param>
		/// <param name="nSpeed">速度</param>
		/// <returns></returns>
		public bool JogMove(int nAxisNo, bool bPositive, int bStart, int nSpeed)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = trio.SetAxisVariable("SPEED", nAxisNo, nSpeed);    // 设置轴速度
				ret &= trio.SetAxisVariable("ACCEL", nAxisNo, nSpeed * 10);   // 设置加速度
				ret &= trio.SetAxisVariable("DECEL", nAxisNo, nSpeed * 10);   // 设置减速度
				if (!ret)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion_SetParam,
					//    string.Format("TRIO controller Aixs {0} set single axis moving parameter Error,result = {1}", nAxisNo, ret.ToString()));
					return false;
				}

				if (bPositive)
					ret = trio.Forward(nAxisNo); //设置正向JOG运动的输入
				else
					ret = trio.Reverse(nAxisNo); //设置正向JOG运动的输入
				if (!ret)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Jog,
					//    string.Format("TRIO controller Aixs {0} start jog Error,result = {1}", nAxisNo, ret.ToString()));
					return false;
				}
				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		/// 轴正常停止
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <returns></returns>
		public bool StopAxis(int nAxisNo)
		{
			return StopEmg(nAxisNo);

		}

		/// <summary>
		/// 急停
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <returns></returns>
		public bool StopEmg(int nAxisNo)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = trio.Cancel(2, nAxisNo);

				if (!ret)
				{

				}


				return ret;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}


		/// <summary>
		/// 获取轴卡运动IO信号
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <returns></returns>
		public long GetAxisDI(int nAxisNo)
		{

			try
			{
				if (!bEnable)
					return -1;

				// |-bit0-|--1--|--2--|--3--|--4--|--5--|--6--|--7--|--8--|...|--11--|--12--|
				// |-ALM--|-PEL-|-MEL-|-ORG-|-EMG-|-EZ--|-INP-|-SVO-|-RDY-|...|-SPEL-|-SMEL-|
				bool ret = true;
				double Wdog = 0.0, axSta = 0.0, Mtype = 0.0, Idle = 0.0, Fe = 0.0, Datumln = 0.0, AxisEnable = 0.0, lnVal = 0.0, dE_Stop = 0.0;
				uint RetSta = 0;
				ret = ret ? trio.GetVariable("WDOG", out Wdog) : false;
				ret = ret ? trio.GetAxisVariable("AXISSTATUS", nAxisNo, out axSta) : false;
				ret = ret ? trio.GetAxisVariable("MTYPE", nAxisNo, out Mtype) : false;
				ret = ret ? trio.GetAxisVariable("IDLE", nAxisNo, out Idle) : false;
				ret = ret ? trio.GetAxisVariable("FE", nAxisNo, out Fe) : false;
				ret = ret ? trio.GetAxisVariable("DATUM_IN", nAxisNo, out Datumln) : false;
				ret = ret ? (Datumln >= 0 ? trio.In((int)Datumln, (int)Datumln, out lnVal) : true) : false;

				//ret = TRIOEcatCard.Instance().trio.In(128, 128, out dE_Stop);
				//ret = ret ? (dE_Stop > 0 ? false : true) : false;

				ret = ret ? trio.GetAxisVariable("AXIS_ENABLE", nAxisNo, out AxisEnable) : false;

				if (!ret)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion_State,
					//    string.Format("TRIO controller Aixs {0} Get Status Error,result = {1}", nAxisNo, ret.ToString()));
					return -1;
				}
				RetSta |= (uint)(((((uint)axSta & (1 << 3)) != 0) ? 1 : 0) << 0); //驱动器报警
				RetSta |= (uint)(((((uint)axSta & (1 << 4)) != 0) ? 1 : 0) << 1); //正限位
				RetSta |= (uint)(((((uint)axSta & (1 << 5)) != 0) ? 1 : 0) << 2); //负限位
				RetSta |= (uint)(((lnVal == 0) ? 1 : 0) << 3); //原点
				RetSta |= (uint)(0);         //急停            
				RetSta |= (uint)(0);
				RetSta |= (uint)(((Idle != 0 && Fe <= 0.05) ? 1 : 0) << 6);      //到位信号
				RetSta |= (uint)(((Wdog != 0 && AxisEnable != 0) ? 1 : 0) << 7); //轴使能
				RetSta |= (uint)(0);
				RetSta |= (uint)(((Idle == 0) ? 1 : 0) << 9);    //运动中
				RetSta |= (uint)(((Mtype == 12) ? 1 : 0) << 10); //回零中
				RetSta |= (uint)(((((uint)axSta & (1 << 9)) != 0) ? 1 : 0) << 11);  //驱动器报警
				RetSta |= (uint)(((((uint)axSta & (1 << 10)) != 0) ? 1 : 0) << 12); //驱动器报警
				RetSta |= (uint)(((((uint)axSta & (1 << 7)) != 0) ? 1 : 0) << 13);  //驱动器报警
				long nStdIo = RetSta;
				return nStdIo;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		/// 清除错误报警
		/// </summary>
		/// <param name="nAxisNo"></param>
		/// <returns></returns>
		public bool ResetAlarm(int nAxisNo)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = trio.Execute(string.Format("ETHERCAT($64,{0}", nAxisNo));
				Thread.Sleep(10);

				ret &= trio.Datum(7, nAxisNo);
				Thread.Sleep(10);

				ret &= trio.Execute("UNIT_CLEAR");
				Thread.Sleep(10);

				ret &= trio.Datum(0, nAxisNo);
				Thread.Sleep(10);

				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		/// 获取轴的当前位置
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <returns></returns>
		public double GetPos(int nAxisNo)
		{
			try
			{
				if (!bEnable)
					return -1;

				double[] pPrfPos = new double[1];
				bool ret = trio.GetAxisVariable("MPOS", nAxisNo, out pPrfPos[0]);
				if (!ret)
				{
					return -1;
				}

				return (long)pPrfPos[0];
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		/// 轴是否正常停止
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <returns>0:正常停止, -1:未到位 其它:急停,报警等</returns>
		public AxisHandle.StopType IsStop(int nAxisNo)
		{

			try
			{
				if (!bEnable)
					return AxisHandle.StopType.Moving;
				double axStatus = -1;
				bool ret = trio.GetAxisVariable("AXISSTATUS", nAxisNo, out axStatus);
				if (!ret)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion_State,
					//    string.Format("TRIO controller Aixs {0} Get Status Error,result = {1}", nAxisNo, ret.ToString()));
					return AxisHandle.StopType.Moving;
				}
				int axis0State = (int)axStatus;
				if ((axis0State & (0x01 << 3)) != 0)   //驱动器报警
				{
					Debug.WriteLine("Axis {0} have sevo alarm signal \r\n", nAxisNo);
					return AxisHandle.StopType.Alarm;
				}
				if ((axis0State & (0x01 << 4)) != 0)   //正限位
				{
					Debug.WriteLine("Axis {0} have PEL signal \r\n", nAxisNo);
					return AxisHandle.StopType.PEL;
				}
				if ((axis0State & (0x01 << 5)) != 0)   //负限位
				{
					Debug.WriteLine("Axis {0} have MEL signal \r\n", nAxisNo);
					return AxisHandle.StopType.MEL;
				}
				if ((axis0State & (0x01 << 9)) != 0)   //软正限位
				{
					Debug.WriteLine("Axis {0} have SPEL signal \r\n", nAxisNo);
					return AxisHandle.StopType.PEL;
				}
				if ((axis0State & (0x01 << 10)) != 0)  //负正限位
				{
					Debug.WriteLine("Axis {0} have SMEL signal \r\n", nAxisNo);
					return AxisHandle.StopType.MEL;
				}

				trio.GetVariable("DRIVE_STATUS", out double value);
				int nValue = (int)0x27;
				if ((nValue & 0x2F) != 0x27)
				{
					Debug.WriteLine("Axis {0} have servo signal \r\n", nAxisNo);
					return AxisHandle.StopType.ServoOff;
				}

				ret = trio.GetAxisVariable("IDLE", nAxisNo, out axStatus); //-1 - IDLE
				if (ret)
					ret = axStatus == -1;
				if (!ret)
					return AxisHandle.StopType.Moving;
				else
					return AxisHandle.StopType.NormalStop;

			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		/// 判断轴是否到位（使用传入的目标点位）
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <param name="nTarget">目标点位</param>
		/// <param name="nInPosError">到位误差</param>
		/// <returns></returns>
		public AxisHandle.StopType IsInPos(int nAxisNo, int nTarget, int nInPosError)
		{
			try
			{
				if (!bEnable)
					return AxisHandle.StopType.Moving;

                AxisHandle.StopType nRet = IsStop(nAxisNo);
				if (nRet == AxisHandle.StopType.Moving)
				{
					int count = 0;
					while (true)
					{
						double[] pEncPos = new double[1];

						trio.GetAxisVariable("MPOS", nAxisNo, out double offsetPos1);
						trio.GetAxisVariable("FE", nAxisNo, out double offsetPos2);
					
						if (Math.Abs(offsetPos1 - nTarget) < nInPosError )
							break;
						count++;
						if (count > 10)
							return AxisHandle.StopType.ErrPos;  //轴停止后位置超限
						Thread.Sleep(1);
					}
				}
				return nRet;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		/// 位置清零
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <returns></returns>
		public bool SetPosZero(int nAxisNo)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = trio.Defpos(0, nAxisNo);  //编码器未清零,待换新固件

				if (!ret)
					return false;

				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}


		/// <summary>
		/// 设置单轴的某一运动参数
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <param name="nParam">参数:1:回原加速度 2:回原最低速度 3:回原最低速度 4:回原最大速度 5:平滑时间(固高卡S曲线) 其它：自定义扩展</param>
		/// <param name="nData">参数值</param>
		/// <returns></returns>
		public bool SetAxisParam(int nAxisNo, int nParam, int nData)
		{
			try
			{
				if (!bEnable)
					return false;

				switch (nParam)
				{
					case 1:
						//m_homePara.acc = (uint)nData;
						trio.SetAxisVariable("ACCEL", nAxisNo, nData); //设置加速度

						break;
					case 2:

						trio.SetAxisVariable("DECEL", nAxisNo, nData); //设置减速度
						break;

					case 3:
						trio.SetAxisVariable("CREEP", nAxisNo, nData); //设置爬行速度
						break;

					case 4:
						trio.SetAxisVariable("SPEED", nAxisNo, nData); //设置轴速度
						break;

					default:
						return false;
				}

				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		/// 设置单轴的某一运动参数(浮点)
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <param name="nParam">参数:1:加速度 2:减速度 3:起跳速度 4:结束速度(凌华卡) 5:平滑时间(固高卡S曲线) 其它：自定义扩展</param>
		/// <param name="nData">参数值</param>
		/// <returns></returns>
		public bool SetAxisParam(int nAxisNo, int nParam, double fData)
		{
			try
			{
				if (!bEnable)
					return false;

				switch (nParam)
				{
					case 1:
						//m_homePara.acc = (uint)nData;
						trio.SetAxisVariable("ACCEL", nAxisNo, fData); //设置加速度

						break;
					case 2:

						trio.SetAxisVariable("DECEL", nAxisNo, fData); //设置减速度
						break;

					case 3:
						trio.SetAxisVariable("CREEP", nAxisNo, fData); //设置爬行速度
						break;

					case 4:
						trio.SetAxisVariable("SPEED", nAxisNo, fData); //设置轴速度
						break;

					default:
						return false;
				}

				return true;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		/// 速度模式旋转轴
		/// </summary>
		/// <param name="nAxisNo"></param>
		/// <param name="dSpeed"></param>
		/// <returns></returns>
		public bool VelocityMove(int nAxisNo, double dSpeed)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = trio.SetAxisVariable("SPEED", nAxisNo, dSpeed);    // 设置轴速度
				ret &= trio.SetAxisVariable("ACCEL", nAxisNo, dSpeed * 10);   // 设置加速度
				ret &= trio.SetAxisVariable("DECEL", nAxisNo, dSpeed * 10);   // 设置减速度
				if (!ret)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion_SetParam,
					//    string.Format("TRIO controller Aixs {0} set single axis moving parameter Error,result = {1}", nAxisNo, ret.ToString()));
					return false;
				}
				ret = trio.Forward(nAxisNo);
				if (!ret)
				{
					//WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Vel,
					//    string.Format("TRIO controler Aixs {0} start Velocity Move Error,result = {1}", nAxisNo, ret.ToString()));
					return false;
				}
				return true;
				//Thread.Sleep(100);

			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		///回原是否正常停止
		/// </summary>
		/// <param name="nAxisNo"></param>
		/// <returns></returns>
		public AxisHandle.StopType IsHomeStop(int nAxisNo)
		{
			try
			{
				if (!bEnable)
					return AxisHandle.StopType.Moving;
				trio.GetAxisVariable("DRIVE_HOMING_STATUS", nAxisNo, out double axStatus);
				if (axStatus == 8 /*&& IsAxisInPos(nAxisNo, 0.01) == 0*/)
				{
					#region 启用正负限位
					//int PELSign = -1, MELSign = -1;
					//switch (nAxisNo)
					//{
					//	case 2:  //镭射Y轴
					//		PELSign = 65; MELSign = 64;
					//		break;
					//	case 8:  //点胶Y轴
					//		PELSign = 121; MELSign = 120;
					//		break;
					//	default:
					//		break;
					//}
					//SetTrioPEL(nAxisNo, PELSign);
					//SetTrioMEL(nAxisNo, MELSign);
					#endregion
					return AxisHandle.StopType.NormalStop;
				}
				return AxisHandle.StopType.Moving;

			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}
		}
		/// <summary>
		/// 是否处于回原点模式
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <returns>1：处于回原点模式；其他：不是</returns>
		public int IsHomeMode(int nAxisNo)
		{
			return -1;
			//try
			//{
			//    if (!bEnable)
			//        return -1;

			//    short[] nctrlModel = new short[1];
			//    uint ret = ImcApi.IMC_GetAxPrfMode(m_hCardHandle, (short)nAxisNo, nctrlModel);
			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {

			//        return -2;
			//    }
			//    int m_ctrlModel = nctrlModel[0] & 0x0f;
			//    if (m_ctrlModel != 15)
			//    {
			//        return -1;
			//    }
			//    else
			//    {
			//        return 1;
			//    }
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    throw new AlarmException(AlarmType.Error, ex.Message);
			//}

		}

		/// <summary>
		/// 插补运动是否停止
		/// </summary>
		/// <param name="nMtSysNo">坐标系号</param>
		/// <returns>0：停止  -1：未完成</returns>
		public int IsMultMoveStop(int nMtSysNo)
		{

			return -1;
			//try
			//{
			//    if (!bEnable)
			//        return -1;

			//    short sts = 0;
			//    uint ret = ImcApi.IMC_CrdGetArrivalSts(m_hCardHandle, (short)nMtSysNo, ref sts);

			//    if (ret == ImcApi.EXE_SUCCESS)
			//    {
			//        if (sts == 1)
			//        {
			//            ImcApi.IMC_CrdStop(m_hCardHandle, (short)nMtSysNo, 0);
			//            ImcApi.IMC_CrdDeleteMtSys(m_hCardHandle, (short)nMtSysNo);
			//            m_nMultInit = 0;
			//            return 0;
			//        }
			//    }

			//    return -1;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return -1;
			//}

		}

		/// <summary>
		/// 是否处于多轴插补运动模式
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <returns>1：多轴插补运动模式；其他：不是</returns>
		public int IsMultMoveMode(int nAxisNo)
		{
			return -1;
			//try
			//{
			//    if (!bEnable)
			//        return -1;

			//    short[] nctrlModel = new short[1];
			//    uint ret = ImcApi.IMC_GetAxPrfMode(m_hCardHandle, (short)nAxisNo, nctrlModel);
			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        return -2;
			//    }
			//    int m_ctrlModel = (nctrlModel[0] & 0x70) >> 4;
			//    if (m_ctrlModel == 0x01)
			//    {
			//        return 1;
			//    }
			//    else
			//    {
			//        return -1;
			//    }
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return -1;
			//}

		}

		/// <summary>
		/// 是否处于多轴插补运动模式
		/// </summary>
		/// <param name="nAxisNo"></param>
		/// <returns></returns>
		public int IsImmediateMoveMode(int nAxisNo)
		{
			return -1;
			//try
			//{
			//    if (!bEnable)
			//        return -1;

			//    short[] nPrfModel = new short[1];
			//    uint ret = ImcApi.IMC_GetAxPrfMode(m_hCardHandle, (short)nAxisNo, nPrfModel);
			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        return -2;
			//    }

			//    //当启动进入多轴插补立即运动时，所查询的轴模式值为12。 
			//    if (nPrfModel[0] == 12)
			//    {
			//        return 1;
			//    }
			//    else
			//    {
			//        return -1;
			//    }
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return -1;
			//}

		}


		/// <summary>
		/// 配置连续插补运动，最多只支持三轴
		/// </summary>
		/// <param name="crdNo">坐标系</param>
		/// <param name="nAxisArray">参与插补运动的轴</param>
		/// <param name="bAbsolute">true:绝对位置模式，　false:相对位置模式</param>
		/// <returns></returns>
		public bool ConfigPointTable(int crdNo, ref int[] nAxisArray, bool bAbsolute)
		{
			return false;
			//try
			//{
			//    if (!bEnable)
			//        return false;

			//    //轴映射 将插补轴0,1,2号轴映射到单轴号0,1,2上
			//    short[] pMaskAxNo = new short[3];
			//    for (int i = 0; i < 3; i++)
			//    {
			//        if (nAxisArray.Length > i)
			//        {
			//            pMaskAxNo[i] = (short)nAxisArray[i];
			//            ServoOn(nAxisArray[i]);

			//            if (m_dictCrdAxis.ContainsKey(nAxisArray[i]))
			//            {
			//                m_dictCrdAxis[nAxisArray[i]] = crdNo;
			//            }
			//            else
			//            {
			//                m_dictCrdAxis.Add(nAxisArray[i], crdNo);
			//            }
			//        }
			//        else
			//        {
			//            //虚轴配-1避免多个插补同时进行，配的虚拟轴被占用的问题
			//            pMaskAxNo[i] = -1;
			//        }
			//    }

			//    //插补坐标系已经建立,先删除
			//    uint ret = ImcApi.IMC_CrdDeleteMtSys(m_hCardHandle, (short)crdNo);

			//    //0号位建立插补坐标系，前瞻数3000段，5000.0的急停减加速度
			//    ret = ImcApi.IMC_CrdSetMtSys(m_hCardHandle, (short)crdNo, pMaskAxNo, 3000, 500000.0);
			//    if ((ret & 0xffff) == 0x0075)
			//    {
			//        //插补坐标系已经建立,先删除
			//        ret = ImcApi.IMC_CrdDeleteMtSys(m_hCardHandle, (short)crdNo);

			//        //Thread.Sleep(100);

			//        ret = ImcApi.IMC_CrdSetMtSys(m_hCardHandle, (short)crdNo, pMaskAxNo, 3000, 500000.0);
			//    }

			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        EventMgr.Error($"IMC30G-E Card IMC_CrdSetMtSys Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//        return false;
			//    }

			//    //清除错误
			//    ret = ImcApi.IMC_CrdClrError(m_hCardHandle, (short)crdNo);

			//    //清除缓存
			//    ret = ImcApi.IMC_CrdClrData(m_hCardHandle, (short)crdNo);

			//    if (bAbsolute)
			//    {
			//        ret = ImcApi.IMC_CrdSetIncMode(m_hCardHandle, (short)crdNo, 0); //插补编程方式为绝对编程方式
			//    }
			//    else
			//    {
			//        ret = ImcApi.IMC_CrdSetIncMode(m_hCardHandle, (short)crdNo, 1); //插补编程方式为绝对编程方式
			//    }


			//    ImcApi.TCrdAdvParam CrdAdvParam = new ImcApi.TCrdAdvParam();
			//    CrdAdvParam.userVelMode = 0;  //系统规划模式
			//    CrdAdvParam.transMode = 1; //过渡模式
			//    CrdAdvParam.noDataProtect = 0; //数据断流无保护
			//    CrdAdvParam.noCoplaneCircOptm = 0; //异面过渡无处理
			//    CrdAdvParam.turnCoef = 1.0; //拐角系数1.0
			//    CrdAdvParam.tol = 0.1; //轨迹精度0.1 unit
			//    ret = ImcApi.IMC_CrdSetAdvParam(m_hCardHandle, (short)crdNo, ref CrdAdvParam);  //设置插补高级参数
			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        EventMgr.Error($"IMC30G-E Card IMC_CrdSetAdvParam Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//        return false;
			//    }


			//    int[] ints = new int[nAxisArray.Length];
			//    for (int i = 0; i < nAxisArray.Length; i++)
			//    {
			//        ints[i] = nAxisArray[i];
			//    }

			//    if (m_dicBoard.ContainsKey(crdNo))
			//        m_dicBoard[crdNo] = ints;
			//    else
			//        m_dicBoard.Add(crdNo, ints);


			//    return true;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return false;
			//}

		}

		/// <summary>
		/// 连续直线插补运动
		/// </summary>
		/// <param name="crdNo">坐标系</param>
		/// <param name="positionArray">运动点位</param>
		/// <param name="acc">加速度</param>
		/// <param name="dec">减速度</param>
		/// <param name="vs">开始速度</param>
		/// <param name="vm">运行速度</param>
		/// <param name="ve">结束速度</param>
		/// <param name="sf">平滑系数</param>
		/// <returns></returns>
		public bool PointTable_Line_Move(int crdNo, ref double[] positionArray, double acc, double dec, double vs, double vm, double ve, double sf)
		{
			return false;
			//try
			//{
			//    if (!bEnable)
			//        return false;

			//    //判断点位长度和参数插补运动的轴数是否相等
			//    if (m_dicBoard[crdNo].Length != positionArray.Length)
			//    {
			//        EventMgr.Error($"IMC30G-E Card MtSys ".txtTrans(TransType.Mark) + $"{crdNo}" + " Dimension error,PointTable_Line_Move".txtTrans(TransType.Mark));
			//        return false;
			//    }

			//    //设置速度，加速度

			//    //通过加减速时间计算加减速度
			//    //acc = vm / acc;
			//    //dec = vm / dec;

			//    //设置运行速度
			//    uint ret = ImcApi.IMC_CrdSetTrajVel(m_hCardHandle, (short)crdNo, vm);

			//    //设置加速度和减速度
			//    ret = ImcApi.IMC_CrdSetTrajAccAndDec(m_hCardHandle, (short)crdNo, acc, dec);

			//    //设置开始速度,结束速度
			//    ret = ImcApi.IMC_CrdUserVelPlan(m_hCardHandle, (short)crdNo, vs, ve);

			//    //设置平滑系数
			//    ret = ImcApi.IMC_CrdSetSmoothParam(m_hCardHandle, (short)crdNo, (int)sf, sf - (int)sf);

			//    //插入运动点位
			//    if (m_dicBoard[crdNo].Length == 3)
			//    {
			//        //三轴插补
			//        ret = ImcApi.IMC_CrdLineXYZ(m_hCardHandle, (short)crdNo, positionArray);

			//        if (ret != ImcApi.EXE_SUCCESS)
			//        {
			//            EventMgr.Error($"IMC30G-E Card IMC_CrdLineXYZ Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//            return false;
			//        }
			//    }
			//    else
			//    {
			//        //二轴插补，默认为XY
			//        ret = ImcApi.IMC_CrdLineXY(m_hCardHandle, (short)crdNo, positionArray);

			//        if (ret != ImcApi.EXE_SUCCESS)
			//        {
			//            EventMgr.Error($"IMC30G-E Card IMC_CrdLineXY Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//            return false;
			//        }
			//    }

			//    //把数据压入队列
			//    short IsFinished = new short();
			//    ret = ImcApi.IMC_CrdEndData(m_hCardHandle, (short)crdNo, ref IsFinished);  //把PC FIFO中的线段送入板卡FIFO中
			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        EventMgr.Error($"IMC30G-E Card IMC_CrdEndData Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//        return false;
			//    }

			//    return true;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return false;
			//}

		}


		public bool PointTable_ArcE_Move(int crdNo, ref double[] centerArray, ref double[] endArray, short dir, double acc, double dec, double vs, double vm, double ve, double sf)
		{
			return false;
			//if (!bEnable)
			//    return false;

			//try
			//{
			//    //判断点位长度和参数插补运动的轴数是否相等
			//    if (m_dicBoard[crdNo].Length != centerArray.Length ||
			//        m_dicBoard[crdNo].Length != 2)
			//    {
			//        EventMgr.Error($"IMC30G-E Card MtSys ".txtTrans(TransType.Mark) + $"{crdNo}" + " Dimension error,PointTable_ArcE_Move".txtTrans(TransType.Mark));
			//        return false;
			//    }

			//    //设置速度，加速度

			//    //通过加减速时间计算加减速度
			//    //acc = vm / acc;
			//    //dec = vm / dec;

			//    //设置运行速度
			//    uint ret = ImcApi.IMC_CrdSetTrajVel(m_hCardHandle, (short)crdNo, vm);

			//    //设置加速度和减速度
			//    ret = ImcApi.IMC_CrdSetTrajAccAndDec(m_hCardHandle, (short)crdNo, acc, dec);

			//    //设置开始速度,结束速度
			//    ret = ImcApi.IMC_CrdUserVelPlan(m_hCardHandle, (short)crdNo, vs, ve);

			//    //设置平滑系数
			//    ret = ImcApi.IMC_CrdSetSmoothParam(m_hCardHandle, (short)crdNo, (int)sf, sf - (int)sf);

			//    //XY平面内圆心末点编程。Z轴不作圆周运动
			//    if (dir == 0)
			//    {
			//        dir = -1;//顺时针
			//    }
			//    else
			//        dir = 1;//逆时针
			//    ret = ImcApi.IMC_CrdArcCenterXYPlane(m_hCardHandle, (short)crdNo, centerArray, endArray, dir);
			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        EventMgr.Error($"IMC30G-E Card IMC_CrdArcCenterXYPlane Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//        return false;
			//    }

			//    //把数据压入队列
			//    short IsFinished = new short();
			//    ret = ImcApi.IMC_CrdEndData(m_hCardHandle, (short)crdNo, ref IsFinished);  //把PC FIFO中的线段送入板卡FIFO中
			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        EventMgr.Error($"IMC30G-E Card IMC_CrdEndData Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//        return false;
			//    }

			//    return true;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return false;
			//}
		}

		public bool PointTable_ArcE_Move(int crdNo, double radius, ref double[] endArray, short dir, double acc, double dec, double vs, double vm, double ve, double sf)
		{
			return false;
			//if (!bEnable)
			//    return false;

			//try
			//{
			//    //判断点位长度和参数插补运动的轴数是否相等
			//    if (m_dicBoard[crdNo].Length != endArray.Length ||
			//        m_dicBoard[crdNo].Length != 2)
			//    {
			//        EventMgr.Error($"IMC30G-E Card MtSys ".txtTrans(TransType.Mark) + $"{crdNo}" + " Dimension error,PointTable_ArcE_Move".txtTrans(TransType.Mark));
			//        return false;
			//    }

			//    //设置速度，加速度

			//    //通过加减速时间计算加减速度
			//    //acc = vm / acc;
			//    //dec = vm / dec;

			//    //设置运行速度
			//    uint ret = ImcApi.IMC_CrdSetTrajVel(m_hCardHandle, (short)crdNo, vm);

			//    //设置加速度和减速度
			//    ret = ImcApi.IMC_CrdSetTrajAccAndDec(m_hCardHandle, (short)crdNo, acc, dec);

			//    //设置开始速度,结束速度
			//    ret = ImcApi.IMC_CrdUserVelPlan(m_hCardHandle, (short)crdNo, vs, ve);

			//    //设置平滑系数
			//    ret = ImcApi.IMC_CrdSetSmoothParam(m_hCardHandle, (short)crdNo, (int)sf, sf - (int)sf);

			//    //XY平面内半径末点编程。Z轴不作圆周运动
			//    ret = ImcApi.IMC_CrdArcRadiusXYPlane(m_hCardHandle, (short)crdNo, radius, endArray, dir);
			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        EventMgr.Error($"IMC30G-E Card IMC_CrdArcRadiusXYPlane Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//        return false;
			//    }

			//    //把数据压入队列
			//    short IsFinished = new short();
			//    ret = ImcApi.IMC_CrdEndData(m_hCardHandle, (short)crdNo, ref IsFinished);  //把PC FIFO中的线段送入板卡FIFO中
			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        EventMgr.Error($"IMC30G-E Card IMC_CrdEndData Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//        return false;
			//    }

			//    return true;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return false;
			//}
		}

		/// <summary>
		/// 插补缓冲区输出DO,在插入位置之后插入
		/// </summary>
		/// <param name="crdNo">坐标系号</param>
		/// <param name="nChannel">输出DO的端口号</param>
		/// <param name="bOn">输出DO的电平，0低电平，1 高电平</param>
		/// <returns></returns>
		public bool PointTable_IO(int crdNo, int nChannel, int bOn)
		{

			return false;
			//try
			//{
			//    if (!bEnable)
			//        return false;

			//    uint ret = ImcApi.IMC_CrdSetDO(m_hCardHandle, (short)crdNo, (short)nChannel, 0, (short)bOn);

			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        EventMgr.Error($"IMC30G-E Card IMC_CrdSetDO Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//        return false;
			//    }

			//    return true;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return false;
			//}

		}

		public bool PointTable_Delay(int crdNo, int nMillsecond)
		{
			return false;
			//try
			//{
			//    if (!bEnable)
			//        return false;

			//    uint ret = ImcApi.IMC_CrdWaitTime(m_hCardHandle, (short)crdNo, nMillsecond);
			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        EventMgr.Error($"IMC30G-E Card IMC_CrdWaitTime Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//        return false;
			//    }

			//    return true;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return false;
			//}

		}

		/// <summary>
		/// 确定连续运动列表的BUFF是否可以插入新的运动 
		/// </summary>
		/// <param name="crdNo"></param>
		/// <returns></returns>
		public bool PointTable_IsIdle(int crdNo)
		{
			return false;
			//try
			//{
			//    if (!bEnable)
			//        return false;

			//    int nSpace = 0;
			//    //获取队列当前的剩余量
			//    uint ret = ImcApi.IMC_CrdGetSpace(m_hCardHandle, (short)crdNo, ref nSpace);
			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        EventMgr.Error($"IMC30G-E Card IMC_CrdGetSpace Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//        return false;
			//    }

			//    return nSpace > 0;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return false;
			//}

		}

		/// <summary>
		/// 启动或停止一个连续运动
		/// </summary>
		/// <param name="crdNo"></param>
		/// <param name="bStart"></param>
		/// <returns></returns>
		public bool PointTable_Start(int crdNo, bool bStart)
		{

			return false;
			//try
			//{
			//    if (!bEnable)
			//        return false;

			//    uint ret = 0;
			//    if (bStart)
			//    {
			//        ret = ImcApi.IMC_CrdStart(m_hCardHandle, (short)crdNo);

			//        if (ret != ImcApi.EXE_SUCCESS)
			//        {
			//            EventMgr.Error($"IMC30G-E Card IMC_CrdStart Error,result = ".txtTrans(TransType.Mark) + $"{ret.ToString("x8")}");
			//            return false;
			//        }
			//    }
			//    else
			//    {
			//        ret = ImcApi.IMC_CrdStop(m_hCardHandle, (short)crdNo, 0);

			//        ret = ImcApi.IMC_CrdClrData(m_hCardHandle, (short)crdNo);

			//        ret = ImcApi.IMC_CrdDeleteMtSys(m_hCardHandle, (short)crdNo);
			//    }

			//    //Thread.Sleep(100);
			//    return true;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return false;
			//}

		}

		/// <summary>
		/// 插补运动是否停止
		/// </summary>
		/// <param name="crdNo"></param>
		/// <returns>0 - 表示停止; -1 - 未停止</returns>
		public int PointTable_IsStop(int crdNo)
		{

			return -1;
			//try
			//{
			//    if (!bEnable)
			//        return -1;

			//    short sts = 0;
			//    //获取插补运动执行缓冲区最后一段的运动状态
			//    uint ret = ImcApi.IMC_CrdGetArrivalSts(m_hCardHandle, (short)crdNo, ref sts);
			//    if (ret == ImcApi.EXE_SUCCESS)
			//    {
			//        if (sts == 1)
			//        {
			//            ImcApi.IMC_CrdStop(m_hCardHandle, (short)crdNo, 0);
			//            ImcApi.IMC_CrdDeleteMtSys(m_hCardHandle, (short)crdNo);
			//            return 0;
			//        }
			//    }

			//    return -1;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return -1;
			//}

		}

		/// <summary>
		/// 是否处于多轴插补运动模式
		/// </summary>
		/// <param name="nAxisNo">轴号</param>
		/// <returns>1：多轴插补运动模式；其他：不是</returns>
		private int IsCrdMode(int nAxisNo)
		{
			return -1;
			//try
			//{
			//    if (!bEnable)
			//        return -1;

			//    short[] nPrfModel = new short[1];
			//    //获取轴规划模式 当该值为-1 时，表示不处于任何规划模式。
			//    //Bit[15:6]    Bit[6:4]                                         Bit[3:0]    
			//    //无意义       多轴规划模式                                     单轴规划模式
			//    //             0X01 : PROFILE_MODE_CRD（插补模式）              0X01 : PROFILE_MODE_PTP（点位模式） 
			//    //             0X02 : PROFILE_MODE_MULTI_SYNC（多轴同步）       0X02 : PROFILE_MODE_JOG（JOG 模式） 
			//    //             0x04：PROFILE_MODE_SGL_BANDPT（捆绑 PT模式）     0X03 : PROFILE_MODE_GEAR（电子齿轮模式） 
			//    //                                                              0X04 : PROFILE_MODE_CAM（电子凸轮模式）
			//    //                                                              0X05 : PROFILE_MODE_PVT（PVT 模式） 
			//    //                                                              0X06 : PROFILE_MODE_GANTRY（龙门模式） 
			//    //                                                              0X07 : PROFILE_MODE_HANDWHEEL（手轮模式）
			//    //                                                              0X09: PROFILE_MODE_PTPC（点位连续模式）
			//    //                                                              0X0b: PROFILE_MODE_CRD_SYNC(插补同步轴模式)
			//    //                                                              0X0f: PROFILE_MODE_HOMING(回零模式)
			//    uint ret = ImcApi.IMC_GetAxPrfMode(m_hCardHandle, (short)nAxisNo, nPrfModel);
			//    if (ret != ImcApi.EXE_SUCCESS)
			//    {
			//        return -2;
			//    }

			//    //PROFILE_MODE_CRD（插补模式）
			//    if ((nPrfModel[0] & 0x10) != 0)
			//    {
			//        return 1;
			//    }
			//    else
			//    {
			//        return -1;
			//    }
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return -1;
			//}

		}

		/// <summary>
		/// 启用软件正限位
		/// </summary>
		/// <param name="nAxisNo"></param>
		/// <param name="bEnable"></param>
		/// <returns></returns>
		public bool SetSPELEnable(int nAxisNo, bool bEnable)
		{


			return true;
			//try
			//{
			//    if (!bEnable)
			//        return false;

			//    uint ret = 0;
			//    if (bEnable)
			//    {
			//        ret = ImcApi.IMC_AxSoftLmtsEnable(m_hCardHandle, (short)nAxisNo);
			//    }
			//    else
			//    {
			//        ret = ImcApi.IMC_AxSoftLmtsDisable(m_hCardHandle, (short)nAxisNo);
			//    }

			//    return ret == ImcApi.EXE_SUCCESS;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    throw new AlarmException(AlarmType.Error, ex.Message);
			//}

		}

		/// <summary>
		/// 启用软件负限位
		/// </summary>
		/// <param name="nAxisNo"></param>
		/// <param name="bEnable"></param>
		/// <returns></returns>
		public bool SetSMELEnable(int nAxisNo, bool bEnable)
		{
			return true;
			//try
			//{
			//    if (!bEnable)
			//        return false;

			//    uint ret = 0;
			//    if (bEnable)
			//    {
			//        ret = ImcApi.IMC_AxSoftLmtsEnable(m_hCardHandle, (short)nAxisNo);
			//    }
			//    else
			//    {
			//        ret = ImcApi.IMC_AxSoftLmtsDisable(m_hCardHandle, (short)nAxisNo);
			//    }

			//    return ret == ImcApi.EXE_SUCCESS;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    throw new AlarmException(AlarmType.Error, ex.Message);
			//}

		}

		/// <summary>
		/// 设置软件正限位位置
		/// </summary>
		/// <param name="nAxisNo"></param>
		/// <param name="pos"></param>
		/// <returns></returns>
		public bool SetSPELPos(int nAxisNo, double pos)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = trio.SetAxisVariable("FS_LIMIT", nAxisNo, pos); // 设置正向软限位（绝对位置）
				return ret;


			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}

		/// <summary>
		/// 设置软件负限位位置
		/// </summary>
		/// <param name="nAxisNo"></param>
		/// <param name="pos"></param>
		/// <returns></returns>
		public bool SetSMELPos(int nAxisNo, double pos)
		{
			try
			{
				if (!bEnable)
					return false;

				bool ret = trio.SetAxisVariable("RS_LIMIT", nAxisNo, pos); // 设置反向软限位（绝对位置）

				return ret;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				throw new AlarmException(AlarmType.Error, ex.Message);
			}

		}
		/// <summary>
		/// 获取板卡固件信息
		/// </summary>
		/// <param name="nAxisNo"></param>
		/// <param name="pos"></param>
		/// <returns></returns>
		public bool GetCardInfo()
		{
			return false;
			//try
			//{
			//    if (!bEnable)
			//        return false;

			//    uint ret = ImcApi.IMC_GetIMC30G_VER(m_hCardHandle, "D:/exe/CrdInfo/CardInfo");
			//    return ret == ImcApi.EXE_SUCCESS;
			//}
			//catch (AlarmException Aex)
			//{
			//    throw Aex;
			//}
			//catch (Exception ex)
			//{
			//    EventMgr.Error(ex.Message);
			//    return false;
			//}
		}



		public bool AbsLinearMove(ref int[] nAxisArray, ref double[] nPosArray, ushort crd, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0)
		{
			throw new NotImplementedException();
		}

		public bool GearStart(int cardIndex, ref short slaveAxNo)
		{
			throw new NotImplementedException();
		}

		public bool GearPTP(int cardIndex, short AxisNO)
		{
			throw new NotImplementedException();
		}

		public bool GearPrf(int cardIndex, ref short MasterAxNo, ref short slaveAxNo)
		{
			throw new NotImplementedException();
		}

		public bool GearStop(int cardIndex, ref short slaveAxNo)
		{
			throw new NotImplementedException();
		}

		public bool GetPrfMode(int cardIndex, ref short AxisNo, out short[] pPrfModeArray)
		{
			throw new NotImplementedException();
		}

		public bool AbsLinearMove(ref short[] nAxisArray, ref double[] nPosArray, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0)
		{
			throw new NotImplementedException();
		}

		public bool MultiSetupSys(ref short[] nAxisArray16, ref int[] nAxisArray32)
		{
			throw new NotImplementedException();
		}

		public bool MultiDeleteSys(ref short[] nAxisArray16, ref int[] nAxisArray32)
		{
			throw new NotImplementedException();
		}

		public bool GetAllMotionDone(ushort carNo)
		{
			throw new NotImplementedException();
		}

		#endregion



		#region IO
		private bool[] arrayDI;
		private bool[] arrayDO;

		/// <summary>
		/// IO读取线程
		/// </summary>
		private Thread tIOStatusRead;

		bool IsAuoRunning = false;

		/// <summary>
		/// DI组的数量，汇川的IO以8个为一组
		/// </summary>
		private int m_nDiGrpCnt = 0;

		/// <summary>
		/// DO组的数量，汇川的IO以8个为一组
		/// </summary>
		private int m_nDoGrpCnt = 0;


		public bool InitIO()
		{
			
			m_nDiGrpCnt = 1;

			m_nDoGrpCnt = 1;


			//开启扫描IO线程
			IOScanningThread();

			return true;
		}

		/// <summary>
		/// 获取所有输入信号(一次性读取)
		/// </summary>
		/// <returns></returns>
		public bool ReadAllDI()
		{
			if (!bEnable)
				return false;
			try
			{
				if (m_nDiGrpCnt < 1)
					return false;

				bool bRet = true;
				uint[] nData = new uint[n_TrioEtherCat_DI_Group/32];
				
				bRet = trio.InArray(0, n_TrioEtherCat_DI_Group-1, out nData);
				if (!bRet)
				{
					return false;
				}
				for (int i = 0; i < nData.Length; i++)
				{
					uint nInputData = 0;
					nInputData |= (uint)((nData[i]));
					m_IOCardStatusIn[i] = nInputData;
					
					for (int nIndex = 0; nIndex < 32; nIndex++)
					{
						bool di_state = (m_IOCardStatusIn[i] & (0x0001 << (nIndex))) != 0;
						arrayDI[32 * i + nIndex] = di_state;
					}
					

				}

				

				return bRet;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				EventMgr.Error(ex.Message);
				return false;
			}

		}
		/// <summary>
		/// 获取所有输出信号(一次性读取)
		/// </summary>
		/// <returns></returns>
		public bool ReadAllDO()
		{
			if (!bEnable)
				return false;

			try
			{
				if (m_nDoGrpCnt < 1)
				{
					return false;
				}
				bool bRet = true;
				uint[] nData1 = new uint[n_TrioEtherCat_DO_Group / 32];

				bRet = trio.ReadOpArray(0, n_TrioEtherCat_DO_Group-1, out nData1);
				if (!bRet)
				{
					return false;
				}
				for (int i = 0; i < nData1.Length; i++)
				{
					uint nOutputData = 0;
					nOutputData |= (uint)((nData1[i]));
					m_IOCardStatusOut[i] = nOutputData;
					for (int nIndex = 0; nIndex < 32; nIndex++)
					{
						bool do_state = (m_IOCardStatusOut[i] & (0x0001 << (nIndex))) != 0;
						arrayDO[32 * i + nIndex] = do_state;
					}

				}
				return bRet;
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				EventMgr.Error(ex.Message);
				return false;
			}

		}

		/// <summary>
		/// 开启IO读取线程
		/// </summary>
		public void IOScanningThread()
		{
			if (tIOStatusRead != null)
			{
				if (!tIOStatusRead.IsAlive)
				{
					tIOStatusRead = new Thread(ReadAllIOData);
					tIOStatusRead.IsBackground = true;
					IsAuoRunning = true;
					tIOStatusRead.Start();
				}
			}
			else
			{
				tIOStatusRead = new Thread(ReadAllIOData);
				tIOStatusRead.IsBackground = true;
				IsAuoRunning = true;
				tIOStatusRead.Start();
			}
		}

		/// <summary>
		/// 开启新线程，只扫描IO 状态
		/// </summary>
		public void ReadAllIOData()
		{
			while (true)
			{
				if (bEnable)//直接运行
				{
					ReadAllDI();
					ReadAllDO();
				}
				Thread.Sleep(2);
			}
		}

		/// <summary>
		/// 获取输入信号
		/// </summary>
		/// <param name="nIndex"></param>
		/// <returns></returns>
		public bool GetDI(int nIndex)
		{
			if (!bEnable)
				return false;

			bool diValue = arrayDI[nIndex];
			return (diValue);
		}
		/// <summary>
		/// 获取输出信号
		/// </summary>
		/// <param name="nIndex"></param>
		/// <returns></returns>
		public bool GetDO(int nIndex)
		{
			if (!bEnable)
				return false;

			bool doValue = arrayDO[nIndex];
			return (doValue);
		}
		/// <summary>
		/// 输出信号
		/// </summary>
		/// <param name="nIndex">输出点序号</param>
		/// <param name="bBit">输出值</param>
		/// <returns></returns>
		public bool WriteDO(int nIndex, bool bBit)
		{
			if (!bEnable)
				return false;

			try
			{

				//nIndex += 32 * m_nCardNo - 1;
				bool ret = trio.Op(nIndex, nIndex, bBit ? 1 : 0);
				if (ret)
					return true;
				else
					return false;


			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				EventMgr.Error(ex.Message);
				return false;
			}

		}


		/// <summary>
		/// 获取输入信号 API
		/// </summary>
		/// <param name="nIndex">输入信号序号</param>
		/// <returns></returns>
		public bool ReadDI(int nIndex)
		{
			if (!bEnable)
				return false;

			try
			{				
				double nBitValue = 0;
				bool nInputData = trio.In(nIndex, nIndex, out nBitValue);
				Thread.Sleep(2);
				return (nBitValue == 1);
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				EventMgr.Error(ex.Message);
				return false;
			}

		}



		/// <summary>
		/// 获取输出信号 API
		/// </summary>
		/// <param name="nIndex">输出信号序号</param>
		/// <returns></returns>
		public bool ReadDO(int nIndex)
		{
			if (!bEnable)
				return false;

			try
			{		
				double nBitValue = 0;
				bool nInputData = trio.ReadOp(nIndex, nIndex, out nBitValue);
			
				return (nBitValue == 1);
			}
			catch (AlarmException Aex)
			{
				throw Aex;
			}
			catch (Exception ex)
			{
				EventMgr.Error(ex.Message);
				return false;
			}

		}

		#endregion

		//>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>-------------操作：VR/Table地址------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
		/// <summary>
		/// 获取VR  nano：0-16384   664X：0-65535
		/// </summary>
		/// <param name="Variable"></param>
		/// <param name="Value"></param>
		/// <returns></returns>
		public bool GetVr(int Variable, out double Value)
		{
			return trio.GetVr(Variable, out Value);
		}
		/// <summary>
		/// 写VR  nano：0-16384   664X：0-65535
		/// </summary>
		/// <param name="Variable"></param>
		/// <param name="Value"></param>
		/// <returns></returns>
		public bool SetVr(int Variable, double Value)
		{
			return trio.SetVr(Variable, Value);
		}
		/// <summary>
		/// 写table 512000个
		/// </summary>
		/// <param name="StartPosition"></param>
		/// <param name="Values"></param>
		public void SetTable(int StartPosition, double[] Values)
		{
			trio.SetTable(StartPosition, Values);
		}
		public void SetTable(int StartPosition, double[] Values, int Offset, int NumberOfValues)
		{
		    trio.SetTable(StartPosition, Values, Offset, NumberOfValues);
		}
		public void SetTable(int StartPosition, int NumberOfValues, double[] Values)
		{
			trio.SetTable(StartPosition, NumberOfValues, Values);
		}
		public bool GetAin(int Variable, out double Value)
		{
			return trio.Ain(Variable, out Value);
		}

		/// <summary>
		/// 获取模拟量输入值
		/// </summary>
		/// <param name="nIndex">通道号</param>
		/// <param name="nValue">模拟量值</param>
		/// <returns></returns>
		//public override bool ReadAI(Int16 nIndex, ref Int16[] nValue)
		//{
		//	if (!bEnable)
		//		return false;

		//	//uint ret = ImcApi.IMC_GetEcatAdVal(m_hCardHandle, nIndex, ref nValue);
		//	//if (ret == ImcApi.EXE_SUCCESS)
		//	//    return true;

		//	//EventMgr.Error($"ReadAI()出错! result = {ret.ToString("x8")}");
		//	return false;
		//}

		public override bool ReadAI(Int16 nIndex, ref Int16 nValue)
		{
			return false;
		}
	}
}

using AF.Card;
using AF.Common;
using AF.Common.UI;
using AF.Communicate;
using AF.Data;
using AF.Data.Language;
using AF.Data.Permission;
using AF.Station.UI;
using System;
using System.Reflection;
using System.Windows.Forms;
using AF.Station;
using AF.File;
using Autoframe.UI;
using AF.Upload;
using AF.Upload.UI;

namespace Autoframe
{
    static class Program
    {
        //Ini文件路径
        public static string g_IniFile = AppDomain.CurrentDomain.BaseDirectory + "Config" + "\\" + "Autoframe.ini";
        public static string StationsParam_IniFile = AppDomain.CurrentDomain.BaseDirectory + "Config" + "\\" + "StationsParam.ini";
        public static string csvRobotPos_Path = AppDomain.CurrentDomain.BaseDirectory + "Config" + "\\" + "csvRobotPos.csv";

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]

        static void Main(string[] args)
        {
            try
            {
                if (ProgramTool.IsProcessOpenAgain())
                {
                    MessageBox.Show("Autoframe 程序已运行".txtTrans( TransType.Trans), "退出".txtTrans(TransType.Trans));
                    return;
                }
                ProgramTool.Init(args);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Init();
                Application.Run(new Form_Main());
                Deinit();                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "系统运行异常".txtTrans(TransType.Trans), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                GC.Collect();
                GC.WaitForFullGCComplete();
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            //事件
            Form_Splash.UpdateUI("注册事件".txtTrans(TransType.Trans), 1);         
            EventMgr.RegisterEvent<PlannedDown>(EventType.PlannedDown);
            EventMgr.RegisterEvent<ErrCustom>(EventType.UnplannedDown);
            Statistics.Init<Material, ThrowReason>(Ini.GetStringValue(Program.g_IniFile, "Info", "ShiftStartTime", ""));

            //寄存器
            Form_Splash.UpdateUI("加载参数".txtTrans(TransType.Trans), 10);
            Parameter.Init();
            Form_Splash.UpdateUI("加载数据组".txtTrans(TransType.Trans), 20);
            DataGroup.Init();
            Form_Splash.UpdateUI("加载寄存器".txtTrans(TransType.Trans), 30);
            Register.Init<RegBit, RegInt, RegDouble, RegString>();
            MemoryMap.Init<MemoryMapBit, MemoryMapInt, MemoryMapDouble, MemoryMapString>();

            //板卡
            Form_Splash.UpdateUI("加载硬件配置".txtTrans(TransType.Trans), 50);
            CardMgr.InitIO<DI, DO, AI>();
            SystemInit.InitHomeMode();//此函数一定要在CardMgr.InitAxis()上方
            CardMgr.InitAxis<AXIS>();
            CylinderMgr.Init<Cylinder>();
            Form_Splash.UpdateUI("初始化板卡".txtTrans(TransType.Trans), 60);
            CardMgr.InitAllCard();

            //通讯
            Form_Splash.UpdateUI("初始化通讯".txtTrans(TransType.Trans), 60);
            TcpClientMgr.Init<TCP_Client>();
            TcpServerMgr.Init<TCP_Server>();
            ComMgr.Init<COM>();
            TcpCommunicate.Init();

            //站位
            Form_Splash.UpdateUI("加载站位".txtTrans(TransType.Trans), 80);
            SystemInit.Init();
            SystemInit.AddStation();
            StationMgr.CheckPoint();//此函数一定要在CardMgr.AddStation()下方            
            SystemIO.RegisterDiEvent();
            DoHandle.IsSafeEvent += SystemInit.IsDoSafe;
            AxisHandle.IsSafeEvent += SystemInit.IsAxisSafe;

            //其他
            ProductConfig.Load();
            Form_Setting_SystemVariable.Init();
			Form_Setting_FileMonitor.Init();
            SystemInit.InitRecipe();            
            EventMgr.SaveEventCode();
            if (Form_Upload.FormCount > 0)
                Form_Manual.AddPage(new Form_Upload());
            Form_Splash.UpdateUI("程序初始化完成".txtTrans(TransType.Trans), 100);
        }

        public static void Deinit()
        {
            PDCA.Deinit();            
            SystemIO.Deinit();
            CardMgr.DeinitAllCard();
            FileMonitor.Deinit();            
            StationMgr.Deinit();
            Register.Deinit();
			Statistics.DeInit();
            //Logger.Deinit() 要放最后
            Logger.Deinit();
        }
    }
}

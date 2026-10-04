using AF.Card;
using AF.Station;
using AF.Common;
using AF.Data;
using AF.Card.UI;
using Autoframe.UI;
using AF.Upload;
using AF.Upload.UI;
using AF.Communicate;

namespace Autoframe
{
    static class SystemInit
    {
        /// <summary>
        ///系统初始化
        /// </summary>
        public static void Init()
        {
            //添加自定义log
            //Logger.AddLogger("自定义1");

            //PDCA，若不需要可屏蔽
            //InitPDCA();

            //增加自定义页
            Form_Auto.AddPage(new Form_RingMonitor());   // 钢环检测结果实时监控
            Form_Auto.AddPage(new Form_MesTest2());          // MES 数据采集接口：上传与测试
            //Form_Auto.AddPage(new Form_Test());
            //Form_Manual.AddPage(new Form_Test());
            //Form_Data.AddPage(new Form_Test(), new Form_Test());
            //Form_Setting.AddPage(new Form_Test(), new Form_Test());
            //Form_Upload.AddPage(new Form_Test());

            //Main界面的添加自定义面板，使用框架的 Form_Auto_CustomPanel 可以快速添加IO。也可以添加自定义的UserControl
            Form_Auto.SetCustomPanel(new object[] { DI.总气压到位 }, new object[] { DO.日光灯 });
            //Form_Auto.SetCustomPanel(new UserControl());

            //绑定安全门，安全门无法通过 Hardware配置 来逻辑取反，可以在 BindSafetyDoor() 进行逻辑取反。
            //SystemIO.BindSafetyDoor(
            //    new object[] { DI.前安全门 }, new object[] { DI.后安全门 },
            //    new object[] { DI.左安全门 }, new object[] { DI.右安全门 }, 
            //    DI.安全门总感应,
            //    new object[] { DO.前安全门锁 }, new object[] { DO.后安全门锁 },
            //     new object[] { DO.左安全门锁 }, new object[] { DO.右安全门锁 }
            //     );
            SystemIO.BindSafetyDoor(
                null, new object[] { DI.后安全门 },
                new object[] { DI.左安全门 }, new object[] { DI.右安全门 },
                null,
                null, null,
                 null, null, new bool[]{ true,false}
                 );
            //绑定系统IO
            SystemIO.BindDI(DI.急停, DI.暂停, DI.复位, DI.启动, new object[] { DI.总气压到位 });
            SystemIO.BindDO(DO.红灯, DO.绿灯, DO.黄灯, DO.蜂鸣器, 10);
            //添加安全光栅
            //SystemIO.AddSafetyCurtain(DI.安全光栅, new object[] { AXIS.料仓升降Z轴, AXIS.OK下料Y轴, AXIS.NG下料Y轴 }, null, DI.启动);
        }

        /// <summary>
        ///添加站位，绑定系统IO
        /// </summary>
        public static void AddStation()
        {
            StationMgr.AddStation(new 上料站("上料站") , 1);
            StationMgr.AddStation(new 拉料站("拉料站") , 1);
            StationMgr.AddStation(new 扩膜站("扩膜站") , 1);
            StationMgr.AddStation(new 摆臂站("摆臂站") , 1);
            StationMgr.AddStation(new IR检测1("IR检测1") , 1);
            StationMgr.AddStation(new IR检测2("IR检测2") , 1);
            StationMgr.AddStation(new 翻转("翻转") , 2);
            StationMgr.AddStation(new 移载("移载") , 2);
            StationMgr.AddStation(new AR检测1("AR检测1") , 2);
            StationMgr.AddStation(new AR检测2("AR检测2") , 2);
            StationMgr.AddStation(new OK下料站("OK下料") , 3);
            StationMgr.AddStation(new NG下料站("NG下料") , 3);
            StationMgr.AddStation(new OK覆膜站("OK覆膜") , 3);
            StationMgr.AddStation(new NG覆膜站("NG覆膜") , 3);
            StationMgr.AddStation(new OK料仓站("OK料仓") , 3);
            StationMgr.AddStation(new NG料仓站("NG料仓") , 3);
            StationMgr.AddStation(new StationRobot("机械手站") , 2);
            StationMgr.AddStation(new DataUpdata("数据更新"), 2);
        }

        /// <summary>
        /// 轴运动安全判定
        /// </summary>
        /// <param name="nAxis">轴号</param>
        /// <param name="dbTargetPos">目标位置，回原、Jog会传null</param>
        /// <param name="strMsg">危险操作信息</param>
        /// <returns></returns>
        public static bool IsAxisSafe(int nAxis, double? dbTargetPos, ref string strMsg)
        {
            switch ((AXIS)nAxis)
            {
                case AXIS.扩膜X轴:
                    if ((dbTargetPos <= -19000 || dbTargetPos >= 130000) && AXIS.顶针模组Z轴.GetPos() > -28)
                    {
                        double zzz = AXIS.顶针模组Z轴.GetPos();
                        return false;
                    }
                    break;
                case AXIS.扩膜Y轴:
                    if ((dbTargetPos <= 20000 || dbTargetPos >= 150000) && AXIS.顶针模组Z轴.GetPos() > -28)
                    {
                        return false;
                    }
                    break;
                //case AXIS.复检X轴:
                //    break;
                //case AXIS.复检Y轴:
                //    double aa = StationRecheck.POINT.拍照点2.GetPos(AXIS.复检Y轴, "复检站");
                //    break;
                //case AXIS.下料Y轴:
                //    //if (DI.取MCH气缸伸出位.Get() || !DI.取MCH气缸缩回位.Get())
                //    //{
                //    //    strMsg = "取MCH气缸有碰撞风险";
                //    //    return false;
                //    //}
                //    break;
                //case AXIS.顶针模组Z轴:
                //    if (AXIS.扩膜X轴.GetPos()>130 || AXIS.扩膜X轴.GetPos() < -15)
                //    {
                //        strMsg = "取MCH气缸有碰撞风险";
                //        return false;
                //    }
                //    break;
                default:
                    break;
            }
            return true;
        }

        /// <summary>
        /// DO操作安全判定
        /// </summary>
        /// <param name="nIndex">DO序号</param>
        /// <param name="strMsg">危险操作信息</param>
        /// <returns></returns>
        public static bool IsDoSafe(int nIndex, ref string strMsg)
        {
            switch ((DO)nIndex)
            {
                //case DO.复检阻挡上:
                //case DO.复检阻挡下:
                //    break;
                //case DO.复检顶升上:
                //case DO.复检顶升下:
                //    break;
                default:
                    break;
            }
            return true;
        }

        /// <summary>
        /// 设置生产配方，能快按生产场景切参数
        /// </summary>
        public static void InitRecipe()
        {
            Parameter.AddRecipe("标准生产", () =>
            {
                //Parameter.SetBit("VisionEnable", true);
            });
            Parameter.AddRecipe("场景1", () =>
            {
            });
            Parameter.AddRecipe("场景2", () =>
            {
            });
        }

        /// <summary>
        /// 自定义回原模式
        /// </summary>
        public static void InitHomeMode()
        {
            //CardMgr.AddHomeMode("InoEcat", -1, "111-222", "音圈电机回原");
        }

        ///// <summary>
        ///// PDCA初始化
        ///// </summary>
        //public static void InitPDCA()
        //{
        //    //添加PDCA数据项
        //    PDCA.AddItem(new PdcaItem_attr("GlueCode"));
        //    PDCA.AddItem(new PdcaItem_attr("Version"));
        //    PDCA.AddItem(new PdcaItem_log_file("Administrator", "1"));
        //    PDCA.AddItem(new PdcaItem_dut_pos("A6"));
        //    PDCA.AddItem(new PdcaItem_attr("Station"));
        //    PDCA.AddItem(new PdcaItem_pdata("W1"));
        //    PDCA.AddItem(new PdcaItem_pdata("W2"));
        //    PDCA.AddItem(new PdcaItem_pdata("X1", "0.7", "5", "HZ"));
        //    PDCA.AddItem(new PdcaItem_pdata("X2", "0.7", "5", "HZ"));

        //    PDCA.Init(TCP_Client.PDCA.GetTcp(), "SEC_v1.0_CoilDispense");
        //}
    }
}

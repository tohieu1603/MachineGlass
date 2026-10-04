
namespace Autoframe
{
    #region IO、Cylinder、Axis

    /// <summary>
    /// 数字量输入
    /// </summary>
    public enum DI
    {
        机械手报警,
        机械手运行,
        机械手停止,
        机械手启动完成,
        DI005,
        DI006,
        DI007,
        DI008,
        OK覆膜下压伸出位,
        OK覆膜下压缩回位,
        OK覆膜切刀伸出位,
        OK覆膜切刀缩回位,
        OK上覆膜有料检测,
        OK下覆膜有料检测,
        OK覆膜超限信号1,
        OK覆膜超限信号2,
        NG覆膜下压伸出位,
        NG覆膜下压缩回位,
        NG覆膜切刀伸出位,
        NG覆膜切刀缩回位,
        NG上覆膜有料检测,
        NG下覆膜有料检测,
        NG覆膜超限信号1,
        NG覆膜超限信号2,
        覆膜上料按钮,
        OK流线感应光电,
        NG流线感应光电,
        DI028,
        DI029,
        DI030,
        DI031,
        DI032,
        IR面吸真空1到位,
        IR面吸真空2到位,
        IR面吸真空3到位,
        IR面吸真空4到位,
        DI037,
        DI038,
        DI039,
        DI040,
        IR面吸真空5到位,
        IR面吸真空6到位,
        IR面吸真空7到位,
        IR面吸真空8到位,
        DI045,
        DI046,
        DI047,
        DI048,
        翻转吸真空到位,
        DI050,
        DI051,
        DI052,
        DI053,
        DI054,
        DI055,
        DI056,
        移载吸真空到位,
        DI058,
        DI059,
        DI060,
        DI061,
        DI062,
        DI063,
        DI064,
        AR面吸真空1到位,
        AR面吸真空2到位,
        AR面吸真空3到位,
        AR面吸真空4到位,
        DI069,
        DI070,
        DI071,
        DI072,
        AR面吸真空5到位,
        AR面吸真空6到位,
        AR面吸真空7到位,
        AR面吸真空8到位,
        DI077,
        DI078,
        DI079,
        DI080,
        NG下料按钮,
        NG下料吸真空到位,
        NG料仓有料,
        NG料仓满料,
        NG料仓到位,
        NG料仓解锁到位,
        NG料仓锁止到位,
        DI088,
        OK下料按钮,
        OK下料吸真空到位,
        OK料仓有料,
        OK料仓满料,
        OK料仓到位,
        OK料仓解锁到位,
        OK料仓锁止到位,
        DI096,
        急停,
        启动,
        复位,
        暂停,
        启动2,
        复位2,
        暂停2,
        DI104,
        总气压到位,
        前安全门,
        后安全门,
        左安全门,
        右安全门,
        安全门总感应,
        DI111,
        DI112,
        料盘上料按钮,
        料盘有无检测,
        料盘超限检测,
        DI116,
        拉料升降伸出位,
        拉料升降缩回位,
        拉料夹爪夹紧位,
        拉料夹爪松开位,
        钢环有料感应,
        摆臂吸真空感应,
        顶针吸真空感应,
        DI124,
        钢环固定气缸1伸出位,
        钢环固定气缸1缩回位,
        钢环固定气缸2伸出位,
        钢环固定气缸2缩回位,
    }

    /// <summary>
    /// 数字量输出
    /// </summary>
    public enum DO
    {
        机械手启动,
        机械手停止,
        机械手复位,
        机械手清除报警,
        DO005,
        DO006,
        DO007,
        DO008,
        OK覆膜下压伸出,
        OK覆膜下压缩回,
        OK覆膜切刀伸出,
        OK覆膜切刀缩回,
        OK流道正转,
        OK流道反转,
        OK气胀轴,
        DO016,
        NG覆膜下压伸出,
        NG覆膜下压缩回,
        NG覆膜切刀伸出,
        NG覆膜切刀缩回,
        NG流道正转,
        NG流道反转,
        NG气胀轴,
        覆膜上料按钮灯,
        CCD1触发,
        CCD2触发,
        CCD3触发,
        CCD4触发,
        CCD3复位,
        CCD6触发,
        CCD7触发,
        CCD6复位,
        IR面吸破真空1,
        IR面吸破真空2,
        IR面吸破真空3,
        IR面吸破真空4,
        IR面1吸真空,
        IR面1破真空,
        DO039,
        DO040,
        IR面吸破真空5,
        IR面吸破真空6,
        IR面吸破真空7,
        IR面吸破真空8,
        IR面2吸真空,
        IR面2破真空,
        DO047,
        DO048,
        翻转吸真空,
        翻转破真空,
        DO051,
        DO052,
        DO053,
        DO054,
        DO055,
        DO056,
        移载吸真空,
        移载破真空,
        DO059,
        DO060,
        DO061,
        DO062,
        DO063,
        DO064,
        AR面吸破真空1,
        AR面吸破真空2,
        AR面吸破真空3,
        AR面吸破真空4,
        AR面1吸真空,
        AR面1破真空,
        DO071,
        DO072,
        AR面吸破真空5,
        AR面吸破真空6,
        AR面吸破真空7,
        AR面吸破真空8,
        AR面2吸真空,
        AR面2破真空,
        DO079,
        DO080,
        NG下料按钮灯,
        NG下料吸真空,
        NG下料破真空,
        DO084,
        DO085,
        NG料仓解锁,
        NG料仓锁定,
        DO088,
        OK下料按钮灯,
        OK下料吸真空,
        OK下料破真空,
        DO092,
        DO093,
        OK料仓解锁,
        OK料仓锁定,
        DO096,
        日光灯,
        启动灯,
        复位灯,
        暂停灯,
        启动灯2,
        复位灯2,
        暂停灯2,
        红灯,
        黄灯,
        绿灯,
        蜂鸣器,
        前安全门锁,
        后安全门锁,
        左安全门锁,
        右安全门锁,
        DO112,
        料盘上料按钮灯,
        DO114,
        DO115,
        DO116,
        拉料升降伸出,
        拉料升降缩回,
        拉料夹爪夹紧,
        拉料夹爪松开,
        摆臂取料吸真空,
        摆臂取料破真空,
        顶针吸真空,
        顶针破真空,
        钢环固定气缸1伸出,
        钢环固定气缸1缩回,
        钢环固定气缸2伸出,
        钢环固定气缸2缩回,
    }

    /// <summary>
    /// 模拟量输入枚举
    /// </summary>
    public enum AI
    {
        //温度,
        //气压,
    }

    /// <summary>
    /// 气缸
    /// </summary>
    public enum Cylinder
    {
        NG覆膜下压气缸,
        NG覆膜切刀气缸,
        OK覆膜下压气缸,
        OK覆膜切刀气缸,
        OK料仓解锁气缸,
        NG料仓解锁气缸,
        拉料升降气缸,
        拉料夹爪气缸,
        摆臂取料真空,
        顶针真空,
        钢环固定气缸1,
        钢环固定气缸2,
    }

    /// <summary>
    /// 轴
    /// </summary>
    public enum AXIS
    {
        料仓升降Z轴 = 0,
        拉料Y轴,
        扩膜X轴,
        扩膜Y轴,
        扩膜Z轴,
        扩膜U轴,
        顶针模组Z轴,
        顶针上下Z轴,
        摆臂旋转U轴,
        摆臂上下Z轴,
        移载X轴,
        移载Y轴,
        移载Z轴,
        IR面检测X1轴,
        IR面检测Z1轴,
        IR面检测Y1轴,
        IR面检测X2轴,
        IR面检测Z2轴,
        IR面检测Y2轴,
        AR面检测X1轴,
        AR面检测Z1轴,
        AR面检测Y1轴,
        AR面检测X2轴,
        AR面检测Z2轴,
        AR面检测Y2轴,
        翻转U轴,
        NG覆膜调整Z轴,
        NG覆膜调整X轴,
        NG下料Y轴,
        NG下料Z轴,
        OK覆膜调整Z轴,
        OK覆膜调整X轴,
        OK下料Y轴,
        OK下料Z轴,

    }

    #endregion

    #region 网口、串口

    /// <summary>
    /// TCP客户端
    /// </summary>
    public enum TCP_Client
    {
        机械手,
        CCD1,
        CCD2,
        CCD3,
        CCD4,
        CCD5,
        CCD6,
        CCD7,
        CCD8,
    };

    /// <summary>
    /// TCP服务器
    /// </summary>
    public enum TCP_Server
    {
        //Server0,
        //Server1,
    }

    /// <summary>
    /// 串口
    /// </summary>
    public enum COM
    {
        //光源,
    };

    #endregion


    #region 寄存器
    /// <summary>
    /// 位寄存器
    /// </summary>
    public enum RegBit
    {
        强制OK,
        上料站初始化完成,
        上料站通知拉料换料完成,
        上料站通知拉料下降3毫米到位,
        上料站通知拉料上升3毫米到位,

        拉料站初始化完成,
        拉料站初始化有料,
        拉料通知扩膜上料完成,
        拉料通知上料换料,
        拉料通知上料下降3,
        拉料通知上料上升3,
        拉料通知扩膜取料完成,

        扩膜站初始化完成,
        扩膜通知摆臂取料,
        扩膜通知拉料下钢环,
        扩膜通知拉料站上钢环,
        扩膜通知拉料站下钢环,
        扩膜开始标定,
        扩膜标定允许摆臂放料,
        扩膜初始化通知摆臂回原,
        扩膜CCD1拍照完成状态,

        扩膜允许清料,
        扩膜拍完一个钢环,

        摆臂允许清料,
        摆臂标定通知扩膜放料完成,

        摆臂站初始化完成,
        摆臂站初始化有料,
        摆臂站初始化Z轴已抬起,
        摆臂通知扩膜取料完成,
        摆臂通知IR检测放料完成,
        摆臂通知IR检测到位放料,
        摆臂停止上料,
        IR载具清料,
        IR检测1初始化吸嘴有料,
        IR检测1初始化完成,
        IR检测1待机位,
        IR检测1上料位,
        IR检测1拍照等待位,
        IR检测1拍照位,
        IR检测1下料等待位,
        IR检测1下料位,
        IR检测1缩回位,
        IR检测1通知摆臂接料准备OK,

        IR检测2初始化吸嘴有料,
        IR检测2初始化完成,
        IR检测2待机位,
        IR检测2上料位,
        IR检测2拍照等待位,
        IR检测2拍照位,
        IR检测2下料等待位,
        IR检测2下料位,
        IR检测2缩回位,
        IR检测2通知摆臂接料准备OK,
        IR检测通知翻转有料,
        IR检测需要清料,


        翻转吸嘴有料,
        翻转初始化完成,
        翻转通知IR检测上料,
        翻转通知IR检测上料完成,
        翻转通知移载有料,


        移载通知翻转吸真空,
        移载初始化吸嘴有料,
        移载初始化完成,
        移载通知AR检测放料完成,
        移载通知翻转取料完成,

        AR检测1初始化吸嘴有料,
        AR检测1初始化完成,
        AR检测1待机位,
        AR检测1上料位,
        AR检测1拍照等待位,
        AR检测1拍照位,
        AR检测1下料等待位,
        AR检测1下料位,
        AR检测1缩回位,

        AR检测2初始化吸嘴有料,
        AR检测2初始化完成,
        AR检测2待机位,
        AR检测2上料位,
        AR检测2拍照等待位,
        AR检测2拍照位,
        AR检测2下料等待位,
        AR检测2下料位,
        AR检测2缩回位,

        AR检测通知移载接料准备OK,
        AR检测通知机械手有料,
        AR检测通知机械手真空已关闭,
        AR通知机械手穴位1有料,
        AR通知机械手穴位2有料,
        AR通知机械手穴位3有料,
        AR通知机械手穴位4有料,

        机械手初始化完成,
        机械手初始化吸嘴1有料,
        机械手初始化吸嘴2有料,
        机械手初始化吸嘴3有料,
        机械手初始化吸嘴4有料,
        机械手通知AR关闭真空,
        机械手通知AR检测取料完成,
        机械手通知OK覆膜放料完成,
        机械手通知NG覆膜放料完成,
        机械手未重置钢环状态,
        机械手申请清载具,
        机械手通知DataUpdata有料,

        DataUpdata初始化有料,
        DataUpdata向机械手要料,

        OK覆膜初始化完成,
        OK覆膜通知机械手可放料,
        OK覆膜通知OK下料有料,
        OK下料初始化完成,
        OK下料通知OK覆膜取料完成,
        OK下料通知OK料仓放料完成,
        OK料仓初始化完成,
        OK料仓通知OK下料可放料,
        NG覆膜初始化完成,
        NG覆膜通知机械手可放料,
        NG覆膜通知NG下料有料,
        NG下料初始化完成,
        NG下料通知OK覆膜取料完成,
        NG下料通知NG料仓放料完成,
        NG料仓初始化完成,
        NG料仓通知NG下料可放料,
    }
    /// <summary>
    /// 整型寄存器
    /// </summary>
    public enum RegInt
    {
        相机功能选择,
        /// <summary>
        /// 站位运行进度百分比，用于界面显示进度条      
        /// </summary>
        Int_Process_Step,
        SelectMaintenanceState,
        BufferBin,
        LoadingBin,
        UnLoadingBin,

        Int_ReworkCount,
        AR通知机械手穴位1有料,
        TestCount,
        RowCCD1,
        ColCCD1,
        PickRingSite,
    };

    /// <summary>
    /// 浮点型寄存器
    /// </summary>
    public enum RegDouble
    {
        CT,
    };

    /// <summary>
    /// 字符串寄存器
    /// </summary>
    public enum RegString
    {
        Platform2ProductGUID,
        str_WorkMode,//返工模式
        OrderSN,
        OrderCreatTime,
        UpRingTime
    }

    #endregion

    #region 物料、事件
    /// <summary>
    /// 生产主料枚举
    /// </summary>
    public enum Material
    {
        //主料，固定为0，不能更改
        Chassis = 0,

        //辅料
        Screw1,
        Screw2,
    }

    /// <summary>
    /// 抛料原因
    /// </summary>
    public enum ThrowReason
    {
        //抛料原因固定格式为：物料名_抛料原因
        Chassis_螺孔定位NG,
        Chassis_相机定位NG,
        Chassis_AOI检测结果NG,

        Screw1_相机定位NG,
        Screw1_吸嘴真空NG,

        Screw2_相机定位NG,
        Screw2_吸嘴真空NG,
    }

    /// <summary>
    /// 计划停机枚举
    /// </summary>
    public enum PlannedDown
    {
        吃饭,
        维护保养,
        换胶水,
    }

    /// <summary>
    /// 非计划停机枚举
    /// </summary>
    public enum ErrCustom
    {
        自定义错误1,
        自定义错误2,
        自定义错误3,
    }
    #endregion

    /// <summary>
    /// 自动流程
    /// </summary>
    public enum AutoProcess
    {
        //0--1：生产、空跑
        生产 = 0,
        空跑,

        //100--199：标定类型
        CCD1标定注册 = 100,
        CCD2标定注册,
        CCD4标定,
        CCD5标定,
        CCD7九点标定,
        CCD8九点标定,
        CCD7旋转标定,
        CCD8旋转标定,
        机械手下相机基准标定,



        //200--299：GRR类型
        扩膜站相机动静态 = 200,
        CCD3静态,
        CCD3动态,
        CCD4CCD5静态,
        CCD4CCD5动态,
        CCD6静态,
        CCD6动态,
        机械手下相机动态,
        机械手下相机静态,

        //300--......：自定义测试项
        AR1测试 = 300,
        AR2测试,
        OK覆膜一键出料,
        NG覆膜一键出料
    }
}

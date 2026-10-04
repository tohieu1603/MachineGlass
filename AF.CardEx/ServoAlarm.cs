using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AF.Card;

namespace AF.CardEx
{
    /// <summary>
    /// 汇川伺服报警码
    /// </summary>
    public class ServoAlarm_Inovance : ServoAlarm
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ServoAlarm_Inovance()
        {
            //厂商
            this.strBrand = "汇川";
            //报警码地址
            this.sdo = new SdoData(0x203F, 0x00, 4);
        }

        /// <summary>
        /// 获取报警码
        /// </summary>
        public override string AlarmCode
        {
            get
            {
                // E939.0
                this.sdo.Reverse();
                string str = this.sdo.Get<string>();
                return $"E{str[1]}{str[3]}{str[4]}.{str[0]}";
            }
        }
    }

    /// <summary>
    /// 高创伺服报警码
    /// </summary>
    public class ServoAlarm_Servotronix : ServoAlarm
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ServoAlarm_Servotronix()
        {
            //厂商
            this.strBrand = "高创";
            //报警码地址
            this.sdo = new SdoData(0x2011, 0x00, 4);
        }

        /// <summary>
        /// 获取报警码
        /// </summary>
        public override string AlarmCode
        {
            get
            {
                return $"E{this.sdo.Get<int>().ToString("D4")}";
            }
        }
    }

    /// <summary>
    /// 帕特尼伺服报警码
    /// </summary>
    public class ServoAlarm_Patni : ServoAlarm
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ServoAlarm_Patni()
        {
            //厂商
            this.strBrand = "帕特尼";
        }

        /// <summary>
        /// 获取报警码
        /// </summary>
        public override string AlarmCode
        {
            get
            {
                // E0150
                return $"E{this.pdo.Get<int>().ToString("D4")}";
            }
        }
    }

    ////bit31-15:厂家使用    bit14-8：报警主码   bit7-0：报警辅码
    //CardMgr.AddAlarmSDO("松下", 0x4DA0, 0x02, 4);

    //    //bit0: 0/1 0:error  1:nomal   bit1-7 预留  
    //    CardMgr.AddAlarmSDO("安川", 0x1001, 0x00, 1);
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AF.Communicate;
using AF.File;
using IMC100APIDLL;

namespace Autoframe
{
    public class InoRobot
    {
        /// <summary>
        /// 连接顺序：1-Connect；2-检测急停及故障并清除故障状态；3-servoON；4-switchAutoMode；5-外部TCP连接；6-start；暂停停止：pause；断开：disConnect
        /// </summary>
        /// <param name="strIpAddr">IP</param>
        /// <param name="comId">自定义该IP对应的操作ID</param>
        public InoRobot(string strIpAddr, int comId = 0)
        {
            //以太网连接
            byte[] bytes = IPAddress.Parse(strIpAddr).GetAddressBytes();
            Array.Reverse(bytes);
            IpAddr = BitConverter.ToUInt32(bytes, 0);
            Port = UInt16.Parse("2222");//API连接端口固定"2222"
            comID = comId;
            ReadErrcodeCsv();
        }

        UInt32 IpAddr;
        UInt16 Port;
        int comID;
        bool b_Permit = false;

       // public InoRobot() : base()
        //{
            //绑定网口
            //this.tcplink = TcpClientMgr.GetTcpLink(1);
          //  IpAddr = TcpClientMgr.GetTcpLink(1).m_strIP;


       // }

        public struct _ErrorCode
        {
            public string _Code;
            public string _ErrInfo;
            public string _ErrReason;
            public string _Method;
        }

        /// <summary>
        /// API连接机械手（不是TCP连接）
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            Int32 ret = 0;
            ret = IMC100API.IMC100_Init_ETH(IpAddr, Port, 5, comID);
            if (ret < 0)
            {
                return false;
            }

            //强制获取控制权
            ret = IMC100API.IMC100_AcqPermit(1, comID);
            if (ret < 0)
            {
                return false;
            }
            b_Permit = true;

            //登陆到管理模式(可依据实际函数需求选择登陆的模式)
            Int32 type = 3;//type：用户模式，0-客户模式，1-编辑模式，2-管理模式，3-厂家模式
            String passwordStr = "297995";
            Byte[] password = new Byte[8];
            password = System.Text.Encoding.Default.GetBytes(passwordStr);
            ret = IMC100API.IMC100_UserLogin(type, password, comID);
            if (ret < 0)
            {
                return false;
            }

            //切换到示教模式
            ret = IMC100API.IMC100_Set_Mode(1, comID);
            if (ret < 0)
            {
                return false;
            }

            //关闭数据流模式
            ret = IMC100API.IMC100_DsMode(0, comID);
            if (ret < 0)
            {
                return false;
            }



            //连接成功
            return true;
        }
        /// <summary>
        /// API断开连接机械手（不是TCP连接）
        /// </summary>
        /// <returns></returns>
        public bool disConnect()
        {
            Int32 ret = 0;
            if (b_Permit)
            {
                ret = IMC100API.IMC100_RemovePermit(comID);
                if (ret < 0)
                {
                    return false;
                }
            }
            ret = IMC100API.IMC100_Exit_ETH(comID);
            if (ret < 0)
            {
                return false;
            }
            return true;
        }

        public bool servoON()
        {
            Int32 ret = 0;
            ret = IMC100API.IMC100_MotorEnable(1, comID);
            if (ret < 0)
            {
                return false;
            }
            return true;
        }

        public bool servoOFF()
        {
            Int32 ret = 0;
            ret = IMC100API.IMC100_MotorEnable(0, comID);
            if (ret < 0)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 切换到自动模式（API连接的情况下，切到自动模式与启动机械手之间需要延时）
        /// </summary>
        /// <returns></returns>
        public bool switchAutoMode()
        {
            Int32 ret = 0;
            ret = IMC100API.IMC100_Set_Mode(2, comID);
            if (ret < 0)
            {
                return false;
            }
            return true;
        }

        public bool start()
        {
            Int32 ret = 0;
            ret = IMC100API.IMC100_PrgCtrl(1, comID);
            if (ret < 0)
            {
                return false;
            }
            return true;
        }

        public bool pause()
        {
            Int32 ret = 0;
            ret = IMC100API.IMC100_PrgCtrl(0, comID);
            if (ret < 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取报警信息
        /// </summary>
        /// <returns>0：正常；其他：异常</returns>
        public string getErrorInfo()
        {
            Int32 ret = 0;
            Int32 sts = 0;
            string strs = "0";
            ret = IMC100API.IMC100_Get_SysErrSts(ref sts, comID);
            if (ret != 0)
            {
                if (sts == 1)//1代表报警
                {
                    ret = IMC100API.IMC100_Get_SysErr(ref sts, comID);
                    if (sts != 0)
                    {
                        strs = "0x" + String.Format("{0:X4}", sts);
                        foreach (var item in m_strErrcode)
                        {
                            if (strs == item._Code)//第一次比对得到的是中文，如果需要英文，就用第二次比对到的内容，或者从配置文件中删除
                            {
                                return item._ErrInfo + "\r\n" +
                                    "问题原因:" + item._ErrReason + "\r\n" +
                                    "处理方案:" + item._Method;
                            }
                        }
                    }
                }
                if (ret == 2)
                {
                    //警告暂不做处理
                }
            }
            return "0";
        }

        /// <summary>
        /// 故障复位（不复位急停）
        /// </summary>
        /// <returns></returns>
        public bool ResetError()
        {
            Int32 ret = 0;
            ret = IMC100API.IMC100_ResetErr(comID);
            if (ret < 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取急停状态
        /// </summary>
        /// <returns></returns>
        public int getEStopSts()
        {
            Int32 ret = 0;
            Int32 err = 0;
            ret = IMC100API.IMC100_Get_EStopSts(ref err, comID);
            return err;
        }

        /// <summary>
        /// 程序指针复位
        /// </summary>
        /// <returns></returns>
        public bool ppToStartLine()
        {
            Int32 ret = 0;
            ret = IMC100API.IMC100_BackStartLine(comID);
            if (ret < 0)
            {
                return false;
            }
            return true;
        }

        private List<_ErrorCode> m_strErrcode = new List<_ErrorCode>();
        /// <summary>
        /// 读取ErrcodeCsv
        /// </summary>
        private void ReadErrcodeCsv()
        {
            try
            {
                string m_strFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
                Csv csvErrcode = new Csv(m_strFilePath + "\\InoRobotError.csv", Encoding.GetEncoding("GB2312"));
                if (!File.Exists(m_strFilePath + "\\InoRobotError.csv"))
                {
                    MessageBox.Show($"InoRobotError.csv文档不存在，请联系软件工程师检查配置文件！\r\n " +
                        $"InoRobotError.csv do not exist, please contact system engineer to check configuration files! \r\n " +
                        $"InoRobotError.csv không tồn tại, xin vui lòng liên hệ với kỹ sư hệ thống để kiểm tra hồ sơ!  ",
                        "警告-Warnning-Cảnh báo");
                }

                csvErrcode.LoadCsvFile();

                for (int nRow = 0; nRow < csvErrcode.RowCount; nRow++)
                {
                    _ErrorCode _ErrCode = new _ErrorCode();
                    _ErrCode._Code = csvErrcode[nRow + 1, 1];
                    _ErrCode._ErrInfo = csvErrcode[nRow + 1, 2];
                    _ErrCode._ErrReason = csvErrcode[nRow + 1, 3];
                    _ErrCode._Method = csvErrcode[nRow + 1, 4];
                    m_strErrcode.Add(_ErrCode);
                }
            }
            catch
            {
                MessageBox.Show($"InoRobotError.csv文档不存在，请联系软件工程师检查配置文件！\r\n " +
                        $"InoRobotError.csv do not exist, please contact system engineer to check configuration files! \r\n " +
                        $"InoRobotError.csv không tồn tại, xin vui lòng liên hệ với kỹ sư hệ thống để kiểm tra hồ sơ!  ",
                        "警告-Warnning-Cảnh báo");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AF.Common;

namespace Autoframe
{
    #region 共享内存
    public enum MemoryMapBit
    {
        Bit1,
        Bit2,
    }

    public enum MemoryMapInt
    {
        Int1,
        Int2,
    }
    public enum MemoryMapDouble
    {
        Double1,
        Double2,
    }

    //单个String 不能超过256个字节
    public enum MemoryMapString
    {
        String1,
        String2,
    }
    #endregion

    /// <summary>
    /// 工具类
    /// </summary>
    public class ProgramToolEx: ProgramTool
    {
        /// <summary>
        /// 连接共享文件夹
        /// </summary>
        /// <param name="path">共享路径</param>
        /// <param name="user">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="error">输出错误</param>
        /// <returns></returns>
        /// <remarks>
        /// Windows 网络共享文件夹连接验证
        /// </remarks>
        public static bool ConnectShareFolder(string path, string user, string password, out string error)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";//启动Windows命令提示符
            process.StartInfo.CreateNoWindow = true;//不显示cmd黑窗口（静默执行）
            process.StartInfo.UseShellExecute = false;//禁用Shell执行，才能重定向输入输出
            process.StartInfo.RedirectStandardInput = true;//重定向输入：向cmd输入命令
            process.StartInfo.RedirectStandardOutput = true;//重定向输出：读取cmd执行结果
            process.StartInfo.RedirectStandardError = true;//重定向错误：读取cmd执行错误

            process.Start();

            //持久化连接（重启电脑后仍保留该共享连接；设为 NO 则仅临时有效）
            process.StandardInput.WriteLine(string.Format("net use {0} /USER:{1} {2} /PERSISTENT:YES", path, user, password));
            process.StandardInput.WriteLine("exit");

            process.WaitForExit();
            error = process.StandardError.ReadToEnd().Trim();
            return string.IsNullOrEmpty(error);
        }
    }
}

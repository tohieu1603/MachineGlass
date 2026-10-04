using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace Autoframe.UI
{
    /// <summary>MES 接口测试界面：接口地址 / Request / Response，每次测试保存 txt 到 D:\exe\Mes\yyyyMMdd.txt</summary>
    public partial class Form_MesTest2 : Form
    {
        /// <summary>测试记录保存目录（固定）</summary>
        private static readonly string SaveDir = @"D:\exe\Mes";

        public Form_MesTest2()
        {
            InitializeComponent();
            LoadConfig();
            lblSave.Text = "测试记录将保存到: " + SaveDir + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
        }

        // ===== 配置读取（App.config / exe.config）=====
        private static string GetCfg(string key, string def)
        {
            try { var v = ConfigurationManager.AppSettings[key]; return string.IsNullOrEmpty(v) ? def : v; }
            catch { return def; }
        }

        private void LoadConfig()
        {
            try
            {
                txtUrl.Text = GetCfg("MesTestUrl", "http://10.18.80.21:8081/api/getwomodel");
                // 自动上传开关不再在界面上显示，保留配置项行为（不影响生产自动上传）
                MesClient.AutoUpload = GetCfg("MesAutoUpload", "false") == "true";
            }
            catch { }
        }

        private void SaveUrlConfig()
        {
            try
            {
                var cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var sec = (AppSettingsSection)cfg.GetSection("appSettings");
                if (sec.Settings["MesTestUrl"] != null) sec.Settings["MesTestUrl"].Value = txtUrl.Text.Trim();
                else sec.Settings.Add("MesTestUrl", txtUrl.Text.Trim());
                cfg.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch { }
        }

        // ===== 发送测试 =====
        private async void btnSend_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();
            string req = txtRequest.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("请填写接口地址", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //if (string.IsNullOrEmpty(req))
            //{
            //    MessageBox.Show("请填写 Request 内容", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            btnSend.Enabled = false;
            btnSend.Text = "发送中...";
            string resp="";
            try
            {
                if (comboBox1.SelectedIndex == 0)
                {
                    resp = Get(url);
                }
                else if (comboBox1.SelectedIndex == 1)
                {
                    resp = await PostJsonAsync(url, req);
                }
                
            }
            catch (Exception ex)
            {
                resp = "请求异常: " + ex.Message;
            }
            txtResponse.Text = resp;
            btnSend.Enabled = true;
            btnSend.Text = "发送测试";

            // 每次测试都把 接口地址 + Request + Response 保存到 D:\exe\Mes\yyyyMMdd.txt
            SaveLog(url, req, resp);
        }
        /// <summary>发送 GET 请求（V1.1 getwomodel 用），返回原始响应文本</summary>
        private static string Get(string url, int timeoutMs = 8000)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.Timeout = timeoutMs;
            req.ReadWriteTimeout = timeoutMs;
            req.KeepAlive = false;
            using (var resp = (HttpWebResponse)req.GetResponse())
            using (var sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                return sr.ReadToEnd();
        }
        private static async System.Threading.Tasks.Task<string> PostJsonAsync(string url, string json)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/json; charset=utf-8";
            req.Timeout = 8000;
            req.ReadWriteTimeout = 8000;

            byte[] body = Encoding.UTF8.GetBytes(json);
            using (var s = await req.GetRequestStreamAsync())
                await s.WriteAsync(body, 0, body.Length);

            try
            {
                using (var resp = (HttpWebResponse)await req.GetResponseAsync())
                using (var sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    return await sr.ReadToEndAsync();
            }
            catch (WebException wex)
            {
                // 服务器返回 4xx/5xx 时，响应体也读出来，方便排查
                if (wex.Response != null)
                {
                    try
                    {
                        using (var sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                            return "HTTP 错误(" + (int)((HttpWebResponse)wex.Response).StatusCode + "):\r\n" + await sr.ReadToEndAsync();
                    }
                    catch { }
                }
                throw;
            }
        }

        private void SaveLog(string url, string req, string resp)
        {
            try
            {
                if (!Directory.Exists(SaveDir)) Directory.CreateDirectory(SaveDir);
                string file = Path.Combine(SaveDir, DateTime.Now.ToString("yyyyMMdd") + ".txt");
                var sb = new StringBuilder();
                sb.AppendLine("==================================================");
                sb.AppendLine("时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendLine("接口地址: " + url);
                sb.AppendLine("Request:");
                sb.AppendLine(req);
                sb.AppendLine("Response:");
                sb.AppendLine(resp);
                sb.AppendLine();
                System.IO.File.AppendAllText(file, sb.ToString(), Encoding.UTF8);
                lblSave.Text = "已保存: " + file;
                SaveUrlConfig();
            }
            catch (Exception ex)
            {
                lblSave.Text = "保存失败: " + ex.Message;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                txtUrl.Text = "http://10.18.80.21:8081/api/getwomodel?woId=CY-SF02-2609180075&domainId=VNML";
                txtRequest.Text = "";
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                txtUrl.Text = "http://10.18.80.21:8081/api/savetestdata";
            }


        }
    }
}

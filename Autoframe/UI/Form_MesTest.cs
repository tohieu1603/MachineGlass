using System;
using System.Configuration;
using System.Windows.Forms;

namespace Autoframe.UI
{
    /// <summary>MES 数据采集接口：上传与测试界面（地址可配置、手动切换机种、调用日志）</summary>
    public partial class Form_MesTest : Form
    {
        public Form_MesTest()
        {
            InitializeComponent();
            LoadConfig();
            MesClient.Log += OnMesLog;
            AppendLog("MES 上传/测试界面已就绪。");
        }

        // ===== 配置读写（App.config / exe.config） =====
        private string GetCfg(string key, string def)
        {
            try { var v = ConfigurationManager.AppSettings[key]; return string.IsNullOrEmpty(v) ? def : v; }
            catch { return def; }
        }

        private void LoadConfig()
        {
            txtGetUrl.Text = MesClient.GetWoModelUrl = GetCfg("MesGetWoModelUrl", MesClient.GetWoModelUrl);
            txtSaveUrl.Text = MesClient.SaveTestDataUrl = GetCfg("MesSaveTestDataUrl", MesClient.SaveTestDataUrl);
            cboDomain.Text = MesClient.DomainId = GetCfg("MesDomainId", "VNLP");
            txtEqp.Text = MesClient.EqpId = GetCfg("MesEqpId", "AOI-001");
            txtWo.Text = MesClient.WoId = GetCfg("MesWoId", "");
            cboModel.Text = MesClient.Model = GetCfg("MesModel", "");
            chkAuto.Checked = MesClient.AutoUpload = GetCfg("MesAutoUpload", "false") == "true";
        }

        private void SaveConfig()
        {
            try
            {
                var cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                SetCfg(cfg, "MesGetWoModelUrl", txtGetUrl.Text.Trim());
                SetCfg(cfg, "MesSaveTestDataUrl", txtSaveUrl.Text.Trim());
                SetCfg(cfg, "MesDomainId", cboDomain.Text.Trim());
                SetCfg(cfg, "MesEqpId", txtEqp.Text.Trim());
                SetCfg(cfg, "MesWoId", txtWo.Text.Trim());
                SetCfg(cfg, "MesModel", cboModel.Text.Trim());
                SetCfg(cfg, "MesAutoUpload", chkAuto.Checked ? "true" : "false");
                cfg.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                ApplyUiToClient();
                AppendLog("✓ 配置已保存到 " + AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            }
            catch (Exception ex)
            {
                AppendLog("✗ 配置保存失败: " + ex.Message);
            }
        }

        private static void SetCfg(Configuration cfg, string key, string value)
        {
            var sec = (AppSettingsSection)cfg.GetSection("appSettings");
            if (sec.Settings[key] != null) sec.Settings[key].Value = value;
            else sec.Settings.Add(key, value);
        }

        private void ApplyUiToClient()
        {
            MesClient.GetWoModelUrl = txtGetUrl.Text.Trim();
            MesClient.SaveTestDataUrl = txtSaveUrl.Text.Trim();
            MesClient.DomainId = cboDomain.Text.Trim();
            MesClient.EqpId = txtEqp.Text.Trim();
            MesClient.WoId = txtWo.Text.Trim();
            MesClient.Model = cboModel.Text.Trim();
            MesClient.AutoUpload = chkAuto.Checked;
        }

        // ===== 日志 =====
        private void OnMesLog(string msg)
        {
            if (IsDisposed) return;
            try
            {
                if (IsHandleCreated) BeginInvoke(new Action<string>(AppendLog), msg);
                else AppendLog(msg);
            }
            catch { }
        }

        private void AppendLog(string msg)
        {
            txtLog.AppendText(msg + Environment.NewLine);
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        // ===== 按钮事件 =====
        private async void btnGetModel_Click(object sender, EventArgs e)
        {
            ApplyUiToClient();
            if (string.IsNullOrWhiteSpace(txtWo.Text))
            {
                AppendLog("✗ 请先填写工单号 woId");
                return;
            }
            btnGetModel.Enabled = false;
            try
            {
                var r = await MesClient.GetWoModel(txtWo.Text.Trim());
                if (r.Code == 0 && !string.IsNullOrEmpty(MesClient.Model))
                {
                    cboModel.Text = MesClient.Model;
                    lblModelResult.Text = "机种: " + MesClient.Model;
                }
                else
                    lblModelResult.Text = r.Error != null ? "获取失败: " + r.Error : "获取失败: code=" + r.Code + " " + r.Msg;
            }
            finally { btnGetModel.Enabled = true; }
        }

        private async void btnTestUpload_Click(object sender, EventArgs e)
        {
            ApplyUiToClient();
            AppendLog("—— 测试上传 savetestdata（示例数据） ——");
            btnTestUpload.Enabled = false;
            try
            {
                await MesClient.SaveTestData(MesClient.BuildSample());
            }
            finally { btnTestUpload.Enabled = true; }
        }

        private async void btnUploadRing_Click(object sender, EventArgs e)
        {
            ApplyUiToClient();
            AppendLog("—— 上传当前钢环 savetestdata ——");
            btnUploadRing.Enabled = false;
            try
            {
                await MesClient.UploadRing(RingMonitor.GetSnapshot());
            }
            finally { btnUploadRing.Enabled = true; }
        }

        private void btnSaveCfg_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void chkAuto_CheckedChanged(object sender, EventArgs e)
        {
            ApplyUiToClient();
        }
    }
}

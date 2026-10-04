using AF.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Autoframe
{
    /// <summary>MES 数据采集接口客户端（AOI -> MES），HTTP+JSON，地址可配置，调用失败不影响设备生产</summary>
    public static class MesClient
    {
        /// <summary>日志回调（线程安全由调用方处理）</summary>
        public static event Action<string> Log;

        // ===== 可配置参数（窗体加载时从 App.config 读入，保存时写回） =====
        public static string GetWoModelUrl = "http://10.18.80.21:8081/api/getwomodel";
        public static string SaveTestDataUrl = "http://10.18.80.21:8081/api/savetestdata";
        public static string DomainId = "VNLP";          // VNLP / VNML
        public static string EqpId = "AOI-001";
        public static string WoId = "";
        public static string Model = "";
        public static bool AutoUpload = false;           // 每完成一个钢环自动上传
        private static string SaveDir = @"D:\exe\Mes";

        private static void Emit(string msg)
        {
            var h = Log;
            if (h != null) h(DateTime.Now.ToString("HH:mm:ss") + "  " + msg);
        }

        /// <summary>发送 POST JSON 请求，返回原始响应文本；异常抛出由调用方记录</summary>
        private static string PostJson(string url, string json, int timeoutMs = 3000)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/json; charset=utf-8";
            req.Timeout = timeoutMs;
            req.ReadWriteTimeout = timeoutMs;
            req.KeepAlive = false;
            byte[] body = Encoding.UTF8.GetBytes(json);
            req.ContentLength = body.Length;
            using (var s = req.GetRequestStream()) s.Write(body, 0, body.Length);
            using (var resp = (HttpWebResponse)req.GetResponse())
            using (var sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                return sr.ReadToEnd();

        }
        /// <summary>发送 GET 请求（V1.1 getwomodel 用），返回原始响应文本</summary>
        public static string Get(string url, int timeoutMs = 3000)
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
        /// <summary>请求结果封装</summary>
        public class MesResult
        {
            public bool Success;     // HTTP 调用成功（不一定 code==0）
            public int Code;         // 业务 code：0 成功 / 500 失败
            public string Msg;       // 业务 msg
            public string Raw;       // 原始响应
            public string Error;     // 异常信息（HTTP 失败时）
        }

        private static MesResult Parse(string raw, string err)
        {
            var r = new MesResult();
            if (!string.IsNullOrEmpty(err)) { r.Error = err; return r; }
            r.Success = true;
            r.Raw = raw;
            try
            {
                var jo = JObject.Parse(raw);
                r.Code = jo["code"] != null ? (int)jo["code"] : -1;
                r.Msg = jo["msg"] != null ? (string)jo["msg"] : "";
            }
            catch (Exception ex)
            {
                r.Error = "响应解析失败: " + ex.Message + "\n" + raw;
            }
            return r;
        }

        /// <summary>获取工单机种（getwomodel）</summary>
        public static async Task<MesResult> GetWoModel(string woId)
        {
            string domain = DomainId;
            string json = JsonConvert.SerializeObject(new { domainId = domain, woId = woId });
            Emit("→ getwomodel  " + GetWoModelUrl + "  " + json);
            string raw = null, err = null;
            try { raw = await Task.Run(() => PostJson(GetWoModelUrl, json)); }
            catch (Exception ex) { err = ex.Message; }
            var r = Parse(raw, err);
            if (!string.IsNullOrEmpty(r.Error)) Emit("✗ getwomodel " + r.Error);
            else
            {
                Emit("← code=" + r.Code + " msg=" + r.Msg + "  " + r.Raw);
                if (r.Code == 0 && r.Raw != null)
                {
                    try
                    {
                        var m = JObject.Parse(r.Raw)["data"]?["model"]?.ToString();
                        if (!string.IsNullOrEmpty(m)) { Model = m; Emit("✓ 机种: " + m); }
                    }
                    catch { }
                }
            }
            return r;
        }

        /// <summary>上传生产/质量数据（savetestdata）</summary>
        public static async Task<MesResult> SaveTestData(MesTestData data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.None);
            Emit("→ savetestdata  " + SaveTestDataUrl + "  " + json);
            string raw = null, err = null;
            try { raw = await Task.Run(() => PostJson(SaveTestDataUrl, json)); }
            catch (Exception ex) { err = ex.Message; }
            var r = Parse(raw, err);
            if (!string.IsNullOrEmpty(r.Error)) Emit("✗ savetestdata " + r.Error);
            else Emit("← code=" + r.Code + " msg=" + r.Msg + "  " + r.Raw);
            return r;
        }

        /// <summary>由钢环快照组包（供测试按钮/上传当前环/自动上传共用）</summary>
        public static MesTestData BuildFromSnapshot(RingMonitor.Snapshot s)
        {
            var d = new MesTestData
            {
                testDate = s.RingTime.ToString("yyyy-MM-dd"),
                domainId = DomainId,
                eqpId = EqpId,
                woId = WoId,
                model = Model,
                startTime = s.RingTime.ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                operatingTime = (int)Math.Max(1, (DateTime.Now - s.RingTime).TotalSeconds),
                inputQty = (int)s.Total,
                goodQty = (int)s.Ok,
                badQty = (int)s.Ng,
                defectList = new List<MesDefect>()
            };
            // NG 原因按名称汇总
            var ng = s.Records.Where(x => x.Result == false && !string.IsNullOrEmpty(x.NGReason)).ToList();
            if (ng.Count == 0 && s.Ng > 0)
                ng = s.Records.Where(x => x.Result == false).ToList();
            var groups = ng.GroupBy(x => x.NGReason ?? "NG");
            int idx = 0;
            foreach (var g in groups)
            {
                idx++;
                d.defectList.Add(new MesDefect
                {
                    defectCode = "D" + idx.ToString("00"),
                    defectName = string.IsNullOrEmpty(g.Key) ? "不良" + idx : g.Key,
                    qty = g.Count()
                });
            }
            return d;
        }

        /// <summary>上传当前钢环（手动/自动共用），失败仅记日志不抛错（不影响生产）</summary>
        public static async Task UploadRing(RingMonitor.Snapshot snap)
        {
            if (snap == null || snap.Total <= 0) { Emit("⏭ 当前环无数据，跳过上传"); return; }
            var data = BuildFromSnapshot(snap);
            await SaveTestData(data);
        }

        /// <summary>上传上一环（换环自动上传用，RingImageData 无 NG 原因明细，按 NG 单类统计）</summary>
        public static async Task UploadRing(RingMonitor.RingImageData ring)
        {
            if (ring == null || ring.Total <= 0) { Emit("⏭ 上一环无数据，跳过上传"); return; }
            var data = new MesTestData
            {
                testDate = ring.RingTime.ToString("yyyy-MM-dd"),
                domainId = DomainId,
                eqpId = EqpId,
                woId = WoId,
                model = Model,
                startTime = ring.RingTime.ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                operatingTime = (int)Math.Max(1, (DateTime.Now - ring.RingTime).TotalSeconds),
                inputQty = ring.Total,
                goodQty = ring.OkCount,
                badQty = ring.NgCount,
                defectList = new List<MesDefect>()
            };
            if (ring.NgCount > 0)
                data.defectList.Add(new MesDefect { defectCode = "D01", defectName = "NG", qty = ring.NgCount });
            await SaveTestData(data);
        }

        /// <summary>构造一份界面示例数据（测试接口连通性）</summary>
        public static MesTestData BuildSample()
        {
            return new MesTestData
            {
                testDate = DateTime.Now.ToString("yyyy-MM-dd"),
                domainId = DomainId,
                eqpId = EqpId,
                woId = string.IsNullOrEmpty(WoId) ? "CY-SF02-2608010001" : WoId,
                model = string.IsNullOrEmpty(Model) ? "PKK4-CGY4 WIDE" : Model,
                startTime = DateTime.Now.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                operatingTime = 300,
                inputQty = 1000,
                goodQty = 900,
                badQty = 100,
                defectList = new List<MesDefect>
                {
                    new MesDefect { defectCode = "D01", defectName = "不良1", qty = 10 },
                    new MesDefect { defectCode = "D02", defectName = "不良2", qty = 90 }
                }
            };
        }
        private static void SaveLog(string url, string req, string resp)
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
                SaveUrlConfig();
            }
            catch (Exception ex)
            {
                EventMgr.Warn("保存失败: " + ex.Message, "mes");
            }
        }
        private static void SaveUrlConfig()
        {
            try
            {
                var cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var sec = (AppSettingsSection)cfg.GetSection("appSettings");
                cfg.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch { }
        }
    }

    /// <summary>savetestdata 请求体</summary>
    public class MesTestData
    {
        public string testDate;
        public string domainId;
        public string eqpId;
        public string woId;
        public string model;
        public string startTime;
        public string endTime;
        public int operatingTime;
        public int inputQty;
        public int goodQty;
        public int badQty;
        public List<MesDefect> defectList;
    }

    /// <summary>不良明细</summary>
    public class MesDefect
    {
        public string defectCode;
        public string defectName;
        public int qty;
    }
}

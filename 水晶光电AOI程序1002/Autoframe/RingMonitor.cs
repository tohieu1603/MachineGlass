using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Configuration;

namespace Autoframe
{
    /// <summary>
    /// 钢环检测结果监控数据源（线程安全）。
    /// 写入端：
    ///   扩膜站 —— CCD1识别料盘规格后 SetRingLayout(行,列,空跑)（生产行列取自CCD1，空跑用参数3×4）；每取出一颗 CCD2拍照后 TakeOut(环内料位号)
    ///   IR检测1/2 —— 摆臂放料完成接料时 AssignNextPos(ProductData)，把全局料位序号写入产品
    ///   AR检测1/2 —— 最终判定（CCD6 AND CCD3结果）后 OnResult(序号,结果,NG原因)
    /// 读取端：
    ///   窗体 Form_RingMonitor 轮询 GetSnapshot()，连续30片/累计50片NG时弹报警，确认后复位滚动计数。
    /// 参数（App.config appSettings，可现场修改）：
    ///   RingRows / RingCols —— 生产料盘默认行×列（CCD1未反馈时使用，默认 17×19=323）
    ///   DryRunRows / DryRunCols —— 空跑规格（默认 3×4）
    ///   RingImagePath —— 钢环NG/OK图片保存目录（默认 exe目录\RingImages）
    ///   RingImageSave —— 是否保存钢环图片（true/false，默认 true）
    /// 图片保存：每做完一个钢环（换环时或环内全部穴位判定完成），自动把该钢环料盘图保存为 PNG
    ///   （含 NG 命名为 RingN_时间_NGx片.png，全部 OK 命名为 RingN_时间_OK.png）。
    /// </summary>
    public static class RingMonitor
    {
        public static int 连续NG报警阈值 = 300;   // 连续出现30片NG报警
        public static int 累计NG报警阈值 = 500;   // 累计50片NG报警

        // ===== 参数（App.config 可设置）=====
        public static readonly int 默认行数 = GetCfgInt("RingRows", 17);
        public static readonly int 默认列数 = GetCfgInt("RingCols", 19);
        public static readonly int 空跑行数 = GetCfgInt("DryRunRows", 3);
        public static readonly int 空跑列数 = GetCfgInt("DryRunCols", 4);
        //public static readonly string 图片保存目录 = GetCfgStr("RingImagePath", "RingImages"); // exe目录下的 RingImages 子目录
        public static readonly string 图片保存目录 = @"D:\exe\RingImages";   // 固定路径，避免现场误改导致图片保存失败
        public static readonly bool 保存钢环图片 = GetCfgBool("RingImageSave", true);

        private static int GetCfgInt(string key, int def)
        {
            try
            {
                string v = ConfigurationManager.AppSettings[key];
                int r;
                return int.TryParse(v, out r) ? r : def;
            }
            catch { return def; }
        }
        private static string GetCfgStr(string key, string def)
        {
            try
            {
                string v = ConfigurationManager.AppSettings[key];
                return string.IsNullOrEmpty(v) ? def : v;
            }
            catch { return def; }
        }
        private static bool GetCfgBool(string key, bool def)
        {
            try
            {
                string v = ConfigurationManager.AppSettings[key];
                bool r;
                return bool.TryParse(v, out r) ? r : def;
            }
            catch { return def; }
        }

        private static readonly object _lock = new object();

        // 当前环
        private static int _ringNo = 0;            // 环号（换环自增）
        private static int _rows = 默认行数;         // 当前环行数（CCD1反馈；空跑固定3；参数可设默认）
        private static int _cols = 默认列数;         // 当前环列数（CCD1反馈；空跑固定4；参数可设默认）
        private static long _ringStart = 0;        // 当前环第一颗料的全局序号（全局序号从1递增）
        private static long _nextAlloc = 1;        // 下一个可分配的全局序号
        private static bool _dryRun = false;       // 当前是否空跑
        private static DateTime _ringTime = DateTime.Now;
        private static int _ringDone = 0;          // 当前环已判定完成数（用于环完成后保存图片）
        private static int _savedRingNo = 0;       // 已保存图片的环号（防重复保存）

        // 在途队列：扩膜站已取出、尚未被IR接料分配料位号的全局序号
        private static readonly Queue<long> _pending = new Queue<long>();

        // 全局序号 → 料状态
        private class Cell
        {
            public bool? Result;      // null=待检/在途
            public string NGReason = "";
            public DateTime T = DateTime.Now;
        }
        private static readonly Dictionary<long, Cell> _cells = new Dictionary<long, Cell>();

        // 统计
        private static long _total, _ok, _ng;      // 总统计（不随报警确认清零）
        private static int _consecNG;              // 连续NG滚动计数
        private static int _alarmTotalNG;          // 累计NG滚动计数（报警确认后清零重计）
        private static bool _alarmConsecFired;     // 连续报警是否已触发待确认
        private static bool _alarmTotalFired;      // 累计报警是否已触发待确认
        private static readonly List<AlarmRecord> _alarmLog = new List<AlarmRecord>();
        private static readonly List<ResultRecord> _records = new List<ResultRecord>();

        public class AlarmRecord
        {
            public string Time;
            public string Type;      // 连续NG / 累计NG
            public int Count;
            public int Limit;
            public bool Acknowledged;
            public string AckTime = "";
        }

        /// <summary>单片结果明细（最新在列表末尾，窗体倒序显示）</summary>
        public class ResultRecord
        {
            public long Seq;         // 全局序号
            public int RingNo;       // 环号
            public int RingPos;      // 环内蛇形料位号
            public bool Result;
            public string NGReason;
            public string Time;
        }

        /// <summary>料盘快照（窗体轮询用）</summary>
        public class Snapshot
        {
            public int RingNo, Rows, Cols;
            public bool DryRun;
            public long RingStart;
            public DateTime RingTime;
            public long Total, Ok, Ng;
            public int ConsecNG, AlarmTotalNG;
            public bool AlarmConsecFired, AlarmTotalFired;
            public List<CellView> Cells = new List<CellView>();     // 当前环每格
            public List<AlarmRecord> AlarmLog = new List<AlarmRecord>();
            public List<ResultRecord> Records = new List<ResultRecord>();   // 单片结果明细
        }
        public class CellView
        {
            public int Pos;              // 环内蛇形料位号 1..N
            public long Seq;             // 全局序号
            public bool? Result;
            public string NGReason;
        }

        /// <summary>钢环图片数据（锁内收集、锁外绘制保存，避免阻塞产线写入端）</summary>
        public class RingImageData
        {
            public int RingNo, Rows, Cols;
            public bool DryRun;
            public DateTime RingTime;
            public int OkCount, NgCount, Total;
            public bool?[] Cells;        // 按蛇形料位号 1..N
        }

        /// <summary>设置当前环料盘规格（CCD1反馈；空跑时3行4列；参数可设默认）。换环时自动保存上一环图片</summary>
        public static void SetRingLayout(int rows, int cols, int ringSit,bool dryRun)
        {
            RingImageData prev = null;
            _ringNo=ringSit;
            lock (_lock)
            {
                // 换环：保存上一环当前状态图片（每做完一个钢环即保存）
                if (_ringNo > 0 && !_dryRun)     //if (_ringNo > 0 && !_dryRun && _savedRingNo != _ringNo) 
                {
                    prev = CollectCurrentRing();
                    _savedRingNo = _ringNo;
                }
                //_ringNo++;
                _rows = Math.Max(1, rows);
                _cols = Math.Max(1, cols);
                _dryRun = dryRun;
                _ringTime = DateTime.Now;
                _ringStart = _nextAlloc;    // 当前环 = 序号 [ringStart, ringStart+rows*cols-1]
                _ringDone = 0;
            }
            if (prev != null) SaveRingImage(prev);   // 锁外绘制保存
            if (prev != null && MesClient.AutoUpload)
            {
                var ring = prev;   // 锁外异步上传，失败仅记日志，不影响设备生产
                System.Threading.Tasks.Task.Run(async () => { try { await MesClient.UploadRing(ring); } catch { } });
            }
        }

        /// <summary>扩膜站取出一颗料（n拍照次数++后调用），环内料位号按蛇形顺序</summary>
        public static void TakeOut(int ringPos)
        {
            lock (_lock)
            {
                if (_dryRun) return;   // 空跑模式不统计
                long seq = _nextAlloc++;
                _pending.Enqueue(seq);
                _cells[seq] = new Cell();
                // 超出当前环容量的序号（上一环在途），不参与当前环显示
            }
        }

        /// <summary>IR站接料时调用：把全局料位序号写入产品，供AR最终判定回写</summary>
        public static void AssignNextPos(ProductData pd)
        {
            //if (pd == null) return;
            //lock (_lock)
            //{
            //    if (_pending.Count > 0)
            //        pd.RingPos = (int)_pending.Dequeue();
            //    else
            //        pd.RingPos = 0;
            //}
        }

        /// <summary>AR站最终判定后调用：写入一颗料的综合结果（IR∩AR，任一NG即NG）。
        /// 当前环全部穴位判定完成时，自动保存该钢环图片</summary>
        public static void OnResult(int seq, bool result, string ngReason)
        {
            RingImageData done = null;
            lock (_lock)
            {
                if (_dryRun) return;   // 空跑模式不统计
                Cell c;
                if (seq > 0 && _cells.TryGetValue(seq, out c)) //
                {
                    if (c.Result != null) return;   // 防重复
                    c.Result = result;
                    c.NGReason = ngReason ?? "";
                    c.T = DateTime.Now;
                    if (seq >= _ringStart && seq < _ringStart + (long)_rows * _cols)
                        _ringDone--;
                }
                else
                {
                    // 序号丢失/越界（如手动清料），仍计入总统计但不进环格
                    _cells[seq] = new Cell { Result = result, NGReason = ngReason ?? "", T = DateTime.Now };
                }
                if (seq >= _ringStart && seq < _ringStart + (long)_rows * _cols)
                    //_ringDone++;
                _records.Add(new ResultRecord
                {
                    Seq = seq,
                    RingNo = _ringNo,
                    RingPos = seq >= _ringStart ? (int)(seq - _ringStart + 1) : 0,
                    Result = result,
                    NGReason = ngReason ?? "",
                    Time = DateTime.Now.ToString("HH:mm:ss.fff")
                });

                _total++;
                if (result)
                {
                    _ok++;
                    _consecNG = 0;
                }
                else
                {
                    _ng++;
                    _consecNG++;
                    _alarmTotalNG++;

                    if (_consecNG >= 连续NG报警阈值 && !_alarmConsecFired)
                    {
                        _alarmConsecFired = true;
                        _alarmLog.Add(new AlarmRecord { Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Type = "连续NG报警", Count = _consecNG, Limit = 连续NG报警阈值 });
                    }
                    if (_alarmTotalNG >= 累计NG报警阈值 && !_alarmTotalFired)
                    {
                        _alarmTotalFired = true;
                        _alarmLog.Add(new AlarmRecord { Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Type = "累计NG报警", Count = _alarmTotalNG, Limit = 累计NG报警阈值 });
                    }
                }

                // 当前环全部穴位判定完成：立即保存该钢环图片（双保险，换环时也已保存）
                if (!_dryRun  && _ringDone >= _rows * _cols)      //if (!_dryRun && _savedRingNo != _ringNo && _ringDone >= _rows * _cols)
                {
                    done = CollectCurrentRing();
                    _savedRingNo = _ringNo;
                }
            }
            if (done != null) 
                SaveRingImage(done);   // 锁外绘制保存
        }

        /// <summary>操作员确认报警：复位对应滚动计数，不再重复弹窗</summary>
        public static void AcknowledgeAlarm()
        {
            lock (_lock)
            {
                string ack = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                if (_alarmConsecFired) { _alarmConsecFired = false; _consecNG = 0; }
                if (_alarmTotalFired) { _alarmTotalFired = false; _alarmTotalNG = 0; }
                for (int i = _alarmLog.Count - 1; i >= 0; i--)
                {
                    if (!_alarmLog[i].Acknowledged) { _alarmLog[i].Acknowledged = true; _alarmLog[i].AckTime = ack; }
                }
            }
        }

        /// <summary>手动重置全部统计与当前环</summary>
        public static void ResetAll()
        {
            lock (_lock)
            {
                _total = _ok = _ng = 0;
                _consecNG = _alarmTotalNG = 0;
                _alarmConsecFired = _alarmTotalFired = false;
                _alarmLog.Clear();
                _records.Clear();
                _cells.Clear();
                _pending.Clear();
                _ringNo = 0;
                _ringStart = _nextAlloc = 1;
                _rows = 17; _cols = 19; _dryRun = false;
                _ringDone = 0; _savedRingNo = 0;
            }
        }

        /// <summary>窗体轮询快照</summary>
        public static Snapshot GetSnapshot()
        {
            lock (_lock)
            {
                Snapshot s = new Snapshot
                {
                    RingNo = _ringNo,
                    Rows = _rows,
                    Cols = _cols,
                    DryRun = _dryRun,
                    RingStart = _ringStart,
                    RingTime = _ringTime,
                    Total = _total,
                    Ok = _ok,
                    Ng = _ng,
                    ConsecNG = _consecNG,
                    AlarmTotalNG = _alarmTotalNG,
                    AlarmConsecFired = _alarmConsecFired,
                    AlarmTotalFired = _alarmTotalFired,
                    AlarmLog = _alarmLog.ToList(),
                    Records = _records.ToList()
                };
                int n = _rows * _cols;
                for (int pos = 1; pos <= n; pos++)
                {
                    long seq = _ringStart + pos - 1;
                    Cell c;
                    bool? r = null; string reason = "";
                    if (_cells.TryGetValue(seq, out c)) { r = c.Result; reason = c.NGReason; }
                    s.Cells.Add(new CellView { Pos = pos, Seq = seq, Result = r, NGReason = reason });
                }
                return s;
            }
        }

        // ===== 钢环 NG/OK 图片保存（每完成一个钢环调用）=====

        /// <summary>锁内收集当前环图片数据</summary>
        private static RingImageData CollectCurrentRing()
        {
            RingImageData d = new RingImageData
            {
                RingNo = _ringNo,
                Rows = _rows,
                Cols = _cols,
                DryRun = _dryRun,
                RingTime = _ringTime
            };
            int n = _rows * _cols;
            d.Cells = new bool?[n];
            for (int pos = 1; pos <= n; pos++)
            {
                long seq = _ringStart + pos - 1;
                Cell c;
                if (_cells.TryGetValue(seq, out c)) d.Cells[pos - 1] = c.Result;
            }
            d.OkCount = d.Cells.Count(x => x == true);
            d.NgCount = d.Cells.Count(x => x == false);
            d.Total = d.OkCount + d.NgCount;
            return d;
        }

        private static readonly Color C_OK = Color.FromArgb(232, 246, 238);
        private static readonly Color C_OKBorder = Color.FromArgb(31, 157, 85);
        private static readonly Color C_NG = Color.FromArgb(252, 236, 236);
        private static readonly Color C_NGBorder = Color.FromArgb(214, 69, 69);
        private static readonly Color C_WAIT = Color.FromArgb(240, 242, 245);
        private static readonly Color C_GRAY = Color.FromArgb(201, 210, 220);
        /// <summary>把d当前钢环料盘图保存为 PNG（空跑/无数据环不保存；失败不影响产线）</summary>
        public static void SaveCurrentRingImage()
        {
            RingImageData d = null;
            lock (_lock)
            {
                //       if (!_dryRun && _savedRingNo != _ringNo)
                d = CollectCurrentRing();
                _savedRingNo = _ringNo;


            }
            if (d != null) SaveRingImage(d);   // 锁外绘制保存
        }
        /// <summary>把钢环料盘图保存为 PNG（空跑/无数据环不保存；失败不影响产线）</summary>
        private static void SaveRingImage(RingImageData d)
        {
            if (!保存钢环图片) return;
            if (d == null || d.DryRun || d.Rows <= 0 || d.Cols <= 0 || d.Total <= 0) return;
            try
            {
                string stempPath = AppDomain.CurrentDomain.BaseDirectory;
                
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 图片保存目录+ "\\"+DateTime.Now.ToString("yyyy-MM-dd"));
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                float cw = 44f, ch = 44f;
                int w = (int)(d.Cols * cw) + 28, h = (int)(d.Rows * ch) + 28 + 52;
                using (Bitmap bmp = new Bitmap(w, h))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.White);
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                        string rate = d.Total > 0 ? (100.0 * d.OkCount / d.Total).ToString("0.0") + "%" : "—";
                        string title = string.Format("钢环#{0}  {1}  规格:{2}行×{3}列(共{4}片)  OK:{5}  NG:{6}  良率:{7}",
                            d.RingNo, d.RingTime.ToString("yyyy-MM-dd HH:mm:ss"), d.Rows, d.Cols, d.Rows * d.Cols, d.OkCount, d.NgCount, rate);
                        using (Font f = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold))
                            g.DrawString(title, f, Brushes.Black, new PointF(10, 10));

                        float ox = 14, oy = 52;
                        for (int r = 0; r < d.Rows; r++)
                        {
                            for (int c = 0; c < d.Cols; c++)
                            {
                                // 蛇形编号与现场取料顺序一致
                                int pos = r * d.Cols + (r % 2 == 0 ? c : d.Cols - 1 - c) + 1;
                                bool? res = d.Cells[pos - 1];
                                Color bg, bd;
                                string txt;
                                if (res == true) { bg = C_OK; bd = C_OKBorder; txt = "OK"; }
                                else if (res == false) { bg = C_NG; bd = C_NGBorder; txt = "NG"; }
                                else { bg = C_WAIT; bd = C_GRAY; txt = "#" + pos; }
                                float x = ox + c * cw, y = oy + r * ch;
                                using (Brush br = new SolidBrush(bg)) g.FillRectangle(br, x + 1f, y + 1f, cw - 2f, ch - 2f);
                                using (Pen pen = new Pen(bd)) g.DrawRectangle(pen, x + 0.5f, y + 0.5f, cw - 1f, ch - 1f);
                                using (Font f = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold))
                                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                                {
                                    Brush tb = res == false ? Brushes.Red : (res == true ? Brushes.Green : Brushes.DimGray);
                                    g.DrawString(txt, f, tb, new RectangleF(x, y, cw, ch), sf);
                                }
                            }
                        }
                    }
                    string fn = string.Format("Ring{0}_{1}_{2}.png",
                        d.RingNo, d.RingTime.ToString("yyyyMMdd_HHmmss"),
                        d.NgCount > 0 ? "NG" + d.NgCount + "片" : "OK");
                    bmp.Save(Path.Combine(dir, fn), ImageFormat.Png);
                }
            }
            catch { /* 保存失败不影响产线运行 */ }
        }
    }
}

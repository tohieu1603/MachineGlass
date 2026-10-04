using System;
using System.Drawing;
using System.Windows.Forms;

namespace Autoframe.UI
{
    /// <summary>
    /// 钢环检测结果实时监控窗体。
    /// 显示当前钢环（CCD1反馈的行×列，空跑固定3×4）每颗料的综合检测结果（IR∩AR，任一NG即NG），
    /// 连续30片NG或累计50片NG时弹出报警，确认后复位滚动计数。
    /// 挂载方式：SystemInit.Init() 中 Form_Auto.AddPage(new Form_RingMonitor());
    /// 布局：上部统计区（topPanel/statPanel）与右侧明细栏（rightPanel）为 Designer 设计固定，
    ///       可在 Visual Studio 窗体设计器中直接拖拽修改；料盘网格为自绘控件（大料盘自动压缩
    ///       格子尽量全部显示，极端窗口/高DPI下格子保持最小30×26像素，超出部分由容器滚动条查看）。
    /// 参数：钢环料盘规格（默认17行×19列=323片，空跑3×4）在 App.config 的 appSettings 中设置。
    /// 图片：每完成一个钢环，自动把该钢环 NG/OK 料盘图保存到 exe 目录\RingImages（可配置）。
    /// </summary>
    public partial class Form_RingMonitor : Form
    {
        // ===== 颜色 =====
        private static readonly Color C_OK = Color.FromArgb(232, 246, 238);   // 浅绿
        private static readonly Color C_OKBorder = Color.FromArgb(31, 157, 85);
        private static readonly Color C_NG = Color.FromArgb(252, 236, 236);    // 浅红
        private static readonly Color C_NGBorder = Color.FromArgb(214, 69, 69);
        private static readonly Color C_WAIT = Color.FromArgb(240, 242, 245);  // 待检灰
        private static readonly Color C_PICK = Color.FromArgb(252, 241, 228);  // 已取/在途黄
        private static readonly Color C_PICKBorder = Color.FromArgb(217, 130, 43);
        private static readonly Color C_TXT = Color.FromArgb(31, 35, 40);

        // ===== 运行时控件/状态 =====
        private GridCanvas _grid;
        private Timer _timer;
        private RingMonitor.Snapshot _snap;
        private int _lastRecCount;
        private bool _alarmShown;   // 本地已弹窗标志

        public Form_RingMonitor()
        {
            InitializeComponent();
            //// 右侧新增显示区（覆膜NG/覆膜OK）宽度随窗体缩放：占窗体宽 22%，可自行调整比例
            //this.Resize += (s, e) =>
            //{
            //    int aw = (int)(this.ClientSize.Width * 0.22f);
            //    if (aw < 260) aw = 260;   // 最小宽度
            //    if (aw > 640) aw = 640;   // 最大宽度
            //    areaHost.Width = aw;
            //};
            // 给统计卡片画浅灰边框（Designer 属性无法表达的轻量装饰）
            foreach (Panel p in new Panel[] { kpiTotal, kpiOK, kpiNG, kpiRate, consecGroup, totalGroup })
            {
                p.Paint += (s, e) => e.Graphics.DrawRectangle(Pens.LightGray, 0, 0, p.Width - 1, p.Height - 1);
            }
            // 料盘自绘控件加入 Fill 容器
            _grid = new GridCanvas { BackColor = Color.FromArgb(244, 246, 248) };
            gridHost.Controls.Add(_grid);

            // 明细列（Designer 中 dgv 已定义样式，列在代码中建立，避免设计器序列化复杂列）
            dgv.Columns.Add("cSeq", "序号");
            dgv.Columns.Add("cPos", "料位");
            dgv.Columns.Add("cRes", "结果");
            dgv.Columns.Add("cReason", "NG原因");
            dgv.Columns.Add("cTime", "时间");
            dgv.Columns["cSeq"].FillWeight = 60;
            dgv.Columns["cPos"].FillWeight = 55;
            dgv.Columns["cRes"].FillWeight = 55;
            dgv.Columns["cReason"].FillWeight = 130;
            dgv.Columns["cTime"].FillWeight = 95;

            _timer = new Timer { Interval = 200 };
            _timer.Tick += (s, e) => Poll();
            _timer.Start();
        }

        // ===== Designer 事件 =====
        private void btnAck_Click(object sender, EventArgs e)
        {
            RingMonitor.AcknowledgeAlarm();
            _alarmShown = false;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定重置全部统计与当前环显示？", "重置统计", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                RingMonitor.ResetAll();
                _alarmShown = false;
                _lastRecCount = 0;
            }
        }

        // ===== 轮询刷新 =====
        private void Poll()
        {
            var s = RingMonitor.GetSnapshot();
            _snap = s;

            lblRing.Text = "当前钢环：#" + s.RingNo + (s.DryRun ? "（空跑 " + s.Rows + "×" + s.Cols + "）" : "");
            lblMode.Text = "规格：" + s.Rows + "行 × " + s.Cols + "列（共" + (s.Rows * s.Cols) + "片）";
            lblTime.Text = "开始：" + s.RingTime.ToString("HH:mm:ss");
            lblTotalValue.Text = s.Total.ToString();
            lblOKValue.Text = s.Ok.ToString();
            lblNGValue.Text = s.Ng.ToString();
            lblRateValue.Text = s.Total > 0 ? (100.0 * s.Ok / s.Total).ToString("0.0") + "%" : "—";
            lblConsec.Text = "连续NG /" + RingMonitor.连续NG报警阈值 + "：" + s.ConsecNG;
            lblTotalNG.Text = "累计NG /" + RingMonitor.累计NG报警阈值 + "：" + s.AlarmTotalNG;
            barConsec.Value = Math.Min(barConsec.Maximum, s.ConsecNG);
            barTotal.Value = Math.Min(barTotal.Maximum, s.AlarmTotalNG);

            if (_grid.Rows != s.Rows || _grid.Cols != s.Cols)
            {
                _grid.Rows = s.Rows;
                _grid.Cols = s.Cols;
            }
            ResizeGridCells();      // 每轮校正格尺寸：窗口/DPI变化后自动重算，防裁切
            RefreshGrid(s);

            // 报警记录列表
            if (lbAlarm.Items.Count != s.AlarmLog.Count)
            {
                lbAlarm.Items.Clear();
                for (int i = s.AlarmLog.Count - 1; i >= 0; i--)
                {
                    var a = s.AlarmLog[i];
                    lbAlarm.Items.Add(a.Time + "  " + a.Type + "（" + a.Count + "/" + a.Limit + "）" + (a.Acknowledged ? " 已确认" : " ★待确认"));
                }
            }

            // 单片结果明细：最新在前，增量插入顶部（可上下滚动查看每一片）
            var recs = s.Records;
            if (recs.Count < _lastRecCount) { dgv.Rows.Clear(); _lastRecCount = 0; }   // 统计被重置
            if (recs.Count > 0 && _lastRecCount == 0)
            {
                dgv.Rows.Clear();
                for (int i = recs.Count - 1; i >= 0; i--)
                {
                    var r = recs[i];
                    int idx = dgv.Rows.Add(r.Seq, r.RingPos, r.Result ? "OK" : "NG", r.NGReason, r.Time);
                    StyleResultRow(dgv.Rows[idx], r.Result);
                }
                _lastRecCount = recs.Count;
            }
            else if (recs.Count > _lastRecCount)
            {
                for (int i = _lastRecCount; i < recs.Count; i++)
                {
                    var r = recs[i];
                    dgv.Rows.Insert(0, r.Seq, r.RingPos, r.Result ? "OK" : "NG", r.NGReason, r.Time);
                    StyleResultRow(dgv.Rows[0], r.Result);
                }
                _lastRecCount = recs.Count;
            }

            // 报警弹窗：false→true 转变时弹一次
            if ((s.AlarmConsecFired || s.AlarmTotalFired) && !_alarmShown)
            {
                _alarmShown = true;
                string msg = "";
                if (s.AlarmConsecFired) msg += "连续NG报警：连续出现 " + s.ConsecNG + " 片 NG（阈值 " + RingMonitor.连续NG报警阈值 + "）\n";
                if (s.AlarmTotalFired) msg += "累计NG报警：累计 " + s.AlarmTotalNG + " 片 NG（阈值 " + RingMonitor.累计NG报警阈值 + "）\n";
                msg += "\n请检查来料与设备状态。点击“确定”确认报警并复位对应计数。";
                MessageBox.Show(this, msg, "检测报警", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RingMonitor.AcknowledgeAlarm();
                _alarmShown = false;
            }
        }

        /// <summary>按容器尺寸重算自绘网格大小：格子尽量撑满且不超过容器（避免多余滚动条），
        /// 窗口过小保持最小格30×26时允许出现滚动条查看全部穴位</summary>
        private void ResizeGridCells()
        {
            if (_grid == null || gridHost == null) return;
            int cols = _grid.Cols, rows = _grid.Rows;
            if (cols <= 0 || rows <= 0) return;
            int availW = gridHost.ClientSize.Width - 12;
            int availH = gridHost.ClientSize.Height - 12;
            if (availW < 20) availW = 20;
            if (availH < 20) availH = 20;
            int cw = Math.Max(30, availW / cols);
            int ch = Math.Max(26, availH / rows);
            while (cols * cw + 12 > availW + 2 && cw > 30) cw--;   // 防水平溢出出现滚动条
            while (rows * ch + 12 > availH + 2 && ch > 26) ch--;   // 防垂直溢出出现滚动条
            _grid.Location = new Point(0, 0);
            _grid.Size = new Size(cols * cw + 12, rows * ch + 12);
            _grid.Invalidate();
        }

        private void RefreshGrid(RingMonitor.Snapshot s)
        {
            if (_grid == null) return;
            _grid.Snap = s;
            _grid.Small = s.Rows * s.Cols > 340;   // 大料盘(如17×19)格小：只显示 OK/NG
            _grid.Invalidate();
        }

        private void StyleResultRow(DataGridViewRow row, bool ok)
        {
            if (ok)
            {
                row.DefaultCellStyle.ForeColor = Color.FromArgb(31, 157, 85);
                row.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 8.5F);
            }
            else
            {
                row.DefaultCellStyle.ForeColor = Color.FromArgb(214, 69, 69);
                row.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_timer != null) { _timer.Stop(); _timer.Dispose(); }
            base.OnFormClosing(e);
        }

        // ===== 自绘料盘网格（蛇形编号，与扩膜站取料顺序一致） =====
        private class GridCanvas : Control
        {
            public int Rows, Cols;
            public RingMonitor.Snapshot Snap;
            public bool Small;   // 大料盘单行显示 OK/NG

            private static readonly Brush BR_TXT = new SolidBrush(Color.FromArgb(31, 35, 40));
            private static readonly Brush BR_OK = new SolidBrush(Color.FromArgb(31, 157, 85));
            private static readonly Brush BR_NG = new SolidBrush(Color.FromArgb(214, 69, 69));

            public GridCanvas()
            {
                DoubleBuffered = true;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                if (Rows <= 0 || Cols <= 0 || Width <= 0 || Height <= 0) return;
                var g = e.Graphics;
                float cw = (float)Width / Cols;
                float ch = (float)Height / Rows;
                float fs = Small ? 7.5F : 10F;
                float posFs = Small ? 6.5F : 7F;  // 料位号字号（比结果小，可自行调整：大料盘5.5 / 普通7）
                using (var font = new Font("Microsoft YaHei UI", fs, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    for (int r = 0; r < Rows; r++)
                    {
                        for (int c = 0; c < Cols; c++)
                        {
                            // 蛇形：偶数行从左到右，奇数行从右到左
                            int pos = r * Cols + (r % 2 == 0 ? c : Cols - 1 - c) + 1;
                            var cv = (Snap != null && pos <= Snap.Cells.Count) ? Snap.Cells[pos - 1] : null;
                            Color bg = C_WAIT;
                            Color border = Color.FromArgb(201, 210, 220);
                            Brush tb = BR_TXT;
                            string posTxt = "#" + pos;   // 料位号
                            string resTxt = "";          // 检测结果 OK/NG
                            if (cv != null)
                            {
                                if (cv.Result == true)
                                {
                                    bg = C_OK; border = C_OKBorder; tb = BR_OK;
                                    resTxt = "OK";
                                }
                                else if (cv.Result == false)
                                {
                                    bg = C_NG; border = C_NGBorder; tb = BR_NG;
                                    resTxt = "NG";
                                }
                                else
                                {
                                    bg = C_PICK; border = C_PICKBorder;
                                }
                            }
                            float x = c * cw, y = r * ch;
                            using (var br = new SolidBrush(bg)) g.FillRectangle(br, x + 1f, y + 1f, cw - 2f, ch - 2f);
                            using (var pen = new Pen(border)) g.DrawRectangle(pen, x + 0.5f, y + 0.5f, cw - 1f, ch - 1f);
                            // 统一两行显示：上行料位号（小字号），下行 OK/NG（保持原字号）
                            using (var fPos = new Font("Microsoft YaHei UI", posFs, FontStyle.Bold))
                            {
                                g.DrawString(posTxt, fPos, tb, new RectangleF(x + 2f, y + 1f, cw - 4f, ch * 0.45f), sf);
                                if (resTxt.Length > 0)
                                    g.DrawString(resTxt, font, tb, new RectangleF(x + 2f, y + 1f + ch * 0.42f, cw - 4f, ch * 0.58f), sf);
                            }
                        }
                    }
                }
            }
        }
    }
}

using AF.Common;
using AF.Common.UI;
using AF.Data;
using AF.Data.Language;
using AF.Data.Permission;
using AF.File;
using AF.Station;
using AF.Station.UI;
using Autoframe;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;
using static AF.Station.StationBase;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using AF.Card;

namespace Autoframe.UI
{
    public partial class Form_Auto : AF.Data.Language.BaseForm
    {
        private static List<Form> formCustom = new List<Form>();

        public Form_Auto()
        {
            InitializeComponent();

            //InitCtrlAnchor();
        }

        /// <summary>
        /// 添加自定义TabPage
        /// </summary>
        /// <param name="form"></param>
        public static void AddPage(params Form[] form)
        {
            foreach (var item in form)
                formCustom.Add(item);
        }

        /// <summary>
        /// 添加自定义面板
        /// </summary>
        /// <param name="arrayDI"></param>
        /// <param name="arrayDO"></param>
        public static void SetCustomPanel(object[] arrayDI = null, object[] arrayDO = null)
        {
            ucQuickPanel = new Form_Auto_CustomPanel();
            ((Form_Auto_CustomPanel)ucQuickPanel).Init(arrayDI, arrayDO);
        }

        /// <summary>
        /// 添加自定义面板
        /// </summary>
        /// <param name="uc">自定义</param>
        public static void SetCustomPanel(UserControl uc)
        {
            ucQuickPanel = uc;
        }

        private static UserControl ucQuickPanel = null;

        void LoadCustom()
        {
            if(ucQuickPanel != null)
            {
                ucQuickPanel.Dock = DockStyle.Fill;
                this.tableLayoutPanel_Main.Controls.Add(ucQuickPanel, 0, 2);
            }
            else
            {
                this.tableLayoutPanel_Main.SetRowSpan(this.dataGridViewProductDataShow, 3);
            }

            if (formCustom == null)
                return;
            if (formCustom.Count < 1)
                return;

            foreach (var form in formCustom)
            {
                TabPage tabPage = new TabPage();
                tabPage.AutoScroll = true;
                tabPage.Name = form.Text;
                tabPage.Text = form.Text;
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;
                tabPage.Controls.Add(form);
                this.tabControl_Auto.Controls.Add(tabPage);
                form.Show();
            }
        }
        private void Form_Auto_Load(object sender, EventArgs e)
        {

            //textBox1.Text = RegInt.TestCount.Get().ToString();
            BackupPointXmlAndCleanOld();
            LoadCustom();
            StationMgr.Stop();
            this.logGroup1.Init();

            ProgramTool.StationStateChangedEvent += OnStationStateChanged;
            OnStationStateChanged(StationState.Manual, StationMgr.State);

            ProductConfig.StoreDataChangedEvent += InitProductDataGridViewHead;            
            InitProductDataGridViewHead();
            ProductConfig.ShowProductDataEvent += OnShowProductData;
            UpdateProductCounter();
            ProductConfig.UpdateCarrierCounter += OnUpdateCarrierCounter;
            OnUpdateCarrierCounter();

            Parameter.UpdateParameter += OnUpdateParameter;
            OnUpdateParameter();
            

            panel_IoSummary.ContextMenu = new ContextMenu();
            MenuItem clearItem = new MenuItem("数据清空");
            clearItem.Click += button_clear_Click;
            panel_IoSummary.ContextMenu.MenuItems.Add(clearItem);

            this.label_MachineName.Text = Ini.GetStringValue(Program.g_IniFile, "Info", "Machine", "");
            this.lb_SiteValue.Text = Ini.GetStringValue(Program.g_IniFile, "Info", "Site", "");
            this.lb_SWVersionValue.Text = Form_Main.SoftwareVersion;
            this.lb_MSPathValue.Text = Assembly.GetExecutingAssembly().Location;
            string hashCode = ProgramTool.GetFileHash(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(hashCode) && hashCode.Length > 6)
                this.lb_MSHashValue.Text = hashCode/*.Substring(0, 6)*/;

            LanguageMgr.Instance.eventChangeLanguage += OnChangeLanguage;
        }

        private void OnUpdateCarrierCounter()
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                this.label_CarrierCount.Text = SystemIO.CarrierCount.ToString();
            });
        }

        private void OnUpdateParameter()
        {
            this.dataGridView_CriticalParam.Rows.Clear();
            foreach (var item in Parameter.GetKeyParameters())
            {
                this.dataGridView_CriticalParam.Rows.Add(item.m_strDescribe.txtTrans(TransType.Trans), item.m_strValue, item.m_strMin, item.m_strMax);
            }
        }

        private void OnChangeLanguage()
        {
            OnUpdateParameter();
            this.logGroup1.OnChangeLanguage();
            foreach (KeyValuePair<string, ProductConfig.SaveOption> keyValuePair in ProductConfig.Instance.Option)
            {
                if (keyValuePair.Value.bShow)
                    dataGridViewProductDataShow.Columns[keyValuePair.Key].HeaderText = keyValuePair.Key.txtTrans(TransType.Trans);
            }
        }


        private void OnStationStateChanged(StationState OldState, StationState NewState)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                if (OldState == StationState.Manual && NewState == StationState.Init)
                {
                    this.logGroup1.Clear();
                }
            });
        }

        /// <summary>
        /// 初始化界面生产数据表
        /// </summary>
        private void InitProductDataGridViewHead()
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                dataGridViewProductDataShow.Rows.Clear();
                dataGridViewProductDataShow.Columns.Clear();
                dataGridViewProductDataShow.RowsDefaultCellStyle.BackColor = Color.Ivory;
                dataGridViewProductDataShow.AlternatingRowsDefaultCellStyle.BackColor = Color.PaleTurquoise;

                if (ProductConfig.Instance.Option != null && ProductConfig.Instance.Option.Count > 0)
                {
                    foreach (KeyValuePair<string, ProductConfig.SaveOption> keyValuePair in ProductConfig.Instance.Option)
                    {
                        if (keyValuePair.Value.bShow)
                        {
                            dataGridViewProductDataShow.Columns.Add(keyValuePair.Key, keyValuePair.Key.txtTrans(TransType.Trans));
                        }
                    }
                    foreach (DataGridViewColumn column in dataGridViewProductDataShow.Columns)
                    {
                        column.SortMode = DataGridViewColumnSortMode.NotSortable;
                        column.Width = 100;
                    }
                }
            });
        }

        /// <summary>
        /// 界面最大显示100行物料数据
        /// </summary>
        public static int ShowCount = 100;

        /// <summary>
        /// 生产数据展示
        /// </summary>
        /// <param name="dicProduct"></param>
        private void OnShowProductData(Dictionary<string, string> dicProduct)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            this.BeginInvoke((Action)delegate
            {
                // 更新计数器
                UpdateProductCounter();
                if (dicProduct.Count < 1)
                    return;

                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridViewProductDataShow, dicProduct.Values.ToArray());
                if (dicProduct.ContainsKey("结果"))
                {
                    bool result;
                    string resultValue = dicProduct["结果"];
                    if (bool.TryParse(resultValue, out result) && !result)
                        row.DefaultCellStyle.ForeColor = Color.Red;
                }
                dataGridViewProductDataShow.Rows.Insert(0, row);
                if (dataGridViewProductDataShow.Rows.Count > ShowCount)
                    dataGridViewProductDataShow.Rows.RemoveAt(dataGridViewProductDataShow.Rows.Count - 1);
                dataGridViewProductDataShow.Refresh();
            });
        }



        /// <summary>
        /// 更新计数器显示
        /// </summary>
        private void UpdateProductCounter()
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            this.BeginInvoke((Action)delegate
            {
                lb_TotalValue.Text = ProductConfig.TotalCount.ToString();
                lb_PassValue.Text = ProductConfig.PassCount.ToString();
                lb_FailValue.Text = ProductConfig.FailCount.ToString();

                if (ProductConfig.TotalCount > 0)
                {
                    double yield = (double)ProductConfig.PassCount / ProductConfig.TotalCount * 100;
                    lb_YieldValue.Text = yield.ToString("F2") + "%";
                }
                else
                {
                    lb_YieldValue.Text = "0.00%";
                }
            });            
        }




        private void button_clear_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否清空统计数据？".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ProductConfig.ClearProductCounter();
                UpdateProductCounter();
            }
        }

        /// <summary>
        /// 程序启动自动备份Point.xml，自动清理7天以上备份文件
        /// </summary>
        private void BackupPointXmlAndCleanOld()
        {
            // =====================路径配置=====================
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // 原始点位文件
            string originalPointFile = Path.Combine(baseDir, "Config", "Point.xml");
            // 固定备份目录 D:\exe\backUpPoints
            string backupDir = @"D:\exe\backUpPoints";
            int keepDays = 7;
            // ==================================================

            try
            {
                // 判断源文件是否存在
                if (!File.Exists(originalPointFile))
                {
                    Logger.Manual($"点位文件不存在，无需备份：{originalPointFile}");
                    return;
                }

                // 创建备份文件夹（不存在自动新建）
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                    Logger.Manual($"备份目录不存在，自动创建：{backupDir}");
                }

                // 备份文件名带时间戳 Point_20260725_152030.xml
                string timeTag = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"Point_{timeTag}.xml";
                string fullBackupPath = Path.Combine(backupDir, backupFileName);

                // 执行备份复制
                File.Copy(originalPointFile, fullBackupPath, true);
                Logger.Process(Name, $"点位文件备份成功 → {fullBackupPath}");

                #region 清理超过7天的旧备份
                DirectoryInfo dirInfo = new DirectoryInfo(backupDir);
                // 筛选所有Point_开头xml备份
                var expiredFiles = dirInfo.GetFiles("Point_*.xml")
                           .Where(f => f.CreationTime < DateTime.Now.AddDays(-keepDays))
                           .ToList();

                foreach (var file in expiredFiles)
                {
                    try
                    {
                        file.Delete();
                        Logger.Manual($"过期点位备份已删除：{file.Name}");
                    }
                    catch (Exception exDel)
                    {
                        Logger.Manual($"删除过期文件[{file.Name}]失败：{exDel.Message}");
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.Manual($"Point.xml自动备份异常：{ex.Message}");
                // 异常只记录日志，不阻塞程序启动
            }
        }
        private void lb_MSPathValue_Click(object sender, EventArgs e)
        {
            //Process.Start(lb_MSPathValue.Text);
        }


    }
}

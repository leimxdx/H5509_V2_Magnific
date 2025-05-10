using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using H5509_V2.界面;
using Sunny.UI;
using Sunny.UI.Win32;

namespace H5509_V2
{
    public partial class _log : UIUserControl
    {
        public delegate void AddLogDelegate(string level, string logType, string info);
        // 声明一个静态的委托实例
        public static AddLogDelegate GlobalAddLog;
        private static string logsDirectory = Path.Combine(Application.StartupPath, "Logs");
        private int currentSearchIndex = -1;  // 当前查找到的行索引
        private string currentSearchKeyword = "";  // 当前搜索关键词
        private 界面.UIProgressIndicatorEx progressIndicator; // 进度条实例
        private bool isProcessing = false;
        public _log()
        {
            InitializeComponent();
            this.dgv_data.AutoGenerateColumns = false;
            GlobalAddLog = AddLog;
            //CleanOldLogs();
            this.startTime.Value = DateTime.Now;
            this.endTime.Value = DateTime.Now;
            this.Load += _log_Load;
            // 初始化进度条相关设置
            // 初始化进度指示器
            progressIndicator = new 界面.UIProgressIndicatorEx
            {
                Size = new Size(220, 153),
                BackColor = System.Drawing.Color.Transparent
                //ForeColor = UIColor.Blue
            };
            // 确保日志目录存在
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }
          
        }

        private void _log_Load(object sender, EventArgs e)
        {
            if (this.cbbLogLevel.Items.Count > 0)
            {
                this.cbbLogLevel.SelectedIndex = 0;
            }
            if (this.cbbLogType.Items.Count > 0)
            {
                this.cbbLogType.SelectedIndex = 0;
            }
            string Excelpath = Path.Combine(logsDirectory, $"Log_{DateTime.Now:yyyyMMdd-HH}.xlsx");
            logsList = ReadExcel(Excelpath, this.startTime.Value, this.endTime.Value);
        }

        public TimeSpan Timestamp { get; set; }
        private List<log> logsList = new List<log>();
      
        DateTime logTimes = DateTime.Now;
       
        // 自动清理30天前的日志
        private void CleanOldLogs()
        {
          
            foreach (var dir in Directory.GetDirectories(logsDirectory))
            {
                // 从文件夹名解析日期（格式：yyyyMMdd）
                string dirName = Path.GetFileName(dir);
                if (DateTime.TryParseExact(dirName, "yyyyMM", null, DateTimeStyles.None, out DateTime dirDate))
                {
                    if (dirDate < DateTime.Now.AddDays(-3))
                    {
                        Directory.Delete(dir, true); // 删除整个日期文件夹
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level">全部输出-all 正常-normal 调试-debug 错误-error 警告-warning</param>
        /// <param name="logType"></param>
        /// <param name="info"></param>
        public void AddLog(string level, string logType, string info)
        {
           string monthDir = DateTime.Now.ToString("yyyyMM");
            string monthlyLogDir = Path.Combine(logsDirectory, monthDir);
            // 确保日期目录存在
            if (!Directory.Exists(monthlyLogDir))
            {
                Directory.CreateDirectory(monthlyLogDir);
            }
            string Excelpath = Path.Combine(monthlyLogDir, $"Log_{DateTime.Now:yyyyMMdd-HH}.xlsx");
            log logs = new log();
            logs.Time = DateTime.Now;
            logs.LogLevel = level;
            logs.LogType = logType;
            logs.LogContent = info;
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                {
                    logsList.Add(logs);
                    this.dgv_data.DataSource = null;
                    this.dgv_data.DataSource = logsList;
                }));
            }
            else 
            {
                logsList.Add(logs);
                this.dgv_data.DataSource = null;
                this.dgv_data.DataSource = logsList;
            }
                XLWorkbook workbook;
            IXLWorksheet worksheet;
            log newLog = new log
            {
                Time = DateTime.Now,
                LogLevel = level,
                LogType = logType,
                LogContent = info
            };
            try
            {
                if (!File.Exists(Excelpath))
                {
                    workbook = new XLWorkbook();
                    worksheet = workbook.AddWorksheet("sheet1");    // 创建名称为sheet1的工作表
                    worksheet.Cell(1, 1).Value = "日志时间";
                    worksheet.Cell(1, 2).Value = "日志等级";
                    worksheet.Cell(1, 3).Value = "日志类型";
                    worksheet.Cell(1, 4).Value = "日志内容";
                    // 确定起始行（现有数据最后一行+1）
                    int startRow = worksheet.LastRowUsed()?.RowNumber()?? 2;

                    // 写入数据
               
                        int currentRow = startRow + 1;

                        worksheet.Cell(currentRow, 1).Value = newLog.Time;
                        worksheet.Cell(currentRow, 2).Value = newLog.LogLevel;
                        worksheet.Cell(currentRow, 3).Value = newLog.LogType;
                        worksheet.Cell(currentRow, 4).Value = newLog.LogContent;

                    //}
                    workbook.SaveAs(Excelpath);
                }
                else
                {
                    workbook = new XLWorkbook(Excelpath);
                    worksheet = workbook.Worksheet("sheet1");
                    if (worksheet == null)
                    {
                        // 如果工作表不存在，创建新的工作表并设置表头
                        worksheet = workbook.AddWorksheet("sheet1");
                        worksheet.Cell(1, 1).Value = "日志时间";
                        worksheet.Cell(1, 2).Value = "日志等级";
                        worksheet.Cell(1, 3).Value = "日志类型";
                        worksheet.Cell(1, 4).Value = "日志内容";
                    }
                    // 确定起始行（现有数据最后一行+1）
                    int startRow = worksheet.LastRowUsed()?.RowNumber()?? 2;

                    // 写入数据
                                     
                        int currentRow = startRow + 1;

                        worksheet.Cell(currentRow, 1).Value = newLog.Time;
                        worksheet.Cell(currentRow, 2).Value = newLog.LogLevel;
                        worksheet.Cell(currentRow, 3).Value = newLog.LogType;
                        worksheet.Cell(currentRow, 4).Value = newLog.LogContent;

                    workbook.SaveAs(Excelpath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"日志写入失败：{ex.Message}");
            }

        }

        private void cbbLogLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 获取当前选中的日志等级
            currentLevelFilter = cbbLogLevel.SelectedItem.ToString();
            // 筛选日志列表
            List<log> filteredLogs;
            currentLevelFilter = currentLevelFilter.Split('-')[1].ToLower();
            //RefreshDataGrid();

        }

        private void cbbLogType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 获取当前选中的日志类型
            currentTypeFilter = cbbLogType.SelectedItem.ToString();
            // 筛选日志列表
            List<log> filteredLogs;
            currentTypeFilter = currentTypeFilter.Split('-')[1].ToLower();
            //RefreshDataGrid();

        }

        private void btnTimeSelect_Click(object sender, EventArgs e)
        {btnTimeSelect.Enabled = false;
            RefreshDataGrid();
            btnTimeSelect.Enabled = true;
        }
        
        private List<log> ReadExcel(string path, DateTime? startTime = null, DateTime? endTime = null)
        {
            var logs = new List<log>();
            if (!File.Exists(path)) return logs;

            try
            {
                using (var workbook = new XLWorkbook(path))
                {
                    var worksheet = workbook.Worksheet(1) ?? workbook.Worksheets.First();
                    foreach (var row in worksheet.RowsUsed().Skip(1))
                    {
                        DateTime logTime = row.Cell(1).GetDateTime();

                        // 检查时间范围
                        if (startTime.HasValue && logTime < startTime.Value) continue;
                        if (endTime.HasValue && logTime > endTime.Value) continue;

                        logs.Add(new log
                        {
                            Time = logTime,
                            LogLevel = row.Cell(2).GetString(),
                            LogType = row.Cell(3).GetString(),
                            LogContent = row.Cell(4).GetString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"读取{path}失败: {ex.Message}");
            }

            return logs;
        }
        private void btnWriteLog_Click(object sender, EventArgs e)
        {
            
            AddLog("debug", "operate", "这是一条debug调试日志");
            AddLog("all", "run", "这是一条调试日志");
            AddLog("error", "spc", "这是一条error调试日志");
            AddLog("warning", "spc", "这是一条warning调试日志");
            AddLog("warning", "spc", "这是一条warning调试日志");
            AddLog("normal", "run", "这是一条调试日志");
            AddLog("debug", "operate", "这是一条debug调试日志");
            AddLog("all", "run", "这是一条调试日志");
            AddLog("error", "spc", "这是一条error调试日志");
            AddLog("warning", "all", "这是一条warning调试日志");
            AddLog("warning", "spc", "这是一条warning调试日志");
            AddLog("all", "run", "这是一条调试日志");

        }
        // 当前筛选条件
        private string currentLevelFilter = "";
        private string currentTypeFilter = "";
        
        // 获取筛选后的日志
        private List<log> GetFilteredLogs()
        {
           
            DateTime start = startTime.Value;
            DateTime end = endTime.Value;
            string keyword=this.txtLog.Text?.Trim();

            // 获取日期范围内的所有日志文件
            var logFiles = GetLogFilesInDateRange(start, end);
            var allLogs = new List<log>();
            foreach (var file in logFiles) // 改为同步读取
            {
                var fileLogs = ReadExcel(file, start, end);
                Debug.WriteLine($"从 {file} 加载了 {fileLogs.Count} 条数据"); // 验证每个文件的数据量
                allLogs.AddRange(fileLogs);
            }
            //// 并行读取文件提升性能
            //Parallel.ForEach(logFiles, file =>
            //{
            //    var fileLogs = ReadExcel(file, start, end);
            //    lock (allLogs)
            //    {
            //        allLogs.AddRange(fileLogs);
            //    }
            //});

            // 应用其他筛选条件
            var query = allLogs.AsQueryable();

            if (!string.IsNullOrEmpty(currentLevelFilter)&&!currentLevelFilter.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(log =>
                    log.LogLevel.Equals(currentLevelFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(currentTypeFilter)&& !currentTypeFilter.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(log =>
                    log.LogType.Equals(currentTypeFilter, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                query=query.Where(log =>log.LogContent.IndexOf(keyword, StringComparison.OrdinalIgnoreCase)>=0);
            }
            return query.OrderBy(log => log.Time).ToList();
           
        }
        // 统一刷新DataGridView
        private void RefreshDataGrid()
        {
            if (isProcessing) return;
            isProcessing = true;
       
            try
            {// 创建并显示进度条
             // 创建并显示进度指示器
                this.Invoke((Action)(() =>
                {
                    progressIndicator = new UIProgressIndicatorEx
                    {
                        //Size = new Size(220, 153),
                       
                        BackColor = System.Drawing.Color.Transparent
                    };              
                    this.Controls.Add(progressIndicator);
                    progressIndicator.BringToFront();
                    // 调用居中方法
                    progressIndicator.CenterToParent();
                    progressIndicator.Start();
                }));

                Application.DoEvents();        
                try
                { 
                    var filteredLogs = GetFilteredLogs();

                    this.Invoke((Action)(() =>
                    {
                        dgv_data.DataSource = null;
                        dgv_data.DataSource = filteredLogs;
                        lblTotalNum.Text = $"总记录数: {dgv_data.RowCount}";
                      
                    }));

                    currentSearchIndex = -1;
                    currentSearchKeyword = "";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"查询失败: {ex.Message}");
                    UIMessageTip.ShowError(this, $"查询失败: {ex.Message}", 2000);
                }
                finally
                {               
                    // 延迟关闭进度指示器
                    Task.Delay(300).ContinueWith(_ =>
                    {
                        this.Invoke((Action)(() =>
                        {
                            progressIndicator.Stop();
                            this.Controls.Remove(progressIndicator);
                           
                        }));
                    });
                }
            }
            finally
            {
                isProcessing = false;
            }
        }
            
        private void endTime_ValueChanged(object sender, DateTime value)
        {
            if (startTime.Value > endTime.Value)
            {
                UIMessageBox.Show("开始时间不能大于结束时间");
                return;
            }
            //RefreshDataGrid();
        }

        private void startTime_ValueChanged(object sender, DateTime value)
        {
           //RefreshDataGrid();
        }

        private void dgv_data_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = (DataGridView)sender;
            using (SolidBrush brush = new SolidBrush(grid.RowHeadersDefaultCellStyle.ForeColor))
            {
                // 在行头绘制序号
                string index = (e.RowIndex + 1).ToString();
                e.Graphics.DrawString(
                    index,
                    grid.RowHeadersDefaultCellStyle.Font,
                    brush,
                    e.RowBounds.Location.X + 15,
                    e.RowBounds.Location.Y + 4
                );
              
            }
        }
        //获取日期范围内的日志文件
        private List<string> GetLogFilesInDateRange(DateTime startDate, DateTime endDate)
        {         
            var logFiles = new List<string>();

            //try
            //{
            //    // 遍历所有日期子目录（格式：yyyyMMdd）
            //    foreach (var monthDir in Directory.GetDirectories(logsDirectory))
            //    {
            //        string dirName = Path.GetFileName(monthDir);

            //        // 解析目录名中的日期（如 "202505" → DateTime）
            //        if (DateTime.TryParseExact(dirName, "yyyyMM", null, DateTimeStyles.None, out DateTime dirDate))
            //        {// 检查月份是否在查询范围内
            //            DateTime firstDayOfMonth = new DateTime(dirDate.Year, dirDate.Month, 1);
            //            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            //            // 如果目录日期不在查询范围内，跳过
            //            if (dirDate.Date < startDate.Date || dirDate.Date > endDate.Date)
            //                continue;

            //            // 遍历该日期目录下的所有日志文件
            //            foreach (var file in Directory.GetFiles(monthDir, "Log_*.xlsx"))
            //            {
            //                string fileName = Path.GetFileNameWithoutExtension(file);

            //                // 从文件名解析小时（格式：Log_yyyyMMdd-HH.xlsx）
            //                if (fileName.Length >= 13) // 确保文件名足够长
            //                {
            //                    string dateHourStr = fileName.Substring(4, 11); // 提取 "yyyyMMdd-HH"

            //                    if (DateTime.TryParseExact(dateHourStr, "yyyyMMdd-HH", null, DateTimeStyles.None, out DateTime fileTime))
            //                    {
            //                        // 检查文件时间是否在查询范围内
            //                        if (fileTime >= startDate && fileTime <= endDate)
            //                        {
            //                            logFiles.Add(file);
            //                        }
            //                    }
            //                }
            //            }
            //        }

            try
            {
                // 生成需要查询的所有月份范围
                for (DateTime month = new DateTime(startDate.Year, startDate.Month, 1);
                     month <= endDate;
                     month = month.AddMonths(1))
                {
                    string monthDir = Path.Combine(logsDirectory, month.ToString("yyyyMM"));

                    if (!Directory.Exists(monthDir))
                        continue;

                    // 获取该月文件夹下所有日志文件
                    foreach (var file in Directory.GetFiles(monthDir, "Log_*.xlsx"))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);

                        // 从文件名解析日期和时间（格式：Log_yyyyMMdd-HH.xlsx）
                        if (fileName.Length >= 13) // 确保文件名足够长
                        {
                            string datePart = fileName.Substring(4, 8);  // yyyyMMdd
                            string hourPart = fileName.Substring(13, 2); // HH

                            if (DateTime.TryParseExact($"{datePart}{hourPart}", "yyyyMMddHH", null, DateTimeStyles.None, out DateTime fileTime))
                            {
                                // 检查文件时间是否在查询范围内
                                if (fileTime >= startDate && fileTime <= endDate)
                                {
                                    logFiles.Add(file);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取日志文件失败: {ex.Message}");
            }

            return logFiles.OrderBy(f => f).ToList();
        }

        private void LogTimer_Tick(object sender, EventArgs e)
        {

        }

        private void startTime_Leave(object sender, EventArgs e)
        {
           
        }

        private void btnSelectKey_Click(object sender, EventArgs e)
        {
            btnTimeSelect.Enabled =false;
            string keyword = txtkeyWords.Text?.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                UIMessageTip.Show(this, "请输入搜索关键词", null, 1000, true, true);
                return;
            }

            // 如果关键词改变或首次搜索，重置搜索位置
            if (!keyword.Equals(currentSearchKeyword, StringComparison.OrdinalIgnoreCase))
            {
                currentSearchKeyword = keyword;
                currentSearchIndex = -1;
            }

            // 获取当前数据源
            var dataSource = dgv_data.DataSource as List<log>;
            if (dataSource == null || dataSource.Count == 0)
            {
                UIMessageTip.Show(this, "没有可搜索的数据", null, 1000, true, true);
                return;
            }

            // 从下一个位置开始搜索
            int startIndex = currentSearchIndex + 1;
            bool found = false;

            for (int i = startIndex; i < dataSource.Count; i++)
            {
                if (dataSource[i].LogContent.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    currentSearchIndex = i;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                // 选中并滚动到该行
                dgv_data.ClearSelection();
                dgv_data.Rows[currentSearchIndex].Selected = true;
                dgv_data.FirstDisplayedScrollingRowIndex = currentSearchIndex;
            }
            else
            {
                // 循环搜索或提示未找到
                if (currentSearchIndex != -1)
                {
                    currentSearchIndex = -1;
                    btnSelectKey_Click(sender, e); // 递归调用从头开始搜索
                }
                else
                {
                    UIMessageTip.Show(this, "未找到匹配项", null, 1000, true, true);
                }
            }
            btnTimeSelect.Enabled = true;
        }
    }
}

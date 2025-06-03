using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using H5509_V2.界面;
using Sunny.UI;
using Sunny.UI.Win32;
using static H5509_V2.APPmaster;

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
        private static Dictionary<string, FileCounter> _fileCounters = new Dictionary<string, FileCounter>();

        class FileCounter
        {
            public const int MaxRows = 60000; // 最大行数常量
            public string FilePath { get; set; }
            public int RowCount { get; set; }  // 仅数据行数（不包含表头）
            public DateTime FirstLogTime { get; set; }
        }
        public _log()
        {
            InitializeComponent();
            this.dgv_data.AutoGenerateColumns = false;
            GlobalAddLog = AddLog;
            //CleanOldLogs();
            this.startTime.Value = DateTime.Now;
            this.endTime.Value = DateTime.Now;
            this.Load += _log_Load;
            lblTotalNum.Text = "总记录数:0";
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
            startTime.Value = DateTime.Today;//0时
            endTime.Value = DateTime.Today.AddDays(1);
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

        public void RefreshLog(string level, string logType, string info)
        {
            try
            {
                dgv_data.Invoke((Action)(() =>
                {
                    logsList.Add(new log
                    {
                        Time = DateTime.Now,
                        LogLevel = level,
                        LogType = logType,
                        LogContent = info
                    });
                    dgv_data.DataSource = null;
                    dgv_data.DataSource = logsList;
                }));
            }
            catch (Exception ex)
            {
                writeDataHelper.RunLog(ex.Message);
            }
        }

        /// <summary>
        /// 输出log(level:all-normal-debug-error-warning)(logType:all-operate-run-spc)
        /// </summary>
        /// <param name="level"> </param>
        /// <param name="logType"></param>
        /// <param name="info"></param>
        public void AddLog(string level, string logType, string info)
        {
            string monthlyDir = Path.Combine(logsDirectory, DateTime.Now.ToString("yyyyMM"));
            var currentFile = GetCurrentLogFile(monthlyDir);

            var logs = new List<(string, string, string)>
            {
                (level, logType, info)
            };

            WriteLogToExcel(currentFile, logs); // 单条也作为列表传入

            if (dgv_data.InvokeRequired) RefreshLog(level, logType, info);
            else RefreshLog(level, logType, info);

        }


        private void WriteLogToExcel(FileCounter fileCounter, List<(string level, string logType, string info)> logs)
        {
            try
            {
                int remainingSpace = FileCounter.MaxRows - fileCounter.RowCount;
                int logsToWrite = Math.Min(logs.Count, remainingSpace);

                if (logsToWrite > 0)
                {
                    // 写入当前文件能容纳的部分
                    using (var workbook = new XLWorkbook(fileCounter.FilePath))
                    {
                        var worksheet = workbook.Worksheet(1);
                        int startRow = worksheet.LastRowUsed().RowNumber() + 1;

                        for (int i = 0; i < logsToWrite; i++)
                        {
                            var (level, logType, info) = logs[i];
                            worksheet.Cell(startRow + i, 1).Value = DateTime.Now;
                            worksheet.Cell(startRow + i, 2).Value = level;
                            worksheet.Cell(startRow + i, 3).Value = logType;
                            worksheet.Cell(startRow + i, 4).Value = info;
                        }

                        workbook.Save();
                        fileCounter.RowCount += logsToWrite;
                        //writeDataHelper.RunLog($"写入文件: {Path.GetFileName(fileCounter.FilePath)}，新增行数: {logsToWrite}，总行数: {fileCounter.RowCount}");
                    }

                    // 处理剩余日志
                    if (logs.Count > logsToWrite)
                    {
                        var remainingLogs = logs.Skip(logsToWrite).ToList();
                        var newFile = GetCurrentLogFile(Path.GetDirectoryName(fileCounter.FilePath));
                        WriteLogToExcel(newFile, remainingLogs);
                    }
                }
                else
                {
                    // 当前文件已满，创建新文件
                    var newFile = GetCurrentLogFile(Path.GetDirectoryName(fileCounter.FilePath));
                    WriteLogToExcel(newFile, logs);
                }
            }
            catch (Exception ex)
            {
                writeDataHelper.RunLog($"写入失败: {fileCounter.FilePath}\n{ex.Message}");
                _fileCounters.Remove(fileCounter.FilePath);
                var newFile = GetCurrentLogFile(Path.GetDirectoryName(fileCounter.FilePath));
                WriteLogToExcel(newFile, logs);
            }
        }

        private void InternalWrite(FileCounter fileCounter, List<(string level, string logType, string info)> logs)
        {
            using (var workbook = new XLWorkbook(fileCounter.FilePath))
            {
                var worksheet = workbook.Worksheet(1);
                int startRow = worksheet.LastRowUsed()?.RowNumber() + 1 ?? 2;

                for (int i = 0; i < logs.Count; i++)
                {
                    var (level, logType, info) = logs[i];
                    worksheet.Cell(startRow + i, 1).Value = DateTime.Now;
                    worksheet.Cell(startRow + i, 2).Value = level;
                    worksheet.Cell(startRow + i, 3).Value = logType;
                    worksheet.Cell(startRow + i, 4).Value = info;
                }

                workbook.Save();
            }

            fileCounter.RowCount += logs.Count;
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
        {
            //btnTimeSelect.Enabled = false;
            RefreshDataGrid();
            //btnTimeSelect.Enabled = true;
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
                writeDataHelper.RunLog($"读取{path}失败: {ex.Message}");
            }

            return logs;
        }

        // 当前筛选条件
        private string currentLevelFilter = "";
        private string currentTypeFilter = "";

        // 获取筛选后的日志
        private List<log> GetFilteredLogs()
        {

            DateTime start = startTime.Value;
            DateTime end = endTime.Value;
            string keyword = this.txtLog.Text?.Trim();

            // 获取日期范围内的所有日志文件
            var logFiles = GetLogFilesInDateRange(start, end);
            var allLogs = new List<log>();
            foreach (var file in logFiles) // 改为同步读取
            {
                var fileLogs = ReadExcel(file, start, end);
                //writeDataHelper.RunLog($"从 {file} 加载了 {fileLogs.Count} 条数据"); // 验证每个文件的数据量
                allLogs.AddRange(fileLogs);
            }


            // 应用其他筛选条件
            var query = allLogs.AsQueryable();

            if (!string.IsNullOrEmpty(currentLevelFilter) && !currentLevelFilter.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(log =>
                    log.LogLevel.Equals(currentLevelFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(currentTypeFilter) && !currentTypeFilter.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(log =>
                    log.LogType.Equals(currentTypeFilter, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(log => log.LogContent.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            return query.OrderBy(log => log.Time).ToList();

        }
        // 统一刷新DataGridView
        private async void RefreshDataGrid()
        {
            if (isProcessing) return;
            isProcessing = true;

            // 创建并显示进度指示器
            this.Invoke((Action)(() =>
            {
                progressIndicator = new UIProgressIndicatorEx
                {
                    BackColor = System.Drawing.Color.Transparent
                };
                this.Controls.Add(progressIndicator);
                progressIndicator.BringToFront();
                progressIndicator.CenterToParent();
                progressIndicator.Start();
            }));

            try
            {
                // 使用Task.Run将数据加载放到后台线程
                var filteredLogs = await Task.Run(() => GetFilteredLogs());

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
                writeDataHelper.RunLog($"查询失败: {ex.Message}");
                UIMessageTip.ShowError(this, $"查询失败: {ex.Message}", 2000);
            }
            finally
            {
                // 确保进度指示器在数据完全加载后才关闭
                this.Invoke((Action)(() =>
                {
                    progressIndicator.Stop();
                    this.Controls.Remove(progressIndicator);
                    progressIndicator.Dispose();
                }));
            }

            isProcessing = false;
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

            // 遍历所有月份目录
            foreach (var monthDir in Directory.GetDirectories(logsDirectory))
            {
                // 从目录名获取月份（格式：yyyyMM）
                string dirName = Path.GetFileName(monthDir);
                if (DateTime.TryParseExact(dirName, "yyyyMM", null, DateTimeStyles.None, out DateTime dirDate))
                {
                    // 检查月份是否在查询时间范围内（优化：先粗略按月份筛选）
                    if (dirDate.Year < startDate.Year || dirDate.Year > endDate.Year)
                        continue;
                    if (dirDate.Year == startDate.Year && dirDate.Month < startDate.Month)
                        continue;
                    if (dirDate.Year == endDate.Year && dirDate.Month > endDate.Month)
                        continue;

                    // 遍历该月所有日志文件
                    foreach (var file in Directory.GetFiles(monthDir, "Log_*.xlsx"))
                    {
                        // 从文件名解析时间（格式：Log_yyyyMMdd-HHmmss.xlsx）
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        string timePart = fileName.Substring(4); // 去掉"Log_"

                        if (DateTime.TryParseExact(timePart, "yyyyMMdd-HHmmss", null, DateTimeStyles.None, out DateTime fileTime))
                        {
                            // 优化：如果文件创建时间完全不在查询范围内，则跳过（减少文件打开次数）
                            if (fileTime.Date > endDate.Date || fileTime.Date < startDate.Date)
                                continue;
                        }

                        // 如果文件名时间在范围内，或无法解析时间，则加入待检查列表
                        logFiles.Add(file);
                    }
                }
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
            //btnTimeSelect.Enabled = false;
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
            //btnTimeSelect.Enabled = true;
        }
        private FileCounter GetCurrentLogFile(string monthlyDir)
        {
            // 1. 尝试查找已有文件
            var existingFiles = Directory.GetFiles(monthlyDir, "Log_*.xlsx")
                .OrderByDescending(f => f)
                .ToList();

            foreach (var file in existingFiles)
            {
                // 如果已有计数器记录，直接返回
                if (_fileCounters.TryGetValue(file, out var counter))
                {
                    if (counter.RowCount < FileCounter.MaxRows)
                        return counter;
                    continue;
                }

                // 新发现的文件，初始化计数器
                try
                {
                    using (var workbook = new XLWorkbook(file))
                    {
                        var worksheet = workbook.Worksheet(1);
                        int rowCount = worksheet.LastRowUsed().RowNumber() - 1; // 减去表头行

                        var newCounters = new FileCounter
                        {
                            FilePath = file,
                            RowCount = rowCount,
                            FirstLogTime = GetFileTimeFromName(file)
                        };

                        _fileCounters[file] = newCounters;
                        //writeDataHelper.RunLog($"发现已有文件: {file}，行数: {rowCount}");

                        if (rowCount < FileCounter.MaxRows)
                            return newCounters;
                    }
                }
                catch (Exception ex)
                {
                    writeDataHelper.RunLog($"加载已有文件失败: {file}\n{ex.Message}");
                }
            }

            // 2. 没有可用文件，创建新文件
            string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string newFilePath = Path.Combine(monthlyDir, $"Log_{timestamp}.xlsx");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.AddWorksheet("Logs");
                worksheet.Cell(1, 1).Value = "日志时间";
                worksheet.Cell(1, 2).Value = "日志等级";
                worksheet.Cell(1, 3).Value = "日志类型";
                worksheet.Cell(1, 4).Value = "日志内容";
                workbook.SaveAs(newFilePath);
            }

            var newCounter = new FileCounter
            {
                FilePath = newFilePath,
                RowCount = 0,
                FirstLogTime = DateTime.Now
            };

            _fileCounters.Add(newFilePath, newCounter);
            writeDataHelper.RunLog($"创建新文件: {newFilePath}");

            return newCounter;
        }

        private DateTime GetFileTimeFromName(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string timePart = fileName.Substring(4); // 去掉"Log_"

            if (DateTime.TryParseExact(timePart, "yyyyMMdd-HHmmss", null, DateTimeStyles.None, out DateTime result))
                return result;

            return File.GetCreationTime(filePath); // 回退使用文件创建时间
        }
    }
}

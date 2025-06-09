using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
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

        List<log> logNeed2Add = new List<log>(); // 用于存储需要添加的日志


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
            //GlobalAddLog = AddLog;
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

            uiComboBox11.DataSource = SerialPort.GetPortNames();
            uiComboBox12.DataSource = new string[] { "9600", "19200", "38400", "57600", "115200" };
            uiComboBox14.DataSource = new string[] { "None", "Odd", "Even" };
            uiComboBox13.DataSource = new string[] { "7", "8" };
            uiComboBox15.DataSource = new string[] { "1", "2" };

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

        public void 刷新日志界面()
        {
            uiComboBox11.Text = 设置信息.测试机信息.串口号;
            uiComboBox12.Text = 设置信息.测试机信息.波特率.ToString();
            uiComboBox14.Text = 设置信息.测试机信息.校验位.ToString();
            uiComboBox13.Text = 设置信息.测试机信息.数据位.ToString();
            uiComboBox15.Text = 设置信息.测试机信息.停止位.ToString();
        }

        public TimeSpan Timestamp { get; set; }
        private List<log> logsList = new List<log>();


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
                RunLog(ex.Message);
            }
        }

        /// <summary>
        /// 输出log(level:all-normal-debug-error-warning)(logType:all-operate-run-spc)
        /// </summary>
        /// <param name="level"> </param>
        /// <param name="logType"></param>
        /// <param name="info"></param>
        public void AddLog(string level, string logType, string info, DateTime dateTime)
        {

            logNeed2Add.Add(new log
            {
                Time = dateTime,
                LogLevel = level,
                LogType = logType,
                LogContent = info
            });

            //if (logNeed2Add.Count >= 1000) // 超过1000条立即写入
            //{
            //    LogTimer_Tick(null, null); // 直接调用定时器事件处理方法
            //}

        }

        private void LogTimer_Tick(object sender, EventArgs e)
        {
            if (logNeed2Add.Count == 0) return; // 没有日志需要写入

            string monthlyDir = Path.Combine(logsDirectory, logNeed2Add[0].Time.ToString("yyyyMM"));
            var currentFile = GetCurrentLogFile(monthlyDir);

            int countTemp = logNeed2Add.Count;
            List<log> logsTemp = logNeed2Add.GetRange(0, countTemp);

            WriteLogToExcel(currentFile, logsTemp);

            logNeed2Add.RemoveRange(0, countTemp);// 清空已添加的日志
        }

        private void WriteLogToExcel(FileCounter fileCounter, List<log> logs)
        {
            try
            {
                int remainingSpace = FileCounter.MaxRows - fileCounter.RowCount;

                if (remainingSpace == 0)
                {
                    // 当前文件已满，创建新文件
                    var newFile = GetCurrentLogFile(Path.GetDirectoryName(fileCounter.FilePath));
                    WriteLogToExcel(newFile, logs);
                    return;
                }

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
                            log temp = logs[i];
                            worksheet.Cell(startRow + i, 1).Value = temp.Time;
                            worksheet.Cell(startRow + i, 2).Value = temp.LogLevel;
                            worksheet.Cell(startRow + i, 3).Value = temp.LogType;
                            worksheet.Cell(startRow + i, 4).Value = temp.LogContent;
                        }

                        workbook.Save();
                        fileCounter.RowCount += logsToWrite;
                        //writeDataHelper.RunLog($"写入文件: {Path.GetFileName(fileCounter.FilePath)}，新增行数: {logsToWrite}，总行数: {fileCounter.RowCount}");
                    }
                }

                // 处理剩余日志（如果有的话）
                if (logs.Count > logsToWrite)
                {
                    var remainingLogs = logs.Skip(logsToWrite).ToList();
                    var newFile = GetCurrentLogFile(Path.GetDirectoryName(fileCounter.FilePath));
                    WriteLogToExcel(newFile, remainingLogs);
                }
            }
            catch (Exception ex)
            {
                RunLog($"写入失败: {fileCounter.FilePath}\n{ex.Message}");
                _fileCounters.Remove(fileCounter.FilePath);
                var newFile = GetCurrentLogFile(Path.GetDirectoryName(fileCounter.FilePath));
                WriteLogToExcel(newFile, logs);
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
                RunLog($"读取{path}失败: {ex.Message}");
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
                RunLog($"查询失败: {ex.Message}");
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
            var tempFiles = new List<string>();
            var logFiles = new List<string>();

            string[] directorys = Directory.GetDirectories(logsDirectory);
            // 遍历所有月份目录
            for (int i = 0; i < directorys.Length; i++)
            {
                // 遍历该月所有日志文件
                foreach (var file in Directory.GetFiles(directorys[i], "Log_*.xlsx"))
                {
                    // 从文件名解析时间（格式：Log_yyyyMMdd-HHmmss.xlsx）
                    //string fileName = Path.GetFileNameWithoutExtension(file);
                    //string timePart = fileName.Substring(4); // 去掉"Log_"

                    //if (DateTime.TryParseExact(timePart, "yyyyMMdd-HHmmss", null, DateTimeStyles.None, out DateTime fileTime))
                    //{
                    //    // 优化：如果文件创建时间完全不在查询范围内，则跳过（减少文件打开次数）
                    //    if (fileTime.Date > endDate.Date || fileTime.Date < startDate.Date)
                    //        continue;
                    //}

                    // 如果文件名时间在范围内，或无法解析时间，则加入待检查列表
                    tempFiles.Add(file);
                }
            }

            //文件名降序
            tempFiles = tempFiles.OrderByDescending(f => f).ToList();


            //筛选符合查询条件的文件名
            //1、找到时间范围内的最后一个文件
            //2、找的时间范围内的第一个文件的前一个文件
            for(int i=0;i< tempFiles.Count;i++ )
            {
                // 从文件名解析时间（格式：Log_yyyyMMdd-HHmmss.xlsx）
                string fileName = Path.GetFileNameWithoutExtension(tempFiles[i]);
                string timePart = fileName.Substring(4); // 去掉"Log_"

                if (DateTime.TryParseExact(timePart, "yyyyMMdd-HHmmss", null, DateTimeStyles.None, out DateTime fileTime))
                {
                    if (fileTime > endDate)
                        continue;

                    logFiles.Add(tempFiles[i]);

                    if (fileTime < startDate)
                        break;
                }

            }

            return logFiles;
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
            //确保目录存在
            if (!Directory.Exists(monthlyDir))
            {
                Directory.CreateDirectory(monthlyDir);
            }


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
                    RunLog($"加载已有文件失败: {file}\n{ex.Message}");
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
            RunLog($"创建新文件: {newFilePath}");

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

        public void DelegteDisplayComInfo(string message)
        {
            try
            {
                if (richTextBox1.InvokeRequired)
                {
                    richTextBox1.Invoke(new Action(() =>
                    {
                        richTextBox1.AppendText(message + '\n');
                        richTextBox1.Select(richTextBox1.TextLength, 0);
                        richTextBox1.ScrollToCaret();
                    }));
                }
                else
                {
                    richTextBox1.AppendText(message + '\n');
                    richTextBox1.Select(richTextBox1.TextLength, 0);
                    richTextBox1.ScrollToCaret();
                }
            }
            catch (Exception ex)
            {
                RunLog($"日志显示异常: {ex.Message}");
            }
        }

        public void DelegteDisplayOperateLog(string message, DateTime datetime)
        {
            try
            {
                message = $"{datetime:yyyy-MM-dd HH:mm:ss.fff}=>{message}";
                if (richTextBox2.InvokeRequired)
                {
                    richTextBox2.Invoke(new Action(() =>
                    {
                        richTextBox2.AppendText(message + '\n');
                        richTextBox2.Select(richTextBox2.TextLength, 0);
                        richTextBox2.ScrollToCaret();
                    }));
                }
                else
                {
                    richTextBox2.AppendText(message + '\n');
                    richTextBox2.Select(richTextBox2.TextLength, 0);
                    richTextBox2.ScrollToCaret();
                }
            }
            catch (Exception ex)
            {
                RunLog($"操作日志显示异常: {ex.Message}");
            }
        }
        public void DelegteDisplayAlarmLog(string message, DateTime datetime)
        {
            try
            {
                message = $"{datetime:yyyy-MM-dd HH:mm:ss.fff}=>{message}";
                if (richTextBox3.InvokeRequired)
                {
                    richTextBox3.Invoke(new Action(() =>
                    {
                        richTextBox3.AppendText(message + '\n');
                        richTextBox3.Select(richTextBox3.TextLength, 0);
                        richTextBox3.ScrollToCaret();
                    }));
                }
                else
                {
                    richTextBox3.AppendText(message + '\n');
                    richTextBox3.Select(richTextBox3.TextLength, 0);
                    richTextBox3.ScrollToCaret();
                }
            }
            catch (Exception ex)
            {
                RunLog($"报警日志显示异常: {ex.Message}");
            }
        }

        private void uiComboBox11_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                uiComboBox11.DataSource = SerialPort.GetPortNames();
                uiComboBox11.Text = 设置信息.测试机信息.串口号;
            }
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            if (设备状态 == 0) Com.CloseCom();
            Com.OpenCom();
            if (Com.IsOpenCom)
            {
                MessageBox.Show("连接成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void chk_senddoutadd()
        {
            this.chk_senddout1.CheckedChanged += new System.EventHandler(this.chk_senddout1_CheckedChanged);
            this.chk_senddout2.CheckedChanged += new System.EventHandler(this.chk_senddout2_CheckedChanged);
            this.chk_senddout3.CheckedChanged += new System.EventHandler(this.chk_senddout3_CheckedChanged);
            this.chk_senddout4.CheckedChanged += new System.EventHandler(this.chk_senddout4_CheckedChanged);
            this.chk_senddout5.CheckedChanged += new System.EventHandler(this.chk_senddout5_CheckedChanged);
            this.chk_senddout6.CheckedChanged += new System.EventHandler(this.chk_senddout6_CheckedChanged);
            this.chk_senddout7.CheckedChanged += new System.EventHandler(this.chk_senddout7_CheckedChanged);
            this.chk_senddout8.CheckedChanged += new System.EventHandler(this.chk_senddout8_CheckedChanged);
        }
        private void chk_senddoutsub()
        {
            this.chk_senddout1.CheckedChanged -= new System.EventHandler(this.chk_senddout1_CheckedChanged);
            this.chk_senddout2.CheckedChanged -= new System.EventHandler(this.chk_senddout2_CheckedChanged);
            this.chk_senddout3.CheckedChanged -= new System.EventHandler(this.chk_senddout3_CheckedChanged);
            this.chk_senddout4.CheckedChanged -= new System.EventHandler(this.chk_senddout4_CheckedChanged);
            this.chk_senddout5.CheckedChanged -= new System.EventHandler(this.chk_senddout5_CheckedChanged);
            this.chk_senddout6.CheckedChanged -= new System.EventHandler(this.chk_senddout6_CheckedChanged);
            this.chk_senddout7.CheckedChanged -= new System.EventHandler(this.chk_senddout7_CheckedChanged);
            this.chk_senddout8.CheckedChanged -= new System.EventHandler(this.chk_senddout8_CheckedChanged);
        }

        private List<bool[]> GetSiteStatus(out short site)
        {
            site = (short)(Convert.ToInt16(uiComboBox1.Text) - 1);
            List<bool[]> isopen = new List<bool[]>()
            {

                new bool[] { chk_senddout1.Checked, chk_senddout2.Checked, chk_senddout3.Checked, chk_senddout4.Checked },
                new bool[] { chk_senddout5.Checked, chk_senddout6.Checked, chk_senddout7.Checked, chk_senddout8.Checked }
            };
            return isopen;
        }

        /// <summary>
        /// 获取发送测试信息
        /// </summary>
        /// <returns></returns>
        private string GetSendisopen()
        {
            List<bool[]> isopen = GetSiteStatus(out short site);
            string str = Com.getstartest(site, isopen);
            return "@START_TEST " + str + "+";
        }
        private void uiComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            richtext_senddata.Text = GetSendisopen();
        }

        private void uiButton3_Click(object sender, EventArgs e)
        {
            chk_senddoutsub();
            chk_senddout1.Checked = true;
            chk_senddout2.Checked = true;
            chk_senddout3.Checked = true;
            chk_senddout4.Checked = true;
            chk_senddout5.Checked = true;
            chk_senddout6.Checked = true;
            chk_senddout7.Checked = true;
            chk_senddout8.Checked = true;
            chk_senddoutadd();
            richtext_senddata.Text = GetSendisopen();
        }

        private void chk_senddout1_CheckedChanged(object sender, EventArgs e)
        {
            richtext_senddata.Text = GetSendisopen();
        }

        private void chk_senddout2_CheckedChanged(object sender, EventArgs e)
        {
            richtext_senddata.Text = GetSendisopen();
        }

        private void chk_senddout3_CheckedChanged(object sender, EventArgs e)
        {
            richtext_senddata.Text = GetSendisopen();
        }

        private void chk_senddout4_CheckedChanged(object sender, EventArgs e)
        {
            richtext_senddata.Text = GetSendisopen();
        }

        private void chk_senddout5_CheckedChanged(object sender, EventArgs e)
        {
            richtext_senddata.Text = GetSendisopen();
        }

        private void chk_senddout6_CheckedChanged(object sender, EventArgs e)
        {
            richtext_senddata.Text = GetSendisopen();
        }

        private void chk_senddout7_CheckedChanged(object sender, EventArgs e)
        {
            richtext_senddata.Text = GetSendisopen();
        }

        private void chk_senddout8_CheckedChanged(object sender, EventArgs e)
        {
            richtext_senddata.Text = GetSendisopen();
        }

        private void uiButton4_Click(object sender, EventArgs e)
        {
            chk_senddoutsub();
            chk_senddout1.Checked = false;
            chk_senddout2.Checked = false;
            chk_senddout3.Checked = false;
            chk_senddout4.Checked = false;
            chk_senddout5.Checked = false;
            chk_senddout6.Checked = false;
            chk_senddout7.Checked = false;
            chk_senddout8.Checked = false;
            chk_senddoutadd();
            richtext_senddata.Text = GetSendisopen();
        }

        private void uiButton5_Click(object sender, EventArgs e)
        {
            List<bool[]> isopen = GetSiteStatus(out short testnum);
            //if (SiteTestState[testnum] == 1)
            //{
            DialogResult dialog = MessageBox.Show("请确认是否发送测试信号?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog == DialogResult.OK)
            {
                Com.Send(testnum, isopen);
            }
            //}
            //else Com.Send(testnum, isopen);
        }

        private void uiButton6_Click(object sender, EventArgs e)
        {
            int site = (short)(Convert.ToInt16(uiComboBox1.Text) - 1);
            if (SiteTestState[site] == 2)
            {
                Com.Listtimer[site].Stop();
                //Com.Iswaitout[site] = true;
                string[] bin = { "20A", "20A", "20A", "20A", "20A", "20A", "20A", "20A" };
                if (Com.Bin.ContainsKey(site))
                {
                    Com.Bin[site] = bin;
                }
                else Com.Bin.Add(site, bin);
                OutSiteQueue.Enqueue(site);
                BinSaveQueue.Enqueue(bin);
                SiteTestState[site] = 3;
                //用来统计数据
                Com.OutBin.Enqueue(new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] });
                Com.Listtestsingal.Add(site);
            }
            else
            {
                MessageBox.Show($"Site:{site + 1}未在测试!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

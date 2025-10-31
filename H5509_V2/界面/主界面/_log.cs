using System;
using System.Collections.Concurrent;
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
using SQLiteHelper;
using SQLServerHelper;

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

        public ConcurrentQueue<log> logNeed2Add = new ConcurrentQueue<log>(); // 用于存储需要添加的日志

        public bool IsSave = false;

        public Action 发送批次信息 { get; set; } // 用于发送批量信息的委托

        private RichTextBox[] siteRichTextBoxs = new RichTextBox[17];

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


            dateTimePicker1.Value = DateTime.Now; //开始时间的结束时间
            dateTimePicker2.Value = DateTime.Now.AddDays(-1); //开始时间的开始时间

            textBox2.Text = Directory.GetCurrentDirectory() + "\\Log";

            siteRichTextBoxs = new RichTextBox[]
            {
                richTextBox1, richTextBox4,richTextBox5,richTextBox6,richTextBox7, richTextBox8,richTextBox9,richTextBox10, richTextBox11,
                richTextBox12, richTextBox13, richTextBox14, richTextBox15, richTextBox16, richTextBox17, richTextBox18, richTextBox19
            };
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
            刷新日志界面(uiTabControl6.SelectedTab.Text);
            //uiComboBox11.Text = 设置信息.测试机信息.串口号;
            //uiComboBox12.Text = 设置信息.测试机信息.波特率.ToString();
            //uiComboBox14.Text = 设置信息.测试机信息.校验位.ToString();
            //uiComboBox13.Text = 设置信息.测试机信息.数据位.ToString();
            //uiComboBox15.Text = 设置信息.测试机信息.停止位.ToString();
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
        public void AddLog(string level, string logType, string info)
        {
            logNeed2Add.Enqueue(new log
            {
                Time = DateTime.Now,
                LogLevel = level,
                LogType = logType,
                LogContent = info
            });
            //Debug.WriteLine("输出表格数量："+logNeed2Add.Count);
            //if (logNeed2Add.Count >= 1000) // 超过1000条立即写入
            //{
            //    LogTimer_Tick(null, null); // 直接调用定时器事件处理方法
            //}

        }

        public void LogTimer_Tick(object sender, EventArgs e)
        {
            IsSave = false;

            if (logNeed2Add.Count == 0) return; // 没有日志需要写入

            int countTemp = logNeed2Add.Count;
            List<log> logsTemp = new List<log>();
            for (int i = 0; i < countTemp; i++)
            {
                logNeed2Add.TryDequeue(out log log);
                logsTemp.Add(log);
            }

            //Debug.WriteLine("获取表格数量:"+ logNeed2Add.Count);

            string monthlyDir = Path.Combine(logsDirectory, logsTemp[0].Time.ToString("yyyyMM"));
            var currentFile = GetCurrentLogFile(monthlyDir);

            WriteLogToExcel(currentFile, logsTemp);

            IsSave = true;
            //logNeed2Add.RemoveRange(0, countTemp);// 清空已添加的日志
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
                        DateTime logTime = row.Cell(1).GetDateTime().ToString().ToDateTime();

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
            for (int i = 0; i < tempFiles.Count; i++)
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

        public void DisPlayComInfo(RichTextBox richTextBox, string message)
        {
            try
            {
                if (richTextBox.InvokeRequired)
                {
                    richTextBox.Invoke(new Action(() =>
                    {
                        richTextBox.AppendText(message + '\n');
                        richTextBox.Select(richTextBox.TextLength, 0);
                        richTextBox.ScrollToCaret();
                    }));
                }
                else
                {
                    richTextBox.AppendText(message + '\n');
                    richTextBox.Select(richTextBox.TextLength, 0);
                    richTextBox.ScrollToCaret();
                }
            }
            catch (Exception ex)
            {
                RunLog($"日志显示异常: {ex.Message}");
            }

        }

        public void DelegteDisplayComInfo(int site, string message)
        {
            DisPlayComInfo(siteRichTextBoxs[0], message);
            DisPlayComInfo(siteRichTextBoxs[site + 1], message);
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
            //if (e.Button == MouseButtons.Left)
            //{
            //    uiComboBox11.DataSource = SerialPort.GetPortNames();
            //    //uiComboBox11.Text = 设置信息.测试机信息.串口号;
            //}
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            StationManagers.ConnectAll();
            bool[] check = StationManagers.CheckComConnect();
            if (check.Contains(false))
            {
                for (int i = 0; i < check.Length; i++)
                {
                    if (!check[i])
                    {
                        MessageBox.Show($"测试机{i + 1}通讯未连接", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else MessageBox.Show($"连接成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //if (设备状态 == 0) Com.CloseCom();
            //Com.OpenCom();
            //if (Com.IsOpenCom)
            //{
            //    MessageBox.Show("连接成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
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
            this.chk_senddout9.CheckedChanged += new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout10.CheckedChanged += new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout11.CheckedChanged += new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout12.CheckedChanged += new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout13.CheckedChanged += new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout14.CheckedChanged += new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout15.CheckedChanged += new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout16.CheckedChanged += new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout17.CheckedChanged += new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout18.CheckedChanged += new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout19.CheckedChanged += new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout20.CheckedChanged += new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout21.CheckedChanged += new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout22.CheckedChanged += new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout23.CheckedChanged += new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout24.CheckedChanged += new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout25.CheckedChanged += new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout26.CheckedChanged += new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout27.CheckedChanged += new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout28.CheckedChanged += new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout29.CheckedChanged += new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout30.CheckedChanged += new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout31.CheckedChanged += new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout32.CheckedChanged += new System.EventHandler(this.uiCheckBox2_CheckedChanged);
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
            this.chk_senddout9.CheckedChanged -= new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout10.CheckedChanged -= new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout11.CheckedChanged -= new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout12.CheckedChanged -= new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout13.CheckedChanged -= new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout14.CheckedChanged -= new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout15.CheckedChanged -= new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout16.CheckedChanged -= new System.EventHandler(this.chk_senddout16_CheckedChanged);
            this.chk_senddout17.CheckedChanged -= new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout18.CheckedChanged -= new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout19.CheckedChanged -= new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout20.CheckedChanged -= new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout21.CheckedChanged -= new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout22.CheckedChanged -= new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout23.CheckedChanged -= new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout24.CheckedChanged -= new System.EventHandler(this.uiCheckBox11_CheckedChanged);
            this.chk_senddout25.CheckedChanged -= new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout26.CheckedChanged -= new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout27.CheckedChanged -= new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout28.CheckedChanged -= new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout29.CheckedChanged -= new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout30.CheckedChanged -= new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout31.CheckedChanged -= new System.EventHandler(this.uiCheckBox2_CheckedChanged);
            this.chk_senddout32.CheckedChanged -= new System.EventHandler(this.uiCheckBox2_CheckedChanged);
        }

        private void GetSiteStatus(out short site, out List<bool[]> listbool)
        {
            site = (short)(Convert.ToInt16(uiComboBox1.Text) - 1);
            List<bool[]> isopen = new List<bool[]>()
            {
                new bool[] { chk_senddout1.Checked, chk_senddout2.Checked, chk_senddout3.Checked, chk_senddout4.Checked },
                new bool[] { chk_senddout5.Checked, chk_senddout6.Checked, chk_senddout7.Checked, chk_senddout8.Checked }
            };
            listbool = isopen;
        }

        private void GetSiteStatus(out List<List<bool[]>> listbool)//out short site, 
        {
            //site = (short)(Convert.ToInt16(uiComboBox1.Text) - 1);
            List<bool[]> isopen = new List<bool[]>()
            {
                new bool[] { chk_senddout1.Checked, chk_senddout2.Checked, chk_senddout3.Checked, chk_senddout4.Checked },
                new bool[] { chk_senddout5.Checked, chk_senddout6.Checked, chk_senddout7.Checked, chk_senddout8.Checked }
            };
            List<bool[]> isopen2 = new List<bool[]>()
            {
                new bool[] { chk_senddout9.Checked, chk_senddout10.Checked, chk_senddout11.Checked, chk_senddout12.Checked },
                new bool[] { chk_senddout13.Checked, chk_senddout14.Checked, chk_senddout15.Checked, chk_senddout16.Checked }
            };
            List<bool[]> isopen3 = new List<bool[]>()
            {
                new bool[] { chk_senddout17.Checked, chk_senddout18.Checked, chk_senddout19.Checked, chk_senddout20.Checked },
                new bool[] { chk_senddout21.Checked, chk_senddout22.Checked, chk_senddout23.Checked, chk_senddout24.Checked }
            };
            List<bool[]> isopen4 = new List<bool[]>()
            {
                new bool[] { chk_senddout25.Checked, chk_senddout26.Checked, chk_senddout27.Checked, chk_senddout28.Checked },
                new bool[] { chk_senddout29.Checked, chk_senddout30.Checked, chk_senddout31.Checked, chk_senddout32.Checked }
            };
            if (设置信息.通讯信息.同时测试 == 2)
            {
                listbool = new List<List<bool[]>>()
                {
                    isopen,
                    isopen2
                };
            }
            else
            {
                listbool = new List<List<bool[]>>()
                {
                    isopen,
                    isopen2,
                    isopen3,
                    isopen4,
                };
            }
        }

        /// <summary>
        /// 获取发送测试信息
        /// </summary>
        /// <returns></returns>
        private string GetSendisopen()
        {
            if (设置信息.通讯信息.同时测试 == 1)
            {
                if (设置信息.通讯信息.通讯协议 == "衡宇")
                {
                    GetSiteStatus(out short site, out List<bool[]> listbool);
                    string str = TestStation.getstartest(TestStation.GetSiteNum((int)site), listbool);
                    //string str = TestStation.getstartest(site, listbool);
                    return "@START_TEST " + str + "+";
                }
                else
                {
                    GetSiteStatus(out short site, out List<bool[]> listbool);
                    string str = TestStation.getstartest(site, listbool);
                    return "@START_TEST " + str + "+";
                }
            }
            else
            {
                GetSiteStatus(out List<List<bool[]>> listbool);
                string str = TestStation.getstartest(listbool);
                return "@START_TEST " + str + "+";
            }
        }
        private void uiComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            richtext_senddata.Text = GetSendisopen();
        }

        private void uiButton3_Click(object sender, EventArgs e)
        {
            int num = 设置信息.通讯信息.同时测试;
            chk_senddoutsub();
            chk_senddout1.Checked = num == 1 || num == 2 || num == 4;
            chk_senddout2.Checked = num == 1 || num == 2 || num == 4;
            chk_senddout3.Checked = num == 1 || num == 2 || num == 4;
            chk_senddout4.Checked = num == 1 || num == 2 || num == 4;
            chk_senddout5.Checked = num == 1 || num == 2 || num == 4;
            chk_senddout6.Checked = num == 1 || num == 2 || num == 4;
            chk_senddout7.Checked = num == 1 || num == 2 || num == 4;
            chk_senddout8.Checked = num == 1 || num == 2 || num == 4;
            chk_senddout9.Checked = num == 2 || num == 4;
            chk_senddout10.Checked = num == 2 || num == 4;
            chk_senddout11.Checked = num == 2 || num == 4;
            chk_senddout12.Checked = num == 2 || num == 4;
            chk_senddout13.Checked = num == 2 || num == 4;
            chk_senddout14.Checked = num == 2 || num == 4;
            chk_senddout15.Checked = num == 2 || num == 4;
            chk_senddout16.Checked = num == 2 || num == 4;
            chk_senddout17.Checked = num == 4;
            chk_senddout18.Checked = num == 4;
            chk_senddout19.Checked = num == 4;
            chk_senddout20.Checked = num == 4;
            chk_senddout21.Checked = num == 4;
            chk_senddout22.Checked = num == 4;
            chk_senddout23.Checked = num == 4;
            chk_senddout24.Checked = num == 4;
            chk_senddout25.Checked = num == 4;
            chk_senddout26.Checked = num == 4;
            chk_senddout27.Checked = num == 4;
            chk_senddout28.Checked = num == 4;
            chk_senddout29.Checked = num == 4;
            chk_senddout30.Checked = num == 4;
            chk_senddout31.Checked = num == 4;
            chk_senddout32.Checked = num == 4;
            //chk_senddout1.Checked = true;
            //chk_senddout2.Checked = true;
            //chk_senddout3.Checked = true;
            //chk_senddout4.Checked = true;
            //chk_senddout5.Checked = true;
            //chk_senddout6.Checked = true;
            //chk_senddout7.Checked = true;
            //chk_senddout8.Checked = true;
            //chk_senddout9.Checked = true;
            //chk_senddout10.Checked = true;
            //chk_senddout11.Checked = true;
            //chk_senddout12.Checked = true;
            //chk_senddout13.Checked = true;
            //chk_senddout14.Checked = true;
            //chk_senddout15.Checked = true;
            //chk_senddout16.Checked = true;
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
            chk_senddout9.Checked = false;
            chk_senddout10.Checked = false;
            chk_senddout11.Checked = false;
            chk_senddout12.Checked = false;
            chk_senddout13.Checked = false;
            chk_senddout14.Checked = false;
            chk_senddout15.Checked = false;
            chk_senddout16.Checked = false;
            chk_senddout17.Checked = false;
            chk_senddout18.Checked = false;
            chk_senddout19.Checked = false;
            chk_senddout20.Checked = false;
            chk_senddout21.Checked = false;
            chk_senddout22.Checked = false;
            chk_senddout23.Checked = false;
            chk_senddout24.Checked = false;
            chk_senddout25.Checked = false;
            chk_senddout26.Checked = false;
            chk_senddout27.Checked = false;
            chk_senddout28.Checked = false;
            chk_senddout29.Checked = false;
            chk_senddout30.Checked = false;
            chk_senddout31.Checked = false;
            chk_senddout32.Checked = false;
            chk_senddoutadd();
            richtext_senddata.Text = GetSendisopen();
        }

        private void uiButton5_Click(object sender, EventArgs e) //@02@#ABCD#
        {
            //@02@#BCD#
            if (设备状态 == 1)
            {
                MessageBox.Show("设备正在运行中...!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (当前权限 == "Operator")
            {
                MessageBox.Show("权限不足!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //List<bool[]> isopen = GetSiteStatus(out short testnum);
            //if (SiteTestState[testnum] == 1)
            //{

            if (richtext_senddata.Text.Trim().Length == 0)
            {
                MessageBox.Show("请输入要发送的数据!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int site = Convert.ToInt32(uiComboBox1.Text);
            DialogResult dialog = MessageBox.Show($"请确认是否发送数据到测试机{TestStation.GetTesterId(site - 1) + 1}?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog == DialogResult.OK)
            {
                StationManagers.Send(site - 1, richtext_senddata.Text); //有点复杂后续再弄，晶存需要
                //Com.Send(testnum, isopen);
            }
            //}
            //else Com.Send(testnum, isopen);
        }
        //private void GetTimeOutBin(int site)
        //{
        //    if (设置信息.通讯信息.通讯协议 == "得一微")
        //    {
        //        int[] sites = { StationId, StationId + 1 };

        //        for (int i = 0; i < sites.Length; i++)
        //        {
        //            if (testDataInfos[sites[i]].SiteTestState == 2)
        //            {
        //                testDataInfos[sites[i]].Iswaitout = true;

        //                testDataInfos[sites[i]].listtesttimespan = DateTime.Now - testDataInfos[sites[i]].listtestdatetime;
        //                testDataInfos[sites[i]].TestTime = DateTime.Now - testDataInfos[sites[i]].listtestdatetime;

        //                testDataInfos[sites[i]].SiteTestBin = new string[] { "20A", "20A", "20A", "20A", "20A", "20A", "20A", "20A" };
        //                testDataInfos[sites[i]].SiteTestState = 3;

        //                DataUpdate(sites[i], testDataInfos[sites[i]]);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        testDataInfos[site].Iswaitout = true;

        //        testDataInfos[site].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
        //        testDataInfos[site].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;

        //        testDataInfos[site].SiteTestBin = new string[] { "20A", "20A", "20A", "20A", "20A", "20A", "20A", "20A" };
        //        testDataInfos[site].SiteTestState = 3;

        //        DataUpdate(site, testDataInfos[site]);
        //    }
        //}

        private void uiButton6_Click(object sender, EventArgs e)
        {
            //int site = (short)(Convert.ToInt16(uiComboBox1.Text) - 1);
            //if (SiteTestState[site] == 2)
            //{
            //    Com.Listtimer[site].Stop();
            //    //Com.Iswaitout[site] = true;
            //    string[] bin = { "20A", "20A", "20A", "20A", "20A", "20A", "20A", "20A" };
            //    if (Com.Bin.ContainsKey(site))
            //    {
            //        Com.Bin[site] = bin;
            //    }
            //    else Com.Bin.Add(site, bin);
            //    //OutSiteQueue.Enqueue(site);
            //    //BinSaveQueue.Enqueue(bin);

            //    OutSiteQueue.Enqueue((site, new TestDataInfo() {SiteTestState = 3, SiteTestBin = bin }));

            //    SiteTestBin[site] = new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] };
            //    SiteTestState[site] = 3;
            //    //用来统计数据
            //    Com.OutBin.Enqueue(new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] });
            //    Com.Listtestsingal.Add(site);
            //}
            //else
            //{
            //    MessageBox.Show($"Site:{site + 1}未在测试!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        private void uiButton18_Click(object sender, EventArgs e)
        {
            update_listBox1();
        }

        public void update_listBox1()
        {
            listBox1.Items.Clear();
            string startime = uiCheckBox210.Checked ? $"and 开始时间 <= '{dateTimePicker1.Value:yyyy年MM月dd日 HH:mm:ss}'" : "";
            string endtime = uiCheckBox1.Checked ? $"and 开始时间 >= '{dateTimePicker2.Value:yyyy年MM月dd日 HH:mm:ss}'" : ""; //结束时间
            string sql = $"select 开始时间,工单号,生产模式 from FlowcaedNoInfo where 工单号 like '%{textBox5.Text}%' {startime} {endtime} order by 开始时间 desc";

            string res = Sqlite.runSql(SQLiteRunningString, sql, out DataTable dt);
            if (res != "Success" || dt.Rows.Count < 1) return;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                listBox1.Items.Add(dt.Rows[i].ItemArray[0] + "_" + dt.Rows[i].ItemArray[1] + "_" + dt.Rows[i].ItemArray[2]);
            }
        }

        public void 刷新日志界面(string text)
        {
            //点击数据查询时更新数据
            if (text == "数据查询")
            {
                //button23.PerformClick();
                uiButton18_Click(uiButton18, EventArgs.Empty);
                if (listBox1.Items.Count > 0) listBox1.SelectedIndex = 0;

                switch (tabControl6.SelectedTab.Text)
                {
                    case "端口信息":
                        {
                            switch (tabControl7.SelectedTab.Text)
                            {
                                //dut总良率
                                case "dut总良率": uiButton7_Click(uiButton7, EventArgs.Empty); break;
                                //当前工单dut实时良率
                                case "当前工单dut实时良率": uiButton9_Click(uiButton9, EventArgs.Empty); break;
                                //工单dut良率查询
                                case "工单dut良率查询": uiButton10_Click(uiButton10, EventArgs.Empty); break;
                                //socket使用次数
                                case "socket使用次数": uiButton13_Click(uiButton13, EventArgs.Empty); break;
                            }
                        }
                        break;
                    //工单详情
                    case "工单详情": uiButton14_Click(uiButton14, EventArgs.Empty); break;
                    //Bin项信息
                    case "Bin项信息": uiButton34_Click(uiButton34, EventArgs.Empty); break;
                    //料表信息
                    case "料表信息": uiButton17_Click(uiButton17, EventArgs.Empty); break;
                }
            }
            else if (text == "通讯log")
            {
                int num = 设置信息.通讯信息.同时测试;
                chk_senddout1.Enabled = num == 1 || num == 2 || num == 4;
                chk_senddout2.Enabled = num == 1 || num == 2 || num == 4;
                chk_senddout3.Enabled = num == 1 || num == 2 || num == 4;
                chk_senddout4.Enabled = num == 1 || num == 2 || num == 4;
                chk_senddout5.Enabled = num == 1 || num == 2 || num == 4;
                chk_senddout6.Enabled = num == 1 || num == 2 || num == 4;
                chk_senddout7.Enabled = num == 1 || num == 2 || num == 4;
                chk_senddout8.Enabled = num == 1 || num == 2 || num == 4;
                chk_senddout9.Enabled = num == 2 || num == 4;
                chk_senddout10.Enabled = num == 2 || num == 4;
                chk_senddout11.Enabled = num == 2 || num == 4;
                chk_senddout12.Enabled = num == 2 || num == 4;
                chk_senddout13.Enabled = num == 2 || num == 4;
                chk_senddout14.Enabled = num == 2 || num == 4;
                chk_senddout15.Enabled = num == 2 || num == 4;
                chk_senddout16.Enabled = num == 2 || num == 4;
                chk_senddout17.Enabled = num == 4;
                chk_senddout18.Enabled = num == 4;
                chk_senddout19.Enabled = num == 4;
                chk_senddout20.Enabled = num == 4;
                chk_senddout21.Enabled = num == 4;
                chk_senddout22.Enabled = num == 4;
                chk_senddout23.Enabled = num == 4;
                chk_senddout24.Enabled = num == 4;
                chk_senddout25.Enabled = num == 4;
                chk_senddout26.Enabled = num == 4;
                chk_senddout27.Enabled = num == 4;
                chk_senddout28.Enabled = num == 4;
                chk_senddout29.Enabled = num == 4;
                chk_senddout30.Enabled = num == 4;
                chk_senddout31.Enabled = num == 4;
                chk_senddout32.Enabled = num == 4;
            }
        }

        private void uiTabControl6_Selecting(object sender, TabControlCancelEventArgs e)
        {
            刷新日志界面(e.TabPage.Text);

        }

        //dut总良率刷新
        private void uiButton7_Click(object sender, EventArgs e)
        {
            dataGridView_dutAll.Columns.Clear();
            //初始化dataGridView_dutAll
            dataGridView_dutAll.EnableHeadersVisualStyles = false;
            dataGridView_dutAll.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dataGridView_dutAll.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            //dataGridView_dutAll.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dataGridView_dutAll.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView_dutAll.DefaultCellStyle.Font = new Font("微软雅黑", 9);
            for (int i = 0; i < 9; i++)
            {
                if (i == 0)
                {
                    dataGridView_dutAll.Columns.Add(0.ToString(), "Site号");
                    dataGridView_dutAll.Columns[i].Width = 60;
                }
                else
                {
                    dataGridView_dutAll.Columns.Add(i.ToString(), i.ToString());
                    dataGridView_dutAll.Columns[i].Width = 119;
                }
                dataGridView_dutAll.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView_dutAll.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //dataGridView_dutAll.Columns[i].HeaderCell.Style.BackColor = Color.LightGray;

            }
            dataGridView_dutAll.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView_dutAll.ColumnHeadersHeight = 30;
            for (int i = 0; i < 16; i++)
            {
                dataGridView_dutAll.Rows.Add();
                dataGridView_dutAll.Rows[i].Cells[0].Value = i.ToString("D2");
                dataGridView_dutAll.Rows[i].Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView_dutAll.Rows[i].Cells[0].ReadOnly = false;
                dataGridView_dutAll.Rows[i].Cells[0].Style.BackColor = Color.LightGray;
                dataGridView_dutAll.Rows[i].Cells[0].Style.SelectionBackColor = Color.LightGray;
                dataGridView_dutAll.Rows[i].Cells[0].Style.SelectionForeColor = Color.Black;
                dataGridView_dutAll.Rows[i].Height = 47;
            }
            update_dataGridView_dutAll();

        }
        public void update_dataGridView_dutAll()
        {
            Task.Run(new Action(() =>
            {
                string sql = "";
                try
                {
                    sql = "SELECT * FROM dutAll";
                    Sqlite.runSql(SQLiteRunningString, sql, out DataTable dataTable);
                    this.Invoke(new Action(() =>
                    {
                        foreach (DataRow dr in dataTable.Rows)
                        {
                            if (dr.ItemArray[3].ToString().Trim(' ') == "0")
                            {
                                continue;
                            }
                            TimeSpan ts;
                            if (dr.ItemArray[3].ToString().Trim(' ').Length < 7)
                            {
                                ts = new TimeSpan(long.Parse(dr.ItemArray[3].ToString()));
                            }
                            else
                            {
                                ts = new TimeSpan(long.Parse(dr.ItemArray[3].ToString().Substring(0, dr.ItemArray[3].ToString().Length - 7) + "0000000"));
                            }
                            string value = ts.ToString().Replace(".", "天") + "\r\n";
                            value += dr.ItemArray[1].ToString() + @"/" + dr.ItemArray[2].ToString() + "=";
                            double d = double.Parse(dr.ItemArray[1].ToString()) / double.Parse(dr.ItemArray[2].ToString());

                            if (d.ToString("P1") == "100.0%")
                            {
                                d = ((int)(d * 1000)) * 1.0 / 1000;
                            }
                            value += d.ToString("P1");

                            dataGridView_dutAll.Rows[int.Parse(dr.ItemArray[0].ToString().Substring(0, 2))].Cells[int.Parse(dr.ItemArray[0].ToString().Substring(2, 1))].Value = value;
                        }
                    }));
                }
                catch (Exception ex)
                {
                    RunLog("查询Dut总良率失败!" + sql);
                }
            }));
        }


        //当前工单dut实时良率刷新
        private void uiButton9_Click(object sender, EventArgs e)
        {
            dataGridView_dutCurrent.Columns.Clear();
            //初始化dataGridView_dutCurrent
            dataGridView_dutCurrent.EnableHeadersVisualStyles = false;
            dataGridView_dutCurrent.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dataGridView_dutCurrent.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            //dataGridView_dutCurrent.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dataGridView_dutCurrent.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView_dutCurrent.DefaultCellStyle.Font = new Font("微软雅黑", 9);
            for (int i = 0; i < 9; i++)
            {
                if (i == 0)
                {
                    dataGridView_dutCurrent.Columns.Add(0.ToString(), "Site号");
                    dataGridView_dutCurrent.Columns[i].Width = 60;
                }
                else
                {
                    dataGridView_dutCurrent.Columns.Add(i.ToString(), i.ToString());
                    dataGridView_dutCurrent.Columns[i].Width = 119;
                }
                dataGridView_dutCurrent.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView_dutCurrent.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //dataGridView_dutCurrent.Columns[i].HeaderCell.Style.BackColor = Color.LightGray;

            }
            dataGridView_dutCurrent.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView_dutCurrent.ColumnHeadersHeight = 30;
            for (int i = 0; i < 16; i++)
            {
                dataGridView_dutCurrent.Rows.Add();
                dataGridView_dutCurrent.Rows[i].Cells[0].Value = i.ToString("D2");
                dataGridView_dutCurrent.Rows[i].Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView_dutCurrent.Rows[i].Cells[0].ReadOnly = false;
                dataGridView_dutCurrent.Rows[i].Cells[0].Style.BackColor = Color.LightGray;
                dataGridView_dutCurrent.Rows[i].Cells[0].Style.SelectionBackColor = Color.LightGray;
                dataGridView_dutCurrent.Rows[i].Cells[0].Style.SelectionForeColor = Color.Black;
                dataGridView_dutCurrent.Rows[i].Height = 47;
            }
            update_dataGridView_dutCurrent();


        }

        public void update_dataGridView_dutCurrent()
        {
            Task.Run(new Action(() =>
            {
                string sql = "";
                try
                {
                    sql = "SELECT * FROM dutCurrent";
                    Sqlite.runSql(SQLiteRunningString, sql, out DataTable dataTable);

                    this.Invoke(new Action(() =>
                    {
                        foreach (DataRow dr in dataTable.Rows)
                        {
                            if (dr.ItemArray[3].ToString().Trim(' ') == "0")
                            {
                                continue;
                            }
                            TimeSpan ts;
                            if (dr.ItemArray[3].ToString().Trim(' ').Length < 7)
                            {
                                ts = new TimeSpan(long.Parse(dr.ItemArray[3].ToString()));
                            }
                            else
                            {
                                ts = new TimeSpan(long.Parse(dr.ItemArray[3].ToString().Substring(0, dr.ItemArray[3].ToString().Length - 7) + "0000000"));
                            }
                            string value = ts.ToString().Replace(".", "天") + "\r\n";
                            value += dr.ItemArray[1].ToString() + @"/" + dr.ItemArray[2].ToString() + "=";
                            double d = double.Parse(dr.ItemArray[1].ToString()) / double.Parse(dr.ItemArray[2].ToString());

                            if (d.ToString("P1") == "100.0%")
                            {
                                d = ((int)(d * 1000)) * 1.0 / 1000;
                            }
                            value += d.ToString("P1");

                            dataGridView_dutCurrent.Rows[int.Parse(dr.ItemArray[0].ToString().Substring(0, 2))].Cells[int.Parse(dr.ItemArray[0].ToString().Substring(2, 1))].Value = value;
                        }
                    }));
                }
                catch (Exception ex)
                {
                    RunLog("查询当前工单dut实时良率失败!" + sql);
                }
            }));
        }



        //工单dut良率查询刷新
        private void uiButton10_Click(object sender, EventArgs e)
        {
            dataGridView_dutFlowcaedNo.Columns.Clear();
            //初始化dataGridView_dutFlowcaedNo
            dataGridView_dutFlowcaedNo.EnableHeadersVisualStyles = false;
            dataGridView_dutFlowcaedNo.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dataGridView_dutFlowcaedNo.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            //dataGridView_dutFlowcaedNo.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dataGridView_dutFlowcaedNo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView_dutFlowcaedNo.DefaultCellStyle.Font = new Font("微软雅黑", 9);
            for (int i = 0; i < 9; i++)
            {
                if (i == 0)
                {
                    dataGridView_dutFlowcaedNo.Columns.Add(0.ToString(), "Site号");
                    dataGridView_dutFlowcaedNo.Columns[i].Width = 60;
                }
                else
                {
                    dataGridView_dutFlowcaedNo.Columns.Add(i.ToString(), i.ToString());
                    dataGridView_dutFlowcaedNo.Columns[i].Width = 119;
                }
                dataGridView_dutFlowcaedNo.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView_dutFlowcaedNo.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //dataGridView_dutFlowcaedNo.Columns[i].HeaderCell.Style.BackColor = Color.LightGray;

            }
            dataGridView_dutFlowcaedNo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView_dutFlowcaedNo.ColumnHeadersHeight = 30;
            for (int i = 0; i < 16; i++)
            {
                dataGridView_dutFlowcaedNo.Rows.Add();
                dataGridView_dutFlowcaedNo.Rows[i].Cells[0].Value = i.ToString("D2");
                dataGridView_dutFlowcaedNo.Rows[i].Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView_dutFlowcaedNo.Rows[i].Cells[0].ReadOnly = false;
                dataGridView_dutFlowcaedNo.Rows[i].Cells[0].Style.BackColor = Color.LightGray;
                dataGridView_dutFlowcaedNo.Rows[i].Cells[0].Style.SelectionBackColor = Color.LightGray;
                dataGridView_dutFlowcaedNo.Rows[i].Cells[0].Style.SelectionForeColor = Color.Black;
                dataGridView_dutFlowcaedNo.Rows[i].Height = 47;
            }
            update_dataGridView_dutFlowcaedNo();
        }

        public void update_dataGridView_dutFlowcaedNo()
        {
            string[] s;
            if (listBox1.SelectedItem == null) return;
            else
            {
                s = listBox1.SelectedItem.ToString().Split('_');
            }

            string sql = "SELECT * FROM dut_" + s[1] + "_" + DateTime.Parse(s[0]).ToString("yyyyMMdd_HH_mm_ss");

            Task.Run(new Action(() =>
            {
                try
                {
                    SqlServer.runSql(SQLServerConnString, sql, out DataTable dataTable);

                    this.Invoke(new Action(() =>
                    {
                        foreach (DataRow dr in dataTable.Rows)
                        {
                            if (dr.ItemArray[3].ToString().Trim(' ') == "0")
                            {
                                continue;
                            }
                            TimeSpan ts;
                            if (dr.ItemArray[3].ToString().Trim(' ').Length < 7)
                            {
                                ts = new TimeSpan(long.Parse(dr.ItemArray[3].ToString()));
                            }
                            else
                            {
                                ts = new TimeSpan(long.Parse(dr.ItemArray[3].ToString().Substring(0, dr.ItemArray[3].ToString().Length - 7) + "0000000"));
                            }
                            string value = ts.ToString().Replace(".", "天") + "\r\n";
                            value += dr.ItemArray[1].ToString() + @"/" + dr.ItemArray[2].ToString() + "=";
                            double d = double.Parse(dr.ItemArray[1].ToString()) / double.Parse(dr.ItemArray[2].ToString());

                            if (d.ToString("P1") == "100.0%")
                            {
                                d = ((int)(d * 1000)) * 1.0 / 1000;
                            }
                            value += d.ToString("P1");

                            dataGridView_dutFlowcaedNo.Rows[int.Parse(dr.ItemArray[0].ToString().Substring(0, 2))].Cells[int.Parse(dr.ItemArray[0].ToString().Substring(2, 1))].Value = value;
                        }
                    }));
                }
                catch (Exception ex)
                {
                    RunLog("查询工单dut良率失败!" + sql);
                }
            }));
        }

        //socket使用次数刷新
        private void uiButton13_Click(object sender, EventArgs e)
        {
            dataGridView_socketUsed.Columns.Clear();
            //初始化dataGridView_socketUsed
            dataGridView_socketUsed.EnableHeadersVisualStyles = false;
            dataGridView_socketUsed.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dataGridView_socketUsed.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            //dataGridView_socketUsed.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dataGridView_socketUsed.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView_socketUsed.DefaultCellStyle.Font = new Font("微软雅黑", 9);
            for (int i = 0; i < 9; i++)
            {
                if (i == 0)
                {
                    dataGridView_socketUsed.Columns.Add(0.ToString(), "Site号");
                    dataGridView_socketUsed.Columns[i].Width = 60;
                }
                else
                {
                    dataGridView_socketUsed.Columns.Add(i.ToString(), i.ToString());
                    dataGridView_socketUsed.Columns[i].Width = 119;
                }
                dataGridView_socketUsed.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView_socketUsed.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //dataGridView_socketUsed.Columns[i].HeaderCell.Style.BackColor = Color.LightGray;

            }
            dataGridView_socketUsed.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView_socketUsed.ColumnHeadersHeight = 30;
            for (int i = 0; i < 16; i++)
            {
                dataGridView_socketUsed.Rows.Add();
                dataGridView_socketUsed.Rows[i].Cells[0].Value = i.ToString("D2");
                dataGridView_socketUsed.Rows[i].Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView_socketUsed.Rows[i].Cells[0].ReadOnly = false;
                dataGridView_socketUsed.Rows[i].Cells[0].Style.BackColor = Color.LightGray;
                dataGridView_socketUsed.Rows[i].Cells[0].Style.SelectionBackColor = Color.LightGray;
                dataGridView_socketUsed.Rows[i].Cells[0].Style.SelectionForeColor = Color.Black;
                dataGridView_socketUsed.Rows[i].Height = 47;
            }

            update_dataGridView_socketUsed();

        }

        public void update_dataGridView_socketUsed()
        {
            Task.Run(new Action(() =>
            {
                string sql = "";
                try
                {
                    sql = "select Id, Num from SocketUse";
                    string res = Sqlite.runSql(SQLiteRunningString, sql, out DataTable dt);
                    if (res != "Success") return;

                    this.Invoke(new Action(() =>
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            //Debug.WriteLine(dr.ItemArray[0].ToString());
                            dataGridView_socketUsed.Rows[int.Parse(dr.ItemArray[0].ToString().Substring(0, 2))].Cells[int.Parse(dr.ItemArray[0].ToString().Substring(2, 1))].Value = dr.ItemArray[1].ToString();

                        }
                    }));
                }
                catch (Exception ex)
                {
                    RunLog("刷新工单详情信息失败!" + sql);
                }
            }));
        }

        //工单详情刷新
        private void uiButton14_Click(object sender, EventArgs e)
        {
            dataGridView_flowcaedNoInfo.Columns.Clear();
            //初始化dataGridView_flowcaedNoInfo
            dataGridView_flowcaedNoInfo.EnableHeadersVisualStyles = false;
            dataGridView_flowcaedNoInfo.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dataGridView_flowcaedNoInfo.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            //dataGridView_flowcaedNoInfo.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dataGridView_flowcaedNoInfo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView_flowcaedNoInfo.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 10);

            dataGridView_flowcaedNoInfo.Columns.Add(0.ToString(), "序号");
            dataGridView_flowcaedNoInfo.Columns[0].Width = 75;
            dataGridView_flowcaedNoInfo.Columns.Add(1.ToString(), "生产信息");
            dataGridView_flowcaedNoInfo.Columns[1].Width = 200;
            dataGridView_flowcaedNoInfo.Columns.Add(2.ToString(), "内容");
            dataGridView_flowcaedNoInfo.Columns[2].Width = 758;
            for (int i = 0; i < dataGridView_flowcaedNoInfo.Columns.Count; i++)
            {
                dataGridView_flowcaedNoInfo.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView_flowcaedNoInfo.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            dataGridView_flowcaedNoInfo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView_flowcaedNoInfo.ColumnHeadersHeight = 34;
            for (int i = 0; i < 40; i++)
            {
                dataGridView_flowcaedNoInfo.Rows.Add();
                dataGridView_flowcaedNoInfo.Rows[i].Cells[0].Value = (i + 1).ToString("D2");
                dataGridView_flowcaedNoInfo.Rows[i].Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView_flowcaedNoInfo.Rows[i].Cells[0].ReadOnly = false;
                dataGridView_flowcaedNoInfo.Rows[i].Cells[0].Style.BackColor = Color.LightGray;
                dataGridView_flowcaedNoInfo.Rows[i].Cells[0].Style.SelectionBackColor = Color.LightGray;
                dataGridView_flowcaedNoInfo.Rows[i].Cells[0].Style.SelectionForeColor = Color.Black;
                dataGridView_flowcaedNoInfo.Rows[i].Height = 20;
            }
            string sql = "select * from flowcaedNoInfo where 产品类型=''";
            string res = Sqlite.runSql(SQLiteRunningString, sql, out DataTable dt);

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                dataGridView_flowcaedNoInfo.Rows[i].Cells[1].Value = dt.Columns[i].ColumnName;
                dataGridView_flowcaedNoInfo.Rows[i].Cells[2].Value = "";
            }

            update_dataGridView_flowcaedNoInfo();

        }

        public void update_dataGridView_flowcaedNoInfo()
        {
            string[] s;
            if (listBox1.SelectedItem == null) return;
            else
            {
                s = listBox1.SelectedItem.ToString().Split('_');
            }
            Task.Run(new Action(() =>
            {
                string sql = "";
                try
                {
                    sql = $"select * from flowcaedNoInfo where 开始时间='{s[0]}' and 工单号='{s[1]}'";
                    string res = Sqlite.runSql(SQLiteRunningString, sql, out DataTable dt);
                    if (res != "Success" || dt.Rows.Count < 1) return;

                    this.Invoke(new Action(() =>
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            dataGridView_flowcaedNoInfo.Rows[i].Cells[2].Value = dt.Rows[0].ItemArray[i].ToString();
                        }
                    }));
                }
                catch (Exception ex)
                {
                    RunLog("刷新工单详情信息失败!" + sql);
                }
            }));
        }

        //料表信息刷新
        private void uiButton17_Click(object sender, EventArgs e)
        {
            dataGridView_f.Columns.Clear();
            //初始化dataGridView_f
            dataGridView_f.EnableHeadersVisualStyles = false;
            dataGridView_f.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dataGridView_f.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            //dataGridView_f.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dataGridView_f.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView_f.DefaultCellStyle.Font = new Font("微软雅黑", 10);

            dataGridView_f.Columns.Add(0.ToString(), "批次开始日期");
            dataGridView_f.Columns[0].Width = 120;
            dataGridView_f.Columns.Add(1.ToString(), "获取MAP时间");
            dataGridView_f.Columns[1].Width = 170;
            dataGridView_f.Columns.Add(2.ToString(), "料号");
            dataGridView_f.Columns[2].Width = 170;
            dataGridView_f.Columns.Add(3.ToString(), "批号");
            dataGridView_f.Columns[3].Width = 90;
            dataGridView_f.Columns.Add(4.ToString(), "MAP信息");
            dataGridView_f.Columns[4].Width = 483;
            for (int i = 0; i < dataGridView_f.Columns.Count; i++)
            {
                dataGridView_f.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView_f.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            update_dataGridView_f();
        }
        public void update_dataGridView_f()
        {
            string[] s;
            if (listBox1.SelectedItem == null) return;
            else
            {
                s = listBox1.SelectedItem.ToString().Split('_');

            }

            string sql = $"select * from f_{s[1].Replace("-", "_") + "_" + DateTime.Parse(s[0]).ToString("yyyyMMdd_HH_mm_ss")}  order by 获取MAP时间";
            Task.Run(new Action(() =>
            {
                try
                {
                    string res = SqlServer.runSql(SQLServerConnString, sql, out DataTable dt);
                    if (res != "Success" || dt.Rows.Count < 1) return;
                    this.Invoke(new Action(() =>
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            dataGridView_f.Rows.Add(dr.ItemArray);
                        }
                    }));
                }
                catch (Exception ex)
                {
                    RunLog("刷新料表信息失败!" + sql);
                }
            }));
        }

        //清除全部选定socket使用次数记录
        private void uiButton12_Click(object sender, EventArgs e)
        {
            if (dataGridView_socketUsed.SelectedCells.Count < 1)
            {
                return;
            }
            for (int i = 0; i < dataGridView_socketUsed.SelectedCells.Count; i++)
            {
                if (dataGridView_socketUsed.SelectedCells[i].ColumnIndex == 0)
                {
                    dataGridView_socketUsed.SelectedCells[i].Selected = false;
                    i--;
                }
            }
            if (dataGridView_socketUsed.SelectedCells.Count < 1)
            {
                return;
            }
            DialogResult result = MessageBox.Show("共选择 " + dataGridView_socketUsed.SelectedCells.Count + " 个dut，请确认是否清零！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                int c = 0;
                for (int i = 0; i < dataGridView_socketUsed.SelectedCells.Count; i++)
                {
                    string sql = $"update SocketUse set Num = {0} where Id = '{dataGridView_socketUsed.SelectedCells[i].RowIndex.ToString("D2") + dataGridView_socketUsed.SelectedCells[i].ColumnIndex.ToString()}'";
                    try
                    {
                        int line = Sqlite.runSqlNonQuery(SQLiteRunningString, sql);
                        if (line > 0)
                        {
                            c++;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"清零失败！共计选择 {dataGridView_socketUsed.SelectedCells.Count} 个，成功 {c} 个，失败 {dataGridView_socketUsed.SelectedCells.Count - c} 个，详细信息：" + ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                }

                if (c == dataGridView_socketUsed.SelectedCells.Count)
                {
                    MessageBox.Show("清零成功！共清零 " + c + " 个dut！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"清零失败！共计选择 {dataGridView_socketUsed.SelectedCells.Count} 个，成功 {c} 个，失败 {dataGridView_socketUsed.SelectedCells.Count - c} 个。", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                update_dataGridView_socketUsed();

            }
        }

        //清除选定dut总良率记录
        private void uiButton8_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("共选择 " + dataGridView_dutAll.SelectedCells.Count + " 个dut，请确认是否清零！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                int c = 0;
                for (int i = 0; i < dataGridView_dutAll.SelectedCells.Count; i++)
                {
                    if (dataGridView_dutAll.SelectedCells[i].ColumnIndex == 0) continue;

                    string sql = $"update dutAll set pass=0,total=0,usedTime='{new TimeSpan().Ticks}' where Id = '{dataGridView_dutAll.SelectedCells[i].RowIndex.ToString("D2") + dataGridView_dutAll.SelectedCells[i].ColumnIndex.ToString()}'";
                    try
                    {
                        int line = Sqlite.runSqlNonQuery(SQLiteRunningString, sql);
                        if (line > 0)
                        {
                            c++;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"清零失败！共计选择 {dataGridView_dutAll.SelectedCells.Count} 个，成功 {c} 个，失败 {dataGridView_dutAll.SelectedCells.Count - c} 个，详细信息：" + ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (c == dataGridView_dutAll.SelectedCells.Count)
                {
                    MessageBox.Show("清零成功！共清零 " + c + " 个dut！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"清零失败！共计选择 {dataGridView_dutAll.SelectedCells.Count} 个，成功 {c} 个，失败 {dataGridView_dutAll.SelectedCells.Count - c} 个。", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                update_dataGridView_dutAll();

            }
        }

        private void tabControl7_Selecting(object sender, TabControlCancelEventArgs e)
        {
            switch (e.TabPage.Text)
            {
                //总良率: uiButton7
                case "dut总良率": uiButton7_Click(uiButton7, EventArgs.Empty); break;
                //当前工单dut实时良率: uiButton9
                case "当前工单dut实时良率": uiButton9_Click(uiButton9, EventArgs.Empty); break;
                //工单dut良率查询: uiButton10
                case "工单dut良率查询": uiButton10_Click(uiButton10, EventArgs.Empty); break;
                //socket使用次数: uiButton13
                case "socket使用次数": uiButton13_Click(uiButton13, EventArgs.Empty); break;
            }
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems == null) return;

            switch (tabControl6.SelectedTab.Text)
            {
                case "端口信息":
                    {
                        switch (tabControl7.SelectedTab.Text)
                        {
                            //dut总良率
                            case "dut总良率": uiButton7_Click(uiButton7, EventArgs.Empty); break;
                            //当前工单dut实时良率
                            case "当前工单dut实时良率": uiButton9_Click(uiButton9, EventArgs.Empty); break;
                            //工单dut良率查询
                            case "工单dut良率查询": uiButton10_Click(uiButton10, EventArgs.Empty); break;
                            //socket使用次数
                            case "socket使用次数": uiButton13_Click(uiButton13, EventArgs.Empty); break;
                        }
                    }
                    break;
                //工单详情
                case "工单详情": uiButton14_Click(uiButton14, EventArgs.Empty); break;
                //Bin项信息
                case "Bin项信息": uiButton34_Click(uiButton34, EventArgs.Empty); break;
                //料表信息
                case "料表信息": uiButton17_Click(uiButton17, EventArgs.Empty); break;
            }
        }

        private void dataGridView_socketUsed_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > 0 && e.RowIndex >= 0)
            {
                textBox7.Text = e.RowIndex.ToString("D2") + e.ColumnIndex.ToString();
                if (decimal.TryParse(dataGridView_socketUsed.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), out decimal res))
                {
                    numericUpDown1.Value = res;
                }
                else
                {
                    numericUpDown1.Value = 0;
                }
            }
        }


        /// <summary>
        /// 修改socket使用次数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton11_Click(object sender, EventArgs e)
        {
            if (dataGridView_socketUsed.SelectedCells.Count != 1)
            {
                MessageBox.Show("修改时请勿选择多个单元格！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (textBox7.Text.Length < 1)
            {
                return;
            }

            DialogResult result = MessageBox.Show($"请确认是否将dut{textBox7.Text}的使用次数从 {dataGridView_socketUsed.SelectedCells[0].Value} 修改为 {numericUpDown1.Value} .", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                string sql = $"update SocketUse set Num = {numericUpDown1.Value} where Id = '{textBox7.Text}'";
                try
                {
                    int line = Sqlite.runSqlNonQuery(SQLiteRunningString, sql);
                    if (line > 0)
                    {
                        MessageBox.Show("修改成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        update_dataGridView_socketUsed();
                    }
                    else MessageBox.Show("修改失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex) { MessageBox.Show("修改失败！详细信息：" + ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }


        /// <summary>
        /// 将DataGridView导出到excel
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="filePath">路径与文件名</param>
        /// <param name="header">是否输出表头，默认false</param>
        public void ExportDataGridViewToExcel(DataGridView dataGridView, string filePath, bool header = false)
        {
            // 创建一个XLWorkbook对象
            using (var workbook = new XLWorkbook())
            {
                // 添加一个工作表
                var worksheet = workbook.Worksheets.Add("Sheet1");

                int h = 1;
                if (header)
                {
                    for (int i = 0; i < dataGridView.Columns.Count; i++)
                    {
                        // 导出列头
                        worksheet.Cell(1, i + 1).Value = dataGridView.Columns[i].HeaderText;
                    }
                    h = 2;
                }

                // 导出数据
                for (int i = 0; i < dataGridView.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView.Columns.Count; j++)
                    {
                        worksheet.Cell(i + h, j + 1).Value = dataGridView.Rows[i].Cells[j].Value?.ToString();
                    }
                }

                for (int i = 0; i < dataGridView.Columns.Count; i++)
                {
                    // 设置列宽以适应内容（可选）
                    worksheet.Column(i + 1).AdjustToContents();
                }

                // 保存工作簿到文件
                workbook.SaveAs(filePath);

            }
        }

        public void ExportDataGridViewToExcel(DataTable dataTable, string filePath, bool header = true)
        {
            // 创建一个XLWorkbook对象
            using (var workbook = new XLWorkbook())
            {
                // 添加一个工作表
                var worksheet = workbook.Worksheets.Add("Sheet1");

                int h = 1;
                if (header)
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        // 导出列头
                        worksheet.Cell(1, i + 1).Value = dataTable.Columns[i].ColumnName;
                    }
                    h = 2;
                }

                // 导出数据
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        if (j == 0 || j == 1)
                        {
                            if (DateTime.TryParse(dataTable.Rows[i][j].ToString(), out DateTime wtime))
                            {
                                worksheet.Cell(i + h, j + 1).Value = wtime;
                            }
                            else worksheet.Cell(i + h, j + 1).Value = dataTable.Rows[i][j].ToString();
                        }
                        else
                        {
                            if (long.TryParse(dataTable.Rows[i][j].ToString(), out long result))
                            {
                                worksheet.Cell(i + h, j + 1).Value = result;
                            }
                            else worksheet.Cell(i + h, j + 1).Value = dataTable.Rows[i][j].ToString();
                        }

                    }
                }

                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    // 设置列宽以适应内容（可选）
                    worksheet.Column(i + 1).AdjustToContents();
                }

                // 保存工作簿到文件
                workbook.SaveAs(filePath);

            }
        }

        private void uiButton16_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFolderPath = folderBrowserDialog.SelectedPath;
                ExportDataGridViewToExcel(dataGridView_f, folderBrowserDialog.SelectedPath + "\\料表信息-" + listBox1.Text.Replace(":", ".") + ".xlsx", true);
            }
        }



        /// <summary>
        /// 初始化DataGridView
        /// </summary>
        /// <param name="dGV">DataGridView</param>
        /// <param name="x">列</param>
        /// <param name="y">行</param>
        void dGV_init(DataGridView dGV, int x, int y)
        {
            try
            {
                dGV.RowsDefaultCellStyle.WrapMode = DataGridViewTriState.False; // 禁止换行
                dGV.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None; // 关闭自动调整行高
                dGV.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 10);

                dGV.RowTemplate.Height = dGV.Size.Height / y; // 设置所有新行的默认高度

                dGV.Columns.Clear();

                for (int i = 0; i < x; i++)
                {
                    dGV.Columns.Add("Column" + i.ToString(), "Column" + i.ToString());
                    dGV.Columns[i].Width = dGV.Width / x; // 设置所有列的宽度
                }

                dGV.Rows.Add(y);
            }
            catch { }
        }

        DataTable dataGridView_out_dt;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dgv">DataGridView</param>
        /// <param name="name">auto2,fix1,fix2,fix3</param>
        /// <param name="s">表名</param>
        /// <param name="num">第N盘</param>
        public void update_dataGridView_out(DataGridView dgv, string name, string s, int num, int d)
        {
            string sql = $"select * from o_{s.Replace("-", "_")} where 名称='{name}' order by 时间 offset {num - 1} rows fetch next 1 rows only";
            Task.Run(new Action(() =>
            {
                try
                {
                    string res = SqlServer.runSql(SQLServerConnString, sql, out dataGridView_out_dt);
                    if (res != "Success") return;
                    //{
                    //    dataGridView1.DataSource = null;
                    //    //dataGridView1.Columns.Clear();

                    //} 

                    if (dataGridView_out_dt.Rows.Count == 0)
                    {
                        dataGridView1.Columns.Clear();
                        dataGridView1.DataSource = null;
                        return;
                    }

                    int x = int.Parse(dataGridView_out_dt.Rows[0].ItemArray[2].ToString().Split('x')[0]);
                    int y = int.Parse(dataGridView_out_dt.Rows[0].ItemArray[2].ToString().Split('x')[1]);

                    string[] value = dataGridView_out_dt.Rows[0].ItemArray[3].ToString().Split(',');

                    this.Invoke(new Action(() =>
                    {
                        dGV_init(dgv, x, y);

                        textBox8.Text = "1";
                        textBox6.Text = "1";
                        textBox1.Text = value[0];
                        textBox3.Text = "";
                        textBox4.Text = "";

                        if (dataGridView_out_dt.Columns.Count > 4)
                        {
                            value = dataGridView_out_dt.Rows[0].ItemArray[3 + d].ToString().Split(',');

                            textBox3.Text = dataGridView_out_dt.Rows[0].ItemArray[4].ToString().Split(',')[0];

                            textBox4.Text = dataGridView_out_dt.Rows[0].ItemArray[5].ToString().Split(',')[0];
                        }
                    }));
                    int z = 0;
                    for (int i = 0; i < x; i++)
                    {
                        for (int j = 0; j < y; j++)
                        {
                            dgv.Rows[j].Cells[i].Value = value[z];
                            if (++z == value.Length) return;
                        }
                    }

                }
                catch (Exception ex)
                {
                    RunLog("刷新Tray盘信息失败!" + sql);
                }
            }));

        }


        private void tabControl6_Selecting(object sender, TabControlCancelEventArgs e)
        {
            switch (e.TabPage.Text)
            {
                case "端口信息":
                    {
                        switch (tabControl7.SelectedTab.Text)
                        {
                            //dut总良率
                            case "dut总良率": uiButton7_Click(uiButton7, EventArgs.Empty); break;
                            //当前工单dut实时良率
                            case "当前工单dut实时良率": uiButton9_Click(uiButton9, EventArgs.Empty); break;
                            //工单dut良率查询
                            case "工单dut良率查询": uiButton10_Click(uiButton10, EventArgs.Empty); break;
                            //socket使用次数
                            case "socket使用次数": uiButton13_Click(uiButton13, EventArgs.Empty); break;
                        }
                    }
                    break;
                //工单详情
                case "工单详情": uiButton14_Click(uiButton14, EventArgs.Empty); break;
                //Bin项信息
                case "Bin项信息": uiButton34_Click(uiButton34, EventArgs.Empty); break;
                //料表信息
                case "料表信息": uiButton17_Click(uiButton17, EventArgs.Empty); break;
            }
        }

        private void uiButton19_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                // 配置对话框属性（可选）
                folderDialog.Description = "请选择一个文件夹"; // 对话框标题
                folderDialog.ShowNewFolderButton = true; // 显示“新建文件夹”按钮
                folderDialog.SelectedPath = Directory.GetCurrentDirectory() + "\\Log";

                // 显示对话框并获取结果
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath; // 选中的文件夹完整路径
                    textBox2.Text = folderPath;
                }
            }
        }

        private void uiButton20_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox2.Text.Length == 0)
                {
                    MessageBox.Show("请选择保存路径!");
                    return;
                }

                if (!new DirectoryInfo(textBox2.Text).Exists)
                {
                    MessageBox.Show("路径不存在!", "提示");
                    return;
                }

                System.Diagnostics.Process.Start("explorer.exe", textBox2.Text);
            }
            catch (Exception ex)
            {
                string errortext = "打开文件夹失败!详细信息:" + ex.Message;
                MessageBox.Show(errortext, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RunLog(errortext);
            }
        }

        private void uiButton21_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length == 0)
            {
                MessageBox.Show("请选择保存路径!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!new DirectoryInfo(textBox2.Text).Exists)
            {
                MessageBox.Show("路径不存在!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DateTime end = dateTimePicker1.Value;
            DateTime start = dateTimePicker2.Value;

            int startYear = start.Year;
            int startMonth = start.Month;
            int startDay = start.Day;

            int endYear = end.Year;
            int endMonth = end.Month;
            int endDay = end.Day;
            int endh = end.Hour;

            try
            {
                string startime = uiCheckBox210.Checked ? $"and 开始时间 <= '{dateTimePicker1.Value:yyyy年MM月dd日 HH:mm:ss}'" : "";
                string endtime = uiCheckBox1.Checked ? $"and 开始时间 >= '{dateTimePicker2.Value:yyyy年MM月dd日 HH:mm:ss}'" : ""; //结束时间
                string sql = $"select * from FlowcaedNoInfo where 工单号 like '%{textBox5.Text}%' {startime} {endtime} order by 开始时间 desc";

                string res = Sqlite.runSql(SQLiteRunningString, sql, out DataTable dt);
                if (res != "Success" || dt.Rows.Count < 1)
                {
                    MessageBox.Show("无数据信息!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //string sql = $"select * from BatchInfo where 结束时间 > '{start.ToString("yyyy-MM-dd HH:mm:ss")}' and 结束时间 < '{end.ToString("yyyy-MM-dd HH:mm:ss")}'";
                //string result = SQLiteHelper.runSql(SQLiteConnString, sql, out DataTable datatable);

                //if (datatable.Rows.Count == 0)
                //{
                //    MessageBox.Show("无数据信息!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    return;
                ///}

                DateTime time = DateTime.Now;
                string fileName = "";
                string lotno = textBox5.Text.Trim().Length > 0 ? $"(工单号_{textBox5.Text.Trim()})" : "";
                if (uiCheckBox1.Checked && uiCheckBox210.Checked)
                {
                    fileName = textBox2.Text + "\\" + $"生产信息{startYear}.{startMonth:00}.{startDay:00}至{endYear}.{endMonth:00}.{endDay:00}{lotno}-{time.Hour:00}{time.Minute:00}{time.Second:00}" + ".xlsx";
                }
                else if (!uiCheckBox1.Checked && uiCheckBox210.Checked)
                {
                    fileName = textBox2.Text + "\\" + $"生产信息{endYear}.{endMonth:00}.{endDay:00}以前{lotno}-{time.Hour:00}{time.Minute:00}{time.Second:00}" + ".xlsx";
                }
                else if (uiCheckBox1.Checked && !uiCheckBox210.Checked)
                {
                    fileName = textBox2.Text + "\\" + $"生产信息{startYear}.{startMonth:00}.{startDay:00}至今{lotno}-{time.Hour:00}{time.Minute:00}{time.Second:00}" + ".xlsx";
                }
                else
                {
                    fileName = textBox2.Text + "\\" + $"所有生产信息{lotno}-{time.Hour:00}{time.Minute:00}{time.Second:00}" + ".xlsx";
                }

                uiButton21.Enabled = false;
                uiButton21.Text = "正在导出...";
                Task.Run(() =>
                {
                    ExportDataGridViewToExcel(dt, fileName, true);
                    this.Invoke(new Action(() =>
                    {
                        uiButton21.Enabled = true;
                        uiButton21.Text = "导出Excel";
                    }));
                });

                //FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                //if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                //{
                //    DateTime time = DateTime.Now;
                //    string fileName = $"生产信息{time.Year}.{time.Month:00}.{time.Day:00}_{time.Hour:00}h{time.Minute:00}m{time.Second:00}s";
                //    string selectedFolderPath = folderBrowserDialog.SelectedPath;
                //    ExportDataGridViewToExcel(dataTable, folderBrowserDialog.SelectedPath + "\\" + $"{fileName}.xlsx");
                //}
            }
            catch (Exception ex)
            {
                uiButton21.Enabled = true;
                uiButton21.Text = "导出Excel";
                MessageBox.Show("导出异常!详细信息:" + ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            发送批次信息();
        }

        private void chk_senddout16_CheckedChanged(object sender, EventArgs e)
        {
            richtext_senddata.Text = GetSendisopen();
        }

        private void uiCheckBox11_CheckedChanged(object sender, EventArgs e)
        {
            richtext_senddata.Text = GetSendisopen();
        }

        private void uiCheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            richtext_senddata.Text = GetSendisopen();
        }

        private void uiButton34_Click(object sender, EventArgs e)
        {
            string[] s;

            if (listBox1.SelectedItem == null) return;
            else
            {
                s = listBox1.SelectedItem.ToString().Split('_');
            }

            if (comboBox_盘位.SelectedIndex == -1)
            {
                if (comboBox_盘位.Items.Count > 0)
                {
                    comboBox_盘位.SelectedIndex = 0;
                }
            }
            if (comboBox_盘数.SelectedIndex == -1)
            {
                if (comboBox_盘数.Items.Count > 0)
                {
                    comboBox_盘数.SelectedIndex = 0;
                }
            }
            if (comboBox_数据显示.SelectedIndex == -1)
            {
                if (comboBox_数据显示.Items.Count > 0)
                {
                    this.comboBox_数据显示.SelectedIndexChanged -= new System.EventHandler(this.comboBox_数据显示_SelectedIndexChanged);
                    comboBox_数据显示.SelectedIndex = 0;
                    this.comboBox_数据显示.SelectedIndexChanged += new System.EventHandler(this.comboBox_数据显示_SelectedIndexChanged);
                }
            }

            try
            {
                if (dataGridView_out_dt != null)
                {
                    dataGridView_out_dt.Columns.Clear();
                    dataGridView_out_dt.Clear();
                }
                if (dataGridView1 != null && dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.Columns.Clear();
                    dataGridView1.DataSource = null;
                }
                update_dataGridView_out(dataGridView1, comboBox_盘位.Text.ToLower(), s[1] + "_" + DateTime.Parse(s[0]).ToString("yyyyMMdd_HH_mm_ss"), int.Parse(comboBox_盘数.Text), comboBox_数据显示.SelectedIndex);
            }
            catch (Exception ex)
            {
                RunLog(ex.Message);
            }

        }

        private void comboBox_盘数_SelectedIndexChanged(object sender, EventArgs e)
        {
            uiButton34_Click(uiButton34, EventArgs.Empty);
        }

        private void comboBox_盘位_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] s;


            if (listBox1.SelectedItem == null) return;
            else
            {
                s = listBox1.SelectedItem.ToString().Split('_');
            }

            comboBox_盘数.Items.Clear();
            string trayname = comboBox_盘位.Text.ToLower();
            Task.Run(new Action(() =>
            {
                string sql = "";
                try
                {
                    sql = $"select count(*) from o_{s[1].Replace("-", "_") + "_" + DateTime.Parse(s[0]).ToString("yyyyMMdd_HH_mm_ss")} where 名称='{trayname}'";
                    string res = SqlServer.runSql(SQLServerConnString, sql, out int num0);
                    if (res == "Success" && num0 > 0)
                    {
                        this.Invoke(new Action(() =>
                        {
                            for (int i = 1; i <= num0; i++)
                            {
                                comboBox_盘数.Items.Add(i.ToString());
                            }
                            comboBox_盘数.SelectedIndex = 0;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    RunLog("刷新Bin项信息失败!" + sql);
                }
            }));
        }

        private void comboBox_数据显示_SelectedIndexChanged(object sender, EventArgs e)
        {
            uiButton34_Click(uiButton34, EventArgs.Empty);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int z = 0;
                if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
                {
                    z = e.ColumnIndex * dataGridView_out_dt.Rows.Count + e.RowIndex;
                }
                else
                {
                    return;
                }

                textBox8.Text = (e.RowIndex + 1).ToString();
                textBox6.Text = (e.ColumnIndex + 1).ToString();

                string[] bin项 = dataGridView_out_dt.Rows[0].ItemArray[3].ToString().Split(',');
                textBox1.Text = bin项[z];

                if (dataGridView_out_dt.Columns.Count < 5)
                {
                    return;
                }

                string[] 测试时间 = dataGridView_out_dt.Rows[0].ItemArray[4].ToString().Split(',');
                textBox3.Text = 测试时间[z];
                string[] 测试工位 = dataGridView_out_dt.Rows[0].ItemArray[5].ToString().Split(',');
                textBox4.Text = 测试工位[z];
            }
            catch { }
        }
    }

}

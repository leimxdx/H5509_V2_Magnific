using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextFileLogger
{
    public enum LogType
    {
        [Description("运行日志")]
        RunLog = 1,

        [Description("通信日志")]
        ComLog = 2,

        [Description("报警日志")]
        AlarmLog = 3,

        [Description("按键日志")]
        KeyLogPath = 4,

        [Description("操作日志")]
        OperateLog = 5,

        [Description("参数日志")]
        ParamLog = 6
    }

    public class WriteData
    {
        private static string ProgramPath = Directory.GetCurrentDirectory();

        public WriteDataHelper WriteDataHelper = new WriteDataHelper(ProgramPath + "\\Log");//

        //创建一个静态对象保存当前的实例
        private static WriteData instance;

        //定义一个锁
        private static readonly object objlock = new object();

        //创建一个私有的构造方法
        private WriteData() { }

        public static WriteData GetWriteDataInstance()
        {
            if (instance == null)
            {
                lock (objlock)
                {
                    if (instance == null)
                    {
                        instance = new WriteData();
                    }
                }
            }
            return instance;
        }

        /// <summary>
        /// 输出log(1: RunLog 2: ComLog 3: AlarmLog 4: KeyLog 5: OperateLog 6: ParamLog)
        /// </summary>
        /// <param name="log"></param>
        /// <param name="logtype">1: RunLog 2: ComLog 3: AlarmLog 4: KeyLog 5: OperateLog 6: ParamLog</param>
        /// <returns></returns>
        public bool RunLog(string log, DateTime datetime, LogType logtype = LogType.RunLog, bool isReceive = false)
        {
            return WriteDataHelper.RunLog(log, datetime, logtype, isReceive);
        }

        /// <summary>
        /// 输出log(1: RunLog 2: ComLog 3: AlarmLog 4: KeyLog 5: OperateLog 6: ParamLog)
        /// </summary>
        /// <param name="log"></param>
        /// <param name="logtype">1: RunLog 2: ComLog 3: AlarmLog 4: KeyLog 5: OperateLog 6: ParamLog</param>
        /// <returns></returns>
        public bool RunLog(string[] log, DateTime[] datetime, LogType logtype = LogType.RunLog, bool isReceive = false)
        {
            return WriteDataHelper.RunLog(log, datetime, logtype, isReceive);
        }
    }

    public class WriteDataHelper
    {
        public WriteDataHelper() { }

        private SimpleHybirdLock writeDataLock = new SimpleHybirdLock();

        public string ComLogPath { get; set; }

        public string RunLogPath { get; set; }

        public string AlarmLogPath { get; set; }

        public string KeyLogPath { get; set; }

        public string OperateLogPath { get; set; }

        public string ParamLogPath { get; set; }

        // 日志文件格式配置（可从配置读取）
        private const string DateFolderFormat = "yyyy";

        private const string MonthFolderFormat = "MM";

        private const string FileNameFormat = "yyyyMMdd.log";

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        private const string DirectionSymbol = " => ";

        private const string ReceiveDirectionSymbol = " <= ";

        public WriteDataHelper(string dataDirectory)
        {
            int year = DateTime.Now.Year;
            RunLogPath = $"{dataDirectory}\\{year}\\RunLog";
            ComLogPath = $"{dataDirectory}\\{year}\\ComLog";
            AlarmLogPath = $"{dataDirectory}\\{year}\\AlarmLog";
            KeyLogPath = $"{dataDirectory}\\{year}\\KeyLog";
            OperateLogPath = $"{dataDirectory}\\{year}\\OperateLog";
            ParamLogPath = $"{dataDirectory}\\{year}\\ParamLog";
        }

        private string DatapPath(string fileName, LogType logtype)//, short logtype = 1
        {
            string path = "";
            string month = "\\" + DateTime.Now.Month.ToString("D2");
            switch ((int)logtype)
            {
                case 1: path = RunLogPath + month; break;
                case 2: path = ComLogPath + month; break;
                case 3: path = AlarmLogPath + month; break;
                case 4: path = KeyLogPath + month; break;
                case 5: path = OperateLogPath + month; break;
                case 6: path = ParamLogPath + month; break;
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Path.Combine(path, fileName);
            //return Path.Combine(path, string.Format("{0}.log", DateTime.Now.ToString("yyyy-MM-dd")));
        }

        /// <summary>
        /// 写运行日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool RunLog(string log, DateTime datetime, LogType logtype = LogType.RunLog, bool isReceive = false)
        {
            string str = " => ";
            if (isReceive) str = " <= ";
            string writeTime = $"{datetime:yyyy-MM-dd HH:mm:ss} {str} ";
            string logPath = DatapPath(datetime.ToString("yyyy-MM-dd") + ".log", logtype);
            if (WriteAppend(logPath, writeTime + log)) return true;
            else return false;
        }

        /// <summary>
        /// 写运行日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool RunLog(string[] log, DateTime[] datetime, LogType logtype = LogType.RunLog, bool isReceive = false)
        {
            string str = " => ";
            if (isReceive) str = " <= ";
            string[] content = new string[log.Length];
            string logPath = DatapPath(datetime[0].ToString("yyyy-MM-dd") + ".log", logtype);
            for (int i = 0; i < content.Length; i++)
            {
                string writeTime = $"{datetime[i]:yyyy-MM-dd HH:mm:ss} {str} ";
                content[i] = writeTime + log[i];
            }
            if (WriteAppend(logPath, content)) return true;
            else return false;
        }

        private bool WriteAppend(string filePath, string content)
        {
            if (WriteAppend(filePath, new string[] { content })) return true;
            else return false;
        }

        private bool WriteAppend(string filePath, string[] contents)
        {
            //Stopwatch stop = new Stopwatch();
            //stop.Start();
            writeDataLock.Enter();
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                {
                    fs.Seek(fs.Length, SeekOrigin.Current);
                    string content = String.Join(Environment.NewLine, contents) + Environment.NewLine;
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(content);
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                //stop.Stop();
                //Debug.WriteLine(stop.ElapsedMilliseconds);
                writeDataLock.Leave();
            }
        }
    }
}

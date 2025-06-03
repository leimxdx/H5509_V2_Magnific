using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiCommunication.Core;

namespace H5509_V2
{
    public class WriteData
    {

        private static string H5509Program = Directory.GetCurrentDirectory();

        public WriteDataHelper WriteDataHelper = new WriteDataHelper(H5509Program + "\\Log");//

        //创建一个静态对象保存当前的实例
        private static WriteData instance;


        //定义一个锁
        private static readonly object objlock = new object();

        //创建一个私有的构造方法
        private WriteData()
        {

        }

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
        public bool RunLog(string log, short logtype = 1, bool isReceive = false)
        {
            return WriteDataHelper.RunLog(log, logtype, isReceive);
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

        private string DatapPath(string fileName, short logtype = 1)
        {
            string path = "";
            string month = "\\" + DateTime.Now.Month.ToString("D2");
            switch (logtype)
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
        private bool WriteAppend(string filePath, string[] contents)
        {
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
                writeDataLock.Leave();
            }
        }

        private bool WriteAppend(string filePath, string content)
        {
            if (WriteAppend(filePath, new string[] { content })) return true;
            else return false;
        }
        /// <summary>
        /// 写运行日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool RunLog(string log, short logtype = 1, bool isReceive = false)
        {
            string str = " => ";
            if (isReceive) str = " <= ";
            DateTime dataTime = DateTime.Now;
            string writeTime = $"{dataTime:yyyy-MM-dd HH:mm:ss} {str} ";
            string logPath = DatapPath(dataTime.ToString("yyyy-MM-dd") + ".log", logtype);
            if (WriteAppend(logPath, writeTime + log)) return true;
            else return false;
        }
    }
}

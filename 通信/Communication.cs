using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static H5509_V2.APPmaster;
namespace H5509_V2
{
    public class Communication
    {
        public bool IsOpenCom { get; set; } = false;
        public bool IsconnectTester { get; set; } = false;
        public List<System.Timers.Timer> Listtimer = new List<System.Timers.Timer>();
        public List<System.Timers.Timer> timeOutTimer = new List<System.Timers.Timer>();

        public List<TimeSpan> listindextimespan = new List<TimeSpan>();
        public List<TimeSpan> listtesttimespan = new List<TimeSpan>();
        public List<DateTime> listtestdatetime = new List<DateTime>();
        private SerialPort serialPort;
        public bool[] Iswaitout = new bool[16];
        public bool[] Isreceive = new bool[16];
        public Dictionary<int, string[]> Bin = new Dictionary<int, string[]>();
        public ConcurrentQueue<string[]> OutBin = new ConcurrentQueue<string[]>();
        public List<int> Listtestsingal = new List<int>();
        //private WriteData writeDataHelper = WriteData.GetWriteDataInstance();
        public Action<string> RefrshLog;

        //public List<TestAriCylinder> listsitetest = new List<TestAriCylinder>();
        //创建GXN轴卡对象
        //private GxnMotionEx motionEx = GxnMotionEx.GetInstance();

        /// <summary>
        /// 打开串口
        /// </summary>
        public void OpenCom()
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"串口{serialPort.PortName}打开失败！详细信息：" + ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //writeDataHelper.RunLog($"串口{serialPort.PortName}打开失败！详细信息：" + ex.Message, 2);
            }

            if (serialPort.IsOpen) IsOpenCom = true;
            else IsOpenCom = false;
        }

        //ListSite.Add(i);
        //ListInput.Add(i);
        //ListOutput.Add(i);
        //ListTest.Add(i);
        public Communication()
        {
            if (Listtimer.Count == 0)
            {
                for (int i = 0; i < 16; i++)
                {
                    System.Timers.Timer timer = new System.Timers.Timer() { Interval = 600 * 1000 }; //默认测试超时时间修改为10分钟
                    Iswaitout[i] = false;
                    Listtimer.Add(timer);
                    listtestdatetime.Add(DateTime.Now);
                    listtesttimespan.Add(new TimeSpan(0));
                    timer.Elapsed += ReceiveTimer;
                }
            }
            if (timeOutTimer.Count == 0)
            {
                for (int i = 0; i < 16; i++)
                {
                    System.Timers.Timer timer = new System.Timers.Timer() { Interval = 5 * 1000 }; //默认timeout超时时间修改为5秒
                    timeOutTimer.Add(timer);
                    timer.Elapsed += timeOut;
                    timer.Stop();
                }
            }


            if (serialPort == null)
            {
                serialPort = new SerialPort();
                serialPort.DataReceived += DataReceivedHandler;
            }
            serialPort.PortName = "COM11";
            serialPort.BaudRate = 9600; //波特率
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;

            OpenCom();
        }


        /// <summary>
        /// 显示通信log
        /// </summary>
        /// <param name="message"></param>
        private void RefreshInfoLog(string message, bool isReceive)
        {
            string str = " => ";
            if (isReceive)
            {
                str = " <= ";
            }
            string datestr = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}{str}";
            Debug.WriteLine(datestr + message);
            //RefrshLog(datestr + message);
        }


        string old = "";
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //Thread.Sleep(1);
                SerialPort sp = (SerialPort)sender;
                string outdata = old + sp.ReadExisting();
                if (outdata[outdata.Length - 1] != '+')
                {
                    old = outdata;
                    return;
                }
                else
                {
                    old = "";
                }
                RefreshInfoLog(outdata, true);
                if (outdata.Length < 1) return;

                Debug.WriteLine(outdata);
                //writeDataHelper.RunLog(outdata, 2, true);

                while (outdata.Length > 0)
                {
                    //@START_INFO设置成功后应返回  "@INFO_OK"
                    if (outdata.StartsWith("@INFO_OK"))
                    {
                        outdata = outdata.Remove(0, outdata.IndexOf('+') + 1);
                        IsconnectTester = true;
                    }

                    if (outdata.StartsWith("@TEST_DONE"))
                    {
                        try
                        {
                            string[] bin = outdata.Substring(14, 23).Split(' ');
                            int site = Convert.ToInt32(outdata.Substring(11, 2));

                            if (SiteTestState[site] == 2) //if (listsitetest[site].TestStatus == true && Iswaitout[site] == false && Isreceive[site] == false)
                            {
                                Listtimer[site].Stop();
                                timeOutTimer[site].Stop();
                                if (Bin.ContainsKey(Convert.ToInt32(site)))
                                {
                                    Bin[site] = bin;
                                }
                                else Bin.Add(site, bin);
                                listtesttimespan[site] = DateTime.Now - listtestdatetime[site];
                                SiteTestState[site] = 3;
                                OutSiteQueue.Enqueue(site);
                                BinSaveQueue.Enqueue(bin);
                                //writeDataHelper.RunLog($"获得Site:{site:D2},Bin:{bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}");
                            }
                            //else writeDataHelper.RunLog($"site:{site:D2}未在测试!", 3);
                            outdata = outdata.Remove(0, outdata.IndexOf('+') + 1);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("数据解析失败！详细信息：" + ex.Message);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //writeDataHelper.RunLog("接收串口数据解析失败！详细信息：" + ex.Message, 2);
            }
        }

        private string outdata;

        public void Send(string str)
        {
            if (serialPort == null || !serialPort.IsOpen)
            {
                return;
            }
            serialPort.Write(str);
            //writeDataHelper.RunLog(str, 2);
            RefreshInfoLog(str, false);
        }

        public string getstartest(short site, List<bool[]> isopen)
        {
            string str = $"{site:00} ";

            for (int i = 0; i < isopen.Count; i++)
            {
                for (int j = 0; j < isopen[i].Length; j++)
                {
                    if (isopen[i][j]) str += "1";
                    else str += "0";
                }
            }
            return str;
        }

        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="site"></param>
        /// <param name="isopen"></param>
        public string Send(short site, List<bool[]> isopen)
        {
            try
            {
                string str = getstartest(site, isopen);

                if (serialPort == null || !serialPort.IsOpen)
                {
                    return "串口未打开";
                }
                serialPort.Write("@START_TEST " + str + "+");
                //writeDataHelper.RunLog("@START_TEST " + str + "+", 2);
                RefreshInfoLog("@START_TEST " + str + "+", false);
                listtestdatetime[site] = DateTime.Now;
                Listtimer[site].Start();
                Iswaitout[site] = false;
                //启动定时器
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        private void timeOut(object sender, EventArgs e)
        {
            (sender as System.Timers.Timer).Stop();
            int t = 0;
            System.Timers.Timer timer = sender as System.Timers.Timer;
            for (int i = 0; i < 16; i++)
            {
                if (timer.Equals(timeOutTimer[i]))
                {
                    t = i;
                    break;
                }
            }
            SiteTestState[t] = 3;
            Iswaitout[t] = true;
            string[] bin = { "20A", "20A", "20A", "20A", "20A", "20A", "20A", "20A" };
            if (Bin.ContainsKey(t))
            {
                Bin[t] = bin;
            }
            else Bin.Add(t, bin);
            OutSiteQueue.Enqueue(t);
            BinSaveQueue.Enqueue(bin);
            //writeDataHelper.RunLog($"{DateTime.Now.ToString("HH:mm:dd")}:Site{t + 1}接收超时", 2);

        }

        private void ReceiveTimer(object sender, EventArgs e)
        {
            (sender as System.Timers.Timer).Stop();
            int t = 0;
            System.Timers.Timer timer = sender as System.Timers.Timer;
            for (int i = 0; i < 16; i++)
            {
                if (timer.Equals(Listtimer[i]))
                {
                    t = i;
                    break;
                }
            }

            //if (motionEx.testerIFParam.超时发送timeout)
            {
                timeOutTimer[t].Start();
                //发送timeout
                Send($"@TIMEOUT {t.ToString("D2")}+");
                return;
            }


            SiteTestState[t] = 3;
            Iswaitout[t] = true;
            string[] bin = { "20A", "20A", "20A", "20A", "20A", "20A", "20A", "20A" };
            if (Bin.ContainsKey(t))
            {
                Bin[t] = bin;
            }
            else Bin.Add(t, bin);
            OutSiteQueue.Enqueue(t);
            BinSaveQueue.Enqueue(bin);
            //writeDataHelper.RunLog($"{DateTime.Now.ToString("HH:mm:dd")}:Site{t + 1}接收超时", 2);
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static H5509_V2.APPmaster;
using static H5509_V2.WriteData;
namespace H5509_V2
{
    public class Communication
    {
        public bool IsOpenCom { get; set; } = false;
        public bool IsconnectTester { get; set; } = false;
        public List<System.Timers.Timer> Listtimer = new List<System.Timers.Timer>();
        public List<System.Timers.Timer> timeOutTimer = new List<System.Timers.Timer>();
        public List<System.Timers.Timer> ListNoTestTimer = new List<System.Timers.Timer>();

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

        public void CloseCom()
        {
            IsOpenCom = false;
            serialPort.Close();
            Thread.Sleep(50);
        }

        public void 刷新串口信息()
        {
            CloseCom();

            serialPort.PortName = 设置信息.测试机信息.串口号;
            serialPort.BaudRate = 设置信息.测试机信息.波特率;
            serialPort.Parity = 设置信息.测试机信息.校验位;
            serialPort.DataBits = 设置信息.测试机信息.数据位;
            serialPort.StopBits = 设置信息.测试机信息.停止位;

            OpenCom();
        }

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

        public void 刷新测试超时时间()
        {
            double testTime = 设置信息.测试机信息.测试超时时间 * 1000; //将秒转换为毫秒
            double testTime2 = 设置信息.测试机信息.timeout超时时间 * 1000; //将秒转换为毫秒
            double testTime3 = 设置信息.测试机信息.离线模式测试等待时间 * 1000; //将秒转换为毫秒
            for (int i = 0; i < Listtimer.Count; i++)
            {
                if (Listtimer[i].Interval != testTime)
                {
                    Listtimer[i].Interval = testTime;
                }
                if (timeOutTimer[i].Interval != testTime2)
                {
                    timeOutTimer[i].Interval = testTime2;
                }
                if (ListNoTestTimer[i].Interval != testTime3)
                {
                    ListNoTestTimer[i].Interval = testTime3;
                }
            }
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
                    System.Timers.Timer timer = new System.Timers.Timer() { Interval = 设置信息.测试机信息.测试超时时间 * 1000 }; //默认测试超时时间修改为10分钟
                    Iswaitout[i] = false;
                    Listtimer.Add(timer);
                    listtestdatetime.Add(DateTime.Now);
                    listtesttimespan.Add(new TimeSpan(0));
                    timer.Elapsed += ReceiveTimer;

                    System.Timers.Timer timer2 = new System.Timers.Timer() { Interval = 设置信息.测试机信息.timeout超时时间 * 1000 }; //默认timeout超时时间修改为5秒
                    timeOutTimer.Add(timer2);
                    timer2.Elapsed += timeOut;
                    timer2.Stop();

                    System.Timers.Timer timer3 = new System.Timers.Timer() { Interval = 设置信息.测试机信息.离线模式测试等待时间 * 1000 };
                    ListNoTestTimer.Add(timer3);
                    timer3.Elapsed += ReceiveNoTestTimer;
                }
            }

            if (serialPort == null)
            {
                serialPort = new SerialPort();
                serialPort.DataReceived += DataReceivedHandler;
            }

            刷新串口信息();
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
            RefrshLog(datestr + message);
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
                else old = "";

                RefreshInfoLog(outdata, true);
                if (outdata.Length < 1) return;

                RunLog(outdata, WriteData.LogType.ComLog, true);

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

                            if (SiteTestState[site] == 2 && (Listtimer[site].Enabled || timeOutTimer[site].Enabled)) //if (listsitetest[site].TestStatus == true && Iswaitout[site] == false && Isreceive[site] == false)
                            {
                                Listtimer[site].Stop();
                                timeOutTimer[site].Stop();
                                if (Bin.ContainsKey(Convert.ToInt32(site)))
                                {
                                    Bin[site] = bin;
                                }
                                else Bin.Add(site, bin);
                                listtesttimespan[site] = DateTime.Now - listtestdatetime[site];
                                TestTime = DateTime.Now - listtestdatetime[site];
                                OutSiteQueue.Enqueue(site);
                                BinSaveQueue.Enqueue(bin);
                                SiteTestBin[site] = new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] };
                                SiteTestState[site] = 3;

                                //用来统计数据
                                OutBin.Enqueue(new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] });
                                Listtestsingal.Add(site);

                                RunLog($"接收Site:{site:D2},Bin:{bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}");
                            }
                            else RunLog($"site:{site:D2}未在测试!", WriteData.LogType.ComLog);
                            outdata = outdata.Remove(0, outdata.IndexOf('+') + 1);
                        }
                        catch (Exception ex)
                        {
                            Listcode.Add("1400150");
                            RunLog($"[Code]1400150,[Msg]数据解析失败！详细信息：" + ex.Message, LogType.AlarmLog);
                            ProgramPause = true;
                            IsPause = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Listcode.Add("1400150");
                RunLog($"[Code]1400150,[Msg]接收串口数据解析失败！详细信息：" + ex.Message, LogType.AlarmLog);
                ProgramPause = true;
                IsPause = true;
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
            RunLog(str, WriteData.LogType.ComLog);
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
        public string Send(short site, List<bool[]> isopen, bool isOFFLine = false)
        {
            try
            {
                string str = getstartest(site, isopen);

                if (isOFFLine) //离线模式，无测试机
                {
                    RunLog("@START_TEST " + str + "+", WriteData.LogType.ComLog);
                    RefreshInfoLog("@START_TEST " + str + "+", false);
                    ListNoTestTimer[site].Start();
                    listtestdatetime[site] = DateTime.Now;
                }
                else //在线模式
                {
                    if (serialPort == null || !serialPort.IsOpen)
                    {
                        return "串口未打开";
                    }
                    serialPort.Write("@START_TEST " + str + "+");
                    RunLog("@START_TEST " + str + "+", WriteData.LogType.ComLog);
                    RefreshInfoLog("@START_TEST " + str + "+", false);
                    listtestdatetime[site] = DateTime.Now;
                    Listtimer[site].Start();
                    Iswaitout[site] = false;
                    //启动定时器
                }
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
            Iswaitout[t] = true;
            string[] bin = { "20A", "20A", "20A", "20A", "20A", "20A", "20A", "20A" };
            if (Bin.ContainsKey(t))
            {
                Bin[t] = bin;
            }
            else Bin.Add(t, bin);
            OutSiteQueue.Enqueue(t);
            BinSaveQueue.Enqueue(bin);
            //用来统计数据
            OutBin.Enqueue(new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] });
            Listtestsingal.Add(t);
            SiteTestBin[t] = new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] };
            SiteTestState[t] = 3;
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

            if (设置信息.测试机信息.超时发送timeout)
            {
                timeOutTimer[t].Start();
                //发送timeout
                Send($"@TIMEOUT {t.ToString("D2")}+");
                return;
            }

            Iswaitout[t] = true;
            string[] bin = { "20A", "20A", "20A", "20A", "20A", "20A", "20A", "20A" };
            if (Bin.ContainsKey(t))
            {
                Bin[t] = bin;
            }
            else Bin.Add(t, bin);
            OutSiteQueue.Enqueue(t);
            BinSaveQueue.Enqueue(bin);
            //用来统计数据
            OutBin.Enqueue(new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] });
            Listtestsingal.Add(t);
            SiteTestBin[t] = new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] };
            SiteTestState[t] = 3;
            //writeDataHelper.RunLog($"{DateTime.Now.ToString("HH:mm:dd")}:Site{t + 1}接收超时", 2);
        }

        private void ReceiveNoTestTimer(object sender, EventArgs e)
        {
            (sender as System.Timers.Timer).Stop();
            int site = 0;
            System.Timers.Timer timer = sender as System.Timers.Timer;
            for (int i = 0; i < 16; i++)
            {
                if (timer.Equals(ListNoTestTimer[i]))
                {
                    site = i;
                    break;
                }
            }

            string bins = string.Join(",", Auto1);
            bins += "," + string.Join(",", Auto2);
            if (Fix1.Length != 0)
                bins += "," + string.Join(",", Fix1);
            if (Fix2.Length != 0)
                bins += "," + string.Join(",", Fix2);
            if (Fix3.Length != 0)
                bins += "," + string.Join(",", Fix3);

            string[] bins2 = bins.Split(',');

            string[] bin = GetRandomBin(0, bins2.Length, bins2);
            //string[] bin = { "20A", "20A", "20A", "20A", "20A", "20A", "20A", "20A" };

            RefreshInfoLog($"@TEST_DONE {site:D2} {bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}+", true);
            RunLog($"@TEST_DONE {site:D2} {bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}+", WriteData.LogType.ComLog, true);

            if (Bin.ContainsKey(site))
            {
                Bin[site] = bin;
            }
            else Bin.Add(site, bin);
            listtesttimespan[site] = DateTime.Now - listtestdatetime[site];
            TestTime = DateTime.Now - listtestdatetime[site];

            OutSiteQueue.Enqueue(site);
            BinSaveQueue.Enqueue(bin);

            //用来统计数据
            OutBin.Enqueue(new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] });
            Listtestsingal.Add(site);
            SiteTestBin[site] = new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] };
            SiteTestState[site] = 3;

            RunLog($"接收Site:{site:D2},随机Bin:{bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}");
        }

        private string[] GetRandomBin(int x, int y, string[] bins)
        {
            Random random = new Random();
            Thread.Sleep(15);
            int dout1 = random.Next(x, y); //模拟Auto1/Auto2/Fix1/Fix2的BIN信号
            Thread.Sleep(15);
            int dout2 = random.Next(x, y);
            Thread.Sleep(15);
            int dout3 = random.Next(x, y);
            Thread.Sleep(15);
            int dout4 = random.Next(x, y);
            Thread.Sleep(15);
            int dout5 = random.Next(x, y);
            Thread.Sleep(15);
            int dout6 = random.Next(x, y);
            Thread.Sleep(15);
            int dout7 = random.Next(x, y);
            Thread.Sleep(15);
            int dout8 = random.Next(x, y);
            Thread.Sleep(15);

            return new string[]
            {
                bins[dout1], bins[dout2], bins[dout3], bins[dout4],
                bins[dout5], bins[dout6], bins[dout7], bins[dout8]
            };
        }
    }
}

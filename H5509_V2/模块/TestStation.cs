using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComMaster.Devices;
using ComMaster.Interfaces;
using TextFileLogger;
using static H5509_V2.APPmaster;
using static SiCommunication.Profinet.Knx.KnxCode;

namespace H5509_V2
{
    public class TestDataInfo
    {
        public int SiteTestState = 0;

        public bool IsconnectTester = false;

        public System.Timers.Timer Listtimer = new System.Timers.Timer(); //测试等待时间
        public System.Timers.Timer timeOutTimer = new System.Timers.Timer(); //测试等待超时时间
        public System.Timers.Timer ListNoTestTimer = new System.Timers.Timer(); //离线模式等待测试时间

        public TimeSpan listtesttimespan = new TimeSpan(0); //记录测试时长
        public DateTime listtestdatetime = new DateTime(0); //记录开始测试时间
        public TimeSpan TestTime = new TimeSpan(0); //记录测试时长

        public bool Iswaitout = false; //是否等待超时
        public bool Isreceive = false; //是否已接收测试数据

        public string[] SiteTestBin = new string[8]; //记录测试结果，int:site? ,string[]:bin{"01", "01" ,..., "01"}
        public bool[][] SiteTestIC = new bool[2][];
    }

    // 单个工位类
    public class TestStation
    {
        public int StationId { get; }

        public ICommunicationDevice Communication;

        private Dictionary<string, object> _currentConfig;

        public event EventHandler<ConnectionEventArgs> ConnectionChanged;

        public bool IsconnectTester { get; set; }

        public Action<int, TestDataInfo> DataUpdate;

        public Action<int, string> RefrshLog;

        public Action<int, bool> UpdateConnect;

        public TestDataInfo[] testDataInfos = new TestDataInfo[16];


        //public int[] SiteTestState = new int[16];

        //public System.Timers.Timer[] Listtimer = new System.Timers.Timer[16]; //测试等待时间
        //public System.Timers.Timer[] timeOutTimer = new System.Timers.Timer[16]; //测试等待超时时间
        //public System.Timers.Timer[] ListNoTestTimer = new System.Timers.Timer[16]; //离线模式等待测试时间

        //public TimeSpan[] listtesttimespan = new TimeSpan[16]; //记录测试时长
        //public DateTime[] listtestdatetime = new DateTime[16]; //记录开始测试时间
        //public TimeSpan TestTime = new TimeSpan(0); //记录测试时长

        //public bool[] Iswaitout = new bool[16]; //是否等待超时
        //public bool[] Isreceive = new bool[16]; //是否已接收测试数据

        //public string[][] OutBin = new string[16][]; //记录测试结果，int:site? ,string[]:bin{"01", "01" ,..., "01"}
        //public ICommunicationDevice CommunicationDevice => _communication;

        public TestStation(int stationId)
        {
            StationId = stationId;

            for (int i = 0; i < 16; i++)
            {
                System.Timers.Timer timer = new System.Timers.Timer() { Interval = 设置信息.测试机信息.测试超时时间 * 1000 }; //默认测试超时时间修改为10分钟
                System.Timers.Timer timer2 = new System.Timers.Timer() { Interval = 设置信息.测试机信息.timeout超时时间 * 1000 }; //默认timeout超时时间修改为5秒
                System.Timers.Timer timer3 = new System.Timers.Timer() { Interval = 设置信息.测试机信息.离线模式测试等待时间 * 1000 };

                timer.Elapsed += ReceiveTimer;
                timer2.Elapsed += timeOut;
                timer3.Elapsed += ReceiveNoTestTimer; //离线模式

                testDataInfos[i] = new TestDataInfo()
                {
                    Iswaitout = false,
                    Listtimer = timer,
                    listtestdatetime = DateTime.Now,
                    listtesttimespan = new TimeSpan(0),
                    timeOutTimer = timer2,
                    ListNoTestTimer = timer3,
                    SiteTestState = 0,
                    Isreceive = false,
                    SiteTestIC = new bool[2][] { new bool[] { false, false, false, false  }, new bool[] { false, false, false, false } },
                };
            }
        }

        public void UpdateTime()
        {
            for (int i = 0; i < testDataInfos.Length; i++)
            {
                testDataInfos[i].Listtimer.Interval = 设置信息.测试机信息.测试超时时间 * 1000;
                testDataInfos[i].timeOutTimer.Interval = 设置信息.测试机信息.timeout超时时间 * 1000;
                testDataInfos[i].ListNoTestTimer.Interval = 设置信息.测试机信息.离线模式测试等待时间 * 1000;
            }
        }

        // 更新通信配置（支持动态切换协议）
        public void UpdateCommunication(Dictionary<string, object> config)
        {
            _currentConfig = config;
            Communication?.Dispose(); // 释放旧连接

            // 根据配置的协议类型创建新实例（可通过工厂模式优化）
            switch (config["Protocol"].ToString().ToUpper())
            {
                case "COM":
                    string portName = config["PortName"].ToString();
                    int baudRate = (int)config["BaudRate"];
                    int dataBits = (int)config["DataBits"];
                    string parity = config["Parity"].ToString();
                    int stopBits = (int)config["StopBits"];
                    Communication = new SerialDevice(portName, baudRate, dataBits, stopBits, parity);
                    Communication.DataReceive = DataReceived;
                    break;
                case "TCP":
                    Communication = new EthernetDevice("", "");
                    break;
                case "LAN": // 假设LAN与TCP类似，或单独实现
                    Communication = new EthernetDevice("", "");
                    break;
                case "GPIB":
                    Communication = new GpibDevice("", "");
                    break;
                default:
                    throw new NotSupportedException($"不支持的协议: {config["Protocol"]}");
            }

            //// 绑定状态变更事件
            //Communication.ConnectionChanged += (sender, e) =>
            //{
            //    ConnectionChanged?.Invoke(this, new ConnectionEventArgs
            //    {
            //        StationId = StationId.ToString(),
            //        IsConnected = e.IsConnected,
            //        Message = e.Message
            //    });
            //};
        }

        public void DataReceived(string outdata)
        {
            Debug.WriteLine($"测试工位{StationId}接收数据:{outdata}");

            RunLog($"({Communication.DeviceName})" + outdata, LogType.ComLog);
            RefreshInfoLog(outdata, true);

            while (outdata.Length > 0)
            {
                //@START_INFO设置成功后应返回  "@INFO_OK"
                if (outdata.StartsWith("@INFO_OK"))
                {
                    outdata = outdata.Remove(0, outdata.IndexOf('+') + 1);
                    UpdateConnect(StationId, true); //IsconnectTester = true;
                    testDataInfos[StationId].IsconnectTester = true;
                }

                if (outdata.StartsWith("@TEST_DONE"))
                {
                    try
                    {
                        if (设置信息.通讯信息.通讯协议 == "得一微")
                        {
                            int site = StationId; //站点号

                            int[] sites = { StationId, StationId + 1 };

                            string bin = outdata.Substring(11, outdata.Length - 12);

                            // 2. 将长字符串按每2个字符一组分割
                            string[] allParts = new string[bin.Length / 2];
                            for (int i = 0; i < allParts.Length; i++)
                            {
                                allParts[i] = bin.Substring(i * 2, 2);
                            }

                            string[][] bins = new string[2][];
                            // 前8个元素组成 bin1
                            bins[0] = allParts.Take(8).ToArray();
                            // 后8个元素组成 bin2
                            bins[1] = allParts.Skip(8).Take(8).ToArray();

                            if (testDataInfos[site].Listtimer.Enabled || testDataInfos[site].timeOutTimer.Enabled)
                            {
                                testDataInfos[site].Listtimer.Stop();
                                testDataInfos[site].timeOutTimer.Stop();

                                //testDataInfos[site].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                                //testDataInfos[site].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;

                                for (int i = 0; i < sites.Length; i++)
                                {
                                    if (testDataInfos[sites[i]].SiteTestState == 2) //if (listsitetest[site].TestStatus == true && Iswaitout[site] == false && Isreceive[site] == false)
                                    {
                                        testDataInfos[sites[i]].SiteTestBin = new string[] { bins[i][0], bins[i][1], bins[i][2], bins[i][3], bins[i][4], bins[i][5], bins[i][6], bins[i][7] };

                                        testDataInfos[sites[i]].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                                        testDataInfos[sites[i]].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;
                                        testDataInfos[sites[i]].SiteTestState = 3;

                                        RunLog($"接收Site:{sites[i]:D2},测试Bin:{bins[i][0]} {bins[i][1]} {bins[i][2]} {bins[i][3]} {bins[i][4]} {bins[i][5]} {bins[i][6]} {bins[i][7]}");

                                        DataUpdate(sites[i], testDataInfos[sites[i]]); //必须要传给StationManager,因为数据都是从StationManager取的,单个TestStation有可能负责16个site,也有可能只负责1个site，所以当查看station2时里面会无数据，因为TestStation1负责16个site的数据发送接受(1台测试机)
                                    }
                                }
                            }
                            else RunLog($"site:{site:D2}未在测试!", LogType.ComLog);
                        }
                        else
                        {

                            string[] bin = outdata.Substring(14, 23).Split(' '); //@TEST_DONE 05 06 01 06 06 06 06 06 06+
                            int site = Convert.ToInt32(outdata.Substring(11, 2));

                            if (testDataInfos[site].SiteTestState == 2 && (testDataInfos[site].Listtimer.Enabled || testDataInfos[site].timeOutTimer.Enabled)) //if (listsitetest[site].TestStatus == true && Iswaitout[site] == false && Isreceive[site] == false)
                            {
                                testDataInfos[site].Listtimer.Stop();
                                testDataInfos[site].timeOutTimer.Stop();

                                testDataInfos[site].SiteTestBin = new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] };

                                testDataInfos[site].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                                testDataInfos[site].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;
                                testDataInfos[site].SiteTestState = 3;

                                RunLog($"接收Site:{site:D2},测试Bin:{bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}");

                                DataUpdate(site, testDataInfos[site]); //必须要传给StationManager,因为数据都是从StationManager取的,单个TestStation有可能负责16个site,也有可能只负责1个site，所以当查看station2时里面会无数据，因为TestStation1负责16个site的数据发送接受(1台测试机)
                            }
                            else RunLog($"site:{site:D2}未在测试!", LogType.ComLog);
                        }
                        outdata = outdata.Remove(0, outdata.IndexOf('+') + 1);
                    }
                    catch (Exception ex)
                    {
                        //Listcode.Add("1400150");
                        //RunLog($"[Code]1400150,[Msg]数据解析失败！详细信息：" + ex.Message, LogType.AlarmLog);
                        //ProgramPause = true;
                        //IsPause = true;
                    }
                }
            }
        }

        // 连接设备（异步）
        public bool ConnectAsync()
        {
            if (Communication == null || _currentConfig == null)
                return false;
            return Communication.Connect();
        }

        // 发送数据
        public void SendAsync(string data)
        {
            if (Communication == null || !Communication.IsConnected)
            {
                return;
            }
            Communication.Send(data);
        }


        public void Send(string str)
        {
            if (Communication == null || !Communication.IsConnected)
            {
                return;
            }
            Communication.Send(str);
            RunLog($"({Communication.DeviceName})" + str, LogType.ComLog);
            RefreshInfoLog(str, false);
        }

        private void RefreshInfoLog(string message, bool isReceive)
        {
            string str = " => ";
            if (isReceive)
            {
                str = " <= ";
            }
            string datestr = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}{str}";
            RefrshLog(StationId, datestr + $"({Communication.DeviceName})" + message);
        }

        public string getstartest(List<List<bool[]>> isopen)
        {
            string str = "";
            for (int x = 0; x < isopen.Count; x++)
            {
                for (int i = 0; i < isopen[x].Count; i++)
                {
                    for (int j = 0; j < isopen[x][i].Length; j++)
                    {
                        if (isopen[x][i][j]) str += "1";
                        else str += "0";
                    }
                }
            }
            return str;
        }

        public string getstartest(int site, List<bool[]> isopen)
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
        public string Send(int site, List<bool[]> isopen, bool isOFFLine = false)
        {
            try
            {
                string str = getstartest(site, isopen);

                if (isOFFLine) //离线模式，无测试机
                {
                    RunLog("@START_TEST " + str + "+", LogType.ComLog);
                    RefreshInfoLog("@START_TEST " + str + "+", false);
                    testDataInfos[site].ListNoTestTimer.Start();
                    testDataInfos[site].listtestdatetime = DateTime.Now;
                    testDataInfos[site].SiteTestState = 2;
                }
                else //在线模式
                {
                    Send("@START_TEST " + str + "+");
                    //RunLog("@START_TEST " + str + "+", LogType.ComLog);
                    //RefreshInfoLog("@START_TEST " + str + "+", false);
                    testDataInfos[site].listtestdatetime = DateTime.Now;
                    testDataInfos[site].Listtimer.Start();
                    testDataInfos[site].Iswaitout = false;
                    testDataInfos[site].SiteTestState = 2;
                    //启动定时器
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        public string Send(int[] states, List<List<bool[]>> isopen, bool isOFFLine = false)
        {
            try
            {
                string str = getstartest(isopen);

                if (isOFFLine) //离线模式，无测试机
                {
                    RunLog("@START_TEST " + str + "+", LogType.ComLog);
                    RefreshInfoLog("@START_TEST " + str + "+", false);

                    testDataInfos[StationId].listtestdatetime = DateTime.Now;
                    testDataInfos[StationId].ListNoTestTimer.Start();
                    testDataInfos[StationId].Iswaitout = false;

                    if (states[0] == 1 || states[0] == 2) testDataInfos[StationId].SiteTestState = 2;
                    if (states[1] == 1 || states[1] == 2) testDataInfos[StationId + 1].SiteTestState = 2;
                }
                else //在线模式
                {
                    testDataInfos[StationId].listtestdatetime = DateTime.Now;
                    testDataInfos[StationId].Listtimer.Start();
                    testDataInfos[StationId].Iswaitout = false;

                    if (states[0] == 1 || states[0] == 2) testDataInfos[StationId].SiteTestState = 2;
                    if (states[1] == 1 || states[1] == 2) testDataInfos[StationId + 1].SiteTestState = 2;

                    Send("@START_TEST " + str + "+");
                    //RunLog("@START_TEST " + str + "+", LogType.ComLog);
                    //RefreshInfoLog("@START_TEST " + str + "+", false);

                    //启动定时器
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        private void GetTimeOutBin(int site)
        {
            if (设置信息.通讯信息.通讯协议 == "得一微")
            {
                //int site = StationId; //站点号

                int[] sites = { StationId, StationId + 1 };

                //testDataInfos[site].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                //testDataInfos[site].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;

                for (int i = 0; i < sites.Length; i++)
                {
                    if (testDataInfos[sites[i]].SiteTestState == 2)
                    {
                        testDataInfos[sites[i]].Iswaitout = true;
                        testDataInfos[sites[i]].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                        testDataInfos[sites[i]].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;
                        testDataInfos[sites[i]].SiteTestBin = new string[] { "20A", "20A", "20A", "20A", "20A", "20A", "20A", "20A" };
                        testDataInfos[sites[i]].SiteTestState = 3;

                        RunLog($"生成Site:{sites[i]:D2},超时Bin:20A 20A 20A 20A 20A 20A 20A 20A");

                        DataUpdate(sites[i], testDataInfos[sites[i]]);
                    }
                }
            }
            else
            {
                testDataInfos[site].Iswaitout = true;

                testDataInfos[site].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                testDataInfos[site].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;

                testDataInfos[site].SiteTestBin = new string[] { "20A", "20A", "20A", "20A", "20A", "20A", "20A", "20A" };
                testDataInfos[site].SiteTestState = 3;

                RunLog($"生成Site:{site:D2},超时Bin:20A 20A 20A 20A 20A 20A 20A 20A");

                DataUpdate(site, testDataInfos[site]);
            }
        }

        private void GetRandomBin(int site)
        {

            string bins = string.Join(",", Auto1);
            bins += "," + string.Join(",", Auto2);
            if (Fix1.Length != 0)
                bins += "," + string.Join(",", Fix1);
            if (Fix2.Length != 0)
                bins += "," + string.Join(",", Fix2);
            if (Fix3.Length != 0)
                bins += "," + string.Join(",", Fix3);

            string[] bins2 = bins.Split(',');


            if (设置信息.通讯信息.通讯协议 == "得一微")
            {
                int[] sites = { StationId, StationId + 1 };

                testDataInfos[site].Listtimer.Stop();
                testDataInfos[site].timeOutTimer.Stop();
                testDataInfos[site].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                testDataInfos[site].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;

                for (int i = 0; i < sites.Length; i++)
                {
                    if (testDataInfos[sites[i]].SiteTestState == 2) //if (listsitetest[site].TestStatus == true && Iswaitout[site] == false && Isreceive[site] == false)
                    {
                        string[] bin = GetRandomBin(bins2, 8);

                        testDataInfos[sites[i]].SiteTestBin = new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] };

                        testDataInfos[sites[i]].SiteTestState = 3;

                        RunLog($"生成Site:{sites[i]:D2},随机Bin:{bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}");

                        DataUpdate(sites[i], testDataInfos[sites[i]]); //必须要传给StationManager,因为数据都是从StationManager取的,单个TestStation有可能负责16个site,也有可能只负责1个site，所以当查看station2时里面会无数据，因为TestStation1负责16个site的数据发送接受(1台测试机)

                    }
                    //else RunLog($"site:{site:D2}未在测试!", LogType.ComLog);
                }
            }
            else
            {
                string[] bin = GetRandomBin(bins2, 8);

                testDataInfos[site].Listtimer.Stop();
                testDataInfos[site].timeOutTimer.Stop();
                RefreshInfoLog($"@TEST_DONE {site:D2} {bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}+", true);
                //RunLog($"@TEST_DONE {site:D2} {bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}+", LogType.ComLog, true);

                testDataInfos[site].SiteTestBin = new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] };
                testDataInfos[site].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                testDataInfos[site].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;

                testDataInfos[site].SiteTestState = 3;

                RunLog($"生成Site:{site:D2},随机Bin:{bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}");

                DataUpdate(site, testDataInfos[site]);
            }
        }

        private void timeOut(object sender, EventArgs e)
        {
            (sender as System.Timers.Timer).Stop();
            int site = 0;
            System.Timers.Timer timer = sender as System.Timers.Timer;
            for (int i = 0; i < 16; i++)
            {
                if (timer.Equals(testDataInfos[i].timeOutTimer))
                {
                    site = i;
                    break;
                }
            }
            GetTimeOutBin(site);

        }

        private void ReceiveTimer(object sender, EventArgs e)
        {
            (sender as System.Timers.Timer).Stop();
            int site = 0;
            System.Timers.Timer timer = sender as System.Timers.Timer;
            for (int i = 0; i < 16; i++)
            {
                if (timer.Equals(testDataInfos[i].Listtimer))
                {
                    site = i;
                    break;
                }
            }

            GetTimeOutBin(site);
        }

        private void ReceiveNoTestTimer(object sender, EventArgs e)
        {
            (sender as System.Timers.Timer).Stop();
            int site = 0;
            System.Timers.Timer timer = sender as System.Timers.Timer;
            for (int i = 0; i < 16; i++)
            {
                if (timer.Equals(testDataInfos[i].ListNoTestTimer))
                {
                    site = i;
                    break;
                }
            }

            if (设置信息.测试机信息.超时发送timeout)
            {
                testDataInfos[site].timeOutTimer.Start();
                //发送timeout
                Send($"@TIMEOUT {site.ToString("D2")}+");
                return;
            }

            GetRandomBin(site);
        }

        private string[] GetRandomBin(string[] bins, int leng)
        {
            Random random = new Random();

            string[] bin = new string[leng];
            for (int i = 0; i < bin.Length; i++)
            {
                bin[i] = bins[random.Next(0, bins.Length)];
                Thread.Sleep(15);
            }
            return bin;
        }

        // 断开连接
        public void Disconnect()
        {
            Communication?.Disconnect();
            Communication?.Dispose();
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
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

        public string[][] BinResult32 = new string[4][];

        public int[][] HasIC32 = new int[4][];

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
                    SiteTestIC = new bool[2][] { new bool[] { false, false, false, false }, new bool[] { false, false, false, false } },
                };
            }

            for (int i = 0; i < 4; i++)
            {
                BinResult32[i] = new string[8];
                HasIC32[i] = new int[8];
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
                    Communication = new SerialDevice(StationId, portName, baudRate, dataBits, stopBits, parity);
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

        public static int GetTesterId(int site)
        {
            int testernum = 设置信息.通讯信息.测试机数量;
            if (testernum == 1) return 0;
            else if (testernum == 4)
            {
                switch (site)
                {
                    case 0: case 1: case 2: case 3: return 0;
                    case 4: case 5: case 6: case 7: return 1;
                    case 8: case 9: case 10: case 11: return 2;
                    case 12: case 13: case 14: case 15: return 3;
                    default: return 0;
                }
            }
            else if (testernum == 8)
            {
                switch (site)
                {
                    case 0: case 1: return 0;
                    case 2: case 3: return 1;
                    case 4: case 5: return 2;
                    case 6: case 7: return 3;
                    case 8: case 9: return 4;
                    case 10: case 11: return 5;
                    case 12: case 13: return 6;
                    case 14: case 15: return 7;
                    default: return 0;
                }
            }
            else if (testernum == 16) return site;
            else return 0;
        }

        private void SaveBinResult(bool isReceive)
        {
            bool istrue = true;
            for (int i = 0; i < HasIC32.Length; i++)
            {
                for (int j = 0; j < HasIC32[i].Length; j++)
                {
                    if (HasIC32[i][j] <= 0)
                    {
                        istrue = false;
                        break;
                    }
                }
            }

            isReceive = istrue;

            if (isReceive)
            {
                #region 待删除

                int site = StationId; //站点号

                int[] sites = { StationId, StationId + 1, StationId + 2, StationId + 3 };

                string[][] bins = BinResult32;
                //string bin = outdata.Substring(11, outdata.Length - 12);

                //// 2. 将长字符串按每2个字符一组分割
                //string[] allParts = new string[bin.Length / 2];
                //for (int i = 0; i < allParts.Length; i++)
                //{
                //    allParts[i] = bin.Substring(i * 2, 2);
                //}

                //string[][] bins = new string[4][];
                //// 前8个元素组成 bin1
                //bins[0] = allParts.Take(8).ToArray();
                //// 后8个元素组成 bin2
                //bins[1] = allParts.Skip(8).Take(8).ToArray();
                //bins[2] = allParts.Skip(16).Take(8).ToArray();
                //bins[3] = allParts.Skip(24).Take(8).ToArray();
                if (testDataInfos[site].Listtimer.Enabled || testDataInfos[site].timeOutTimer.Enabled)
                {
                    //if (bins[0].Length != 8 || bins[1].Length != 8 || bins[2].Length != 8 || bins[3].Length != 8)
                    //{
                    //    throw new Exception("接收数据长度异常");
                    //}

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
                else RunLog($"site:{site:D2}未在测试!", LogType.RunLog);

                #endregion
            }
        }

        public void DataReceived(string outdata)
        {
            Debug.WriteLine($"测试工位{StationId}接收数据:{outdata}");

            RunLog($"(测试机{GetTesterId(StationId) + 1}:{Communication.DeviceName})" + outdata, LogType.ComLog);
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

                            //string pattern = @"^@TEST_DONE\s+((\bF\d+\b\s+)+)\+$";
                            //Match match = Regex.Match(outdata, pattern);
                            //if (match.Success)
                            //{
                            //    string allIdentifiers = match.Groups[1].Value; // Groups[1] 对应第一个捕获组
                            //    string[] allValues = allIdentifiers.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            //}

                            string bin = outdata.Substring(11, outdata.Length - 12);

                            string[] allParts = bin.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            //// 2. 将长字符串按每2个字符一组分割
                            //string[] allParts = new string[bin.Length / 2];
                            //for (int i = 0; i < allParts.Length; i++)
                            //{
                            //    allParts[i] = bin.Substring(i * 2, 2);
                            //}

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
                            else RunLog($"site:{site:D2}未在测试!", LogType.RunLog);
                        }
                        else if (设置信息.通讯信息.通讯协议 == "慧荣") //SMI eMMC芯片
                        {
                            //接收@TEST_DONE 25 B1+
                            //接收@TEST_DONE 25 160+

                            bool isReceive = false;

                            //string pattern = @"^@TEST_DONE\s+(\d+)\s+([^+]+)\+$";
                            string pattern = @"@TEST_DONE\s+(\d+)\s+([^+]+)\+";
                            //foreach (string str in testStrings)
                            //{
                            //string str = "@TEST_DONE 25 160+";
                            MatchCollection matches = Regex.Matches(outdata, pattern);
                            foreach (Match match in matches)
                            {
                                // 从第一个捕获组（Groups[1]）中获取值
                                string numberPart = match.Groups[1].Value; // 对应 (\d+)
                                string codePart = match.Groups[2].Value;  // 对应 ([^+]+)

                                int dutnum = Convert.ToInt32(numberPart);
                                int tens = dutnum / 10;  // 十位数
                                int units = dutnum % 10 - 1; // 个位数

                                BinResult32[tens][units] = codePart;
                                HasIC32[tens][units] = 1;

                                outdata = outdata.Remove(0, outdata.IndexOf('+') + 1);
                                //Convert.ToInt32($"接收Dut{numberPart},Bin{codePart}");
                            }

                            SaveBinResult(isReceive);
                        }
                        else if (设置信息.通讯信息.通讯协议 == "衡宇")
                        {
                            string[] bin = outdata.Substring(14, 39).Split(' '); //@TEST_DONE 00 0001 0001 0001 0001 0001 0001 0001 0001+
                            int site = StationId + Convert.ToInt32(outdata.Substring(11, 2));

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
                            else RunLog($"site:{site:D2}未在测试!", LogType.RunLog);
                        }
                        else //晶存
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
                            else RunLog($"site:{site:D2}未在测试!", LogType.RunLog);
                        }
                        if (!outdata.Contains("+")) break;
                        else outdata = outdata.Remove(0, outdata.IndexOf('+') + 1);
                    }
                    catch (Exception ex)
                    {
                        Listcode.Add("1400150");
                        RunLog($"[Code]1400150,[Msg]数据解析失败！详细信息：" + ex.Message.Replace("\r\n", " "), LogType.AlarmLog);
                        int 测试机id = TestStation.GetTesterId(StationId);
                        串口数据异常Msg = $"(测试机{测试机id + 1}){ex.Message.Replace("\r\n", " ")}";
                        ProgramPause = true;
                        IsPause = true;
                        if (!outdata.Contains("+")) break;
                        else outdata = outdata.Remove(0, outdata.IndexOf('+') + 1);
                    }
                }
                else if (outdata.StartsWith("@START_TEST_RES"))
                {
                    try
                    {
                        if (设置信息.通讯信息.通讯协议 == "慧荣")
                        {
                            //发送111111111
                            //接收TEST_RES 00000000200002000000
                            char[] chars = outdata.Substring(16, 32).ToCharArray(); //@START_TEST_RES 00000000000000000000000000000000+
                            if (chars.Length == 32)
                            {
                                for (int i = 0; i < chars.Length; i++)
                                {
                                    int tens = i / 8;  // 十位数
                                    int units = i % 8; // 个位数
                                    if (HasIC32[tens][units] == 0)
                                    {
                                        if (chars[i] == '2')
                                        {
                                            HasIC32[tens][units] = 3;
                                            BinResult32[tens][units] = 设置信息.测试机信息.超时Bin字符串显示;
                                        }
                                    }
                                }

                                SaveBinResult(false);
                            }
                        }
                        outdata = outdata.Remove(0, outdata.IndexOf('+') + 1);
                    }
                    catch (Exception ex)
                    {
                        Listcode.Add("1400150");
                        RunLog($"[Code]1400150,[Msg]数据解析失败！详细信息：" + ex.Message.Replace("\r\n", " "), LogType.AlarmLog);
                        int 测试机id = TestStation.GetTesterId(StationId);
                        串口数据异常Msg = $"(测试机{测试机id + 1}){ex.Message.Replace("\r\n", " ")}";
                        ProgramPause = true;
                        IsPause = true;
                        if (!outdata.Contains("+")) break; 
                        else outdata = outdata.Remove(0, outdata.IndexOf('+') + 1);
                    }
                }
                //else if (outdata.StartsWith("@UNKNOW_CMD"))
                //{ 

                //}
                else
                {
                    outdata = "";
                    break;
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
            RunLog($"(测试机{GetTesterId(StationId) + 1}:{Communication.DeviceName})" + str, LogType.ComLog);
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

            RefrshLog(StationId, datestr + $"(测试机{GetTesterId(StationId) + 1}:{Communication.DeviceName})" + message);
        }

        public static string getstartest(List<List<bool[]>> isopen)
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

        public static string getstartest(int site, List<bool[]> isopen)
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

        public static int GetSiteNum(int site)
        {
            switch (site)
            {
                case 0: case 4: case 8: case 12: return 0;
                case 1: case 5: case 9: case 13: return 1;
                case 2: case 6: case 10: case 14: return 2;
                case 3: case 7: case 11: case 15: return 3;
                default: return 0;
            }
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
                //晶存site9 10 11 12测 site=8 9 10 11 //衡宇site9 10 11 12测，发0 1 2 3

                if (设置信息.通讯信息.通讯协议 == "衡宇")
                {
                    string str = getstartest(GetSiteNum(site), isopen);

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
                else //晶存
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

                    for (int i = 0; i < states.Length; i++)
                    {
                        if (states[i] == 1 || states[i] == 10) testDataInfos[StationId + i].SiteTestState = 2;
                        //if (states[0] == 1 || states[0] == 10) testDataInfos[StationId].SiteTestState = 2;
                        //if (states[1] == 1 || states[1] == 10) testDataInfos[StationId + 1].SiteTestState = 2;
                    }
                }
                else //在线模式
                {
                    testDataInfos[StationId].listtestdatetime = DateTime.Now;
                    testDataInfos[StationId].Listtimer.Start();
                    testDataInfos[StationId].Iswaitout = false;

                    if (isopen.Count == 4) //4个site测试
                    {
                        BinResult32 = new string[4][];
                        HasIC32 = new int[4][];

                        for (int i = 0; i < 4; i++)
                        {
                            BinResult32[i] = new string[8];
                            HasIC32[i] = new int[8];
                        }

                        for (int i = 0; i < isopen.Count; i++) //4个site
                        {
                            for (int j = 0; j < isopen[i].Count; j++) //2列
                            {
                                for (int n = 0; n < isopen[i][j].Length; n++) //4个dut
                                {
                                    HasIC32[i][j * 4 + n] = isopen[i][j][n] ? 0 : 2;
                                    BinResult32[i][j * 4 + n] = isopen[i][j][n] ? "XX" : "YY";
                                }
                            }
                        }
                    }

                    for (int i = 0; i < states.Length; i++)
                    {
                        if (states[i] == 1 || states[i] == 10) testDataInfos[StationId + i].SiteTestState = 2;
                        //if (states[0] == 1 || states[0] == 10) testDataInfos[StationId].SiteTestState = 2;
                        //if (states[1] == 1 || states[1] == 10) testDataInfos[StationId + 1].SiteTestState = 2;
                    }

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
            string timeoutbin = 设置信息.测试机信息.超时Bin字符串显示;
            if (设置信息.通讯信息.同时测试 == 2)
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
                        testDataInfos[sites[i]].SiteTestBin = new string[] { timeoutbin, timeoutbin, timeoutbin, timeoutbin, timeoutbin, timeoutbin, timeoutbin, timeoutbin };
                        testDataInfos[sites[i]].SiteTestState = 3;

                        string logstr = $"生成Site:{sites[i] + 1:D2},超时Bin:{timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin}";
                        RunLog(logstr, LogType.ComLog);
                        RefreshInfoLog(logstr, false);
                        DataUpdate(sites[i], testDataInfos[sites[i]]);
                    }
                }
            }
            else if (设置信息.通讯信息.同时测试 == 4)
            {
                //int site = StationId; //站点号

                if (设置信息.通讯信息.通讯协议 == "慧荣")
                {
                    for (int i = 0; i < HasIC32.Length; i++)
                    {
                        for (int j = 0; j < HasIC32[i].Length; j++)
                        {
                            if (HasIC32[i][j] == 0)
                            {
                                BinResult32[i][j] = timeoutbin;
                            }
                        }
                    }

                    string[][] bins = BinResult32;

                    int[] sites = { StationId, StationId + 1, StationId + 2, StationId + 3 };

                    //testDataInfos[site].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                    //testDataInfos[site].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;

                    for (int i = 0; i < sites.Length; i++)
                    {
                        if (testDataInfos[sites[i]].SiteTestState == 2)
                        {
                            testDataInfos[sites[i]].Iswaitout = true;
                            testDataInfos[sites[i]].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                            testDataInfos[sites[i]].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;
                            testDataInfos[sites[i]].SiteTestBin = new string[] { bins[i][0], bins[i][1], bins[i][2], bins[i][3], bins[i][4], bins[i][5], bins[i][6], bins[i][7] };
                            testDataInfos[sites[i]].SiteTestState = 3;

                            string logstr = $"生成Site:{sites[i] + 1:D2},超时Bin:{bins[i][0]} {bins[i][1]} {bins[i][2]} {bins[i][3]} {bins[i][4]} {bins[i][5]} {bins[i][6]} {bins[i][7]}";
                            RunLog(logstr, LogType.ComLog);
                            RefreshInfoLog(logstr, false);

                            DataUpdate(sites[i], testDataInfos[sites[i]]);
                        }
                    }
                }
                else //
                {
                    int[] sites = { StationId, StationId + 1, StationId + 2, StationId + 3 };

                    //testDataInfos[site].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                    //testDataInfos[site].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;

                    for (int i = 0; i < sites.Length; i++)
                    {
                        if (testDataInfos[sites[i]].SiteTestState == 2)
                        {
                            testDataInfos[sites[i]].Iswaitout = true;
                            testDataInfos[sites[i]].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                            testDataInfos[sites[i]].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;
                            testDataInfos[sites[i]].SiteTestBin = new string[] { timeoutbin, timeoutbin, timeoutbin, timeoutbin, timeoutbin, timeoutbin, timeoutbin, timeoutbin };
                            testDataInfos[sites[i]].SiteTestState = 3;
                            
                            string logstr = $"生成Site:{sites[i] + 1:D2},超时Bin:{timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin}";
                            RunLog(logstr, LogType.ComLog);
                            RefreshInfoLog(logstr, false);

                            DataUpdate(sites[i], testDataInfos[sites[i]]);
                        }
                    }
                }
            }
            else
            {
                //if (设置信息.通讯信息.通讯协议 == "衡宇")

                testDataInfos[site].Iswaitout = true;

                testDataInfos[site].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                testDataInfos[site].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;

                testDataInfos[site].SiteTestBin = new string[] { timeoutbin, timeoutbin, timeoutbin, timeoutbin, timeoutbin, timeoutbin, timeoutbin, timeoutbin };
                testDataInfos[site].SiteTestState = 3;

                string logstr = $"生成Site:{site + 1:D2},超时Bin:{timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin} {timeoutbin}";
                RunLog(logstr, LogType.ComLog);
                RefreshInfoLog(logstr, false);

                DataUpdate(site, testDataInfos[site]);

            }
        }

        private string[] GetBins(string[] bins2)
        {
            string[] bin = new string[8];
            string trays = 设置信息.测试机信息.离线模式IC放置位置;
            if (trays == "AutoFix")
            {
                bin = GetRandomBin(bins2, 8);
            }
            else
            {
                string binic = "01";
                if (trays == "Auto1") binic = Auto1.Split(',')[0];
                else if (trays == "Auto2") binic = Auto2.Split(',')[0];
                else if (trays == "Fix1") binic = Fix1.Split(',')[0];
                else if (trays == "Fix2") binic = Fix2.Split(',')[0];
                else if (trays == "Fix3") binic = Fix3.Split(',')[0];
                bin = new string[] { binic, binic, binic, binic, binic, binic, binic, binic };
            }
            return bin;
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


            if (设置信息.通讯信息.同时测试 == 2)//"得一微"
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
                        string[] bin = GetBins(bins2);

                        testDataInfos[sites[i]].SiteTestBin = new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] };

                        testDataInfos[sites[i]].SiteTestState = 3;

                        string logstr = $"生成Site:{sites[i] + 1:D2},随机Bin:{bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}";
                        RunLog(logstr, LogType.ComLog);
                        RefreshInfoLog(logstr, false);

                        DataUpdate(sites[i], testDataInfos[sites[i]]); //必须要传给StationManager,因为数据都是从StationManager取的,单个TestStation有可能负责16个site,也有可能只负责1个site，所以当查看station2时里面会无数据，因为TestStation1负责16个site的数据发送接受(1台测试机)

                    }
                    //else RunLog($"site:{site:D2}未在测试!", LogType.ComLog);
                }
            }
            else if (设置信息.通讯信息.同时测试 == 4) //"慧荣"
            {
                int[] sites = { StationId, StationId + 1, StationId + 2, StationId + 3 };

                testDataInfos[site].Listtimer.Stop();
                testDataInfos[site].timeOutTimer.Stop();
                testDataInfos[site].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                testDataInfos[site].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;

                for (int i = 0; i < sites.Length; i++)
                {
                    if (testDataInfos[sites[i]].SiteTestState == 2) //if (listsitetest[site].TestStatus == true && Iswaitout[site] == false && Isreceive[site] == false)
                    {
                        string[] bin = GetBins(bins2);

                        testDataInfos[sites[i]].SiteTestBin = new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] };

                        testDataInfos[sites[i]].SiteTestState = 3;

                        string logstr = $"生成Site:{sites[i] + 1:D2},随机Bin:{bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}";
                        RunLog(logstr, LogType.ComLog);
                        RefreshInfoLog(logstr, false);

                        DataUpdate(sites[i], testDataInfos[sites[i]]); //必须要传给StationManager,因为数据都是从StationManager取的,单个TestStation有可能负责16个site,也有可能只负责1个site，所以当查看station2时里面会无数据，因为TestStation1负责16个site的数据发送接受(1台测试机)

                    }
                    //else RunLog($"site:{site:D2}未在测试!", LogType.ComLog);
                }
            }
            else //"晶存" 或 "衡宇"
            {
                string[] bin = GetBins(bins2);

                testDataInfos[site].Listtimer.Stop();
                testDataInfos[site].timeOutTimer.Stop();
                RefreshInfoLog($"@TEST_DONE {site:D2} {bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}+", true);
                //RunLog($"@TEST_DONE {site:D2} {bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}+", LogType.ComLog, true);

                testDataInfos[site].SiteTestBin = new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] };
                testDataInfos[site].listtesttimespan = DateTime.Now - testDataInfos[site].listtestdatetime;
                testDataInfos[site].TestTime = DateTime.Now - testDataInfos[site].listtestdatetime;

                testDataInfos[site].SiteTestState = 3;

                string logstr = $"生成Site:{site + 1:D2},随机Bin:{bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}";
                RunLog(logstr, LogType.ComLog);
                RefreshInfoLog(logstr, false);

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


            if (设置信息.测试机信息.超时发送timeout)
            {
                testDataInfos[site].timeOutTimer.Start();
                //发送timeout
                Send($"@TIMEOUT {site.ToString("D2")}+");
                return;
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

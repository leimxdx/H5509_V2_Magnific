using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComMaster.Devices;
using ComMaster.Interfaces;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Presentation;
using static H5509_V2.APPmaster;

namespace H5509_V2
{
    public class StationManager
    {
        private Dictionary<int, TestStation> _stations; // 工位ID到实例的映射

        // 新增映射表：groupName+deviceId → 工位ID
        private Dictionary<(string group, string id), int> _deviceMap = new Dictionary<(string, string), int>();

        private readonly object _lock = new object(); // 线程安全锁

        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        //public int[] SiteTestState = new int[16];

        //public bool[][][] SiteTestIC = new bool[16][][]; //保存Site测试结果

        //public string[][] SiteTestBin = new string[16][]; //保存Site测试结果对应的Bin信息

        //public ConcurrentQueue<string[]> BinSaveQueue = new ConcurrentQueue<string[]>(); //用来保存接收的Bin数据的队列
        public Action<int, string> RefreshLog;

        public TestDataInfo[] testDataInfos = new TestDataInfo[16];

        //public ConcurrentQueue<(int SiteId, TestDataInfo Data)> OutSiteQueue = new ConcurrentQueue<(int, TestDataInfo)>();

        public StationManager()
        {
            _stations = new Dictionary<int, TestStation>();

            InitializeStations(); // 初始化16个工位
        }

        // 初始化16个工位（可根据需求动态添加）
        private void InitializeStations()
        {
            for (int i = 0; i < 16; i++)
            {
                _stations[i] = new TestStation(i);
                _stations[i].DataUpdate = DataReceived;
                _stations[i].RefrshLog = UpdateLog;
                _stations[i].UpdateConnect = UpdateConect;
                testDataInfos[i] = new TestDataInfo()
                {
                    Iswaitout = false,
                    Listtimer = new System.Timers.Timer(),
                    listtestdatetime = DateTime.Now,
                    listtesttimespan = new TimeSpan(0),
                    timeOutTimer = new System.Timers.Timer(),
                    ListNoTestTimer = new System.Timers.Timer(),
                    SiteTestBin = new string[8],
                };
            }
        }

        public void UpdateTime()
        {
            for (int i = 0; i < _stations.Count; i++)
            {
                _stations[i].UpdateTime();
            }
        }

        public void ConnectAll()
        {
            GetStationId(out int[] site);
            //bool[] isconnect = new bool[site.Length];
            for (int i = 0; i < site.Length; i++)
            {
                TestStation testStation = GetStation(site[i]);
                testStation.ConnectAsync();
                //isconnect[i] = testStation.Communication.IsConnected;
            }
        }

        // 注册设备到分组
        public void RegisterDevice(string groupName, string deviceId, int stationId)
        {
            _deviceMap[(groupName, deviceId)] = stationId;
        }

        public void Connect(int station)
        {
            _stations[station].ConnectAsync();
        }

        public bool CheckTestState(int state)
        {
            for (int i = 0; i < testDataInfos.Length; i++)
            {
                if (testDataInfos[i].SiteTestState == state)
                {
                    return true;
                }
            }
            return false;
        }


        public void SetStationState(int site, int state)
        {
            testDataInfos[site].SiteTestState = state;
        }

        public void SetStationIC(int site, bool[][] opens)
        {
            testDataInfos[site].SiteTestIC = opens;
        }

        public bool[][] GetStationIC(int site)
        {
            return testDataInfos[site].SiteTestIC;
        }

        public bool[][][] GetStationICs()
        {
            bool[][][] ics = new bool[testDataInfos.Length][][];
            for (int i = 0; i < testDataInfos.Length; i++)
            {
                ics[i] = testDataInfos[i].SiteTestIC;
            }
            return ics;
        }

        public void SetStationBin(int site, string[] bin)
        {
            testDataInfos[site].SiteTestBin = bin;
        }

        public string[][] GetStationBins()
        {
            string[][] bins = new string[testDataInfos.Length][];
            for (int i = 0; i < testDataInfos.Length; i++)
            {
                bins[i] = testDataInfos[i].SiteTestBin;
            }
            return bins;
        }

        public void SetIsconnectTester(int site, bool isconnect)
        {
            testDataInfos[site].IsconnectTester = isconnect;
            _stations[site].testDataInfos[site].IsconnectTester = isconnect;
        }

        public void SetIsconnectTester(bool isconnect)
        {
            for (int i = 0; i < testDataInfos.Length; i++)
            {
                testDataInfos[i].IsconnectTester = isconnect;
                _stations[i].testDataInfos[i].IsconnectTester = isconnect;
            }
        }

        public int[] GetStationStates()
        {
            int[] states = new int[testDataInfos.Length];
            for (int i = 0; i < testDataInfos.Length; i++)
            {
                states[i] = testDataInfos[i].SiteTestState;
            }
            return states;
        }

        public int GetStationState(int site)
        {
            return testDataInfos[site].SiteTestState;
        }

        public void DataReceived(int site, TestDataInfo data)
        {
            data.SiteTestIC = testDataInfos[site].SiteTestIC;
            testDataInfos[site] = data;

            TestDataInfo testdata = new TestDataInfo()
            {
                SiteTestIC = data.SiteTestIC,
                Iswaitout = data.Iswaitout,
                Listtimer = data.Listtimer,
                timeOutTimer = data.timeOutTimer,
                SiteTestBin = data.SiteTestBin,
                listtesttimespan = data.listtesttimespan,
                TestTime = data.TestTime,
                SiteTestState = data.SiteTestState,
            };

            OutSiteQueue.Enqueue((site, testdata));

            TestDataInfo testdata2 = new TestDataInfo()
            {
                SiteTestIC = data.SiteTestIC,
                Iswaitout = data.Iswaitout,
                Listtimer = data.Listtimer,
                timeOutTimer = data.timeOutTimer,
                SiteTestBin = data.SiteTestBin,
                listtesttimespan = data.listtesttimespan,
                TestTime = data.TestTime,
                SiteTestState = data.SiteTestState,
            };
            OutSiteQueue2.Enqueue((site, testdata2));

            // 处理接收到的数据
        }

        public void UpdateLog(int site, string log)
        {
            RefreshLog(site, log);
        }

        public void UpdateConect(int site, bool isconnect)
        {
            testDataInfos[site].IsconnectTester = isconnect;
        }

        // 根据工位ID获取工位实例
        public TestStation GetStation(int stationId)
        {
            lock (_lock)
            {
                if (_stations.TryGetValue(stationId, out var station))
                    return station;
                throw new ArgumentException($"工位{stationId}不存在");
            }
        }

        // 批量更新工位配置（从文件/数据库加载）
        public void UpdateStationsConfig(Dictionary<int, Dictionary<string, object>> configDict)
        {
            foreach (var kvp in configDict)
            {
                var station = GetStation(kvp.Key);

                // 添加空值检查
                station?.UpdateCommunication(kvp.Value);
            }
        }

        public bool[] CheckComConnect()
        {
            GetStationId(out int[] site);
            bool[] isconnect = new bool[site.Length];
            for (int i = 0; i < site.Length; i++)
            {
                isconnect[i] = _stations[site[i]].Communication.IsConnected;
            }
            return isconnect;
        }

        public bool[] CheckTesterConnect()
        {
            GetStationId(out int[] site);
            bool[] isconnect = new bool[site.Length];
            for (int i = 0; i < site.Length; i++)
            {
                isconnect[i] = testDataInfos[site[i]].IsconnectTester;
            }
            return isconnect;
        }

        public void GetStationId(out int[] sendsite)
        {
            switch (设置信息.通讯信息.测试机数量)
            {
                case 0: sendsite = new int[] { 0 }; break;

                case 4: sendsite = new int[] { 0, 4, 8, 12 }; break; // 每4个site为一组，取组起始值

                case 8: sendsite = new int[] { 0, 2, 4, 6, 8, 10, 12, 14 }; break; // 每2个site为一组，取组起始值

                case 16: sendsite = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }; break; // 直接映射

                default: sendsite = new int[] { 0 }; break;
            }
        }

        public void GetSendDevice(int site, out int sendsite)
        {
            switch (设置信息.通讯信息.测试机数量)
            {
                case 0: sendsite = 0; break;

                case 4: sendsite = (site / 4) * 4; break; // 每4个site为一组，取组起始值

                case 8: sendsite = (site / 2) * 2; break; // 每2个site为一组，取组起始值

                case 16: sendsite = site; break; // 直接映射

                default: sendsite = 0; break;
            }
        }

        public void Send(int stationId, string data)
        {
            GetSendDevice(stationId, out int sendsite);
            TestStation testStation = GetStation(sendsite);

            //UpdateLog(data, false);
            testStation.Send(data);
        }


        public string Send(int stationId, List<bool[]> open, bool isOffline = false)
        {
            GetSendDevice(stationId, out int sendsite);
            TestStation testStation = GetStation(sendsite);
            return testStation.Send(stationId, open, isOffline);
        }


        public string Send(int stationId, int[] teststate, List<List<bool[]>> open, bool isOffline = false)
        {
            GetSendDevice(stationId, out int sendsite);
            TestStation testStation = GetStation(sendsite);
            return testStation.Send(teststate, open, isOffline);
        }

        public ICommunicationDevice GetDevice(string groupName, string deviceId)
        {
            //try
            //{
            //    _rwLock.EnterReadLock();
            //    if (_stations.TryGetValue(groupName, out var devices)
            //      && devices.TryGetValue(deviceId, out var device))
            //    {
            //        return device;
            //    }
            return null;
            //}
            //finally
            //{
            //    _rwLock.ExitReadLock();
            //}
        }

        public T GetDevice<T>(string groupName, string deviceId) where T : class, ICommunicationDevice
        {
            // 1. 通过分组和设备ID找到工位
            if (!_deviceMap.TryGetValue((groupName, deviceId), out int stationId))
                throw new KeyNotFoundException($"设备 {groupName}/{deviceId} 未注册");

            // 2. 获取对应工位
            if (!_stations.TryGetValue(stationId, out var station))
                throw new KeyNotFoundException($"工位 {stationId} 不存在");

            // 3. 获取设备并转换类型
            var device = station.Communication as T;
            return device ?? throw new InvalidCastException(
                $"工位 {stationId} 的设备不是 {typeof(T).Name}");
        }

        #region  串口通信相关
        public ISerialDevice GetSerialDevice(string deviceId)
        {
            return GetDevice<ISerialDevice>("Serial", deviceId);
        }

        public ISerialDevice AddSerialDevice(string deviceId, string deviceName)
        {
            return null;
        }
        #endregion

        #region  TCP/IP 通信相关
        public IEthernetDevice GetEthernetDevice(string deviceId)
        {
            return GetDevice<IEthernetDevice>("Ethernet", deviceId);
        }
        #endregion
    }


}

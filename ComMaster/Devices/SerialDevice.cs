using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComMaster.Common;
using ComMaster.Interfaces;
using SiCommunication;
using SiCommunication.Serial;
using static System.Net.Mime.MediaTypeNames;

namespace ComMaster.Devices
{
    public sealed class SerialDevice : DeviceBase, ISerialDevice  //DeviceBase,
    {
        #region  属性字段
        //SerialBase
        private SerialPort _serialPort; //SerialBase

        private readonly object _syncLock = new object();

        /// <summary>
        /// string:消息内容; bool:是否为接收消息; bool:是否为报警消息; bool:是否保存日志
        /// </summary>
        public Action<string, bool, bool, bool> RunLog { get; set; }

        public string PortName { get; set; }

        public int BaudRate { get; set; }

        public int DataBits { get; set; }

        public int StopBits { get; set; }

        public string Parity { get; set; }

        public override bool IsConnected => _serialPort?.IsOpen ?? false; //如果 _serialPort 不为 null，则返回 _serialPort.IsOpen 的值 //如果左侧表达式为 null，则返回右侧的值（false）

        public bool IsconnectTester { get; set; } //是否连接测试机

        public bool WaitReceive { get; set; } //等待接收

        public int SiteTestState { get; set; } //测试状态 0:空闲 1:料已放入 2:等待接收 3:已接收
        /// <summary>
        /// 接收超时时间(毫秒)
        /// </summary>
        public int ReceiveTimeout { get; set; }
        /// <summary>
        /// 换行符
        /// </summary>
        public string NewLine { get; set; }

        #endregion

        #region 构造函数
        public SerialDevice(string portName, int baudRate, int dataBits, int stopBits, string parity)  : base("deviceId", "deviceName")
        {
            DeviceType = "Serial";
            PortName = portName;
            BaudRate = baudRate;
            DataBits = dataBits;
            StopBits = stopBits;
            Parity = parity;
            ReceiveTimeout = 1000;
            NewLine = ",";

            InitializeSerialPort();
        }
        #endregion

        #region 共有方法

        public override bool Connect()
        {
            lock (_syncLock)
            {
                if (IsConnected) return true;

                try
                {
                    InitializeSerialPort();

                    _serialPort.Open();
                    UpdateCommunicationTime();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public override void Disconnect()
        {
            lock (_syncLock)
            {
                if (!IsConnected) return;

                try
                {
                    _serialPort.Close();
                }
                catch (Exception)
                { }
            }
        }

        public byte[] ReadData()
        {
            lock (_syncLock)
            {
                if (!IsConnected) return new byte[0];

                try
                {
                    byte[] buffer = new byte[_serialPort.ReadBufferSize];
                    var result = _serialPort.Read(buffer, 0, 1000);
                    UpdateCommunicationTime();
                    return buffer;
                }
                catch (Exception)
                {
                    return new byte[0];
                }
            }
        }

        public byte[] SendAndReceive(byte[] data, int timeout = 1000)
        {
            lock (_syncLock)
            {
                if (!IsConnected) return new byte[0];

                try
                {
                    //var oldTimeout = _serialPort.ReceiveTimeOut;
                    //_serialPort.ReceiveTimeOut = timeout;

                    //try
                    //{
                    //    var result = _serialPort.ReadFromCoreServer(data);
                    //    if (result.IsSuccess)
                    //    {
                    //        UpdateCommunicationTime();
                    //        return result.Content;
                    //    }
                    return new byte[0];
                    //}
                    //finally
                    //{
                    //    _serialPort.ReceiveTimeOut = oldTimeout;
                    //}
                }
                catch (Exception)
                {
                    return new byte[0];
                }
            }
        }

        public bool SendData(string data)
        {
            lock (_syncLock)
            {
                if (!IsConnected) return false;

                try
                {
                    _serialPort.Write(data);
                    UpdateCommunicationTime();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public override void Send(string str)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
            {
                return;
            }
            _serialPort.Encoding = Encoding.UTF8;
            _serialPort.Write(str);
            //RunLog(str, LogType.ComLog);
            //RefreshInfoLog(str, false);
        }

        protected override void CopyConfigProperties(DeviceBase source)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 私有方法

        private void InitializeSerialPort()
        {
            lock (_syncLock)
            {
                if (_serialPort != null)
                {
                    if (IsConnected) Disconnect();
                    _serialPort.Dispose();
                }

                _serialPort = new SerialPort
                {
                    PortName = PortName,
                    BaudRate = BaudRate,
                    DataBits = DataBits,
                    StopBits = GetStopBitsValue(),
                    Parity = GetParityValue()
                };
                _serialPort.DataReceived += DataReceivedHandler;

                //_serialPort = new SerialBase();
                //_serialPort.SerialPortInni(PortName, BaudRate, DataBits, GetStopBitsValue(), GetParityValue());
                //_serialPort.ReceiveTimeOut = ReceiveTimeout;
                //_serialPort.RtsEnable = true;
            }
        }

        string old = "";
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //Thread.Sleep(1);
                SerialPort sp = (SerialPort)sender;
                string outdata = old + sp.ReadExisting();
                outdata = outdata.Replace("\r", "").Replace("\n", "");
                if (outdata[outdata.Length - 1] != '+')
                {
                    old = outdata;
                    return;
                }
                else old = "";

                //RefreshInfoLog(outdata, true);
                if (outdata.Length < 1) return;

                //RunLog(outdata, true, false, false);

                DataReceive(outdata);
                //while (outdata.Length > 0)
                //{
                    ////@START_INFO设置成功后应返回  "@INFO_OK"
                    //if (outdata.StartsWith("@INFO_OK"))
                    //{
                    //    outdata = outdata.Remove(0, outdata.IndexOf('+') + 1);
                    //    IsconnectTester = true;
                    //}

                    //if (outdata.StartsWith("@TEST_DONE"))
                    //{
                    //    try
                    //    {
                    //        string[] bin = outdata.Substring(14, 23).Split(' ');
                    //        int site = Convert.ToInt32(outdata.Substring(11, 2));

                    //        //if (SiteTestState[site] == 2 && (Listtimer[site].Enabled || timeOutTimer[site].Enabled)) //if (listsitetest[site].TestStatus == true && Iswaitout[site] == false && Isreceive[site] == false)
                    //        //{
                    //        //    Listtimer[site].Stop();
                    //        //    timeOutTimer[site].Stop();
                    //        //    if (Bin.ContainsKey(Convert.ToInt32(site)))
                    //        //    {
                    //        //        Bin[site] = bin;
                    //        //    }
                    //        //    else Bin.Add(site, bin);
                    //        //    listtesttimespan[site] = DateTime.Now - listtestdatetime[site];
                    //        //    TestTime = DateTime.Now - listtestdatetime[site];
                    //        //    OutSiteQueue.Enqueue(site);
                    //        //    BinSaveQueue.Enqueue(bin);
                    //        //    SiteTestBin[site] = new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] };
                    //        //    SiteTestState[site] = 3;

                    //        //    //用来统计数据
                    //        //    OutBin.Enqueue(new string[] { bin[0], bin[1], bin[2], bin[3], bin[4], bin[5], bin[6], bin[7] });
                    //        //    Listtestsingal.Add(site);

                    //        //    RunLog($"接收Site:{site:D2},Bin:{bin[0]} {bin[1]} {bin[2]} {bin[3]} {bin[4]} {bin[5]} {bin[6]} {bin[7]}", false, false, true);
                    //        //}
                    //        //else RunLog($"site:{site:D2}未在测试!", false, false, false);
                    //outdata = outdata.Remove(0, outdata.IndexOf('+') + 1);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        RunLog("数据解析失败！详细信息：" + ex.Message, false, true, false);
                    //        //Listcode.Add("1400150");
                    //        //RunLog($"[Code]1400150,[Msg]数据解析失败！详细信息：" + ex.Message, LogType.AlarmLog);
                    //        //ProgramPause = true;
                    //        //IsPause = true;
                    //        break;
                    //    }
                    //}
                //}
            }
            catch (Exception ex)
            {
                RunLog("接收串口数据失败！详细信息：" + ex.Message, false, true, false);
                //Listcode.Add("1400150");
                //RunLog($"[Code]1400150,[Msg]接收串口数据解析失败！详细信息：" + ex.Message, LogType.AlarmLog);
                //ProgramPause = true;
                //IsPause = true;
            }
        }

        private System.IO.Ports.Parity GetParityValue()
        {
            switch (Parity?.ToLower())
            {
                case "none":
                    return System.IO.Ports.Parity.None;
                case "even":
                    return System.IO.Ports.Parity.Even;
                case "odd":
                    return System.IO.Ports.Parity.Odd;
                case "mark":
                    return System.IO.Ports.Parity.Mark;
                case "space":
                    return System.IO.Ports.Parity.Space;
                default:
                    return System.IO.Ports.Parity.None;
            }
        }

        private System.IO.Ports.StopBits GetStopBitsValue()
        {
            switch (StopBits)
            {
                case 0:
                    return System.IO.Ports.StopBits.None;
                case 2:
                    return System.IO.Ports.StopBits.Two;
                case 15:
                    return System.IO.Ports.StopBits.OnePointFive;
                default:
                    return System.IO.Ports.StopBits.One;
            }
        }


        #endregion

    }
}

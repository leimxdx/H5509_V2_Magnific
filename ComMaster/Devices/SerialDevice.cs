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
    public sealed class SerialDevice : ISerialDevice  //DeviceBase,
    {
        #region  属性字段
        //SerialBase
        private SerialPort _serialPort; //SerialBase

        private readonly object _syncLock = new object();

        private bool _isDisposed;

        private static readonly object _lock = new object();

        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        /// <summary>
        /// string:消息内容; bool:是否为接收消息; bool:是否为报警消息; bool:是否保存日志
        /// </summary>
        public Action<string, bool, bool, bool> RunLog { get; set; }

        public string PortName { get; set; }

        public int BaudRate { get; set; }

        public int DataBits { get; set; }

        public int StopBits { get; set; }

        public string Parity { get; set; }

        public bool IsConnected => _serialPort?.IsOpen ?? false; //如果 _serialPort 不为 null，则返回 _serialPort.IsOpen 的值 //如果左侧表达式为 null，则返回右侧的值（false）

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

        public DateTime LastConnectionTime { get; set; }


        public Action<string> DataReceive { get; set; }

        public string DeviceId => throw new NotImplementedException();

        public string DeviceName{ get { return PortName; } set { PortName = value; } }

        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int StationId;

        #endregion

        #region 构造函数
        public SerialDevice(int stationid, string portName, int baudRate, int dataBits, int stopBits, string parity)
        {
            StationId = stationid;
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


        private void UpdateCommunicationTime()
        {
            LastConnectionTime = DateTime.Now;
        }

        public bool Connect()
        {
            lock (_syncLock)
            {
                if (IsConnected)
                {
                    _serialPort.DiscardOutBuffer();
                    return true;
                }

                try
                {
                    //InitializeSerialPort();

                    _serialPort.Open();
                    _serialPort.DiscardOutBuffer();
                    Console.WriteLine($"连接信息->工作站:{StationId},串口:{_serialPort.PortName},连接成功");
                    UpdateCommunicationTime();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"连接信息->工作站:{StationId},串口:{_serialPort.PortName},连接失败，详细信息：{ex.Message}");
                    return false;
                }
            }
        }

        public void Disconnect()
        {
            lock (_syncLock)
            {
                if (!IsConnected) return;

                try
                {
                    _serialPort.Close();
                    Console.WriteLine($"连接信息->工作站:{StationId},串口:{_serialPort.PortName},断开连接");
                }
                catch (Exception ex)
                { 
                    Console.WriteLine(ex.Message);
                    Console.WriteLine($"连接信息->工作站:{StationId},串口:{_serialPort.PortName},断开失败，详细信息：{ex.Message}");
                }
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

        public void Send(string str)
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

        private void CopyConfigProperties(DeviceBase source)
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

                Connect();
                //_serialPort = new SerialBase();
                //_serialPort.SerialPortInni(PortName, BaudRate, DataBits, GetStopBitsValue(), GetParityValue());
                //_serialPort.ReceiveTimeOut = ReceiveTimeout;
                //_serialPort.RtsEnable = true;
            }
        }

        string old = "";

        public event EventHandler<ConnectionEventArgs> ConnectionChanged;

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //Thread.Sleep(1);
                SerialPort sp = (SerialPort)sender;
                string outdata = old + sp.ReadExisting();
                outdata = outdata.Replace("\r", "").Replace("\n", "");
                if (!outdata.EndsWith("+"))
                {
                    old = outdata;
                    return;
                }
                else old = "";

                //if (outdata.Length < 1) return;

                DataReceive(outdata);
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

        public Task<bool> ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                if (IsConnected) Disconnect();
                // 释放托管资源
                _rwLock.Dispose();
            }
            // 释放非托管资源
            _isDisposed = true;
        }



    }
}

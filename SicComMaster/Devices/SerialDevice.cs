using SicComMaster.Common;
using SicComMaster.Interfaces;
using SiCommunication.Serial;
using Sunny.UI;
using System;

namespace SicComMaster.Devices
{
    public sealed class SerialDevice : DeviceBase, ISerialDevice
    {
        #region  属性字段
        private SerialBase _serialPort;
        private readonly object _syncLock = new object();

        public string PortName { get; set; }

        public int BaudRate { get; set; }

        public int DataBits { get; set; }

        public int StopBits { get; set; }

        public string Parity { get; set; }

        public override  bool IsConnected => _serialPort?.IsOpen() ?? false;

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
        public SerialDevice(string deviceId, string deviceName) : base(deviceId, deviceName)
        {
            DeviceType = "Serial";
            PortName = "COM12";
            BaudRate = 9600;
            DataBits = 8;
            StopBits = 1;
            Parity = "None";
            ReceiveTimeout = 1000;
            NewLine = "\r\n";

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

                    var result = _serialPort.Open();
                    if ( result.IsSuccess)
                    {
                        UpdateCommunicationTime();
                        return true;
                    }
                    return false;
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
                    //var result = _serialPort.ReadFromCoreServer();
                    //if (result.IsSuccess)
                    //{
                    //    UpdateCommunicationTime();
                    //    return result.Content;
                    //}

                    return new byte[0];
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
                    var oldTimeout = _serialPort.ReceiveTimeOut;
                    _serialPort.ReceiveTimeOut = timeout;

                    try
                    {
                        var result = _serialPort.ReadFromCoreServer(data);
                        if (result.IsSuccess)
                        {
                            UpdateCommunicationTime();
                            return result.Content;
                        }
                        return new byte[0];
                    }
                    finally
                    {
                        _serialPort.ReceiveTimeOut = oldTimeout;
                    }
                }
                catch (Exception)
                {
                    return new byte[0];
                }
            }
        }

        public bool SendData(byte[] data)
        {
            lock (_syncLock)
            {
                if (!IsConnected) return false;

                try
                {
                    //var result = _serialPort.(data);
                    //if (result.IsSuccess)
                    //{
                    //    UpdateCommunicationTime();
                    //    return true;
                    //}
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
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

                _serialPort = new SerialBase();
                _serialPort.SerialPortInni(PortName, BaudRate, DataBits, GetStopBitsValue(), GetParityValue());
                _serialPort.ReceiveTimeOut = ReceiveTimeout;
                _serialPort.RtsEnable = true;
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

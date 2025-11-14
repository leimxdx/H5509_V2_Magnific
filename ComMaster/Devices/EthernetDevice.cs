using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComMaster.Common;
using ComMaster.Interfaces;
using SiCommunication;
using SiCommunication.Core.Net;

namespace ComMaster.Devices
{
    public sealed class EthernetDevice : IEthernetDevice  //DeviceBase,
    {
        #region 属性字段

        private Socket _Socket; //SerialBase

        private IPAddress _IpAddress;

        private IPEndPoint _RemoteEndPoint; //private NetworkAlienClient _client;

        private readonly object _syncLock = new object();

        private bool _isConnected;

        private bool _isDisposed;

        public string IpAddress { get; set; }

        public int Port { get; set; }

        public int ConnectionTimeout { get; set; }

        public bool KeepConnection { get; set; }

        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        /// <summary>
        /// string:消息内容; bool:是否为接收消息; bool:是否为报警消息; bool:是否保存日志
        /// </summary>
        public Action<string, bool, bool, bool> RunLog { get; set; }

        public string DeviceId { get; set; }

        public string DeviceName { get; set; }

        public string Description { get; set; }

        public bool IsConnected { get; set; }

        public DateTime LastConnectionTime { get; set; }

        public Action<string> DataReceive { get; set; }

        public int StationId;

        private Task ReceiveTask;

        public CancellationTokenSource cts = new CancellationTokenSource(); //创建取消线程源对象

        public event EventHandler<ConnectionEventArgs> ConnectionChanged;

        #endregion

        public EthernetDevice(int stationid, string ipAddr, int port)
        {
            StationId = stationid;
            IpAddress = ipAddr;
            Port = port;
            ConnectionTimeout = 3000;
            KeepConnection = true;

            InitializeSerialPort();
        }

        private void InitializeSerialPort()
        {
            lock (_syncLock)
            {
                cts.Cancel();

                if (_Socket != null)
                {
                    if (IsConnected) Disconnect();
                    _Socket.Dispose();
                    Thread.Sleep(100); //等待线程退出
                }

                ConnectSocket();

                //IsConnected = false;
                //_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //_IpAddress = IPAddress.Parse(IpAddress); // 替换为目标电脑的IP
                //_RemoteEndPoint = new IPEndPoint(_IpAddress, Port); // 9090是端口号

                //_Socket. += DataReceivedHandler;

                //_Socket.SetKeepAlive(1000, 3);

                //Debug.WriteLine($"IP:{_IpAddress},Port:{Port} 连接中...");

                //Connect(_IpAddress, Port);

                //_serialPort = new SerialBase();
                //_serialPort.SerialPortInni(PortName, BaudRate, DataBits, GetStopBitsValue(), GetParityValue());
                //_serialPort.ReceiveTimeOut = ReceiveTimeout;
                //_serialPort.RtsEnable = true;
            }
        }
        private void UpdateCommunicationTime()
        {
            LastConnectionTime = DateTime.Now;
        }

        public bool ConnectSocket()
        {
            try
            {
                if (_Socket != null)
                {
                    CloseSocket();
                }

                IsConnected = false;
                _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _IpAddress = IPAddress.Parse(IpAddress); // 替换为目标电脑的IP
                _RemoteEndPoint = new IPEndPoint(_IpAddress, Port); // 9090是端口号

                Connect(_IpAddress, Port);

                //_Socket.SendTimeout = 10;
                //_Socket.Connect(_RemoteEndPoint);
                //Console.WriteLine($"连接信息->工作站:{StationId},TCP:{_IpAddress},Port:{Port},连接成功");
                //IsConnected = true;

                cts = new CancellationTokenSource();
                ReceiveTask = new Task(DataReceivedHandler, cts.Token);
                ReceiveTask.Start();

                UpdateCommunicationTime();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"连接信息->工作站:{StationId},TCP:{_IpAddress},Port:{Port},连接失败，详细信息：{ex.Message}");
                IsConnected = false;
                return false;
            }
        }

        public void CloseSocket()
        {
            if (_Socket != null && _Socket.Connected)
            {
                try
                {
                    _Socket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException ex)
                {
                    // 记录或处理异常，但可能不需要抛出
                    Console.WriteLine($"关闭连接时发生错误: {ex.SocketErrorCode}");
                }
                finally
                {
                    _Socket.Close();
                }
            }
            else
            {
                // 如果未连接，直接关闭即可
                _Socket?.Close();
            }
        }

        public bool Connect()
        {
            lock (_syncLock)
            {
                if (IsConnected)
                {
                    return true;
                }

                return ConnectSocket();
                 
            }

        }
        public void Connect(IPAddress ipAddress, int port)
        {
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            // 这里的 "this" 指向当前实例，asyncCallback 是实例方法
            IAsyncResult result = _Socket.BeginConnect(remoteEP,
                new AsyncCallback(this.asyncCallback), _Socket);
        }

        // 实例方法
        private void asyncCallback(IAsyncResult ar)
        {
            // 回调处理逻辑
            try
            {
                _Socket.EndConnect(ar);
                IsConnected = true;
                Debug.WriteLine($"连接信息->工作站:{StationId},TCP:{_IpAddress},Port:{Port},连接成功");
            }
            catch (Exception ex)
            {
                IsConnected = false;
                Debug.WriteLine($"连接信息->工作站:{StationId},TCP:{_IpAddress},Port:{Port},连接失败，详细信息：{ex.Message}");
            }
        }

        public Task<bool> ConnectAsync()
        {
            return Task.Run(() => { return false; });
        }

        public void Disconnect()
        {
            //if (_Socket != null && _Socket.Connected)
            //{
            //    try
            //    {
            //        _Socket.Shutdown(SocketShutdown.Both);
            //    }
            //    catch (SocketException ex)
            //    {
            //        // 记录或处理异常，但可能不需要抛出
            //        Console.WriteLine($"关闭连接时发生错误: {ex.SocketErrorCode}");
            //    }
            //    finally
            //    {
            //        _Socket.Close();
            //    }
            //}
            //else
            //{
            //    // 如果未连接，直接关闭即可
            //    _Socket?.Close();
            //}
            lock (_syncLock)
            {
                if (!IsConnected) return;

                try
                {
                    cts.Cancel();
                    _Socket.Shutdown(SocketShutdown.Both);
                    _Socket.Close();
                    IsConnected = false;
                    Debug.WriteLine($"连接信息->工作站:{StationId},TCP:{_IpAddress},Port:{Port},断开连接");
                    Thread.Sleep(30); //为了断开线程
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine($"连接信息->工作站:{StationId},TCP:{_IpAddress},Port:{Port},断开失败，详细信息：{ex.Message}");
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
                    byte[] buffer = new byte[_Socket.ReceiveBufferSize];
                    var result = _Socket.Receive(buffer, buffer.Length, SocketFlags.None);
                    UpdateCommunicationTime();
                    return buffer;
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
                    byte[] sendBuffer = Encoding.UTF8.GetBytes(data);
                    _Socket.Send(sendBuffer);
                    UpdateCommunicationTime();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public void Send(string data)
        {
            if (_Socket == null || !_Socket.Connected)
            {
                return;
            }
            byte[] sendBuffer = Encoding.UTF8.GetBytes(data);
            _Socket.Send(sendBuffer); //int bytesSent = 
        }

        string old = "";
        private void DataReceivedHandler()
        {
            Debug.WriteLine($"连接信息->工作站:{StationId},TCP:{_IpAddress},Port:{Port},接收数据任务开启");
            while (true)
            {
                try
                {
                    // 检查取消请求
                    if (cts.Token.IsCancellationRequested) // 也可以使用传入的token
                    {
                        Debug.WriteLine($"连接信息->工作站:{StationId},TCP:{_IpAddress},Port:{Port},任务已被取消,正在退出...");

                        break; // 或 return
                    }
                    else
                    {
                        if (IsConnected)
                        {
                            byte[] buffer = new byte[1024 * 1024 * 2];
                            int r = _Socket.Receive(buffer); //实际收到的有效数字节数
                            if (r == 0) break;
                            string str = Encoding.UTF8.GetString(buffer, 0, r);

                            string outdata = old + str;
                            outdata = outdata.Replace("\r", "").Replace("\n", "");
                            if (!outdata.EndsWith("+"))
                            {
                                old = outdata;
                                continue;
                            }
                            else old = "";

                            //if (outdata.Length < 1) return;

                            DataReceive(outdata);
                        }
                    }
                    Thread.Sleep(50);
                }
                catch (Exception ex) when (ex.Message.Contains("一个封锁操作被对 WSACancelBlockingCall 的调用中断。"))
                {
                    Debug.WriteLine("接收TCP数据失败！详细信息：" + ex.Message);
                    Debug.WriteLine($"连接信息->工作站:{StationId},TCP:{_IpAddress},Port:{Port},任务强制退出中...");
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("接收TCP数据失败！详细信息：" + ex.Message);
                }
            }

            IsConnected = false;
            Debug.WriteLine($"连接信息->工作站:{StationId},TCP:{_IpAddress},Port:{Port},已退出...");
        }



        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

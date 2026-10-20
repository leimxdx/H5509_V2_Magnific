using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComMaster.Interfaces;

namespace H5509_V2
{
    /// <summary>
    /// 站点TCP配置（在 IEtherNetConfig 基础上增加方向字段）。
    /// Mode: "Client"(默认，上位机连测试机) / "Server"(上位机监听，测试机来连)。
    /// 老配置没有该字段，反序列化为null，按Client处理。
    /// 注意：不能修改 ComMaster 里的 IEtherNetConfig —— 本解决方案未给 ComMaster 配置生成映射，
    ///       改动 ComMaster 不会参与编译，H5509_V2 只会链接旧DLL。
    /// </summary>
    public class TcpSiteConfig : IEtherNetConfig
    {
        public string Mode { get; set; }
    }

    /// <summary>
    /// TCP服务端设备：上位机监听端口，由测试机作为客户端连上来。
    /// 帧处理与 ComMaster.Devices.EthernetDevice 保持一致：以 '+' 结尾聚包，整块交给 DataReceive。
    /// IsConnected 表示"监听已启动(就绪)"；是否真有测试机接入用 HasClient 表示。
    /// 一个端口对应一台测试机，对端断开后自动回到 Accept，无需外部重连。
    /// </summary>
    public sealed class TcpServerDevice : IEthernetDevice
    {
        #region 属性字段

        private TcpListener _listener;

        private TcpClient _client;

        private NetworkStream _stream;

        private readonly object _syncLock = new object();

        private bool _isDisposed;

        private CancellationTokenSource cts;

        public int StationId;

        public int Port { get; set; }

        public string DeviceId { get; set; }

        public string DeviceName { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// 监听已启动即视为就绪(服务端模式不允许因为"测试机还没连"就阻断开批)
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// 当前是否有测试机接入
        /// </summary>
        public bool HasClient
        {
            get
            {
                TcpClient client = _client;
                return client != null && client.Connected;
            }
        }

        public DateTime LastConnectionTime { get; set; }

        public Action<string> DataReceive { get; set; }

        public event EventHandler<ConnectionEventArgs> ConnectionChanged;

        #endregion

        public TcpServerDevice(int stationid, int port)
        {
            StationId = stationid;
            Port = port;
            DeviceName = $"Server:{port}";
            Connect();
        }

        #region 连接/断开

        public bool Connect()
        {
            lock (_syncLock)
            {
                if (IsConnected) return true;

                try
                {
                    cts = new CancellationTokenSource();
                    _listener = new TcpListener(IPAddress.Any, Port);
                    //端口可立即重用，保证切换配置后能马上重新监听
                    _listener.Server.SetSocketOption(
                        SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    _listener.Start();
                    IsConnected = true;
                    LastConnectionTime = DateTime.Now;

                    Task acceptTask = new Task(AcceptLoop, cts.Token);
                    acceptTask.Start();

                    Debug.WriteLine($"连接信息->工作站:{StationId},TCP服务端 Port:{Port},监听已启动");
                    return true;
                }
                catch (Exception ex)
                {
                    IsConnected = false;
                    Debug.WriteLine($"连接信息->工作站:{StationId},TCP服务端 Port:{Port},监听失败，详细信息：{ex.Message}");
                    return false;
                }
            }
        }

        public Task<bool> ConnectAsync()
        {
            return Task.Run(() => { return Connect(); });
        }

        public void Disconnect()
        {
            lock (_syncLock)
            {
                IsConnected = false;

                try { cts?.Cancel(); } catch (Exception) { }
                try { _listener?.Stop(); } catch (Exception) { }
                try { _client?.Close(); } catch (Exception) { }

                _client = null;
                _stream = null;

                Debug.WriteLine($"连接信息->工作站:{StationId},TCP服务端 Port:{Port},已停止监听");
            }
        }

        #endregion

        #region 发送

        public void Send(string data)
        {
            lock (_syncLock)
            {
                TcpClient client = _client;
                NetworkStream stream = _stream;

                if (client == null || stream == null || !client.Connected)
                {
                    Debug.WriteLine($"发送信息->工作站:{StationId},TCP服务端 Port:{Port},暂无测试机接入，指令被丢弃");
                    return;
                }

                try
                {
                    byte[] sendBuffer = Encoding.UTF8.GetBytes(data);
                    stream.Write(sendBuffer, 0, sendBuffer.Length);
                    stream.Flush();
                    LastConnectionTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"发送信息->工作站:{StationId},TCP服务端 Port:{Port},发送失败，详细信息：{ex.Message}");
                }
            }
        }

        #endregion

        #region 接收

        private void AcceptLoop()
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();

                    if (cts.Token.IsCancellationRequested)
                    {
                        CloseQuietly(client);
                        break;
                    }

                    lock (_syncLock)
                    {
                        //一个端口只对应一台测试机：新连接顶掉旧连接
                        CloseQuietly(_client);
                        _client = client;
                        _stream = client.GetStream();
                        LastConnectionTime = DateTime.Now;
                    }

                    Debug.WriteLine($"连接信息->工作站:{StationId},TCP服务端 Port:{Port},测试机已接入");
                    ConnectionChanged?.Invoke(this, new ConnectionEventArgs
                    {
                        StationId = StationId.ToString(),
                        IsConnected = true,
                        Message = $"测试机已接入({Port})"
                    });

                    //单连接：读循环阻塞在这里，对端断开后回到 Accept 继续等待
                    ReceiveLoop(client);
                }
                catch (Exception) when (cts.Token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"接收信息->工作站:{StationId},TCP服务端 Port:{Port},异常：{ex.Message}");
                    Thread.Sleep(200); //避免异常空转
                }
            }

            IsConnected = false;
            Debug.WriteLine($"连接信息->工作站:{StationId},TCP服务端 Port:{Port},接收任务已退出");
        }

        private void ReceiveLoop(TcpClient client)
        {
            string old = "";
            byte[] buffer = new byte[1024 * 64];

            try
            {
                NetworkStream stream = client.GetStream();

                while (!cts.Token.IsCancellationRequested)
                {
                    int r = stream.Read(buffer, 0, buffer.Length);
                    if (r <= 0) break; //对端关闭

                    LastConnectionTime = DateTime.Now;

                    string outdata = old + Encoding.UTF8.GetString(buffer, 0, r);
                    outdata = outdata.Replace("\r", "").Replace("\n", "");

                    if (!outdata.EndsWith("+"))
                    {
                        old = outdata;
                        continue;
                    }
                    else old = "";

                    DataReceive?.Invoke(outdata);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"接收信息->工作站:{StationId},TCP服务端 Port:{Port},读取失败，详细信息：{ex.Message}");
            }
            finally
            {
                CloseQuietly(client);

                lock (_syncLock)
                {
                    if (ReferenceEquals(_client, client))
                    {
                        _client = null;
                        _stream = null;
                    }
                }

                Debug.WriteLine($"连接信息->工作站:{StationId},TCP服务端 Port:{Port},测试机已断开");
                ConnectionChanged?.Invoke(this, new ConnectionEventArgs
                {
                    StationId = StationId.ToString(),
                    IsConnected = false,
                    Message = $"测试机已断开({Port})"
                });
            }
        }

        private void CloseQuietly(TcpClient client)
        {
            if (client == null) return;

            try { client.Close(); } catch (Exception) { }
        }

        #endregion

        #region 释放

        public void Dispose()
        {
            if (_isDisposed) return;

            Disconnect();

            _listener = null;
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

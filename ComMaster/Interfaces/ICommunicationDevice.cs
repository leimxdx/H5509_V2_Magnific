using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComMaster.Interfaces
{
    public interface ICommunicationDevice : IDisposable
    {
        /// <summary>
        /// 设备ID，唯一标识
        /// </summary>
        string DeviceId { get; }
        /// <summary>
        /// 设备名称
        /// </summary>
        string DeviceName { get; set; }
        /// <summary>
        /// 设备描述
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// 设备是否已连接
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// 最后一次通信时间
        /// </summary>
        DateTime LastConnectionTime { get; }
        /// <summary>
        /// 连接设备
        /// </summary>
        /// <returns></returns>
        bool Connect();
        /// <summary>
        /// 异步连接设备
        /// </summary>
        /// <returns></returns>
        Task<bool> ConnectAsync();
        /// <summary>
        /// 断开设备
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        void Send(string data);

        /// <summary>
        /// 数据接收
        /// </summary>
        Action<string> DataReceive { get; set; }
        /// <summary>
        /// 状态变更事件（用于通知界面）
        /// </summary>
        event EventHandler<ConnectionEventArgs> ConnectionChanged;

    }

    // 状态变更事件参数
    public class ConnectionEventArgs : EventArgs
    {
        public string StationId { get; set; } // 工位ID
        public bool IsConnected { get; set; }
        public string Message { get; set; } // 附加信息（如错误原因）
    }
}

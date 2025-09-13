using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicComMaster.Interfaces
{
    public interface  ICommunicationDevice : IDisposable
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
        /// 保存配置
        /// </summary>
        void SaveConfig();
        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        bool LoadConfig();
    }
}

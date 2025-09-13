using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicComMaster.Interfaces
{
    /// <summary>
    /// 以太网通信设备接口
    /// </summary>
    public interface IEthernetDevice : ICommunicationDevice
    {
        /// <summary>
        /// IP 地址
        /// </summary>
        string IpAddress  { get; set; }
        /// <summary>
        /// 端口号
        /// </summary>
        int Port { get; set; }
        /// <summary>
        /// 连接超时时间(毫秒)
        /// </summary>
        int ConnectionTimeout { get; set; }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns>是否发送成功</returns>
        bool SendData(byte[] data);
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns></returns>
        byte[] ReadData();
        /// <summary>
        /// 发送数据并接收数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="timeout"></param>
        /// <returns>接收的数据</returns>
        byte[] SendAndReceive(byte[] data,int timeout = 1000);
    }
}

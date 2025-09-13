using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicComMaster.Interfaces
{
    /// <summary>
    /// 串口通信设备接口
    /// </summary>
    public interface ISerialDevice : ICommunicationDevice
    {
        /// <summary>
        /// 串口名称
        /// </summary>
        string PortName { get; set; }
        /// <summary>
        /// 波特率
        /// </summary>
        int BaudRate { get; set; }
        /// <summary>
        /// 数据位
        /// </summary>
        int DataBits { get; set; }
        /// <summary>
        /// 停止位
        /// </summary>
        int StopBits { get; set; }
        /// <summary>
        /// 校验位
        /// </summary>
        string Parity { get; set; }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns>是否发送成功</returns>
        bool SendData(byte[] data);
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns>读取的数据</returns>
        byte[] ReadData();
        /// <summary>
        /// 发送数据并接收数据
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <param name="timeout">超时时间(毫秒)</param>
        /// <returns>接收的数据</returns>
        byte[] SendAndReceive(byte[] data, int timeout = 1000);
    }
}

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComMaster.Interfaces
{
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
        bool SendData(string data);
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


    // 1. 定义串口配置类
    public class SerialPortConfig
    {
        public string Protocol { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
    }
}

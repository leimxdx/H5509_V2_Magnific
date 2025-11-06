using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComMaster.Interfaces
{
    public interface IEthernetDevice : ICommunicationDevice
    {
    }

    // 1. 定义串口配置类
    public class IEtherNetConfig
    {
        public string Protocol { get; set; }
        public string PortName { get; set; } //IP地址
        public int Port { get; set; } //端口号
    }
}

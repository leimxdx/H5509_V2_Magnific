using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IHardwareGXN.Communication
{
    public interface ICommInterface : IDisposable
    {
        bool Connect(string portName, int baudRate); // 连接设备
        bool Disconnect();                           // 断开连接
        byte[] ReadData(int timeoutMs);              // 读取数据
        int WriteData(byte[] data);                  // 写入数据

        event EventHandler<string> DataReceived;     // 数据接收事件
    }
}

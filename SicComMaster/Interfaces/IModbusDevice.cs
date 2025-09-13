using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicComMaster.Interfaces
{
    /// <summary>
    /// Modbus通信设备接口
    /// </summary>
    public interface IModbusDevice : ICommunicationDevice
    {
        /// <summary>
        /// 站号
        /// </summary>
        byte SlaveAddress { get; set; }
        /// <summary>
        /// 读取线圈状态
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">长度</param>
        /// <returns>读取结果</returns>
        bool[] ReadCoils(string address, ushort length);
        /// <summary>
        /// 读取离散输入状态
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">长度</param>
        /// <returns>读取的结果</returns>
        bool[] ReadDiscreteInputs(string address, ushort length);
        /// <summary>
        /// 读取保持寄存器
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">长度</param>
        /// <returns>读取的结果</returns>
        ushort[] ReadHoldingRegisters(string address, ushort length);
        /// <summary>
        /// 读取输入寄存器
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">长度</param>
        /// <returns>读取的结果</returns>
        ushort[] ReadInputRegisters(string address, ushort length);
        /// <summary>
        /// 写入单个线圈
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns>是否写入成功</returns>
        bool WriteCoil(string address, bool value);
        /// <summary>
        /// 写多个线圈
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">值数组</param>
        /// <returns>是否写入成功</returns>
        bool WriteCoils(string address, bool[] values);
        /// <summary>
        /// 写单个保持寄存器
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns>是否写入成功</returns>
        bool WriteRegister(string address, ushort value);
        /// <summary>
        /// 写多个保持寄存器
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">值数组</param>
        /// <returns>是否写入成功</returns>
        bool WriteRegisters(string address, ushort[] values);

    }
}

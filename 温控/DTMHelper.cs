using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SiCommunication;
using SiCommunication.Core;
using SiCommunication.ModBus;

namespace H5509_V2
{
    public sealed class DTMHelper
    {
        private ModbusTcpNet modbusTcpNet;
        private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        /// <summary>
        /// 错误码映射表
        /// </summary>
        Dictionary<ushort, string> errorCodes = new Dictionary<ushort, string>()
        {
            { 0x8001, "EEPROM无法写入" },
            { 0x8002, "输入传感器断线或未接" },
            { 0x8003, "ADC读取失败" },
            { 0x8004, "内部通讯错误" },
            { 0x8005, "输入错误" },
            { 0x8006, "通道禁能" },
            { 0x8007, "输入数据未稳定" }
        };

        public DTMHelper()
        {

        }

        public DTMHelper(string ip, int port)
        {
            modbusTcpNet = new ModbusTcpNet(ip, port);
        }

        #region  通信连接 / 断开

        public bool Connect()
        {
            return modbusTcpNet.ConnectServer().IsSuccess;
        }

        public void Disconnect()
        {
            modbusTcpNet.ConnectClose();
        }

        #endregion

        #region  地址转换方法
        /// <summary>
        /// 将十六进制地址转换为十进制地址
        /// </summary>
        /// <param name="hexAddress"></param>
        /// <returns></returns>
        public static int HexAddressToDecimal(string hexAddress)
        {
            // 去除十六进制前缀"0x"或"0X"
            string hex = hexAddress.StartsWith("H", StringComparison.OrdinalIgnoreCase)
                ? hexAddress.Substring(1)
                : hexAddress;

            // 将处理后的字符串转换为十进制整数
            return int.Parse(hex, NumberStyles.HexNumber);
        }
        #endregion

        #region 通道使能/屏蔽
        /// <summary>
        /// 使能/屏蔽通道
        /// </summary>
        /// <param name="staID">站号</param>
        /// <param name="chanel">通道id</param>
        /// <param name="enable"></param>
        /// <returns></returns>
        public bool EnableOrDisableChanel(int staID, int chanel, bool enable)
        {
            rwLock.EnterWriteLock();
            try
            {
                string address = HexAddressToDecimal($"H{staID}258").ToString();
                var result = modbusTcpNet.Write($"{address}.{chanel}", enable);
                return result.IsSuccess;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 使能/屏蔽通道
        /// </summary>
        /// <param name="staID">站号</param>
        /// <param name="enable"></param>
        /// <returns></returns>
        public bool EnableOrDisableChanel(int staID, bool enable)
        {
            rwLock.EnterWriteLock();
            try
            {
                string address = HexAddressToDecimal($"H{staID}258").ToString();
                var result = modbusTcpNet.Write($"{address}", enable ? 0xFF : 0x00);
                return result.IsSuccess;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
        #endregion

        #region 通道执行/停止
        /// <summary>
        /// 通道执行/停止
        /// </summary>
        /// <param name="staID">站号</param>
        /// <param name="chanelId">通道号</param>
        /// <param name="enable"></param>
        /// <returns></returns>
        public bool StartOrStopChanle(int staID, int chanelId, bool enable)
        {
            rwLock.EnterWriteLock();
            try
            {
                string address = HexAddressToDecimal($"H{staID}24{(8 + chanelId).ToString("X")}").ToString();
                var result = modbusTcpNet.Write($"{address}", enable ? (ushort)1 : (ushort)0);
                return result.IsSuccess;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 通道执行/停止
        /// </summary>
        /// <param name="staID">站号</param>
        /// <param name="enable"></param>
        /// <returns></returns>
        public bool StartOrStopChanle(int staID, bool enable)
        {
            rwLock.EnterWriteLock();
            try
            {
                ushort[] data = new ushort[8];
                for (int i = 0; i < 8; i++)
                {
                    data[i] = enable ? (ushort)1 : (ushort)0;
                }

                string address = HexAddressToDecimal($"H{staID}248").ToString();
                var result = modbusTcpNet.Write($"{address}", data);
                return result.IsSuccess;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        #endregion

        #region 通道自整定
        /// <summary>
        /// 通道自整定
        /// </summary>
        /// <param name="staID">站号</param>
        /// <param name="chanelId">通道号</param>
        /// <param name="enable"></param>
        /// <returns></returns>
        public bool ChanleStartOrStopAutoPid(int staID, int chanelId, bool enable)
        {
            rwLock.EnterWriteLock();
            try
            {
                string address = HexAddressToDecimal($"H{staID}25{(0 + chanelId).ToString("X")}").ToString();
                var result = modbusTcpNet.Write($"{address}", enable ? (ushort)1 : (ushort)0);
                return result.IsSuccess;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 通道自整定
        /// </summary>
        /// <param name="staID">站号</param>
        /// <param name="enable"></param>
        /// <returns></returns>
        public bool ChanleStartOrStopAutoPid(int staID, bool enable)
        {
            rwLock.EnterWriteLock();
            try
            {
                ushort[] data = new ushort[8];
                for (int i = 0; i < 8; i++)
                {
                    data[i] = enable ? (ushort)1 : (ushort)0;
                }

                string address = HexAddressToDecimal($"H{staID}250").ToString();
                var result = modbusTcpNet.Write($"{address}", data);
                return result.IsSuccess;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        #endregion

        #region 通道读取PV值

        /// <summary>
        /// 通道读取PV值
        /// </summary>
        /// <param name="staID">站号</param>
        /// <param name="chanelId">通道号</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string ReadPv(int staID, int chanelId, out ushort[] value)
        {
            rwLock.EnterWriteLock();
            try
            {
                value = new ushort[8];
                string address = HexAddressToDecimal($"H{staID}26{(8 + chanelId).ToString("X")}").ToString();
                var result = modbusTcpNet.ReadUInt16(address, 1);
                if (result.IsSuccess)
                {
                    value = result.Content;
                    return "Success";
                }
                else
                {
                    return result.Message;
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 通道读取PV值
        /// </summary>
        /// <param name="staID">站号</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string ReadPv(int staID, out ushort[] value)
        {
            rwLock.EnterWriteLock();
            try
            {
                value = new ushort[8];
                string address = HexAddressToDecimal($"H{staID}268").ToString();
                var result = modbusTcpNet.ReadUInt16(address, 8);
                if (result.IsSuccess)
                {
                    value = result.Content;
                    return "Success";
                }
                else
                {
                    return result.Message;
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        #endregion

        #region 通道读写SV值

        /// <summary>
        /// 通道读取SV值
        /// </summary>
        /// <param name="staID">站号</param>
        /// <param name="chanelId">通道号</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string ReadSv(int staID, int chanelId, out short[] value)
        {
            rwLock.EnterWriteLock();
            try
            {
                value = new short[8];
                string address = HexAddressToDecimal($"H{staID}27{(0 + chanelId).ToString("X")}").ToString();
                var result = modbusTcpNet.ReadInt16(address, 1);
                if (result.IsSuccess)
                {
                    value = result.Content;
                    return "Success";
                }
                else
                {
                   return result.Message;
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
        public string ReadSv(int staID, out short[] value)
        {
            rwLock.EnterWriteLock();
            try
            {
                value = new short[8];
                string address = HexAddressToDecimal($"H{staID}270").ToString();
                var result = modbusTcpNet.ReadInt16(address, 8);
                if (result.IsSuccess)
                {
                    value = result.Content;
                    return "Success";
                }
                else
                {
                    return result.Message;
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public string WriteSvMax(int staID, int chanelId, double value)
        {
            rwLock.EnterWriteLock();
            try
            {
                string address = HexAddressToDecimal($"H{staID}00{8 + chanelId:X}").ToString();
                var result = modbusTcpNet.Write(address, (ushort)(value * 10));
                if (result.IsSuccess)
                {
                    return "Success";
                }
                else
                {
                    return result.Message;
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
        public string WriteSvMax(int staID, double value)
        {
            rwLock.EnterWriteLock();
            try
            {
                short[] values = new short[8];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (short)(value * 10);
                }
                string address = HexAddressToDecimal($"H{staID}008").ToString();
                var result = modbusTcpNet.Write(address, values);
                if (result.IsSuccess)
                {
                    return "Success";
                }
                else
                {
                    return result.Message;
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
        public string WriteSvMin(int staID, int chanelId, double value)
        {
            rwLock.EnterWriteLock();
            try
            {
                string address = HexAddressToDecimal($"H{staID}01{chanelId}").ToString();
                var result = modbusTcpNet.Write(address, (ushort)(value * 10));
                if (result.IsSuccess)
                {
                    return "Success";
                }
                else
                {
                    return result.Message;
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
        public string WriteSvMin(int staID, double value)
        {
            rwLock.EnterWriteLock();
            try
            {
                short[] values = new short[8];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (short)(value * 10);
                }
                string address = HexAddressToDecimal($"H{staID}010").ToString();
                var result = modbusTcpNet.Write(address, values);
                if (result.IsSuccess)
                {
                    return "Success";
                }
                else
                {
                    return result.Message;
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public string ReadSvMax(int staID, out short[] value)
        {
            rwLock.EnterWriteLock();
            try
            {
                value = new short[8];
                string address = HexAddressToDecimal($"H{staID}008").ToString();
                var result = modbusTcpNet.ReadInt16(address, 8);
                if (result.IsSuccess)
                {
                    value = result.Content;
                    return "Success";
                }
                else
                {
                    return result.Message;
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }


        public string ReadSvMin(int staID, out short[] value)
        {
            rwLock.EnterWriteLock();
            try
            {
                value = new short[8];
                string address = HexAddressToDecimal($"H{staID}010").ToString();
                var result = modbusTcpNet.ReadInt16(address, 8);
                if (result.IsSuccess)
                {
                    value = result.Content;
                    return "Success";
                }
                else
                {
                    return result.Message;
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public string WriteSv(int staID, int chanelId, double value)
        {
            rwLock.EnterWriteLock();
            try
            {
                string address = HexAddressToDecimal($"H{staID}00{chanelId}").ToString();
                var result = modbusTcpNet.Write(address, (ushort)(value * 10));
                if (result.IsSuccess)
                {
                    return "Success";
                }
                else
                {
                    return result.Message;
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public string WriteSv(int staID, double value)
        {
            rwLock.EnterWriteLock();
            try
            {
                short[] values = new short[8];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (short)(value * 10);
                }
                string address = HexAddressToDecimal($"H{staID}000").ToString();
                var result = modbusTcpNet.Write(address, values);
                if (result.IsSuccess)
                {
                    return "Success";
                }
                else
                {
                    return result.Message;
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        #endregion

        #region 通道同时读取PV&SV值
        /// <summary>
        /// 通道同时读取PV&SV值
        /// </summary>
        /// <param name="staID">站号</param>
        /// <param name="chanelId"></param>
        /// <returns></returns>
        public async Task<ushort[]> ReadPvAndSv(int staID, int chanelId)
        {
            rwLock.EnterWriteLock();
            try
            {
                string address = HexAddressToDecimal($"H{staID}26{(8 + chanelId).ToString("X")}").ToString();
                var result1 = await modbusTcpNet.ReadUInt16Async(address, 1);

                if (!result1.IsSuccess)
                {
                    return result1.Content;
                }

                address = HexAddressToDecimal($"H{staID}27{(0 + chanelId).ToString("X")}").ToString();
                var result2 = await modbusTcpNet.ReadUInt16Async(address, 1);
                if (!result2.IsSuccess)
                {
                    return result2.Content;
                }

                return new ushort[] { result1.Content[0], result2.Content[0] };
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public ushort[] ReadPvAndSv(int staID)
        {
            rwLock.EnterWriteLock();
            try
            {
                string address = HexAddressToDecimal($"H{staID}268").ToString();
                var result = modbusTcpNet.ReadUInt16(address, 16);
                if (result.IsSuccess)
                {
                    return result.Content;
                }
                else
                {
                    throw new Exception(result.ToString());
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }


        #endregion

        #region 获取通道状态
        /// <summary>
        /// 获取错误码
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public string GetErrorCode(ushort errorCode)
        {
            if (errorCode >= 0x8001 && errorCode <= 0x8007)
            {
                return errorCodes.ContainsKey(errorCode) ? errorCodes[errorCode] : "未知错误";
            }

            return string.Empty;
        }
        #endregion

    }
}

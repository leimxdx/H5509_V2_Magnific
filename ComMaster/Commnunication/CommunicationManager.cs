using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComMaster.Interfaces;

namespace ComMaster.Commnunication
{
    public sealed class CommunicationManager : IDisposable
    {
        #region 属性字段
        private bool _isDisposed;
        private string _configPath;
        private static CommunicationManager _instance;
        private static readonly object _lock = new object();

        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ICommunicationDevice>> _devices;

        #endregion

        #region 构造

        public static CommunicationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new CommunicationManager();
                    }
                }
                return _instance;
            }
        }

        private CommunicationManager()
        {
            _devices = new ConcurrentDictionary<string, ConcurrentDictionary<string, ICommunicationDevice>>();
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "Communications.json");


        }



        #endregion

        #region 共有方法
        /// <summary>
        /// 获取所有设备分组
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllGroups()
        {
            try
            {
                _rwLock.EnterReadLock();
                return _devices.Keys.ToList();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public List<ICommunicationDevice> GetCommunicationDevices(string groupName)
        {
            try
            {
                _rwLock.EnterReadLock();
                if (_devices.TryGetValue(groupName, out var devices))
                {
                    return devices.Values.ToList();
                }
                return new List<ICommunicationDevice>();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public ICommunicationDevice GetDevice(string groupName, string deviceId)
        {
            try
            {
                _rwLock.EnterReadLock();
                if (_devices.TryGetValue(groupName, out var devices)
                  && devices.TryGetValue(deviceId, out var device))
                {
                    return device;
                }
                return null;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public T GetDevice<T>(string groupName, string deviceId) where T : class, ICommunicationDevice
        {
            var device = GetDevice(groupName, deviceId);
            return device as T;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region  串口通信相关
        public ISerialDevice GetSerialDevice(string deviceId)
        {
            return GetDevice<ISerialDevice>("Serial", deviceId);
        }

        public ISerialDevice AddSerialDevice(string deviceId, string deviceName)
        {
            return null;
        }
        #endregion

        #region  TCP/IP 通信相关
        public IEthernetDevice GetEthernetDevice(string deviceId)
        {
            return GetDevice<IEthernetDevice>("Ethernet", deviceId);
        }
        #endregion

        #region Modbus通信相关
        public IModbusDevice GetModbusDevice(string deviceId)
        {
            return GetDevice<IModbusDevice>("Modbus", deviceId);
        }
        #endregion
    }
}

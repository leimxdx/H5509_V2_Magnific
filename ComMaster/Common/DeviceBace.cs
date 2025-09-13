using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ComMaster.Interfaces;

namespace ComMaster.Common
{
    public abstract class DeviceBase : ICommunicationDevice
    {
        #region 属性字段

        private bool _isDisposed;
        private string _configPath;
        private static readonly object _lock = new object();
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        public event EventHandler<ConnectionEventArgs> ConnectionChanged;
        public Action<string> DataReceive { get; set; }
        public string DeviceId { get; }

        public string DeviceName { get; set; }

        public string Description { get; set; }

        public abstract bool IsConnected { get; }

        public DateTime LastConnectionTime { get; protected set; }

        public string DeviceType { get; protected set; }

        #endregion

        #region 构造函数
        protected DeviceBase(string deviceId, string deviceName)
        {
            DeviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
            DeviceName = deviceName ?? throw new ArgumentNullException(nameof(deviceName));

            LastConnectionTime = DateTime.MinValue;
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "Devices");
        }

        #endregion

        #region 公有方法
        /// <summary>
        /// 连接设备
        /// </summary>
        /// <returns></returns>
        public abstract bool Connect();

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        public abstract void Send(string data);

        /// <summary>
        /// 异步连接
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> ConnectAsync()
        {
            return await Task.Run(() => Connect());
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// 保存配置
        /// </summary>
        public virtual void SaveConfig()
        {
            try
            {
                _rwLock.EnterWriteLock();

                //string filePath = GetConfigFilePath();
                //string jsonConfig = JsonConvert.SerializeObject(this, Formatting.Indented);

                //File.WriteAllText(filePath, jsonConfig);

            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        public bool LoadConfig()
        {
            try
            {
                _rwLock.EnterWriteLock();
                //string filePath = GetConfigFilePath();

                //if (!File.Exists(filePath)) return false;

                //string jsonConfig = File.ReadAllText(filePath);
                //var config = JsonConvert.DeserializeObject<DeviceBase>(jsonConfig);

                //if (config != null)
                //{
                //    CopyConfigProperties(config);
                //    return true;
                //}
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region 虚方法

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                if (IsConnected) Disconnect();
                // 释放托管资源
                _rwLock.Dispose();
            }
            // 释放非托管资源
            _isDisposed = true;
        }

        protected virtual string GetConfigFilePath()
        {
            return Path.Combine(_configPath, $"{DeviceType}_{DeviceId}.json");
        }

        protected abstract void CopyConfigProperties(DeviceBase source);

        protected void UpdateCommunicationTime()
        {
            LastConnectionTime = DateTime.Now;
        }

        #endregion

        #region 私有方法 - 检查配置路径
        /// <summary>
        /// 确保配置路径存在
        /// </summary>
        private void EnsureConfigPathExists()
        {
            if (!Directory.Exists(_configPath))
            {
                Directory.CreateDirectory(_configPath);
            }
        }
        #endregion
    }
}

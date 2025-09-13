using SicComMaster.Common;
using SicComMaster.Interfaces;
using SiCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicComMaster.Devices
{
    public sealed class EthernetDevice : DeviceBase, IEthernetDevice
    {
        #region 属性字段

        private NetworkAlienClient _client;
        private readonly object _syncLock = new object();
        private bool _isConnected;

        public string IpAddress { get; set; }

        public int Port { get; set; }

        public int ConnectionTimeout { get; set; }

        public bool KeepConnection { get; set; }

        public override bool IsConnected => throw new NotImplementedException();

        #endregion

        public EthernetDevice(string deviceId, string deviceName) : base(deviceId, deviceName)
        {
            DeviceType = "Client";
            IpAddress = "127.0.0.1";
            Port = 502;
            ConnectionTimeout = 3000;
            KeepConnection = true;

            //InitializeClient();
        }

        #region 共有方法

        public override bool Connect()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public byte[] ReadData()
        {
            throw new NotImplementedException();
        }

        public byte[] SendAndReceive(byte[] data, int timeout = 1000)
        {
            throw new NotImplementedException();
        }

        public bool SendData(byte[] data)
        {
            throw new NotImplementedException();
        }

        protected override void CopyConfigProperties(DeviceBase source)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}

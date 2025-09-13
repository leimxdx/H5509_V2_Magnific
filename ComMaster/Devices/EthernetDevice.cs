using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComMaster.Common;
using ComMaster.Interfaces;
using SiCommunication.Core.Net;

namespace ComMaster.Devices
{
    public sealed class EthernetDevice : DeviceBase, IEthernetDevice  //DeviceBase,
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


        public override bool Connect()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        protected override void CopyConfigProperties(DeviceBase source)
        {
            throw new NotImplementedException();
        }

        public override void Send(string data)
        {
            throw new NotImplementedException();
        }
    }
}

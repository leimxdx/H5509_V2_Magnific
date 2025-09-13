using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComMaster.Common;
using ComMaster.Interfaces;

namespace ComMaster.Devices
{
    public sealed class ModbusDevice : DeviceBase, IModbusDevice  //DeviceBase,
    {
        public ModbusDevice(string deviceId, string deviceName) : base(deviceId, deviceName)
        {
            DeviceType = "Modbus";
        }

        public override bool IsConnected => throw new NotImplementedException();

        public override bool Connect()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override void Send(string data)
        {
            throw new NotImplementedException();
        }

        protected override void CopyConfigProperties(DeviceBase source)
        {
            throw new NotImplementedException();
        }
    }
}

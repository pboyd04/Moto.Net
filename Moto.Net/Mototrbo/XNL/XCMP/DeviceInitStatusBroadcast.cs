using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public struct DeviceStatus
    {
        public byte DeviceType;
        public UInt16 Status;
        public Dictionary<byte, byte> Descriptor;
    }

    public class DeviceInitStatusBroadcast : XCMPPacket
    {
        protected byte majorVersion;
        protected byte minorVersion;
        protected byte revVersion;
        protected byte EntityType;
        protected byte initComplete;
        protected DeviceStatus Status;

        public DeviceInitStatusBroadcast(byte[] data) : base(data)
        {
            this.majorVersion = this.data[0];
            this.minorVersion = this.data[1];
            this.revVersion = this.data[2];
            this.EntityType = this.data[3];
            this.initComplete = this.data[4];
            if(this.initComplete == 0x00)
            {
                this.Status.DeviceType = data[5];
                this.Status.Status = (UInt16)(data[6] << 8 | data[7]);
                byte len = data[8];
                if(len > 0)
                {
                    this.Status.Descriptor = new Dictionary<byte, byte>();
                    for(int i = 0; i < len; i+=2)
                    {
                        this.Status.Descriptor[data[5 + i]] = data[5 + i + 1];
                    }
                }
            }
        }

        public string Version
        {
            get
            {
                return this.majorVersion + "." + this.minorVersion + "." + this.revVersion;
            }
        }

        public bool InitComplete
        {
            get
            {
                return (this.initComplete != 0x00);
            }
        }
    }
}

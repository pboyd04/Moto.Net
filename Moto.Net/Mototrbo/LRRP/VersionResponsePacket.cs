using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.LRRP
{
    public class VersionResponsePacket : LRRPPacket
    {
        protected byte version;

        public VersionResponsePacket() : base(LRRPPacketType.ProtocolVersionResponse)
        {
            this.version = 1;
        }

        public VersionResponsePacket(byte[] data) : base(data)
        {
            if(this.data[0] != 0x36)
            {
                throw new NotImplementedException(string.Format("Unknown Tag {0} in version response!", this.data[0]));
            }
            this.version = this.data[1];
        }

        public int Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = (byte)value;
            }
        }

        public override byte[] Encode()
        {
            this.data = new byte[2];
            this.data[0] = 0x36;
            this.data[1] = this.version;
            return base.Encode();
        }

        public override string ToString()
        {
            return base.ToString() + ", Version: " + this.version;
        }
    }
}

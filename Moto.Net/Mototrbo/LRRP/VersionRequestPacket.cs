using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.LRRP
{
    public class VersionRequestPacket : LRRPPacket
    {
        public VersionRequestPacket() : base(LRRPPacketType.ProtocolVersionRequest)
        {

        }

        public VersionRequestPacket(UInt32 requestID) : this()
        {
            this.requestID = requestID;
        }

        public VersionRequestPacket(byte[] data) : base(data)
        {

        }

        public override byte[] Encode()
        {
            this.data = new byte[2];
            this.data[0] = 0x3F; //The whole LRRP seems to be TLV based. I think this tag is for protocol version request and the next byte is the length (0)
            return base.Encode();
        }
    }
}

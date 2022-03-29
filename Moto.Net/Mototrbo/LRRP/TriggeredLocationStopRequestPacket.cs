using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.LRRP
{
    public class TriggeredLocationStopRequestPacket : LRRPPacket
    {
        public TriggeredLocationStopRequestPacket() : base(LRRPPacketType.TriggeredLocationStopRequest)
        {

        }

        public TriggeredLocationStopRequestPacket(byte[] data) : base(data)
        {

        }

        public TriggeredLocationStopRequestPacket(uint requestID) : this()
        {
            this.RequestID = requestID;
        }

        public override byte[] Encode()
        {
            this.data = new byte[0];
            return base.Encode();
        }
    }
}

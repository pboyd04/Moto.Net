using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.LRRP
{
    public class TriggeredLocationPacket : ImmediateLocationResponsePacket
    {
        public TriggeredLocationPacket() : base(LRRPPacketType.TriggeredLocationData)
        {

        }

        public TriggeredLocationPacket(byte[] data) : base(data)
        {

        }
    }
}

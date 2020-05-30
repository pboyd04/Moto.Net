using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.LRRP
{
    public class TriggeredLocatonStopResponsePacket : TriggeredLocationStartResponsePacket
    {
        public TriggeredLocatonStopResponsePacket() : base(LRRPPacketType.TriggeredLocationStopResponse)
        {

        }

        public TriggeredLocatonStopResponsePacket(byte[] data) : base(data)
        {

        }
    }
}

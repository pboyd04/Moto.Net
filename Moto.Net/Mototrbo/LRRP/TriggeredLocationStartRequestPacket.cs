using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.LRRP
{
    public class TriggeredLocationStartRequestPacket : ImmediateLocationRequestPacket
    {
        public TriggeredLocationStartRequestPacket() : base(LRRPPacketType.TriggeredLocationStartRequest)
        {
            this.triggerPeriodically = -1;
        }

        public TriggeredLocationStartRequestPacket(byte[] data) : base(data)
        {

        }

        public int TriggerPeriodically
        {
            get
            {
                return this.triggerPeriodically;
            }
            set
            {
                this.triggerPeriodically = value;
            }
        }
    }
}

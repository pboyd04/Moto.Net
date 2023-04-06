using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public enum TimeSlotStatus : byte
    {
        Transmitting = 0x01,
        Idle = 0x02
    }

    public class CallStatusNotificationPacket : Packet
    {
        private TimeSlotStatus slot1;
        private TimeSlotStatus slot2;

        public CallStatusNotificationPacket(RadioID id) : base(PacketType.CallStatusNotification)
        {
            this.id = id;
        }

        public CallStatusNotificationPacket(Byte[] data) : base(data)
        {
            this.slot1 = (TimeSlotStatus)this.data[0];
            this.slot2 = (TimeSlotStatus)this.data[1];
        }

        protected override string DataString()
        {
            return "{Slot 1: "+this.slot1+", Slot 2: "+this.slot2+"}";
        }

        public override byte[] Encode()
        {
            this.data = new byte[2];
            this.data[0] = (byte)TimeSlotStatus.Idle;
            this.data[1] = (byte)TimeSlotStatus.Idle;
            return base.Encode();
        }
    }
}

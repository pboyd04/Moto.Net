using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.LRRP
{
    public class TriggeredLocationStartResponsePacket : LRRPPacket
    {
        public int responseCode;

        protected TriggeredLocationStartResponsePacket(LRRPPacketType type) : base(type)
        {

        }

        public TriggeredLocationStartResponsePacket() : base(LRRPPacketType.TriggeredLocationStartResponse)
        {

        }

        public TriggeredLocationStartResponsePacket(byte[] data) : base(data)
        {
            if(this.data[0] == 0x37)
            {
                this.responseCode = data[1];
            }
            else if(this.data[0] == 0x38)
            {
                //This seems to imply success. 
                this.responseCode = 0;
            }
            else
            {
                throw new NotImplementedException("Unknown data byte "+ BitConverter.ToString(this.data));
            }
        }

        public int ResponseCode
        {
            get
            {
                return this.responseCode;
            }
            set
            {
                this.responseCode = value;
            }
        }

        public override string ToString()
        {
            return base.ToString()+" , Response Code: "+responseCode;
        }
    }
}

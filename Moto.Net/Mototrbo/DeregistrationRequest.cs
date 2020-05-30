using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public class DeregistrationRequest : Packet
    {
        public DeregistrationRequest(RadioID id) : base(PacketType.DeregisterRequest)
        {
            this.id = id;
        }

        protected override string DataString()
        {
            return "{}";
        }

        public override byte[] Encode()
        {
            this.data = new byte[0];
            return base.Encode();
        }
    }
}

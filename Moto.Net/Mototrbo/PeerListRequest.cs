using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public class PeerListRequest : Packet
    {
        public PeerListRequest(RadioID id) : base(PacketType.PeerListRequest)
        {
            this.id = id;
            this.data = new byte[0];
        }
    }
}

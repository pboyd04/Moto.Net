using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public class PeerKeepAliveReply : Packet
    {
        protected RadioSystemType systype;

        public PeerKeepAliveReply(RadioID id) : this(id, RadioSystemType.IPSiteConnect)
        {
        }

        public PeerKeepAliveReply(RadioID id, RadioSystemType type) : base(PacketType.PeerKeepAliveReply)
        {
            this.id = id;
            this.systype = type;
        }

        public override byte[] Encode()
        {
            this.data = new byte[6];
            this.data[0] = 0x65;
            this.data[4] = 0xa0;
            this.data[5] = 0x2c;
            return base.Encode();
        }
    }
}

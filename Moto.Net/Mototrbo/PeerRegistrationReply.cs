using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public class PeerRegistrationReply : Packet
    {
        protected RadioSystemType systype;

        public PeerRegistrationReply(RadioID id) : this(id, RadioSystemType.IPSiteConnect)
        {
        }

        public PeerRegistrationReply(RadioID id, RadioSystemType type) : base(PacketType.PeerRegisterReply)
        {
            this.id = id;
            this.systype = type;
        }

        public PeerRegistrationReply(byte[] data) : base(data)
        {
            this.systype = (RadioSystemType)this.data[0];
        }

        public override byte[] Encode()
        {
            this.data = new byte[4];
            this.data[0] = (byte)this.systype;
            this.data[1] = 0x06;
            this.data[2] = (byte)this.systype;
            return base.Encode();
        }
    }
}

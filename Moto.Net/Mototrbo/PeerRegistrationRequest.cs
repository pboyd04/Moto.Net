using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public class PeerRegistrationRequest : Packet
    {
        protected RadioSystemType systype;

        public PeerRegistrationRequest(RadioID id) : this(id, RadioSystemType.IPSiteConnect)
        {
        }

        public PeerRegistrationRequest(RadioID id, RadioSystemType type) : base(PacketType.PeerRegisterRequest)
        {
            this.id = id;
            this.systype = type;
        }

        public PeerRegistrationRequest(byte[] data) : base(data)
        {
            this.systype = (RadioSystemType)this.data[0];
        }

        protected override string DataString()
        {
            return "{}";
        }

        public override byte[] Encode()
        {
            this.data = new byte[4];
            this.data[0] = (byte)this.systype;
            this.data[1] = 0x06;
            this.data[2] = (byte)this.systype;
            return base.Encode();
        }

        public RadioSystemType SystemType
        {
            get
            {
                return this.systype;
            }
            set
            {
                this.systype = value;
            }
        }
    }
}

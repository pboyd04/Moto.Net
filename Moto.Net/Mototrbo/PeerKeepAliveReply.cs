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
        private readonly bool digital;
        private readonly RegistrationFlags flags;

        public PeerKeepAliveReply(RadioID id, RegistrationFlags flags) : this(id, RadioSystemType.IPSiteConnect, flags)
        {
        }

        public PeerKeepAliveReply(RadioID id, RadioSystemType type, RegistrationFlags flags) : base(PacketType.PeerKeepAliveReply)
        {
            this.digital = true;
            this.id = id;
            this.systype = type;
            this.flags = flags;
        }

        public override byte[] Encode()
        {
            this.data = new byte[5];
            this.data[0] = 0x45;
            if (this.digital)
            {
                this.data[0] |= 0x20;
            }
            else
            {
                this.data[0] |= 0x10;
            }
            byte[] bytes = BitConverter.GetBytes((UInt32)this.flags);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Array.Copy(bytes, 0, this.data, 1, 4);
            return base.Encode();
        }
    }
}

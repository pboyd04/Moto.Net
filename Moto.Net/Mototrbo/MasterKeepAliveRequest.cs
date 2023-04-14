using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public class MasterKeepAliveRequest : Packet
    {
        protected bool digital;
        protected RegistrationFlags flags;
        protected RadioSystemType systype;

        public MasterKeepAliveRequest(RadioID id, RadioSystemType type, RegistrationFlags flags) : base(PacketType.MasterKeepAliveRequest)
        {
            this.id = id;
            this.digital = true;
            this.flags = flags & ~RegistrationFlags.Software;
            this.systype = type;
        }

        public override byte[] Encode()
        {
            this.data = new byte[9];
            this.data[0] = 0x65;
            /*
            if (this.digital)
            {
                this.data[0] |= 0x20;
            }
            else
            {
                this.data[0] |= 0x10;
            }*/
            byte[] bytes = BitConverter.GetBytes((UInt32)this.flags);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Array.Copy(bytes, 0, this.data, 1, 4);
            this.data[5] = (byte)this.systype;
            this.data[6] = 0x06;
            this.data[7] = (byte)this.systype;
            return base.Encode();
        }

        protected override string DataString()
        {
            this.Encode();
            return "{Data: "+BitConverter.ToString(this.data)+"}";
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

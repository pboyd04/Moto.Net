using System;

namespace Moto.Net.Mototrbo
{
    public class MasterRegistrationRequest : Packet
    {
        protected bool digital;
        protected RegistrationFlags flags;
        protected RadioSystemType systype;

        public MasterRegistrationRequest(RadioID id, RadioSystemType type, RegistrationFlags flags) : base(PacketType.RegistrationRequest)
        {
            this.id = id;
            this.digital = true;
            this.flags = flags;
            this.systype = type;
        }

        protected override string DataString()
        {
            return "{}";
        }

        public override byte[] Encode()
        {
            this.data = new byte[9];
            this.data[0] = 0x45; //Still haven't fully parsed this byte. The least significant 2 nibbles are the channel slot 0 status, the next 2 are the channel slot 1 status
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
            this.data[5] = (byte)this.systype;
            this.data[6] = 0x06;
            this.data[7] = (byte)this.systype;
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

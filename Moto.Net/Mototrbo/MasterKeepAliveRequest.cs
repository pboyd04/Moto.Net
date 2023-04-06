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
            this.flags = flags;
            this.systype = type;
        }

        //This is how I think byte 4 breaks down...
        private enum byte4Mode : byte
        {
            VoiceCallSupport = 0x04,
            DataCallSupport = 0x08,
            SomethingElseThatIsRequired = 0x10,
            XNLDevice = 0x20,
        }

        public override byte[] Encode()
        {
            this.data = new byte[9];
            /*
            this.data[0] = 0x45;
            if(this.digital)
            {
                this.data[0] |= 0x20;
            }
            else
            {
                this.data[0] |= 0x10;
            }
            if(this.supportsCSBK)
            {
                this.data[3] |= 0x80;
            }
            this.data[3] |= 0x20;
            this.data[4] = 0x2C;
            */
            //this.data[0] = 0x65; //This reports 2 channels
            this.data[0] = 0x60;
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
            return "{}";
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

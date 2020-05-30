using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL
{
    public class DevAuthKeyReplyPacket : XNLPacket
    {
        private Address tempID;
        private byte[] authKey;

        public DevAuthKeyReplyPacket() : base(OpCode.DeviceAuthKeyReply)
        {

        }

        public DevAuthKeyReplyPacket(byte[] data) : base(data)
        {
            this.tempID = new Address(this.data);
            this.authKey = this.data.Skip(2).ToArray();
        }

        public Address TempID
        {
            get
            {
                return this.tempID;
            }
            set
            {
                this.tempID = value;
            }
        }

        public byte[] AuthKey
        {
            get
            {
                return this.authKey;
            }
            set
            {
                this.authKey = value;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL
{
    public class DevConnectionReplyPacket : XNLPacket
    {
        protected Address assignedID;
        protected byte[] authInfo;

        public DevConnectionReplyPacket() : base(OpCode.DeviceAuthKeyReply)
        {
        }

        public DevConnectionReplyPacket(Address dest, Address src, Address assignedID, byte[] authInfo) : base(OpCode.DeviceAuthKeyReply)
        {
            this.dest = dest;
            this.src = src;
            this.assignedID = assignedID;
            this.authInfo = authInfo;
            this.data = new byte[4+this.authInfo.Length];
            this.data[0] = 0x01;
            this.data[1] = 0x04;
            this.assignedID.AddToArray(this.data, 2);
            Array.Copy(this.authInfo, 0, this.data, 4, this.authInfo.Length);
        }

        public DevConnectionReplyPacket(byte[] data) : base(data)
        {
            this.assignedID = new Address(this.data, 2);
            this.authInfo = this.data.Skip(4).ToArray();
        }

        public Address AssignedID
        {
            get
            {
                return this.assignedID;
            }
        }
    }
}

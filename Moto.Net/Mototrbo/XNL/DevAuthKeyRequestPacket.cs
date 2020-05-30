using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL
{
    public class DevAuthKeyRequestPacket : XNLPacket
    {
        public DevAuthKeyRequestPacket(Address dest) : base(OpCode.DeviceAuthKeyRequest)
        {
            this.isXCMP = false;
            this.flags = 0;
            this.dest = dest;
            this.src = new Address(0);
            this.transactionID = 0;
            this.data = new byte[0];
        }
    }
}

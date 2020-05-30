using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL
{
    public class InitPacket : XNLPacket
    {
        public InitPacket() : base(OpCode.DeviceMasterQuery)
        {
            this.isXCMP = false;
            this.flags = 0;
            this.dest = new Address(0);
            this.src = new Address(0);
            this.transactionID = 0;
            this.data = new byte[0];
        }
    }
}

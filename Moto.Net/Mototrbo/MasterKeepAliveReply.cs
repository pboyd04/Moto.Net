using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public class MasterKeepAliveReply : Packet
    {
        private readonly bool digital;

        public MasterKeepAliveReply(byte[] data) : base(data)
        {
            digital = ((this.data[0] & 0x40) != 0);
        }
    }
}

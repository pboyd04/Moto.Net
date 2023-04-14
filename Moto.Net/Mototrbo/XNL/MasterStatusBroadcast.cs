using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL
{
    public class MasterStatusBroadcast : XNLPacket
    {
        private byte type;
        public MasterStatusBroadcast() : base(OpCode.MasterStatusBroadcast)
        {

        }

        public MasterStatusBroadcast(byte[] data) : base(data)
        {
            type = this.data[3];
        }

        public byte Type //This seems to be 1 for repeaters and 2 for regular radios
        {
            get
            {
                return type;
            }
        }
    }
}

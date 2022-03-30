using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class CloneReadReply : XCMPPacket
    {
        protected UInt16 zone;
        protected UInt16 channel;
        protected UInt16 dataType;

        public CloneReadReply() : base(XCMPOpCode.CloneReadReply)
        {

        }

        public CloneReadReply(byte[] data) : base(data)
        {
            //data[3-4] = 0x8001 - I think this basically asks for data about zone in the next word
            zone = (UInt16)(data[5] << 8 | data[6]);
            //data[7-8] = 0x8002 - I think this basically asks for data about channel in the next word
            channel = (UInt16)(data[9] << 8 | data[10]);
            dataType = (UInt16)(data[11] << 8 | data[12]);
            //Next two bytes are the length, just skip that... (would need if I were parsing responses with multiple data bits in them)
            this.data = data.Skip(15).ToArray();
        }
    }
}

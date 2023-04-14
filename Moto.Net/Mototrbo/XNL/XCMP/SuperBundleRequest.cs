using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class SuperBundleRequest : XCMPPacket
    {
        public SuperBundleRequest() : base(XCMPOpCode.CPS_SuperBundleRequest)
        {

        }

        public override byte[] Encode()
        {
            //The first byte is the number of messages being sent in this bundle
            //Then for each command in the bundle
            //  The first 2 bytes are the length of the message
            //  The opcode (counts against size)
            //  The message payload
            return base.Encode();
        }
    }
}

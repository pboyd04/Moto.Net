using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class TanapaNumberRequest : XCMPPacket
    {
        public TanapaNumberRequest() : base(XCMPOpCode.CPS_TanapaNumberRequest)
        {
            this.data = new byte[2]; //I don't know what goes here
        }

        public TanapaNumberRequest(byte[] data) : base(data)
        {
        }
    }
}

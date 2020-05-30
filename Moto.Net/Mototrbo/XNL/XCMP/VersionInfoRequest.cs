using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class VersionInfoRequest : XCMPPacket
    { 
        public VersionInfoRequest() : base(XCMPOpCode.VersionInfoRequest)
        {
            this.data = new byte[1];
            this.data[0] = 0x00;
        }

        public VersionInfoRequest(byte[] data) : base(data)
        {
        }
    }
}

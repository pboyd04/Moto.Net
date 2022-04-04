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
            this.data[0] = 0x00; //I think this is the type of firmware, like code, bootloader, codeplug, etc. Just leaving this on the regular firmware for now.
        }

        public VersionInfoRequest(byte[] data) : base(data)
        {
        }
    }
}

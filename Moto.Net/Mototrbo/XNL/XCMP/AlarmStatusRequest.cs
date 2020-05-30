using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class AlarmStatusRequest : XCMPPacket
    { 
        public AlarmStatusRequest() : base(XCMPOpCode.AlarmStatusRequest)
        {
            this.data = new byte[1];
            this.data[0] = 0x04;
        }

        public AlarmStatusRequest(byte[] data) : base(data)
        {
        }
    }
}

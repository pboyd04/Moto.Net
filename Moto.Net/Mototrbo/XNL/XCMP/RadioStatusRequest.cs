using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class RadioStatusRequest : XCMPPacket
    {
        protected XCMPStatus statusType;

        public RadioStatusRequest(XCMPStatus statusType) : base(XCMPOpCode.RadioStatusRequest)
        {
            this.statusType = statusType;
            this.data = new byte[1];
            this.data[0] = (byte)statusType;
        }


        public RadioStatusRequest(byte[] data) : base(data)
        {
            this.statusType = (XCMPStatus)data[3];
            this.data = data.Skip(4).ToArray();
        }
    }
}

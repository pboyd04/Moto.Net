using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class RadioStatusReply : XCMPPacket
    {
        protected XCMPStatus statusType;

        public RadioStatusReply(byte[] data) : base(data)
        {
            this.statusType = (XCMPStatus)data[3];
            this.data = data.Skip(4).ToArray();
        }

        public XCMPStatus StatusType
        {
            get
            {
                return this.statusType;
            }
        }
    }
}

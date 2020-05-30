using Moto.Net.Mototrbo.XNL.XCMP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL
{
    public class DataPacket : XNLPacket
    {
        protected XCMPPacket xcmp;

        public DataPacket() : base(OpCode.DataMessage)
        {

        }

        public DataPacket(byte[] data) : base(data)
        {
            if(this.isXCMP)
            {
                this.xcmp = XCMPPacket.Decode(this.data);
            }
        }

        public DataPacket(XCMPPacket pkt) : base(OpCode.DataMessage)
        {
            this.xcmp = pkt;
            this.isXCMP = true;
            this.data = pkt.Encode();
        }

        public XCMPPacket XCMP
        {
            get
            {
                return this.xcmp;
            }
        }
    }
}

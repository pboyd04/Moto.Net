using Moto.Net.Mototrbo.XNL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public class XNLXCMPPacket : Packet
    {
        protected XNLPacket xnl;

        public XNLXCMPPacket(byte[] data) : base(data)
        {
            this.xnl = XNLPacket.Decode(this.data);
        }

        public XNLXCMPPacket(RadioID id, XNLPacket pkt) : base(PacketType.XnlXCMPPacket)
        {
            this.id = id;
            this.data = pkt.Encode();
            this.xnl = pkt;
        }

        protected override string DataString()
        {
            return "{XNL: "+this.xnl+"}";
        }

        public XNLPacket XNLData
        {
            get
            {
                return this.xnl;
            }
        }
    }
}

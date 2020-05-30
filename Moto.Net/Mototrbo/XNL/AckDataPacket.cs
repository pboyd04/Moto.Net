using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL
{
    public class AckDataPacket : XNLPacket
    {

        public AckDataPacket() : base(OpCode.DataMessageAck)
        {

        }

        public AckDataPacket(byte[] data) : base(data)
        {
        }

        public AckDataPacket(XNLPacket pkt) : base(OpCode.DataMessageAck)
        {
            this.isXCMP = pkt.IsXCMP;
            this.flags = pkt.Flags;
            this.dest = pkt.Destination;
            this.src = pkt.Source;
            this.transactionID = pkt.TransactionID;
            this.data = new byte[0];
        }
    }
}

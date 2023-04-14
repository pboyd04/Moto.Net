using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class CloneReadRequest : XCMPPacket
    {
        public CloneReadRequest(UInt16 zone, UInt16 channel, byte dataType) : base(XCMPOpCode.CloneReadRequest)
        {
            //If I'm interpretting things right then RDAC asks for a couple of things at once. I'm just going to ask for one thing at once to simplify things for now
            this.data = new byte[10];
            this.data[0] = 0x80;
            this.data[1] = 0x01;
            this.data[2] = (byte)(zone >> 8);
            this.data[3] = (byte)zone;
            this.data[4] = 0x80;
            this.data[5] = 0x02;
            this.data[6] = (byte)(channel >> 8);
            this.data[7] = (byte)channel;
            this.data[8] = 0x00;
            this.data[9] = dataType; //0x0F is the channel name
        }

        public CloneReadRequest(UInt16 indexType, UInt16 index, UInt16 dataType) : base(XCMPOpCode.CloneReadRequest)
        {
            this.data = new byte[6];
            this.data[0] = (byte)(indexType >> 8);
            this.data[1] = (byte)indexType;
            this.data[2] = (byte)(index >> 8);
            this.data[3] = (byte)index;
            this.data[4] = (byte)(dataType >> 8);
            this.data[5] = (byte)dataType;
        }
    }
}

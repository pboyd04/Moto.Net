using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public enum ChannelSelectFunction
    {
        GetCurrentZoneNumber = 0x80, //Zone will contain the current zone id
        GetZoneCount = 0x81, //Zone will contain the number of zones (+1 in my config I think for the channel pool?)
        GetChannelCount = 0x82, //Channel will contain the channel count for the requested zone
    }

    public class ChannelSelectRequest : XCMPPacket
    {
        protected ChannelSelectFunction function; //0x80 seems to be basically a does this exist function... 0x82... not sure
        protected UInt16 zone;
        protected UInt16 channel;

        public ChannelSelectRequest(ChannelSelectFunction function, UInt16 zone, UInt16 channel) : base(XCMPOpCode.ChannelSelectRequest)
        {
            this.function = function;
            this.zone = zone;
            this.channel = channel;
            this.data = new byte[5];
            this.data[0] = (byte)function;
            this.data[1] = (byte)(zone >> 8);
            this.data[2] = (byte)(zone);
            this.data[3] = (byte)(channel >> 8);
            this.data[4] = (byte)(channel);
        }


        public ChannelSelectRequest(byte[] data) : base(data)
        {
            this.function = (ChannelSelectFunction)data[3];
            this.zone = (UInt16)(data[4] << 8 | data[5]);
            this.channel = (UInt16)(data[6] << 8 | data[7]);
        }
    }
}

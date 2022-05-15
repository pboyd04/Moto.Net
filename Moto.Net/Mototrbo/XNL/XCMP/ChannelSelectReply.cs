using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class ChannelSelectReply : XCMPReplyPacket
    {
        protected ChannelSelectFunction function;
        protected UInt16 zone;
        protected UInt16 channel;

        public ChannelSelectReply(byte[] data) : base(data)
        {
            this.function = (ChannelSelectFunction)data[3];
            this.zone = (UInt16)(data[4] << 8 | data[5]);
            this.channel = (UInt16)(data[6] << 8 | data[7]);
        }

        public ChannelSelectFunction Function
        {
            get
            {
                return this.function;
            }
        }

        public UInt16 Zone
        {
            get { return this.zone; }
        }

        public UInt16 Channel
        {
            get { return this.channel; }
        }
    }
}

using System;
using System.Linq;
using System.Text;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class TanapaNumberReply : XCMPReplyPacket
    {
        protected String number;

        public TanapaNumberReply(byte[] data) : base(data)
        {
            this.number = ASCIIEncoding.ASCII.GetString(data.Skip(3).ToArray());
        }

        public String String
        {
            get
            {
                return this.number;
            }
        }

        public override string ToString()
        {
            return base.ToString() + ": " + string.Join(",", this.data);
        }
    }
}

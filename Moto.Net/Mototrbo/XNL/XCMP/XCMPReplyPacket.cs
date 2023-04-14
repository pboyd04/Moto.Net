using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public enum XCMPErrorCode
    {
        Success = 0,
        ReInitXNL = 0x02,
        BadParams = 0x04,
        SecuredCommand = 0x06, //This seems to happen when you invoke certain CPS type commands
    }

    public class XCMPReplyPacket : XCMPPacket
    {
        protected XCMPErrorCode errorCode;

        public XCMPReplyPacket(XCMPOpCode op) : base(op)
        {
            errorCode = XCMPErrorCode.Success;
        }

        public XCMPReplyPacket(byte[] data) : base(data)
        {
            errorCode = (XCMPErrorCode)data[2];
        }

        public XCMPErrorCode ErrorCode
        {
            get { return errorCode; }
        }
    }
}

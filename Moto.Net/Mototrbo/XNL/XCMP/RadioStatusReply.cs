using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class RadioStatusReply : XCMPReplyPacket
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected XCMPStatus statusType;

        public RadioStatusReply(byte[] data) : base(data)
        {
            if(data.Length < 4)
            {
                log.ErrorFormat("Failed to parse reply {0}", BitConverter.ToString(data));
                return;
            }
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

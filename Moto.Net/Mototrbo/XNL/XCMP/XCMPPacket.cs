using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class XCMPPacket
    {
        protected XCMPOpCode opcode;
        protected byte[] data;

        public XCMPPacket(XCMPOpCode opCode)
        {
            this.opcode = opCode;
            this.data = new byte[0];
        }

        public XCMPPacket(byte[] data)
        {
            this.opcode = (XCMPOpCode)(data[0] << 8 | data[1]);
            this.data = data.Skip(2).ToArray();
        }

        public override string ToString()
        {
            return base.ToString() + ": {OpCode: " + this.opcode.ToString("X") + ", Data: " + BitConverter.ToString(this.data) + "}";
        }

        public static XCMPPacket Decode(byte[] data)
        {
            XCMPOpCode opcode = (XCMPOpCode)(data[0] << 8 | data[1]);
            switch(opcode)
            {
                case XCMPOpCode.DeviceinitStatusBroadcast:
                    return new DeviceInitStatusBroadcast(data);
                case XCMPOpCode.RadioStatusReply:
                    return new RadioStatusReply(data);
                case XCMPOpCode.VersionInfoReply:
                    return new VersionInfoReply(data);
                case XCMPOpCode.AlarmStatusReply:
                    return new AlarmStatusReply(data);
                default:
                    return new XCMPPacket(data);
            }
        }

        public byte[] Encode()
        {
            byte[] res = new byte[2 + this.data.Length];
            opcode.AddToArray(res, 0);
            Array.Copy(this.data, 0, res, 2, this.data.Length);
            return res;
        }

        public XCMPOpCode OpCode
        {
            get
            {
                return this.opcode;
            }
            set
            {
                this.opcode = value;
            }
        }

        public byte[] Data
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
            }
        }
    }
}

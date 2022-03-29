using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.LRRP
{
    public class LRRPPacket
    {
        protected LRRPPacketType type;
        protected UInt32 requestID;
        protected byte[] data;

        public LRRPPacket(LRRPPacketType type)
        {
            this.type = type;
        }

        public LRRPPacket(byte[] data)
        {
            //I'm not sure what it means when the 0x80 bit is set... just ignore it till I figure it out.
            this.type = (LRRPPacketType)(data[0] & 0x7F);
            //data[1] is the packet size...
            //This next bit seems to be a TLV, not sure what other tags are valid...
            if (data[2] != 0x22)
            {
                throw new NotImplementedException(string.Format("Unknown tag byte {0} ({1})", data[2], BitConverter.ToString(data)));
            }
            int offset;
            switch (data[3] & 0x0F)
            {
                case 1:
                    this.requestID = data[4];
                    offset = 5;
                    break;
                case 2:
                    this.requestID = (UInt32)(data[4] << 8 | data[5]);
                    offset = 6;
                    break;
                case 3:
                    this.requestID = (UInt32)(data[4] << 16 | data[5] << 8 | data[6]);
                    offset = 7;
                    break;
                case 4:
                    this.requestID = (UInt32)(data[4] << 24 | data[5] << 16 | data[6] << 8 | data[7]);
                    offset = 8;
                    break;
                default:
                    throw new NotImplementedException(string.Format("Unknown length byte {0}", data[3]));

            }
            if (data[1] + 2 < data.Length)
            {
                //When I get this back from the repeater it's got a bunch of extra junk on the end.
                this.data = new byte[data[1] - (offset - 2)];
                Array.Copy(data, offset, this.data, 0, this.data.Length);
            }
            else
            {
                this.data = data.Skip(offset).ToArray();
            }
            
        }

        public static LRRPPacket Decode(byte[] data)
        {
            try
            {
                switch ((LRRPPacketType)(data[0] & 0x7F))
                {
                    case LRRPPacketType.ImmediateLocationRequest:
                        return new ImmediateLocationRequestPacket(data);
                    case LRRPPacketType.ImmediateLocationResponse:
                        return new ImmediateLocationResponsePacket(data);
                    case LRRPPacketType.ProtocolVersionRequest:
                        return new VersionRequestPacket(data);
                    case LRRPPacketType.ProtocolVersionResponse:
                        return new VersionResponsePacket(data);
                    case LRRPPacketType.TriggeredLocationData:
                        return new TriggeredLocationPacket(data);
                    case LRRPPacketType.TriggeredLocationStartRequest:
                        return new TriggeredLocationStartRequestPacket(data);
                    case LRRPPacketType.TriggeredLocationStartResponse:
                        return new TriggeredLocationStartResponsePacket(data);
                    case LRRPPacketType.TriggeredLocationStopResponse:
                        return new TriggeredLocatonStopResponsePacket(data);
                    default:
                        return new LRRPPacket(data);
                }
            } catch(Exception ex)
            {
                StreamWriter tw = File.AppendText("E:\\RadioCalls\\Exceptions.txt");
                tw.WriteLine(BitConverter.ToString(data));
                tw.WriteLine(ex);
                tw.Close();
                Console.WriteLine(ex);
                return null;
            }
        }

        public virtual byte[] Encode()
        {
            int reqLength = 4;
            if(this.requestID <= 0xFFFFFF)
            {
                //reqLength = 3;
            }
            byte[] ret = new byte[this.data.Length + 4 + reqLength];
            ret[0] = (byte)this.type;
            ret[1] = (byte)(ret.Length - 2);
            ret[2] = 0x22;
            ret[3] = (byte)reqLength;
            if (reqLength == 4)
            {
                ret[4] = (byte)(this.requestID >> 24);
                ret[5] = (byte)(this.requestID >> 16);
                ret[6] = (byte)(this.requestID >> 8);
                ret[7] = (byte)(this.requestID);
                Array.Copy(this.data, 0, ret, 8, this.data.Length);
            }
            else
            {
                ret[4] = (byte)(this.requestID >> 16);
                ret[5] = (byte)(this.requestID >> 8);
                ret[6] = (byte)(this.requestID);
                Array.Copy(this.data, 0, ret, 7, this.data.Length);
            }
            return ret;
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(" PacketType: {0}, RequestID: {1}", this.type, this.requestID);
        }

        public UInt32 RequestID
        {
            get
            {
                return this.requestID;
            }
            set
            {
                this.requestID = value;
            }
        }

        public LRRPPacketType Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
    }
}

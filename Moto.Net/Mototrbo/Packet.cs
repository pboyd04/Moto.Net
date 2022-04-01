using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public class Packet
    {
        protected PacketType type;
        protected RadioID id;
        protected Byte[] data;

        public Packet(PacketType type)
        {
            this.type = type;
            this.data = new byte[0];
        }

        public Packet(Byte[] data)
        {
            this.type = (PacketType)data[0];
            this.id = new RadioID(data, 1);
            this.data = data.Skip(5).ToArray();
        }

        public static Packet Decode(Byte[] data)
        {
            PacketType type = (PacketType)data[0];
            switch(type)
            {
                case PacketType.XnlXCMPPacket:
                    return new XNLXCMPPacket(data);
                case PacketType.RegistrationReply:
                    return new MasterRegistrationReply(data);
                case PacketType.PeerRegisterRequest:
                    return new PeerRegistrationRequest(data);
                case PacketType.PeerRegisterReply:
                    return new PeerRegistrationReply(data);
                case PacketType.PeerListReply:
                    return new PeerListReply(data);
                case PacketType.GroupDataCall:
                case PacketType.GroupVoiceCall:
                case PacketType.PrivateDataCall:
                case PacketType.PrivateVoiceCall:
                    return new UserPacket(data);
                default:
                    return new Packet(data);
            }
        }

        public virtual byte[] Encode()
        {
            byte[] res = new byte[5 + this.data.Length];
            res[0] = (byte)this.type;
            this.id.AddToArray(res, 1, 4);
            Array.Copy(this.data, 0, res, 5, this.data.Length);
            return res;
        }

        protected virtual string DataString()
        {
            return "["+string.Join(",", this.data)+"]";
        }

        public override string ToString()
        {
            string ret = base.ToString() + ": {PacketType: "+this.type+", ID: "+this.id+", Data: ";
            ret += this.DataString();
            return ret+"}";
        }

        public PacketType PacketType
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

        public RadioID ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }
    }
}

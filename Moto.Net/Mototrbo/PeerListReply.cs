using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public struct Peer
    {
        public RadioID ID;
        public string Address;
        public UInt16 Port;
        public byte Mode;
    }

    public class PeerListReply : Packet
    {
        public Peer[] peers;

        public PeerListReply() : base(PacketType.PeerListReply)
        {

        }

        public PeerListReply(byte[] data) : base(data)
        {
            UInt16 length = (UInt16)(data[5] << 8 | data[6]);
            int count = length / 11;
            this.peers = new Peer[count];
            for(int i = 0; i < this.peers.Length; i++)
            {
                this.peers[i].ID = new RadioID(data, 7 + (i * 11));
                this.peers[i].Address = data[11 + (i * 11)] + "." + data[12 + (i * 11)] + "." + data[13 + (i * 11)] + "." + data[14 + (i * 11)];
                this.peers[i].Port = (UInt16)(data[15 + (i * 11)] << 8 | data[16 + (i * 11)]);
                this.peers[i].Mode = data[17 + (i * 11)];
            }
        }

        public Peer[] Peers
        {
            get
            {
                return this.peers;
            }
        }
    }
}

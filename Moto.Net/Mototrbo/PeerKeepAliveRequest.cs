﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public class PeerKeepAliveRequest : Packet
    {
        protected bool digital;
        protected RegistrationFlags flags;

        public PeerKeepAliveRequest(RadioID id, bool digital, RegistrationFlags flags) : base(PacketType.PeerKeepAliveRequest)
        {
            this.id = id;
            this.digital = digital;
            this.flags = flags;
        }

        public override byte[] Encode()
        {
            this.data = new byte[5];
            this.data[0] = 0x65;
            if (this.digital)
            {
                this.data[0] |= 0x20;
            }
            else
            {
                this.data[0] |= 0x10;
            }
            byte[] bytes = BitConverter.GetBytes((UInt32)this.flags);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Array.Copy(bytes, 0, this.data, 1, 4);
            return base.Encode();
        }
    }
}

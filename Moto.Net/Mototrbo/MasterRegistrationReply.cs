using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moto.Net;

namespace Moto.Net.Mototrbo
{
    public class MasterRegistrationReply : Packet
    {
        protected bool digital;
        protected bool supportsCSBK;
        protected UInt16 peerCount;
        protected RadioSystemType masterSystemType;
        protected byte masterVersion;
        protected RadioSystemType requestedSystemType;
        protected byte requestedVersion;

        public MasterRegistrationReply() : base(PacketType.RegistrationReply)
        {

        }

        public MasterRegistrationReply(Byte[] data) : base(data)
        {
            this.digital = (this.data[0] & 0x20) != 0;
            this.supportsCSBK = (this.data[3] & 0x80) != 0;
            this.peerCount = (UInt16)(this.data[5] << 8 | this.data[6]);
            this.masterSystemType = (RadioSystemType)this.data[7];
            this.MasterVersion = this.data[8];
            this.requestedSystemType = (RadioSystemType)this.data[9];
            this.requestedVersion = this.data[10];
        }

        protected override String DataString()
        {
            return "{Unknown1: " + (this.data[0] | ~0x20) + ", Digital: " + this.digital + ", Unknown2: [" + this.data[1] + "," + this.data[2] + "," + (this.data[3] | ~0x80) + "," + this.data[4] +
                "], CSBK: " + this.supportsCSBK + ", Peer Count: "+this.peerCount+", Master System Type: " + this.masterSystemType + ", MasterVersion: " + this.masterVersion;
        }

        public bool Digital
        {
            get
            {
                return this.digital;
            }
            set
            {
                this.digital = value;
            }

        }
         
        public bool SupportsCSBK
        {
            get
            {
                return this.supportsCSBK;
            }
            set
            {
                this.supportsCSBK = value;
            }
        }

        public UInt16 PeerCount
        {
            get
            {
                return this.peerCount;
            }
            set
            {
                this.peerCount = value;
            }
        }
        
        public RadioSystemType MasterSystemType
        {
            get
            {
                return this.masterSystemType;
            }
            set
            {
                this.masterSystemType = value;
            }
        }

        public byte MasterVersion
        {
            get
            {
                return this.masterVersion;
            }
            set
            {
                this.masterVersion = value;
            }
        }

        public RadioSystemType RequestedSystemType
        {
            get
            {
                return this.requestedSystemType;
            }
            set
            {
                this.requestedSystemType = value;
            }
        }

        public byte RequestedVersion
        {
            get
            {
                return this.requestedVersion;
            }
            set
            {
                this.requestedVersion = value;
            }
        }
    }
}

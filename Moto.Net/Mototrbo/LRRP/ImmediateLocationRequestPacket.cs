using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.LRRP
{
    public class ImmediateLocationRequestPacket : LRRPPacket
    {
        protected bool requestAccuracy;
        protected bool requestAltitude;
        protected bool requestHorizontalDirection;
        protected bool requestHorizontalSpeed;
        protected bool requestTime;
        //These are more useful on Triggered requests... but that is basically the same opp so here they are...
        protected int triggerPeriodically;
        protected int triggerOnMove;
        protected bool triggerOnGpio;

        protected ImmediateLocationRequestPacket(LRRPPacketType type) : base(type)
        {

        }

        public ImmediateLocationRequestPacket() : base(LRRPPacketType.ImmediateLocationRequest)
        {

        }

        public ImmediateLocationRequestPacket(UInt32 requestID) : this()
        {
            this.RequestID = requestID;
        }

        public ImmediateLocationRequestPacket(byte[] data) : base(data)
        {
        }

        public bool RequestAccuracy
        {
            get
            {
                return this.requestAccuracy;
            }
            set
            {
                this.requestAccuracy = value;
            }
        }

        public bool RequestAltitude
        {
            get
            {
                return this.requestAltitude;
            }
            set
            {
                this.requestAltitude = value;
            }
        }

        public bool RequestHorizontalDirection
        {
            get
            {
                return this.requestHorizontalDirection;
            }
            set
            {
                this.requestHorizontalDirection = value;
            }
        }

        public bool RequestHorizontalSpeed
        {
            get
            {
                return this.requestHorizontalSpeed;
            }
            set
            {
                this.requestHorizontalSpeed = value;
            }
        }

        public bool RequestTime
        {
            get
            {
                return this.requestTime;
            }
            set
            {
                this.requestTime = value;
            }
        }

        public override byte[] Encode()
        {
            MemoryStream ms = new MemoryStream();
            if(this.triggerPeriodically == -1)
            {
                ms.WriteByte(0x34);
            }
            else if(this.triggerPeriodically != 0)
            {
                ms.WriteByte(0x34);
                ms.WriteByte(0x31);
                ms.WriteByte((byte)this.triggerPeriodically);
            }
            if(this.triggerOnMove != 0)
            {
                ms.WriteByte(0x34);
                ms.WriteByte(0x78);
                ms.WriteByte((byte)this.triggerOnMove);
            }
            if(this.triggerOnGpio)
            {
                ms.WriteByte(0x42);
            }
            if(this.requestAccuracy && this.requestTime)
            {
                ms.WriteByte(0x51);
            }
            else if(this.requestAccuracy)
            {
                ms.WriteByte(0x50);
            }
            else if(this.requestTime)
            {
                ms.WriteByte(0x52);
            }
            if(this.requestAltitude)
            {
                ms.WriteByte(0x54);
            }
            if(this.requestHorizontalDirection)
            {
                ms.WriteByte(0x57);
            }
            this.data = ms.ToArray();
            return base.Encode();
        }
    }
}

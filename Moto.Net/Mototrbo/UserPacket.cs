﻿using Moto.Net.Mototrbo.Bursts;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public class RTPData
    {
        private static UInt16 gSequenceNumber = 100;

        private readonly byte version;
        private readonly bool padding;
        private readonly bool extension;
        private readonly byte csrcCount;
        private readonly bool marker;
        //94 seems to be CSBK, 94 seems to be a call
        private readonly byte payloadType;
        private readonly UInt16 sequenceNumber;
        private readonly UInt32 timestamp;
        private readonly UInt32 ssrcId;

        public RTPData(byte PayloadType)
        {
            this.version = 2;
            this.padding = false;
            this.extension = false;
            this.csrcCount = 0;
            this.marker = false;
            this.payloadType = PayloadType;
            this.sequenceNumber = gSequenceNumber++;
            this.timestamp = (UInt32)(DateTime.Now.Ticks);
            this.ssrcId = 0;
        }

        public RTPData(byte[] data, int offset)
        {
            this.version = (byte)(data[offset] >> 6);
            this.padding = ((data[offset] & 0x20) != 0);
            this.extension = ((data[offset] & 0x10) != 0);
            this.csrcCount = (byte)(data[offset] & 0x0F);
            this.marker = ((data[offset+1] & 0x80) != 0);
            this.payloadType = (byte)(data[offset+1] & 0x7F);
            this.sequenceNumber = (UInt16)(data[offset+2] << 8 | data[offset+3]);
            this.timestamp = (UInt32)(data[offset+4] << 24 | data[offset+5] << 16 | data[offset+6] << 8 | data[offset+7]);
            this.ssrcId = (UInt32)(data[offset+8] << 24 | data[offset+9] << 16 | data[offset+10] << 8 | data[offset+11]);
        }

        public byte[] Encode()
        {
            byte[] ret = new byte[12];
            ret[0] = (byte)((this.version << 6) | this.csrcCount);
            if(this.padding)
            {
                ret[0] |= 0x20;
            }
            if(this.extension)
            {
                ret[0] |= 0x10;
            }
            ret[1] = this.payloadType;
            if(this.marker)
            {
                ret[1] |= 0x80;
            }
            ret[2] = (byte)(this.sequenceNumber >> 8);
            ret[3] = (byte)this.sequenceNumber;
            ret[4] = (byte)(this.timestamp >> 24);
            ret[5] = (byte)(this.timestamp >> 16);
            ret[6] = (byte)(this.timestamp >> 8);
            ret[7] = (byte)(this.timestamp);
            ret[8] = (byte)(this.ssrcId >> 24);
            ret[9] = (byte)(this.ssrcId >> 16);
            ret[10] = (byte)(this.ssrcId >> 8);
            ret[11] = (byte)(this.ssrcId);
            return ret;
        }

        public override string ToString()
        {
            return base.ToString() + string.Format("Version: {0}, Padding: {1}, Extension: {2}, CSRCCount {3}, Marker: {4}, PayloadType: {5}, Sequence Number: {6}, Timestamp: {7}, SSRCID: {8}", this.version, this.padding, this.Extension, this.csrcCount, this.marker, this.payloadType, this.sequenceNumber, this.timestamp, this.ssrcId);
        }

        public UInt16 SequenceNumber
        {
            get
            {
                return this.sequenceNumber;
            }
        }

        public bool Extension
        {
            get
            {
                return this.extension;
            }
        }
    }

    public class UserPacket : Packet
    {
        protected RadioID src;
        protected RadioID dest;
        protected byte calltype; //Seems to normally be 0, but 1 when sending data
        protected UInt32 groupTag;
        protected bool encrypted;
        protected bool end;
        protected bool timeslot;
        protected bool phone;
        protected RTPData rtp;
        protected Burst burst;

        public UserPacket(byte[] data) : base(data)
        {
            this.src = new RadioID(data, 6, 3);
            this.dest = new RadioID(data, 9, 3);
            this.calltype = data[12];
            //This seems to be some kind of group tag to help group packets together
            this.groupTag = (UInt32)((data[13] << 24) | (data[14] << 16) | (data[15] << 8) | data[16]);
            this.encrypted = ((data[17] & 0x80) != 0);
            this.end = ((data[17] & 0x40) != 0);
            this.timeslot = ((data[17] & 0x20) != 0);
            this.phone = ((data[17] & 0x10) != 0);
            //RTP Data...
            this.rtp = new RTPData(data, 18);
            //Burst data...
            if (this.rtp.Extension)
            {
                throw new PacketParsingException("Have a header extension! Don't know how to process packet!");
            }
            else
            {
                this.burst = Burst.Decode(data.Skip(30).ToArray());
            }
        }

        public UserPacket(bool data, bool group) : base(PacketType.GroupVoiceCall)
        {
            if (data == true)
            {
                if(group == false)
                {
                    this.type = PacketType.PrivateDataCall;
                }
                else
                {
                    this.type = PacketType.GroupDataCall;
                }
            }
            else if(group == false)
            {
                this.type = PacketType.PrivateVoiceCall;
            }
            this.rtp = new RTPData(93);
        }

        public UserPacket(RadioID id, bool data, bool group, RadioID source, RadioID target, bool encrypted, bool phone, UInt32 groupTag, Burst b) : this(data, group)
        {
            this.id = id;
            this.src = source;
            this.dest = target;
            this.encrypted = encrypted;
            this.phone = phone;
            this.groupTag = groupTag;
            this.burst = b;
        }

        public override byte[] Encode()
        {
            byte[] burstdata = this.burst.Encode();
            this.data = new byte[25 + burstdata.Length];
            this.src.AddToArray(this.data, 1, 3);
            this.dest.AddToArray(this.data, 4, 3);
            data[7] = this.calltype;
            data[8] = (byte)(groupTag >> 24);
            data[9] = (byte)(groupTag >> 16);
            data[10] = (byte)(groupTag >> 8);
            data[11] = (byte)(groupTag);
            if (this.encrypted)
            {
                this.data[12] |= 0x80;
            }
            if (this.end)
            {
                this.data[12] |= 0x40;
            }
            if(this.timeslot)
            {
                this.data[12] |= 0x20;
            }
            if(this.phone)
            {
                this.data[12] |= 0x10;
            }
            byte[] rtpdata = this.rtp.Encode();
            Array.Copy(rtpdata, 0, this.data, 13, rtpdata.Length);
            Array.Copy(burstdata, 0, this.data, 25, burstdata.Length);
            return base.Encode();
        }

        public RadioID Source
        {
            get
            {
                return this.src;
            }
            set
            {
                this.src = value;
            }
        }

        public RadioID Destination
        {
            get
            {
                return this.dest;
            }
            set
            {
                this.dest = value;
            }
        }

        public byte CallType
        {
            get
            {
                return this.calltype;
            }
            set
            {
                this.calltype = value;
            }
        }

        public UInt32 GroupTag
        {
            get
            {
                return this.groupTag;
            }
            set
            {
                this.groupTag = value;
            }
        }

        public Burst Burst
        {
            get
            {
                return this.burst;
            }
            set
            {
                this.burst = value;
            }
        }

        public bool Encrypted
        {
            get
            {
                return this.encrypted;
            }
            set
            {
                this.encrypted = value;
            }
        }

        public bool End
        {
            get
            {
                return this.end;
            }
            set
            {
                this.end = value;
            }
        }

        public bool PhoneCall
        {
            get
            {
                return this.phone;
            }
            set
            {
                this.phone = value;
            }
        }

        protected int TimeSlot
        {
            get
            {
                if(this.timeslot)
                {
                    return 2;
                }
                return 1;
            }
            set
            {
                switch(value)
                {
                    case 1:
                        this.timeslot = false;
                        break;
                    case 2:
                        this.timeslot = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Value " + value + " this not a valid timeslot");
                }
            }
        }

        public RTPData RTP
        {
            get
            {
                return this.rtp;
            }
            set
            {
                this.rtp = value;
            }
        }
    }
}
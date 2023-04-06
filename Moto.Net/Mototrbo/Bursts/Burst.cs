using Moto.Net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.Bursts
{
    public class Burst
    {
        protected bool slot;
        protected DataType type;
        protected bool rssiOk;
        protected bool rsParity;
        protected bool crcParity;
        protected bool lcParity;
        protected bool hasRSSI;
        protected bool burstSource;
        protected bool hardSync;
        protected bool hasSlotType;
        protected byte syncType;
        protected byte colorCode;
        protected byte slotType;
        protected float rssi;
        protected byte[] data;
        protected UInt16 crc;

        protected Burst(DataType type)
        {
            this.type = type;
        }

        public Burst(byte[] data)
        {
            this.slot = ((data[0] & 0x80) != 0);
            this.type = (DataType)(data[0] & 0x7f);
            this.rssiOk = ((data[1] & 0x40) != 0);
            this.rsParity = ((data[1] & 0x04) != 0);
            this.crcParity = ((data[1] & 0x02) != 0);
            this.lcParity = ((data[1] & 0x01) != 0);
            //next two bytes are the payload in words...
            this.hasRSSI = ((data[4] & 0x80) != 0);
            this.burstSource = ((data[4] & 0x01) != 0);
            this.hardSync = ((data[5] & 0x40) != 0);
            this.hasSlotType = ((data[5] & 0x08) != 0);
            this.syncType = (byte)(data[5] & 0x03);
            UInt16 offset = (UInt16)(data[6] << 8 | data[7]);
            offset = (UInt16)((offset / 8) + 8);
            if (offset > data.Length)
            {
                throw new ArgumentException("Burst not in expected format: " + string.Join(",", data));
            }
            int newLength = (data.Length - 8) - (data.Length - offset) - 2;
            this.data = new byte[newLength];
            Array.Copy(data, 8, this.data, 0, newLength);
            this.crc = (UInt16)(data[offset - 2] << 8 | data[offset - 1]);
            if (this.hasSlotType)
            {
                this.colorCode = (byte)(data[offset + 1] >> 4);
                this.slotType = (byte)(data[offset + 1] & 0x0F);
                offset += 2;
            }
            if (this.hasRSSI)
            {
                this.rssi = Util.CalcRSSI(data, offset);
            }
        }

        public DataType Type
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

        public bool HasRSSI
        {
            get
            {
                return this.hasRSSI;
            }
        }

        public float RSSI
        {
            get
            {
                return this.rssi;
            }
        }

        public int Slot
        {
            get
            {
                if(this.slot)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
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

        protected virtual string DataString()
        {
            if (this.data != null)
            {
                return "data: [" + string.Join(",", this.data) + "]";
            }
            return "data: [(null)]";
        }

        public override string ToString()
        {
            return base.ToString()+": { type: "+this.type+", RSSIOk: " + this.rssiOk + ", CRC Parity: " + this.crcParity + ", LC Parity: " + this.lcParity + ", Has RSSI: " + this.hasRSSI +
                ", Burst Source: " + this.burstSource + ", Hard Sync: " + this.hardSync + ", Has Slot Type: " + this.hasSlotType + ", Sync Type: " + this.syncType + ", Color Code: " + 
                this.colorCode + ", Slot Type: " + this.slotType + ", RSSI: " + this.rssi + ", " + this.DataString()+"}";
        }

        public virtual byte[] Encode()
        {
            UInt16 datalen = (UInt16)(this.data.Length+2);
            int length = 8 + (int)datalen;
            if(this.hasSlotType)
            {
                length += 2;
            }
            if(this.hasRSSI)
            {
                throw new NotImplementedException("Don't currently have logic to encode RSSI");
                // Offset 2 bytes: length += 2;
            }
            byte[] res = new byte[length];
            res[0] = (byte)this.Type;
            if(this.slot)
            {
                res[0] |= 0x80;
            }
            if (this.rssiOk)
            {
                res[1] |= 0x40;
            }
            if (this.rsParity)
            {
                res[1] |= 0x04;
            }
            if (this.crcParity)
            {
                res[1] |= 0x02;
            }
            if (this.lcParity)
            {
                res[1] |= 0x01;
            }
            res[2] = (byte)(((length - 4) / 2) >> 8);
            res[3] = (byte)(((length - 4) / 2));
            if (this.hasRSSI)
            {
                res[4] |= 0x80;
            }
            if (this.burstSource)
            {
                res[4] |= 0x01;
            }
            if (this.hardSync)
            {
                res[5] |= 0x40;
            }
            res[5] = this.syncType;
            if (this.hasSlotType)
            {
                res[5] |= 0x08;
            }
            res[6] = (byte)((datalen*8) >> 8);
            res[7] = (byte)(datalen*8);
            UInt16 pktCrc = (UInt16)(CRC.CalcCRC16CCITT(this.data, 0, this.data.Length) ^ 0x5A5A);
            Array.Copy(this.data, 0, res, 8, this.data.Length);
            res[datalen + 6] = (byte)(pktCrc >> 8);
            res[datalen + 7] = (byte)(pktCrc);
            if (this.hasSlotType)
            {
                res[datalen + 9] = (byte)((this.colorCode << 4) | this.slotType);
            }
            return res;
        }

        public static Burst Decode(byte[] data)
        {
            DataType dt = (DataType)(data[0] & 0x7f);
            switch(dt)
            {
                case DataType.CSBK:
                    return CSBKBurst.Decode(data);
                case DataType.DataHeader:
                    return new DataHeader(data);
                case DataType.RateThreeQuarter:
                    //Thsi only seems to be used for data bursts
                    return new DataBurst(data);
                case DataType.RateFullData:
                    //This only seems to be used for voice bursts
                    return new VoiceBurst(data);
                case DataType.UnknownSmall:
                    return new UnknownSmall(data);
                default:
                    return new Burst(data);
            }
        }
    }
}

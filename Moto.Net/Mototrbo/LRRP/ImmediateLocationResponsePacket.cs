using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.LRRP
{
    public class ImmediateLocationResponsePacket : LRRPPacket
    {
        protected DateTime? reportedTime;
        protected float latitude;
        protected float longitude;
        protected float radius;
        protected float altitude;
        protected float altitueAccuracy;
        protected byte horizontalDirection;
        protected float horizontalSpeed;
        protected LRRPResponseCodes responseCode = (LRRPResponseCodes)(-1);

        protected ImmediateLocationResponsePacket(LRRPPacketType type) : base(type)
        {

        }

        public ImmediateLocationResponsePacket() : base(LRRPPacketType.ImmediateLocationResponse)
        {
        }

        public ImmediateLocationResponsePacket(byte[] data) : base(data)
        {
            int offset = 0;
            while(offset < this.data.Length)
            {
                switch(this.data[offset])
                {
                    case 0x34:
                        this.reportedTime = this.ReadDateTime(this.data, offset + 1);
                        offset += 6;
                        break;
                    case 0x37:
                        this.responseCode = (LRRPResponseCodes)this.data[offset + 1];
                        offset += 2;
                        if (((byte)this.responseCode & 0x80) != 0)
                        {
                            this.responseCode = (LRRPResponseCodes)((byte)this.responseCode << 7 | this.data[offset]);
                            offset += 1;
                        }
                        break;
                    case 0x51:
                        offset += 1;
                        this.latitude = this.ReadLatitude(this.data, offset);
                        offset += 4;
                        this.longitude = this.ReadLongitude(this.data, offset);
                        offset += 4;
                        this.radius = ReadFloat(this.data, offset);
                        offset += 2;
                        break;
                    case 0x55:
                        offset += 1;
                        this.latitude = this.ReadLatitude(this.data, offset);
                        offset += 4;
                        this.longitude = this.ReadLongitude(this.data, offset);
                        offset += 4;
                        this.radius = ReadFloat(this.data, offset);
                        offset += 2;
                        this.altitude = ReadFloat(this.data, offset);
                        offset += 2;
                        this.altitueAccuracy = ReadFloat(this.data, offset);
                        offset += 3;
                        break;
                    case 0x56:
                        this.horizontalDirection = this.data[offset + 1];
                        offset += 2;
                        break;
                    case 0x66:
                        offset += 1;
                        this.latitude = this.ReadLatitude(this.data, offset);
                        offset += 4;
                        this.longitude = this.ReadLongitude(this.data, offset);
                        offset += 4;
                        break;
                    case 0x69:
                        offset += 1;
                        this.latitude = this.ReadLatitude(this.data, offset);
                        offset += 4;
                        this.longitude = this.ReadLongitude(this.data, offset);
                        offset += 4;
                        this.altitude = this.ReadFloat(this.data, offset);
                        offset += 3;
                        break;
                    case 0x6C:
                        offset += 1;
                        this.horizontalSpeed = this.ReadFloat(this.data, offset);
                        offset += 3;
                        break;
                    default:
                        throw new NotImplementedException("Unknown tag " + this.data[offset]+" at offset "+offset+"("+BitConverter.ToString(this.data)+")");
                }
            }
        }

        protected DateTime ReadDateTime(byte[] data, int offset)
        {
            int year = (data[offset] << 6) | ((data[offset + 1] >> 2) & 0x3F);
            int month = (data[offset + 1] & 3) << 2 | (int)data[offset + 2] >> 6 & 3;
            int day = data[offset + 2] >> 1 & 31;
            int hour = (data[offset + 2] & 1) << 4 | (int)data[offset + 3] >> 4 & 0x0F;
            int minute = (data[offset + 3] & 15) << 2 | (int)data[offset + 4] >> 6 & 3;
            int second = data[offset + 4] & 0x3F;
            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        }

        protected float ReadLatitude(byte[] data, int offset)
        {
            Int32 tmpLat = (data[offset] << 24 | data[offset + 1] << 16 | data[offset + 2] << 8 | data[offset + 3]);
            return (float)(tmpLat * (180.0 / 0xFFFFFFFF));
        }

        protected float ReadLongitude(byte[] data, int offset)
        {
            Int32 tmpLong = (data[offset] << 24 | data[offset + 1] << 16 | data[offset + 2] << 8 | data[offset + 3]);
            return (float)(tmpLong * (360.0 / 0xFFFFFFFF));
        }

        protected float ReadFloat(byte[] data, int offset)
        {
            return (float)((float)data[offset] + (data[offset + 1] * 0.01));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if(responseCode != (LRRPResponseCodes)(-1))
            {
                sb.AppendFormat(", Response Code {0}", responseCode);
            }
            if(reportedTime.HasValue)
            {
                sb.AppendFormat(", Time {0}", reportedTime.Value.ToLocalTime());
            }
            if(this.latitude != 0.0f)
            {
                sb.AppendFormat(", Lat: {0}", latitude);
            }
            if (this.longitude != 0.0f)
            {
                sb.AppendFormat(", Long: {0}", longitude);
            }
            if (this.radius != 0.0f)
            {
                sb.AppendFormat(", Radius: {0}", radius);
            }
            if (this.altitude != 0.0f)
            {
                sb.AppendFormat(", Alititude: {0}", altitude);
            }
            if (this.altitueAccuracy != 0.0f)
            {
                sb.AppendFormat(", Alititude Accuracy: {0}", altitueAccuracy);
            }
            if(this.horizontalDirection != 0)
            {
                sb.AppendFormat(", Horizontal Direction: {0}", horizontalDirection);
            }
            return base.ToString() + sb.ToString();
        }

        public float Latitude
        {
            get
            {
                return this.latitude;
            }
        }

        public float Longitude
        {
            get
            {
                return this.longitude;
            }
        }
    }
}

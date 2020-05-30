using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.Bursts
{
    public enum ContentType
    {
        UnifiedData = 0x00,
        TCPHeaderCompression = 0x02,
        UDPHeaderCompression = 0x03,
        IPPacket = 0x04,
        ARP = 0x05,
        ProprietaryData = 0x09,
        ShortData = 0x0A
    }

    public class DataHeader : Burst
    {
        protected bool isGroup;
        protected bool responseRequested;
        protected bool compressed;
        protected byte headerType;
        protected byte padOctetCount;
        protected ContentType contentType;
        protected RadioID to;
        protected RadioID from;
        protected bool fullMessage;
        protected byte blocksFollow;

        public DataHeader(byte[] data) : base(data)
        {
            this.isGroup = ((data[8] & 0x80) != 0);
            this.responseRequested = ((data[8] & 0x40) != 0);
            this.compressed = ((data[8] & 0x20) != 0);
            this.headerType = (byte)(data[8] & 0x0F);
            this.padOctetCount = (byte)(data[8] & 0x10);
            this.padOctetCount |= (byte)(data[9] & 0x0F);
            this.contentType = (ContentType)(data[9] >> 4);
            this.to = new RadioID(data, 10, 3);
            this.from = new RadioID(data, 13, 3);
            this.fullMessage = ((data[16] & 0x80) != 0);
            this.blocksFollow = (byte)(data[16] & 0x7F);
            this.crc = (UInt16)(data[17] << 8 | data[18]);
        }

        protected override string DataString()
        {
            return "\n" +
                "Is Group: " + this.isGroup + "\n" +
                "Response Requested: " + this.responseRequested + "\n" +
                "Compressed: " + this.compressed + "\n" +
                "Header Type: " + this.headerType + "\n" +
                "Pad Octet Count: " + this.padOctetCount + "\n" +
                "Content Type: " + this.contentType + "\n" +
                "To: " + this.to + "\n" +
                "From: " + this.from + "\n" +
                "Full Message: " + this.fullMessage + "\n" +
                "Blocks Follow: " + this.blocksFollow + "\n" +
                "CRC: " + this.crc;
        }

        public ContentType ContentType
        {
            get
            {
                return this.contentType;
            }
        }

        public byte HeaderType
        {
            get
            {
                return this.headerType;
            }
        }
    }
}

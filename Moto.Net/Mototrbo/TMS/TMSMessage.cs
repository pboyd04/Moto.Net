using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.TMS
{
    public class TMSMessage
    {
        protected bool extensionExists;
        protected bool requiresAck;
        protected bool reservedFlag;
        protected bool system;
        protected MessageType type;
        protected byte sequenceNumber;
        protected byte encoding;
        protected string message;

        public TMSMessage(string message)
        {
            this.type = MessageType.SimpleText;
            this.reservedFlag = true;
            this.message = message;
        }

        public TMSMessage(string message, bool confirm) : this(message)
        {
            this.requiresAck = confirm;
        }

        public TMSMessage(byte[] data)
        {
            this.extensionExists = ((data[2] & 0x80) != 0);
            this.requiresAck = ((data[2] & 0x40) != 0);
            this.reservedFlag = ((data[2] & 0x20) != 0);
            this.system = ((data[2] & 0x10) != 0);
            this.type = (MessageType)(data[2] & 0x1F);
            if(data[3] > 0)
            {
                throw new Exception("Unknown TMS format!");
            }
            this.sequenceNumber = (byte)(data[4] & 0x1F);
            if (this.type == MessageType.Ack)
            {
                return;
            }
            this.encoding = (byte)(data[5]);
            int index = 6;
            if (data[index] == 0x0d && data[index + 2] == 0x0a)
            {
                //Skip the CRLF
                index += 4;
            }
            this.message = Encoding.Unicode.GetString(data, index, (data.Length - index) - 4).Replace("\0", "");
        }

        public bool RequiresAck
        {
            get
            {
                return this.requiresAck;
            }
            set
            {
                this.requiresAck = value;
            }
        }

        public MessageType Type
        {
            get
            {
                return this.type;
            }
        }

        public byte SequenceNumber
        {
            get
            {
                return this.sequenceNumber;
            }
        }

        public string Message
        {
            get
            {
                return this.message;
            }
        }

        public byte[] Encode()
        {
            byte[] msg = Encoding.Unicode.GetBytes(this.message);
            byte len = (byte)(msg.Length + 8);
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(0x00);
            ms.WriteByte(len);
            byte x = 0xA0;
            if(this.requiresAck)
            {
                x |= 0x40;
            }
            ms.WriteByte(x);
            ms.WriteByte(0x00);
            ms.WriteByte((byte)(0x80 | this.SequenceNumber));
            ms.WriteByte(0x04);
            ms.WriteByte(0x0D);
            ms.WriteByte(0x00);
            ms.WriteByte(0x0A);
            ms.WriteByte(0x00);
            ms.Write(msg, 0, msg.Length);
            return ms.ToArray();
        }
    }
}

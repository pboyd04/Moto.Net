using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL
{
    public class XNLPacket
    {
        protected OpCode opcode;
        protected bool isXCMP;
        protected byte flags;
        protected Address dest;
        protected Address src;
        protected UInt16 transactionID;
        protected byte[] data;

        public XNLPacket(byte[] data)
        {
            this.opcode = (OpCode)data[3];
            this.isXCMP = data[4] == 0x01;
            this.flags = data[5];
            this.dest = new Address(data, 6);
            this.src = new Address(data, 8);
            this.transactionID = (UInt16)((data[10] << 8) | data[11]);
            UInt16 len = (UInt16)((data[12] << 8) | data[13]);
            this.data = new byte[len];
            Array.Copy(data, 14, this.data, 0, len);
        }

        public XNLPacket(OpCode opcode)
        {
            this.opcode = opcode;
        }

        public XNLPacket(OpCode opcode, byte[] data)
        {
            this.opcode = opcode;
            this.isXCMP = data[0] == 0x01;
            this.flags = data[1];
            this.dest = new Address(data, 2);
            this.src = new Address(data, 4);
            this.transactionID = (UInt16)((data[6] << 8) | data[7]);
            UInt16 len = (UInt16)((data[8] << 8) | data[9]);
            this.data = new byte[len];
            Array.Copy(data, 10, this.data, 0, len);
            Console.WriteLine(this);
        }

        public static XNLPacket Decode(Byte[] data)
        {
            OpCode pktOpCode = (OpCode)data[3];
            switch (pktOpCode)
            {
                case OpCode.MasterStatusBroadcast:
                    return new MasterStatusBroadcast(data);
                case OpCode.DataMessage:
                    return new DataPacket(data);
                case OpCode.DeviceAuthKeyReply:
                    return new DevAuthKeyReplyPacket(data);
                case OpCode.DeviceConnectionReply:
                    return new DevConnectionReplyPacket(data);
                case OpCode.DeviceSysMapBroadcast:
                    return new DevSysMapBroadcastPacket(data);
                default:
                    return new XNLPacket(data);
            }
        }

        public override string ToString()
        {
            return base.ToString()+": {OpCode: "+this.opcode+", XCMP: "+this.isXCMP+", Flags: "+this.flags+", Dest: "+this.dest+", Src: "+this.src+", Transaction ID: "+this.transactionID+", Data: "+BitConverter.ToString(this.data)+"}";
        }

        public byte[] Encode()
        {
            UInt16 length = (UInt16)(12 + this.data.Length);
            byte[] res = new byte[length + 2];
            res[0] = (byte)(length >> 8);
            res[1] = (byte)length;
            res[2] = 0;
            res[3] = (byte)opcode;
            if (this.isXCMP)
            {
                res[4] = 0x01;
            }
            else
            {
                res[4] = 0x00;
            }
            res[5] = this.flags;
            this.dest.AddToArray(res, 6);
            this.src.AddToArray(res, 8);
            res[10] = (byte)(this.transactionID >> 8);
            res[11] = (byte)this.transactionID;
            res[12] = (byte)(this.data.Length >> 8);
            res[13] = (byte)(this.data.Length);
            Array.Copy(this.data, 0, res, 14, this.data.Length);
            return res;
        }

        public OpCode OpCode
        {
            get
            {
                return this.opcode;
            }
            set
            {
                this.opcode = value;
            }
        }

        public bool IsXCMP
        {
            get
            {
                return this.isXCMP;
            }
            set
            {
                this.isXCMP = value;
            }
        }

        public byte Flags
        {
            get
            {
                return this.flags;
            }
            set
            {
                this.flags = value;
            }
        }

        public Address Source
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

        public Address Destination
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

        public UInt16 TransactionID
        {
            get
            {
                return this.transactionID;
            }
            set
            {
                this.transactionID = value;
            }
        }
    }
}

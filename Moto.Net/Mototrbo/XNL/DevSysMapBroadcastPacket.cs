using System;

namespace Moto.Net.Mototrbo.XNL
{
    public struct DevSysEntry
    {
        public XNLDevType DeviceType;
        public byte DeviceNumber; //Always seems to be 1
        public Address XNLAddress;
        public byte AuthIndex; 

        public DevSysEntry(byte[] data, int offset)
        {
            this.DeviceType = (XNLDevType)data[offset + 0];
            this.DeviceNumber = (byte)data[offset + 1];
            this.XNLAddress = new Address(data, offset + 2);
            this.AuthIndex = (byte)data[4];
        }

        public override string ToString()
        {
            return base.ToString() + ": { DeviceType: "+ this.DeviceType + ", DeviceNumber: " + this.DeviceNumber + ", XNLAddress: " + this.XNLAddress + ", AuthIndex: " + this.AuthIndex + "}";
        }
    }

    public class DevSysMapBroadcastPacket : XNLPacket
    {
        protected DevSysEntry[] entries;

        public DevSysMapBroadcastPacket(byte[] data) : base(data)
        {
            UInt16 count = (UInt16)(this.data[0] << 8 | this.data[1]);
            entries = new DevSysEntry[count];
            for(int i = 0; i < count; i++)
            {
                entries[i] = new DevSysEntry(this.data, 2+i*5);
            }
        }

        public override string ToString()
        {
            return "DevSysMapBroadcastPacket: {OpCode: " + this.opcode + ", XCMP: " + this.isXCMP + ", Flags: " + this.flags + ", Dest: " + this.dest + ", Src: " + this.src + ", Transaction ID: " + this.transactionID + ", Entries: " + String.Join(",", entries) + "}";
        }

        public DevSysEntry[] Entries
        {
            get
            {
                return entries;
            }
        }
    }
}
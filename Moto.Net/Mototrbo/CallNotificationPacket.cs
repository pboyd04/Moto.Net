using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public enum CallType : byte
    {
        Unknown2 = 0x30, //This seems to go out before some data calls...
        Unknown1 = 0x32, //This seems to go out before most data type requests maybe some sort of wake up packet?
        CallAlertRequest = 0x45,
        CallAlertResponse = 0x46,
        RadioCheckRequest = 0x47,
        RadioCheckResponse = 0x48,
        RadioDisableRequest = 0x49,
        RadioEnableRequest = 0x4B,
        RadioMonitorRequest = 0x4D,
        RadioMonitorResponse = 0x4E,
        GroupVoiceCall = 0x4F,
        PrivateVoiceCall = 0x50,
        PrivateDataCall = 0x52,
        AllCall = 0x53,
        DataResponse = 0x54,
    }

    public class CallNotificationPacket : Packet
    {
        //This is redundant...
        private RadioID source;
        //Parsing this as a RadioID since I assume this is what it is, but it always seems to be 0...
        private RadioID destination;
        private byte slot;
        private TimeSlotStatus status;
        private RadioID callFrom;
        private RadioID callTo;
        private CallType callType;
        private bool encrypted;
        private float rssi;

        public CallNotificationPacket(RadioID id) : base(PacketType.CallNotification)
        {
            this.id = id;
        }

        public CallNotificationPacket(Byte[] data) : base(data)
        {
            this.source = new RadioID(this.data, 0);
            this.destination = new RadioID(this.data, 4);
            this.slot = (byte)(this.data[8]+1);
            //Next byte always seems to be 0...
            this.status = (TimeSlotStatus)this.data[10];
            this.callFrom = new RadioID(this.data, 11, 3);
            this.callTo = new RadioID(this.data, 14, 3);
            this.callType = (CallType)this.data[17];
            //Next byte always seems to be 0...
            this.encrypted = (bool)(this.data[19] != 0);
            //Next byte always seems to be 0 or 16 when encrypted (reundant data?). The next two bytes are also always 0... 
            this.rssi = Util.CalcRSSI(this.data, 23);
            //This is also always 0...
        }

        protected override string DataString()
        {
            StringBuilder sb = new StringBuilder("{");
            sb.AppendFormat("Slot: {0}, Status: {1}, Call From: {2}, Call To: {3}, Call Type: {4}, Encrypted: {5}, RSSI: {6}", this.slot, this.status, this.callFrom, this.callTo, this.callType, this.encrypted, this.rssi);
            sb.Append("}");
            return sb.ToString();
        }

        public override byte[] Encode()
        {
            this.data = new byte[0];
            return base.Encode();
        }
    }
}

// [1,0,1,0,3,240,0,0,200,79,0,0,0,0,0,45,155,0]
// [1,0,2,0,3,240,0,0,200,79,0,0,0,0,0,45,127,0]
// [0,0,2,0,3,240,0,0,201,79,0,0,0,0,0,45,194,0]

// [0,0,1,0,8,53,0,0,200,79,0,0,0,0,0,45,137,0]
// [0,0,2,0,8,53,0,0,200,79,0,0,0,0,0,45,173,0]


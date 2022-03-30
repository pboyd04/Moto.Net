using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public enum XNLDevAttributes
    {
        DeviceFamily = 0, //5 seems to be the normal value here (but I'm supposed to send 0 from what I can see in responses)
        Display = 2, //0 means none
        RFBand = 4, //1 seems to be UHF (as does 9... and 10?)
        GPIO = 5,
        RadioType = 7, //Seems to be 0 for mobile, 1 for portable, and 2 for repeaters
        Keypad = 9, //I think this is just boolean, 0 or 1
        ChannelKnob = 13, //0 means none, 255 means the free spinning ones, otherwise it is the number of channels on the dial.
        VirtualPersonality = 14, //No idea what this means
        Bluetooth = 17, //255 means no bluetooth, 253 means turned off, 0 means yes
        GPS = 20, //255 means no GPS support, 253 means turned off, 0 means yes
        //1, 10, and 19 have started showing up on newer radios. don't know what they mean yet...
        //1 - 0 XPR3550,XPR3550e,XPR7550e
        //10 - 1 XPR3550,XPR3550e
        //19 - 255 XPR3550,XP3550e,XPR7550e
    }

    public struct DeviceStatus
    {
        public XNLDevType DeviceType;
        public UInt16 Status;
        public Dictionary<XNLDevAttributes, byte> Descriptor;

        public override string ToString()
        {
            if(Descriptor == null)
            {
                return "DeviceStatus: {DeviceType: " + this.DeviceType + ", Status: " + Status + "}";
            }
            return "DeviceStatus: {DeviceType: " + this.DeviceType + ", Status: " + Status + ", Descriptor: " + string.Join(",", Descriptor) + "}";
        }
    }

    public class DeviceInitStatusBroadcast : XCMPPacket
    {
        protected byte majorVersion;
        protected byte minorVersion;
        protected byte revVersion;
        protected XNLDevType entityType;
        protected byte initComplete;
        protected DeviceStatus Status;

        public DeviceInitStatusBroadcast(byte[] data) : base(data)
        {
            this.majorVersion = this.data[0];
            this.minorVersion = this.data[1];
            this.revVersion = this.data[2];
            this.entityType = (XNLDevType)this.data[3];
            this.initComplete = this.data[4];
            if(this.initComplete == 0x00)
            {
                this.Status.DeviceType = (XNLDevType)this.data[5];
                this.Status.Status = (UInt16)(this.data[6] << 8 | this.data[7]);
                byte len = this.data[8];
                if (len > 0)
                {
                    this.Status.Descriptor = new Dictionary<XNLDevAttributes, byte>();
                    for(int i = 0; i < len; i+=2)
                    {
                        this.Status.Descriptor[(XNLDevAttributes)this.data[5 + i]] = this.data[5 + i + 1];
                    }
                }
            }
        }

        public DeviceInitStatusBroadcast(string version, XNLDevType entityType, byte initComplete, DeviceStatus Status) : base(XCMPOpCode.DeviceinitStatusBroadcast)
        {
            string[] split = version.Split('.');
            this.majorVersion = byte.Parse(split[0]);
            this.minorVersion = byte.Parse(split[1]);
            this.revVersion = byte.Parse(split[2]);
            this.entityType = entityType;
            this.initComplete = initComplete;
            this.Status = Status;
            if (initComplete == 0x00)
            {
                int length = 9;
                if(Status.Descriptor != null)
                {
                    length += Status.Descriptor.Count * 2;
                }
                this.data = new byte[length];
                this.data[0] = this.majorVersion;
                this.data[1] = this.minorVersion;
                this.data[2] = this.revVersion;
                this.data[3] = (byte)this.entityType;
                this.data[4] = this.initComplete;
                this.data[5] = (byte)this.Status.DeviceType;
                this.data[6] = (byte)(this.Status.Status >> 8);
                this.data[7] = (byte)this.Status.Status;
                this.data[8] = (byte)(length - 9);
                if (Status.Descriptor != null)
                {
                    int i = 9;
                    foreach(KeyValuePair<XNLDevAttributes, byte> pair in Status.Descriptor)
                    {
                        this.data[i++] = (byte)pair.Key;
                        this.data[i++] = pair.Value;
                    }
                }
            }
            else
            {
                this.data = new byte[5];
                this.data[0] = this.majorVersion;
                this.data[1] = this.minorVersion;
                this.data[2] = this.revVersion;
                this.data[3] = (byte)this.entityType;
                this.data[4] = this.initComplete;
            }
        }

        public override string ToString()
        {
            return "DeviceInitStatusBroadcast: {OpCode: " + this.opcode + ", Version: " + this.Version + ", InitComplete: " + this.initComplete + ", EntityType: " + this.entityType + ", Status: " + this.Status + "}";
        }

        public string Version
        {
            get
            {
                return this.majorVersion + "." + this.minorVersion + "." + this.revVersion;
            }
        }

        public XNLDevType EntityType
        {
            get
            {
                return this.entityType;
            }
        }

        public bool InitComplete
        {
            get
            {
                return (this.initComplete != 0x00);
            }
        }
    }
}

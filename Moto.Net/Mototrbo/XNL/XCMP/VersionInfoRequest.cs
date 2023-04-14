using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public enum VersionInfoType : byte
    {
        FirmwareVersion = 0x00,
        CodeplugVersion = 0x0F,
        CodeplugVersion2 = 0x41, //This seems to be needed when in CPS mode? Not sure why it's different
        BootloaderVersion = 0x50,
    }

    public class VersionInfoRequest : XCMPPacket
    { 
        public VersionInfoRequest() : base(XCMPOpCode.VersionInfoRequest)
        {
            this.data = new byte[1];
            this.data[0] = 0x00; //I think this is the type of firmware, like code, bootloader, codeplug, etc. Just leaving this on the regular firmware for now.
        }

        public VersionInfoRequest(VersionInfoType type) : base(XCMPOpCode.VersionInfoRequest)
        {
            this.data = new byte[1];
            this.data[0] = (byte)type;
        }

        public VersionInfoRequest(byte[] data) : base(data)
        {
        }
    }
}

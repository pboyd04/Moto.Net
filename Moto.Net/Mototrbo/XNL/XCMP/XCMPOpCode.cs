using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public enum XCMPOpCode
    {
        DeviceinitStatusBroadcast = 0xB400,
        RadioStatusRequest = 0x000E,
        RadioStatusReply = 0x800E,
        VersionInfoRequest = 0x000F,
        VersionInfoReply = 0x800F,
        CPS_TanapaNumberRequest = 0x001F,
        CPS_TanapaNumberReply = 0x801F,
        CPS_SuperBundleRequest = 0x002E,
        CPS_SuperBundleReply = 0x802E,
        CloneReadRequest = 0x010A,
        CloneReadReply = 0x810A,
        TransmitPowerLevelRequest = 0x0408, //Next byte of 0x80 gets the power level while 0x00 sets the power level. 0 is low and 3 is high... not sure what the values inbetween are...
        TransmitPowerLevelReply = 0x8408,
        RadioPowerRequest = 0x040A, //Next byte of 1 seems to shutdown some radios and reboot others, 2 seems to restart all
        RadioPowerReply = 0x840A,
        ChannelSelectRequest = 0x040D,
        ChannelSelectReply = 0x840D,
        RRCtrlBroadcast = 0xB41C,
        AlarmStatusRequest = 0x042E,
        AlarmStatusReply = 0x842E,
    }
    //Unknown packet: {OpCode: 0000B402, Data: 01-09-00-00-0C-01-05-00-01-00-04-00}
    //0x0010 - With byte of 0 seems to get model number
    //0x8010 - Response which is the model number
    //0x0011 - With byte of 0 seems to get serial number
    //0x8011 - Response which is the serial number
    //0x0012 - Sent by CPS with no payload
    //0x8012 - Repsonse to 0x0012 - 0c 78 e1 b9 06 c5 4d 3a 8f 26 4c e5 c4 f0 b9 df (consistent across radios)
    //0x003d - With two 0 bytes
    //0x803d - 01 00 01 00
    //0x0100 - CPS - I think this is some kind of unlock type command needed before accessing the codeplug
    //0x0104 - CPS - This seems to download the actual code plug



    public static class EnumExtension
    {
        public static void AddToArray(this XCMPOpCode opcode, byte[] array, int startOffset)
        {
            UInt16 intval = (UInt16)opcode;
            byte[] bytes = BitConverter.GetBytes(intval);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Array.Copy(bytes, 0, array, startOffset, 2);
        }
    }
}

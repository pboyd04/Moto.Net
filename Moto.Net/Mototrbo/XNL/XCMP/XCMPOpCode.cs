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
        ChannelSelectRequest = 0x040D,
        ChannelSelectReply = 0x840D,
        RRCtrlBroadcast = 0xB41C,
        AlarmStatusRequest = 0x042E,
        AlarmStatusReply = 0x842E
    }

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

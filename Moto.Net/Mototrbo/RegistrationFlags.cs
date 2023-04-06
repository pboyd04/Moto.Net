using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    [Flags]
    public enum RegistrationFlags : UInt32
    {
        //Byte 4
        VoiceCallSupport            = 0x00000004,
        DataCallSupport             = 0x00000008,
        SomethingElseThatIsRequired = 0x00000010,
        XNLDevice                   = 0x00000020,
        //Byte 3
        Software                    = 0x00002000, //This seems to only be set by things like RDAC and other programs and not other repeaters
        CallMonitor                 = 0x00004000, //Setting this enables the CallNotification and CallStatusNotification packets
        CSKBSupport                 = 0x00008000,
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.LRRP
{
    public enum LRRPPacketType
    {
        ImmediateLocationRequest = 0x05,
        ImmediateLocationResponse = 0x07,
        TriggeredLocationStartRequest = 0x09,
        TriggeredLocationStartResponse = 0x0B,
        TriggeredLocationData = 0x0D,
        TriggeredLocationStopRequest = 0x0F,
        TriggeredLocationStopResponse = 0x11,
        ProtocolVersionRequest = 0x14,
        ProtocolVersionResponse = 0x15,
    }
}

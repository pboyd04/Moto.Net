using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.TMS
{
    public enum MessageType
    {
        SimpleText = 0,
        ServiceAvailability = 0x10,
        Ack = 0x1F
    }
}

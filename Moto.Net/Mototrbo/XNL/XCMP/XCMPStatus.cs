using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public enum XCMPStatus
    {
        RSSI = 0x02,
        ModelNumber = 0x07,
        SerialNumber = 0x08,
        RepeaterSerialNumber = 0x0B,
        RadioID = 0x0E,
        RadioAlias = 0x0F
    }
}

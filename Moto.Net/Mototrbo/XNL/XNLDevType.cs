using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL
{
    public enum XNLDevType
    {
        RFTransceiver = 1, //According to xcmp-xnl-dissector
        AlsoRepeater = 4, //According to experimentation
        RadioControlStation = 5, //According to experimentation
        Repeater = 9, //According to experimentation
        IPPeripheral = 10 //According to xcmp-xnl-dissector
    }
}

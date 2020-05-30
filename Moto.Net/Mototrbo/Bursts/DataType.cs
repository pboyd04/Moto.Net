using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.Bursts
{
    public enum DataType
    {
        //Taken from ETSI TS 102 361-1
        PIHeader = 0x00,
        VoiceLCHeader = 0x01,
        TerminatorWithLC = 0x02,
        CSBK = 0x03,
        MBCHeader = 0x04,
        MBCContinuation = 0x05,
        DataHeader = 0x06,
        RateHalfData = 0x07,
        RateThreeQuarter = 0x08,
        Idle = 0x09,
        RateFullData = 0x0a,
        USBD = 0x0b,
        //End values from ETSI TS 102 361-1

        //Don't know what this is... but it doesn't fit the regular format...
        UnknownSmall = 0x13
    }
}

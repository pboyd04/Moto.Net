using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.LRRP
{
    public enum LRRPResponseCodes
    {
        Success = 0,
        BadCommand = 0x0A, //This seems to happen when I ask for something it can't understand
        NoGPS = 0x10, //This seems to happen at startup when there is no GPS signal
        NotEnoughGPS = 0x200 //This seems to happen when GPS is up but not enough to give good data
    }
}

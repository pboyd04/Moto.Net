using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public static class Util
    {
        public static float CalcRSSI(byte[] data, int offset)
        {
            return (float)(-1.0 * (float)data[offset] + (data[offset + 1] * 1000.0 + 128.0) / 256000.0);
        }
    }
}

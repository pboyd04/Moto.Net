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
            if(data[offset] == 0 && data[offset + 1] == 0)
            {
                return -1;
            }
            return (float)(-1.0 * (float)data[offset] + (data[offset + 1] * 1000.0 + 128.0) / 256000.0);
        }

        public static uint ReadVLQ(byte[] data, int offset, out int len)
        {
            uint ret = data[offset];
            if((ret & 0x80) == 0)
            {
                len = 1;
                return ret;
            }
            ret = (byte)(ret & 0x7F);
            for(int i = 1; i <= 4; i++) //does motorolla use any ints bigger than 32-bit in VLQ?
            {
                ret = (ret << 7) | (byte)(data[offset + i] & 0x7F);
                if((data[offset + i] & 0x80) == 0)
                {
                    len = i + 1;
                    return ret;
                }
            }
            throw new NotImplementedException("Unknown integer length for VLQ!");
        }
    }
}

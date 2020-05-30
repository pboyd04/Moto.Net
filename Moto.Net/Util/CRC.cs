using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Util
{
    public static class CRC
    {
        public static UInt16 CalcCRC16CCITT(byte[] data, int offset, int length)
        {
            UInt16 crc = 0;
            UInt16 polynomial = 0x1021;
            for (int i = 0; i < length; i++)
            {
                crc ^= (UInt16)((uint)data[i + offset] << 8);
                for (byte j = 0; j < (byte)8; j++)
                {
                    if ((crc & 0x8000) != 0)
                    {
                        crc = (UInt16)((int)crc << 1 ^ (int)polynomial);
                    }
                    else
                    {
                        crc = (UInt16)((int)crc << 1);
                    }
                }
            }
            return crc;
        }
    }
}

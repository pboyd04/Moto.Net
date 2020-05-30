using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL
{
    public static class Encrypter
    {
        public static byte[] Encrypt(byte[] data)
        {
            UInt32 dword1 = Encrypter.ArrayToInt(data, 0);
            UInt32 dword2 = Encrypter.ArrayToInt(data, 4);
            string const1Str = ConfigurationManager.AppSettings.Get("XNLConst1");
            UInt32 num1 = UInt32.Parse(const1Str);
            string const2Str = ConfigurationManager.AppSettings.Get("XNLConst2");
            UInt32 num2 = UInt32.Parse(const2Str);
            string const3Str = ConfigurationManager.AppSettings.Get("XNLConst3");
            UInt32 num3 = UInt32.Parse(const3Str);
            string const4Str = ConfigurationManager.AppSettings.Get("XNLConst4");
            UInt32 num4 = UInt32.Parse(const4Str);
            string const5Str = ConfigurationManager.AppSettings.Get("XNLConst5");
            UInt32 num5 = UInt32.Parse(const5Str);
            string const6Str = ConfigurationManager.AppSettings.Get("XNLConst6");
            UInt32 num6 = UInt32.Parse(const6Str);
            for (int index = 0; index < 32; ++index)
            {
                num1 += num2;
                dword1 += (uint)(((int)dword2 << 4) + (int)num3 ^ (int)dword2 + (int)num1 ^ (int)(dword2 >> 5) + (int)num4);
                dword2 += (uint)(((int)dword1 << 4) + (int)num5 ^ (int)dword1 + (int)num1 ^ (int)(dword1 >> 5) + (int)num6);
            }
            byte[] res = new byte[8];
            Encrypter.IntToArray(dword1, res, 0);
            Encrypter.IntToArray(dword2, res, 4);
            return res;
        }

        private static UInt32 ArrayToInt(byte[] data, int start)
        {
            UInt32 ret = 0;
            for(int i = 0; i < 4; i++)
            {
                ret = ret << 8 | data[i + start];
            }
            return ret;
        }

        private static void IntToArray(UInt32 i, byte[] data, int start)
        {
            for(int index = 0; index < 4; ++index)
            {
                data[start + 3 - index] = (byte)(i & (uint)byte.MaxValue);
                i >>= 8;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Moto.Net.Mototrbo.XNL
{
    interface MotoEncrpter
    {
        byte[] EncryptAuthKey(byte[] data);
    }

    public static class Encrypter
    {
        public static byte[] Encrypt(byte[] data)
        {
            UInt32 dword1 = Encrypter.ArrayToInt(data, 0);
            UInt32 dword2 = Encrypter.ArrayToInt(data, 4);
            string const1Str = ConfigurationManager.AppSettings.Get("XNLConst1");
            string const2Str = ConfigurationManager.AppSettings.Get("XNLConst2");
            string const3Str = ConfigurationManager.AppSettings.Get("XNLConst3");
            string const4Str = ConfigurationManager.AppSettings.Get("XNLConst4");
            string const5Str = ConfigurationManager.AppSettings.Get("XNLConst5");
            string const6Str = ConfigurationManager.AppSettings.Get("XNLConst6");
            if(const1Str == null || const2Str == null || const3Str == null || const4Str == null || const5Str == null || const6Str == null)
            {
                //See if we have TRBONet server
                Console.WriteLine("Falling back to trbonet crypter...");
                try
                {
                    Assembly trbonet = Assembly.LoadFrom("TRBOnet.Server.exe");
                    Type crypter = trbonet.GetType("NS.Enginee.Mototrbo.Utils.XNLRepeaterCrypter");
                    MethodInfo mi = crypter.GetMethod("Encrypt", BindingFlags.Public | BindingFlags.Static);
                    //The method alters the data in place...
                    mi.Invoke(null, new object[] { data });
                    return data;
                } catch(Exception ex)
                {
                    throw new Exception("Unable to encrypt XNL data!", ex);
                }
            }
            UInt32 num1 = UInt32.Parse(const1Str);
            UInt32 num2 = UInt32.Parse(const2Str);
            UInt32 num3 = UInt32.Parse(const3Str);
            UInt32 num4 = UInt32.Parse(const4Str);
            UInt32 num5 = UInt32.Parse(const5Str);
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

        public static byte[] EncryptControlStation(byte[] data)
        {
            UInt32 dword1 = Encrypter.ArrayToInt(data, 0);
            UInt32 dword2 = Encrypter.ArrayToInt(data, 4);
            string const1Str = ConfigurationManager.AppSettings.Get("XNLControlConst1");
            string const2Str = ConfigurationManager.AppSettings.Get("XNLControlConst2");
            string const3Str = ConfigurationManager.AppSettings.Get("XNLControlConst3");
            string const4Str = ConfigurationManager.AppSettings.Get("XNLControlConst4");
            string const5Str = ConfigurationManager.AppSettings.Get("XNLControlConst5");
            string const6Str = ConfigurationManager.AppSettings.Get("XNLControlConst6");
            if (const1Str == null || const2Str == null || const3Str == null || const4Str == null || const5Str == null || const6Str == null)
            {
                //See if we have TRBONet server
                Console.WriteLine("Falling back to trbonet crypter...");
                try
                {
                    Assembly trbonet = Assembly.LoadFrom("TRBOnet.Server.exe");
                    Type crypter = trbonet.GetType("NS.Enginee.Mototrbo.Utils.XNLMasterCrypter");
                    MethodInfo mi = crypter.GetMethod("Encrypt", BindingFlags.Public | BindingFlags.Static);
                    //The method alters the data in place...
                    mi.Invoke(null, new object[] { data });
                    return data;
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to encrypt XNL data!", ex);
                }
            }
            UInt32 num1 = UInt32.Parse(const1Str);
            UInt32 num2 = UInt32.Parse(const2Str);
            UInt32 num3 = UInt32.Parse(const3Str);
            UInt32 num4 = UInt32.Parse(const4Str);
            UInt32 num5 = UInt32.Parse(const5Str);
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

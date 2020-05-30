using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.Bursts
{
    public class UnknownSmall : Burst
    {
        public UnknownSmall(byte[] data) : base((DataType)data[0])
        {
            //I don't know what this whole packet is much less how to interpret the next 3 bytes...
            this.data = data.Skip(1).ToArray();
        }
    }
}

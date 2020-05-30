using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.Bursts
{
    public class VoiceHeader : Burst
    {
        protected bool isProtected;
        protected byte featureID;
        protected byte serviceOptions;
        protected RadioID dest;
        protected RadioID src;

        public VoiceHeader(byte[] data) : base(data)
        {
            this.isProtected = ((data[8] & 0x80) != 0);
            this.featureID = data[9];
            this.serviceOptions = data[10];
            this.dest = new RadioID(data, 11, 3);
            this.src = new RadioID(data, 14, 3);
        }
    }
}

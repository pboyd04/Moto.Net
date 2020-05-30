using Moto.Net.Mototrbo.Bursts.CSBK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.Bursts
{
    public class CSBKBurst : Burst
    {
        protected bool lastBlock;
        protected bool protectFlag;
        protected CSBKOpCode opCode;
        protected byte featureID;

        protected CSBKBurst(CSBKOpCode opCode) : base (DataType.CSBK)
        {
            this.opCode = opCode;
            this.hasSlotType = true;
            this.slotType = 0x03;
            this.syncType = 0x02;
        }

        public CSBKBurst(byte[] data) : base(data)
        {
            this.lastBlock = (this.data[0] & 0x80) != 0;
            this.protectFlag = (this.data[0] & 0x40) != 0;
            this.opCode = (CSBKOpCode)(this.data[0] & 0x3F);
            this.featureID = this.data[1];
            this.data = this.data.Skip(2).ToArray();
        }

        new public static CSBKBurst Decode(byte[] data)
        {
            CSBKOpCode tmp = (CSBKOpCode)(data[8] & 0x3F);
            switch(tmp)
            {
                case CSBKOpCode.MototrboRadioCheck:
                    return new RadioCheck(data);
                case CSBKOpCode.Preamble:
                    return new Preamble(data);
                default:
                    return new CSBKBurst(data);
            }
        }

        public override byte[] Encode()
        {
            byte[] mydata = this.data;
            this.data = new byte[2 + mydata.Length];
            this.data[0] = (byte)this.opCode;
            if (this.lastBlock)
            {
                this.data[0] |= 0x80;
            }
            if (this.protectFlag)
            {
                this.data[0] |= 0x40;
            }
            this.data[1] = this.featureID;
            Array.Copy(mydata, 0, this.data, 2, mydata.Length);
            return base.Encode();
        }

        public CSBKOpCode CSBKOpCode
        {
            get
            {
                return this.opCode;
            }
        }

        public byte FeatureID
        {
            get
            {
                return this.featureID;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.Bursts.CSBK
{
    public class RadioCheck : CSBKBurst
    {
        protected RadioID from;
        protected RadioID to;
        protected bool isAck;

        public RadioCheck(byte[] data) : base(data)
        {
            isAck = (this.data[1] & 0x80) != 0;
            from = new RadioID(this.data, 2, 3);
            to = new RadioID(this.data, 5, 3);
        }

        public RadioCheck(RadioID from, RadioID to) : base(CSBKOpCode.MototrboRadioCheck)
        {
            //Radio Check is motorola specific.
            this.featureID = 0x10;
            this.lastBlock = true;
            this.hasSlotType = true;
            this.slotType = 3;
            this.syncType = 2;
            this.from = from;
            this.to = to;
        }

        public bool IsAck
        {
            get
            {
                return this.isAck;
            }
        }

        public override byte[] Encode()
        {
            this.data = new byte[8];
            if (this.isAck)
            {
                this.data[1] |= 0x80;
            }
            this.from.AddToArray(this.data, 2, 3);
            this.to.AddToArray(this.data, 5, 3);
            return base.Encode();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.Bursts.CSBK
{
    public class Preamble : CSBKBurst
    {
        bool dataNext;
        bool isGroup;
        byte csbkBlocksFollow;
        RadioID target;
        RadioID source;

        public Preamble(byte[] data) : base(data)
        {
            dataNext = ((this.data[0] & 0x80) != 0);
            isGroup = ((this.data[0] & 0x40) != 0);
            csbkBlocksFollow = this.data[1];
            this.target = new RadioID(this.data, 2, 3);
            this.source = new RadioID(this.data, 5, 3);
        }

        public Preamble(byte followingBlocks, RadioID target, RadioID source) : base(CSBKOpCode.Preamble)
        {
            this.csbkBlocksFollow = followingBlocks;
            this.target = target;
            this.source = source;
            this.lastBlock = true;
        }

        public override byte[] Encode()
        {
            this.data = new byte[8];
            if(this.dataNext)
            {
                this.data[0] |= 0x80;
            }
            if(this.isGroup)
            {
                this.data[0] |= 0x40;
            }
            this.data[1] = this.csbkBlocksFollow;
            this.target.AddToArray(this.data, 2, 3);
            this.source.AddToArray(this.data, 5, 3);
            return base.Encode();
        }

        public bool DataNext
        {
            get
            {
                return this.dataNext;
            }
            set
            {
                this.dataNext = value;
            }
        }

        public bool Group
        {
            get
            {
                return this.isGroup;
            }
            set
            {
                this.isGroup = value;
            }
        }

        public byte BlocksFollow
        {
            get
            {
                return this.csbkBlocksFollow;
            }
            set
            {
                this.csbkBlocksFollow = value;
            }
        }

        public RadioID Target
        {
            get
            {
                return this.target;
            }
            set
            {
                this.target = value;
            }
        }

        public RadioID Source
        {
            get
            {
                return this.source;
            }
            set
            {
                this.source = value;
            }
        }

    }
}

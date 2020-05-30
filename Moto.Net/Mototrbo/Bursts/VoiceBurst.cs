using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.Bursts
{
    public class VoiceBurst : Burst
    {
        protected byte flags;
        protected List<byte[]> frames;
        protected bool[] frameErrors;
        protected byte[] lcHardBits;

		public VoiceBurst(byte[] data) : base((DataType)(data[0] & 0x7f))
		{
            this.slot = ((data[0] & 0x80) != 0);
            this.rssiOk = ((data[1] & 0x40) != 0);
            this.rsParity = ((data[1] & 0x04) != 0);
            this.crcParity = ((data[1] & 0x02) != 0);
            this.lcParity = ((data[1] & 0x01) != 0);
            this.flags = data[2];

            this.frames = new List<byte[]>();
            this.frameErrors = new bool[3];

            int x = 0;
            int y = 3;
			for(int i = 0; i < 3; i++)
            {
                byte[] frame = new byte[7];
				for(int j = 0; j < 7; j++)
                {
                    byte tmp = data[y];
					if(x > 0)
                    {
						if(j > 0)
                        {
                            frame[j - 1] |= (byte)(tmp >> (8 - x));
                        }
                        frame[j] = (byte)(tmp << x);
                    }
					else
                    {
                        frame[j] = tmp;
                    }
                    y++;
                }
                frame[6] &= 0x80;
                x += 2;
                y--;
                this.frames.Add(frame);
            }

            this.frameErrors[0] = ((data[2] & 0x01) != 0);
			this.frameErrors[1] = ((data[9] & 0x01) != 0);
            this.frameErrors[1] = ((data[15] & 0x01) != 0);
			if((this.flags & 0x02) != 0)
            {
                this.lcHardBits = new byte[4];
                this.lcHardBits[0] = data[22];
                this.lcHardBits[1] = data[23];
                this.lcHardBits[2] = data[24];
                this.lcHardBits[3] = data[25];
            }
			else if((this.flags & 0x10) != 0)
            {
                throw new NotImplementedException("Embeded LC bits...");
            }
			else if ((this.flags & 0x04) != 0)
            {
                throw new NotImplementedException("EMB...");
            }
        }

		public List<byte[]> Frames
        {
            get
            {
                return this.frames;
            }
        }
    }
}

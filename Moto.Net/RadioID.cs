using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net
{
    public class RadioID
    {
        private UInt32 id;

        public RadioID(UInt32 id)
        {
            this.id = id;
        }

        public RadioID(byte[] array) : this(array, 0, 4)
        {

        }

        public RadioID(byte[] array, int startOffset) : this(array, startOffset, 4)
        {

        }

        public RadioID(byte[] array, int startOffset, int length)
        {
            byte[] res = new byte[4];
            if (length == 4)
            {
                Array.Copy(array, startOffset, res, 0, 4);
            }
            else
            {
                Array.Copy(array, startOffset, res, 1, 3);
            }
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(res);
            }
            this.id = BitConverter.ToUInt32(res, 0);
        }

        public override string ToString()
        {
            return "" + this.id;
        }

        public override bool Equals(object obj)
        {
            if((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            RadioID r = (RadioID)obj;
            return this.id == r.id;
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }

        public UInt32 Int
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public void AddToArray(byte[] array, int startOffset, int length)
        {
            byte[] bytes = BitConverter.GetBytes(this.id);
            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Array.Copy(bytes, (length == 3) ? 1 : 0, array, startOffset, length);
        }
    }
}

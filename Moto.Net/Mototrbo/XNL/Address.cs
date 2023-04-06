using System;

namespace Moto.Net.Mototrbo.XNL
{
    public class Address
    {
        private readonly UInt16 address;

        public Address(UInt16 address)
        {
            this.address = address;
        }

        public Address(byte[] array) : this(array, 0)
        {

        }

        public Address(byte[] array, int startOffset)
        {
            byte[] res = new byte[2];
            Array.Copy(array, startOffset, res, 0, 2);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(res);
            }
            this.address = BitConverter.ToUInt16(res, 0);
        }

        public override string ToString()
        {
            return "" + this.address;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            Address a = (Address)obj;
            return this.address == a.address;
        }

        public override int GetHashCode()
        {
            return this.address.GetHashCode();
        }

        public UInt16 Int
        {
            get
            {
                return this.address;
            }
        }

        public void AddToArray(byte[] array, int startOffset)
        {
            byte[] bytes = BitConverter.GetBytes(this.address);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Array.Copy(bytes, 0, array, startOffset, 2);
        }
    }
}
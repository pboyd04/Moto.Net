using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL
{
    public enum DevConnectionEncrypterType
    {
        RepeaterIPSC,
        Subscriber,
        CPS
    }

    public class DevConnectionRequestPacket : XNLPacket
    {
        protected Address connection;
        protected byte connectionType;
        protected byte connectionIndex;
        protected byte[] key;

        public DevConnectionRequestPacket() : base(OpCode.DeviceConnectionRequest)
        {
        }

        public DevConnectionRequestPacket(Address dest, Address src, Address connectionAddress, byte connectionType, byte connectionIndex, byte[] key, DevConnectionEncrypterType type) : base(OpCode.DeviceConnectionRequest)
        {
            this.dest = dest;
            this.src = src;
            this.connection = connectionAddress;
            this.connectionType = connectionType;
            this.connectionIndex = connectionIndex;
            switch(type)
            {
                case DevConnectionEncrypterType.RepeaterIPSC:
                    this.key = Encrypter.Encrypt(key);
                    break;
                case DevConnectionEncrypterType.Subscriber:
                    this.key = Encrypter.EncryptControlStation(key);
                    break;
                case DevConnectionEncrypterType.CPS:
                    this.key = Encrypter.EncryptCPSRadio(key);
                    break;
            }
            this.data = new byte[4+this.key.Length];
            this.connection.AddToArray(this.data, 0);
            this.data[2] = this.connectionType;
            this.data[3] = this.connectionIndex;
            Array.Copy(this.key, 0, this.data, 4, this.key.Length);
        }
    }
}

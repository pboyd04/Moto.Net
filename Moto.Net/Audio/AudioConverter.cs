namespace Moto.Net.Audio
{
    public abstract class AudioConverter
    {
        public abstract byte[] Decode(byte[] data);
        public abstract byte[] Encode(byte[] data);
    }
}

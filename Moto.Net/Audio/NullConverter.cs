namespace Moto.Net.Audio
{
    //Simple test converter that just copies the data
    public class NullConverter : AudioConverter
    {
        public override byte[] Decode(byte[] data)
        {
            return data;
        }

        public override byte[] Encode(byte[] data)
        {
            return data;
        }
    }
}

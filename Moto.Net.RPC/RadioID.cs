namespace Moto.Net.RPC
{
    public class RadioID
    {
        int id;

        public int Int
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

        public override string ToString()
        {
            return this.id.ToString();
        }
    }
}

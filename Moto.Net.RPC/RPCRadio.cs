namespace Moto.Net.RPC
{
    public class RPCRadio
    {
        protected RadioID id;
        protected string name;
        protected string serialNumber;
        protected string modelNumber;
        protected string firmwareVersion;

        public RadioID ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }

        public string SerialNumber
        {
            get
            {
                return serialNumber;
            }
            set
            {
                serialNumber = value;
            }
        }

        public string ModelNumber
        {
            get
            {
                return modelNumber;
            }
            set
            {
                modelNumber = value;
            }
        }

        public string FirmwareVersion
        {
            get
            {
                return firmwareVersion;
            }
            set
            {
                firmwareVersion = value;
            }
        }
    }
}
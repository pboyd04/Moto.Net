using Moto.Net;
using MySql.Data.MySqlClient;
using System;
using System.Text;

namespace MotoMond
{
    public class DBRadio
    {
        private readonly uint radioId;
        private string name;
        private DateTime lastSeen;
        private float lastRSSI;
        private int samples;
        private double totalRSSI;
        private float minRSSI;
        private DateTime minRSSITime;
        private float maxRSSI;
        private DateTime maxRSSITime;
        //These will only be present if the radio has been connected as a control radio for now...
        private String serialNumber;
        private String modelNumber;
        private String firmwareVersion;

        public DBRadio(uint radioid, float rssi)
        {
            this.radioId = radioid;
            this.name = "";
            this.lastSeen = DateTime.Now;
            this.lastRSSI = rssi;
            this.samples = 1;
            this.totalRSSI = rssi;
            this.minRSSI = rssi;
            this.minRSSITime = DateTime.Now;
            this.maxRSSI = rssi;
            this.maxRSSITime = DateTime.Now;
        }

        public DBRadio(uint radioid, string name, DateTime lastseen, float lastrssi, int samples, double totalrssi, float minrssi, DateTime minrssitime, float maxrssi, DateTime maxrssitime)
        {
            this.radioId = radioid;
            this.name = name;
            this.lastSeen = lastseen;
            this.lastRSSI = lastrssi;
            this.samples = samples;
            this.totalRSSI = totalrssi;
            this.minRSSI = minrssi;
            this.minRSSITime = minrssitime;
            this.maxRSSI = maxrssi;
            this.maxRSSITime = maxrssitime;
        }

        public DBRadio(MySqlDataReader reader, string tableName)
        {
            if (tableName.Equals("repeaters"))
            {
                this.radioId = (uint)reader.GetInt32("id");
                if (!reader.IsDBNull(reader.GetOrdinal("name")))
                {
                    this.name = reader.GetString("name");
                }
                this.serialNumber = reader.GetString("serialnumber");
                this.modelNumber = reader.GetString("modelnumber");
                this.firmwareVersion = reader.GetString("firmwareversion");
                this.lastSeen = reader.GetDateTime("lastseen");
            }
            else
            {
                this.radioId = reader.GetUInt32("id");
                this.name = reader.GetString("name");
                this.lastSeen = reader.GetDateTime("lastseen");
                this.lastRSSI = (float)reader.GetDouble("lastrssi");
                this.totalRSSI = reader.GetDouble("totalrssi");
                this.minRSSI = (float)reader.GetDouble("minrssi");
                this.maxRSSI = (float)reader.GetDouble("maxrssi");
                this.samples = reader.GetInt32("samples");
                this.minRSSITime = reader.GetDateTime("minrssitime");
                this.maxRSSITime = reader.GetDateTime("maxrssitime");
            }
        }

        public RadioID ID
        {
            get
            {
                return new RadioID(radioId);
            }
        }

        public uint RadioId
        {
            get
            {
                return this.radioId;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public DateTime LastSeen
        {
            get
            {
                return this.lastSeen;
            }
        }

        public float LastRSSI
        {
            get
            {
                return this.lastRSSI;
            }
        }

        public int Samples
        {
            get
            {
                return this.samples;
            }
        }

        public double TotalRSSI
        {
            get
            {
                return this.totalRSSI;
            }
        }

        public float MinRSSI
        {
            get
            {
                return this.minRSSI;
            }
        }

        public DateTime MinRSSITime
        {
            get
            { 
                return this.minRSSITime;
            }
        }

        public float MaxRSSI
        {
            get
            {
                return this.maxRSSI;
            }
        }

        public DateTime MaxRSSITime
        {
            get
            {
                return this.maxRSSITime;
            }
        }

        public void AddValues(DBRadio b)
        {
            if ((this.name.Length == 0 && b.name.Length > 0))
            {
                this.name = b.name;
            }
            if (this.lastSeen < b.lastSeen)
            {
                this.lastSeen = b.lastSeen;
            }
            this.lastRSSI = b.lastRSSI;
            this.totalRSSI = b.totalRSSI;
            this.minRSSI = b.minRSSI;
            this.maxRSSI = b.maxRSSI;
            this.samples = b.samples;
            this.minRSSITime = b.minRSSITime;
            this.maxRSSITime = b.maxRSSITime;
        }

        public void AddReading(float rssi)
        {
            this.samples++;
            this.totalRSSI += rssi;
            this.lastRSSI = rssi;
            this.lastSeen = DateTime.Now;
            if(rssi < this.minRSSI)
            {
                this.minRSSI = rssi;
                this.minRSSITime = DateTime.Now;
            }
            if(rssi > this.maxRSSI)
            {
                this.maxRSSI = rssi;
                this.maxRSSITime = DateTime.Now;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.AppendLine(":");
            sb.AppendFormat(" ID:              {0}\n", this.radioId);
            sb.AppendFormat(" Name:            {0}\n", this.Name);
            sb.AppendFormat(" LastSeen:        {0}\n", this.lastSeen);
            if (this.lastRSSI != 0)
            {
                sb.AppendFormat(" LastRSSI:        {0}\n", this.lastRSSI);
                sb.AppendFormat(" Average RSSI:    {0}\n", this.totalRSSI / this.samples);
                sb.AppendFormat(" MinRSSI:         {0} @ {1}\n", this.minRSSI, this.minRSSITime);
                sb.AppendFormat(" MaxRSSI:         {0} @ {1}\n", this.maxRSSI, this.maxRSSITime);
            }
            if(this.serialNumber != null)
            {
                sb.AppendFormat(" SerialNumber:    {0}\n", this.serialNumber);
                sb.AppendFormat(" ModelNumber:     {0}\n", this.modelNumber);
                sb.AppendFormat(" FirmwareVersion: {0}\n", this.firmwareVersion);
            }
            return sb.ToString();
        }
    }
}

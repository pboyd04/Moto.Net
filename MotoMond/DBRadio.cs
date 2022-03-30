using Moto.Net;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotoMond
{
    public class DBRadio
    {
        public uint RadioId;
        public string Name;
        public DateTime LastSeen;
        public float LastRSSI;
        public int Samples;
        public double TotalRSSI;
        public float MinRSSI;
        public DateTime MinRSSITime;
        public float MaxRSSI;
        public DateTime MaxRSSITime;
        //These will only be present if the radio has been connected as a control radio for now...
        public String SerialNumber;
        public String ModelNumber;
        public String FirmwareVersion;

        public DBRadio(uint radioid, float rssi)
        {
            this.RadioId = radioid;
            this.Name = "";
            this.LastSeen = DateTime.Now;
            this.LastRSSI = rssi;
            this.Samples = 1;
            this.TotalRSSI = rssi;
            this.MinRSSI = rssi;
            this.MinRSSITime = DateTime.Now;
            this.MaxRSSI = rssi;
            this.MaxRSSITime = DateTime.Now;
        }

        public DBRadio(uint radioid, string name, DateTime lastseen, float lastrssi, int samples, double totalrssi, float minrssi, DateTime minrssitime, float maxrssi, DateTime maxrssitime)
        {
            this.RadioId = radioid;
            this.Name = name;
            this.LastSeen = lastseen;
            this.LastRSSI = lastrssi;
            this.Samples = samples;
            this.TotalRSSI = totalrssi;
            this.MinRSSI = minrssi;
            this.MinRSSITime = minrssitime;
            this.MaxRSSI = maxrssi;
            this.MaxRSSITime = maxrssitime;
        }

        public DBRadio(MySqlDataReader reader, string tableName)
        {
            if (tableName.Equals("repeaters"))
            {
                this.RadioId = (uint)reader.GetInt32("id");
                if (!reader.IsDBNull(reader.GetOrdinal("name")))
                {
                    this.Name = reader.GetString("name");
                }
                this.SerialNumber = reader.GetString("serialnumber");
                this.ModelNumber = reader.GetString("modelnumber");
                this.FirmwareVersion = reader.GetString("firmwareversion");
                this.LastSeen = reader.GetDateTime("lastseen");
            }
            else
            {
                this.RadioId = reader.GetUInt32("id");
                this.Name = reader.GetString("name");
                this.LastSeen = reader.GetDateTime("lastseen");
                this.LastRSSI = (float)reader.GetDouble("lastrssi");
                this.TotalRSSI = reader.GetDouble("totalrssi");
                this.MinRSSI = (float)reader.GetDouble("minrssi");
                this.MaxRSSI = (float)reader.GetDouble("maxrssi");
                this.Samples = reader.GetInt32("samples");
                this.MinRSSITime = reader.GetDateTime("minrssitime");
                this.MaxRSSITime = reader.GetDateTime("maxrssitime");
            }
        }

        public RadioID ID
        {
            get
            {
                return new RadioID(RadioId);
            }
        }

        public void AddValues(DBRadio b)
        {
            if ((this.Name == null && b.Name != null) || (this.Name.Length == 0 && b.Name.Length > 0))
            {
                this.Name = b.Name;
            }
            if (this.LastSeen < b.LastSeen)
            {
                this.LastSeen = b.LastSeen;
            }
            this.LastRSSI = b.LastRSSI;
            this.TotalRSSI = b.TotalRSSI;
            this.MinRSSI = b.MinRSSI;
            this.MaxRSSI = b.MaxRSSI;
            this.Samples = b.Samples;
            this.MinRSSITime = b.MinRSSITime;
            this.MaxRSSITime = b.MaxRSSITime;
        }

        public void AddReading(float rssi)
        {
            this.Samples++;
            this.TotalRSSI += rssi;
            this.LastRSSI = rssi;
            this.LastSeen = DateTime.Now;
            if(rssi < this.MinRSSI)
            {
                this.MinRSSI = rssi;
                this.MinRSSITime = DateTime.Now;
            }
            if(rssi > this.MaxRSSI)
            {
                this.MaxRSSI = rssi;
                this.MaxRSSITime = DateTime.Now;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.AppendLine(":");
            sb.AppendFormat(" ID:              {0}\n", this.RadioId);
            sb.AppendFormat(" Name:            {0}\n", this.Name);
            sb.AppendFormat(" LastSeen:        {0}\n", this.LastSeen);
            if (this.LastRSSI != 0)
            {
                sb.AppendFormat(" LastRSSI:        {0}\n", this.LastRSSI);
                sb.AppendFormat(" Average RSSI:    {0}\n", this.TotalRSSI / this.Samples);
                sb.AppendFormat(" MinRSSI:         {0} @ {1}\n", this.MinRSSI, this.MinRSSITime);
                sb.AppendFormat(" MaxRSSI:         {0} @ {1}\n", this.MaxRSSI, this.MaxRSSITime);
            }
            if(this.SerialNumber != null)
            {
                sb.AppendFormat(" SerialNumber:    {0}\n", this.SerialNumber);
                sb.AppendFormat(" ModelNumber:     {0}\n", this.ModelNumber);
                sb.AppendFormat(" FirmwareVersion: {0}\n", this.FirmwareVersion);
            }
            return sb.ToString();
        }
    }
}

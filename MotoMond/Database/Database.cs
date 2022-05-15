using Moto.Net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotoMond.Database
{
    public abstract class Database : ConfigurationSection, IDatabase
    {
        protected bool disposedValue;
        protected string[] tables;

        [ConfigurationProperty("ConnectionString")]
        public DBElement ConnectionStringElement
        {
            get { return this["ConnectionString"] as DBElement; }
            set { this["ConnectionString"] = value; }
        }

        [ConfigurationProperty("Tables")]
        public DBElement TablesElement
        {
            get { return this["Tables"] as DBElement; }
            set { this["Tables"] = value; }
        }

        [ConfigurationProperty("Type")]
        public DBElement TypeElement
        {
            get { return this["Type"] as DBElement; }
            set { this["Type"] = value; }
        }

        protected override void PostDeserialize()
        {
            base.PostDeserialize();
            this.Connect();
            this.tables = this.TablesElement.InnerText.Split(',');
        }

        protected bool ShouldHaveTable(string tableName)
        {
            return this.tables.Contains(tableName);
        }

        public abstract void Connect();
        public abstract void CreateTables();
        public abstract string UpdateConnectedRadio(RadioID id, string serialNum, string modelNum, string fwver);
        public abstract bool SetNameByID(RadioID id, string name);
        public abstract List<DBRadio> ReadRadios();
        public abstract DBRadio ReadRadio(RadioID id);
        public abstract void WriteVoiceCall(RadioID from, RadioID to, DateTime start, DateTime end, float rssi, int slot, string filename);
        public abstract void UpdateRadio(DBRadio radio);
        public abstract void WriteRSSI(RadioID id, Tuple<float, float> rssis);
        public abstract void WriteLocation(RadioID id, float lat, float lon, float? rssi);

        protected abstract void Dispose(bool disposing);

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Database()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

using Moto.Net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MotoMond.Database
{
    public class DBElement : ConfigurationElement
    {
        public string InnerText { get; private set; }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            InnerText = reader.ReadElementContentAsString();
        }
    }

    public interface IDatabase : IDisposable
    {
        [ConfigurationProperty("ConnectionString")]
        DBElement ConnectionStringElement
        {
            get; set;
        }

        [ConfigurationProperty("Tables")]
        DBElement TablesElement
        {
            get; set;
        }

        [ConfigurationProperty("Type")]
        DBElement TypeElement
        {
            get; set;
        }

        void Connect();
        void CreateTables();
        string UpdateConnectedRadio(Moto.Net.RadioID id, string serialNum, string modelNum, string fwver);
        bool SetNameByID(Moto.Net.RadioID id, string name);
        List<DBRadio> ReadRadios();
        DBRadio ReadRadio(Moto.Net.RadioID id);
        void WriteVoiceCall(Moto.Net.RadioID from, Moto.Net.RadioID to, DateTime start, DateTime end, float rssi, int slot, string filename);
        void UpdateRadio(DBRadio radio);
        void WriteRSSI(Moto.Net.RadioID id, Tuple<float, float> rssis);
        void WriteLocation(Moto.Net.RadioID id, float lat, float lon, float? rssi);
    }
}

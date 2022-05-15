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
        string UpdateConnectedRadio(RadioID id, string serialNum, string modelNum, string fwver);
        bool SetNameByID(RadioID id, string name);
        List<DBRadio> ReadRadios();
        DBRadio ReadRadio(RadioID id);
        void WriteVoiceCall(RadioID from, RadioID to, DateTime start, DateTime end, float rssi, int slot, string filename);
        void UpdateRadio(DBRadio radio);
        void WriteRSSI(RadioID id, Tuple<float, float> rssis);
        void WriteLocation(RadioID id, float lat, float lon, float? rssi);
    }
}

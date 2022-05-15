using Moto.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotoMond.Database
{
    public class DatabaseMultiPlexer : Database
    {
        List<IDatabase> children = new List<IDatabase>();

        public void AddChild(IDatabase db)
        {
            children.Add(db);
        }

        public override void Connect()
        {
            foreach(IDatabase db in children)
            {
                db.Connect();
            }
        }

        public override void WriteLocation(RadioID id, float lat, float lon, float? rssi)
        {
            foreach (IDatabase db in children)
            {
                db.WriteLocation(id, lat, lon, rssi);
            }
        }

        public override bool SetNameByID(RadioID id, string name)
        {
            bool ret = false;
            foreach (IDatabase db in children)
            {
                if(db.SetNameByID(id, name))
                {
                    ret = true;
                }
            }
            //At least one worked if this is true
            return ret;
        }

        public override string UpdateConnectedRadio(RadioID id, string serialNum, string modelNum, string fwver)
        {
            string ret = String.Empty;
            foreach (IDatabase db in children)
            {
                string tmp = db.UpdateConnectedRadio(id, serialNum, modelNum, fwver);
                if(ret == String.Empty && tmp != String.Empty)
                {
                    ret = tmp;
                }
            }
            //Will return the first name from the first DB
            return ret;
        }

        public override DBRadio ReadRadio(RadioID id)
        {
            foreach(IDatabase db in children)
            {
                DBRadio tmp = db.ReadRadio(id);
                if(tmp != null)
                {
                    return tmp;
                }
            }
            return null;
        }

        public override void UpdateRadio(DBRadio radio)
        {
            foreach (IDatabase db in children)
            {
                db.UpdateRadio(radio);
            }
        }

        public override List<DBRadio> ReadRadios()
        {
            HashSet<DBRadio> tmp = new HashSet<DBRadio>();
            foreach (IDatabase db in children)
            {
                List<DBRadio> dBRadios = db.ReadRadios();
                foreach(DBRadio r in dBRadios)
                {
                    tmp.Add(r);
                }
            }
            return tmp.ToList();
        }

        public override void WriteVoiceCall(RadioID from, RadioID to, DateTime start, DateTime end, float rssi, int slot, string filename)
        {
            foreach(IDatabase db in children)
            {
                db.WriteVoiceCall(from, to, start, end, rssi, slot, filename);
            }
        }

        public override void CreateTables()
        {
            foreach(IDatabase db in children)
            {
                db.CreateTables();
            }
        }

        public override void WriteRSSI(RadioID id, Tuple<float, float> rssis)
        {
            foreach(IDatabase db in children)
            {
                db.WriteRSSI(id, rssis);
            }
        }

        protected override void Dispose(bool disposing)
        {
            foreach(IDatabase db in children)
            {
                db.Dispose();
            }
            this.children.Clear();
        }
    }
}

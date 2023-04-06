using InfluxDB.Client;
using InfluxDB.Client.Writes;
using InfluxDB.Client.Api.Domain;
using Moto.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotoMond.Database
{
    public class InfluxDB : Database
    {
        InfluxDBClient conn;

        public override void Connect()
        {
            string connString = this.ConnectionStringElement.InnerText;
            conn = InfluxDBClientFactory.Create(connString);
        }

        public override void CreateTables()
        {
            //You don't have to create tables on Influx
        }

        public override DBRadio ReadRadio(RadioID id)
        {
            if (!this.ShouldHaveTable("radio"))
            {
                return null;
            }
            throw new NotImplementedException();
        }

        public override List<DBRadio> ReadRadios()
        {
            if (!this.ShouldHaveTable("radio"))
            {
                return new List<DBRadio>();
            }
            throw new NotImplementedException();
        }

        public override bool SetNameByID(RadioID id, string name)
        {
            if (!this.ShouldHaveTable("radio"))
            {
                return false;
            }
            throw new NotImplementedException();
        }

        public override string UpdateConnectedRadio(RadioID id, string serialNum, string modelNum, string fwver)
        {
            if(!this.ShouldHaveTable("connectedRadios"))
            {
                return String.Empty;
            }
            throw new NotImplementedException();
        }

        public override void UpdateRadio(DBRadio radio)
        {
            if (!this.ShouldHaveTable("radio"))
            {
                return;
            }
            throw new NotImplementedException();
        }

        public override void WriteLocation(RadioID id, float lat, float lon, float? rssi)
        {
            if (!this.ShouldHaveTable("location"))
            {
                return;
            }
            throw new NotImplementedException();
        }

        public override void WriteRSSI(RadioID id, Tuple<float, float> rssis)
        {
            if (!this.ShouldHaveTable("rssi"))
            {
                return;
            }
            PointData[] points = new PointData[2];
            points[0] = PointData.Measurement("rssi").Tag("RadioID", id.ToString()).Tag("TimeSlot", "1").Field("value", rssis.Item1).Timestamp(DateTime.UtcNow, WritePrecision.S);
            points[1] = PointData.Measurement("rssi").Tag("RadioID", id.ToString()).Tag("TimeSlot", "2").Field("value", rssis.Item2).Timestamp(DateTime.UtcNow, WritePrecision.S);
            conn.GetWriteApi().WritePoints(points);
        }

        public override void WriteVoiceCall(RadioID from, RadioID to, DateTime start, DateTime end, float rssi, int slot, string filename)
        {
            if (!this.ShouldHaveTable("voicecalls"))
            {
                return;
            }
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(conn != null)
                {
                    conn.Dispose();
                    conn = null;
                }
            }
            this.disposedValue = true;
        }
    }
}

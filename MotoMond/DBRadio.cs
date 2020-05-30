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
    }
}

using Moto.Net;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotoMond.Database
{
    public class MySQLDB : Database
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected MySqlConnection conn;

        public override void Connect()
        {
            this.conn = new MySqlConnection(this.ConnectionStringElement.InnerText);
            this.conn.Open();
        }

        public override void CreateTables()
        {
            MySqlCommand command;
            if (this.ShouldHaveTable("connectedRadios"))
            {
                command = conn.CreateCommand();
                command.CommandText = "CREATE TABLE IF NOT EXISTS connectedRadios(id INT NOT NULL, name VARCHAR(100), serialnumber VARCHAR(12), modelnumber VARCHAR(14), firmwareversion VARCHAR(12), lastseen DATETIME, PRIMARY KEY (id));";
                command.ExecuteNonQuery();
            }
            if (this.ShouldHaveTable("rssi"))
            {
                command = conn.CreateCommand();
                command.CommandText = "CREATE TABLE IF NOT EXISTS rssi(rssiid INT NOT NULL AUTO_INCREMENT, radioid INT NOT NULL, time DATETIME, rssi1 DOUBLE, rssi2 DOUBLE, PRIMARY KEY (rssiid));";
                command.ExecuteNonQuery();
            }
            if (this.ShouldHaveTable("voicecalls"))
            {
                command = conn.CreateCommand();
                command.CommandText = "CREATE TABLE IF NOT EXISTS voicecalls(callid INT NOT NULL AUTO_INCREMENT, source INT, dest INT, start DATETIME, end DATETIME, rssi DOUBLE, slot INT, recordingpath VARCHAR(255), PRIMARY KEY (callid));";
                command.ExecuteNonQuery();
            }
            if (this.ShouldHaveTable("radio"))
            {
                command = conn.CreateCommand();
                command.CommandText = "CREATE TABLE IF NOT EXISTS radio(id INT UNSIGNED NOT NULL, name VARCHAR(100), lastseen DATETIME, lastrssi FLOAT, samples INT, totalrssi DOUBLE, minrssi FLOAT, minrssitime DATETIME, maxrssi FLOAT, maxrssitime DATETIME, PRIMARY KEY(`id`));";
                command.ExecuteNonQuery();
            }
            if (this.ShouldHaveTable("location"))
            {
                command = conn.CreateCommand();
                command.CommandText = "CREATE TABLE IF NOT EXISTS location(id INT UNSIGNED NOT NULL AUTO_INCREMENT, radioid INT NOT NULL, latitude FLOAT, longitude FLOAT, rssi FLOAT, timestamp DATETIME, PRIMARY KEY(`id`));";
                command.ExecuteNonQuery();
            }
        }

        public override DBRadio ReadRadio(RadioID id)
        {
            if(!this.ShouldHaveTable("radio"))
            {
                return null;
            }
            string sql = "SELECT * FROM radio WHERE id=@id";
            MySqlCommand cmd = new MySqlCommand(sql, this.conn);
            cmd.Parameters.AddWithValue("@id", id.Int);
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
                    return new DBRadio(rdr.GetUInt32(0), rdr.GetString(1), rdr.GetDateTime(2), rdr.GetFloat(3), rdr.GetInt32(4), rdr.GetDouble(5), rdr.GetFloat(6), rdr.GetDateTime(7), rdr.GetFloat(8), rdr.GetDateTime(9));
                }
                return null;
            }
        }

        public override List<DBRadio> ReadRadios()
        {
            List<DBRadio> ret = new List<DBRadio>();
            MySqlCommand cmd;
            if (this.ShouldHaveTable("connectedRadios"))
            {
                cmd = new MySqlCommand("SELECT * from connectedRadios;", this.conn);
                cmd.Prepare();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ret.Add(new DBRadio(reader, "repeaters"));
                    }
                }
            }
            if (this.ShouldHaveTable("radio"))
            {
                cmd = new MySqlCommand("SELECT * from radio;", this.conn);
                cmd.Prepare();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DBRadio r = new DBRadio(reader, "radio");
                        DBRadio old = ret.Find(i => i.RadioId == r.RadioId);
                        if (old == null)
                        {
                            ret.Add(r);
                        }
                        else
                        {
                            old.AddValues(r);
                        }
                    }
                }
            }
            return ret;
        }

        public override bool SetNameByID(RadioID id, string name)
        {
            bool success = false;
            string sql;
            MySqlCommand cmd;
            if (this.ShouldHaveTable("connectedRadios"))
            {
                sql = "UPDATE connectedRadios SET name=@name WHERE id=@id;";
                cmd = new MySqlCommand(sql, this.conn);
                cmd.Parameters.AddWithValue("@id", id.Int);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Prepare();
                if (cmd.ExecuteNonQuery() == 1)
                {
                    success = true;
                }
            }
            if (this.ShouldHaveTable("radio"))
            {
                sql = "UPDATE radio SET name=@name WHERE id=@id;";
                cmd = new MySqlCommand(sql, this.conn);
                cmd.Parameters.AddWithValue("@id", id.Int);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Prepare();
                if (cmd.ExecuteNonQuery() == 1)
                {
                    success = true;
                }
            }
            return success;
        }

        public override string UpdateConnectedRadio(RadioID id, string serialNum, string modelNum, string fwver)
        {
            if(!this.ShouldHaveTable("connectedRadios"))
            {
                return String.Empty;
            }
            string sql = "INSERT INTO connectedRadios(id, serialnumber, modelnumber, firmwareversion, lastseen) VALUES (@id, @serialnumber, @modelnumber, @fw, NOW()) ON DUPLICATE KEY UPDATE serialnumber=@serialnumber, modelnumber=@modelnumber, firmwareversion=@fw, lastseen=NOW();";
            MySqlCommand cmd = new MySqlCommand(sql, this.conn);
            cmd.Parameters.AddWithValue("@id", id.Int);
            cmd.Parameters.AddWithValue("@serialnumber", serialNum);
            cmd.Parameters.AddWithValue("@modelnumber", modelNum);
            cmd.Parameters.AddWithValue("@fw", fwver);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand("SELECT name from connectedRadios WHERE id=" + id.Int, this.conn);
            object test = cmd.ExecuteScalar();
            if (test.GetType() == typeof(DBNull))
            {
                return String.Empty;
            }
            return (string)test;
        }

        public override void UpdateRadio(DBRadio radio)
        {
            if(!this.ShouldHaveTable("radio"))
            {
                return;
            }
            string sql = "INSERT INTO radio(id, name, lastseen, lastrssi, samples, totalrssi, minrssi, minrssitime, maxrssi, maxrssitime) VALUES (@id, @name, @lastseen, @lastrssi, @samples, @totalrssi, @minrssi, @minrssitime, @maxrssi, @maxrssitime) ON DUPLICATE KEY UPDATE id=@id, name=@name, lastseen=@lastseen, samples=@samples, totalrssi=@totalrssi, minrssi=@minrssi, minrssitime=@minrssitime, maxrssi=@maxrssi, maxrssitime=@maxrssitime;";
            MySqlCommand cmd = new MySqlCommand(sql, this.conn);
            cmd.Parameters.AddWithValue("@id", radio.RadioId);
            cmd.Parameters.AddWithValue("@name", radio.Name);
            cmd.Parameters.AddWithValue("@lastseen", radio.LastSeen);
            cmd.Parameters.AddWithValue("@lastrssi", radio.LastRSSI);
            cmd.Parameters.AddWithValue("@samples", radio.Samples);
            cmd.Parameters.AddWithValue("@totalrssi", radio.TotalRSSI);
            cmd.Parameters.AddWithValue("@minrssi", radio.MinRSSI);
            cmd.Parameters.AddWithValue("@minrssitime", radio.MinRSSITime);
            cmd.Parameters.AddWithValue("@maxrssi", radio.MaxRSSI);
            cmd.Parameters.AddWithValue("@maxrssitime", radio.MaxRSSITime);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public override void WriteLocation(RadioID id, float lat, float lon, float? rssi)
        {
            if (!this.ShouldHaveTable("location"))
            {
                return;
            }
            string sql = "INSERT INTO location(radioid, latitude, longitude, rssi, timestamp) VALUES (@id, @lat, @long, @rssi, NOW());";
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", id.Int);
                cmd.Parameters.AddWithValue("@lat", lat);
                cmd.Parameters.AddWithValue("@long", lon);
                cmd.Parameters.AddWithValue("@rssi", rssi);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }

        public override void WriteRSSI(RadioID id, Tuple<float, float> rssis)
        {
            if (!this.ShouldHaveTable("rssi"))
            {
                return;
            }
            string sql = "INSERT INTO rssi(radioid, time, rssi1, rssi2) VALUES (@id, NOW(), @rssi1, @rssi2);";
            MySqlCommand cmd = new MySqlCommand(sql, this.conn);
            cmd.Parameters.AddWithValue("@id", id.Int);
            cmd.Parameters.AddWithValue("@rssi1", rssis.Item1);
            cmd.Parameters.AddWithValue("@rssi2", rssis.Item2);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public override void WriteVoiceCall(RadioID from, RadioID to, DateTime start, DateTime end, float rssi, int slot, string filename)
        {
            if (!this.ShouldHaveTable("voicecalls"))
            {
                return;
            }
            string sql = "INSERT INTO voicecalls(source, dest, start, end, rssi, slot, recordingpath) VALUES (@src, @dest, @start, @end, @rssi, @slot, @path);";
            MySqlCommand cmd = new MySqlCommand(sql, this.conn);
            cmd.Parameters.AddWithValue("@src", from.Int);
            cmd.Parameters.AddWithValue("@dest", to.Int);
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);
            cmd.Parameters.AddWithValue("@rssi", rssi);
            cmd.Parameters.AddWithValue("@slot", slot);
            cmd.Parameters.AddWithValue("@path", filename);
            cmd.Prepare();
            try
            {
                cmd.ExecuteNonQuery();
            } catch(MySql.Data.MySqlClient.MySqlException ex)
            {
                log.ErrorFormat("Failed to insert voice call into db {0}. RSSI {1}", ex.Message, rssi);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }
            this.disposedValue = true;
        }
    }
}

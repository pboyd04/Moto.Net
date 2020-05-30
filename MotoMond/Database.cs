using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moto.Net;
using Moto.Net.Mototrbo.Bursts;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.BC;

namespace MotoMond
{
    public class Database
    {
        protected MySqlConnection conn;

        public Database(string connStr)
        {
            this.conn = new MySqlConnection(connStr);
            this.conn.Open();
        }

        ~Database()
        {
            if(conn != null)
            {
                conn.Close();
                conn = null;
            }
        }

        public bool IsSetup
        {
            get
            {
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "SELECT * FROM repeaters;";
                MySqlDataReader reader;
                try
                {
                    reader = command.ExecuteReader();
                    reader.Close();
                    return true;
                }
                catch 
                {
                    return false;
                }
            }
        }

        public void CreateTables()
        {
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "CREATE TABLE repeaters(id INT NOT NULL, name VARCHAR(100), serialnumber VARCHAR(12), modelnumber VARCHAR(14), firmwareversion VARCHAR(12), lastseen DATETIME, PRIMARY KEY (id));";
            command.ExecuteNonQuery();

            command = conn.CreateCommand();
            command.CommandText = "CREATE TABLE rssi(rssiid INT NOT NULL AUTO_INCREMENT, radioid INT NOT NULL, time DATETIME, rssi1 DOUBLE, rssi2 DOUBLE, PRIMARY KEY (rssiid));";
            command.ExecuteNonQuery();

            command = conn.CreateCommand();
            command.CommandText = "CREATE TABLE voicecalls(callid INT NOT NULL AUTO_INCREMENT, source INT, dest INT, start DATETIME, end DATETIME, rssi DOUBLE, slot INT, recordingpath VARCHAR(255), PRIMARY KEY (callid));";
            command.ExecuteNonQuery();

            command = conn.CreateCommand();
            command.CommandText = "CREATE TABLE radio(id INT UNSIGNED NOT NULL, name VARCHAR(100), lastseen DATETIME, lastrssi FLOAT, samples INT, totalrssi DOUBLE, minrssi FLOAT, minrssitime DATETIME, maxrssi FLOAT, maxrssitime DATETIME, PRIMARY KEY(`id`));";
            command.ExecuteNonQuery();
        }

        public string UpdateRepeater(RadioID id, string serialNum, string modelNum, string fwver)
        {
            string sql = "INSERT INTO repeaters(id, serialnumber, modelnumber, firmwareversion, lastseen) VALUES (@id, @serialnumber, @modelnumber, @fw, NOW()) ON DUPLICATE KEY UPDATE serialnumber=@serialnumber, modelnumber=@modelnumber, firmwareversion=@fw, lastseen=NOW();";
            MySqlCommand cmd = new MySqlCommand(sql, this.conn);
            cmd.Parameters.AddWithValue("@id", id.Int);
            cmd.Parameters.AddWithValue("@serialnumber", serialNum);
            cmd.Parameters.AddWithValue("@modelnumber", modelNum);
            cmd.Parameters.AddWithValue("@fw", fwver);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand("SELECT name from repeaters WHERE id="+id.Int, this.conn);
            object test = cmd.ExecuteScalar();
            if(test.GetType() == typeof(DBNull))
            {
                return String.Empty;
            }
            return (string)test;
        }

        public void WriteVoiceCall(RadioID from, RadioID to, DateTime start, DateTime end, float rssi, int slot, string filename)
        {
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
            cmd.ExecuteNonQuery();
        }

        public DBRadio ReadRadio(RadioID id)
        {
            string sql = "SELECT * FROM radio WHERE id=@id";
            MySqlCommand cmd = new MySqlCommand(sql, this.conn);
            cmd.Parameters.AddWithValue("@id", id.Int);
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                if(rdr.Read())
                {
                    return new DBRadio(rdr.GetUInt32(0), rdr.GetString(1), rdr.GetDateTime(2), rdr.GetFloat(3), rdr.GetInt32(4), rdr.GetDouble(5), rdr.GetFloat(6), rdr.GetDateTime(7), rdr.GetFloat(8), rdr.GetDateTime(9));
                }
                return null;
            }
        }

        public void UpdateRadio(DBRadio radio)
        {
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

        public void WriteRSSI(RadioID id, Tuple<float, float> rssis)
        {
            string sql = "INSERT INTO rssi(radioid, time, rssi1, rssi2) VALUES (@id, NOW(), @rssi1, @rssi2);";
            MySqlCommand cmd = new MySqlCommand(sql, this.conn);
            cmd.Parameters.AddWithValue("@id", id.Int);
            cmd.Parameters.AddWithValue("@rssi1", rssis.Item1);
            cmd.Parameters.AddWithValue("@rssi2", rssis.Item2);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
    }
}

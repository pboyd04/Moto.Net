using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection.Emit;
using System.Threading;
using Moto.Net;
using Moto.Net.Mototrbo;
using Moto.Net.Mototrbo.Bursts;
using Moto.Net.Mototrbo.Bursts.CSBK;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Net;
using Moto.Net.Mototrbo.LRRP;
using PcapDotNet.Packets.IpV4;
using Moto.Net.Mototrbo.TMS;
using System.Text;

namespace MotoMond
{
    class Program
    {
        static Database db;
        static RPCServer srv;
        static RadioSystem sys;
        static LRRPClient lrrp;
        static TMSClient tms;

        static void Main(string[] args)
        {
            bool go = true;

            srv = new RPCServer();

            db = new Database(ConfigurationManager.AppSettings.Get("dbConnectionString"));
            if(!db.IsSetup)
            {
                Console.WriteLine("Seting up DB...");
                db.CreateTables();
            }

            string masterIP = "192.168.0.100";
            int masterPort = 50000;
            if (args.Length > 1)
            {
                string[] parts = args[1].Split(':');
                masterIP = parts[0];
                if(parts.Length > 1)
                {
                    masterPort = int.Parse(parts[1]);
                }
            }

            RadioSystemType type = (RadioSystemType)RadioSystemType.Parse(typeof(RadioSystemType), ConfigurationManager.AppSettings.Get("systemType"));

            sys = new RadioSystem(uint.Parse(ConfigurationManager.AppSettings.Get("systemId")), type);
            srv.SetSystem(sys);
            sys.GotRadioCall += HandleUserCall;
            Radio master = sys.ConnectToMaster(masterIP, masterPort);
            Console.WriteLine("Master ID = {0}", master.ID);
            master.InitXNL();
            Console.WriteLine("    XNL ID = {0}", master.XNLID);
            Console.WriteLine("    XCMP Version = {0}", master.XCMPVersion);
            string serialNum = master.SerialNumber;
            string modelNum = master.ModelNumber;
            string fwver = master.FirmwareVersion;
            Console.WriteLine("    Serial Number = {0}", serialNum);
            Console.WriteLine("    Model Number = {0}", modelNum);
            Console.WriteLine("    Firmware Version = {0}", fwver);
            Console.WriteLine("    Alarms:");
            Dictionary<string, bool> alarms = master.GetAlarmStatus();
            foreach(KeyValuePair<string, bool> kvp in alarms)
            {
                Console.WriteLine("        {0}: {1}", kvp.Key, kvp.Value);
            }
            string name = db.UpdateRepeater(master.ID, serialNum, modelNum, fwver);
            sys.Master.Name = name;
            Tuple<float, float> rssis = master.RSSI;
            Console.WriteLine("    RSSI: {0} {1}", rssis.Item1, rssis.Item2);
            db.WriteRSSI(master.ID, rssis);
            Radio[] radios = sys.GetPeers();
            Console.WriteLine("Found {0} other radios...", radios.Length);
            foreach(Radio r in radios)
            {
                PeerRadio pr = (PeerRadio)r;
                pr.SendPeerRegistration();
                Console.WriteLine("Peer ID = {0}", r.ID);
                Console.WriteLine("    Peer IP = {0}", pr.Endpoint);
                if(r.InitXNL() == false)
                {
                    Console.WriteLine("    Retrying peer init!");
                    pr.SendPeerRegistration();
                    r.InitXNL();
                }
                Console.WriteLine("    XNL ID = {0}", r.XNLID);
                Console.WriteLine("    XCMP Version = {0}", r.XCMPVersion);
                serialNum = r.SerialNumber;
                modelNum = r.ModelNumber;
                fwver = r.FirmwareVersion;
                Console.WriteLine("    Serial Number = {0}", serialNum);
                Console.WriteLine("    Model Number = {0}", modelNum);
                Console.WriteLine("    Firmware Version = {0}", fwver);
                Console.WriteLine("    Alarms:");
                alarms = r.GetAlarmStatus();
                foreach (KeyValuePair<string, bool> kvp in alarms)
                {
                    Console.WriteLine("        {0}: {1}", kvp.Key, kvp.Value);
                }
                name = db.UpdateRepeater(r.ID, serialNum, modelNum, fwver);
                r.Name = name;
                rssis = r.RSSI;
                Console.WriteLine("    RSSI: {0} {1}", rssis.Item1, rssis.Item2);
                db.WriteRSSI(r.ID, rssis);
            }
            System.Timers.Timer t = new System.Timers.Timer(30000);
            t.Elapsed += GetRSSI;
            t.Enabled = true;
            t.AutoReset = true;
            lrrp = new LRRPClient();
            tms = new TMSClient();
            sys.RegisterLRRPClient(lrrp);
            sys.RegisterTMSClient(tms);
            CommandProcessor cmd = new CommandProcessor(sys, lrrp, tms);

            while (go)
            {
                Console.Write("cmd> ");
                string cmdStr = Console.ReadLine();
                string[] parts = cmdStr.Split(' ');
                switch(parts[0])
                {
                    case "exit":
                        go = false;
                        break;
                    case "":
                        //Just print the new command line
                        break;
                    default:
                        CommandResult res = cmd.ProcessCommand(parts[0], parts.Skip(1).ToArray());
                        if(res.Success == false)
                        {
                            if (res.ex != null)
                            {
                                Console.WriteLine("Command Failed! "+res.ex);
                            }
                            else
                            {
                                Console.WriteLine("Command Failed!");
                            }
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach(KeyValuePair<string, object> pair in res.Data)
                            {
                                sb.Append(pair.Key + ": " + pair.Value + ", ");
                            }
                            Console.WriteLine("Success! "+sb.ToString());
                        }
                        break;
                }
            }
            sys.Dispose();
            lrrp.Dispose();
            tms.Dispose();
        }

        private static void GetRSSI(Object src, System.Timers.ElapsedEventArgs e)
        {
            if (sys.Master.ActiveCallCount == 0)
            {
                Tuple<float, float> rssis = sys.Master.RSSI;
                db.WriteRSSI(sys.Master.ID, rssis);
            }
            foreach(Radio r in sys.Peers)
            {
                if(r.ActiveCallCount == 0)
                {
                    Tuple<float, float> rssis = r.RSSI;
                    db.WriteRSSI(r.ID, rssis);
                }
            }
        }

        static void HandleUserCall(object sender, CallEventArgs e)
        {
            RadioCall call = e.Call;
            string rssiStr = "";
            if (!double.IsNaN(call.RSSI))
            {
                rssiStr = " RSSI "+ call.RSSI;
                DBRadio r = db.ReadRadio(call.From);
                if(r == null)
                {
                    r = new DBRadio(call.From.Int, call.RSSI);
                }
                else
                {
                    r.AddReading(call.RSSI);
                }
                db.UpdateRadio(r);
            }
            if(call.IsAudio)
            {
                Console.WriteLine("Audio Call : {0} => {1} "+rssiStr, call.From, call.To);
                AudioCall ac = (AudioCall)call;
                string filename = String.Format(@"E:\RadioCalls\{0} - {1} to {2}.mp3", call.Start.ToString("yyyy-MM-ddTHH-mm-ss"), call.From.Int, call.To.Int);
                try
                {
                    ac.SaveToMP3(filename);
                } 
                catch(Exception)
                {
                    Console.WriteLine("Unable to decode audio!");
                }
                db.WriteVoiceCall(call.From, call.To, call.Start, call.End, call.RSSI, call.Slot, filename);
                srv.PublishVoiceCall(call);
            }
            else
            {
                DataCall dc = (DataCall)call;
                switch (dc.DataType)
                {
                    case CallDataType.TMS:
                        if (dc.TextMessage.Type == MessageType.Ack)
                        {
                            Console.WriteLine("Text Message Ack : {0} => {1}" + rssiStr, call.From, call.To);
                        }
                        else if (dc.TextMessage != null)
                        {
                            Console.WriteLine("Text Message : {0} => {1} \"{2}\"" +rssiStr, call.From, call.To, dc.TextMessage.Message);
                        }
                        else
                        {
                            Console.WriteLine("Text Message : {0} => {1} unable to parse!"+rssiStr, call.From, call.To);
                        }
                        break;
                    case CallDataType.LRRP:
                        LRRPPacket pkt = dc.LRRPPacket;
                        if (pkt.Type == LRRPPacketType.ImmediateLocationResponse || pkt.Type == LRRPPacketType.TriggeredLocationData)
                        {
                            Console.WriteLine("Got LRRP Packet from {0} {1} {2}", call.From, pkt, rssiStr);
                        }
                        else
                        {
                            Console.WriteLine("Got LRRP Control Message: {0} => {1}" + rssiStr, call.From, call.To);
                        }
                        break;
                    case CallDataType.ICMP:
                        Console.WriteLine("Got ICMP Ping: {0} => {1}"+rssiStr, call.From, call.To);
                        break;
                    case CallDataType.RadioCheck:
                        Console.WriteLine("Got Radio Check: {0} => {1}"+rssiStr, call.From, call.To);
                        break;
                    case CallDataType.RadioCheckAck:
                        Console.WriteLine("Got Radio Check Ack: {0} => {1}"+rssiStr, call.From, call.To);
                        break;
                    case CallDataType.UnknownSmall:
                    case CallDataType.IPAck:
                        //Just ignore this
                        break;
                    default:
                        Console.WriteLine("Data Call Type is {0}", dc.DataType);
                        Console.WriteLine("Got Unknown radio call: {0} => {1}"+rssiStr, call.From, call.To);
                        Console.WriteLine("    " + BitConverter.ToString(call.Data));
                        break;
                }
            }
        }
    }
}

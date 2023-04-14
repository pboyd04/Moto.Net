using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using Moto.Net;
using Moto.Net.Mototrbo.Bursts;
using System.Linq;
using System.Net;
using Moto.Net.Mototrbo.LRRP;
using Moto.Net.Mototrbo.TMS;
using System.Text;
using System.Reflection;
using System.Text.Json;
using MotoMond.Database;

namespace MotoMond
{
    static class Program
    {
        static IDatabase db;
        static RPCServer srv;
        static RadioSystem sys;
        static Dictionary<string, string> modelMap;
        static System.Timers.Timer rssiWatchdog;

        static void Main(string[] args)
        {
            using (srv = new RPCServer())
            {
                //Grab out model lookup data
                Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("MotoMond.MototrboModels.json");
                using (StreamReader reader = new StreamReader(s))
                {
                    String jsonString = reader.ReadToEnd();
                    modelMap = JsonSerializer.Deserialize<Dictionary<String, String>>(jsonString);
                }
                Dictionary<RadioID, IPAddress> controlStations = new Dictionary<RadioID, IPAddress>();
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ConfigurationSectionGroup group = config.GetSectionGroup("Databases");
                if (group.Sections.Count == 1)
                {
                    db = (IDatabase)group.Sections[0];
                }
                else
                {
                    db = new DatabaseMultiPlexer();
                    foreach (IDatabase child in group.Sections)
                    {
                        ((DatabaseMultiPlexer)db).AddChild(child);
                    }
                }
                using (db)
                {
                    db.CreateTables();

                    string masterIP = "192.168.0.100";
                    int masterPort = 50000;
                    if (args.Length > 1)
                    {
                        string[] parts = args[1].Split(':');
                        masterIP = parts[0];
                        if (parts.Length > 1)
                        {
                            masterPort = int.Parse(parts[1]);
                        }
                    }

                    RadioSystemType type = (RadioSystemType)RadioSystemType.Parse(typeof(RadioSystemType), ConfigurationManager.AppSettings.Get("systemType"));

                    using (sys = new RadioSystem(uint.Parse(ConfigurationManager.AppSettings.Get("systemId")), type))
                    {
                        srv.SetSystem(sys);
                        sys.GotRadioCall += HandleUserCall;
                        Radio master = sys.ConnectToMaster(masterIP, masterPort);
                        ProcessRadio(master, "Master");
                        Radio[] radios = sys.GetPeers();
                        Console.WriteLine("Found {0} other radios...", radios.Length);
                        foreach (Radio r in radios)
                        {
                            PeerRadio pr = (PeerRadio)r;
                            pr.SendPeerRegistration();
                            ProcessRadio(r, "Peer");
                        }
                        StartNoiseFloorCollector();
                        LRRPClient lrrp = new LRRPClient();
                        TMSClient tms = new TMSClient();
                        sys.RegisterLRRPClient(lrrp);
                        sys.RegisterTMSClient(tms);
                        System.Net.NetworkInformation.NetworkInterface[] ifaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                        foreach (System.Net.NetworkInformation.NetworkInterface iface in ifaces)
                        {
                            if (iface.Description.Contains("MOTOTRBO Radio"))
                            {
                                Console.WriteLine("Found potential control station on {0}", iface.Name);
                                var ips = iface.GetIPProperties().GatewayAddresses;
                                using (LocalRadio lr = new LocalRadio(sys, ips[0].Address))
                                {
                                    ProcessRadio(lr, "Control Station");
                                    controlStations.Add(lr.ID, ips[0].Address);
                                }
                            }
                        }
                        CommandProcessor cmd = new CommandProcessor(sys, lrrp, tms, controlStations, db);
                        lrrp.GotLocationData += Lrrp_GotLocationData;
                        RunCli(cmd);
                        StopNoiseFloorCollector();
                        lrrp.Dispose();
                        tms.Dispose();
                    }
                }
            }
        }

        private static void Lrrp_GotLocationData(object sender, LRRPPacketEventArgs e)
        {
            ImmediateLocationResponsePacket pkt = (ImmediateLocationResponsePacket)e.Packet;
            db.WriteLocation(e.ID, pkt.Latitude, pkt.Longitude, e.Call?.RSSI);
        }

        private static void ProcessRadio(Radio r, string dispname)
        {
            if(r is PeerRadio)
            {
                PeerRadio pr = (PeerRadio)r;
                if (r.InitXNL() == false)
                {
                    Console.WriteLine("    Retrying peer init!");
                    pr.SendPeerRegistration();
                    r.InitXNL();
                }
            }
            else
            {
                r.InitXNL();
            }
            //For control stations we don't know the ID until after XNL is initialized
            Console.WriteLine(dispname + " ID = {0}", r.ID);
            Console.WriteLine("    XNL ID = {0} {1}", r.XNLID, r.XNLClientID);
            Console.WriteLine("    XCMP Version = {0}", r.XCMPVersion);
            string fwver = r.FirmwareVersion;
            Console.WriteLine("    Firmware Version = {0}", fwver);
            string cpVer = r.CodeplugVersion;
            Console.WriteLine("    Codeplug Version = {0}", cpVer);
            string serialNum = r.SerialNumber;
            Console.WriteLine("    Serial Number = {0}", serialNum);
            string modelNum = r.ModelNumber;
            if (modelMap.ContainsKey(modelNum))
            {
                Console.WriteLine("    Model Number = {0} ({1})", modelNum, modelMap[modelNum.Trim()]);
            }
            else
            {
                Console.WriteLine("    Model Number = {0}", modelNum);
            }
            Console.WriteLine("    Alarms:");
            Dictionary<string, bool> alarms = r.GetAlarmStatus();
            foreach (KeyValuePair<string, bool> kvp in alarms)
            {
                Console.WriteLine("        {0}: {1}", kvp.Key, kvp.Value);
            }
            string name = db.UpdateConnectedRadio(r.ID, serialNum, modelNum, fwver);
            r.Name = name;
            Tuple<float, float> rssis = r.RSSI;
            if (!(r is LocalRadio))
            {
                //This isn't available for local radios
                Console.WriteLine("    RSSI: {0} {1}", rssis.Item1, rssis.Item2);
                db.WriteRSSI(r.ID, rssis);
            }
            int zoneCount = r.ZoneCount;
            Console.WriteLine("    Zone Count = {0}", zoneCount);
            for(int i = 0; i < zoneCount; i++)
            {
                int channelCount = r.GetChannelCountForZone((UInt16)(i + 1));
                Console.WriteLine("        Zone[{0}] Channel Count = {1}", i+1, channelCount);
                for(int j = 0; j < channelCount; j++)
                {
                    Console.WriteLine("            Channel {0} Name = {1}", j+1, r.GetChannelName((UInt16)(i + 1), (UInt16)(j + 1)));
                }
            }
        }

        private static void RunCli(CommandProcessor cmd)
        {
            bool go = true;
            while (go)
            {
                Console.Write("cmd> ");
                string cmdStr = Console.ReadLine();
                string[] parts = cmdStr.Split(' ');
                switch (parts[0])
                {
                    case "exit":
                        go = false;
                        break;
                    case "":
                        //Just print the new command line
                        break;
                    default:
                        CommandResult res = cmd.ProcessCommand(parts[0], parts.Skip(1).ToArray());
                        StringBuilder sb = new StringBuilder();
                        foreach (KeyValuePair<string, object> pair in res.Data)
                        {
                            var value = pair.Value;
                            if(!(value is String) && value is System.Collections.IEnumerable)
                            {
                                //For some reason join doesn't work here...
                                StringBuilder sb2 = new StringBuilder();
                                foreach(var item in ((System.Collections.IEnumerable)value))
                                {
                                    sb2.Append(item.ToString());
                                    sb2.Append("\n");
                                }
                                value = sb2.ToString();
                            }
                            sb.Append(pair.Key + ": " + value + ", ");
                        }
                        if (res.Success == false)
                        {
                            if (res.Ex != null)
                            {
                                Console.WriteLine("Command Failed! " + res.Ex);
                            }
                            else
                            {
                                Console.WriteLine("Command Failed! " + sb);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Success! " + sb);
                        }
                        break;
                }
            }
        }

        private static void StartNoiseFloorCollector()
        {
            rssiWatchdog = new System.Timers.Timer(30000);
            rssiWatchdog.Elapsed += GetRSSI;
            rssiWatchdog.Enabled = true;
            rssiWatchdog.AutoReset = true;
        }

        private static void StopNoiseFloorCollector()
        {
            rssiWatchdog.Stop();
            rssiWatchdog.Enabled = false;
        }

        private static void GetRSSI(Object src, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Console.WriteLine("Master Active Call Count is {0}", sys.Master.ActiveCallCount);
                if (sys.Master.ActiveCallCount == 0)
                {
                    Console.WriteLine("Getting RSSI for radio {0}", sys.Master.ID);
                    Tuple<float, float> rssis = sys.Master.RSSI;
                    Console.WriteLine("Got RSSI {0}", rssis);
                    //Skip writing error data...
                    if(rssis.Item1 != -1f && rssis.Item2 != -1f)
                    {
                        db.WriteRSSI(sys.Master.ID, rssis);
                    }
                }
                foreach (Radio r in sys.Peers)
                {
                    if (r.ActiveCallCount == 0)
                    {
                        Console.WriteLine("Getting RSSI for radio {0}", r.ID);
                        Tuple<float, float> rssis = r.RSSI;
                        Console.WriteLine("Got RSSI {0}", rssis);
                        if (rssis.Item1 == -1f || rssis.Item2 == -1f)
                        {
                            continue;
                        }
                        db.WriteRSSI(r.ID, rssis);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to get RSSI. {0}\n{1}", ex.Message, ex.StackTrace);
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
                        if(pkt == null)
                        {
                            Console.WriteLine("Failed to decode LRRP packet!");
                            foreach(KeyValuePair<UInt16, Burst> pair in call.Bursts)
                            {
                                Console.WriteLine("Burst {0}", pair.Key);
                                Console.WriteLine("    " + pair.Value);
                            }
                            return;
                        }
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
                    case CallDataType.TCPAck:
                        //Just ignore this
                        break;
                    case CallDataType.UnknownIP:
                        Console.WriteLine("Got Unknown IP Traffic! {0} => {1}" + rssiStr, call.From, call.To);
                        if ((((DataCall)call).Datagram).IsValid)
                        {
                            Console.WriteLine("    Source IP = {0} Dest IP = {1} Protocol {2}", ((DataCall)call).Datagram.Source, ((DataCall)call).Datagram.Destination, ((DataCall)call).Datagram.Protocol);
                            Console.WriteLine("    Source Port = {0} Dest Port = {1}", ((DataCall)call).Datagram.Transport.SourcePort, ((DataCall)call).Datagram.Transport.DestinationPort);
                            Console.WriteLine("    " + BitConverter.ToString(((DataCall)call).Datagram.Transport.Payload.ToArray()));
                        }
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

using Moto.Net;
using Moto.Net.Mototrbo.Bursts.CSBK;
using Moto.Net.Mototrbo.LRRP;
using Moto.Net.Mototrbo.TMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MotoMond
{
    public class CommandResult
    {
        public bool Success;
        public Exception ex;
        public Dictionary<string, object> Data;

        public CommandResult()
        {
            this.Data = new Dictionary<string, object>();
        }
    }

    public delegate CommandResult CliCommand(String[] args);

    public struct CMD
    {
        public String HelpText;
        public bool Debug;
        public CliCommand Cmd;

        public CMD(string helpText, CliCommand cmd, bool debug)
        {
            HelpText = helpText;
            Cmd = cmd;
            Debug = debug;
        }
    }

    public class CommandProcessor
    {
        protected RadioSystem sys;
        protected LRRPClient lrrp;
        protected TMSClient tms;
        protected Dictionary<string, CMD> Commands;
        protected Dictionary<RadioID, IPAddress> radios;
        protected Database db;

        public CommandProcessor(RadioSystem sys, LRRPClient lrrp, TMSClient tms, Dictionary<RadioID, IPAddress> controlStations, Database db)
        {
            this.sys = sys;
            this.lrrp = lrrp;
            this.tms = tms;
            this.radios = controlStations;
            this.db = db;
            this.Commands = new Dictionary<string, CMD>()
            {
                {"check", new CMD("Sends a radio check to a radio", this.RadioCheck, false) },
                {"getip", new CMD("Gets the ip for a radio", this.RadioIP, false) },
                {"getsystem", new CMD("Gets the system details", this.GetSystem, false) },
                {"getradio", new CMD("Gets the radio details", this.GetRadio, false) },
                {"locate", new CMD("Gets the radio location", this.RadioLocate, false) },
                {"startlocate", new CMD("Tells the radio to periodically send its location back to the server", this.RadioStartLocate, false) },
                {"stoplocate", new CMD("Tells the radio to stop sending its location back to the server", this.RadioStopLocate, false) },
                {"text", new CMD("Sends the provided text to the radio", this.RadioText, false) },
                {"listradios", new CMD("List radios from the system or from the db", this.ListRadios, false) },
                {"help", new CMD("Displays this help message", this.Help, false) },
                //These commands could result in odd behavior and so are left out of the help on purpose
                {"debug_radiostatus", new CMD("Send a radio status command to the radio", this.DebugRadioStatus, true) },
                {"debug_xcmp", new CMD("Send a XCMP packet to the radio ", this.DebugXCMP, true) },
                {"debug_xnl", new CMD("Send a XNL packet to the radio ", this.DebugXNL, true) }
            };
        }

        public CommandResult ProcessCommand(string command, string[] args)
        {
            string cmd = command.ToLower();
            if(this.Commands.ContainsKey(cmd))
            {
                return this.Commands[cmd].Cmd(args);
            }
            else
            {
                CommandResult res = new CommandResult();
                res.Success = false;
                res.ex = new ArgumentException("Unknown command " + command);
                return res;
            }
        }

        private CommandResult Help(string[] args)
        {
            CommandResult res = new CommandResult();
            res.Success = true;
            res.Data["help"] = "";
            foreach(KeyValuePair<string, CMD> kv in this.Commands)
            {
                if (kv.Value.Debug == false)
                {
                    res.Data["help"] += kv.Key + " : " + kv.Value.HelpText + "\n";
                }
            }
            return res;
        }

        private CommandResult GetRadio(string[] args)
        {
            CommandResult res = new CommandResult();
            if (args.Length < 1)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument radio ID!");
                return res;
            }
            uint id = uint.Parse(args[0]);
            Radio r = sys.FindRadioByID(new RadioID(id));
            if(r == null)
            {
                res.Success = false;
            }
            else
            {
                res.Success = true;
                res.Data["radio"] = r;
            }
            return res;
        }
        
        private CommandResult GetSystem(string[] args)
        {
            CommandResult res = new CommandResult();
            res.Success = true;
            res.Data["sys"] = this.sys;
            return res;
        }

        private CommandResult RadioCheck(string[] args)
        {
            CommandResult res = new CommandResult();
            if (args.Length < 1)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument radio ID!");
                return res;
            }
            uint id = uint.Parse(args[0]);
            float rssi = 0.0F;
            bool ret = sys.RadioCheck(new RadioID(id), ref rssi);
            res.Success = ret;
            res.Data["rssi"] = rssi;
            return res;
        }

        private CommandResult RadioIP(string[] args)
        {
            CommandResult res = new CommandResult();
            if (args.Length < 1)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument radio ID!");
                return res;
            }
            uint id = uint.Parse(args[0]);
            IPAddress ret = sys.GetIPForRadio(new RadioID(id));
            res.Success = true;
            res.Data["ip"] = ret;
            return res;
        }

        private CommandResult RadioLocate(string[] args)
        {
            CommandResult res = new CommandResult();
            if (args.Length < 1)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument radio ID!");
                return res;
            }
            uint id = uint.Parse(args[0]);
            Tuple<float, float, float?> ret = lrrp.GetCurrentLocation(new RadioID(id), sys);
            if (ret.Item1 == 0.0f)
            {
                res.Success = false;
            }
            else
            {
                res.Success = true;
                if (ret.Item3.HasValue)
                {
                    res.Data["rssi"] = ret.Item3;
                }
                res.Data["latitude"] = ret.Item1;
                res.Data["longitude"] = ret.Item2;
            }
            return res;
        }

        private CommandResult RadioStartLocate(string[] args)
        {
            CommandResult res = new CommandResult();
            if (args.Length < 1)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument radio ID!");
                return res;
            }
            if (args.Length < 2)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument request id!");
                return res;
            }
            if (args.Length < 3)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument period!");
                return res;
            }
            uint id = uint.Parse(args[0]);
            uint reqid = uint.Parse(args[1]);
            uint period = uint.Parse(args[2]);
            int ret = lrrp.StartTriggeredLocate(new RadioID(id), sys, reqid, period);
            res.Success = (ret == 0);
            res.Data["rc"] = (LRRPResponseCodes)ret;
            return res;
        }

        private CommandResult RadioStopLocate(string[] args)
        {
            CommandResult res = new CommandResult();
            if (args.Length < 1)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument radio ID!");
                return res;
            }
            if (args.Length < 2)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument request id!");
                return res;
            }
            uint id = uint.Parse(args[0]);
            uint reqid = uint.Parse(args[1]);
            int ret = lrrp.StopTriggeredLocate(new RadioID(id), sys, reqid);
            res.Success = (ret == 0);
            res.Data["rc"] = (LRRPResponseCodes)ret;
            return res;
        }

        private CommandResult RadioText(string[] args)
        {
            CommandResult res = new CommandResult();
            if (args.Length < 1)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument radio ID!");
                return res;
            }
            if (args.Length < 2)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument message!");
                return res;
            }
            uint id = uint.Parse(args[0]);
            string message = string.Join(" ", args.Skip(1).ToArray());
            bool ret = tms.SendText(message, new RadioID(id), sys, true);
            res.Success = ret;
            return res;
        }

        private CommandResult ListRadios(string[] args)
        {
            CommandResult res = new CommandResult();
            String source = "db";
            if (args.Length > 1)
            {
                source = args[1];
            }
            if(source.Equals("db"))
            {
                res.Data["radios"] = this.db.ReadRadios();
            }
            else
            {
                //TODO implement
            }
            res.Success = true;
            return res;
        }

        private Radio getRadioForDebug(RadioID id, ref bool shouldClose)
        {
            if (!this.radios.ContainsKey(id))
            {
                shouldClose = false;
                Radio r1 = this.sys.FindRadioByID(id);
                return r1;
            }
            shouldClose = true;
            IPAddress ip = this.radios[id];
            return new LocalRadio(this.sys, ip);
        }

        private CommandResult DebugRadioStatus(string[] args)
        {
            CommandResult res = new CommandResult();
            if (args.Length < 1)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument radio ID!");
                return res;
            }
            if (args.Length < 2)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument status!");
                return res;
            }
            RadioID id = new RadioID(uint.Parse(args[0]));
            bool shouldClose = false;
            Radio r = getRadioForDebug(id, ref shouldClose);
            if(shouldClose)
            {
                using(r)
                {
                    r.InitXNL();
                    byte status = byte.Parse(args[1]);
                    byte[] data = r.GetRadioStatus((Moto.Net.Mototrbo.XNL.XCMP.XCMPStatus)status);
                    res.Success = true;
                    res.Data["data"] = BitConverter.ToString(data);
                    res.Data["ASCII"] = ASCIIEncoding.ASCII.GetString(data);
                }
            }
            else
            {
                byte status = byte.Parse(args[1]);
                byte[] data = r.GetRadioStatus((Moto.Net.Mototrbo.XNL.XCMP.XCMPStatus)status);
                res.Success = true;
                res.Data["data"] = BitConverter.ToString(data);
                res.Data["ASCII"] = ASCIIEncoding.ASCII.GetString(data);
            }
            return res;
        }

        private CommandResult DebugXCMP(string[] args)
        {
            CommandResult res = new CommandResult();
            if (args.Length < 1)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument radio ID!");
                return res;
            }
            if (args.Length < 2)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required packet data!");
                return res;
            }
            RadioID id = new RadioID(uint.Parse(args[0]));
            byte[] pktData = Enumerable.Range(0, args[1].Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(args[1].Substring(x, 2), 16)).ToArray();
            Moto.Net.Mototrbo.XNL.XCMP.XCMPPacket pkt = new Moto.Net.Mototrbo.XNL.XCMP.XCMPPacket(pktData);
            bool shouldClose = false;
            Radio r = getRadioForDebug(id, ref shouldClose);
            if (shouldClose)
            {
                using (r)
                {
                    r.InitXNL();
                    byte[] data = r.SendXCMP(pkt);
                    res.Success = true;
                    res.Data["data"] = BitConverter.ToString(data);
                }
            }
            else
            {
                byte[] data = r.SendXCMP(pkt);
                res.Success = true;
                res.Data["data"] = BitConverter.ToString(data);
            }
            return res;
        }

        private CommandResult DebugXNL(string[] args)
        {
            CommandResult res = new CommandResult();
            if (args.Length < 1)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required argument radio ID!");
                return res;
            }
            if (args.Length < 2)
            {
                res.Success = false;
                res.ex = new ArgumentException("Missing required packet data!");
                return res;
            }
            RadioID id = new RadioID(uint.Parse(args[0]));
            byte[] pktData = Enumerable.Range(0, args[1].Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(args[1].Substring(x, 2), 16)).ToArray();
            Moto.Net.Mototrbo.XNL.XNLPacket pkt = new Moto.Net.Mototrbo.XNL.XNLPacket((Moto.Net.Mototrbo.XNL.OpCode)pktData[0], pktData.Skip(1).ToArray());
            bool shouldClose = false;
            Radio r = getRadioForDebug(id, ref shouldClose);
            if (shouldClose)
            {
                using (r)
                {
                    r.InitXNL();
                    byte[] data = r.SendXNL(pkt);
                    res.Success = true;
                    res.Data["data"] = BitConverter.ToString(data);
                }
            }
            else
            {
                byte[] data = r.SendXNL(pkt);
                res.Success = true;
                res.Data["data"] = BitConverter.ToString(data);
            }
            return res;
        }
    }
}

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

    public class CommandProcessor
    {
        protected RadioSystem sys;
        protected LRRPClient lrrp;
        protected TMSClient tms;

        public CommandProcessor(RadioSystem sys, LRRPClient lrrp, TMSClient tms)
        {
            this.sys = sys;
            this.lrrp = lrrp;
            this.tms = tms;
        }

        public CommandResult ProcessCommand(string command, string[] args)
        {
            string cmd = command.ToLower();
            CommandResult res = new CommandResult();
            switch (cmd)
            {
                case "check":
                    return RadioCheck(args);
                case "getip":
                    return RadioIP(args);
                case "getsystem":
                    res.Success = true;
                    res.Data["sys"] = this.sys;
                    return res;
                case "getradio":
                    return GetRadio(args);
                case "locate":
                    return RadioLocate(args);
                case "text":
                    return RadioText(args);
                default:
                    res.Success = false;
                    res.ex = new ArgumentException("Unknown command "+command);
                    return res;
            }
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
            float rssi = 0.0F;
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
    }
}

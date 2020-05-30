using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moto.Net.Mototrbo;
using Moto.Net.Mototrbo.XNL;
using Moto.Net.Mototrbo.XNL.XCMP;

namespace Moto.Net
{
    public class CallEventArgs : EventArgs
    {
        public RadioCall Call;
        public bool Handled;

        public CallEventArgs(RadioCall call) : base()
        {
            this.Call = call;
        }
    }
    public delegate void CallHander(object sender, CallEventArgs e);

    public abstract class Radio : IDisposable
    {
        protected bool isDisposed;
        protected RadioID id;
        protected RadioSystem sys;
        protected XNLClient xnlClient;
        protected XCMPClient xcmpClient;
        protected Dictionary<RadioID, RadioCall> activeCalls;
        protected string name;

        public event PacketHandler GotXNLXCMPPacket;
        public event PacketHandler GotUserPacket;
        public event CallHander GotUserCall;

        public Radio()
        {
            this.activeCalls = new Dictionary<RadioID, RadioCall>();
        }

        public RadioID ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public Address XNLID
        {
            get
            {
                if(this.xnlClient != null)
                {
                    return this.xnlClient.MasterID;
                }
                return new Address(0xFFFF);
            }
        }

        public String XCMPVersion
        {
            get
            {
                if(this.xcmpClient != null)
                {
                    return this.xcmpClient.Version;
                }
                return "";
            }
        }

        public String SerialNumber
        {
            get
            {
                if(this.xcmpClient != null)
                {
                    RadioStatusReply reply = this.xcmpClient.GetRadioStatus(XCMPStatus.SerialNumber);
                    return ASCIIEncoding.ASCII.GetString(reply.Data);
                }
                return "";
            }
        }

        public String ModelNumber
        {
            get
            {
                if (this.xcmpClient != null)
                {
                    RadioStatusReply reply = this.xcmpClient.GetRadioStatus(XCMPStatus.ModelNumber);
                    return ASCIIEncoding.ASCII.GetString(reply.Data);
                }
                return "";
            }
        }

        public String FirmwareVersion
        {
            get
            {
                if(this.xcmpClient != null)
                {
                    VersionInfoReply reply = this.xcmpClient.GetVersionInfo();
                    return reply.Version;
                }
                return "";
            }
        }

        public virtual UInt32 TimeSlots
        {
            get
            {
                return 0;
            }
        }

        public Tuple<float,float> RSSI
        {
            get
            {
                if(this.xcmpClient != null)
                {
                    RadioStatusReply reply = this.xcmpClient.GetRadioStatus(XCMPStatus.RSSI);
                    return new Tuple<float, float>(Mototrbo.Util.CalcRSSI(reply.Data, 0), Mototrbo.Util.CalcRSSI(reply.Data, 2));
                }
                return new Tuple<float, float>(-1, -1);
            }
        }

        public int ActiveCallCount
        {
            get
            {
                return this.activeCalls.Count;
            }
        }

        public Dictionary<string, bool> GetAlarmStatus()
        {
            Dictionary<string, bool> ret = new Dictionary<string, bool>();
            if (this.xcmpClient != null)
            {
                AlarmStatusReply reply = this.xcmpClient.GetAlarmStatus();
                AlarmStatus[] alarms = reply.Alarms;
                for(int i = 0; i < alarms.Length; i++)
                {
                    ret[alarms[i].Alarm.ToString()] = (alarms[i].State == 0x01);
                }
            }
            return ret;
        }

        protected void FireXNLPacket(Packet pkt, System.Net.IPEndPoint ep)
        {
            if (this.GotXNLXCMPPacket != null)
            {
                PacketEventArgs e = new PacketEventArgs(pkt, ep);
                this.GotXNLXCMPPacket(this, e);
            }
        }

        protected void FireUserPacket(Packet pkt, System.Net.IPEndPoint ep)
        {
            if(this.GotUserPacket != null)
            {
                PacketEventArgs e = new PacketEventArgs(pkt, ep);
                this.GotUserPacket(this, e);
            }
            UserPacket upkt = (UserPacket)pkt;
            RadioID to = upkt.Destination;
            if(activeCalls.ContainsKey(to))
            {
                //Console.WriteLine("Appending to active call...");
                activeCalls[to].AppendPkt(upkt);
            }
            else
            {
                //Console.WriteLine("Creating new call...");
                if (upkt.PacketType == PacketType.GroupDataCall || upkt.PacketType == PacketType.PrivateDataCall)
                {
                    activeCalls[to] = new DataCall(upkt);
                }
                else
                {
                    activeCalls[to] = new AudioCall(upkt);
                }
            }
            if(activeCalls[to].IsEnded)
            {
                this.GotUserCall?.Invoke(this, new CallEventArgs(activeCalls[to]));
                activeCalls.Remove(to);
            }
        }

        public abstract void SendPacket(Packet pkt);
        public abstract bool InitXNL();
        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

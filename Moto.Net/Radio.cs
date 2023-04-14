using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Moto.Net.Mototrbo;
using Moto.Net.Mototrbo.XNL;
using Moto.Net.Mototrbo.XNL.XCMP;

namespace Moto.Net
{
    public class CallEventArgs : EventArgs
    {
        private readonly RadioCall call;
        private bool handled;

        public CallEventArgs(RadioCall call)
        {
            this.call = call;
        }

        public RadioCall Call
        {
            get
            {
                return this.call;
            }
        }

        public bool IsHandled
        {
            get
            {
                return handled;
            }
            set
            {
                handled = value;
            }
        }
    }
    public delegate void CallHander(object sender, CallEventArgs e);

    public abstract class Radio : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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

        protected Radio()
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

        public Address XNLClientID
        {
            get
            {
                if(this.xcmpClient != null)
                {
                    return this.xnlClient.XNLID;
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
                    XCMPStatus status = XCMPStatus.SerialNumber;
                    if(this.xcmpClient.isRepeater)
                    {
                        status = XCMPStatus.RepeaterSerialNumber;
                    }
                    RadioStatusReply reply = this.xcmpClient.GetRadioStatus(status);
                    return ASCIIEncoding.ASCII.GetString(reply.Data);
                }
                return "";
            }
        }

        public String PhysicalSerialNumber
        {
            get
            {
                if (this.xcmpClient != null)
                {
                    XCMPStatus status = XCMPStatus.PhysicalSerialNumber;
                    RadioStatusReply reply = this.xcmpClient.GetRadioStatus(status);
                    string ret = BitConverter.ToString(reply.Data);
                    return ret.Replace("-", "");
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
                    String ret = ASCIIEncoding.ASCII.GetString(reply.Data);
                    return ret.TrimEnd(new char[] { '\0' });
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

        public String CodeplugVersion
        {
            get
            {
                if (this.xcmpClient != null)
                {
                    VersionInfoReply reply = this.xcmpClient.GetVersionInfo(VersionInfoType.CodeplugVersion);
                    if(reply.ErrorCode == XCMPErrorCode.BadParams)
                    {
                        reply = this.xcmpClient.GetVersionInfo(VersionInfoType.CodeplugVersion2);
                    }
                    return reply.Version;
                }
                return "";
            }
        }

        public String BootloaderVersion
        {
            get
            {
                if (this.xcmpClient != null)
                {
                    VersionInfoReply reply = this.xcmpClient.GetVersionInfo(VersionInfoType.BootloaderVersion);
                    return reply.Version;
                }
                return "";
            }
        }

        public String TanapaNumber
        {
            get
            {
                if (this.xcmpClient != null)
                {
                    return this.xcmpClient.GetTanapaNumber();
                }
                return "";
            }
        }

        public RFBand RFBand
        {
            get
            {
                if (this.xcmpClient != null)
                {
                    return this.xcmpClient.Band;
                }
                return RFBand.Unknown;
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
                    if (reply != null && reply.Data.Length > 2)
                    {
                        return new Tuple<float, float>(Mototrbo.Util.CalcRSSI(reply.Data, 0), Mototrbo.Util.CalcRSSI(reply.Data, 2));
                    }
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

        public UInt16 ZoneCount
        {
            get
            {
                if (this.xcmpClient != null)
                {
                    ChannelSelectReply reply = this.xcmpClient.GetChannelSelect(ChannelSelectFunction.GetZoneCount, 1);
                    return reply.Zone;
                }
                return 0;
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

        public UInt16 GetChannelCountForZone(UInt16 zoneId)
        {
            if(this.xcmpClient != null)
            {
                ChannelSelectReply reply = this.xcmpClient.GetChannelSelect(ChannelSelectFunction.GetChannelCount, zoneId);
                return reply.Channel;
            }
            return 0;
        }

        public byte[] SendXCMP(XCMPPacket pkt)
        {
            if (this.xcmpClient != null)
            {
                XCMPPacket res = this.xcmpClient.SendPacketAndWaitForSameType(pkt);
                return res.Encode();
            }
            return new byte[0];
        }

        public byte[] SendXNL(XNLPacket pkt)
        {
            if(this.xnlClient != null)
            {
                SemaphoreSlim signal = new SemaphoreSlim(0, 1);
                XNLPacket res = null;
                PacketHandler handler = new PacketHandler((sender, e) => {
                    if(e.Packet.PacketType == PacketType.XnlXCMPPacket)
                    {
                        res = ((XNLXCMPPacket)e.Packet).XNLData;
                        signal.Release();
                    }
                });
                this.GotXNLXCMPPacket += handler;
                this.xnlClient.SendPacket(pkt, false);
                if (signal.Wait(5000) == false)
                {
                    this.GotXNLXCMPPacket -= handler;
                    return new byte[0];
                }
                this.GotXNLXCMPPacket -= handler;
                return res.Encode();
            }
            return new byte[0];
        }

        public String GetChannelName(UInt16 zoneId, UInt16 channel)
        {
            if (this.xcmpClient != null)
            {
                CloneReadReply reply = this.xcmpClient.DoCloneRead(zoneId, channel);
                String ret = Encoding.BigEndianUnicode.GetString(reply.Data);
                return ret.TrimEnd(new char[] { '\0' });
            }
            return "";
        }

        public void DoCloneRead(UInt16 indexType, UInt16 index, UInt16 dataType)
        {
            if (this.xcmpClient != null)
            {
                CloneReadReply reply = this.xcmpClient.DoCloneRead(indexType, index, dataType);
            }
        }

        public byte[] GetRadioStatus(XCMPStatus status)
        {
            RadioStatusReply reply = this.xcmpClient.GetRadioStatus(status);
            if (reply == null)
            {
                return null;
            }
            return reply.Data;
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
                log.DebugFormat("{0}: Appending to active call...", this.ID);
                activeCalls[to].AppendPkt(upkt);
            }
            else
            {
                log.DebugFormat("{0}: Creating new call...", this.ID);
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

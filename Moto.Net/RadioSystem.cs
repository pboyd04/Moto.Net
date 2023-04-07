using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moto.Net.Mototrbo;
using System.Net;
using System.Threading;
using Moto.Net.Mototrbo.Bursts;
using Moto.Net.Mototrbo.Bursts.CSBK;
using PcapDotNet.Packets.Dns;
using Moto.Net.Mototrbo.LRRP;
using Moto.Net.Mototrbo.TMS;

namespace Moto.Net
{
    public class RadioSystem : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly RadioID myID;
        private readonly RadioSystemType type;
        protected internal UDPClient client;
        protected MasterRadio master;
        protected List<Radio> peers;
        protected IPEndPoint restChannel;
        protected UDPClient restClient;
        protected Dictionary<RadioID, RadioCall> activeCalls;
        protected LRRPClient lrrp;
        protected TMSClient tms;
        protected RegistrationFlags registrationFlags = RegistrationFlags.CSKBSupport | RegistrationFlags.CallMonitor | RegistrationFlags.Software | RegistrationFlags.XNLDevice | RegistrationFlags.SomethingElseThatIsRequired | RegistrationFlags.DataCallSupport | RegistrationFlags.VoiceCallSupport;

        public event CallHander GotRestCall;
        public event CallHander GotRadioCall;
        protected event CallHander InternalCallHandler;

        public RadioSystem(uint radioid, RadioSystemType type) : this(radioid, type, 50000)
        {

        }

        public RadioSystem(uint radioid, RadioSystemType type, UInt16 port)
        {
            this.peers = new List<Radio>();
            this.activeCalls = new Dictionary<RadioID, RadioCall>();
            this.myID = new RadioID(radioid);
            this.type = type;
            this.client = new UDPClient(port);
            this.client.GotPeerRegisterRequest += new PacketHandler(this.HandlePeerRegisterRequest);
            this.client.GetPeerKeepAliveRequest += new PacketHandler(this.HandlePeerKeepAliveRequest);
            this.client.GotPeerListReply += new PacketHandler(this.NullHandler);
            this.client.GotUserPacket += RestClient_GotUserPacket;
        }

        private void NullHandler(object sender, PacketEventArgs e)
        {
            //Just ignore these requests...
        }

        public MasterRadio ConnectToMaster(string address, int port)
        {
            IPAddress addr = IPAddress.Parse(address);
            IPEndPoint ipend = new IPEndPoint(addr, port);
            this.master = new MasterRadio(this, ipend);
            //Wait for the master to respond
            while(this.master.ID == null)
            {
                System.Threading.Thread.Sleep(100);
            }
            this.master.GotUserCall += HandleUserCall;
            return this.master;
        }

        public void SendRegistrationPacket(IPEndPoint ipend)
        {
            MasterRegistrationRequest pkt = new MasterRegistrationRequest(this.myID, this.type, this.RegistrationFlags);
            this.client.Send(pkt, ipend);
        }

        public void RawSendToRestChannel(byte[] bytes)
        {
            this.client.RawSend(bytes, this.restChannel);
            this.client.RawSend(bytes, new IPEndPoint(IPAddress.Parse("255.255.255.255"), this.restChannel.Port));
        }

        public void SendPacketToRestChannel(Packet pkt)
        {
            this.client.Send(pkt, this.restChannel);
            this.client.Send(pkt, new IPEndPoint(IPAddress.Parse("255.255.255.255"), this.restChannel.Port));
        }

        public void SendCall(RadioCall call)
        {
            UserPacket[] packets = call.ToPackets(this.myID);
            foreach(UserPacket p in packets)
            {
                this.SendPacketToRestChannel(p);
                Thread.Sleep(60); //This seems to be the minimum time to get this to work correctly
            }
        }

        public bool RadioCheck(RadioID toCheck, ref float rssi)
        {
            CSBKBurst burst = new RadioCheck(this.ID, toCheck);
            RadioCall call = DataCall.CSBKRadioCall(6, false, this.ID, toCheck, burst);
            float myRSSI = 0.0F;
            SemaphoreSlim signal = new SemaphoreSlim(0, 1);
            CallHander handler = new CallHander((sender, e) => {
                if(e.Call.IsAudio)
                {
                    return;
                }
                DataCall intcall = (DataCall)e.Call;
                if(intcall.From.Equals(toCheck) && intcall.To.Equals(this.myID) && intcall.DataType == CallDataType.RadioCheckAck)
                {
                    myRSSI = intcall.RSSI;
                    e.IsHandled = true;
                    signal.Release();
                }
            });
            this.InternalCallHandler += handler;
            this.SendCall(call);
            if (signal.Wait(5000) == false)
            {
                this.InternalCallHandler -= handler;
                return false;
            }
            this.InternalCallHandler -= handler;
            rssi = myRSSI;
            return true;
        }

        public Radio[] GetPeers()
        {
            PeerListRequest plr = new PeerListRequest(this.myID);
            List<Radio> radios = new List<Radio>();
            SemaphoreSlim signal = new SemaphoreSlim(0, 1);
            PacketHandler handler = new PacketHandler((sender, e) => {
                PeerListReply reply = (PeerListReply)e.Packet;
                Peer[] pktPeers = reply.Peers;
                for(int i = 0; i < pktPeers.Length; i++)
                {
                    if (pktPeers[i].ID.Int == 0)
                    {
                        this.restChannel = new IPEndPoint(IPAddress.Parse(pktPeers[i].Address), pktPeers[i].Port);
                    }
                    else if (pktPeers[i].ID.Equals(this.myID))
                    {
                        //This is me
                        continue;
                    }
                    else
                    {
                        Radio r = new PeerRadio(this, new IPEndPoint(IPAddress.Parse(pktPeers[i].Address), pktPeers[i].Port));
                        r.GotUserCall += HandleUserCall;
                        this.peers.Add(r);
                        radios.Add(r);
                    }
                }
                signal.Release();
            });
            this.client.GotPeerListReply += handler;
            this.master.SendPacket(plr);
            if (signal.Wait(5000) == false)
            {
                this.client.GotPeerListReply -= handler;
                //Retry after timeout...
                return this.GetPeers();
            }
            this.client.GotPeerListReply -= handler;
            return radios.ToArray();
        }

        public void RegisterLRRPClient(LRRPClient client)
        {
            this.lrrp = client;
            this.lrrp.RetainRecentlySent = true;
            this.lrrp.Debounce = true;
        }

        public void RegisterTMSClient(TMSClient client)
        {
            this.tms = client;
            this.tms.RetainRecentlySent = true;
            this.tms.Debounce = true;
        }

        private void HandleUserCall(object sender, CallEventArgs e)
        {
            if(!e.Call.IsAudio)
            {
                DataCall dc = (DataCall)e.Call;
                switch(dc.DataType)
                {
                    case CallDataType.LRRP:
                        if(this.lrrp != null)
                        {
                            if(lrrp.RecentlySent(dc.LRRPPacket, this.GetIPForRadio(e.Call.To)))
                            {
                                //Drop this...
                                return;
                            }
                            else if(lrrp.DebouncePacket(dc.LRRPPacket, this.GetIPForRadio(e.Call.From), dc))
                            {
                                //Sent through LRRP processing, drop it...
                                return;
                            }
                        }
                        break;
                    case CallDataType.TMS:
                        if(this.tms != null)
                        {
                            if(tms.RecentlySent(dc.TextMessage, this.GetIPForRadio(e.Call.To)))
                            {
                                //Drop this...
                                return;
                            }
                            else if(tms.DebouncePacket(dc.TextMessage, this.GetIPForRadio(e.Call.From), dc))
                            {
                                //Sent through TMS processing, drop it...
                                return;
                            }
                        }
                        break;
                    default:
                        log.WarnFormat("Unknown data type {0}", dc.DataType);
                        break;
                }
            }
            this.InternalCallHandler?.Invoke(sender, e);
            if (!e.IsHandled)
            {
                this.GotRadioCall?.Invoke(sender, e);
            }
        }

        private void RestClient_GotUserPacket(object sender, PacketEventArgs e)
        {
            if(e.Packet.ID.Equals(this.myID))
            {
                //Ignore my own packets...
                return;
            }
            UserPacket upkt = (UserPacket)e.Packet;
            RadioID to = upkt.Destination;
            if (activeCalls.ContainsKey(to))
            {
                log.Info("Appending to active call...");
                activeCalls[to].AppendPkt(upkt);
            }
            else
            {
                log.Info("Creating new call...");
                if (upkt.PacketType == PacketType.GroupDataCall || upkt.PacketType == PacketType.PrivateDataCall)
                {
                    activeCalls[to] = new DataCall(upkt);
                }
                else
                {
                    activeCalls[to] = new AudioCall(upkt);
                }
            }
            if (activeCalls[to].IsEnded)
            {
                this.GotRestCall?.Invoke(this, new CallEventArgs(activeCalls[to]));
                activeCalls.Remove(to);
            }
        }

        public Radio FindRadioByID(RadioID id)
        {
            if(this.master != null && (this.master.ID == null || this.master.ID.Equals(id)))
            {
                return master;
            }
            foreach(Radio r in this.peers)
            {
                if(r.ID != null && r.ID.Equals(id))
                {
                    return r;
                }
            }
            //TODO handle remote radios
            return null;
        }

        public Radio FindRadioByEndpoint(IPEndPoint ep)
        {
            if(this.master != null && this.master.Endpoint.Equals(ep))
            {
                return master;
            }
            foreach (Radio r in this.peers)
            {
                IPRadio ir = (IPRadio)r;
                if (ir.Endpoint.Equals(ep))
                {
                    return r;
                }
            }
            //TODO handle remote radios
            return null;
        }

        public Radio FindRadioByPacket(Packet pkt, IPEndPoint ep)
        {
            Radio r = FindRadioByID(pkt.ID);
            if(r != null)
            {
                return r;
            }
            return FindRadioByEndpoint(ep);
        }

        public IPAddress GetIPForRadio(RadioID id)
        {
            Radio r = FindRadioByID(id);
            if (r != null && r is IPRadio)
            {
                IPRadio ir = (IPRadio)r;
                return ir.Endpoint.Address;
            }
            return GetSystemIPForRadio(id);
        }

        public IPAddress GetSystemIPForRadio(RadioID id)
        {
            byte[] tmp = new byte[4];
            id.AddToArray(tmp, 0, 4);
            tmp[0] = 12;
            return new IPAddress(tmp);
        }

        private void HandlePeerRegisterRequest(object sender, PacketEventArgs e)
        {
            log.InfoFormat("Got register request {0}", e.Packet);
            Radio r = this.FindRadioByPacket(e.Packet, e.EP);
            Packet resp = new PeerRegistrationReply(this.myID, this.type);
            if(r == null)
            {
                log.Info("Replying to unknown radio...");
                this.client.Send(resp, e.EP);
            }
            else
            {
                log.InfoFormat("Replying to known radio {0}", r);
                if(r.ID == null)
                {
                    log.InfoFormat("Updating Radio ID {0}", e.Packet.ID);
                    r.ID = e.Packet.ID;
                }
                r.SendPacket(resp);
            }
        }

        private void HandlePeerKeepAliveRequest(object sender, PacketEventArgs e)
        {
            log.InfoFormat("Got peer keep alive request {0}", e.Packet);
            Radio r = this.FindRadioByPacket(e.Packet, e.EP);
            Packet resp = new PeerKeepAliveReply(this.myID, this.type);
            if (r == null)
            {
                log.Info("Replying to unknown radio...");
                this.client.Send(resp, e.EP);
            }
            else
            {
                log.InfoFormat("Replying to known radio {0}", r);
                ((PeerRadio)r).StartKeepAlive(resp);
                r.SendPacket(resp);
            }
        }

        public Radio Master
        {
            get
            {
                return this.master;
            }
        }

        public Radio[] Peers
        {
            get
            {
                return this.peers.ToArray();
            }
        }

        public RadioSystemType SystemType
        {
            get
            {
                return this.type;
            }
        }

        public RadioID ID
        {
            get
            {
                return this.myID;
            }
        }

        public uint TimeSlots
        {
            get
            {
                uint slots = master.TimeSlots;
                
                return slots;
            }
        }

        public RegistrationFlags RegistrationFlags
        {
            get
            {
                return this.registrationFlags;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.lrrp = null; //Caller owns this make them explicitly dispose
                    this.tms = null; //Caller owns this make them explicitly dispose
                    if (this.master != null)
                    {
                        this.master.Dispose();
                        this.master = null;
                    }
                    this.client.Dispose();
                    this.client = null;
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}

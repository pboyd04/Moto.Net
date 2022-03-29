using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.LRRP
{
    public class LRRPPacketEventArgs : EventArgs
    {
        public LRRPPacket Packet;
        public IPEndPoint Endpoint;
        public byte CAI;
        public RadioID ID;
        public DateTime Timestamp;
        public DataCall Call;

        public LRRPPacketEventArgs(LRRPPacket p, IPEndPoint ep)
        {
            this.Packet = p;
            this.Endpoint = ep;
            byte[] tmp = ep.Address.GetAddressBytes();
            this.CAI = tmp[0];
            this.ID = new RadioID(tmp, 1, 3);
            this.Timestamp = DateTime.Now;
        }

        public LRRPPacketEventArgs(LRRPPacket p, IPEndPoint ep, DataCall call) : this(p, ep)
        {
            this.Call = call;
        }
    }

    public delegate void LRRPPacketHandler(object sender, LRRPPacketEventArgs e);

    public class LRRPClient : IDisposable
    {
        protected UdpClient client;
        protected bool running;
        protected Thread thread;
        protected bool retainRecentlySent = false;
        protected List<LRRPPacketEventArgs> recentlySent;
        protected Dictionary<LRRPPacketEventArgs, System.Timers.Timer> recentlyRecieved;
        protected List<LRRPPacketEventArgs> recentlyHandled;
        protected bool deboune = false;

        public event LRRPPacketHandler GotLocationData;
        public event LRRPPacketHandler GotLRRPControl;

        public LRRPClient() : this(4001)
        {

        }

        public LRRPClient(int port)
        {
            this.recentlyRecieved = new Dictionary<LRRPPacketEventArgs, System.Timers.Timer>();
            this.recentlyHandled = new List<LRRPPacketEventArgs>();
            client = new UdpClient(port);
            this.running = true;
            this.thread = new Thread(this.ListenThread);
            this.thread.Start();
        }

        public void ListenThread()
        {
            while (running)
            {
                this.recentlyHandled.RemoveAll(x => x.Timestamp.AddSeconds(5) < DateTime.Now);
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = this.client.Receive(ref RemoteIpEndPoint);
                LRRPPacket p = LRRPPacket.Decode(receiveBytes);
                //Console.WriteLine("Got LRRP Packet {0}", p);
                LRRPPacketEventArgs e = new LRRPPacketEventArgs(p, RemoteIpEndPoint);
                bool handled = false;
                foreach(LRRPPacketEventArgs x in this.recentlyHandled)
                {
                    if (e.Endpoint.Address.Equals(x.Endpoint.Address))
                    {
                        if (e.Packet.Type == x.Packet.Type && e.Packet.RequestID == x.Packet.RequestID)
                        {
                            //Drop this it was already handled from the repeater...
                            handled = true;
                            break;
                        }
                    }
                }
                if(handled)
                {
                    continue;
                }
                if(this.deboune)
                {
                    if(this.GotLocationData == null && this.GotLRRPControl == null)
                    {
                        //No event handlers this is the easy case...
                        continue;
                    }
                    System.Timers.Timer t = new System.Timers.Timer(1000);
                    t.Elapsed += new System.Timers.ElapsedEventHandler((sender, evt) => {
                        ReallySendEvent(e);
                    });
                    t.AutoReset = false;
                    t.Start();
                }
                else
                {
                    ReallySendEvent(e);
                }
            }
        }

        private bool ReallySendEvent(LRRPPacketEventArgs e)
        {
            if(e.Packet == null)
            {
                //Couldn't decode the packet...
                return false;
            }
            switch (e.Packet.Type)
            {
                case LRRPPacketType.ImmediateLocationResponse:
                case LRRPPacketType.TriggeredLocationData:
                    if (this.GotLocationData != null)
                    {
                        this.GotLocationData.Invoke(this, e);
                        return true;
                    }
                    return false;
                case LRRPPacketType.ProtocolVersionResponse:
                case LRRPPacketType.TriggeredLocationStartResponse:
                case LRRPPacketType.TriggeredLocationStopResponse:
                    if(this.GotLRRPControl != null)
                    {
                        this.GotLRRPControl.Invoke(this, e);
                        return true;
                    }
                    return false;
                default:
                    Console.WriteLine("Got an unknown LRRP packet {0}", e.Packet);
                    return false;
            }
        }

        public bool Send(LRRPPacket packet, IPEndPoint ep)
        {
            byte[] tmp = packet.Encode();
            int ret = client.Send(tmp, tmp.Length, ep);
            this.recentlySent?.Add(new LRRPPacketEventArgs(packet, ep));
            return (ret == tmp.Length);
        }

        public int GetRemoteRadioLRRPVersion(string ipAddress)
        {
            LRRPPacket verReq = new VersionRequestPacket(1);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipAddress), 4001);
            int ver = -1;
            SemaphoreSlim signal = new SemaphoreSlim(0, 1);
            LRRPPacketHandler handler = new LRRPPacketHandler((sender, e) => { 
                if(e.Packet.Type == LRRPPacketType.ProtocolVersionResponse && e.Endpoint.Equals(ep))
                {
                    ver = ((VersionResponsePacket)e.Packet).Version;
                    signal.Release();
                }
            });
            this.GotLRRPControl += handler;
            this.Send(verReq, ep);
            if (signal.Wait(5000) == false)
            {
                this.GotLRRPControl -= handler;
                return -1;
            }
            this.GotLRRPControl -= handler;
            return ver;
        }

        public int GetRemoteRadioLRRPVersion(RadioID id, RadioSystem sys)
        {
            IPAddress ip = sys.GetIPForRadio(id);
            return this.GetRemoteRadioLRRPVersion(ip.ToString());
        }

        public Tuple<float, float, float?> GetCurrentLocation(string ipAddress)
        {
            ImmediateLocationRequestPacket lrp = new ImmediateLocationRequestPacket(1);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipAddress), 4001);
            Tuple<float, float, float?> ret = new Tuple<float, float, float?>(0.0f, 0.0f, null);
            SemaphoreSlim signal = new SemaphoreSlim(0, 1);
            LRRPPacketHandler handler = new LRRPPacketHandler((sender, e) => {
                if(e.Packet.Type == LRRPPacketType.ImmediateLocationResponse && e.Endpoint.Equals(ep))
                {
                    ImmediateLocationResponsePacket rp = (ImmediateLocationResponsePacket)e.Packet;
                    float? rssi = null;
                    if(e.Call != null && !double.IsNaN(e.Call.RSSI))
                    {
                        rssi = e.Call.RSSI;
                    }
                    ret = new Tuple<float, float, float?>(rp.Latitude, rp.Longitude, rssi);
                    signal.Release();
                }
            });
            this.GotLocationData += handler;
            this.Send(lrp, ep);
            if (signal.Wait(5000) == false)
            {
                this.GotLocationData -= handler;
                return ret;
            }
            this.GotLocationData -= handler;
            return ret;
        }

        public Tuple<float, float, float?> GetCurrentLocation(RadioID id, RadioSystem sys)
        {
            IPAddress ip = sys.GetIPForRadio(id);
            return this.GetCurrentLocation(ip.ToString());
        }

        public int StartTriggeredLocate(string ipAddress, uint requestID, uint period)
        {
            TriggeredLocationStartRequestPacket pkt = new TriggeredLocationStartRequestPacket(requestID);
            pkt.TriggerPeriodically = (int)period;
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipAddress), 4001);
            SemaphoreSlim signal = new SemaphoreSlim(0, 1);
            int resp = -1;
            LRRPPacketHandler handler = new LRRPPacketHandler((sender, e) => {
                if (e.Packet.Type == LRRPPacketType.TriggeredLocationStartResponse && e.Endpoint.Equals(ep))
                {
                    resp = ((TriggeredLocationStartResponsePacket)e.Packet).ResponseCode;
                    signal.Release();
                }
            });
            this.GotLRRPControl += handler;
            this.Send(pkt, ep);
            if (signal.Wait(5000) == false)
            {
                this.GotLRRPControl -= handler;
                return -1;
            }
            this.GotLRRPControl -= handler;
            return resp;
        }

        public int StartTriggeredLocate(RadioID id, RadioSystem sys, uint requestID, uint period)
        {
            IPAddress ip = sys.GetIPForRadio(id);
            return this.StartTriggeredLocate(ip.ToString(), requestID, period);
        }

        public int StopTriggeredLocate(string ipAddress, uint requestID)
        {
            TriggeredLocationStopRequestPacket pkt = new TriggeredLocationStopRequestPacket(requestID);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipAddress), 4001);
            SemaphoreSlim signal = new SemaphoreSlim(0, 1);
            int resp = -1;
            LRRPPacketHandler handler = new LRRPPacketHandler((sender, e) => {
                if (e.Packet.Type == LRRPPacketType.TriggeredLocationStopResponse && e.Endpoint.Equals(ep))
                {
                    resp = ((TriggeredLocatonStopResponsePacket)e.Packet).ResponseCode;
                    signal.Release();
                }
            });
            this.GotLRRPControl += handler;
            this.Send(pkt, ep);
            if (signal.Wait(5000) == false)
            {
                this.GotLRRPControl -= handler;
                return -1;
            }
            this.GotLRRPControl -= handler;
            return resp;
        }

        public int StopTriggeredLocate(RadioID id, RadioSystem sys, uint requestID)
        {
            IPAddress ip = sys.GetIPForRadio(id);
            return this.StopTriggeredLocate(ip.ToString(), requestID);
        }

        public bool RetainRecentlySent
        {
            get
            {
                return retainRecentlySent;
            }
            set
            {
                retainRecentlySent = value;
                if (retainRecentlySent)
                {
                    this.recentlySent = new List<LRRPPacketEventArgs>();
                }
                else
                {
                    this.recentlySent = null;
                }
            }
        }

        public bool Debounce
        {
            get
            {
                return this.deboune;
            }
            set
            {
                this.deboune = true;
            }
        }

        public bool RecentlySent(LRRPPacket pkt, IPAddress ipAddress)
        {
            //Remove any packets older than 5 seconds...
            this.recentlySent.RemoveAll(x => x.Timestamp.AddSeconds(5) < DateTime.Now);
            foreach (LRRPPacketEventArgs p in this.recentlySent)
            {
                if (p.Endpoint.Address.Equals(ipAddress))
                {
                    if (p.Packet.Type == pkt.Type && p.Packet.RequestID == pkt.RequestID)
                    {
                        this.recentlySent.Remove(p);
                        return true;
                    }
                }
            }
            return false;
        }

        //We prefer the packet recieved by the repeater because it has RSSI
        public bool DebouncePacket(LRRPPacket pkt, IPAddress ipAddress, DataCall call)
        {
            //Remove expired timers...
            this.recentlyRecieved = this.recentlyRecieved.Where(pair => pair.Value.Enabled == false).ToDictionary(pair => pair.Key, pair => pair.Value);
            LRRPPacketEventArgs myE = new LRRPPacketEventArgs(pkt, new IPEndPoint(ipAddress, 4001), call);
            foreach (KeyValuePair<LRRPPacketEventArgs, System.Timers.Timer> pair in this.recentlyRecieved)
            {
                Console.WriteLine("Does {0} == {1}", pair.Key.Endpoint.Address, ipAddress);
                if (pair.Key.Endpoint.Address.Equals(ipAddress))
                {
                    if (pair.Key.Packet.Type == pkt.Type && pair.Key.Packet.RequestID == pkt.RequestID)
                    {
                        pair.Value.Stop();
                        this.recentlyRecieved.Remove(pair.Key);
                        return ReallySendEvent(myE);
                    }
                }
            }
            if (this.ReallySendEvent(myE))
            {
                this.recentlyHandled.Add(myE);
                return true;
            }
            return false;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                this.running = false;
                this.thread.Abort();
                if (disposing)
                {
                    this.client.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LRRPClient()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

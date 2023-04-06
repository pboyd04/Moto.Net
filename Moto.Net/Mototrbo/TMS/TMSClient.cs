using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.TMS
{
    public class TMSMessageEventArgs : EventArgs
    {
        public TMSMessage Packet;
        public IPEndPoint Endpoint;
        public byte CAI;
        public RadioID ID;
        public DateTime Timestamp;
        public DataCall Call;

        public TMSMessageEventArgs(TMSMessage p, IPEndPoint ep) : this(p, ep, null)
        {

        }

        public TMSMessageEventArgs(TMSMessage p, IPEndPoint ep, DataCall call)
        {
            this.Packet = p;
            this.Endpoint = ep;
            byte[] tmp = ep.Address.GetAddressBytes();
            this.CAI = tmp[0];
            this.ID = new RadioID(tmp, 1, 3);
            this.Timestamp = DateTime.Now;
            this.Call = call;
        }
    }
    public delegate void TMSMessageHandler(object sender, TMSMessageEventArgs e);

    public class TMSClient : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected UdpClient client;
        protected bool running;
        protected Thread thread;
        protected bool retainRecentlySent = false;
        protected bool deboune = false;
        protected List<TMSMessageEventArgs> recentlySent;
        protected Dictionary<TMSMessageEventArgs, System.Timers.Timer> recentlyRecieved;
        protected List<TMSMessageEventArgs> recentlyHandled;

        public event TMSMessageHandler GotText;
        public event TMSMessageHandler GotAck;

        public TMSClient() : this(4007)
        {

        }

        public TMSClient(int port)
        {
            this.recentlyRecieved = new Dictionary<TMSMessageEventArgs, System.Timers.Timer>();
            this.recentlyHandled = new List<TMSMessageEventArgs>();
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
                TMSMessage m = new TMSMessage(receiveBytes);
                TMSMessageEventArgs e = new TMSMessageEventArgs(m, RemoteIpEndPoint);
                foreach (TMSMessageEventArgs x in this.recentlyHandled)
                {
                    if (e.Endpoint.Address.Equals(x.Endpoint.Address))
                    {
                        if (e.Packet.Type == x.Packet.Type && e.Packet.SequenceNumber == x.Packet.SequenceNumber)
                        {
                            //Drop this it was already handled from the repeater...
                        }
                    }
                }
                if (this.deboune)
                {
                    if (this.GotText == null && this.GotAck == null)
                    {
                        //No event handlers this is the easy case...
                        continue;
                    }
                    System.Timers.Timer t = new System.Timers.Timer(1000);
                    t.Elapsed += new System.Timers.ElapsedEventHandler((sender, evt) => {
                        ReallySendEvent(e);
                    });
                    t.Start();
                }
                else
                {
                    ReallySendEvent(e);
                }
            }
        }

        private bool ReallySendEvent(TMSMessageEventArgs e)
        {
            switch (e.Packet.Type)
            {
                case MessageType.SimpleText:
                    if (this.GotText != null)
                    {
                        this.GotText.Invoke(this, e);
                        return true;
                    }
                    return false;
                case MessageType.Ack:
                    if (this.GotAck != null)
                    {
                        this.GotAck.Invoke(this, e);
                        return true;
                    }
                    return false;
                default:
                    log.ErrorFormat("Got an unknown TMS packet {0}", e.Packet);
                    return false;
            }
        }

        public bool Send(TMSMessage msg, IPEndPoint ep)
        {
            byte[] tmp = msg.Encode();
            int ret = client.Send(tmp, tmp.Length, ep);
            this.recentlySent?.Add(new TMSMessageEventArgs(msg, ep));
            return (ret == tmp.Length);
        }

        public bool SendText(string message, string ipAddress, bool confirmReciept)
        {
            TMSMessage msg = new TMSMessage(message, confirmReciept);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipAddress), 4007);
            if(confirmReciept == false)
            {
                return this.Send(msg, ep);
            }
            SemaphoreSlim signal = new SemaphoreSlim(0, 1);
            TMSMessageHandler handler = new TMSMessageHandler((sender, e) => {
                if (e.Packet.Type == MessageType.Ack && e.Endpoint.Equals(ep))
                {
                    signal.Release();
                }
            });
            this.GotAck += handler;
            this.Send(msg, ep);
            if (signal.Wait(5000) == false)
            {
                this.GotAck -= handler;
                return false;
            }
            this.GotAck -= handler;
            return true;
        }

        public bool SendText(string message, RadioID id, RadioSystem sys, bool confirmReciept)
        {
            IPAddress ip = sys.GetIPForRadio(id);
            return this.SendText(message, ip.ToString(), confirmReciept);
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
                    this.recentlySent = new List<TMSMessageEventArgs>();
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

        public bool RecentlySent(TMSMessage pkt, IPAddress ipAddress)
        {
            //Remove any packets older than 5 seconds...
            this.recentlySent.RemoveAll(x => x.Timestamp.AddSeconds(5) < DateTime.Now);
            foreach (TMSMessageEventArgs p in this.recentlySent)
            {
                if (p.Endpoint.Address.Equals(ipAddress))
                {
                    if (p.Packet.Type == pkt.Type && p.Packet.SequenceNumber == pkt.SequenceNumber)
                    {
                        this.recentlySent.Remove(p);
                        return true;
                    }
                }
            }
            return false;
        }

        //We prefer the packet recieved by the repeater because it has RSSI
        public bool DebouncePacket(TMSMessage pkt, IPAddress ipAddress, DataCall call)
        {
            //Remove expired timers...
            this.recentlyRecieved = this.recentlyRecieved.Where(pair => pair.Value.Enabled == false).ToDictionary(pair => pair.Key, pair => pair.Value);
            TMSMessageEventArgs myE = new TMSMessageEventArgs(pkt, new IPEndPoint(ipAddress, 4001), call);
            foreach (KeyValuePair<TMSMessageEventArgs, System.Timers.Timer> pair in this.recentlyRecieved)
            {
                if (pair.Key.Endpoint.Address.Equals(ipAddress))
                {
                    if (pair.Key.Packet.Type == pkt.Type && pair.Key.Packet.SequenceNumber == pkt.SequenceNumber)
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
                    this.client.Dispose();
                    this.client = null;
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

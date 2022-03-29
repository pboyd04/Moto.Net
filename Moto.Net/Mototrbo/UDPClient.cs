using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public class PacketEventArgs : EventArgs
    {
        public Packet packet;
        public IPEndPoint ep;

        public PacketEventArgs(Packet p, IPEndPoint ep) : base()
        {
            this.packet = p;
            this.ep = ep;
        }
    }
    public delegate void PacketHandler(object sender, PacketEventArgs e);

    public class UDPClient : IDisposable
    {
        protected System.Net.Sockets.UdpClient rawClient;
        protected bool running;
        protected bool ignoreUnknown;
        protected Thread thread;
        protected BlockingCollection<Packet> output;

        public event PacketHandler GotXNLXCMPPacket;
        public event PacketHandler GotRegistrationReply;
        public event PacketHandler GotPeerRegisterRequest;
        public event PacketHandler GotPeerRegisterReply;
        public event PacketHandler GetPeerKeepAliveRequest;
        public event PacketHandler GetPeerKeepAliveReply;
        public event PacketHandler GotPeerListReply;
        public event PacketHandler GotMasterKeepAliveReply;
        public event PacketHandler GotUserPacket;

        public UDPClient(Int32 port) : this(port, false)
        {
        }

        public UDPClient(Int32 port, bool ignoreUnknown)
        {
            this.ignoreUnknown = ignoreUnknown;
            this.rawClient = new System.Net.Sockets.UdpClient(port);
            this.output = new BlockingCollection<Packet>();
            this.running = true;
            this.thread = new Thread(this.ListenThread);
            this.thread.Start();
        }

        public void ListenThread()
        {
            while(running)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = this.rawClient.Receive(ref RemoteIpEndPoint);
                Packet p = Packet.Decode(receiveBytes);
                //Console.WriteLine("Recieved {0}", p.ToString());
                PacketEventArgs e = new PacketEventArgs(p, RemoteIpEndPoint);
                switch (p.PacketType)
                {
                    case PacketType.XnlXCMPPacket:
                        if(this.GotXNLXCMPPacket != null)
                        {
                            this.GotXNLXCMPPacket(this, e);
                            break;
                        }
                        goto default;
                    case PacketType.RegistrationReply:
                        if (this.GotRegistrationReply != null)
                        {
                            this.GotRegistrationReply(this, e);
                            break;
                        }
                        goto default;
                    case PacketType.PeerRegisterRequest:
                        if (this.GotPeerRegisterRequest != null)
                        {
                            this.GotPeerRegisterRequest(this, e);
                            break;
                        }
                        goto default;
                    case PacketType.PeerRegisterReply:
                        if (this.GotPeerRegisterReply != null)
                        {
                            this.GotPeerRegisterReply(this, e);
                            break;
                        }
                        goto default;
                    case PacketType.PeerKeepAliveRequest:
                        if (this.GetPeerKeepAliveRequest != null)
                        {
                            this.GetPeerKeepAliveRequest(this, e);
                            break;
                        }
                        goto default;
                    case PacketType.PeerKeepAliveReply:
                        if (this.GetPeerKeepAliveReply != null)
                        {
                            this.GetPeerKeepAliveReply(this, e);
                            break;
                        }
                        goto default;
                    case PacketType.MasterKeepAliveReply:
                        if(this.GotMasterKeepAliveReply != null)
                        {
                            this.GotMasterKeepAliveReply(this, e);
                            break;
                        }
                        goto default;
                    case PacketType.PeerListReply:
                        if(this.GotPeerListReply != null)
                        {
                            this.GotPeerListReply(this, e);
                            break;
                        }
                        goto default;
                    case PacketType.GroupDataCall:
                    case PacketType.GroupVoiceCall:
                    case PacketType.PrivateDataCall:
                    case PacketType.PrivateVoiceCall:
                        if(this.GotUserPacket != null)
                        {
                            this.GotUserPacket(this, e);
                            break;
                        }
                        goto default;
                    default:
                        if(ignoreUnknown)
                        {
                            continue;
                        }
                        Console.WriteLine("Got an unknown packet {0}", p);
                        output.Add(p);
                        break;
                }
            }
        }

        public bool Send(Packet packet, IPEndPoint remotesystem)
        {
            byte[] bytes;
            int ret;
            //Console.WriteLine("Sending packet {0} to {1}", packet, remotesystem);
            bytes = packet.Encode();
            ret = this.rawClient.Send(bytes, bytes.Length, remotesystem);
            return (ret == bytes.Length);
        }

        public bool RawSend(byte[] bytes, IPEndPoint remotesystem)
        {
            int ret = this.rawClient.Send(bytes, bytes.Length, remotesystem);
            return (ret == bytes.Length);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                this.thread.Abort();
                if (disposing)
                {
                    this.rawClient.Close();
                    this.output.Dispose();
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

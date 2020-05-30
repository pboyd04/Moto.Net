using Moto.Net.Mototrbo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Moto.Net
{
    public class PeerRadio : IPRadio
    {
        public PeerRadio(RadioSystem sys, IPEndPoint ipend) : base(sys, ipend)
        {
            this.sys.client.GotPeerRegisterReply += Client_GotPeerRegisterReply;
            this.sys.client.GetPeerKeepAliveReply += Client_GetPeerKeepAliveReply;
            //Start keep alive
            Packet pkt = new PeerKeepAliveRequest(sys.ID);
            this.SendPacket(pkt);
        }

        private void Client_GetPeerKeepAliveReply(object sender, PacketEventArgs e)
        {
            if(e.packet.ID.Equals(this.ID))
            {
                System.Timers.Timer t = new System.Timers.Timer(5000);
                t.Elapsed += this.SendKeepAlive;
                t.Enabled = true;
            }
        }

        private void Client_GotPeerRegisterReply(object sender, PacketEventArgs e)
        {
            //Only care about replys from the same radio, need to do it by address though...
            if (e.ep.Equals(this.Endpoint))
            {
                this.id = e.packet.ID;
            }
        }

        public void SendPeerRegistration()
        {
            SemaphoreSlim signal = new SemaphoreSlim(0, 1);
            PeerRegistrationRequest pkt = new PeerRegistrationRequest(this.sys.ID, this.sys.SystemType);
            Thread t = new Thread(() =>
            {
                while (this.id == null)
                {
                    Thread.Sleep(50);
                };
                signal.Release();
            });
            this.SendPacket(pkt);
            t.Start();
            if (signal.Wait(5000) == false)
            {
                //Retry after timeout...
                t.Abort();
                SendPeerRegistration();
                return;
            }
            t.Join();
        }

        private void SendKeepAlive(Object src, ElapsedEventArgs e)
        {
            Packet pkt = new PeerKeepAliveRequest(sys.ID);
            this.SendPacket(pkt);
            System.Timers.Timer t = (System.Timers.Timer)src;
            t.Stop();
            t.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed) return;

            if(disposing)
            {
                DeregistrationRequest pkt = new DeregistrationRequest(this.sys.ID);
                this.SendPacket(pkt);
            }

            isDisposed = true;
        }

        public override UInt32 TimeSlots
        {
            get
            {
                return 2;
            }
        }
    }
}

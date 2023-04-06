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
    public class MasterRadio : IPRadio
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public MasterRadio(RadioSystem sys, IPEndPoint ipend) : base(sys, ipend)
        {
            this.sys.client.GotRegistrationReply += new PacketHandler(this.HandleRegistrationPacket);
            sys.SendRegistrationPacket(ipend);
        }

        private void HandleRegistrationPacket(object sender, PacketEventArgs e)
        {
            MasterRegistrationReply mrPkt = (MasterRegistrationReply)e.packet;
            this.id = mrPkt.ID;
            this.sys.client.GotRegistrationReply -= this.HandleRegistrationPacket;
            log.DebugFormat("Found Master Radio {0}", this.id);
            //Start Keep Alive
            this.sys.client.GotMasterKeepAliveReply += new PacketHandler(this.HandleKeepAlive);
            MasterKeepAliveRequest kapkt = new MasterKeepAliveRequest(this.sys.ID, this.sys.SystemType, this.sys.RegistrationFlags);
            this.SendPacket(kapkt);
        }

        public void HandleKeepAlive(object sender, PacketEventArgs e)
        {
            System.Timers.Timer t = new System.Timers.Timer(5000);
            t.Elapsed += this.SendKeepAlive;
            t.Enabled = true;
        }

        private void SendKeepAlive(Object src, ElapsedEventArgs e)
        {
            MasterKeepAliveRequest kapkt = new MasterKeepAliveRequest(this.sys.ID, this.sys.SystemType, this.sys.RegistrationFlags);
            this.SendPacket(kapkt);
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

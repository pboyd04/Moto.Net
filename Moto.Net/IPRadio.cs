using Moto.Net.Mototrbo;
using Moto.Net.Mototrbo.XNL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net
{
    /// <summary>
    /// An IPRadio is one available via the system's IP network without routing over the Radio's wireless IP network
    /// </summary>
    public abstract class IPRadio : Radio
    {
        protected readonly IPEndPoint ep;

        public IPRadio(RadioSystem sys, IPEndPoint endpoint) : base()
        {
            this.sys = sys;
            this.ep = endpoint;
            this.sys.client.GotUserPacket += new PacketHandler(this.HandleUserPacket);
        }

        public IPEndPoint Endpoint
        {
            get
            {
                return this.ep;
            }
        }

        public override void SendPacket(Packet pkt)
        {
            this.sys.client.Send(pkt, this.ep);
        }

        public override bool InitXNL()
        {
            this.sys.client.GotXNLXCMPPacket += new PacketHandler(this.HandleXNLPacket);
            this.xnlClient = new XNLClient(this, this.sys.ID);
            if(this.xnlClient.InitSuccess == false || this.xnlClient.Dead)
            {
                return false;
            }
            this.xcmpClient = new Mototrbo.XNL.XCMP.XCMPClient(this.xnlClient);
            if(this.xnlClient.Dead)
            {
                this.xcmpClient = null;
                this.xnlClient = null;
                return false;
            }
            return true;
        }

        private void HandleXNLPacket(object sender, PacketEventArgs e)
        {
            //Is the XNL Packet from the correct radio?
            if (this.ep.Equals(ep) && e.packet.ID.Equals(this.ID))
            {
                this.FireXNLPacket(e.packet, e.ep);
            }
        }

        private void HandleUserPacket(object sender, PacketEventArgs e)
        {
            this.FireUserPacket(e.packet, e.ep);
        }
    }
}

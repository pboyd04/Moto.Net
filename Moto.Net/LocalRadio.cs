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
    public class LocalRadio : Radio
    {
        protected readonly IPEndPoint ep;
        protected internal TCPClient client;

        public LocalRadio(RadioSystem sys, IPAddress ip)
        {
            this.sys = sys;
            this.ep = new IPEndPoint(ip, 8002);
            this.client = new TCPClient(ep);
        }

        public override bool InitXNL()
        {
            this.client.GotXNLXCMPPacket += new PacketHandler(this.HandleXNLPacket);
            this.xnlClient = new XNLClient(this, this.sys.ID);
            if (this.xnlClient.InitSuccess == false)
            {
                return false;
            }
            this.xcmpClient = new Mototrbo.XNL.XCMP.XCMPClient(this.xnlClient);
            Mototrbo.XNL.XCMP.RadioStatusReply reply = this.xcmpClient.GetRadioStatus(Mototrbo.XNL.XCMP.XCMPStatus.RadioID);
            this.id = new RadioID(reply.Data);
            return true;
        }

        public override void SendPacket(Packet pkt)
        {
            if (pkt.PacketType == PacketType.XnlXCMPPacket)
            {
                this.client.Send(((XNLXCMPPacket)pkt).XNLData);
                return;
            }
            throw new NotImplementedException("SendPacket doesn't support packet type " + pkt.PacketType);
        }

        private void HandleXNLPacket(object sender, PacketEventArgs e)
        {
            //Is the XNL Packet from the correct radio?
            if (this.ep.Equals(ep))
            {
                this.FireXNLPacket(e.packet, e.EP);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                client.Dispose();
            }

            isDisposed = true;
        }
    }
}

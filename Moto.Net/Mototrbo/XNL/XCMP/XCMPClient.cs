using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class XCMPClient : IDisposable
    {
        protected XNLClient client;
        protected string version;
        protected bool ready;
        protected BlockingCollection<XCMPPacket> receivedQueue;

        public XCMPClient(XNLClient xnlclient)
        {
            this.receivedQueue = new BlockingCollection<XCMPPacket>();
            this.ready = false;
            this.client = xnlclient;
            this.client.GotDataPacket += new XNLPacketHandler(this.HandleXNLDataPacket);
            //Wait for the Device Init Status Broadcast to indicate complete
            while(this.ready == false)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        public string Version
        {
            get
            {
                return this.version;
            }
        }

        public void SendPacket(XCMPPacket pkt)
        {
            DataPacket dp = new DataPacket(pkt);
            this.client.SendPacket(dp);
        }

        protected XCMPPacket WaitForPacket()
        {
            while (true)
            {
                try
                {
                    return this.receivedQueue.Take();
                }
                catch (InvalidOperationException) { }
                //TODO Timeout...
            }
        }

        public RadioStatusReply GetRadioStatus(XCMPStatus statusType)
        {
            //Console.WriteLine("Getting Radio Status {0}...", statusType);
            RadioStatusRequest req = new RadioStatusRequest(statusType);
            this.SendPacket(req);
            //Console.WriteLine("Sent Packet...");
            while (true)
            {
                //Console.WriteLine("Waiting for Packet...");
                XCMPPacket pkt = this.WaitForPacket();
                //Console.WriteLine("Got Packet {0}", pkt);
                if (pkt.OpCode == XCMPOpCode.RadioStatusReply)
                {
                    RadioStatusReply rsr = (RadioStatusReply)pkt;
                    if (rsr.StatusType == statusType)
                    {
                        return rsr;
                    }
                }
                //Requeue this packet it wasn't for us...
                this.receivedQueue.Add(pkt);
                //TODO Timeout
            }
        }

        public VersionInfoReply GetVersionInfo()
        {
            XCMPPacket req = new VersionInfoRequest();
            this.SendPacket(req);
            while(true)
            {
                XCMPPacket pkt = this.WaitForPacket();
                if (pkt.OpCode == XCMPOpCode.VersionInfoReply)
                {
                    VersionInfoReply vir = (VersionInfoReply)pkt;
                    return vir;
                }
                //Requeue this packet it wasn't for us...
                this.receivedQueue.Add(pkt);
                //TODO Timeout
            }
        }

        public AlarmStatusReply GetAlarmStatus()
        {
            AlarmStatusRequest req = new AlarmStatusRequest();
            this.SendPacket(req);
            while (true)
            {
                XCMPPacket pkt = this.WaitForPacket();
                if (pkt.OpCode == XCMPOpCode.AlarmStatusReply)
                {
                    AlarmStatusReply asr = (AlarmStatusReply)pkt;
                    return asr;
                }
                //Requeue this packet it wasn't for us...
                this.receivedQueue.Add(pkt);
                //TODO Timeout
            }
        }

        private void HandleXNLDataPacket(object sender, XNLEventArgs e)
        {
            XNLPacket pkt = e.Packet;
            if (pkt.IsXCMP)
            {
                DataPacket dp = (DataPacket)pkt;
                XCMPPacket xcmp = dp.XCMP;
                switch(xcmp.OpCode)
                {
                    case XCMPOpCode.DeviceinitStatusBroadcast:
                        DeviceInitStatusBroadcast disb = (DeviceInitStatusBroadcast)xcmp;
                        version = disb.Version;
                        if(disb.InitComplete)
                        {
                            this.ready = true;
                        }
                        break;
                    default:
                        //Console.WriteLine("Got Unknown XCMP Packet {0}", dp.XCMP);
                        this.receivedQueue.Add(xcmp);
                        break;
                }
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
                    this.receivedQueue.Dispose();
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

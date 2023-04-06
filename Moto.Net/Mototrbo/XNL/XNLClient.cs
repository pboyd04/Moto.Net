using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Moto.Net.Mototrbo.XNL
{
    public class XNLEventArgs : EventArgs
    {
        private readonly XNLPacket packet;

        public XNLEventArgs(XNLPacket pkt)
        {
            this.packet = pkt;
        }

        public XNLPacket Packet
        {
            get
            {
                return packet;
            }
        }
    }
    public delegate void XNLPacketHandler(object sender, XNLEventArgs e);

    public class XNLClient
    {
        protected class XNLTransaction
        {
            XNLPacket pkt;
            int retryCount = 0;

            public XNLTransaction(XNLPacket pkt)
            {
                this.pkt = pkt;
                this.retryCount = 0;
            }

            public XNLPacket Packet
            {
                get
                {
                    return pkt;
                }
            }

            public int RetryCount
            {
                get
                {
                    return retryCount;
                }
                set
                {
                    retryCount = value;
                }
            }
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int initRetryCount = 0;

        protected Radio r;
        protected RadioID id;
        protected Address masterID;
        protected Address xnlID;
        protected bool tcpConnection;
        protected UInt16 transactionID;
        protected byte flags;
        protected Dictionary<UInt16, XNLTransaction> pendingTransactions;
        protected bool initSuccess;
        protected List<DevSysEntry> otherRadios;
        protected bool isDead;

        public event XNLPacketHandler GotDataPacket;

        public XNLClient(Radio r, RadioID sysid) : this(r, sysid, false)
        {
        }

        public XNLClient(Radio r, RadioID sysId, bool tcp)
        {
            this.r = r;
            this.id = sysId;
            this.transactionID = 0;
            this.flags = 0;
            this.tcpConnection = tcp;
            this.pendingTransactions = new Dictionary<ushort, XNLTransaction>();
            this.otherRadios = new List<DevSysEntry>();
            this.isDead = false;

            r.GotXNLXCMPPacket += new PacketHandler(this.HandleXNLPacket);
            this.initSuccess = this.Init();
        }

        private void HandleXNLPacket(object sender, PacketEventArgs e)
        {
            XNLXCMPPacket xpkt = (XNLXCMPPacket)e.packet;
            XNLPacket xnl = xpkt.XNLData;
            switch (xnl.OpCode)
            {
                case OpCode.DeviceSysMapBroadcast:
                    //Check the sysmap to see what is there...
                    this.ProcessSysMap((DevSysMapBroadcastPacket)xnl);
                    break;
                case OpCode.MasterStatusBroadcast:
                    log.Debug("Got master status broadcast...");
                    this.masterID = xnl.Source;
                    break;
                case OpCode.DeviceAuthKeyReply:
                    this.StartConnection((DevAuthKeyReplyPacket)xnl);
                    break;
                case OpCode.DeviceConnectionReply:
                    DevConnectionReplyPacket rp = (DevConnectionReplyPacket)xnl;
                    this.xnlID = rp.AssignedID;
                    break;
                case OpCode.DataMessage:
                    if(this.GotDataPacket != null && (xnl.Destination.Int == 0 || xnl.Destination.Equals(this.xnlID)))
                    {
                        this.GotDataPacket(this, new XNLEventArgs(xnl));
                        this.AckPacket(xnl);
                        break;
                    }
                    goto default;
                case OpCode.DataMessageAck:
                    //This data message has been acknowledged by the master remove our retry attempts
                    this.pendingTransactions.Remove(xnl.TransactionID);
                    break;
                default:
                    log.ErrorFormat("Unhandled XNL Data: {0}", xnl);
                    break;
            }
        }

        private bool Init()
        {
            log.DebugFormat("{0}: Initializing...", System.Reflection.MethodBase.GetCurrentMethod().Name);
            if (!(this.r is LocalRadio))
            {
                XNLPacket initPkt = new InitPacket();
                this.SendPacket(initPkt);
            }
            //Wait for broadcast packet
            int waitCount = 0;
            while(this.masterID == null)
            {
                System.Threading.Thread.Sleep(100);
                waitCount++;
                if(waitCount > 50)
                {
                    initRetryCount++;
                    if(initRetryCount > 3)
                    {
                        log.DebugFormat("{0}: Exit. Timeout", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        return false;
                    }
                    if(isDead)
                    {
                        log.DebugFormat("{0}: Exit. No encrypter", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        return false;
                    }
                    return this.Init();
                }
            }
            this.GetAuthKey();
            log.DebugFormat("{0}: Exit", System.Reflection.MethodBase.GetCurrentMethod().Name);
            return true;
        }

        public bool ReInit()
        {
            return this.initSuccess = this.Init();
        }

        private void GetAuthKey()
        {
            log.Debug("Sending auth key request...");
            XNLPacket pkt = new DevAuthKeyRequestPacket(this.masterID);
            this.SendPacket(pkt);
        }

        private void StartConnection(DevAuthKeyReplyPacket pkt)
        {
            log.DebugFormat("Sending connection request {0}...", pkt.TempID);
            try
            {
                XNLPacket newPkt = new DevConnectionRequestPacket(this.masterID, pkt.TempID, new Address(0), 0x0A, 0x01, pkt.AuthKey, ((this.r is MasterRadio) || (this.r is PeerRadio)));
                this.SendPacket(newPkt);
            }
            catch(XNLNotSupportedException)
            {
                log.WarnFormat("Unable to finish XNL connection to device due to lack of encrypter. Operating at reduced feature set...");
                this.isDead = true;
            }
            
        }

        private void ProcessSysMap(DevSysMapBroadcastPacket pkt)
        {
            foreach(DevSysEntry dev in pkt.Entries)
            {
                if(dev.XNLAddress.Equals(this.xnlID) || dev.XNLAddress.Equals(this.masterID))
                {
                    //This is me or the already connected master, skip it...
                    continue;
                }
                log.DebugFormat("Adding Radio... {0}", dev);
                this.otherRadios.Add(dev);
            }
        }

        private void AckPacket(XNLPacket pkt)
        {
            this.SendPacket(new AckDataPacket(pkt));
        }

        public void SendPacket(XNLPacket xnl)
        {
            if(xnl.OpCode == OpCode.DataMessage)
            {
                xnl.Source = this.xnlID;
                xnl.Destination = this.masterID;
                xnl.TransactionID = this.transactionID++;
                xnl.Flags = this.flags++;
                if(this.flags > 0x07)
                {
                    this.flags = 0;
                }
                this.pendingTransactions[xnl.TransactionID] = new XNLTransaction(xnl);
                System.Timers.Timer t = new System.Timers.Timer(5000);
                t.Elapsed += (sender, e) => this.ResendPacket(sender, e, xnl.TransactionID);
                t.Start();
            }
            XNLXCMPPacket pkt = new XNLXCMPPacket(this.id, xnl);
            r.SendPacket(pkt);
        }

        public void SendPacket(XNLPacket xnl, bool overridSrcAndDest)
        {
            if (xnl.OpCode == OpCode.DataMessage)
            {
                if (overridSrcAndDest == true)
                {
                    xnl.Source = this.xnlID;
                    xnl.Destination = this.masterID;
                }
                xnl.TransactionID = this.transactionID++;
                xnl.Flags = this.flags++;
                if (this.flags > 0x07)
                {
                    this.flags = 0;
                }
                this.pendingTransactions[xnl.TransactionID] = new XNLTransaction(xnl);
                System.Timers.Timer t = new System.Timers.Timer(5000);
                t.Elapsed += (sender, e) => this.ResendPacket(sender, e, xnl.TransactionID);
                t.Start();
            }
            XNLXCMPPacket pkt = new XNLXCMPPacket(this.id, xnl);
            log.DebugFormat("Sending {0} to {1}", pkt, xnl.Destination);
            r.SendPacket(pkt);
        }

        private void ResendPacket(object src, ElapsedEventArgs e, UInt16 transactionID)
        {
            if(this.pendingTransactions.ContainsKey(transactionID))
            {
                log.InfoFormat("Transaction {0} is still pending...", transactionID);
                this.pendingTransactions[transactionID].RetryCount++;
                if(this.pendingTransactions[transactionID].RetryCount >= 3)
                {
                    this.ReInit();
                }
                XNLXCMPPacket pkt = new XNLXCMPPacket(this.id, this.pendingTransactions[transactionID].Packet);
                log.DebugFormat("Retrying {0}", pkt);
                r.SendPacket(pkt);
            }
        }

        public Address MasterID
        {
            get
            {
                return this.masterID;
            }
        }

        public Address XNLID
        {
            get
            {
                return this.xnlID;
            }
        }

        public bool InitSuccess
        {
            get
            {
                return this.initSuccess;
            }
        }

        public bool Dead
        {
            get
            {
                return this.isDead;
            }
        }
    }
}

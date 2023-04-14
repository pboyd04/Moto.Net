﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class XCMPClient : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected XNLClient client;
        protected string version;
        protected bool ready;
        protected BlockingCollection<XCMPPacket> receivedQueue;
        protected XNLDevType devType;
        protected RFBand rfBand;

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
                if(this.client.Dead)
                {
                    return;
                }
            }
        }

        public string Version
        {
            get
            {
                return this.version;
            }
        }

        public bool isRepeater
        {
            get
            {
                return this.devType == XNLDevType.Repeater;
            }
        }

        public void SendPacket(XCMPPacket pkt)
        {
            DataPacket dp = new DataPacket(pkt);
            this.client.SendPacket(dp);
        }

        protected XCMPPacket WaitForPacket(int timeout)
        {
            while (true)
            {
                try
                {
                    XCMPPacket pkt;
                    if(this.receivedQueue.TryTake(out pkt, timeout))
                    {
                        return pkt;
                    }
                    return null;
                }
                catch (InvalidOperationException)
                {
                    //Ignore this exception
                }
                //TODO Timeout...
            }
        }

        public XCMPPacket SendPacketAndWaitForSameType(XCMPPacket pkt)
        {
            DataPacket dp = new DataPacket(pkt);
            this.client.SendPacket(dp);
            //The reply code is 0x8<whatever the sent code was>
            XCMPOpCode repCode = (XCMPOpCode)(((UInt16)pkt.OpCode) | 0x8000);
            while (true)
            {
                log.DebugFormat("Waiting for Packet...");
                XCMPPacket res = this.WaitForPacket(5000);
                log.DebugFormat("Got Packet {0}", res);
                if (repCode == res.OpCode)
                {
                    return res;
                }
                //Requeue this packet it wasn't for us...
                this.receivedQueue.Add(res);
                //TODO Timeout
            }
        }

        public RadioStatusReply GetRadioStatus(XCMPStatus statusType)
        {
            log.DebugFormat("Getting Radio Status {0}...", statusType);
            RadioStatusRequest req = new RadioStatusRequest(statusType);
            this.SendPacket(req);
            log.DebugFormat("Sent Packet... {0}", BitConverter.ToString(req.Encode()));
            while (true)
            {
                log.DebugFormat("Waiting for Packet...");
                XCMPPacket pkt = this.WaitForPacket(5000);
                if(pkt == null)
                {
                    log.Warn("Timedout waiting for radio status packet!");
                    return null;
                }
                log.DebugFormat("Got Packet {0}", pkt);
                if (pkt.OpCode == XCMPOpCode.RadioStatusReply)
                {
                    RadioStatusReply rsr = (RadioStatusReply)pkt;
                    if (rsr.StatusType == statusType || rsr.ErrorCode != XCMPErrorCode.Success)
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
                XCMPPacket pkt = this.WaitForPacket(5000);
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

        public VersionInfoReply GetVersionInfo(VersionInfoType type)
        {
            XCMPPacket req = new VersionInfoRequest(type);
            this.SendPacket(req);
            while (true)
            {
                XCMPPacket pkt = this.WaitForPacket(5000);
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

        public String GetTanapaNumber()
        {
            XCMPPacket req = new TanapaNumberRequest();
            this.SendPacket(req);
            while (true)
            {
                XCMPPacket pkt = this.WaitForPacket(5000);
                if (pkt.OpCode == XCMPOpCode.CPS_TanapaNumberReply)
                {
                    TanapaNumberReply vir = (TanapaNumberReply)pkt;
                    return vir.String;
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
                XCMPPacket pkt = this.WaitForPacket(5000);
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

        public ChannelSelectReply GetChannelSelect(ChannelSelectFunction function, UInt16 zone)
        {
            XCMPPacket req = new ChannelSelectRequest(function, zone, 1);
            this.SendPacket(req);
            while (true)
            {
                XCMPPacket pkt = this.WaitForPacket(5000);
                if (pkt == null)
                {
                    log.Warn("Timedout waiting for channel select packet!");
                    return null;
                }
                if (pkt.OpCode == XCMPOpCode.ChannelSelectReply)
                {
                    ChannelSelectReply r = (ChannelSelectReply)pkt;
                    return r;
                }
                //Requeue this packet it wasn't for us...
                this.receivedQueue.Add(pkt);
                //TODO Timeout
            }
        }

        public CloneReadReply DoCloneRead(UInt16 zoneId, UInt16 channel)
        {
            XCMPPacket req = new CloneReadRequest(zoneId, channel, 0x0F);
            this.SendPacket(req);
            while (true)
            {
                XCMPPacket pkt = this.WaitForPacket(5000);
                if (pkt.OpCode == XCMPOpCode.CloneReadReply)
                {
                    CloneReadReply r = (CloneReadReply)pkt;
                    return r;
                }
                //Requeue this packet it wasn't for us...
                this.receivedQueue.Add(pkt);
                //TODO Timeout
            }
        }

        public CloneReadReply DoCloneRead(UInt16 indexType, UInt16 index, UInt16 dataType)
        {
            XCMPPacket req = new CloneReadRequest(indexType, index, dataType);
            this.SendPacket(req);
            while (true)
            {
                XCMPPacket pkt = this.WaitForPacket(5000);
                if (pkt.OpCode == XCMPOpCode.CloneReadReply)
                {
                    CloneReadReply r = (CloneReadReply)pkt;
                    return r;
                }
                //Requeue this packet it wasn't for us...
                this.receivedQueue.Add(pkt);
                //TODO Timeout
            }
        }

        public RFBand Band
        {
            get
            {
                return this.rfBand;
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
                        devType = disb.EntityType;
                        if(disb.Status.Descriptor != null && disb.Status.Descriptor.ContainsKey(XNLDevAttributes.RFBand))
                        {
                            this.rfBand = (RFBand)disb.Status.Descriptor[XNLDevAttributes.RFBand];
                        }
                        if(disb.InitComplete)
                        {
                            this.ready = true;
                        }
                        else //if (disb.EntityType == XNLDevType.RadioControlStation)
                        {
                            //In this case I need to respond with my own details it seems...
                            DeviceStatus status = new DeviceStatus();
                            status.Status = 0;
                            status.DeviceType = XNLDevType.IPPeripheral;
                            status.Descriptor = new Dictionary<XNLDevAttributes, byte>();
                            //status.Descriptor[XNLDevAttributes.DeviceFamily] = 0;
                            //status.Descriptor[XNLDevAttributes.Display] = 0xFF;
                            DeviceInitStatusBroadcast send = new DeviceInitStatusBroadcast(disb.Version, (XNLDevType)0, 0, status);
                            this.SendPacket(send);
                        }
                        break;
                    case XCMPOpCode.RRCtrlBroadcast:
                        //Drop this type I don't know what it is right now, but don't see any reason to put it in the queue either
                        break;
                    case XCMPOpCode.VersionInfoReply:
                    case XCMPOpCode.RadioStatusReply:
                    case XCMPOpCode.AlarmStatusReply:
                    case XCMPOpCode.ChannelSelectReply:
                    case XCMPOpCode.CloneReadReply:
                        //These packets I know about and have logic to handle...
                        if(((XCMPReplyPacket)xcmp).ErrorCode == XCMPErrorCode.ReInitXNL)
                        {
                            Console.WriteLine("Starting XNL ReInit!");
                            //Need to reinit my XNL connection...
                            this.ready = this.client.ReInit();
                        }
                        this.receivedQueue.Add(xcmp);
                        break;
                    default:
                        log.ErrorFormat("Got Unknown XCMP Packet {0}", dp.XCMP);
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

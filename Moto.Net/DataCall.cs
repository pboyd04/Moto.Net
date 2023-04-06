using Moto.Net.Mototrbo;
using Moto.Net.Mototrbo.Bursts;
using Moto.Net.Mototrbo.Bursts.CSBK;
using Moto.Net.Mototrbo.LRRP;
using Moto.Net.Mototrbo.TMS;
using System;
using System.Text.Json.Serialization;

namespace Moto.Net
{
    public enum CallDataType
    {
        Unknown,
        UnknownCSBK,
        UnknownIP,
        UnknownSmall, //I think this is some kind of postamble to keep in sync or something... allow apps just ignore it
        RadioCheck,
        RadioCheckAck,
        ICMP,
        TMS,
        LRRP,
        IPAck,
        TCPAck
    }

    public class DataCall : RadioCall
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected PcapDotNet.Packets.IpV4.IpV4Datagram _datagram;

        public DataCall()
        {
            this.isAudio = false;
            this.isPhoneCall = false;
        }

        public DataCall(UserPacket pkt) : base(pkt)
        {
            if(this.IsAudio == true)
            {
                throw new ArgumentException("Cannot process audio packets as DataCall");
            }
        }

        public DataCall(bool group, bool encrypted, RadioID from, RadioID to) : this()
        {
            this.isGroupCall = group;
            this.isEncrypted = encrypted;
            this.from = from;
            this.to = to;
            this.groupTag = (UInt32)new Random().Next(0);
        }

        public static DataCall CSBKRadioCall(int numberOfPreambles, bool group, RadioID from, RadioID to, CSBKBurst burst)
        {
            DataCall call = new DataCall(group, false, from, to);
            for (int i = 0; i < numberOfPreambles; i++)
            {
                Preamble pre = new Preamble((byte)(numberOfPreambles - i), to, from);
                call.bursts.Add((UInt16)i, pre);
            }
            call.bursts.Add((UInt16)numberOfPreambles, burst);
            return call;
        }

        public CallDataType DataType
        {
            get
            {
                CallDataType ret = CallDataType.Unknown;
                foreach (Burst b in bursts.Values)
                {
                    if(b.Type == Mototrbo.Bursts.DataType.CSBK)
                    {
                        ret = CallDataType.UnknownCSBK;
                        break;
                    }
                    else if (b.Type == Mototrbo.Bursts.DataType.DataHeader)
                    {
                        DataHeader dh = (DataHeader)b;
                        if (dh.ContentType == ContentType.IPPacket)
                        {
                            ret = CallDataType.UnknownIP;
                            break;
                        }
                        else
                        {
                            log.ErrorFormat("Unknown data header type {0}", dh.ContentType);
                        }
                    }
                    else if(b.Type == Mototrbo.Bursts.DataType.UnknownSmall)
                    {
                        return CallDataType.UnknownSmall;
                    }
                    else
                    {
                        log.ErrorFormat("Unknown burst type {0}", b.Type);
                    }
                }
                if(this.Data == null || this.Data.Length == 0)
                {
                    if(this.bursts.Values[0].Type == Mototrbo.Bursts.DataType.DataHeader)
                    {
                        DataHeader dh = (DataHeader)this.bursts.Values[0];
                        if(dh.HeaderType == 1)
                        {
                            return CallDataType.IPAck;
                        }
                    }
                    return ret;
                }
                if(ret == CallDataType.UnknownCSBK)
                {
                    CSBKBurst cb = (CSBKBurst)bursts.Values[0];
                    if(cb.CSBKOpCode == CSBKOpCode.MototrboRadioCheck)
                    {
                        if(((Moto.Net.Mototrbo.Bursts.CSBK.RadioCheck)this.bursts.Values[0]).IsAck)
                        {
                            return CallDataType.RadioCheckAck;
                        }
                        return CallDataType.RadioCheck;
                    }
                    foreach (Burst b in bursts.Values)
                    {
                        if(b.Type != Mototrbo.Bursts.DataType.CSBK)
                        {
                            log.ErrorFormat("Got non-CSBK burst in CSBK data call? {0}", b);
                            continue;
                        }
                        cb = (CSBKBurst)b;
                        log.InfoFormat("OpCode = "+cb.CSBKOpCode+", Feature ID = "+cb.FeatureID+", Data = "+BitConverter.ToString(cb.Data));
                    }
                }
                else if(ret == CallDataType.UnknownIP)
                {
                    if(this.Datagram.Protocol == PcapDotNet.Packets.IpV4.IpV4Protocol.InternetControlMessageProtocol)
                    {
                        return CallDataType.ICMP;
                    }
                    if(this.Datagram.Protocol == PcapDotNet.Packets.IpV4.IpV4Protocol.Tcp)
                    {
                        PcapDotNet.Packets.Transport.TcpDatagram td = this.Datagram.Tcp;
                        if (td.IsAcknowledgment)
                        {
                            return CallDataType.TCPAck;
                        }
                    }
                    else if(this.Datagram.Protocol == PcapDotNet.Packets.IpV4.IpV4Protocol.Udp)
                    {
                        if(this.UDPDatagram == null)
                        {
                            return CallDataType.UnknownIP;
                        }
                        if(this.UDPDatagram.DestinationPort == 4001)
                        {
                            return CallDataType.LRRP;
                        }
                        else if(this.UDPDatagram.DestinationPort == 4007)
                        {
                            return CallDataType.TMS;
                        }
                    }
                }
                return ret;
            }
        }

        [JsonIgnore]
        public PcapDotNet.Packets.IpV4.IpV4Datagram Datagram
        {
            get
            {
                if(_datagram == null)
                {
                    _datagram = new PcapDotNet.Packets.Packet(this.Data, DateTime.Now, PcapDotNet.Packets.DataLinkKind.IpV4).IpV4;
                }
                return _datagram;
            }
        }

        [JsonIgnore]
        public PcapDotNet.Packets.Transport.UdpDatagram UDPDatagram
        {
            get
            {
                return this.Datagram.Udp;
            }
        }

        [JsonIgnore]
        public TMSMessage TextMessage
        {
            get
            {
                PcapDotNet.Packets.Transport.UdpDatagram udp = this.UDPDatagram;
                if (udp == null)
                {
                    return null;
                }
                return new TMSMessage(udp.Payload.ToMemoryStream().ToArray());
            }
        }

        public LRRPPacket LRRPPacket
        {
            get
            {
                PcapDotNet.Packets.Transport.UdpDatagram udp = this.UDPDatagram;
                if (udp == null)
                {
                    return null;
                }
                return LRRPPacket.Decode(udp.Payload.ToMemoryStream().ToArray());
            }
        }
    }
}

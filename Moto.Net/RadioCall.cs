using Moto.Net.Mototrbo;
using Moto.Net.Mototrbo.Bursts;
using System;
using System.Collections.Generic;
using Moto.Net.Audio;

using System.Threading;
using System.Text.Json.Serialization;
using System.Linq;

namespace Moto.Net
{
    public class RadioCall
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected DateTime startTime;
        protected DateTime endTime;
        protected SortedList<UInt16, Burst> bursts;
        protected bool isGroupCall;
        protected bool isAudio;
        protected bool isEncrypted;
        protected bool isEnded;
        protected bool isPhoneCall;
        protected RadioID from;
        protected RadioID to;
        protected int callslot;
        protected UInt32 groupTag;

        protected RadioCall()
        {
            this.bursts = new SortedList<UInt16, Burst>();
        }

        protected RadioCall(UserPacket upkt)
        {
            this.startTime = DateTime.Now;
            this.bursts = new SortedList<UInt16, Burst>();
            this.bursts[upkt.RTP.SequenceNumber] = upkt.Burst;
            this.isGroupCall = (upkt.PacketType == PacketType.GroupDataCall || upkt.PacketType == PacketType.GroupVoiceCall);
            this.isAudio = (upkt.PacketType == PacketType.PrivateVoiceCall || upkt.PacketType == PacketType.GroupVoiceCall);
            this.from = upkt.Source;
            this.to = upkt.Destination;
            this.isEncrypted = upkt.Encrypted;
            this.isEnded = upkt.End;
            this.isPhoneCall = upkt.PhoneCall;
            this.groupTag = upkt.GroupTag;
        }

        public bool IsAudio
        {
            get
            {
                return this.isAudio;
            }
        }

        public bool IsEnded
        {
            get
            {
                return this.isEnded;
            }
        }

        public RadioID From
        {
            get
            {
                return this.from;
            }
        }

        public RadioID To
        {
            get
            {
                return this.to;
            }
        }

        public DateTime Start
        {
            get
            {
                return this.startTime;
            }
        }

        public DateTime End
        {
            get
            {
                return this.endTime;
            }
        }

        public float RSSI
        {
            get
            {
                int count = 0;
                float rssi = 0;
                foreach(Burst b in this.bursts.Values)
                {
                    if(b.HasRSSI)
                    {
                        count++;
                        rssi += b.RSSI;
                    }
                }
                return rssi / count;
            }
        }

        public int Slot
        {
            get
            {
                return bursts.Values[0].Slot;
            }
        }

        [JsonIgnore]
        public byte[] Data
        {
            get
            {
                List<byte> buffer = new List<byte>();
                foreach (KeyValuePair<UInt16, Burst> pair in this.bursts)
                {
                    log.DebugFormat("Burst sequence number is {0}", pair.Key);
                    Burst b = pair.Value;
                    if (b.Type == DataType.DataHeader)
                    {
                        continue;
                    }
                    else if (b.Type == DataType.RateFullData)
                    {
                        VoiceBurst vb = (VoiceBurst)b;
                        for (int i = 0; i < 3; i++)
                        {
                            byte[] data = vb.Frames[i];
                            buffer.AddRange(data);
                        }
                        continue;
                    }
                    else if (b.Type == DataType.UnknownSmall)
                    {
                        //Ignore...
                    }
                    else
                    {
                        log.DebugFormat("Got packet {0}", b.Type);
                        log.DebugFormat("Got data {0}", BitConverter.ToString(b.Data));
                    }
                    buffer.AddRange(b.Data);
                }
                return buffer.ToArray();
            }
        }

        public byte[] PCMData
        {
            get
            {
                try
                {
                    AMBEConverter ac = new AMBEConverter();
                    List<byte> buffer = new List<byte>();
                    foreach (Burst b in this.bursts.Values)
                    {
                        if (b.Type == DataType.RateFullData)
                        {
                            VoiceBurst vb = (VoiceBurst)b;
                            for (int i = 0; i < 3; i++)
                            {
                                byte[] data = vb.Frames[i];
                                buffer.AddRange(ac.Decode(data));
                            }
                        }
                    }
                    return buffer.ToArray();
                }
                catch (AudioNotSupportedException)
                {
                    //Can't do audio
                    return null;
                }
            }
        }

        public void AppendPkt(UserPacket upkt)
        {
            if(upkt.GroupTag != this.groupTag)
            {
                log.ErrorFormat("Got packet with mismatching group tag! {0} != {1}", upkt.GroupTag, this.groupTag);
            }
            try
            {
                this.bursts.Add(upkt.RTP.SequenceNumber, upkt.Burst);
            }
            catch(ArgumentException ex)
            {
                if(ex.Message.Equals("An entry with the same key already exists."))
                {
                    //The burst is a retransmission ignore the error
                }
                throw;
            }
            this.isEnded = upkt.End;
            if(upkt.End)
            {
                this.endTime = DateTime.Now;
            }
        }

        public UserPacket[] ToPackets(RadioID initiator)
        {
            UserPacket[] ret = new UserPacket[this.bursts.Count];
            for(int i = 0; i < ret.Length; i++)
            {
                if(i > 0)
                {
                    //Make sure we get different timestamps...
                    Thread.Sleep(1);
                }
                UserPacket pkt = new UserPacket(initiator, !this.isAudio, this.isGroupCall, this.from, this.to, this.isEncrypted, this.isPhoneCall, this.groupTag, this.bursts[(UInt16)i]);
                ret[i] = pkt;
            }
            ret[ret.Length - 1].End = true;
            return ret;
        }

        [JsonIgnore]
        public SortedList<UInt16, Burst> Bursts
        {
            get
            {
                return this.bursts;
            }
        }
    }
}

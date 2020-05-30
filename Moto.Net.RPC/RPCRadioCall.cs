using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Moto.Net.RPC
{
    public class RPCRadioCall
    {
        protected bool isAudio;
        protected bool isEnded;
        protected bool isTextMessage;
        protected RadioID from;
        protected RadioID to;
        protected DateTime start;
        protected DateTime end;
        protected float rssi;
        protected byte[] pcm;

        public bool IsAudio
        {
            get
            {
                return this.isAudio;
            }
            set
            {
                this.isAudio = value;
            }
        }

        public bool IsEnded
        {
            get
            {
                return this.isEnded;
            }
            set
            {
                this.isEnded = value;
            }
        }

        public bool IsTextMessage
        {
            get
            {
                return this.isTextMessage;
            }
            set
            {
                this.isTextMessage = value;
            }
        }

        public RadioID From
        {
            get
            {
                return this.from;
            }
            set
            {
                this.from = value;
            }
        }

        public RadioID To
        {
            get
            {
                return this.to;
            }
            set
            {
                this.to = value;
            }
        }

        public DateTime Start
        {
            get
            {
                return this.start;
            }
            set
            {
                this.start = value;
            }
        }

        public DateTime End
        {
            get
            {
                return this.end;
            }
            set
            {
                this.end = value;
            }
        }

        public float RSSI
        {
            get
            {
                return this.rssi;
            }
            set
            {
                this.rssi = value;
            }
        }

        public byte[] PCMData
        {
            get
            {
                return this.pcm;
            }
            set
            {
                this.pcm = value;
            }
        }

        public byte[] ResamplePCM(int samplerate, uint channels)
        {
            MemoryStream ms = new MemoryStream(this.PCMData);
            RawSourceWaveStream rsws = new RawSourceWaveStream(ms, new WaveFormat(8000, 16, 1));
            WaveFormat fmt = new WaveFormat(samplerate, 1);
            WaveFormatConversionStream stream = new WaveFormatConversionStream(fmt, rsws);
            if(channels == 2)
            {
                WaveFormatConversionStream tmp = stream;
                fmt = new WaveFormat(samplerate, 2);
                stream = new WaveFormatConversionStream(fmt, tmp);
            }
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            return buffer;
        }
    }
}

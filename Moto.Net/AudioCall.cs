using Moto.Net.Mototrbo;
using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Moto.Net
{
    public class AudioCall : RadioCall
    {
        public AudioCall(UserPacket pkt) : base(pkt)
        {
            if (this.IsAudio == false)
            {
                throw new ArgumentException("Cannot process data packets as AudioCall");
            }
        }

        protected IWaveProvider WaveData()
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(this.PCMData);
            return new RawSourceWaveStream(ms, new NAudio.Wave.WaveFormat(8000, 16, 1));
        }

        public void Play()
        {
            WaveOutEvent wo = new WaveOutEvent();
            wo.Init(this.WaveData());
            wo.Play();
            while (wo.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(500);
            }
            wo.Dispose();
        }

        public void SaveToWAV(string path)
        {
            WaveFileWriter.CreateWaveFile(path, this.WaveData());
        }

        public void SaveToWMA(string path)
        {
            MediaFoundationApi.Startup();
            MediaFoundationEncoder.EncodeToWma(this.WaveData(), path);
        }

        public void SaveToMP3(string path)
        {
            MediaFoundationApi.Startup();
            MediaFoundationEncoder.EncodeToMp3(this.WaveData(), path);
        }
    }
}

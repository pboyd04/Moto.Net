using System;
using System.Runtime.InteropServices;

namespace Moto.Net.Audio
{
    /*
     * This requires users have their own copy of DSP Inovations DMR Voder library. If someone wants to write something that uses an open source equivalent then that's fine. For now you need your own licensed copy of the library. 
     */
    public class AMBEConverter : AudioConverter, IDisposable
    {
        //I'm not really sure what readonly would do to native calls. Let's just leave all of this alone...
#pragma warning disable IDE0044 // Add readonly modifier
        private byte[] decoderWorkingBuffer;
        private GCHandle decoderWorkingBufferHandle;
        private byte[] decoderInput;
        private GCHandle decoderInputHandle;
        private short[] decoderOutput;
        private GCHandle decoderOutputHandle;
        private byte[] encoderWorkingBuffer;
        private GCHandle encoderWorkingBufferHandle;
        private bool[] encoderOutput;
        private GCHandle encoderOutputHandle;
        private short[] encoderInput;
        private GCHandle  encoderInputHandle;
#pragma warning restore IDE0044 // Add readonly modifier

        //Uses DSP Inovations vocoder. To decode the audio you need to get your own copy...

        public AMBEConverter() : base()
        {
            this.decoderWorkingBuffer = new byte[3608];
            this.decoderWorkingBufferHandle = GCHandle.Alloc((object)this.decoderWorkingBuffer, GCHandleType.Pinned);
            this.decoderInput = new byte[49];
            this.decoderInputHandle = GCHandle.Alloc((object)this.decoderInput, GCHandleType.Pinned);
            this.decoderOutput = new short[160];
            this.decoderOutputHandle = GCHandle.Alloc((object)this.decoderOutput, GCHandleType.Pinned);
            try
            {
                AMBEConverter.DSPINI_DMR_Decoder_Init(Marshal.UnsafeAddrOfPinnedArrayElement<byte>(this.decoderWorkingBuffer, 0), (short)1);
            }
            catch (DllNotFoundException ex)
            {
                throw new AudioNotSupportedException("Audio Not supported!", ex);
            }
            this.encoderWorkingBuffer = new byte[7924];
            this.encoderWorkingBufferHandle = GCHandle.Alloc((object)this.encoderWorkingBuffer, GCHandleType.Pinned);
            this.encoderOutput = new bool[49];
            this.encoderOutputHandle = GCHandle.Alloc((object)this.encoderOutput, GCHandleType.Pinned);
            this.encoderInput = new short[160];
            this.encoderInputHandle = GCHandle.Alloc((object)this.encoderInput, GCHandleType.Pinned);
            try
            {
                AMBEConverter.DSPINI_DMR_Encoder_Init(Marshal.UnsafeAddrOfPinnedArrayElement<byte>(this.encoderWorkingBuffer, 0), 0, 1);
            } catch(DllNotFoundException ex)
            {
                throw new AudioNotSupportedException("Audio Not supported!", ex);
            }
        }

        public override byte[] Decode(byte[] data)
        {
            int num = data.Length / 7;
            byte[] ret = new byte[num * 320];
            for (int i = 0; i < num; i++)
            {
                if (!this.IsSlientFrame(data, 7 * i))
                {
                    this.BlockDecode(data, 7 * i, ret, 320 * i);
                    if (!this.IsEmpty(ret, 320 * i))
                    {
                        byte[] testBlock = new byte[320];
                        this.BlockDecode(AMBEConverter.SilenceFrame, 0, testBlock, 0);
                        if (!this.IsEmpty(testBlock, 0))
                            return (byte[])null;
                    }
                }
            }
            return ret;
        }

        private void BlockDecode(byte[] src, int srcOffset, byte[] dest, int destOffset)
        {
            this.CreateDecoderInput(src, srcOffset);
            AMBEConverter.DSPINI_DMR_Voc_Dec(Marshal.UnsafeAddrOfPinnedArrayElement<byte>(this.decoderWorkingBuffer, 0), Marshal.UnsafeAddrOfPinnedArrayElement<byte>(this.decoderInput, 0), Marshal.UnsafeAddrOfPinnedArrayElement<short>(this.decoderOutput, 0));
            Buffer.BlockCopy((Array)this.decoderOutput, 0, dest, destOffset, 320);
        }

        private bool IsEmpty(byte[] buffer, int offset)
        {
            for (int i = 0; i < 320; i++)
            {
                if (buffer[offset + i] != (byte)0)
                    return true;
            }
            return false;
        }

        private bool IsSlientFrame(byte[] buffer, int offset)
        {
            for (int i = 0; i < 5; i++)
            {
                if ((int)AMBEConverter.SilenceFrame[i] != (int)buffer[offset + i])
                    return false;
            }
            return ((int)AMBEConverter.SilenceFrame[5] & 240) == ((int)buffer[offset + 7 - 2] & 240);
        }

        private void CreateDecoderInput(byte[] data, int offset)
        {
            int index1 = 0;
            for (int i = 0; i < 7; i++)
            {
                byte num = data[i + offset];
                for (int j = 0; j < 8 && index1 < 49; ++index1)
                {
                    this.decoderInput[index1] = ((int)num & 128) != 0 ? (byte)1 : (byte)0;
                    num <<= 1;
                    j++;
                }
            }
        }

        public override byte[] Encode(byte[] data)
        {
            int num = data.Length / 320;
            byte[] numArray = new byte[num * 7];
            for (int index = 0; index < num; ++index)
            {
                Buffer.BlockCopy((Array)data, 320 * index, (Array)this.encoderInput, 0, 320);
                int num2 = (int)AMBEConverter.DSPINI_DMR_Voc_Enc(Marshal.UnsafeAddrOfPinnedArrayElement<byte>(this.encoderWorkingBuffer, 0), Marshal.UnsafeAddrOfPinnedArrayElement<short>(this.encoderInput, 0), Marshal.UnsafeAddrOfPinnedArrayElement<bool>(this.encoderOutput, 0));
                this.ConvertOutput(numArray, 7 * index);
            }
            return !Array.Exists<byte>(numArray, (Predicate<byte>)(byte_0 => byte_0 > (byte)0)) ? (byte[])null : numArray;
        }

        private void ConvertOutput(byte[] buffer, int offset)
        {
            int srcOffset = 0;
            for (int i = 0; i < 7; i++)
            {
                byte num = 0;
                for (int j = 0; j < 8 && srcOffset < 49; ++srcOffset)
                {
                    if (this.encoderOutput[srcOffset])
                        num |= (byte)(1 << 7 - j);
                    j++;
                }
                buffer[offset + i] = num;
            }
        }

        public static byte[] SilenceFrame
        {
            get
            {
                return new byte[7] {0xF8, 0x01, 0xA9, 0x9F, 0x8C, 0xE0, 0x80};
            }
        }

        [DllImport("res1033", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DSPINI_DMR_Decoder_Init(IntPtr intptr_0, short short_0);

        [DllImport("res1033", CallingConvention = CallingConvention.Cdecl)]
        private static extern short DSPINI_DMR_Voc_Dec(IntPtr intptr_0, IntPtr intptr_1, IntPtr intptr_2);

        [DllImport("res1033", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DSPINI_DMR_Encoder_Init(IntPtr intptr_0, short short_0, short short_1);

        [DllImport("res1033", CallingConvention = CallingConvention.Cdecl)]
        private static extern short DSPINI_DMR_Voc_Enc(IntPtr intptr_0, IntPtr intptr_1, IntPtr intptr_2);

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    decoderWorkingBufferHandle.Free();
                    decoderInputHandle.Free();
                    decoderOutputHandle.Free();
                    encoderWorkingBufferHandle.Free();
                    encoderOutputHandle.Free();
                    encoderInputHandle.Free();
                }

                disposedValue = true;
            }
        }

        ~AMBEConverter()
        {
           // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
           Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using Xstream.Codec;

#if WIN32
using size_t = System.UInt32;
#else
using size_t = System.UInt64;
#endif

namespace Xstream
{
    public unsafe class DxAudio
    {
        const int XAUDIO2_DEFAULT_CHANNELS = 0;
        const int XAUDIO2_COMMIT_NOW = 0;
        const int SDL_AUDIOBUFFERQUEUE_PACKETLEN = 8 * 1024;

        unsafe class PrivateAudioData : IDisposable
        {
            internal GCHandle handle;

            internal AutoResetEvent semaphore;
            internal byte* mixbuf;
            internal int mixlen;
            internal byte* nextbuf;

            public void Dispose()
            {
                semaphore.Close();

                if (mixbuf != null)
                {
                    Marshal.FreeHGlobal((IntPtr)mixbuf);
                    mixbuf = null;
                }

                if (handle.IsAllocated)
                    handle.Free();
            }
        }

        sealed class Callbacks : CallbackBase, VoiceCallback
        {
            Callbacks() { }

            public static readonly Callbacks Instance = new Callbacks();

            public void OnBufferEnd(IntPtr context)
            {
                // Just signal the SDL audio thread and get out of XAudio2's way.
                DxAudio device = (DxAudio)GCHandle.FromIntPtr(context).Target;
                device._hidden.semaphore.Set();
            }

            public void OnBufferStart(IntPtr context) { }

            public void OnLoopEnd(IntPtr context) { }

            public void OnStreamEnd() { }

            public void OnVoiceError(IntPtr context, Result error)
            {
                DxAudio device = (DxAudio)GCHandle.FromIntPtr(context).Target;
                device.OpenedAudioDeviceDisconnected();
            }

            public void OnVoiceProcessingPassEnd() { }

            public void OnVoiceProcessingPassStart(int bytesRequired) { }
        }

        public bool Initialized => _dev != null;

        string _dev;
        int _sampleRate;
        int _channels;
        int _samples;

        XAudio2 _xaudio2;
        MasteringVoice _masteringVoice;
        WaveFormatEx _waveFormat;
        SourceVoice _sourceVoice;
        int _bufferSize;
        int _bufferSize2;
        PrivateAudioData _hidden;
        DataQueue _queue;
        int _worklen;
        byte* _workbuf;

        Thread _thread;
        object _lock = new object();
        bool _paused;
        bool _shutdown;
        bool _enabled;

        ~DxAudio()
        {
            Close();
        }

        public DxAudio(int sampleRate, int channels)
        {
            _sampleRate = sampleRate;
            _channels = channels;
            _dev = null;
        }

        /*
         * 这个samples是指样本帧中音频缓存区的大小。
         * 样本帧是一块音频数据，其大小指定为 format * channels
         * 其中format指的是每个样本的位数，这里使用WAVE_FORMAT_IEEE_FLOAT即32位4字节浮点型。
         * 在PCM中format等同于sampleSize
         * 
         * @see: https://my.oschina.net/u/4365632/blog/3319770
         *       https://wiki.libsdl.org/SDL_AudioSpec#Remarks
         */
        public int Initialize(int samples)
        {
            _samples = samples;

            _shutdown = false;// just in case.
            _paused = true;
            _enabled = true;

            OpenDevice();

            // pool a few packets to start. Enough for two callbacks.
            _queue = DataQueuePacket.NewDataQueue(SDL_AUDIOBUFFERQUEUE_PACKETLEN, (uint)_bufferSize2);

            // Allocate a scratch audio buffer
            _worklen = _bufferSize;
            _workbuf = (byte*)Marshal.AllocHGlobal(_worklen);

            _thread = new Thread(RunAudio);
            // The audio mixing is always a high priority thread
            _thread.Priority = ThreadPriority.Highest;
            // Start the audio thread
            _thread.Start();

            Pause(0);// start audio playing.

            return 0;
        }

        private void OpenDevice()
        {
            try
            {
                GetGlobalDefaultDevice27();
            }
            catch
            {
                GetGlobalDefaultDevice();
            }

            _xaudio2 = new XAudio2(XAudio2Flags.None, ProcessorSpecifier.DefaultProcessor);

            /*
             * We use XAUDIO2_DEFAULT_CHANNELS instead of _channels. On
             * Xbox360, this means 5.1 output, but on Windows, it means "figure out
             * what the system has." It might be preferable to let XAudio2 blast
             * stereo output to appropriate surround sound configurations
             * instead of clamping to 2 channels, even though we'll configure the
             * Source Voice for whatever number of channels you supply.
             */
            _masteringVoice = new MasteringVoice(_xaudio2, XAUDIO2_DEFAULT_CHANNELS, _sampleRate, _dev);

            _waveFormat = new WaveFormatEx(SDL_AudioFormat.AUDIO_F32, _channels, _sampleRate);
            _sourceVoice = new SourceVoice(_xaudio2
                , _waveFormat.Local
                , VoiceFlags.NoSampleRateConversion | VoiceFlags.NoPitch
                , 1.0f
                , Callbacks.Instance);

            // Initialize all variables that we clean on shutdown
            _hidden = new PrivateAudioData();
            _hidden.handle = GCHandle.Alloc(this);
            _hidden.semaphore = new AutoResetEvent(false);

            _bufferSize = _waveFormat.BlockAlign * _samples;
            _bufferSize2 = _bufferSize * 2;

            // We feed a Source, it feeds the Mastering, which feeds the device.
            _hidden.mixlen = _bufferSize;
            _hidden.mixbuf = (byte*)Marshal.AllocHGlobal(_bufferSize2);
            _hidden.nextbuf = _hidden.mixbuf;
            Program.SetMemory(_hidden.mixbuf, _waveFormat.Silence, (uint)_bufferSize2);

            _xaudio2.StartEngine();
            _sourceVoice.Start(XAUDIO2_COMMIT_NOW);
        }

        /*
         * 相关内容已在Version28中移除
         * 
         * @see: https://docs.microsoft.com/en-us/windows/win32/xaudio2/xaudio2-versions
         */
        private void GetGlobalDefaultDevice27()
        {
            _xaudio2 = new XAudio2(XAudio2Version.Version27);

            for (int i = 0; i < _xaudio2.DeviceCount; i++)
            {
                DeviceDetails device = _xaudio2.GetDeviceDetails(i);

                if (device.Role == DeviceRole.GlobalDefaultDevice)
                {
                    _dev = device.DeviceID;
                    break;
                }
            }

            _xaudio2.Dispose();
        }

        private void GetGlobalDefaultDevice()
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice");
            foreach (ManagementObject mo in mos.Get())
            {
                foreach (var p in mo.Properties)
                {
                    Console.WriteLine($"{p.Name}:{p.Value}");
                }
            }
        }

        public int Update(PCMSample sample)
        {
            fixed (byte* p = sample.SampleData)
            {
                return UpdateAudio(p, (uint)sample.SampleData.Length);
            }
        }

        private int UpdateAudio(byte* data, uint length)
        {
            if (!Initialized)
            {
                Debug.WriteLine("XAudio2 not initialized yet...");
                return -1;
            }

            return QueueAudio(data, length);
        }

        public int Close()
        {
            _dev = null;

            _shutdown = true;
            _enabled = false;

            if (_thread != null)
            {
                _thread.Join();
            }

            if (_workbuf != null)
            {
                Marshal.FreeHGlobal((IntPtr)_workbuf);
                _workbuf = null;
            }

            if (_hidden != null)
            {
                CloseDevice();
            }

            DataQueuePacket.FreeDataQueue(_queue);

            return 0;
        }

        // The general mixing thread function
        private void RunAudio()
        {
            uint data_len = (uint)_bufferSize;
            int delay = _samples * 1000 / _sampleRate;
            byte* data;

            // Loop, filling the audio buffers
            while (!_shutdown)
            {
                // Fill the current buffer with sound
                if (_enabled)
                {
                    data = _hidden.nextbuf;
                }
                else
                {
                    /*
                     * if the device isn't enabled, we still write to the
                     * work_buffer, so the app's callback will fire with
                     * a regular frequency, in case they depend on that
                     * for timing or progress. They can use hotplug
                     * now to know if the device failed.
                     */
                    data = null;
                }

                if (data == null)
                {
                    data = _workbuf;
                }

                lock (_lock)
                {
                    if (_paused)
                    {
                        Program.SetMemory(data, _waveFormat.Silence, data_len);
                    }
                    else
                    {
                        BufferQueueDrainCallback(data, data_len);
                    }
                }

                if (data == _workbuf)
                {
                    // nothing to do; pause like we queued a buffer to play.
                    Program.Delay(delay);
                }
                else// writing directly to the device.
                {
                    // queue this buffer and wait for it to finish playing.
                    PlayDevice();
                    WaitDevice();
                }
            }

            _sourceVoice?.Discontinuity();

            // Wait for the audio to drain.
            Program.Delay(delay * 2);
        }

        private void BufferQueueDrainCallback(byte* stream, uint len)
        {
            // this function always holds the mixer lock before being called.
            uint dequeued = (uint)DataQueuePacket.ReadFromDataQueue(_queue, stream, len);

            stream += dequeued;
            len -= dequeued;

            if (len > 0)
            {
                // fill any remaining space in the stream with silence.
                Program.SetMemory(stream, _waveFormat.Silence, len);
            }
        }

        private void PlayDevice()
        {
            if (!_enabled)// shutting down?
                return;

            AudioBuffer buffer = new AudioBuffer();
            byte* mixbuf = _hidden.mixbuf;
            byte* nextbuf = _hidden.nextbuf;

            buffer.AudioBytes = _hidden.mixlen;
            buffer.AudioDataPointer = (IntPtr)nextbuf;
            buffer.Context = GCHandle.ToIntPtr(_hidden.handle);

            if (nextbuf == mixbuf)
            {
                nextbuf += _hidden.mixlen;
            }
            else
            {
                nextbuf = mixbuf;
            }
            _hidden.nextbuf = nextbuf;

            try
            {
                _sourceVoice.SubmitSourceBuffer(buffer, null);
            }
            catch (SharpDXException e)
            {
                if (e.ResultCode.Code == ResultCode.DeviceInvalidated.Code)
                {
                    // !!! FIXME: possibly disconnected or temporary lost. Recover?
                }

                // uhoh, panic!
                _sourceVoice.FlushSourceBuffers();
                OpenedAudioDeviceDisconnected();
            }
        }

        private void WaitDevice()
        {
            if (_enabled)
            {
                _hidden.semaphore.WaitOne();
            }
        }

        public void Pause(int pause_on)
        {
            lock (_lock)
            {
                _paused = pause_on != 0 ? true : false;
            }
        }

        private void CloseDevice()
        {
            _sourceVoice?.Stop(PlayFlags.None, XAUDIO2_COMMIT_NOW);
            _sourceVoice?.FlushSourceBuffers();
            _sourceVoice?.DestroyVoice();
            _sourceVoice?.Dispose();

            _xaudio2?.StopEngine();

            _masteringVoice?.DestroyVoice();
            _masteringVoice?.Dispose();

            _xaudio2?.Dispose();

            _sourceVoice = null;
            _masteringVoice = null;
            _xaudio2 = null;

            _hidden?.Dispose();
            _hidden = null;
        }

        // The audio backends call this when a currently-opened device is lost.
        private void OpenedAudioDeviceDisconnected()
        {
            if (!_enabled)
                return;

            lock (_lock)
            {
                // Ends the audio callback and mark the device as STOPPED, but the
                // app still needs to close the device to free resources.
                _enabled = false;
            }
        }

        private int QueueAudio(byte* data, uint len)
        {
            if (len > 0)
            {
                lock (_lock)
                {
                    return DataQueuePacket.WriteToDataQueue(_queue, data, len);
                }
            }

            return 0;
        }
    }

    struct WaveFormatEx
    {
        [StructLayout(LayoutKind.Sequential)]
        struct WAVEFORMATEX_C
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        public WaveFormat Local
        {
            get => WaveFormat.CreateCustomFormat(FormatTag
                , SamplesPerSec
                , Channels
                , AvgBytesPerSec
                , BlockAlign
                , BitsPerSample);
        }

        public int Silence;

        public WaveFormatEncoding FormatTag;
        public int Channels;
        public int SamplesPerSec;
        public int AvgBytesPerSec;
        public int BlockAlign;
        public int BitsPerSample;
        public int ExtraSize;

        public WaveFormatEx(SDL_AudioFormat format, int nChannels, int nSamplesPerSec)
        {
            SDL_AudioFormat test_format = format.FirstAudioFormat();
            bool valid_format = false;

            while (!valid_format && test_format != 0)
            {
                switch (test_format)
                {
                    case SDL_AudioFormat.AUDIO_U8:
                    case SDL_AudioFormat.AUDIO_S16:
                    case SDL_AudioFormat.AUDIO_S32:
                    case SDL_AudioFormat.AUDIO_F32:
                        format = test_format;
                        valid_format = true;
                        break;
                }
                test_format = format.NextAudioFormat();
            }

            if (!valid_format)
            {
                throw new NotSupportedException("XAudio2: Unsupported audio format");
            }

            switch (format)
            {
                case SDL_AudioFormat.AUDIO_U8:
                    Silence = 0x80;
                    break;
                default:
                    Silence = 0x00;
                    break;
            }

            FormatTag = format.AUDIO_ISFLOAT() ? WaveFormatEncoding.IeeeFloat : WaveFormatEncoding.Pcm;
            BitsPerSample = format.AUDIO_BITSIZE();
            Channels = nChannels;
            SamplesPerSec = nSamplesPerSec;

            BlockAlign = nChannels * (BitsPerSample / 8);
            AvgBytesPerSec = nSamplesPerSec * BlockAlign;
            ExtraSize = Marshal.SizeOf<WAVEFORMATEX_C>();
        }
    }

    static class SDL_AudioFormat_Extensions
    {
        static int AUDIO_MASK_BITSIZE = 0xFF;
        static int AUDIO_MASK_DATATYPE = 1 << 8;
        static int AUDIO_MASK_ENDIAN = 1 << 12;
        static int AUDIO_MASK_SIGNED = 1 << 15;

        static int AND(this SDL_AudioFormat x, int y) => (int)x & y;

        public static int AUDIO_BITSIZE(this SDL_AudioFormat x) => x.AND(AUDIO_MASK_BITSIZE);

        public static bool AUDIO_ISFLOAT(this SDL_AudioFormat x) => x.AND(AUDIO_MASK_DATATYPE) != 0;
        public static bool AUDIO_ISBIGENDIAN(this SDL_AudioFormat x) => x.AND(AUDIO_MASK_ENDIAN) != 0;
        public static bool AUDIO_ISSIGNED(this SDL_AudioFormat x) => x.AND(AUDIO_MASK_SIGNED) != 0;

        public static bool AUDIO_ISINT(this SDL_AudioFormat x) => !x.AUDIO_ISFLOAT();
        public static bool AUDIO_ISLITTLEENDIAN(this SDL_AudioFormat x) => !x.AUDIO_ISBIGENDIAN();
        public static bool AUDIO_ISUNSIGNED(this SDL_AudioFormat x) => !x.AUDIO_ISSIGNED();

        static int _formatIdx;
        static int _formatIdxSub;
        static SDL_AudioFormat[][] _formatList = new SDL_AudioFormat[][] {
            new SDL_AudioFormat[]{SDL_AudioFormat.AUDIO_U8, SDL_AudioFormat.AUDIO_S8, SDL_AudioFormat.AUDIO_S16LSB, SDL_AudioFormat.AUDIO_S16MSB, SDL_AudioFormat.AUDIO_U16LSB,SDL_AudioFormat.AUDIO_U16MSB, SDL_AudioFormat.AUDIO_S32LSB, SDL_AudioFormat.AUDIO_S32MSB, SDL_AudioFormat.AUDIO_F32LSB, SDL_AudioFormat.AUDIO_F32MSB},
            new SDL_AudioFormat[]{SDL_AudioFormat.AUDIO_S8, SDL_AudioFormat.AUDIO_U8, SDL_AudioFormat.AUDIO_S16LSB, SDL_AudioFormat.AUDIO_S16MSB, SDL_AudioFormat.AUDIO_U16LSB,SDL_AudioFormat.AUDIO_U16MSB, SDL_AudioFormat.AUDIO_S32LSB, SDL_AudioFormat.AUDIO_S32MSB, SDL_AudioFormat.AUDIO_F32LSB, SDL_AudioFormat.AUDIO_F32MSB},
            new SDL_AudioFormat[]{SDL_AudioFormat.AUDIO_S16LSB, SDL_AudioFormat.AUDIO_S16MSB, SDL_AudioFormat.AUDIO_U16LSB, SDL_AudioFormat.AUDIO_U16MSB, SDL_AudioFormat.AUDIO_S32LSB,SDL_AudioFormat.AUDIO_S32MSB, SDL_AudioFormat.AUDIO_F32LSB, SDL_AudioFormat.AUDIO_F32MSB, SDL_AudioFormat.AUDIO_U8, SDL_AudioFormat.AUDIO_S8},
            new SDL_AudioFormat[]{SDL_AudioFormat.AUDIO_S16MSB, SDL_AudioFormat.AUDIO_S16LSB, SDL_AudioFormat.AUDIO_U16MSB, SDL_AudioFormat.AUDIO_U16LSB, SDL_AudioFormat.AUDIO_S32MSB,SDL_AudioFormat.AUDIO_S32LSB, SDL_AudioFormat.AUDIO_F32MSB, SDL_AudioFormat.AUDIO_F32LSB, SDL_AudioFormat.AUDIO_U8,SDL_AudioFormat. AUDIO_S8},
            new SDL_AudioFormat[]{SDL_AudioFormat.AUDIO_U16LSB,SDL_AudioFormat. AUDIO_U16MSB, SDL_AudioFormat.AUDIO_S16LSB,SDL_AudioFormat. AUDIO_S16MSB, SDL_AudioFormat.AUDIO_S32LSB,SDL_AudioFormat.AUDIO_S32MSB, SDL_AudioFormat.AUDIO_F32LSB, SDL_AudioFormat.AUDIO_F32MSB, SDL_AudioFormat.AUDIO_U8, SDL_AudioFormat.AUDIO_S8},
            new SDL_AudioFormat[]{SDL_AudioFormat.AUDIO_U16MSB, SDL_AudioFormat.AUDIO_U16LSB, SDL_AudioFormat.AUDIO_S16MSB, SDL_AudioFormat.AUDIO_S16LSB, SDL_AudioFormat.AUDIO_S32MSB,SDL_AudioFormat.AUDIO_S32LSB, SDL_AudioFormat.AUDIO_F32MSB, SDL_AudioFormat.AUDIO_F32LSB, SDL_AudioFormat.AUDIO_U8, SDL_AudioFormat.AUDIO_S8},
            new SDL_AudioFormat[]{SDL_AudioFormat.AUDIO_S32LSB, SDL_AudioFormat.AUDIO_S32MSB, SDL_AudioFormat.AUDIO_F32LSB, SDL_AudioFormat.AUDIO_F32MSB, SDL_AudioFormat.AUDIO_S16LSB,SDL_AudioFormat.AUDIO_S16MSB,SDL_AudioFormat.AUDIO_U16LSB, SDL_AudioFormat.AUDIO_U16MSB, SDL_AudioFormat.AUDIO_U8, SDL_AudioFormat.AUDIO_S8},
            new SDL_AudioFormat[]{SDL_AudioFormat.AUDIO_S32MSB, SDL_AudioFormat.AUDIO_S32LSB, SDL_AudioFormat.AUDIO_F32MSB, SDL_AudioFormat.AUDIO_F32LSB, SDL_AudioFormat.AUDIO_S16MSB,SDL_AudioFormat.AUDIO_S16LSB, SDL_AudioFormat.AUDIO_U16MSB, SDL_AudioFormat.AUDIO_U16LSB, SDL_AudioFormat.AUDIO_U8, SDL_AudioFormat.AUDIO_S8},
            new SDL_AudioFormat[]{SDL_AudioFormat.AUDIO_F32LSB, SDL_AudioFormat.AUDIO_F32MSB, SDL_AudioFormat.AUDIO_S32LSB, SDL_AudioFormat.AUDIO_S32MSB, SDL_AudioFormat.AUDIO_S16LSB,SDL_AudioFormat.AUDIO_S16MSB, SDL_AudioFormat.AUDIO_U16LSB, SDL_AudioFormat.AUDIO_U16MSB, SDL_AudioFormat.AUDIO_U8, SDL_AudioFormat.AUDIO_S8},
            new SDL_AudioFormat[]{SDL_AudioFormat.AUDIO_F32MSB, SDL_AudioFormat.AUDIO_F32LSB, SDL_AudioFormat.AUDIO_S32MSB, SDL_AudioFormat.AUDIO_S32LSB, SDL_AudioFormat.AUDIO_S16MSB,SDL_AudioFormat.AUDIO_S16LSB, SDL_AudioFormat.AUDIO_U16MSB, SDL_AudioFormat.AUDIO_U16LSB,SDL_AudioFormat. AUDIO_U8, SDL_AudioFormat.AUDIO_S8},
        };

        public static SDL_AudioFormat FirstAudioFormat(this SDL_AudioFormat format)
        {
            for (_formatIdx = 0; _formatIdx < _formatList.Length; ++_formatIdx)
            {
                if (_formatList[_formatIdx][0] == format)
                {
                    break;
                }
            }
            _formatIdxSub = 0;
            return NextAudioFormat();
        }

        public static SDL_AudioFormat NextAudioFormat(this SDL_AudioFormat useless) => NextAudioFormat();

        static SDL_AudioFormat NextAudioFormat()
        {
            if (_formatIdx == _formatList.Length || _formatIdxSub == _formatList[0].Length)
            {
                return 0;
            }
            return _formatList[_formatIdx][_formatIdxSub++];
        }
    }

    enum SDL_AudioFormat
    {
        AUDIO_U8 = 0x0008,
        AUDIO_S8 = 0x8008,
        AUDIO_U16LSB = 0x0010,
        AUDIO_S16LSB = 0x8010,
        AUDIO_U16MSB = 0x1010,
        AUDIO_S16MSB = 0x9010,
        AUDIO_U16 = AUDIO_U16LSB,
        AUDIO_S16 = AUDIO_S16LSB,

        AUDIO_S32LSB = 0x8020,
        AUDIO_S32MSB = 0x9020,
        AUDIO_S32 = AUDIO_S32LSB,

        AUDIO_F32LSB = 0x8120,
        AUDIO_F32MSB = 0x9120,
        AUDIO_F32 = AUDIO_F32LSB,

        // x86 little-endian
        AUDIO_U16SYS = AUDIO_U16LSB,
        AUDIO_S16SYS = AUDIO_S16LSB,
        AUDIO_S32SYS = AUDIO_S32LSB,
        AUDIO_F32SYS = AUDIO_F32LSB
    }
}

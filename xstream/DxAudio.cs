using NAudio.CoreAudioApi;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Diagnostics;
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

        struct PrivateAudioData
        {
            //internal GCHandle handle;
            //internal IntPtr device;

            internal AudioBuffer buffer;
            internal AutoResetEvent semaphore;
            internal byte* mixbuf;
            internal int mixlen;
            internal byte* nextbuf;
        }

        /*
        sealed class Callbacks : CallbackBase, VoiceCallback
        {
            private Callbacks() { }

            public static readonly Callbacks Instance = new Callbacks();

            public void OnBufferEnd(IntPtr context)
            {
                // Just signal the audio thread and get out of XAudio2's way.
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
        */

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
        PrivateAudioData _hidden;
        DataQueue _queue;
        int _worklen;
        byte* _workbuf;

        Thread _thread;
        object _lock = new object();
        bool _paused;
        bool _shutdown;
        bool _enabled;

        public DxAudio(int sampleRate, int channels)
        {
            _sampleRate = sampleRate;
            _channels = channels;
            _dev = null;
        }

        /*
         * 这个samples是指样本帧中音频缓存区的大小。
         * 样本帧是一块音频数据，其大小指定为 format * channels
         * 其中format指的是每个样本的位数，这里使用WAVE_FORMAT_IEEE_FLOAT即4字节(32位)浮点型。
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

            try
            {
                OpenDevice();

                // pool a few packets to start. Enough for two callbacks.
                _queue = DataQueuePacket.NewDataQueue(SDL_AUDIOBUFFERQUEUE_PACKETLEN, (size_t)(_bufferSize * 2));

                // Allocate a scratch audio buffer
                _worklen = _bufferSize;
                _workbuf = (byte*)Marshal.AllocHGlobal(_worklen);

                _thread = new Thread(RunAudio);

                // The audio mixing is always a high priority thread
                _thread.Priority = ThreadPriority.Highest;

                // Start the audio thread
                _thread.Start();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to open audio: {e.Message}");
                Close();
                return 1;
            }

            Pause(0);// start audio playing.

            return 0;
        }

        private void OpenDevice()
        {
            try
            {
                /*
                 * 相关内容已在Version28中移除
                 * 
                 * @see: https://docs.microsoft.com/en-us/windows/win32/xaudio2/xaudio2-versions
                 */
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
            catch
            {
                var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                _dev = device?.ID;
            }

            Debug.Assert(_xaudio2 == null || _xaudio2.IsDisposed);

            if (_dev == null)
            {
                // 在CreateMasteringVoice时将szDeviceId指定默认值NULL会使XAudio2选择全局默认音频设备
                // 由于之前我们已经主动获取设备ID了，为了避免出现意外情况，这里直接抛错就行了
                throw new NotSupportedException("没有扬声器");
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
                , _waveFormat
                , VoiceFlags.NoSampleRateConversion | VoiceFlags.NoPitch
                , 1.0f
                //, Callbacks.Instance);
                , true);

            _sourceVoice.BufferEnd += OnBufferEnd;
            _sourceVoice.VoiceError += OnVoiceError;

            _bufferSize = _waveFormat.BlockAlign * _samples;

            //_hidden.handle = GCHandle.Alloc(this);
            //_hidden.device = GCHandle.ToIntPtr(_hidden.handle);
            _hidden.buffer = new AudioBuffer();
            _hidden.semaphore = new AutoResetEvent(false);

            // We feed a Source, it feeds the Mastering, which feeds the device.
            _hidden.mixlen = _bufferSize;
            _hidden.mixbuf = (byte*)Marshal.AllocHGlobal(2 * _hidden.mixlen);
            _hidden.nextbuf = _hidden.mixbuf;
            Program.SetMemory(_hidden.mixbuf, _waveFormat.Silence, (size_t)(2 * _hidden.mixlen));

            // Start everything playing!
            _xaudio2.StartEngine();
            _sourceVoice.Start(XAUDIO2_COMMIT_NOW);
        }

        // The general mixing thread function
        private void RunAudio()
        {
            int delay = _samples * 1000 / _sampleRate;
            size_t data_len = (size_t)_bufferSize;
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

            _sourceVoice.Discontinuity();

            // Wait for the audio to drain.
            Program.Delay(delay * 2);
        }

        private void BufferQueueDrainCallback(byte* stream, size_t len)
        {
            // this function always holds the mixer lock before being called.
            size_t dequeued;

            Debug.Assert(Initialized);// this shouldn't ever happen, right?!

            dequeued = DataQueuePacket.ReadFromDataQueue(_queue, stream, len);
            stream += dequeued;
            len -= dequeued;

            if (len > 0)// fill any remaining space in the stream with silence.
            {
                Debug.Assert(DataQueuePacket.CountDataQueue(_queue) == 0);
                Program.SetMemory(stream, _waveFormat.Silence, len);
            }
        }

        private void PlayDevice()
        {
            AudioBuffer buffer;
            byte* mixbuf = _hidden.mixbuf;
            byte* nextbuf = _hidden.nextbuf;
            int mixlen = _hidden.mixlen;

            if (!_enabled)// shutting down?
                return;

            // Submit the next filled buffer
            buffer = _hidden.buffer;
            buffer.AudioBytes = mixlen;
            buffer.AudioDataPointer = (IntPtr)nextbuf;
            //buffer.Context = _hidden.device;

            if (nextbuf == mixbuf)
            {
                nextbuf += mixlen;
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
                if (e.HResult == ResultCode.DeviceInvalidated.Code)
                {
                    // !!! FIXME: possibly disconnected or temporary lost. Recover?
                }

                // uhoh, panic!
                _sourceVoice.FlushSourceBuffers();
                OpenedAudioDeviceDisconnected();
            }
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

            // Post the event, if desired
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

        public int Update(PCMSample sample)
        {
            fixed (byte* p = sample.SampleData)
            {
                return QueueAudio(p, (size_t)sample.SampleData.Length);
            }
        }

        private int QueueAudio(void* data, size_t len)
        {
            if (!Initialized)
            {
                Debug.WriteLine("XAudio2 not initialized yet...");
                return -1;
            }

            if (len > 0)
            {
                lock (_lock)
                {
                    return DataQueuePacket.WriteToDataQueue(_queue, data, len);
                }
            }

            return 0;
        }

        public void ClearQueuedAudio()
        {
            if (!Initialized)
                return;// nothing to do.

            // Blank out the device and release the mutex. Free it afterwards.
            lock (_lock)
            {
                // Keep up to two packets in the pool to reduce future malloc pressure.
                DataQueuePacket.ClearDataQueue(_queue, SDL_AUDIOBUFFERQUEUE_PACKETLEN * 2);
            }
        }

        public void Close()
        {
            if (!Initialized)
                return;

            _shutdown = true;
            _enabled = false;
            _thread?.Join();

            if (_workbuf != null)
            {
                Marshal.FreeHGlobal((IntPtr)_workbuf);
                _workbuf = null;
            }

            DataQueuePacket.FreeDataQueue(_queue);

            CloseDevice();
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

            //if (_hidden.handle.IsAllocated)
            //{
            //    _hidden.handle.Free();
            //    _hidden.device = IntPtr.Zero;
            //}

            if (_hidden.mixbuf != null)
            {
                Marshal.FreeHGlobal((IntPtr)_hidden.mixbuf);
                _hidden.mixbuf = null;
            }

            _dev = null;
        }

        private void OnBufferEnd(IntPtr context) => _hidden.semaphore.Set();
        private void OnVoiceError(SourceVoice.VoiceErrorArgs args) => OpenedAudioDeviceDisconnected();
    }

    class WaveFormatEx : WaveFormat
    {
        public int Silence;

        public WaveFormatEx(SDL_AudioFormat format, int channels, int sampleRate)
            : base(sampleRate, format.AUDIO_BITSIZE(), channels)
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

            waveFormatTag = format.AUDIO_ISFLOAT() ? WaveFormatEncoding.IeeeFloat : WaveFormatEncoding.Pcm;
            //bitsPerSample = (short)format.AUDIO_BITSIZE();
            //this.channels = (short)channels;
            //this.sampleRate = sampleRate;

            //blockAlign = (short)(channels * (bitsPerSample / 8));
            //averageBytesPerSecond = sampleRate * blockAlign;
            extraSize = (short)Marshal.SizeOf<WAVEFORMATEX>();
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
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

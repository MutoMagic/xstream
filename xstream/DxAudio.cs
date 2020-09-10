using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Xstream.Codec;

namespace Xstream
{
    public unsafe class DxAudio
    {
        public bool Initialized => _dev != null;

        string _dev;
        int _sampleRate;
        int _channels;
        int _samples;

        XAudio2 _xaudio2;
        MasteringVoice _masteringVoice;
        WAVEFORMATEX _waveFormat;
        SourceVoice _sourceVoice;

        int _bufferSize;
        DataQueue _queue;
        byte* _mixbuf;
        int _mixlen;
        byte* _nextbuf;

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

            // _waveFormat.wBitsPerSample / 8 * _channels * samples;
            _bufferSize = _waveFormat.nBlockAlign * samples;

            const uint packetlen = 8 * 1024;// SDL_AUDIOBUFFERQUEUE_PACKETLEN
            uint initialslack = (uint)(_bufferSize * 2);
            uint wantpackets = initialslack + (packetlen - 1) / packetlen;

            _queue = new DataQueue();
            _queue.packet_size = packetlen;

            for (int i = 0; i < wantpackets; i++)
            {
                DataQueuePacket packet = new DataQueuePacket(packetlen);

                // don't care if this fails, we'll deal later.
                if (packet.data != null)
                {
                    //packet.datalen = 0;
                    //packet.startpos = 0;
                    packet.next = _queue.pool;
                    _queue.pool = packet;
                }
            }

            _mixlen = 2 * _bufferSize;
            _mixbuf = (byte*)Marshal.AllocHGlobal(_mixlen);
            _nextbuf = _mixbuf;
            Program.SetMemory(_mixbuf, _waveFormat.silence, _mixlen);

            _worklen = _bufferSize;
            _workbuf = (byte*)Marshal.AllocHGlobal(_worklen);

            _thread = new Thread(RunAudio);
            //The audio mixing is always a high priority thread
            _thread.Priority = ThreadPriority.Highest;
            _thread.Start();

            Pause(0);// start audio playing.

            return 0;
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

            if (length > 0)
            {
                lock (_lock)
                {
                    return DataQueuePacket.WriteToDataQueue(_queue, data, length);
                }
            }

            return 0;
        }

        public int Close()
        {
            _dev = null;

            _shutdown = true;
            _enabled = false;

            _sourceVoice?.Stop();
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

            if (_mixbuf != null)
            {
                Marshal.FreeHGlobal((IntPtr)_mixbuf);
                _mixbuf = null;
            }

            if (_workbuf != null)
            {
                Marshal.FreeHGlobal((IntPtr)_workbuf);
                _workbuf = null;
            }

            return 0;
        }

        private void OpenDevice()
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
            _xaudio2 = new XAudio2();

            // The mastering voices encapsulates an audio device.
            // It is the ultimate destination for all audio that passes through an audio graph.
            // #define XAUDIO2_DEFAULT_CHANNELS 0
            _masteringVoice = new MasteringVoice(_xaudio2, 0, _sampleRate, _dev);

            _waveFormat = new WAVEFORMATEX(SDL_AudioFormat.AUDIO_F32, _channels, _sampleRate);
            _sourceVoice = new SourceVoice(_xaudio2, _waveFormat.local);

            _xaudio2.StartEngine();
            _sourceVoice.Start();
        }

        // The general mixing thread function
        private void RunAudio()
        {
            int delay = _samples * 1000 / _sampleRate;

            byte* data;

            // Loop, filling the audio buffers
            while (!_shutdown)
            {
                // Fill the current buffer with sound
                if (_enabled)
                {
                    data = _nextbuf;
                }
                else
                {
                    /*
                     * if the device isn't enabled, we still write to the
                     * work_buffer, so the app's callback will fire with
                     * a regular frequency, in case they depend on that
                     * for timing or progress. They can use hotplug
                     * now to know if the device failed.
                     * Streaming playback uses work_buffer, too.
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
                        Program.SetMemory(data, _waveFormat.silence, _bufferSize);
                    }
                    else
                    {
                        BufferQueueDrainCallback(data, _bufferSize);
                    }
                }

                if (data == _workbuf)
                {
                    // nothing to do; pause like we queued a buffer to play.
                    Program.Delay(delay);
                }
                else
                {
                    // writing directly to the device.
                    // queue this buffer and wait for it to finish playing.
                    PlayDevice();
                    WaitDevice();
                }
            }

            // Wait for the audio to drain.
            Program.Delay(delay * 2);
        }

        private void BufferQueueDrainCallback(byte* stream, int len)
        {
            // this function always holds the mixer lock before being called.
            uint dequeued = DataQueuePacket.ReadFromDataQueue(_queue, stream, (uint)len);
            stream += dequeued;
            len -= (int)dequeued;

            if (len > 0)
            {
                // fill any remaining space in the stream with silence.
                Program.SetMemory(stream, _waveFormat.silence, len);
            }
        }

        private void PlayDevice()
        {

        }

        private void WaitDevice()
        {

        }

        public void Pause(int pause_on)
        {
            lock (_lock)
            {
                _paused = pause_on != 0 ? true : false;
            }
        }
    }

    struct WAVEFORMATEX
    {
        public WaveFormat local;
        public int silence;

        public WaveFormatEncoding wFormatTag;
        public int nChannels;
        public int nSamplesPerSec;
        public int nAvgBytesPerSec;
        public int nBlockAlign;
        public int wBitsPerSample;
        public int cbSize;

        public WAVEFORMATEX(SDL_AudioFormat format, int nChannels, int nSamplesPerSec)
        {
            switch (format)
            {
                case SDL_AudioFormat.AUDIO_U8:
                    silence = 0x80;
                    break;
                default:
                    silence = 0x00;
                    break;
            }

            wFormatTag = format.AUDIO_ISFLOAT() ? WaveFormatEncoding.IeeeFloat : WaveFormatEncoding.Pcm;
            wBitsPerSample = format.AUDIO_BITSIZE();
            this.nChannels = nChannels;
            this.nSamplesPerSec = nSamplesPerSec;

            nBlockAlign = wBitsPerSample / 8 * nChannels;
            nAvgBytesPerSec = nSamplesPerSec * nBlockAlign;
            cbSize = 0;

            local = WaveFormat.CreateCustomFormat(wFormatTag
                , nSamplesPerSec
                , nChannels
                , nAvgBytesPerSec
                , nBlockAlign
                , wBitsPerSample);
        }
    }

    static class SDL_AudioFormat_Extensions
    {
        public static int AND(this SDL_AudioFormat x, int y) => (int)x & y;

        public static int AUDIO_BITSIZE(this SDL_AudioFormat x) => x.AND(0xFF);

        public static bool AUDIO_ISFLOAT(this SDL_AudioFormat x) => x.AND(1 << 8) == 1;
        public static bool AUDIO_ISBIGENDIAN(this SDL_AudioFormat x) => x.AND(1 << 12) == 1;
        public static bool AUDIO_ISSIGNED(this SDL_AudioFormat x) => x.AND(1 << 15) == 1;

        public static bool AUDIO_ISINT(this SDL_AudioFormat x) => !x.AUDIO_ISFLOAT();
        public static bool AUDIO_ISLITTLEENDIAN(this SDL_AudioFormat x) => !x.AUDIO_ISBIGENDIAN();
        public static bool AUDIO_ISUNSIGNED(this SDL_AudioFormat x) => !x.AUDIO_ISSIGNED();
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

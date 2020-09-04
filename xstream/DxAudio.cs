using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
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

        XAudio2 _xaudio2;
        MasteringVoice _masteringVoice;
        WaveFormat _waveFormat;
        SourceVoice _sourceVoice;

        int _bufferSize;
        DataQueue _queue;

        Thread _thread;
        int _threadId;

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
            _shutdown = false;// just in case.
            _paused = true;
            _enabled = true;

            OpenDevice();

            // waveFormat.BitsPerSample / 8 * _channels * samples;
            _bufferSize = _waveFormat.BlockAlign * samples;

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

            _thread = new Thread(RunAudio);

            //The audio mixing is always a high priority thread
            _thread.Priority = ThreadPriority.Highest;
            _threadId = _thread.ManagedThreadId;

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

            _waveFormat = new WaveFormat(_sampleRate, 32, _channels);// BitsPerSample = 32
            _sourceVoice = new SourceVoice(_xaudio2, _waveFormat);

            _xaudio2.StartEngine();
            _sourceVoice.Start();
        }

        private void RunAudio()
        {
            while (!_shutdown)
            {

            }
        }

        private void BufferQueueDrainCallback(byte* stream, int len)
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
}

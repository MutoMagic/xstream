using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using SharpDX.XAudio2.Fx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xstream.Codec;

namespace Xstream
{
    struct DataQueuePacket
    {
        internal uint datalen;// bytes currently in use in this packet.
        internal int startpos;// bytes currently consumed in this packet.
        internal IntPtr data;// packet data
    }

    public unsafe class DxAudio
    {
        public bool Initialized => _dev != null;

        string _dev;
        int _sampleRate;
        int _channels;

        XAudio2 _xaudio2;
        MasteringVoice _masteringVoice;

        SourceVoice _sourceVoice;
        int _bufferSize;

        object _lock = new object();
        bool _paused;
        Queue<DataQueuePacket> _buffer_queue;

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

            var waveFormat = new WaveFormat(_sampleRate, 32, _channels);// BitsPerSample = 32
            _sourceVoice = new SourceVoice(_xaudio2, waveFormat);

            _xaudio2.StartEngine();
            _sourceVoice.Start();

            // waveFormat.BitsPerSample / 8 * _channels * samples;
            _bufferSize = waveFormat.BlockAlign * samples;
            _buffer_queue = new Queue<DataQueuePacket>();

            Pause(0);// start audio playing.

            return 0;
        }

        public int Update(PCMSample sample)
        {
            fixed (byte* p = sample.SampleData)
            {
                return UpdateAudio((IntPtr)p, (uint)sample.SampleData.Length);
            }
        }

        private int UpdateAudio(IntPtr data, uint length)
        {
            if (!Initialized)
            {
                Debug.WriteLine("XAudio2 not initialized yet...");
                return -1;
            }

            return Queue(data, length);
        }

        public int Close()
        {
            _sourceVoice?.Stop();
            _sourceVoice?.FlushSourceBuffers();
            _sourceVoice?.DestroyVoice();
            _sourceVoice?.Dispose();

            _xaudio2?.StopEngine();

            _masteringVoice?.DestroyVoice();
            _masteringVoice?.Dispose();

            _xaudio2?.Dispose();

            _dev = null;
            _sourceVoice = null;
            _masteringVoice = null;
            _xaudio2 = null;

            return 0;
        }

        public void Pause(int pause_on)
        {
            lock (_lock)
            {
                _paused = pause_on != 0 ? true : false;
            }
        }

        public int Queue(IntPtr data, uint length)
        {
            int rc = 0;

            if (!Initialized)
                return -1;

            lock (_lock)
            {
                if (length > 0)
                    rc = WriteToDataQueue(data, length);
            }

            return rc;
        }

        public int WriteToDataQueue(IntPtr data, uint length)
        {
            DataQueuePacket packet;

            packet.datalen = length;
            packet.startpos = 0;
            packet.data = data;

            _buffer_queue.Enqueue(packet);

            return 0;
        }
    }
}

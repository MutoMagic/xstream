using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xstream.Codec;

namespace Xstream
{
    unsafe struct DataQueuePacket
    {
        internal uint datalen;// bytes currently in use in this packet.
        internal uint startpos;// bytes currently consumed in this packet.
        object _next;// next item in linked list.
        internal byte* data;// packet data

        // 防止在结构布局中导致循环
        internal DataQueuePacket? next
        {
            get { return (DataQueuePacket?)_next; }
            set { _next = value; }
        }

        public DataQueuePacket(DataQueuePacket? next, uint packetlen)
        {
            datalen = 0;
            startpos = 0;
            _next = next;

            // #define SDL_VARIABLE_LENGTH_ARRAY 1
            // Uint8 data[SDL_VARIABLE_LENGTH_ARRAY];
            data = (byte*)Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte))
                + (int)packetlen);
        }
    }

    struct DataQueue
    {
        internal DataQueuePacket? head;// device fed from here.
        internal DataQueuePacket? tail;// queue fills to here.
        internal DataQueuePacket? pool;// these are unused packets.
        internal uint packet_size;// size of new packets
        internal uint queued_bytes;// number of bytes of data in the queue.

        public DataQueue(uint packetlen)
        {
            head = null;
            tail = null;
            pool = null;
            packet_size = packetlen;
            queued_bytes = 0;
        }
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
        DataQueue _buffer_queue;

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

            const uint packetlen = 8 * 1024;// SDL_AUDIOBUFFERQUEUE_PACKETLEN
            uint initialslack = (uint)(_bufferSize * 2);
            uint wantpackets = initialslack + (packetlen - 1) / packetlen;

            _buffer_queue = new DataQueue(packetlen);

            for (int i = 0; i < wantpackets; i++)
            {
                _buffer_queue.pool = new DataQueuePacket(_buffer_queue.pool, packetlen);
            }

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

            QueueAudio(data, length);
            return 0;
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

        private void QueueAudio(IntPtr data, uint length)
        {
            if (length > 0)
            {
                lock (_lock)
                {
                    WriteToDataQueue(data, length);
                }
            }
        }

        private void WriteToDataQueue(IntPtr data, uint length)
        {
            while (length > 0)
            {
                DataQueuePacket? packet = _buffer_queue.tail;
                if (!packet.HasValue || packet?.datalen >= _buffer_queue.packet_size)
                {
                    // tail packet missing or completely full; we need a new packet.
                }
            }
        }

        public void BufferQueueDrainCallback(byte* stream, int len)
        {

        }
    }
}

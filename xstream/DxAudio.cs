using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using Xstream.Codec;

namespace Xstream
{
    unsafe class DataQueuePacket
    {
        internal uint datalen;// bytes currently in use in this packet.
        internal uint startpos;// bytes currently consumed in this packet.
        internal DataQueuePacket next;// next item in linked list.
        // #define SDL_VARIABLE_LENGTH_ARRAY 1
        // Uint8 data[SDL_VARIABLE_LENGTH_ARRAY];
        internal byte* data;// packet data

        public DataQueuePacket(DataQueuePacket _next, uint packetlen)
        {
            datalen = 0;
            startpos = 0;
            next = _next;

            // Marshal.SizeOf(typeof(byte)) * 1
            data = (byte*)Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte))
                + (int)packetlen);
        }

        public static void FreeDataQueueList(DataQueuePacket packet)
        {
            while (packet != null)
            {
                DataQueuePacket next = packet.next;
                Marshal.FreeHGlobal((IntPtr)packet.data);
                packet = next;
            }
        }
    }

    class DataQueue
    {
        internal DataQueuePacket head;// device fed from here.
        internal DataQueuePacket tail;// queue fills to here.
        internal DataQueuePacket pool;// these are unused packets.
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
        DataQueue _queue;

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

            _queue = new DataQueue(packetlen);

            for (int i = 0; i < wantpackets; i++)
            {
                _queue.pool = new DataQueuePacket(_queue.pool, packetlen);
            }

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

            return QueueAudio(data, length);
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

        private int QueueAudio(byte* data, uint length)
        {
            if (length > 0)
            {
                lock (_lock)
                {
                    return WriteToDataQueue(data, length);
                }
            }

            return 0;
        }

        private int WriteToDataQueue(byte* data, uint length)
        {
            DataQueuePacket orighead = _queue.head;
            DataQueuePacket origtail = _queue.tail;
            uint origlen = origtail != null ? origtail.datalen : 0;
            uint datalen;

            while (length > 0)
            {
                DataQueuePacket packet = _queue.tail;
                if (packet == null || packet.datalen >= _queue.packet_size)
                {
                    // tail packet missing or completely full; we need a new packet.
                    packet = AllocateDataQueuePacket();
                    if (packet == null)
                    {
                        if (origtail == null)
                        {
                            packet = _queue.head;// whole queue.
                        }
                        else
                        {
                            packet = origtail.next;// what we added to existing queue.
                            origtail.next = null;
                            origtail.datalen = origlen;
                        }
                        _queue.head = orighead;
                        _queue.tail = origtail;
                        _queue.pool = null;

                        DataQueuePacket.FreeDataQueueList(packet);// give back what we can.
                        return new OutOfMemoryException().HResult;
                    }
                }

                datalen = Math.Min(length, _queue.packet_size - packet.datalen);
                CopyMemory(packet.data + packet.datalen, data, datalen);
                data += datalen;
                length -= datalen;
                packet.datalen += datalen;
                _queue.queued_bytes += datalen;
            }

            return 0;
        }

        private DataQueuePacket AllocateDataQueuePacket()
        {
            DataQueuePacket packet = _queue.pool;

            if (packet != null)
            {
                // we have one available in the pool.
                _queue.pool = packet.next;
            }
            else
            {
                // Have to allocate a new one!
                packet = new DataQueuePacket(null, _queue.packet_size);
            }

            if (_queue.tail == null)
            {
                _queue.head = packet;
            }
            else
            {
                _queue.tail.next = packet;
            }
            _queue.tail = packet;
            return packet;
        }

        private void BufferQueueDrainCallback(byte* stream, int len)
        {

        }

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl
            , SetLastError = false), SuppressUnmanagedCodeSecurity]
        public static unsafe extern void* CopyMemory(void* dest, void* src, ulong count);
    }
}

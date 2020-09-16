using Org.BouncyCastle.Security;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if WIN32
using size_t = System.UInt32;
#else
using size_t = System.UInt64;
#endif

namespace Xstream
{
    unsafe class DataQueuePacket : IDisposable
    {
        const int SDL_VARIABLE_LENGTH_ARRAY = 1;

        [StructLayout(LayoutKind.Sequential)]
        struct SDL_DataQueuePacket
        {
            public size_t datalen;
            public size_t startpos;
            public SDL_DataQueuePacket* next;
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = SDL_VARIABLE_LENGTH_ARRAY)]
            //public byte[] data;
            public fixed byte data[SDL_VARIABLE_LENGTH_ARRAY];
        }

        internal OutOfMemoryException err;

        internal size_t datalen;// bytes currently in use in this packet.
        internal size_t startpos;// bytes currently consumed in this packet.
        internal DataQueuePacket next;// next item in linked list.
        // Uint8 data[SDL_VARIABLE_LENGTH_ARRAY];
        internal byte* data;// packet data

        ~DataQueuePacket()
        {
            Dispose();
        }

        public DataQueuePacket(int packetlen)
        {
            try
            {
                SDL_DataQueuePacket p;// 栈是从高地址到底地址存储数据
                packetlen += Marshal.SizeOf<SDL_DataQueuePacket>() - (int)(
                    (size_t)(&p.datalen) - (size_t)(&p.data));

                data = (byte*)Marshal.AllocHGlobal(packetlen);
            }
            catch (OutOfMemoryException e)
            {
                data = null;
                err = e;
            }
        }

        public void Dispose()
        {
            if (data != null)
            {
                Marshal.FreeHGlobal((IntPtr)data);
                data = null;// 预防野指针
                GC.SuppressFinalize(this);
            }
        }

        public static DataQueue NewDataQueue(size_t _packetlen, size_t initialslack)
        {
            DataQueue queue = new DataQueue();

            size_t packetlen = _packetlen > 0 ? _packetlen : 1024;
            size_t wantpackets = (initialslack + (packetlen - 1)) / packetlen;

            queue.packet_size = packetlen;

            for (size_t i = 0; i < wantpackets; i++)
            {
                DataQueuePacket packet = new DataQueuePacket((int)packetlen);

                // don't care if this fails, we'll deal later.
                if (packet.data != null)
                {
                    packet.datalen = 0;
                    packet.startpos = 0;
                    packet.next = queue.pool;
                    queue.pool = packet;
                }
            }

            return queue;
        }

        public static int WriteToDataQueue(DataQueue queue, void* _data, size_t _len)
        {
            /*
             * 有些变量看似是多余的，这里参考以下几点，请慎重考虑后再作修改！
             * 
             * 1.访问静态变量和实例变量将会比访问局部变量多耗费2-3个时钟周期。
             * 2.修改参数通常会使代码的可读性更差，并且可能需要更多的时间来了解函数中实际发生的事情。
             * 因此，通常不建议修改参数。将此优化留给编译器。
             */
            size_t len = _len;
            byte* data = (byte*)_data;
            size_t packet_size = queue != null ? queue.packet_size : 0;
            DataQueuePacket orighead;
            DataQueuePacket origtail;
            size_t origlen;
            size_t datalen;

            if (queue == null)
            {
                // 连queue都没了，错误没地方存，根本无法返回HResult，还不如直接throw
                throw new InvalidParameterException("queue");
            }

            orighead = queue.head;
            origtail = queue.tail;
            origlen = origtail != null ? origtail.datalen : 0;

            while (len > 0)
            {
                DataQueuePacket packet = queue.tail;
                Debug.Assert(packet == null || packet.datalen <= queue.packet_size);
                if (packet == null || packet.datalen >= queue.packet_size)
                {
                    // tail packet missing or completely full; we need a new packet.
                    packet = AllocateDataQueuePacket(queue);
                    if (packet.data == null)
                    {
                        OutOfMemoryException err = packet.err;

                        // uhoh, reset so we've queued nothing new, free what we can.
                        if (origtail == null)
                        {
                            packet = queue.head;// whole queue.
                        }
                        else
                        {
                            packet = origtail.next;// what we added to existing queue.
                            origtail.next = null;
                            origtail.datalen = origlen;
                        }
                        queue.head = orighead;
                        queue.tail = origtail;
                        queue.pool = null;

                        FreeDataQueueList(packet);// give back what we can.
                        return err.HResult;
                    }
                }

                datalen = Math.Min(len, packet_size - packet.datalen);
                Program.CopyMemory(packet.data + packet.datalen, data, datalen);
                data += datalen;
                len -= datalen;
                packet.datalen += datalen;
                queue.queued_bytes += datalen;
            }

            return 0;
        }

        private static DataQueuePacket AllocateDataQueuePacket(DataQueue queue)
        {
            DataQueuePacket packet;

            Debug.Assert(queue != null);

            packet = queue.pool;
            if (packet != null)
            {
                // we have one available in the pool.
                queue.pool = packet.next;
            }
            else
            {
                // Have to allocate a new one!
                packet = new DataQueuePacket((int)queue.packet_size);
                if (packet.data == null)
                    return null;
            }

            packet.datalen = 0;
            packet.startpos = 0;
            packet.next = null;

            Debug.Assert((queue.head != null) == (queue.queued_bytes != 0));
            if (queue.tail == null)
            {
                queue.head = packet;
            }
            else
            {
                queue.tail.next = packet;
            }
            queue.tail = packet;
            return packet;
        }

        public static void FreeDataQueueList(DataQueuePacket packet)
        {
            while (packet != null)
            {
                packet.Dispose();
                packet = packet.next;
            }
        }

        public static size_t ReadFromDataQueue(DataQueue queue, void* _buf, size_t _len)
        {
            size_t len = _len;
            byte* buf = (byte*)_buf;
            byte* ptr = buf;
            DataQueuePacket packet;

            if (queue == null)
            {
                return 0;
            }

            while (len > 0 && (packet = queue.head) != null)
            {
                size_t avail = packet.datalen - packet.startpos;
                size_t cpy = Math.Min(len, avail);

                Program.CopyMemory(ptr, packet.data + packet.startpos, cpy);
                packet.startpos += cpy;
                ptr += cpy;
                queue.queued_bytes -= cpy;
                len -= cpy;

                if (packet.startpos == packet.datalen)// packet is done, put it in the pool.
                {
                    queue.head = packet.next;
                    Debug.Assert(packet.next != null || packet == queue.tail);
                    packet.next = queue.pool;
                    queue.pool = packet;
                }
            }

            Debug.Assert((queue.head != null) == (queue.queued_bytes != 0));

            if (queue.head == null)
            {
                queue.tail = null;// in case we drained the queue entirely.
            }

            return (size_t)(ptr - buf);
        }

        public static void FreeDataQueue(DataQueue queue)
        {
            if (queue != null)
            {
                FreeDataQueueList(queue.head);
                FreeDataQueueList(queue.pool);
            }
        }

        public static void ClearDataQueue(DataQueue queue, size_t slack)
        {
            size_t packet_size = queue != null ? queue.packet_size : 1;
            size_t slackpackets = (slack + (packet_size - 1)) / packet_size;
            DataQueuePacket packet;
            DataQueuePacket prev = null;

            if (queue == null)
            {
                return;
            }

            packet = queue.head;

            // merge the available pool and the current queue into one list.
            if (packet != null)
            {
                queue.tail.next = queue.pool;
            }
            else
            {
                packet = queue.pool;
            }

            // Remove the queued packets from the device.
            queue.tail = null;
            queue.head = null;
            queue.queued_bytes = 0;
            queue.pool = packet;

            // Optionally keep some slack in the pool to reduce malloc pressure.
            for (size_t i = 0; packet != null && i < slackpackets; i++)
            {
                prev = packet;
                packet = packet.next;
            }

            if (prev != null)
            {
                prev.next = null;
            }
            else
            {
                queue.pool = null;
            }

            FreeDataQueueList(packet);// free extra packets
        }

        public static size_t CountDataQueue(DataQueue queue)
        {
            return queue != null ? queue.queued_bytes : 0;
        }
    }

    class DataQueue
    {
        internal DataQueuePacket head;// device fed from here.
        internal DataQueuePacket tail;// queue fills to here.
        internal DataQueuePacket pool;// these are unused packets.
        internal size_t packet_size;// size of new packets
        internal size_t queued_bytes;// number of bytes of data in the queue.
    }
}

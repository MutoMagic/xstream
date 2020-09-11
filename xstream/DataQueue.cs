using System;
using System.Runtime.InteropServices;

namespace Xstream
{
    unsafe class DataQueuePacket : IDisposable
    {
        internal OutOfMemoryException err;

        internal uint datalen;// bytes currently in use in this packet.
        internal uint startpos;// bytes currently consumed in this packet.
        internal DataQueuePacket next;// next item in linked list.
        // #define SDL_VARIABLE_LENGTH_ARRAY 1
        // Uint8 data[SDL_VARIABLE_LENGTH_ARRAY];
        internal byte* data;// packet data

        ~DataQueuePacket()
        {
            Dispose();
        }

        public DataQueuePacket(uint packetlen)
        {
            try
            {
                data = (byte*)Marshal.AllocHGlobal(Marshal.SizeOf<byte>() + (int)packetlen);
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
            }
        }

        public static DataQueue NewDataQueue(uint _packetlen, uint initialslack)
        {
            DataQueue queue = new DataQueue();

            uint packetlen = _packetlen > 0 ? _packetlen : 1024;
            uint wantpackets = (initialslack + (packetlen - 1)) / packetlen;

            queue.packet_size = packetlen;

            for (uint i = 0; i < wantpackets; i++)
            {
                DataQueuePacket packet = new DataQueuePacket(packetlen);

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

        public static int WriteToDataQueue(DataQueue queue, byte* data, uint length)
        {
            DataQueuePacket orighead = queue.head;
            DataQueuePacket origtail = queue.tail;
            uint origlen = origtail != null ? origtail.datalen : 0;
            uint datalen;

            while (length > 0)
            {
                DataQueuePacket packet = queue.tail;
                if (packet == null || packet.datalen >= queue.packet_size)
                {
                    // tail packet missing or completely full; we need a new packet.
                    packet = AllocateDataQueuePacket(queue);
                    if (packet.data == null)
                    {
                        OutOfMemoryException err = packet.err;

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

                datalen = Math.Min(length, queue.packet_size - packet.datalen);
                Program.CopyMemory(packet.data + packet.datalen, data, datalen);
                data += datalen;
                length -= datalen;
                packet.datalen += datalen;
                queue.queued_bytes += datalen;
            }

            return 0;
        }

        private static DataQueuePacket AllocateDataQueuePacket(DataQueue queue)
        {
            DataQueuePacket packet = queue.pool;

            if (packet != null)
            {
                // we have one available in the pool.
                queue.pool = packet.next;
            }
            else
            {
                // Have to allocate a new one!
                packet = new DataQueuePacket(queue.packet_size);
                if (packet.data == null)
                    return null;
            }

            packet.datalen = 0;
            packet.startpos = 0;
            packet.next = null;

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

        public static uint ReadFromDataQueue(DataQueue queue, byte* buf, uint len)
        {
            if (queue == null)
                return 0;

            DataQueuePacket packet = queue.head;
            byte* ptr = buf;

            while (len > 0 && packet != null)
            {
                uint avail = packet.datalen - packet.startpos;
                uint cpy = Math.Min(len, avail);

                Program.CopyMemory(ptr, packet.data + packet.startpos, cpy);
                packet.startpos += cpy;
                ptr += cpy;
                queue.queued_bytes -= cpy;
                len -= cpy;

                if (packet.startpos == packet.datalen)
                {
                    queue.head = packet.next;
                    packet.next = queue.pool;
                    queue.pool = packet;
                }
            }

            if (queue.head == null)
            {
                queue.tail = null;// in case we drained the queue entirely.
            }

            return (uint)(ptr - buf);
        }
    }

    class DataQueue
    {
        internal DataQueuePacket head;// device fed from here.
        internal DataQueuePacket tail;// queue fills to here.
        internal DataQueuePacket pool;// these are unused packets.
        internal uint packet_size;// size of new packets
        internal uint queued_bytes;// number of bytes of data in the queue.
    }
}

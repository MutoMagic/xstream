using System;
using System.Runtime.InteropServices;

namespace Xstream
{
    unsafe class DataQueuePacket : IDisposable
    {
        internal uint datalen;// bytes currently in use in this packet.
        internal uint startpos;// bytes currently consumed in this packet.
        internal DataQueuePacket next;// next item in linked list.
        // #define SDL_VARIABLE_LENGTH_ARRAY 1
        // Uint8 data[SDL_VARIABLE_LENGTH_ARRAY];
        internal byte* data;// packet data
        internal OutOfMemoryException err;

        ~DataQueuePacket()
        {
            Dispose();
        }

        public DataQueuePacket(int packetlen)
        {
            try
            {
                // Marshal.SizeOf(typeof(byte)) * 1
                int size = Marshal.SizeOf(typeof(byte)) + packetlen;

                data = (byte*)Marshal.AllocHGlobal(size);
                Program.ZeroMemory(data, (uint)size);
            }
            catch (OutOfMemoryException e)
            {
                data = null;
                err = e;
            }
        }

        public DataQueuePacket(uint packetlen) : this((int)packetlen) { }

        public void Dispose()
        {
            if (data != null)
            {
                Marshal.FreeHGlobal((IntPtr)data);
                data = null;// 预防野指针
            }
        }

        public static void FreeDataQueueList(DataQueuePacket packet)
        {
            while (packet != null)
            {
                packet.Dispose();
                packet = packet.next;
            }
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

            //packet.datalen = 0;
            //packet.startpos = 0;
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

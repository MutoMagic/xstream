using System;
using System.Runtime.InteropServices;

namespace Xstream
{
    partial class Native
    {
        public static int CompareMemory<T1, T2>(T1 obj1, T2 obj2, int count)
        {
            /*
             * UIntPtr与IntPtr在指针的实现上都一样，似乎没有什么区别。
             * 需要注意的是：IntPtr与CLS兼容，因为CLR之上有一些语言不支持unsigned。
             * 请统一使用IntPtr（重要的事情说三遍！！！
             */
            IntPtr pCount = new IntPtr(count);

            //GCHandle h1 = GCHandle.Alloc(obj1, GCHandleType.Pinned);
            //GCHandle h2 = GCHandle.Alloc(obj2, GCHandleType.Pinned);
            IntPtr p1 = Marshal.AllocHGlobal(count);
            IntPtr p2 = Marshal.AllocHGlobal(count);
            Marshal.StructureToPtr(obj1, p1, true);
            Marshal.StructureToPtr(obj2, p2, true);
            try
            {
                //return CompareMemory(h1.AddrOfPinnedObject(), h2.AddrOfPinnedObject(), pCount);
                return CompareMemory(p1, p2, pCount);
            }
            finally
            {
                //h1.Free();
                //h2.Free();
                Marshal.FreeHGlobal(p1);
                Marshal.FreeHGlobal(p2);
            }
        }

        [DllImport("msvcrt.dll", EntryPoint = "memset")]
        public static extern IntPtr SetMemory(IntPtr dest, int ch, IntPtr count);

        [DllImport("msvcrt.dll", EntryPoint = "memcpy")]
        public static extern IntPtr CopyMemory(IntPtr dest, IntPtr src, IntPtr count);

        [DllImport("msvcrt.dll", EntryPoint = "memcmp")]
        static extern int CompareMemory(IntPtr lhs, IntPtr rhs, IntPtr count);
    }
}

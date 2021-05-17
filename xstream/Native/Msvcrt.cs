using System;
using System.Runtime.InteropServices;

namespace Xstream
{
    partial class Native
    {
        [DllImport("msvcrt.dll", EntryPoint = "memset")]
        public static extern IntPtr SetMemory(IntPtr dest, int ch, IntPtr count);

        [DllImport("msvcrt.dll", EntryPoint = "memcpy")]
        public static extern IntPtr CopyMemory(IntPtr dest, IntPtr src, IntPtr count);

        [DllImport("msvcrt.dll", EntryPoint = "memcmp")]
        public static extern int CompareMemory(IntPtr lhs, IntPtr rhs, IntPtr count);
    }
}

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Xstream
{
    partial class Native
    {
        #region FormatMessage()

        public const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        public const uint FORMAT_MESSAGE_FROM_STRING = 0x00000400;
        public const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        public const uint FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        public const uint FORMAT_MESSAGE_MAX_WIDTH_MASK = 0x000000FF;

        #endregion

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        public static extern uint GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            uint nSize,
            string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint FormatMessage(
            uint dwFlags,
            IntPtr lpSource,
            uint dwMessageId,
            uint dwLanguageId,
            IntPtr lpBuffer,
            uint nSize,
            IntPtr Arguments);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory")]
        public static extern void ZeroMemory(IntPtr Destination, IntPtr Length);

        [DllImport("kernel32.dll")]
        public static extern void Sleep(uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();
    }
}

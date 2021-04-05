using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Xstream
{
    partial class Native
    {
        const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        //const uint FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        //const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        //const uint FORMAT_MESSAGE_FROM_STRING = 0x00000400;
        const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;

        public static void Sleep(int ms)
        {
            uint dwMilliseconds = (uint)ms;

            uint max_delay = 0xFFFFFFFF / 1000U;
            if (dwMilliseconds > max_delay)
                dwMilliseconds = max_delay;
            Sleep(dwMilliseconds);// Thread.Sleep(millisecondsTimeout < 0) 会报错
        }

        public static string GetLastError()
        {
            IntPtr lpMsgBuf = IntPtr.Zero;
            uint len = FormatMessage(
                FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS
                , IntPtr.Zero
                , (uint)Marshal.GetLastWin32Error()
                , 0
                , lpMsgBuf
                , 0
                , IntPtr.Zero);
            if (len == 0)
            {
                throw new SystemException($"win32 FormatMessage err: {Marshal.GetLastWin32Error()}");
            }

            string sRet = Marshal.PtrToStringAnsi(lpMsgBuf);
            lpMsgBuf = LocalFree(lpMsgBuf);
            if (lpMsgBuf != IntPtr.Zero)
            {
                throw new SystemException($"win32 LocalFree err: {GetLastError()}");
            }
            return sRet;
        }

        public static string GetPrivateProfileString(string section, string key, string def, string filePath)
        {
            StringBuilder sb = new StringBuilder(0xff);
            GetPrivateProfileString(section, key, def, sb, (uint)sb.Capacity, filePath);
            sb.Delete(';');
            sb.Delete('#');
            return sb.ToString();
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern uint GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            uint nSize,
            string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint FormatMessage(
            uint dwFlags,
            IntPtr lpSource,
            uint dwMessageId,
            uint dwLanguageId,
            IntPtr lpBuffer,
            uint nSize,
            IntPtr Arguments);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory")]
        public static extern void ZeroMemory(IntPtr Destination, IntPtr Length);

        [DllImport("kernel32.dll")]
        static extern void Sleep(uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();
    }

    static class StringBuilder_Extensions
    {
        public static int IndexOf(this StringBuilder sb, char value)
        {
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == value)
                {
                    return i;
                }
            }

            return -1;
        }

        public static void Delete(this StringBuilder sb, int startIndex)
        {
            if (startIndex != -1)
            {
                sb.Length = startIndex;
            }
        }

        public static void Delete(this StringBuilder sb, char startIndex)
            => sb.Delete(sb.IndexOf(startIndex));
    }
}

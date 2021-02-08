using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

#if WIN32
using size_t = System.UInt32;
#else
using size_t = System.UInt64;
#endif

namespace Xstream
{
    public static class Native
    {
        const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        //const uint FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        //const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        //const uint FORMAT_MESSAGE_FROM_STRING = 0x00000400;
        const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;

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

        public static void Delete(this StringBuilder sb, char value) => sb.Delete(sb.IndexOf(value));

        public static string GetPrivateProfileString(string section, string key, string def, string filePath)
        {
            StringBuilder sb = new StringBuilder(0xff);
            GetPrivateProfileString(section, key, def, sb, (uint)sb.Capacity, filePath);
            sb.Delete(';');
            sb.Delete('#');
            return sb.ToString();
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
            if (lpMsgBuf != null)
            {
                throw new SystemException($"win32 LocalFree err: {GetLastError()}");
            }
            return sRet;
        }

        public static void Delay(int ms) => Delay((uint)ms);

        public static void Delay(uint ms)
        {
            uint max_delay = 0xFFFFFFFF / 1000U;
            if (ms > max_delay)
                ms = max_delay;
            Sleep(ms);// Thread.Sleep(millisecondsTimeout < 0) 会报错
        }

        [DllImport("kernel32")]
        public static extern bool AllocConsole();
        [DllImport("kernel32")]
        public static extern bool FreeConsole();

        [DllImport("kernel32")]
        static extern uint GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            uint nSize,
            string lpFileName);

        [DllImport("kernel32.dll")]
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
        public static unsafe extern void ZeroMemory(void* Destination, size_t Length);
        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void* SetMemory(void* dest, int c, size_t byteCount);
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void* CopyMemory(void* dest, void* src, size_t count);

        [DllImport("kernel32")]
        static extern void Sleep(uint dwMilliseconds);
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
        [DllImport("user32.dll")]
        public extern static bool DestroyIcon(IntPtr handle);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessage(
            out NativeMessage lpMsg,
            size_t hWnd,
            uint wMsgFilterMin,
            uint wMsgFilterMax,
            uint wRemoveMsg);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostThreadMessage(uint threadId, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
            //public uint lPrivate;
        }
    }
}

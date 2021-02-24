using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

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
            if (lpMsgBuf != IntPtr.Zero)
            {
                throw new SystemException($"win32 LocalFree err: {GetLastError()}");
            }
            return sRet;
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

        #region 内存

        public static int memcmp<T1, T2>(T1 obj1, T2 obj2, int count)
        {
            GCHandle h1 = GCHandle.Alloc(obj1);
            GCHandle h2 = GCHandle.Alloc(obj2);

            try
            {
                return memcmp(h1.AddrOfPinnedObject(), h2.AddrOfPinnedObject(), (size_t)count);
            }
            finally
            {
                h1.Free();
                h2.Free();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory")]
        public static unsafe extern void ZeroMemory(void* Destination, size_t Length);

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void* SetMemory(void* dest, int c, size_t byteCount);

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void* CopyMemory(void* dest, void* src, size_t count);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GlobalSize(IntPtr handle);

        [DllImport("msvcrt.dll")]
        static extern int memcmp(IntPtr ptr1, IntPtr ptr2, size_t count);

        #endregion

        #region 线程

        public static void Delay(int ms) => Delay((uint)ms);

        public static void Delay(uint ms)
        {
            uint max_delay = 0xFFFFFFFF / 1000U;
            if (ms > max_delay)
                ms = max_delay;
            Sleep(ms);// Thread.Sleep(millisecondsTimeout < 0) 会报错
        }

        [DllImport("kernel32")]
        static extern void Sleep(uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        #endregion

        #region GUI

        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int ENUM_REGISTRY_SETTINGS = -2;

        public const int CCHDEVICENAME = 32;
        public const int CCHFORMNAME = 32;

        public const int DM_ORIENTATION = 0x00000001;
        public const int DM_PAPERSIZE = 0x00000002;
        public const int DM_PAPERLENGTH = 0x00000004;
        public const int DM_PAPERWIDTH = 0x00000008;
        public const int DM_SCALE = 0x00000010;
        public const int DM_POSITION = 0x00000020;
        public const int DM_NUP = 0x00000040;
        public const int DM_DISPLAYORIENTATION = 0x00000080;
        public const int DM_COPIES = 0x00000100;
        public const int DM_DEFAULTSOURCE = 0x00000200;
        public const int DM_PRINTQUALITY = 0x00000400;
        public const int DM_COLOR = 0x00000800;
        public const int DM_DUPLEX = 0x00001000;
        public const int DM_YRESOLUTION = 0x00002000;
        public const int DM_TTOPTION = 0x00004000;
        public const int DM_COLLATE = 0x00008000;
        public const int DM_FORMNAME = 0x00010000;
        public const int DM_LOGPIXELS = 0x00020000;
        public const int DM_BITSPERPEL = 0x00040000;
        public const int DM_PELSWIDTH = 0x00080000;
        public const int DM_PELSHEIGHT = 0x00100000;
        public const int DM_DISPLAYFLAGS = 0x00200000;
        public const int DM_DISPLAYFREQUENCY = 0x00400000;
        public const int DM_ICMMETHOD = 0x00800000;
        public const int DM_ICMINTENT = 0x01000000;
        public const int DM_MEDIATYPE = 0x02000000;
        public const int DM_DITHERTYPE = 0x04000000;
        public const int DM_PANNINGWIDTH = 0x08000000;
        public const int DM_PANNINGHEIGHT = 0x10000000;
        public const int DM_DISPLAYFIXEDOUTPUT = 0x20000000;

        public const int PM_NOREMOVE = 0x0000;
        public const int PM_REMOVE = 0x0001;
        public const int PM_NOYIELD = 0x0002;

        #region Window Messages

        public const uint WM_ACTIVATE = 0x0006;
        public const uint WM_ACTIVATEAPP = 0x001C;
        public const uint WM_AFXFIRST = 0x0360;
        public const uint WM_AFXLAST = 0x037F;
        public const uint WM_APP = 0x8000;
        public const uint WM_ASKCBFORMATNAME = 0x030C;
        public const uint WM_CANCELJOURNAL = 0x004B;
        public const uint WM_CANCELMODE = 0x001F;
        public const uint WM_CAPTURECHANGED = 0x0215;
        public const uint WM_CHANGECBCHAIN = 0x030D;
        public const uint WM_CHANGEUISTATE = 0x0127;
        public const uint WM_CHAR = 0x0102;
        public const uint WM_CHARTOITEM = 0x002F;
        public const uint WM_CHILDACTIVATE = 0x0022;
        public const uint WM_CLEAR = 0x0303;
        public const uint WM_CLOSE = 0x0010;
        public const uint WM_COMMAND = 0x0111;
        public const uint WM_COMPACTING = 0x0041;
        public const uint WM_COMPAREITEM = 0x0039;
        public const uint WM_CONTEXTMENU = 0x007B;
        public const uint WM_COPY = 0x0301;
        public const uint WM_COPYDATA = 0x004A;
        public const uint WM_CREATE = 0x0001;
        public const uint WM_CTLCOLORBTN = 0x0135;
        public const uint WM_CTLCOLORDLG = 0x0136;
        public const uint WM_CTLCOLOREDIT = 0x0133;
        public const uint WM_CTLCOLORLISTBOX = 0x0134;
        public const uint WM_CTLCOLORMSGBOX = 0x0132;
        public const uint WM_CTLCOLORSCROLLBAR = 0x0137;
        public const uint WM_CTLCOLORSTATIC = 0x0138;
        public const uint WM_CUT = 0x0300;
        public const uint WM_DEADCHAR = 0x0103;
        public const uint WM_DELETEITEM = 0x002D;
        public const uint WM_DESTROY = 0x0002;
        public const uint WM_DESTROYCLIPBOARD = 0x0307;
        public const uint WM_DEVICECHANGE = 0x0219;
        public const uint WM_DEVMODECHANGE = 0x001B;
        public const uint WM_DISPLAYCHANGE = 0x007E;
        public const uint WM_DRAWCLIPBOARD = 0x0308;
        public const uint WM_DRAWITEM = 0x002B;
        public const uint WM_DROPFILES = 0x0233;
        public const uint WM_ENABLE = 0x000A;
        public const uint WM_ENDSESSION = 0x0016;
        public const uint WM_ENTERIDLE = 0x0121;
        public const uint WM_ENTERMENULOOP = 0x0211;
        public const uint WM_ENTERSIZEMOVE = 0x0231;
        public const uint WM_ERASEBKGND = 0x0014;
        public const uint WM_EXITMENULOOP = 0x0212;
        public const uint WM_EXITSIZEMOVE = 0x0232;
        public const uint WM_FONTCHANGE = 0x001D;
        public const uint WM_GETDLGCODE = 0x0087;
        public const uint WM_GETFONT = 0x0031;
        public const uint WM_GETHOTKEY = 0x0033;
        public const uint WM_GETICON = 0x007F;
        public const uint WM_GETMINMAXINFO = 0x0024;
        public const uint WM_GETOBJECT = 0x003D;
        public const uint WM_GETTEXT = 0x000D;
        public const uint WM_GETTEXTLENGTH = 0x000E;
        public const uint WM_HANDHELDFIRST = 0x0358;
        public const uint WM_HANDHELDLAST = 0x035F;
        public const uint WM_HELP = 0x0053;
        public const uint WM_HOTKEY = 0x0312;
        public const uint WM_HSCROLL = 0x0114;
        public const uint WM_HSCROLLCLIPBOARD = 0x030E;
        public const uint WM_ICONERASEBKGND = 0x0027;
        public const uint WM_IME_CHAR = 0x0286;
        public const uint WM_IME_COMPOSITION = 0x010F;
        public const uint WM_IME_COMPOSITIONFULL = 0x0284;
        public const uint WM_IME_CONTROL = 0x0283;
        public const uint WM_IME_ENDCOMPOSITION = 0x010E;
        public const uint WM_IME_KEYDOWN = 0x0290;
        public const uint WM_IME_KEYLAST = 0x010F;
        public const uint WM_IME_KEYUP = 0x0291;
        public const uint WM_IME_NOTIFY = 0x0282;
        public const uint WM_IME_REQUEST = 0x0288;
        public const uint WM_IME_SELECT = 0x0285;
        public const uint WM_IME_SETCONTEXT = 0x0281;
        public const uint WM_IME_STARTCOMPOSITION = 0x010D;
        public const uint WM_INITDIALOG = 0x0110;
        public const uint WM_INITMENU = 0x0116;
        public const uint WM_INITMENUPOPUP = 0x0117;
        public const uint WM_INPUTLANGCHANGE = 0x0051;
        public const uint WM_INPUTLANGCHANGEREQUEST = 0x0050;
        public const uint WM_KEYDOWN = 0x0100;
        public const uint WM_KEYFIRST = 0x0100;
        public const uint WM_KEYLAST = 0x0108;
        public const uint WM_KEYUP = 0x0101;
        public const uint WM_KILLFOCUS = 0x0008;
        public const uint WM_LBUTTONDBLCLK = 0x0203;
        public const uint WM_LBUTTONDOWN = 0x0201;
        public const uint WM_LBUTTONUP = 0x0202;
        public const uint WM_MBUTTONDBLCLK = 0x0209;
        public const uint WM_MBUTTONDOWN = 0x0207;
        public const uint WM_MBUTTONUP = 0x0208;
        public const uint WM_MDIACTIVATE = 0x0222;
        public const uint WM_MDICASCADE = 0x0227;
        public const uint WM_MDICREATE = 0x0220;
        public const uint WM_MDIDESTROY = 0x0221;
        public const uint WM_MDIGETACTIVE = 0x0229;
        public const uint WM_MDIICONARRANGE = 0x0228;
        public const uint WM_MDIMAXIMIZE = 0x0225;
        public const uint WM_MDINEXT = 0x0224;
        public const uint WM_MDIREFRESHMENU = 0x0234;
        public const uint WM_MDIRESTORE = 0x0223;
        public const uint WM_MDISETMENU = 0x0230;
        public const uint WM_MDITILE = 0x0226;
        public const uint WM_MEASUREITEM = 0x002C;
        public const uint WM_MENUCHAR = 0x0120;
        public const uint WM_MENUCOMMAND = 0x0126;
        public const uint WM_MENUDRAG = 0x0123;
        public const uint WM_MENUGETOBJECT = 0x0124;
        public const uint WM_MENURBUTTONUP = 0x0122;
        public const uint WM_MENUSELECT = 0x011F;
        public const uint WM_MOUSEACTIVATE = 0x0021;
        public const uint WM_MOUSEFIRST = 0x0200;
        public const uint WM_MOUSEHOVER = 0x02A1;
        public const uint WM_MOUSELAST = 0x020D;
        public const uint WM_MOUSELEAVE = 0x02A3;
        public const uint WM_MOUSEMOVE = 0x0200;
        public const uint WM_MOUSEWHEEL = 0x020A;
        public const uint WM_MOUSEHWHEEL = 0x020E;
        public const uint WM_MOVE = 0x0003;
        public const uint WM_MOVING = 0x0216;
        public const uint WM_NCACTIVATE = 0x0086;
        public const uint WM_NCCALCSIZE = 0x0083;
        public const uint WM_NCCREATE = 0x0081;
        public const uint WM_NCDESTROY = 0x0082;
        public const uint WM_NCHITTEST = 0x0084;
        public const uint WM_NCLBUTTONDBLCLK = 0x00A3;
        public const uint WM_NCLBUTTONDOWN = 0x00A1;
        public const uint WM_NCLBUTTONUP = 0x00A2;
        public const uint WM_NCMBUTTONDBLCLK = 0x00A9;
        public const uint WM_NCMBUTTONDOWN = 0x00A7;
        public const uint WM_NCMBUTTONUP = 0x00A8;
        public const uint WM_NCMOUSEHOVER = 0x02A0;
        public const uint WM_NCMOUSELEAVE = 0x02A2;
        public const uint WM_NCMOUSEMOVE = 0x00A0;
        public const uint WM_NCPAINT = 0x0085;
        public const uint WM_NCRBUTTONDBLCLK = 0x00A6;
        public const uint WM_NCRBUTTONDOWN = 0x00A4;
        public const uint WM_NCRBUTTONUP = 0x00A5;
        public const uint WM_NCXBUTTONDBLCLK = 0x00AD;
        public const uint WM_NCXBUTTONDOWN = 0x00AB;
        public const uint WM_NCXBUTTONUP = 0x00AC;
        public const uint WM_NCUAHDRAWCAPTION = 0x00AE;
        public const uint WM_NCUAHDRAWFRAME = 0x00AF;
        public const uint WM_NEXTDLGCTL = 0x0028;
        public const uint WM_NEXTMENU = 0x0213;
        public const uint WM_NOTIFY = 0x004E;
        public const uint WM_NOTIFYFORMAT = 0x0055;
        public const uint WM_NULL = 0x0000;
        public const uint WM_PAINT = 0x000F;
        public const uint WM_PAINTCLIPBOARD = 0x0309;
        public const uint WM_PAINTICON = 0x0026;
        public const uint WM_PALETTECHANGED = 0x0311;
        public const uint WM_PALETTEISCHANGING = 0x0310;
        public const uint WM_PARENTNOTIFY = 0x0210;
        public const uint WM_PASTE = 0x0302;
        public const uint WM_PENWINFIRST = 0x0380;
        public const uint WM_PENWINLAST = 0x038F;
        public const uint WM_POWER = 0x0048;
        public const uint WM_POWERBROADCAST = 0x0218;
        public const uint WM_PRINT = 0x0317;
        public const uint WM_PRINTCLIENT = 0x0318;
        public const uint WM_QUERYDRAGICON = 0x0037;
        public const uint WM_QUERYENDSESSION = 0x0011;
        public const uint WM_QUERYNEWPALETTE = 0x030F;
        public const uint WM_QUERYOPEN = 0x0013;
        public const uint WM_QUEUESYNC = 0x0023;
        public const uint WM_QUIT = 0x0012;
        public const uint WM_RBUTTONDBLCLK = 0x0206;
        public const uint WM_RBUTTONDOWN = 0x0204;
        public const uint WM_RBUTTONUP = 0x0205;
        public const uint WM_RENDERALLFORMATS = 0x0306;
        public const uint WM_RENDERFORMAT = 0x0305;
        public const uint WM_SETCURSOR = 0x0020;
        public const uint WM_SETFOCUS = 0x0007;
        public const uint WM_SETFONT = 0x0030;
        public const uint WM_SETHOTKEY = 0x0032;
        public const uint WM_SETICON = 0x0080;
        public const uint WM_SETREDRAW = 0x000B;
        public const uint WM_SETTEXT = 0x000C;
        public const uint WM_SETTINGCHANGE = 0x001A;
        public const uint WM_SHOWWINDOW = 0x0018;
        public const uint WM_SIZE = 0x0005;
        public const uint WM_SIZECLIPBOARD = 0x030B;
        public const uint WM_SIZING = 0x0214;
        public const uint WM_SPOOLERSTATUS = 0x002A;
        public const uint WM_STYLECHANGED = 0x007D;
        public const uint WM_STYLECHANGING = 0x007C;
        public const uint WM_SYNCPAINT = 0x0088;
        public const uint WM_SYSCHAR = 0x0106;
        public const uint WM_SYSCOLORCHANGE = 0x0015;
        public const uint WM_SYSCOMMAND = 0x0112;
        public const uint WM_SYSDEADCHAR = 0x0107;
        public const uint WM_SYSKEYDOWN = 0x0104;
        public const uint WM_SYSKEYUP = 0x0105;
        public const uint WM_TCARD = 0x0052;
        public const uint WM_TIMECHANGE = 0x001E;
        public const uint WM_TIMER = 0x0113;
        public const uint WM_UNDO = 0x0304;
        public const uint WM_UNINITMENUPOPUP = 0x0125;
        public const uint WM_USER = 0x0400;
        public const uint WM_USERCHANGED = 0x0054;
        public const uint WM_VKEYTOITEM = 0x002E;
        public const uint WM_VSCROLL = 0x0115;
        public const uint WM_VSCROLLCLIPBOARD = 0x030A;
        public const uint WM_WINDOWPOSCHANGED = 0x0047;
        public const uint WM_WINDOWPOSCHANGING = 0x0046;
        public const uint WM_WININICHANGE = 0x001A;
        public const uint WM_XBUTTONDBLCLK = 0x020D;
        public const uint WM_XBUTTONDOWN = 0x020B;
        public const uint WM_XBUTTONUP = 0x020C;

        #endregion

        #region Window Styles

        public const long WS_OVERLAPPED = 0x00000000L;
        public const long WS_POPUP = 0x80000000L;
        public const long WS_CHILD = 0x40000000L;
        public const long WS_MINIMIZE = 0x20000000L;
        public const long WS_VISIBLE = 0x10000000L;
        public const long WS_DISABLED = 0x08000000L;
        public const long WS_CLIPSIBLINGS = 0x04000000L;
        public const long WS_CLIPCHILDREN = 0x02000000L;
        public const long WS_MAXIMIZE = 0x01000000L;
        public const long WS_BORDER = 0x00800000L;
        public const long WS_DLGFRAME = 0x00400000L;
        public const long WS_VSCROLL = 0x00200000L;
        public const long WS_HSCROLL = 0x00100000L;
        public const long WS_SYSMENU = 0x00080000L;
        public const long WS_THICKFRAME = 0x00040000L;
        public const long WS_GROUP = 0x00020000L;
        public const long WS_TABSTOP = 0x00010000L;

        public const long WS_MINIMIZEBOX = 0x00020000L;
        public const long WS_MAXIMIZEBOX = 0x00010000L;

        public const long WS_CAPTION = WS_BORDER | WS_DLGFRAME;
        public const long WS_ICONIC = WS_MINIMIZE;
        public const long WS_SIZEBOX = WS_THICKFRAME;

        public const long WS_OVERLAPPEDWINDOW
            = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
        public const long WS_TILED = WS_OVERLAPPED;
        public const long WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW;
        public const long WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU;
        public const long WS_CHILDWINDOW = WS_CHILD;

        public const long STYLE_BASIC = WS_CLIPSIBLINGS | WS_CLIPCHILDREN;
        public const long STYLE_FULLSCREEN = WS_POPUP;
        public const long STYLE_BORDERLESS = WS_POPUP;
        public const long STYLE_NORMAL = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX;
        public const long STYLE_RESIZABLE = WS_THICKFRAME | WS_MAXIMIZEBOX;
        public const long STYLE_MASK = STYLE_FULLSCREEN | STYLE_BORDERLESS | STYLE_NORMAL | STYLE_RESIZABLE;

        #endregion

        public static long GetWindowLongPtr(IntPtr hWnd, GWL nIndex)
            => GetWindowLongPtr(hWnd, (int)nIndex).ToInt64();

        // This static method is required because Win32 does not support GetWindowLongPtr directly
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
            {
                return GetWindowLongPtr64(hWnd, nIndex);
            }
            return GetWindowLongPtr32(hWnd, nIndex);
        }

        public static long SetWindowLongPtr(HandleRef hWnd, GWL nIndex, long dwNewLong)
            => SetWindowLongPtr(hWnd, (int)nIndex, new IntPtr(dwNewLong)).ToInt64();

        // This helper static method is required because the 32-bit version of user32.dll does not contain this API
        // (on any versions of Windows), so linking the method will fail at run-time. The bridge dispatches the request
        // to the correct function (GetWindowLong in 32-bit mode and GetWindowLongPtr in 64-bit mode)
        public static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            return new IntPtr(SetWindowLong32(hWnd
                , nIndex
                , dwNewLong.ToInt32()
                ));
        }

        public static bool SetWindowPos(
            IntPtr hWnd,
            SpecialWindowHandles hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            SetWindowPosFlags uFlags)
            => SetWindowPos(hWnd, (IntPtr)hWndInsertAfter, X, Y, cx, cy, uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumDisplayDevices(
            string lpDevice,
            uint iDevNum,
            ref DISPLAY_DEVICE lpDisplayDevice,
            uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        /// <summary>Deletes the specified device context (DC).</summary>
        /// <param name="hdc">A handle to the device context.</param>
        /// <returns><para>If the function succeeds, the return value is nonzero.</para><para>If the function fails, the return value is zero.</para></returns>
        /// <remarks>An application must not delete a DC whose handle was obtained by calling the <c>GetDC</c> function. Instead, it must call the <c>ReleaseDC</c> function to free the DC.</remarks>
        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC([In] IntPtr hdc);

        /// <summary>
        ///        Creates a bitmap compatible with the device that is associated with the specified device context.
        /// </summary>
        /// <param name="hdc">A handle to a device context.</param>
        /// <param name="nWidth">The bitmap width, in pixels.</param>
        /// <param name="nHeight">The bitmap height, in pixels.</param>
        /// <returns>If the function succeeds, the return value is a handle to the compatible bitmap (DDB). If the function fails, the return value is <see cref="System.IntPtr.Zero"/>.</returns>
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap([In] IntPtr hdc, int nWidth, int nHeight);

        /// <summary>Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system resources associated with the object. After the object is deleted, the specified handle is no longer valid.</summary>
        /// <param name="hObject">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
        /// <returns>
        ///   <para>If the function succeeds, the return value is nonzero.</para>
        ///   <para>If the specified handle is not valid or is currently selected into a DC, the return value is zero.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Do not delete a drawing object (pen or brush) while it is still selected into a DC.</para>
        ///   <para>When a pattern brush is deleted, the bitmap associated with the brush is not deleted. The bitmap must be deleted independently.</para>
        /// </remarks>
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        /// <summary>
        ///        Retrieves the bits of the specified compatible bitmap and copies them into a buffer as a DIB using the specified format.
        /// </summary>
        /// <param name="hdc">A handle to the device context.</param>
        /// <param name="hbmp">A handle to the bitmap. This must be a compatible bitmap (DDB).</param>
        /// <param name="uStartScan">The first scan line to retrieve.</param>
        /// <param name="cScanLines">The number of scan lines to retrieve.</param>
        /// <param name="lpvBits">A pointer to a buffer to receive the bitmap data. If this parameter is <see cref="IntPtr.Zero"/>, the function passes the dimensions and format of the bitmap to the <see cref="BITMAPINFO"/> structure pointed to by the <paramref name="lpbi"/> parameter.</param>
        /// <param name="lpbi">A pointer to a <see cref="BITMAPINFO"/> structure that specifies the desired format for the DIB data.</param>
        /// <param name="uUsage">The format of the bmiColors member of the <see cref="BITMAPINFO"/> structure. It must be one of the following values.</param>
        /// <returns>If the lpvBits parameter is non-NULL and the function succeeds, the return value is the number of scan lines copied from the bitmap.
        /// If the lpvBits parameter is NULL and GetDIBits successfully fills the <see cref="BITMAPINFO"/> structure, the return value is nonzero.
        /// If the function fails, the return value is zero.
        /// This function can return the following value: ERROR_INVALID_PARAMETER (87 (0×57))</returns>
        [DllImport("gdi32.dll", EntryPoint = "GetDIBits")]
        public static extern int GetDIBits(
            [In] IntPtr hdc,
            [In] IntPtr hbmp,
            uint uStartScan,
            uint cScanLines,
            [Out] byte[] lpvBits,
            ref BITMAPINFO lpbi,
            DIB_Color_Mode uUsage);

        [DllImport("user32.dll")]
        public extern static bool DestroyIcon(IntPtr handle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessage(
            out MSG lpMsg,
            size_t hWnd,
            uint wMsgFilterMin,
            uint wMsgFilterMax,
            uint wRemoveMsg);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostThreadMessage(uint threadId, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool AdjustWindowRectEx(ref RECT lpRect, uint dwStyle, bool bMenu, uint dwExStyle);

        [DllImport("user32.dll")]
        public static extern IntPtr GetMenu(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        ///     Changes the size, position, and Z order of a child, pop-up, or top-level window. These windows are ordered
        ///     according to their appearance on the screen. The topmost window receives the highest rank and is the first window
        ///     in the Z order.
        ///     <para>See https://msdn.microsoft.com/en-us/library/windows/desktop/ms633545%28v=vs.85%29.aspx for more information.</para>
        /// </summary>
        /// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br />A handle to the window.</param>
        /// <param name="hWndInsertAfter">
        ///     C++ ( hWndInsertAfter [in, optional]. Type: HWND )<br />A handle to the window to precede the positioned window in
        ///     the Z order. This parameter must be a window handle or one of the following values.
        ///     <list type="table">
        ///     <itemheader>
        ///         <term>HWND placement</term><description>Window to precede placement</description>
        ///     </itemheader>
        ///     <item>
        ///         <term>HWND_BOTTOM ((HWND)1)</term>
        ///         <description>
        ///         Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost
        ///         window, the window loses its topmost status and is placed at the bottom of all other windows.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>HWND_NOTOPMOST ((HWND)-2)</term>
        ///         <description>
        ///         Places the window above all non-topmost windows (that is, behind all topmost windows). This
        ///         flag has no effect if the window is already a non-topmost window.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>HWND_TOP ((HWND)0)</term><description>Places the window at the top of the Z order.</description>
        ///     </item>
        ///     <item>
        ///         <term>HWND_TOPMOST ((HWND)-1)</term>
        ///         <description>
        ///         Places the window above all non-topmost windows. The window maintains its topmost position
        ///         even when it is deactivated.
        ///         </description>
        ///     </item>
        ///     </list>
        ///     <para>For more information about how this parameter is used, see the following Remarks section.</para>
        /// </param>
        /// <param name="X">C++ ( X [in]. Type: int )<br />The new position of the left side of the window, in client coordinates.</param>
        /// <param name="Y">C++ ( Y [in]. Type: int )<br />The new position of the top of the window, in client coordinates.</param>
        /// <param name="cx">C++ ( cx [in]. Type: int )<br />The new width of the window, in pixels.</param>
        /// <param name="cy">C++ ( cy [in]. Type: int )<br />The new height of the window, in pixels.</param>
        /// <param name="uFlags">
        ///     C++ ( uFlags [in]. Type: UINT )<br />The window sizing and positioning flags. This parameter can be a combination
        ///     of the following values.
        ///     <list type="table">
        ///     <itemheader>
        ///         <term>HWND sizing and positioning flags</term>
        ///         <description>Where to place and size window. Can be a combination of any</description>
        ///     </itemheader>
        ///     <item>
        ///         <term>SWP_ASYNCWINDOWPOS (0x4000)</term>
        ///         <description>
        ///         If the calling thread and the thread that owns the window are attached to different input
        ///         queues, the system posts the request to the thread that owns the window. This prevents the calling
        ///         thread from blocking its execution while other threads process the request.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_DEFERERASE (0x2000)</term>
        ///         <description>Prevents generation of the WM_SYNCPAINT message. </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_DRAWFRAME (0x0020)</term>
        ///         <description>Draws a frame (defined in the window's class description) around the window.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_FRAMECHANGED (0x0020)</term>
        ///         <description>
        ///         Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message
        ///         to the window, even if the window's size is not being changed. If this flag is not specified,
        ///         WM_NCCALCSIZE is sent only when the window's size is being changed
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_HIDEWINDOW (0x0080)</term><description>Hides the window.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOACTIVATE (0x0010)</term>
        ///         <description>
        ///         Does not activate the window. If this flag is not set, the window is activated and moved to
        ///         the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter
        ///         parameter).
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOCOPYBITS (0x0100)</term>
        ///         <description>
        ///         Discards the entire contents of the client area. If this flag is not specified, the valid
        ///         contents of the client area are saved and copied back into the client area after the window is sized or
        ///         repositioned.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOMOVE (0x0002)</term>
        ///         <description>Retains the current position (ignores X and Y parameters).</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOOWNERZORDER (0x0200)</term>
        ///         <description>Does not change the owner window's position in the Z order.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOREDRAW (0x0008)</term>
        ///         <description>
        ///         Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies
        ///         to the client area, the nonclient area (including the title bar and scroll bars), and any part of the
        ///         parent window uncovered as a result of the window being moved. When this flag is set, the application
        ///         must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOREPOSITION (0x0200)</term><description>Same as the SWP_NOOWNERZORDER flag.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOSENDCHANGING (0x0400)</term>
        ///         <description>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOSIZE (0x0001)</term>
        ///         <description>Retains the current size (ignores the cx and cy parameters).</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOZORDER (0x0004)</term>
        ///         <description>Retains the current Z order (ignores the hWndInsertAfter parameter).</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_SHOWWINDOW (0x0040)</term><description>Displays the window.</description>
        ///     </item>
        ///     </list>
        /// </param>
        /// <returns><c>true</c> or nonzero if the function succeeds, <c>false</c> or zero otherwise or if function fails.</returns>
        /// <remarks>
        ///     <para>
        ///     As part of the Vista re-architecture, all services were moved off the interactive desktop into Session 0.
        ///     hwnd and window manager operations are only effective inside a session and cross-session attempts to manipulate
        ///     the hwnd will fail. For more information, see The Windows Vista Developer Story: Application Compatibility
        ///     Cookbook.
        ///     </para>
        ///     <para>
        ///     If you have changed certain window data using SetWindowLong, you must call SetWindowPos for the changes to
        ///     take effect. Use the following combination for uFlags: SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER |
        ///     SWP_FRAMECHANGED.
        ///     </para>
        ///     <para>
        ///     A window can be made a topmost window either by setting the hWndInsertAfter parameter to HWND_TOPMOST and
        ///     ensuring that the SWP_NOZORDER flag is not set, or by setting a window's position in the Z order so that it is
        ///     above any existing topmost windows. When a non-topmost window is made topmost, its owned windows are also made
        ///     topmost. Its owners, however, are not changed.
        ///     </para>
        ///     <para>
        ///     If neither the SWP_NOACTIVATE nor SWP_NOZORDER flag is specified (that is, when the application requests that
        ///     a window be simultaneously activated and its position in the Z order changed), the value specified in
        ///     hWndInsertAfter is used only in the following circumstances.
        ///     </para>
        ///     <list type="bullet">
        ///     <item>Neither the HWND_TOPMOST nor HWND_NOTOPMOST flag is specified in hWndInsertAfter. </item>
        ///     <item>The window identified by hWnd is not the active window. </item>
        ///     </list>
        ///     <para>
        ///     An application cannot activate an inactive window without also bringing it to the top of the Z order.
        ///     Applications can change an activated window's position in the Z order without restrictions, or it can activate
        ///     a window and then move it to the top of the topmost or non-topmost windows.
        ///     </para>
        ///     <para>
        ///     If a topmost window is repositioned to the bottom (HWND_BOTTOM) of the Z order or after any non-topmost
        ///     window, it is no longer topmost. When a topmost window is made non-topmost, its owners and its owned windows
        ///     are also made non-topmost windows.
        ///     </para>
        ///     <para>
        ///     A non-topmost window can own a topmost window, but the reverse cannot occur. Any window (for example, a
        ///     dialog box) owned by a topmost window is itself made a topmost window, to ensure that all owned windows stay
        ///     above their owner.
        ///     </para>
        ///     <para>
        ///     If an application is not in the foreground, and should be in the foreground, it must call the
        ///     SetForegroundWindow function.
        ///     </para>
        ///     <para>
        ///     To use SetWindowPos to bring a window to the top, the process that owns the window must have
        ///     SetForegroundWindow permission.
        ///     </para>
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            SetWindowPosFlags uFlags);

        #endregion
    }

    [Flags()]
    public enum DisplayDeviceStateFlags : int
    {
        /// <summary>The device is part of the desktop.</summary>
        AttachedToDesktop = 0x1,
        MultiDriver = 0x2,
        /// <summary>The device is part of the desktop.</summary>
        PrimaryDevice = 0x4,
        /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
        MirroringDriver = 0x8,
        /// <summary>The device is VGA compatible.</summary>
        VGACompatible = 0x10,
        /// <summary>The device is removable; it cannot be the primary display.</summary>
        Removable = 0x20,
        /// <summary>The device has more display modes than its output devices support.</summary>
        ModesPruned = 0x8000000,
        Remote = 0x4000000,
        Disconnect = 0x2000000
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct DISPLAY_DEVICE
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        [MarshalAs(UnmanagedType.U4)]
        public DisplayDeviceStateFlags StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    #region DEVMODEA

    [StructLayout(LayoutKind.Sequential)]
    public struct DUMMYSTRUCTNAME
    {
        public short dmOrientation;
        public short dmPaperSize;
        public short dmPaperLength;
        public short dmPaperWidth;
        public short dmScale;
        public short dmCopies;
        public short dmDefaultSource;
        public short dmPrintQuality;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DUMMYSTRUCTNAME2
    {
        public Point dmPosition;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DUMMYUNIONNAME
    {
        [FieldOffset(0)]
        public DUMMYSTRUCTNAME DUMMYSTRUCTNAME;
        [FieldOffset(0)]
        public Point dmPosition;
        [FieldOffset(0)]
        public DUMMYSTRUCTNAME2 DUMMYSTRUCTNAME2;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DUMMYUNIONNAME2
    {
        [FieldOffset(0)]
        public int dmDisplayFlags;
        [FieldOffset(0)]
        public int dmNup;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Native.CCHDEVICENAME)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public DUMMYUNIONNAME DUMMYUNIONNAME;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Native.CCHFORMNAME)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public DUMMYUNIONNAME2 DUMMYUNIONNAME2;
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

    #endregion

    public enum BitmapCompressionMode : uint
    {
        BI_RGB = 0,
        BI_RLE8 = 1,
        BI_RLE4 = 2,
        BI_BITFIELDS = 3,
        BI_JPEG = 4,
        BI_PNG = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public BitmapCompressionMode biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;

        public void Init() => biSize = (uint)Marshal.SizeOf(this);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public uint[] bmiColors;
    }

    public enum DIB_Color_Mode : uint
    {
        DIB_RGB_COLORS = 0,
        DIB_PAL_COLORS = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr handle;
        public uint msg;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public Point p;
        //public uint lPrivate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;// 指定矩形左上角的x坐标
        public int Top;// 指定矩形左上角的y坐标
        public int Right;// 指定矩形右下角的x坐标
        public int Bottom;// 指定矩形右下角的y坐标

        /*
        public static readonly RECT Empty;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public RECT(Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

        public int X { get => Left; set { Right -= Left - value; Left = value; } }

        public int Y { get => Top; set { Bottom -= Top - value; Top = value; } }

        public int Width { get => Right - Left; set => Right = value + Left; }

        public int Height { get => Bottom - Top; set => Bottom = value + Top; }

        public Point Location { get => new Point(Left, Top); set { X = value.X; Y = value.Y; } }

        public Size Size { get => new Size(Width, Height); set { Width = value.Width; Height = value.Height; } }

        public static implicit operator Rectangle(RECT r) => new Rectangle(r.Left, r.Top, r.Width, r.Height);

        public static implicit operator RECT(Rectangle r) => new RECT(r);

        public static bool operator ==(RECT r1, RECT r2) => r1.Equals(r2);

        public static bool operator !=(RECT r1, RECT r2) => !r1.Equals(r2);

        public bool Equals(RECT r) => r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;

        public override bool Equals(object obj)
        {
            if (obj is RECT)
            {
                return Equals((RECT)obj);
            }
            else if (obj is Rectangle)
            {
                return Equals((Rectangle)obj);
            }
            return false;
        }

        public override int GetHashCode() => ((Rectangle)this).GetHashCode();

        public override string ToString() => string.Format(System.Globalization.CultureInfo.CurrentCulture
                , "{{Left={0},Top={1},Right={2},Bottom={3}}}"
                , Left, Top, Right, Bottom);
        */
    }

    public enum GWL
    {
        EXSTYLE = -20,
        HINSTANCE = -6,
        HWNDPARENT = -8,
        ID = -12,
        STYLE = -16,
        USERDATA = -21,
        WNDPROC = -4
    }

    /// <summary>
    ///     Special window handles
    /// </summary>
    public enum SpecialWindowHandles
    {
        /// <summary>
        ///     Places the window at the top of the Z order.
        /// </summary>
        HWND_TOP = 0,
        /// <summary>
        ///     Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
        /// </summary>
        HWND_BOTTOM = 1,
        /// <summary>
        ///     Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
        /// </summary>
        HWND_TOPMOST = -1,
        /// <summary>
        ///     Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
        /// </summary>
        HWND_NOTOPMOST = -2
    }

    [Flags]
    public enum SetWindowPosFlags : uint
    {
        /// <summary>
        ///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
        /// </summary>
        SWP_ASYNCWINDOWPOS = 0x4000,

        /// <summary>
        ///     Prevents generation of the WM_SYNCPAINT message.
        /// </summary>
        SWP_DEFERERASE = 0x2000,

        /// <summary>
        ///     Draws a frame (defined in the window's class description) around the window.
        /// </summary>
        SWP_DRAWFRAME = 0x0020,

        /// <summary>
        ///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
        /// </summary>
        SWP_FRAMECHANGED = 0x0020,

        /// <summary>
        ///     Hides the window.
        /// </summary>
        SWP_HIDEWINDOW = 0x0080,

        /// <summary>
        ///     Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
        /// </summary>
        SWP_NOACTIVATE = 0x0010,

        /// <summary>
        ///     Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
        /// </summary>
        SWP_NOCOPYBITS = 0x0100,

        /// <summary>
        ///     Retains the current position (ignores X and Y parameters).
        /// </summary>
        SWP_NOMOVE = 0x0002,

        /// <summary>
        ///     Does not change the owner window's position in the Z order.
        /// </summary>
        SWP_NOOWNERZORDER = 0x0200,

        /// <summary>
        ///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
        /// </summary>
        SWP_NOREDRAW = 0x0008,

        /// <summary>
        ///     Same as the SWP_NOOWNERZORDER flag.
        /// </summary>
        SWP_NOREPOSITION = 0x0200,

        /// <summary>
        ///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
        /// </summary>
        SWP_NOSENDCHANGING = 0x0400,

        /// <summary>
        ///     Retains the current size (ignores the cx and cy parameters).
        /// </summary>
        SWP_NOSIZE = 0x0001,

        /// <summary>
        ///     Retains the current Z order (ignores the hWndInsertAfter parameter).
        /// </summary>
        SWP_NOZORDER = 0x0004,

        /// <summary>
        ///     Displays the window.
        /// </summary>
        SWP_SHOWWINDOW = 0x0040,
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

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Xstream
{
    public static partial class Native
    {
        public static readonly Version WINVER = Environment.OSVersion.Version;
        public static readonly int _WIN32_WINNT = WINVER.Major << 8 | WINVER.Minor;
        public const int _WIN32_WINNT_NT4 = 0x0400;             // Windows NT 4.0
        public const int _WIN32_WINNT_WIN2K = 0x0500;           // Windows 2000
        public const int _WIN32_WINNT_WINXP = 0x0501;           // Windows XP
        public const int _WIN32_WINNT_WS03 = 0x0502;            // Windows Server 2003
        public const int _WIN32_WINNT_WIN6 = 0x0600;            // Windows Vista
        public const int _WIN32_WINNT_VISTA = 0x0600;           // Windows Vista
        public const int _WIN32_WINNT_WS08 = 0x0600;            // Windows Server 2008
        public const int _WIN32_WINNT_LONGHORN = 0x0600;        // Windows Vista
        public const int _WIN32_WINNT_WIN7 = 0x0601;            // Windows 7
        public const int _WIN32_WINNT_WIN8 = 0x0602;            // Windows 8
        public const int _WIN32_WINNT_WINBLUE = 0x0603;         // Windows 8.1
        public const int _WIN32_WINNT_WINTHRESHOLD = 0x0A00;    // Windows 10
        public const int _WIN32_WINNT_WIN10 = 0x0A00;           // Windows 10

        public const long STYLE_BASIC = WS_CLIPSIBLINGS | WS_CLIPCHILDREN;
        public const long STYLE_FULLSCREEN = WS_POPUP;
        public const long STYLE_BORDERLESS = WS_POPUP;
        public const long STYLE_NORMAL = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX;
        public const long STYLE_RESIZABLE = WS_THICKFRAME | WS_MAXIMIZEBOX;
        public const long STYLE_MASK = STYLE_FULLSCREEN | STYLE_BORDERLESS | STYLE_NORMAL | STYLE_RESIZABLE;

        public const uint SDL_PIXELFORMAT_UNKNOWN = 0;
        public static readonly uint SDL_PIXELFORMAT_INDEX1LSB
            = SDL_Define_PixelFormat(SDL_PixelType.INDEX1, SDL_BitmapOrder.B4321, 0, 1, 0);
        public static readonly uint SDL_PIXELFORMAT_INDEX1MSB
            = SDL_Define_PixelFormat(SDL_PixelType.INDEX1, SDL_BitmapOrder.B1234, 0, 1, 0);
        public static readonly uint SDL_PIXELFORMAT_INDEX4LSB
            = SDL_Define_PixelFormat(SDL_PixelType.INDEX4, SDL_BitmapOrder.B4321, 0, 4, 0);
        public static readonly uint SDL_PIXELFORMAT_INDEX4MSB
            = SDL_Define_PixelFormat(SDL_PixelType.INDEX4, SDL_BitmapOrder.B1234, 0, 4, 0);
        public static readonly uint SDL_PIXELFORMAT_INDEX8
            = SDL_Define_PixelFormat(SDL_PixelType.INDEX8, 0, 0, 8, 1);
        public static readonly uint SDL_PIXELFORMAT_RGB332
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED8, SDL_PackedOrder.XRGB, SDL_PackedLayout.L332, 8, 1);
        public static readonly uint SDL_PIXELFORMAT_RGB444
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED16, SDL_PackedOrder.XRGB, SDL_PackedLayout.L4444, 12, 2);
        public static readonly uint SDL_PIXELFORMAT_RGB555
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED16, SDL_PackedOrder.XRGB, SDL_PackedLayout.L1555, 15, 2);
        public static readonly uint SDL_PIXELFORMAT_BGR555
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED16, SDL_PackedOrder.XBGR, SDL_PackedLayout.L1555, 15, 2);
        public static readonly uint SDL_PIXELFORMAT_ARGB4444
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED16, SDL_PackedOrder.ARGB, SDL_PackedLayout.L4444, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_RGBA4444
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED16, SDL_PackedOrder.RGBA, SDL_PackedLayout.L4444, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_ABGR4444
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED16, SDL_PackedOrder.ABGR, SDL_PackedLayout.L4444, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_BGRA4444
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED16, SDL_PackedOrder.BGRA, SDL_PackedLayout.L4444, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_ARGB1555
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED16, SDL_PackedOrder.ARGB, SDL_PackedLayout.L1555, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_RGBA5551
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED16, SDL_PackedOrder.RGBA, SDL_PackedLayout.L5551, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_ABGR1555
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED16, SDL_PackedOrder.ABGR, SDL_PackedLayout.L1555, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_BGRA5551
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED16, SDL_PackedOrder.BGRA, SDL_PackedLayout.L5551, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_RGB565
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED16, SDL_PackedOrder.XRGB, SDL_PackedLayout.L565, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_BGR565
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED16, SDL_PackedOrder.XBGR, SDL_PackedLayout.L565, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_RGB24
            = SDL_Define_PixelFormat(SDL_PixelType.ARRAYU8, SDL_ArrayOrder.RGB, 0, 24, 3);
        public static readonly uint SDL_PIXELFORMAT_BGR24
            = SDL_Define_PixelFormat(SDL_PixelType.ARRAYU8, SDL_ArrayOrder.BGR, 0, 24, 3);
        public static readonly uint SDL_PIXELFORMAT_RGB888
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED32, SDL_PackedOrder.XRGB, SDL_PackedLayout.L8888, 24, 4);
        public static readonly uint SDL_PIXELFORMAT_RGBX8888
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED32, SDL_PackedOrder.RGBX, SDL_PackedLayout.L8888, 24, 4);
        public static readonly uint SDL_PIXELFORMAT_BGR888
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED32, SDL_PackedOrder.XBGR, SDL_PackedLayout.L8888, 24, 4);
        public static readonly uint SDL_PIXELFORMAT_BGRX8888
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED32, SDL_PackedOrder.BGRX, SDL_PackedLayout.L8888, 24, 4);
        public static readonly uint SDL_PIXELFORMAT_ARGB8888
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED32, SDL_PackedOrder.ARGB, SDL_PackedLayout.L8888, 32, 4);
        public static readonly uint SDL_PIXELFORMAT_RGBA8888
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED32, SDL_PackedOrder.RGBA, SDL_PackedLayout.L8888, 32, 4);
        public static readonly uint SDL_PIXELFORMAT_ABGR8888
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED32, SDL_PackedOrder.ABGR, SDL_PackedLayout.L8888, 32, 4);
        public static readonly uint SDL_PIXELFORMAT_BGRA8888
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED32, SDL_PackedOrder.BGRA, SDL_PackedLayout.L8888, 32, 4);
        public static readonly uint SDL_PIXELFORMAT_ARGB2101010
            = SDL_Define_PixelFormat(SDL_PixelType.PACKED32, SDL_PackedOrder.ARGB, SDL_PackedLayout.L2101010, 32, 4);
        public static readonly uint SDL_PIXELFORMAT_YV12 = MakeFourCC('Y', 'V', '1', '2');// Y + V + U  (3 planes)
        public static readonly uint SDL_PIXELFORMAT_IYUV = MakeFourCC('I', 'Y', 'U', 'V');// Y + U + V  (3 planes)
        public static readonly uint SDL_PIXELFORMAT_YUY2 = MakeFourCC('Y', 'U', 'Y', '2');// Y0+U0+Y1+V0 (1 plane)
        public static readonly uint SDL_PIXELFORMAT_UYVY = MakeFourCC('U', 'Y', 'V', 'Y');// U0+Y0+V0+Y1 (1 plane)
        public static readonly uint SDL_PIXELFORMAT_YVYU = MakeFourCC('Y', 'V', 'Y', 'U');// Y0+V0+Y1+U0 (1 plane)

        static SDL_VideoDevice _video;

        static Native()
        {
            InitModes();
        }

        public static T GetValue<T>(Enum val) => Unsafe.As<byte, T>(ref Unsafe.As<RawData>(val).data);
        public static uint GetValue(Enum val) => Unsafe.As<byte, uint>(ref Unsafe.As<RawData>(val).data);

        #region CBool

        public static bool CBool(sbyte val) => val != 0;
        public static bool CBool(byte val) => val != 0;
        public static bool CBool(short val) => val != 0;
        public static bool CBool(ushort val) => val != 0;
        public static bool CBool(int val) => val != 0;
        public static bool CBool(uint val) => val != 0U;
        public static bool CBool(long val) => val != 0L;
        public static bool CBool(ulong val) => val != 0UL;
        public static bool CBool(double val) => val != 0D;
        public static bool CBool(float val) => val != 0F;
        public static bool CBool(char val) => val != 0;
        public static bool CBool(IntPtr val) => val != IntPtr.Zero;
        public static unsafe bool CBool(void* val) => val != (void*)0;
        public static unsafe bool CBool(Enum val)
        {
            fixed (void* pVal = &Unsafe.As<RawData>(val).data)
            {
                return val.GetTypeCode() switch
                {
                    TypeCode.SByte => CBool(*(sbyte*)pVal),
                    TypeCode.Byte => CBool(*(byte*)pVal),
                    TypeCode.Int16 => CBool(*(short*)pVal),
                    TypeCode.UInt16 => CBool(*(ushort*)pVal),
                    TypeCode.Int32 => CBool(*(int*)pVal),
                    TypeCode.UInt32 => CBool(*(uint*)pVal),
                    TypeCode.Int64 => CBool(*(long*)pVal),
                    TypeCode.UInt64 => CBool(*(ulong*)pVal),
                    _ => throw new InvalidOperationException($"Invalid primitive type {val.GetTypeCode()}")
                };
            }
        }

        //public static T CBool<T>(bool val) where T : IConvertible => (T)Convert.ChangeType(val, typeof(T));
        public static T CBool<T>(bool val) => Unsafe.As<bool, T>(ref val);
        public static byte CBool(bool val) => Unsafe.As<bool, byte>(ref val);

        #endregion

        #region windows modes

        static unsafe bool GetDisplayMode(string deviceName, uint index, ref SDL_DisplayMode mode)
        {
            DEVMODE data;
            DEVMODE devmode = new DEVMODE();
            IntPtr hdc;

            devmode.dmSize = (ushort)Marshal.SizeOf(devmode);
            devmode.dmDriverExtra = 0;
            if (!EnumDisplaySettings(deviceName, index, ref devmode))
            {
                return false;
            }

            data = devmode;
            data.dmFields = DM_BITSPERPEL
                | DM_PELSWIDTH
                | DM_PELSHEIGHT
                | DM_DISPLAYFREQUENCY
                | DM_DISPLAYFLAGS;

            // Fill in the mode information
            mode.format = SDL_PIXELFORMAT_UNKNOWN;
            mode.w = (int)devmode.dmPelsWidth;
            mode.h = (int)devmode.dmPelsHeight;
            mode.refresh_rate = (int)devmode.dmDisplayFrequency;
            mode.DeviceMode = data;

            if (index == ENUM_CURRENT_SETTINGS
                && (hdc = CreateDC(deviceName, null, null, IntPtr.Zero)) != IntPtr.Zero)
            {
                BITMAPINFO bmi;
                IntPtr hbm;

                bmi = new BITMAPINFO();
                bmi.bmiHeader.Init();

                hbm = CreateCompatibleBitmap(hdc, 1, 1);
                GetDIBits(hdc, hbm, 0, 1, null, ref bmi, DIB_RGB_COLORS);
                GetDIBits(hdc, hbm, 0, 1, null, ref bmi, DIB_RGB_COLORS);
                DeleteObject(hbm);
                DeleteDC(hdc);
                if (bmi.bmiHeader.biCompression == BI_BITFIELDS)
                {
                    fixed (RGBQUAD* bmiColors = bmi.bmiColors)
                    {
                        switch (*(uint*)bmiColors)
                        {
                            case 0x00FF0000:
                                mode.format = SDL_PIXELFORMAT_RGB888;
                                break;
                            case 0x000000FF:
                                mode.format = SDL_PIXELFORMAT_BGR888;
                                break;
                            case 0xF800:
                                mode.format = SDL_PIXELFORMAT_RGB565;
                                break;
                            case 0x7C00:
                                mode.format = SDL_PIXELFORMAT_RGB555;
                                break;
                        }
                    }
                }
                else if (bmi.bmiHeader.biBitCount == 8)
                {
                    mode.format = SDL_PIXELFORMAT_INDEX8;
                }
                else if (bmi.bmiHeader.biBitCount == 4)
                {
                    mode.format = SDL_PIXELFORMAT_INDEX4LSB;
                }
            }
            else
            {
                // FIXME: Can we tell what this will be?
                if ((devmode.dmFields & DM_BITSPERPEL) == DM_BITSPERPEL)
                {
                    switch (devmode.dmBitsPerPel)
                    {
                        case 32:
                            mode.format = SDL_PIXELFORMAT_RGB888;
                            break;
                        case 24:
                            mode.format = SDL_PIXELFORMAT_RGB24;
                            break;
                        case 16:
                            mode.format = SDL_PIXELFORMAT_RGB565;
                            break;
                        case 15:
                            mode.format = SDL_PIXELFORMAT_RGB555;
                            break;
                        case 8:
                            mode.format = SDL_PIXELFORMAT_INDEX8;
                            break;
                        case 4:
                            mode.format = SDL_PIXELFORMAT_INDEX4LSB;
                            break;
                    }
                }
            }
            return true;
        }

        static bool AddDisplay(string deviceName)
        {
            SDL_VideoDisplay display = new SDL_VideoDisplay();
            SDL_DisplayMode mod = new SDL_DisplayMode();
            DISPLAY_DEVICE device = new DISPLAY_DEVICE();

            Debug.WriteLine("Display: {0}", deviceName);
            if (!GetDisplayMode(deviceName, ENUM_CURRENT_SETTINGS, ref mod))
            {
                return false;
            }

            device.cb = (uint)Marshal.SizeOf(device);
            if (EnumDisplayDevices(deviceName, 0, ref device, 0))
            {
                display.name = device.DeviceString;
            }
            display.desktop_mode = mod;
            display.current_mode = mod;
            display.DeviceName = deviceName;
            AddVideoDisplay(display);
            return true;
        }

        static void InitModes()
        {
            int pass;
            uint i, j, count;
            DISPLAY_DEVICE device;

            device = new DISPLAY_DEVICE();
            device.cb = (uint)Marshal.SizeOf(device);

            // Get the primary display in the first pass
            for (pass = 0; pass < 2; ++pass)
            {
                for (i = 0; ; ++i)
                {
                    string deviceName;

                    if (!EnumDisplayDevices(null, i, ref device, 0)) break;
                    if (!CBool(device.StateFlags & DISPLAY_DEVICE_ATTACHED_TO_DESKTOP)) continue;
                    if (pass == 0)
                    {
                        if (!CBool(device.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE)) continue;
                    }
                    else
                    {
                        if (CBool(device.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE)) continue;
                    }
                    deviceName = device.DeviceName;
                    Debug.WriteLine("Device: {0}", deviceName);
                    count = 0;
                    for (j = 0; ; ++j)
                    {
                        if (!EnumDisplayDevices(deviceName, j, ref device, 0)) break;
                        if (!CBool(device.StateFlags & DISPLAY_DEVICE_ATTACHED_TO_DESKTOP)) continue;
                        if (pass == 0)
                        {
                            if (!CBool(device.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE)) continue;
                        }
                        else
                        {
                            if (CBool(device.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE)) continue;
                        }
                        count += CBool(AddDisplay(device.DeviceName));
                    }
                    if (count == 0)
                    {
                        AddDisplay(deviceName);
                    }
                }
            }
            if (_video.num_displays == 0)
            {
                throw new NotSupportedException("No displays available");
            }
        }

        #endregion

        static int AddVideoDisplay(SDL_VideoDisplay display)
        {
            SDL_VideoDisplay[] displays;
            int index;

            displays = new SDL_VideoDisplay[_video.num_displays + 1];

            index = _video.num_displays++;
            displays[index] = display;
            displays[index].device = _video;

            _video.displays?.CopyTo(displays, 0);
            _video.displays = displays;

            if (string.IsNullOrEmpty(display.name))
            {
                displays[index].name = index.ToString();
            }

            return index;
        }

        #region enumerated pixel format definitions

        static uint MakeFourCC(char ch0, char ch1, char ch2, char ch3)
        {
            byte[] chs = Encoding.ASCII.GetBytes(new char[] { ch0, ch1, ch2, ch3 });
            return chs[0] | (uint)chs[1] << 8 | (uint)chs[2] << 16 | (uint)chs[3] << 24;
        }

        static uint SDL_Define_PixelFormat(Enum type, Enum order, Enum layout, uint bits, uint bytes)
            => SDL_Define_PixelFormat(GetValue(type), GetValue(order), GetValue(layout), bits, bytes);

        static uint SDL_Define_PixelFormat(Enum type, Enum order, uint layout, uint bits, uint bytes)
            => SDL_Define_PixelFormat(GetValue(type), GetValue(order), layout, bits, bytes);

        static uint SDL_Define_PixelFormat(Enum type, uint order, uint layout, uint bits, uint bytes)
            => SDL_Define_PixelFormat(GetValue(type), order, layout, bits, bytes);

        static uint SDL_Define_PixelFormat(uint type, uint order, uint layout, uint bits, uint bytes)
            => 1 << 28 | type << 24 | order << 20 | layout << 16 | bits << 8 | bytes << 0;

        enum SDL_PixelType : uint
        {
            UNKNOWN,
            INDEX1,
            INDEX4,
            INDEX8,
            PACKED8,
            PACKED16,
            PACKED32,
            ARRAYU8,
            ARRAYU16,
            ARRAYU32,
            ARRAYF16,
            ARRAYF32
        }

        enum SDL_BitmapOrder : uint
        {
            NONE,
            B4321,
            B1234
        }

        enum SDL_PackedOrder : uint
        {
            NONE,
            XRGB,
            RGBX,
            ARGB,
            RGBA,
            XBGR,
            BGRX,
            ABGR,
            BGRA
        }

        enum SDL_ArrayOrder : uint
        {
            NONE,
            RGB,
            RGBA,
            ARGB,
            BGR,
            BGRA,
            ABGR
        }

        enum SDL_PackedLayout : uint
        {
            NONE,
            L332,
            L4444,
            L1555,
            L5551,
            L565,
            L8888,
            L2101010,
            L1010102
        }

        #endregion

        class RawData
        {
            public byte data;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_VideoDevice
    {
        public int num_displays;
        public SDL_VideoDisplay[] displays;
        public Form[] windows;

        public SDL_VideoDisplay PrimaryDisplay => displays[0];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_VideoDisplay
    {
        public string name;
        public int max_display_modes;
        public int num_display_modes;
        public SDL_DisplayMode[] display_modes;
        public SDL_DisplayMode desktop_mode;
        public SDL_DisplayMode current_mode;

        public Form fullscreen_window;

        public SDL_VideoDevice device;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_DisplayMode
    {
        public uint format;// pixel format
        public int w;// width
        public int h;// height
        public int refresh_rate;// refresh rate (or zero for unspecified)

        public DEVMODE DeviceMode;
    }
}

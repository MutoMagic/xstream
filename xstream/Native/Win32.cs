using SharpDX.Direct3D9;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

        static uint _numDisplays;
        static VideoDisplay[] _displays;

        public static VideoDisplay PrimaryDisplay => _displays[0];

        static Native()
        {
            //InitModes();
        }

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
        public static uint CBool(bool val) => Unsafe.As<bool, uint>(ref val);

        #endregion

        #region windows modes

        static unsafe bool GetDisplayMode(string deviceName, uint index, ref DisplayMode mode)
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
            mode.format = Format.Unknown;
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
                                mode.format = Format.X8R8G8B8;
                                break;
                            case 0x000000FF:
                                mode.format = Format.X8B8G8R8;
                                break;
                            case 0xF800:
                                mode.format = Format.R5G6B5;
                                break;
                            case 0x7C00:
                                mode.format = Format.X1R5G5B5;
                                break;
                        }
                    }
                }
                else if (bmi.bmiHeader.biBitCount == 8)
                {
                    mode.format = Format.P8;// FIXME: It could be D3DFMT_UNKNOWN?
                }
                else if (bmi.bmiHeader.biBitCount == 4)
                {
                    mode.format = Format.Unknown;
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
                            mode.format = Format.X8R8G8B8;
                            break;
                        case 24:
                            mode.format = Format.R8G8B8;
                            break;
                        case 16:
                            mode.format = Format.R5G6B5;
                            break;
                        case 15:
                            mode.format = Format.X1R5G5B5;
                            break;
                        case 8:
                            mode.format = Format.P8;
                            break;
                        case 4:
                            mode.format = Format.Unknown;
                            break;
                    }
                }
            }
            return true;
        }

        static int AddVideoDisplay(VideoDisplay display)
        {
            VideoDisplay[] displays;
            int index;

            displays = new VideoDisplay[_numDisplays + 1];
            index = _numDisplays++;
            displays[index] = display;

            _displays?.CopyTo(displays, 0);
            _displays = displays;

            if (string.IsNullOrEmpty(display.name))
            {
                displays[index].name = index.ToString();
            }

            return index;
        }

        static bool AddDisplay(string deviceName)
        {
            VideoDisplay display = new VideoDisplay();
            DisplayMode mod = new DisplayMode();
            DISPLAY_DEVICE device = new DISPLAY_DEVICE();

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
            if (_numDisplays == 0)
            {
                throw new NotSupportedException("No displays available");
            }
        }

        #endregion
    }

    class RawData
    {
        public byte data;
    }

    public struct VideoDisplay
    {
        public string name;
        public int max_display_modes;
        public int num_display_modes;
        public DisplayMode[] display_modes;
        public DisplayMode desktop_mode;
        public DisplayMode current_mode;

        public string DeviceName;
    }

    public struct DisplayMode
    {
        public Format format;// pixel format
        public int w;// width
        public int h;// height
        public int refresh_rate;// refresh rate (or zero for unspecified)

        public DEVMODE DeviceMode;
    }
}

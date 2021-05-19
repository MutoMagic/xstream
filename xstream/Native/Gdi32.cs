using System;
using System.Runtime.InteropServices;

namespace Xstream
{
    partial class Native
    {
        #region GetDIBits()

        public const uint DIB_RGB_COLORS = 0;
        public const uint DIB_PAL_COLORS = 1;

        public const uint BI_RGB = 0;
        public const uint BI_RLE8 = 1;
        public const uint BI_RLE4 = 2;
        public const uint BI_BITFIELDS = 3;
        public const uint BI_JPEG = 4;
        public const uint BI_PNG = 5;

        #endregion

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(
            string pwszDriver,
            string pwszDevice,
            string pszPort,
            IntPtr pdm);// NULL

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(
            string pwszDriver,
            string pwszDevice,
            string pszPort,
            ref DEVMODE pdm);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteDC([In] IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap([In] IntPtr hdc, int cx, int cy);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr ho);

        [DllImport("gdi32.dll")]
        public static extern int GetDIBits(
            [In] IntPtr hdc,
            [In] IntPtr hbm,
            uint start,
            uint cLines,
            [Out] byte[] lpvBits,
            ref BITMAPINFO lpbmi,
            uint usage);
    }

    #region tagBITMAPINFO

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;

        public void Init() => biSize = Native.SizeOf(this);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RGBQUAD
    {
        public ushort rgbBlue;
        public ushort rgbGreen;
        public ushort rgbRed;
        public ushort rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public RGBQUAD[] bmiColors;
    }

    #endregion
}

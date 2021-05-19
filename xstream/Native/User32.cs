using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Xstream
{
    partial class Native
    {
        #region ShowWindow()

        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_NORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_FORCEMINIMIZE = 11;
        public const int SW_MAX = 11;

        #endregion

        #region SetWindowPos()

        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOREDRAW = 0x0008;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_HIDEWINDOW = 0x0080;
        public const uint SWP_NOCOPYBITS = 0x0100;
        public const uint SWP_NOOWNERZORDER = 0x0200;
        public const uint SWP_NOSENDCHANGING = 0x0400;
        public const uint SWP_DEFERERASE = 0x2000;
        public const uint SWP_ASYNCWINDOWPOS = 0x4000;

        public const uint SWP_DRAWFRAME = SWP_FRAMECHANGED;
        public const uint SWP_NOREPOSITION = SWP_NOOWNERZORDER;

        #endregion

        #region GetWindowLong()

        public const int GWLP_WNDPROC = -4;
        public const int GWLP_HINSTANCE = -6;
        public const int GWLP_HWNDPARENT = -8;
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;
        public const int GWLP_USERDATA = -21;
        public const int GWLP_ID = -12;

        public const int DWLP_MSGRESULT = 0;
        public static readonly int DWLP_DLGPROC = DWLP_MSGRESULT + IntPtr.Size;
        public static readonly int DWLP_USER = DWLP_DLGPROC + IntPtr.Size;

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
        public const long WS_CAPTION = 0x00C00000L;// WS_BORDER | WS_DLGFRAME
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

        public const long WS_TILED = WS_OVERLAPPED;
        public const long WS_ICONIC = WS_MINIMIZE;
        public const long WS_SIZEBOX = WS_THICKFRAME;
        public const long WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW;

        public const long WS_OVERLAPPEDWINDOW
            = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
        public const long WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU;
        public const long WS_CHILDWINDOW = WS_CHILD;

        // Extended Window Styles
        public const long WS_EX_DLGMODALFRAME = 0x00000001L;
        public const long WS_EX_NOPARENTNOTIFY = 0x00000004L;
        public const long WS_EX_TOPMOST = 0x00000008L;
        public const long WS_EX_ACCEPTFILES = 0x00000010L;
        public const long WS_EX_TRANSPARENT = 0x00000020L;
        public const long WS_EX_MDICHILD = 0x00000040L;
        public const long WS_EX_TOOLWINDOW = 0x00000080L;
        public const long WS_EX_WINDOWEDGE = 0x00000100L;
        public const long WS_EX_CLIENTEDGE = 0x00000200L;
        public const long WS_EX_CONTEXTHELP = 0x00000400L;
        public const long WS_EX_RIGHT = 0x00001000L;
        public const long WS_EX_LEFT = 0x00000000L;
        public const long WS_EX_RTLREADING = 0x00002000L;
        public const long WS_EX_LTRREADING = 0x00000000L;
        public const long WS_EX_LEFTSCROLLBAR = 0x00004000L;
        public const long WS_EX_RIGHTSCROLLBAR = 0x00000000L;
        public const long WS_EX_CONTROLPARENT = 0x00010000L;
        public const long WS_EX_STATICEDGE = 0x00020000L;
        public const long WS_EX_APPWINDOW = 0x00040000L;

        public const long WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE;
        public const long WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;

        public const long WS_EX_LAYERED = 0x00080000;
        public const long WS_EX_NOINHERITLAYOUT = 0x00100000L;// Disable inheritence of mirroring by children
        public const long WS_EX_NOREDIRECTIONBITMAP = 0x00200000L;
        public const long WS_EX_LAYOUTRTL = 0x00400000L;// Right to left mirroring
        public const long WS_EX_COMPOSITED = 0x02000000L;
        public const long WS_EX_NOACTIVATE = 0x08000000L;

        #endregion

        #region ChangeDisplaySettingsEx()

        public const uint CDS_UPDATEREGISTRY = 0x00000001;
        public const uint CDS_TEST = 0x00000002;
        public const uint CDS_FULLSCREEN = 0x00000004;
        public const uint CDS_GLOBAL = 0x00000008;
        public const uint CDS_SET_PRIMARY = 0x00000010;
        public const uint CDS_VIDEOPARAMETERS = 0x00000020;
        public const uint CDS_ENABLE_UNSAFE_MODES = 0x00000100;
        public const uint CDS_DISABLE_UNSAFE_MODES = 0x00000200;
        public const uint CDS_NORESET = 0x10000000;
        public const uint CDS_RESET = 0x40000000;

        public const int DISP_CHANGE_SUCCESSFUL = 0;
        public const int DISP_CHANGE_RESTART = 1;
        public const int DISP_CHANGE_FAILED = -1;
        public const int DISP_CHANGE_BADMODE = -2;
        public const int DISP_CHANGE_NOTUPDATED = -3;
        public const int DISP_CHANGE_BADFLAGS = -4;
        public const int DISP_CHANGE_BADPARAM = -5;
        public const int DISP_CHANGE_BADDUALVIEW = -6;

        #endregion

        #region EnumDisplaySettings()

        public const uint ENUM_CURRENT_SETTINGS = 0xFFFFFFFF;
        public const uint ENUM_REGISTRY_SETTINGS = 0xFFFFFFFE;

        // field selection bits
        public const uint DM_ORIENTATION = 0x00000001;
        public const uint DM_PAPERSIZE = 0x00000002;
        public const uint DM_PAPERLENGTH = 0x00000004;
        public const uint DM_PAPERWIDTH = 0x00000008;
        public const uint DM_SCALE = 0x00000010;
        public const uint DM_POSITION = 0x00000020;
        public const uint DM_NUP = 0x00000040;
        public const uint DM_DISPLAYORIENTATION = 0x00000080;
        public const uint DM_COPIES = 0x00000100;
        public const uint DM_DEFAULTSOURCE = 0x00000200;
        public const uint DM_PRINTQUALITY = 0x00000400;
        public const uint DM_COLOR = 0x00000800;
        public const uint DM_DUPLEX = 0x00001000;
        public const uint DM_YRESOLUTION = 0x00002000;
        public const uint DM_TTOPTION = 0x00004000;
        public const uint DM_COLLATE = 0x00008000;
        public const uint DM_FORMNAME = 0x00010000;
        public const uint DM_LOGPIXELS = 0x00020000;
        public const uint DM_BITSPERPEL = 0x00040000;
        public const uint DM_PELSWIDTH = 0x00080000;
        public const uint DM_PELSHEIGHT = 0x00100000;
        public const uint DM_DISPLAYFLAGS = 0x00200000;
        public const uint DM_DISPLAYFREQUENCY = 0x00400000;
        public const uint DM_ICMMETHOD = 0x00800000;
        public const uint DM_ICMINTENT = 0x01000000;
        public const uint DM_MEDIATYPE = 0x02000000;
        public const uint DM_DITHERTYPE = 0x04000000;
        public const uint DM_PANNINGWIDTH = 0x08000000;
        public const uint DM_PANNINGHEIGHT = 0x10000000;
        public const uint DM_DISPLAYFIXEDOUTPUT = 0x20000000;

        // orientation selections
        public const short DMORIENT_PORTRAIT = 1;
        public const short DMORIENT_LANDSCAPE = 2;

        // paper selections
        public const short DMPAPER_FIRST = DMPAPER_LETTER;
        public const short DMPAPER_LETTER = 1;              // Letter 8 1/2 x 11 in
        public const short DMPAPER_LETTERSMALL = 2;         // Letter Small 8 1/2 x 11 in
        public const short DMPAPER_TABLOID = 3;             // Tabloid 11 x 17 in
        public const short DMPAPER_LEDGER = 4;              // Ledger 17 x 11 in
        public const short DMPAPER_LEGAL = 5;               // Legal 8 1/2 x 14 in
        public const short DMPAPER_STATEMENT = 6;           // Statement 5 1/2 x 8 1/2 in
        public const short DMPAPER_EXECUTIVE = 7;           // Executive 7 1/4 x 10 1/2 in
        public const short DMPAPER_A3 = 8;                  // A3 297 x 420 mm
        public const short DMPAPER_A4 = 9;                  // A4 210 x 297 mm
        public const short DMPAPER_A4SMALL = 10;            // A4 Small 210 x 297 mm
        public const short DMPAPER_A5 = 11;                 // A5 148 x 210 mm
        public const short DMPAPER_B4 = 12;                 // B4 (JIS) 250 x 354
        public const short DMPAPER_B5 = 13;                 // B5 (JIS) 182 x 257 mm
        public const short DMPAPER_FOLIO = 14;              // Folio 8 1/2 x 13 in
        public const short DMPAPER_QUARTO = 15;             // Quarto 215 x 275 mm
        public const short DMPAPER_10X14 = 16;              // 10x14 in
        public const short DMPAPER_11X17 = 17;              // 11x17 in
        public const short DMPAPER_NOTE = 18;               // Note 8 1/2 x 11 in
        public const short DMPAPER_ENV_9 = 19;              // Envelope #9 3 7/8 x 8 7/8
        public const short DMPAPER_ENV_10 = 20;             // Envelope #10 4 1/8 x 9 1/2
        public const short DMPAPER_ENV_11 = 21;             // Envelope #11 4 1/2 x 10 3/8
        public const short DMPAPER_ENV_12 = 22;             // Envelope #12 4 \276 x 11
        public const short DMPAPER_ENV_14 = 23;             // Envelope #14 5 x 11 1/2
        public const short DMPAPER_CSHEET = 24;             // C size sheet
        public const short DMPAPER_DSHEET = 25;             // D size sheet
        public const short DMPAPER_ESHEET = 26;             // E size sheet
        public const short DMPAPER_ENV_DL = 27;             // Envelope DL 110 x 220mm
        public const short DMPAPER_ENV_C5 = 28;             // Envelope C5 162 x 229 mm
        public const short DMPAPER_ENV_C3 = 29;             // Envelope C3  324 x 458 mm
        public const short DMPAPER_ENV_C4 = 30;             // Envelope C4  229 x 324 mm
        public const short DMPAPER_ENV_C6 = 31;             // Envelope C6  114 x 162 mm
        public const short DMPAPER_ENV_C65 = 32;            // Envelope C65 114 x 229 mm
        public const short DMPAPER_ENV_B4 = 33;             // Envelope B4  250 x 353 mm
        public const short DMPAPER_ENV_B5 = 34;             // Envelope B5  176 x 250 mm
        public const short DMPAPER_ENV_B6 = 35;             // Envelope B6  176 x 125 mm
        public const short DMPAPER_ENV_ITALY = 36;          // Envelope 110 x 230 mm
        public const short DMPAPER_ENV_MONARCH = 37;        // Envelope Monarch 3.875 x 7.5 in
        public const short DMPAPER_ENV_PERSONAL = 38;       // 6 3/4 Envelope 3 5/8 x 6 1/2 in
        public const short DMPAPER_FANFOLD_US = 39;         // US Std Fanfold 14 7/8 x 11 in
        public const short DMPAPER_FANFOLD_STD_GERMAN = 40; // German Std Fanfold 8 1/2 x 12 in
        public const short DMPAPER_FANFOLD_LGL_GERMAN = 41; // German Legal Fanfold 8 1/2 x 13 in

        public const short DMPAPER_ISO_B4 = 42;                     // B4 (ISO) 250 x 353 mm
        public const short DMPAPER_JAPANESE_POSTCARD = 43;          // Japanese Postcard 100 x 148 mm
        public const short DMPAPER_9X11 = 44;                       // 9 x 11 in
        public const short DMPAPER_10X11 = 45;                      // 10 x 11 in
        public const short DMPAPER_15X11 = 46;                      // 15 x 11 in
        public const short DMPAPER_ENV_INVITE = 47;                 // Envelope Invite 220 x 220 mm
        public const short DMPAPER_RESERVED_48 = 48;                // RESERVED--DO NOT USE
        public const short DMPAPER_RESERVED_49 = 49;                // RESERVED--DO NOT USE
        public const short DMPAPER_LETTER_EXTRA = 50;               // Letter Extra 9 \275 x 12 in
        public const short DMPAPER_LEGAL_EXTRA = 51;                // Legal Extra 9 \275 x 15 in
        public const short DMPAPER_TABLOID_EXTRA = 52;              // Tabloid Extra 11.69 x 18 in
        public const short DMPAPER_A4_EXTRA = 53;                   // A4 Extra 9.27 x 12.69 in
        public const short DMPAPER_LETTER_TRANSVERSE = 54;          // Letter Transverse 8 \275 x 11 in
        public const short DMPAPER_A4_TRANSVERSE = 55;              // A4 Transverse 210 x 297 mm
        public const short DMPAPER_LETTER_EXTRA_TRANSVERSE = 56;    // Letter Extra Transverse 9\275 x 12 in
        public const short DMPAPER_A_PLUS = 57;                     // SuperA/SuperA/A4 227 x 356 mm
        public const short DMPAPER_B_PLUS = 58;                     // SuperB/SuperB/A3 305 x 487 mm
        public const short DMPAPER_LETTER_PLUS = 59;                // Letter Plus 8.5 x 12.69 in
        public const short DMPAPER_A4_PLUS = 60;                    // A4 Plus 210 x 330 mm
        public const short DMPAPER_A5_TRANSVERSE = 61;              // A5 Transverse 148 x 210 mm
        public const short DMPAPER_B5_TRANSVERSE = 62;              // B5 (JIS) Transverse 182 x 257 mm
        public const short DMPAPER_A3_EXTRA = 63;                   // A3 Extra 322 x 445 mm
        public const short DMPAPER_A5_EXTRA = 64;                   // A5 Extra 174 x 235 mm
        public const short DMPAPER_B5_EXTRA = 65;                   // B5 (ISO) Extra 201 x 276 mm
        public const short DMPAPER_A2 = 66;                         // A2 420 x 594 mm
        public const short DMPAPER_A3_TRANSVERSE = 67;              // A3 Transverse 297 x 420 mm
        public const short DMPAPER_A3_EXTRA_TRANSVERSE = 68;        // A3 Extra Transverse 322 x 445 mm

        public const short DMPAPER_DBL_JAPANESE_POSTCARD = 69;          // Japanese Double Postcard 200 x 148 mm
        public const short DMPAPER_A6 = 70;                             // A6 105 x 148 mm
        public const short DMPAPER_JENV_KAKU2 = 71;                     // Japanese Envelope Kaku #2
        public const short DMPAPER_JENV_KAKU3 = 72;                     // Japanese Envelope Kaku #3
        public const short DMPAPER_JENV_CHOU3 = 73;                     // Japanese Envelope Chou #3
        public const short DMPAPER_JENV_CHOU4 = 74;                     // Japanese Envelope Chou #4
        public const short DMPAPER_LETTER_ROTATED = 75;                 // Letter Rotated 11 x 8 1/2 11 in
        public const short DMPAPER_A3_ROTATED = 76;                     // A3 Rotated 420 x 297 mm
        public const short DMPAPER_A4_ROTATED = 77;                     // A4 Rotated 297 x 210 mm
        public const short DMPAPER_A5_ROTATED = 78;                     // A5 Rotated 210 x 148 mm
        public const short DMPAPER_B4_JIS_ROTATED = 79;                 // B4 (JIS) Rotated 364 x 257 mm
        public const short DMPAPER_B5_JIS_ROTATED = 80;                 // B5 (JIS) Rotated 257 x 182 mm
        public const short DMPAPER_JAPANESE_POSTCARD_ROTATED = 81;      // Japanese Postcard Rotated 148 x 100 mm
        public const short DMPAPER_DBL_JAPANESE_POSTCARD_ROTATED = 82;  // Double Japanese Postcard Rotated 148 x 200 mm
        public const short DMPAPER_A6_ROTATED = 83;                     // A6 Rotated 148 x 105 mm
        public const short DMPAPER_JENV_KAKU2_ROTATED = 84;             // Japanese Envelope Kaku #2 Rotated
        public const short DMPAPER_JENV_KAKU3_ROTATED = 85;             // Japanese Envelope Kaku #3 Rotated
        public const short DMPAPER_JENV_CHOU3_ROTATED = 86;             // Japanese Envelope Chou #3 Rotated
        public const short DMPAPER_JENV_CHOU4_ROTATED = 87;             // Japanese Envelope Chou #4 Rotated
        public const short DMPAPER_B6_JIS = 88;                         // B6 (JIS) 128 x 182 mm
        public const short DMPAPER_B6_JIS_ROTATED = 89;                 // B6 (JIS) Rotated 182 x 128 mm
        public const short DMPAPER_12X11 = 90;                          // 12 x 11 in
        public const short DMPAPER_JENV_YOU4 = 91;                      // Japanese Envelope You #4
        public const short DMPAPER_JENV_YOU4_ROTATED = 92;              // Japanese Envelope You #4 Rotated
        public const short DMPAPER_P16K = 93;                           // PRC 16K 146 x 215 mm
        public const short DMPAPER_P32K = 94;                           // PRC 32K 97 x 151 mm
        public const short DMPAPER_P32KBIG = 95;                        // PRC 32K(Big) 97 x 151 mm
        public const short DMPAPER_PENV_1 = 96;                         // PRC Envelope #1 102 x 165 mm
        public const short DMPAPER_PENV_2 = 97;                         // PRC Envelope #2 102 x 176 mm
        public const short DMPAPER_PENV_3 = 98;                         // PRC Envelope #3 125 x 176 mm
        public const short DMPAPER_PENV_4 = 99;                         // PRC Envelope #4 110 x 208 mm
        public const short DMPAPER_PENV_5 = 100;                        // PRC Envelope #5 110 x 220 mm
        public const short DMPAPER_PENV_6 = 101;                        // PRC Envelope #6 120 x 230 mm
        public const short DMPAPER_PENV_7 = 102;                        // PRC Envelope #7 160 x 230 mm
        public const short DMPAPER_PENV_8 = 103;                        // PRC Envelope #8 120 x 309 mm
        public const short DMPAPER_PENV_9 = 104;                        // PRC Envelope #9 229 x 324 mm
        public const short DMPAPER_PENV_10 = 105;                       // PRC Envelope #10 324 x 458 mm
        public const short DMPAPER_P16K_ROTATED = 106;                  // PRC 16K Rotated
        public const short DMPAPER_P32K_ROTATED = 107;                  // PRC 32K Rotated
        public const short DMPAPER_P32KBIG_ROTATED = 108;               // PRC 32K(Big) Rotated
        public const short DMPAPER_PENV_1_ROTATED = 109;                // PRC Envelope #1 Rotated 165 x 102 mm
        public const short DMPAPER_PENV_2_ROTATED = 110;                // PRC Envelope #2 Rotated 176 x 102 mm
        public const short DMPAPER_PENV_3_ROTATED = 111;                // PRC Envelope #3 Rotated 176 x 125 mm
        public const short DMPAPER_PENV_4_ROTATED = 112;                // PRC Envelope #4 Rotated 208 x 110 mm
        public const short DMPAPER_PENV_5_ROTATED = 113;                // PRC Envelope #5 Rotated 220 x 110 mm
        public const short DMPAPER_PENV_6_ROTATED = 114;                // PRC Envelope #6 Rotated 230 x 120 mm
        public const short DMPAPER_PENV_7_ROTATED = 115;                // PRC Envelope #7 Rotated 230 x 160 mm
        public const short DMPAPER_PENV_8_ROTATED = 116;                // PRC Envelope #8 Rotated 309 x 120 mm
        public const short DMPAPER_PENV_9_ROTATED = 117;                // PRC Envelope #9 Rotated 324 x 229 mm
        public const short DMPAPER_PENV_10_ROTATED = 118;               // PRC Envelope #10 Rotated 458 x 324 mm

        public static readonly short DMPAPER_LAST
            = _WIN32_WINNT >= _WIN32_WINNT_WIN2K ? DMPAPER_PENV_10_ROTATED
            : _WIN32_WINNT >= _WIN32_WINNT_NT4 ? DMPAPER_A3_EXTRA_TRANSVERSE
            : DMPAPER_FANFOLD_LGL_GERMAN;

        public const short DMPAPER_USER = 256;

        // DEVMODE dmDisplayOrientation specifiations
        public const uint DMDO_DEFAULT = 0;
        public const uint DMDO_90 = 1;
        public const uint DMDO_180 = 2;
        public const uint DMDO_270 = 3;

        // DEVMODE dmDisplayFixedOutput specifiations
        public const uint DMDFO_DEFAULT = 0;
        public const uint DMDFO_STRETCH = 1;
        public const uint DMDFO_CENTER = 2;

        // color enable/disable for color printers
        public const short DMCOLOR_MONOCHROME = 1;
        public const short DMCOLOR_COLOR = 2;

        // duplex enable
        public const short DMDUP_SIMPLEX = 1;
        public const short DMDUP_VERTICAL = 2;
        public const short DMDUP_HORIZONTAL = 3;

        // TrueType options
        public const short DMTT_BITMAP = 1;             // print TT fonts as graphics
        public const short DMTT_DOWNLOAD = 2;           // download TT fonts as soft fonts
        public const short DMTT_SUBDEV = 3;             // substitute device fonts for TT fonts
        public const short DMTT_DOWNLOAD_OUTLINE = 4;   // download TT fonts as outline soft fonts

        // Collation selections
        public const short DMCOLLATE_FALSE = 0;
        public const short DMCOLLATE_TRUE = 1;

        // DEVMODE dmDisplayFlags flags
        public const uint DM_INTERLACED = 0x00000002;
        public const uint DMDISPLAYFLAGS_TEXTMODE = 0x00000004;

        // dmNup , multiple logical page per physical page options
        public const uint DMNUP_SYSTEM = 1;
        public const uint DMNUP_ONEUP = 2;

        // ICM methods
        public const uint DMICMMETHOD_NONE = 1;     // ICM disabled
        public const uint DMICMMETHOD_SYSTEM = 2;   // ICM handled by system
        public const uint DMICMMETHOD_DRIVER = 3;   // ICM handled by driver
        public const uint DMICMMETHOD_DEVICE = 4;   // ICM handled by device

        public const uint DMICMMETHOD_USER = 256;   // Device-specific methods start here

        // ICM Intents
        public const uint DMICM_SATURATE = 1;           // Maximize color saturation
        public const uint DMICM_CONTRAST = 2;           // Maximize color contrast
        public const uint DMICM_COLORIMETRIC = 3;       // Use specific color metric
        public const uint DMICM_ABS_COLORIMETRIC = 4;   // Use specific color metric

        public const uint DMICM_USER = 256;             // Device-specific intents start here

        // Media types
        public const uint DMMEDIA_STANDARD = 1;     // Standard paper
        public const uint DMMEDIA_TRANSPARENCY = 2; // Transparency
        public const uint DMMEDIA_GLOSSY = 3;       // Glossy paper

        public const uint DMMEDIA_USER = 256;       // Device-specific media start here

        // Dither types
        public const uint DMDITHER_NONE = 1;            // No dithering
        public const uint DMDITHER_COARSE = 2;          // Dither with a coarse brush
        public const uint DMDITHER_FINE = 3;            // Dither with a fine brush
        public const uint DMDITHER_LINEART = 4;         // LineArt dithering
        public const uint DMDITHER_ERRORDIFFUSION = 5;  // LineArt dithering
        public const uint DMDITHER_RESERVED6 = 6;       // LineArt dithering
        public const uint DMDITHER_RESERVED7 = 7;       // LineArt dithering
        public const uint DMDITHER_RESERVED8 = 8;       // LineArt dithering
        public const uint DMDITHER_RESERVED9 = 9;       // LineArt dithering
        public const uint DMDITHER_GRAYSCALE = 10;      // Device does grayscaling

        public const uint DMDITHER_USER = 256;          // Device-specific dithers start here

        #endregion

        #region EnumDisplayDevices()

        public const uint DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x00000001;
        public const uint DISPLAY_DEVICE_MULTI_DRIVER = 0x00000002;
        public const uint DISPLAY_DEVICE_PRIMARY_DEVICE = 0x00000004;
        public const uint DISPLAY_DEVICE_MIRRORING_DRIVER = 0x00000008;
        public const uint DISPLAY_DEVICE_VGA_COMPATIBLE = 0x00000010;
        public const uint DISPLAY_DEVICE_REMOVABLE = 0x00000020;
        public const uint DISPLAY_DEVICE_MODESPRUNED = 0x08000000;
        public const uint DISPLAY_DEVICE_REMOTE = 0x04000000;
        public const uint DISPLAY_DEVICE_DISCONNECT = 0x02000000;

        // Child device state
        public const uint DISPLAY_DEVICE_ACTIVE = 0x00000001;
        public const uint DISPLAY_DEVICE_ATTACHED = 0x00000002;

        #endregion

        #region Window Messages

        public const uint WM_NULL = 0x0000;
        public const uint WM_CREATE = 0x0001;
        public const uint WM_DESTROY = 0x0002;
        public const uint WM_MOVE = 0x0003;
        public const uint WM_SIZE = 0x0005;
        public const uint WM_ACTIVATE = 0x0006;
        public const uint WM_SETFOCUS = 0x0007;
        public const uint WM_KILLFOCUS = 0x0008;
        public const uint WM_ENABLE = 0x000A;
        public const uint WM_SETREDRAW = 0x000B;
        public const uint WM_SETTEXT = 0x000C;
        public const uint WM_GETTEXT = 0x000D;
        public const uint WM_GETTEXTLENGTH = 0x000E;
        public const uint WM_PAINT = 0x000F;
        public const uint WM_CLOSE = 0x0010;
        public const uint WM_QUERYENDSESSION = 0x0011;
        public const uint WM_QUERYOPEN = 0x0013;
        public const uint WM_ENDSESSION = 0x0016;
        public const uint WM_QUIT = 0x0012;
        public const uint WM_ERASEBKGND = 0x0014;
        public const uint WM_SYSCOLORCHANGE = 0x0015;
        public const uint WM_SHOWWINDOW = 0x0018;
        public const uint WM_WININICHANGE = 0x001A;
        public const uint WM_SETTINGCHANGE = WM_WININICHANGE;
        public const uint WM_DEVMODECHANGE = 0x001B;
        public const uint WM_ACTIVATEAPP = 0x001C;
        public const uint WM_FONTCHANGE = 0x001D;
        public const uint WM_TIMECHANGE = 0x001E;
        public const uint WM_CANCELMODE = 0x001F;
        public const uint WM_SETCURSOR = 0x0020;
        public const uint WM_MOUSEACTIVATE = 0x0021;
        public const uint WM_CHILDACTIVATE = 0x0022;
        public const uint WM_QUEUESYNC = 0x0023;
        public const uint WM_GETMINMAXINFO = 0x0024;
        public const uint WM_PAINTICON = 0x0026;
        public const uint WM_ICONERASEBKGND = 0x0027;
        public const uint WM_NEXTDLGCTL = 0x0028;
        public const uint WM_SPOOLERSTATUS = 0x002A;
        public const uint WM_DRAWITEM = 0x002B;
        public const uint WM_MEASUREITEM = 0x002C;
        public const uint WM_DELETEITEM = 0x002D;
        public const uint WM_VKEYTOITEM = 0x002E;
        public const uint WM_CHARTOITEM = 0x002F;
        public const uint WM_SETFONT = 0x0030;
        public const uint WM_GETFONT = 0x0031;
        public const uint WM_SETHOTKEY = 0x0032;
        public const uint WM_GETHOTKEY = 0x0033;
        public const uint WM_QUERYDRAGICON = 0x0037;
        public const uint WM_COMPAREITEM = 0x0039;
        public const uint WM_GETOBJECT = 0x003D;
        public const uint WM_COMPACTING = 0x0041;
        public const uint WM_COMMNOTIFY = 0x0044;// no longer suported
        public const uint WM_WINDOWPOSCHANGING = 0x0046;
        public const uint WM_WINDOWPOSCHANGED = 0x0047;
        public const uint WM_POWER = 0x0048;
        public const uint WM_COPYDATA = 0x004A;
        public const uint WM_CANCELJOURNAL = 0x004B;
        public const uint WM_NOTIFY = 0x004E;
        public const uint WM_INPUTLANGCHANGEREQUEST = 0x0050;
        public const uint WM_INPUTLANGCHANGE = 0x0051;
        public const uint WM_TCARD = 0x0052;
        public const uint WM_HELP = 0x0053;
        public const uint WM_USERCHANGED = 0x0054;
        public const uint WM_NOTIFYFORMAT = 0x0055;
        public const uint WM_CONTEXTMENU = 0x007B;
        public const uint WM_STYLECHANGING = 0x007C;
        public const uint WM_STYLECHANGED = 0x007D;
        public const uint WM_DISPLAYCHANGE = 0x007E;
        public const uint WM_GETICON = 0x007F;
        public const uint WM_SETICON = 0x0080;
        public const uint WM_NCCREATE = 0x0081;
        public const uint WM_NCDESTROY = 0x0082;
        public const uint WM_NCCALCSIZE = 0x0083;
        public const uint WM_NCHITTEST = 0x0084;
        public const uint WM_NCPAINT = 0x0085;
        public const uint WM_NCACTIVATE = 0x0086;
        public const uint WM_GETDLGCODE = 0x0087;
        public const uint WM_SYNCPAINT = 0x0088;
        public const uint WM_NCMOUSEMOVE = 0x00A0;
        public const uint WM_NCLBUTTONDOWN = 0x00A1;
        public const uint WM_NCLBUTTONUP = 0x00A2;
        public const uint WM_NCLBUTTONDBLCLK = 0x00A3;
        public const uint WM_NCRBUTTONDOWN = 0x00A4;
        public const uint WM_NCRBUTTONUP = 0x00A5;
        public const uint WM_NCRBUTTONDBLCLK = 0x00A6;
        public const uint WM_NCMBUTTONDOWN = 0x00A7;
        public const uint WM_NCMBUTTONUP = 0x00A8;
        public const uint WM_NCMBUTTONDBLCLK = 0x00A9;
        public const uint WM_NCXBUTTONDOWN = 0x00AB;
        public const uint WM_NCXBUTTONUP = 0x00AC;
        public const uint WM_NCXBUTTONDBLCLK = 0x00AD;
        public const uint WM_INPUT_DEVICE_CHANGE = 0x00FE;
        public const uint WM_INPUT = 0x00FF;
        public const uint WM_KEYFIRST = 0x0100;
        public const uint WM_KEYDOWN = 0x0100;
        public const uint WM_KEYUP = 0x0101;
        public const uint WM_CHAR = 0x0102;
        public const uint WM_DEADCHAR = 0x0103;
        public const uint WM_SYSKEYDOWN = 0x0104;
        public const uint WM_SYSKEYUP = 0x0105;
        public const uint WM_SYSCHAR = 0x0106;
        public const uint WM_SYSDEADCHAR = 0x0107;
        public const uint WM_UNICHAR = 0x0109;

        public static readonly uint WM_KEYLAST
            = _WIN32_WINNT >= _WIN32_WINNT_WINXP ? 0x0109
            : 0x0108U;

        public const uint WM_IME_STARTCOMPOSITION = 0x010D;
        public const uint WM_IME_ENDCOMPOSITION = 0x010E;
        public const uint WM_IME_COMPOSITION = 0x010F;
        public const uint WM_IME_KEYLAST = 0x010F;
        public const uint WM_INITDIALOG = 0x0110;
        public const uint WM_COMMAND = 0x0111;
        public const uint WM_SYSCOMMAND = 0x0112;
        public const uint WM_TIMER = 0x0113;
        public const uint WM_HSCROLL = 0x0114;
        public const uint WM_VSCROLL = 0x0115;
        public const uint WM_INITMENU = 0x0116;
        public const uint WM_INITMENUPOPUP = 0x0117;
        public const uint WM_GESTURE = 0x0119;
        public const uint WM_GESTURENOTIFY = 0x011A;
        public const uint WM_MENUSELECT = 0x011F;
        public const uint WM_MENUCHAR = 0x0120;
        public const uint WM_ENTERIDLE = 0x0121;
        public const uint WM_MENURBUTTONUP = 0x0122;
        public const uint WM_MENUDRAG = 0x0123;
        public const uint WM_MENUGETOBJECT = 0x0124;
        public const uint WM_UNINITMENUPOPUP = 0x0125;
        public const uint WM_MENUCOMMAND = 0x0126;
        public const uint WM_CHANGEUISTATE = 0x0127;
        public const uint WM_UPDATEUISTATE = 0x0128;
        public const uint WM_QUERYUISTATE = 0x0129;
        public const uint WM_CTLCOLORMSGBOX = 0x0132;
        public const uint WM_CTLCOLOREDIT = 0x0133;
        public const uint WM_CTLCOLORLISTBOX = 0x0134;
        public const uint WM_CTLCOLORBTN = 0x0135;
        public const uint WM_CTLCOLORDLG = 0x0136;
        public const uint WM_CTLCOLORSCROLLBAR = 0x0137;
        public const uint WM_CTLCOLORSTATIC = 0x0138;
        public const uint WM_MOUSEFIRST = 0x0200;
        public const uint WM_MOUSEMOVE = 0x0200;
        public const uint WM_LBUTTONDOWN = 0x0201;
        public const uint WM_LBUTTONUP = 0x0202;
        public const uint WM_LBUTTONDBLCLK = 0x0203;
        public const uint WM_RBUTTONDOWN = 0x0204;
        public const uint WM_RBUTTONUP = 0x0205;
        public const uint WM_RBUTTONDBLCLK = 0x0206;
        public const uint WM_MBUTTONDOWN = 0x0207;
        public const uint WM_MBUTTONUP = 0x0208;
        public const uint WM_MBUTTONDBLCLK = 0x0209;
        public const uint WM_MOUSEWHEEL = 0x020A;
        public const uint WM_XBUTTONDOWN = 0x020B;
        public const uint WM_XBUTTONUP = 0x020C;
        public const uint WM_XBUTTONDBLCLK = 0x020D;
        public const uint WM_MOUSEHWHEEL = 0x020E;

        public static readonly uint WM_MOUSELAST
            = _WIN32_WINNT >= _WIN32_WINNT_WIN6 ? 0x020E
            : _WIN32_WINNT >= _WIN32_WINNT_WIN2K ? 0x020D
            : _WIN32_WINNT >= _WIN32_WINNT_NT4 ? 0x020A
            : 0x0209U;

        public const uint WM_PARENTNOTIFY = 0x0210;
        public const uint WM_ENTERMENULOOP = 0x0211;
        public const uint WM_EXITMENULOOP = 0x0212;
        public const uint WM_NEXTMENU = 0x0213;
        public const uint WM_SIZING = 0x0214;
        public const uint WM_CAPTURECHANGED = 0x0215;
        public const uint WM_MOVING = 0x0216;
        public const uint WM_POWERBROADCAST = 0x0218;
        public const uint WM_DEVICECHANGE = 0x0219;
        public const uint WM_MDICREATE = 0x0220;
        public const uint WM_MDIDESTROY = 0x0221;
        public const uint WM_MDIACTIVATE = 0x0222;
        public const uint WM_MDIRESTORE = 0x0223;
        public const uint WM_MDINEXT = 0x0224;
        public const uint WM_MDIMAXIMIZE = 0x0225;
        public const uint WM_MDITILE = 0x0226;
        public const uint WM_MDICASCADE = 0x0227;
        public const uint WM_MDIICONARRANGE = 0x0228;
        public const uint WM_MDIGETACTIVE = 0x0229;
        public const uint WM_MDISETMENU = 0x0230;
        public const uint WM_ENTERSIZEMOVE = 0x0231;
        public const uint WM_EXITSIZEMOVE = 0x0232;
        public const uint WM_DROPFILES = 0x0233;
        public const uint WM_MDIREFRESHMENU = 0x0234;
        public const uint WM_POINTERDEVICECHANGE = 0x238;
        public const uint WM_POINTERDEVICEINRANGE = 0x239;
        public const uint WM_POINTERDEVICEOUTOFRANGE = 0x23A;
        public const uint WM_TOUCH = 0x0240;
        public const uint WM_NCPOINTERUPDATE = 0x0241;
        public const uint WM_NCPOINTERDOWN = 0x0242;
        public const uint WM_NCPOINTERUP = 0x0243;
        public const uint WM_POINTERUPDATE = 0x0245;
        public const uint WM_POINTERDOWN = 0x0246;
        public const uint WM_POINTERUP = 0x0247;
        public const uint WM_POINTERENTER = 0x0249;
        public const uint WM_POINTERLEAVE = 0x024A;
        public const uint WM_POINTERACTIVATE = 0x024B;
        public const uint WM_POINTERCAPTURECHANGED = 0x024C;
        public const uint WM_TOUCHHITTESTING = 0x024D;
        public const uint WM_POINTERWHEEL = 0x024E;
        public const uint WM_POINTERHWHEEL = 0x024F;
        public const uint WM_IME_SETCONTEXT = 0x0281;
        public const uint WM_IME_NOTIFY = 0x0282;
        public const uint WM_IME_CONTROL = 0x0283;
        public const uint WM_IME_COMPOSITIONFULL = 0x0284;
        public const uint WM_IME_SELECT = 0x0285;
        public const uint WM_IME_CHAR = 0x0286;
        public const uint WM_IME_REQUEST = 0x0288;
        public const uint WM_IME_KEYDOWN = 0x0290;
        public const uint WM_IME_KEYUP = 0x0291;
        public const uint WM_MOUSEHOVER = 0x02A1;
        public const uint WM_MOUSELEAVE = 0x02A3;
        public const uint WM_NCMOUSEHOVER = 0x02A0;
        public const uint WM_NCMOUSELEAVE = 0x02A2;
        public const uint WM_WTSSESSION_CHANGE = 0x02B1;
        public const uint WM_TABLET_FIRST = 0x02c0;
        public const uint WM_TABLET_LAST = 0x02df;
        public const uint WM_DPICHANGED = 0x02E0;
        public const uint WM_CUT = 0x0300;
        public const uint WM_COPY = 0x0301;
        public const uint WM_PASTE = 0x0302;
        public const uint WM_CLEAR = 0x0303;
        public const uint WM_UNDO = 0x0304;
        public const uint WM_RENDERFORMAT = 0x0305;
        public const uint WM_RENDERALLFORMATS = 0x0306;
        public const uint WM_DESTROYCLIPBOARD = 0x0307;
        public const uint WM_DRAWCLIPBOARD = 0x0308;
        public const uint WM_PAINTCLIPBOARD = 0x0309;
        public const uint WM_VSCROLLCLIPBOARD = 0x030A;
        public const uint WM_SIZECLIPBOARD = 0x030B;
        public const uint WM_ASKCBFORMATNAME = 0x030C;
        public const uint WM_CHANGECBCHAIN = 0x030D;
        public const uint WM_HSCROLLCLIPBOARD = 0x030E;
        public const uint WM_QUERYNEWPALETTE = 0x030F;
        public const uint WM_PALETTEISCHANGING = 0x0310;
        public const uint WM_PALETTECHANGED = 0x0311;
        public const uint WM_HOTKEY = 0x0312;
        public const uint WM_PRINT = 0x0317;
        public const uint WM_PRINTCLIENT = 0x0318;
        public const uint WM_APPCOMMAND = 0x0319;
        public const uint WM_THEMECHANGED = 0x031A;
        public const uint WM_CLIPBOARDUPDATE = 0x031D;
        public const uint WM_DWMCOMPOSITIONCHANGED = 0x031E;
        public const uint WM_DWMNCRENDERINGCHANGED = 0x031F;
        public const uint WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;
        public const uint WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321;
        public const uint WM_DWMSENDICONICTHUMBNAIL = 0x0323;
        public const uint WM_DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326;
        public const uint WM_GETTITLEBARINFOEX = 0x033F;
        public const uint WM_HANDHELDFIRST = 0x0358;
        public const uint WM_HANDHELDLAST = 0x035F;
        public const uint WM_AFXFIRST = 0x0360;
        public const uint WM_AFXLAST = 0x037F;
        public const uint WM_PENWINFIRST = 0x0380;
        public const uint WM_PENWINLAST = 0x038F;
        public const uint WM_APP = 0x8000;
        public const uint WM_USER = 0x0400;

        #endregion

        public const uint QS_KEY = 0x0001;
        public const uint QS_MOUSEMOVE = 0x0002;
        public const uint QS_MOUSEBUTTON = 0x0004;
        public const uint QS_POSTMESSAGE = 0x0008;
        public const uint QS_TIMER = 0x0010;
        public const uint QS_PAINT = 0x0020;
        public const uint QS_SENDMESSAGE = 0x0040;
        public const uint QS_HOTKEY = 0x0080;
        public const uint QS_ALLPOSTMESSAGE = 0x0100;
        public const uint QS_RAWINPUT = 0x0400;

        public const uint QS_MOUSE = QS_MOUSEMOVE | QS_MOUSEBUTTON;
        public const uint QS_INPUT = QS_MOUSE | QS_KEY | QS_RAWINPUT;
        public const uint QS_ALLINPUT = QS_INPUT | QS_POSTMESSAGE | QS_TIMER | QS_PAINT | QS_HOTKEY | QS_SENDMESSAGE;
        public const uint QS_ALLEVENTS = QS_INPUT | QS_POSTMESSAGE | QS_TIMER | QS_PAINT | QS_HOTKEY;

        #region PeekMessage()

        public const uint PM_NOREMOVE = 0x0000;
        public const uint PM_REMOVE = 0x0001;
        public const uint PM_NOYIELD = 0x0002;

        public const uint PM_QS_INPUT = QS_INPUT << 16;
        public const uint PM_QS_PAINT = QS_PAINT << 16;
        public const uint PM_QS_POSTMESSAGE = (QS_POSTMESSAGE | QS_HOTKEY | QS_TIMER) << 16;
        public const uint PM_QS_SENDMESSAGE = QS_SENDMESSAGE << 16;

        #endregion

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessage(
            out MSG lpMsg,
            IntPtr hWnd,// NULL
            uint wMsgFilterMin,
            uint wMsgFilterMax,
            uint wRemoveMsg);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessage(
            out MSG lpMsg,
            HandleRef hWnd,
            uint wMsgFilterMin,
            uint wMsgFilterMax,
            uint wRemoveMsg);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplayDevices(
            string lpDevice,
            uint iDevNum,
            ref DISPLAY_DEVICE lpDisplayDevice,
            uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, uint iModeNum, ref DEVMODE lpDevMode);

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettingsEx(
            string lpszDeviceName,
            ref DEVMODE lpDevMode,
            IntPtr hwnd,// Reserved; must be NULL.
            uint dwflags,
            IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(HandleRef hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLongPtr(HandleRef hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr GetMenu(HandleRef hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AdjustWindowRectEx(ref RECT lpRect, uint dwStyle, bool bMenu, uint dwExStyle);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(
            HandleRef hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(HandleRef hWnd, int nCmdShow);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public Point pt;
        //public uint lPrivate;
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
        public uint dmDisplayOrientation;
        public uint dmDisplayFixedOutput;
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
        public uint dmDisplayFlags;
        [FieldOffset(0)]
        public uint dmNup;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public ushort dmSpecVersion;
        public ushort dmDriverVersion;
        public ushort dmSize;
        public ushort dmDriverExtra;
        public uint dmFields;
        public DUMMYUNIONNAME DUMMYUNIONNAME;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public ushort dmLogPixels;
        public uint dmBitsPerPel;
        public uint dmPelsWidth;
        public uint dmPelsHeight;
        public DUMMYUNIONNAME2 DUMMYUNIONNAME2;
        public uint dmDisplayFrequency;
        public uint dmICMMethod;
        public uint dmICMIntent;
        public uint dmMediaType;
        public uint dmDitherType;
        public uint dmReserved1;
        public uint dmReserved2;
        public uint dmPanningWidth;
        public uint dmPanningHeight;
    }

    #endregion

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct DISPLAY_DEVICE
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        [MarshalAs(UnmanagedType.U4)]
        public uint StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;        // 指定矩形左上角的x坐标
        public int top;         // 指定矩形左上角的y坐标
        public int right;       // 指定矩形右下角的x坐标
        public int bottom;      // 指定矩形右下角的y坐标

        /*
        public static readonly RECT Empty;

        public int X { get => left; set { right -= left - value; left = value; } }
        public int Y { get => top; set { bottom -= top - value; top = value; } }
        public int Width { get => right - left; set => right = value + left; }
        public int Height { get => bottom - top; set => bottom = value + top; }

        public Point Location { get => new Point(left, top); set { X = value.X; Y = value.Y; } }
        public Size Size { get => new Size(Width, Height); set { Width = value.Width; Height = value.Height; } }

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public RECT(Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

        public static implicit operator Rectangle(RECT r) => new Rectangle(r.left, r.top, r.Width, r.Height);
        public static implicit operator RECT(Rectangle r) => new RECT(r);

        public static bool operator ==(RECT r1, RECT r2) => r1.Equals(r2);
        public static bool operator !=(RECT r1, RECT r2) => !r1.Equals(r2);

        public bool Equals(RECT r) => r.left == left && r.top == top && r.right == right && r.bottom == bottom;
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
                , left, top, right, bottom);
        */
    }
}

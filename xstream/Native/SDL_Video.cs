using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Xstream
{
    partial class Native
    {
        public const uint SDL_PIXELFORMAT_UNKNOWN = 0;
        public static readonly uint SDL_PIXELFORMAT_INDEX1LSB
            = SDL_Define_PixelFormat(SDL_PixelTypes.INDEX1, SDL_BitmapOrder.B4321, 0, 1, 0);
        public static readonly uint SDL_PIXELFORMAT_INDEX1MSB
            = SDL_Define_PixelFormat(SDL_PixelTypes.INDEX1, SDL_BitmapOrder.B1234, 0, 1, 0);
        public static readonly uint SDL_PIXELFORMAT_INDEX4LSB
            = SDL_Define_PixelFormat(SDL_PixelTypes.INDEX4, SDL_BitmapOrder.B4321, 0, 4, 0);
        public static readonly uint SDL_PIXELFORMAT_INDEX4MSB
            = SDL_Define_PixelFormat(SDL_PixelTypes.INDEX4, SDL_BitmapOrder.B1234, 0, 4, 0);
        public static readonly uint SDL_PIXELFORMAT_INDEX8
            = SDL_Define_PixelFormat(SDL_PixelTypes.INDEX8, 0, 0, 8, 1);
        public static readonly uint SDL_PIXELFORMAT_RGB332
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED8, SDL_PackedOrder.XRGB, SDL_PackedLayout.L332, 8, 1);
        public static readonly uint SDL_PIXELFORMAT_RGB444
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED16, SDL_PackedOrder.XRGB, SDL_PackedLayout.L4444, 12, 2);
        public static readonly uint SDL_PIXELFORMAT_RGB555
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED16, SDL_PackedOrder.XRGB, SDL_PackedLayout.L1555, 15, 2);
        public static readonly uint SDL_PIXELFORMAT_BGR555
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED16, SDL_PackedOrder.XBGR, SDL_PackedLayout.L1555, 15, 2);
        public static readonly uint SDL_PIXELFORMAT_ARGB4444
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED16, SDL_PackedOrder.ARGB, SDL_PackedLayout.L4444, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_RGBA4444
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED16, SDL_PackedOrder.RGBA, SDL_PackedLayout.L4444, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_ABGR4444
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED16, SDL_PackedOrder.ABGR, SDL_PackedLayout.L4444, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_BGRA4444
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED16, SDL_PackedOrder.BGRA, SDL_PackedLayout.L4444, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_ARGB1555
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED16, SDL_PackedOrder.ARGB, SDL_PackedLayout.L1555, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_RGBA5551
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED16, SDL_PackedOrder.RGBA, SDL_PackedLayout.L5551, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_ABGR1555
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED16, SDL_PackedOrder.ABGR, SDL_PackedLayout.L1555, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_BGRA5551
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED16, SDL_PackedOrder.BGRA, SDL_PackedLayout.L5551, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_RGB565
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED16, SDL_PackedOrder.XRGB, SDL_PackedLayout.L565, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_BGR565
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED16, SDL_PackedOrder.XBGR, SDL_PackedLayout.L565, 16, 2);
        public static readonly uint SDL_PIXELFORMAT_RGB24
            = SDL_Define_PixelFormat(SDL_PixelTypes.ARRAYU8, SDL_ArrayOrder.RGB, 0, 24, 3);
        public static readonly uint SDL_PIXELFORMAT_BGR24
            = SDL_Define_PixelFormat(SDL_PixelTypes.ARRAYU8, SDL_ArrayOrder.BGR, 0, 24, 3);
        public static readonly uint SDL_PIXELFORMAT_RGB888
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED32, SDL_PackedOrder.XRGB, SDL_PackedLayout.L8888, 24, 4);
        public static readonly uint SDL_PIXELFORMAT_RGBX8888
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED32, SDL_PackedOrder.RGBX, SDL_PackedLayout.L8888, 24, 4);
        public static readonly uint SDL_PIXELFORMAT_BGR888
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED32, SDL_PackedOrder.XBGR, SDL_PackedLayout.L8888, 24, 4);
        public static readonly uint SDL_PIXELFORMAT_BGRX8888
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED32, SDL_PackedOrder.BGRX, SDL_PackedLayout.L8888, 24, 4);
        public static readonly uint SDL_PIXELFORMAT_ARGB8888
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED32, SDL_PackedOrder.ARGB, SDL_PackedLayout.L8888, 32, 4);
        public static readonly uint SDL_PIXELFORMAT_RGBA8888
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED32, SDL_PackedOrder.RGBA, SDL_PackedLayout.L8888, 32, 4);
        public static readonly uint SDL_PIXELFORMAT_ABGR8888
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED32, SDL_PackedOrder.ABGR, SDL_PackedLayout.L8888, 32, 4);
        public static readonly uint SDL_PIXELFORMAT_BGRA8888
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED32, SDL_PackedOrder.BGRA, SDL_PackedLayout.L8888, 32, 4);
        public static readonly uint SDL_PIXELFORMAT_ARGB2101010
            = SDL_Define_PixelFormat(SDL_PixelTypes.PACKED32, SDL_PackedOrder.ARGB, SDL_PackedLayout.L2101010, 32, 4);
        public static readonly uint SDL_PIXELFORMAT_YV12 = MakeFourCC('Y', 'V', '1', '2');// Y + V + U  (3 planes)
        public static readonly uint SDL_PIXELFORMAT_IYUV = MakeFourCC('I', 'Y', 'U', 'V');// Y + U + V  (3 planes)
        public static readonly uint SDL_PIXELFORMAT_YUY2 = MakeFourCC('Y', 'U', 'Y', '2');// Y0+U0+Y1+V0 (1 plane)
        public static readonly uint SDL_PIXELFORMAT_UYVY = MakeFourCC('U', 'Y', 'V', 'Y');// U0+Y0+V0+Y1 (1 plane)
        public static readonly uint SDL_PIXELFORMAT_YVYU = MakeFourCC('Y', 'V', 'Y', 'U');// Y0+V0+Y1+U0 (1 plane)

        public const uint SDL_WINDOW_FULLSCREEN = 0x00000001;       // fullscreen window
        public const uint SDL_WINDOW_OPENGL = 0x00000002;           // window usable with OpenGL context
        public const uint SDL_WINDOW_SHOWN = 0x00000004;            // window is visible
        public const uint SDL_WINDOW_HIDDEN = 0x00000008;           // window is not visible
        public const uint SDL_WINDOW_BORDERLESS = 0x00000010;       // no window decoration
        public const uint SDL_WINDOW_RESIZABLE = 0x00000020;        // window can be resized
        public const uint SDL_WINDOW_MINIMIZED = 0x00000040;        // window is minimized
        public const uint SDL_WINDOW_MAXIMIZED = 0x00000080;        // window is maximized
        public const uint SDL_WINDOW_INPUT_GRABBED = 0x00000100;    // window has grabbed input focus
        public const uint SDL_WINDOW_INPUT_FOCUS = 0x00000200;      // window has input focus
        public const uint SDL_WINDOW_MOUSE_FOCUS = 0x00000400;      // window has mouse focus
        public const uint SDL_WINDOW_FULLSCREEN_DESKTOP = SDL_WINDOW_FULLSCREEN | 0x00001000;
        public const uint SDL_WINDOW_FOREIGN = 0x00000800;          // window not created by SDL

        // Used to indicate that you don't care what the window position is.
        public static readonly uint SDL_WINDOWPOS_UNDEFINED = SDL_WindowPos_Undefined_Display(0);
        // Used to indicate that the window position should be centered.
        public static readonly uint SDL_WINDOWPOS_CENTERED = SDL_WindowPos_Centered_Display(0);

        static Exception SDL_UninitializedVideo // 像属性一样的方法（便于书写），本质上还是方法
            => new NativeException("Video subsystem has not been initialized");
        static SDL_VideoDevice _video;

        static unsafe bool Check_Window_Magic(SDL_Window window, out Exception retval)
        {
            if (_video == null)
            {
                retval = SDL_UninitializedVideo;
                return true;
            }

            void* magic = _video.FixedMagic.AddrOfPinnedObject().ToPointer();
            if (window == null || window.magic != magic)
            {
                retval = new NativeException("Invalid window");
                return true;
            }

            retval = null;
            return false;
        }

        static unsafe bool Check_Display_Index(int displayIndex, out Exception retval)
        {
            if (_video == null)
            {
                retval = SDL_UninitializedVideo;
                return true;
            }

            if (displayIndex < 0 || displayIndex >= _video.num_displays)
            {
                retval = new NativeException("displayIndex must be in the range 0 - {0}"
                    , _video.num_displays - 1);
                return true;
            }

            retval = null;
            return false;
        }

        #region add display

        static int SDL_AddVideoDisplay(SDL_VideoDisplay display)
        {
            SDL_VideoDisplay[] displays;
            int index;

            displays = Resize(_video.displays, _video.num_displays + 1);

            index = _video.num_displays++;
            displays[index] = display;
            displays[index].device = _video;
            _video.displays = displays;

            if (display.name == null)
            {
                displays[index].name = index.ToString();
            }

            return index;
        }

        static int CompareModes(SDL_DisplayMode a, SDL_DisplayMode b)
        {
            if (a.w != b.w)
            {
                return b.w - a.w;
            }
            if (a.h != b.h)
            {
                return b.h - a.h;
            }
            if (SDL_BitsPerPixel(a.format) != SDL_BitsPerPixel(b.format))
            {
                return (int)(SDL_BitsPerPixel(b.format) - SDL_BitsPerPixel(a.format));
            }
            if (SDL_PixelLayout(a.format) != SDL_PixelLayout(b.format))
            {
                return (int)(SDL_PixelLayout(b.format) - SDL_PixelLayout(a.format));
            }
            if (a.refresh_rate != b.refresh_rate)
            {
                return b.refresh_rate - a.refresh_rate;// 降序排列
            }
            return 0;
        }

        static int CompareMemory(SDL_DisplayMode a, SDL_DisplayMode b)
        {
            if (a.format != b.format)
            {
                return (int)(a.format - b.format);
            }
            if (a.w != b.w)
            {
                return a.w - b.w;
            }
            if (a.h != b.h)
            {
                return a.h - b.h;
            }
            if (a.refresh_rate != b.refresh_rate)
            {
                return a.refresh_rate - b.refresh_rate;
            }
            if (_video.CompareMemory != null)
            {
                return _video.CompareMemory(a.driverdata, b.driverdata);
            }
            return 0;
        }

        static bool SDL_AddDisplayMode(SDL_VideoDisplay display, SDL_DisplayMode mode)
        {
            SDL_DisplayMode[] modes;
            int i, nmodes;

            // Make sure we don't already have the mode in the list
            modes = display.display_modes;
            nmodes = display.num_display_modes;
            for (i = nmodes; CBool(i--);)
            {
                if (CompareMemory(mode, modes[i]) == 0)
                {
                    return false;
                }
            }

            // Go ahead and add the new mode
            if (nmodes == display.max_display_modes)
            {
                modes = Resize(display.display_modes, display.max_display_modes + 32);
                display.display_modes = modes;
                display.max_display_modes += 32;
            }
            modes[nmodes] = mode;
            display.num_display_modes++;

            // Re-sort video modes
            Array.Sort(display.display_modes, 0, display.num_display_modes
                , Comparer<SDL_DisplayMode>.Create(CompareModes));

            return true;
        }

        #endregion

        static Rectangle SDL_GetDisplayBounds(int displayIndex)
        {
            if (Check_Display_Index(displayIndex, out Exception e)) throw e;

            SDL_VideoDisplay display = _video.displays[displayIndex];
            if (_video.GetDisplayBounds != null)
            {
                return _video.GetDisplayBounds(_video, display);
            }

            // Assume that the displays are left to right
            Rectangle rect;
            if (displayIndex == 0)
            {
                rect = Rectangle.Empty;
                rect.X = 0;
                rect.Y = 0;
            }
            else
            {
                rect = SDL_GetDisplayBounds(displayIndex - 1);
                rect.X += rect.Width;// 默认多屏向右拓展？
            }
            rect.Width = display.current_mode.w;
            rect.Height = display.current_mode.h;
            return rect;
        }

        static unsafe int SDL_GetWindowDisplayIndex(SDL_Window window)
        {
            int displayIndex;
            int i, dist;
            int closest = -1;
            int closest_dist = 0x7FFFFFFF;
            Point center = Point.Empty;
            Point delta = Point.Empty;
            Rectangle rect;

            if (Check_Window_Magic(window, out Exception e)) throw e;

            if (SDL_WindowPos_IsUndefined(window.x) ||
                SDL_WindowPos_IsCentered(window.x))
            {
                displayIndex = window.x & 0xFFFF;
                if (displayIndex >= _video.num_displays)
                {
                    displayIndex = 0;
                }
                return displayIndex;
            }
            if (SDL_WindowPos_IsUndefined(window.y) ||
                SDL_WindowPos_IsCentered(window.y))
            {
                displayIndex = window.y & 0xFFFF;
                if (displayIndex >= _video.num_displays)
                {
                    displayIndex = 0;
                }
                return displayIndex;
            }

            // Find the display containing the window
            for (i = 0; i < _video.num_displays; ++i)
            {
                SDL_VideoDisplay display = _video.displays[i];

                if (display.fullscreen_window == window)
                {
                    return i;
                }
            }
            center.X = window.x + window.w / 2;
            center.Y = window.y + window.h / 2;
            for (i = 0; i < _video.num_displays; ++i)
            {
                rect = SDL_GetDisplayBounds(i);
                if (SDL_EnclosePoints(&center, 1, &rect, null))
                {
                    return i;
                }

                // 取两个中心点距离最小的那个
                delta.X = center.X - (rect.X + rect.Width / 2);
                delta.Y = center.Y - (rect.Y + rect.Width / 2);
                dist = delta.X * delta.X + delta.Y * delta.Y;// 确保无负数
                if (dist < closest_dist)
                {
                    closest = i;
                    closest_dist = dist;
                }
            }
            if (closest < 0)
            {
                throw new NativeException("Couldn't find any displays");
            }
            return closest;
        }

        static SDL_VideoDisplay SDL_GetDisplayForWindow(SDL_Window window)
        {
            int displayIndex = SDL_GetWindowDisplayIndex(window);
            if (displayIndex >= 0)
            {
                return _video.displays[displayIndex];
            }
            else
            {
                return null;
            }
        }

        static int SDL_GetNumDisplayModesForDisplay(SDL_VideoDisplay display)
        {
            if (!CBool(display.num_display_modes) && CBool(_video.GetDisplayModes))
            {
                _video.GetDisplayModes(_video, display);
                Array.Sort(display.display_modes, 0, display.num_display_modes
                    , Comparer<SDL_DisplayMode>.Create(CompareModes));
            }
            return display.num_display_modes;
        }

        static SDL_DisplayMode SDL_GetClosestDisplayModeForDisplay(
            SDL_VideoDisplay display,
            SDL_DisplayMode mode,
            SDL_DisplayMode closest)
        {
            uint target_format;
            int target_refresh_rate;
            int i;
            SDL_DisplayMode current, match;

            if (CBool(mode) || CBool(closest))
            {
                throw new NativeException("Missing desired mode or closest mode parameter");
            }

            // Default to the desktop format
            if (CBool(mode.format))
            {
                target_format = mode.format;
            }
            else
            {
                target_format = display.desktop_mode.format;
            }

            // Default to the desktop refresh rate
            if (CBool(mode.refresh_rate))
            {
                target_refresh_rate = mode.refresh_rate;
            }
            else
            {
                target_refresh_rate = display.desktop_mode.refresh_rate;
            }

            match = null;
            for (i = 0; i < SDL_GetNumDisplayModesForDisplay(display); ++i)
            {
                current = display.display_modes[i];

                if (CBool(current.w) && (current.w < mode.w))
                {
                    // Out of sorted modes large enough here
                    break;
                }
                if (CBool(current.h) && (current.h < mode.h))
                {
                    if (CBool(current.w) && (current.w == mode.w))
                    {
                        // Out of sorted modes large enough here
                        break;
                    }

                    /*
                     * Wider, but not tall enough, due to a different
                     * aspect ratio. This mode must be skipped, but closer
                     * modes may still follow.
                     */
                    continue;
                }
                if (match == null || current.w < match.w || current.h < match.h)
                {
                    match = current;
                    continue;
                }
                if (current.format != match.format)
                {
                    // Sorted highest depth to lowest
                    if (current.format == target_format ||
                        (SDL_BitsPerPixel(current.format) >= SDL_BitsPerPixel(target_format)
                        && SDL_PixelType(current.format) == SDL_PixelType(target_format))
                        )
                    {
                        match = current;
                    }
                    continue;
                }
                if (current.refresh_rate != match.refresh_rate)
                {
                    // Sorted highest refresh to lowest
                    if (current.refresh_rate >= target_refresh_rate)
                    {
                        match = current;
                    }
                }
            }
            if (match != null)
            {
                if (CBool(match.format))
                {
                    closest.format = match.format;
                }
                else
                {
                    closest.format = mode.format;
                }
                if (CBool(match.w) && CBool(match.h))
                {
                    closest.w = match.w;
                    closest.h = match.h;
                }
                else
                {
                    closest.w = mode.w;
                    closest.h = mode.h;
                }
                if (CBool(match.refresh_rate))
                {
                    closest.refresh_rate = match.refresh_rate;
                }
                else
                {
                    closest.refresh_rate = mode.refresh_rate;
                }
                closest.driverdata = match.driverdata;

                // Pick some reasonable defaults if the app and driver don't care
                if (!CBool(closest.format))
                {
                    closest.format = SDL_PIXELFORMAT_RGB888;
                }
                if (!CBool(closest.w))
                {
                    closest.w = 640;
                }
                if (!CBool(closest.h))
                {
                    closest.h = 480;
                }
                return closest;
            }
            return null;
        }

        static SDL_DisplayMode SDL_GetWindowDisplayMode(SDL_Window window)
        {
            SDL_DisplayMode fullscreen_mode;
            SDL_VideoDisplay display;

            if (Check_Window_Magic(window, out Exception e)) throw e;

            fullscreen_mode = window.fullscreen_mode.DeepCopy();
            if (!CBool(fullscreen_mode.w))
            {
                fullscreen_mode.w = window.w;
            }
            if (!CBool(fullscreen_mode.h))
            {
                fullscreen_mode.h = window.h;
            }

            display = SDL_GetDisplayForWindow(window);

            // if in desktop size mode, just return the size of the desktop
            if ((window.flags & SDL_WINDOW_FULLSCREEN_DESKTOP) == SDL_WINDOW_FULLSCREEN_DESKTOP)
            {
                fullscreen_mode = display.desktop_mode;
            }
            else if (SDL_GetClosestDisplayModeForDisplay(SDL_GetDisplayForWindow(window)
               , fullscreen_mode
               , fullscreen_mode) == null)
            {
                throw new NativeException("Couldn't find display mode match");
            }

            return fullscreen_mode;
        }

        static void SDL_MinimizeWindow(SDL_Window window)
        {
            if (Check_Window_Magic(window, out Exception e)) throw e;

            if (CBool(window.flags & SDL_WINDOW_MINIMIZED))
            {
                return;
            }

            SDL_UpdateFullscreenMode(window, false);

            if (_video.MinimizeWindow != null)
            {
                _video.MinimizeWindow(_video, window);
            }
        }

        static void SDL_SetDisplayModeForDisplay(SDL_VideoDisplay display, SDL_DisplayMode mode)
        {
            SDL_DisplayMode display_mode;
            SDL_DisplayMode current_mode;

            if (mode != null)
            {
                display_mode = mode.DeepCopy();

                // Default to the current mode
                if (!CBool(display_mode.format))
                {
                    display_mode.format = display.current_mode.format;
                }
                if (!CBool(display_mode.w))
                {
                    display_mode.w = display.current_mode.w;
                }
                if (!CBool(display_mode.h))
                {
                    display_mode.h = display.current_mode.h;
                }
                if (!CBool(display_mode.refresh_rate))
                {
                    display_mode.refresh_rate = display.current_mode.refresh_rate;
                }

                // Get a good video mode, the closest one possible
                if (SDL_GetClosestDisplayModeForDisplay(display
                    , display_mode
                    , display_mode) == null)
                {
                    throw new NativeException("No video mode large enough for {0}x{1}"
                        , display_mode.w
                        , display_mode.h);
                }
            }
            else
            {
                display_mode = display.desktop_mode;
            }

            // See if there's anything left to do
            current_mode = display.current_mode;
            if (CompareMemory(display_mode, current_mode) == 0)
            {
                return;
            }

            // Actually change the display mode
            if (_video.SetDisplayMode == null)
            {
                throw new NativeException("Video driver doesn't support changing display mode");
            }
            _video.SetDisplayMode(_video, display, display_mode);
            display.current_mode = display_mode;
        }

        static void SDL_UpdateFullscreenMode(SDL_Window window, bool fullscreen)
        {
            SDL_VideoDisplay display = SDL_GetDisplayForWindow(window);
            SDL_Window other;

            if (fullscreen)
            {
                // Hide any other fullscreen windows
                if (display.fullscreen_window != null &&
                    display.fullscreen_window != window)
                {
                    SDL_MinimizeWindow(window);
                }
            }

            // See if anything needs to be done now
            if ((display.fullscreen_window == window) == fullscreen)
            {
                return;
            }

            // See if there are any fullscreen windows
            for (other = _video.windows; other != null; other = other.next)
            {
                bool setDisplayMode = false;

                if (other == window)
                {
                    setDisplayMode = fullscreen;
                }
                else if (Fullscreen_Visible(other) && SDL_GetDisplayForWindow(other) == display)
                {
                    setDisplayMode = true;
                }

                if (setDisplayMode)
                {
                    SDL_DisplayMode fullscreen_mode;

                    try
                    {
                        fullscreen_mode = SDL_GetWindowDisplayMode(other);
                        bool resized = true;

                        if (other.w == fullscreen_mode.w && other.h == fullscreen_mode.h)
                        {
                            resized = false;
                        }

                        // only do the mode change if we want exclusive fullscreen
                        if ((window.flags & SDL_WINDOW_FULLSCREEN_DESKTOP) != SDL_WINDOW_FULLSCREEN_DESKTOP)
                        {
                            SDL_SetDisplayModeForDisplay(display, fullscreen_mode);
                        }
                        else
                        {
                            SDL_SetDisplayModeForDisplay(display, null);
                        }

                        if (_video.SetWindowFullscreen != null)
                        {
                            _video.SetWindowFullscreen(_video, other, display, true);
                        }
                    }
                    catch (NativeException e)
                    {
                        IgnoreInDebugWriteLine(e);
                    }
                }
            }


        }

        static void SDL_SetWindowFullscreen(SDL_Window window, uint flags)
        {
            if (Check_Window_Magic(window, out Exception e)) throw e;

            flags &= FULLSCREEN_MASK;

            if (flags == (window.flags & FULLSCREEN_MASK))
            {
                return;
            }

            // clear the previous flags and OR in the new ones
            window.flags &= ~FULLSCREEN_MASK;
            window.flags |= flags;

            SDL_UpdateFullscreenMode(window, Fullscreen_Visible(window));
        }

        #region enumerated pixel format definitions

        static uint SDL_PixelFlag(uint x) => x >> 28 & 0x0F;
        static uint SDL_PixelType(uint x) => x >> 24 & 0x0F;
        static uint SDL_PixelOrder(uint x) => x >> 20 & 0x0F;
        static uint SDL_PixelLayout(uint x) => x >> 16 & 0x0F;
        static uint SDL_BitsPerPixel(uint x) => x >> 8 & 0xFF;
        static uint SDL_BytesPerPixel(uint x)
            => SDL_IsPixelFormat_FourCC(x) ?
            x == SDL_PIXELFORMAT_YUY2 ||
            x == SDL_PIXELFORMAT_UYVY ||
            x == SDL_PIXELFORMAT_YVYU ? 2U : 1U
            : x >> 0 & 0xFF;

        static bool SDL_IsPixelFormat_Indexed(uint format)
            => !SDL_IsPixelFormat_FourCC(format) && (
            SDL_PixelType(format) == GetValue(SDL_PixelTypes.INDEX1) ||
            SDL_PixelType(format) == GetValue(SDL_PixelTypes.INDEX4) ||
            SDL_PixelType(format) == GetValue(SDL_PixelTypes.INDEX8));
        static bool SDL_IsPixelFormat_Alpha(uint format)
            => !SDL_IsPixelFormat_FourCC(format) && (
            SDL_PixelOrder(format) == GetValue(SDL_PackedOrder.ARGB) ||
            SDL_PixelOrder(format) == GetValue(SDL_PackedOrder.RGBA) ||
            SDL_PixelOrder(format) == GetValue(SDL_PackedOrder.ABGR) ||
            SDL_PixelOrder(format) == GetValue(SDL_PackedOrder.BGRA));
        static bool SDL_IsPixelFormat_FourCC(uint format) => CBool(format) && SDL_PixelFlag(format) != 1;

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

        enum SDL_PixelTypes : uint
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

        #region fullscreen

        const uint FULLSCREEN_MASK = SDL_WINDOW_FULLSCREEN_DESKTOP | SDL_WINDOW_FULLSCREEN;

        static bool Fullscreen_Visible(SDL_Window w)
            => CBool(w.flags & SDL_WINDOW_FULLSCREEN)
            && CBool(w.flags & SDL_WINDOW_SHOWN)
            && !CBool(w.flags & SDL_WINDOW_MINIMIZED);

        #endregion

        #region window pos

        const uint SDL_WINDOWPOS_UNDEFINED_MASK = 0x1FFF0000;
        const uint SDL_WINDOWPOS_CENTERED_MASK = 0x2FFF0000;

        static uint SDL_WindowPos_Undefined_Display(uint x) => SDL_WINDOWPOS_UNDEFINED_MASK | x;
        static bool SDL_WindowPos_IsUndefined(int x) => (x & 0xFFFF0000) == SDL_WINDOWPOS_UNDEFINED_MASK;

        static uint SDL_WindowPos_Centered_Display(uint x) => SDL_WINDOWPOS_CENTERED_MASK | 0;
        static bool SDL_WindowPos_IsCentered(int x) => (x & 0xFFFF0000) == SDL_WINDOWPOS_CENTERED_MASK;

        #endregion
    }

    public class SDL_Window : Form
    {
        public unsafe void* magic;
        public uint id;
        public int x, y;
        public int w, h;
        public int min_w, min_h;
        public int max_w, max_h;
        public uint flags;

        public SDL_DisplayMode fullscreen_mode;// struct

        public SDL_Window prev;
        public SDL_Window next;

        public IntPtr GetHandle()
        {
            if (InvokeRequired)
            {
                // 解决窗体关闭时出现“访问已释放句柄”的异常
                while (!IsHandleCreated)
                {
                    if (Disposing || IsDisposed)
                        return IntPtr.Zero;
                }

                GetHandleCallback d = new GetHandleCallback(GetHandle);
                return (IntPtr)Invoke(d, null);
            }
            else
            {
                return Handle;
            }
        }

        delegate IntPtr GetHandleCallback();
    }

    public class SDL_VideoDevice
    {
        public DVideoInit VideoInit;
        public DVideoQuit VideoQuit;
        public DGetDisplayBounds GetDisplayBounds;
        public DGetDisplayModes GetDisplayModes;
        public DSetDisplayMode SetDisplayMode;
        public DCompareMemory CompareMemory;
        public DMinimizeWindow MinimizeWindow;
        public DSetWindowFullscreen SetWindowFullscreen;

        public int num_displays;
        public SDL_VideoDisplay[] displays;
        public SDL_Window windows;// 内部链表
        public byte window_magic;

        public SDL_VideoDisplay PrimaryDisplay => displays[0];
        public GCHandle FixedMagic { get; private set; }

        ~SDL_VideoDevice()
        {
            FixedMagic.Free();
        }

        public SDL_VideoDevice()
        {
            FixedMagic = GCHandle.Alloc(window_magic, GCHandleType.Pinned);
        }

        public delegate void DVideoInit(SDL_VideoDevice _this);
        public delegate void DVideoQuit(SDL_VideoDevice _this);
        public delegate Rectangle DGetDisplayBounds(SDL_VideoDevice _this, SDL_VideoDisplay display);
        public delegate void DGetDisplayModes(SDL_VideoDevice _this, SDL_VideoDisplay display);
        public delegate void DSetDisplayMode(SDL_VideoDevice _this, SDL_VideoDisplay display, SDL_DisplayMode mode);
        public delegate int DCompareMemory(SDL_DisplayModeData m1, SDL_DisplayModeData m2);
        public delegate void DMinimizeWindow(SDL_VideoDevice _this, SDL_Window window);
        public delegate void DSetWindowFullscreen(
            SDL_VideoDevice _this,
            SDL_Window window,
            SDL_VideoDisplay display,
            bool fullscreen);
    }

    public class SDL_VideoDisplay
    {
        public string name;
        public int max_display_modes;
        public int num_display_modes;
        public SDL_DisplayMode[] display_modes;
        public SDL_DisplayMode desktop_mode;
        public SDL_DisplayMode current_mode;

        public SDL_Window fullscreen_window;

        public SDL_VideoDevice device;

        public SDL_DisplayData driverdata;      // void *driverdata;
    }

    public class SDL_DisplayMode : Prototype<SDL_DisplayMode>
    {
        public uint format;                     // pixel format
        public int w;                           // width
        public int h;                           // height
        public int refresh_rate;                // refresh rate (or zero for unspecified)
        public SDL_DisplayModeData driverdata;  // driver-specific data, initialize to null
    }

    [StructInterface]
    public interface SDL_DisplayData { }

    [StructInterface]
    public interface SDL_DisplayModeData { }
}

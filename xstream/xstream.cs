using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xstream.Codec;

namespace Xstream
{
    public partial class Xstream : Form
    {
        const uint USER = 0x0400;

        const int WM_CLOSE = 0x0010;

        #region Window Styles

        const long WS_CLIPSIBLINGS = 0x00800000L;
        const long WS_CLIPCHILDREN = 0x02000000L;
        const long WS_POPUP = 0x80000000L;
        const long WS_OVERLAPPED = 0x00000000L;
        const long WS_CAPTION = 0x00C00000L;
        const long WS_SYSMENU = 0x00080000L;
        const long WS_MINIMIZEBOX = 0x00020000L;
        const long WS_THICKFRAME = 0x00040000L;
        const long WS_MAXIMIZEBOX = 0x00010000L;

        const long STYLE_BASIC = WS_CLIPSIBLINGS | WS_CLIPCHILDREN;
        const long STYLE_FULLSCREEN = WS_POPUP;
        const long STYLE_BORDERLESS = WS_POPUP;
        const long STYLE_NORMAL = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX;
        const long STYLE_RESIZABLE = WS_THICKFRAME | WS_MAXIMIZEBOX;
        const long STYLE_MASK = STYLE_FULLSCREEN | STYLE_BORDERLESS | STYLE_NORMAL | STYLE_RESIZABLE;

        #endregion

        readonly CancellationTokenSource _cancellationTokenSource;

        DxAudio _audioRenderer;
        DxVideo _videoRenderer;
        FFmpegDecoder _decoder;

        DxInput _input;
        event EventHandler<InputEventArgs> _handleInputEvent;

        Thread _thread;
        uint _threadId;

        delegate IntPtr GetHandleCallback();
        int _numDisplays;
        VideoDisplay[] _displays;
        VideoDisplay PrimaryDisplay => _displays[0];
        DisplayMode _fullscreenMode;

        public Xstream()
        {
            InitModes();

            InitializeComponent();

            ClientSize = new Size(Program.Nano.Configuration.VideoMaximumWidth
                , Program.Nano.Configuration.VideoMaximumHeight);

            if (Config.Fullscreen)
            {
                DisplayMode fullscreen_mode;

                if (Config.Borderless)
                {
                    //FormBorderStyle = FormBorderStyle.None;
                    //WindowState = FormWindowState.Maximized;
                }

                long style = Native.GetWindowLongPtr(Handle, GWL.STYLE);
                style &= ~STYLE_MASK;
                style |= STYLE_FULLSCREEN;

                Rectangle bounds = GetDisplayBounds(PrimaryDisplay);

                Native.SetWindowLongPtr(new HandleRef(this, Handle), GWL.STYLE, style);
                Native.SetWindowPos(Handle
                    , SpecialWindowHandles.HWND_NOTOPMOST
                    , bounds.X
                    , bounds.Y
                    , bounds.Width
                    , bounds.Height
                    , SetWindowPosFlags.SWP_NOCOPYBITS);
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            Bitmap map = new Bitmap($"{baseDir}/Images/icon.png");
            Icon = Icon.FromHandle(map.GetHicon());
            FormClosed += (object sender, FormClosedEventArgs e) =>
            {
                Native.DestroyIcon(Icon.Handle);
            };

            KeyPreview = Config.KeyPreview;
            KeyDown += (sender, e) =>
            {
                if (!KeyPreview)
                    return;

                e.SuppressKeyPress = true;

                switch (e.KeyCode)
                {
                    default:
                        MessageBox.Show("Form.KeyPress: '" + e.KeyCode + "' consumed.");
                        break;
                }
            };

            // DirectX / FFMPEG setup

            _cancellationTokenSource = new CancellationTokenSource();

            _audioRenderer = new DxAudio(
                (int)Program.AudioFormat.SampleRate, (int)Program.AudioFormat.Channels);
            _videoRenderer = new DxVideo(
                (int)Program.VideoFormat.Width, (int)Program.VideoFormat.Height);

            _decoder = new FFmpegDecoder(Program.Nano, Program.AudioFormat, Program.VideoFormat);

            Program.Nano.AudioFrameAvailable += _decoder.ConsumeAudioData;
            Program.Nano.VideoFrameAvailable += _decoder.ConsumeVideoData;

            if (Config.UseController)
            {
                _input = new DxInput($"{baseDir}/gamecontrollerdb.txt");
                _handleInputEvent += _input.HandleInput;
            }

            Shown += (object sender, EventArgs e) =>
            {
                _thread = new Thread(MainLoop);
                _thread.Start();
            };
        }

        Task StartInputFrameSendingTask()
        {
            return Task.Run(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        await Program.Nano.Input.SendInputFrame(
                            DateTime.UtcNow, _input.Buttons, _input.Analog, _input.Extension);
                    }
                    catch
                    {
                        Thread.Sleep(millisecondsTimeout: 5);
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        public void MainLoop()
        {
            _threadId = Native.GetCurrentThreadId();

            if (Config.UseController && !_input.Initialize(this))
                throw new InvalidOperationException("Failed to init DirectX Input");

            _audioRenderer.Initialize(this, 4096);// Good default buffer size
            _videoRenderer.Initialize(this);

            _decoder.Start();

            if (Config.UseController)
            {
                StartInputFrameSendingTask();
            }

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (_decoder.DecodedAudioQueue.Count > 0)
                {
                    var sample = _decoder.DecodedAudioQueue.Dequeue();
                    _audioRenderer.Update(sample);
                }

                //if (_decoder.DecodedVideoQueue.Count > 0)
                //{
                //    var frame = _decoder.DecodedVideoQueue.Dequeue();
                //    _videoRenderer.Update(frame);
                //}

                if (!Native.PeekMessage(out NativeMessage m, 0, 0, 0, Native.PM_REMOVE))
                {
                    continue;
                }

                switch (SDL_(m.msg))
                {
                    case SDL_EventType.QUIT:
                        Debug.WriteLine("Quit, bye!");
                        _cancellationTokenSource.Cancel();
                        break;
                    default:
                        break;
                }
            }

            // closes input controller
            if (Config.UseController)
            {
                _input.CloseController();
            }

            _audioRenderer.Close();
            //_decoder.Dispose();
        }

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

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLOSE)
            {
                PostMessage(SDL_EventType.QUIT, m.WParam, m.LParam);
            }

            base.WndProc(ref m);
        }

        internal bool PostMessage(SDL_EventType msg) => PostMessage(msg, IntPtr.Zero, IntPtr.Zero);

        internal bool PostMessage(SDL_EventType msg, IntPtr wParam, IntPtr lParam)
        {
            if (_threadId > 0 && _thread.IsAlive)
            {
                bool result = Native.PostThreadMessage(_threadId, WM_(msg), wParam, lParam);
                if (!result)
                {
                    Debug.WriteLine($"向{_threadId}线程发送消息失败：{Native.GetLastError()}");
                }
                return result;
            }

            Debug.WriteLine($"无法向{_threadId}线程发送消息，目标线程当前状态为{_thread.ThreadState}");
            return false;
        }

        static uint WM_(SDL_EventType msg) => USER + (uint)msg;
        static SDL_EventType SDL_(uint msg) => (SDL_EventType)(msg - USER);

        #region modes

        static int CmpModes(DisplayMode a, DisplayMode b)
        {
            if (a.w != b.w)
            {
                return b.w - a.w;
            }
            if (a.h != b.h)
            {
                return b.h - a.h;
            }
            if (a.format != b.format)
            {
                return b.format - a.format;// 降序排列
            }
            if (a.refresh_rate != b.refresh_rate)
            {
                return b.refresh_rate - a.refresh_rate;
            }
            return 0;
        }

        static bool AddDisplayMode(ref VideoDisplay display, DisplayMode mode)
        {
            DisplayMode[] modes;
            int i, nmodes;

            // Make sure we don't already have the mode in the list
            modes = display.display_modes;
            nmodes = display.num_display_modes;
            for (i = nmodes; i-- > 0;)
            {
                if (Native.memcmp(mode, modes[i], Marshal.SizeOf(mode)) == 0)
                {
                    return false;
                }
            }

            // Go ahead and add the new mode
            if (nmodes == display.max_display_modes)
            {
                modes = new DisplayMode[display.max_display_modes += 32];
                display.display_modes?.CopyTo(modes, 0);
                display.display_modes = modes;
            }
            modes[nmodes] = mode;
            display.num_display_modes++;

            // Re-sort video modes
            Array.Sort(display.display_modes, 0, display.num_display_modes
                , Comparer<DisplayMode>.Create(CmpModes));

            return true;
        }

        static void GetDisplayModes(ref VideoDisplay display)
        {
            DisplayMode mode = new DisplayMode();

            for (int i = 0; ; ++i)
            {
                if (!GetDisplayMode(display.DeviceName, i, ref mode))
                {
                    break;
                }

                if (mode.format == Format.P8)
                {
                    // We don't support palettized modes now
                    continue;
                }

                if (mode.format != Format.Unknown)
                {
                    AddDisplayMode(ref display, mode);
                }
            }

            //if (display.display_modes == null)
            //{
            //    display.display_modes = new DisplayMode[display.max_display_modes];
            //}
        }

        static bool GetDisplayMode(string deviceName, int index, ref DisplayMode mode)
        {
            DEVMODE data;
            DEVMODE devmode = new DEVMODE();
            IntPtr hdc;

            devmode.dmSize = (short)Marshal.SizeOf(devmode);
            devmode.dmDriverExtra = 0;
            if (!Native.EnumDisplaySettings(deviceName, index, ref devmode))
            {
                return false;
            }

            data = devmode;
            data.dmFields = Native.DM_BITSPERPEL
                | Native.DM_PELSWIDTH
                | Native.DM_PELSHEIGHT
                | Native.DM_DISPLAYFREQUENCY
                | Native.DM_DISPLAYFLAGS;

            // Fill in the mode information
            mode.format = Format.Unknown;
            mode.w = devmode.dmPelsWidth;
            mode.h = devmode.dmPelsHeight;
            mode.refresh_rate = devmode.dmDisplayFrequency;
            mode.DeviceMode = data;

            if (index == Native.ENUM_CURRENT_SETTINGS
                && (hdc = Native.CreateDC(deviceName, null, null, IntPtr.Zero)) != null)
            {
                BITMAPINFO bmi;
                IntPtr hbm;

                bmi = new BITMAPINFO();
                bmi.bmiHeader.Init();

                hbm = Native.CreateCompatibleBitmap(hdc, 1, 1);
                Native.GetDIBits(hdc, hbm, 0, 1, null, ref bmi, DIB_Color_Mode.DIB_RGB_COLORS);
                Native.GetDIBits(hdc, hbm, 0, 1, null, ref bmi, DIB_Color_Mode.DIB_RGB_COLORS);
                Native.DeleteObject(hbm);
                Native.DeleteDC(hdc);
                if (bmi.bmiHeader.biCompression == BitmapCompressionMode.BI_BITFIELDS)
                {
                    switch (bmi.bmiColors[0])
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
                if ((devmode.dmFields & Native.DM_BITSPERPEL) == Native.DM_BITSPERPEL)
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

        int AddVideoDisplay(VideoDisplay display)
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

        bool AddDisplay(string deviceName)
        {
            VideoDisplay display = new VideoDisplay();
            DisplayMode mod = new DisplayMode();
            DISPLAY_DEVICE device = new DISPLAY_DEVICE();

            if (!GetDisplayMode(deviceName, Native.ENUM_CURRENT_SETTINGS, ref mod))
            {
                return false;
            }

            device.cb = Marshal.SizeOf(device);
            if (Native.EnumDisplayDevices(deviceName, 0, ref device, 0))
            {
                display.name = device.DeviceString;
            }
            display.desktop_mode = mod;
            display.current_mode = mod;
            display.DeviceName = deviceName;
            AddVideoDisplay(display);
            return true;
        }

        void InitModes()
        {
            int pass, count;
            uint i, j;
            DISPLAY_DEVICE device;

            device = new DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(device);

            // Get the primary display in the first pass
            for (pass = 0; pass < 2; ++pass)
            {
                for (i = 0; ; ++i)
                {
                    string deviceName;

                    if (!Native.EnumDisplayDevices(null, i, ref device, 0)) break;
                    if ((device.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) == 0) continue;
                    if (pass == 0)
                    {
                        if ((device.StateFlags & DisplayDeviceStateFlags.PrimaryDevice) == 0) continue;
                    }
                    else
                    {
                        if ((device.StateFlags & DisplayDeviceStateFlags.PrimaryDevice) != 0) continue;
                    }
                    deviceName = device.DeviceName;
                    Debug.WriteLine("Device: {0}", deviceName);
                    count = 0;
                    for (j = 0; ; ++j)
                    {
                        if (!Native.EnumDisplayDevices(deviceName, j, ref device, 0)) break;
                        if ((device.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) == 0) continue;
                        if (pass == 0)
                        {
                            if ((device.StateFlags & DisplayDeviceStateFlags.PrimaryDevice) == 0) continue;
                        }
                        else
                        {
                            if ((device.StateFlags & DisplayDeviceStateFlags.PrimaryDevice) != 0) continue;
                        }
                        count += AddDisplay(device.DeviceName) ? 1 : 0;
                    }
                    if (count == 0)
                    {
                        AddDisplay(deviceName);
                    }
                }
            }
            if (_numDisplays == 0)
            {
                throw new Exception("No displays available");
            }
        }

        #endregion

        static Rectangle GetDisplayBounds(VideoDisplay display)
        {
            DEVMODE data = display.current_mode.DeviceMode;
            return new Rectangle(data.DUMMYUNIONNAME.dmPosition.X
                , data.DUMMYUNIONNAME.dmPosition.Y
                , data.dmPelsWidth
                , data.dmPelsHeight);
        }

        static int GetNumDisplayModesForDisplay(ref VideoDisplay display)
        {
            if (display.num_display_modes == 0)
            {
                GetDisplayModes(ref display);
                Array.Sort(display.display_modes, 0, display.num_display_modes
                    , Comparer<DisplayMode>.Create(CmpModes));
            }
            return display.num_display_modes;
        }

        static DisplayMode? GetClosestDisplayModeForDisplay(
            ref VideoDisplay display,
            DisplayMode mode,
            ref DisplayMode closest)
        {
            Format target_format;
            int target_refresh_rate;
            int i;
            DisplayMode current;
            DisplayMode? match;

            // Default to the desktop format
            if (mode.format != 0)
            {
                target_format = mode.format;
            }
            else
            {
                target_format = display.desktop_mode.format;
            }

            // Default to the desktop refresh rate
            if (mode.refresh_rate != 0)
            {
                target_refresh_rate = mode.refresh_rate;
            }
            else
            {
                target_refresh_rate = display.desktop_mode.refresh_rate;
            }

            match = null;
            for (i = 0; i < GetNumDisplayModesForDisplay(ref display); ++i)
            {
                current = display.display_modes[i];

                if (current.w != 0 && (current.w < mode.w))
                {
                    // Out of sorted modes large enough here
                    break;
                }
                if (current.h != 0 && (current.h < mode.h))
                {
                    if (current.w != 0 && (current.w == mode.w))
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
                if (!match.HasValue || current.w < match.Value.w || current.h < match.Value.h)
                {
                    match = current;
                    continue;
                }
                if (current.format != match.Value.format)
                {
                    // Sorted highest depth to lowest
                    if (current.format == target_format)
                    {
                        match = current;
                    }
                    continue;
                }
                if (current.refresh_rate != match.Value.refresh_rate)
                {
                    // Sorted highest refresh to lowest
                    if (current.refresh_rate >= target_refresh_rate)
                    {
                        match = current;
                    }
                }
            }
            if (match.HasValue)
            {
                if (match.Value.format != 0)
                {
                    closest.format = match.Value.format;
                }
                else
                {
                    closest.format = mode.format;
                }
                if (match.Value.w != 0 && match.Value.h != 0)
                {
                    closest.w = match.Value.w;
                    closest.h = match.Value.h;
                }
                else
                {
                    closest.w = mode.w;
                    closest.h = mode.h;
                }
                if (match.Value.refresh_rate != 0)
                {
                    closest.refresh_rate = match.Value.refresh_rate;
                }
                else
                {
                    closest.refresh_rate = mode.refresh_rate;
                }
                closest.DeviceMode = match.Value.DeviceMode;

                // Pick some reasonable defaults if the app and driver don't care
                if (closest.format == 0)
                {
                    closest.format = Format.X8R8G8B8;
                }
                if (closest.w == 0)
                {
                    closest.w = 640;
                }
                if (closest.h == 0)
                {
                    closest.h = 480;
                }
                return closest;
            }
            return null;
        }

        int GetWindowDisplayIndex()
        {
            int i, dist;
            int closest = -1;
            int closest_dist = 0x7FFFFFFF;
            Point center;
            Point delta;
            Rectangle rect;

            if (Config.Fullscreen)
            {
                return 0;
            }

            // Find the display containing the window
            center = new Point(ClientRectangle.X + ClientRectangle.Width / 2
                , ClientRectangle.Y + ClientRectangle.Height / 2);
            for (i = 0; i < _numDisplays; ++i)
            {
                rect = GetDisplayBounds(_displays[i]);
                // TODO
            }

            return closest;
        }

        DisplayMode GetWindowDisplayMode()
        {
            DisplayMode fullscreen_mode = _fullscreenMode;

            if (fullscreen_mode.w == 0)
            {
                fullscreen_mode.w = ClientSize.Width;
            }
            if (fullscreen_mode.h == 0)
            {
                fullscreen_mode.h = ClientSize.Height;
            }

            // if in desktop size mode, just return the size of the desktop
            if (Config.Borderless)
            {
                fullscreen_mode = PrimaryDisplay.desktop_mode;
            }
            else if (!GetClosestDisplayModeForDisplay(ref _displays[GetWindowDisplayIndex()]
                , fullscreen_mode
                , ref fullscreen_mode).HasValue)
            {
                throw new Exception("Couldn't find display mode match");
            }

            return fullscreen_mode;
        }
    }

    struct VideoDisplay
    {
        public string name;
        public int max_display_modes;
        public int num_display_modes;
        public DisplayMode[] display_modes;
        public DisplayMode desktop_mode;
        public DisplayMode current_mode;

        public DisplayMode fullscreen_mode;

        public string DeviceName;
    }

    struct DisplayMode
    {
        public Format format;// pixel format
        public int w;// width
        public int h;// height
        public int refresh_rate;// refresh rate (or zero for unspecified)

        public DEVMODE DeviceMode;
    }

    enum SDL_EventType : uint
    {
        // Application events
        QUIT = 0x100,// < User-requested quit

        // Audio hotplug events
        AUDIODEVICEADDED = 0x1100,// < A new audio device is available
        AUDIODEVICEREMOVED, // < An audio device has been removed.

        // Game controller events
        CONTROLLERAXISMOTION = 0x650,// < Game controller axis motion
        CONTROLLERBUTTONDOWN,// < Game controller button pressed
        CONTROLLERBUTTONUP,// < Game controller button released
        CONTROLLERDEVICEADDED,// < A new Game controller has been inserted into the system
        CONTROLLERDEVICEREMOVED,// < An opened Game controller has been removed
        CONTROLLERDEVICEREMAPPED// < The controller mapping was updated
    }
}

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
    public partial class Xstream : SDL_Window
    {
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
        Rectangle _windowed;// Stored position and size for windowed mode

        public Xstream()
        {
            InitializeComponent();

            VideoDisplay display = _displays[GetWindowDisplayIndex()];
            Rectangle bounds = GetDisplayBounds(display);

            _windowed.Width = Program.Nano.Configuration.VideoMaximumWidth;
            _windowed.Height = Program.Nano.Configuration.VideoMaximumHeight;
            _windowed.X = bounds.X + (bounds.Width - _windowed.Width) / 2;
            _windowed.Y = bounds.Y + (bounds.Height - _windowed.Height) / 2;

            if (Config.Fullscreen)
            {
                //if (Config.Borderless)
                //{
                //    FormBorderStyle = FormBorderStyle.None;
                //    WindowState = FormWindowState.Maximized;
                //}

                UpdateFullscreenMode(true);
            }
            else
            {
                ClientSize = new Size(_windowed.Width, _windowed.Height);
            }

            // If the window was created fullscreen, make sure the mode code matches
            UpdateFullscreenMode(Config.Fullscreen);

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            Bitmap map = new Bitmap($"{baseDir}/Images/icon.png");
            Icon = Icon.FromHandle(map.GetHicon());
            FormClosed += (object sender, FormClosedEventArgs e) =>
            {
                Native.DestroyIcon(Icon.Handle);
            };

            Text = Config.CurrentMapping.TokenFilePath;

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

                if (!Native.PeekMessage(out MSG m, IntPtr.Zero, 0, 0, Native.PM_REMOVE))
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
            if (m.Msg == Native.WM_CLOSE)
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

        static uint WM_(SDL_EventType msg) => Native.WM_USER + (uint)msg;
        static SDL_EventType SDL_(uint msg) => (SDL_EventType)(msg - Native.WM_USER);

        #region modes

        void SetDisplayModeForDisplay(ref VideoDisplay display, DisplayMode? mod)
        {
            DisplayMode display_mode;
            DisplayMode current_mode;

            if (mod.HasValue)
            {
                display_mode = mod.Value;

                // Default to the current mode
                if (display_mode.format == 0)
                {
                    display_mode.format = display.current_mode.format;
                }
                if (display_mode.w == 0)
                {
                    display_mode.w = display.current_mode.w;
                }
                if (display_mode.h == 0)
                {
                    display_mode.h = display.current_mode.h;
                }
                if (display_mode.refresh_rate == 0)
                {
                    display_mode.refresh_rate = display.current_mode.refresh_rate;
                }

                // Get a good video mode, the closest one possible
                if (!GetClosestDisplayModeForDisplay(ref display, display_mode, ref display_mode).HasValue)
                {
                    throw Shell.Log("No video mode large enough for {0}x{1}"
                        , display_mode.w, display_mode.h);
                }
            }
            else
            {
                display_mode = display.desktop_mode;
            }

            // See if there's anything left to do
            current_mode = display.current_mode;
            if (Native.memcmp(display_mode, current_mode, Marshal.SizeOf(display_mode)) == 0)
            {
                return;
            }

            // Actually change the display mode
            SetDisplayMode(display, ref display_mode);
            display.current_mode = display_mode;
        }

        #endregion

        

        unsafe int GetWindowDisplayIndex()
        {
            int i, dist;
            int closest = -1;
            int closest_dist = 0x7FFFFFFF;
            Point center = Point.Empty;
            Point delta = Point.Empty;
            Rectangle rect;

            if (Config.Fullscreen)
            {
                return 0;
            }

            // Find the display containing the window
            center.X = ClientRectangle.X + ClientRectangle.Width / 2;
            center.Y = ClientRectangle.Y + ClientRectangle.Height / 2;
            for (i = 0; i < _numDisplays; ++i)
            {
                rect = GetDisplayBounds(_displays[i]);
                if (EnclosePoints(&center, 1, &rect, null))
                {
                    return i;
                }

                delta.X = center.X - (rect.X + rect.Width / 2);
                delta.Y = center.Y - (rect.Y + rect.Height / 2);
                dist = delta.X * delta.X + delta.Y * delta.Y;
                if (dist < closest_dist)
                {
                    closest = i;
                    closest_dist = dist;
                }
            }
            return closest;
        }

        #region window

        static long GetWindowStyle()
        {
            long style = 0;

            if (Config.Fullscreen)
            {
                if (Config.Borderless)
                {
                    style |= Native.STYLE_BORDERLESS;
                }
                else
                {
                    style |= Native.STYLE_FULLSCREEN;
                }
            }
            else
            {
                style |= Native.STYLE_NORMAL;
                style |= Native.STYLE_RESIZABLE;
            }
            return style;
        }

        void SetWindowFullscreen(VideoDisplay display, bool fullscreen)
        {
            RECT rect;
            Rectangle bounds;
            long style;
            bool menu;
            int x, y;
            int w, h;

            style = Native.GetWindowLongPtr86(Handle, Native.GWL_STYLE);
            style &= ~Native.STYLE_MASK;
            style |= GetWindowStyle();

            bounds = GetDisplayBounds(display);

            if (fullscreen)
            {
                x = bounds.X;
                y = bounds.Y;
                w = bounds.Width;
                h = bounds.Height;
            }
            else
            {
                rect.Left = 0;
                rect.Top = 0;
                rect.Right = _windowed.Width;
                rect.Bottom = _windowed.Height;
                menu = (style & Native.WS_CHILDWINDOW) != 0 ? false : (Native.GetMenu(Handle) != IntPtr.Zero);
                Native.AdjustWindowRectEx(ref rect, (uint)style, menu, 0);
                w = rect.Right - rect.Left;
                h = rect.Bottom - rect.Top;
                x = _windowed.X + rect.Left;
                y = _windowed.Y + rect.Top;
            }

            Native.SetWindowLongPtr86(new HandleRef(this, Handle), Native.GWL_STYLE, style);
            Native.SetWindowPos(Handle
                , Native.HWND_NOTOPMOST
                , x, y, w, h
                , Native.SWP_NOCOPYBITS);
        }

        #endregion

        void UpdateFullscreenMode(bool fullscreen)
        {
            int displayIndex = GetWindowDisplayIndex();
            DisplayMode fullscreen_mode = GetWindowDisplayMode();

            if (fullscreen)
            {
                // only do the mode change if we want exclusive fullscreen
                if (!Config.Borderless)
                {
                    SetDisplayModeForDisplay(ref _displays[displayIndex], fullscreen_mode);
                }
                else
                {
                    SetDisplayModeForDisplay(ref _displays[displayIndex], null);
                }

                SetWindowFullscreen(_displays[displayIndex], true);
                return;
            }

            // Nope, restore the desktop mode
            SetDisplayModeForDisplay(ref _displays[displayIndex], null);

            SetWindowFullscreen(_displays[displayIndex], false);
        }
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

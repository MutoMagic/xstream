using SmartGlass.Common;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xstream.Codec;

namespace Xstream
{
    public partial class Xstream : Form
    {
        const int WM_CLOSE = 0x0010;
        const uint USER = 0x0400;
        const int PM_REMOVE = 0x0001;

        bool _useController;
        GamestreamConfiguration _config;

        readonly CancellationTokenSource _cancellationTokenSource;

        DxAudio _audioRenderer;
        DxVideo _videoRenderer;
        FFmpegDecoder _decoder;

        DxInput _input;
        event EventHandler<InputEventArgs> _handleInputEvent;

        Thread _thread;
        uint _threadId;

        public Xstream(bool useController, GamestreamConfiguration config)
        {
            _useController = useController;
            _config = config;

            ClientSize = new Size(_config.VideoMaximumWidth, _config.VideoMaximumHeight);

            KeyPreview = Program.GetSettingBool("useController.KeyPreview");
            KeyDown += (sender, e) =>
            {
                MessageBox.Show("Form.KeyPress: '" + e.KeyCode + "' consumed.");
            };

            InitializeComponent();

            // DirectX / FFMPEG setup

            _cancellationTokenSource = new CancellationTokenSource();

            _audioRenderer = new DxAudio(
                (int)Program.AudioFormat.SampleRate, (int)Program.AudioFormat.Channels);
            _videoRenderer = new DxVideo(
                (int)Program.VideoFormat.Width, (int)Program.VideoFormat.Height, this);

            _decoder = new FFmpegDecoder(Program.Nano, Program.AudioFormat, Program.VideoFormat);

            if (_useController)
            {
                _input = new DxInput($"{AppDomain.CurrentDomain.BaseDirectory}/gamecontrollerdb.txt");
                _handleInputEvent += _input.HandleInput;
            }

            Program.Nano.AudioFrameAvailable += _decoder.ConsumeAudioData;
            Program.Nano.VideoFrameAvailable += _decoder.ConsumeVideoData;

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
            _threadId = Program.GetCurrentThreadId();

            if (_useController && !_input.Initialize(this))
                throw new InvalidOperationException("Failed to init DirectX Input");

            _audioRenderer.Initialize(4096);// Good default buffer size
            //_videoRenderer.Initialize();

            _decoder.Start();

            if (_useController)
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

                if (!Program.PeekMessage(out NativeMessage m, 0, 0, 0, PM_REMOVE))
                {
                    continue;
                }

                switch ((SDL_EventType)(m.msg - USER))
                {
                    case SDL_EventType.QUIT:
                        break;
                }
            }

            // closes input controller
            if (_useController)
            {
                _input.CloseController();
            }

            _audioRenderer.Close();
            //_decoder.Dispose();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLOSE)
            {
                PostMessage(SDL_EventType.QUIT, m.WParam, m.LParam);
            }

            base.WndProc(ref m);
        }

        internal bool PostMessage(SDL_EventType msg, IntPtr wParam, IntPtr lParam)
        {
            if (_threadId > 0 && _thread.IsAlive)
            {
                return Program.PostThreadMessage(_threadId, (uint)(USER + msg), wParam, lParam);
            }

            throw new ThreadStateException($"{_threadId}:{_thread.ThreadState}");
        }
    }

    enum SDL_EventType : uint
    {
        QUIT = 0x100,
    }
}

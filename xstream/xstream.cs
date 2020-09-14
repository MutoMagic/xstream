using SharpDX;
using SharpDX.Multimedia;
using SmartGlass.Common;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xstream.Codec;

namespace Xstream
{
    public partial class Xstream : Form
    {
        readonly CancellationTokenSource _cancellationTokenSource;
        DxAudio _audioRenderer;
        DxVideo _videoRenderer;
        public DxInput Input;

        public FFmpegDecoder Decoder;

        public event EventHandler<InputEventArgs> HandleInputEvent;
        bool _useController;

        GamestreamConfiguration _config;
        bool looping;

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

            //_audioRenderer = new DxAudio(
            //    (int)Program.AudioFormat.SampleRate, (int)Program.AudioFormat.Channels);
            _videoRenderer = new DxVideo(
                (int)Program.VideoFormat.Width, (int)Program.VideoFormat.Height, this);

            Decoder = new FFmpegDecoder(Program.Nano, Program.AudioFormat, Program.VideoFormat);

            if (_useController)
            {
                Input = new DxInput($"{AppDomain.CurrentDomain.BaseDirectory}/gamecontrollerdb.txt");
                HandleInputEvent += Input.HandleInput;
            }

            Program.Nano.AudioFrameAvailable += Decoder.ConsumeAudioData;
            Program.Nano.VideoFrameAvailable += Decoder.ConsumeVideoData;

            Shown += MainLoop;
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
                            DateTime.UtcNow, Input.Buttons, Input.Analog, Input.Extension);
                    }
                    catch
                    {
                        Thread.Sleep(millisecondsTimeout: 5);
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        public void MainLoop(object sender, EventArgs e)
        {
            if (_useController && !Input.Initialize(this))
                throw new InvalidOperationException("Failed to init DirectX Input");

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            SoundStream stream = new SoundStream(File.OpenRead($"{desktop}\\[SHANA]日本群星 (オムニバス) - 恋ゴコロ.wav"));
            _audioRenderer = new DxAudio(stream.Format.SampleRate, stream.Format.Channels);
            _data = stream.ToDataStream();
            stream.Close();
            _numToRead = (int)_data.Length;
            //data.Close();

            _audioRenderer.Initialize(1024);

            //Decoder.Start();

            if (_useController)
            {
                StartInputFrameSendingTask();
            }

            looping = true;
        }

        DataStream _data;
        int _numToRead;
        int _numRead = 0;

        protected override void WndProc(ref Message m)
        {
            if (!looping)
            {
                goto end;
            }

            //if (Decoder.DecodedAudioQueue.Count > 0)
            //{
            //    var sample = Decoder.DecodedAudioQueue.Dequeue();
            //    _audioRenderer.Update(sample);
            //}

            if (_numToRead > 0)
            {
                byte[] d = _data.ReadRange<byte>(_numToRead < 1024 ? _numToRead : 1024);
                _numRead += d.Length;
                _numToRead -= d.Length;
                _audioRenderer.Update(new PCMSample(d));
            }

        end:

            base.WndProc(ref m);
        }
    }
}

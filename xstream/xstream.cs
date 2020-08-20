using SmartGlass.Common;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Xstream.Codec;

namespace Xstream
{
    public partial class Xstream : Form
    {
        readonly CancellationTokenSource _cancellationTokenSource;
        DxAudio _audioRenderer;
        DxVideo _videoRenderer;

        public FFmpegDecoder Decoder;

        event EventHandler<InputEventArgs> HandleInputEvent;
        bool _useController;

        GamestreamConfiguration _config;

        public Xstream()
        {
            InitializeComponent();

            // DirectX / FFMPEG setup

            _cancellationTokenSource = new CancellationTokenSource();

            _audioRenderer = new DxAudio(
                (int)Program.AudioFormat.SampleRate, (int)Program.AudioFormat.Channels);
            _videoRenderer = new DxVideo(
                (int)Program.VideoFormat.Width, (int)Program.VideoFormat.Height, this);

            Decoder = new FFmpegDecoder(Program.Nano, Program.AudioFormat, Program.VideoFormat);

            if (_useController)
            {

            }

            Program.Nano.AudioFrameAvailable += Decoder.ConsumeAudioData;
            Program.Nano.VideoFrameAvailable += Decoder.ConsumeVideoData;

            // MainLoop
            
        }

        public Xstream(bool useController, GamestreamConfiguration config) : this()
        {
            _useController = useController;
            _config = config;

            this.ClientSize = new Size(_config.VideoMaximumWidth, _config.VideoMaximumHeight);
        }
    }
}

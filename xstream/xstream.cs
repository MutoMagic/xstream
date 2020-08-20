using SmartGlass;
using SmartGlass.Common;
using SmartGlass.Nano;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XboxWebApi.Authentication;
using XboxWebApi.Authentication.Model;
using Xstream.Codec;

namespace Xstream
{
    public partial class Xstream : Form
    {
        readonly CancellationTokenSource _cancellationTokenSource;
        DxAudio _audioRenderer;

        // DxVideo
        string _fontSourceRegular;
        string _fontSourceBold;

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

            // DxVideo
            _fontSourceRegular = $"{AppDomain.CurrentDomain.BaseDirectory}Fonts/Xolonium-Regular.ttf";
            _fontSourceBold = $"{AppDomain.CurrentDomain.BaseDirectory}Fonts/Xolonium-Bold.ttf";

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

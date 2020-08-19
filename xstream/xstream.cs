﻿using SmartGlass;
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

        string _fontSourceRegular;
        string _fontSourceBold;

        public FFmpegDecoder Decoder;

        public Xstream()
        {
            InitializeComponent();

            // DirectX / FFMPEG setup

            _cancellationTokenSource = new CancellationTokenSource();

            _audioRenderer = new DxAudio(
                (int)Program.AudioFormat.SampleRate, (int)Program.AudioFormat.Channels);

            // DxVideo
            this.ClientSize = new Size((int)Program.VideoFormat.Width, (int)Program.VideoFormat.Height);
            _fontSourceRegular = $"{AppDomain.CurrentDomain.BaseDirectory}Fonts/Xolonium-Regular.ttf";
            _fontSourceBold = $"{AppDomain.CurrentDomain.BaseDirectory}Fonts/Xolonium-Bold.ttf";

            Decoder = new FFmpegDecoder(Program.Nano, Program.AudioFormat, Program.VideoFormat);

            // TODO:SdlInput

            Program.Nano.AudioFrameAvailable += Decoder.ConsumeAudioData;
            Program.Nano.VideoFrameAvailable += Decoder.ConsumeVideoData;

            // MainLoop
            
        }
    }
}

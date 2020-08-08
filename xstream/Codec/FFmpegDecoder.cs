using SmartGlass.Nano.Consumer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xstream.Codec
{
    public class FFmpegDecoder
    {
        VideoAssembler _videoAssembler;
        FFmpegAudio _audioHandler;
        FFmpegVideo _videoHandler;

        DateTime _audioRefTimestamp;
        DateTime _videoRefTimestamp;

        uint _audioFrameId;
        uint _videoFrameId;

        public FFmpegDecoder()
        {
            _videoAssembler = new VideoAssembler();

            _videoRefTimestamp = Program.Nano.Video.ReferenceTimestamp;
            _audioRefTimestamp = Program.Nano.Audio.ReferenceTimestamp;

            _audioFrameId = Program.Nano.Audio.FrameId;
            _videoFrameId = Program.Nano.Video.FrameId;

            _audioHandler = new FFmpegAudio();
            _videoHandler = new FFmpegVideo();


        }
    }
}

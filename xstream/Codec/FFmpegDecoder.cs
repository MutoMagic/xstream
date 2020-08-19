using SmartGlass.Nano;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xstream.Codec
{
    public class FFmpegDecoder
    {
        NanoClient _nano;
        AudioFormat _audioFormat;
        VideoFormat _videoFormat;

        VideoAssembler _videoAssembler;
        DateTime _audioRefTimestamp;
        DateTime _videoRefTimestamp;
        uint _audioFrameId;
        uint _videoFrameId;
        FFmpegAudio _audioHandler;
        FFmpegVideo _videoHandler;

        public Queue<PCMSample> DecodedAudioQueue { get; private set; }
        public Queue<YUVFrame> DecodedVideoQueue { get; private set; }

        public FFmpegDecoder(NanoClient nano, AudioFormat audioFormat, VideoFormat videoFormat)
        {
            _nano = nano;
            _audioFormat = audioFormat;
            _videoFormat = videoFormat;

            _videoAssembler = new VideoAssembler();

            _audioRefTimestamp = _nano.Audio.ReferenceTimestamp;
            _videoRefTimestamp = _nano.Video.ReferenceTimestamp;

            _audioFrameId = _nano.Audio.FrameId;
            _videoFrameId = _nano.Video.FrameId;

            _audioHandler = new FFmpegAudio();
            _videoHandler = new FFmpegVideo();

            _audioHandler.Initialize(_audioFormat);
            _videoHandler.Initialize(_videoFormat);
            _audioHandler.CreateDecoderContext();
            _videoHandler.CreateDecoderContext();

            DecodedAudioQueue = new Queue<PCMSample>();
            DecodedVideoQueue = new Queue<YUVFrame>();

            // Register queues for decoded video frames / audio samples
            _audioHandler.SampleDecoded += DecodedAudioQueue.Enqueue;
            _videoHandler.FrameDecoded += DecodedVideoQueue.Enqueue;
        }
    }
}

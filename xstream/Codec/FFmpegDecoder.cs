using SmartGlass.Nano;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;
using System;
using System.Collections.Generic;

namespace Xstream.Codec
{
    public class FFmpegDecoder
    {
        NanoClient _nano;
        AudioFormat _audioFormat;
        VideoFormat _videoFormat;

        AudioAssembler _audioAssembler;
        VideoAssembler _videoAssembler;
        DateTime _audioRefTimestamp;
        DateTime _videoRefTimestamp;
        uint _audioFrameId;
        uint _videoFrameId;
        FFmpegAudio _audioHandler;
        FFmpegVideo _videoHandler;

        public Queue<PCMSample> DecodedAudioQueue { get; private set; }
        public Queue<YUVFrame> DecodedVideoQueue { get; private set; }

        bool _audioContextInitialized;
        bool _videoContextInitialized;

        public FFmpegDecoder(NanoClient nano, AudioFormat audioFormat, VideoFormat videoFormat)
        {
            _nano = nano;
            _audioFormat = audioFormat;
            _videoFormat = videoFormat;

            _audioAssembler = new AudioAssembler();
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

        /* Called by NanoClient on freshly received data */
        public void ConsumeAudioData(object sender, AudioDataEventArgs args)
        {
            // TODO: Sorting
            AACFrame frame = _audioAssembler.AssembleAudioFrame(
                data: args.AudioData,
                profile: AACProfile.LC,
                samplingFreq: (int)_audioFormat.SampleRate,
                channels: (byte)_audioFormat.Channels);

            if (!_audioContextInitialized)
            {
                _audioHandler.UpdateCodecParameters(frame.GetCodecSpecificData());
                _audioContextInitialized = true;
            }

            if (frame == null)
                return;

            // Enqueue encoded audio data in decoder
            _audioHandler.PushData(frame);
        }

        public void ConsumeVideoData(object sender, VideoDataEventArgs args)
        {
            // TODO: Sorting
            var frame = _videoAssembler.AssembleVideoFrame(args.VideoData);

            if (frame == null)
                return;

            // Enqueue encoded video data in decoder
            if (_videoContextInitialized)
                _videoHandler.PushData(frame);
            else if (frame.PrimaryType == NalUnitType.SEQUENCE_PARAMETER_SET)
            {
                _videoHandler.UpdateCodecParameters(frame.GetCodecSpecificDataAvcc());
                _videoContextInitialized = true;
            }
        }

        public void Start()
        {
            _audioHandler.DecodingThread().Start();
        }
    }
}

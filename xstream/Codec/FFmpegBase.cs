using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xstream.Codec
{
    unsafe abstract class FFmpegBase : IDisposable
    {
        public bool IsDecoder
        {
            get
            {
                return (pCodec != null && ffmpeg.av_codec_is_decoder(pCodec) > 0);
            }
        }
        public bool IsEncoder
        {
            get
            {
                return (pCodec != null && ffmpeg.av_codec_is_encoder(pCodec) > 0);
            }
        }
        public bool Initialized
        {
            get;
            internal set;
        }
        public bool ContextCreated
        {
            get;
            internal set;
        }

        internal bool doResample = false;
        internal AVCodecID avCodecID;
        internal AVFrame* pDecodedFrame;
        internal AVPacket* pPacket;

        internal AVCodec* pCodec;
        internal AVCodecContext* pCodecContext;
        internal SwrContext* pResampler;

        public FFmpegBase()
        {
            Initialized = false;
            ContextCreated = false;

            pDecodedFrame = ffmpeg.av_frame_alloc();
            pPacket = ffmpeg.av_packet_alloc();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

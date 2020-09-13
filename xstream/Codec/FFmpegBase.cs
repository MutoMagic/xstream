using FFmpeg.AutoGen;
using System;
using System.Threading;

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
        internal AVCodecID avCodecID; // 编解码器ID
        internal AVFrame* pDecodedFrame;// 编解码后数据
        internal AVPacket* pPacket; // 编解码前数据

        internal AVCodec* pCodec; // 音视频编解码器
        internal AVCodecContext* pCodecContext; // AVCodec的上下文
        internal SwrContext* pResampler; // 重采样

        public FFmpegBase()
        {
            Initialized = false;
            ContextCreated = false;

            pDecodedFrame = ffmpeg.av_frame_alloc();
            pPacket = ffmpeg.av_packet_alloc();
        }

        /// <summary>
        /// Set Codec specific parameters for decoding
        /// </summary>
        /// <param name="codecData">Codec specific data</param>
        internal abstract void UpdateCodecParameters(byte[] codecData);

        /// <summary>
        /// Sets the codec context parameters.
        /// </summary>
        internal abstract void SetCodecContextParams(AVCodecContext* codecContext);

        /// <summary>
        /// Sets the resampler parameters.
        /// </summary>
        internal abstract SwrContext* CreateResampler(AVCodecContext* codecContext);

        /// <summary>
        /// Start decoding thread
        /// </summary>
        /// <returns></returns>
        public abstract Thread DecodingThread();

        /// <summary>
        /// Inits the Codec context.
        /// </summary>
        /// <param name="encoder">If set to <c>true</c> encoder.</param>
        void CreateContext(bool encoder = false)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("Instance is not initialized yet, call Initialize() first");
            }
            else if (ContextCreated)
            {
                throw new InvalidOperationException("Context already initialized!");
            }

            if (encoder)
                pCodec = ffmpeg.avcodec_find_encoder(avCodecID);
            else
                pCodec = ffmpeg.avcodec_find_decoder(avCodecID);

            if (pCodec == null)
            {
                throw new InvalidOperationException("VideoCodec not found");
            }

            pCodecContext = ffmpeg.avcodec_alloc_context3(pCodec);
            if (pCodecContext == null)
            {
                throw new InvalidOperationException("Could not allocate codec context");
            }

            // Call to abstract method
            SetCodecContextParams(pCodecContext);

            // 读取码流时，可能不是按一个完整的包来读取的，所以要判断下。
            /*
             * 通知解码器，我们能够处理截断的流。
             * 为什么会有截断流?因为视频流中的数据是被分割放入包中的，因为每个视频帧的数据大小是可变的。
             * 那么两帧之间的边界就不一定刚好是包的边界，这里通知解码器，我们可以处理截断流。
             */
            if ((pCodec->capabilities & ffmpeg.AV_CODEC_CAP_TRUNCATED) == ffmpeg.AV_CODEC_CAP_TRUNCATED)
                pCodecContext->flags |= ffmpeg.AV_CODEC_FLAG_TRUNCATED;// 截断的方式来读取，流帧边界可以在包中

            // 打开解码器
            if (ffmpeg.avcodec_open2(pCodecContext, pCodec, null) < 0)
            {
                throw new InvalidOperationException("Could not open codec");
            }

            if (doResample)
            {
                // Call to abstract method
                pResampler = CreateResampler(pCodecContext);
                if (ffmpeg.swr_is_initialized(pResampler) <= 0)
                {
                    throw new InvalidOperationException("Failed to init resampler");
                }
            }

            ContextCreated = true;
        }

        /// <summary>
        /// Initializes the Codec context as decoder.
        /// </summary>
        public void CreateDecoderContext()
        {
            CreateContext(encoder: false);
        }

        /// <summary>
        /// Initializes the Codec context as encoder.
        /// </summary>
        public void CreateEncoderContext()
        {
            CreateContext(encoder: true);
        }

        /// <summary>
        /// Send an encoded packet / frame in ffmpeg decoding queue.
        /// </summary>
        /// <returns>Return value of avcodec_send_packet: 0 on success, -1 on failure</returns>
        /// <param name="data">Encoded Data blob</param>
        internal int EnqueuePacketForDecoding(byte[] data)
        {
            if (!this.IsDecoder)
            {
                Console.WriteLine("QueuePacketForDecoding: Context is not initialized for decoding");
                return -1;
            }
            int ret;
            fixed (byte* pData = data)
            {
                pPacket->data = pData;
                pPacket->size = data.Length;

                ret = ffmpeg.avcodec_send_packet(pCodecContext, pPacket);
            }
            if (ret < 0)
            {
                Console.WriteLine($"Error: Code: {ret}, Msg:{FFmpegHelper.av_strerror(ret)}");
            }

            ffmpeg.av_packet_unref(pPacket);
            return 0;
        }

        public void Dispose()
        {
            ffmpeg.avcodec_close(pCodecContext);
            ffmpeg.av_free(pCodecContext);
            ffmpeg.av_free(pCodec);
            //ffmpeg.av_packet_free(pPacket);
            //ffmpeg.av_frame_free(pDecodedFrame);
        }
    }
}

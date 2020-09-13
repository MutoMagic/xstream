using FFmpeg.AutoGen;
using SmartGlass.Nano;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Xstream.Codec
{
    unsafe class FFmpegVideo : FFmpegBase
    {
        Queue<H264Frame> encodedDataQueue;

        int videoWidth;
        int videoHeight;
        int fps;
        AVRational avTimebase;
        uint bpp;
        uint bytes;
        ulong redMask;
        ulong greenMask;
        ulong blueMask;
        AVPixelFormat avSourcePixelFormat;
        AVPixelFormat avTargetPixelFormat;

        public event Action<YUVFrame> FrameDecoded;

        public void PushData(H264Frame data) => encodedDataQueue.Enqueue(data);

        public FFmpegVideo() : base()
        {
            encodedDataQueue = new Queue<H264Frame>();
        }

        public void Initialize(VideoFormat format)
        {
            Initialize(format.Codec, (int)format.Width, (int)format.Height, (int)format.FPS,
                       format.Bpp, format.Bytes, format.RedMask, format.GreenMask, format.BlueMask);
        }

        /// <summary>
        /// Initialize the specified codecID, videoWidth, videoHeight, fps, bpp, bytes, redMask, greenMask and blueMask.
        /// </summary>
        /// <param name="codecID">Codec identifier.</param>
        /// <param name="videoWidth">Video width.</param>
        /// <param name="videoHeight">Video height.</param>
        /// <param name="fps">Fps.</param>
        /// <param name="bpp">Bpp.</param>
        /// <param name="bytes">Bytes.</param>
        /// <param name="redMask">Red mask.</param>
        /// <param name="greenMask">Green mask.</param>
        /// <param name="blueMask">Blue mask.</param>
        public void Initialize(VideoCodec codecID, int videoWidth, int videoHeight, int fps,
                               uint bpp = 0, uint bytes = 0, ulong redMask = 0x0, ulong greenMask = 0x0, ulong blueMask = 0x0)
        {
            this.videoWidth = videoWidth;
            this.videoHeight = videoHeight;
            this.fps = fps;
            avTimebase = new AVRational { num = 1, den = fps };

            // valid only for VideoCodec.RGB
            this.bpp = bpp;
            this.bytes = bytes;
            this.redMask = redMask;
            this.greenMask = greenMask;
            this.blueMask = blueMask;

            switch (codecID)
            {
                case VideoCodec.H264:
                    avCodecID = AVCodecID.AV_CODEC_ID_H264;
                    avSourcePixelFormat = AVPixelFormat.AV_PIX_FMT_YUV420P;
                    avTargetPixelFormat = AVPixelFormat.AV_PIX_FMT_YUV420P;
                    break;
                case VideoCodec.YUV:
                    avCodecID = AVCodecID.AV_CODEC_ID_YUV4;
                    avSourcePixelFormat = AVPixelFormat.AV_PIX_FMT_YUV420P;
                    avTargetPixelFormat = AVPixelFormat.AV_PIX_FMT_YUV420P;
                    break;
                case VideoCodec.RGB:
                    throw new NotImplementedException("FFmpegVideo: VideoCodec.RGB");
                default:
                    throw new NotSupportedException($"Invalid VideoCodec: {codecID}");
            }

            if (avSourcePixelFormat != avTargetPixelFormat)
                doResample = true;
            Initialized = true;

            Debug.WriteLine($"Codec ID: {avCodecID}");
            Debug.WriteLine($"Source Pixel Format: {avSourcePixelFormat}");
            Debug.WriteLine($"Target Pixel Format: {avTargetPixelFormat}");
            Debug.WriteLine($"Resolution: {videoWidth}x{videoHeight}, FPS: {fps}");
        }

        /// <summary>
        /// Update Codec context with extradata, needed for decoding
        /// </summary>
        /// <param name="codecData">Codec specific data (SPS/PPS) in AVCC format</param>
        internal override void UpdateCodecParameters(byte[] codecData)
        {
            pCodecContext->extradata = (byte*)ffmpeg.av_mallocz((ulong)(codecData.Length + ffmpeg.AV_INPUT_BUFFER_PADDING_SIZE));
            Marshal.Copy(codecData, 0, (IntPtr)pCodecContext->extradata, codecData.Length);
            pCodecContext->extradata_size = codecData.Length;
        }

        /// <summary>
        /// Sets the codec context parameters.
        /// </summary>
        internal override void SetCodecContextParams(AVCodecContext* codecContext)
        {
            codecContext->width = videoWidth;
            codecContext->height = videoHeight;
            codecContext->time_base = avTimebase;
            codecContext->pix_fmt = avSourcePixelFormat;
        }

        /// <summary>
        /// Sets the resampler parameters.
        /// </summary>
        internal override SwrContext* CreateResampler(AVCodecContext* codecContext)
        {
            SwrContext* resampler = ffmpeg.swr_alloc();

            ffmpeg.av_opt_set_pixel_fmt(resampler, "in_pixel_fmt", codecContext->pix_fmt, 0);
            ffmpeg.av_opt_set_video_rate(resampler, "in_video_rate", codecContext->time_base, 0);
            ffmpeg.av_opt_set_image_size(resampler, "in_image_size", codecContext->width, codecContext->height, 0);

            ffmpeg.av_opt_set_pixel_fmt(resampler, "out_pixel_fmt", avTargetPixelFormat, 0);
            ffmpeg.av_opt_set_video_rate(resampler, "out_video_rate", codecContext->time_base, 0);
            ffmpeg.av_opt_set_image_size(resampler, "out_image_size", codecContext->width, codecContext->height, 0);

            ffmpeg.swr_init(resampler);
            return resampler;
        }

        public override Thread DecodingThread()
        {
            throw new NotImplementedException();
        }
    }
}

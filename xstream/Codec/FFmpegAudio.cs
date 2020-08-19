using FFmpeg.AutoGen;
using SmartGlass.Nano;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Xstream.Codec
{
    unsafe class FFmpegAudio : FFmpegBase
    {
        Queue<AACFrame> encodedDataQueue;

        uint sampleSize;
        uint sampleType;
        int sampleRate;
        int channels;
        long avChannelLayout;
        AVSampleFormat avSourceSampleFormat;
        AVSampleFormat avTargetSampleFormat;

        public event Action<PCMSample> SampleDecoded;

        // C #define PushData(AACFrame data) encodedDataQueue.Enqueue(data)
        public void PushData(AACFrame data) => encodedDataQueue.Enqueue(data);

        public FFmpegAudio() : base()
        {
            encodedDataQueue = new Queue<AACFrame>();
        }

        public void Initialize(AudioFormat format)
        {
            Initialize(format.Codec, (int)format.SampleRate, (int)format.Channels,
                       format.SampleSize, format.SampleType);
        }

        /// <summary>
        /// Initialize the specified codecID, sampleRate, channels, sampleSize and sampleType.
        /// </summary>
        /// <param name="codecID">编解码器标识符</param>
        /// <param name="sampleRate">采样率</param>
        /// <param name="channels">通道数</param>
        /// <param name="sampleSize">样本大小</param>
        /// <param name="sampleType">样本类型</param>
        public void Initialize(AudioCodec codecID, int sampleRate, int channels,
                               uint sampleSize = 0, uint sampleType = 0)
        {
            this.sampleRate = sampleRate;
            this.channels = channels;

            /* AudioCodec.PCM specific */
            this.sampleSize = sampleSize;
            this.sampleType = sampleType;
            /* specific end */

            avChannelLayout = ffmpeg.av_get_default_channel_layout(channels);// 根据通道数返回默认的通道布局（左右声道)

            switch (codecID)
            {
                case AudioCodec.AAC:
                    avCodecID = AVCodecID.AV_CODEC_ID_AAC;
                    avSourceSampleFormat = AVSampleFormat.AV_SAMPLE_FMT_FLTP;
                    avTargetSampleFormat = AVSampleFormat.AV_SAMPLE_FMT_FLT;
                    break;
                case AudioCodec.Opus:
                    avCodecID = AVCodecID.AV_CODEC_ID_OPUS;
                    avSourceSampleFormat = AVSampleFormat.AV_SAMPLE_FMT_FLTP;
                    avTargetSampleFormat = AVSampleFormat.AV_SAMPLE_FMT_FLT;
                    break;
                case AudioCodec.PCM:
                    throw new NotImplementedException("FFmpegAudio: AudioCodec.PCM");
                default:
                    throw new NotSupportedException($"Invalid AudioCodec: {codecID}");
            }

            if (avTargetSampleFormat != avSourceSampleFormat)
                doResample = true;
            Initialized = true;

            Debug.WriteLine($"Codec ID: {avCodecID}");
            Debug.WriteLine($"Source Sample Format: {avSourceSampleFormat}");
            Debug.WriteLine($"Target Sample Format: {avTargetSampleFormat}");
            Debug.WriteLine($"Channels: {channels}, SampleRate: {sampleRate}");
        }

        /// <summary>
        /// Update Codec context with extradata, needed for decoding
        /// </summary>
        /// <param name="codecData">Audiocodec specific data</param>
        internal override void UpdateCodecParameters(byte[] codecData)
        {
            // AVCodecContext->extradata: The allocated memory should be AV_INPUT_BUFFER_PADDING_SIZE bytes larger
            pCodecContext->extradata = (byte*)ffmpeg.av_mallocz((ulong)codecData.Length + ffmpeg.AV_INPUT_BUFFER_PADDING_SIZE);
            Marshal.Copy(codecData, 0, (IntPtr)pCodecContext->extradata, codecData.Length);// 在托管对象（数组）和非托管对象（IntPtr）之间进行内容的复制
            pCodecContext->extradata_size = codecData.Length;
        }

        internal override void SetCodecContextParams(AVCodecContext* codecContext)
        {
            codecContext->sample_rate = sampleRate;
            codecContext->sample_fmt = avSourceSampleFormat;
            codecContext->channels = channels;
            codecContext->channel_layout = (ulong)avChannelLayout;
        }

        internal override SwrContext* CreateResampler(AVCodecContext* codecContext)
        {
            SwrContext* resampler = ffmpeg.swr_alloc();

            // 设置源通道数
            ffmpeg.av_opt_set_int(resampler, "in_channel_count", codecContext->channels, 0);
            // 设置源通道布局
            ffmpeg.av_opt_set_channel_layout(resampler, "in_channel_layout", (long)codecContext->channel_layout, 0);
            // 设置源采样率
            ffmpeg.av_opt_set_int(resampler, "in_sample_rate", codecContext->sample_rate, 0);
            // 设置源样本格式
            ffmpeg.av_opt_set_sample_fmt(resampler, "in_sample_fmt", codecContext->sample_fmt, 0);

            ffmpeg.av_opt_set_int(resampler, "out_channel_count", codecContext->channels, 0);
            ffmpeg.av_opt_set_channel_layout(resampler, "out_channel_layout", (long)codecContext->channel_layout, 0);
            ffmpeg.av_opt_set_int(resampler, "out_sample_rate", codecContext->sample_rate, 0);
            ffmpeg.av_opt_set_sample_fmt(resampler, "out_sample_fmt", avTargetSampleFormat, 0);

            ffmpeg.swr_init(resampler);
            return resampler;
        }
    }
}

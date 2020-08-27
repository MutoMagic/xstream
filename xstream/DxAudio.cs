using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using SharpDX.XAudio2.Fx;
using System;
using System.Diagnostics;
using System.Threading;

namespace Xstream
{
    public unsafe class DxAudio
    {
        int _sampleRate;
        int _channels;

        public DxAudio(int sampleRate, int channels)
        {
            _sampleRate = sampleRate;
            _channels = channels;
        }

        /*
         * 这个samples是指样本帧中音频缓存区的大小。
         * 样本帧是一块音频数据，其大小指定为 format * channels
         * 其中format指的是每个样本的位数，这里使用WAVE_FORMAT_IEEE_FLOAT即32位4字节浮点型。
         * 在PCM中format等同于sampleSize
         * 
         * @see: https://my.oschina.net/u/4365632/blog/3319770
         *       https://wiki.libsdl.org/SDL_AudioSpec#Remarks
         */
        public int Initialize(int samples)
        {
            var xaudio2 = new XAudio2();

            // The mastering voices encapsulates an audio device.
            // It is the ultimate destination for all audio that passes through an audio graph.
            var masteringVoice = new MasteringVoice(xaudio2);

            var waveFormat = new WaveFormat(_sampleRate, 32, _channels);// BitsPerSample = 32
            var sourceVoice = new SourceVoice(xaudio2, waveFormat);

            // waveFormat.BitsPerSample / 8 * _channels * samples;
            int bufferSize = waveFormat.BlockAlign * samples;
            var dataStream = new DataStream(bufferSize, true, true);

            for (int i = 0; i < samples; i++)
            {
                double vibrato = Math.Cos(2 * Math.PI * 10.0 * i / waveFormat.SampleRate);
                float value = (float)(Math.Cos(2 * Math.PI * (220.0 + 4.0 * vibrato) * i / waveFormat.SampleRate) * 0.5);
                dataStream.Write(value);
                dataStream.Write(value);
            }
            dataStream.Position = 0;

            var audioBuffer = new AudioBuffer { Stream = dataStream, Flags = BufferFlags.EndOfStream, AudioBytes = bufferSize };

            var reverb = new Reverb(xaudio2);
            var effectDescriptor = new EffectDescriptor(reverb);
            sourceVoice.SetEffectChain(effectDescriptor);
            sourceVoice.EnableEffect(0);

            sourceVoice.SubmitSourceBuffer(audioBuffer, null);

            sourceVoice.Start();

            Console.WriteLine("Play sound");
            for (int i = 0; i < 60; i++)
            {
                Console.Write(".");
                Console.Out.Flush();
                Thread.Sleep(1000);
            }

            return 0;
        }
    }
}

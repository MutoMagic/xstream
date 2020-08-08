using System;
using System.Collections.Generic;
using System.Text;

namespace Xstream
{
    class FormAudio
    {
        int _sampleRate;
        int _channels;

        public FormAudio(int sampleRate, int channels)
        {
            _sampleRate = sampleRate;
            _channels = channels;
        }
    }
}

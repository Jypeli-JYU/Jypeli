#if ANDROID
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Media = Android.Media;

namespace Jypeli.Audio.Android
{
    internal class AudioTrackContainer
    {
        public Media.AudioTrack AudioTrack { get; set; }
        public byte[] Bytes { get; set; }

        public bool Looping { get; set; }

        public double Pitch { get; set; }

        public double Volume { get; set; }

        public AudioTrackContainer(Media.AudioTrack audioTrack, byte[] bytes)
        {
            AudioTrack = audioTrack;
            Bytes = bytes;
            Volume = 1;
        }
    }
}

#endif
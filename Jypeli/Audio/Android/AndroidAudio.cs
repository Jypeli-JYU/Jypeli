#if ANDROID
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioTrack = Android.Media.AudioTrack;
using Media = Android.Media;

namespace Jypeli.Audio.Android
{
    internal class AndroidAudio : IAudioOutput<AudioTrackContainer>
    {
        public AudioTrackContainer LoadSound(string assetName)
        {
            using Stream stream = Game.Device.StreamContent(assetName);

            return LoadSound(stream);
        }

        public AudioTrackContainer LoadSound(Stream stream)
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            return LoadSound(bytes);
        }

        public AudioTrackContainer LoadSound(byte[] bytes)
        {
            var at = new AudioTrack.Builder()
                .SetAudioAttributes(new Media.AudioAttributes.Builder()
                    .SetUsage(Media.AudioUsageKind.Game)
                    .SetContentType(Media.AudioContentType.Music)
                    .Build())
                .SetAudioFormat(new Media.AudioFormat.Builder()
                    .SetEncoding(Media.Encoding.Pcm16bit)
                    .SetChannelMask(Media.ChannelOut.Mono)
                    .SetSampleRate(44100)
                    .Build()).SetBufferSizeInBytes(bytes.Length).SetTransferMode(Media.AudioTrackMode.Static).Build();
            // TODO: AudioTrackMode.Static ei ehkä ole paras vaihtoehto jos äänitiedosto on hyvin pitkä.
            at.Write(bytes, 0, bytes.Length);

            return new AudioTrackContainer(at, bytes);
        }

        public void Destroy(AudioTrackContainer handle)
        {
            handle.AudioTrack.Release();
            handle.AudioTrack.Dispose();
        }

        public AudioTrackContainer Duplicate(AudioTrackContainer handle)
        {
            byte[] newBytes = new byte[handle.Bytes.Length];
            Array.Copy(handle.Bytes, newBytes, handle.Bytes.Length);
            return LoadSound(newBytes);
        }

        public void Play(AudioTrackContainer handle)
        {
            handle.AudioTrack.Stop();
            handle.AudioTrack.Play();
        }
        
        public void Pause(AudioTrackContainer handle)
        {
            handle.AudioTrack.Pause();
        }

        public void Stop(AudioTrackContainer handle)
        {
            handle.AudioTrack.Stop();
        }

        public double GetDuration(AudioTrackContainer handle)
        {
            return handle.AudioTrack.BufferSizeInFrames / 44100;
        }

        public bool GetLooping(AudioTrackContainer handle)
        {
            return handle.Looping;
        }

        public void SetLooping(AudioTrackContainer handle, bool value)
        {
            handle.AudioTrack.SetLoopPoints(0, handle.AudioTrack.BufferSizeInFrames, value ? -1 : 0);
            handle.Looping = value;
        }

        public double GetPitch(AudioTrackContainer handle)
        {
            return handle.Pitch;
        }

        public void SetPitch(AudioTrackContainer handle, double v)
        {
            handle.AudioTrack.PlaybackParams = new Media.PlaybackParams().SetPitch((float)v);
            handle.Pitch = v;
        }

        public double GetVolume(AudioTrackContainer handle)
        {
            return handle.Volume;
        }

        public void SetVolume(AudioTrackContainer handle, double value)
        {
            handle.AudioTrack.SetVolume((float)value);
            handle.Volume = value;
        }

        public double GetPan(AudioTrackContainer handle)
        {
            // TODO: Implement
            return 0;
        }

        public void SetPan(AudioTrackContainer handle, double value)
        {
            // TODO: Implement
        }

        public Vector GetPosition(AudioTrackContainer handle)
        {
            // TODO: Implement
            return Vector.Zero;
        }

        public void SetPosition(AudioTrackContainer handle, Vector value)
        {
            // TODO: Implement
        }
    }
}
#endif
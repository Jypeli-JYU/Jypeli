using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli.Audio
{
    internal interface IAudioOutput<T>
    {
        public double GetDuration(T handle);
        bool GetLooping(T handle);
        void SetLooping(T handle, bool value);
        double GetPan(T handle);
        void SetPan(T handle, double value);
        Vector GetPosition(T handle);
        void SetPosition(T handle, Vector value);
        double GetVolume(T handle);
        void SetVolume(T handle, double value);
        double GetPitch(T handle);
        void SetPitch(T handle, double v);
        void Play(T handle);
        void Stop(T handle);
        void Pause(T handle);
        T LoadSound(string assetName);
        T LoadSound(System.IO.Stream stream);
        T Duplicate(T handle);
        void Destroy(T handle);
    }
}

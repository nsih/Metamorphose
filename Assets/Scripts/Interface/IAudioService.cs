using FMODUnity;
using FMOD.Studio;
using UnityEngine;

namespace TJR.Core.Interface
{
    public interface IAudioService
    {
        void PlayOneShot(EventReference sound, Vector3 position);
        void PlayOneShot(string eventPath, Vector3 position);
        void PlayMusic(EventReference musicEventReference);
        void PlayMusic(string eventPath);
        void StopMusic(bool fadeOut = true);
        void SetMusicPitch(float pitch);
        EventInstance CreateInstance(EventReference eventReference);
    }
}
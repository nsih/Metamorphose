using FMODUnity;
using UnityEngine;

namespace TJR.Core.Interface
{
    public interface IAudioService
    {
        void PlayOneShot(EventReference sound, Vector3 position);
        void PlayMusic(EventReference musicEventReference);
    }
}
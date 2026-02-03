using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

namespace GamePlay
{
    public class AudioService : IDisposable
    {
        EventInstance _musicEventInstance;

        Bus _masterBus;

        List<EventInstance> _eventInstances = new List<EventInstance>();

        public AudioService()
        {
            _masterBus = RuntimeManager.GetBus("bus:/");
            Debug.Log("AudioService Constructed");
        }

        public void PlayOneShot(EventReference sound, Vector3 position)
        {
            RuntimeManager.PlayOneShot(sound, position);
        }

        public void PlayMusic(EventReference musicEventReference)
        {
            if (_musicEventInstance.isValid())
            {
                _musicEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _musicEventInstance.release();
            }

            _musicEventInstance = CreateInstance(musicEventReference);
            _musicEventInstance.start();
        }

        public EventInstance CreateInstance(EventReference eventReference)
        {
            var instance = RuntimeManager.CreateInstance(eventReference);
            _eventInstances.Add(instance);
            return instance;
        }

        void CleanUp()
        {
            foreach (var instance in _eventInstances)
            {
                instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                instance.release();
            }
            _eventInstances.Clear();

            _musicEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _musicEventInstance.release();
        }

        public void Dispose()
        {
            CleanUp();
            Debug.Log("AudioService Disposed");
        }
    }
}
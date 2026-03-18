using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using TJR.Core.Interface;

namespace GamePlay
{
    public class AudioService : IDisposable, IAudioService
    {
        EventInstance _musicEventInstance;
        Bus _masterBus;
        List<EventInstance> _eventInstances = new List<EventInstance>();

        public AudioService()
{
    _masterBus = RuntimeManager.GetBus("bus:/");
    Debug.Log("AudioService init");
    Debug.Log($"Bank 로드 여부: {RuntimeManager.HaveAllBanksLoaded}");
}

        public void PlayOneShot(EventReference sound, Vector3 position)
        {
            RuntimeManager.PlayOneShot(sound, position);
        }

        public void PlayOneShot(string eventPath, Vector3 position)
        {
            RuntimeManager.PlayOneShot(eventPath, position);
        }

        public void PlayMusic(EventReference musicEventReference)
        {
            StopCurrentMusic();
            _musicEventInstance = CreateInstance(musicEventReference);
            _musicEventInstance.start();
        }

        public void PlayMusic(string eventPath)
{
    Debug.Log($"PlayMusic 호출: {eventPath}");
    Debug.Log($"Bank 로드 여부: {RuntimeManager.HaveAllBanksLoaded}");
    StopCurrentMusic();
    _musicEventInstance = RuntimeManager.CreateInstance(eventPath);
    _eventInstances.Add(_musicEventInstance);
    var result = _musicEventInstance.start();
    Debug.Log($"start 결과: {result}");
}

        public void StopMusic(bool fadeOut = true)
        {
            if (!_musicEventInstance.isValid()) return;

            var mode = fadeOut
                ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT
                : FMOD.Studio.STOP_MODE.IMMEDIATE;
            _musicEventInstance.stop(mode);
        }

        public void SetMusicPitch(float pitch)
        {
            if (_musicEventInstance.isValid())
                _musicEventInstance.setPitch(pitch);
        }

        public EventInstance CreateInstance(EventReference eventReference)
        {
            var instance = RuntimeManager.CreateInstance(eventReference);
            _eventInstances.Add(instance);
            return instance;
        }

        void StopCurrentMusic()
        {
            if (!_musicEventInstance.isValid()) return;

            _musicEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _musicEventInstance.release();
        }

        void CleanUp()
        {
            foreach (var instance in _eventInstances)
            {
                instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                instance.release();
            }
            _eventInstances.Clear();

            StopCurrentMusic();
        }

        public void Dispose()
        {
            CleanUp();
            Debug.Log("AudioService disposed");
        }
    }
}
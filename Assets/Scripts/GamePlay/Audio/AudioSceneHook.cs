using UnityEngine;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using TJR.Core.Interface;

namespace GamePlay
{
    public class AudioSceneHook : MonoBehaviour
    {
        [Inject] private IAudioService _audio;
        [Inject] private PlayerSpawner _playerSpawner;

        [Header("피치 설정")]
        [SerializeField] private float _slowPitch = 0.75f;
        [SerializeField] private float _normalPitch = 1.0f;
        [SerializeField] private float _pitchInDuration = 0.3f;
        [SerializeField] private float _pitchOutDuration = 0.5f;

        private BulletTimeManager _bulletTime;
        private CancellationTokenSource _cts;

        void Start()
        {
            if (_playerSpawner == null)
            {
                Debug.Log("AudioSceneHook: PlayerSpawner null");
                return;
            }

            GameObject playerObj = _playerSpawner.GetPlayer();

            if (playerObj == null)
            {
                Debug.Log("AudioSceneHook: player null");
                return;
            }

            _bulletTime = playerObj.GetComponent<BulletTimeManager>();

            if (_bulletTime == null)
            {
                Debug.Log("AudioSceneHook: BulletTimeManager null");
                return;
            }

            _bulletTime.OnBulletTimeStart += OnBulletTimeStart;
            _bulletTime.OnBulletTimeEnd   += OnBulletTimeEnd;

            Debug.Log($"AudioSceneHook 연결 완료: {_bulletTime.GetInstanceID()}");
        }

        void OnDestroy()
        {
            if (_bulletTime != null)
            {
                _bulletTime.OnBulletTimeStart -= OnBulletTimeStart;
                _bulletTime.OnBulletTimeEnd   -= OnBulletTimeEnd;
            }

            CancelPitch();
        }

        void OnBulletTimeStart()
        {
            Debug.Log("OnBulletTimeStart 호출");
            CancelPitch();
            _cts = new CancellationTokenSource();
            LerpPitch(_normalPitch, _slowPitch, _pitchInDuration, _cts.Token).Forget();
        }

        void OnBulletTimeEnd()
        {
            CancelPitch();
            _cts = new CancellationTokenSource();
            LerpPitch(_slowPitch, _normalPitch, _pitchOutDuration, _cts.Token).Forget();
        }

        private async UniTaskVoid LerpPitch(float from, float to, float duration, CancellationToken token)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (token.IsCancellationRequested) return;

                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float pitch = Mathf.Lerp(from, to, t);
                _audio.SetMusicPitch(pitch);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _audio.SetMusicPitch(to);
        }

        private void CancelPitch()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}
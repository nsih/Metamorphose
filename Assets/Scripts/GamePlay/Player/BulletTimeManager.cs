using UnityEngine;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class BulletTimeManager : MonoBehaviour
{
    [Inject] private PlayerStat _playerStat;

    private float _defaultFixedDeltaTime;
    private CancellationTokenSource _cts;

    public bool IsBulletTimeActive { get; private set; }

    // 구독 포인트: 시각 연출, FMOD 훅 등
    public event Action OnBulletTimeStart;
    public event Action OnBulletTimeEnd;

    void Awake()
    {
        Debug.Log($"BulletTimeManager instanceID: {GetInstanceID()}");
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void TriggerSlowMotion()
    {
        if (_playerStat == null) return;

        bool wasActive = IsBulletTimeActive;

        CancelCurrentTask();

        _cts = new CancellationTokenSource();
        ProcessBulletTime(_playerStat.SlowMotionDuration, _playerStat.TimeSlowFactor, wasActive, _cts.Token).Forget();
    }

    private async UniTaskVoid ProcessBulletTime(float duration, float scale, bool wasActive, CancellationToken token)
    {
        IsBulletTimeActive = true;

        try
        {
            SetTimeScale(scale);

            // 이미 활성 중이었으면 이벤트 재발행 안함
            if (!wasActive)
            {
                OnBulletTimeStart?.Invoke();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(duration), ignoreTimeScale: true, cancellationToken: token);
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                SetTimeScale(1f);
                OnBulletTimeEnd?.Invoke();
                IsBulletTimeActive = false;
                DisposeCTS();
            }
        }
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = _defaultFixedDeltaTime * scale;
    }

    private void CancelCurrentTask()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            DisposeCTS();
        }
    }

    private void DisposeCTS()
    {
        _cts?.Dispose();
        _cts = null;
    }

    void OnDestroy()
    {
        CancelCurrentTask();
        SetTimeScale(1f);
    }
}
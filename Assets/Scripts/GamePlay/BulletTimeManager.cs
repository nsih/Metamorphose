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

    // 외부에서 상태를 확인해야 한다면 프로퍼티로 제공
    public bool IsBulletTimeActive { get; private set; }

    void Awake()
    {
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void TriggerSlowMotion()
    {
        if (_playerStat == null) return;

        // 1. 기존에 실행 중인 불렛타임이 있다면 취소 (타이머 리셋 효과)
        CancelCurrentTask();

        // 2. 새로운 토큰 생성 및 실행
        _cts = new CancellationTokenSource();
        ProcessBulletTime(_playerStat.SlowMotionDuration, _playerStat.TimeSlowFactor, _cts.Token).Forget();
    }

    private async UniTaskVoid ProcessBulletTime(float duration, float scale, CancellationToken token)
    {
        // 상태값 변경
        IsBulletTimeActive = true;
        
        try
        {
            // 1. 시간 느리게 설정
            SetTimeScale(scale);
            Debug.Log($"Bullet Time On: {duration}s");

            // 2. 대기 (ignoreTimeScale: true 필수)
            // 게임 시간이 느려져도, 실제 시간(Unscaled Time) 기준으로 duration만큼 기다림
            await UniTask.Delay(TimeSpan.FromSeconds(duration), ignoreTimeScale: true, cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            // TriggerSlowMotion에서 CancelCurrentTask()를 호출했을 때 여기로 튐.
            // 여기서는 딱히 할 게 없음 (새로운 태스크가 TimeScale을 덮어쓸 것이므로)
        }
        finally
        {
            // 3. 종료 처리
            // 토큰이 취소된 경우(새 불렛타임 시작)가 아니라, 진짜 시간이 다 돼서 끝난 경우에만 원복
            if (!token.IsCancellationRequested)
            {
                SetTimeScale(1f);
                IsBulletTimeActive = false;
                Debug.Log("Bullet Time Off");
                DisposeCTS();
            }
        }
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        // 물리 연산 안정성을 위해 fixedDeltaTime도 비율에 맞춰 조정 (아주 중요)
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
        // 씬 이동이나 오브젝트 파괴 시 안전하게 정리
        CancelCurrentTask();
        SetTimeScale(1f); // 강제 원복
    }
}
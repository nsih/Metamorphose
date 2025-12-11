using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerModel : IDisposable
{
    // 유사 파사드
    private PlayerStat _stat;
    private CancellationTokenSource _cts; // 비동기 작업 취소용

    
    //초기화는 SO로 초기화 되는 데이터
    public float MaxHP => _stat.MaxHealth;
    public float MoveSpeed => _stat.MoveSpeed;
    public float JumpForce => _stat.JumpForce;
    public float DashSpeed => _stat.DashSpeed;
    public float DashDuration => _stat.DashDuration;
    public int MaxDashChargeStack => _stat.MaxDashChargeStack;
    public float DashChargeTime => _stat.DashChargeTime;
    public float TimeSlowFactor => _stat.TimeSlowFactor;
    public float SlowMotionDuration => _stat.SlowMotionDuration;


    // Runtime Data
    public AsyncReactiveProperty<float> CurrentHP { get; private set; }
    public AsyncReactiveProperty<int> CurrentDashCount { get; private set; }
    public AsyncReactiveProperty<float> DashCooldownNormalized { get; private set; }


    public PlayerModel(PlayerStat stat)
    {
        _stat = stat;
        _cts = new CancellationTokenSource();

        CurrentHP = new AsyncReactiveProperty<float>(_stat.MaxHealth);
        CurrentDashCount = new AsyncReactiveProperty<int>(_stat.MaxDashChargeStack);
        DashCooldownNormalized = new AsyncReactiveProperty<float>(0f);

        StartCooldownLoop(_cts.Token).Forget();
    }

    private async UniTaskVoid StartCooldownLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.WaitUntil(() => CurrentDashCount.Value < MaxDashChargeStack, cancellationToken: token);

            float localTimer = 0f; 

            while (localTimer < DashChargeTime)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);

                // 불렛타임 무시 (Unscaled Time)
                localTimer += Time.unscaledDeltaTime;

                DashCooldownNormalized.Value = localTimer / DashChargeTime;
            }

            CurrentDashCount.Value++;
            DashCooldownNormalized.Value = 0f;
        }
    }

    public void TakeDamage(float amount)
    {
        CurrentHP.Value -= amount;
        if (CurrentHP.Value < 0) CurrentHP.Value = 0;
    }

    public bool TryConsumeDash()
    {
        if (CurrentDashCount.Value > 0)
        {
            CurrentDashCount.Value--;

            return true;
        }
        return false;
    }


    public void Dispose()
    {
        _cts?.Cancel(); // 루프 정지
        _cts?.Dispose();
        CurrentHP?.Dispose();
        CurrentDashCount?.Dispose();
        DashCooldownNormalized?.Dispose();
    }
}
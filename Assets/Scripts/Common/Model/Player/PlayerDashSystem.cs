using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;

public class PlayerDashSystem : IDisposable
{
    public float DashSpeed { get; private set; }
    public float DashDuration { get; private set; }
    public int MaxDashChargeStack { get; private set; }
    public float DashChargeTime { get; private set; }

    public ReactiveProperty<int> CurrentDashCount { get; private set; }
    public ReactiveProperty<float> DashCooldownNormalized { get; private set; }

    private CancellationTokenSource _cts;

    public PlayerDashSystem(PlayerStat stat)
    {
        DashSpeed = stat.DashSpeed;
        DashDuration = stat.DashDuration;
        MaxDashChargeStack = stat.MaxDashChargeStack;
        DashChargeTime = stat.DashChargeTime;

        CurrentDashCount = new ReactiveProperty<int>(MaxDashChargeStack);
        DashCooldownNormalized = new ReactiveProperty<float>(0f);

        _cts = new CancellationTokenSource();
        StartCooldownLoop(_cts.Token).Forget();
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

    public void Reset()
    {
        // 기존 루프 종료
        _cts?.Cancel();
        _cts?.Dispose();

        CurrentDashCount.Value = MaxDashChargeStack;
        DashCooldownNormalized.Value = 0f;

        // 루프 재시작
        _cts = new CancellationTokenSource();
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
                localTimer += Time.unscaledDeltaTime;
                DashCooldownNormalized.Value = localTimer / DashChargeTime;
            }

            CurrentDashCount.Value++;
            DashCooldownNormalized.Value = 0f;
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        CurrentDashCount?.Dispose();
        DashCooldownNormalized?.Dispose();
    }
}
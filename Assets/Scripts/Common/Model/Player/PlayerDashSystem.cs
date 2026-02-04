using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerDashSystem
{
    public float DashSpeed { get; private set; }
    public float DashDuration { get; private set; }
    public int MaxDashChargeStack { get; private set; }
    public float DashChargeTime { get; private set; }
    
    public AsyncReactiveProperty<int> CurrentDashCount { get; private set; }
    public AsyncReactiveProperty<float> DashCooldownNormalized { get; private set; }
    
    private CancellationTokenSource _cts;

    public PlayerDashSystem(PlayerStat stat)
    {
        DashSpeed = stat.DashSpeed;
        DashDuration = stat.DashDuration;
        MaxDashChargeStack = stat.MaxDashChargeStack;
        DashChargeTime = stat.DashChargeTime;
        
        CurrentDashCount = new AsyncReactiveProperty<int>(MaxDashChargeStack);
        DashCooldownNormalized = new AsyncReactiveProperty<float>(0f);
        
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
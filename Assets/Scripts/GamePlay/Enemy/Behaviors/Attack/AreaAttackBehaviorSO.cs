// Assets/Scripts/GamePlay/Enemy/Behaviors/Attack/AreaAttackBehaviorSO.cs
// 2026-04-26
// Rect 형태 제거에 따라 ResolveAngle 폐기. Activate 시그니처 단순화

using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

[CreateAssetMenu(fileName = "Attack_Area", menuName = "SO/Enemy/Behaviors/Attack/AreaAttack")]
public class AreaAttackBehaviorSO : EnemyAttackBehaviorSO
{
    public List<AreaAttackEntry> Entries;

    public override void Execute(EnemyContext ctx)
    {
        if (ctx.IsDead) return;
        if (ctx.Target == null) return;

        if (Entries == null || Entries.Count == 0) return;

        if (ctx.AreaPool == null)
        {
            Debug.LogWarning("area pool null");
            return;
        }

        // 쿨타임
        float timeSinceLastAttack = Time.time - ctx.LastAttackTime;
        if (timeSinceLastAttack < ctx.CurrentAttackCoolTime) return;

        // 시퀀스 진행 중
        if (ctx.GetInt(EnemyContextKeys.AreaAttackActive) == 1) return;

        ctx.SetInt(EnemyContextKeys.AreaAttackActive, 1);
        ctx.LastAttackTime = Time.time;

        var token = ctx.DestroyCancellationToken;
        FireSequenceAsync(ctx, token).Forget();
    }

    private async UniTaskVoid FireSequenceAsync(EnemyContext ctx, CancellationToken token)
    {
        try
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                if (entry.Config == null) continue;

                if (entry.Delay > 0f)
                {
                    float waited = 0f;
                    while (waited < entry.Delay)
                    {
                        token.ThrowIfCancellationRequested();
                        waited += Time.deltaTime;
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                    }
                }

                Vector3 spawnPos = ResolvePosition(entry.Config, ctx);
                var indicator = ctx.AreaPool.Get();
                indicator.Activate(entry.Config, spawnPos, ctx.Target, ctx.Self, ctx.AreaPool);
            }
        }
        catch (System.OperationCanceledException)
        {
            // 시퀀스 중단
        }
        finally
        {
            ctx.SetInt(EnemyContextKeys.AreaAttackActive, 0);
        }
    }

    private Vector3 ResolvePosition(AreaAttackConfigSO config, EnemyContext ctx)
    {
        switch (config.PositionStrategy)
        {
            case AreaPositionStrategy.TargetPosition:
                return ctx.Target.position;

            case AreaPositionStrategy.OwnerPosition:
                return ctx.Self.position;

            case AreaPositionStrategy.OwnerRelativeFixed:
                return ctx.Self.position + (Vector3)config.FixedOffset;

            case AreaPositionStrategy.OwnerRelativeRandom:
                Vector2 randOffset = Random.insideUnitCircle * config.RandomRadius;
                return ctx.Self.position + (Vector3)randOffset;

            case AreaPositionStrategy.WorldFixed:
                return (Vector3)config.WorldPosition;

            case AreaPositionStrategy.WorldRandom:
                float x = Random.Range(config.RandomBounds.xMin, config.RandomBounds.xMax);
                float y = Random.Range(config.RandomBounds.yMin, config.RandomBounds.yMax);
                return new Vector3(x, y, 0f);

            default:
                return ctx.Target.position;
        }
    }

    private void OnValidate()
    {
        if (Entries == null || Entries.Count == 0)
        {
            Debug.LogWarning($"{name}: Entries 비어있음");
            return;
        }

        for (int i = 0; i < Entries.Count; i++)
        {
            if (Entries[i].Config == null)
                Debug.LogWarning($"{name}: Entries[{i}] Config null");
        }
    }
}
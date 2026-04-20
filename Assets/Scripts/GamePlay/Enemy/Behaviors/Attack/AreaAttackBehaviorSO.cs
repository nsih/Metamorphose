// Assets/Scripts/GamePlay/Enemy/Behaviors/Attack/AreaAttackBehaviorSO.cs
// 2026-04-20 장판 공격 행동 SO 신규
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "Attack_Area", menuName = "SO/Enemy/Behaviors/Attack/AreaAttack")]
public class AreaAttackBehaviorSO : EnemyAttackBehaviorSO
{
    public List<AreaAttackEntry> Entries;

    public override void Execute(EnemyContext ctx)
    {
        if (ctx.IsDead) return;
        if (ctx.Target == null) return;

        if (Time.time - ctx.LastAttackTime < ctx.CurrentAttackCoolTime) return;

        // 시퀀스 진행 중
        if (ctx.GetInt(EnemyContextKeys.AreaAttackActive) == 1) return;

        if (ctx.AreaPool == null)
        {
            Debug.LogWarning("AreaAttackBehaviorSO: AreaPool null");
            return;
        }

        if (Entries == null || Entries.Count == 0) return;

        ctx.SetInt(EnemyContextKeys.AreaAttackActive, 1);
        ctx.LastAttackTime = Time.time;

        FireSequenceAsync(ctx).Forget();
    }

    private async UniTaskVoid FireSequenceAsync(EnemyContext ctx)
    {
        var token = ctx.DestroyCancellationToken;

        try
        {
            foreach (var entry in Entries)
            {
                if (token.IsCancellationRequested) break;
                if (entry.Config == null) continue;

                if (entry.Delay > 0f)
                {
                    float elapsed = 0f;
                    while (elapsed < entry.Delay)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                        elapsed += Time.deltaTime;
                    }
                }

                if (token.IsCancellationRequested) break;

                Vector3 position = ResolvePosition(entry.Config, ctx);
                float angle = ResolveAngle(entry.Config, ctx);

                AreaIndicator indicator = ctx.AreaPool.Get();
                indicator.Activate(entry.Config, position, angle, ctx.Target, ctx.Self, ctx.AreaPool);
            }
        }
        catch (System.OperationCanceledException) { }
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
                return ctx.Target != null ? ctx.Target.position : ctx.Self.position;

            case AreaPositionStrategy.OwnerPosition:
                return ctx.Self.position;

            case AreaPositionStrategy.OwnerRelativeFixed:
                return ctx.Self.position + (Vector3)config.FixedOffset;

            case AreaPositionStrategy.OwnerRelativeRandom:
                Vector2 random = Random.insideUnitCircle * config.RandomRadius;
                return ctx.Self.position + (Vector3)random;

            case AreaPositionStrategy.WorldFixed:
                return (Vector3)config.WorldPosition;

            case AreaPositionStrategy.WorldRandom:
                float rx = Random.Range(config.RandomBounds.xMin, config.RandomBounds.xMax);
                float ry = Random.Range(config.RandomBounds.yMin, config.RandomBounds.yMax);
                return new Vector3(rx, ry, 0f);

            default:
                return ctx.Self.position;
        }
    }

    private float ResolveAngle(AreaAttackConfigSO config, EnemyContext ctx)
    {
        switch (config.Direction)
        {
            case AreaDirection.None:
                return 0f;

            case AreaDirection.TowardTarget:
                if (ctx.Target == null) return 0f;
                Vector2 dir = ctx.Target.position - ctx.Self.position;
                return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            case AreaDirection.FixedAngle:
                return config.FixedAngleDeg;

            case AreaDirection.OwnerForward:
                return Mathf.Atan2(ctx.Self.right.y, ctx.Self.right.x) * Mathf.Rad2Deg;

            default:
                return 0f;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Entries == null) return;
        for (int i = 0; i < Entries.Count; i++)
        {
            if (Entries[i].Config == null)
                Debug.LogWarning($"AreaAttackBehaviorSO: Entries[{i}] Config null");
        }
    }
#endif
}

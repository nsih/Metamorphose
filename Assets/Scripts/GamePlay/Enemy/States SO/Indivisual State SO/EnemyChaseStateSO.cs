using UnityEngine;
using Common;

[CreateAssetMenu(fileName = "EnemyChaseState", menuName = "SO/Enemy/States/Chase")]
public class EnemyChaseStateSO : EnemyStateSO
{
    public override EnemyState StateType => EnemyState.Chase;

    public override void Enter(EnemyContext ctx)
    {
        if (ctx.Movement != null)
            ctx.Movement.enabled = true;
    }

    public override void Execute(EnemyContext ctx)
    {
        if (ctx.Target == null) return;
        if (ctx.CurrentMoveStrategy == null) return;

        ctx.CurrentMoveStrategy.Move(ctx.Self, ctx.Target, ctx.Data);
    }

    public override void Exit(EnemyContext ctx)
    {
        if (ctx.Movement != null)
            ctx.Movement.enabled = false;
    }

    public override void Reset(EnemyContext ctx)
    {
        if (ctx.Movement != null)
            ctx.Movement.enabled = false;
    }
}
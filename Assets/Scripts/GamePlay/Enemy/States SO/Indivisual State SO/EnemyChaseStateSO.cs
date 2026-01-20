using UnityEngine;
using Common;

[CreateAssetMenu(fileName = "EnemyChaseState", menuName = "SO/Enemy/States/Chase")]
public class EnemyChaseStateSO : EnemyStateSO
{
    public override EnemyState StateType => EnemyState.Chase;

    public override void Enter(EnemyContext ctx)
    {
        
    }

    public override void Execute(EnemyContext ctx)
    {
        if (ctx.Target == null || ctx.CurrentMoveStrategy == null) return;
        
        ctx.Movement.ExecuteMove(ctx.CurrentMoveStrategy);
    }

    public override void Exit(EnemyContext ctx)
    {
        
    }

    public override void Reset(EnemyContext ctx)
    {
        
    }
}
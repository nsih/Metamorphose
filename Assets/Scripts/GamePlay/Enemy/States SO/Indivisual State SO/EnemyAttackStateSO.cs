using UnityEngine;
using Common;

[CreateAssetMenu(fileName = "EnemyAttackState", menuName = "SO/Enemy/States/Attack")]
public class EnemyAttackStateSO : EnemyStateSO
{
    public override EnemyState StateType => EnemyState.Attack;

    public override void Enter(EnemyContext ctx)
    {
        
    }

    public override void Execute(EnemyContext ctx)
    {
        if (ctx.Target == null) return;
        if (ctx.CurrentAttackStrategy == null) return;

        float timeSinceLastAttack = Time.time - ctx.LastAttackTime;
        
        if (timeSinceLastAttack >= ctx.CurrentAttackCoolTime)
        {
            ctx.CurrentAttackStrategy.Attack(ctx.Self, ctx.Target);
            ctx.LastAttackTime = Time.time;
        }
    }

    public override void Exit(EnemyContext ctx)
    {
        if (ctx.Emitter != null)
            ctx.Emitter.Stop();
    }

    public override void Reset(EnemyContext ctx)
    {
        ctx.LastAttackTime = 0f;
        
        if (ctx.Emitter != null)
            ctx.Emitter.Kill();
    }
}
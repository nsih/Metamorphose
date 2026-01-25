using UnityEngine;

[CreateAssetMenu(fileName = "Move_Chase", menuName = "SO/Enemy/Behaviors/Move/Chase")]
public class ChaseBehaviorSO : EnemyMoveBehaviorSO
{
    public override void Execute(EnemyContext ctx)
    {
        if (ctx.IsDead) return;
        if (ctx.Target == null) return;
        if (ctx.Rigidbody == null) return;
        
        Vector2 dir = (ctx.Target.position - ctx.Self.position).normalized;
        ctx.Rigidbody.linearVelocity = dir * ctx.CurrentMoveSpeed;
    }
}
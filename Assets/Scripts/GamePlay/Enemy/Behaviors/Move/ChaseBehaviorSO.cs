using UnityEngine;

[CreateAssetMenu(fileName = "Move_Chase", menuName = "SO/Enemy/Behaviors/Move/Chase")]
public class ChaseBehaviorSO : EnemyMoveBehaviorSO
{
    public override void Execute(EnemyContext ctx)
    {
        if (ctx.IsDead) return;
        if (ctx.Target == null) return;
        
        Vector3 dir = (ctx.Target.position - ctx.Self.position).normalized;
        ctx.Self.position += dir * ctx.CurrentMoveSpeed * Time.deltaTime;
    }
}
using UnityEngine;
using BulletPro;

[CreateAssetMenu(fileName = "Attack_Shoot", menuName = "SO/Enemy/Behaviors/Attack/Shoot")]
public class ShootBehaviorSO : EnemyAttackBehaviorSO
{
    public EmitterProfile Pattern;
    
    public override void Execute(EnemyContext ctx)
    {
        if (ctx.IsDead) return;
        if (ctx.Target == null) return;
        if (ctx.Emitter == null) return;
        
        float timeSinceLastAttack = Time.time - ctx.LastAttackTime;
        if (timeSinceLastAttack < ctx.CurrentAttackCoolTime) return;
        
        if (ctx.Emitter.emitterProfile != Pattern)
            ctx.Emitter.emitterProfile = Pattern;
        
        ctx.Emitter.Stop();
        ctx.Emitter.Play();
        ctx.LastAttackTime = Time.time;
        
        int count = ctx.GetInt(EnemyContextKeys.ShootCount);
        count++;
        ctx.SetInt(EnemyContextKeys.ShootCount, count);
        
        Debug.Log($"Shoot count: {count}");
    }
}
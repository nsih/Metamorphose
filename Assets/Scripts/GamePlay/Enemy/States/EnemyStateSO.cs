using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "State_", menuName = "SO/Enemy/State")]
public class EnemyStateSO : ScriptableObject
{
    [Header("Behaviors")]
    public EnemyMoveBehaviorSO MoveBehavior;
    public EnemyAttackBehaviorSO AttackBehavior;
    public List<EnemyUtilityBehaviorSO> UtilityBehaviors;
    
    [Header("State Settings")]
    [Tooltip("이 상태 진입 시 Emitter 정지")]
    public bool StopEmitterOnEnter;
    
    [Tooltip("이 상태 이탈 시 Emitter 정지")]
    public bool StopEmitterOnExit = true;

    public virtual void Enter(EnemyContext ctx)
    {
        if (StopEmitterOnEnter && ctx.Emitter != null)
            ctx.Emitter.Stop();
        
        if (ctx.Rigidbody != null)
            ctx.Rigidbody.linearVelocity = Vector2.zero;
    }

    public virtual void Execute(EnemyContext ctx)
    {
        MoveBehavior?.Execute(ctx);
        AttackBehavior?.Execute(ctx);
        
        if (UtilityBehaviors != null)
        {
            foreach (var behavior in UtilityBehaviors)
            {
                behavior?.Execute(ctx);
            }
        }
    }

    public virtual void Exit(EnemyContext ctx)
    {
        if (StopEmitterOnExit && ctx.Emitter != null)
            ctx.Emitter.Stop();
        
        if (ctx.Rigidbody != null)
            ctx.Rigidbody.linearVelocity = Vector2.zero;
    }
}
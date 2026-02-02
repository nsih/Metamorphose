using UnityEngine;

[CreateAssetMenu(fileName = "State_", menuName = "SO/Enemy/State/Resettable")]
public class ResettableStateSO : EnemyStateSO
{
    [Header("Reset Options")]
    public bool ResetShootCount;
    public bool ResetTeleportComplete;
    
    public override void Enter(EnemyContext ctx)
    {
        base.Enter(ctx);
        
        if (ResetShootCount)
            ctx.SetInt(EnemyContextKeys.ShootCount, 0);
        
        if (ResetTeleportComplete)
            ctx.SetInt(EnemyContextKeys.TeleportComplete, 0);
    }
}
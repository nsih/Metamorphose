using UnityEngine;

[CreateAssetMenu(fileName = "State_", menuName = "SO/Enemy/State/Resettable")]
public class ResettableStateSO : EnemyStateSO
{
    private static readonly int ShootCountKey = "ShootCount".GetHashCode();
    private static readonly int TeleportCompleteKey = "TeleportComplete".GetHashCode();
    
    [Header("Reset Options")]
    public bool ResetShootCount;
    public bool ResetTeleportComplete;
    
    public override void Enter(EnemyContext ctx)
    {
        base.Enter(ctx);
        
        if (ResetShootCount)
            ctx.SetInt(ShootCountKey, 0);
        
        if (ResetTeleportComplete)
            ctx.SetInt(TeleportCompleteKey, 0);
    }
}
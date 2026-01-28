using UnityEngine;

[CreateAssetMenu(fileName = "State_Teleport", menuName = "SO/Enemy/State/Teleport")]
public class TeleportStateSO : EnemyStateSO
{
    private static readonly int TeleportCompleteKey = "TeleportComplete".GetHashCode();
    
    public override void Enter(EnemyContext ctx)
    {
        base.Enter(ctx);
        ctx.SetInt(TeleportCompleteKey, 0);
        //complete condition 초기화를 위한 state
    }
}
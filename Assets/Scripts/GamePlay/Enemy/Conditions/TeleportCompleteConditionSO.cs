using UnityEngine;

[CreateAssetMenu(fileName = "Cond_TeleportComplete", menuName = "SO/Enemy/Conditions/TeleportComplete")]
public class TeleportCompleteConditionSO : TransitionConditionSO
{
    private static readonly int TeleportCompleteKey = "TeleportComplete".GetHashCode();
    
    public override bool Evaluate(EnemyContext ctx)
    {
        return ctx.GetInt(TeleportCompleteKey) == 1;
    }
}
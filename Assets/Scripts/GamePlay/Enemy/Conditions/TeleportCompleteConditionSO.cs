using UnityEngine;

[CreateAssetMenu(fileName = "Cond_TeleportComplete", menuName = "SO/Enemy/Conditions/TeleportComplete")]
public class TeleportCompleteConditionSO : TransitionConditionSO
{
    public override bool Evaluate(EnemyContext ctx)
    {
        return ctx.GetInt(EnemyContextKeys.TeleportComplete) == 1;
    }
}
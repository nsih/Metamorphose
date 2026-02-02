using UnityEngine;

[CreateAssetMenu(fileName = "Cond_ShootCount", menuName = "SO/Enemy/Conditions/ShootCount")]
public class ShootCountConditionSO : TransitionConditionSO
{
    public int RequiredCount = 1;
    
    public override bool Evaluate(EnemyContext ctx)
    {
        int count = ctx.GetInt(EnemyContextKeys.ShootCount);
        return count >= RequiredCount;
    }
}
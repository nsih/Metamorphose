using UnityEngine;

[CreateAssetMenu(fileName = "Cond_ShootCount", menuName = "SO/Enemy/Conditions/ShootCount")]
public class ShootCountConditionSO : TransitionConditionSO
{
    public int RequiredCount = 1;
    
    private static readonly int ShootCountKey = "ShootCount".GetHashCode();
    
    public override bool Evaluate(EnemyContext ctx)
    {
        int count = ctx.GetInt(ShootCountKey);
        return count >= RequiredCount;
    }
}
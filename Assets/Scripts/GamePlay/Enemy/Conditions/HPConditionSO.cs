using UnityEngine;

[CreateAssetMenu(fileName = "Cond_HP", menuName = "SO/Enemy/Conditions/HP")]
public class HPConditionSO : TransitionConditionSO
{
    [Range(0f, 1f)]
    public float ThresholdPercent;
    public bool Below;
    
    public override bool Evaluate(EnemyContext ctx)
    {
        float ratio = (float)ctx.CurrentHP / ctx.MaxHP;
        
        if (Below)
            return ratio <= ThresholdPercent;
        else
            return ratio >= ThresholdPercent;
    }
}
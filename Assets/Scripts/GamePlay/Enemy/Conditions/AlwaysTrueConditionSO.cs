using UnityEngine;

[CreateAssetMenu(fileName = "Cond_AlwaysTrue", menuName = "SO/Enemy/Conditions/AlwaysTrue")]
public class AlwaysTrueConditionSO : TransitionConditionSO
{
    public override bool Evaluate(EnemyContext ctx)
    {
        return true;
    }
}
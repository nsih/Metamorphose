using UnityEngine;

[CreateAssetMenu(fileName = "Cond_Distance", menuName = "SO/Enemy/Conditions/Distance")]
public class DistanceConditionSO : TransitionConditionSO
{
    public enum CompareType { Less, Greater }
    
    public CompareType Compare;
    public float Distance;
    
    public override bool Evaluate(EnemyContext ctx)
    {
        if (ctx.Target == null) return false;
        
        float dist = ctx.DistanceToTarget;
        
        if (Compare == CompareType.Less)
            return dist < Distance;
        else
            return dist > Distance;
    }
}
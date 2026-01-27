using UnityEngine;

[CreateAssetMenu(fileName = "Cond_TimeInState", menuName = "SO/Enemy/Conditions/TimeInState")]
public class TimeInStateConditionSO : TransitionConditionSO
{
    [Tooltip("이 상태에 머문 시간(초)")]
    public float Duration = 2f;
    
    public override bool Evaluate(EnemyContext ctx)
    {
        return ctx.TimeInCurrentState >= Duration;
    }
}
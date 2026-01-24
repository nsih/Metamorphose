using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Transition_", menuName = "SO/Enemy/Transition")]
public class TransitionRuleSO : ScriptableObject
{
    public EnemyStateSO FromState;
    public EnemyStateSO ToState;
    
    [Tooltip("모든 조건이 만족해야 상태전이")]
    public List<TransitionConditionSO> Conditions;
    
    public bool Evaluate(EnemyContext ctx)
    {
        if (Conditions == null || Conditions.Count == 0)
            return false;
        
        foreach (var condition in Conditions)
        {
            if (condition == null) continue;
            
            if (!condition.Evaluate(ctx))
                return false;
        }
        
        return true;
    }
}
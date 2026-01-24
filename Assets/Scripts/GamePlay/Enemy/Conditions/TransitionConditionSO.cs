using UnityEngine;

public abstract class TransitionConditionSO : ScriptableObject
{
    public abstract bool Evaluate(EnemyContext ctx);
}
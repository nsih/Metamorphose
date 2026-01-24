using UnityEngine;

public abstract class EnemyBehaviorSO : ScriptableObject
{
    public abstract void Execute(EnemyContext ctx);
}
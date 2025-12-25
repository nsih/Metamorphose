using UnityEngine;

public abstract class EnemyAttackStrategySO : ScriptableObject
{
    public abstract void Attack(Transform enemy, Transform target);
}

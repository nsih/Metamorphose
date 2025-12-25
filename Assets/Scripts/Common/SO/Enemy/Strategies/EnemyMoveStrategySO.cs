using UnityEngine;

public abstract class EnemyMoveStrategySO : ScriptableObject
{
    public abstract void Move(Transform enemy, Transform target, EnemyDataSO stats);
}
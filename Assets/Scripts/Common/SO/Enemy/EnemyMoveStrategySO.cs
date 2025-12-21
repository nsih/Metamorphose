using UnityEngine;

[CreateAssetMenu(fileName = "EnemyMoveStrategySO", menuName = "Scriptable Objects/EnemyMoveStrategySO")]
public abstract class EnemyMoveStrategySO : ScriptableObject
{
    public abstract void Move(Transform enemy, Transform target, EnemyDataSO stats);
}
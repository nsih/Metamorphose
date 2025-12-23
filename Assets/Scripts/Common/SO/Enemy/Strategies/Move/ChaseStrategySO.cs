using UnityEngine;

[CreateAssetMenu(fileName = "ChaseStrategy", menuName = "SO/Movement/Chase")]
public class ChaseStrategySO : EnemyMoveStrategySO
{
    public override void Move(Transform enemy, Transform target, EnemyDataSO stats)
    {
        if (target == null) return;

        Vector3 dir = (target.position - enemy.position).normalized;

        enemy.position += dir * stats.MoveSpeed * Time.deltaTime;
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "GroundChaseStrategy", menuName = "SO/Movement/Ground Chase")]
public class GroundChaseStrategySO : EnemyMoveStrategySO
{
    public override void Move(Transform enemy, Transform target, EnemyDataSO stats)
    {
        float sizeX = Mathf.Abs(enemy.localScale.x);
        float sizeY = enemy.localScale.y;
        float sizeZ = enemy.localScale.z;

        if (target == null) return;

        Vector3 targetPos = target.position;
        targetPos.y = enemy.position.y; 
        Vector3 dir = (targetPos - enemy.position).normalized;

        enemy.position += dir * stats.MoveSpeed * Time.deltaTime;
        
        if (dir.x > 0) 
            enemy.localScale = new Vector3(sizeX, sizeY, sizeZ);
        else if (dir.x < 0) 
            enemy.localScale = new Vector3(-sizeX, sizeY, sizeZ);
    }
}
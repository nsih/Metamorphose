using UnityEngine;

public class EnemyFactory
{
    public EnemyFactory()
    {
    }

    public EnemyHealth Create(Vector3 position, EnemyDataSO data)
    {
        // get
        EnemyHealth enemy = EnemyPoolManager.Instance.Get();

        // location
        enemy.transform.position = position;
        enemy.transform.rotation = Quaternion.identity;

        // SO 주입
        enemy.Initialize(data);

        // Pool disable
        enemy.SetReleaseAction(() => EnemyPoolManager.Instance.Release(enemy));

        return enemy;
    }
}
using UnityEngine;

public class EnemyFactory
{
    private Transform _playerTransform;

    public EnemyFactory(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    public Enemy Create(Vector3 position, EnemyDataSO data)
    {
        Enemy enemy = EnemyPoolManager.Instance.Get();

        enemy.transform.position = position;
        enemy.transform.rotation = Quaternion.identity;

        enemy.Initialize(data);
        enemy.SetTarget(_playerTransform);
        enemy.SetReleaseAction(() => EnemyPoolManager.Instance.Release(enemy));

        return enemy;
    }
}
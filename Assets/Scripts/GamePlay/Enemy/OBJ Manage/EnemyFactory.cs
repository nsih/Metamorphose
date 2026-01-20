using UnityEngine;

public class EnemyFactory
{
    private readonly Transform _playerTransform;
    private readonly EnemyPoolManager _poolManager;

    public EnemyFactory(Transform playerTransform, EnemyPoolManager poolManager)
    {
        _playerTransform = playerTransform;
        _poolManager = poolManager;
    }

    public Enemy Create(Vector3 position, EnemyDataSO data)
    {
        Enemy enemy = _poolManager.Get();

        enemy.transform.position = position;
        enemy.transform.rotation = Quaternion.identity;

        enemy.Initialize(data);
        enemy.SetTarget(_playerTransform);
        enemy.SetReleaseAction(() => _poolManager.Release(enemy));

        return enemy;
    }
}
using UnityEngine;
using UnityEngine.Pool;
using Reflex.Core;
using Reflex.Attributes;
using Reflex.Injectors;

public class EnemyPoolManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Enemy _prefab;
    [SerializeField] private int _defaultCapacity = 20;
    [SerializeField] private int _maxSize = 100;

    [Header("Debug Info")]
    [SerializeField] private int _totalCount;
    [SerializeField] private int _activeCount;
    [SerializeField] private int _inactiveCount;

    [Inject] private Container _container;

    private ObjectPool<Enemy> _pool;

    private void Awake()
    {
        InitPool();
    }

    private void Update()
    {
        if (_pool != null)
        {
            _totalCount = _pool.CountAll;
            _activeCount = _pool.CountActive;
            _inactiveCount = _pool.CountInactive;
        }
    }

    private void InitPool()
    {
        _pool = new ObjectPool<Enemy>(
            createFunc: CreateEnemy,
            actionOnGet: OnGetEnemy,
            actionOnRelease: OnReleaseEnemy,
            actionOnDestroy: OnDestroyEnemy,
            collectionCheck: true,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );
    }

    private Enemy CreateEnemy()
    {
        var enemy = Instantiate(_prefab, transform);

        if (_container != null)
            GameObjectInjector.InjectRecursive(enemy.gameObject, _container);

        return enemy;
    }

    private void OnGetEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(true);
    }

    private void OnReleaseEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    private void OnDestroyEnemy(Enemy enemy)
    {
        Destroy(enemy.gameObject);
    }

    public Enemy Get() => _pool.Get();
    public void Release(Enemy enemy) => _pool.Release(enemy);
}
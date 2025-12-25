using UnityEngine;
using UnityEngine.Pool;

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance;

    [Header("Settings")]
    [SerializeField] private Enemy _prefab; 
    [SerializeField] private int _defaultCapacity = 20;
    [SerializeField] private int _maxSize = 100;

    [Header("Debug Info (Read Only)")]
    [SerializeField] private int _totalCount;
    [SerializeField] private int _activeCount;
    [SerializeField] private int _inactiveCount;


    private ObjectPool<Enemy> _pool;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

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

    //콜백
    private Enemy CreateEnemy()
    {
        return Instantiate(_prefab, transform);
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

    //Public API
    public Enemy Get() => _pool.Get();
    public void Release(Enemy enemy) => _pool.Release(enemy);
}
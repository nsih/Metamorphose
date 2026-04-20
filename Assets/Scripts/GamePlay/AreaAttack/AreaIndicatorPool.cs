// Assets/Scripts/GamePlay/AreaAttack/AreaIndicatorPool.cs
// 2026-04-20 장판 풀 신규 (Reflex DI 등록, 싱글턴 아님)
using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using Reflex.Core;
using Reflex.Attributes;
using Reflex.Injectors;

public class AreaIndicatorPool : MonoBehaviour
{
    [SerializeField] private AreaIndicator _prefab;
    [SerializeField] private int _defaultCapacity = 10;
    [SerializeField] private int _maxSize = 50;

    [Inject] private Container _container;

    private ObjectPool<AreaIndicator> _pool;
    private List<AreaIndicator> _activeIndicators = new List<AreaIndicator>();

    private void Awake()
    {
        _pool = new ObjectPool<AreaIndicator>(
            createFunc: CreateIndicator,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyIndicator,
            collectionCheck: true,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );
    }

    private AreaIndicator CreateIndicator()
    {
        var indicator = Instantiate(_prefab, transform);

        if (_container != null)
            GameObjectInjector.InjectRecursive(indicator.gameObject, _container);

        return indicator;
    }

    private void OnGet(AreaIndicator indicator)
    {
        indicator.gameObject.SetActive(true);
        _activeIndicators.Add(indicator);
    }

    private void OnRelease(AreaIndicator indicator)
    {
        _activeIndicators.Remove(indicator);
        indicator.gameObject.SetActive(false);
    }

    private void OnDestroyIndicator(AreaIndicator indicator)
    {
        Destroy(indicator.gameObject);
    }

    public AreaIndicator Get() => _pool.Get();

    public void Release(AreaIndicator indicator) => _pool.Release(indicator);

    // 씬 전환 시 전체 반환
    public void ReleaseAll()
    {
        var snapshot = new List<AreaIndicator>(_activeIndicators);
        foreach (var indicator in snapshot)
        {
            indicator.ForceRelease();
        }
    }
}

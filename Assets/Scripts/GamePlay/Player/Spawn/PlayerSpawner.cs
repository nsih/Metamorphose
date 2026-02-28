using UnityEngine;
using Reflex.Core;
using Reflex.Attributes;
using Reflex.Injectors;
using Cysharp.Threading.Tasks;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _spawnPoint;

    [Inject] private Container _container;

    private static GameObject _cachedPlayerInstance;

    public GameObject Spawn()
    {
        Vector3 pos = _spawnPoint != null ? _spawnPoint.position : Vector3.zero;

        if (_cachedPlayerInstance != null)
        {
            _cachedPlayerInstance.transform.position = pos;
            ReactivateAsync(_cachedPlayerInstance).Forget();
            return _cachedPlayerInstance;
        }

        if (_playerPrefab == null)
        {
            Debug.LogError("PlayerSpawner error: Prefab missing");
            return null;
        }

        GameObject player = Instantiate(_playerPrefab, pos, Quaternion.identity);
        player.name = "Player";

        DontDestroyOnLoad(player);
        GameObjectInjector.InjectRecursive(player, _container);

        _cachedPlayerInstance = player;

        return player;
    }

    // BulletCollisionManager 초기화 대기 후 재활성화
    private async UniTaskVoid ReactivateAsync(GameObject player)
    {
        player.SetActive(false);

        await UniTask.WaitUntil(() => BulletPro.BulletCollisionManager.instance != null);

        player.SetActive(true);
    }
}
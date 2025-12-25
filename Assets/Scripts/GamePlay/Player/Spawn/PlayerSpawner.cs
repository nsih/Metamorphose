using UnityEngine;
using Reflex.Core;
using Reflex.Attributes;
using Reflex.Injectors;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _spawnPoint;

    [Inject] private Container _container; 


    //부수지 마루요
    private static GameObject _cachedPlayerInstance;

    public GameObject Spawn()
    {
        Vector3 pos = _spawnPoint != null ? _spawnPoint.position : Vector3.zero;

        //최초실행 아닐때
        if (_cachedPlayerInstance != null)
        {
            _cachedPlayerInstance.transform.position = pos;

            if (!_cachedPlayerInstance.activeSelf) 
                _cachedPlayerInstance.SetActive(true);

            return _cachedPlayerInstance;
        }

        //최초실행
        if (_playerPrefab == null)
        {
            Debug.LogError("PlayerSpawner error: Prefab missing");
            return null;
        }
        
        GameObject player = Instantiate(_playerPrefab, pos, Quaternion.identity);
        player.name = "Player";



        DontDestroyOnLoad(player);
        GameObjectInjector.InjectRecursive(player, _container);

        // caching
        _cachedPlayerInstance = player;
        
        return player;
    }
}
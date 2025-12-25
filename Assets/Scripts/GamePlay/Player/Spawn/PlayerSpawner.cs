using UnityEngine;
using Reflex.Core;
using Reflex.Attributes;
using Reflex.Injectors;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _spawnPoint;


    [Inject] private Container _container; 

    public GameObject Spawn()
    {
        if (_playerPrefab == null)
        {
            Debug.LogError("PlayerSpawner error: Prefab missing");
            return null;
        }

        Vector3 pos = _spawnPoint != null ? _spawnPoint.position : Vector3.zero;
        
        GameObject player = Instantiate(_playerPrefab, pos, Quaternion.identity);
        player.name = "Player";

        GameObjectInjector.InjectRecursive(player, _container);

        return player;
    }
}
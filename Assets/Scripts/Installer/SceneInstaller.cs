using Reflex.Core;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SceneInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private MapUIManager _mapUIManager;
    [SerializeField] private MapManager _mapManager;
    [SerializeField] private EnemyPoolManager _enemyPoolManager;

    public void InstallBindings(ContainerBuilder builder)
    {
        builder.AddSingleton(_playerSpawner);

        var roomProperty = new AsyncReactiveProperty<RoomManager>(null);
        builder.AddSingleton(
            roomProperty, 
            typeof(AsyncReactiveProperty<RoomManager>),
            typeof(IReadOnlyAsyncReactiveProperty<RoomManager>)
        );

        if (_mapUIManager != null)
            builder.AddSingleton(_mapUIManager);

        if (_mapManager != null)
            builder.AddSingleton(_mapManager);

        if (_enemyPoolManager != null)
            builder.AddSingleton(_enemyPoolManager);
    }
}
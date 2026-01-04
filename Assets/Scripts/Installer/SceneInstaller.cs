using System;
using Reflex.Core;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SceneInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private MapUIManager _mapUIManager;

    public void InstallBindings(ContainerBuilder builder)
    {
        builder.AddSingleton(_playerSpawner);

        var roomProperty = new AsyncReactiveProperty<RoomManager>(null);
        builder.AddSingleton(
            roomProperty, 
            typeof(AsyncReactiveProperty<RoomManager>),             // RoomManager용 (쓰기 가능)
            typeof(IReadOnlyAsyncReactiveProperty<RoomManager>)     // UI용 (읽기 전용)
        );

        if (_mapUIManager != null)
        {
            builder.AddSingleton(_mapUIManager);
        }
    }
}
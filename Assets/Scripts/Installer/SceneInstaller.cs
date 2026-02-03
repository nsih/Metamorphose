using Reflex.Core;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using Reflex.Extensions;
using GamePlay;
using FMODUnity;

public class SceneInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private MapUIManager _mapUIManager;
    [SerializeField] private MapManager _mapManager;
    [SerializeField] private EnemyPoolManager _enemyPoolManager;
    [SerializeField] private EventReference _musicEventReference;

    ContainerBuilder _builder;

    public void InstallBindings(ContainerBuilder builder)
    {
        builder.RegisterValue(_playerSpawner);

        var roomProperty = new AsyncReactiveProperty<RoomManager>(null);
        builder.RegisterValue(
            roomProperty, 
            new Type[] { typeof(AsyncReactiveProperty<RoomManager>), typeof(IReadOnlyAsyncReactiveProperty<RoomManager>) }
        );

        if (_mapUIManager != null)
            builder.RegisterValue(_mapUIManager);

        if (_mapManager != null)
            builder.RegisterValue(_mapManager);

        if (_enemyPoolManager != null)
            builder.RegisterValue(_enemyPoolManager);

        builder.OnContainerBuilt += OnBuilt;
    }

    void OnBuilt(Container container)
    {
        container.Single<AudioService>().PlayMusic(_musicEventReference);
    }
}
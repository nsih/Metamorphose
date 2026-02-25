using Reflex.Core;
using UnityEngine;
using System;
using Reflex.Extensions;
using GamePlay;
using FMODUnity;
using TJR.Core.Interface;
using R3;

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

        var roomProperty = new ReactiveProperty<RoomManager>(null);
        builder.RegisterValue(roomProperty, new Type[] { typeof(ReactiveProperty<RoomManager>) });

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
        // container.Single<IAudioService>().PlayMusic(_musicEventReference);
    }
}
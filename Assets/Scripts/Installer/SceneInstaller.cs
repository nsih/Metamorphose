using Reflex.Core;
using UnityEngine;
using System;
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

    [SerializeField] private EventReference _gameplayBGM;

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
        var audio = container.Single<IAudioService>();

        if (_gameplayBGM.IsNull)
            audio.PlayMusic(FMODEvents.Music.Gameplay);
        else
            audio.PlayMusic(_gameplayBGM);

        // 씬 진입 시 입력 재활성화 (포탈 진입 시 SetEnabled(false) 복원)
        container.Single<IInputService>().SetEnabled(true);
    }
}
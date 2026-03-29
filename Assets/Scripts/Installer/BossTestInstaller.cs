using Reflex.Core;
using UnityEngine;
using System;
using FMODUnity;
using R3;
using GamePlay;
using TJR.Core.Interface;

public class BossTestInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private EnemyPoolManager _enemyPoolManager;
    [SerializeField] private EventReference _bgm;

    public void InstallBindings(ContainerBuilder builder)
    {
        builder.RegisterValue(_playerSpawner);

        var roomProperty = new ReactiveProperty<RoomManager>(null);
        builder.RegisterValue(roomProperty, new Type[] { typeof(ReactiveProperty<RoomManager>) });

        if (_enemyPoolManager != null)
            builder.RegisterValue(_enemyPoolManager);

        builder.OnContainerBuilt += OnBuilt;
    }

    private void OnBuilt(Container container)
    {
        var audio = container.Single<IAudioService>();

        if (_bgm.IsNull)
            audio.PlayMusic(FMODEvents.Music.Gameplay);
        else
            audio.PlayMusic(_bgm);

        container.Single<IInputService>().SetEnabled(true);
    }
}
// Assets/Scripts/Installer/BossTestInstaller.cs
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
    [SerializeField] private TopDownCameraController _cameraController;
    [SerializeField] private BossIntroController _bossIntroController;
    [SerializeField] private CutinView _cutinView;

    [SerializeField] private EventReference _bgm;

    public void InstallBindings(ContainerBuilder builder)
    {
        builder.RegisterValue(_playerSpawner);

        var roomProperty = new ReactiveProperty<RoomManager>(null);
        builder.RegisterValue(roomProperty, new Type[] { typeof(ReactiveProperty<RoomManager>) });

        var bossProperty = new ReactiveProperty<BossController>(null);
        builder.RegisterValue(bossProperty, new Type[] { typeof(ReactiveProperty<BossController>) });

        if (_enemyPoolManager != null)
            builder.RegisterValue(_enemyPoolManager);

        if (_cameraController != null)
            builder.RegisterValue(_cameraController);

        if (_bossIntroController != null)
            builder.RegisterValue(_bossIntroController);

        if (_cutinView != null)
            builder.RegisterValue(_cutinView, new Type[] { typeof(ICutinService) });

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
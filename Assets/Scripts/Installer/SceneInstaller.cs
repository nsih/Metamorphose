// Assets/Scripts/Installer/SceneInstaller.cs
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
    [SerializeField] private TopDownCameraController _cameraController;
    [SerializeField] private RoomClearFlowController _roomClearFlowController;
    [SerializeField] private RunEndManager _runEndManager;
    [SerializeField] private BossIntroController _bossIntroController;
    [SerializeField] private CutinView _cutinView;
    [SerializeField] private AreaIndicatorPool _areaIndicatorPool;

    [SerializeField] private EventReference _gameplayBGM;

    public void InstallBindings(ContainerBuilder builder)
    {
        builder.RegisterValue(_playerSpawner);

        var roomProperty = new ReactiveProperty<RoomManager>(null);
        builder.RegisterValue(roomProperty, new Type[] { typeof(ReactiveProperty<RoomManager>) });

        var bossProperty = new ReactiveProperty<BossController>(null);
        builder.RegisterValue(bossProperty, new Type[] { typeof(ReactiveProperty<BossController>) });

        if (_mapUIManager != null)
            builder.RegisterValue(_mapUIManager);

        if (_mapManager != null)
            builder.RegisterValue(_mapManager);

        if (_enemyPoolManager != null)
            builder.RegisterValue(_enemyPoolManager);

        if (_cameraController != null)
            builder.RegisterValue(_cameraController);

        if (_roomClearFlowController != null)
            builder.RegisterValue(_roomClearFlowController);

        if (_runEndManager != null)
            builder.RegisterValue(_runEndManager);

        if (_bossIntroController != null)
            builder.RegisterValue(_bossIntroController);

        if (_cutinView != null)
            builder.RegisterValue(_cutinView, new Type[] { typeof(ICutinService) });

        if (_areaIndicatorPool != null)
            builder.RegisterValue(_areaIndicatorPool);

        builder.OnContainerBuilt += OnBuilt;
    }

    void OnBuilt(Container container)
    {
        var audio = container.Single<IAudioService>();

        if (_gameplayBGM.IsNull)
            audio.PlayMusic(FMODEvents.Music.Gameplay);
        else
            audio.PlayMusic(_gameplayBGM);

        container.Single<IInputService>().SetEnabled(true);
    }
}
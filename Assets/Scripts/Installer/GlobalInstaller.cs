using System;
using GamePlay;
using Reflex.Core;
using TJR.Core.Interface;
using UnityEngine;

public class GlobalInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private PlayerStat _playerStatSO;
    [SerializeField] private PlayerWeaponData _startWeapon;
    [SerializeField] private ItemDatabase _itemDatabase;

    public void InstallBindings(ContainerBuilder builder)
    {
        builder.RegisterValue(new AudioService(), new Type[] { typeof(IAudioService) });
        builder.RegisterValue(_playerStatSO, new Type[] { typeof(PlayerStat) });
        builder.RegisterValue(new PlayerInputService(), new Type[] { typeof(IInputService) });
        builder.RegisterValue(new SceneLoader(), new Type[] { typeof(ISceneLoader) });
        builder.RegisterValue(new RunResultModel());

        builder.RegisterValue(_itemDatabase);

        var registry = new AcquiredItemRegistry();
        builder.RegisterValue(registry);

        var repository = new ItemRepository(_itemDatabase);
        builder.RegisterValue(repository);

        var playerModel = new PlayerModel(_playerStatSO, _startWeapon, registry);
        builder.RegisterValue(playerModel);

        var itemApplyService = new ItemApplyService(playerModel, registry);
        builder.RegisterValue(itemApplyService);
    }
}
using System;
using Reflex.Core;
using UnityEngine;

public class GlobalInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private PlayerStat _playerStatSO;
    [SerializeField] private PlayerWeaponData _startWeapon;

    public void InstallBindings(ContainerBuilder builder)
    {
        //Debug.Log("Global Install Start");

        builder.RegisterValue(_playerStatSO, new Type[] { typeof(PlayerStat) });
        builder.RegisterValue(new PlayerModel(_playerStatSO, _startWeapon));
        builder.RegisterValue(new PlayerInputService(), new Type[] { typeof(IInputService) });
    }
}

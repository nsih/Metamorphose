using System;
using Reflex.Core;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GlobalInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private PlayerStat _playerStatSO;

    public void InstallBindings(ContainerBuilder builder)
    {
        //Debug.Log("Global Install Start");

        builder.AddSingleton(_playerStatSO, typeof(PlayerStat));
        builder.AddSingleton(new PlayerModel(_playerStatSO));
        builder.AddSingleton(typeof(PlayerInputService), typeof(IInputService));
    }
}

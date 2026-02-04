using System;
using GamePlay;
using Reflex.Core;
using TJR.Core.Interface;
using UnityEngine;

namespace TJR.Core.Installer
{
    /// <summary>
    /// Root Installer for the game.
    /// </summary>
    public class GlobalInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private PlayerStat _playerStatSO;
        [SerializeField] private PlayerWeaponData _startWeapon;

        public void InstallBindings(ContainerBuilder builder)
        {
            //Debug.Log("Global Install Start");
            builder.RegisterValue(new AudioService(), new Type[] { typeof(IAudioService) });
            builder.RegisterValue(new PlayerModel(_playerStatSO, _startWeapon));
            builder.RegisterValue(new PlayerInputService(), new Type[] { typeof(IInputService) });
        }
    }
}

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
        public void InstallBindings(ContainerBuilder builder)
        {
            builder.RegisterValue(new AudioService(), new Type[] { typeof(IAudioService) });
            builder.RegisterValue(new PlayerInputService(), new Type[] { typeof(IInputService) });
        }
    }
}

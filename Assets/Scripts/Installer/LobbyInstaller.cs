using Reflex.Core;
using UnityEngine;
using GamePlay;

public class LobbyInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private LobbyController _lobbyController;
    [SerializeField] private PlayerSpawner _playerSpawner;

    public void InstallBindings(ContainerBuilder builder)
    {
        if (_lobbyController != null)
            builder.RegisterValue(_lobbyController);

        if (_playerSpawner != null)
            builder.RegisterValue(_playerSpawner);
    }
}
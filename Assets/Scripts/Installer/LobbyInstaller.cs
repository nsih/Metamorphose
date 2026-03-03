using Reflex.Core;
using UnityEngine;
using GamePlay;

public class LobbyInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private LobbyController _lobbyController;
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private LobbyDialogueController _lobbyDialogueController;

    public void InstallBindings(ContainerBuilder builder)
    {
        if (_lobbyController != null)
            builder.RegisterValue(_lobbyController);

        if (_playerSpawner != null)
            builder.RegisterValue(_playerSpawner);

        if (_lobbyDialogueController != null)
            builder.RegisterValue(_lobbyDialogueController);
    }
}
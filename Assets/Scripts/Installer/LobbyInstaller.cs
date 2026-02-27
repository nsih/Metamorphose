using Reflex.Core;
using UnityEngine;

public class LobbyInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private LobbyController _lobbyController;

    public void InstallBindings(ContainerBuilder builder)
    {
        if (_lobbyController != null)
            builder.RegisterValue(_lobbyController);
    }
}
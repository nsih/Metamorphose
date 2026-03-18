using Reflex.Core;
using UnityEngine;
using GamePlay;
using FMODUnity;
using TJR.Core.Interface;

public class LobbyInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private LobbyController _lobbyController;
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private LobbyDialogueController _lobbyDialogueController;

    [SerializeField] private EventReference _lobbyBGM;

    public void InstallBindings(ContainerBuilder builder)
    {
        if (_lobbyController != null)
            builder.RegisterValue(_lobbyController);

        if (_playerSpawner != null)
            builder.RegisterValue(_playerSpawner);

        if (_lobbyDialogueController != null)
            builder.RegisterValue(_lobbyDialogueController);

        builder.OnContainerBuilt += OnBuilt;
    }

    void OnBuilt(Reflex.Core.Container container)
    {
        //Debug.Log("LobbyInstaller OnBuilt 호출");
        var audio = container.Single<IAudioService>();

        if (_lobbyBGM.IsNull)
        {
            //Debug.Log($"BGM 경로: {FMODEvents.Music.Lobby}");
            audio.PlayMusic(FMODEvents.Music.Lobby);
        }
        else
        {
            //Debug.Log($"BGM EventReference 사용");
            audio.PlayMusic(_lobbyBGM);
        }
    }
}
using System;
using Reflex.Core;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private PlayerStat _playerStatSO;

    [SerializeField] private RoomManager _roomManager;


    public void InstallBindings(ContainerBuilder builder)
    {
        //Debug.Log("scene install start");

        //data
        builder.AddSingleton(_playerStatSO, typeof(PlayerStat));

        //player model
        var model = new PlayerModel(_playerStatSO);
        builder.AddSingleton(model);

        //interface
        builder.AddSingleton(typeof(PlayerInputService), typeof(IInputService));


        //room manager
        builder.AddSingleton(new AsyncReactiveProperty<RoomManager>(null));


        
        //Debug.Log("scene install done");
    }
}
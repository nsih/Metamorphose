using System;
using Reflex.Core;
using UnityEngine;

public class GameInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private PlayerStat _playerStatSO;


    public void InstallBindings(ContainerBuilder builder)
    {
        Debug.Log("scene install start");

        //data
        builder.AddSingleton(_playerStatSO, typeof(PlayerStat));

        //player model
        var model = new PlayerModel(_playerStatSO);
        builder.AddSingleton(model);

        //interface
        builder.AddSingleton(typeof(PlayerInputService), typeof(IInputService));


        
        Debug.Log("scene install done");
    }
}
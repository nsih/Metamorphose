using System;
using Reflex.Core;
using UnityEngine;

public class GameInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private PlayerStat _playerStatAsset;


    public void InstallBindings(ContainerBuilder builder)
    {
        Debug.Log("scene install start");

        //data
        builder.AddSingleton(_playerStatAsset, typeof(PlayerStat));

        //interface
        builder.AddSingleton(typeof(PlayerInputService), typeof(IInputService));


        
        Debug.Log("scene install done");
    }
}
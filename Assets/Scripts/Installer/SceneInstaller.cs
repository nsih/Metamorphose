using System;
using Reflex.Core;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SceneInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private PlayerSpawner _playerSpawner;

    //[SerializeField] private RoomManager _roomManager;


    public void InstallBindings(ContainerBuilder builder)
    {
        //Debug.Log("scene Install Start");

        //PlayerSpawner
        builder.AddSingleton(_playerSpawner);


        //room manager
        builder.AddSingleton(new AsyncReactiveProperty<RoomManager>(null));
    }
}
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
        var roomProperty = new AsyncReactiveProperty<RoomManager>(null);
        builder.AddSingleton(
            roomProperty, 
            typeof(AsyncReactiveProperty<RoomManager>),          // RoomManager용 (쓰기 가능)
            typeof(IReadOnlyAsyncReactiveProperty<RoomManager>)  // UI용 (읽기 전용)
        );
        //builder.AddSingleton(new AsyncReactiveProperty<RoomManager>(null));
    }
}
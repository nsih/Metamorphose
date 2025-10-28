using Reflex.Core;
using UnityEngine;

public class GameInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private PlayerStat _playerStatAsset;


    public void InstallBindings(ContainerBuilder builder)
    {
        Debug.Log("의존성 등록 시작");

        //data
        builder.AddSingleton(_playerStatAsset, typeof(PlayerStat));

        //interface
        builder.AddSingleton(typeof(IInputService), typeof(PlayerInputService));

        Debug.Log("의존성 등록 완료");
    }
}
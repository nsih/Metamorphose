using Cysharp.Threading.Tasks;

public interface ISceneLoader
{
    UniTask LoadLobbyAsync();
    UniTask LoadGamePlayAsync();


    //임시 보스씬
    UniTask LoadBossTestAsync();
}
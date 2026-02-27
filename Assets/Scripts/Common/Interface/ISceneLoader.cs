using Cysharp.Threading.Tasks;

public interface ISceneLoader
{
    UniTask LoadLobbyAsync();
    UniTask LoadGamePlayAsync();
}
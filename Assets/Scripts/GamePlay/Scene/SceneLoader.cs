using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SceneLoader : ISceneLoader
{
    private const string LobbyScene = "Lobby";
    private const string GamePlayScene = "PlayerTest";
    private const string BossTestScene = "BossTest";

    public async UniTask LoadLobbyAsync()
    {
        await SceneManager.LoadSceneAsync(LobbyScene).ToUniTask();
    }

    public async UniTask LoadGamePlayAsync()
    {
        await SceneManager.LoadSceneAsync(GamePlayScene).ToUniTask();
    }

    public async UniTask LoadBossTestAsync()
    {
        await SceneManager.LoadSceneAsync(BossTestScene).ToUniTask();
    }
}
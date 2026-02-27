using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace GamePlay
{
    public class SceneLoader : ISceneLoader
    {
        private const string LobbyScene = "Lobby";
        private const string GamePlayScene = "GamePlay";

        public async UniTask LoadLobbyAsync()
        {
            await SceneManager.LoadSceneAsync(LobbyScene).ToUniTask();
        }

        public async UniTask LoadGamePlayAsync()
        {
            await SceneManager.LoadSceneAsync(GamePlayScene).ToUniTask();
        }
    }
}
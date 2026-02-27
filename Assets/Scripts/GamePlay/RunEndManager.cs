using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;
using Common;

namespace GamePlay
{
    public class RunEndManager : MonoBehaviour
    {
        [Inject] private PlayerModel _playerModel;
        [Inject] private ISceneLoader _sceneLoader;
        [Inject] private RunResultModel _runResultModel;

        private bool _isEnding = false;

        private void Start()
        {
            _playerModel.Health.OnDeath += OnPlayerDeath;
        }

        private void OnDestroy()
        {
            if (_playerModel != null)
                _playerModel.Health.OnDeath -= OnPlayerDeath;
        }

        private void OnPlayerDeath()
        {
            if (_isEnding) return;
            _isEnding = true;
            HandleGameOver().Forget();
        }

        // 보스 처치 시 외부에서 호출
        public void NotifyRunClear()
        {
            if (_isEnding) return;
            _isEnding = true;
            HandleClear().Forget();
        }

        private async UniTaskVoid HandleGameOver()
        {
            _runResultModel.LastResult.Value = RunEndReason.GameOver;
            await UniTask.Delay(1500);
            await _sceneLoader.LoadLobbyAsync();
        }

        private async UniTaskVoid HandleClear()
        {
            _runResultModel.LastResult.Value = RunEndReason.Clear;
            await UniTask.Delay(2000);
            await _sceneLoader.LoadLobbyAsync();
        }
    }
}
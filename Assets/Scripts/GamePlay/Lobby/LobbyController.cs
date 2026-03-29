using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;
using Common;

public class LobbyController : MonoBehaviour
{
    [Inject] private ISceneLoader _sceneLoader;
    [Inject] private PlayerModel _playerModel;
    [Inject] private RunResultModel _runResultModel;

    private bool _isLoading = false;

    private void Start()
    {
        _playerModel.ResetForNewRun();

        if (_runResultModel.LastResult.Value != RunEndReason.None)
        {
            Debug.Log($"last run: {_runResultModel.LastResult.Value}");
        }

        _runResultModel.LastResult.Value = RunEndReason.None;
    }

    // 기존 호출부 호환용 유지
    public void StartRun()
    {
        if (_isLoading) return;
        _isLoading = true;
        LoadGamePlay().Forget();
    }

    // 포탈 TransitionAsync에서 loadAction으로 전달
    public async UniTask StartRunAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        Time.timeScale = 1f;
        await _sceneLoader.LoadGamePlayAsync();
    }

    private async UniTaskVoid LoadGamePlay()
    {
        Time.timeScale = 1f;
        await _sceneLoader.LoadGamePlayAsync();
    }
}
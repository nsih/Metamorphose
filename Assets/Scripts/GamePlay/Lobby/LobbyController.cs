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

    public void StartRun()
    {
        if (_isLoading) return;
        _isLoading = true;
        LoadGamePlay().Forget();
    }

    private async UniTaskVoid LoadGamePlay()
    {
        Time.timeScale = 1f;
        await _sceneLoader.LoadGamePlayAsync();
    }
}
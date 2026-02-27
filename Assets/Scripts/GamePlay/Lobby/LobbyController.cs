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
            // TODO: 결과 UI 연결 지점
        }

        _runResultModel.LastResult.Value = RunEndReason.None;
    }

    // StartButton OnClick에 연결
    public void StartRun()
    {
        if (_isLoading) return;
        _isLoading = true;
        LoadGamePlay().Forget();
    }

    private async UniTaskVoid LoadGamePlay()
    {
        await _sceneLoader.LoadGamePlayAsync();
    }
}
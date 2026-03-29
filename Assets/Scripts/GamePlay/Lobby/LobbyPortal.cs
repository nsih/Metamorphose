using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;
using Common;

public class LobbyPortal : MonoBehaviour
{
    [Inject] private LobbyController _lobbyController;
    [Inject] private IInputService _input;
    [Inject] private ISceneTransitionService _sceneTransition;

    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;

        _input.SetEnabled(false);
        TransitionAndLoadAsync().Forget();
    }

    private async UniTaskVoid TransitionAndLoadAsync()
    {
        await _sceneTransition.TransitionAsync(() => _lobbyController.StartRunAsync());
    }
}
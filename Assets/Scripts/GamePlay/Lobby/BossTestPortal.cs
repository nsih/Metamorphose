using UnityEngine;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using Common;

public class BossTestPortal : MonoBehaviour
{
    [Inject] private IInputService _input;
    [Inject] private ISceneLoader _sceneLoader;
    [Inject] private ISceneTransitionService _sceneTransition;

    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;
        _input.SetEnabled(false);

        TransitionAsync().Forget();
    }

    private async UniTaskVoid TransitionAsync()
    {
        await _sceneTransition.TransitionAsync(() => _sceneLoader.LoadBossTestAsync());
    }
}
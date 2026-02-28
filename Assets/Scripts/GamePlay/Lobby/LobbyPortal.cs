using UnityEngine;
using Reflex.Attributes;

public class LobbyPortal : MonoBehaviour
{
    [Inject] private LobbyController _lobbyController;

    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;

        // 입력 차단
        Time.timeScale = 0f;

        _lobbyController.StartRun();
    }
}
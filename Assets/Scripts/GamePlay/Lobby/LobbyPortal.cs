using UnityEngine;
using Reflex.Attributes;

public class LobbyPortal : MonoBehaviour
{
    [Inject] private LobbyController _lobbyController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _lobbyController.StartRun();
    }
}
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyNpc : MonoBehaviour
{
    [SerializeField] private string _npcId = "companion_01";

    public event Action<string> OnInteractRequested;

    private bool _playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
    }

    private void Update()
    {
        if (_playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OnInteractRequested?.Invoke(_npcId);
            Debug.Log($"interact: {_npcId}");
        }
    }

    public string NpcId => _npcId;
}
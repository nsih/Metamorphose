// Assets/Scripts/GamePlay/Portal/ClearPortal.cs
using UnityEngine;
using System;

public class ClearPortal : MonoBehaviour, IInteractable
{
    [SerializeField] private float _interactRadius = 1.5f;
    [SerializeField] private string _promptText = "진입";

    public event Action<bool> OnPlayerRangeChanged;
    public event Action OnPortalEntered;

    // IInteractable
    public Transform InteractPoint => transform;
    public string PromptText => _promptText;

    private bool _playerInRange;
    private bool _entered;

    private void Awake()
    {
        var col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        col.radius = _interactRadius;
    }

    public void Interact()
    {
        if (_entered) return;
        if (!_playerInRange) return;

        _entered = true;
        Debug.Log("ClearPortal: 진입");
        OnPortalEntered?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        OnPlayerRangeChanged?.Invoke(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
        OnPlayerRangeChanged?.Invoke(false);
    }
}
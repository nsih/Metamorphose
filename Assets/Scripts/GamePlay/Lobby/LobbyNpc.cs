using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class LobbyNpc : MonoBehaviour
{
    [SerializeField] private string _npcId = "companion_01";
    [SerializeField] private DialogueRunner _dialogueRunner;

    public event Action<string> OnInteractRequested;

    // 대화 시작 프레임에 DialogueView가 E키를 동시 소비하지 않도록 방지
    public static bool DialogueStartedThisFrame { get; private set; } = false;

    private bool _playerInRange = false;

    private void Awake()
    {
        if (_dialogueRunner == null)
            _dialogueRunner = FindObjectOfType<DialogueRunner>();
    }

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
        DialogueStartedThisFrame = false;

        // 대화 진행 중에는 상호작용 E키 무시
        if (_dialogueRunner != null && _dialogueRunner.IsDialogueRunning) return;

        if (_playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            DialogueStartedThisFrame = true;
            OnInteractRequested?.Invoke(_npcId);
            Debug.Log($"interact: {_npcId}");
        }
    }

    public string NpcId => _npcId;
}
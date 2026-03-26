using UnityEngine;
using Yarn.Unity;
using Reflex.Attributes;
using GamePlay;
using R3;

public class LobbyDialogueController : MonoBehaviour
{
    [SerializeField] private DialogueRunner _dialogueRunner;
    [SerializeField] private LobbyNpc[] _npcs;
    [SerializeField] private DialogueBridge _bridge;

    [Inject] private IInputService _input;

    private CompositeDisposable _disposables = new CompositeDisposable();

    private void Start()
    {
        foreach (var npc in _npcs)
            npc.OnInteractRequested += OnNpcInteract;

        if (_bridge != null)
        {
            _bridge.IsActive
                .Subscribe(active => _input?.SetEnabled(!active))
                .AddTo(_disposables);
        }
    }

    private void OnDestroy()
    {
        foreach (var npc in _npcs)
            npc.OnInteractRequested -= OnNpcInteract;

        _disposables.Dispose();
        _input?.SetEnabled(true);
    }

    private void OnNpcInteract(string npcId)
    {
        if (_dialogueRunner.IsDialogueRunning) return;

        string nodeName = GetNodeName(npcId);

        if (_dialogueRunner.YarnProject == null ||
            !System.Array.Exists(_dialogueRunner.YarnProject.NodeNames, n => n == nodeName))
        {
            Debug.Log($"node not found: {nodeName}");
            return;
        }

        Debug.Log($"start dialogue: {nodeName}");
        _dialogueRunner.StartDialogue(nodeName);
    }

    private string GetNodeName(string npcId)
    {
        return $"NPC_{npcId}";
    }
}
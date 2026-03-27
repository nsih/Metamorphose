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
        // 인스펙터 미연결 시 씬에서 자동 탐색
        if (_bridge == null)
            _bridge = FindObjectOfType<DialogueBridge>();

        if (_input == null)
            Debug.LogError("LobbyDialogueController: IInputService null, 이동 차단 불가");

        if (_bridge == null)
            Debug.LogError("LobbyDialogueController: DialogueBridge null, IsActive 구독 불가");

        foreach (var npc in _npcs)
            npc.OnInteractRequested += OnNpcInteract;

        if (_bridge != null)
        {
            _bridge.IsActive
                .Subscribe(active =>
                {
                    if (_input != null)
                        _input.SetEnabled(!active);
                    else
                        Debug.LogError("IsActive 실행: input null로 이동 차단 불가");
                })
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
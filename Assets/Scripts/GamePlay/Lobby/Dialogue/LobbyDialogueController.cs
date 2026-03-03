using UnityEngine;
using Yarn.Unity;
using Reflex.Attributes;
using GamePlay;

// LobbyNpc 이벤트 수신 -> DialogueRunner 실행
public class LobbyDialogueController : MonoBehaviour
{
    [SerializeField] private DialogueRunner _dialogueRunner;
    [SerializeField] private LobbyNpc[] _npcs;

    private void Start()
    {
        foreach (var npc in _npcs)
            npc.OnInteractRequested += OnNpcInteract;
    }

    private void OnDestroy()
    {
        foreach (var npc in _npcs)
            npc.OnInteractRequested -= OnNpcInteract;
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

    // npcId -> Yarn 노드 이름 변환
    private string GetNodeName(string npcId)
    {
        return $"NPC_{npcId}";
    }
}
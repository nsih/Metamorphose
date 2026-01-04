using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Common;
using Reflex.Attributes;
using System.Threading;
using System.Collections.Generic;

public class RoomClearFlowUI : MonoBehaviour
{
    public static RoomClearFlowUI Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject _rewardPanel;

    [Header("Reward UI")]
    [SerializeField] private Transform _rewardButtonContainer;
    [SerializeField] private Button _rewardButtonPrefab;

    [Header("Data")]
    [SerializeField] private RewardLibrary _rewardLibrary;

    private enum FlowState { None, Reward, MapSelect }
    private FlowState _currentState;

    private IReadOnlyAsyncReactiveProperty<RoomManager> _globalCurrentRoomHandle;
    private PlayerModel _playerModel;
    private MapUIManager _mapUIManager;

    [Inject]
    public void Construct(
        IReadOnlyAsyncReactiveProperty<RoomManager> globalHandle,
        PlayerModel playerModel,
        MapUIManager mapUIManager)
    {
        _globalCurrentRoomHandle = globalHandle;
        _playerModel = playerModel;
        _mapUIManager = mapUIManager;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        CloseAllPanels();
    }

    private void Start()
    {
        if (_mapUIManager != null)
        {
            _mapUIManager.OnNodeSelected += OnMapNodeSelected;
        }

        MonitorRoomChanges().Forget();
    }

    private void OnDestroy()
    {
        if (_mapUIManager != null)
        {
            _mapUIManager.OnNodeSelected -= OnMapNodeSelected;
        }
    }

    private async UniTaskVoid MonitorRoomChanges()
    {
        await foreach (var currentRoom in _globalCurrentRoomHandle)
        {
            if (currentRoom == null) continue;
            MonitorCurrentRoomState(currentRoom).Forget();
        }
    }

    private async UniTaskVoid MonitorCurrentRoomState(RoomManager room)
    {
        var destroyToken = this.GetCancellationTokenOnDestroy();
        var roomDestroyToken = room.GetCancellationTokenOnDestroy();
        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(destroyToken, roomDestroyToken).Token;

        try
        {
            await room.CurrentRoomState
                .Where(state => state == RoomState.Complete)
                .FirstAsync(linkedToken);

            StartClearFlow();
        }
        catch (System.OperationCanceledException) { }
    }

    private void StartClearFlow()
    {
        if (_currentState != FlowState.None) return;
        SetState(FlowState.Reward);
    }

    private void SetState(FlowState newState)
    {
        _currentState = newState;
        CloseAllPanels();

        switch (_currentState)
        {
            case FlowState.Reward:
                _rewardPanel.SetActive(true);
                GenerateRewardButtons();
                break;

            case FlowState.MapSelect:
                if (_mapUIManager != null)
                {
                    _mapUIManager.ShowMap();
                    _mapUIManager.RenderMap(
                        MapManager.Instance.GetCurrentMap(),
                        MapManager.Instance.CurrentNode
                    );
                    _mapUIManager.HighlightAvailableNodes(
                        MapManager.Instance.CurrentNode.NextNodes
                    );
                }
                break;

            case FlowState.None:
                break;
        }
    }

    private void GenerateRewardButtons()
    {
        foreach (Transform child in _rewardButtonContainer)
        {
            Destroy(child.gameObject);
        }

        if (_rewardLibrary == null)
        {
            Debug.LogError("RoomClearFlowUI: RewardLibrary null");
            return;
        }

        if (_rewardButtonPrefab == null)
        {
            Debug.LogError("RoomClearFlowUI: RewardButtonPrefab null");
            return;
        }

        List<RewardData> rewards = _rewardLibrary.GetRandomRewards(_playerModel.RewardChoiceCount);

        if (rewards.Count == 0)
        {
            Debug.LogWarning("RoomClearFlowUI: 보상 cnt error");
            return;
        }

        foreach (var reward in rewards)
        {
            if (reward == null) continue;

            Button btn = Instantiate(_rewardButtonPrefab, _rewardButtonContainer);
            
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                string effectsSummary = GetEffectsSummary(reward);
                text.text = $"[{reward.Rarity}]\n{reward.DisplayName}\n\n{effectsSummary}";
            }

            RewardData capturedReward = reward;
            btn.onClick.AddListener(() => OnRewardSelected(capturedReward));
        }

        Debug.Log($"보상 버튼 {rewards.Count}개 생성");
    }

    private string GetEffectsSummary(RewardData reward)
    {
        if (reward.Effects == null || reward.Effects.Count == 0)
        {
            return reward.Description;
        }

        if (reward.Effects.Count == 1)
        {
            return reward.Description;
        }

        List<string> lines = new List<string>();
        foreach (var effect in reward.Effects)
        {
            lines.Add($"• {GetEffectDescription(effect)}");
        }
        
        return string.Join("\n", lines);
    }

    private string GetEffectDescription(RewardEffect effect)
    {
        switch (effect.Type)
        {
            case RewardType.MaxHP:
                return $"최대 체력 +{effect.Value}";
            case RewardType.MaxHPPercent:
                return $"최대 체력 +{effect.Value * 100}%";
            case RewardType.Heal:
                return $"체력 회복 +{effect.Value}";
            
            case RewardType.Damage:
                return $"공격력 +{effect.Value}";
            case RewardType.DamagePercent:
                return $"공격력 +{effect.Value * 100}%";
            case RewardType.DamageMultiplier:
                return $"공격력 ×{effect.Value}";
            
            case RewardType.AttackSpeed:
                return $"공격 속도 +{effect.Value * 100}%";
            
            case RewardType.Multishot:
                return $"발사체 +{effect.Value}";
            
            case RewardType.MoveSpeed:
                return $"이동 속도 +{effect.Value}";
            
            default:
                return $"{effect.Type} +{effect.Value}";
        }
    }

    private void OnRewardSelected(RewardData reward)
    {
        Debug.Log($"UI: 보상 선택됨 - {reward.DisplayName}");
        
        if (_playerModel != null)
        {
            _playerModel.Reward.ApplyReward(reward);
        }
        else
        {
            Debug.LogError("RoomClearFlowUI: PlayerModel null");
        }
        
        SetState(FlowState.MapSelect);
    }

    private void OnMapNodeSelected(MapNode node)
    {
        Debug.Log($"RoomClearFlowUI: 맵 노드 선택됨 - {node}");
        
        SetState(FlowState.None);
        MapManager.Instance.MoveToNode(node);
    }

    private void CloseAllPanels()
    {
        if (_rewardPanel != null) _rewardPanel.SetActive(false);
        
        if (_mapUIManager != null)
        {
            _mapUIManager.HideMap();
        }
    }
}
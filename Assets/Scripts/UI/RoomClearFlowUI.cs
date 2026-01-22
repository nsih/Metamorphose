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
    [Header("UI Panels")]
    [SerializeField] private GameObject _rewardPanel;

    [Header("Reward UI")]
    [SerializeField] private Transform _rewardButtonContainer;
    [SerializeField] private Button _rewardButtonPrefab;

    [Header("Data")]
    [SerializeField] private RewardLibrary _rewardLibrary;

    [Header("References")]
    [SerializeField] private MapToggleController _mapToggleController;

    private enum FlowState { None, Reward, MapSelect }
    private FlowState _currentState;

    private IReadOnlyAsyncReactiveProperty<RoomManager> _globalCurrentRoomHandle;
    private PlayerModel _playerModel;
    private MapUIManager _mapUIManager;
    private MapManager _mapManager;

    [Inject]
    public void Construct(
        IReadOnlyAsyncReactiveProperty<RoomManager> globalHandle,
        PlayerModel playerModel,
        MapUIManager mapUIManager,
        MapManager mapManager)
    {
        _globalCurrentRoomHandle = globalHandle;
        _playerModel = playerModel;
        _mapUIManager = mapUIManager;
        _mapManager = mapManager;
    }

    private void Awake()
    {
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

        switch (_currentState)
        {
            case FlowState.Reward:
                CloseAllPanels();
                _rewardPanel.SetActive(true);
                GenerateRewardButtons();
                break;

            case FlowState.MapSelect:
                CloseAllPanels();
                _mapToggleController?.OpenMap();
                break;

            case FlowState.None:
                CloseAllPanels();
                break;
        }
    }

    private void GenerateRewardButtons()
    {
        foreach (Transform child in _rewardButtonContainer)
        {
            Destroy(child.gameObject);
        }

        if (_rewardLibrary == null || _rewardButtonPrefab == null) return;

        List<RewardData> rewards = _rewardLibrary.GetRandomRewards(_playerModel.RewardChoiceCount);

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
        _playerModel?.Reward.ApplyReward(reward);
        SetState(FlowState.MapSelect);
    }

    private void OnMapNodeSelected(MapNode node)
    {
        if (node.State != NodeState.Available) return;
        
        SetState(FlowState.None);
        _mapManager.MoveToNode(node);
    }

    private void CloseAllPanels()
    {
        _rewardPanel?.SetActive(false);
        _mapToggleController?.CloseMap();
    }
}
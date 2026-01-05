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

    [Header("References")]
    [SerializeField] private MapToggleController _mapToggleController;

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
        
        Debug.Log("RoomClearFlowUI: Construct 완료");
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        Debug.Log("RoomClearFlowUI: Awake");
        CloseAllPanels();
    }

    private void Start()
    {
        Debug.Log($"RoomClearFlowUI: Start - _mapToggleController={_mapToggleController != null}");
        
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
        Debug.Log("StartClearFlow 호출");
        if (_currentState != FlowState.None) return;
        SetState(FlowState.Reward);
    }

    private void SetState(FlowState newState)
    {
        Debug.Log($"SetState: {_currentState} -> {newState}");
        _currentState = newState;

        switch (_currentState)
        {
            case FlowState.Reward:
                Debug.Log("FlowState.Reward 진입");
                CloseAllPanels();
                _rewardPanel.SetActive(true);
                GenerateRewardButtons();
                break;

            case FlowState.MapSelect:
                Debug.Log("FlowState.MapSelect 진입");
                CloseAllPanels();
                
                if (_mapToggleController != null)
                {
                    Debug.Log("OpenMap 호출 시작");
                    _mapToggleController.OpenMap();
                }
                else
                {
                    Debug.LogError("_mapToggleController가 null입니다!");
                }
                break;

            case FlowState.None:
                Debug.Log("FlowState.None 진입");
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
        Debug.Log($"OnRewardSelected: {reward.DisplayName}");
        
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
        Debug.Log($"OnMapNodeSelected: {node}");
        
        if (node.State != NodeState.Available)
        {
            Debug.LogWarning($"선택 불가능한 노드입니다: {node.State}");
            return;
        }
        
        Debug.Log("SetState(None) 호출 시작");
        SetState(FlowState.None);
        
        Debug.Log("MoveToNode 호출 시작");
        MapManager.Instance.MoveToNode(node);
    }

    private void CloseAllPanels()
    {
        Debug.Log("CloseAllPanels 호출");
        
        if (_rewardPanel != null)
        {
            _rewardPanel.SetActive(false);
        }
        
        if (_mapToggleController != null)
        {
            _mapToggleController.CloseMap();
        }
        else
        {
            Debug.LogWarning("CloseAllPanels: _mapToggleController null");
        }
    }
}
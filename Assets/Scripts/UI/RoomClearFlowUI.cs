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
    [SerializeField] private GameObject _mapSelectPanel;
    
    [Header("Reward UI")]
    [SerializeField] private Transform _rewardButtonContainer;  // 보상 버튼들이 들어갈 부모
    [SerializeField] private Button _rewardButtonPrefab;        // 보상 버튼 프리팹

    [Header("Map Select Setup")]
    [SerializeField] private Transform _mapButtonContainer;
    [SerializeField] private Button _mapButtonPrefab;

    [Header("Data")]
    [SerializeField] private RewardLibrary _rewardLibrary;      // 보상 라이브러리

    private enum FlowState { None, Reward, MapSelect }
    private FlowState _currentState;

    private IReadOnlyAsyncReactiveProperty<RoomManager> _globalCurrentRoomHandle;
    private PlayerModel _playerModel; // ⭐ 추가

    [Inject]
    public void Construct(
        IReadOnlyAsyncReactiveProperty<RoomManager> globalHandle,
        PlayerModel playerModel) // ⭐ 추가
    {
        _globalCurrentRoomHandle = globalHandle;
        _playerModel = playerModel;
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
        MonitorRoomChanges().Forget();
    }

    // main logic
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

    // 상태 관리
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
                GenerateRewardButtons(); // 보상 버튼 동적 생성
                break;

            case FlowState.MapSelect:
                _mapSelectPanel.SetActive(true);
                GenerateMapButtons();
                break;
                
            case FlowState.None:
                break;
        }
    }

    // 보상 버튼 생성
    private void GenerateRewardButtons()
    {
        // 기존 버튼 제거
        foreach (Transform child in _rewardButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Null 체크
        if (_rewardLibrary == null)
        {
            Debug.LogError("RoomClearFlowUI: RewardLibrary가 할당되지 않았습니다!");
            return;
        }

        if (_rewardButtonPrefab == null)
        {
            Debug.LogError("RoomClearFlowUI: RewardButtonPrefab이 할당되지 않았습니다!");
            return;
        }

        // 랜덤 보상 3개 뽑기 (하드 코딩 나중에 수정 해야함)
        //List<RewardData> rewards = _rewardLibrary.GetRandomRewards(3);
        //int choiceCount = _playerModel != null ? _playerModel.RewardChoiceCount : 3;
        //List<RewardData> rewards = _rewardLibrary.GetRandomRewards(choiceCount);
        List<RewardData> rewards = _rewardLibrary.GetRandomRewards(_playerModel.RewardChoiceCount);

        if (rewards.Count == 0)
        {
            Debug.LogWarning("RoomClearFlowUI: 보상을 뽑을 수 없습니다!");
            return;
        }

        // 보상 버튼 생성
        foreach (var reward in rewards)
        {
            if (reward == null) continue;

            Button btn = Instantiate(_rewardButtonPrefab, _rewardButtonContainer);
            
            // 버튼 텍스트 설정
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                // 효과 요약 생성
                string effectsSummary = GetEffectsSummary(reward);
                text.text = $"[{reward.Rarity}]\n{reward.DisplayName}\n\n{effectsSummary}";
            }

            // 클릭 이벤트 연결 (Closure Capture 방지)
            RewardData capturedReward = reward;
            btn.onClick.AddListener(() => OnRewardSelected(capturedReward));
        }

        Debug.Log($"보상 버튼 {rewards.Count}개 생성");
    }

    // 효과 요약 텍스트 생성
    private string GetEffectsSummary(RewardData reward)
    {
        if (reward.Effects == null || reward.Effects.Count == 0)
        {
            return reward.Description;
        }

        // 효과가 1개면 Description 사용
        if (reward.Effects.Count == 1)
        {
            return reward.Description;
        }

        // 효과가 여러 개면 리스트로 표시
        List<string> lines = new List<string>();
        foreach (var effect in reward.Effects)
        {
            lines.Add($"• {GetEffectDescription(effect)}");
        }
        
        return string.Join("\n", lines);
    }

    // ⭐ 새로 추가: 개별 효과 설명 생성
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

    // 보상 선택 처리
    private void OnRewardSelected(RewardData reward)
    {
        Debug.Log($"UI: 보상 선택됨 - {reward.DisplayName}");
        
        // 보상 적용
        if (_playerModel != null)
        {
            _playerModel.Reward.ApplyReward(reward);
        }
        else
        {
            Debug.LogError("RoomClearFlowUI: PlayerModel이 주입되지 않았습니다!");
        }
        
        // 맵 선택 화면으로 전환
        SetState(FlowState.MapSelect);
    }

    // 맵 선택
    private void OnMapButtonClicked(MapNode nextNode)
    {
        Debug.Log($"🗺️ ROOM PICKED: {nextNode.Type}");
        SetState(FlowState.None);
        
        MapManager.Instance.MoveToNode(nextNode);
    }

    // Helper
    private void CloseAllPanels()
    {
        if(_rewardPanel != null) _rewardPanel.SetActive(false);
        if(_mapSelectPanel != null) _mapSelectPanel.SetActive(false);
    }

    private void GenerateMapButtons()
    {
        foreach (Transform child in _mapButtonContainer)
        {
            Destroy(child.gameObject);
        }

        var nextNodes = MapManager.Instance.CurrentNode.NextNodes;

        if (nextNodes == null || nextNodes.Count == 0)
        {
            Debug.LogWarning("DEAD END");
            return;
        }

        foreach (var node in nextNodes)
        {
            Button btn = Instantiate(_mapButtonPrefab, _mapButtonContainer);
            
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = $"Go to\n{node.Type}";

            MapNode targetNode = node; 
            btn.onClick.AddListener(() => OnMapButtonClicked(targetNode));
        }
    }
}
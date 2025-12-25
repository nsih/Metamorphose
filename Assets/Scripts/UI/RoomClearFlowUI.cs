using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq; // UniTask.Linq 필수 (이벤트 스트림 처리)
using Common;
using Reflex.Attributes; // DI
using System.Threading;

public class RoomClearFlowUI : MonoBehaviour
{
    public static RoomClearFlowUI Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject _rewardPanel;
    [SerializeField] private GameObject _mapSelectPanel;
    
    // [Fix] 보상 패널에 있는 '확인(Confirm)' 버튼을 인스펙터에서 연결해야 함
    [SerializeField] private Button _rewardConfirmButton; 

    [Header("Map Select Setup")]
    [SerializeField] private Transform _buttonContainer;
    [SerializeField] private Button _mapButtonPrefab;

    private enum FlowState { None, Reward, MapSelect }
    private FlowState _currentState;

    private IReadOnlyAsyncReactiveProperty<RoomManager> _globalCurrentRoomHandle;

    [Inject]
    public void Construct(IReadOnlyAsyncReactiveProperty<RoomManager> globalHandle)
    {
        _globalCurrentRoomHandle = globalHandle;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // reward event
        if (_rewardConfirmButton != null)
        {
            _rewardConfirmButton.onClick.RemoveAllListeners();
            _rewardConfirmButton.onClick.AddListener(OnRewardConfirmed);
        }
        else
        {
            Debug.LogError("Reward button");
        }
        
        CloseAllPanels();
    }

    private void Start()
    {
        MonitorRoomChanges().Forget();
    }



    // main logic
    private async UniTaskVoid MonitorRoomChanges()
    {
        // _globalCurrentRoomHandle이 바뀔 때마다(새 방 진입 시) 루프가 돕니다.
        await foreach (var currentRoom in _globalCurrentRoomHandle)
        {
            if (currentRoom == null) continue;

            // 새로운 방의 상태를 구독
            MonitorCurrentRoomState(currentRoom).Forget();
        }
    }

    private async UniTaskVoid MonitorCurrentRoomState(RoomManager room)
    {
        var destroyToken = this.GetCancellationTokenOnDestroy();
        var roomDestroyToken = room.GetCancellationTokenOnDestroy();
        
        // 링크된 토큰: UI가 꺼지거나 or 방이 사라지면 감시 중단
        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(destroyToken, roomDestroyToken).Token;

        try
        {
            await room.CurrentRoomState
                .Where(state => state == RoomState.Complete)
                .FirstAsync(linkedToken); // 최초 1회 통과

            StartClearFlow();
        }
        catch (System.OperationCanceledException) { }
    }


    // 상태 관리
    private void StartClearFlow()
    {
        // 이미 진행 중이면 무시
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
                // 여기에 보상 데이터 세팅 로직 추가 가능 (ex: RewardManager.GetRandomRewards())
                break;

            case FlowState.MapSelect:
                _mapSelectPanel.SetActive(true);
                GenerateMapButtons(); // 버튼 동적 생성
                break;
                
            case FlowState.None:
                // 게임 플레이 중 (패널 다 꺼짐)
                break;
        }
    }

    // 보상 선택
    public void OnRewardConfirmed()
    {
        Debug.Log("UI PICKED (Reward Confirmed)");
        SetState(FlowState.MapSelect);
    }

    // 맵 선택
    private void OnMapButtonClicked(MapNode nextNode)
    {
        Debug.Log($"ROOM PICKED: {nextNode.Type}");
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
        foreach (Transform child in _buttonContainer)
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
            Button btn = Instantiate(_mapButtonPrefab, _buttonContainer);
            
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = $"Go to\n{node.Type}";

            // [Closure Capture 방지]
            MapNode targetNode = node; 
            btn.onClick.AddListener(() => OnMapButtonClicked(targetNode));
        }
    }
}
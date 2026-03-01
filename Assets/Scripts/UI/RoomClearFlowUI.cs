using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Common;
using Reflex.Attributes;
using System.Collections.Generic;
using R3;

public class RoomClearFlowUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _rewardPanel;

    [Header("Reward UI")]
    [SerializeField] private Transform _rewardButtonContainer;
    [SerializeField] private Button _rewardButtonPrefab;

    [Header("References")]
    [SerializeField] private MapToggleController _mapToggleController;

    private enum FlowState { None, Reward, MapSelect }
    private FlowState _currentState;

    private ReactiveProperty<RoomManager> _globalCurrentRoomHandle;
    private PlayerModel _playerModel;
    private MapUIManager _mapUIManager;
    private MapManager _mapManager;
    private ItemRepository _itemRepository;
    private ItemApplyService _itemApplyService;

    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private System.IDisposable _roomStateSubscription;

    [Inject]
    public void Construct(
        ReactiveProperty<RoomManager> globalHandle,
        PlayerModel playerModel,
        MapUIManager mapUIManager,
        MapManager mapManager,
        ItemRepository itemRepository,
        ItemApplyService itemApplyService)
    {
        _globalCurrentRoomHandle = globalHandle;
        _playerModel = playerModel;
        _mapUIManager = mapUIManager;
        _mapManager = mapManager;
        _itemRepository = itemRepository;
        _itemApplyService = itemApplyService;
    }

    private void Awake()
    {
        _rewardPanel?.SetActive(false);
    }

    private void Start()
    {
        if (_mapUIManager != null)
            _mapUIManager.OnNodeSelected += OnMapNodeSelected;

        _globalCurrentRoomHandle
            .Subscribe(room =>
            {
                _roomStateSubscription?.Dispose();
                _currentState = FlowState.None;

                if (room == null) return;

                _roomStateSubscription = room.CurrentRoomState
                    .Subscribe(state =>
                    {
                        if (state == RoomState.Complete)
                            StartClearFlow();
                    });
            })
            .AddTo(_disposables);
    }

    private void OnDestroy()
    {
        if (_mapUIManager != null)
            _mapUIManager.OnNodeSelected -= OnMapNodeSelected;

        _disposables.Dispose();
        _roomStateSubscription?.Dispose();
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
                _mapToggleController?.CloseMap();
                _rewardPanel?.SetActive(true);
                GenerateRewardButtons();
                break;

            case FlowState.MapSelect:
                _rewardPanel?.SetActive(false);
                _mapToggleController?.OpenMap();
                break;

            case FlowState.None:
                _rewardPanel?.SetActive(false);
                _mapToggleController?.CloseMap();
                break;
        }
    }

    private void GenerateRewardButtons()
    {
        foreach (Transform child in _rewardButtonContainer)
            Destroy(child.gameObject);

        if (_rewardButtonPrefab == null) return;

        List<ItemSO> items = _itemRepository.GetRandomRewards(
            _playerModel.RewardChoiceCount,
            _itemApplyService.Registry);

        foreach (var item in items)
        {
            if (item == null) continue;

            Button btn = Instantiate(_rewardButtonPrefab, _rewardButtonContainer);

            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = BuildItemText(item);

            ItemSO captured = item;
            btn.onClick.AddListener(() => OnItemSelected(captured));
        }
    }

    private string BuildItemText(ItemSO item)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[{item.tier}]");
        sb.AppendLine(item.nameKr);
        sb.AppendLine();
        sb.Append(item.description);
        return sb.ToString();
    }

    private void OnItemSelected(ItemSO item)
    {
        _itemApplyService.Apply(item);
        SetState(FlowState.MapSelect);
    }

    private void OnMapNodeSelected(MapNode node)
    {
        if (node.State != NodeState.Available) return;

        SetState(FlowState.None);
        _mapManager.MoveToNode(node);
    }
}
// Assets/Scripts/UI/RoomClearFlowUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Common;
using Reflex.Attributes;
using System.Collections.Generic;

public class RoomClearFlowUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _rewardPanel;

    [Header("Reward UI")]
    [SerializeField] private Transform _rewardButtonContainer;
    [SerializeField] private Button _rewardButtonPrefab;

    [Header("References")]
    [SerializeField] private MapToggleController _mapToggleController;

    private RoomClearFlowController _flowController;
    private PlayerModel _playerModel;
    private MapUIManager _mapUIManager;
    private MapManager _mapManager;
    private ItemRepository _itemRepository;
    private ItemApplyService _itemApplyService;
    private IInputService _inputService;

    [Inject]
    public void Construct(
        RoomClearFlowController flowController,
        PlayerModel playerModel,
        MapUIManager mapUIManager,
        MapManager mapManager,
        ItemRepository itemRepository,
        ItemApplyService itemApplyService,
        IInputService inputService)
    {
        _flowController = flowController;
        _playerModel = playerModel;
        _mapUIManager = mapUIManager;
        _mapManager = mapManager;
        _itemRepository = itemRepository;
        _itemApplyService = itemApplyService;
        _inputService = inputService;
    }

    private void Awake()
    {
        _rewardPanel?.SetActive(false);
    }

    private void Start()
    {
        if (_mapUIManager != null)
            _mapUIManager.OnNodeSelected += OnMapNodeSelected;

        if (_flowController != null)
        {
            _flowController.OnRequestRewardUI += ShowRewardUI;
            _flowController.OnRequestMapUI += ShowMapUI;
        }
    }

    private void OnDestroy()
    {
        if (_mapUIManager != null)
            _mapUIManager.OnNodeSelected -= OnMapNodeSelected;

        if (_flowController != null)
        {
            _flowController.OnRequestRewardUI -= ShowRewardUI;
            _flowController.OnRequestMapUI -= ShowMapUI;
        }
    }

    private void ShowRewardUI()
    {
        _mapToggleController?.CloseMap();
        _rewardPanel?.SetActive(true);
        GenerateRewardButtons();
    }

    private void ShowMapUI()
    {
        _rewardPanel?.SetActive(false);
        _inputService?.SetEnabled(true);
        _mapToggleController?.OpenMap();
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
        _rewardPanel?.SetActive(false);

        // Controller에 보상 완료 알림 -> 맵 UI 요청
        _flowController?.NotifyRewardComplete();
    }

    private void OnMapNodeSelected(MapNode node)
    {
        if (node.State != NodeState.Available) return;

        _mapToggleController?.CloseMap();
        _inputService?.SetEnabled(true);
        _mapManager.MoveToNode(node);
    }
}
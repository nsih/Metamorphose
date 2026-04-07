// Assets/Scripts/UI/SimpleRoomUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Common;
using Reflex.Attributes;
using R3;

public class SimpleRoomUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private Button _nextButton;

    [Header("References")]
    [SerializeField] private MapToggleController _mapToggleController;

    private ReactiveProperty<RoomManager> _roomHandle;
    private MapManager _mapManager;
    private MapUIManager _mapUIManager;
    private IInputService _inputService;

    private readonly SerialDisposable _roomSub = new SerialDisposable();
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    private RoomManager _currentRoom;

    [Inject]
    public void Construct(
        ReactiveProperty<RoomManager> roomHandle,
        MapManager mapManager,
        MapUIManager mapUIManager,
        IInputService inputService)
    {
        _roomHandle = roomHandle;
        _mapManager = mapManager;
        _mapUIManager = mapUIManager;
        _inputService = inputService;
    }

    private void Awake()
    {
        _panel?.SetActive(false);
    }

    private void Start()
    {
        _roomSub.AddTo(_disposables);

        _roomHandle
            .Subscribe(OnRoomChanged)
            .AddTo(_disposables);

        if (_nextButton != null)
            _nextButton.onClick.AddListener(OnNextClicked);

        if (_mapUIManager != null)
            _mapUIManager.OnNodeSelected += OnMapNodeSelected;
    }

    private void OnDestroy()
    {
        if (_mapUIManager != null)
            _mapUIManager.OnNodeSelected -= OnMapNodeSelected;

        _disposables.Dispose();
    }

    private void OnRoomChanged(RoomManager room)
    {
        _panel?.SetActive(false);
        _currentRoom = room;

        if (room == null)
        {
            _roomSub.Disposable = Disposable.Empty;
            return;
        }

        if (!IsSimpleRoom())
        {
            _roomSub.Disposable = Disposable.Empty;
            return;
        }

        // 이벤트/상점방이면 즉시 UI 표시
        ShowSimpleUI();
    }

    private void ShowSimpleUI()
    {
        if (_mapManager == null || _mapManager.CurrentNode == null) return;

        RoomType type = _mapManager.CurrentNode.Type;

        if (_titleText != null)
        {
            if (type == RoomType.Event)
                _titleText.text = "이벤트";
            else if (type == RoomType.Shop)
                _titleText.text = "상점";
            else
                _titleText.text = "";
        }

        _panel?.SetActive(true);
        _inputService?.SetEnabled(false);
    }

    private void OnNextClicked()
    {
        _panel?.SetActive(false);

        if (_currentRoom != null)
        {
            _currentRoom.CompleteRoom();
        }

        _inputService?.SetEnabled(true);
        _mapToggleController?.OpenMap();
    }

    private void OnMapNodeSelected(MapNode node)
    {
        if (node.State != NodeState.Available) return;

        _mapToggleController?.CloseMap();
        _inputService?.SetEnabled(true);
        _mapManager.MoveToNode(node);
    }

    private bool IsSimpleRoom()
    {
        if (_mapManager == null || _mapManager.CurrentNode == null) return false;

        RoomType type = _mapManager.CurrentNode.Type;
        if (type == RoomType.Event || type == RoomType.Shop)
            return true;

        return false;
    }
}
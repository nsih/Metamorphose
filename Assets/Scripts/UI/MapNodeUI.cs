using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Common;

public class MapNodeUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image _background;
    [SerializeField] private Image _icon;
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _debugText;

    [Header("Visual Settings")]
    [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color _availableColor = Color.white;
    [SerializeField] private Color _completedColor = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color _currentColor = Color.yellow;
    [SerializeField] private Color _highlightColor = Color.cyan;

    [Header("Icons")]
    [SerializeField] private Sprite _battleIcon;
    [SerializeField] private Sprite _shopIcon;
    [SerializeField] private Sprite _eliteIcon;
    [SerializeField] private Sprite _bossIcon;
    [SerializeField] private Sprite _eventIcon;
    [SerializeField] private Sprite _startIcon;

    private MapNode _node;
    private bool _isCurrent;
    private bool _isHighlighted;
    private MapUIManager _manager;

    public void Initialize(MapNode node, bool isCurrent, MapUIManager manager)
    {
        _node = node;
        _isCurrent = isCurrent;
        _manager = manager;
        
        UpdateVisuals();
        SetupButton();
    }

    private void UpdateVisuals()
    {
        if (_node == null) return;

        _icon.sprite = GetIconForType(_node.Type);

        if (_isCurrent)
        {
            _background.color = _currentColor;
        }
        else if (_isHighlighted)
        {
            _background.color = _highlightColor;
        }
        else
        {
            switch (_node.State)
            {
                case NodeState.Locked:
                    _background.color = _lockedColor;
                    break;
                case NodeState.Available:
                    _background.color = _availableColor;
                    break;
                case NodeState.Completed:
                    _background.color = _completedColor;
                    break;
            }
        }

        if (_debugText != null)
        {
            _debugText.text = $"L{_node.Layer}-{_node.IndexInLayer}";
        }
    }

    private Sprite GetIconForType(RoomType type)
    {
        switch (type)
        {
            case RoomType.Start: return _startIcon != null ? _startIcon : _battleIcon;
            case RoomType.Battle: return _battleIcon;
            case RoomType.Shop: return _shopIcon;
            case RoomType.Elite: return _eliteIcon;
            case RoomType.Boss: return _bossIcon;
            case RoomType.Event: return _eventIcon;
            default: return _battleIcon;
        }
    }

    private void SetupButton()
    {
        if (_button == null) return;

        _button.onClick.RemoveAllListeners();

        if (_node.State == NodeState.Available && !_isCurrent)
        {
            _button.interactable = true;
            _button.onClick.AddListener(OnClicked);
        }
        else
        {
            _button.interactable = false;
        }
    }

    private void OnClicked()
    {
        if (_manager != null)
        {
            _manager.OnNodeClicked(_node);
        }
    }

    public void SetHighlight(bool highlight)
    {
        _isHighlighted = highlight;
        UpdateVisuals();
    }
}
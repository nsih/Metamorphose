// Assets/Scripts/UI/InteractPromptView.cs
using UnityEngine;
using TMPro;
using Reflex.Attributes;

public class InteractPromptView : MonoBehaviour
{
    [SerializeField] private RectTransform _promptRoot;
    [SerializeField] private TextMeshProUGUI _promptText;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Vector3 _worldOffset = new Vector3(0, 1.5f, 0);

    private RoomClearFlowController _flowController;
    private IInputService _inputService;
    private Camera _mainCamera;

    private ClearPortal _trackedPortal;
    private bool _isVisible;

    [Inject]
    public void Construct(RoomClearFlowController flowController, IInputService inputService)
    {
        _flowController = flowController;
        _inputService = inputService;
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        Hide();

        _inputService.OnInteractPressed += OnInteract;
    }

    private void OnDestroy()
    {
        if (_inputService != null)
            _inputService.OnInteractPressed -= OnInteract;

        UnbindPortal();
    }

    private void Update()
    {
        // ActivePortal 변경 감지
        ClearPortal current = null;
        if (_flowController != null)
            current = _flowController.ActivePortal;

        if (current != _trackedPortal)
        {
            UnbindPortal();
            if (current != null)
            {
                BindPortal(current);
            }
        }

        // 위치 갱신
        if (_isVisible && _trackedPortal != null && _mainCamera != null)
        {
            Vector3 worldPos = _trackedPortal.InteractPoint.position + _worldOffset;
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPos);

            if (screenPos.z > 0)
            {
                _promptRoot.position = screenPos;
            }
        }
    }

    private void BindPortal(ClearPortal portal)
    {
        _trackedPortal = portal;
        _trackedPortal.OnPlayerRangeChanged += OnRangeChanged;

        if (_promptText != null)
        {
            _promptText.text = $"[E] {portal.PromptText}";
        }
    }

    private void UnbindPortal()
    {
        if (_trackedPortal != null)
        {
            _trackedPortal.OnPlayerRangeChanged -= OnRangeChanged;
            _trackedPortal = null;
        }
        Hide();
    }

    private void OnRangeChanged(bool inRange)
    {
        if (inRange)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void OnInteract()
    {
        if (!_isVisible) return;
        if (_trackedPortal == null) return;

        _trackedPortal.Interact();
    }

    private void Show()
    {
        _isVisible = true;
        if (_canvasGroup != null)
            _canvasGroup.alpha = 1f;
        if (_promptRoot != null)
            _promptRoot.gameObject.SetActive(true);
    }

    private void Hide()
    {
        _isVisible = false;
        if (_canvasGroup != null)
            _canvasGroup.alpha = 0f;
        if (_promptRoot != null)
            _promptRoot.gameObject.SetActive(false);
    }
}
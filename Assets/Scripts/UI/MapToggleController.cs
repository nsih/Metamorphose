// Assets/Scripts/UI/MapToggleController.cs
using UnityEngine;
using UnityEngine.InputSystem;
using Reflex.Attributes;

public class MapToggleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _mapScrollView;
    [SerializeField] private MapUIManager _mapUIManager;

    private MapManager _mapManager;
    private IInputService _inputService;

    private bool _isMapOpen = false;

    [Inject]
    public void Construct(MapManager mapManager, IInputService inputService)
    {
        _mapManager = mapManager;
        _inputService = inputService;
    }

    private void Start()
    {
        if (_mapScrollView != null)
        {
            _mapScrollView.SetActive(false);
            _isMapOpen = false;
        }
    }

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleMap();
        }
    }

    private void ToggleMap()
    {
        if (_mapScrollView == null) return;

        if (_isMapOpen)
        {
            CloseMap();
            return;
        }

        OpenMap();
    }

    public void OpenMap()
    {
        if (_mapScrollView == null) return;

        if (_mapManager == null || _mapManager.CurrentMap == null || _mapManager.CurrentNode == null)
        {
            Debug.Log("MapToggle: map data not ready");
            return;
        }

        _isMapOpen = true;
        _mapScrollView.SetActive(true);
        _inputService?.SetEnabled(false);
        RefreshMapUI();
    }

    public void CloseMap()
    {
        if (_mapScrollView == null) return;

        _isMapOpen = false;
        _mapScrollView.SetActive(false);
        _inputService?.SetEnabled(true);
    }

    private void RefreshMapUI()
    {
        if (_mapUIManager == null || _mapManager == null) return;

        var currentMap = _mapManager.CurrentMap;
        var currentNode = _mapManager.CurrentNode;

        if (currentMap == null || currentNode == null) return;

        _mapUIManager.RenderMap(currentMap, currentNode);

        if (currentNode.NextNodeIds != null && currentNode.NextNodeIds.Count > 0)
        {
            _mapUIManager.HighlightAvailableNodes(currentNode.NextNodeIds);
        }
    }

    private void OnDestroy()
    {
        if (_isMapOpen)
        {
            _inputService?.SetEnabled(true);
        }
    }
}
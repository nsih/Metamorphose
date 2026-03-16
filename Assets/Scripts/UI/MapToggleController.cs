using UnityEngine;
using UnityEngine.InputSystem;
using Reflex.Attributes;

public class MapToggleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _mapScrollView;
    [SerializeField] private MapUIManager _mapUIManager;

    private MapManager _mapManager;

    private bool _isMapOpen = false;
    private float _previousTimeScale = 1f;

    [Inject]
    public void Construct(MapManager mapManager)
    {
        _mapManager = mapManager;
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
            _isMapOpen = false;
            _mapScrollView.SetActive(false);
            Time.timeScale = _previousTimeScale;
            return;
        }

        if (_mapManager == null || _mapManager.CurrentMap == null || _mapManager.CurrentNode == null)
        {
            Debug.Log("MapToggle: map data not ready");
            return;
        }

        _isMapOpen = true;
        _mapScrollView.SetActive(true);
        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        RefreshMapUI();
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
        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        RefreshMapUI();
    }

    public void CloseMap()
    {
        if (_mapScrollView == null) return;

        _isMapOpen = false;
        _mapScrollView.SetActive(false);
        Time.timeScale = _previousTimeScale;
    }

    private void OnDestroy()
    {
        if (_isMapOpen)
        {
            Time.timeScale = _previousTimeScale;
        }
    }
}
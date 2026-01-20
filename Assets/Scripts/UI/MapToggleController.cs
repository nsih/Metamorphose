using UnityEngine;
using Reflex.Attributes;

public class MapToggleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _mapScrollView;
    [SerializeField] private MapUIManager _mapUIManager;
    
    [Header("Settings")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.Tab;

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
        if (Input.GetKeyDown(_toggleKey))
        {
            ToggleMap();
        }
    }

    private void ToggleMap()
    {
        if (_mapScrollView == null) return;

        _isMapOpen = !_isMapOpen;
        _mapScrollView.SetActive(_isMapOpen);

        if (_isMapOpen)
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            RefreshMapUI();
        }
        else
        {
            Time.timeScale = _previousTimeScale;
        }
    }

    private void RefreshMapUI()
    {
        if (_mapUIManager == null || _mapManager == null) return;
        
        var currentMap = _mapManager.GetCurrentMap();
        var currentNode = _mapManager.CurrentNode;
        
        if (currentMap == null || currentNode == null) return;
        
        _mapUIManager.RenderMap(currentMap, currentNode);
        
        if (currentNode.NextNodes != null && currentNode.NextNodes.Count > 0)
        {
            _mapUIManager.HighlightAvailableNodes(currentNode.NextNodes);
        }
    }

    public void OpenMap()
    {
        if (_mapScrollView == null) return;

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
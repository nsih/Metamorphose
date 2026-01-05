using UnityEngine;

public class MapToggleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _mapScrollView;
    [SerializeField] private MapUIManager _mapUIManager;
    
    [Header("Settings")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.Tab;
    
    private bool _isMapOpen = false;
    private float _previousTimeScale = 1f;

    private void Start()
    {
        if (_mapScrollView != null)
        {
            _mapScrollView.SetActive(false);
            _isMapOpen = false;
            Debug.Log("MapToggleController: 초기화 완료");
        }
        else
        {
            Debug.LogError("MapToggleController: _mapScrollView null!");
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
        if (_mapScrollView == null)
        {
            Debug.LogError("MapScrollView가 할당되지 않았습니다!");
            return;
        }

        _isMapOpen = !_isMapOpen;
        _mapScrollView.SetActive(_isMapOpen);

        if (_isMapOpen)
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            
            RefreshMapUI();
            
            Debug.Log($"맵 토글 열림: timeScale={Time.timeScale}");
        }
        else
        {
            Time.timeScale = _previousTimeScale;
            Debug.Log($"맵 토글 닫힘: timeScale={Time.timeScale}");
        }
    }

    private void RefreshMapUI()
    {
        Debug.Log("RefreshMapUI 호출");
        
        if (_mapUIManager == null)
        {
            Debug.LogError("MapToggleController: _mapUIManager null!");
            return;
        }
        
        if (MapManager.Instance == null)
        {
            Debug.LogError("MapToggleController: MapManager.Instance null!");
            return;
        }
        
        var currentMap = MapManager.Instance.GetCurrentMap();
        var currentNode = MapManager.Instance.CurrentNode;
        
        if (currentMap == null)
        {
            Debug.LogError("MapToggleController: currentMap null!");
            return;
        }
        
        if (currentNode == null)
        {
            Debug.LogError("MapToggleController: currentNode null!");
            return;
        }
        
        Debug.Log($"RenderMap 호출: currentNode={currentNode}");
        _mapUIManager.RenderMap(currentMap, currentNode);
        
        if (currentNode.NextNodes != null && currentNode.NextNodes.Count > 0)
        {
            Debug.Log($"하이라이트: {currentNode.NextNodes.Count}개 노드");
            _mapUIManager.HighlightAvailableNodes(currentNode.NextNodes);
        }
    }

    public void OpenMap()
    {
        Debug.Log("OpenMap 호출됨");
        
        if (_mapScrollView == null)
        {
            Debug.LogError("OpenMap: _mapScrollView null!");
            return;
        }

        _isMapOpen = true;
        _mapScrollView.SetActive(true);
        
        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        
        RefreshMapUI();
        
        Debug.Log($"맵 강제 열림: Active={_mapScrollView.activeSelf}, timeScale={Time.timeScale}");
    }

    public void CloseMap()
    {
        Debug.Log("CloseMap 호출됨");
        
        if (_mapScrollView == null)
        {
            Debug.LogError("CloseMap: _mapScrollView null!");
            return;
        }

        _isMapOpen = false;
        _mapScrollView.SetActive(false);
        
        Time.timeScale = _previousTimeScale;
        
        Debug.Log($"맵 강제 닫힘: Active={_mapScrollView.activeSelf}, timeScale={Time.timeScale}");
    }

    private void OnDestroy()
    {
        if (_isMapOpen)
        {
            Time.timeScale = _previousTimeScale;
        }
    }
}
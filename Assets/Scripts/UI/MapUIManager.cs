using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Common;
using System;

public class MapUIManager : MonoBehaviour
{
    [Header("ScrollView Components")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _contentRect;
    [SerializeField] private GameObject _mapUIRoot;

    [Header("Prefabs")]
    [SerializeField] private MapNodeUI _nodeUIPrefab;

    [Header("Layout Settings")]
    [SerializeField] private float _horizontalSpacing = 300f;
    [SerializeField] private float _verticalSpacing = 150f;
    [SerializeField] private float _padding = 100f;

    [Header("Organic Layout Settings")]
    [SerializeField] private float _positionJitter = 20f;
    [SerializeField] private int _layoutSeed = 42;

    [Header("Line Renderer")]
    [SerializeField] private MapLineRenderer _lineRenderer;

    private Dictionary<MapNode, MapNodeUI> _nodeUIMap = new Dictionary<MapNode, MapNodeUI>();
    private Dictionary<MapNode, RectTransform> _nodePositions = new Dictionary<MapNode, RectTransform>();
    private Map _currentMap;
    private MapNode _currentNode;
    private int _maxNodesPerLayer;

    public event Action<MapNode> OnNodeSelected;

    private void Awake()
    {
        if (_mapUIRoot != null)
        {
            _mapUIRoot.SetActive(false);
        }
    }

    public void RenderMap(Map map, MapNode currentNode)
    {
        _currentMap = map;
        _currentNode = currentNode;
        ClearMap();

        _maxNodesPerLayer = CalculateMaxNodesPerLayer(map);

        _contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        _contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        _contentRect.pivot = new Vector2(0.5f, 0.5f);
        _contentRect.anchoredPosition = Vector2.zero;

        CalculateContentSize(map);
        CreateNodeUIs(map, currentNode);
        DrawConnections();

        Canvas.ForceUpdateCanvases();
        
        ScrollToCurrentNode();
    }

    private int CalculateMaxNodesPerLayer(Map map)
    {
        int maxNodes = 0;
        for(int layer = 0; layer < map.LayerCount; layer++)
        {
            List<MapNode> nodes = map.GetNodesInLayer(layer);
            if(nodes.Count > maxNodes)
            {
                maxNodes = nodes.Count;
            }
        }

        return maxNodes;
    }

    private void ScrollToCurrentNode()
    {
        if (_currentNode == null || !_nodePositions.ContainsKey(_currentNode))
        {
            _scrollRect.horizontalNormalizedPosition = 0f;
            return;
        }

        RectTransform currentNodeRect = _nodePositions[_currentNode];
        
        float contentWidth = _contentRect.rect.width;
        float viewportWidth = _scrollRect.viewport.rect.width;
        
        if (contentWidth <= viewportWidth)
        {
            _scrollRect.horizontalNormalizedPosition = 0f;
            return;
        }

        float nodeWorldX = currentNodeRect.anchoredPosition.x;
        
        float contentLeftEdge = -contentWidth / 2f;
        float nodePositionInContent = nodeWorldX - contentLeftEdge;
        
        float targetPosition = nodePositionInContent - (viewportWidth / 2f);
        
        float scrollableWidth = contentWidth - viewportWidth;
        float normalizedPosition = Mathf.Clamp01(targetPosition / scrollableWidth);
        
        _scrollRect.horizontalNormalizedPosition = normalizedPosition;
    }

    void CalculateContentSize(Map map)
    {
        float width = map.LayerCount * _horizontalSpacing + _padding * 2;
        float height = _maxNodesPerLayer * _verticalSpacing + _padding * 2;

        _contentRect.sizeDelta = new Vector2(width, height);
    }

    void CreateNodeUIs(Map map, MapNode currentNode)
    {
        for(int layer = 0; layer < map.LayerCount; layer++)
        {
            List<MapNode> nodes = map.GetNodesInLayer(layer);
            foreach (var node in nodes)
            {
                Vector2 position = CalculateNodePosition(node.Layer, node.IndexInLayer, node.NodeID, node.Type);

                MapNodeUI nodeUI = Instantiate(_nodeUIPrefab, _contentRect);
                RectTransform rect = nodeUI.GetComponent<RectTransform>();
                rect.anchoredPosition = position;

                bool isCurrent = (node == currentNode);
                nodeUI.Initialize(node, isCurrent, this);

                _nodeUIMap[node] = nodeUI;
                _nodePositions[node] = rect;
            }
        }
    }

    private Vector2 CalculateNodePosition(int layer, int indexInLayer, int nodeId, RoomType type)
    {
        float totalWidth = (_currentMap.LayerCount - 1) * _horizontalSpacing;
        float startX = -totalWidth / 2f;
        float x = startX + (layer * _horizontalSpacing);
        
        float y;
        
        if (type == RoomType.Start || type == RoomType.Boss)
        {
            y = 0f;
        }
        else
        {
            float totalHeight = (_maxNodesPerLayer - 1) * _verticalSpacing;
            float startY = totalHeight / 2f;
            y = startY - (indexInLayer * _verticalSpacing);
        }

        System.Random rng = new System.Random(_layoutSeed + nodeId);
        float jitterX = ((float)rng.NextDouble() * 2f - 1f) * _positionJitter;
        float jitterY = ((float)rng.NextDouble() * 2f - 1f) * _positionJitter;
        
        return new Vector2(x + jitterX, y + jitterY);
    }

    private void DrawConnections()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.DrawLines(_nodePositions, _currentMap);
        }
    }

    public void HighlightAvailableNodes(List<int> availableNodeIds)
    {
        foreach (var kvp in _nodeUIMap)
        {
            bool isAvailable = availableNodeIds.Contains(kvp.Key.NodeID);
            kvp.Value.SetHighlight(isAvailable);
        }
    }

    public void OnNodeClicked(MapNode node)
    {
        Debug.Log($"MapUI: 노드 클릭됨 - {node}");

        if (node.State != NodeState.Available)
        {
            Debug.LogWarning($"접근 불가: {node.State} 상태");
            return;
        }

        if (!_currentMap.IsNodeConnected(_currentNode.NodeID, node.NodeID))
        {
            Debug.LogWarning($"직접 연결되지 않은 노드: {node}");
            return;
        }

        OnNodeSelected?.Invoke(node);
    }

    private void ClearMap()
    {
        foreach (var kvp in _nodeUIMap)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value.gameObject);
            }
        }

        _nodeUIMap.Clear();
        _nodePositions.Clear();

        if (_lineRenderer != null)
        {
            _lineRenderer.ClearLines();
        }
    }

    private void OnDestroy()
    {
        ClearMap();
    }
}
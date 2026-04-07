using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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

        // 일단 임시 크기로 노드 먼저 생성
        CalculateContentSize(map, 0f);
        CreateNodeUIs(map, currentNode);
        DrawConnections();

        StartCoroutine(ScrollAfterLayout());
    }

    private IEnumerator ScrollAfterLayout()
    {
        // 레이아웃 빌드 완료 대기
        yield return new WaitForEndOfFrame();

        // 실제 인스턴스에서 노드 크기 읽기
        float nodeHeight = GetActualNodeHeight();
        float nodeWidth = GetActualNodeWidth();

        // 실제 크기 기반으로 content 재계산
        CalculateContentSize(_currentMap, nodeHeight);

        // 레이아웃 재적용
        Canvas.ForceUpdateCanvases();

        float contentHeight = _contentRect.rect.height;
        float viewportHeight = _scrollRect.viewport.rect.height;

        _scrollRect.StopMovement();
        _scrollRect.vertical = contentHeight > viewportHeight;

        _contentRect.anchoredPosition = new Vector2(CalculateHorizontalOffset(nodeWidth), 0f);
    }

    private float GetActualNodeHeight()
    {
        foreach (var rect in _nodePositions.Values)
        {
            if (rect != null)
                return rect.rect.height;
        }
        return 0f;
    }

    private float GetActualNodeWidth()
    {
        foreach (var rect in _nodePositions.Values)
        {
            if (rect != null)
                return rect.rect.width;
        }
        return 0f;
    }

    private float CalculateHorizontalOffset(float nodeWidth)
    {
        if (_currentNode == null || !_nodePositions.ContainsKey(_currentNode))
            return 0f;

        float contentWidth = _contentRect.rect.width;
        float viewportWidth = _scrollRect.viewport.rect.width;

        if (contentWidth <= viewportWidth)
            return 0f;

        float nodeX = _nodePositions[_currentNode].anchoredPosition.x;
        float maxOffset = (contentWidth - viewportWidth) / 2f;
        float targetOffset = -nodeX;

        return Mathf.Clamp(targetOffset, -maxOffset, maxOffset);
    }

    private int CalculateMaxNodesPerLayer(Map map)
    {
        int maxNodes = 0;
        for (int layer = 0; layer < map.LayerCount; layer++)
        {
            List<MapNode> nodes = map.GetNodesInLayer(layer);
            if (nodes.Count > maxNodes)
            {
                maxNodes = nodes.Count;
            }
        }
        return maxNodes;
    }

    private void CalculateContentSize(Map map, float nodeHeight)
    {
        float width = map.LayerCount * _horizontalSpacing + _padding * 2;
        // 노드 중심 간격 범위 + 노드 실제 높이(위아래 절반씩) + 패딩
        float height = (_maxNodesPerLayer - 1) * _verticalSpacing + nodeHeight + _padding * 2;

        _contentRect.sizeDelta = new Vector2(width, height);
    }

    private void CreateNodeUIs(Map map, MapNode currentNode)
    {
        for (int layer = 0; layer < map.LayerCount; layer++)
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
        Debug.Log($"MapUI: node clicked - {node}");

        if (node.State != NodeState.Available)
        {
            Debug.LogWarning($"MapUI: invalid state - {node.State}");
            return;
        }

        if (!_currentMap.IsNodeConnected(_currentNode.NodeID, node.NodeID))
        {
            Debug.LogWarning($"MapUI: not connected - {node}");
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
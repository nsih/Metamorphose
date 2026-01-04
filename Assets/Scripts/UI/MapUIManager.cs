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

    [Header("Line Renderer")]
    [SerializeField] private MapLineRenderer _lineRenderer;

    private Dictionary<MapNode, MapNodeUI> _nodeUIMap = new Dictionary<MapNode, MapNodeUI>();
    private Dictionary<MapNode, RectTransform> _nodePositions = new Dictionary<MapNode, RectTransform>();
    private List<List<MapNode>> _currentGrid;

    // 이벤트 추가
    public event Action<MapNode> OnNodeSelected;

    private void Awake()
    {
        HideMap();
    }

    public void ShowMap()
    {
        
        if (_mapUIRoot == null)
        {
            return;
        }
        
        _mapUIRoot.SetActive(true);
    }

    public void HideMap()
    {
        if (_mapUIRoot != null)
        {
            _mapUIRoot.SetActive(false);
        }
    }

    public void RenderMap(List<List<MapNode>> grid, MapNode currentNode)
    {
        _currentGrid = grid;
        ClearMap();

        // Content를 중앙 기준으로
        _contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        _contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        _contentRect.pivot = new Vector2(0.5f, 0.5f);
        _contentRect.anchoredPosition = Vector2.zero;
        
        _scrollRect.horizontalNormalizedPosition = 0f;

        CalculateContentSize(grid);
        CreateNodeUIs(grid, currentNode);
        DrawConnections();

        Canvas.ForceUpdateCanvases();
        _scrollRect.horizontalNormalizedPosition = 0f;
    }
    private void CalculateContentSize(List<List<MapNode>> grid)
    {
        float width = grid.Count * _horizontalSpacing + _padding * 2;

        int maxNodesInLayer = 0;
        foreach (var layer in grid)
        {
            if (layer.Count > maxNodesInLayer)
            {
                maxNodesInLayer = layer.Count;
            }
        }

        float height = maxNodesInLayer * _verticalSpacing + _padding * 2;

        _contentRect.sizeDelta = new Vector2(width, height);
    }

    private void CreateNodeUIs(List<List<MapNode>> grid, MapNode currentNode)
    {
        foreach (var layer in grid)
        {
            int totalNodesInLayer = layer.Count;

            for (int i = 0; i < layer.Count; i++)
            {
                MapNode node = layer[i];
                Vector2 position = CalculateNodePosition(node.Layer, i, totalNodesInLayer);

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

    private Vector2 CalculateNodePosition(int layer, int indexInLayer, int totalNodesInLayer)
    {
        // 중앙 기준 계산
        float totalWidth = (_currentGrid.Count - 1) * _horizontalSpacing;
        float startX = -totalWidth / 2f;
        float x = startX + (layer * _horizontalSpacing);
        
        float totalHeight = (totalNodesInLayer - 1) * _verticalSpacing;
        float startY = totalHeight / 2f;
        float y = startY - (indexInLayer * _verticalSpacing);
        
        return new Vector2(x, y);
    }

    private void DrawConnections()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.DrawLines(_nodePositions, _currentGrid);
        }
    }

    private void ScrollToCurrentNode(MapNode currentNode)
    {
        if (currentNode == null || !_nodePositions.ContainsKey(currentNode)) return;

        // 일단 맨 왼쪽으로 스크롤 (0 = 맨 왼쪽)
        _scrollRect.horizontalNormalizedPosition = 0f;
    }

    public void HighlightAvailableNodes(List<MapNode> availableNodes)
    {
        foreach (var kvp in _nodeUIMap)
        {
            bool isAvailable = availableNodes.Contains(kvp.Key);
            kvp.Value.SetHighlight(isAvailable);
        }
    }

    public void OnNodeClicked(MapNode node)
    {
        Debug.Log($"MapUI: 노드 클릭됨 - {node}");

        if (node.State != NodeState.Available) return;

        // 이벤트 발생
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
            _lineRenderer.DrawLines(new Dictionary<MapNode, RectTransform>(), new List<List<MapNode>>());
        }
    }

    private void OnDestroy()
    {
        ClearMap();
    }
}
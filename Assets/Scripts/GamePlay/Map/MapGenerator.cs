using System.Collections.Generic;
using UnityEngine;
using Common;
using Common.Model;

public class MapGenerator
{
    private MapGenerationConfig _config;
    private int _nodeIdCounter = 0;

    public MapGenerator(MapGenerationConfig config)
    {
        _config = config;
    }

    public List<List<MapNode>> GenerateMap()
    {
        _nodeIdCounter = 0;

        List<List<MapNode>> grid = CreateGrid();
        HashSet<MapNode> usedNodes = GeneratePaths(grid);
        RemoveOrphanNodes(grid, usedNodes);

        return grid;
    }

    private List<List<MapNode>> CreateGrid()
    {
        List<List<MapNode>> grid = new List<List<MapNode>>();

        for (int layer = 0; layer < _config.LayerCount; layer++)
        {
            List<MapNode> currentLayer = new List<MapNode>();

            if (layer == 0)
            {
                MapNode startNode = CreateNode(layer, 0, RoomType.Start);
                currentLayer.Add(startNode);
            }
            else if (layer == _config.LayerCount - 1)
            {
                MapNode bossNode = CreateNode(layer, 0, RoomType.Boss);
                currentLayer.Add(bossNode);
            }
            else
            {
                var constraint = _config.GetConstraintForLayer(layer);
                
                // 필수 타입이 있으면 전체 층을 그 타입으로
                if (constraint != null && constraint.HasRequiredType)
                {
                    Debug.Log($"Layer {layer} 전체를 {constraint.RequiredType}로 생성");
                    
                    for (int index = 0; index < _config.NodesPerLayer; index++)
                    {
                        MapNode node = CreateNode(layer, index, constraint.RequiredType);
                        currentLayer.Add(node);
                    }
                }
                else
                {
                    // 필수 타입 없으면 확률 기반 랜덤
                    for (int index = 0; index < _config.NodesPerLayer; index++)
                    {
                        RoomType type = _config.RollRoomTypeWithConstraint(layer);
                        MapNode node = CreateNode(layer, index, type);
                        currentLayer.Add(node);
                    }
                }
            }

            grid.Add(currentLayer);
        }

        Debug.Log($"그리드 생성 완료: {grid.Count}층, 총 {_nodeIdCounter}개 노드");
        return grid;
    }

    private HashSet<MapNode> GeneratePaths(List<List<MapNode>> grid)
    {
        HashSet<MapNode> usedNodes = new HashSet<MapNode>();

        for (int i = 0; i < _config.PathCount; i++)
        {
            List<MapNode> path = GenerateSinglePath(grid);
            
            foreach (var node in path)
            {
                usedNodes.Add(node);
            }

            ConnectPath(path);

            Debug.Log($"경로 {i + 1} 생성: {path.Count}개 노드");
        }

        Debug.Log($"총 {usedNodes.Count}개 노드 사용됨 (전체 {_nodeIdCounter}개 중)");
        return usedNodes;
    }

    private List<MapNode> GenerateSinglePath(List<List<MapNode>> grid)
    {
        List<MapNode> path = new List<MapNode>();

        MapNode current = grid[0][0];
        path.Add(current);

        for (int layer = 1; layer < grid.Count; layer++)
        {
            List<MapNode> nextLayer = grid[layer];
            List<MapNode> adjacentNodes = GetAdjacentNodes(current, nextLayer);

            if (adjacentNodes.Count == 0)
            {
                Debug.LogError($"경로 생성 실패: Layer {layer}에 인접 노드 없음");
                break;
            }

            int randomIndex = Random.Range(0, adjacentNodes.Count);
            current = adjacentNodes[randomIndex];
            path.Add(current);
        }

        return path;
    }

    private void ConnectPath(List<MapNode> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            MapNode from = path[i];
            MapNode to = path[i + 1];

            if (!from.NextNodes.Contains(to))
            {
                from.NextNodes.Add(to);
            }
        }
    }

    private MapNode CreateNode(int layer, int indexInLayer, RoomType type)
    {
        GameObject prefab = _config.GetPrefabForType(type);
        MapNode node = new MapNode(_nodeIdCounter++, layer, indexInLayer, type, prefab);
        return node;
    }

    public bool IsAdjacent(MapNode from, MapNode to)
    {
        if (to.Layer != from.Layer + 1)
        {
            return false;
        }

        if (from.Type == RoomType.Start)
        {
            return true;
        }

        if (to.Type == RoomType.Boss)
        {
            return true;
        }

        int indexDiff = Mathf.Abs(to.IndexInLayer - from.IndexInLayer);
        return indexDiff <= 1;
    }

    public List<MapNode> GetAdjacentNodes(MapNode from, List<MapNode> nextLayer)
    {
        List<MapNode> adjacentNodes = new List<MapNode>();

        foreach (var node in nextLayer)
        {
            if (IsAdjacent(from, node))
            {
                adjacentNodes.Add(node);
            }
        }

        return adjacentNodes;
    }

    private void RemoveOrphanNodes(List<List<MapNode>> grid, HashSet<MapNode> usedNodes)
    {
        int removedCount = 0;

        foreach (var layer in grid)
        {
            int removed = layer.RemoveAll(node => !usedNodes.Contains(node));
            removedCount += removed;
        }

        Debug.Log($"고아 노드 {removedCount}개 제거 완료");
    }

    #region Debug

    public void PrintGrid(List<List<MapNode>> grid)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== 그리드 구조 ===");

        int totalNodes = 0;
        for (int i = 0; i < grid.Count; i++)
        {
            sb.Append($"Layer {i}: ");
            foreach (var node in grid[i])
            {
                sb.Append($"[{node.Type}] ");
            }
            sb.AppendLine($"({grid[i].Count}개)");
            totalNodes += grid[i].Count;
        }

        sb.AppendLine($"총 {totalNodes}개 노드");
        Debug.Log(sb.ToString());
    }

    public void PrintConnections(List<List<MapNode>> grid)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== 노드 연결 구조 ===");

        int totalConnections = 0;

        foreach (var layer in grid)
        {
            foreach (var node in layer)
            {
                if (node.NextNodes.Count > 0)
                {
                    sb.Append($"{node} → ");
                    foreach (var next in node.NextNodes)
                    {
                        sb.Append($"{next} ");
                    }
                    sb.AppendLine();
                    totalConnections += node.NextNodes.Count;
                }
            }
        }

        sb.AppendLine($"총 {totalConnections}개 연결");
        Debug.Log(sb.ToString());
    }

    #endregion
}
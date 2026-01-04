using System.Collections.Generic;
using UnityEngine;
using Common;
using Common.Model;

/// <summary>
/// 1. 노드 그리드 생성
/// 2. n 번 시작 부터 보스까지 올라가면서 경로 만듬
/// 3. 고아 노드 삭제
/// </summary>
public class MapGenerator
{
    private MapGenerationConfig _config;
    private int _nodeIdCounter = 0;

    public MapGenerator(MapGenerationConfig config)
    {
        _config = config;
    }

    // 전체 맵 생성
    public List<List<MapNode>> GenerateMap()
    {
        _nodeIdCounter = 0;

        // 그리드 생성
        List<List<MapNode>> grid = CreateGrid();

        // 경로 생성
        HashSet<MapNode> usedNodes = GeneratePaths(grid);

        // 고아 노드 제거
        RemoveOrphanNodes(grid, usedNodes);

        return grid;
    }

    // 그리드 생성 LayerCount × NodesPerLayer
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
                // 제약 조건 확인
                var constraint = _config.GetConstraintForLayer(layer);
                
                // 필수 타입이 있으면 전체 층을 그 타입으로
                if (constraint?.RequiredType != null)
                {
                    for (int index = 0; index < _config.NodesPerLayer; index++)
                    {
                        MapNode node = CreateNode(layer, index, constraint.RequiredType.Value);
                        currentLayer.Add(node);
                    }
                }
                else
                {
                    // 필수 타입 없으면 확률내 랜덤
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

    // 랜덤 경로 생성
    private HashSet<MapNode> GeneratePaths(List<List<MapNode>> grid)
    {
        HashSet<MapNode> usedNodes = new HashSet<MapNode>();

        for (int i = 0; i < _config.PathCount; i++)
        {
            List<MapNode> path = GenerateSinglePath(grid);
            
            // 경로의 모든 노드를 사용된 노드로 마킹
            foreach (var node in path)
            {
                usedNodes.Add(node);
            }

            // 경로 연결 (NextNodes 설정)
            ConnectPath(path);

            Debug.Log($"경로 {i + 1} 생성: {path.Count}개 노드");
        }

        Debug.Log($"총 {usedNodes.Count}개 노드 사용됨 (전체 {_nodeIdCounter}개 중)");
        return usedNodes;
    }

    // 단일 경로 생성 (Start → Boss)
    private List<MapNode> GenerateSinglePath(List<List<MapNode>> grid)
    {
        List<MapNode> path = new List<MapNode>();

        // Start 노드
        MapNode current = grid[0][0];
        path.Add(current);

        // 중간 층들 순회
        for (int layer = 1; layer < grid.Count; layer++)
        {
            List<MapNode> nextLayer = grid[layer];
            
            // 현재 노드와 인접한 노드들 찾기
            List<MapNode> adjacentNodes = GetAdjacentNodes(current, nextLayer);

            if (adjacentNodes.Count == 0)
            {
                Debug.LogError($"경로 생성 실패: Layer {layer}에 인접 노드 없음!");
                break;
            }

            // 인접 노드 중 랜덤 선택
            int randomIndex = Random.Range(0, adjacentNodes.Count);
            current = adjacentNodes[randomIndex];
            path.Add(current);
        }

        return path;
    }

    // 경로 연결 (NextNodes 설정)
    private void ConnectPath(List<MapNode> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            MapNode from = path[i];
            MapNode to = path[i + 1];

            // 이미 연결되어 있지 않으면 추가
            if (!from.NextNodes.Contains(to))
            {
                from.NextNodes.Add(to);
            }
        }
    }

    // 노드 생성
    private MapNode CreateNode(int layer, int indexInLayer, RoomType type)
    {
        GameObject prefab = _config.GetPrefabForType(type);
        MapNode node = new MapNode(_nodeIdCounter++, layer, indexInLayer, type, prefab);
        return node;
    }

    // 인접 노드 판단
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

    // 인접 노드들 반환
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

    //고아 노드 삭제
    private void RemoveOrphanNodes(List<List<MapNode>> grid, HashSet<MapNode> usedNodes)
    {
        int removedCount = 0;

        foreach (var layer in grid)
        {
            // RemoveAll: 조건에 맞는 요소 제거
            int removed = layer.RemoveAll(node => !usedNodes.Contains(node));
            removedCount += removed;
        }

        Debug.Log($"고아 노드 {removedCount}개 제거 완료");
    }

    #region Debug

    // 그리드 정보 출력
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

    // 연결 정보 출력
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
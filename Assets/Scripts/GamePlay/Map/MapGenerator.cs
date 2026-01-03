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

    // 전체 맵 생성 (Phase 3에서 완성)
    public List<List<MapNode>> GenerateMap()
    {
        _nodeIdCounter = 0;

        // Phase 2: 그리드 생성
        List<List<MapNode>> grid = CreateGrid();

        // Phase 3: 경로 생성 (TODO)
        // Phase 4: 고아 노드 제거 (TODO)

        return grid;
    }

    // 그리드 생성: LayerCount × NodesPerLayer
    private List<List<MapNode>> CreateGrid()
    {
        List<List<MapNode>> grid = new List<List<MapNode>>();

        for (int layer = 0; layer < _config.LayerCount; layer++)
        {
            List<MapNode> currentLayer = new List<MapNode>();

            // 첫 층 (Start)
            if (layer == 0)
            {
                MapNode startNode = CreateNode(layer, 0, RoomType.Start);
                currentLayer.Add(startNode);
            }
            // 마지막 층 (Boss)
            else if (layer == _config.LayerCount - 1)
            {
                MapNode bossNode = CreateNode(layer, 0, RoomType.Boss);
                currentLayer.Add(bossNode);
            }
            // 중간 층 (일반 방들)
            else
            {
                for (int index = 0; index < _config.NodesPerLayer; index++)
                {
                    // 제약 조건 고려해서 방 타입 결정
                    RoomType type = _config.RollRoomTypeWithConstraint(layer);
                    MapNode node = CreateNode(layer, index, type);
                    currentLayer.Add(node);
                }
            }

            grid.Add(currentLayer);
        }

        Debug.Log($"그리드 생성 완료: {grid.Count}층, 총 {_nodeIdCounter}개 노드");
        return grid;
    }

    // 노드 생성
    private MapNode CreateNode(int layer, int indexInLayer, RoomType type)
    {
        GameObject prefab = _config.GetPrefabForType(type);
        MapNode node = new MapNode(_nodeIdCounter++, layer, indexInLayer, type, prefab);
        return node;
    }

    // 인접 노드 판단 (바로 위 층의 같은 위치 또는 ±1)
    public bool IsAdjacent(MapNode from, MapNode to)
    {
        // 다음 층이 아니면 인접 아님
        if (to.Layer != from.Layer + 1)
        {
            return false;
        }

        // Start 노드는 다음 층 모든 노드와 인접
        if (from.Type == RoomType.Start)
        {
            return true;
        }

        // 이전 층 모든 노드는 Boss와 인접
        if (to.Type == RoomType.Boss)
        {
            return true;
        }

        // 일반 노드: 같은 인덱스 또는 ±1 인덱스만 인접
        int indexDiff = Mathf.Abs(to.IndexInLayer - from.IndexInLayer);
        return indexDiff <= 1;
    }

    // 특정 노드의 다음 층 인접 노드들 반환
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

    #region Debug

    // 그리드 정보 출력
    public void PrintGrid(List<List<MapNode>> grid)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== 그리드 구조 ===");

        for (int i = 0; i < grid.Count; i++)
        {
            sb.Append($"Layer {i}: ");
            foreach (var node in grid[i])
            {
                sb.Append($"[{node.Type}] ");
            }
            sb.AppendLine($"({grid[i].Count}개)");
        }

        Debug.Log(sb.ToString());
    }

    #endregion
}
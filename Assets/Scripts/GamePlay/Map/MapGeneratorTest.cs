using UnityEngine;
using System.Collections.Generic;

public class MapGeneratorTest : MonoBehaviour
{
    [SerializeField] private MapGenerationConfig _config;

    void Start()
    {
        if (_config == null)
        {
            Debug.LogError("Config가 할당되지 않았습니다!");
            return;
        }

        TestPathGeneration();
    }

    // ⭐ Phase 3 테스트
    void TestPathGeneration()
    {
        Debug.Log("=== Phase 3 테스트: 경로 생성 ===");

        MapGenerator generator = new MapGenerator(_config);
        List<List<MapNode>> grid = generator.GenerateMap();

        // 그리드 출력
        generator.PrintGrid(grid);

        // 연결 정보 출력
        generator.PrintConnections(grid);

        // 경로 검증
        ValidatePaths(grid);
    }

    // 경로 검증 (Start → Boss 도달 가능 확인)
    void ValidatePaths(List<List<MapNode>> grid)
    {
        Debug.Log("=== 경로 검증 ===");

        MapNode start = grid[0][0];
        MapNode boss = grid[grid.Count - 1][0];

        // DFS로 Start → Boss 경로 존재 확인
        HashSet<MapNode> visited = new HashSet<MapNode>();
        bool canReachBoss = DFS(start, boss, visited);

        if (canReachBoss)
        {
            Debug.Log("✅ Start → Boss 경로 존재!");
        }
        else
        {
            Debug.LogError("❌ Start → Boss 경로 없음!");
        }
    }

    // DFS로 경로 존재 확인
    bool DFS(MapNode current, MapNode target, HashSet<MapNode> visited)
    {
        if (current == target)
        {
            return true;
        }

        visited.Add(current);

        foreach (var next in current.NextNodes)
        {
            if (!visited.Contains(next))
            {
                if (DFS(next, target, visited))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
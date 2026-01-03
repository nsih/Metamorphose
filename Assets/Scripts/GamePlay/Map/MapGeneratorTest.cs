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

        TestMapGeneration(); // ⭐ 이름 변경
    }

    // ⭐ Phase 4 테스트
    void TestMapGeneration()
    {
        Debug.Log("=== Phase 1~4 테스트: 완전한 맵 생성 ===");

        MapGenerator generator = new MapGenerator(_config);
        List<List<MapNode>> finalMap = generator.GenerateMap();

        // 최종 맵 출력
        generator.PrintGrid(finalMap);
        generator.PrintConnections(finalMap);

        // 경로 검증
        ValidatePaths(finalMap);
    }

    // 경로 검증
    void ValidatePaths(List<List<MapNode>> grid)
    {
        Debug.Log("=== 경로 검증 ===");

        MapNode start = grid[0][0];
        MapNode boss = grid[grid.Count - 1][0];

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
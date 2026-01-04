using UnityEngine;
using System.Collections.Generic;

public class MapGeneratorTest : MonoBehaviour
{
    [SerializeField] private MapGenerationConfig _config;

    void Start()
    {
        if (_config == null)
        {
            Debug.LogError("Config가 할당되지 않았습니다");
            return;
        }

        TestMapGeneration();
    }

    // Phase 5 테스트
    void TestMapGeneration()
    {
        Debug.Log("Phase 1-5 테스트: 제약 조건 적용 맵 생성");

        MapGenerator generator = new MapGenerator(_config);
        List<List<MapNode>> finalMap = generator.GenerateMap();

        // 최종 맵 출력
        generator.PrintGrid(finalMap);
        
        // 제약 조건 검증
        ValidateConstraints(finalMap);

        // 연결 정보
        generator.PrintConnections(finalMap);

        // 경로 검증
        ValidatePaths(finalMap);
    }

    void ValidatePaths(List<List<MapNode>> grid)
    {
        Debug.Log("경로 검증");

        MapNode start = grid[0][0];
        MapNode boss = grid[grid.Count - 1][0];

        HashSet<MapNode> visited = new HashSet<MapNode>();
        bool canReachBoss = DFS(start, boss, visited);

        if (canReachBoss)
        {
            Debug.Log("Start -> Boss 경로 존재");
        }
        else
        {
            Debug.LogError("Start -> Boss 경로 없음");
        }
    }

    // 제약 조건 검증
    void ValidateConstraints(List<List<MapNode>> grid)
    {
        Debug.Log("제약 조건 검증");

        if (_config.Constraints.Count == 0)
        {
            Debug.Log("제약 조건 없음");
            return;
        }

        bool allSatisfied = true;

        foreach (var constraint in _config.Constraints)
        {
            if (constraint.Layer >= grid.Count || constraint.Layer < 0)
            {
                continue;
            }

            var layer = grid[constraint.Layer];

            // 필수 타입 검증
            if (constraint.RequiredType.HasValue)
            {
                bool allMatch = true;
                int count = 0;
                
                foreach (var node in layer)
                {
                    if (node.Type == constraint.RequiredType.Value)
                    {
                        count++;
                    }
                    else
                    {
                        allMatch = false;
                    }
                }

                if (allMatch && count > 0)
                {
                    Debug.Log($"Layer {constraint.Layer}: 전체 {count}개 노드가 {constraint.RequiredType.Value}");
                }
                else if (count > 0)
                {
                    Debug.LogWarning($"Layer {constraint.Layer}: {count}개만 {constraint.RequiredType.Value} (전체 {layer.Count}개 중)");
                }
                else
                {
                    Debug.LogError($"Layer {constraint.Layer}: {constraint.RequiredType.Value} 없음");
                    allSatisfied = false;
                }
            }

            // 금지 타입 검증
            if (constraint.BannedTypes != null && constraint.BannedTypes.Count > 0)
            {
                bool foundBanned = false;
                
                foreach (var node in layer)
                {
                    if (constraint.BannedTypes.Contains(node.Type))
                    {
                        Debug.LogWarning($"Layer {constraint.Layer}: 금지된 {node.Type} 발견");
                        allSatisfied = false;
                        foundBanned = true;
                    }
                }
                
                if (!foundBanned)
                {
                    Debug.Log($"Layer {constraint.Layer}: 금지 타입 없음");
                }
            }
        }

        if (allSatisfied)
        {
            Debug.Log("모든 제약 조건 만족");
        }
        else
        {
            Debug.LogWarning("일부 제약 조건 위반");
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
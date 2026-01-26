using UnityEngine;
using System.Collections.Generic;
using Common;

public class MapGeneratorTest : MonoBehaviour
{
    [SerializeField] private MapGenerationConfig _config;

    void Start()
    {
        if (_config == null)
        {
            Debug.LogError("Config null");
            return;
        }

        TestMapGeneration();
    }

    void TestMapGeneration()
    {
        MapGenerator generator = new MapGenerator(_config);
        Map map = generator.GenerateMap();

        ValidateConstraints(map);
        ValidatePaths(map);
    }

    void ValidatePaths(Map map)
    {
        MapNode start = map.StartNode;
        MapNode boss = map.BossNode;

        HashSet<MapNode> visited = new HashSet<MapNode>();
        bool canReachBoss = DFS(map,start, boss, visited);

        if (canReachBoss)
        {
            Debug.Log("Can reach boss");
        }
        else
        {
            Debug.Log("Cannot reach boss");
        }
    }

    void ValidateConstraints(Map map)
    {
        if (_config.Constraints.Count == 0)
        {   
            Debug.Log("제약 조건 없음");
            return;
        }

        bool allSatisfied = true;

        foreach (var constraint in _config.Constraints)
        {
            if (constraint.Layer >= map.LayerCount || constraint.Layer < 0)
            {
                continue;
            }

            var layer = map.GetNodesInLayer(constraint.Layer);

            // 필수 타입 검증
            if (constraint.HasRequiredType)
            {
                bool allMatch = true;
                int count = 0;
                
                foreach (var node in layer)
                {
                    if (node.Type == constraint.RequiredType)
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
                    //Debug.Log($"Layer {constraint.Layer}: 전체 {count}개 노드가 {constraint.RequiredType}");
                }
                else if (count > 0)
                {
                    //Debug.LogWarning($"Layer {constraint.Layer}: {count}개만 {constraint.RequiredType} (전체 {layer.Count}개 중)");
                }
                else
                {
                    //Debug.LogError($"Layer {constraint.Layer}: {constraint.RequiredType} 없음");
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
                        //Debug.LogWarning($"Layer {constraint.Layer}: 금지된 {node.Type} 발견");
                        allSatisfied = false;
                        foundBanned = true;
                    }
                }
                
                if (!foundBanned)
                {
                    //Debug.Log($"Layer {constraint.Layer}: 금지 타입 없음");
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

    bool DFS(Map map, MapNode current, MapNode target, HashSet<MapNode> visited)
    {
        if (current == target)
        {
            return true;
        }

        visited.Add(current);

        foreach (var nextId in current.NextNodeIds)
        {
            MapNode next = map.GetNode(nextId);
            if (!visited.Contains(next))
            {
                if (DFS(map, next, target, visited))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
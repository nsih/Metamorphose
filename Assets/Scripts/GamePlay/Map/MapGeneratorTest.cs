using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// MapGenerator 테스트용 (Phase 2 완료 후 삭제)
/// </summary>
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

        TestGridGeneration();
    }

    void TestGridGeneration()
    {
        Debug.Log("=== Phase 2 테스트: 그리드 생성 ===");

        MapGenerator generator = new MapGenerator(_config);
        List<List<MapNode>> grid = generator.GenerateMap();

        // 그리드 출력
        generator.PrintGrid(grid);

        // 인접 노드 테스트
        TestAdjacency(generator, grid);
    }

    void TestAdjacency(MapGenerator generator, List<List<MapNode>> grid)
    {
        Debug.Log("=== 인접 노드 테스트 ===");

        // Layer 1의 첫 번째 노드 선택
        if (grid.Count > 1 && grid[1].Count > 0)
        {
            MapNode testNode = grid[1][0];
            Debug.Log($"테스트 노드: {testNode}");

            // 다음 층 인접 노드 찾기
            if (grid.Count > 2)
            {
                List<MapNode> adjacentNodes = generator.GetAdjacentNodes(testNode, grid[2]);
                Debug.Log($"인접 노드 {adjacentNodes.Count}개:");
                foreach (var node in adjacentNodes)
                {
                    Debug.Log($"  - {node}");
                }
            }
        }
    }
}

/*

//테스트 컨피그

그리드 생성 완료: 15층, 총 67개 노드
UnityEngine.Debug:Log (object)
MapGenerator:CreateGrid () (at Assets/Scripts/GamePlay/Map/MapGenerator.cs:71)
MapGenerator:GenerateMap () (at Assets/Scripts/GamePlay/Map/MapGenerator.cs:27)
MapGeneratorTest:TestGridGeneration () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:27)
MapGeneratorTest:Start () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:19)

=== 그리드 구조 ===
Layer 0: [Start] (1개)
Layer 1: [Event] [Battle] [Elite] [Event] [Event] (5개)
Layer 2: [Shop] [Elite] [Battle] [Battle] [Battle] (5개)
Layer 3: [Shop] [Battle] [Shop] [Battle] [Event] (5개)
Layer 4: [Battle] [Shop] [Elite] [Battle] [Event] (5개)
Layer 5: [Elite] [Elite] [Shop] [Battle] [Battle] (5개)
Layer 6: [Elite] [Shop] [Elite] [Shop] [Shop] (5개)
Layer 7: [Shop] [Battle] [Elite] [Event] [Shop] (5개)
Layer 8: [Elite] [Shop] [Shop] [Elite] [Elite] (5개)
Layer 9: [Shop] [Battle] [Battle] [Battle] [Battle] (5개)
Layer 10: [Elite] [Battle] [Shop] [Event] [Elite] (5개)
Layer 11: [Battle] [Elite] [Battle] [Event] [Battle] (5개)
Layer 12: [Battle] [Elite] [Battle] [Battle] [Event] (5개)
Layer 13: [Battle] [Event] [Battle] [Elite] [Shop] (5개)
Layer 14: [Boss] (1개)

UnityEngine.Debug:Log (object)
MapGenerator:PrintGrid (System.Collections.Generic.List`1<System.Collections.Generic.List`1<MapNode>>) (at Assets/Scripts/GamePlay/Map/MapGenerator.cs:143)
MapGeneratorTest:TestGridGeneration () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:30)
MapGeneratorTest:Start () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:19)

=== 인접 노드 테스트 ===
UnityEngine.Debug:Log (object)
MapGeneratorTest:TestAdjacency (MapGenerator,System.Collections.Generic.List`1<System.Collections.Generic.List`1<MapNode>>) (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:38)
MapGeneratorTest:TestGridGeneration () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:33)
MapGeneratorTest:Start () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:19)

테스트 노드: [L1-0] Event (ID:1)
UnityEngine.Debug:Log (object)
MapGeneratorTest:TestAdjacency (MapGenerator,System.Collections.Generic.List`1<System.Collections.Generic.List`1<MapNode>>) (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:44)
MapGeneratorTest:TestGridGeneration () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:33)
MapGeneratorTest:Start () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:19)

인접 노드 2개:
UnityEngine.Debug:Log (object)
MapGeneratorTest:TestAdjacency (MapGenerator,System.Collections.Generic.List`1<System.Collections.Generic.List`1<MapNode>>) (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:50)
MapGeneratorTest:TestGridGeneration () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:33)
MapGeneratorTest:Start () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:19)

  - [L2-0] Shop (ID:6)
UnityEngine.Debug:Log (object)
MapGeneratorTest:TestAdjacency (MapGenerator,System.Collections.Generic.List`1<System.Collections.Generic.List`1<MapNode>>) (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:53)
MapGeneratorTest:TestGridGeneration () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:33)
MapGeneratorTest:Start () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:19)

  - [L2-1] Elite (ID:7)
UnityEngine.Debug:Log (object)
MapGeneratorTest:TestAdjacency (MapGenerator,System.Collections.Generic.List`1<System.Collections.Generic.List`1<MapNode>>) (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:53)
MapGeneratorTest:TestGridGeneration () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:33)
MapGeneratorTest:Start () (at Assets/Scripts/GamePlay/Map/MapGeneratorTest.cs:19)

*/
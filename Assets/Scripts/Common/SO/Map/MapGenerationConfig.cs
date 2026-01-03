using UnityEngine;
using System.Collections.Generic;
using Common;

[CreateAssetMenu(fileName = "MapGenerationConfig", menuName = "SO/Map/Generation Config")]
public class MapGenerationConfig : ScriptableObject
{
    [Header("Grid Structure")]
    [Range(5, 30)]
    public int LayerCount = 15;

    [Range(3, 10)]
    public int NodesPerLayer = 5;

    [Header("Path Generation")]
    [Range(3, 20)]
    public int PathCount = 8;

    [Header("Room Type Probabilities")]
    public List<RoomTypeChance> RoomTypeChances = new List<RoomTypeChance>
    {
        new RoomTypeChance(RoomType.Battle, 0.50f),
        new RoomTypeChance(RoomType.Elite, 0.15f),
        new RoomTypeChance(RoomType.Shop, 0.20f),
        new RoomTypeChance(RoomType.Event, 0.15f)
    };

    [Header("Room Prefabs")]
    public List<RoomPrefabMapping> RoomPrefabs = new List<RoomPrefabMapping>();

    [Header("Path Constraints")]
    public List<PathConstraint> Constraints = new List<PathConstraint>();



    // 특정 계층에 적용되는 제약 조건 반환
    public PathConstraint GetConstraintForLayer(int layer)
    {
        return Constraints.Find(c => c.Layer == layer);
    }



    // 방 타입에 해당하는 프리팹 반환 (없으면 첫 번째 프리팹)
    public GameObject GetPrefabForType(RoomType type)
    {
        var mapping = RoomPrefabs.Find(m => m.Type == type);
        
        if (mapping != null && mapping.Prefab != null)
        {
            return mapping.Prefab;
        }

        Debug.LogWarning($"MapGenerationConfig: {type} is null");
        
        if (RoomPrefabs.Count > 0 && RoomPrefabs[0].Prefab != null)
        {
            return RoomPrefabs[0].Prefab;
        }

        return null;
    }


    // 확률 기반으로 방 타입 랜덤 선택 (대부분 노드)
    public RoomType RollRoomType()
    {
        if (RoomTypeChances.Count == 0)
        {
            Debug.LogWarning("MapGenerationConfig: RoomTypeChances empty");
            return RoomType.Battle;
        }

        float roll = Random.value;
        float cumulative = 0f;

        foreach (var chance in RoomTypeChances)
        {
            cumulative += chance.Probability;
            if (roll <= cumulative)
            {
                return chance.Type;
            }
        }

        return RoomTypeChances[0].Type;
    }


    // 특정 타입을 제외하고 방 타입 랜덤 선택
    public RoomType RollRoomType(List<RoomType> excludeTypes)
    {
        if (excludeTypes == null || excludeTypes.Count == 0)
        {
            return RollRoomType();
        }

        var validChances = new List<RoomTypeChance>();
        foreach (var chance in RoomTypeChances)
        {
            if (!excludeTypes.Contains(chance.Type))
            {
                validChances.Add(chance);
            }
        }
        
        if (validChances.Count == 0)
        {
            Debug.LogWarning("MapGenerationConfig: 모든 타입이 금지");
            return RoomType.Battle;
        }

        float total = 0f;
        foreach (var chance in validChances)
        {
            total += chance.Probability;
        }

        if (total <= 0f)
        {
            return validChances[0].Type;
        }

        float roll = Random.value * total;
        float cumulative = 0f;

        foreach (var chance in validChances)
        {
            cumulative += chance.Probability;
            if (roll <= cumulative)
            {
                return chance.Type;
            }
        }

        return validChances[0].Type;
    }

    // 제약 조건을 고려하여 방 타입 결정 (필수 타입 > 금지 타입 > 일반)
    public RoomType RollRoomTypeWithConstraint(int layer)
    {
        var constraint = GetConstraintForLayer(layer);
        
        if (constraint?.RequiredType != null)
        {
            return constraint.RequiredType.Value;
        }

        if (constraint?.BannedTypes != null && constraint.BannedTypes.Count > 0)
        {
            return RollRoomType(constraint.BannedTypes);
        }

        return RollRoomType();
    }

    #if UNITY_EDITOR
    // 확률 합계 검증
    [ContextMenu("Validate Probabilities")]
    private void ValidateProbabilities()
    {
        float total = 0f;
        foreach (var chance in RoomTypeChances)
        {
            total += chance.Probability;
        }

        if (Mathf.Abs(total - 1f) > 0.01f)
        {
            Debug.LogWarning($"확률 합계: {total:F2}");
        }
        else
        {
            Debug.Log("valid chances");
        }
    }
    
    // 현재 설정 정보 출력
    [ContextMenu("Print Configuration")]
    private void PrintConfiguration()
    {
        Debug.Log($"Grid: {LayerCount}층 × {NodesPerLayer}노드\n" +
                  $"경로: {PathCount}개");
    }
    #endif
}
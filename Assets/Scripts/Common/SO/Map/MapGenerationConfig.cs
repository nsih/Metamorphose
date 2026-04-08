using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Common;

[CreateAssetMenu(fileName = "MapGenerationConfig", menuName = "SO/Map/Generation Config")]
public class MapGenerationConfig : ScriptableObject
{
    [Header("Grid Structure")]
    [Range(3, 30)]
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
    [Tooltip("같은 타입의 방 여러 개 등록 가능 (랜덤 선택됨)")]
    public List<RoomPrefabMapping> RoomPrefabs = new List<RoomPrefabMapping>();

    [Header("Path Constraints")]
    public List<PathConstraint> Constraints = new List<PathConstraint>();

    public PathConstraint GetConstraintForLayer(int layer)
    {
        foreach (var c in Constraints)
        {
            if (c.Layer == layer)
            {
                return c;
            }
        }
        
        return null;
    }

    // 같은 타입의 프리팹 중 랜덤 선택
    public GameObject GetPrefabForType(RoomType type)
    {
        var matchingPrefabs = RoomPrefabs
            .Where(m => m.Type == type && m.Prefab != null)
            .ToList();
        
        if (matchingPrefabs.Count == 0)
        {
            Debug.LogWarning($"MapGenerationConfig: {type} 타입의 프리팹이 없습니다");
            
            if (RoomPrefabs.Count > 0 && RoomPrefabs[0].Prefab != null)
            {
                return RoomPrefabs[0].Prefab;
            }
            
            return null;
        }

        int randomIndex = Random.Range(0, matchingPrefabs.Count);
        return matchingPrefabs[randomIndex].Prefab;
    }

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
            Debug.LogWarning("MapGenerationConfig: 모든 타입이 금지됨");
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

    public RoomType RollRoomTypeWithConstraint(int layer)
    {
        var constraint = GetConstraintForLayer(layer);
        
        if (constraint != null && constraint.HasRequiredType)
        {
            return constraint.RequiredType;
        }

        if (constraint?.BannedTypes != null && constraint.BannedTypes.Count > 0)
        {
            return RollRoomType(constraint.BannedTypes);
        }

        return RollRoomType();
    }

    #if UNITY_EDITOR
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
            Debug.LogWarning($"확률 합계: {total:F2} (100%가 아님)");
        }
        else
        {
            Debug.Log("확률 합계 정상: 100%");
        }
    }
    
    [ContextMenu("Print Configuration")]
    private void PrintConfiguration()
    {
        Debug.Log($"=== Map Generation Config ===\n" +
                  $"Grid: {LayerCount}층 × {NodesPerLayer}노드\n" +
                  $"경로: {PathCount}개\n" +
                  $"등록된 방 프리팹: {RoomPrefabs.Count}개");
        
        // 타입별 개수 출력
        var grouped = RoomPrefabs
            .Where(m => m.Prefab != null)
            .GroupBy(m => m.Type);
        
        foreach (var group in grouped)
        {
            Debug.Log($"  • {group.Key}: {group.Count()}개 변형");
        }
    }

    [ContextMenu("Validate Constraints")]
    private void ValidateConstraints()
    {
        if (Constraints.Count == 0)
        {
            Debug.Log("제약 조건 없음");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== 제약 조건 검증 ===");

        foreach (var constraint in Constraints)
        {
            sb.Append($"Layer {constraint.Layer}: ");

            if (constraint.HasRequiredType)
            {
                sb.Append($"필수=[{constraint.RequiredType}] ");
            }

            if (constraint.BannedTypes != null && constraint.BannedTypes.Count > 0)
            {
                sb.Append($"금지=[");
                foreach (var banned in constraint.BannedTypes)
                {
                    sb.Append($"{banned} ");
                }
                sb.Append("]");
            }

            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }
    #endif
}
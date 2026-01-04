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
        foreach (var c in Constraints)
        {
            if (c.Layer == layer)
            {
                return c;
            }
        }
        
        return null;
    }

    // 방 타입에 해당하는 프리팹 반환
    public GameObject GetPrefabForType(RoomType type)
    {
        var mapping = RoomPrefabs.Find(m => m.Type == type);
        
        if (mapping != null && mapping.Prefab != null)
        {
            return mapping.Prefab;
        }

        Debug.LogWarning($"MapGenerationConfig: {type} prefab null");
        
        if (RoomPrefabs.Count > 0 && RoomPrefabs[0].Prefab != null)
        {
            return RoomPrefabs[0].Prefab;
        }

        return null;
    }

    // 확률 기반으로 방 타입 랜덤 선택
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

    // 제약 조건을 고려하여 방 타입 결정
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
            Debug.LogWarning($"확률 합계: {total:F2}");
        }
        else
        {
            Debug.Log("확률 합계 정상: 100%");
        }
    }
    
    [ContextMenu("Print Configuration")]
    private void PrintConfiguration()
    {
        Debug.Log($"Grid: {LayerCount}층 x {NodesPerLayer}노드\n" +
                  $"경로: {PathCount}개");
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
                sb.Append("] ");
            }

            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }

    [ContextMenu("Add Example Constraints")]
    private void AddExampleConstraints()
    {
        Constraints.Clear();

        // 5층: 무조건 상점
        PathConstraint shop5 = new PathConstraint();
        shop5.Layer = 5;
        shop5.HasRequiredType = true;
        shop5.RequiredType = RoomType.Shop;
        shop5.BannedTypes = new List<RoomType>();
        Constraints.Add(shop5);

        // 10층: 무조건 상점
        PathConstraint shop10 = new PathConstraint();
        shop10.Layer = 10;
        shop10.HasRequiredType = true;
        shop10.RequiredType = RoomType.Shop;
        shop10.BannedTypes = new List<RoomType>();
        Constraints.Add(shop10);

        // 1~6층: 엘리트 금지
        for (int layer = 1; layer <= 6; layer++)
        {
            PathConstraint noElite = new PathConstraint();
            noElite.Layer = layer;
            noElite.HasRequiredType = false;
            noElite.BannedTypes = new List<RoomType> { RoomType.Elite };
            Constraints.Add(noElite);
        }

        UnityEditor.EditorUtility.SetDirty(this);

        Debug.Log($"예시 제약 조건 {Constraints.Count}개 추가됨");
    }
    #endif
}
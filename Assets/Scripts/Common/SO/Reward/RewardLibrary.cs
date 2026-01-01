using System.Collections.Generic;
using UnityEngine;
using Common;

[CreateAssetMenu(fileName = "RewardLibrary", menuName = "SO/Reward/Reward Library")]
public class RewardLibrary : ScriptableObject
{
    [Header("Reward Pool")]
    [Tooltip("일반 보상 리스트")]
    public List<RewardData> CommonRewards = new List<RewardData>();
    
    [Tooltip("희귀 보상 리스트")]
    public List<RewardData> RareRewards = new List<RewardData>();
    
    [Tooltip("영웅 보상 리스트")]
    public List<RewardData> EpicRewards = new List<RewardData>();
    

    [Header("Settings")]
    [Range(0f, 1f)]
    [Tooltip("일반 보상 확률")]
    public float CommonChance = 0.70f;
    
    [Range(0f, 1f)]
    [Tooltip("희귀 보상 확률")]
    public float RareChance = 0.25f;
    
    [Range(0f, 1f)]
    [Tooltip("영웅 보상 확률")]
    public float EpicChance = 0.05f;
    
    [Header("Duplicate Settings")]
    [Tooltip("중복 보상 허용 여부")]
    public bool AllowDuplicates = false;

    //n개 보상 가져오기
    public List<RewardData> GetRandomRewards(int count = 3)
    {
        List<RewardData> results = new List<RewardData>();
        List<RewardData> usedRewards = new List<RewardData>(); // 중복 방지용
        
        for (int i = 0; i < count; i++)
        {
            RewardRarity rarity = RollRarity();
            RewardData reward = GetRandomByRarity(rarity, usedRewards);
            
            if (reward != null)
            {
                results.Add(reward);
                if (!AllowDuplicates)
                {
                    usedRewards.Add(reward);
                }
            }
            else
            {
                Debug.LogWarning($"보상을 뽑을 수 없습니다. Rarity: {rarity}");
            }
        }
        
        return results;
    }

    // RollRarity
    private RewardRarity RollRarity()
    {
        float roll = Random.Range(0f, 1f);
        
        if (roll < EpicChance)
        {
            return RewardRarity.Epic;
        }
        else if (roll < EpicChance + RareChance)
        {
            return RewardRarity.Rare;
        }
        else
        {
            return RewardRarity.Common;
        }
    }


    /// 희귀도 풀에서 아이템 가져오기
    private RewardData GetRandomByRarity(RewardRarity rarity, List<RewardData> excludeList)
    {
        List<RewardData> pool = GetPoolByRarity(rarity);
        
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning($"{rarity} 보상 풀이 비어있습니다!");
            return null;
        }
        
        // 중복 제외된 풀 생성
        List<RewardData> availablePool = new List<RewardData>(pool);
        if (!AllowDuplicates && excludeList.Count > 0)
        {
            availablePool.RemoveAll(r => excludeList.Contains(r));
        }
        
        if (availablePool.Count == 0)
        {
            Debug.LogWarning($"{rarity} 보상 풀에서 선택 가능한 보상이 없습니다!");
            return null;
        }
        
        int randomIndex = Random.Range(0, availablePool.Count);
        return availablePool[randomIndex];
    }

    // 희귀도 풀
    private List<RewardData> GetPoolByRarity(RewardRarity rarity)
    {
        switch (rarity)
        {
            case RewardRarity.Common:
                return CommonRewards;
            case RewardRarity.Rare:
                return RareRewards;
            case RewardRarity.Epic:
                return EpicRewards;
            default:
                return CommonRewards;
        }
    }

    #region Editor Helper
    
    #if UNITY_EDITOR
    [ContextMenu("Validate Probabilities")]
    private void ValidateProbabilities()
    {
        float total = CommonChance + RareChance + EpicChance;
        if (Mathf.Abs(total - 1f) > 0.01f)
        {
            Debug.LogWarning($"total Chance Error: {total * 100}%");
        }
        else
        {
            Debug.Log($"total Chance : {total * 100}%");
        }
    }
    
    [ContextMenu("Print Pool Sizes")]
    private void PrintPoolSizes()
    {
        Debug.Log($"reward pool size - Common: {CommonRewards.Count}, Rare: {RareRewards.Count}, Epic: {EpicRewards.Count}");
    }
    #endif

    #endregion
}
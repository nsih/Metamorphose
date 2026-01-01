using UnityEngine;
using Common;
using System.Collections.Generic;

[System.Serializable]
public class RewardEffect
{
    public RewardType Type;
    public float Value;
}

[CreateAssetMenu(fileName = "Reward_", menuName = "SO/Reward/Reward Data")]
public class RewardData : ScriptableObject
{
    [Header("Reward Info")]
    public RewardRarity Rarity;
    
    [Header("Effects")]
    [Tooltip("이 보상이 적용하는 효과들 (여러 개 가능)")]
    public List<RewardEffect> Effects = new List<RewardEffect>();
    
    [Header("UI Display")]
    public Sprite Icon;
    public string DisplayName;
    
    [TextArea(3, 5)]
    public string Description;
}
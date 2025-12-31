using UnityEngine;
using Common;

[CreateAssetMenu(fileName = "Reward_", menuName = "SO/Reward/Reward Data")]
public class RewardData : ScriptableObject
{
    public RewardType Type;
    public RewardRarity Rarity;
    public float Value;

    // public bool IsStackable = true;        // 중복 획득 가능?
    // public int MaxStack = 5;               // 최대 스택 개수
    
    public Sprite Icon;
    public string DisplayName;
    
    [TextArea(3, 5)]
    public string Description;
}
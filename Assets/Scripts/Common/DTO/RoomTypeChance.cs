using UnityEngine;
using Common;

[System.Serializable]
public class RoomTypeChance
{
    public RoomType Type;
    
    [Range(0f, 1f)]
    public float Probability;

    public RoomTypeChance()
    {
        Type = RoomType.Battle;
        Probability = 0f;
    }

    public RoomTypeChance(RoomType type, float probability)
    {
        Type = type;
        Probability = Mathf.Clamp01(probability);
    }
}
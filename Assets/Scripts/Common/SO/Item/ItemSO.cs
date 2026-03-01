using System;
using Common;
using UnityEngine;

[Serializable]
public struct StatModifierEntry
{
    public string statName;
    public float value;
    public StatModType modType;
}

[CreateAssetMenu(menuName = "SO/Item/ItemSO", fileName = "item_")]
public class ItemSO : ScriptableObject
{
    [Header("identity")]
    public string itemId;
    public string itemName;
    public string nameKr;

    [TextArea(2, 4)]
    public string description;

    [Header("classification")]
    public ItemTier tier;
    public ItemType itemType;
    public bool isUnique;

    [Header("visual")]
    public Sprite sprite;
    public string spritePath;

    [Header("stats")]
    public StatModifierEntry[] statModifiers;

    [Header("on pickup")]
    public float onPickupHeal;

    [Header("active")]
    public float activeCooldown;

    [Header("tags")]
    public string[] tags;
}
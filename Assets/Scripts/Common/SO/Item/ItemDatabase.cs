using System;
using System.Collections.Generic;
using UnityEngine;
using Common;

[CreateAssetMenu(menuName = "SO/Item/ItemDatabase", fileName = "ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField]
    private ItemSO[] items = Array.Empty<ItemSO>();

    [Header("tier probability")]
    [Range(0f, 1f)] public float commonChance = 0.70f;
    [Range(0f, 1f)] public float rareChance = 0.25f;
    [Range(0f, 1f)] public float bossChance = 0.05f;

    public IReadOnlyList<ItemSO> Items => items;

    public void SetItems(ItemSO[] newItems)
    {
        items = newItems;
    }

    public List<ItemSO> GetRewardPool(AcquiredItemRegistry registry)
    {
        var pool = new List<ItemSO>();

        foreach (var item in items)
        {
            if (item == null) continue;
            if (item.isUnique && registry.IsAcquired(item.itemId)) continue;
            pool.Add(item);
        }

        return pool;
    }

    public ItemTier RollTier()
    {
        float roll = UnityEngine.Random.value;

        if (roll < bossChance)
            return ItemTier.Boss;

        if (roll < bossChance + rareChance)
            return ItemTier.Rare;

        return ItemTier.Common;
    }
}
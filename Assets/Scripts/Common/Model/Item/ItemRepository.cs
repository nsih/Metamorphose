using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Common;

public class ItemRepository
{
    private readonly ItemDatabase _database;
    private readonly Dictionary<string, ItemSO> _lookup;

    public ItemRepository(ItemDatabase database)
    {
        _database = database;
        _lookup = database.Items
            .Where(item => item != null)
            .ToDictionary(item => item.itemId);
    }

    public ItemSO Get(string itemId)
    {
        _lookup.TryGetValue(itemId, out var result);
        return result;
    }

    public IEnumerable<ItemSO> GetAll()
    {
        return _lookup.Values;
    }

    public IEnumerable<ItemSO> GetByTier(ItemTier tier)
    {
        return _lookup.Values.Where(item => item.tier == tier);
    }

    public IEnumerable<ItemSO> GetByTag(string tag)
    {
        return _lookup.Values.Where(item =>
            item.tags != null && Array.IndexOf(item.tags, tag) >= 0);
    }

    public List<ItemSO> GetRandomRewards(int count, AcquiredItemRegistry registry)
    {
        var pool = _database.GetRewardPool(registry);
        var result = new List<ItemSO>();
        var used = new HashSet<string>();

        int maxAttempts = count * 10;
        int attempts = 0;

        while (result.Count < count && attempts < maxAttempts)
        {
            attempts++;

            if (pool.Count == 0) break;

            ItemTier targetTier = _database.RollTier();

            var candidates = pool
                .Where(item => item.tier == targetTier && !used.Contains(item.itemId))
                .ToList();

            if (candidates.Count == 0)
            {
                candidates = pool
                    .Where(item => !used.Contains(item.itemId))
                    .ToList();
            }

            if (candidates.Count == 0) break;

            int idx = UnityEngine.Random.Range(0, candidates.Count);
            ItemSO picked = candidates[idx];

            result.Add(picked);
            used.Add(picked.itemId);

            Debug.Log($"reward selected: [{picked.tier}] {picked.itemId}");
        }

        return result;
    }
}
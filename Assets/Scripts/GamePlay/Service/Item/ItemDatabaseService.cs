using System;
using System.Collections.Generic;
using System.Linq;
using TJR.Core.Common.SO;
using UnityEngine;

namespace TJR.Core.GamePlay.Service
{
    public class ItemDatabaseService : IDisposable
    {
        Dictionary<string, ItemSO> _itemDatabase;

        public ItemDatabaseService(ItemDatabaseSO itemDatabase)
        {
            _itemDatabase = itemDatabase.items.ToDictionary(item => item.label, item => item);
        }

        public ItemSO GetItem(string itemId)
        {
            if (!_itemDatabase.TryGetValue(itemId, out var item))
            {
                Debug.LogError($"ItemDatabaseService: Item {itemId} not found");
                return null;
            }

            return item;
        }

        public void Dispose()
        {
            _itemDatabase.Clear();
        }
    }
}
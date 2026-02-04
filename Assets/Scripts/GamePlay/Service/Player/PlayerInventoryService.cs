using System;
using System.Collections.Generic;
using System.Linq;
using TJR.Core.Common.Data;
using TJR.Core.Common.SO;
using UnityEngine;

namespace TJR.Core.GamePlay.Service
{
    public class PlayerInventoryService : IDisposable
    {
        PlayerModel _player;

        Dictionary<string, ItemSO> _itemDatabase;
        List<Item> _items;

        public PlayerInventoryService(ItemDatabaseSO itemDatabase, PlayerModel player)
        {
            _itemDatabase = itemDatabase.items.ToDictionary(item => item.label, item => item);
            _items = new List<Item>();
            _player = player;
            Debug.Log($"PlayerInventoryService: {_itemDatabase.Count} items loaded");
        }

        public void AddItem(string itemId)
        {
            if (!_itemDatabase.TryGetValue(itemId, out var item))
            {
                Debug.LogError($"PlayerInventoryService: Item {itemId} not found");
                return;
            }

            _items.Add(new Item { id = item.label });
            item.Apply(_player);
        }

        public void Dispose()
        {
            _player = null;
            _itemDatabase.Clear();
            _items.Clear();
        }
    }
}
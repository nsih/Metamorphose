using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using TJR.Core.Common.Data;
using TJR.Core.Common.SO;
using UnityEngine;

namespace TJR.Core.GamePlay.Service
{
    public class PlayerInventoryService : IDisposable
    {
        [Inject] PlayerModel _player;
        [Inject] ItemDatabaseService _itemDatabaseService;

        List<Item> _items;

        public PlayerInventoryService()
        {
            _items = new List<Item>();
        }

        public void AddItem(string itemId)
        {
            var item = _itemDatabaseService.GetItem(itemId);

            if (item == null)
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
            _itemDatabaseService = null;
            _items.Clear();
        }
    }
}
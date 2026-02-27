using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

namespace TJR.Core.GamePlay.Service
{
    public class ShopService
    {
        [Inject] PlayerGoldService _playerGoldService;
        [Inject] PlayerInventoryService _playerInventoryService;
        [Inject] ItemDatabaseService _itemDatabaseService;

        List<string> _availableItems;

        public ShopService()
        {
            _availableItems = new List<string>();
        }

        /// <summary>
        /// 판매 가능한 아이템 목록 반환
        /// </summary>
        /// <returns>판매 가능한 아이템 목록</returns>
        public List<string> GetAvailableItems()
        {
            return _availableItems;
        }

        /// <summary>
        /// 판매 가능한 아이템 추가
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        public void AddAvailableItem(string itemId)
        {
            _availableItems.Add(itemId);
        }

        /// <summary>
        /// 판매 가능한 아이템 제거
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        public void RemoveAvailableItem(string itemId)
        {
            _availableItems.Remove(itemId);
        }

        public void ClearAvailableItems()
        {
            _availableItems.Clear();
        }

        /// <summary>
        /// 아이템 구매
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        public void BuyItem(string itemId)
        {
            var item = _itemDatabaseService.GetItem(itemId);

            if (item == null)
            {
                Debug.LogError($"ShopService: Item {itemId} not found");
                return;
            }

            if (_playerGoldService.Gold.Value < item.price)
            {
                Debug.LogError($"ShopService: Not enough gold to buy item {itemId}");
                return;
            }

            _playerGoldService.RemoveGold(item.price);
            _playerInventoryService.AddItem(itemId);
            RemoveAvailableItem(itemId);
        }
    }
}
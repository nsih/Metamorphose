using Reflex.Attributes;

namespace TJR.Core.GamePlay.Service
{
    public class ShopService
    {
        [Inject] PlayerGoldService _playerGoldService;
        [Inject] PlayerInventoryService _playerInventoryService;

        public void BuyItem(string itemId)
        {

        }
    }
}
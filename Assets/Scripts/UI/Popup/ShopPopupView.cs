using R3;
using Reflex.Extensions;
using TJR.Core.GamePlay.Service;
using TJR.Core.Interface;
using UnityEngine;

namespace TJR.Core.UI
{
    public class ShopPopupView : MonoBehaviour
    {
        [SerializeField] RectTransform _shopItemViewContainer;
        [SerializeField] ShopItemView _shopItemViewPrefab;

        void Awake()
        {
            _shopItemViewPrefab.gameObject.SetActive(false);
            CreateShopItemViews();
        }

        void CreateShopItemViews()
        {
            var shopService = this.gameObject.scene.GetSceneContainer().Single<ShopService>();
            var availableItems = shopService.GetAvailableItems();
            foreach (var itemId in availableItems)
            {
                var shopItemView = Instantiate(_shopItemViewPrefab, _shopItemViewContainer);
                shopItemView.SetItem(itemId);
                shopItemView.gameObject.SetActive(true);
                shopItemView.OnItemBought.Subscribe(OnItemBought).AddTo(this);
            }
        }

        void OnItemBought(Unit _)
        {
            this.Close();
        }

        public void Close()
        {
            var shopService = this.gameObject.scene.GetSceneContainer().Single<ShopService>();
            shopService.ClearAvailableItems();

            var popupService = this.gameObject.scene.GetSceneContainer().Single<IPopupService>();
            popupService.ClosePopup(gameObject);
        }
    }
}
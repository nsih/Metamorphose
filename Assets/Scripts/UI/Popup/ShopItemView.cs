using R3;
using R3.Triggers;
using Reflex.Extensions;
using TJR.Core.GamePlay.Service;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TJR.Core.UI
{
    public class ShopItemView : MonoBehaviour
    {
        public Subject<Unit> OnItemBought = new Subject<Unit>();

        [SerializeField] Button _button;
        [SerializeField] TMP_Text _itemNameText;

        string _itemId;

        void Awake()
        {
            _itemNameText.text = string.Empty;
            _button.OnPointerClickAsObservable()
                .Subscribe(OnClicked)
                .AddTo(this);
        }

        public void SetItem(string itemId)
        {
            var itemDatabaseService = this.gameObject.scene.GetSceneContainer().Single<ItemDatabaseService>();
            var item = itemDatabaseService.GetItem(itemId);

            if (item == null)
            {
                Debug.LogError($"ShopItemView: Item {itemId} not found");
                return;
            }

            _itemNameText.text = item.name;
            _itemId = itemId;
        }

        void OnClicked(PointerEventData eventData)
        {
            var shopService = this.gameObject.scene.GetSceneContainer().Single<ShopService>();
            shopService.BuyItem(_itemId);

            _button.interactable = false;
            OnItemBought.OnNext(Unit.Default);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace TJR.Core.Common.SO
{
    [CreateAssetMenu(fileName = "SO_ItemDatabase", menuName = "TJR/SO/Item/Item Database")]
    public class ItemDatabaseSO : ScriptableObject
    {
        public List<ItemSO> items;

        void OnEnable()
        {
            if (items == null) items = new List<ItemSO>();
        }
    }
}
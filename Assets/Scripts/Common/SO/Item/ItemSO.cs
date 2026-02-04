using System.Collections.Generic;
using UnityEngine;

namespace TJR.Core.Common.SO
{
    [CreateAssetMenu(fileName = "SO_NewItem", menuName = "TJR/SO/Item/Item")]
    public class ItemSO : ScriptableObject
    {
        public string label;
        [SerializeReference] public List<ItemAbility> abilities;

        public void Apply(PlayerModel player)
        {
            foreach (var ability in abilities)
            {
                ability.Apply(player);
            }
        }

        void OnEnable()
        {
            if (string.IsNullOrEmpty(label)) label = name;
            if (abilities == null) abilities = new List<ItemAbility>();
        }
    }
}
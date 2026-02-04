using System.Collections.Generic;
using UnityEngine;

namespace TJR.Core.Common.SO
{
    [CreateAssetMenu(fileName = "SO_NewItem", menuName = "TJR/SO/Item")]
    public class ItemSO : ScriptableObject
    {
        public string label;
        [SerializeReference] public List<ItemAbility> abilities;

        void OnEnable()
        {
            if (string.IsNullOrEmpty(label)) label = name;
            if (abilities == null) abilities = new List<ItemAbility>();
        }
    }
}
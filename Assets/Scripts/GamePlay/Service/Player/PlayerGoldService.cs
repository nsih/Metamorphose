using System;
using R3;
using UnityEngine;

namespace TJR.Core.GamePlay.Service
{
    public class PlayerGoldService : IDisposable
    {
        public ReactiveProperty<int> Gold { get; private set; }

        public int _gold;

        public PlayerGoldService()
        {
            _gold = 0;
            Gold = new ReactiveProperty<int>(_gold);
        }

        public void AddGold(int amount)
        {
            _gold += amount;
            Gold.Value = _gold;
        }

        public void RemoveGold(int amount)
        {
            if (_gold - amount < 0)
            {
                Debug.LogError("PlayerGoldService: Not enough gold");
                return;
            }

            _gold -= amount;
            Gold.Value = _gold;
        }

        public void Dispose()
        {
            Gold.Dispose();
        }
    }
}
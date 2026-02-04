using UnityEngine;

namespace TJR.Core.GamePlay.Service
{
    public class PlayerGoldService
    {
        public int Gold { get; private set; }

        public void AddGold(int amount)
        {
            Gold += amount;
        }

        public void RemoveGold(int amount)
        {
            if (Gold - amount < 0)
            {
                Debug.LogError("PlayerGoldService: Not enough gold");
                return;
            }

            Gold -= amount;
        }
    }
}
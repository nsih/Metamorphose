using System;

namespace TJR.Core.Common.SO
{
    [Serializable]
    public abstract class ItemAbility
    {
        public abstract void Apply(PlayerModel player);
    }

    [Serializable]
    public class MaxHPAbility : ItemAbility
    {
        public int amount;

        public override void Apply(PlayerModel player)
        {
            player.Health.AddMaxHP(amount);
        }
    }

    [Serializable]
    public class DamageAbility : ItemAbility
    {
        public int amount;

        public override void Apply(PlayerModel player)
        {
            player.Weapon.AddDamage(amount);
        }
    }
}
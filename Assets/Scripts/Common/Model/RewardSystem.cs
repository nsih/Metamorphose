using UnityEngine;
using Common;
using Common.Model;

public class RewardSystem
{
    private PlayerModel _model;

    public RewardSystem(PlayerModel model)
    {
        _model = model;
    }

    public void ApplyReward(RewardData reward)
    {
        if (reward == null) return;

        if (reward.Effects == null || reward.Effects.Count == 0) return;

        foreach (var effect in reward.Effects)
            ApplyEffect(effect);
    }

    public void ApplyRewards(RewardData[] rewards)
    {
        foreach (var reward in rewards)
            ApplyReward(reward);
    }

    private void ApplyEffect(RewardEffect effect)
    {
        switch (effect.Type)
        {
            case RewardType.MaxHP:
                _model.Health.AddMaxHP(effect.Value);
                break;

            case RewardType.MaxHPPercent:
                _model.Health.AddMaxHPPercent(effect.Value);
                break;

            case RewardType.Heal:
                _model.Health.Heal(effect.Value);
                break;

            case RewardType.Damage:
                _model.Weapon.AddDamage(effect.Value);
                break;

            case RewardType.DamagePercent:
                _model.Weapon.AddDamagePercent(effect.Value);
                break;

            case RewardType.DamageMultiplier:
                _model.Weapon.AddDamageMultiplier(effect.Value);
                break;

            case RewardType.AttackSpeed:
                _model.Weapon.IncreaseAttackSpeed(effect.Value);
                break;

            case RewardType.Multishot:
                _model.Weapon.AddProjectileCount((int)effect.Value);
                break;

            case RewardType.MoveSpeed:
                _model.Stats.MoveSpeed.AddModifier(new StatModifier(effect.Value, StatModType.Flat));
                break;

            case RewardType.RewardChoiceCount:
                _model.Stats.RewardChoiceCount.AddModifier(new StatModifier((int)effect.Value, StatModType.Flat));
                break;

            default:
                Debug.LogWarning($"RewardSystem: unhandled type {effect.Type}");
                break;
        }
    }
}
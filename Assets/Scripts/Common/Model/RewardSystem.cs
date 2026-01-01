using UnityEngine;
using Common;

public class RewardSystem
{
    private PlayerModel _model;

    public RewardSystem(PlayerModel model)
    {
        _model = model;
    }

    /// <summary>
    /// 보상 적용 (여러 효과 처리)
    /// </summary>
    public void ApplyReward(RewardData reward)
    {
        if (reward == null)
        {
            Debug.LogError("RewardSystem: reward is null!");
            return;
        }

        if (reward.Effects == null || reward.Effects.Count == 0)
        {
            Debug.LogWarning($"RewardSystem: {reward.name} has no effects");
            return;
        }

        Debug.Log($"보상 적용: [{reward.Rarity}] {reward.DisplayName} ({reward.Effects.Count}개 효과)");

        // 모든 효과를 순서대로 적용
        foreach (var effect in reward.Effects)
        {
            ApplyEffect(effect);
        }
    }

    /// <summary>
    /// 개별 효과 적용
    /// </summary>
    private void ApplyEffect(RewardEffect effect)
    {
        switch (effect.Type)
        {
            // Health
            case RewardType.MaxHP:
                _model.Health.AddMaxHP(effect.Value);
                Debug.Log($"  → 최대 체력 +{effect.Value}: {_model.MaxHP}");
                break;

            case RewardType.MaxHPPercent:
                _model.Health.AddMaxHPPercent(effect.Value);
                Debug.Log($"  → 최대 체력 +{effect.Value * 100}%: {_model.MaxHP}");
                break;

            case RewardType.Heal:
                _model.Health.Heal(effect.Value);
                Debug.Log($"  → 체력 회복 +{effect.Value}: {_model.CurrentHP.Value}");
                break;

            // DMG
            case RewardType.Damage:
                _model.Weapon.AddDamage(effect.Value);
                Debug.Log($"  → 공격력 +{effect.Value}: {_model.Damage}");
                break;

            case RewardType.DamagePercent:
                _model.Weapon.AddDamagePercent(effect.Value);
                Debug.Log($"  → 공격력 +{effect.Value * 100}%: {_model.Damage}");
                break;

            case RewardType.DamageMultiplier:
                _model.Weapon.AddDamageMultiplier(effect.Value);
                Debug.Log($"  → 공격력 ×{effect.Value}: {_model.Damage}");
                break;

            // Attack Speed
            case RewardType.AttackSpeed:
                _model.Weapon.IncreaseAttackSpeed(effect.Value);
                Debug.Log($"  → 공격 속도 +{effect.Value * 100}%: {_model.FireRate}");
                break;

            // Projectile
            case RewardType.Multishot:
                _model.Weapon.AddProjectileCount((int)effect.Value);
                Debug.Log($"  → 발사체 +{effect.Value}: {_model.ProjectileCount}");
                break;

            // Movement
            case RewardType.MoveSpeed:
                _model.Stats.IncreaseMoveSpeed(effect.Value);
                Debug.Log($"  → 이동 속도 +{effect.Value}: {_model.MoveSpeed}");
                break;

            // reward cnt
            case RewardType.RewardChoiceCount:
                _model.Stats.IncreaseRewardChoiceCount((int)effect.Value);
                Debug.Log($"  → 보상 선택지 +{effect.Value}: {_model.RewardChoiceCount}");
                break;

            default:
                Debug.LogWarning($"RewardSystem: 처리되지 않은 보상 타입 - {effect.Type}");
                break;
        }
    }

    /// <summary>
    /// 여러 보상 한꺼번에 적용
    /// </summary>
    public void ApplyRewards(RewardData[] rewards)
    {
        foreach (var reward in rewards)
        {
            ApplyReward(reward);
        }
    }
}
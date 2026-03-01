using Common;
using Common.Model;
using UnityEngine;

public class ItemApplyService
{
    public AcquiredItemRegistry Registry => _registry;
    
    private readonly PlayerModel _playerModel;
    private readonly AcquiredItemRegistry _registry;

    public ItemApplyService(PlayerModel playerModel, AcquiredItemRegistry registry)
    {
        _playerModel = playerModel;
        _registry = registry;
    }

    public void Apply(ItemSO item)
    {
        if (item == null)
        {
            Debug.LogWarning("ItemApplyService: item null");
            return;
        }

        if (item.statModifiers != null)
        {
            foreach (var entry in item.statModifiers)
                ApplyStat(entry);
        }

        if (item.onPickupHeal > 0f)
            _playerModel.Health.Heal(item.onPickupHeal);

        if (item.isUnique)
            _registry.Register(item.itemId);

        Debug.Log($"item applied: [{item.tier}] {item.itemId}");
    }

    private void ApplyStat(StatModifierEntry entry)
    {
        var modifier = new StatModifier(entry.value, entry.modType);

        switch (entry.statName)
        {
            case "MaxHp":
                _playerModel.Health.MaxHP.AddModifier(modifier);
                break;

            case "Damage":
                _playerModel.Weapon.Damage.AddModifier(modifier);
                break;

            case "FireRate":
                _playerModel.Weapon.FireRate.AddModifier(
                    new StatModifier(-entry.value, entry.modType));
                break;

            case "ProjectileCount":
                _playerModel.Weapon.ProjectileCount.AddModifier(modifier);
                _playerModel.Weapon.SpreadAngle.AddModifier(
                    new StatModifier(entry.value * 5f, StatModType.Flat));
                break;

            case "Range":
                _playerModel.Weapon.Range.AddModifier(modifier);
                break;

            case "SpeedScale":
                _playerModel.Weapon.SpeedScale.AddModifier(modifier);
                break;

            case "HomingStrength":
                _playerModel.Weapon.HomingStrength.AddModifier(modifier);
                break;

            case "MoveSpeed":
                _playerModel.Stats.MoveSpeed.AddModifier(modifier);
                break;

            case "RewardChoiceCount":
                _playerModel.Stats.RewardChoiceCount.AddModifier(modifier);
                break;

            default:
                Debug.LogWarning($"ItemApplyService: unknown statName '{entry.statName}'");
                break;
        }
    }
}
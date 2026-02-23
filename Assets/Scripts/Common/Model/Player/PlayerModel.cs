using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using BulletPro;
using Common;

public class PlayerModel : IDisposable
{
    public PlayerHealthSystem Health { get; private set; }
    public PlayerWeaponSystem Weapon { get; private set; }
    public PlayerDashSystem Dash { get; private set; }
    public PlayerStatsSystem Stats { get; private set; }
    public RewardSystem Reward { get; private set; }

    public PlayerModel(PlayerStat stat, PlayerWeaponData initialWeapon)
    {
        Health = new PlayerHealthSystem(stat.MaxHealth);
        Weapon = new PlayerWeaponSystem(initialWeapon);
        Dash = new PlayerDashSystem(stat);
        Stats = new PlayerStatsSystem(stat);
        Reward = new RewardSystem(this);
    }

    // Health
    public float MaxHP => Health.MaxHP.Value;
    public AsyncReactiveProperty<float> CurrentHP => Health.CurrentHP;
    public void TakeDamage(float amount) => Health.TakeDamage(amount);
    public void Heal(float amount) => Health.Heal(amount);
    
    // Weapon
    public EmitterProfile CurrentProfile => Weapon.CurrentProfile;
    public float Damage => Weapon.Damage.Value;
    public float FireRate => Weapon.FireRate.Value;
    public float Range => Weapon.Range.Value;
    public int ProjectileCount => (int)Weapon.ProjectileCount.Value;
    public float SpreadAngle => Weapon.SpreadAngle.Value;
    public float SpeedScale => Weapon.SpeedScale.Value;
    public float HomingStrength => Weapon.HomingStrength.Value;
    public void SetWeapon(PlayerWeaponData data) => Weapon.SetWeapon(data);
    
    // Dash
    public float DashSpeed => Dash.DashSpeed;
    public float DashDuration => Dash.DashDuration;
    public int MaxDashChargeStack => Dash.MaxDashChargeStack;
    public float DashChargeTime => Dash.DashChargeTime;
    public AsyncReactiveProperty<int> CurrentDashCount => Dash.CurrentDashCount;
    public AsyncReactiveProperty<float> DashCooldownNormalized => Dash.DashCooldownNormalized;
    public bool TryConsumeDash() => Dash.TryConsumeDash();
    
    // Stats
    public float MoveSpeed => Stats.MoveSpeed;
    public float TimeSlowFactor => Stats.TimeSlowFactor;
    public float SlowMotionDuration => Stats.SlowMotionDuration;
    public int RewardChoiceCount => Stats.RewardChoiceCount;

    // Reward
    public void IncreaseMaxHP(float amount) => Health.AddMaxHP(amount);
    public void IncreaseDamage(float amount) => Weapon.AddDamage(amount);
    public void IncreaseMultishot(int amount) => Weapon.AddProjectileCount(amount);
    public void IncreaseAttackSpeed(float percent) => Weapon.IncreaseAttackSpeed(percent);

    public void Dispose()
    {
        Health?.Dispose();
        Dash?.Dispose();
        Weapon?.Dispose();
    }
}
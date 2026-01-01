using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using BulletPro;
using Common;

public class PlayerModel : IDisposable
{
    // 하위시스템
    public PlayerHealthSystem Health { get; private set; }//HP
    public PlayerWeaponSystem Weapon { get; private set; }//공격
    public PlayerDashSystem Dash { get; private set; }//대시
    public PlayerStatsSystem Stats { get; private set; }//스택
    public RewardSystem Reward { get; private set; } // 보상

    public PlayerModel(PlayerStat stat, PlayerWeaponData initialWeapon)
    {
        Health = new PlayerHealthSystem(stat.MaxHealth);
        Weapon = new PlayerWeaponSystem(initialWeapon);
        Dash = new PlayerDashSystem(stat);
        Stats = new PlayerStatsSystem(stat);
        Reward = new RewardSystem(this);
    }

    // 파사드 프로퍼티
    
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
    public float JumpForce => Stats.JumpForce;
    public float TimeSlowFactor => Stats.TimeSlowFactor;
    public float SlowMotionDuration => Stats.SlowMotionDuration;

    // reward 함수 (파사드 하위 시스템의 함수)
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
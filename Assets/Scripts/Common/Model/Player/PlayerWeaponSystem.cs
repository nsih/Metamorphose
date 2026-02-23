using BulletPro;
using UnityEngine;
using Common;
using Common.Model;
using System;

public class PlayerWeaponSystem
{
    public EmitterProfile CurrentProfile { get; private set; }
    
    public ModifiableStat Damage { get; private set; }
    public ModifiableStat FireRate { get; private set; }
    public ModifiableStat Range { get; private set; }
    public ModifiableStat ProjectileCount { get; private set; }
    public ModifiableStat SpreadAngle { get; private set; }
    public ModifiableStat SpeedScale { get; private set; }
    public ModifiableStat HomingStrength { get; private set; }

    public PlayerWeaponSystem(PlayerWeaponData initialWeapon)
    {
        if (initialWeapon != null)
        {
            SetWeapon(initialWeapon);
        }
        else
        {
            Debug.LogError("PlayerWeaponSystem: initialWeapon is null");
            InitializeWithDefaults();
        }
    }

    private void InitializeWithDefaults()
    {
        Damage = new ModifiableStat(1f);
        FireRate = new ModifiableStat(0.5f);
        Range = new ModifiableStat(30f);
        ProjectileCount = new ModifiableStat(1);
        SpreadAngle = new ModifiableStat(0f);
        SpeedScale = new ModifiableStat(20f);
        HomingStrength = new ModifiableStat(0f);
    }

    public void SetWeapon(PlayerWeaponData data)
    {
        CurrentProfile = data.Profile;
        
        Damage = new ModifiableStat(data.BaseDamage);
        FireRate = new ModifiableStat(data.BaseFireRate);
        Range = new ModifiableStat(data.BaseRange);
        ProjectileCount = new ModifiableStat(data.BaseProjectileCount);
        SpreadAngle = new ModifiableStat(data.BaseSpreadAngle);
        SpeedScale = new ModifiableStat(data.BaseSpeedScale);
        HomingStrength = new ModifiableStat(data.BaseHomingStrength);

        Debug.Log($"Weapon changed to: {data.name}");
    }

    public void AddDamage(float amount)
    {
        Damage.AddModifier(new StatModifier(amount, StatModType.Flat));
    }

    public void AddDamagePercent(float percent)
    {
        Damage.AddModifier(new StatModifier(percent, StatModType.PercentAdd));
    }

    public void AddDamageMultiplier(float multiplier)
    {
        Damage.AddModifier(new StatModifier(multiplier - 1f, StatModType.PercentMult));
    }

    public void AddProjectileCount(int amount)
    {
        ProjectileCount.AddModifier(new StatModifier(amount, StatModType.Flat));
        SpreadAngle.AddModifier(new StatModifier(amount * 5f, StatModType.Flat));
    }

    public void IncreaseAttackSpeed(float percent)
    {
        FireRate.AddModifier(new StatModifier(-percent, StatModType.PercentAdd));
    }

    public void Dispose()
    {
        // 필요시 이벤트 해제
    }
}
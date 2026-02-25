using System;
using Common.Model;
using Common;
using R3;

public class PlayerHealthSystem : IDisposable
{
    public ModifiableStat MaxHP { get; private set; }
    public ReactiveProperty<float> CurrentHP { get; private set; }

    public event Action OnDeath;

    private float _lastMaxHP;

    public PlayerHealthSystem(float baseMaxHP)
    {
        MaxHP = new ModifiableStat(baseMaxHP);
        CurrentHP = new ReactiveProperty<float>(MaxHP.Value);
        _lastMaxHP = MaxHP.Value;

        MaxHP.OnValueChanged += OnMaxHPChanged;
    }

    private void OnMaxHPChanged(float newMax)
    {
        float diff = newMax - _lastMaxHP;
        if (diff > 0)
        {
            CurrentHP.Value += diff;
        }
        else
        {
            if (CurrentHP.Value > newMax)
                CurrentHP.Value = newMax;
        }
        _lastMaxHP = newMax;
    }

    public void TakeDamage(float amount)
    {
        CurrentHP.Value -= amount;
        if (CurrentHP.Value < 0) CurrentHP.Value = 0;

        if (CurrentHP.Value <= 0)
            OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        float nextHP = CurrentHP.Value + amount;
        if (nextHP > MaxHP.Value) nextHP = MaxHP.Value;
        CurrentHP.Value = nextHP;
    }

    public void AddMaxHP(float amount)
    {
        MaxHP.AddModifier(new StatModifier(amount, StatModType.Flat));
    }

    public void AddMaxHPPercent(float percent)
    {
        MaxHP.AddModifier(new StatModifier(percent, StatModType.PercentAdd));
    }

    public void Dispose()
    {
        if (MaxHP != null)
            MaxHP.OnValueChanged -= OnMaxHPChanged;

        CurrentHP?.Dispose();
    }
}
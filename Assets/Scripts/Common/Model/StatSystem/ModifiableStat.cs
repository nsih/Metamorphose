using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.Model
{
    [Serializable]
    public class ModifiableStat
    {
        [SerializeField] 
        private float _baseValue;

        private bool _isDirty = true;
        private float _cachedValue;

        private readonly List<StatModifier> _modifiers = new List<StatModifier>();

        public event Action<float> OnValueChanged;

        public ModifiableStat(float baseValue = 0f)
        {
            _baseValue = baseValue;
        }

        public float BaseValue
        {
            get => _baseValue;
            set
            {
                if (Math.Abs(_baseValue - value) > 0.0001f)
                {
                    _baseValue = value;
                    _isDirty = true;
                    OnValueChanged?.Invoke(Value);
                }
            }
        }

        public float Value
        {
            get
            {
                if (_isDirty)
                {
                    _cachedValue = CalculateFinalValue();
                    _isDirty = false;
                }
                return _cachedValue;
            }
        }

        public void AddModifier(StatModifier mod)
        {
            _isDirty = true;
            _modifiers.Add(mod);
            _modifiers.Sort(CompareModifierOrder);
            OnValueChanged?.Invoke(Value);
        }

        public bool RemoveModifier(StatModifier mod)
        {
            if (_modifiers.Remove(mod))
            {
                _isDirty = true;
                OnValueChanged?.Invoke(Value);
                return true;
            }
            return false;
        }

        public void ClearModifiers()
        {
            if (_modifiers.Count > 0)
            {
                _modifiers.Clear();
                _isDirty = true;
                OnValueChanged?.Invoke(Value);
            }
        }

        private int CompareModifierOrder(StatModifier a, StatModifier b)
        {
            if (a.Type < b.Type) return -1;
            if (a.Type > b.Type) return 1;
            return 0;
        }

        private float CalculateFinalValue()
        {
            float finalValue = BaseValue;
            float sumPercentAdd = 0;

            for (int i = 0; i < _modifiers.Count; i++)
            {
                StatModifier mod = _modifiers[i];

                if (mod.Type == StatModType.Flat)
                {
                    finalValue += mod.Value;
                }
                else if (mod.Type == StatModType.PercentAdd)
                {
                    sumPercentAdd += mod.Value;
                }
                else if (mod.Type == StatModType.PercentMult)
                {
                    if (sumPercentAdd != 0)
                    {
                        finalValue *= (1 + sumPercentAdd);
                        sumPercentAdd = 0;
                    }
                    finalValue *= (1 + mod.Value);
                }
            }

            if (sumPercentAdd != 0)
            {
                finalValue *= (1 + sumPercentAdd);
            }

            return (float)Math.Round(finalValue, 4);
        }
    }
}
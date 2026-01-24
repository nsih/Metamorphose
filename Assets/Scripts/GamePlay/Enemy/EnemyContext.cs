using UnityEngine;
using BulletPro;
using System.Collections.Generic;

public class EnemyContext
{
    public EnemyBrainSO Brain { get; private set; }
    
    public int CurrentHP { get; set; }
    public int MaxHP => Brain.MaxHP;
    public bool IsEnraged { get; private set; }
    public bool IsDead { get; set; }
    public float LastAttackTime { get; set; }
    
    public Transform Target { get; set; }
    public Transform Self { get; private set; }
    
    public SpriteRenderer SpriteRenderer { get; private set; }
    public BulletEmitter Emitter { get; private set; }
    
    private Color _originalColor;

    
    private Dictionary<int, float> _floatData = new Dictionary<int, float>();
    private Dictionary<int, int> _intData = new Dictionary<int, int>();

    public EnemyContext(Transform self, SpriteRenderer spriteRenderer, BulletEmitter emitter)
    {
        Self = self;
        SpriteRenderer = spriteRenderer;
        Emitter = emitter;
        
        if (spriteRenderer != null)
            _originalColor = spriteRenderer.color;
    }

    public void Initialize(EnemyBrainSO brain, Transform target)
    {
        Brain = brain;
        Target = target;
        CurrentHP = brain.MaxHP;
        IsDead = false;
        IsEnraged = false;
        LastAttackTime = 0f;
    }

    public float DistanceToTarget
    {
        get
        {
            if (Target == null) return float.MaxValue;
            return Vector3.Distance(Self.position, Target.position);
        }
    }

    public float CurrentMoveSpeed
    {
        get
        {
            if (IsEnraged && Brain.HasEnragedPhase)
                return Brain.EnragedMoveSpeed;
            return Brain.MoveSpeed;
        }
    }

    public float CurrentAttackCoolTime
    {
        get
        {
            if (IsEnraged && Brain.HasEnragedPhase)
                return Brain.EnragedAttackCoolTime;
            return Brain.AttackCoolTime;
        }
    }

    public float GetFloat(int key, float defaultValue = 0f)
    {
        return _floatData.TryGetValue(key, out float val) ? val : defaultValue;
    }

    public void SetFloat(int key, float value)
    {
        _floatData[key] = value;
    }

    public int GetInt(int key, int defaultValue = 0)
    {
        return _intData.TryGetValue(key, out int val) ? val : defaultValue;
    }

    public void SetInt(int key, int value)
    {
        _intData[key] = value;
    }

    public void CheckEnrage()
    {
        if (IsEnraged || !Brain.HasEnragedPhase) return;
        
        if (CurrentHP <= MaxHP * Brain.EnrageThreshold)
        {
            IsEnraged = true;
        }
    }

    public void Reset()
    {
        CurrentHP = Brain != null ? Brain.MaxHP : 0;
        IsDead = false;
        IsEnraged = false;
        LastAttackTime = 0f;
        
        _floatData.Clear();
        _intData.Clear();
        
        if (SpriteRenderer != null)
            SpriteRenderer.color = _originalColor;
        
        if (Emitter != null)
            Emitter.Kill();
    }
}
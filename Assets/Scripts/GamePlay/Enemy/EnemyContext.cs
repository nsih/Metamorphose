using UnityEngine;
using BulletPro;

public class EnemyContext
{
    public EnemyDataSO Data { get; private set; }
    
    public int CurrentHP { get; set; }
    public int MaxHP => Data.MaxHp;
    public bool IsEnraged { get; private set; }
    public bool IsDead { get; set; }
    public float LastAttackTime { get; set; }
    
    public Transform Target { get; set; }
    public Transform Self { get; private set; }
    
    public EnemyMovement Movement { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
    public BulletEmitter Emitter { get; private set; }
    
    private Color _originalColor;

    public EnemyContext(Transform self, EnemyMovement movement, SpriteRenderer spriteRenderer, BulletEmitter emitter)
    {
        Self = self;
        Movement = movement;
        SpriteRenderer = spriteRenderer;
        Emitter = emitter;
        
        if (spriteRenderer != null)
            _originalColor = spriteRenderer.color;
    }

    public void Initialize(EnemyDataSO data, Transform target)
    {
        Data = data;
        Target = target;
        CurrentHP = data.MaxHp;
        IsDead = false;
        IsEnraged = false;
        LastAttackTime = 0f;
        
        if (Movement != null)
            Movement.Initialize(target, data);
    }

    public float DistanceToTarget
    {
        get
        {
            if (Target == null) return float.MaxValue;
            return Vector3.Distance(Self.position, Target.position);
        }
    }

    public EnemyMoveStrategySO CurrentMoveStrategy
    {
        get
        {
            if (IsEnraged && Data.EnragedMoveStrategy != null)
                return Data.EnragedMoveStrategy;
            return Data.MoveStrategy;
        }
    }

    public EnemyAttackStrategySO CurrentAttackStrategy
    {
        get
        {
            if (IsEnraged && Data.EnragedAttackStrategy != null)
                return Data.EnragedAttackStrategy;
            return Data.AttackStrategy;
        }
    }

    public float CurrentMoveSpeed
    {
        get
        {
            if (IsEnraged && Data.HasEnragedPhase)
                return Data.EnragedMoveSpeed;
            return Data.MoveSpeed;
        }
    }

    public float CurrentAttackCoolTime
    {
        get
        {
            if (IsEnraged && Data.HasEnragedPhase)
                return Data.EnragedAttackCoolTime;
            return Data.AttackCoolTime;
        }
    }

    public void CheckEnrage()
    {
        if (IsEnraged || !Data.HasEnragedPhase) return;
        
        if (CurrentHP <= MaxHP * Data.EnrageThreshold)
        {
            IsEnraged = true;
        }
    }

    public void Reset()
    {
        CurrentHP = Data != null ? Data.MaxHp : 0;
        IsDead = false;
        IsEnraged = false;
        LastAttackTime = 0f;
        
        if (SpriteRenderer != null)
            SpriteRenderer.color = _originalColor;
        
        if (Emitter != null)
            Emitter.Kill();
    }
}
// Assets/Scripts/GamePlay/Enemy/EnemyContext.cs
using UnityEngine;
using BulletPro;

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
        
        if (SpriteRenderer != null)
            SpriteRenderer.color = _originalColor;
        
        if (Emitter != null)
            Emitter.Kill();
    }
}
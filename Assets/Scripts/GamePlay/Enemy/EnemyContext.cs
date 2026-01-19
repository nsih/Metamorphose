using UnityEngine;
using BulletPro;

public class EnemyContext
{
    // 데이터
    public EnemyDataSO Data { get; private set; }
    
    // 런타임 상태
    public int CurrentHP { get; set; }
    public int MaxHP => Data.MaxHp;
    public bool IsEnraged { get; private set; }
    public bool IsDead { get; set; }
    public float LastAttackTime { get; set; }
    
    // 타겟
    public Transform Target { get; set; }
    public Transform Self { get; private set; }
    
    // 컴포넌트 참조
    public EnemyMovement Movement { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
    public BulletEmitter Emitter { get; private set; }
    
    // 원본 색상 저장
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

    // 현재 페이즈에 맞는 전략 반환
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

    // 발악 체크
    public void CheckEnrage()
    {
        if (IsEnraged || !Data.HasEnragedPhase) return;
        
        if (CurrentHP <= MaxHP * Data.EnrageThreshold)
        {
            IsEnraged = true;
            Debug.Log($"[{Self.name}] Enraged");
        }
    }

    // 풀링 시 초기화
    public void Reset()
    {
        CurrentHP = Data != null ? Data.MaxHp : 0;
        IsDead = false;
        IsEnraged = false;
        LastAttackTime = 0f;
        
        // 색상 복구
        if (SpriteRenderer != null)
            SpriteRenderer.color = _originalColor;
        
        // 이동 멈춤
        if (Movement != null)
            Movement.enabled = false;
        
        // 공격 멈춤
        if (Emitter != null)
            Emitter.Kill();
    }
}
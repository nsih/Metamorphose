using UnityEngine;
using BulletPro;
using System.Collections.Generic;
using System;

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
    
    public Action<Vector3, EnemyBrainSO> SpawnAction { get; set; }
    
    private Color _originalColor;
    private Dictionary<int, float> _floatData = new Dictionary<int, float>();
    private Dictionary<int, int> _intData = new Dictionary<int, int>();
    public float TimeInCurrentState { get; set; }
    
    public Rigidbody2D Rigidbody { get; private set; }
    public AreaIndicatorPool AreaPool { get; set; }
    
    private System.Threading.CancellationToken _destroyCancellationToken;
    public System.Threading.CancellationToken DestroyCancellationToken => _destroyCancellationToken;

    // 시야 캐싱
    private float _lastVisibilityCheckTime;
    private bool _cachedVisibility;
    private LayerMask _lastObstacleLayer;

    public EnemyContext(Transform self, SpriteRenderer spriteRenderer, BulletEmitter emitter, Rigidbody2D rigidbody)
    {
        Self = self;
        SpriteRenderer = spriteRenderer;
        Emitter = emitter;
        Rigidbody = rigidbody;

        var mb = self != null ? self.GetComponent<MonoBehaviour>() : null;
        _destroyCancellationToken = mb != null ? mb.destroyCancellationToken : System.Threading.CancellationToken.None;

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
        
        _lastVisibilityCheckTime = -999f;
        _cachedVisibility = false;
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

    public bool CheckTargetVisibility(LayerMask obstacleLayer, float maxDistance, float originOffset)
    {
        float interval = Brain?.VisibilityCheckInterval ?? 0.1f;
        
        bool sameLayer = obstacleLayer.value == _lastObstacleLayer.value;
        bool withinInterval = Time.time - _lastVisibilityCheckTime < interval;
        
        if (sameLayer && withinInterval)
            return _cachedVisibility;
        
        _lastVisibilityCheckTime = Time.time;
        _lastObstacleLayer = obstacleLayer;
        
        if (Target == null)
        {
            _cachedVisibility = false;
            return false;
        }
        
        Vector2 origin = Self.position;
        Vector2 targetPos = Target.position;
        Vector2 direction = (targetPos - origin).normalized;
        float distance = Vector2.Distance(origin, targetPos);
        
        if (maxDistance > 0)
            distance = Mathf.Min(maxDistance, distance);
        
        Vector2 offsetOrigin = origin + direction * originOffset;
        float adjustedDistance = distance - originOffset;
        
        if (adjustedDistance <= 0)
        {
            _cachedVisibility = true;
            return true;
        }
        
        RaycastHit2D hit = Physics2D.Raycast(offsetOrigin, direction, adjustedDistance, obstacleLayer);
        _cachedVisibility = hit.collider == null;
        
        return _cachedVisibility;
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
        
        _lastVisibilityCheckTime = -999f;
        _cachedVisibility = false;
        
        if (SpriteRenderer != null)
            SpriteRenderer.color = _originalColor;
        
        if (Emitter != null)
            Emitter.Kill();
    }
}
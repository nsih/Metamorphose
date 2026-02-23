using UnityEngine;
using BulletPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Reflex.Core;
using Reflex.Extensions;
using TJR.Core.GamePlay.Service;

[RequireComponent(typeof(BulletReceiver))]
[RequireComponent(typeof(EnemyFSM))]
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyBrainSO _defaultBrain;

    private BulletReceiver _receiver;
    private EnemyFSM _fsm;
    private SpriteRenderer _spriteRenderer;
    private BulletEmitter _emitter;

    private EnemyContext _ctx;
    private EnemyBrainSO _currentBrain;

    public event Action OnHit;
    public event Action OnDeath;

    private Action _returnToPool;
    private Action<Vector3, EnemyBrainSO> _spawnAction;

    private Color _originalColor;
    private CancellationTokenSource _flashCts;

    private Rigidbody2D _rigidbody;
    private EnemyMuzzleAim _muzzleAim;
    
    private Vector3 _defaultScale;

    private void Awake()
    {
        _receiver = GetComponent<BulletReceiver>();
        _fsm = GetComponent<EnemyFSM>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _emitter = GetComponentInChildren<BulletEmitter>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _muzzleAim = GetComponentInChildren<EnemyMuzzleAim>();

        if (_rigidbody != null)
            _rigidbody.freezeRotation = true;

        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
        
        _defaultScale = transform.localScale;

        _ctx = new EnemyContext(transform, _spriteRenderer, _emitter, _rigidbody);

        OnDeath += () => {
            gameObject.scene.GetSceneContainer().Single<PlayerGoldService>().AddGold(100);
        };
    }

    private void OnEnable()
    {
        if (_receiver != null)
            _receiver.OnHitByBullet.AddListener(OnBulletHit);
    }

    private void OnDisable()
    {
        if (_receiver != null)
            _receiver.OnHitByBullet.RemoveListener(OnBulletHit);

        CancelFlash();
    }

    public void Initialize(EnemyBrainSO brain)
    {
        _currentBrain = brain != null ? brain : _defaultBrain;
        
        if (_currentBrain == null)
        {
            Debug.LogError("Enemy: brain null");
            return;
        }

        ResetState();
        ApplyVisual(_currentBrain);
        
        _ctx.Initialize(_currentBrain, null);
        _ctx.SpawnAction = _spawnAction;
        _fsm.Initialize(_currentBrain, _ctx);
    }

    private void ApplyVisual(EnemyBrainSO brain)
    {
        if (_spriteRenderer != null)
        {
            if (brain.Sprite != null)
                _spriteRenderer.sprite = brain.Sprite;
            
            _spriteRenderer.color = brain.Color;
            _originalColor = brain.Color;
        }
        
        transform.localScale = new Vector3(
            _defaultScale.x * brain.Scale.x,
            _defaultScale.y * brain.Scale.y,
            _defaultScale.z
        );
    }

    private void ResetState()
    {
        CancelFlash();
        
        if (_spriteRenderer != null)
            _spriteRenderer.color = _originalColor;
        
        if (_emitter != null)
            _emitter.Kill();
        
        _ctx.Reset();
    }

    public void SetTarget(Transform target)
    {
        if (_ctx != null)
            _ctx.Target = target;
        
        if (_muzzleAim != null)
            _muzzleAim.SetTarget(target);
    }

    public void SetReleaseAction(Action returnToPool)
    {
        _returnToPool = returnToPool;
    }

    public void SetSpawnAction(Action<Vector3, EnemyBrainSO> spawnAction)
    {
        _spawnAction = spawnAction;
        if (_ctx != null)
            _ctx.SpawnAction = spawnAction;
    }

    public void OnBulletHit(Bullet bullet, Vector3 hitPoint)
    {
        if (_ctx == null || _ctx.IsDead) return;

        float damage = 1f;
        if (bullet.dynamicSolver != null)
        {
            damage = bullet.moduleParameters.GetFloat(BPParams.Damage);
            if (damage <= 0) damage = 1;
        }

        TakeDamage((int)damage);
        bullet.Die();
    }

    public void TakeDamage(int dmg)
    {
        if (_ctx == null || _ctx.IsDead) return;

        _ctx.CurrentHP -= dmg;
        OnHit?.Invoke();
        PlayFlashEffect().Forget();

        if (_ctx.CurrentHP <= 0 && !_ctx.IsDead)
        {
            _ctx.IsDead = true;
            DieSequence().Forget();
        }
    }

    private async UniTaskVoid PlayFlashEffect()
    {
        if (_spriteRenderer == null) return;

        CancelFlash();
        _flashCts = new CancellationTokenSource();

        try
        {
            _spriteRenderer.color = Color.red;
            await UniTask.Delay(100, cancellationToken: _flashCts.Token);
            _spriteRenderer.color = _originalColor;
        }
        catch (OperationCanceledException) { }
    }

    private void CancelFlash()
    {
        if (_flashCts != null)
        {
            _flashCts.Cancel();
            _flashCts.Dispose();
            _flashCts = null;
        }
    }

    private async UniTaskVoid DieSequence()
    {
        _fsm.Stop();
        
        if (_emitter != null)
            _emitter.Stop();
        
        if (_currentBrain.DeathDelay > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_currentBrain.DeathDelay));
        }
        
        if (_currentBrain.DeathEffect != null)
        {
            await _currentBrain.DeathEffect.Execute(_ctx);
        }
        
        OnDeath?.Invoke();

        if (_returnToPool != null)
            _returnToPool.Invoke();
        else
            gameObject.SetActive(false);
    }
}
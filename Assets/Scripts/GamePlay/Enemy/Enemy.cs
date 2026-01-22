using UnityEngine;
using BulletPro;
using Cysharp.Threading.Tasks;
using System.Threading;

[RequireComponent(typeof(BulletReceiver))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyFSM))]
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyDataSO _defaultData;

    private BulletReceiver _receiver;
    private EnemyMovement _movement;
    private EnemyFSM _fsm;
    private SpriteRenderer _spriteRenderer;
    private BulletEmitter _emitter;

    private EnemyContext _ctx;
    private EnemyDataSO _currentData;

    public event System.Action OnHit;
    public event System.Action OnDeath;

    private System.Action _returnToPool;

    private Color _originalColor;
    private CancellationTokenSource _flashCts;

    private void Awake()
    {
        _receiver = GetComponent<BulletReceiver>();
        _movement = GetComponent<EnemyMovement>();
        _fsm = GetComponent<EnemyFSM>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _emitter = GetComponent<BulletEmitter>();

        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;

        _ctx = new EnemyContext(transform, _movement, _spriteRenderer, _emitter);
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

    public void Initialize(EnemyDataSO data)
    {
        _currentData = data != null ? data : _defaultData;
        
        if (_currentData == null)
        {
            Debug.LogError("Enemy: data null");
            return;
        }

        ResetState();
        
        _ctx.Initialize(_currentData, null);
        _fsm.Initialize(_ctx);
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
        {
            _ctx.Target = target;
            
            if (_movement != null)
                _movement.Initialize(target, _currentData);
        }
    }

    public void SetReleaseAction(System.Action returnToPool)
    {
        _returnToPool = returnToPool;
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

        if (_ctx.CurrentHP <= 0)
        {
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
        catch (System.OperationCanceledException) { }
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
        _ctx.IsDead = true;
        _fsm.Stop();
        OnDeath?.Invoke();

        await UniTask.Yield();

        if (_returnToPool != null)
            _returnToPool.Invoke();
        else
            gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        CancelFlash();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_currentData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _currentData.AggroRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _currentData.AttackRange);
    }

    private void OnGUI()
    {
        if (_ctx == null || _ctx.Target == null || _ctx.IsDead) return;
        if (_fsm == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.8f);
        if (screenPos.z < 0) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 15;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;

        switch (_fsm.CurrentStateType)
        {
            case Common.EnemyState.Idle:
                style.normal.textColor = Color.green;
                break;
            case Common.EnemyState.Chase:
                style.normal.textColor = Color.yellow;
                break;
            case Common.EnemyState.Attack:
                style.normal.textColor = Color.red;
                break;
        }

        screenPos.y = Screen.height - screenPos.y;

        string enragedText = _ctx.IsEnraged ? " [ENRAGED]" : "";
        string text = $"{_fsm.CurrentStateType}{enragedText}\nHP: {_ctx.CurrentHP}/{_ctx.MaxHP}";
        
        GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 25, 100, 50), text, style);
    }
#endif
}
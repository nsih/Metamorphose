using UnityEngine;
using BulletPro;
using Cysharp.Threading.Tasks;
using System.Threading;

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

    public event System.Action OnHit;
    public event System.Action OnDeath;

    private System.Action _returnToPool;

    private Color _originalColor;
    private CancellationTokenSource _flashCts;

    private void Awake()
    {
        _receiver = GetComponent<BulletReceiver>();
        _fsm = GetComponent<EnemyFSM>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _emitter = GetComponent<BulletEmitter>();

        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;

        _ctx = new EnemyContext(transform, _spriteRenderer, _emitter);
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
        
        _ctx.Initialize(_currentBrain, null);
        _fsm.Initialize(_currentBrain, _ctx);
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
        if (_currentBrain == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _currentBrain.AggroRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _currentBrain.AttackRange);
    }

    private void OnGUI()
    {
        if (_ctx == null || _ctx.Target == null || _ctx.IsDead) return;
        if (_fsm == null || _fsm.CurrentState == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.8f);
        if (screenPos.z < 0) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 15;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;

        screenPos.y = Screen.height - screenPos.y;

        string enragedText = _ctx.IsEnraged ? " [ENRAGED]" : "";
        string stateName = _fsm.CurrentState.name;
        string text = $"{stateName}{enragedText}\nHP: {_ctx.CurrentHP}/{_ctx.MaxHP}";
        
        GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 25, 100, 50), text, style);
    }
#endif
}
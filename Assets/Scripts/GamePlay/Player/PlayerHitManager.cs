using UnityEngine;
using Reflex.Attributes;
using BulletPro;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(BulletReceiver))]
public class PlayerHitManager : MonoBehaviour, IDamageable
{
    [Inject] private PlayerModel _model;

    private BulletReceiver _receiver;
    private SpriteRenderer _spriteRenderer;
    private PlayerBomb _playerBomb;
    private PlayerDash _playerDash;
    private BulletTimeManager _bulletTimeManager;

    public bool IsInvincible =>
        (_playerDash != null && _playerDash.IsDashing) ||
        (_playerBomb != null && _playerBomb.IsActive);

    void Awake()
    {
        _receiver = GetComponent<BulletReceiver>();
        _playerDash = GetComponent<PlayerDash>();
        _playerBomb = GetComponent<PlayerBomb>();
        _bulletTimeManager = GetComponent<BulletTimeManager>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (_model == null)
        {
            Debug.LogError("PlayerHitManager: DI Error");
            return;
        }

        if (_receiver != null)
        {
            _receiver.OnHitByBullet.AddListener(HandleBulletHit);
        }
    }

    void OnDestroy()
    {
        if (_receiver != null)
        {
            _receiver.OnHitByBullet.RemoveListener(HandleBulletHit);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (_model.CurrentHP.Value <= 0) return;
        if (IsInvincible) return;

        _model.TakeDamage(dmg);
        PlayHitFeedback().Forget();

        if (_model.CurrentHP.Value <= 0)
        {
            OnDead();
        }
    }

    public void HandleBulletHit(Bullet bullet, Vector3 hitPoint)
    {
        if (_model.CurrentHP.Value <= 0) return;

        if (IsInvincible)
        {
            if (_bulletTimeManager != null)
                _bulletTimeManager.TriggerSlowMotion();
            return;
        }

        float damage = 1f;
        if (bullet != null)
        {
            damage = bullet.moduleParameters.GetFloat("_Damage");
            if (damage == 0) damage = 1;
        }

        _model.TakeDamage(damage);
        PlayHitFeedback().Forget();

        bullet.Die();

        if (_model.CurrentHP.Value <= 0)
        {
            OnDead();
        }
    }

    // 시각 비활성화만 담당 — 씬 전환은 RunEndManager가 OnDeath 이벤트로 처리
    private void OnDead()
    {
        gameObject.SetActive(false);
    }

    private async UniTaskVoid PlayHitFeedback()
    {
        if (_spriteRenderer == null) return;

        var token = this.GetCancellationTokenOnDestroy();
        try
        {
            _spriteRenderer.color = Color.red;
            await UniTask.Delay(100, cancellationToken: token);
            _spriteRenderer.color = Color.white;
        }
        catch (System.OperationCanceledException) { }
    }
}
// Assets/Scripts/GamePlay/Player/PlayerHitManager.cs
using UnityEngine;
using Reflex.Attributes;
using BulletPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using TJR.Core.Interface;

[RequireComponent(typeof(BulletReceiver))]
public class PlayerHitManager : MonoBehaviour, IDamageable
{
    [Inject] private PlayerModel _model;
    [Inject] private IAudioService _audio;

    private BulletReceiver _receiver;
    private SpriteRenderer _spriteRenderer;
    private PlayerBomb _playerBomb;
    private PlayerDash _playerDash;
    private BulletTimeManager _bulletTimeManager;

    private bool _isPostHitInvincible;
    private CancellationTokenSource _blinkCts;
    private Color _originalColor;

    public bool IsInvincible =>
        _isPostHitInvincible ||
        (_playerDash != null && _playerDash.IsDashing) ||
        (_playerBomb != null && _playerBomb.IsActive);

    void Awake()
    {
        _receiver = GetComponent<BulletReceiver>();
        _playerDash = GetComponent<PlayerDash>();
        _playerBomb = GetComponent<PlayerBomb>();
        _bulletTimeManager = GetComponent<BulletTimeManager>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
    }

    void Start()
    {
        if (_model == null)
            Debug.LogError("PlayerHitManager: DI 실패");
    }

    void OnEnable()
    {
        if (_receiver != null)
            _receiver.OnHitByBullet.AddListener(HandleBulletHit);
    }

    void OnDisable()
    {
        if (_receiver != null)
            _receiver.OnHitByBullet.RemoveListener(HandleBulletHit);

        CancelBlink();
    }

    public void TakeDamage(float dmg)
    {
        if (_model.CurrentHP.Value <= 0) return;
        if (IsInvincible) return;

        _model.TakeDamage(dmg);
        PlayHitBlinkAsync().Forget();

        if (_model.CurrentHP.Value <= 0)
            OnDead();
    }

    public void HandleBulletHit(Bullet bullet, Vector3 hitPoint)
    {
        if (_model.CurrentHP.Value <= 0) return;

        // 피격 무적 중 - 무시만, graze 아님
        if (_isPostHitInvincible) return;

        // 대시/봄 무적 중 - graze 발동
        bool isDashBombInvincible =
            (_playerDash != null && _playerDash.IsDashing) ||
            (_playerBomb != null && _playerBomb.IsActive);

        if (isDashBombInvincible)
        {
            if (_bulletTimeManager != null)
                _bulletTimeManager.TriggerSlowMotion();
            if (_model != null)
                _model.Dash.RefillAllCharges();
            _audio?.PlayOneShot(GamePlay.FMODEvents.SFX.Player.Graze, transform.position);
            return;
        }

        float damage = 1f;
        if (bullet != null && bullet.dynamicSolver != null)
        {
            damage = bullet.moduleParameters.GetFloat(BPParams.Damage);
            if (damage <= 0f) damage = 1f;
        }

        _model.TakeDamage(damage);
        PlayHitBlinkAsync().Forget();

        bullet.Die();

        if (_model.CurrentHP.Value <= 0)
            OnDead();
    }

    // 시각 비활성화만 담당, 씬 전환은 RunEndManager가 처리
    private void OnDead()
    {
        CancelBlink();
        gameObject.SetActive(false);
    }

    private async UniTaskVoid PlayHitBlinkAsync()
    {
        Debug.Log($"blink: sr={_spriteRenderer != null}, duration={_model.PostHitInvincibleDuration}");


        if (_spriteRenderer == null) return;

        CancelBlink();
        _blinkCts = new CancellationTokenSource();
        var token = _blinkCts.Token;

        _isPostHitInvincible = true;

        float elapsed = 0f;
        float duration = _model.PostHitInvincibleDuration;
        bool dim = true;

        try
        {
            while (elapsed < duration)
            {
                float alpha = dim ? 0.3f : 0.7f;
                SetAlpha(alpha);
                dim = !dim;

                await UniTask.Delay(80, cancellationToken: token);
                elapsed += 0.08f;
            }
        }
        catch (System.OperationCanceledException) { }
        finally
        {
            _isPostHitInvincible = false;
            SetAlpha(1f);
        }
    }

    private void SetAlpha(float alpha)
    {
        if (_spriteRenderer == null) return;
        Color c = _originalColor;
        c.a = alpha;
        _spriteRenderer.color = c;
    }

    private void CancelBlink()
    {
        if (_blinkCts != null)
        {
            _blinkCts.Cancel();
            _blinkCts.Dispose();
            _blinkCts = null;
        }

        _isPostHitInvincible = false;

        if (_spriteRenderer != null)
            SetAlpha(1f);
    }
}
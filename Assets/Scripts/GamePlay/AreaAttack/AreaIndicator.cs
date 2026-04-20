// Assets/Scripts/GamePlay/AreaAttack/AreaIndicator.cs
// 2026-04-20 장판 인디케이터 본체 구현
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using FMOD.Studio;
using TJR.Core.Interface;
using Reflex.Attributes;

public class AreaIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private CircleCollider2D _circleCollider;
    [SerializeField] private BoxCollider2D _boxCollider;

    [Inject] private IAudioService _audio;

    private AreaAttackConfigSO _config;
    private Transform _target;
    private Transform _owner;
    private AreaIndicatorPool _pool;

    private MaterialPropertyBlock _mpb;
    private CancellationTokenSource _lifecycleCts;
    private bool _isReleased;
    private float _lastHitTime = -999f;

    private EventInstance _warningLoopInstance;
    private bool _warningLoopPlaying;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
    }

    // 풀에서 꺼낸 뒤 호출
    public void Activate(AreaAttackConfigSO config, Vector3 position, float angleDeg,
                         Transform target, Transform owner, AreaIndicatorPool pool)
    {
        _config = config;
        _target = target;
        _owner = owner;
        _pool = pool;
        _isReleased = false;
        _lastHitTime = -999f;
        _warningLoopPlaying = false;

        transform.position = position;
        transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

        _lifecycleCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

        RunLifecycleAsync(_lifecycleCts.Token).Forget();
    }

    // 강제 중단
    public void ForceRelease()
    {
        if (_isReleased) return;

        _lifecycleCts?.Cancel();
        StopWarningLoop();
        ReleaseToPool();
    }

    private async UniTaskVoid RunLifecycleAsync(CancellationToken token)
    {
        try
        {
            SetupCollider();
            SetupVisual();

            DisableCollider();

            // Warning 단계
            await RunWarningPhase(token);

            if (token.IsCancellationRequested) return;

            // Activate 단계
            await RunActivatePhase(token);

            if (token.IsCancellationRequested) return;

            // Linger 단계
            if (_config.LingerDuration > 0f)
            {
                await RunLingerPhase(token);
            }

            if (token.IsCancellationRequested) return;

            // Destroy 단계
            await RunDestroyPhase(token);
        }
        catch (System.OperationCanceledException) { }
        finally
        {
            StopWarningLoop();
            DisableCollider();

            if (!_isReleased)
                ReleaseToPool();
        }
    }

    // Warning: 경고 비주얼 + 방향 추적
    private async UniTask RunWarningPhase(CancellationToken token)
    {
        _spriteRenderer.enabled = true;
        _spriteRenderer.sprite = _config.WarningSprite;

        // 경고 시작 SE
        if (!string.IsNullOrEmpty(_config.WarningStartSFX))
            _audio?.PlayOneShot(_config.WarningStartSFX, transform.position);

        // 경고 루프 SE
        if (!string.IsNullOrEmpty(_config.WarningLoopSFX))
        {
            _warningLoopInstance = _audio != null
                ? _audio.CreateInstance(_config.WarningLoopSFX)
                : default;

            if (_warningLoopInstance.isValid())
            {
                _warningLoopInstance.start();
                _warningLoopPlaying = true;
            }
        }

        float elapsed = 0f;
        float duration = _config.WarningDuration;

        while (elapsed < duration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);

            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(0f, _config.WarningColor.a, Mathf.Clamp01(elapsed / _config.FadeInDuration));

            _mpb.SetFloat("_Progress", progress);
            _mpb.SetColor("_Color", _config.WarningColor);
            _mpb.SetFloat("_Alpha", alpha);
            _mpb.SetFloat("_PulseSpeed", _config.PulseSpeed);
            _spriteRenderer.SetPropertyBlock(_mpb);

            // 방향 추적 (LockDirectionOnWarning=false 시 매 프레임 갱신)
            if (!_config.LockDirectionOnWarning)
                UpdateRotation();
        }

        StopWarningLoop();
    }

    // Activate: 콜라이더 활성 + 플래시 비주얼
    private async UniTask RunActivatePhase(CancellationToken token)
    {
        _spriteRenderer.sprite = _config.ActivateSprite != null
            ? _config.ActivateSprite
            : _config.WarningSprite;

        _mpb.SetColor("_Color", _config.ActivateColor);
        _mpb.SetFloat("_Alpha", 1f);
        _mpb.SetFloat("_Progress", 1f);
        _mpb.SetFloat("_PulseSpeed", 0f);
        _spriteRenderer.SetPropertyBlock(_mpb);

        EnableCollider();

        if (!string.IsNullOrEmpty(_config.ActivateSFX))
            _audio?.PlayOneShot(_config.ActivateSFX, transform.position);

        // 물리 판정 보장을 위해 1 FixedUpdate 대기
        await UniTask.WaitForFixedUpdate(token);
    }

    // Linger: 콜라이더 유지 + 지속 판정 (OnTriggerStay2D에서 처리)
    private async UniTask RunLingerPhase(CancellationToken token)
    {
        float elapsed = 0f;
        float duration = _config.LingerDuration;

        while (elapsed < duration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            elapsed += Time.deltaTime;
        }
    }

    // Destroy: 페이드아웃 후 풀 반환
    private async UniTask RunDestroyPhase(CancellationToken token)
    {
        DisableCollider();

        float elapsed = 0f;
        float duration = _config.FadeOutDuration;
        float startAlpha = _config.ActivateColor.a;

        while (elapsed < duration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(elapsed / duration));
            _mpb.SetFloat("_Alpha", alpha);
            _spriteRenderer.SetPropertyBlock(_mpb);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHitManager hit = other.GetComponent<PlayerHitManager>();
        if (hit == null) return;

        if (Time.time - _lastHitTime < _config.HitInterval) return;

        if (hit.TryGraze())
        {
            _lastHitTime = Time.time;
            return;
        }

        hit.TakeDamage(_config.Damage);
        _lastHitTime = Time.time;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!_config.ContinuousDamage) return;
        if (!other.CompareTag("Player")) return;

        PlayerHitManager hit = other.GetComponent<PlayerHitManager>();
        if (hit == null) return;

        if (Time.time - _lastHitTime < _config.HitInterval) return;

        if (hit.TryGraze())
        {
            _lastHitTime = Time.time;
            return;
        }

        hit.TakeDamage(_config.Damage);
        _lastHitTime = Time.time;
    }

    private void SetupCollider()
    {
        if (_config.Shape == AreaShape.Circle)
        {
            if (_circleCollider != null)
                _circleCollider.radius = _config.Size.x;

            if (_boxCollider != null)
                _boxCollider.enabled = false;
        }
        else
        {
            if (_boxCollider != null)
                _boxCollider.size = _config.Size;

            if (_circleCollider != null)
                _circleCollider.enabled = false;
        }
    }

    private void SetupVisual()
    {
        _spriteRenderer.sprite = _config.WarningSprite;
        _spriteRenderer.enabled = true;

        _mpb.SetColor("_Color", _config.WarningColor);
        _mpb.SetFloat("_Alpha", 0f);
        _mpb.SetFloat("_Progress", 0f);
        _mpb.SetFloat("_PulseSpeed", _config.PulseSpeed);
        _spriteRenderer.SetPropertyBlock(_mpb);
    }

    private void EnableCollider()
    {
        if (_config.Shape == AreaShape.Circle && _circleCollider != null)
            _circleCollider.enabled = true;
        else if (_config.Shape == AreaShape.Rect && _boxCollider != null)
            _boxCollider.enabled = true;
    }

    private void DisableCollider()
    {
        if (_circleCollider != null) _circleCollider.enabled = false;
        if (_boxCollider != null) _boxCollider.enabled = false;
    }

    private void UpdateRotation()
    {
        if (_config.Direction == AreaDirection.None) return;

        float angle = ResolveAngle();
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private float ResolveAngle()
    {
        switch (_config.Direction)
        {
            case AreaDirection.TowardTarget:
                if (_target == null) return 0f;
                Vector2 dir = _target.position - transform.position;
                return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            case AreaDirection.FixedAngle:
                return _config.FixedAngleDeg;

            case AreaDirection.OwnerForward:
                if (_owner == null) return 0f;
                return Mathf.Atan2(_owner.right.y, _owner.right.x) * Mathf.Rad2Deg;

            default:
                return 0f;
        }
    }

    private void StopWarningLoop()
    {
        if (!_warningLoopPlaying) return;

        if (_warningLoopInstance.isValid())
        {
            _warningLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _warningLoopInstance.release();
            _warningLoopInstance = default;
        }

        _warningLoopPlaying = false;
    }

    private void ReleaseToPool()
    {
        if (_isReleased) return;
        _isReleased = true;

        _spriteRenderer.enabled = false;
        DisableCollider();

        _lifecycleCts?.Dispose();
        _lifecycleCts = null;

        _pool?.Release(this);
    }
}

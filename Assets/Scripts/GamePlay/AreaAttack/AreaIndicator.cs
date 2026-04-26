// Assets/Scripts/GamePlay/AreaAttack/AreaIndicator.cs
// 2026-04-26
// Rect 형태 제거. Circle 전용으로 단순화. 방향 추적 로직 폐기

using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using FMODUnity;

public class AreaIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private CircleCollider2D _circleCollider;

    private AreaAttackConfigSO _config;
    private Transform _target;
    private Transform _owner;

    private MaterialPropertyBlock _mpb;
    private bool _isReleased;
    private float _lastHitTime;

    // FMOD 루프 인스턴스
    private FMOD.Studio.EventInstance _warningLoopInstance;
    private bool _hasWarningLoop;

    // 강제 종료용
    private CancellationTokenSource _lifecycleCts;

    // 풀 참조 (Activate 시점에 저장)
    private AreaIndicatorPool _pool;

    public void Activate(AreaAttackConfigSO config, Vector3 position,
                         Transform target, Transform owner, AreaIndicatorPool pool)
    {
        _config = config;
        _target = target;
        _owner = owner;
        _pool = pool;
        _isReleased = false;
        _lastHitTime = -999f;

        transform.position = position;
        transform.rotation = Quaternion.identity;

        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();

        _lifecycleCts?.Cancel();
        _lifecycleCts?.Dispose();
        _lifecycleCts = new CancellationTokenSource();

        var linked = CancellationTokenSource.CreateLinkedTokenSource(
            _lifecycleCts.Token, destroyCancellationToken);

        RunLifecycleAsync(linked.Token).Forget();
    }

    public void ForceRelease()
    {
        if (_isReleased) return;

        _lifecycleCts?.Cancel();
        _lifecycleCts?.Dispose();
        _lifecycleCts = null;

        StopWarningLoop();
        DisableCollider();
        DoRelease();
    }

    private async UniTaskVoid RunLifecycleAsync(CancellationToken token)
    {
        try
        {
            SetupCollider();
            SetupVisual();

            // -- Warning --
            await WarningPhaseAsync(token);

            // -- Activate --
            await ActivatePhaseAsync(token);

            // -- Linger --
            if (_config.LingerDuration > 0f)
            {
                await LingerPhaseAsync(token);
            }

            // -- Destroy --
            await DestroyPhaseAsync(token);
        }
        catch (System.OperationCanceledException)
        {
            // ForceRelease 또는 오브젝트 파괴
        }
        finally
        {
            StopWarningLoop();
            DisableCollider();
            DoRelease();
        }
    }

    // --- Warning ---
    private async UniTask WarningPhaseAsync(CancellationToken token)
    {
        DisableCollider();

        _spriteRenderer.sprite = _config.WarningSprite;
        _spriteRenderer.enabled = true;

        // 경고 시작 SE
        if (!string.IsNullOrEmpty(_config.WarningStartSFX))
            RuntimeManager.PlayOneShot(_config.WarningStartSFX, transform.position);

        // 경고 루프 SE
        StartWarningLoop();

        float elapsed = 0f;
        float duration = _config.WarningDuration;
        float fadeIn = _config.FadeInDuration;

        while (elapsed < duration)
        {
            token.ThrowIfCancellationRequested();

            float progress = elapsed / duration;
            float alpha = fadeIn > 0f ? Mathf.Clamp01(elapsed / fadeIn) : 1f;

            // 셰이더 갱신
            _mpb.SetFloat("_Progress", progress);
            _mpb.SetFloat("_Alpha", alpha);
            _mpb.SetFloat("_PulseSpeed", _config.PulseSpeed);
            _mpb.SetColor("_Color", _config.WarningColor);
            _spriteRenderer.SetPropertyBlock(_mpb);

            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    // --- Activate ---
    private async UniTask ActivatePhaseAsync(CancellationToken token)
    {
        StopWarningLoop();

        // 발동 SE
        if (!string.IsNullOrEmpty(_config.ActivateSFX))
            RuntimeManager.PlayOneShot(_config.ActivateSFX, transform.position);

        // 비주얼 플래시
        if (_config.ActivateSprite != null)
            _spriteRenderer.sprite = _config.ActivateSprite;

        _mpb.SetFloat("_Progress", 1f);
        _mpb.SetFloat("_Alpha", 1f);
        _mpb.SetFloat("_PulseSpeed", 0f);
        _mpb.SetColor("_Color", _config.ActivateColor);
        _spriteRenderer.SetPropertyBlock(_mpb);

        // 콜라이더 활성화
        EnableCollider();

        // 첫 판정 즉시 통과 보장
        _lastHitTime = -999f;
        await UniTask.WaitForFixedUpdate(token);
    }

    // --- Linger ---
    private async UniTask LingerPhaseAsync(CancellationToken token)
    {
        float elapsed = 0f;

        while (elapsed < _config.LingerDuration)
        {
            token.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    // --- Destroy ---
    private async UniTask DestroyPhaseAsync(CancellationToken token)
    {
        DisableCollider();

        float elapsed = 0f;
        float fadeOut = _config.FadeOutDuration;

        while (elapsed < fadeOut)
        {
            token.ThrowIfCancellationRequested();

            float alpha = 1f - Mathf.Clamp01(elapsed / fadeOut);
            _mpb.SetFloat("_Alpha", alpha);
            _spriteRenderer.SetPropertyBlock(_mpb);

            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    // --- 트리거 ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var hit = other.GetComponent<PlayerHitManager>();
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

        var hit = other.GetComponent<PlayerHitManager>();
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

    // --- 콜라이더 ---
    private void SetupCollider()
    {
        _circleCollider.enabled = false;
        _circleCollider.radius = _config.Radius;
        _circleCollider.offset = Vector2.zero;
    }

    private void EnableCollider()
    {
        _circleCollider.enabled = true;
    }

    private void DisableCollider()
    {
        _circleCollider.enabled = false;
    }

    // --- 비주얼 ---
    private void SetupVisual()
    {
        _spriteRenderer.enabled = true;
        _spriteRenderer.sprite = _config.WarningSprite;

        _mpb.SetFloat("_Progress", 0f);
        _mpb.SetFloat("_Alpha", 0f);
        _mpb.SetFloat("_PulseSpeed", _config.PulseSpeed);
        _mpb.SetColor("_Color", _config.WarningColor);
        _spriteRenderer.SetPropertyBlock(_mpb);

        // Circle 스케일
        float diameter = _config.Radius * 2f;
        transform.localScale = new Vector3(diameter, diameter, 1f);
    }

    // --- 사운드 ---
    private void StartWarningLoop()
    {
        if (string.IsNullOrEmpty(_config.WarningLoopSFX)) return;

        _warningLoopInstance = RuntimeManager.CreateInstance(_config.WarningLoopSFX);
        _warningLoopInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        _warningLoopInstance.start();
        _hasWarningLoop = true;
    }

    private void StopWarningLoop()
    {
        if (!_hasWarningLoop) return;

        _warningLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _warningLoopInstance.release();
        _hasWarningLoop = false;
    }

    // --- 풀 반환 ---
    private void DoRelease()
    {
        if (_isReleased) return;
        _isReleased = true;

        _spriteRenderer.enabled = false;
        transform.localScale = Vector3.one;

        if (_pool != null)
            _pool.Release(this);
    }
}
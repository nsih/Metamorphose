using UnityEngine;
using UnityEngine.InputSystem;
using Reflex.Attributes;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TJR.Core.Interface;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [Inject] private IInputService _input;
    [Inject] private PlayerModel _model;
    [Inject] private IAudioService _audio;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private bool _isDashing = false;
    private CancellationTokenSource _dashCts;

    public bool IsDashing => _isDashing;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        if (_input != null)
            _input.OnDashPressed += HandleDash;
    }

    void OnDestroy()
    {
        if (_input != null) _input.OnDashPressed -= HandleDash;
        CancelDash();
    }

    private void HandleDash()
    {
        if (!_model.TryConsumeDash()) return;

        CancelDash();
        _dashCts = new CancellationTokenSource();
        DashAsync(_dashCts.Token).Forget();
    }

    private void CancelDash()
    {
        if (_dashCts != null)
        {
            _dashCts.Cancel();
            _dashCts.Dispose();
            _dashCts = null;
        }
    }

    private async UniTaskVoid DashAsync(CancellationToken token)
    {
        _isDashing = true;
        SetAlpha(0.3f);

        _audio?.PlayOneShot(GamePlay.FMODEvents.SFX.Player.Dash, transform.position);

        Vector2 moveInput = _input.MoveDirection;
        Vector2 dashDirection;

        if (moveInput.magnitude > 0.1f)
        {
            dashDirection = moveInput.normalized;
        }
        else
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(
                new Vector3(mouseScreen.x, mouseScreen.y, 0f));
            mousePos.z = 0;
            dashDirection = ((Vector2)(mousePos - transform.position)).normalized;
        }

        float elapsed = 0f;

        try
        {
            // 이동 구간
            while (elapsed < _model.DashDuration)
            {
                float delta = Time.fixedUnscaledDeltaTime;
                _rb.MovePosition(_rb.position + dashDirection * _model.DashSpeed * delta);
                elapsed += delta;
                await UniTask.WaitForFixedUpdate(token);
            }

            _rb.linearVelocity = Vector2.zero;
            SetAlpha(1f);

            // 후보정 무적 구간
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(_model.PostDashInvincibleDuration),
                ignoreTimeScale: true,
                cancellationToken: token
            );
        }
        catch (OperationCanceledException)
        {
            SetAlpha(1f);
            _isDashing = false;
            return;
        }

        if (this == null) return;

        _isDashing = false;
    }

    private void SetAlpha(float alpha)
    {
        if (_spriteRenderer == null) return;
        var c = _spriteRenderer.color;
        c.a = alpha;
        _spriteRenderer.color = c;
    }
}
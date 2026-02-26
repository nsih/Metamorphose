using UnityEngine;
using Reflex.Attributes;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [Inject] private IInputService _input;
    [Inject] private PlayerModel _model;

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
        {
            _input.OnDashPressed += HandleDash;
        }
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

        Vector2 moveInput = _input.MoveDirection;
        Vector2 dashDirection;

        if (moveInput.magnitude > 0.1f)
        {
            dashDirection = moveInput.normalized;
        }
        else
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            dashDirection = ((Vector2)(mousePos - transform.position)).normalized;
        }

        float elapsed = 0f;

        try
        {
            while (elapsed < _model.DashDuration)
            {
                float delta = Time.fixedUnscaledDeltaTime;
                _rb.MovePosition(_rb.position + dashDirection * _model.DashSpeed * delta);
                elapsed += delta;
                await UniTask.WaitForFixedUpdate(token);
            }
        }
        catch (OperationCanceledException)
        {
            SetAlpha(1f);
            return;
        }

        if (this == null) return;

        _rb.linearVelocity = Vector2.zero;
        _isDashing = false;
        SetAlpha(1f);
    }


    //애니메이션이 없어서 대쉬하는것 같지가 않아요
    private void SetAlpha(float alpha)
    {
        if (_spriteRenderer == null) return;
        var c = _spriteRenderer.color;
        c.a = alpha;
        _spriteRenderer.color = c;
    }
}
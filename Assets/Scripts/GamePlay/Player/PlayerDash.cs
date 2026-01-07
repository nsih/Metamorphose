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
    private bool _isDashing = false;
    private CancellationTokenSource _dashCts;

    public bool IsDashing => _isDashing;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
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
        
        _rb.linearVelocity = dashDirection * _model.DashSpeed;

        try
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(_model.DashDuration),
                ignoreTimeScale: true,
                cancellationToken: token
            );
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (this == null) return;

        _rb.linearVelocity = Vector2.zero;
        _isDashing = false;
    }
}
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
    private float _defaultGravity;
    private bool _isDashing = false;
    private CancellationTokenSource _dashCts; 

    public bool IsDashing => _isDashing;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _defaultGravity = _rb.gravityScale;
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
        
        // 방향 계산
        float moveInputX = _input.MoveDirection.x; 
        float dashDirection;

        if (Mathf.Abs(moveInputX) > 0.1f)
            dashDirection = Mathf.Sign(moveInputX); 
        else
            dashDirection = transform.localScale.x > 0 ? 1f : -1f; // 입력 없으면 바라보는 방향
        
        _rb.gravityScale = 0f;
        
        _rb.linearVelocity = new Vector2(dashDirection * _model.DashSpeed, 0);

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

        _rb.gravityScale = _defaultGravity;
        _rb.linearVelocity = Vector2.zero;
        _isDashing = false;
    }
}
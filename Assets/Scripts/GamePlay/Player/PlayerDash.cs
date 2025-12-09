using UnityEngine;
using Reflex.Attributes;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    // DI
    [Inject] private IInputService _input;
    [Inject] private PlayerModel _model;

    private Rigidbody2D _rb;
    private float _defaultGravity;
    private bool _isDashing = false;
    
    public bool IsDashing => _isDashing;

    // 비동기 토큰 관리
    private CancellationTokenSource _dashCts; 

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

    void Update()
    {
        _model?.UpdateDashCooldown(Time.deltaTime);
    }

    private void HandleDash()
    {
        // 스택 깍고 이벤트 발생
        if (!_model.TryConsumeDash()) return;

        // 아무튼 대쉬 취소
        CancelDash();

        // 대쉬 시작
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
        float dashDirection;
        float moveInputX = _input.MoveDirection.x; 

        if (Mathf.Abs(moveInputX) > 0.1f)
            dashDirection = Mathf.Sign(moveInputX); 
        else
            dashDirection = transform.localScale.x > 0 ? 1f : -1f; // 입력 없으면 바라보는 방향
        
        // 물리 적용 (데이터는 모델을 통해 가져옵니다: _model.DashSpeed)
        _rb.gravityScale = 0f;
        
        // Unity 6000 이상: linearVelocity / 이전 버전: velocity
        _rb.linearVelocity = new Vector2(dashDirection * _model.DashSpeed, 0);

        try
        {
            // 시간 지연 (모델 데이터 사용: _model.DashDuration)
            await UniTask.Delay(
                TimeSpan.FromSeconds(_model.DashDuration),
                cancellationToken: token
            );
        }
        catch (OperationCanceledException)
        {
            // 취소당했으면(연타 등) 즉시 종료
            return;
        }

        // 대시 종료 처리 (안전장치 추가)
        if (this == null) return; 

        _rb.gravityScale = _defaultGravity;
        _rb.linearVelocity = Vector2.zero;
        _isDashing = false;
    }
}
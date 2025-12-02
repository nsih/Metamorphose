using UnityEngine;
using Reflex.Attributes;
using System;

using System.Threading;
using Cysharp.Threading.Tasks;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [Inject] private IInputService _input;
    [Inject] private PlayerStat _playerStat;




    private Rigidbody2D _rb;

    private float _defaultGravity;
        private bool _isDashing = false;
    
    public bool IsDashing => _isDashing;

    private int _currentDashCharges;   // 현재 충전 횟수
    private float _chargeRegenTimer;   // 충전 타이머


    private CancellationTokenSource _dashCts; //대쉬 캔슬 토큰 소스



    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _defaultGravity = _rb.gravityScale;
    }

    void Start()
    {
        if (_input == null) return;
        _input.OnDashPressed += HandleDash;

        if (_playerStat != null)
        {
            _currentDashCharges = _playerStat.MaxDashChargeStack;
        }
    }

    void OnDestroy()
    {
        if (_input != null) _input.OnDashPressed -= HandleDash;

        _dashCts?.Cancel();
        _dashCts?.Dispose();
    }

    void Update()
    {
        DashCoolDown();
    }



    private void HandleDash()
    {
        // 스택확인
        if (_currentDashCharges <= 0) return;
        // 토큰 확인
        if (_isDashing && _dashCts != null)
        {
            _dashCts.Cancel();  // 야, 하던 거 멈춰.
            _dashCts.Dispose(); // 다 쓴 리모컨 버리기.
        }

        _currentDashCharges--;
        Debug.Log($"DashStack: {_currentDashCharges}");

        // [변경 5] 새 대시를 위한 새 리모컨(토큰) 발급
        _dashCts = new CancellationTokenSource();
        
        // 토큰을 들려 보낸다.
        DashAsync(_dashCts.Token).Forget();
    }
    
    private async UniTaskVoid DashAsync(CancellationToken token)
    {
        _isDashing = true;
        
        float dashDirection;
        
        float moveInputX = _input.MoveDirection.x; 

        if (Mathf.Abs(moveInputX) > 0.1f)
        {
            dashDirection = Mathf.Sign(moveInputX); 
        }
        else
        {
            dashDirection = transform.localScale.x > 0 ? 1f : -1f;
        }
        
        _rb.gravityScale = 0f;
        _rb.linearVelocity = new Vector2(dashDirection * _playerStat.DashSpeed, 0);


        //
        try
        {
            // 딜레이 중에 취소 토큰(token)을 감시함
            await UniTask.Delay(
                TimeSpan.FromSeconds(_playerStat.DashDuration),
                cancellationToken: token
            );
        }
        catch (OperationCanceledException)
        {
            // 취소당했으면 여기로 옴.
            return;
        }
        //

        if (this == null) return; 

        _rb.gravityScale = _defaultGravity;
        _rb.linearVelocity = Vector2.zero;
        _isDashing = false;
    }
    
    void DashCoolDown()
    {
        if (_playerStat != null && _currentDashCharges < _playerStat.MaxDashChargeStack)
        {
            _chargeRegenTimer += Time.deltaTime;

            if (_chargeRegenTimer >= _playerStat.DashChargeTime)
            {
                _currentDashCharges++;
                _chargeRegenTimer = 0f;
                Debug.Log($"DashStack: {_currentDashCharges}");
            }
        }
    }
}

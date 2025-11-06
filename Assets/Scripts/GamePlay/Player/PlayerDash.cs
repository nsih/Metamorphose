using UnityEngine;
using Reflex.Attributes;
using System;
using Cysharp.Threading.Tasks;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [Inject] private IInputService _input;
    [Inject] private PlayerStat _playerStat;
    



    private Rigidbody2D _rb;
    private bool _isDashing = false;
    
    public bool IsDashing => _isDashing;

    private int _currentDashCharges;   // 현재 충전 횟수
    private float _chargeRegenTimer;   // 충전 타이머



    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
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
    }

    void Update()
    {
        DashCoolDown();
    }



    private void HandleDash()
    {
        if (_currentDashCharges <= 0 || _isDashing) return;

        _currentDashCharges--;
        Debug.Log($"DashStack: {_currentDashCharges}");

        DashAsync().Forget();
    }

    private async UniTaskVoid DashAsync()
    {
        _isDashing = true;

        float dashDirection = transform.localScale.x > 0 ? 1 : -1;

        float originalGravity = _rb.gravityScale;
        _rb.gravityScale = 0f;
        _rb.linearVelocity = new Vector2(dashDirection * _playerStat.DashSpeed, 0);

        // 대시 지속 시간만큼 대기
        await UniTask.Delay(
            TimeSpan.FromSeconds(_playerStat.DashDuration),
            cancellationToken: this.GetCancellationTokenOnDestroy()
        );

        if (this == null) return;

        _rb.gravityScale = originalGravity;
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

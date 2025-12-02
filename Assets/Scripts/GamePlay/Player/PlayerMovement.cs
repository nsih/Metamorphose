using UnityEngine;
using Reflex.Attributes;

[RequireComponent(typeof(Rigidbody2D))] // [!!] Rigidbody2D 요구 추가
public class PlayerMovement : MonoBehaviour
{
    [Inject]
    private IInputService iInputService;

    [Inject]
    private PlayerStat playerStat;

    private Rigidbody2D _rb;
    private PlayerDash _playerDash;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerDash = GetComponent<PlayerDash>();
    }

    void Start()
    {
        // [!!] 의존성 주입 확인 (PlayerDash와 동일한 가드 절 패턴)
        if (iInputService == null || playerStat == null)
        {
            Debug.LogError("PlayerMovement: 의존성 주입 실패!");
        }
    }

    void FixedUpdate()
    {
        // 1. 대시 중일 때는 이동 및 방향 전환을 모두 막음
        if (_playerDash != null && _playerDash.IsDashing)
        {
            return;
        }
        
        // 2. 의존성이 주입되었는지 확인
        if (iInputService != null && playerStat != null)
        {
            float horizontalInput = iInputService.MoveDirection.x;
            
            // 3. 이동 속도 설정
            _rb.linearVelocity = new Vector2
            (
                horizontalInput * playerStat.MoveSpeed,
                _rb.linearVelocity.y 
            );

            // 4. [!!] 방향 전환(Flipping) 로직 추가
            // 입력이 있을 때만 방향을 바꿈 (0일 때는 마지막 방향 유지)
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // 현재 스케일 값을 가져옴
                Vector3 newScale = transform.localScale;
                
                // Mathf.Sign()은 입력이 +면 1, -면 -1을 반환함
                newScale.x = Mathf.Sign(horizontalInput); 
                
                // 새 스케일 값을 적용
                transform.localScale = newScale;
            }
        }
    }
}
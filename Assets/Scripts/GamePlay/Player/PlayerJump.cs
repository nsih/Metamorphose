using UnityEngine;
using Reflex.Attributes;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerJump : MonoBehaviour
{
    [Inject] private IInputService _input;
    
    // [변경 1] PlayerStat 대신 PlayerModel 주입
    [Inject] private PlayerModel _model; 

    private Rigidbody2D _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (_input == null)
        {
            Debug.LogError("PlayerJump: DI error");
            return;
        }

        _input.OnJumpPressed += HandleJump;
    }

    void OnDestroy()
    {
        if (_input != null)
        {
            _input.OnJumpPressed -= HandleJump;
        }
    }

    private void HandleJump()
    {
        // TODO: Ground Check 추가 필요
        
        if (_model != null)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0); 
            
            // 파사드
            _rb.AddForce(Vector2.up * _model.JumpForce, ForceMode2D.Impulse);
        }
    }
}
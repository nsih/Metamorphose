using UnityEngine;
using Reflex.Attributes;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerJump : MonoBehaviour
{
    [Inject] private IInputService _input;
    [Inject] private PlayerStat _playerStat;

    private Rigidbody2D _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    #region event subscribe
    private void OnEnable()
    {
        if (_input != null) 
        {
            _input.OnJumpPressed += HandleJump;
        }
    }
    private void OnDisable()
    {
        if (_input != null)
        {
            _input.OnJumpPressed -= HandleJump;
        }
    }        
    #endregion

    void Start()
    {
        if (_input == null)
            Debug.LogError("IInputService injection error");
        else
        {
            _input.OnJumpPressed -= HandleJump;
            _input.OnJumpPressed += HandleJump;
        }
    }

    private void HandleJump()
    {
        // ground check 추가 필요
        if (_playerStat != null)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0); 
            _rb.AddForce(Vector2.up * _playerStat.JumpForce, ForceMode2D.Impulse);
        }
    }
}
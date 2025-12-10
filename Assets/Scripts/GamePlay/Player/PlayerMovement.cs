using UnityEngine;
using Reflex.Attributes;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Inject]
    private IInputService _input;

    [Inject]
    private PlayerModel _model; 

    private Rigidbody2D _rb;
    private PlayerDash _playerDash;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerDash = GetComponent<PlayerDash>();
    }

    void Start()
    {
        if (_input == null || _model == null)
        {
            Debug.LogError("PlayerMovement: DI error");
        }
    }

    void FixedUpdate()
    {
        if (_playerDash != null && _playerDash.IsDashing)
        {
            return;
        }
        
        if (_input != null && _model != null)
        {
            float horizontalInput = _input.MoveDirection.x;
            
            // 파사드 패턴
            _rb.linearVelocity = new Vector2
            (
                horizontalInput * _model.MoveSpeed,
                _rb.linearVelocity.y 
            );
            /*
            // 방향전환
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                Vector3 newScale = transform.localScale;
                newScale.x = Mathf.Sign(horizontalInput); 
                transform.localScale = newScale;
            }
            */
        }
    }
}
using UnityEngine;
using Reflex.Attributes;


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
        if (iInputService == null)
        {
            Debug.LogError("IInputService error");
        }

        if (playerStat == null)
        {
            Debug.LogError("PlayerStats error");
        }
    }

    void FixedUpdate()
    {
        if (_playerDash != null && _playerDash.IsDashing)
        {
            return;
        }
        
        if (iInputService != null && playerStat != null)
        {
            float horizontalInput = iInputService.MoveDirection.x;
            
            _rb.linearVelocity = new Vector2
            (
                horizontalInput * playerStat.MoveSpeed,
                _rb.linearVelocity.y 
            );
        }
    }
}
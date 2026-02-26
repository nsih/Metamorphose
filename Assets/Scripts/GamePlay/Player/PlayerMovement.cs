using UnityEngine;
using Reflex.Attributes;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Inject] private IInputService _input;
    [Inject] private PlayerModel _model;

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
        if (_playerDash != null && _playerDash.IsDashing) return;
        if (_input == null || _model == null) return;

        Vector2 delta = _input.MoveDirection.normalized * _model.MoveSpeed * Time.fixedUnscaledDeltaTime;
        _rb.MovePosition(_rb.position + delta);
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputService : IInputService, IDisposable
{
    private readonly PlayerControls _controls;

    public Vector2 MoveDirection => _controls.Player.Move.ReadValue<Vector2>();
    public bool IsAttackPressed => _controls.Player.Attack.IsPressed();

    public event Action OnDashPressed;
    public event Action OnBombPressed;

    private void OnBombPerformed(InputAction.CallbackContext ctx)
    {
        OnBombPressed?.Invoke();
    }

    public PlayerInputService()
    {
        _controls = new PlayerControls();
        _controls.Enable();

        _controls.Player.Bomb.performed += OnBombPerformed;
        _controls.Player.Dash.performed += OnDash;
    }

    public void Dispose()
    {
        _controls.Disable();
        
        _controls.Player.Bomb.performed -= OnBombPerformed;
        _controls.Player.Dash.performed -= OnDash;
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        OnDashPressed?.Invoke();
    }
}
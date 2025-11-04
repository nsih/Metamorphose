using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputService : IInputService, IDisposable
{
    private readonly PlayerControls _controls;

    public Vector2 MoveDirection => _controls.Player.Move.ReadValue<Vector2>();

    public event Action OnJumpPressed;

    public event Action OnDashPressed;

    public PlayerInputService()
    {
        _controls = new PlayerControls();
        _controls.Enable();


        _controls.Player.Jump.performed += OnJump;
        _controls.Player.Dash.performed += OnDash;
    }

    public void Dispose()
    {
        _controls.Disable();
        _controls.Player.Jump.performed -= OnJump;
        _controls.Player.Dash.performed -= OnDash;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        OnJumpPressed?.Invoke();
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        OnDashPressed?.Invoke();
    }
}
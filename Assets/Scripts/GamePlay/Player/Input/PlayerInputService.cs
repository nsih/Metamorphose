using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputService : IInputService, IDisposable
{
    private readonly PlayerControls _controls;

    public Vector2 MoveDirection => _controls.Player.Move.ReadValue<Vector2>();

    public event Action OnJumpPressed;

    public PlayerInputService()
    {
        _controls = new PlayerControls();
        _controls.Enable();

        
        _controls.Player.Jump.performed += OnJump;
    }

    public void Dispose()
    {
        _controls.Player.Jump.performed -= OnJump;
        _controls.Disable();
    }
    
    private void OnJump(InputAction.CallbackContext context)
    {
        OnJumpPressed?.Invoke();
    }
}
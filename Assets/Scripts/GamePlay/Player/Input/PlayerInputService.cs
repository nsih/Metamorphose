// Assets/Scripts/GamePlay/Player/Input/PlayerInputService.cs
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputService : IInputService, IDisposable
{
    private readonly PlayerControls _controls;

    public bool IsEnabled { get; private set; } = true;

    public Vector2 MoveDirection => IsEnabled ? _controls.Player.Move.ReadValue<Vector2>() : Vector2.zero;
    public bool IsAttackPressed => IsEnabled && _controls.Player.Attack.IsPressed();

    public event Action OnDashPressed;
    public event Action OnBombPressed;
    public event Action OnInteractPressed;

    public PlayerInputService()
    {
        _controls = new PlayerControls();
        _controls.Enable();


        _controls.Player.Dash.performed += OnDash;
        _controls.Player.Interact.performed += OnInteract;
    }

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
    }

    private void OnBombPerformed(InputAction.CallbackContext ctx)
    {
        if (!IsEnabled) return;
        OnBombPressed?.Invoke();
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (!IsEnabled) return;
        OnDashPressed?.Invoke();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!IsEnabled) return;
        OnInteractPressed?.Invoke();
    }

    public void Dispose()
    {
        _controls.Disable();
        _controls.Player.Dash.performed -= OnDash;
        _controls.Player.Interact.performed -= OnInteract;
    }
}
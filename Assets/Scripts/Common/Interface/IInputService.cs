using UnityEngine;
using System;

public interface IInputService
{
    Vector2 MoveDirection { get; }
    bool IsAttackPressed { get; }
    bool IsEnabled { get; }
    void SetEnabled(bool enabled);
    event Action OnDashPressed;
    event Action OnBombPressed;
}
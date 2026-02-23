using UnityEngine;
using System;

public interface IInputService
{
    event Action OnDashPressed;
    event Action OnBombPressed;

    
    Vector2 MoveDirection { get; }
    
    bool IsAttackPressed { get; }
}
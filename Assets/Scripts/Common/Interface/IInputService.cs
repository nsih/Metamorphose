using UnityEngine;
using System;

public interface IInputService
{
    Vector2 MoveDirection { get; }
    
    bool IsAttackPressed { get; }

    event Action OnDashPressed;
}
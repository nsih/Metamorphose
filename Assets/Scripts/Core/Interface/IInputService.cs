using UnityEngine;
using System;

public interface IInputService
{
    Vector2 MoveDirection { get; }

    event Action OnJumpPressed;
    //bool IsAttackPressed { get; }
}
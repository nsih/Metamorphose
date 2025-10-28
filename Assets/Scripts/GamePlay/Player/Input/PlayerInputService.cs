using UnityEngine;
//using Core;

public class PlayerInputService : IInputService
{
    public Vector2 MoveDirection
    {
        get
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            return new Vector2(horizontal, vertical);
        }
    }

    // public bool IsJumpPressed
    // {
    //     get { return Input.GetButtonDown("Jump"); }
    // }

    // public bool IsAttackPressed
    // {
    //     get { return Input.GetButtonDown("Fire1"); }
    // }
}
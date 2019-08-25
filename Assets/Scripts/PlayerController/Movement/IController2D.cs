using UnityEngine;

public interface IController2D
{
    bool IsGrounded();

    Vector2 Move(Vector2 moveDistance, Vector2 input);
}

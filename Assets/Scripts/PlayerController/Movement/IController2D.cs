using UnityEngine;
/// <summary>
/// Implemented so that we can loosely couple the player controller logic by referring to this interface. 
/// </summary>
public interface IController2D
{
    bool IsGrounded();

    Vector2 Move(Vector2 moveDistance, Vector2 input);

    void SetPlatformMoveDistance(Vector2 moveDistance);
}

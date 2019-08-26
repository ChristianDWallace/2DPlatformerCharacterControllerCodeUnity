using UnityEngine;

/// <summary>
/// Implemented in case we ever want to change the way we handle collision detection, as our collision detection script inherits from this interface, and therefore can be loosely coupled. 
/// </summary>
public interface ICollisionDetection
{
    bool IsGrounded();
    Vector2 CollisionHandling(Vector2 moveDistance, Vector2 input);
}



using UnityEngine;

public interface ICollisionDetection
{
    bool IsGrounded();
    Vector2 CollisionHandling(Vector2 moveDistance, Vector2 input);
}



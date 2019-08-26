using UnityEngine;

[RequireComponent(typeof(CollisionDetection))]
public partial class Controller2D : MonoBehaviour, IController2D
{
    private ICollisionDetection _collisions;

    private bool _isPlatformed;

    private Vector2 _platformMoveDistance; 

    // Start is called before the first frame update
    void Start()
    {
        _collisions = GetComponent<CollisionDetection>();
    }

    public bool IsGrounded()
    {
        return _collisions.IsGrounded();
    }

    /// <summary>
    /// The 
    /// </summary>
    /// <param name="moveDistance"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public Vector2 Move(Vector2 moveDistance, Vector2 input)
    {
        if (_isPlatformed)
        {
            PlatformMove(_platformMoveDistance); 
        }

        moveDistance = _collisions.CollisionHandling(moveDistance, input);

        transform.Translate(moveDistance);

        return moveDistance;
    }

    /// <summary>
    /// Called from platforms that our pushing our character around. 
    /// </summary>
    /// <param name="moveDistance"></param>
    public void SetPlatformMoveDistance(Vector2 moveDistance)
    {
        _platformMoveDistance = moveDistance;
        _isPlatformed = true; 
    }

    /// <summary>
    /// Move according to the platform move distance we recieved from the platform that is pushing us. 
    /// </summary>
    /// <param name="moveDistance"></param>
    /// <returns></returns>
    private Vector2 PlatformMove(Vector2 moveDistance)
    {
        if (_isPlatformed && _platformMoveDistance != new Vector2(0f, 0f))
        {
            _platformMoveDistance = _collisions.CollisionHandling(_platformMoveDistance, Vector2.zero);
            _isPlatformed = false;
            transform.Translate(_platformMoveDistance);
            _platformMoveDistance = Vector2.zero;
        }

        return moveDistance;
    }
}

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RaycastController))]
public class PlatformController : MonoBehaviour
{
    /// <summary>
    /// Public Reference Types
    /// </summary>
    [Header("Collisions")]
    [SerializeField]
    private LayerMask _collisionMask;

    [Header("Movement Values")]
    [SerializeField]
    private Vector3[] _myLocalWaypoints;

    /// <summary>
    /// Public Value Types
    /// </summary>
    [SerializeField] private float _speed = 0f;

    [SerializeField] private float _waitTime = 0.5f;

    [SerializeField] private bool _isCyclic = false;

    [Range(0, 2)] [SerializeField] private float _easing = 1f;

    /// <summary>
    /// Private Reference Types
    /// </summary>
    private Vector3[] _globalWaypoints;

    private Vector3 _smoothing;

    private RaycastController _controller;

    private Dictionary<Transform, Controller2D> _passengerDictionary = new Dictionary<Transform, Controller2D>();

    private HashSet<PassengerMovement> _movedPassengers = new HashSet<PassengerMovement>();

    /// <summary>
    /// Private Value Types
    /// </summary>
    private float _nextMoveTime;

    private int _fromWaypointIndex;

    private float _percentBetweenWaypoints;

    public LayerMask CollisionMask
    {
        get => _collisionMask;
        set => _collisionMask = value;
    }

    public Vector3[] MyLocalWaypoints
    {
        get => _myLocalWaypoints;
        set => _myLocalWaypoints = value;
    }

    void Start()
    {
        _controller = GetComponent<RaycastController>();
        SetGlobalWaypoints();
    }

    void FixedUpdate()
    {
        _controller.UpdateRayCastOrigins();

        Vector3 moveDistance = CalculatePlatformMovement();

        CalculatePassengerMovement(moveDistance);

        MovePassengers(true);
        transform.Translate(moveDistance);
        MovePassengers(false);
    }

    void SetGlobalWaypoints()
    {
        _globalWaypoints = new Vector3[_myLocalWaypoints.Length];
        for (int i = 0; i < _myLocalWaypoints.Length; i++)
        {
            _globalWaypoints[i] = _myLocalWaypoints[i] + transform.position;
        }
    }

    Vector3 CalculatePlatformMovement()
    {

        if (Time.time < _nextMoveTime)
        {
            return Vector3.zero;
        }

        _fromWaypointIndex %= _globalWaypoints.Length;

        var toWaypointIndex = (_fromWaypointIndex + 1) % _globalWaypoints.Length;

        var distanceBetweenWaypoints =
            Vector3.Distance(_globalWaypoints[_fromWaypointIndex], _globalWaypoints[toWaypointIndex]);

        _percentBetweenWaypoints += Mathf.Clamp01(Time.fixedDeltaTime * (_speed / distanceBetweenWaypoints));

        var _easedPercentBetweenWaypoints = Ease(_percentBetweenWaypoints);

        var newPos = Vector3.Lerp(_globalWaypoints[_fromWaypointIndex], _globalWaypoints[toWaypointIndex],
            _easedPercentBetweenWaypoints);

        if (_percentBetweenWaypoints >= 1)
        {
            _percentBetweenWaypoints = 0;
            _fromWaypointIndex++;
            if (_fromWaypointIndex >= _globalWaypoints.Length - 1)
            {
                if (!_isCyclic)
                {
                    _fromWaypointIndex = 0;
                    System.Array.Reverse(_globalWaypoints);
                }
            }

            _nextMoveTime = Time.time + _waitTime;
        }

        return newPos - transform.position;
    }

    //EASING:
    // y = (x^a)/(x^a + (1-x)^a)
    //Given a value of x between 0 and 1 this equation will give us back a y
    //value between 0 and 1
    //but it will be eased depending on the a value.
    //IF A = 1 we get a straight line (no acceleration factor, just steady speed)
    //If we set it to 2 then we will accelerate between waypoints (so going fastest in the middle of our transition)
    float Ease(float x)
    {
        var a = _easing + 1;
        var power = Mathf.Pow(x, a);
        return power / (power + Mathf.Pow(1 - x, a));
    }

    void CalculatePassengerMovement(Vector3 moveDistance)
    {
        _movedPassengers.Clear();

        var directionX = Mathf.Sign(moveDistance.x);
        var directionY = Mathf.Sign(moveDistance.y);

        //Vertically moving Platform
        if (moveDistance.y != 0)
        {
            var rayLength =
                Mathf.Abs(moveDistance.y) +
                _controller.GetSkinWidth(); //we want the overall length of the y value but we want to add skinwidth to set the ray where it should be. 

            for (int i = 0; i < _controller.VerticalRayCount; i++)
            {
                var rayOrigin = (directionY == -1) ? _controller.GetRaycastOrigins().BottomLeft : _controller.GetRaycastOrigins().TopLeft;
                rayOrigin += Vector2.right * (_controller.GetVerticalSpacing() * i);

                var hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, CollisionMask);

                if (hit && hit.distance != 0)
                {
                    var pushX = (directionY == 1) ? moveDistance.x : 0;
                    var pushY = moveDistance.y;
                    var newMovement =
                        new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true);

                    if (!_movedPassengers.Contains(newMovement))
                    {
                        _movedPassengers.Add(newMovement);
                    }


                }
            }
        }

        //Horizontally moving platform
        if (moveDistance.x != 0)
        {
            var rayLength =
                Mathf.Abs(moveDistance.x) +
                _controller
                    .GetSkinWidth(); //we want the overall length of the y value but we want to add skinwidth to set the ray where it should be. 

            for (int i = 0; i < _controller.HorizontalRayCount; i++)
            {
                var rayOrigin = (directionX == -1)
                    ? _controller.GetRaycastOrigins().BottomLeft
                    : _controller.GetRaycastOrigins().BottomRight;

                rayOrigin += Vector2.up * (_controller.GetHorizontalSpacing() * i);
                var hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);

                if (hit && hit.distance != 0)
                {
                    var pushX = moveDistance.x * directionX;
                    var pushY = 0;
                    var newMovement =
                        new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false);

                    if (!_movedPassengers.Contains(newMovement))
                    {
                        _movedPassengers.Add(newMovement);
                    }
                }
            }
        }

        //Passenger on top of horizontal or downward moving platform. 
        if (directionY == -1 || (moveDistance.y == 0 && moveDistance.x != 0))
        {
            var rayLength = 0f;

            rayLength = _controller.GetSkinWidth() * 2;

            for (int i = 0; i < _controller.VerticalRayCount; i++)
            {
                var rayOrigin = _controller.GetRaycastOrigins().TopLeft;

                rayOrigin += Vector2.right * (_controller.GetVerticalSpacing() * i);
                var hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, CollisionMask);

                if (hit && hit.distance != 0)
                {
                    var pushX = moveDistance.x;
                    var pushY = moveDistance.y;
                    var newMovement =
                        new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true);

                    if (!_movedPassengers.Contains(newMovement))
                    {
                        _movedPassengers.Add(newMovement);
                    }
                }
            }
        }
    }

    void MovePassengers(bool beforeMovePlatform)
    {
        foreach (PassengerMovement passenger in _movedPassengers)
        {
            if (!_passengerDictionary.ContainsKey(passenger.Transform))
            {
                _passengerDictionary.Add(passenger.Transform, passenger.Transform.GetComponent<Controller2D>());
            }

            if (passenger.MoveBeforePlatform == beforeMovePlatform)
            {
                _passengerDictionary[passenger.Transform].SetPlatformMoveDistance(passenger.MoveDistance);
            }
        }
    }

    /// <summary>
    /// Draw the waypoints for our platform. 
    /// </summary>
    void OnDrawGizmos()
    {
        if (_myLocalWaypoints != null)
        {
            Gizmos.color = Color.red;
            var size = .3f;

            for (int i = 0; i < _myLocalWaypoints.Length; i++)
            {
                var globalWaypoints = (Application.isPlaying) ? _globalWaypoints[i] : _myLocalWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypoints - Vector3.up * size, globalWaypoints + Vector3.up * size);
                Gizmos.DrawLine(globalWaypoints - Vector3.left * size, globalWaypoints + Vector3.left * size);
            }
        }
    }
}

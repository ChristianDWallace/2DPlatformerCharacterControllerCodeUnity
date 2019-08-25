using UnityEngine;

[RequireComponent(typeof(RaycastController))]
public class CollisionDetection : MonoBehaviour, ICollisionDetection
{
    /// <summary>
    /// Public Value Type Fields
    /// </summary>
    [Header("Collision Detection Raycast")]
    [SerializeField]
    private LayerMask _collisionMask;

    [SerializeField]
    private float _maxSlopeAngle = 80f;

    [SerializeField]
    private string _jumpThroughTag = "JumpThrough";


    /// <summary>
    /// Private Reference Types
    /// </summary>
    private CollisionInfo _collisions;

    private RaycastController _controller;

    public LayerMask CollisionMask { get => _collisionMask; set => _collisionMask = value; }

    void Start()
    {
        _controller = GetComponent<RaycastController>();
        _maxSlopeAngle *= Mathf.Deg2Rad;
    }

    /// <summary>
    /// The public method for managing our collision detections. 
    /// </summary>
    /// <param name="moveDistance"></param>
    /// <returns></returns>
    public Vector2 CollisionHandling(Vector2 moveDistance, Vector2 input)
    {
        _controller.UpdateRayCastOrigins();
        _collisions.Reset();

        if (moveDistance.y < 0)
        {
            moveDistance = DescendSlope(moveDistance);
        }

        if (moveDistance.x != 0)
        {
            moveDistance = HorizontalCollisions(moveDistance);
        }

        if (moveDistance.y != 0)
        {
            moveDistance = VerticalCollisions(moveDistance, input);
        }

        return moveDistance;
    }

    /// <summary>
    /// Manage Horizontal Collisions
    /// </summary>
    /// <param name="moveDistance"></param>
    /// <returns></returns>
    Vector2 HorizontalCollisions(Vector2 moveDistance)
    {
        var directionX = Mathf.Sign(moveDistance.x);
        var rayLength = Mathf.Abs(moveDistance.x) + _controller.GetSkinWidth(); //we want the overall length of the x value but we want to add skinwidth to set the ray where it should be. 

        for (int i = 0; i < _controller.HorizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? _controller.GetRaycastOrigins().BottomLeft : _controller.GetRaycastOrigins().BottomRight;

            rayOrigin += Vector2.up * (_controller.GetHorizontalSpacing() * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);

            if (hit)
            {

                //If we get trapped inside of an object we want to be able to move out of it
                if (hit.distance == 0)
                {
                    continue;
                }

                //When a raycast hits a surface it stores the angle of that surface. 
                //For climbing slopes we want to be able to calculate the angle of the slope.
                //To do this we use the global up (normal of the world) and the normal of the surface
                //we are colliding with. If we collide with a surface, the difference between the global up
                //and the surface normal is going to be equivalent to the angle of the slope we want to climb
                //So the EQUATION IS:
                // Slope angle = Angle(hit.normal, Vector2.up)
                var slopeAngle = Vector2.Angle(Vector2.up, hit.normal) * Mathf.Deg2Rad;
                if (i == 0 && Mathf.Abs(slopeAngle) <= _maxSlopeAngle)
                {
                    var distanceToSlopeStart = 0f;
                    if (slopeAngle != _collisions.PreviousSlopeAngle)
                    {
                        distanceToSlopeStart = hit.distance - _controller.GetSkinWidth();
                        moveDistance.x -= distanceToSlopeStart * directionX;
                    }
                    moveDistance = ClimbSlope(moveDistance, slopeAngle, hit.normal);
                    moveDistance.x += distanceToSlopeStart * directionX;
                }
                else if (!_collisions.ClimbingSlope || Mathf.Abs(slopeAngle) > _maxSlopeAngle)
                {

                    moveDistance.x = (hit.distance - _controller.GetSkinWidth()) * directionX;

                    //If we are climbing a slope and hit something beside us. 
                    if (_collisions.ClimbingSlope)
                    {
                        //Tan(angle) = y/x 
                        moveDistance.y = Mathf.Tan(_collisions.CurrentSlopeAngle) *
                                         Mathf.Abs(moveDistance.x);
                    }

                    rayLength = hit.distance;

                    _collisions.SetX(directionX);

                }
            }

            Debug.DrawRay(rayOrigin,
                Vector2.right * directionX * rayLength,
                Color.green);
        }

        return moveDistance;
    }

    /// <summary>
    /// Handle vertical collisions so that the object collides with others.
    ///
    /// POTENTIAL IMPROVEMENT BY CALLING THIS FROM HORIZONTAL CODE
    /// ADDING IN THE DIRECTION X
    /// AND CASTING OUR RAYS FROM BOTTOM RIGHT OR TOP RIGHT ITERATING BACKWARDS BASED
    /// ON DIRECTION
    /// </summary>
    /// <param name="moveDistance"></param>
    /// <returns></returns>
    Vector2 VerticalCollisions(Vector2 moveDistance, Vector2 input)
    {
        var directionY = Mathf.Sign(moveDistance.y);
        var rayLength = Mathf.Abs(moveDistance.y) + _controller.GetSkinWidth(); //we want the overall length of the y value but we want to add skinwidth to set the ray where it should be. 

        for (int i = 0; i < _controller.VerticalRayCount; i++)
        {
            var rayOrigin = (directionY == -1) ? _controller.GetRaycastOrigins().BottomLeft : _controller.GetRaycastOrigins().TopLeft;

            rayOrigin += Vector2.right * (_controller.GetVerticalSpacing() * i + moveDistance.x);
            var hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, CollisionMask);

            if (hit)
            {
                //jumping through platforms. 
                if (hit.collider.tag == _jumpThroughTag)
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }
                    else if (transform.gameObject == _collisions.FallingThroughPlatform)
                    {
                        continue;
                    }
                    else if (input.y == -1)
                    {
                        _collisions.FallingThroughPlatform = hit.transform.gameObject;
                        continue;
                    }
                }

                //Set our y moveDistance equal to the amount we have to move to get from our current position to the point at which the ray intersected with an obstacle. 
                if (!_collisions.SlidingDownSlope)
                {
                    moveDistance.y = (hit.distance - _controller.GetSkinWidth()) * directionY;
                }
                else
                {
                    moveDistance.y = _collisions.SlopeNormal.y * moveDistance.y;
                }


                //If we are climbing a slope and hit something above us. 
                if (_collisions.ClimbingSlope)
                {
                    //Tan(angle) = y/x 
                    //x * Tan(angle) = y
                    //x = y/Tan(Angle)
                    moveDistance.x = moveDistance.y / Mathf.Tan(_collisions.CurrentSlopeAngle) *
                                     Mathf.Sign(moveDistance.x);
                }


                _collisions.SetY(directionY);
                rayLength = hit.distance;



            }
            else if (_collisions.FallingThroughPlatform != null)
            {
                _collisions.ResetFallingThroughPlatform();
            }

            Debug.DrawRay(rayOrigin,
                Vector2.up * directionY * rayLength,
                Color.green);
        }

        //THIS CODE PREEMPTS A BUG WE HAVE WHERE WE CLIP INTO 
        //A piece of geometry when the angle of the slope we are climbing changes. 
        if (_collisions.ClimbingSlope)
        {
            moveDistance = PreemptSlopeChangeRaycast(moveDistance, rayLength);
        }


        return moveDistance;
    }

    /// <summary>
    /// We want our speed when climbing the slope to be the same as if we were climbing normally.
    ///
    /// So we will treat our speed on the X axis as the total distance along the slope we want to move,
    /// then using that distance, and the slope angle, we will calculate how high up to move on the y moveDistance.
    ///
    /// The TRIG FUNCTION for this operation functions as follows:
    /// WHat we have: The angle of the slope and the Hypotenuse distance
    /// What we want: The width and height of the slope Triangle.
    /// The sin of the angle = y/hypotenuse
    /// cosine = x/hypotenuse
    ///
    /// We want to solve for x and y
    ///
    /// so
    ///
    /// TO FIND Y
    /// sin(angle) = y/hypotenuse
    /// multiply both sides by hypotenuse and our answer is
    /// y = sin(angle) * hypotenuse
    ///
    ///
    /// TO FIND X
    /// cos(angle) = x/hypotenuse
    /// multiply both sides by hypotenuse
    /// x = cos(angle) * hypotenuse
    /// 
    /// </summary>
    /// <param name="moveDistance"></param>
    /// <param name="slopeAngle"></param>
    Vector2 ClimbSlope(Vector2 moveDistance, float slopeAngle, Vector2 hitNormal)
    {
        var hypotenuse = Mathf.Abs(moveDistance.x) / Mathf.Cos(slopeAngle);
        var climbMoveDistanceY = Mathf.Sin(slopeAngle) * hypotenuse;

        //if our vertical velocity is less than the calculate velocity then we know we are not currently jumping
        //And so we should apply the slope climb velocity.
        if (moveDistance.y <= climbMoveDistanceY)
        {
            moveDistance.y = climbMoveDistanceY;
            moveDistance.x = Mathf.Cos(slopeAngle) * hypotenuse * Mathf.Sign(moveDistance.x);

            _collisions.Below = _collisions.ClimbingSlope = true; //We need to ensure that, despite our upward movement, we can still jump. 
            _collisions.CurrentSlopeAngle = slopeAngle;
            _collisions.SlopeNormal = hitNormal;

        }

        return moveDistance;

    }

    /// <summary>
    /// Method for descending slope. We first check if we are moving downwards in the Collision Handling method
    /// If we are then we send a raycast downwards to infinity to check where we will collide below. If the collision is a slope that we
    /// are close enough to touch, and the slope x direction is the same as our x direction, then we will descend it using the same
    /// mathematical formulas as the once used in the Climb slope method. 
    /// </summary>
    /// <param name="moveDistance"></param>
    /// <returns></returns>
    Vector2 DescendSlope(Vector2 moveDistance)
    {

        moveDistance = SlideDownSlope(moveDistance);

        if (!_collisions.SlidingDownSlope)
        {
            var directionX = Mathf.Sign(moveDistance.x);
            var rayOrigin = (directionX == -1) ? _controller.GetRaycastOrigins().BottomRight : _controller.GetRaycastOrigins().BottomLeft;
            var hit = Physics2D.Raycast(rayOrigin, -Vector2.up, _controller.GetSkinWidth(), _collisionMask);

            if (hit && Mathf.Sign(hit.normal.x) == directionX)
            {
                //POTENTIAL BUG: when slope gets a bit too steep we seem to just fall off of it rather than descending.
                var slopeAngle = Vector2.Angle(hit.normal, Vector2.up) * Mathf.Deg2Rad;
                if (slopeAngle != 0 && Mathf.Abs(slopeAngle) <= _maxSlopeAngle)
                {
                    var hypotenuse = Mathf.Abs(moveDistance.x) / Mathf.Cos(slopeAngle);
                    var yDistance = Mathf.Sin(slopeAngle) * hypotenuse;
                    moveDistance.x = Mathf.Cos(slopeAngle) * hypotenuse * directionX;

                    moveDistance.y -= yDistance;

                    _collisions.DescendingSlope = _collisions.Below = true;
                    _collisions.CurrentSlopeAngle = slopeAngle;
                    _collisions.SlopeNormal = hit.normal;
                }
            }
        }

        return moveDistance;
    }

    Vector2 SlideDownSlope(Vector2 moveDistance)
    {
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(_controller.GetRaycastOrigins().BottomLeft, Vector2.down,
            Mathf.Abs(moveDistance.y) + _controller.GetSkinWidth(), CollisionMask);

        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(_controller.GetRaycastOrigins().BottomRight, Vector2.down,
            Mathf.Abs(moveDistance.y) + _controller.GetSkinWidth(), CollisionMask);

        //POTENTIAL BUG: If we are standing on a ledge and a slope is directly below us (within touching distance) and we change directions 
        //We may be pulled towards the slope. 
        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            moveDistance = SlideDownMaxSlope(maxSlopeHitLeft, moveDistance);
            if (!_collisions.SlidingDownSlope)
            {
                moveDistance = SlideDownMaxSlope(maxSlopeHitRight, moveDistance);
            }
        }
        return moveDistance;
    }

    /// <summary>
    /// We have a slope and the player falling down the slope
    /// We know the distance the player will fall
    /// we wanna know how far we move on the x
    ///
    /// tan(slopeAngle) = (y - hit.distance)x
    /// x*tan(slopeAngle) = y - hit.distance
    ///
    /// x = (y - hit.distance)/tan(slopeAngle);
    /// </summary>
    /// <param name="moveDistance"></param>
    Vector2 SlideDownMaxSlope(RaycastHit2D hit, Vector2 moveDistance)
    {
        if (hit)
        {
            var slopeAngle = Vector2.Angle(hit.normal, Vector2.up) * Mathf.Deg2Rad;
            if (Mathf.Abs(slopeAngle) > _maxSlopeAngle)
            {
                moveDistance.x = hit.normal.x * ((Mathf.Abs(moveDistance.y) + _controller.GetSkinWidth()) / Mathf.Tan(slopeAngle));

                _collisions.CurrentSlopeAngle = slopeAngle;
                _collisions.SlidingDownSlope = true;
                _collisions.SlopeNormal = hit.normal;
            }
        }

        return moveDistance;

    }

    /// <summary>
    /// In this method we cast out a horizontal ray after having checked our vertical rays.
    /// This is so that we can see if the slope we are climbing changes its angle before our x
    /// Actually collides with the new slope. This is because, without this raycast, we clip into the
    /// intersection between the old and new slope for a few frames. 
    /// </summary>
    /// <param name="moveDistance"></param>
    /// <param name="rayLength"></param>
    /// <returns></returns>
    Vector2 PreemptSlopeChangeRaycast(Vector2 moveDistance, float rayLength)
    {
        var directionX = Mathf.Sign(moveDistance.x);
        rayLength = Mathf.Abs(moveDistance.x) + _controller.GetSkinWidth();
        var rayOrigin = ((directionX == -1) ? _controller.GetRaycastOrigins().BottomLeft : _controller.GetRaycastOrigins().BottomRight) +
                            Vector2.up * moveDistance.y;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, _collisionMask);

        if (hit)
        {
            var slopeAngle = Vector2.Angle(hit.normal, Vector2.up) * Mathf.Deg2Rad;

            if (slopeAngle != _collisions.CurrentSlopeAngle && Mathf.Abs(slopeAngle) <= _maxSlopeAngle)
            {
                moveDistance.x = (hit.distance - _controller.GetSkinWidth()) * directionX;
            }

            _collisions.CurrentSlopeAngle = slopeAngle;
            _collisions.SlopeNormal = hit.normal;
        }

        return moveDistance;
    }

    public bool IsGrounded()
    {
        return _collisions.Below;
    }
}
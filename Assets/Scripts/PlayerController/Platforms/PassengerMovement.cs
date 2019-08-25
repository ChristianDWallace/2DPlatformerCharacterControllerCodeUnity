using UnityEngine;

public struct PassengerMovement
{
    public Transform Transform { get; set; }
    public Vector3 MoveDistance { get; set; }
    public bool StandingOnPlatform { get; set; }
    public bool MoveBeforePlatform { get; set; }

    public PassengerMovement(Transform _transform, Vector3 _moveDistance, bool _stiandingOnPlatform,
        bool _moveBeforePlatform)
    {
        Transform = _transform;
        MoveDistance = _moveDistance;
        StandingOnPlatform = _stiandingOnPlatform;
        MoveBeforePlatform = _moveBeforePlatform; 
    }
}

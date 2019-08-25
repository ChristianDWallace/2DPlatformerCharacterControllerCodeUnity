using UnityEngine;

public struct CollisionInfo
{
    public bool Above { get; set; }
    public bool Below { get; set; }
    public bool Left { get; set; }
    public bool Right { get; set; }
    public bool ClimbingSlope { get; set; }
    public bool DescendingSlope { get; set; }
    
    public bool SlidingDownSlope { get; set; }

    public float CurrentSlopeAngle { get; set; }
    public float PreviousSlopeAngle { get; set; }

    public Vector2 SlopeNormal { get; set; }
    public GameObject FallingThroughPlatform { get; set; }

    public void Reset()
    {
        Above = Below = Left = Right = ClimbingSlope = 
            DescendingSlope = SlidingDownSlope = false;

        PreviousSlopeAngle = CurrentSlopeAngle;
        CurrentSlopeAngle = 0;

        SlopeNormal = Vector2.zero; 
    }

    public void SetY(float direction)
    {
        Below = direction == -1;
        Above = !Below;
    }

    public void SetX(float direction)
    {
        Left = direction == -1;
        Right = !Left;
    }

    public void ResetFallingThroughPlatform()
    {
        FallingThroughPlatform = null;
    }
}
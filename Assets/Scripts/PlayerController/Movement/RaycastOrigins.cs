using UnityEngine;

public struct RaycastOrigins
{
    public Vector2 TopLeft { get; set; }
    public Vector2 TopRight { get; set; }
    public Vector2 BottomLeft { get; set; }
    public Vector2 BottomRight { get; set; }

    /// <summary>
    /// Updates the values of the origins for our raycast boundaries. 
    /// </summary>
    /// <param name="bounds"> The boundary we want to fit our raycast to. </param>
    public void SetBounds(Bounds bounds)
    {
        BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }
}

using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    /// <summary>
    /// Inspector Viewable Value types
    /// </summary>

    [Header("Rays")]
    [SerializeField]
    private float _distanceBetweenRays = 0.2f;
    [SerializeField]
    private float _skinWidth = 0.1f;
    /// <summary>
    /// Private Reference Fields
    /// </summary>
    private BoxCollider2D _collider;

    private RaycastOrigins _origins; 

    public int VerticalRayCount { get => _verticalRayCount; set => _verticalRayCount = value; }
    public int HorizontalRayCount { get => _horizontalRayCount; set => _horizontalRayCount = value; }



    /// <summary>
    /// Private Value Type Fields
    /// </summary>

    /***********RAYS*************/
    
    private int _horizontalRayCount;
    private int _verticalRayCount; 
    private float _horizontalRaySpacing,
        _verticalRaySpacing;

    void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        CalculateRaySpacing();
    }

    Bounds GetBounds()
    {
        //Get the boundary value of the box collider. 
        var bounds = _collider.bounds;
        //shrink the boundary of our raycast origins into the box collider. 
        bounds.Expand(_skinWidth * -2);

        return bounds;
    }

    /// <summary>
    /// A Method used to change the origins of our raycasts for our box collider 
    /// </summary>
    public void UpdateRayCastOrigins()
    {
        var bounds = GetBounds();
        _origins.SetBounds(bounds);
    }

    /// <summary>
    /// Calculates the spacing between the rays at the edges of our controller.
    /// </summary>
    /// <param name="bounds"> The bounds of our box collider. </param>
    private void CalculateRaySpacing()
    {
        var bounds = GetBounds();

        var boundsWitdth = bounds.size.x;
        var boundsHeight = bounds.size.y; 

        //calculate ray counts based on bounds size and distance between rays. 
        _horizontalRayCount = Mathf.RoundToInt(boundsHeight / _distanceBetweenRays);
        _verticalRayCount = Mathf.RoundToInt(boundsWitdth / _distanceBetweenRays); 

        HorizontalRayCount = Mathf.Clamp(HorizontalRayCount, 2, int.MaxValue);
        VerticalRayCount = Mathf.Clamp(VerticalRayCount, 2, int.MaxValue);

        _horizontalRaySpacing = bounds.size.y / (HorizontalRayCount - 1);
        _verticalRaySpacing = bounds.size.x / (VerticalRayCount - 1);
    }

    public RaycastOrigins GetRaycastOrigins()
    {
        return _origins; 
    }

    public float GetSkinWidth()
    {
        return _skinWidth; 
    }

    public float GetHorizontalSpacing()
    {
        return _horizontalRaySpacing;
    }

    public float GetVerticalSpacing()
    {
        return _verticalRaySpacing; 
    }

    public Bounds GetColliderBounds()
    {
        return _collider.bounds; 
    }
}

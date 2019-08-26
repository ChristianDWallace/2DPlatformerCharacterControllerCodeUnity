using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    /// <summary>
    /// Inspector Viewable Value Types
    /// </summary>
    [SerializeField]
    private float _verticalOffset = 1f,
        _lookAheadDistanceX = .5f,
        _lookSmoothTimeX = .1f,
        _lookSmoothTimeY = .1f,
        _focusZoomOut = -10f;

    /// <summary>
    /// Inspector Viewable Reference Types
    /// </summary>
    [SerializeField] private RaycastController _target;

    [SerializeField] private Vector2 _focusAreaSize;

    /// <summary>
    /// Private Value Types
    /// </summary>
    private FocusArea _focusArea;

    private float _currentLookAheadX,
        _targetLookAheadX,
        _lookAheadDirectionX,
        _smoothLookVelocityX,
        _smoothLookVelocityY;

    private bool _lookAheadStopped;



    public RaycastController Target
    {
        get => _target;
        set => _target = value;
    }

    public Vector2 FocusAreaSize
    {
        get => _focusAreaSize;
        set => _focusAreaSize = value;
    }

    void Start()
    {
        _focusArea = new FocusArea(Target.GetColliderBounds(), FocusAreaSize);
    }

    void FixedUpdate()
    {
        MovePosition();
    }

    void MovePosition()
    {
        _focusArea.Update(Target.GetColliderBounds());

        Vector3 focusPosition = _focusArea.Center + Vector2.up * _verticalOffset;

        if (_focusArea.MovedThisFrame.x != 0)
        {
            _lookAheadDirectionX = Mathf.Sign(_focusArea.MovedThisFrame.x);
            float x = PlayerInput.GetInput().x;
            if (x == Mathf.Sign(_focusArea.MovedThisFrame.x) && x != 0)
            {
                _lookAheadStopped = false;
                _targetLookAheadX = _lookAheadDirectionX * _lookAheadDistanceX;
            }
            else
            {
                if (!_lookAheadStopped)
                {
                    _lookAheadStopped = true;
                    _targetLookAheadX = _currentLookAheadX +
                                        (_lookAheadDirectionX * _lookAheadDistanceX - _currentLookAheadX) / 4;
                }

            }
        }

        _currentLookAheadX = Mathf.SmoothDamp(_currentLookAheadX, _targetLookAheadX, ref _smoothLookVelocityX,
            _lookSmoothTimeX);

        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref _smoothLookVelocityY,
            _lookSmoothTimeY);
        focusPosition += Vector3.right * _currentLookAheadX;

        transform.position = focusPosition + Vector3.forward * _focusZoomOut;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, .5f);
        Gizmos.DrawCube(_focusArea.Center, FocusAreaSize);
    }
}
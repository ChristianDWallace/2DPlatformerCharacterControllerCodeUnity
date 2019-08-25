using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class BasicPlayerFunctions : MonoBehaviour
{ 

    /// <summary>
    /// Public Value Type Fields
    /// </summary>
    [Header("Movement Values")]
    [SerializeField]
    private float _moveSpeed = .15f;

    [SerializeField]
    private float _velocityXSmoothTimeAirborne = .1f,
        _velocityXSmoothTimeGrounded = .05f;

    [SerializeField]
    private float _maxJumpHeight = 1f,
        _minJumpHeight = .2f,
        _timeToJumpApex = .4f,
        _fallMultiplier = 1.06f;

    [SerializeField]
    private float _jumpPadding = 0.1f,
        _groundedPadding = 0.05f;


    /// <summary>
    /// PRIVATE REFERENCE TYPE FIELDS
    /// </summary>
    private IController2D _controller;
    private Vector2 _velocity;
    private Vector2 _input;

    /// <summary>
    /// PRIVATE VALUE TYPE FIELDS
    /// </summary>
    private float _gravity;
    private float _maxJumpVelocity,
        _minJumpVelocity;
    private float _velocityXSmoothing;

    private float _jumpTimer,
        _groundedTimer;

    private float _previousXInputDirection;

    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<Controller2D>();
        SetJumpValues();
    }

    void Update()
    {
        GetInput(); 
    }

    void FixedUpdate()
    {
        ApplyPhysics();
    }

    //SETJUMPVALUES SUMMARY
    //_maxJumpHeight, _timeToJumpApex
    //Want: gravity, jumpVelocity
    void SetJumpValues()
    {
        //Formula:
        //_maxJumpHeight = (gravity * _timeToJumpApex^2)/2
        //Rearrange the equation: 
        // 2* _maxJumpHeight = gravity * _timeToJumpApex^2
        // (2 * _maxJumpHeight)/ _timeToJumpApex^2
        _gravity = -(2 * _maxJumpHeight) / Mathf.Pow(_timeToJumpApex, 2);

        //Formula for velocity
        //velocityFinal = velocity Initial + acceleration * time
        //jumpVelocity = gravity * _timeToJumpApex
        _maxJumpVelocity = Mathf.Abs(_gravity) * _timeToJumpApex;
        
        //Final velocity ^2 = initial velocity ^2 + 2 * acceleration * displacement
        //for jumping initial velocity is 0 so 
        //Final velocity^2 = 2 * acceleration * displacement
        //so
        //minimum jump force = sqrt(2 * gravity * minJumpHeight); 
        _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * _minJumpHeight);
    }

    void GetInput()
    {
        _input = PlayerInput.GetInput();

        if (_input.x != _previousXInputDirection && _input.x != 0)
        {
            _previousXInputDirection = _input.x;
            gameObject.transform.localScale = new Vector3(Mathf.Sign(_input.x), gameObject.transform.localScale.y, gameObject.transform.localScale.z);
        }

        if (_jumpTimer > 0)
        {
            _jumpTimer -= Time.deltaTime;
        }

        if (_groundedTimer > 0)
        {
            _groundedTimer -= Time.deltaTime;
        }

        if (_controller.IsGrounded())
        {
            _groundedTimer = _groundedPadding;
        }
        if (PlayerInput.GetJump())
        {
            _jumpTimer = _jumpPadding;
        }
        else if (PlayerInput.ReleaseJump() && _velocity.y > _minJumpVelocity)
        {
            _velocity.y = _minJumpVelocity;
        }
    }

    void ApplyPhysics()
    {

        if (_jumpTimer > 0 && _groundedTimer > 0)
        {
           _velocity.y = _maxJumpVelocity;
            _jumpTimer = _groundedTimer = 0;
        }
        _velocity.x = Mathf.SmoothDamp(_velocity.x,
            _input.x * _moveSpeed,
            ref _velocityXSmoothing,
            _velocity.y == 0 ? _velocityXSmoothTimeGrounded : _velocityXSmoothTimeAirborne);


        _velocity.y += _gravity * Time.fixedDeltaTime;
        

        if (Mathf.Sign(_velocity.y) == -1 && !_controller.IsGrounded())
        {
            _velocity.y *= _fallMultiplier;
        }
        
        _velocity = _controller.Move(_velocity, _input);
    }

    public bool IsGrounded()
    {
        return _controller.IsGrounded(); 
    }

    public float GetVelocityX()
    {
        return _velocity.x;
    }
}

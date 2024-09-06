using UnityEngine;
using Fusion;
using NetworkRigidbody2D = Fusion.Addons.Physics.NetworkRigidbody2D;

public class PlayerRigidBodyMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private LayerMask _groundLayer;
    private NetworkRigidbody2D _rb;
    private InputController _inputController;

    [SerializeField] float _speed = 10f;
    [SerializeField] float _jumpForce = 10f;
    [SerializeField] float _maxVelocity = 8f;

    [SerializeField] private float fallMultiplier = 3.3f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    private readonly float wallSlidingMultiplier = 1f;

    private Vector2 _groundHorizontalDragVector = new Vector2(.1f, 1);
    private Vector2 _airHorizontalDragVector = new Vector2(.98f, 1);
    private Vector2 _horizontalSpeedReduceVector = new Vector2(.95f, 1);
    private Vector2 _verticalSpeedReduceVector = new Vector2(1, .95f);

    private Collider2D _collider;
    [Networked]
    private NetworkBool IsGrounded { get; set; }
    private bool _wallSliding;
    private Vector2 _wallSlidingNormal;

    private float _jumpBufferThreshold = .2f;
    private float _jumpBufferTime;

    [Networked]
    private float CoyoteTimeThreshold { get; set; } = .1f;
    [Networked]
    private float TimeLeftGrounded { get; set; }
    [Networked]
    private NetworkBool CoyoteTimeCD { get; set; }
    [Networked]
    private NetworkBool WasGrounded { get; set; }

    [Networked] public Vector3 Velocity { get; set; }



    void Awake()
    {
        _rb = GetComponent<NetworkRigidbody2D>();
        _collider = GetComponentInChildren<Collider2D>();
        _inputController = GetBehaviour<InputController>();
    }

    public override void Spawned()
    { 
        Debug.Log("spawned!"); 
        Runner.SetPlayerAlwaysInterested(Object.InputAuthority, Object, true);
        _rb.Rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    /// <summary>
    /// Detects grounded and wall sliding state
    /// </summary>
    private void DetectGroundAndWalls()
    {
        WasGrounded = IsGrounded;
        IsGrounded = default;
        _wallSliding = default;

        IsGrounded = (bool)Runner.GetPhysicsScene2D().OverlapBox((Vector2)transform.position + Vector2.down * (_collider.bounds.extents.y - .3f), Vector2.one * .85f, 0, _groundLayer);
        if (IsGrounded)
        {
            CoyoteTimeCD = false;
            return;
        }

        if (WasGrounded)
        {
            if (CoyoteTimeCD)
            {
                CoyoteTimeCD = false;
            }
            else
            {
                TimeLeftGrounded = Runner.SimulationTime;
            }
        }

        _wallSliding = Runner.GetPhysicsScene2D().OverlapCircle(transform.position + Vector3.right * (_collider.bounds.extents.x), .1f, _groundLayer);
        if (_wallSliding)
        {
            _wallSlidingNormal = Vector2.left;
            return;
        }
        else
        {
            _wallSliding = Runner.GetPhysicsScene2D().OverlapCircle(transform.position - Vector3.right * (_collider.bounds.extents.x), .1f, _groundLayer);
            if (_wallSliding)
            {
                _wallSlidingNormal = Vector2.right;
            }
        }

    }

    public bool GetGrounded()
    {
        return IsGrounded;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput<InputData>(out var input))
        {
            var pressed = input.GetButtonPressed(_inputController.PrevButtons);
            _inputController.PrevButtons = input.Buttons;
            UpdateMovement(input);
            Jump(pressed);
            BetterJumpLogic(input);
        }

        Velocity = _rb.Rigidbody.velocity;
    }

    void UpdateMovement(InputData input)
    {
        DetectGroundAndWalls();

        if (input.GetButton(InputButton.LEFT))
        {
            //Reset x velocity if start moving in oposite direction.
            if (_rb.Rigidbody.velocity.x > 0 && IsGrounded)
            { 
                _rb.Rigidbody.velocity *= Vector2.up; 
            }
            _rb.Rigidbody.AddForce(Vector2.left * _speed * Runner.DeltaTime, ForceMode2D.Force);
        }
        else if (input.GetButton(InputButton.RIGHT))
        {
            //Reset x velocity if start moving in oposite direction.
            if (_rb.Rigidbody.velocity.x < 0 && IsGrounded)
            {
                _rb.Rigidbody.velocity *= Vector2.up;
            }
            _rb.Rigidbody.AddForce(Vector2.right * _speed * Runner.DeltaTime, ForceMode2D.Force);
        }
        else
        {
            //Different horizontal drags depending if grounded or not.
            if (IsGrounded)
                _rb.Rigidbody.velocity *= _groundHorizontalDragVector;
            else
                _rb.Rigidbody.velocity *= _airHorizontalDragVector;
        }

        LimitSpeed();
    }

    private void LimitSpeed()
    {
        //Limit horizontal velocity
        if (Mathf.Abs(_rb.Rigidbody.velocity.x) > _maxVelocity)
        {
            _rb.Rigidbody.velocity *= _horizontalSpeedReduceVector;
        }

        if (Mathf.Abs(_rb.Rigidbody.velocity.y) > _maxVelocity * 2)
        {
            _rb.Rigidbody.velocity *= _verticalSpeedReduceVector;
        }
    }

    #region Jump
    private void Jump(NetworkButtons pressedButtons)
    {

        //Jump
        if (pressedButtons.IsSet(InputButton.JUMP) || CalculateJumpBuffer())
        {
            
                if (!IsGrounded && pressedButtons.IsSet(InputButton.JUMP))
                {
                    _jumpBufferTime = Runner.SimulationTime;
                }

                if (IsGrounded || CalculateCoyoteTime())
                {
                    _rb.Rigidbody.velocity *= Vector2.right; //Reset y Velocity
                    _rb.Rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                    CoyoteTimeCD = true;
                }
                else if (_wallSliding)
                {
                    _rb.Rigidbody.velocity *= Vector2.zero; //Reset y and x Velocity
                    _rb.Rigidbody.AddForce((Vector2.up + (_wallSlidingNormal)) * _jumpForce, ForceMode2D.Impulse);
                    CoyoteTimeCD = true;
                }
        }
    }

    private bool CalculateJumpBuffer()
    {
        return (Runner.SimulationTime <= _jumpBufferTime + _jumpBufferThreshold) && IsGrounded;
    }

    private bool CalculateCoyoteTime()
    {
        return (Runner.SimulationTime <= TimeLeftGrounded + CoyoteTimeThreshold);
    }

 
    /// <summary>
    /// 입력 및 현재 낙하 진행 상황에 따라 플레이어의 중력이 증가합니다.
    /// </summary>
    /// <param name="input"></param>
    private void BetterJumpLogic(InputData input)
    {
        if (IsGrounded) { return; }
        if (_rb.Rigidbody.velocity.y < 0)
        {
            if (_wallSliding && input.AxisPressed())
            {
                _rb.Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (wallSlidingMultiplier - 1) * Runner.DeltaTime;
            }
            else
            {
                _rb.Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Runner.DeltaTime;
            }
        }
        else if (_rb.Rigidbody.velocity.y > 0 && !input.GetButton(InputButton.JUMP))
        {
            _rb.Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Runner.DeltaTime;
        }
    }
    #endregion
}

using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkRigidbody2D _rb2;
    private bool _isGrounded;
    public float moveSpeed = 5f;  // 이동 속도
    public float jumpForce = 7f;  // 점프 힘
    public float latencyCompensation = 0.1f; // 네트워크 지연 보정 값

    private Vector2 predictedPosition;
    private NetworkRunner _runner;

    private void Awake()
    {
        _rb2 = GetComponent<NetworkRigidbody2D>();
        _rb2.Rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;  // 보간 활성화
    }

    private void Start()
    {
        // NetworkRunner를 할당 (필요시 외부에서 할당되도록 할 수도 있음)
        _runner = FindObjectOfType<NetworkRunner>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 플레이어가 땅에 닿았는지 확인
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (_runner == null) return; // NetworkRunner가 할당되지 않았으면 리턴

        if (GetInput(out NetworkInputData data))
        {
            // 가로축 이동
            Vector2 moveDirection = new Vector2(data.direction.x * moveSpeed, _rb2.Rigidbody.velocity.y);
            
            // 클라이언트 예측: 호스트의 움직임을 예측
            if (Runner.IsForward)
            {
                _rb2.Rigidbody.velocity = moveDirection;

                // 점프
                if (data.direction.y > 0 && _isGrounded)
                {
                    _rb2.Rigidbody.velocity = new Vector2(_rb2.Rigidbody.velocity.x, jumpForce);
                }

                // 네트워크 딜레이 보정: Runner.DeltaTime을 사용하여 예측
                float latency = _runner.DeltaTime * latencyCompensation;
                predictedPosition = (Vector2)transform.position + _rb2.Rigidbody.velocity * latency;
            }
        }

        // 서버 보정: 서버에서 받은 위치로 보정
        if (Runner.IsResimulation)
        {
            // 서버로부터 수신한 호스트의 실제 위치로 보정
            transform.position = Vector2.Lerp(transform.position, predictedPosition, 0.1f);
        }
    }
}

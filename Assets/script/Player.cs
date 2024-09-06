using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkRigidbody2D _rb2;
    private bool _isGrounded;
    public float moveSpeed = 5f;  // 이동 속도
    public float jumpForce = 7f;  // 점프 힘

    private void Awake()
    {
        _rb2 = GetComponent<NetworkRigidbody2D>();
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
        if (GetInput(out NetworkInputData data))
        {
            // 가로축 이동
            Vector2 moveDirection = new Vector2(data.direction.x * moveSpeed, _rb2.Rigidbody.velocity.y); // 좌우 이동에만 속도를 적용
            _rb2.Rigidbody.velocity = moveDirection;

            // 점프
            if (data.direction.y > 0 && _isGrounded) // 위쪽 키(W)를 누르고 땅에 닿아 있을 때만 점프
            {
                _rb2.Rigidbody.velocity = new Vector2(_rb2.Rigidbody.velocity.x, jumpForce); // 위쪽으로 속도 적용
            }
        }
    }
}
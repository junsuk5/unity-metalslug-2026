using UnityEngine;

/// <summary>
/// 플레이어의 좌우 이동, 방향 전환, 점프를 처리합니다.
/// </summary>
public class PlayerMotor
{
    private readonly Transform _transform;
    private readonly Rigidbody2D _rigidbody;
    private readonly float _moveSpeed;
    private readonly float _jumpForce;

    public PlayerMotor(Transform transform, Rigidbody2D rigidbody, float moveSpeed, float jumpForce)
    {
        _transform = transform;
        _rigidbody = rigidbody;
        _moveSpeed = moveSpeed;
        _jumpForce = jumpForce;
    }

    /// <summary>
    /// 이동 방향으로 캐릭터를 돌리고 그 방향으로 이동합니다.
    /// </summary>
    /// <param name="direction">좌(-1), 우(1)</param>
    public void Move(float direction)
    {
        Vector3 scale = _transform.localScale;
        scale.x = Mathf.Sign(direction) * Mathf.Abs(scale.x);
        _transform.localScale = scale;

        _transform.Translate(_moveSpeed * Time.deltaTime * new Vector3(direction, 0f, 0f));
    }

    public void Jump()
    {
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.AddForce(Vector2.up * _jumpForce);
    }
}

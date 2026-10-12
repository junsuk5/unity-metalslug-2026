using NUnit.Framework;
using UnityEngine;

public class PlayerMotorTests
{
    private const float MoveSpeed = 6f;
    private const float JumpForce = 500f;

    private GameObject _gameObject;
    private Rigidbody2D _rigidbody;
    private PlayerMotor _motor;

    [SetUp]
    public void SetUp()
    {
        _gameObject = new GameObject("PlayerMotorTests");
        _gameObject.transform.localScale = new Vector3(4f, 4f, 1f);
        _rigidbody = _gameObject.AddComponent<Rigidbody2D>();
        _motor = new PlayerMotor(_gameObject.transform, _rigidbody, MoveSpeed, JumpForce);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_gameObject);
    }

    [Test]
    public void 오른쪽으로_이동하면_오른쪽을_보고_크기는_유지된다()
    {
        _motor.Move(1f, 0.1f);

        Assert.AreEqual(new Vector3(4f, 4f, 1f), _gameObject.transform.localScale);
    }

    [Test]
    public void 왼쪽으로_이동하면_왼쪽을_보고_크기는_유지된다()
    {
        _motor.Move(-1f, 0.1f);

        Assert.AreEqual(new Vector3(-4f, 4f, 1f), _gameObject.transform.localScale);
    }

    [Test]
    public void 이동량은_속도와_경과시간의_곱이다()
    {
        _motor.Move(1f, 0.5f);

        Assert.AreEqual(MoveSpeed * 0.5f, _gameObject.transform.position.x, 1e-4f);
    }

    [Test]
    public void 왼쪽을_보고_있어도_왼쪽으로_이동한다()
    {
        _motor.Move(-1f, 0.5f);
        _motor.Move(-1f, 0.5f);

        Assert.AreEqual(-MoveSpeed, _gameObject.transform.position.x, 1e-4f);
        Assert.AreEqual(0f, _gameObject.transform.position.y, 1e-4f);
    }

    [Test]
    public void 점프하면_기존_속도를_초기화한다()
    {
        _rigidbody.linearVelocity = new Vector2(3f, -5f);

        _motor.Jump();

        Assert.AreEqual(Vector2.zero, _rigidbody.linearVelocity);
    }
}

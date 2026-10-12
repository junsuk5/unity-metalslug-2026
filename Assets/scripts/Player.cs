using FSM;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 6.0f;
    [SerializeField] private float _jumpForce = 500f;

    private PlayerAnimator _animator;
    private PlayerMotor _motor;
    private GroundDetector _groundDetector;
    private AudioSource _audioSource;
    [SerializeField] private AudioClip _gunfireClip;

    public StateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }

    public IPlayerInput Input { get; private set; }
    public bool IsGrounded => _groundDetector.IsGrounded;

    void Awake()
    {
        _animator = new PlayerAnimator(GetComponent<Animator>());
        _motor = new PlayerMotor(transform, GetComponent<Rigidbody2D>(), _moveSpeed, _jumpForce);
        _groundDetector = new GroundDetector();
        _groundDetector.GroundedChanged += _animator.SetGrounded;
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        Input = new UnityPlayerInput();

        StateMachine = new StateMachine();
        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
        JumpState = new PlayerJumpState(this);
    }

    void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    void Update()
    {
        StateMachine.Update();

        // 공격은 어느 상태에서든 가능합니다.
        if (Input.AttackPressed)
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        StateMachine.FixedUpdate();
    }

    public void Move(float direction)
    {
        _motor.Move(direction);
    }

    public void Jump()
    {
        _motor.Jump();
    }

    public void SetWalkAnimation(bool isWalking)
    {
        _animator.SetWalking(isWalking);
    }

    private void Attack()
    {
        _animator.PlayAttack();

        if (_gunfireClip != null)
        {
            _audioSource.PlayOneShot(_gunfireClip);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        _groundDetector.OnContactEnter(other.collider);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        _groundDetector.OnContactExit(other.collider);
    }
}

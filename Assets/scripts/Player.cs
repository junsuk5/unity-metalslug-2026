using FSM;
using UnityEngine;

public class Player : MonoBehaviour, IPlayerContext
{
    [SerializeField] private float _moveSpeed = 6.0f;
    [SerializeField] private float _jumpForce = 500f;
    [SerializeField] private AudioClip _gunfireClip;

    private PlayerAnimator _animator;
    private PlayerMotor _motor;
    private GroundDetector _groundDetector;
    private PlayerWeapon _weapon;
    private StateMachine _stateMachine;

    public PlayerState IdleState { get; private set; }
    public PlayerState MoveState { get; private set; }
    public PlayerState JumpState { get; private set; }

    public IPlayerInput Input { get; private set; }
    public bool IsGrounded => _groundDetector.IsGrounded;

    void Awake()
    {
        _animator = new PlayerAnimator(GetComponent<Animator>());
        _motor = new PlayerMotor(transform, GetComponent<Rigidbody2D>(), _moveSpeed, _jumpForce);
        _groundDetector = new GroundDetector();
        _groundDetector.GroundedChanged += _animator.SetGrounded;
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        _weapon = new PlayerWeapon(_animator, new PlayerAudio(audioSource, _gunfireClip));

        Input = new UnityPlayerInput();

        _stateMachine = new StateMachine();
        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
        JumpState = new PlayerJumpState(this);
    }

    void Start()
    {
        _stateMachine.Initialize(IdleState);
    }

    void Update()
    {
        _stateMachine.Update();

        // 공격은 어느 상태에서든 가능합니다.
        if (Input.AttackPressed)
        {
            _weapon.Fire();
        }
    }

    void FixedUpdate()
    {
        _stateMachine.FixedUpdate();
    }

    public void Move(float direction)
    {
        _motor.Move(direction, Time.deltaTime);
    }

    public void Jump()
    {
        _motor.Jump();
    }

    public void SetWalking(bool isWalking)
    {
        _animator.SetWalking(isWalking);
    }

    public void ChangeState(PlayerState next)
    {
        _stateMachine.ChangeState(next);
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

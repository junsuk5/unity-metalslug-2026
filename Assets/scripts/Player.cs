using FSM;
using UnityEngine;

public class Player : MonoBehaviour
{
    private const float MoveSpeed = 6.0f;
    private const float JumpForce = 500f;

    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private AudioSource _audioSource;
    [SerializeField] private AudioClip _gunfireClip;

    public StateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }

    /// <summary>좌(-1), 우(1), 입력 없음(0)</summary>
    public float MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool IsGrounded { get; private set; }

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

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
        ReadInput();

        StateMachine.Update();

        // 공격은 어느 상태에서든 가능합니다.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        StateMachine.FixedUpdate();
    }

    private void ReadInput()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveInput = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveInput = 1f;
        }
        else
        {
            MoveInput = 0f;
        }

        JumpPressed = Input.GetKeyDown(KeyCode.UpArrow);
    }

    public void Move(float direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(direction) * Mathf.Abs(scale.x);
        transform.localScale = scale;

        transform.Translate(MoveSpeed * Time.deltaTime * new Vector3(direction, 0f, 0f));
    }

    public void Jump()
    {
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.AddForce(Vector2.up * JumpForce);
    }

    public void SetWalkAnimation(bool isWalking)
    {
        _animator.SetBool("Walk", isWalking);
    }

    private void Attack()
    {
        _animator.SetTrigger("attack");

        if (_gunfireClip != null)
        {
            _audioSource.PlayOneShot(_gunfireClip);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            IsGrounded = true;
            _animator.SetBool("Grounded", true);
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            IsGrounded = false;
            _animator.SetBool("Grounded", false);
        }
    }
}

/// <summary>
/// 점프하여 공중에 있는 상태입니다. 공중에서도 좌우 이동이 가능합니다.
/// </summary>
public class PlayerJumpState : PlayerState
{
    // 점프 직후에는 아직 땅에 닿아 있으므로, 한 번 땅을 떠난 뒤에만 착지로 판정합니다.
    private bool _hasLeftGround;

    public PlayerJumpState(IPlayerContext context) : base(context) { }

    public override void Enter()
    {
        _hasLeftGround = false;
        context.Jump();
    }

    public override void Update()
    {
        bool isMoving = context.Input.Horizontal != 0f;
        context.SetWalking(isMoving);
        if (isMoving)
        {
            context.Move(context.Input.Horizontal);
        }

        if (!context.IsGrounded)
        {
            _hasLeftGround = true;
        }
        else if (_hasLeftGround)
        {
            context.ChangeState(isMoving ? context.MoveState : context.IdleState);
        }
    }
}

/// <summary>
/// 점프하여 공중에 있는 상태입니다. 공중에서도 좌우 이동이 가능합니다.
/// </summary>
public class PlayerJumpState : PlayerState
{
    // 점프 직후에는 아직 땅에 닿아 있으므로, 한 번 땅을 떠난 뒤에만 착지로 판정합니다.
    private bool _hasLeftGround;

    public PlayerJumpState(Player player) : base(player) { }

    public override void Enter()
    {
        _hasLeftGround = false;
        player.Jump();
    }

    public override void Update()
    {
        bool isMoving = player.MoveInput != 0f;
        player.SetWalkAnimation(isMoving);
        if (isMoving)
        {
            player.Move(player.MoveInput);
        }

        if (!player.IsGrounded)
        {
            _hasLeftGround = true;
        }
        else if (_hasLeftGround)
        {
            player.StateMachine.ChangeState(isMoving ? player.MoveState : player.IdleState);
        }
    }
}

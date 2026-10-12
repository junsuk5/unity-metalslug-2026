/// <summary>
/// 제자리에 서 있는 상태입니다.
/// </summary>
public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(Player player) : base(player) { }

    public override void Enter()
    {
        player.SetWalkAnimation(false);
    }

    public override void Update()
    {
        if (player.Input.JumpPressed)
        {
            player.StateMachine.ChangeState(player.JumpState);
        }
        else if (player.Input.Horizontal != 0f)
        {
            player.StateMachine.ChangeState(player.MoveState);
        }
    }
}

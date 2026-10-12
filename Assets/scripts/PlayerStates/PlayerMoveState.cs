/// <summary>
/// 좌우로 걷는 상태입니다.
/// </summary>
public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(Player player) : base(player) { }

    public override void Enter()
    {
        player.SetWalkAnimation(true);
    }

    public override void Update()
    {
        if (player.Input.JumpPressed)
        {
            player.StateMachine.ChangeState(player.JumpState);
            return;
        }

        if (player.Input.Horizontal == 0f)
        {
            player.StateMachine.ChangeState(player.IdleState);
            return;
        }

        player.Move(player.Input.Horizontal);
    }
}
